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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ThemeForge.Quality;

internal static class QualityWindow
{
    internal const string StylesUri = "pack://application:,,,/ThemeForge.Controls;component/Styles/Studio.xaml";
    internal static Window Open(FrameworkElement content, QualityOptions options, double scale = 1)
    {
        content.LayoutTransform = new ScaleTransform(scale, scale);
        Window window = new Window
        {
            Width = options.Width, Height = options.Height, Left = -10000, Top = -10000,
            ShowActivated = false, ShowInTaskbar = false, WindowStyle = WindowStyle.None,
            Content = new ScrollViewer { Content = content, VerticalScrollBarVisibility = ScrollBarVisibility.Auto },
        };
        window.SetResourceReference(Control.BackgroundProperty, "BackgroundBrush");
        window.SetResourceReference(Control.ForegroundProperty, "TextPrimaryBrush");
        window.Show(); Pump(window); return window;
    }
    internal static void Pump(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(static () => { }, DispatcherPriority.Loaded);
        window.UpdateLayout();
    }
    internal static void Close(Window window) { window.Content = null; window.Close(); }
}
