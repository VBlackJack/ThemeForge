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

/// <summary>The closed set of supported palette slots; layout tokens remain framework-owned.</summary>
public static class PaletteSlots
{
    /// <summary>Canonical palette colours.</summary>
    public static IReadOnlyList<string> Canonical { get; } = Array.AsReadOnly(new[]
    { "Background", "CurrentLine", "Selection", "Foreground", "Comment", "Cyan", "Green", "Orange", "Pink", "Purple", "Red", "Yellow" });
    /// <summary>Semantic surface, accent, text and status colours.</summary>
    public static IReadOnlyList<string> Semantic { get; } = Array.AsReadOnly(new[]
    { "Surface", "SurfaceAlt", "Border", "Accent", "AccentHover", "AccentPressed", "TextPrimary", "TextSecondary", "Success", "Warning", "Error", "Info" });
    /// <summary>Additional accent colours.</summary>
    public static IReadOnlyList<string> Extended { get; } = Array.AsReadOnly(new[] { "Blue" });
    /// <summary>Deterministic complete slot order for validation and serialization.</summary>
    public static IReadOnlyList<string> All { get; } = Array.AsReadOnly(Canonical.Concat(Semantic).Concat(Extended).ToArray());
}
