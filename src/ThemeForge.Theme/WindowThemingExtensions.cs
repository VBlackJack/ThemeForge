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
using ThemeForge.Theme.Interop;

namespace ThemeForge.Theme;

/// <summary>
/// Per-window extension that themes the operating system non-client area (the title
/// bar caption and its colors) from the current theme.
/// </summary>
public static class WindowThemingExtensions
{
    /// <summary>
    /// Themes the title bar of <paramref name="window"/> from the current theme and
    /// keeps it in sync with <see cref="IThemeService.ThemeChanged"/>. Best-effort:
    /// on Windows versions that predate a given attribute, that attribute is skipped
    /// and the title bar keeps the OS default. The caller writes this once per window
    /// (the project template generates it in <c>MainWindow</c>).
    /// </summary>
    /// <param name="window">The window whose title bar is themed.</param>
    /// <param name="themeService">The theme service driving re-synchronization.</param>
    /// <param name="options">Optional color overrides. When null, all colors derive
    /// from the theme.</param>
    /// <returns>
    /// A handle that unsubscribes from theme changes when disposed. The window's
    /// <c>Closed</c> event also disposes it; disposal is idempotent.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="window"/> or <paramref name="themeService"/> is null.
    /// </exception>
    public static IDisposable ApplyThemeForgeTitleBar(
        this Window window,
        IThemeService themeService,
        TitleBarOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(themeService);

        TitleBarThemer themer = new TitleBarThemer(
            window,
            themeService,
            options ?? new TitleBarOptions(),
            new NativeWindowChrome(),
            Environment.OSVersion.Version);
        themer.Apply();
        return themer;
    }
}
