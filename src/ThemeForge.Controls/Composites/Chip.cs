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
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ThemeForge.Controls.Composites;

/// <summary>
/// Rounded-rect token used for filters, selections, and removable items.
/// Inherits <see cref="ButtonBase"/> so consumers get <c>Click</c>,
/// <c>Command</c>, <c>CommandParameter</c> and <c>Content</c> for free, and
/// adds an optional leading <see cref="Icon"/>, an optional close affordance
/// driven by <see cref="IsRemovable"/> / <see cref="RemoveCommand"/>, and a
/// passive <see cref="IsSelected"/> visual state.
/// </summary>
/// <remarks>
/// Usage:
/// <code>
/// &lt;dfc:Chip Content="Filter"   Icon="{StaticResource Geo.Filter}"/&gt;
/// &lt;dfc:Chip Content="Removable" IsRemovable="True"
///           RemoveCommand="{Binding RemoveTagCommand}"/&gt;
/// &lt;dfc:Chip Content="Active"   IsSelected="{Binding IsActive, Mode=TwoWay}"/&gt;
/// </code>
/// <see cref="IsSelected"/> is intentionally passive: clicking the chip body
/// raises <c>Click</c> and invokes <c>Command</c>, but never toggles
/// IsSelected on its own. The consumer drives selection (toggle handler or
/// binding) — same contract as Material Design filter chips.
/// </remarks>
public sealed class Chip : ButtonBase
{
    static Chip()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Chip),
            new FrameworkPropertyMetadata(typeof(Chip)));
    }

    /// <summary>Optional vector path data rendered as the leading icon.</summary>
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(Geometry),
            typeof(Chip),
            new PropertyMetadata(null));

    public Geometry? Icon
    {
        get => (Geometry?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Side length of the leading icon in DIP.</summary>
    public static readonly DependencyProperty IconSizeProperty =
        DependencyProperty.Register(
            nameof(IconSize),
            typeof(double),
            typeof(Chip),
            new PropertyMetadata(14.0));

    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    /// <summary>When true, exposes a trailing close button that fires <see cref="RemoveCommand"/>.</summary>
    public static readonly DependencyProperty IsRemovableProperty =
        DependencyProperty.Register(
            nameof(IsRemovable),
            typeof(bool),
            typeof(Chip),
            new PropertyMetadata(false));

    public bool IsRemovable
    {
        get => (bool)GetValue(IsRemovableProperty);
        set => SetValue(IsRemovableProperty, value);
    }

    /// <summary>Command invoked when the close button is clicked.</summary>
    public static readonly DependencyProperty RemoveCommandProperty =
        DependencyProperty.Register(
            nameof(RemoveCommand),
            typeof(ICommand),
            typeof(Chip),
            new PropertyMetadata(null));

    public ICommand? RemoveCommand
    {
        get => (ICommand?)GetValue(RemoveCommandProperty);
        set => SetValue(RemoveCommandProperty, value);
    }

    /// <summary>Parameter passed to <see cref="RemoveCommand"/> on close.</summary>
    public static readonly DependencyProperty RemoveCommandParameterProperty =
        DependencyProperty.Register(
            nameof(RemoveCommandParameter),
            typeof(object),
            typeof(Chip),
            new PropertyMetadata(null));

    public object? RemoveCommandParameter
    {
        get => GetValue(RemoveCommandParameterProperty);
        set => SetValue(RemoveCommandParameterProperty, value);
    }

    /// <summary>Selected visual state. Two-way bindable. Never toggled by the control itself.</summary>
    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(
            nameof(IsSelected),
            typeof(bool),
            typeof(Chip),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>
    /// Exposes a UIA peer that surfaces <see cref="ContentControl.Content"/>
    /// (or AutomationProperties.Name / HelpText fallback) as the accessible
    /// Name, so screen readers and automation harnesses pick up chip labels.
    /// </summary>
    protected override AutomationPeer OnCreateAutomationPeer() => new ChipAutomationPeer(this);
}

/// <summary>
/// UIA peer for <see cref="Chip"/>. Reports the control as <c>Button</c> and
/// projects the chip's content (with name/helptext fallbacks) as the
/// accessible Name.
/// </summary>
internal sealed class ChipAutomationPeer : ButtonBaseAutomationPeer
{
    public ChipAutomationPeer(Chip owner) : base(owner)
    {
    }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Button;

    protected override string GetNameCore()
    {
        Chip owner = (Chip)Owner;

        if (owner.Content is not null)
        {
            string? contentText = owner.Content.ToString();
            if (!string.IsNullOrWhiteSpace(contentText))
            {
                return contentText;
            }
        }

        string explicitName = AutomationProperties.GetName(owner);
        if (!string.IsNullOrWhiteSpace(explicitName))
        {
            return explicitName;
        }

        string helpText = AutomationProperties.GetHelpText(owner);
        if (!string.IsNullOrWhiteSpace(helpText))
        {
            return helpText;
        }

        return base.GetNameCore();
    }

    protected override string GetClassNameCore() => nameof(Chip);
}
