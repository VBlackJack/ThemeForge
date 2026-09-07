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

namespace ThemeForge.Theme.Palettes;

/// <summary>Portable provenance for documents derived from built-in palettes.</summary>
public static class BuiltInPaletteAttribution
{
    private const string NoticeUrl = "https://github.com/VBlackJack/ThemeForge/blob/main/NOTICE";
    /// <summary>Retains source authorship and references the complete canonical attribution.</summary>
    public static string Describe(string name)
    {
        string source = name switch
        {
            ThemeNames.Dracula => "Dracula Theme by Zeno Rocha, MIT. https://draculatheme.com",
            ThemeNames.Drakul => "Drakul by Julien Bombled, Apache-2.0, derived from Dracula Theme by Zeno Rocha, MIT. https://draculatheme.com",
            ThemeNames.Magellan => "Magellan by Julien Bombled, Apache-2.0; brand colours derived from Magellan corporate identity.",
            _ => $"{name} original palette by Julien Bombled, Apache-2.0.",
        };
        return $"Derived palette. Source: {source} Full attribution: {NoticeUrl}";
    }
}
