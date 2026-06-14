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
using System.Windows.Media;
using System.Windows.Threading;

namespace ThemeForge.Theme;

public sealed partial class ThemeService
{
    private const string SystemAccentMarkerKey = "ThemeForge.ActiveSystemAccentMarker";
    private const string SystemAccentMarkerValue = "SystemAccent";

    private ISystemAccentProvider? _systemAccentProvider;
    private bool _applyingFromSystemAccent;

    public ThemeService(
        Application application,
        ISystemAccentProvider systemAccentProvider,
        IReadOnlyList<string>? availableThemes = null)
        : this(application, availableThemes)
    {
        ArgumentNullException.ThrowIfNull(systemAccentProvider);
        _systemAccentProvider = systemAccentProvider;
    }

    /// <inheritdoc/>
    public bool IsFollowingSystemAccent { get; private set; }

    /// <inheritdoc/>
    public void EnableSystemAccentFollow()
    {
        _systemAccentProvider ??= new RegistrySystemAccentProvider();

        IsFollowingSystemAccent = true;
        _systemAccentProvider.Changed -= OnSystemAccentChanged;
        _systemAccentProvider.Changed += OnSystemAccentChanged;

        ClearAccentTintForSystemAccent();
        ApplySystemAccent(_systemAccentProvider.GetCurrentAccent());
    }

    /// <inheritdoc/>
    public void DisableSystemAccentFollow()
    {
        if (!IsFollowingSystemAccent)
        {
            return;
        }

        IsFollowingSystemAccent = false;
        if (_systemAccentProvider is not null)
        {
            _systemAccentProvider.Changed -= OnSystemAccentChanged;
        }

        ApplySystemAccent(null);
    }

    private void OnSystemAccentChanged(object? sender, EventArgs e)
    {
        if (_systemAccentProvider is null)
        {
            return;
        }

        Color? accent = _systemAccentProvider.GetCurrentAccent();
        ApplyAccentOnApplicationDispatcher(accent);
    }

    private void ApplyAccentOnApplicationDispatcher(Color? accent)
    {
        Dispatcher dispatcher = _application.Dispatcher;
        if (dispatcher.CheckAccess() ||
            dispatcher.HasShutdownStarted ||
            dispatcher.HasShutdownFinished ||
            !dispatcher.Thread.IsAlive)
        {
            ApplySystemAccent(accent);
            return;
        }

        dispatcher.Invoke(() => ApplySystemAccent(accent));
    }

    private void ApplySystemAccent(Color? accent)
    {
        IList<ResourceDictionary> merged = _application.Resources.MergedDictionaries;
        bool changed = RemoveMarkedDictionary(merged, SystemAccentMarkerKey);
        if (accent is Color color)
        {
            merged.Add(CreateSystemAccentDictionary(color));
            changed = true;
        }

        if (changed && !string.IsNullOrWhiteSpace(_currentTheme))
        {
            RaiseCurrentThemeChanged();
        }
    }

    private void ClearAccentTintForSystemAccent()
    {
        if (_currentAccentTint == AccentTint.Default)
        {
            return;
        }

        _applyingFromSystemAccent = true;
        try
        {
            ApplyAccentTint(AccentTint.Default);
        }
        finally
        {
            _applyingFromSystemAccent = false;
        }
    }

    private static ResourceDictionary CreateSystemAccentDictionary(Color color)
        => CreateAccentOverrideDictionary(color, SystemAccentMarkerKey, SystemAccentMarkerValue);

    private void RaiseCurrentThemeChanged()
    {
        _themeRevision++;
        ThemeChanged?.Invoke(
            this,
            new ThemeChangedEventArgs(_currentTheme, _currentTheme, _themeRevision));
    }
}
