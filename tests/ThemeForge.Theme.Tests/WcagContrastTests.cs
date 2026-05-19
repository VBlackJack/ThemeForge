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

/// <summary>
/// Runtime contrast gate for on-accent and on-semantic surfaces.
/// </summary>
[Collection("ThemeService resource dictionary tests")]
public sealed class WcagContrastTests : IDisposable
{
    private const string ThemeMarkerKey = "ThemeForge.ActiveThemeMarker";
    private const double AaNormalTextMinimum = 4.5;

    public WcagContrastTests()
    {
        ClearTaggedDictionaries();
    }

    public void Dispose()
    {
        ClearTaggedDictionaries();
    }

    [StaFact]
    public void ApplyTheme_ForEveryTheme_MeetsAaContrastAgainstBackground()
    {
        ThemeService service = new ThemeService(TestApplication.Instance);
        string[] pairedKeys = new[]
        {
            "AccentBrush",
            "SuccessBrush",
            "WarningBrush",
            "ErrorBrush",
            "InfoBrush",
        };

        foreach (string themeName in ThemeNames.All)
        {
            service.ApplyTheme(themeName);

            Color background = ReadBrushColor("BackgroundBrush");
            foreach (string pairedKey in pairedKeys)
            {
                Color other = ReadBrushColor(pairedKey);
                double ratio = ContrastRatio(background, other);

                ratio.Should().BeGreaterThanOrEqualTo(
                    AaNormalTextMinimum,
                    "BackgroundBrush vs {0} on theme '{1}' must meet WCAG AA (got {2:F2}:1)",
                    pairedKey,
                    themeName,
                    ratio);
            }
        }
    }

    private static Color ReadBrushColor(string key)
    {
        object? resource = Application.Current?.Resources[key];
        SolidColorBrush brush = resource as SolidColorBrush
            ?? throw new InvalidOperationException($"Resource '{key}' must resolve to a SolidColorBrush.");

        return brush.Color;
    }

    private static double SrgbToLinear(double channel)
    {
        if (channel <= 0.03928)
        {
            return channel / 12.92;
        }

        return Math.Pow((channel + 0.055) / 1.055, 2.4);
    }

    private static double RelativeLuminance(Color color)
    {
        double r = SrgbToLinear(color.R / 255d);
        double g = SrgbToLinear(color.G / 255d);
        double b = SrgbToLinear(color.B / 255d);

        return (0.2126 * r) + (0.7152 * g) + (0.0722 * b);
    }

    private static double ContrastRatio(Color a, Color b)
    {
        double la = RelativeLuminance(a);
        double lb = RelativeLuminance(b);
        double lighter = Math.Max(la, lb);
        double darker = Math.Min(la, lb);

        return (lighter + 0.05) / (darker + 0.05);
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
            if (merged[i].Contains(ThemeMarkerKey))
            {
                merged.RemoveAt(i);
            }
        }
    }
}
