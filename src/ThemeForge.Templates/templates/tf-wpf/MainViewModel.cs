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

using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using ThemeForge.Theme;

namespace ThemeForge.WpfApp;

/// <summary>
/// Backs the demo window: a theme picker and a Follow Windows toggle, both kept in
/// sync with the live theme state through <see cref="IThemeService.ThemeChanged"/>.
/// </summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly IThemeService _themeService;
    private readonly IWindowsThemeFollower _windowsFollower;
    private readonly ISystemThemeFollower _systemFollower;
    private readonly AppThemeConfig _config;
    private bool _synchronizing;

    public MainViewModel(
        IThemeService themeService,
        IWindowsThemeFollower windowsFollower,
        ISystemThemeFollower systemFollower,
        AppThemeConfig config)
    {
        _themeService = themeService;
        _windowsFollower = windowsFollower;
        _systemFollower = systemFollower;
        _config = config;
        _selectedTheme = themeService.CurrentTheme;
        _followWindows = systemFollower.IsFollowingSystem;
        themeService.ThemeChanged += OnThemeChanged;
    }

    /// <summary>The themes the picker offers.</summary>
    public IReadOnlyList<string> AvailableThemes => _themeService.AvailableThemes;

    [ObservableProperty]
    private string _selectedTheme;

    [ObservableProperty]
    private bool _followWindows;

    partial void OnSelectedThemeChanged(string value)
    {
        if (_synchronizing || string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        _themeService.ApplyTheme(value);
    }

    partial void OnFollowWindowsChanged(bool value)
    {
        if (_synchronizing)
        {
            return;
        }

        if (value)
        {
            _windowsFollower.FollowWindows(_config.Follow);
        }
        else
        {
            _themeService.ApplyTheme(SelectedTheme);
        }
    }

    private void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
    {
        // Reflect the live state without re-triggering the apply handlers above.
        _synchronizing = true;
        try
        {
            SelectedTheme = _themeService.CurrentTheme;
            FollowWindows = _systemFollower.IsFollowingSystem;
        }
        finally
        {
            _synchronizing = false;
        }
    }
}
