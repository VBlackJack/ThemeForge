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

namespace ThemeForge.Theme.Persistence;

/// <summary>
/// The persisted theme intent of the end user: either an explicit theme plus
/// accent, or "follow Windows". Pure data with no behavior and no I/O.
/// </summary>
public sealed record ThemePreference
{
    /// <summary>The current schema version written by this assembly.</summary>
    public const int SchemaVersion = 1;

    /// <summary>
    /// The explicitly chosen theme name, or <see langword="null"/> when none was
    /// recorded (for example while following Windows).
    /// </summary>
    public string? ThemeName { get; init; }

    /// <summary>The chosen accent tint override.</summary>
    public AccentTint AccentTint { get; init; }

    /// <summary>Whether the user opted to follow the Windows theme and accent.</summary>
    public bool FollowWindows { get; init; }

    /// <summary>
    /// The schema version of this record. Defaults to <see cref="SchemaVersion"/>;
    /// a store discards payloads carrying any other value.
    /// </summary>
    public int Version { get; init; } = SchemaVersion;
}
