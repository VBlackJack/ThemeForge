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

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ThemeForge.Theme;

namespace ThemeForge.Studio.ViewModels;

/// <summary>
/// View model of the Studio main window. Owns the theme picker state and
/// the sidebar navigation between gallery sections.
/// </summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly IThemeService _themeService;
    private readonly ISystemThemeFollower _systemThemeFollower;
    private readonly ISystemAccentFollower _systemAccentFollower;

    public MainViewModel(
        IThemeService themeService,
        ISystemThemeFollower systemThemeFollower,
        ISystemAccentFollower systemAccentFollower,
        IEnumerable<GallerySectionViewModel> sections)
    {
        ArgumentNullException.ThrowIfNull(themeService);
        ArgumentNullException.ThrowIfNull(systemThemeFollower);
        ArgumentNullException.ThrowIfNull(systemAccentFollower);
        ArgumentNullException.ThrowIfNull(sections);
        _themeService = themeService;
        _systemThemeFollower = systemThemeFollower;
        _systemAccentFollower = systemAccentFollower;

        Themes = new ObservableCollection<ThemeEntry>(
            themeService.AvailableThemes.Select(n =>
            {
                // Disambiguate the Dracula/Drakul intentional sibling pair
                // for the picker so users don't typo-jump between them.
                string display = n switch
                {
                    "Dracula" => "Dracula (canon)",
                    "Drakul" => "Drakul (AA-compliant)",
                    _ => n,
                };
                return new ThemeEntry(n, display, ThemeNames.GetFamily(n));
            }));
        _selectedTheme = Themes.FirstOrDefault(t => t.Name == themeService.CurrentTheme);
        _isFollowingSystem = systemThemeFollower.IsFollowingSystem;
        _isFollowingSystemAccent = systemAccentFollower.IsFollowingSystemAccent;
        AvailableAccentTints = themeService.AvailableAccentTints;
        _selectedAccentTint = themeService.CurrentAccentTint;

        Sections = new ObservableCollection<GallerySectionViewModel>(sections);
        _selectedSection = Sections.FirstOrDefault();

        themeService.ThemeChanged += OnThemeChanged;
    }

    /// <summary>Theme entries exposed to the picker, grouped by Family in the view.</summary>
    public ObservableCollection<ThemeEntry> Themes { get; }

    /// <summary>Accent tints exposed to the accent picker.</summary>
    public IReadOnlyList<AccentTint> AvailableAccentTints { get; }

    /// <summary>Gallery sections shown in the sidebar.</summary>
    public ObservableCollection<GallerySectionViewModel> Sections { get; }

    [ObservableProperty]
    private ThemeEntry? _selectedTheme;

    [ObservableProperty]
    private bool _isFollowingSystem;

    [ObservableProperty]
    private bool _isFollowingSystemAccent;

    [ObservableProperty]
    private AccentTint _selectedAccentTint;

    [ObservableProperty]
    private GallerySectionViewModel? _selectedSection;

    public bool IsAccentTintPickerEnabled => !IsFollowingSystemAccent;

    partial void OnSelectedThemeChanged(ThemeEntry? value)
    {
        if (value is null || value.Name == _themeService.CurrentTheme)
        {
            return;
        }

        _themeService.ApplyTheme(value.Name);
    }

    partial void OnIsFollowingSystemChanged(bool value)
    {
        if (value == _systemThemeFollower.IsFollowingSystem)
        {
            return;
        }

        if (value)
        {
            _systemThemeFollower.EnableSystemFollow(ThemeNames.Folio, ThemeNames.Drakul);
            return;
        }

        _systemThemeFollower.DisableSystemFollow();
    }

    partial void OnIsFollowingSystemAccentChanged(bool value)
    {
        OnPropertyChanged(nameof(IsAccentTintPickerEnabled));
        if (value == _systemAccentFollower.IsFollowingSystemAccent)
        {
            return;
        }

        if (value)
        {
            _systemAccentFollower.EnableSystemAccentFollow();
            return;
        }

        _systemAccentFollower.DisableSystemAccentFollow();
    }

    partial void OnSelectedAccentTintChanged(AccentTint value)
    {
        if (value == _themeService.CurrentAccentTint)
        {
            return;
        }

        _themeService.ApplyAccentTint(value);
    }

    private void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
    {
        // Keep the selection in sync if the theme was changed by another caller
        // (e.g. a future palette editor or a keyboard shortcut).
        if (SelectedTheme?.Name != e.CurrentTheme)
        {
            SelectedTheme = Themes.FirstOrDefault(t => t.Name == e.CurrentTheme);
        }

        if (IsFollowingSystem != _systemThemeFollower.IsFollowingSystem)
        {
            IsFollowingSystem = _systemThemeFollower.IsFollowingSystem;
        }

        if (IsFollowingSystemAccent != _systemAccentFollower.IsFollowingSystemAccent)
        {
            IsFollowingSystemAccent = _systemAccentFollower.IsFollowingSystemAccent;
        }

        if (SelectedAccentTint != _themeService.CurrentAccentTint)
        {
            SelectedAccentTint = _themeService.CurrentAccentTint;
        }
    }
}
