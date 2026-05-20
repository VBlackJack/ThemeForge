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
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ThemeForge.Controls.Composites;

[TemplatePart(Name = PartTextBox, Type = typeof(TextBox))]
[TemplatePart(Name = PartUpButton, Type = typeof(RepeatButton))]
[TemplatePart(Name = PartDownButton, Type = typeof(RepeatButton))]
public sealed class NumericUpDown : RangeBase
{
    private const string PartTextBox = "PART_TextBox";
    private const string PartUpButton = "PART_UpButton";
    private const string PartDownButton = "PART_DownButton";
    private const int MaxDecimalPlaces = 15;

    private TextBox? _textBox;
    private RepeatButton? _upButton;
    private RepeatButton? _downButton;

    static NumericUpDown()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(typeof(NumericUpDown)));
        MinimumProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(double.MinValue));
        MaximumProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(double.MaxValue));
        SmallChangeProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(1.0));
        LargeChangeProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(10.0));
    }

    public static readonly DependencyProperty DecimalPlacesProperty = DependencyProperty.Register(nameof(DecimalPlaces),
        typeof(int), typeof(NumericUpDown), new FrameworkPropertyMetadata(0, OnDecimalPlacesChanged),
        ValidateDecimalPlaces);

    public int DecimalPlaces { get => (int)GetValue(DecimalPlacesProperty); set => SetValue(DecimalPlacesProperty, value); }

    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(nameof(IsReadOnly),
        typeof(bool), typeof(NumericUpDown),
        new FrameworkPropertyMetadata(false, OnIsReadOnlyChanged));

    public bool IsReadOnly { get => (bool)GetValue(IsReadOnlyProperty); set => SetValue(IsReadOnlyProperty, value); }

    public void IncreaseValue() => StepValue(SmallChange);

    public void DecreaseValue() => StepValue(-SmallChange);

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        DetachTemplateParts();
        _textBox = GetTemplateChild(PartTextBox) as TextBox;
        _upButton = GetTemplateChild(PartUpButton) as RepeatButton;
        _downButton = GetTemplateChild(PartDownButton) as RepeatButton;
        AttachTemplateParts();
        UpdateText();
        UpdateButtonStates();
    }

    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);
        UpdateText();
        UpdateButtonStates();
        if (FrameworkElementAutomationPeer.FromElement(this) is NumericUpDownAutomationPeer peer)
        {
            peer.RaiseValueChanged(oldValue, newValue);
        }
    }

    protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
    { base.OnMinimumChanged(oldMinimum, newMinimum); UpdateButtonStates(); }

    protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
    { base.OnMaximumChanged(oldMaximum, newMaximum); UpdateButtonStates(); }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (IsReadOnly)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Up: StepValue(SmallChange); e.Handled = true; break;
            case Key.Down: StepValue(-SmallChange); e.Handled = true; break;
            case Key.PageUp: StepValue(LargeChange); e.Handled = true; break;
            case Key.PageDown: StepValue(-LargeChange); e.Handled = true; break;
            case Key.Enter: CommitText(); break;
        }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        if (IsReadOnly || !IsKeyboardFocusWithin) { return; }
        StepValue(e.Delta > 0 ? SmallChange : -SmallChange);
        e.Handled = true;
    }

    protected override AutomationPeer OnCreateAutomationPeer()
        => new NumericUpDownAutomationPeer(this);

    private static bool ValidateDecimalPlaces(object value)
    { int places = (int)value; return places >= 0 && places <= MaxDecimalPlaces; }

    private static void OnDecimalPlacesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        NumericUpDown control = (NumericUpDown)d;
        control.SetCurrentValue(ValueProperty, control.RoundToPrecision(control.Value));
        control.UpdateText();
    }

    private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((NumericUpDown)d).UpdateButtonStates();

    private void StepValue(double delta)
    {
        if (!IsReadOnly) { SetCurrentValue(ValueProperty, RoundToPrecision(Value + delta)); }
    }

    private double RoundToPrecision(double value)
        => Math.Round(value, DecimalPlaces, MidpointRounding.AwayFromZero);

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
        if (isParsed)
        {
            SetCurrentValue(ValueProperty, RoundToPrecision(parsed));
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
