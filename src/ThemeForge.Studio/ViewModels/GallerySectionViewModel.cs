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

namespace ThemeForge.Studio.ViewModels;

/// <summary>
/// Lightweight descriptor for an entry in the Studio sidebar navigation.
/// Pairs a human-readable name with the gallery view to display when the
/// section is selected.
/// </summary>
public sealed class GallerySectionViewModel
{
    public GallerySectionViewModel(string name, UserControl view)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(view);
        Name = name;
        View = view;
    }

    /// <summary>Display label shown in the sidebar.</summary>
    public string Name { get; }

    /// <summary>The control rendered in the main content area when selected.</summary>
    public UserControl View { get; }

    /// <summary>
    /// Overridden so the default WPF <c>ListBoxItem</c> automation peer
    /// surfaces the section <see cref="Name"/> instead of the view-model's
    /// fully-qualified type name as the UIA Name.
    /// </summary>
    public override string ToString() => Name;
}
