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

using FluentAssertions;
using ThemeForge.Controls.Composites;
using Xunit;

namespace ThemeForge.Controls.Tests;

public sealed class NumericUpDownTests
{
    [StaFact]
    public void Defaults_UseUnboundedIntegerSpinnerMetadata()
    {
        _ = TestApplication.Instance;
        NumericUpDown control = new NumericUpDown();

        control.Minimum.Should().Be(double.MinValue);
        control.Maximum.Should().Be(double.MaxValue);
        control.SmallChange.Should().Be(1);
        control.LargeChange.Should().Be(10);
    }

    [StaFact]
    public void IncreaseValue_AddsSmallChange()
    {
        _ = TestApplication.Instance;
        NumericUpDown control = new NumericUpDown { SmallChange = 2, Value = 3 };

        control.IncreaseValue();

        control.Value.Should().Be(5);
    }

    [StaFact]
    public void DecreaseValue_SubtractsSmallChange()
    {
        _ = TestApplication.Instance;
        NumericUpDown control = new NumericUpDown { SmallChange = 2, Value = 3 };

        control.DecreaseValue();

        control.Value.Should().Be(1);
    }

    [StaFact]
    public void Value_IsCoercedIntoRange()
    {
        _ = TestApplication.Instance;
        NumericUpDown control = new NumericUpDown { Maximum = 10, Value = 999 };

        control.Value.Should().Be(10);
    }

    [StaFact]
    public void IncreaseValue_AtMaximum_RemainsAtMaximum()
    {
        _ = TestApplication.Instance;
        NumericUpDown control = new NumericUpDown { Maximum = 5, Value = 5 };

        control.IncreaseValue();

        control.Value.Should().Be(5);
    }

    [StaFact]
    public void DecreaseValue_AtMinimum_RemainsAtMinimum()
    {
        _ = TestApplication.Instance;
        NumericUpDown control = new NumericUpDown { Minimum = 5, Value = 5 };

        control.DecreaseValue();

        control.Value.Should().Be(5);
    }

    [StaFact]
    public void IncreaseValue_RoundsToDecimalPlacesToAvoidDrift()
    {
        _ = TestApplication.Instance;
        NumericUpDown control = new NumericUpDown { DecimalPlaces = 1, SmallChange = 0.1 };

        control.IncreaseValue();
        control.IncreaseValue();
        control.IncreaseValue();

        control.Value.Should().BeApproximately(0.3, 1e-9);
    }

    [StaFact]
    public void IncreaseValue_WhenReadOnly_DoesNotChangeValue()
    {
        _ = TestApplication.Instance;
        NumericUpDown control = new NumericUpDown { IsReadOnly = true, Value = 3 };

        control.IncreaseValue();

        control.Value.Should().Be(3);
    }

    [StaFact]
    public void DecimalPlaces_OutsideRoundRange_Throws()
    {
        _ = TestApplication.Instance;
        NumericUpDown control = new NumericUpDown();

        Action tooLow = () => control.DecimalPlaces = -1;
        Action tooHigh = () => control.DecimalPlaces = 16;

        tooLow.Should().Throw<ArgumentException>();
        tooHigh.Should().Throw<ArgumentException>();
    }

    [StaFact]
    public void DecimalPlaces_InsideRoundRange_IsAccepted()
    {
        _ = TestApplication.Instance;
        NumericUpDown control = new NumericUpDown();

        Action setZero = () => control.DecimalPlaces = 0;
        Action setMax = () => control.DecimalPlaces = 15;

        setZero.Should().NotThrow();
        setMax.Should().NotThrow();
    }

    [StaFact]
    public void DecimalPlaces_Changed_RoundsCurrentValue()
    {
        _ = TestApplication.Instance;
        NumericUpDown control = new NumericUpDown { DecimalPlaces = 3, Value = 3.567 };

        control.DecimalPlaces = 1;

        control.Value.Should().Be(3.6);
    }

    [StaFact]
    public void Value_DirectSet_IsRoundedToDecimalPlaces()
    {
        _ = TestApplication.Instance;
        NumericUpDown control = new NumericUpDown { DecimalPlaces = 2, Value = 3.14159 };

        control.Value.Should().Be(3.14);
    }

    [StaFact]
    public void Value_DirectSet_RoundsHalfAwayFromZero()
    {
        _ = TestApplication.Instance;
        NumericUpDown control = new NumericUpDown { DecimalPlaces = 2, Value = 0.125 };

        control.Value.Should().Be(0.13);
    }

    [StaFact]
    public void Value_DirectSetBelowMinimum_IsClamped()
    {
        _ = TestApplication.Instance;
        NumericUpDown control = new NumericUpDown { Minimum = 0, Value = -5 };

        control.Value.Should().Be(0);
    }

    [StaFact]
    public void Value_DirectSetOutOfRangeAndImprecise_IsRoundedThenClamped()
    {
        _ = TestApplication.Instance;
        NumericUpDown control = new NumericUpDown { DecimalPlaces = 1, Maximum = 10, Value = 12.36 };

        control.Value.Should().Be(10);
    }

    [StaFact]
    public void Value_DirectSetAtPrecisionAndInRange_IsUnchanged()
    {
        _ = TestApplication.Instance;
        NumericUpDown control = new NumericUpDown { DecimalPlaces = 2, Minimum = 0, Maximum = 100, Value = 42.5 };

        control.Value.Should().Be(42.5);
    }

    [StaFact]
    public void IncreaseValue_RaisesValueChanged()
    {
        _ = TestApplication.Instance;
        NumericUpDown control = new NumericUpDown();
        int raisedCount = 0;
        control.ValueChanged += (_, _) => raisedCount++;

        control.IncreaseValue();

        raisedCount.Should().Be(1);
    }
}
