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
/// Single entry point that swaps the active theme ResourceDictionary in the
/// hosting WPF application. Consumers depend on this abstraction so the
/// theme engine can be mocked in tests and replaced if needed.
/// </summary>
public interface IThemeService
{
    /// <summary>The name of the currently applied theme.</summary>
    string CurrentTheme { get; }

    /// <summary>
    /// Monotonic counter incremented every time a theme is applied. Bumped
    /// before <see cref="ThemeChanged"/> is raised so converters and one-way
    /// bindings can observe the change and re-evaluate.
    /// </summary>
    int ThemeRevision { get; }

    /// <summary>
    /// The list of theme names the service can apply. Stable, sorted,
    /// case-sensitive — see <see cref="ThemeNames"/> for canonical constants.
    /// </summary>
    IReadOnlyList<string> AvailableThemes { get; }

    /// <summary>The list of accent tints the service can apply. Stable order.</summary>
    IReadOnlyList<AccentTint> AvailableAccentTints { get; }

    /// <summary>
    /// The currently applied accent tint. <see cref="AccentTint.Default"/>
    /// means the active theme's native accent is used.
    /// </summary>
    AccentTint CurrentAccentTint { get; }

    /// <summary>
    /// Raised after the active theme has been swapped. The handler runs on
    /// the UI thread.
    /// </summary>
    event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    /// <summary>
    /// Apply the named theme. Idempotent — applying the current theme is a
    /// no-op. Unknown theme names throw <see cref="ArgumentException"/>.
    /// </summary>
    /// <param name="name">A theme name from <see cref="AvailableThemes"/>.</param>
    void ApplyTheme(string name);

    /// <summary>
    /// Apply an accent tint override on top of the current theme.
    /// </summary>
    /// <remarks>
    /// Idempotent. <see cref="AccentTint.Default"/> removes any tint override
    /// and restores the theme's native accent. Applying a tint bumps
    /// <see cref="ThemeRevision"/> and raises <see cref="ThemeChanged"/> so
    /// <c>DynamicResource</c> consumers re-evaluate.
    /// </remarks>
    /// <param name="tint">The accent tint to apply.</param>
    void ApplyAccentTint(AccentTint tint);
}
