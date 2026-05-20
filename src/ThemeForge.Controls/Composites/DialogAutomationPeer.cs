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
/// Automation peer for <see cref="Dialog"/>. Reports the dialog as a Pane
/// control (modal-like structural container that hosts focused content),
/// and derives its accessible name from
/// <see cref="System.Windows.Controls.HeaderedContentControl.Header"/> when
/// no explicit <c>AutomationProperties.Name</c> has been set.
/// </summary>
public sealed class DialogAutomationPeer : FrameworkElementAutomationPeer
{
    public DialogAutomationPeer(Dialog owner) : base(owner)
    {
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
        => AutomationControlType.Pane;

    protected override string GetClassNameCore()
        => nameof(Dialog);

    protected override string GetNameCore()
    {
        string explicitName = base.GetNameCore();
        if (!string.IsNullOrEmpty(explicitName))
        {
            return explicitName;
        }

        if (Owner is Dialog dialog && dialog.Header is string headerText)
        {
            return headerText;
        }

        return string.Empty;
    }
}
