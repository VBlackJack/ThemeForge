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
using FluentAssertions;
using ThemeForge.Controls.Composites;
using Xunit;

namespace ThemeForge.Controls.Tests;

public sealed class CardAutomationPeerTests
{
    [StaFact]
    public void Ctor_StoresOwner()
    {
        _ = TestApplication.Instance;
        Card card = new Card();

        CardAutomationPeer peer = new CardAutomationPeer(card);

        peer.Owner.Should().BeSameAs(card);
    }

    [StaFact]
    public void GetClassName_ReturnsCardName()
    {
        _ = TestApplication.Instance;
        CardAutomationPeer peer = new CardAutomationPeer(new Card());

        peer.GetClassName().Should().Be("Card");
    }

    [StaFact]
    public void GetAutomationControlType_ReturnsGroup()
    {
        _ = TestApplication.Instance;
        CardAutomationPeer peer = new CardAutomationPeer(new Card());

        peer.GetAutomationControlType().Should().Be(AutomationControlType.Group);
    }

    [StaFact]
    public void GetName_WithExplicitAutomationProperty_WinsOverHeader()
    {
        _ = TestApplication.Instance;
        Card card = new Card { Header = "Header should be ignored" };
        AutomationProperties.SetName(card, "Explicit name");
        CardAutomationPeer peer = new CardAutomationPeer(card);

        peer.GetName().Should().Be("Explicit name");
    }

    [StaFact]
    public void GetName_WithoutExplicitButWithHeader_ReturnsHeaderToString()
    {
        _ = TestApplication.Instance;
        Card card = new Card { Header = "Settings" };
        CardAutomationPeer peer = new CardAutomationPeer(card);

        peer.GetName().Should().Be("Settings");
    }

    [StaFact]
    public void GetName_WithNoExplicitNameAndNoHeader_ReturnsEmpty()
    {
        _ = TestApplication.Instance;
        CardAutomationPeer peer = new CardAutomationPeer(new Card());

        peer.GetName().Should().BeEmpty();
    }
}
