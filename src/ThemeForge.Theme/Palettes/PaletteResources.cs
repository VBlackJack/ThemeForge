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
using System.Windows.Media;

namespace ThemeForge.Theme.Palettes;

/// <summary>Converts validated data to framework resources and captures palettes for editing.</summary>
public static class PaletteResources
{
    private const string TokensUri = "pack://application:,,,/ThemeForge.Theme;component/Themes/Shared/DesignTokens.xaml";

    /// <summary>Builds colour and frozen brush pairs, with framework layout tokens only.</summary>
    public static ResourceDictionary Create(ThemePalette palette)
    {
        ThemePalette validated = PaletteValidation.Normalize(palette);
        ResourceDictionary resources = new ResourceDictionary();
        resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(TokensUri, UriKind.Absolute) });
        foreach (KeyValuePair<string, string> slot in validated.Colors)
        {
            PaletteValidation.TryParseColor(slot.Value, out Color color);
            SolidColorBrush brush = new SolidColorBrush(color);
            brush.Freeze();
            resources[slot.Key + "Color"] = color;
            resources[slot.Key + "Brush"] = brush;
        }
        resources["ThemeDisplayName"] = validated.DisplayName;
        resources["ThemeFamily"] = validated.Family;
        resources["ThemeAttribution"] = validated.Attribution;
        return resources;
    }

    /// <summary>Captures the effective brush colours, including live editor or accent overrides.</summary>
    public static ThemePalette Capture(ResourceDictionary resources, string name, string attribution)
    {
        Dictionary<string, string> colors = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (string slot in PaletteSlots.All)
        {
            if (resources[slot + "Brush"] is not SolidColorBrush brush || brush.Color.A != byte.MaxValue)
            { throw new ArgumentException($"A solid opaque brush is required for '{slot}'."); }
            colors.Add(slot, PaletteValidation.ToHex(brush.Color));
        }
        return PaletteValidation.Normalize(new ThemePalette
        {
            Name = name, DisplayName = name, Family = resources["ThemeFamily"] as string ?? "Dark",
            Attribution = attribution, Colors = colors,
        });
    }
}
