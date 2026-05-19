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

namespace ThemeForge.Theme.Tests;

[Collection("ThemeService resource dictionary tests")]
public sealed class AccentTintTests : IDisposable
{
    private const string ThemeMarkerKey = "ThemeForge.ActiveThemeMarker";
    private const string AccentTintMarkerKey = "ThemeForge.ActiveAccentTintMarker";

    public AccentTintTests()
    {
        ClearTaggedDictionaries();
    }

    public void Dispose()
    {
        ClearTaggedDictionaries();
    }

    [StaFact]
    public void ApplyAccentTint_DefaultOnFreshService_IsNoOp()
    {
        ThemeService service = new ThemeService(TestApplication.Instance);
        int fireCount = 0;
        service.ThemeChanged += (_, _) => fireCount++;

        service.ApplyAccentTint(AccentTint.Default);

        service.CurrentAccentTint.Should().Be(AccentTint.Default);
        service.ThemeRevision.Should().Be(0);
        fireCount.Should().Be(0);
    }

    [StaFact]
    public void ApplyAccentTint_CyanAfterTheme_PatchesAccentBrush()
    {
        ThemeService service = new ThemeService(TestApplication.Instance);
        service.ApplyTheme(ThemeNames.Dracula);
        Color expected = ReadBrushColor("CyanBrush");

        service.ApplyAccentTint(AccentTint.Cyan);

        ReadBrushColor("AccentBrush").Should().Be(expected);
    }

    [StaFact]
    public void ApplyAccentTint_CyanAfterTheme_DerivesHoverAndPressed()
    {
        ThemeService service = new ThemeService(TestApplication.Instance);
        service.ApplyTheme(ThemeNames.Dracula);

        service.ApplyAccentTint(AccentTint.Cyan);

        Color accentColor = ReadBrushColor("AccentBrush");
        Color hoverColor = ReadBrushColor("AccentHoverBrush");
        Color pressedColor = ReadBrushColor("AccentPressedBrush");
        OklabConverter.Oklab accent = OklabConverter.FromColor(accentColor);
        Color expectedHover = OklabConverter.ToColor(OklabConverter.Lighten(accent, 0.08));
        Color expectedPressed = OklabConverter.ToColor(OklabConverter.Darken(accent, 0.08));

        hoverColor.Should().Be(expectedHover);
        pressedColor.Should().Be(expectedPressed);
        OklabConverter.FromColor(hoverColor).L.Should().BeGreaterThan(accent.L);
        OklabConverter.FromColor(pressedColor).L.Should().BeLessThan(accent.L);
    }

    [StaFact]
    public void ApplyAccentTint_ThenDefault_RestoresThemeAccent()
    {
        ThemeService service = new ThemeService(TestApplication.Instance);
        service.ApplyTheme(ThemeNames.Dracula);
        Color expected = ReadBrushColor("AccentBrush");

        service.ApplyAccentTint(AccentTint.Cyan);
        service.ApplyAccentTint(AccentTint.Default);

        ReadBrushColor("AccentBrush").Should().Be(expected);
        service.CurrentAccentTint.Should().Be(AccentTint.Default);
    }

    [StaFact]
    public void ApplyAccentTint_PersistsAcrossThemeSwap()
    {
        ThemeService service = new ThemeService(TestApplication.Instance);
        service.ApplyTheme(ThemeNames.Dracula);
        service.ApplyAccentTint(AccentTint.Cyan);

        service.ApplyTheme(ThemeNames.Parchment);

        Color expected = ReadBrushColor("CyanBrush");
        service.CurrentAccentTint.Should().Be(AccentTint.Cyan);
        ReadBrushColor("AccentBrush").Should().Be(expected);
    }

    [StaFact]
    public void ApplyAccentTint_BumpsRevisionAndRaisesThemeChangedOnce()
    {
        ThemeService service = new ThemeService(TestApplication.Instance);
        service.ApplyTheme(ThemeNames.Dracula);
        int startRevision = service.ThemeRevision;
        int fireCount = 0;
        ThemeChangedEventArgs? captured = null;
        service.ThemeChanged += (_, args) =>
        {
            fireCount++;
            captured = args;
        };

        service.ApplyAccentTint(AccentTint.Cyan);

        fireCount.Should().Be(1);
        service.ThemeRevision.Should().Be(startRevision + 1);
        captured.Should().NotBeNull();
        captured!.PreviousTheme.Should().Be(ThemeNames.Dracula);
        captured.CurrentTheme.Should().Be(ThemeNames.Dracula);
        captured.Revision.Should().Be(service.ThemeRevision);
    }

    private static Color ReadBrushColor(string key)
    {
        object? resource = TestApplication.Instance.Resources[key];
        SolidColorBrush brush = resource as SolidColorBrush
            ?? throw new InvalidOperationException($"Resource '{key}' must resolve to a SolidColorBrush.");

        return brush.Color;
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
}
