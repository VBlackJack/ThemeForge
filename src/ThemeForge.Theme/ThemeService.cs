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
public sealed partial class ThemeService :
    IThemeService,
    ISystemThemeFollower,
    ISystemAccentFollower,
    IThemeIntentNotifier,
    IDisposable
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
    private bool _disposed;

    public ThemeService(Application application, IReadOnlyList<string>? availableThemes = null)
    {
        ArgumentNullException.ThrowIfNull(application);
        _application = application;
        AvailableThemes = availableThemes ?? ThemeNames.All;
    }

    public string CurrentTheme => _currentTheme;

    public int ThemeRevision => _themeRevision;

    public IReadOnlyList<string> AvailableThemes { get; private set; }

    public IReadOnlyList<AccentTint> AvailableAccentTints => AccentTints.All;

    public AccentTint CurrentAccentTint => _currentAccentTint;

    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    /// <inheritdoc/>
    public event EventHandler? ThemeIntentChanged;

    public void ApplyTheme(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!AvailableThemes.Contains(name, StringComparer.Ordinal))
        {
            throw new ArgumentException(
                $"Theme '{name}' is not in AvailableThemes.", nameof(name));
        }

        bool stopFollowing = IsFollowingSystem && !_applyingFromFollow;
        if (string.Equals(_currentTheme, name, StringComparison.Ordinal))
        {
            if (stopFollowing) { DisableSystemFollow(); }
            return;
        }

        // Prepare everything before changing the active palette or user intent.
        ResourceDictionary newDictionary = LoadExternalOrBuiltIn(name);
        ResourceDictionary? accentDictionary = null;
        if (_currentAccentTint != AccentTint.Default)
        {
            accentDictionary = CreateAccentTintDictionary(_currentAccentTint, newDictionary);
        }
        else if (IsFollowingSystemAccent && _systemAccentProvider?.GetCurrentAccent() is Color color)
        {
            accentDictionary = CreateSystemAccentDictionary(color);
        }

        if (stopFollowing) { StopSystemFollow(); }
        IList<ResourceDictionary> merged = _application.Resources.MergedDictionaries;
        RemoveMarkedDictionary(merged, AccentTintMarkerKey);
        RemoveMarkedDictionary(merged, SystemAccentMarkerKey);
        RemoveMarkedDictionary(merged, ThemeMarkerKey);
        merged.Insert(0, newDictionary);
        if (accentDictionary is not null) { merged.Add(accentDictionary); }

        string previous = _currentTheme;
        _currentTheme = name;
        _themeRevision++;

        ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(previous, name, _themeRevision));
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

    private static bool RemoveMarkedDictionary(IList<ResourceDictionary> merged, string markerKey)
    {
        bool removed = false;
        for (int i = merged.Count - 1; i >= 0; i--)
        {
            if (merged[i].Contains(markerKey))
            {
                merged.RemoveAt(i);
                removed = true;
            }
        }

        return removed;
    }
}
