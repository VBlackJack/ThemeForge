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
using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace ThemeForge.Controls.Composites;

public sealed class SegmentItem : ListBoxItem
{
    static SegmentItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SegmentItem),
            new FrameworkPropertyMetadata(typeof(SegmentItem)));
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        if (ItemsControl.ItemsControlFromItemContainer(this) is SegmentedControl control)
        {
            AutomationPeer? peer = FrameworkElementAutomationPeer.FromElement(control)
                ?? FrameworkElementAutomationPeer.CreatePeerForElement(control);
            if (peer is SegmentedControlAutomationPeer segmentedControlPeer)
            {
                return new SegmentItemAutomationPeer(this, segmentedControlPeer);
            }
        }

        return base.OnCreateAutomationPeer();
    }
}
