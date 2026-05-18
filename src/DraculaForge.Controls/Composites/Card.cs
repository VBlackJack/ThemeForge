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

namespace DraculaForge.Controls.Composites;

/// <summary>
/// Surface-elevated container with optional header and footer regions.
/// Inherits <see cref="HeaderedContentControl"/> (Header + Content = body)
/// and adds a <see cref="Footer"/> dependency property for a trailing slot.
/// </summary>
/// <remarks>
/// Usage:
/// <code>
/// &lt;dfc:Card Header="Settings" Footer="Apply"&gt;
///   &lt;StackPanel&gt;...&lt;/StackPanel&gt;
/// &lt;/dfc:Card&gt;
/// </code>
/// </remarks>
public sealed class Card : HeaderedContentControl
{
    static Card()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Card),
            new FrameworkPropertyMetadata(typeof(Card)));
    }

    /// <summary>Optional trailing content (action row, status line, etc.).</summary>
    public static readonly DependencyProperty FooterProperty =
        DependencyProperty.Register(
            nameof(Footer),
            typeof(object),
            typeof(Card),
            new PropertyMetadata(null));

    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    /// <summary>Optional explicit template for the Footer content.</summary>
    public static readonly DependencyProperty FooterTemplateProperty =
        DependencyProperty.Register(
            nameof(FooterTemplate),
            typeof(DataTemplate),
            typeof(Card),
            new PropertyMetadata(null));

    public DataTemplate? FooterTemplate
    {
        get => (DataTemplate?)GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }
}
