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

namespace ThemeForge.Controls.Composites;

/// <summary>
/// Vertical stack container for <see cref="Toast"/> items. Subscribes to
/// <see cref="Toast.Dismissed"/> on each child to remove it from <see cref="ItemsControl.Items"/>
/// when the toast self-dismisses or the user closes it.
/// </summary>
/// <remarks>
/// Add toasts via <c>host.Items.Add(new Toast { ... })</c>. The host does not
/// currently support <see cref="ItemsControl.ItemsSource"/> binding for the
/// MVP — items are managed directly.
/// </remarks>
public sealed class ToastHost : ItemsControl
{
    static ToastHost()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ToastHost),
            new FrameworkPropertyMetadata(typeof(ToastHost)));
    }

    /// <summary>
    /// Tells the ItemsControl framework that a <see cref="Toast"/> is already
    /// its own container — no <see cref="System.Windows.Controls.ContentPresenter"/>
    /// wrapper is needed. Without this override the wrapped Toast never enters
    /// the visual tree directly, its Loaded event never fires, and the
    /// fade-in storyboard never runs (Opacity stays at 0).
    /// </summary>
    protected override bool IsItemItsOwnContainerOverride(object? item)
        => item is Toast;

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (item is Toast toast)
        {
            toast.Dismissed -= OnToastDismissed;
            toast.Dismissed += OnToastDismissed;
        }
    }

    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        base.ClearContainerForItemOverride(element, item);
        if (item is Toast toast)
        {
            toast.Dismissed -= OnToastDismissed;
        }
    }

    private void OnToastDismissed(object? sender, EventArgs e)
    {
        if (sender is Toast toast)
        {
            Items.Remove(toast);
        }
    }
}
