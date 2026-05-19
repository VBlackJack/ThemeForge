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

namespace ThemeForge.Controls.Composites;

/// <summary>
/// Automation peer for <see cref="Toast"/>. Reports Group control type and
/// sets the live region politeness based on <see cref="Toast.Severity"/> so
/// screen readers announce the toast when it appears.
/// </summary>
public sealed class ToastAutomationPeer : FrameworkElementAutomationPeer
{
    public ToastAutomationPeer(Toast owner) : base(owner)
    {
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
        => AutomationControlType.Group;

    protected override string GetClassNameCore() => nameof(Toast);

    protected override string GetNameCore()
    {
        string explicitName = base.GetNameCore();
        if (!string.IsNullOrEmpty(explicitName))
        {
            return explicitName;
        }
        if (Owner is Toast toast)
        {
            // Compose "Title — Message" for richer screen reader narration.
            if (!string.IsNullOrEmpty(toast.Title) && !string.IsNullOrEmpty(toast.Message))
            {
                return $"{toast.Title} — {toast.Message}";
            }
            if (!string.IsNullOrEmpty(toast.Title))
            {
                return toast.Title;
            }
            if (!string.IsNullOrEmpty(toast.Message))
            {
                return toast.Message;
            }
        }
        return string.Empty;
    }

    protected override AutomationLiveSetting GetLiveSettingCore()
    {
        if (Owner is Toast toast && toast.Severity == ToastSeverity.Error)
        {
            return AutomationLiveSetting.Assertive;
        }
        return AutomationLiveSetting.Polite;
    }
}
