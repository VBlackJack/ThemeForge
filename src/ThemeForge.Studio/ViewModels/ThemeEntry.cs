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

namespace ThemeForge.Studio.ViewModels;

/// <summary>
/// Pairs a theme name with its display label and family group so the picker
/// ComboBox can render disambiguated labels (Dracula/Drakul) and group by
/// family (Root / Dark / Light / Alt) via CollectionViewSource.
/// </summary>
/// <param name="Name">Canonical theme name passed to IThemeService.ApplyTheme.</param>
/// <param name="DisplayName">Human label shown in the picker.</param>
/// <param name="Family">Family group label.</param>
public sealed record ThemeEntry(string Name, string DisplayName, string Family)
{
    /// <summary>
    /// Overrides the record-generated <see cref="object.ToString"/> so the
    /// default <c>ComboBoxItem</c> automation peer surfaces just the
    /// display name instead of the full positional-record dump
    /// (<c>ThemeEntry { Name = ..., DisplayName = ..., Family = ... }</c>).
    /// </summary>
    public override string ToString() => DisplayName;
}
