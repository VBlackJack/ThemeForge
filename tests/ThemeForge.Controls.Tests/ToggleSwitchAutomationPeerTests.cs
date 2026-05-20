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

public sealed class ToggleSwitchAutomationPeerTests
{
    [StaFact]
    public void GetName_WithExplicitAutomationProperty_WinsOverContent()
    {
        _ = TestApplication.Instance;
        ToggleSwitch toggleSwitch = new ToggleSwitch { Content = "Content should be ignored" };
        AutomationProperties.SetName(toggleSwitch, "Explicit name");
        AutomationPeer peer = CreatePeer(toggleSwitch);

        peer.GetName().Should().Be("Explicit name");
    }

    [StaFact]
    public void GetName_WithoutExplicitButWithStringContent_ReturnsContent()
    {
        _ = TestApplication.Instance;
        ToggleSwitch toggleSwitch = new ToggleSwitch { Content = "Notifications" };
        AutomationPeer peer = CreatePeer(toggleSwitch);

        peer.GetName().Should().Be("Notifications");
    }

    [StaFact]
    public void GetName_WithNonStringContent_ReturnsEmptyWithoutFqn()
    {
        _ = TestApplication.Instance;
        ToggleSwitch toggleSwitch = new ToggleSwitch { Content = new Rectangle() };
        AutomationPeer peer = CreatePeer(toggleSwitch);

        string name = peer.GetName();

        name.Should().NotContain("System.Windows");
        name.Should().BeEmpty();
    }

    private static AutomationPeer CreatePeer(ToggleSwitch toggleSwitch)
    {
        AutomationPeer? peer = UIElementAutomationPeer.CreatePeerForElement(toggleSwitch);
        peer.Should().NotBeNull();
        return peer!;
    }
}
