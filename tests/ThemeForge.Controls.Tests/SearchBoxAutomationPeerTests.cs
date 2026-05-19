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

using FluentAssertions;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using ThemeForge.Controls.Composites;
using Xunit;

namespace ThemeForge.Controls.Tests;

public sealed class SearchBoxAutomationPeerTests
{
    [StaFact]
    public void Ctor_StoresOwner()
    {
        _ = TestApplication.Instance;
        var box = new SearchBox();

        var peer = new SearchBoxAutomationPeer(box);

        peer.Owner.Should().BeSameAs(box);
    }

    [StaFact]
    public void GetClassName_ReturnsSearchBoxName()
    {
        _ = TestApplication.Instance;
        var peer = new SearchBoxAutomationPeer(new SearchBox());

        peer.GetClassName().Should().Be("SearchBox");
    }

    [StaFact]
    public void GetAutomationControlType_ReturnsEdit()
    {
        _ = TestApplication.Instance;
        var peer = new SearchBoxAutomationPeer(new SearchBox());

        // Inherited from TextBoxAutomationPeer.
        peer.GetAutomationControlType().Should().Be(AutomationControlType.Edit);
    }

    [StaFact]
    public void GetName_WithExplicitAutomationProperty_WinsOverPlaceholder()
    {
        _ = TestApplication.Instance;
        var box = new SearchBox { PlaceholderText = "Search composites..." };
        AutomationProperties.SetName(box, "Filter input");
        var peer = new SearchBoxAutomationPeer(box);

        peer.GetName().Should().Be("Filter input");
    }

    [StaFact]
    public void GetName_WithoutExplicitButWithPlaceholder_ReturnsPlaceholder()
    {
        _ = TestApplication.Instance;
        var box = new SearchBox { PlaceholderText = "Search composites..." };
        var peer = new SearchBoxAutomationPeer(box);

        peer.GetName().Should().Be("Search composites...");
    }

    [StaFact]
    public void GetName_WithNoExplicitAndNoPlaceholder_ReturnsEmpty()
    {
        _ = TestApplication.Instance;
        var peer = new SearchBoxAutomationPeer(new SearchBox());

        peer.GetName().Should().BeEmpty();
    }
}
