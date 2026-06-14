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
using System.Windows.Threading;

namespace ThemeForge.Theme;

public sealed partial class ThemeService
{
    private ISystemThemeProvider? _systemThemeProvider;
    private string _lightTheme = string.Empty;
    private string _darkTheme = string.Empty;
    private bool _applyingFromFollow;

    public ThemeService(
        Application application,
        ISystemThemeProvider systemThemeProvider,
        IReadOnlyList<string>? availableThemes = null)
        : this(application, availableThemes)
    {
        ArgumentNullException.ThrowIfNull(systemThemeProvider);
        _systemThemeProvider = systemThemeProvider;
    }

    /// <inheritdoc/>
    public bool IsFollowingSystem { get; private set; }

    /// <inheritdoc/>
    public void EnableSystemFollow(string lightTheme, string darkTheme)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lightTheme);
        ArgumentException.ThrowIfNullOrWhiteSpace(darkTheme);
        EnsureThemeIsAvailable(lightTheme, nameof(lightTheme));
        EnsureThemeIsAvailable(darkTheme, nameof(darkTheme));

        _lightTheme = lightTheme;
        _darkTheme = darkTheme;
        _systemThemeProvider ??= new RegistrySystemThemeProvider();

        IsFollowingSystem = true;
        _systemThemeProvider.Changed -= OnSystemThemeChanged;
        _systemThemeProvider.Changed += OnSystemThemeChanged;
        ApplyForMode(_systemThemeProvider.GetCurrentMode());
    }

    /// <inheritdoc/>
    public void DisableSystemFollow()
    {
        IsFollowingSystem = false;
        if (_systemThemeProvider is not null)
        {
            _systemThemeProvider.Changed -= OnSystemThemeChanged;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        DisableSystemFollow();
        DisableSystemAccentFollow();
        (_systemThemeProvider as IDisposable)?.Dispose();
        (_systemAccentProvider as IDisposable)?.Dispose();
    }

    private void OnSystemThemeChanged(object? sender, EventArgs e)
    {
        if (_systemThemeProvider is null)
        {
            return;
        }

        SystemThemeMode mode = _systemThemeProvider.GetCurrentMode();
        ApplyOnApplicationDispatcher(mode);
    }

    private void ApplyOnApplicationDispatcher(SystemThemeMode mode)
    {
        Dispatcher dispatcher = _application.Dispatcher;
        if (dispatcher.CheckAccess() ||
            dispatcher.HasShutdownStarted ||
            dispatcher.HasShutdownFinished ||
            !dispatcher.Thread.IsAlive)
        {
            ApplyForMode(mode);
            return;
        }

        dispatcher.Invoke(() => ApplyForMode(mode));
    }

    private void ApplyForMode(SystemThemeMode mode)
    {
        if (mode == SystemThemeMode.Unknown)
        {
            return;
        }

        string target = mode == SystemThemeMode.Light ? _lightTheme : _darkTheme;
        _applyingFromFollow = true;
        try
        {
            ApplyTheme(target);
        }
        finally
        {
            _applyingFromFollow = false;
        }
    }

    private void EnsureThemeIsAvailable(string themeName, string parameterName)
    {
        if (!AvailableThemes.Contains(themeName, StringComparer.Ordinal))
        {
            throw new ArgumentException(
                $"Theme '{themeName}' is not in AvailableThemes.", parameterName);
        }
    }
}
