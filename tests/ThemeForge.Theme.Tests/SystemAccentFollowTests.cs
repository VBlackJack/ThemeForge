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
public sealed class SystemAccentFollowTests : IDisposable
{
    private const string ThemeMarkerKey = "ThemeForge.ActiveThemeMarker";
    private const string AccentTintMarkerKey = "ThemeForge.ActiveAccentTintMarker";
    private const string SystemAccentMarkerKey = "ThemeForge.ActiveSystemAccentMarker";

    public SystemAccentFollowTests()
    {
        ClearTaggedDictionaries();
    }

    public void Dispose()
    {
        ClearTaggedDictionaries();
    }

    [StaFact]
    public void EnableSystemAccentFollow_AppliesOpaqueSystemAccent()
    {
        Color color = Color.FromRgb(17, 34, 51);
        using ThemeService service = CreateService(color);

        service.EnableSystemAccentFollow();

        ReadBrushColor("AccentBrush").Should().Be(color);
        ReadBrushColor("AccentBrush").A.Should().Be(255);
        service.IsFollowingSystemAccent.Should().BeTrue();
    }

    [StaFact]
    public void ProviderChanged_WhenFollowingSystemAccent_ReappliesAccent()
    {
        FakeSystemAccentProvider provider = new FakeSystemAccentProvider(Color.FromRgb(17, 34, 51));
        using ThemeService service = CreateService(provider);

        service.EnableSystemAccentFollow();
        provider.Raise(Color.FromRgb(90, 80, 70));

        ReadBrushColor("AccentBrush").Should().Be(Color.FromRgb(90, 80, 70));
    }

    [StaFact]
    public void DisableSystemAccentFollow_RemovesOverride()
    {
        using ThemeService service = CreateService(Color.FromRgb(17, 34, 51));
        Color themeAccent = ReadBrushColor("AccentBrush");
        service.EnableSystemAccentFollow();

        service.DisableSystemAccentFollow();

        ReadBrushColor("AccentBrush").Should().Be(themeAccent);
        service.IsFollowingSystemAccent.Should().BeFalse();
        CountMarked(SystemAccentMarkerKey).Should().Be(0);
    }

    [StaFact]
    public void EnableSystemAccentFollow_WithActiveTint_ClearsCurrentAccentTint()
    {
        using ThemeService service = CreateService(Color.FromRgb(17, 34, 51));
        service.ApplyAccentTint(AccentTint.Cyan);

        service.EnableSystemAccentFollow();

        service.CurrentAccentTint.Should().Be(AccentTint.Default);
        ReadBrushColor("AccentBrush").Should().Be(Color.FromRgb(17, 34, 51));
        CountMarked(AccentTintMarkerKey).Should().Be(0);
    }

    [StaFact]
    public void ApplyAccentTint_WhenFollowingSystemAccent_DisablesFollow()
    {
        using ThemeService service = CreateService(Color.FromRgb(17, 34, 51));
        service.EnableSystemAccentFollow();

        service.ApplyAccentTint(AccentTint.Purple);

        service.IsFollowingSystemAccent.Should().BeFalse();
        service.CurrentAccentTint.Should().Be(AccentTint.Purple);
        ReadBrushColor("AccentBrush").Should().Be(ReadBrushColor("PurpleBrush"));
    }

    [StaFact]
    public void ApplyTheme_WhenFollowingSystemAccent_KeepsSystemAccent()
    {
        using ThemeService service = CreateService(Color.FromRgb(17, 34, 51));
        service.EnableSystemAccentFollow();

        service.ApplyTheme(ThemeNames.Parchment);

        service.IsFollowingSystemAccent.Should().BeTrue();
        service.CurrentAccentTint.Should().Be(AccentTint.Default);
        ReadBrushColor("AccentBrush").Should().Be(Color.FromRgb(17, 34, 51));
        CountMarked(SystemAccentMarkerKey).Should().Be(1);
    }

    [StaFact]
    public void Dispose_RemovesSubscriptionAndDisposesProvider()
    {
        FakeSystemAccentProvider provider = new FakeSystemAccentProvider(Color.FromRgb(17, 34, 51));
        ThemeService service = CreateService(provider);
        service.EnableSystemAccentFollow();

        service.Dispose();

        provider.SubscriberCount.Should().Be(0);
        provider.IsDisposed.Should().BeTrue();
    }

    private static ThemeService CreateService(Color color) =>
        CreateService(new FakeSystemAccentProvider(color));

    private static ThemeService CreateService(FakeSystemAccentProvider provider)
    {
        ThemeService service = new ThemeService(TestApplication.Instance, provider);
        service.ApplyTheme(ThemeNames.Dracula);
        return service;
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
