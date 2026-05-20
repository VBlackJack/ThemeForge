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
using System.Windows.Controls.Primitives;

namespace ThemeForge.Controls.Composites;

public sealed class NumericUpDownAutomationPeer : FrameworkElementAutomationPeer, IRangeValueProvider
{
    public NumericUpDownAutomationPeer(NumericUpDown owner) : base(owner)
    {
    }

    private NumericUpDown OwnerControl => (NumericUpDown)Owner;

    public bool IsReadOnly => OwnerControl.IsReadOnly || !OwnerControl.IsEnabled;

    public double Maximum => OwnerControl.Maximum;

    public double Minimum => OwnerControl.Minimum;

    public double SmallChange => OwnerControl.SmallChange;

    public double LargeChange => OwnerControl.LargeChange;

    public double Value => OwnerControl.Value;

    public override object? GetPattern(PatternInterface patternInterface)
        => patternInterface == PatternInterface.RangeValue ? this : base.GetPattern(patternInterface);

    public void SetValue(double value)
    {
        if (!OwnerControl.IsEnabled || OwnerControl.IsReadOnly)
        {
            throw new ElementNotEnabledException();
        }

        if (value < OwnerControl.Minimum || value > OwnerControl.Maximum)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        OwnerControl.SetCurrentValue(RangeBase.ValueProperty, value);
    }

    internal void RaiseValueChanged(double oldValue, double newValue)
        => RaisePropertyChangedEvent(RangeValuePatternIdentifiers.ValueProperty, oldValue, newValue);

    protected override string GetClassNameCore()
        => nameof(NumericUpDown);

    protected override AutomationControlType GetAutomationControlTypeCore()
        => AutomationControlType.Spinner;
}
