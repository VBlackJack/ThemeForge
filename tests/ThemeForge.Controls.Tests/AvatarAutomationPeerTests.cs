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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FluentAssertions;
using ThemeForge.Controls.Composites;
using Xunit;

namespace ThemeForge.Controls.Tests;

public sealed class AvatarAutomationPeerTests
{
    [StaFact]
    public void Ctor_StoresOwner()
    {
        _ = TestApplication.Instance;
        Avatar avatar = new Avatar();

        AvatarAutomationPeer peer = new AvatarAutomationPeer(avatar);

        peer.Owner.Should().BeSameAs(avatar);
    }

    [StaFact]
    public void GetClassName_ReturnsAvatarName()
    {
        _ = TestApplication.Instance;
        AvatarAutomationPeer peer = new AvatarAutomationPeer(new Avatar());

        peer.GetClassName().Should().Be("Avatar");
    }

    [StaFact]
    public void GetAutomationControlType_WithoutSource_ReturnsText()
    {
        _ = TestApplication.Instance;
        Avatar avatar = new Avatar { Initials = "JB" };
        AvatarAutomationPeer peer = new AvatarAutomationPeer(avatar);

        peer.GetAutomationControlType().Should().Be(AutomationControlType.Text);
    }

    [StaFact]
    public void GetAutomationControlType_WithSource_ReturnsImage()
    {
        _ = TestApplication.Instance;
        Avatar avatar = new Avatar
        {
            Initials = "JB",
            Source = BitmapSource.Create(
                pixelWidth: 1,
                pixelHeight: 1,
                dpiX: 96,
                dpiY: 96,
                pixelFormat: PixelFormats.Bgra32,
                palette: null,
                pixels: new byte[] { 0, 0, 0, 0 },
                stride: 4),
        };
        AvatarAutomationPeer peer = new AvatarAutomationPeer(avatar);

        peer.GetAutomationControlType().Should().Be(AutomationControlType.Image);
    }

    [StaFact]
    public void GetName_WithExplicitAutomationProperty_WinsOverInitials()
    {
        _ = TestApplication.Instance;
        Avatar avatar = new Avatar { Initials = "JB" };
        AutomationProperties.SetName(avatar, "Julien Bombled");
        AvatarAutomationPeer peer = new AvatarAutomationPeer(avatar);

        peer.GetName().Should().Be("Julien Bombled");
    }

    [StaFact]
    public void GetName_WithoutExplicitButWithInitials_ReturnsInitials()
    {
        _ = TestApplication.Instance;
        Avatar avatar = new Avatar { Initials = "JB" };
        AvatarAutomationPeer peer = new AvatarAutomationPeer(avatar);

        peer.GetName().Should().Be("JB");
    }

    [StaFact]
    public void GetName_WithNoExplicitNameAndNoInitials_ReturnsEmpty()
    {
        _ = TestApplication.Instance;
        AvatarAutomationPeer peer = new AvatarAutomationPeer(new Avatar());

        peer.GetName().Should().BeEmpty();
    }
}
