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
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using FluentAssertions;
using ThemeForge.Controls.Composites;
using ThemeForge.Controls.Theming;

namespace ThemeForge.Controls.Tests;

public sealed class TemplateRegressionTests
{
    [StaFact]
    public void PersistentToast_CloseButtonIsFocusableNamedAndDismisses()
    {
        _ = TestApplication.Instance;
        Toast toast = new Toast { Duration = TimeSpan.Zero };
        int dismissed = 0;
        toast.Dismissed += (_, _) => dismissed++;
        WindowTestHost.Render(toast, _ =>
        {
            Button close = (Button)toast.Template.FindName("PART_CloseButton", toast);
            close.Focusable.Should().BeTrue();
            close.IsTabStop.Should().BeTrue();
            AutomationProperties.GetName(close).Should().Be("Close notification");
            toast.Resources["ToastCloseButtonName"] = "Fermer la notification";
            AutomationProperties.GetName(close).Should().Be("Fermer la notification");
            close.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(close), 0, Key.Enter)
            { RoutedEvent = Keyboard.KeyDownEvent });
            dismissed.Should().Be(1);
        });
    }

    [StaFact]
    public void ColumnHeader_GripperDragsResizeTheColumn()
    {
        DataGrid grid = new DataGrid { AutoGenerateColumns = false, CanUserAddRows = false };
        ResourceDictionary styles = Load("Styles/DataGridStyle.xaml");
        grid.Resources.MergedDictionaries.Add(styles);
        DataGridTextColumn column = new DataGridTextColumn { Header = "Name", Width = 100 };
        grid.Columns.Add(column);
        grid.ItemsSource = new[] { "Row" };
        WindowTestHost.Render(grid, _ =>
        {
            DataGridColumnHeader header = Descendants(grid).OfType<DataGridColumnHeader>().First(item => item.Column == column);
            Thumb grip = (Thumb)header.Template.FindName("PART_RightHeaderGripper", header);
            grip.Should().NotBeNull();
            double width = column.ActualWidth;
            grip.RaiseEvent(new DragStartedEventArgs(0, 0) { RoutedEvent = Thumb.DragStartedEvent });
            grip.RaiseEvent(new DragDeltaEventArgs(25, 0) { RoutedEvent = Thumb.DragDeltaEvent });
            grip.RaiseEvent(new DragCompletedEventArgs(25, 0, false) { RoutedEvent = Thumb.DragCompletedEvent });
            grid.UpdateLayout();
            column.ActualWidth.Should().BeApproximately(width + 25, 0.1);
            grid.CanUserResizeColumns = false;
            grip.Visibility.Should().Be(Visibility.Collapsed);
        });
    }

    [StaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void ReducedMotion_LeavesVisibleStaticStates(bool hostOptOut)
    {
        StackPanel root = new StackPanel();
        Motion.SetReduceMotion(root, hostOptOut);
        ToggleSwitch toggle = new ToggleSwitch { Style = LoadStyle("Themes/ToggleSwitch.xaml", typeof(ToggleSwitch)), IsChecked = true };
        ProgressBar progress = new ProgressBar { Style = LoadStyle("Styles/ProgressBarStyle.xaml", typeof(ProgressBar)), IsIndeterminate = true };
        Toast toast = new Toast { Style = LoadStyle("Themes/Toast.xaml", typeof(Toast)), Duration = TimeSpan.Zero };
        foreach (Control control in new Control[] { toggle, progress, toast })
        {
            Motion.SetSystemAnimationsEnabled(control, hostOptOut);
            root.Children.Add(control);
        }
        WindowTestHost.Render(root, _ =>
        {
            Border thumb = (Border)toggle.Template.FindName("PART_Thumb", toggle);
            FrameworkElement segment = (FrameworkElement)progress.Template.FindName("IndeterminateBar", progress);
            ((TranslateTransform)thumb.RenderTransform).X.Should().Be(16);
            thumb.RenderTransform.HasAnimatedProperties.Should().BeFalse();
            segment.Visibility.Should().Be(Visibility.Visible);
            ((TranslateTransform)segment.RenderTransform).X.Should().Be(0);
            segment.RenderTransform.HasAnimatedProperties.Should().BeFalse();
            toast.Opacity.Should().Be(1);
            toast.HasAnimatedProperties.Should().BeFalse();
            toggle.IsChecked = false;
            ((TranslateTransform)thumb.RenderTransform).X.Should().Be(0);
        });
    }

    internal static ResourceDictionary Load(string path)
    {
        _ = TestApplication.Instance;
        return new ResourceDictionary { Source = new Uri($"pack://application:,,,/ThemeForge.Controls;component/{path}") };
    }

    private static Style LoadStyle(string path, Type type) => (Style)Load(path)[type];

    internal static IEnumerable<DependencyObject> Descendants(DependencyObject parent)
    {
        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, index);
            yield return child;
            foreach (DependencyObject descendant in Descendants(child)) { yield return descendant; }
        }
    }
}
