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

namespace ThemeForge.Theme;

public sealed partial class ThemeService : IExternalThemeCatalog
{
    private readonly Dictionary<string, ThemePalette> _externalPalettes = new Dictionary<string, ThemePalette>(StringComparer.Ordinal);

    /// <inheritdoc/>
    public void RegisterPalette(ThemePalette palette)
    {
        ThemePalette snapshot = PaletteValidation.Normalize(palette);
        if (ThemeNames.All.Contains(snapshot.Name, StringComparer.Ordinal) || CurrentTheme == snapshot.Name)
        { throw new ArgumentException("Cannot replace a built-in or active palette.", nameof(palette)); }
        _externalPalettes[snapshot.Name] = snapshot;
        AvailableThemes = Array.AsReadOnly(AvailableThemes.Concat(new[] { snapshot.Name }).Distinct(StringComparer.Ordinal).ToArray());
    }

    private ResourceDictionary LoadExternalOrBuiltIn(string name)
    {
        if (!_externalPalettes.TryGetValue(name, out ThemePalette? palette)) { return LoadThemeDictionary(name); }
        ResourceDictionary resources = PaletteResources.Create(palette);
        resources[ThemeMarkerKey] = name;
        return resources;
    }
}
