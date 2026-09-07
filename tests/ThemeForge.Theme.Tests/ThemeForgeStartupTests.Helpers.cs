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

using System.IO;
using FluentAssertions;
using ThemeForge.Theme.DependencyInjection;
using ThemeForge.Theme.Persistence;
using Xunit;

namespace ThemeForge.Theme.Tests;

/// <summary>
/// Pure orchestration tests for the bootstrap. No WPF Application and no STA: the
/// validation of Model A is that the orchestrator runs entirely against fakes.
/// </summary>
public sealed partial class ThemeForgeStartupTests
{
    private static ThemeForgeStartup Create(
        FakeThemeService service, ThemeForgeOptions options, IThemePreferenceStore? store) =>
        new ThemeForgeStartup(service, service, service, options, store);

    private static WindowsFollowOptions Mapping() =>
        new WindowsFollowOptions { LightTheme = ThemeNames.Folio, DarkTheme = ThemeNames.Drakul };

    private sealed class FakeThemeService :
        IThemeService, ISystemThemeFollower, ISystemAccentFollower, IWindowsThemeFollower
    {
        private EventHandler<ThemeChangedEventArgs>? _themeChanged;
        private int _revision;

        public FakeThemeService(IReadOnlyList<string>? availableThemes = null)
        {
            AvailableThemes = availableThemes ?? ThemeNames.All;
        }

        public string CurrentTheme { get; private set; } = string.Empty;
        public int ThemeRevision => _revision;
        public IReadOnlyList<string> AvailableThemes { get; }
        public IReadOnlyList<AccentTint> AvailableAccentTints { get; } = AccentTints.All;
        public AccentTint CurrentAccentTint { get; private set; } = AccentTint.Default;
        public bool IsFollowingSystem { get; private set; }
        public bool IsFollowingSystemAccent { get; private set; }
        public int ThemeChangedSubscriberCount => _themeChanged?.GetInvocationList().Length ?? 0;

        public event EventHandler<ThemeChangedEventArgs>? ThemeChanged
        {
            add => _themeChanged += value;
            remove => _themeChanged -= value;
        }

        public void ApplyTheme(string name)
        {
            IsFollowingSystem = false;
            IsFollowingSystemAccent = false;
            CurrentTheme = name;
            Raise();
        }

        public void ApplyAccentTint(AccentTint tint)
        {
            IsFollowingSystemAccent = false;
            CurrentAccentTint = tint;
            Raise();
        }

        public void EnableSystemFollow(string lightTheme, string darkTheme)
        {
            IsFollowingSystem = true;
            CurrentTheme = darkTheme;
            Raise();
        }

        public void DisableSystemFollow() => IsFollowingSystem = false;

        public void EnableSystemAccentFollow() => IsFollowingSystemAccent = true;

        public void DisableSystemAccentFollow() => IsFollowingSystemAccent = false;

        public void FollowWindows(WindowsFollowOptions options)
        {
            IsFollowingSystem = true;
            IsFollowingSystemAccent = options.FollowAccent;
            CurrentTheme = options.DarkTheme;
            Raise();
        }

        public void SimulateSystemThemeChange(string resolvedTheme)
        {
            CurrentTheme = resolvedTheme;
            Raise();
        }

        private void Raise()
        {
            _revision++;
            _themeChanged?.Invoke(this, new ThemeChangedEventArgs(CurrentTheme, CurrentTheme, _revision));
        }
    }

    private sealed class RecordingPreferenceStore : IThemePreferenceStore
    {
        private readonly ThemePreference? _initial;

        public RecordingPreferenceStore(ThemePreference? initial = null)
        {
            _initial = initial;
        }

        public int SaveCount { get; private set; }
        public ThemePreference? Saved { get; private set; }
        public bool ThrowOnSave { get; set; }

        public ThemePreference? Load() => _initial;

        public void Save(ThemePreference preference)
        {
            if (ThrowOnSave)
            {
                throw new IOException("Simulated write failure.");
            }

            SaveCount++;
            Saved = preference;
        }
    }
}
