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

using ThemeForge.Theme.Persistence;

namespace ThemeForge.Theme.DependencyInjection;

/// <summary>
/// Configures the ThemeForge bootstrap: the default applied when no usable
/// preference exists, the Follow Windows mapping, and how the user's choice is
/// persisted. Supplied through the options-based
/// <see cref="ServiceCollectionExtensions.AddThemeForge(Microsoft.Extensions.DependencyInjection.IServiceCollection, System.Windows.Application, System.Action{ThemeForgeOptions})"/>.
/// </summary>
public sealed class ThemeForgeOptions
{
    /// <summary>
    /// Theme applied at startup when no usable preference is restored and Follow
    /// Windows is not the default. Required unless <see cref="FollowWindowsByDefault"/>
    /// is set with a <see cref="WindowsFollow"/> mapping.
    /// </summary>
    public string? DefaultTheme { get; set; }

    /// <summary>Accent tint applied alongside <see cref="DefaultTheme"/>.</summary>
    public AccentTint DefaultAccentTint { get; set; } = AccentTint.Default;

    /// <summary>
    /// The theme names the engine may apply. When null, the canonical ThemeForge
    /// set is used. Absorbs the parameter of the legacy registration overload.
    /// </summary>
    public IReadOnlyList<string>? AvailableThemes { get; set; }

    /// <summary>
    /// The light/dark theme pair (and accent-follow choice) used both when Follow
    /// Windows is the default and when a persisted "follow Windows" preference is
    /// restored. Required for either of those paths.
    /// </summary>
    public WindowsFollowOptions? WindowsFollow { get; set; }

    /// <summary>
    /// Whether to follow Windows on first run (no stored preference). Requires
    /// <see cref="WindowsFollow"/>. Defaults to <see langword="false"/>.
    /// </summary>
    public bool FollowWindowsByDefault { get; set; }

    /// <summary>
    /// Explicit preference store. Wins over <see cref="ApplicationName"/> when both
    /// are set. When neither is set, persistence stays off (no restore, no auto-save).
    /// </summary>
    public IThemePreferenceStore? PreferenceStore { get; set; }

    /// <summary>
    /// Convenience: when set (and <see cref="PreferenceStore"/> is not), a JSON store
    /// is created under <c>%AppData%/{ApplicationName}/preferences.json</c>.
    /// </summary>
    public string? ApplicationName { get; set; }

    /// <summary>
    /// Optional error sink shared by the preference store (read/parse failures) and
    /// the auto-save wiring (write failures). The core imposes no logger.
    /// </summary>
    public Action<Exception>? OnError { get; set; }
}
