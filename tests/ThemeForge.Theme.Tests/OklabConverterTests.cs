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
using Xunit;

namespace ThemeForge.Theme.Tests;

public sealed class OklabConverterTests
{
    [Fact]
    public void RoundTrip_RedHex_PreservesColorWithin1Unit()
        => AssertRoundTrip(Color.FromRgb(0xFF, 0x55, 0x55));

    [Fact]
    public void RoundTrip_CyanHex_PreservesColorWithin1Unit()
        => AssertRoundTrip(Color.FromRgb(0x8B, 0xE9, 0xFD));

    [Fact]
    public void RoundTrip_GreenHex_PreservesColorWithin1Unit()
        => AssertRoundTrip(Color.FromRgb(0x50, 0xFA, 0x7B));

    [Fact]
    public void RoundTrip_NearWhiteHex_PreservesColorWithin1Unit()
        => AssertRoundTrip(Color.FromRgb(0xF8, 0xF8, 0xF2));

    [Fact]
    public void Lighten_IncreasesL_ByDelta()
    {
        OklabConverter.Oklab lab = OklabConverter.FromColor(Color.FromRgb(0xBD, 0x93, 0xF9));

        OklabConverter.Oklab lighter = OklabConverter.Lighten(lab, 0.08);

        lighter.L.Should().BeApproximately(lab.L + 0.08, 0.000000001);
    }

    [Fact]
    public void Darken_DecreasesL_ByDelta()
    {
        OklabConverter.Oklab lab = OklabConverter.FromColor(Color.FromRgb(0xBD, 0x93, 0xF9));

        OklabConverter.Oklab darker = OklabConverter.Darken(lab, 0.08);

        darker.L.Should().BeApproximately(lab.L - 0.08, 0.000000001);
    }

    [Fact]
    public void Lighten_AtMaxL_ClampsToOne()
    {
        OklabConverter.Oklab lab = new OklabConverter.Oklab(0.98, 0.01, -0.01);

        OklabConverter.Oklab lighter = OklabConverter.Lighten(lab, 0.08);

        lighter.L.Should().Be(1.0);
    }

    [Fact]
    public void Darken_AtMinL_ClampsToZero()
    {
        OklabConverter.Oklab lab = new OklabConverter.Oklab(0.02, 0.01, -0.01);

        OklabConverter.Oklab darker = OklabConverter.Darken(lab, 0.08);

        darker.L.Should().Be(0.0);
    }

    private static void AssertRoundTrip(Color expected)
    {
        OklabConverter.Oklab lab = OklabConverter.FromColor(expected);
        Color actual = OklabConverter.ToColor(lab);

        Math.Abs(actual.R - expected.R).Should().BeLessThanOrEqualTo(1);
        Math.Abs(actual.G - expected.G).Should().BeLessThanOrEqualTo(1);
        Math.Abs(actual.B - expected.B).Should().BeLessThanOrEqualTo(1);
    }
}
