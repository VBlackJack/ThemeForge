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
using System.Windows.Controls;
using System.Windows.Media;

namespace ThemeForge.Controls.Composites;

/// <summary>
/// Button that displays a vector <see cref="Icon"/> (Geometry) alongside an
/// optional text <see cref="Label"/>. When Label is null or empty the
/// button collapses to a square icon-only button.
/// </summary>
/// <remarks>
/// Usage:
/// <code>
/// &lt;dfc:IconButton Icon="{StaticResource Geo.Save}" Label="Save"/&gt;
/// &lt;dfc:IconButton Icon="{StaticResource Geo.Close}"/&gt;  &lt;!-- icon only --&gt;
/// </code>
/// </remarks>
public sealed class IconButton : Button
{
    static IconButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(IconButton),
            new FrameworkPropertyMetadata(typeof(IconButton)));
    }

    /// <summary>Vector path data rendered as the button icon.</summary>
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(Geometry),
            typeof(IconButton),
            new PropertyMetadata(null));

    public Geometry? Icon
    {
        get => (Geometry?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Side length of the icon in DIP.</summary>
    public static readonly DependencyProperty IconSizeProperty =
        DependencyProperty.Register(
            nameof(IconSize),
            typeof(double),
            typeof(IconButton),
            new PropertyMetadata(16.0));

    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    /// <summary>Optional text label rendered to the right of the icon.</summary>
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(IconButton),
            new PropertyMetadata(null));

    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>
    /// Exposes a UIA peer whose Name falls back through
    /// <see cref="Label"/> -> <c>AutomationProperties.Name</c> ->
    /// <c>AutomationProperties.HelpText</c>. Required because the default
    /// <see cref="ButtonAutomationPeer"/> reads <see cref="Button.Content"/>,
    /// which is always <c>null</c> on this control (Icon + Label are
    /// separate dependency properties).
    /// </summary>
    protected override AutomationPeer OnCreateAutomationPeer() => new IconButtonAutomationPeer(this);
}

/// <summary>
/// UIA peer for <see cref="IconButton"/> that surfaces a meaningful Name
/// even when the button is icon-only.
/// </summary>
internal sealed class IconButtonAutomationPeer : ButtonAutomationPeer
{
    public IconButtonAutomationPeer(IconButton owner) : base(owner)
    {
    }

    protected override string GetNameCore()
    {
        IconButton owner = (IconButton)Owner;

        if (!string.IsNullOrWhiteSpace(owner.Label))
        {
            return owner.Label;
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

    protected override string GetClassNameCore() => nameof(IconButton);
}
