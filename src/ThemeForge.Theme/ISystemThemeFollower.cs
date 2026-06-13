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
public interface ISystemThemeFollower
{
    /// <summary>Gets whether the active theme follows the operating system mode.</summary>
    bool IsFollowingSystem { get; }

    /// <summary>Starts following the operating system light/dark mode.</summary>
    /// <param name="lightTheme">Theme to apply when the system is in light mode.</param>
    /// <param name="darkTheme">Theme to apply when the system is in dark mode.</param>
    void EnableSystemFollow(string lightTheme, string darkTheme);

    /// <summary>Stops following the operating system light/dark mode.</summary>
    void DisableSystemFollow();
}
