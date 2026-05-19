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
using ThemeForge.Controls.Composites;
using Xunit;

namespace ThemeForge.Controls.Tests;

/// <summary>
/// Behavioral tests for the <see cref="Toast"/> control. AutomationPeer
/// invariants live in <see cref="ToastAutomationPeerTests"/>.
/// </summary>
public sealed class ToastTests
{
    [StaFact]
    public void Dismiss_RaisesDismissedEventOnce()
    {
        _ = TestApplication.Instance;
        var toast = new Toast { Title = "Test", Message = "Body" };
        int dismissedCount = 0;
        toast.Dismissed += (_, _) => dismissedCount++;

        toast.Dismiss();

        dismissedCount.Should().Be(1);
    }

    [StaFact]
    public void Dismiss_CalledTwice_RaisesEventTwice()
    {
        // Current contract: Dismiss is not guarded; each call raises the event.
        // ToastHost handles the cleanup so a second Dismiss on an already-removed
        // toast is a no-op from the host's perspective (Items.Remove on missing
        // item is safe). If a "single-shot" guard becomes desirable, change the
        // contract and update this test.
        _ = TestApplication.Instance;
        var toast = new Toast();
        int count = 0;
        toast.Dismissed += (_, _) => count++;

        toast.Dismiss();
        toast.Dismiss();

        count.Should().Be(2);
    }

    [StaFact]
    public void Dismiss_WithNoSubscriber_DoesNotThrow()
    {
        _ = TestApplication.Instance;
        var toast = new Toast();

        var act = toast.Dismiss;

        act.Should().NotThrow();
    }
}
