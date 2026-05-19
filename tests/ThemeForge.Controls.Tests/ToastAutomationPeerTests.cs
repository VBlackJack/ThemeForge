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

public sealed class ToastAutomationPeerTests
{
    [StaFact]
    public void Ctor_StoresOwner()
    {
        _ = TestApplication.Instance;
        Toast toast = new Toast();

        ToastAutomationPeer peer = new ToastAutomationPeer(toast);

        peer.Owner.Should().BeSameAs(toast);
    }

    [StaFact]
    public void GetClassName_ReturnsToastName()
    {
        _ = TestApplication.Instance;
        ToastAutomationPeer peer = new ToastAutomationPeer(new Toast());

        peer.GetClassName().Should().Be("Toast");
    }

    [StaFact]
    public void GetAutomationControlType_ReturnsGroup()
    {
        _ = TestApplication.Instance;
        ToastAutomationPeer peer = new ToastAutomationPeer(new Toast());

        peer.GetAutomationControlType().Should().Be(AutomationControlType.Group);
    }

    [StaFact]
    public void GetLiveSetting_ForInfo_ReturnsPolite()
    {
        _ = TestApplication.Instance;
        ToastAutomationPeer peer = new ToastAutomationPeer(new Toast { Severity = ToastSeverity.Info });

        peer.GetLiveSetting().Should().Be(AutomationLiveSetting.Polite);
    }

    [StaFact]
    public void GetLiveSetting_ForSuccess_ReturnsPolite()
    {
        _ = TestApplication.Instance;
        ToastAutomationPeer peer = new ToastAutomationPeer(new Toast { Severity = ToastSeverity.Success });

        peer.GetLiveSetting().Should().Be(AutomationLiveSetting.Polite);
    }

    [StaFact]
    public void GetLiveSetting_ForWarning_ReturnsPolite()
    {
        _ = TestApplication.Instance;
        ToastAutomationPeer peer = new ToastAutomationPeer(new Toast { Severity = ToastSeverity.Warning });

        peer.GetLiveSetting().Should().Be(AutomationLiveSetting.Polite);
    }

    [StaFact]
    public void GetLiveSetting_ForError_ReturnsAssertive()
    {
        _ = TestApplication.Instance;
        ToastAutomationPeer peer = new ToastAutomationPeer(new Toast { Severity = ToastSeverity.Error });

        peer.GetLiveSetting().Should().Be(AutomationLiveSetting.Assertive);
    }

    [StaFact]
    public void GetName_WithExplicitAutomationProperty_WinsOverContent()
    {
        _ = TestApplication.Instance;
        Toast toast = new Toast { Title = "Title", Message = "Message" };
        AutomationProperties.SetName(toast, "Explicit toast name");
        ToastAutomationPeer peer = new ToastAutomationPeer(toast);

        peer.GetName().Should().Be("Explicit toast name");
    }

    [StaFact]
    public void GetName_WithTitleAndMessage_ReturnsComposed()
    {
        _ = TestApplication.Instance;
        Toast toast = new Toast { Title = "Saved", Message = "Profile updated." };
        ToastAutomationPeer peer = new ToastAutomationPeer(toast);

        peer.GetName().Should().Be("Saved — Profile updated.");
    }

    [StaFact]
    public void GetName_WithTitleOnly_ReturnsTitle()
    {
        _ = TestApplication.Instance;
        Toast toast = new Toast { Title = "Saved" };
        ToastAutomationPeer peer = new ToastAutomationPeer(toast);

        peer.GetName().Should().Be("Saved");
    }

    [StaFact]
    public void GetName_WithMessageOnly_ReturnsMessage()
    {
        _ = TestApplication.Instance;
        Toast toast = new Toast { Message = "Profile updated." };
        ToastAutomationPeer peer = new ToastAutomationPeer(toast);

        peer.GetName().Should().Be("Profile updated.");
    }

    [StaFact]
    public void GetName_WithNothing_ReturnsEmpty()
    {
        _ = TestApplication.Instance;
        ToastAutomationPeer peer = new ToastAutomationPeer(new Toast());

        peer.GetName().Should().BeEmpty();
    }
}
