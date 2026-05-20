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

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using FluentAssertions;
using ThemeForge.Controls.Composites;
using Xunit;

namespace ThemeForge.Controls.Tests;

public sealed class SegmentedControlAutomationPeerTests
{
    [StaFact]
    public void SegmentedControlPeer_GetClassName_ReturnsSegmentedControlName()
    {
        _ = TestApplication.Instance;
        SegmentedControlAutomationPeer peer = new SegmentedControlAutomationPeer(new SegmentedControl());

        peer.GetClassName().Should().Be("SegmentedControl");
    }

    [StaFact]
    public void SegmentedControlPeer_GetAutomationControlType_ReturnsList()
    {
        _ = TestApplication.Instance;
        SegmentedControlAutomationPeer peer = new SegmentedControlAutomationPeer(new SegmentedControl());

        peer.GetAutomationControlType().Should().Be(AutomationControlType.List);
    }

    [StaFact]
    public void SegmentedControlPeer_GetPattern_WithSelection_ReturnsSelectionProvider()
    {
        _ = TestApplication.Instance;
        SegmentedControlAutomationPeer peer = new SegmentedControlAutomationPeer(new SegmentedControl());

        object? pattern = peer.GetPattern(PatternInterface.Selection);

        pattern.Should().BeAssignableTo<ISelectionProvider>();
    }

    [StaFact]
    public void SegmentItemPeer_GetClassName_ReturnsSegmentItemName()
    {
        _ = TestApplication.Instance;
        SegmentItemAutomationPeer peer = CreateSegmentItemPeer();

        peer.GetClassName().Should().Be("SegmentItem");
    }

    [StaFact]
    public void SegmentItemPeer_GetAutomationControlType_ReturnsListItem()
    {
        _ = TestApplication.Instance;
        SegmentItemAutomationPeer peer = CreateSegmentItemPeer();

        peer.GetAutomationControlType().Should().Be(AutomationControlType.ListItem);
    }

    [StaFact]
    public void SegmentItemPeer_GetPattern_WithSelectionItem_ReturnsSelectionItemProvider()
    {
        _ = TestApplication.Instance;
        SegmentItemAutomationPeer peer = CreateSegmentItemPeer();

        object? pattern = peer.GetPattern(PatternInterface.SelectionItem);

        pattern.Should().BeAssignableTo<ISelectionItemProvider>();
    }

    private static SegmentItemAutomationPeer CreateSegmentItemPeer()
    {
        SegmentedControlAutomationPeer parentPeer = new SegmentedControlAutomationPeer(new SegmentedControl());
        return new SegmentItemAutomationPeer(new SegmentItem(), parentPeer);
    }
}
