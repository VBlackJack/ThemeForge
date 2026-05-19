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

namespace ThemeForge.Controls.Composites;

/// <summary>
/// Sliding on/off control that extends <see cref="ToggleButton"/>. The
/// thumb glides across a pill-shaped track when the user toggles state;
/// the track color snaps between <c>CommentBrush</c> (Off) and
/// <c>AccentBrush</c> (On). An optional label is rendered to the right of
/// the switch via the inherited <see cref="System.Windows.Controls.ContentControl.Content"/>
/// property.
/// </summary>
/// <remarks>
/// Usage:
/// <code>
/// &lt;dfc:ToggleSwitch Content="Notifications" IsChecked="{Binding NotifyEnabled, Mode=TwoWay}"/&gt;
/// &lt;dfc:ToggleSwitch/&gt; &lt;!-- no label --&gt;
/// </code>
/// State lives on the inherited <see cref="ToggleButton.IsChecked"/> DP —
/// no <c>IsOn</c> alias is introduced. The class name already conveys
/// semantics; a redundant DP would only confuse consumers reading the API.
/// Clicking anywhere on the control (track, thumb, or label) toggles the
/// state through the inherited <c>ToggleButton</c> click behavior.
/// </remarks>
public sealed class ToggleSwitch : ToggleButton
{
    static ToggleSwitch()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ToggleSwitch),
            new FrameworkPropertyMetadata(typeof(ToggleSwitch)));
    }

    /// <summary>
    /// Exposes a UIA peer that keeps the inherited Toggle pattern (correct
    /// for an on/off control — there is no <c>ControlType.ToggleSwitch</c>
    /// in UIA) while surfacing <see cref="System.Windows.Controls.ContentControl.Content"/>
    /// as the accessible Name and reporting <c>ToggleSwitch</c> as the
    /// className.
    /// </summary>
    protected override AutomationPeer OnCreateAutomationPeer() => new ToggleSwitchAutomationPeer(this);
}

/// <summary>
/// UIA peer for <see cref="ToggleSwitch"/>. Keeps the inherited
/// <see cref="AutomationControlType.Button"/> + Toggle pattern from
/// <see cref="ToggleButtonAutomationPeer"/>, overrides the class name and
/// projects the chip's content (with Name/HelpText fallbacks) as the
/// accessible Name.
/// </summary>
internal sealed class ToggleSwitchAutomationPeer : ToggleButtonAutomationPeer
{
    public ToggleSwitchAutomationPeer(ToggleSwitch owner) : base(owner)
    {
    }

    protected override string GetClassNameCore() => nameof(ToggleSwitch);

    protected override string GetNameCore()
    {
        ToggleSwitch owner = (ToggleSwitch)Owner;

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
}
