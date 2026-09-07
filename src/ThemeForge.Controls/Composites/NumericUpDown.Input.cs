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

using System.Globalization;
using System.Windows;

namespace ThemeForge.Controls.Composites;

public sealed partial class NumericUpDown
{
    private void CommitText()
    {
        if (_textBox is null || IsReadOnly)
        {
            return;
        }

        bool isParsed = double.TryParse(
            _textBox.Text,
            NumberStyles.Float | NumberStyles.AllowThousands,
            CultureInfo.CurrentCulture,
            out double parsed);
        if (isParsed && double.IsFinite(parsed))
        {
            SetCurrentValue(ValueProperty, parsed);
        }

        UpdateText();
    }

    private void UpdateText()
    {
        if (_textBox is not null)
        {
            string format = "F" + DecimalPlaces.ToString(CultureInfo.InvariantCulture);
            _textBox.Text = Value.ToString(format, CultureInfo.CurrentCulture);
        }
    }

    private void UpdateButtonStates()
    {
        if (_upButton is not null) { _upButton.IsEnabled = !IsReadOnly && Value < Maximum; }
        if (_downButton is not null) { _downButton.IsEnabled = !IsReadOnly && Value > Minimum; }
    }

    private void AttachTemplateParts()
    {
        if (_upButton is not null) { _upButton.Click += OnUpButtonClick; }
        if (_downButton is not null) { _downButton.Click += OnDownButtonClick; }
        if (_textBox is not null) { _textBox.LostFocus += OnTextBoxLostFocus; }
    }

    private void DetachTemplateParts()
    {
        if (_upButton is not null) { _upButton.Click -= OnUpButtonClick; }
        if (_downButton is not null) { _downButton.Click -= OnDownButtonClick; }
        if (_textBox is not null) { _textBox.LostFocus -= OnTextBoxLostFocus; }
    }

    private void OnUpButtonClick(object sender, RoutedEventArgs e) => StepValue(SmallChange);

    private void OnDownButtonClick(object sender, RoutedEventArgs e) => StepValue(-SmallChange);

    private void OnTextBoxLostFocus(object sender, RoutedEventArgs e) => CommitText();
}
