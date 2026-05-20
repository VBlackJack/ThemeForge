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

using System.Windows;
using System.Windows.Controls;

namespace ThemeForge.Controls.Composites;

public sealed class SegmentedControl : ListBox
{
    static SegmentedControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SegmentedControl),
            new FrameworkPropertyMetadata(typeof(SegmentedControl)));
        SelectionModeProperty.OverrideMetadata(
            typeof(SegmentedControl),
            new FrameworkPropertyMetadata(SelectionMode.Single, null, CoerceSelectionMode));
    }

    protected override DependencyObject GetContainerForItemOverride()
        => new SegmentItem();

    protected override bool IsItemItsOwnContainerOverride(object item)
        => item is SegmentItem;

    private static object CoerceSelectionMode(DependencyObject d, object baseValue)
        => SelectionMode.Single;
}
