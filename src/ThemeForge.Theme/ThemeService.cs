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

namespace ThemeForge.Theme;

/// <summary>
/// Default <see cref="IThemeService"/> implementation that swaps a
/// ResourceDictionary in <see cref="Application.Resources"/> at runtime.
/// </summary>
/// <remarks>
/// The active theme dictionary is identified by tagging it with a sentinel
/// resource key (<see cref="ThemeMarkerKey"/>). On every apply, the previous
/// tagged dictionary is removed and the new one is inserted at index 0 so
/// base theme resources stay first in the merge stack. Accent tint overrides
/// are appended because WPF resolves duplicate merged-dictionary keys from
/// the last matching dictionary.
/// </remarks>
public sealed partial class ThemeService : IThemeService, ISystemThemeFollower, IDisposable
{
    private const string ThemeMarkerKey = "ThemeForge.ActiveThemeMarker";
    private const string AccentTintMarkerKey = "ThemeForge.ActiveAccentTintMarker";
    private const string ThemePackUriFormat =
        "pack://application:,,,/ThemeForge.Theme;component/Themes/{0}.xaml";
    private const double AccentTintLightDelta = 0.08;

    private readonly Application _application;
    private string _currentTheme = string.Empty;
    private AccentTint _currentAccentTint = AccentTint.Default;
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

    public IReadOnlyList<AccentTint> AvailableAccentTints => AccentTints.All;

    public AccentTint CurrentAccentTint => _currentAccentTint;

    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    public void ApplyTheme(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!AvailableThemes.Contains(name, StringComparer.Ordinal))
        {
            throw new ArgumentException(
                $"Theme '{name}' is not in AvailableThemes.", nameof(name));
        }

        if (IsFollowingSystem && !_applyingFromFollow)
        {
            DisableSystemFollow();
        }

        if (string.Equals(_currentTheme, name, StringComparison.Ordinal))
        {
            return; // idempotent on no-op
        }

        ResourceDictionary newDictionary = LoadThemeDictionary(name);
        IList<ResourceDictionary> merged = _application.Resources.MergedDictionaries;

        RemoveMarkedDictionary(merged, AccentTintMarkerKey);
        RemoveMarkedDictionary(merged, ThemeMarkerKey);

        // Keep the active base theme at the front; tint overrides are appended
        // when present because WPF gives later duplicate keys precedence.
        merged.Insert(0, newDictionary);
        if (_currentAccentTint != AccentTint.Default)
        {
            ResourceDictionary tintDictionary = CreateAccentTintDictionary(_currentAccentTint);
            // WPF lets later merged dictionaries override earlier duplicate keys.
            merged.Add(tintDictionary);
        }

        string previous = _currentTheme;
        _currentTheme = name;
        _themeRevision++;

        ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(previous, name, _themeRevision));
    }

    public void ApplyAccentTint(AccentTint tint)
    {
        if (tint == _currentAccentTint)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_currentTheme))
        {
            throw new InvalidOperationException("Apply a theme before applying an accent tint.");
        }

        IList<ResourceDictionary> merged = _application.Resources.MergedDictionaries;
        RemoveMarkedDictionary(merged, AccentTintMarkerKey);

        _currentAccentTint = tint;
        if (tint != AccentTint.Default)
        {
            ResourceDictionary tintDictionary = CreateAccentTintDictionary(tint);
            // WPF lets later merged dictionaries override earlier duplicate keys.
            merged.Add(tintDictionary);
        }

        _themeRevision++;
        ThemeChanged?.Invoke(
            this,
            new ThemeChangedEventArgs(_currentTheme, _currentTheme, _themeRevision));
    }

    private static ResourceDictionary LoadThemeDictionary(string name)
    {
        Uri uri = new Uri(string.Format(ThemePackUriFormat, name), UriKind.Absolute);
        ResourceDictionary dict = new ResourceDictionary { Source = uri };

        // Tag the dictionary so we can find it again at the next swap. The
        // marker value is irrelevant; only its key presence matters.
        if (!dict.Contains(ThemeMarkerKey))
        {
            dict[ThemeMarkerKey] = name;
        }

        return dict;
    }

    private static void RemoveMarkedDictionary(IList<ResourceDictionary> merged, string markerKey)
    {
        for (int i = merged.Count - 1; i >= 0; i--)
        {
            if (merged[i].Contains(markerKey))
            {
                merged.RemoveAt(i);
            }
        }
    }
}
