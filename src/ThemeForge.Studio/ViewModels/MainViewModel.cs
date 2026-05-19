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

    public MainViewModel(IThemeService themeService, IEnumerable<GallerySectionViewModel> sections)
    {
        ArgumentNullException.ThrowIfNull(themeService);
        ArgumentNullException.ThrowIfNull(sections);
        _themeService = themeService;

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

        Sections = new ObservableCollection<GallerySectionViewModel>(sections);
        _selectedSection = Sections.FirstOrDefault();

        themeService.ThemeChanged += OnThemeChanged;
    }

    /// <summary>Theme entries exposed to the picker, grouped by Family in the view.</summary>
    public ObservableCollection<ThemeEntry> Themes { get; }

    /// <summary>Gallery sections shown in the sidebar.</summary>
    public ObservableCollection<GallerySectionViewModel> Sections { get; }

    [ObservableProperty]
    private ThemeEntry? _selectedTheme;

    [ObservableProperty]
    private GallerySectionViewModel? _selectedSection;

    partial void OnSelectedThemeChanged(ThemeEntry? value)
    {
        if (value is null || value.Name == _themeService.CurrentTheme)
        {
            return;
        }

        _themeService.ApplyTheme(value.Name);
    }

    private void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
    {
        // Keep the selection in sync if the theme was changed by another caller
        // (e.g. a future palette editor or a keyboard shortcut).
        if (SelectedTheme?.Name != e.CurrentTheme)
        {
            SelectedTheme = Themes.FirstOrDefault(t => t.Name == e.CurrentTheme);
        }
    }
}
