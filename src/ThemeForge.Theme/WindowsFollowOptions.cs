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
/// Configures the combined Windows-follow preset exposed by
/// <see cref="IWindowsThemeFollower.FollowWindows"/>. The core ships no default
/// theme pair: callers supply the light and dark themes explicitly.
/// </summary>
public sealed class WindowsFollowOptions
{
    /// <summary>Theme applied when the operating system is in light mode.</summary>
    public required string LightTheme { get; init; }

    /// <summary>Theme applied when the operating system is in dark mode.</summary>
    public required string DarkTheme { get; init; }

    /// <summary>
    /// Whether the preset also follows the operating system accent color. Defaults
    /// to <see langword="true"/>: an unqualified "follow Windows" tracks the accent
    /// as well. Set to <see langword="false"/> to follow light/dark only.
    /// </summary>
    public bool FollowAccent { get; init; } = true;
}
