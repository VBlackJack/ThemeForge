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
using System.Windows.Shapes;
using FluentAssertions;
using ThemeForge.Controls.Composites;
using Xunit;

namespace ThemeForge.Controls.Tests;

public sealed class DialogAutomationPeerTests
{
    [StaFact]
    public void Ctor_StoresOwner()
    {
        _ = TestApplication.Instance;
        Dialog dialog = new Dialog();

        DialogAutomationPeer peer = new DialogAutomationPeer(dialog);

        peer.Owner.Should().BeSameAs(dialog);
    }

    [StaFact]
    public void GetClassName_ReturnsDialogName()
    {
        _ = TestApplication.Instance;
        DialogAutomationPeer peer = new DialogAutomationPeer(new Dialog());

        peer.GetClassName().Should().Be("Dialog");
    }

    [StaFact]
    public void GetAutomationControlType_ReturnsPane()
    {
        _ = TestApplication.Instance;
        DialogAutomationPeer peer = new DialogAutomationPeer(new Dialog());

        peer.GetAutomationControlType().Should().Be(AutomationControlType.Pane);
    }

    [StaFact]
    public void GetName_WithExplicitAutomationProperty_WinsOverHeader()
    {
        _ = TestApplication.Instance;
        Dialog dialog = new Dialog { Header = "Header should be ignored" };
        AutomationProperties.SetName(dialog, "Explicit name");
        DialogAutomationPeer peer = new DialogAutomationPeer(dialog);

        peer.GetName().Should().Be("Explicit name");
    }

    [StaFact]
    public void GetName_WithoutExplicitButWithHeader_ReturnsHeaderToString()
    {
        _ = TestApplication.Instance;
        Dialog dialog = new Dialog { Header = "Confirm action" };
        DialogAutomationPeer peer = new DialogAutomationPeer(dialog);

        peer.GetName().Should().Be("Confirm action");
    }

    [StaFact]
    public void GetName_WithNoExplicitNameAndNoHeader_ReturnsEmpty()
    {
        _ = TestApplication.Instance;
        DialogAutomationPeer peer = new DialogAutomationPeer(new Dialog());

        peer.GetName().Should().BeEmpty();
    }

    [StaFact]
    public void GetName_WithNonStringHeader_ReturnsEmptyWithoutFqn()
    {
        _ = TestApplication.Instance;
        Dialog dialog = new Dialog { Header = new Rectangle() };
        DialogAutomationPeer peer = new DialogAutomationPeer(dialog);

        string name = peer.GetName();

        name.Should().NotContain("System.Windows");
        name.Should().BeEmpty();
    }
}
