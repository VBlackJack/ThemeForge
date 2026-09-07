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
public sealed partial class WindowsFollowTests : IDisposable
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

}
