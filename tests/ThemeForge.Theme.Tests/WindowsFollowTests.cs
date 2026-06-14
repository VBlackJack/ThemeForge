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
using FluentAssertions;
using Xunit;

namespace ThemeForge.Theme.Tests;

[Collection("ThemeService resource dictionary tests")]
public sealed class WindowsFollowTests : IDisposable
{
    private const string ThemeMarkerKey = "ThemeForge.ActiveThemeMarker";
    private const string AccentTintMarkerKey = "ThemeForge.ActiveAccentTintMarker";
    private const string SystemAccentMarkerKey = "ThemeForge.ActiveSystemAccentMarker";

    public WindowsFollowTests()
    {
        ClearTaggedDictionaries();
    }

    public void Dispose()
    {
        ClearTaggedDictionaries();
    }

    [StaFact]
    public void FollowWindows_WithFollowAccent_ArmsBothAxes()
    {
        Color accent = Color.FromRgb(17, 34, 51);
        FakeSystemThemeProvider themeProvider = new FakeSystemThemeProvider(SystemThemeMode.Dark);
        FakeSystemAccentProvider accentProvider = new FakeSystemAccentProvider(accent);
        using ThemeService service = new ThemeService(
            TestApplication.Instance, themeProvider, accentProvider);

        service.FollowWindows(new WindowsFollowOptions
        {
            LightTheme = ThemeNames.Folio,
            DarkTheme = ThemeNames.Drakul,
            FollowAccent = true,
        });

        service.CurrentTheme.Should().Be(ThemeNames.Drakul);
        service.IsFollowingSystem.Should().BeTrue();
        service.IsFollowingSystemAccent.Should().BeTrue();
        ReadBrushColor("AccentBrush").Should().Be(accent);
    }

    [StaFact]
    public void FollowWindows_WithoutFollowAccent_ArmsThemeAxisOnly()
    {
        FakeSystemThemeProvider themeProvider = new FakeSystemThemeProvider(SystemThemeMode.Dark);
        FakeSystemAccentProvider accentProvider = new FakeSystemAccentProvider(Color.FromRgb(17, 34, 51));
        using ThemeService service = new ThemeService(
            TestApplication.Instance, themeProvider, accentProvider);

        service.FollowWindows(new WindowsFollowOptions
        {
            LightTheme = ThemeNames.Folio,
            DarkTheme = ThemeNames.Drakul,
            FollowAccent = false,
        });

        service.IsFollowingSystem.Should().BeTrue();
        service.IsFollowingSystemAccent.Should().BeFalse();
        CountMarked(SystemAccentMarkerKey).Should().Be(0);
    }

    [StaFact]
    public void FollowWindows_DefaultsFollowAccentToTrue()
    {
        FakeSystemThemeProvider themeProvider = new FakeSystemThemeProvider(SystemThemeMode.Dark);
        FakeSystemAccentProvider accentProvider = new FakeSystemAccentProvider(Color.FromRgb(17, 34, 51));
        using ThemeService service = new ThemeService(
            TestApplication.Instance, themeProvider, accentProvider);

        service.FollowWindows(new WindowsFollowOptions
        {
            LightTheme = ThemeNames.Folio,
            DarkTheme = ThemeNames.Drakul,
        });

        service.IsFollowingSystemAccent.Should().BeTrue();
    }

    [StaFact]
    public void FollowWindows_WithLightMode_AppliesLightTheme()
    {
        FakeSystemThemeProvider themeProvider = new FakeSystemThemeProvider(SystemThemeMode.Light);
        FakeSystemAccentProvider accentProvider = new FakeSystemAccentProvider(Color.FromRgb(17, 34, 51));
        using ThemeService service = new ThemeService(
            TestApplication.Instance, themeProvider, accentProvider);

        service.FollowWindows(new WindowsFollowOptions
        {
            LightTheme = ThemeNames.Folio,
            DarkTheme = ThemeNames.Drakul,
        });

        service.CurrentTheme.Should().Be(ThemeNames.Folio);
    }

    [StaFact]
    public void FollowWindows_WithUnknownMode_KeepsCurrentThemeAndRevision()
    {
        FakeSystemThemeProvider themeProvider = new FakeSystemThemeProvider(SystemThemeMode.Unknown);
        FakeSystemAccentProvider accentProvider = new FakeSystemAccentProvider(Color.FromRgb(17, 34, 51));
        using ThemeService service = new ThemeService(
            TestApplication.Instance, themeProvider, accentProvider);
        service.ApplyTheme(ThemeNames.Slate);
        int revision = service.ThemeRevision;

        service.FollowWindows(new WindowsFollowOptions
        {
            LightTheme = ThemeNames.Folio,
            DarkTheme = ThemeNames.Drakul,
            FollowAccent = false,
        });

        service.CurrentTheme.Should().Be(ThemeNames.Slate);
        service.ThemeRevision.Should().Be(revision);
        service.IsFollowingSystem.Should().BeTrue();
    }

    [StaFact]
    public void FollowWindows_ProviderChange_SwitchesThemeAndAccent()
    {
        FakeSystemThemeProvider themeProvider = new FakeSystemThemeProvider(SystemThemeMode.Light);
        FakeSystemAccentProvider accentProvider = new FakeSystemAccentProvider(Color.FromRgb(17, 34, 51));
        using ThemeService service = new ThemeService(
            TestApplication.Instance, themeProvider, accentProvider);
        service.FollowWindows(new WindowsFollowOptions
        {
            LightTheme = ThemeNames.Folio,
            DarkTheme = ThemeNames.Drakul,
        });

        themeProvider.Raise(SystemThemeMode.Dark);
        accentProvider.Raise(Color.FromRgb(90, 80, 70));

        service.CurrentTheme.Should().Be(ThemeNames.Drakul);
        ReadBrushColor("AccentBrush").Should().Be(Color.FromRgb(90, 80, 70));
    }

    [StaFact]
    public void ApplyTheme_AfterFollowWindows_DisablesThemeFollow()
    {
        FakeSystemThemeProvider themeProvider = new FakeSystemThemeProvider(SystemThemeMode.Light);
        FakeSystemAccentProvider accentProvider = new FakeSystemAccentProvider(Color.FromRgb(17, 34, 51));
        using ThemeService service = new ThemeService(
            TestApplication.Instance, themeProvider, accentProvider);
        service.FollowWindows(new WindowsFollowOptions
        {
            LightTheme = ThemeNames.Folio,
            DarkTheme = ThemeNames.Drakul,
        });

        service.ApplyTheme(ThemeNames.Cinder);

        service.CurrentTheme.Should().Be(ThemeNames.Cinder);
        service.IsFollowingSystem.Should().BeFalse();
        themeProvider.SubscriberCount.Should().Be(0);
    }

    [StaFact]
    public void FollowWindows_WithNullOptions_Throws()
    {
        FakeSystemThemeProvider themeProvider = new FakeSystemThemeProvider(SystemThemeMode.Dark);
        FakeSystemAccentProvider accentProvider = new FakeSystemAccentProvider(Color.FromRgb(17, 34, 51));
        using ThemeService service = new ThemeService(
            TestApplication.Instance, themeProvider, accentProvider);

        Action act = () => service.FollowWindows(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [StaFact]
    public void FollowWindows_WithUnknownTheme_Throws()
    {
        FakeSystemThemeProvider themeProvider = new FakeSystemThemeProvider(SystemThemeMode.Dark);
        FakeSystemAccentProvider accentProvider = new FakeSystemAccentProvider(Color.FromRgb(17, 34, 51));
        using ThemeService service = new ThemeService(
            TestApplication.Instance, themeProvider, accentProvider);

        Action act = () => service.FollowWindows(new WindowsFollowOptions
        {
            LightTheme = "NotARealTheme",
            DarkTheme = ThemeNames.Drakul,
        });

        act.Should().Throw<ArgumentException>();
    }

    private static Color ReadBrushColor(string key) =>
        (TestApplication.Instance.Resources[key] as SolidColorBrush
            ?? throw new InvalidOperationException($"Resource '{key}' must resolve to a SolidColorBrush.")).Color;

    private static int CountMarked(string markerKey) =>
        TestApplication.Instance.Resources.MergedDictionaries.Count(dict => dict.Contains(markerKey));

    private static void ClearTaggedDictionaries()
    {
        Application? app = Application.Current;
        if (app is null)
        {
            return;
        }

        IList<ResourceDictionary> merged = app.Resources.MergedDictionaries;
        for (int i = merged.Count - 1; i >= 0; i--)
        {
            if (merged[i].Contains(ThemeMarkerKey) ||
                merged[i].Contains(AccentTintMarkerKey) ||
                merged[i].Contains(SystemAccentMarkerKey))
            {
                merged.RemoveAt(i);
            }
        }
    }

    private sealed class FakeSystemThemeProvider : ISystemThemeProvider, IDisposable
    {
        private EventHandler? _changed;

        public FakeSystemThemeProvider(SystemThemeMode mode)
        {
            Mode = mode;
        }

        public SystemThemeMode Mode { get; private set; }
        public bool IsDisposed { get; private set; }
        public int SubscriberCount => _changed?.GetInvocationList().Length ?? 0;

        public event EventHandler? Changed
        {
            add => _changed += value;
            remove => _changed -= value;
        }

        public SystemThemeMode GetCurrentMode() => Mode;

        public void Raise(SystemThemeMode mode)
        {
            Mode = mode;
            _changed?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose() => IsDisposed = true;
    }

    private sealed class FakeSystemAccentProvider : ISystemAccentProvider, IDisposable
    {
        private EventHandler? _changed;

        public FakeSystemAccentProvider(Color? accent)
        {
            Accent = accent;
        }

        public Color? Accent { get; private set; }
        public bool IsDisposed { get; private set; }
        public int SubscriberCount => _changed?.GetInvocationList().Length ?? 0;

        public event EventHandler? Changed
        {
            add => _changed += value;
            remove => _changed -= value;
        }

        public Color? GetCurrentAccent() => Accent;

        public void Raise(Color? accent)
        {
            Accent = accent;
            _changed?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose() => IsDisposed = true;
    }
}
