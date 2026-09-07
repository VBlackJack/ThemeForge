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

using System.Text.Json.Serialization;

namespace ThemeForge.Theme.Palettes;

/// <summary>A portable, data-only palette. Resource keys are defined by <see cref="PaletteSlots"/>.</summary>
public sealed record ThemePalette
{
    /// <summary>Current JSON schema version.</summary>
    public const int SchemaVersion = 1;
    /// <summary>Version used when reading this document.</summary>
    [JsonRequired]
    public int Version { get; init; } = SchemaVersion;
    /// <summary>Stable ASCII identifier used by theme selection and preferences.</summary>
    [JsonRequired]
    public string Name { get; init; } = string.Empty;
    /// <summary>Human-readable palette title.</summary>
    [JsonRequired]
    public string DisplayName { get; init; } = string.Empty;
    /// <summary>Light, Dark, Root or Alt. Descriptive only; does not change the colours.</summary>
    [JsonRequired]
    public string Family { get; init; } = "Dark";
    /// <summary>Original authors, source and license information retained on export.</summary>
    [JsonRequired]
    public string Attribution { get; init; } = string.Empty;
    /// <summary>All required slots, without Color/Brush suffixes, as opaque hexadecimal colours.</summary>
    [JsonRequired]
    public IReadOnlyDictionary<string, string> Colors { get; init; } = new Dictionary<string, string>();
}
