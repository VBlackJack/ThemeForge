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

using System.Windows.Automation.Peers;

namespace ThemeForge.Controls.Composites;

/// <summary>
/// Automation peer for <see cref="Card"/>. Reports the card as a Group control
/// (it is a structural container, not an interactive control), and derives its
/// accessible name from <see cref="System.Windows.Controls.HeaderedContentControl.Header"/>
/// when no explicit <c>AutomationProperties.Name</c> has been set.
/// </summary>
public sealed class CardAutomationPeer : FrameworkElementAutomationPeer
{
    public CardAutomationPeer(Card owner) : base(owner)
    {
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
        => AutomationControlType.Group;

    protected override string GetClassNameCore()
        => nameof(Card);

    protected override string GetNameCore()
    {
        // 1. An explicit AutomationProperties.Name on the Card wins.
        string explicitName = base.GetNameCore();
        if (!string.IsNullOrEmpty(explicitName))
        {
            return explicitName;
        }

        // 2. Otherwise, fall back to Header.ToString() when the header is set.
        if (Owner is Card card && card.Header is { } header)
        {
            return header.ToString() ?? string.Empty;
        }

        // 3. No name available — let consumers know via empty string (default).
        return string.Empty;
    }
}
