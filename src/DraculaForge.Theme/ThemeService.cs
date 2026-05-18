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

namespace DraculaForge.Theme;

/// <summary>
/// Default <see cref="IThemeService"/> implementation that swaps a
/// ResourceDictionary in <see cref="Application.Resources"/> at runtime.
/// </summary>
/// <remarks>
/// The active theme dictionary is identified by tagging it with a sentinel
/// resource key (<see cref="ThemeMarkerKey"/>). On every apply, the previous
/// tagged dictionary is removed and the new one is inserted at index 0 so
/// theme brushes resolve before any consumer-defined overrides further down
/// the merge stack.
/// </remarks>
public sealed class ThemeService : IThemeService
{
    private const string ThemeMarkerKey = "DraculaForge.ActiveThemeMarker";
    private const string ThemePackUriFormat =
        "pack://application:,,,/DraculaForge.Theme;component/Themes/{0}.xaml";

    private readonly Application _application;
    private string _currentTheme = string.Empty;
    private int _themeRevision;

    public ThemeService(Application application, IReadOnlyList<string>? availableThemes = null)
    {
        ArgumentNullException.ThrowIfNull(application);
        _application = application;
        AvailableThemes = availableThemes ?? ThemeNames.All;
    }

    public string CurrentTheme => _currentTheme;

    public int ThemeRevision => _themeRevision;

    public IReadOnlyList<string> AvailableThemes { get; }

    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    public void ApplyTheme(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!AvailableThemes.Contains(name, StringComparer.Ordinal))
        {
            throw new ArgumentException(
                $"Theme '{name}' is not in AvailableThemes.", nameof(name));
        }

        if (string.Equals(_currentTheme, name, StringComparison.Ordinal))
        {
            return; // idempotent on no-op
        }

        var newDictionary = LoadThemeDictionary(name);
        var merged = _application.Resources.MergedDictionaries;

        // Drop any previously tagged active theme dictionary.
        for (int i = merged.Count - 1; i >= 0; i--)
        {
            if (merged[i].Contains(ThemeMarkerKey))
            {
                merged.RemoveAt(i);
            }
        }

        // Insert the new dictionary first so theme resources win lookup.
        merged.Insert(0, newDictionary);

        var previous = _currentTheme;
        _currentTheme = name;
        _themeRevision++;

        ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(previous, name, _themeRevision));
    }

    private static ResourceDictionary LoadThemeDictionary(string name)
    {
        var uri = new Uri(string.Format(ThemePackUriFormat, name), UriKind.Absolute);
        var dict = new ResourceDictionary { Source = uri };

        // Tag the dictionary so we can find it again at the next swap. The
        // marker value is irrelevant; only its key presence matters.
        if (!dict.Contains(ThemeMarkerKey))
        {
            dict[ThemeMarkerKey] = name;
        }

        return dict;
    }
}
