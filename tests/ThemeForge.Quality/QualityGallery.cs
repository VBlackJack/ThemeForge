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
using System.Windows.Automation;
using System.Windows.Controls;
using ThemeForge.Controls.Composites;
using ThemeForge.Controls.Theming;

namespace ThemeForge.Quality;

internal static class QualityGallery
{
    internal static StackPanel Create(QualityOptions options)
    {
        StackPanel panel = new StackPanel(); panel.SetResourceReference(FrameworkElement.MarginProperty, "SpacingXl");
        Motion.SetReduceMotion(panel, true);
        panel.Children.Add(new TextBlock { Text = options.SampleText, TextWrapping = TextWrapping.Wrap });
        panel.Children.Add(new Button { Content = options.ActionText, IsDefault = true });
        panel.Children.Add(new Button { Content = options.DisabledText, IsEnabled = false });
        ToggleSwitch toggle = new ToggleSwitch { Content = options.ActionText, IsChecked = true };
        panel.Children.Add(toggle);
        TextBox input = new TextBox { Text = options.ErrorText };
        input.SetResourceReference(Control.BorderBrushProperty, "ErrorBrush");
        AutomationProperties.SetName(input, options.ErrorText); panel.Children.Add(input);
        NumericUpDown number = new NumericUpDown { Value = 42 };
        AutomationProperties.SetName(number, options.ActionText); panel.Children.Add(number);
        panel.Children.Add(new ProgressBar { IsIndeterminate = true });
        Toast toast = new Toast { Title = options.ActionText, Message = options.SampleText, Duration = TimeSpan.Zero };
        panel.Children.Add(toast);
        DataGrid grid = new DataGrid { AutoGenerateColumns = false, CanUserAddRows = false };
        grid.Columns.Add(new DataGridTextColumn { Header = options.ActionText, Binding = new System.Windows.Data.Binding(".") });
        grid.ItemsSource = new[] { options.SampleText, options.ErrorText }; panel.Children.Add(grid);
        return panel;
    }
}
