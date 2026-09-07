// Copyright 2026 Julien Bombled
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.IO;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ThemeForge.Controls.Composites;
using ThemeForge.Studio.ViewModels;
using ThemeForge.Studio.Views;
using ThemeForge.Theme;

namespace ThemeForge.Quality;

internal static class VisualScenario
{
    internal static object Run(Application application, ThemeService service, QualityOptions options, string output)
    {
        List<object> captures = new List<object>();
        foreach (string theme in options.Themes)
        {
            service.ApplyTheme(theme);
            foreach (double scale in options.Scales)
            {
                StackPanel gallery = QualityGallery.Create(options);
                Window window = QualityWindow.Open(gallery, options, scale);
                try
                {
                    Button button = gallery.Children.OfType<Button>().First();
                    AutomationPeer buttonPeer = UIElementAutomationPeer.CreatePeerForElement(button);
                    if (buttonPeer.GetName() != options.ActionText || buttonPeer.GetPattern(PatternInterface.Invoke) is null)
                    { throw new InvalidOperationException("Button automation contract failed."); }
                    ToggleSwitch toggle = gallery.Children.OfType<ToggleSwitch>().Single();
                    AutomationPeer togglePeer = UIElementAutomationPeer.CreatePeerForElement(toggle);
                    if (togglePeer.GetPattern(PatternInterface.Toggle) is null) { throw new InvalidOperationException("Toggle automation contract failed."); }
                    NumericUpDown number = gallery.Children.OfType<NumericUpDown>().Single();
                    if (UIElementAutomationPeer.CreatePeerForElement(number).GetPattern(PatternInterface.RangeValue) is null)
                    { throw new InvalidOperationException("Numeric automation contract failed."); }
                    Toast toast = gallery.Children.OfType<Toast>().Single();
                    Button close = (Button)toast.Template.FindName("PART_CloseButton", toast);
                    if (!close.Focusable || string.IsNullOrWhiteSpace(AutomationProperties.GetName(close)))
                    { throw new InvalidOperationException("Toast close accessibility failed."); }
                    bool focusAccepted = button.Focus();
                    string file = $"gallery-{theme}-{scale.ToString("0.00", CultureInfo.InvariantCulture)}.png";
                    Capture(window, Path.Combine(output, file));
                    captures.Add(new { Theme = theme, Scale = scale, File = file, FocusAccepted = focusAccepted, AutomationChecks = 4 });
                }
                finally { QualityWindow.Close(window); }
                using PaletteEditorViewModel editor = new PaletteEditorViewModel(service, application.Resources);
                PaletteEditorView view = new PaletteEditorView(editor);
                Window editorWindow = QualityWindow.Open(view, options, scale);
                try
                {
                    Capture(editorWindow, Path.Combine(output, $"editor-{theme}-{scale.ToString("0.00", CultureInfo.InvariantCulture)}.png"));
                    ScrollViewer scroll = (ScrollViewer)editorWindow.Content; scroll.ScrollToEnd(); QualityWindow.Pump(editorWindow);
                    Capture(editorWindow, Path.Combine(output, $"editor-details-{theme}-{scale.ToString("0.00", CultureInfo.InvariantCulture)}.png"));
                }
                finally { QualityWindow.Close(editorWindow); }
            }
        }
        return new { Captures = captures, Scaling = "WPF layout scale; physical monitor DPI changes require manual verification",
            Manual = new[] { "Physical keyboard traversal", "Narrator reading order and announcements", "Native monitor DPI changes" } };
    }
    private static void Capture(Window window, string path)
    {
        QualityWindow.Pump(window);
        RenderTargetBitmap bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(window);
        PngBitmapEncoder encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using FileStream stream = File.Create(path); encoder.Save(stream);
    }
}
