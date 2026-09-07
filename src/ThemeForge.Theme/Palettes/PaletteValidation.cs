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
using System.Globalization;
using System.Windows.Media;

namespace ThemeForge.Theme.Palettes;

/// <summary>Validates untrusted palette data and takes an immutable normalized snapshot.</summary>
public static class PaletteValidation
{
    /// <summary>Maximum size of a palette JSON document.</summary>
    public const int MaximumFileBytes = 65536;
    private const int MaximumNameLength = 64;
    private const int MaximumTitleLength = 128;
    private const int MaximumAttributionLength = 2048;

    /// <summary>Rejects unknown versions, fields, slots, identifiers and non-opaque colours.</summary>
    public static ThemePalette Normalize(ThemePalette palette)
    {
        ArgumentNullException.ThrowIfNull(palette);
        if (palette.Version != ThemePalette.SchemaVersion) { throw new ArgumentException("Unsupported palette version."); }
        string name = palette.Name;
        if (string.IsNullOrEmpty(name) || name.Length > MaximumNameLength || name[0] is not (>= 'A' and <= 'Z') ||
            name.Any(character => !char.IsAsciiLetterOrDigit(character)))
        { throw new ArgumentException("Palette name must start with an uppercase ASCII letter and contain only ASCII letters and digits (maximum 64)."); }
        if (string.IsNullOrWhiteSpace(palette.DisplayName) || palette.DisplayName.Length > MaximumTitleLength)
        { throw new ArgumentException("Palette display name is required (maximum 128)."); }
        if (palette.Family is not ("Light" or "Dark" or "Root" or "Alt")) { throw new ArgumentException("Unsupported palette family."); }
        if (string.IsNullOrWhiteSpace(palette.Attribution) || palette.Attribution.Length > MaximumAttributionLength)
        { throw new ArgumentException("Palette attribution is required (maximum 2048)."); }
        if (palette.Colors is null || palette.Colors.Count != PaletteSlots.All.Count ||
            palette.Colors.Keys.Any(key => !PaletteSlots.All.Contains(key, StringComparer.Ordinal)))
        { throw new ArgumentException("Palette must contain exactly the documented colour slots."); }
        Dictionary<string, string> colors = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (string slot in PaletteSlots.All)
        {
            if (!palette.Colors.TryGetValue(slot, out string? value) || !TryParseColor(value, out Color color))
            { throw new ArgumentException($"Invalid or missing opaque colour for '{slot}'."); }
            colors.Add(slot, ToHex(color));
        }
        return palette with { Colors = new ReadOnlyDictionary<string, string>(colors) };
    }

    /// <summary>Parses #RRGGBB or #FFRRGGBB without invoking a XAML/type converter.</summary>
    public static bool TryParseColor(string? value, out Color color)
    {
        color = default;
        if (string.IsNullOrEmpty(value) || value[0] != '#') { return false; }
        string digits = value[1..];
        if (digits.Length == 8 && digits.StartsWith("FF", StringComparison.OrdinalIgnoreCase)) { digits = digits[2..]; }
        if (digits.Length != 6 || !uint.TryParse(digits, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out uint rgb))
        { return false; }
        color = Color.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        return true;
    }

    /// <summary>Canonical opaque hexadecimal representation.</summary>
    public static string ToHex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";
}
