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

using System.Collections.ObjectModel;
using System.Windows.Media;
using ThemeForge.Theme.Palettes;

namespace ThemeForge.Studio.ViewModels;

public sealed partial class PaletteEditorViewModel
{
    /// <summary>Text/background diagnostic pairs for the current document.</summary>
    public ObservableCollection<ContrastEntry> Contrasts { get; } = new ObservableCollection<ContrastEntry>();
    /// <summary>Original preview background.</summary>
    public Brush BeforeBackground => ReadBrush(_original, "Background");
    /// <summary>Original preview text.</summary>
    public Brush BeforeForeground => ReadBrush(_original, "TextPrimary");
    /// <summary>Edited preview background.</summary>
    public Brush AfterBackground => ReadBrush(_current, "Background");
    /// <summary>Edited preview text.</summary>
    public Brush AfterForeground => ReadBrush(_current, "TextPrimary");
    private void RefreshDiagnostics()
    {
        Contrasts.Clear();
        foreach ((string text, string background) in new[]
        { ("TextPrimary", "Background"), ("TextSecondary", "Background"), ("TextPrimary", "Surface"), ("TextSecondary", "SurfaceAlt") })
        {
            PaletteValidation.TryParseColor(_current.Colors[text], out Color foreground);
            PaletteValidation.TryParseColor(_current.Colors[background], out Color surface);
            Contrasts.Add(new ContrastEntry(text + " / " + background, PaletteContrast.Ratio(foreground, surface)));
        }
        OnPropertyChanged(nameof(BeforeBackground)); OnPropertyChanged(nameof(BeforeForeground));
        OnPropertyChanged(nameof(AfterBackground)); OnPropertyChanged(nameof(AfterForeground));
    }
    private static Brush ReadBrush(ThemePalette palette, string slot)
    {
        PaletteValidation.TryParseColor(palette.Colors[slot], out Color color);
        SolidColorBrush brush = new SolidColorBrush(color); brush.Freeze(); return brush;
    }
}
