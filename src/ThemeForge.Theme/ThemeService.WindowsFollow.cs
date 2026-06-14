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

namespace ThemeForge.Theme;

public sealed partial class ThemeService : IWindowsThemeFollower
{
    /// <summary>
    /// Initializes a service wired with both system providers. This is the only
    /// constructor that accepts the light/dark and accent providers together,
    /// which the combined <see cref="FollowWindows"/> preset and its tests need.
    /// </summary>
    /// <param name="application">The hosting WPF application.</param>
    /// <param name="systemThemeProvider">Provider for the system light/dark mode.</param>
    /// <param name="systemAccentProvider">Provider for the system accent color.</param>
    /// <param name="availableThemes">Optional list of applicable theme names.</param>
    public ThemeService(
        Application application,
        ISystemThemeProvider systemThemeProvider,
        ISystemAccentProvider systemAccentProvider,
        IReadOnlyList<string>? availableThemes = null)
        : this(application, availableThemes)
    {
        ArgumentNullException.ThrowIfNull(systemThemeProvider);
        ArgumentNullException.ThrowIfNull(systemAccentProvider);
        _systemThemeProvider = systemThemeProvider;
        _systemAccentProvider = systemAccentProvider;
    }

    /// <inheritdoc/>
    public void FollowWindows(WindowsFollowOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        EnableSystemFollow(options.LightTheme, options.DarkTheme);
        if (options.FollowAccent)
        {
            EnableSystemAccentFollow();
        }
    }
}
