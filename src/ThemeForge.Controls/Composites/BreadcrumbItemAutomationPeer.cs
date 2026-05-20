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

namespace ThemeForge.Controls.Composites;

public sealed class BreadcrumbItemAutomationPeer : ButtonBaseAutomationPeer, IInvokeProvider
{
    public BreadcrumbItemAutomationPeer(BreadcrumbItem owner) : base(owner)
    {
    }

    private BreadcrumbItem OwnerItem => (BreadcrumbItem)Owner;

    protected override string GetClassNameCore()
        => nameof(BreadcrumbItem);

    protected override AutomationControlType GetAutomationControlTypeCore()
        => AutomationControlType.Button;

    public override object? GetPattern(PatternInterface patternInterface)
        => patternInterface == PatternInterface.Invoke
            ? this
            : base.GetPattern(patternInterface);

    protected override string GetNameCore()
    {
        string explicitName = AutomationProperties.GetName(Owner);
        if (!string.IsNullOrWhiteSpace(explicitName))
        {
            return explicitName;
        }

        if (OwnerItem.Content is string text)
        {
            return text;
        }

        return string.Empty;
    }

    public void Invoke()
    {
        if (!OwnerItem.IsEnabled)
        {
            throw new ElementNotEnabledException();
        }

        OwnerItem.AutomationInvoke();
    }
}
