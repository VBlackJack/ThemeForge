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

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using FluentAssertions;
using ThemeForge.Controls.Composites;
using Xunit;

namespace ThemeForge.Controls.Tests;

public sealed class NumericUpDownAutomationPeerTests
{
    [StaFact]
    public void GetClassName_ReturnsNumericUpDownName()
    {
        _ = TestApplication.Instance;
        NumericUpDownAutomationPeer peer = new NumericUpDownAutomationPeer(new NumericUpDown());

        peer.GetClassName().Should().Be("NumericUpDown");
    }

    [StaFact]
    public void GetAutomationControlType_ReturnsSpinner()
    {
        _ = TestApplication.Instance;
        NumericUpDownAutomationPeer peer = new NumericUpDownAutomationPeer(new NumericUpDown());

        peer.GetAutomationControlType().Should().Be(AutomationControlType.Spinner);
    }

    [StaFact]
    public void GetPattern_WithRangeValue_ReturnsRangeValueProvider()
    {
        _ = TestApplication.Instance;
        NumericUpDownAutomationPeer peer = new NumericUpDownAutomationPeer(new NumericUpDown());

        object? pattern = peer.GetPattern(PatternInterface.RangeValue);

        pattern.Should().BeAssignableTo<IRangeValueProvider>();
    }

    [StaFact]
    public void GetPattern_WithInvoke_ReturnsNull()
    {
        _ = TestApplication.Instance;
        NumericUpDownAutomationPeer peer = new NumericUpDownAutomationPeer(new NumericUpDown());

        peer.GetPattern(PatternInterface.Invoke).Should().BeNull();
    }

    [StaFact]
    public void Provider_ExposesOwnerRangeValues()
    {
        _ = TestApplication.Instance;
        NumericUpDown owner = new NumericUpDown { Minimum = -5, Maximum = 15, SmallChange = 0.5, LargeChange = 5, Value = 3 };
        NumericUpDownAutomationPeer peer = new NumericUpDownAutomationPeer(owner);

        peer.Minimum.Should().Be(-5);
        peer.Maximum.Should().Be(15);
        peer.SmallChange.Should().Be(0.5);
        peer.LargeChange.Should().Be(5);
        peer.Value.Should().Be(3);
    }

    [StaFact]
    public void IsReadOnly_WithWritableEnabledOwner_ReturnsFalse()
    {
        _ = TestApplication.Instance;
        NumericUpDownAutomationPeer peer = new NumericUpDownAutomationPeer(new NumericUpDown());

        peer.IsReadOnly.Should().BeFalse();
    }

    [StaFact]
    public void IsReadOnly_WithReadOnlyOwner_ReturnsTrue()
    {
        _ = TestApplication.Instance;
        NumericUpDownAutomationPeer peer = new NumericUpDownAutomationPeer(new NumericUpDown { IsReadOnly = true });

        peer.IsReadOnly.Should().BeTrue();
    }

    [StaFact]
    public void IsReadOnly_WithDisabledOwner_ReturnsTrue()
    {
        _ = TestApplication.Instance;
        NumericUpDownAutomationPeer peer = new NumericUpDownAutomationPeer(new NumericUpDown { IsEnabled = false });

        peer.IsReadOnly.Should().BeTrue();
    }

    [StaFact]
    public void SetValue_WithValidValue_UpdatesOwnerValue()
    {
        _ = TestApplication.Instance;
        NumericUpDown owner = new NumericUpDown { Minimum = 0, Maximum = 10 };
        NumericUpDownAutomationPeer peer = new NumericUpDownAutomationPeer(owner);

        peer.SetValue(4);

        owner.Value.Should().Be(4);
    }

    [StaFact]
    public void SetValue_WithReadOnlyOwner_Throws()
    {
        _ = TestApplication.Instance;
        NumericUpDownAutomationPeer peer = new NumericUpDownAutomationPeer(new NumericUpDown { IsReadOnly = true });

        Action act = () => peer.SetValue(4);

        act.Should().Throw<ElementNotEnabledException>();
    }

    [StaFact]
    public void SetValue_WithDisabledOwner_Throws()
    {
        _ = TestApplication.Instance;
        NumericUpDownAutomationPeer peer = new NumericUpDownAutomationPeer(new NumericUpDown { IsEnabled = false });

        Action act = () => peer.SetValue(4);

        act.Should().Throw<ElementNotEnabledException>();
    }

    [StaFact]
    public void SetValue_WithOutOfRangeValue_Throws()
    {
        _ = TestApplication.Instance;
        NumericUpDownAutomationPeer peer = new NumericUpDownAutomationPeer(new NumericUpDown { Minimum = 0, Maximum = 10 });

        Action act = () => peer.SetValue(12);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
