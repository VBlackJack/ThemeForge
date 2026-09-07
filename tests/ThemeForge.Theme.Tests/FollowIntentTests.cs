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

using System.Windows.Media;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ThemeForge.Theme.DependencyInjection;
using ThemeForge.Theme.Persistence;

namespace ThemeForge.Theme.Tests;

[Collection("ThemeService resource dictionary tests")]
public sealed class FollowIntentTests
{
    [StaFact]
    public void SameThemeFollowTransitions_ArePersistedAndStopAfterDisposal()
    {
        using ThemeService service = new ThemeService(TestApplication.Instance, new FixedThemeProvider());
        RecordingStore store = new RecordingStore();
        ServiceCollection services = new ServiceCollection();
        services.AddSingleton<IThemeService>(service);
        services.AddThemeForge(TestApplication.Instance, options =>
        {
            options.DefaultTheme = ThemeNames.Dracula;
            options.PreferenceStore = store;
        });
        using ServiceProvider provider = services.BuildServiceProvider();
        IDisposable startup = provider.UseThemeForge();
        int revision = service.ThemeRevision;

        service.EnableSystemFollow(ThemeNames.Folio, ThemeNames.Dracula);
        store.Last!.FollowWindows.Should().BeTrue();
        service.ApplyTheme(ThemeNames.Dracula);
        store.Last!.FollowWindows.Should().BeFalse();
        store.Last.ThemeName.Should().Be(ThemeNames.Dracula);
        service.ThemeRevision.Should().Be(revision);

        service.EnableSystemFollow(ThemeNames.Folio, ThemeNames.Dracula);
        service.DisableSystemFollow();
        store.Last!.FollowWindows.Should().BeFalse();
        startup.Dispose();
        int count = store.Count;
        service.EnableSystemFollow(ThemeNames.Folio, ThemeNames.Dracula);
        store.Count.Should().Be(count);
    }

    [StaFact]
    public void Dispose_Service_DoesNotSaveTeardownAsManualPreference()
    {
        ThemeService service = new ThemeService(TestApplication.Instance, new FixedThemeProvider(), new FixedAccentProvider());
        RecordingStore store = new RecordingStore();
        ThemeForgeStartup startup = new ThemeForgeStartup(service, service, service,
            new ThemeForgeOptions { DefaultTheme = ThemeNames.Dracula }, store);
        startup.Run();
        service.FollowWindows(new WindowsFollowOptions { LightTheme = ThemeNames.Folio, DarkTheme = ThemeNames.Dracula });
        int count = store.Count;
        service.Dispose();
        store.Count.Should().Be(count);
        store.Last!.FollowWindows.Should().BeTrue();
        startup.Dispose();
    }

    [StaFact]
    public void FollowAccentFalse_RemovesExistingSubscriptionAndOverride()
    {
        FixedAccentProvider accent = new FixedAccentProvider();
        using ThemeService service = new ThemeService(TestApplication.Instance, new FixedThemeProvider(), accent);
        service.FollowWindows(new WindowsFollowOptions { LightTheme = ThemeNames.Folio, DarkTheme = ThemeNames.Dracula });
        service.FollowWindows(new WindowsFollowOptions
        {
            LightTheme = ThemeNames.Folio, DarkTheme = ThemeNames.Dracula, FollowAccent = false,
        });
        service.IsFollowingSystemAccent.Should().BeFalse();
        accent.Subscribers.Should().Be(0);
        TestApplication.Instance.Resources.MergedDictionaries.Should().NotContain(
            dictionary => dictionary.Contains("ThemeForge.ActiveSystemAccentMarker"));
    }

    private sealed class FixedThemeProvider : ISystemThemeProvider
    {
        public SystemThemeMode GetCurrentMode() => SystemThemeMode.Dark;
        public event EventHandler? Changed { add { } remove { } }
    }

    private sealed class FixedAccentProvider : ISystemAccentProvider
    {
        public int Subscribers { get; private set; }
        public Color? GetCurrentAccent() => Colors.Blue;
        public event EventHandler? Changed { add => Subscribers++; remove => Subscribers = Math.Max(0, Subscribers - 1); }
    }

    private sealed class RecordingStore : IThemePreferenceStore
    {
        public int Count { get; private set; }
        public ThemePreference? Last { get; private set; }
        public ThemePreference? Load() => null;
        public void Save(ThemePreference preference) { Count++; Last = preference; }
    }
}
