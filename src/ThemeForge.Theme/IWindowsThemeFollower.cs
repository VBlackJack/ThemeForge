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

/// <summary>Opt-in capability layered on top of IThemeService for SemVer safety.</summary>
public interface IWindowsThemeFollower
{
    /// <summary>
    /// Arms both Windows-follow capabilities in a single opt-in call: light/dark
    /// mode (<see cref="ISystemThemeFollower"/>) and, when
    /// <see cref="WindowsFollowOptions.FollowAccent"/> is true, the system accent
    /// (<see cref="ISystemAccentFollower"/>).
    /// </summary>
    /// <remarks>
    /// To stop following, override the relevant axis manually: calling
    /// <see cref="IThemeService.ApplyTheme"/> cuts the light/dark follow, and
    /// <see cref="IThemeService.ApplyAccentTint"/> cuts the accent follow. The
    /// follow state is observed through <see cref="ISystemThemeFollower.IsFollowingSystem"/>
    /// and <see cref="ISystemAccentFollower.IsFollowingSystemAccent"/>; this preset
    /// exposes no combined state of its own.
    /// </remarks>
    /// <param name="options">The light/dark theme pair and accent-follow choice.</param>
    /// <exception cref="System.ArgumentNullException"><paramref name="options"/> is null.</exception>
    /// <exception cref="System.ArgumentException">A theme name is absent from AvailableThemes.</exception>
    void FollowWindows(WindowsFollowOptions options);
}
