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
using System.Windows.Controls;
using FluentAssertions;
using ThemeForge.Controls.Composites;
using Xunit;

namespace ThemeForge.Controls.Tests;

public sealed class BreadcrumbAutomationPeerTests
{
    [StaFact]
    public void BreadcrumbPeer_GetClassName_ReturnsBreadcrumbName()
    {
        _ = TestApplication.Instance;
        BreadcrumbAutomationPeer peer = new BreadcrumbAutomationPeer(new Breadcrumb());

        peer.GetClassName().Should().Be("Breadcrumb");
    }

    [StaFact]
    public void BreadcrumbPeer_GetAutomationControlType_ReturnsGroup()
    {
        _ = TestApplication.Instance;
        BreadcrumbAutomationPeer peer = new BreadcrumbAutomationPeer(new Breadcrumb());

        peer.GetAutomationControlType().Should().Be(AutomationControlType.Group);
    }

    [StaFact]
    public void BreadcrumbItemPeer_GetClassName_ReturnsBreadcrumbItemName()
    {
        _ = TestApplication.Instance;
        BreadcrumbItemAutomationPeer peer = new BreadcrumbItemAutomationPeer(new BreadcrumbItem());

        peer.GetClassName().Should().Be("BreadcrumbItem");
    }

    [StaFact]
    public void BreadcrumbItemPeer_GetAutomationControlType_ReturnsButton()
    {
        _ = TestApplication.Instance;
        BreadcrumbItemAutomationPeer peer = new BreadcrumbItemAutomationPeer(new BreadcrumbItem());

        peer.GetAutomationControlType().Should().Be(AutomationControlType.Button);
    }

    [StaFact]
    public void BreadcrumbItemPeer_GetPattern_WithInvoke_ReturnsInvokeProvider()
    {
        _ = TestApplication.Instance;
        BreadcrumbItemAutomationPeer peer = new BreadcrumbItemAutomationPeer(new BreadcrumbItem());

        object? pattern = peer.GetPattern(PatternInterface.Invoke);

        pattern.Should().BeAssignableTo<IInvokeProvider>();
    }

    [StaFact]
    public void BreadcrumbItemPeer_Invoke_RaisesClick()
    {
        _ = TestApplication.Instance;
        BreadcrumbItem item = new BreadcrumbItem();
        int clickCount = 0;
        item.Click += (_, _) => clickCount++;
        IInvokeProvider provider = new BreadcrumbItemAutomationPeer(item);

        provider.Invoke();

        clickCount.Should().Be(1);
    }

    [StaFact]
    public void BreadcrumbItemPeer_Invoke_WithDisabledOwner_Throws()
    {
        _ = TestApplication.Instance;
        BreadcrumbItem item = new BreadcrumbItem { IsEnabled = false };
        IInvokeProvider provider = new BreadcrumbItemAutomationPeer(item);

        provider.Invoking(p => p.Invoke())
            .Should()
            .Throw<ElementNotEnabledException>();
    }

    [StaFact]
    public void BreadcrumbItemPeer_GetName_WithStringContent_ReturnsContent()
    {
        _ = TestApplication.Instance;
        BreadcrumbItemAutomationPeer peer = new BreadcrumbItemAutomationPeer(
            new BreadcrumbItem { Content = "Accueil" });

        peer.GetName().Should().Be("Accueil");
    }

    [StaFact]
    public void BreadcrumbItemPeer_GetName_WithExplicitName_ReturnsExplicitName()
    {
        _ = TestApplication.Instance;
        BreadcrumbItem item = new BreadcrumbItem { Content = "Accueil" };
        AutomationProperties.SetName(item, "Home");
        BreadcrumbItemAutomationPeer peer = new BreadcrumbItemAutomationPeer(item);

        peer.GetName().Should().Be("Home");
    }

    [StaFact]
    public void BreadcrumbItemPeer_GetName_WithNonTextContent_ReturnsEmpty()
    {
        _ = TestApplication.Instance;
        BreadcrumbItemAutomationPeer peer = new BreadcrumbItemAutomationPeer(
            new BreadcrumbItem { Content = new TextBlock() });

        peer.GetName().Should().BeEmpty();
    }
}
