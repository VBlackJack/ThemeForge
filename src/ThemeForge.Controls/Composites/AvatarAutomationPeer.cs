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
/// Automation peer for <see cref="Avatar"/>. Reports <c>Image</c> when the
/// avatar is showing a Source bitmap, <c>Text</c> when showing initials.
/// Accessible name falls back to <see cref="Avatar.Initials"/> when no
/// explicit <c>AutomationProperties.Name</c> is set.
/// </summary>
public sealed class AvatarAutomationPeer : FrameworkElementAutomationPeer
{
    public AvatarAutomationPeer(Avatar owner) : base(owner)
    {
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        if (Owner is Avatar avatar && avatar.IsImageMode)
        {
            return AutomationControlType.Image;
        }
        return AutomationControlType.Text;
    }

    protected override string GetClassNameCore() => nameof(Avatar);

    protected override string GetNameCore()
    {
        string explicitName = base.GetNameCore();
        if (!string.IsNullOrEmpty(explicitName))
        {
            return explicitName;
        }
        if (Owner is Avatar avatar && !string.IsNullOrEmpty(avatar.Initials))
        {
            return avatar.Initials;
        }
        return string.Empty;
    }
}
