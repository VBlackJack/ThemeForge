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
using ThemeForge.Theme.Interop;
using Xunit;

namespace ThemeForge.Theme.Tests.Interop;

public sealed class TitleBarPlannerTests
{
    private static readonly Color DarkBackground = Color.FromRgb(0x28, 0x2A, 0x36);
    private static readonly Color LightBackground = Color.FromRgb(0xF8, 0xF8, 0xF2);
    private static readonly Color Foreground = Color.FromRgb(0xF8, 0xF8, 0xF2);
    private static readonly Version Win11 = new Version(10, 0, 22631);
    private static readonly Version Win10Modern = new Version(10, 0, 19045);
    private static readonly Version Win10Legacy = new Version(10, 0, 19000);
    private static readonly Version Win10Old = new Version(10, 0, 18000);

    [Fact]
    public void ResolveColors_NullOptions_DerivesFromTheme()
    {
        (Color caption, Color text, Color? border) =
            TitleBarPlanner.ResolveColors(DarkBackground, Foreground, new TitleBarOptions());

        caption.Should().Be(DarkBackground);
        text.Should().Be(Foreground);
        border.Should().BeNull();
    }

    [Fact]
    public void ResolveColors_Overrides_TakePrecedence()
    {
        TitleBarOptions options = new TitleBarOptions
        {
            CaptionColor = Color.FromRgb(1, 2, 3),
            TextColor = Color.FromRgb(4, 5, 6),
            BorderColor = Color.FromRgb(7, 8, 9),
        };

        (Color caption, Color text, Color? border) =
            TitleBarPlanner.ResolveColors(DarkBackground, Foreground, options);

        caption.Should().Be(Color.FromRgb(1, 2, 3));
        text.Should().Be(Color.FromRgb(4, 5, 6));
        border.Should().Be(Color.FromRgb(7, 8, 9));
    }

    [Fact]
    public void ToColorRef_ConvertsRgbToBgr()
    {
        TitleBarPlanner.ToColorRef(Color.FromRgb(0x11, 0x22, 0x33)).Should().Be(0x00332211);
    }

    [Fact]
    public void BuildPlan_DarkBackground_SetsImmersiveDarkToOne()
    {
        IReadOnlyList<TitleBarPlanner.DwmInstruction> plan =
            TitleBarPlanner.BuildPlan(DarkBackground, DarkBackground, Foreground, null, Win11);

        plan.Should().Contain(new TitleBarPlanner.DwmInstruction(TitleBarPlanner.DwmwaUseImmersiveDarkMode, 1));
    }

    [Fact]
    public void BuildPlan_LightBackground_SetsImmersiveDarkToZero()
    {
        IReadOnlyList<TitleBarPlanner.DwmInstruction> plan =
            TitleBarPlanner.BuildPlan(LightBackground, LightBackground, Foreground, null, Win11);

        plan.Should().Contain(new TitleBarPlanner.DwmInstruction(TitleBarPlanner.DwmwaUseImmersiveDarkMode, 0));
    }

    [Fact]
    public void BuildPlan_Win11_IncludesCaptionAndTextColors()
    {
        IReadOnlyList<TitleBarPlanner.DwmInstruction> plan =
            TitleBarPlanner.BuildPlan(DarkBackground, DarkBackground, Foreground, null, Win11);

        plan.Should().Contain(i => i.Attribute == TitleBarPlanner.DwmwaCaptionColor);
        plan.Should().Contain(i => i.Attribute == TitleBarPlanner.DwmwaTextColor);
    }

    [Fact]
    public void BuildPlan_Win10Modern_OnlyImmersiveDarkNoColors()
    {
        IReadOnlyList<TitleBarPlanner.DwmInstruction> plan =
            TitleBarPlanner.BuildPlan(DarkBackground, DarkBackground, Foreground, null, Win10Modern);

        plan.Should().ContainSingle()
            .Which.Attribute.Should().Be(TitleBarPlanner.DwmwaUseImmersiveDarkMode);
    }

    [Fact]
    public void BuildPlan_Win10Legacy_UsesLegacyImmersiveDarkAttribute()
    {
        IReadOnlyList<TitleBarPlanner.DwmInstruction> plan =
            TitleBarPlanner.BuildPlan(DarkBackground, DarkBackground, Foreground, null, Win10Legacy);

        plan.Should().ContainSingle()
            .Which.Attribute.Should().Be(TitleBarPlanner.DwmwaUseImmersiveDarkModeLegacy);
    }

    [Fact]
    public void BuildPlan_OldOs_SendsNoAttributes()
    {
        IReadOnlyList<TitleBarPlanner.DwmInstruction> plan =
            TitleBarPlanner.BuildPlan(DarkBackground, DarkBackground, Foreground, null, Win10Old);

        plan.Should().BeEmpty();
    }

    [Fact]
    public void BuildPlan_BorderColor_IncludedOnlyWhenSet()
    {
        IReadOnlyList<TitleBarPlanner.DwmInstruction> withoutBorder =
            TitleBarPlanner.BuildPlan(DarkBackground, DarkBackground, Foreground, null, Win11);
        IReadOnlyList<TitleBarPlanner.DwmInstruction> withBorder =
            TitleBarPlanner.BuildPlan(DarkBackground, DarkBackground, Foreground, Color.FromRgb(7, 8, 9), Win11);

        withoutBorder.Should().NotContain(i => i.Attribute == TitleBarPlanner.DwmwaBorderColor);
        withBorder.Should().Contain(i => i.Attribute == TitleBarPlanner.DwmwaBorderColor);
    }
}
