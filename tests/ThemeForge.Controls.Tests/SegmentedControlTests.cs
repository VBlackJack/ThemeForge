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

using System.Windows.Controls;
using FluentAssertions;
using ThemeForge.Controls.Composites;
using Xunit;

namespace ThemeForge.Controls.Tests;

public sealed class SegmentedControlTests
{
    [StaFact]
    public void SelectionMode_SetToMultiple_CoercedToSingle()
    {
        _ = TestApplication.Instance;
        SegmentedControl control = new SegmentedControl();

        control.SelectionMode = SelectionMode.Multiple;

        control.SelectionMode.Should().Be(SelectionMode.Single);
    }

    [StaFact]
    public void SelectionMode_SetToExtended_CoercedToSingle()
    {
        _ = TestApplication.Instance;
        SegmentedControl control = new SegmentedControl();

        control.SelectionMode = SelectionMode.Extended;

        control.SelectionMode.Should().Be(SelectionMode.Single);
    }

}
