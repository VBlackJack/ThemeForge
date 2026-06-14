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
public sealed class ThemeForgeStartupTests
{
    [Fact]
    public void Run_NoStore_AppliesDefaultThemeAndAccent()
    {
        FakeThemeService service = new FakeThemeService();
        ThemeForgeOptions options = new ThemeForgeOptions
        {
            DefaultTheme = ThemeNames.Carmilla,
            DefaultAccentTint = AccentTint.Cyan,
        };
        ThemeForgeStartup startup = Create(service, options, store: null);

        startup.Run();

        service.CurrentTheme.Should().Be(ThemeNames.Carmilla);
        service.CurrentAccentTint.Should().Be(AccentTint.Cyan);
    }

    [Fact]
    public void Run_NoStore_FollowDefault_ArmsFollow()
    {
        FakeThemeService service = new FakeThemeService();
        ThemeForgeOptions options = new ThemeForgeOptions
        {
            FollowWindowsByDefault = true,
            WindowsFollow = Mapping(),
        };
        ThemeForgeStartup startup = Create(service, options, store: null);

        startup.Run();

        service.IsFollowingSystem.Should().BeTrue();
    }

    [Fact]
    public void Run_StoreWithExplicitPreference_AppliesPersistedThemeAndAccent()
    {
        FakeThemeService service = new FakeThemeService();
        RecordingPreferenceStore store = new RecordingPreferenceStore(new ThemePreference
        {
            ThemeName = ThemeNames.Slate,
            AccentTint = AccentTint.Purple,
            FollowWindows = false,
        });
        ThemeForgeOptions options = new ThemeForgeOptions { DefaultTheme = ThemeNames.Carmilla };
        ThemeForgeStartup startup = Create(service, options, store);

        startup.Run();

        service.CurrentTheme.Should().Be(ThemeNames.Slate);
        service.CurrentAccentTint.Should().Be(AccentTint.Purple);
    }

    [Fact]
    public void Run_StoreWithFollowPreference_ReArmsFollowFromOptions()
    {
        FakeThemeService service = new FakeThemeService();
        RecordingPreferenceStore store = new RecordingPreferenceStore(new ThemePreference { FollowWindows = true });
        ThemeForgeOptions options = new ThemeForgeOptions
        {
            DefaultTheme = ThemeNames.Carmilla,
            WindowsFollow = Mapping(),
        };
        ThemeForgeStartup startup = Create(service, options, store);

        startup.Run();

        service.IsFollowingSystem.Should().BeTrue();
    }

    [Fact]
    public void Run_PersistedThemeRemoved_FallsBackToDefaultWithoutThrowing()
    {
        FakeThemeService service = new FakeThemeService();
        RecordingPreferenceStore store = new RecordingPreferenceStore(new ThemePreference
        {
            ThemeName = "GhostTheme",
            FollowWindows = false,
        });
        ThemeForgeOptions options = new ThemeForgeOptions { DefaultTheme = ThemeNames.Carmilla };
        ThemeForgeStartup startup = Create(service, options, store);

        startup.Run();

        service.CurrentTheme.Should().Be(ThemeNames.Carmilla);
    }

    [Fact]
    public void AutoSave_ManualThemeChange_PersistsExplicitIntent()
    {
        FakeThemeService service = new FakeThemeService();
        RecordingPreferenceStore store = new RecordingPreferenceStore();
        ThemeForgeOptions options = new ThemeForgeOptions { DefaultTheme = ThemeNames.Carmilla };
        ThemeForgeStartup startup = Create(service, options, store);
        startup.Run();

        service.ApplyTheme(ThemeNames.Slate);

        store.SaveCount.Should().Be(1);
        store.Saved!.ThemeName.Should().Be(ThemeNames.Slate);
        store.Saved.FollowWindows.Should().BeFalse();
    }

    [Fact]
    public void AutoSave_WhileFollowing_PersistsFollowIntentAndDedupsToggles()
    {
        FakeThemeService service = new FakeThemeService();
        RecordingPreferenceStore store = new RecordingPreferenceStore();
        ThemeForgeOptions options = new ThemeForgeOptions
        {
            DefaultTheme = ThemeNames.Carmilla,
            WindowsFollow = Mapping(),
            FollowWindowsByDefault = true,
        };
        ThemeForgeStartup startup = Create(service, options, store);
        startup.Run();

        service.SimulateSystemThemeChange(ThemeNames.Folio);
        service.SimulateSystemThemeChange(ThemeNames.Drakul);

        store.SaveCount.Should().Be(1);
        store.Saved!.FollowWindows.Should().BeTrue();
        store.Saved.ThemeName.Should().BeNull();
    }

    [Fact]
    public void AutoSave_SaveThrows_RoutesToOnErrorAndDoesNotPropagate()
    {
        FakeThemeService service = new FakeThemeService();
        RecordingPreferenceStore store = new RecordingPreferenceStore { ThrowOnSave = true };
        int errorCount = 0;
        ThemeForgeOptions options = new ThemeForgeOptions
        {
            DefaultTheme = ThemeNames.Carmilla,
            OnError = _ => errorCount++,
        };
        ThemeForgeStartup startup = Create(service, options, store);
        startup.Run();

        Action act = () => service.ApplyTheme(ThemeNames.Slate);

        act.Should().NotThrow();
        errorCount.Should().Be(1);
    }

    [Fact]
    public void Run_CalledTwice_DoesNotDoubleSubscribe()
    {
        FakeThemeService service = new FakeThemeService();
        RecordingPreferenceStore store = new RecordingPreferenceStore();
        ThemeForgeOptions options = new ThemeForgeOptions { DefaultTheme = ThemeNames.Carmilla };
        ThemeForgeStartup startup = Create(service, options, store);

        startup.Run();
        startup.Run();

        service.ThemeChangedSubscriberCount.Should().Be(1);
    }

    [Fact]
    public void Dispose_Unsubscribes_NoFurtherSave()
    {
        FakeThemeService service = new FakeThemeService();
        RecordingPreferenceStore store = new RecordingPreferenceStore();
        ThemeForgeOptions options = new ThemeForgeOptions { DefaultTheme = ThemeNames.Carmilla };
        ThemeForgeStartup startup = Create(service, options, store);
        startup.Run();

        startup.Dispose();
        service.ApplyTheme(ThemeNames.Slate);

        service.ThemeChangedSubscriberCount.Should().Be(0);
        store.SaveCount.Should().Be(0);
    }

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
