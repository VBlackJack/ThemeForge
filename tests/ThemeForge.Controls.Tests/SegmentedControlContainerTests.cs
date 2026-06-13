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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Media;
using FluentAssertions;
using ThemeForge.Controls.Composites;
using Xunit;

namespace ThemeForge.Controls.Tests;

public sealed class SegmentedControlContainerTests
{
    [StaFact]
    public void ItemsSource_GeneratesSegmentItemContainers()
    {
        SegmentedControl control = new SegmentedControl { ItemsSource = new[] { "One", "Two", "Three" } };

        WindowTestHost.Render(control, _ =>
        {
            for (int index = 0; index < 3; index++)
            {
                control.ItemContainerGenerator.ContainerFromIndex(index)
                    .Should()
                    .BeOfType<SegmentItem>();
            }
        });
    }

    [StaFact]
    public void RawSegmentItems_AreTheirOwnContainers()
    {
        SegmentItem first = new SegmentItem();
        SegmentItem second = new SegmentItem();
        SegmentedControl control = new SegmentedControl();
        control.Items.Add(first);
        control.Items.Add(second);

        WindowTestHost.Render(control, _ =>
        {
            control.ItemContainerGenerator.ContainerFromIndex(0).Should().BeSameAs(first);
            control.ItemContainerGenerator.ContainerFromIndex(1).Should().BeSameAs(second);
        });
    }

    [StaFact]
    public void GeneratedContainers_RenderInVisualTree()
    {
        SegmentedControl control = new SegmentedControl { ItemsSource = new[] { "A", "B" } };

        WindowTestHost.Render(control, _ =>
            FindVisualChildren<SegmentItem>(control).Should().HaveCount(2));
    }

    [StaFact]
    public void AutomationPeer_Children_MatchItems_WithoutRecursion()
    {
        SegmentedControl control = new SegmentedControl { ItemsSource = new[] { "A", "B", "C" } };

        WindowTestHost.Render(control, _ =>
        {
            AutomationPeer peer = UIElementAutomationPeer.CreatePeerForElement(control)
                ?? throw new InvalidOperationException("SegmentedControl automation peer was not created.");
            peer.Should().BeOfType<SegmentedControlAutomationPeer>();

            List<AutomationPeer> children = peer.GetChildren() ?? new List<AutomationPeer>();
            children.OfType<SegmentItemAutomationPeer>().Should().HaveCount(3);

            foreach (AutomationPeer child in children)
            {
                List<AutomationPeer> grandChildren = child.GetChildren() ?? new List<AutomationPeer>();
                grandChildren.Should().NotContain(child);
            }
        });
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
        where T : DependencyObject
    {
        int count = VisualTreeHelper.GetChildrenCount(root);
        for (int index = 0; index < count; index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
            {
                yield return match;
            }

            foreach (T descendant in FindVisualChildren<T>(child))
            {
                yield return descendant;
            }
        }
    }
}
