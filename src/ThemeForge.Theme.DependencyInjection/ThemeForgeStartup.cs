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
/// Orchestrates the explicit bootstrap: restore the persisted preference (or apply
/// a default), arm Follow Windows when intended, and auto-save on theme changes.
/// Owns the theme end to end so App.xaml never merges a theme dictionary.
/// </summary>
internal sealed class ThemeForgeStartup : IDisposable
{
    private readonly IThemeService _themeService;
    private readonly IWindowsThemeFollower _windowsFollower;
    private readonly ISystemThemeFollower _themeFollower;
    private readonly ThemeForgeOptions _options;
    private readonly IThemePreferenceStore? _store;

    private bool _hasRun;
    private bool _disposed;
    private ThemePreference? _lastWritten;

    public ThemeForgeStartup(
        IThemeService themeService,
        IWindowsThemeFollower windowsFollower,
        ISystemThemeFollower themeFollower,
        ThemeForgeOptions options,
        IThemePreferenceStore? store)
    {
        ArgumentNullException.ThrowIfNull(themeService);
        ArgumentNullException.ThrowIfNull(windowsFollower);
        ArgumentNullException.ThrowIfNull(themeFollower);
        ArgumentNullException.ThrowIfNull(options);
        _themeService = themeService;
        _windowsFollower = windowsFollower;
        _themeFollower = themeFollower;
        _options = options;
        _store = store;
    }

    /// <summary>
    /// Runs the bootstrap once. A second call is a no-op so wiring stays idempotent
    /// and the auto-save handler is never subscribed twice.
    /// </summary>
    public void Run()
    {
        if (_hasRun)
        {
            return;
        }

        _hasRun = true;
        Restore();

        // Subscribe only after the restore so re-applying the stored choice does
        // not immediately re-save it.
        if (_store is not null)
        {
            _themeService.ThemeChanged += OnThemeChanged;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _themeService.ThemeChanged -= OnThemeChanged;
    }

    private void Restore()
    {
        ThemePreference? preference = _store?.Load();
        if (preference is not null && TryApplyPreference(preference))
        {
            return;
        }

        ApplyDefault();
    }

    private bool TryApplyPreference(ThemePreference preference)
    {
        if (preference.FollowWindows && _options.WindowsFollow is not null)
        {
            _windowsFollower.FollowWindows(_options.WindowsFollow);
            return true;
        }

        // A persisted theme name may no longer exist (theme removed since the last
        // save). Degrade to the default path instead of throwing at startup.
        if (!string.IsNullOrWhiteSpace(preference.ThemeName) &&
            _themeService.AvailableThemes.Contains(preference.ThemeName, StringComparer.Ordinal))
        {
            _themeService.ApplyTheme(preference.ThemeName);
            _themeService.ApplyAccentTint(preference.AccentTint);
            return true;
        }

        return false;
    }

    private void ApplyDefault()
    {
        if (_options.FollowWindowsByDefault && _options.WindowsFollow is not null)
        {
            _windowsFollower.FollowWindows(_options.WindowsFollow);
            return;
        }

        // AddThemeForge validates that a viable default exists, so DefaultTheme is
        // non-null and present in AvailableThemes when this branch is reached.
        _themeService.ApplyTheme(_options.DefaultTheme!);
        _themeService.ApplyAccentTint(_options.DefaultAccentTint);
    }

    private void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
    {
        // Persist the intent, never the resolved theme: while following Windows the
        // momentary theme must not be pinned, or the next boot would stop following.
        bool following = _themeFollower.IsFollowingSystem;
        ThemePreference current = new ThemePreference
        {
            FollowWindows = following,
            ThemeName = following ? null : _themeService.CurrentTheme,
            AccentTint = _themeService.CurrentAccentTint,
        };

        if (current == _lastWritten)
        {
            return; // Anti write-storm: nothing changed in the persisted intent.
        }

        try
        {
            _store!.Save(current);
            _lastWritten = current;
        }
        catch (Exception ex)
        {
            // A disk failure during a theme change must never crash the UI; report
            // it through the shared sink and keep running.
            _options.OnError?.Invoke(ex);
        }
    }
}
