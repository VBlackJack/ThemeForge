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
/// Automation peer for <see cref="SearchBox"/>. Reports Edit control type
/// (inherited from <see cref="TextBoxAutomationPeer"/>), with the accessible
/// name falling back to <see cref="SearchBox.PlaceholderText"/> when no
/// explicit AutomationProperties.Name is set.
/// </summary>
public sealed class SearchBoxAutomationPeer : TextBoxAutomationPeer
{
    public SearchBoxAutomationPeer(SearchBox owner) : base(owner)
    {
    }

    protected override string GetClassNameCore() => nameof(SearchBox);

    protected override string GetNameCore()
    {
        var explicitName = base.GetNameCore();
        if (!string.IsNullOrEmpty(explicitName))
        {
            return explicitName;
        }
        if (Owner is SearchBox box && !string.IsNullOrEmpty(box.PlaceholderText))
        {
            return box.PlaceholderText;
        }
        return string.Empty;
    }
}
