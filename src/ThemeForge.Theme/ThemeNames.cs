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

namespace ThemeForge.Theme;

/// <summary>
/// Canonical names for every shipped v6 theme variant.
/// </summary>
/// <remarks>
/// The set ships 16 variants : the historical Dracula MIT palette by Zeno
/// Rocha (kept as the framework's root), Drakul (Dracula sibling AA-compliant),
/// plus 14 original Apache 2.0 palettes by Julien Bombled. The final v6 palette
/// was validated after five independent cross-review passes by an independent reviewer, an independent reviewer,
/// and an independent reviewer. It carries the v5 uniform-Oklab-L* accent model, then adds
/// the Bracken rename and Drakul sibling :
/// <list type="bullet">
///   <item>All 10 Dark variants share a 7-accent band at Oklab L*
///         0.7185-0.7209.</item>
///   <item>The 2 Light variants share a darker accent band at Oklab L*
///         0.4995-0.5012.</item>
///   <item>The 2 Alt variants reuse the Dark band but break exactly one
///         signature accent while staying in the same Oklab L* band.</item>
///   <item>Comments on Julien-authored variants target uniform WCAG relative
///         luminance around 0.38 and clear AA against Background and
///         CurrentLine.</item>
///   <item>15 of 16 palettes clear the contracted AA text pairs. Dracula keeps
///         the canonical MIT Comment color unchanged; Drakul provides the
///         AA-compliant sibling.</item>
/// </list>
/// </remarks>
public static class ThemeNames
{
    // Dracula original — MIT, Zeno Rocha
    public const string Dracula = "Dracula";

    // 10 Dark family (Background hue rotation, ≥ 20° spacing)
    public const string Striga = "Striga";   // H  12° — Romanian night-witch
    public const string Cinder = "Cinder";  // H  50° — mourning hearth, warm amber
    public const string Bracken = "Bracken"; // H 110° — bracken fern, necropolis sentinel

    // 16th variant — Dracula sibling, AA-compliant
    public const string Drakul = "Drakul"; // Canonical Dracula MIT + Comment lifted to clear WCAG AA
    public const string Tarn = "Tarn";    // H 170° — highland tarn, petrol-teal
    public const string Mortis = "Mortis";   // H 200° — slate-cyan morgue
    public const string Slate = "Slate";    // H 225° — cold monastery stone
    public const string Voivode = "Voivode";  // H 255° — Wallachian throne, deep indigo
    public const string Carmilla = "Carmilla"; // H 290° — Le Fanu velvet violet
    public const string Whitby = "Whitby";   // H 320° — North Sea slate-mauve
    public const string Vesper = "Vesper";   // H 345° — chapel ruin rose

    // 2 Light family
    public const string Parchment = "Parchment"; // H  40° — warm vellum cream
    public const string Folio = "Folio";    // H 220° — cool ivory

    // 2 Alt family (signature accent broken)
    public const string Wormwood = "Wormwood";  // viridian Green broken
    public const string Sconce = "Sconce";    // amber Orange broken

    /// <summary>
    /// All shipped variant names, ordered as a perceptual hue rotation
    /// rather than alphabetically. Dracula root first, then Dark family
    /// 0° → 360°, then Light by hue, then Alt by signature hue.
    /// </summary>
    public static IReadOnlyList<string> All { get; } = new[]
    {
        Dracula,
        Drakul,
        // Dark — Bg hue 0° → 360°
        Striga, Cinder, Bracken, Tarn, Mortis, Slate, Voivode, Carmilla, Whitby, Vesper,
        // Light
        Parchment, Folio,
        // Alt
        Wormwood, Sconce,
    };

    /// <summary>Returns the family label of a theme name (Root / Dark / Light / Alt).</summary>
    public static string GetFamily(string name)
    {
        if (string.Equals(name, Dracula, StringComparison.Ordinal) ||
            string.Equals(name, Drakul, StringComparison.Ordinal))
        {
            return "Root";
        }
        if (string.Equals(name, Parchment, StringComparison.Ordinal) ||
            string.Equals(name, Folio, StringComparison.Ordinal))
        {
            return "Light";
        }
        if (string.Equals(name, Wormwood, StringComparison.Ordinal) ||
            string.Equals(name, Sconce, StringComparison.Ordinal))
        {
            return "Alt";
        }
        return "Dark";
    }
}
