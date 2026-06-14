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

/// <summary>
/// Phase B: behavioral tests that exercise the ResourceDictionary swap, event
/// firing semantics, and marker bookkeeping. These tests share a process-wide
/// <see cref="Application"/> instance (WPF singleton) and therefore clean up
/// any tagged dictionary before and after each test.
/// </summary>
[Collection("ThemeService resource dictionary tests")]
public sealed class ThemeServiceBehavioralTests : IDisposable
{
    // Hardcoded mirror of ThemeService.ThemeMarkerKey (private const). These
    // tests validate the marker invariant, so the coupling is intentional.
    private const string ThemeMarkerKey = "ThemeForge.ActiveThemeMarker";
    private const string AccentTintMarkerKey = "ThemeForge.ActiveAccentTintMarker";
    private const string SystemAccentMarkerKey = "ThemeForge.ActiveSystemAccentMarker";

    public ThemeServiceBehavioralTests()
    {
        ClearTaggedDictionaries();
    }

    public void Dispose()
    {
        ClearTaggedDictionaries();
    }

    [StaFact]
    public void ApplyTheme_FirstCall_BumpsRevisionAndFiresEvent()
    {
        ThemeService service = new ThemeService(TestApplication.Instance);
        ThemeChangedEventArgs? captured = null;
        service.ThemeChanged += (_, args) => captured = args;

        service.ApplyTheme(ThemeNames.Dracula);

        service.CurrentTheme.Should().Be(ThemeNames.Dracula);
        service.ThemeRevision.Should().Be(1);
        captured.Should().NotBeNull();
        captured!.PreviousTheme.Should().BeEmpty();
        captured.CurrentTheme.Should().Be(ThemeNames.Dracula);
        captured.Revision.Should().Be(1);
    }

    [StaFact]
    public void ApplyTheme_BumpsRevisionBeforeFiringEvent()
    {
        ThemeService service = new ThemeService(TestApplication.Instance);
        int revisionInsideHandler = -1;
        service.ThemeChanged += (sender, _) =>
        {
            revisionInsideHandler = ((IThemeService)sender!).ThemeRevision;
        };

        service.ApplyTheme(ThemeNames.Dracula);

        revisionInsideHandler.Should().Be(1);
    }

    [StaFact]
    public void ApplyTheme_TwiceWithSameName_IsNoOp()
    {
        ThemeService service = new ThemeService(TestApplication.Instance);
        int fireCount = 0;
        service.ThemeChanged += (_, _) => fireCount++;

        service.ApplyTheme(ThemeNames.Dracula);
        service.ApplyTheme(ThemeNames.Dracula);

        service.ThemeRevision.Should().Be(1);
        fireCount.Should().Be(1);
    }

    [StaFact]
    public void ApplyTheme_SequentialApplies_TrackPreviousAndBumpRevision()
    {
        ThemeService service = new ThemeService(TestApplication.Instance);
        List<ThemeChangedEventArgs> events = new List<ThemeChangedEventArgs>();
        service.ThemeChanged += (_, args) => events.Add(args);

        service.ApplyTheme(ThemeNames.Dracula);
        service.ApplyTheme(ThemeNames.Drakul);

        events.Should().HaveCount(2);
        events[0].PreviousTheme.Should().BeEmpty();
        events[0].CurrentTheme.Should().Be(ThemeNames.Dracula);
        events[0].Revision.Should().Be(1);
        events[1].PreviousTheme.Should().Be(ThemeNames.Dracula);
        events[1].CurrentTheme.Should().Be(ThemeNames.Drakul);
        events[1].Revision.Should().Be(2);
    }

    [StaFact]
    public void ApplyTheme_AfterMultipleApplies_OnlyOneTaggedDictionaryRemains()
    {
        ThemeService service = new ThemeService(TestApplication.Instance);
        IList<ResourceDictionary> merged = TestApplication.Instance.Resources.MergedDictionaries;

        service.ApplyTheme(ThemeNames.Dracula);
        service.ApplyTheme(ThemeNames.Drakul);
        service.ApplyTheme(ThemeNames.Striga);

        int tagged = 0;
        foreach (ResourceDictionary dict in merged)
        {
            if (dict.Contains(ThemeMarkerKey))
            {
                tagged++;
            }
        }
        tagged.Should().Be(1);
    }

    [StaFact]
    public void ApplyTheme_InsertsActiveThemeAtIndexZero()
    {
        ThemeService service = new ThemeService(TestApplication.Instance);
        IList<ResourceDictionary> merged = TestApplication.Instance.Resources.MergedDictionaries;

        service.ApplyTheme(ThemeNames.Dracula);

        merged.Should().NotBeEmpty();
        merged[0].Contains(ThemeMarkerKey).Should().BeTrue();
    }

    [StaFact]
    public void ApplyTheme_ForEveryTheme_ExposesSharedDesignTokens()
    {
        ThemeService service = new ThemeService(TestApplication.Instance);

        foreach (string themeName in ThemeNames.All)
        {
            service.ApplyTheme(themeName);

            TestApplication.Instance.TryFindResource("SpacingMd")
                .Should().Be(new Thickness(8), $"{themeName} should merge shared spacing tokens");
            TestApplication.Instance.TryFindResource("RadiusMd")
                .Should().Be(new CornerRadius(4), $"{themeName} should merge shared radius tokens");
            TestApplication.Instance.TryFindResource("FontSizeLg")
                .Should().Be(18d, $"{themeName} should merge shared font size tokens");
        }
    }

    private static void ClearTaggedDictionaries()
    {
        // Application.Current may be null if no [StaFact] has touched
        // TestApplication.Instance yet — nothing to clean in that case.
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
}
