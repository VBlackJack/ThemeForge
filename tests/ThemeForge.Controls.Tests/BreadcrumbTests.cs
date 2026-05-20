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

public sealed class BreadcrumbTests
{
    [StaFact]
    public void OnItemsChanged_SetsAlternationCountToItemCount()
    {
        _ = TestApplication.Instance;
        Breadcrumb control = new Breadcrumb();

        control.Items.Add("Home");
        control.Items.Add("Projects");
        control.Items.Add("ThemeForge");

        control.AlternationCount.Should().Be(3);
    }

    [StaFact]
    public void AlternationCount_TracksFurtherAdds()
    {
        _ = TestApplication.Instance;
        Breadcrumb control = new Breadcrumb();

        control.Items.Add("Home");
        control.Items.Add("Projects");
        control.Items.Add("ThemeForge");
        control.Items.Add("Controls");

        control.AlternationCount.Should().Be(4);
    }

    [StaFact]
    public void IsCurrent_DefaultsToFalse()
    {
        _ = TestApplication.Instance;
        BreadcrumbItem item = new BreadcrumbItem();

        item.IsCurrent.Should().BeFalse();
    }

    [StaFact]
    public void IsCurrent_IsSettable()
    {
        _ = TestApplication.Instance;
        BreadcrumbItem item = new BreadcrumbItem();

        item.IsCurrent = true;

        item.IsCurrent.Should().BeTrue();
    }
}
