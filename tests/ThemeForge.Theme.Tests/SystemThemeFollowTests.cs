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
using FluentAssertions;
using Xunit;

namespace ThemeForge.Theme.Tests;

[Collection("ThemeService resource dictionary tests")]
public sealed class SystemThemeFollowTests : IDisposable
{
    private const string ThemeMarkerKey = "ThemeForge.ActiveThemeMarker";
    private const string AccentTintMarkerKey = "ThemeForge.ActiveAccentTintMarker";

    public SystemThemeFollowTests()
    {
        ClearTaggedDictionaries();
    }

    public void Dispose()
    {
        ClearTaggedDictionaries();
    }

    [StaFact]
    public void EnableSystemFollow_WithLightMode_AppliesLightTheme()
    {
        FakeSystemThemeProvider provider = new FakeSystemThemeProvider(SystemThemeMode.Light);
        using ThemeService service = new ThemeService(TestApplication.Instance, provider);

        service.EnableSystemFollow(ThemeNames.Folio, ThemeNames.Drakul);

        service.CurrentTheme.Should().Be(ThemeNames.Folio);
        service.IsFollowingSystem.Should().BeTrue();
    }

    [StaFact]
    public void EnableSystemFollow_WithDarkMode_AppliesDarkTheme()
    {
        FakeSystemThemeProvider provider = new FakeSystemThemeProvider(SystemThemeMode.Dark);
        using ThemeService service = new ThemeService(TestApplication.Instance, provider);

        service.EnableSystemFollow(ThemeNames.Folio, ThemeNames.Drakul);

        service.CurrentTheme.Should().Be(ThemeNames.Drakul);
        service.IsFollowingSystem.Should().BeTrue();
    }

    [StaFact]
    public void EnableSystemFollow_WithUnknownMode_KeepsCurrentTheme()
    {
        FakeSystemThemeProvider provider = new FakeSystemThemeProvider(SystemThemeMode.Unknown);
        using ThemeService service = new ThemeService(TestApplication.Instance, provider);
        service.ApplyTheme(ThemeNames.Slate);
        int revision = service.ThemeRevision;

        service.EnableSystemFollow(ThemeNames.Folio, ThemeNames.Drakul);

        service.CurrentTheme.Should().Be(ThemeNames.Slate);
        service.ThemeRevision.Should().Be(revision);
        service.IsFollowingSystem.Should().BeTrue();
    }

    [StaFact]
    public void ProviderChanged_WhenFollowingSystem_SwitchesTheme()
    {
        FakeSystemThemeProvider provider = new FakeSystemThemeProvider(SystemThemeMode.Light);
        using ThemeService service = new ThemeService(TestApplication.Instance, provider);
        service.EnableSystemFollow(ThemeNames.Folio, ThemeNames.Drakul);

        provider.Raise(SystemThemeMode.Dark);

        service.CurrentTheme.Should().Be(ThemeNames.Drakul);
    }

    [StaFact]
    public void DisableSystemFollow_PreventsFutureProviderChanges()
    {
        FakeSystemThemeProvider provider = new FakeSystemThemeProvider(SystemThemeMode.Light);
        using ThemeService service = new ThemeService(TestApplication.Instance, provider);
        service.EnableSystemFollow(ThemeNames.Folio, ThemeNames.Drakul);

        service.DisableSystemFollow();
        provider.Raise(SystemThemeMode.Dark);

        service.CurrentTheme.Should().Be(ThemeNames.Folio);
        service.IsFollowingSystem.Should().BeFalse();
        provider.SubscriberCount.Should().Be(0);
    }

    [StaFact]
    public void ApplyTheme_WhenFollowingSystem_DisablesFollow()
    {
        FakeSystemThemeProvider provider = new FakeSystemThemeProvider(SystemThemeMode.Light);
        using ThemeService service = new ThemeService(TestApplication.Instance, provider);
        service.EnableSystemFollow(ThemeNames.Folio, ThemeNames.Drakul);

        service.ApplyTheme(ThemeNames.Cinder);

        service.CurrentTheme.Should().Be(ThemeNames.Cinder);
        service.IsFollowingSystem.Should().BeFalse();
        provider.SubscriberCount.Should().Be(0);
    }

    [StaFact]
    public void Dispose_RemovesSubscriptionAndDisposesProvider()
    {
        FakeSystemThemeProvider provider = new FakeSystemThemeProvider(SystemThemeMode.Light);
        ThemeService service = new ThemeService(TestApplication.Instance, provider);
        service.EnableSystemFollow(ThemeNames.Folio, ThemeNames.Drakul);

        service.Dispose();

        provider.SubscriberCount.Should().Be(0);
        provider.IsDisposed.Should().BeTrue();
    }

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
            if (merged[i].Contains(ThemeMarkerKey) || merged[i].Contains(AccentTintMarkerKey))
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

        public SystemThemeMode GetCurrentMode()
            => Mode;

        public void Raise(SystemThemeMode mode)
        {
            Mode = mode;
            _changed?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
