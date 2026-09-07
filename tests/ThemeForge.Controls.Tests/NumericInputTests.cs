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
using System.Windows.Controls;
using FluentAssertions;
using ThemeForge.Controls.Composites;

namespace ThemeForge.Controls.Tests;

public sealed class NumericInputTests
{
    [StaTheory]
    [InlineData("NaN")]
    [InlineData("1e5000")]
    [InlineData("-1e5000")]
    [InlineData("not a number")]
    public void Commit_InvalidText_PreservesValueAndRestoresDisplay(string input)
    {
        _ = TestApplication.Instance;
        ResourceDictionary styles = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/ThemeForge.Controls;component/Themes/NumericUpDown.xaml"),
        };
        NumericUpDown control = new NumericUpDown { Style = (Style)styles[typeof(NumericUpDown)], Value = 42 };
        control.ApplyTemplate();
        TextBox text = (TextBox)control.Template.FindName("PART_TextBox", control);
        text.Text = input;

        Action commit = () => text.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent));

        commit.Should().NotThrow();
        control.Value.Should().Be(42);
        text.Text.Should().Be("42");
    }

    [StaFact]
    public void Step_Overflow_ClampsBeforeAssigningDependencyProperty()
    {
        NumericUpDown control = new NumericUpDown { Value = double.MaxValue, SmallChange = double.MaxValue };
        Action increase = control.IncreaseValue;
        increase.Should().NotThrow();
        control.Value.Should().Be(double.MaxValue);
    }
}
