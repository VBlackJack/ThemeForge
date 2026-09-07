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
using ThemeForge.Theme.Palettes;

namespace ThemeForge.Theme.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    private static void ValidateExternalPalettes(ThemeForgeOptions options)
    {
        ArgumentNullException.ThrowIfNull(options.Palettes);
        ThemePalette[] palettes = options.Palettes.Select(PaletteValidation.Normalize).ToArray();
        HashSet<string> names = new HashSet<string>(ThemeNames.All, StringComparer.Ordinal);
        foreach (ThemePalette palette in palettes)
        {
            if (!names.Add(palette.Name)) { throw new ArgumentException($"Duplicate or built-in palette name '{palette.Name}'."); }
        }
        options.Palettes = Array.AsReadOnly(palettes);
        if (palettes.Length > 0)
        {
            options.AvailableThemes = Array.AsReadOnly((options.AvailableThemes ?? ThemeNames.All)
                .Concat(palettes.Select(palette => palette.Name)).Distinct(StringComparer.Ordinal).ToArray());
        }
    }

    private static ThemeService CreateThemeService(Application application, ThemeForgeOptions options)
    {
        ThemeService service = new ThemeService(application, options.AvailableThemes);
        foreach (ThemePalette palette in options.Palettes) { service.RegisterPalette(palette); }
        return service;
    }
}
