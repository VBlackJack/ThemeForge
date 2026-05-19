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
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;

namespace ThemeForge.Controls.Composites;

/// <summary>
/// Circular avatar with two display modes: <see cref="Source"/> (image) takes
/// priority when set; otherwise <see cref="Initials"/> are rendered on the
/// control's <c>Background</c>. <see cref="Size"/> sets both Width and Height
/// in one shot so the avatar stays perfectly round.
/// </summary>
public sealed class Avatar : Control
{
    static Avatar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Avatar),
            new FrameworkPropertyMetadata(typeof(Avatar)));
    }

    public Avatar()
    {
        // Sync Width/Height with the default Size at construction — the
        // PropertyChangedCallback does not fire for the DP's default value.
        Width = Size;
        Height = Size;
    }

    /// <summary>Image source displayed in the avatar disk. When null, Initials are shown.</summary>
    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(
            nameof(Source),
            typeof(ImageSource),
            typeof(Avatar),
            new PropertyMetadata(null, OnSourceChanged));

    public ImageSource? Source
    {
        get => (ImageSource?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>Initials shown when no Source is provided (typically 1–3 letters).</summary>
    public static readonly DependencyProperty InitialsProperty =
        DependencyProperty.Register(
            nameof(Initials),
            typeof(string),
            typeof(Avatar),
            new PropertyMetadata(string.Empty));

    public string Initials
    {
        get => (string)GetValue(InitialsProperty);
        set => SetValue(InitialsProperty, value);
    }

    /// <summary>Diameter of the avatar. Sets both Width and Height for guaranteed roundness.</summary>
    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(
            nameof(Size),
            typeof(double),
            typeof(Avatar),
            new PropertyMetadata(40.0, OnSizeChanged));

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>True when <see cref="Source"/> is set. Drives the template trigger that swaps Image / Initials.</summary>
    private static readonly DependencyPropertyKey IsImageModePropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsImageMode),
            typeof(bool),
            typeof(Avatar),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsImageModeProperty = IsImageModePropertyKey.DependencyProperty;

    public bool IsImageMode => (bool)GetValue(IsImageModeProperty);

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer()
        => new AvatarAutomationPeer(this);

    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        Avatar avatar = (Avatar)d;
        avatar.SetValue(IsImageModePropertyKey, e.NewValue is not null);
    }

    private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        Avatar avatar = (Avatar)d;
        double size = (double)e.NewValue;
        if (size > 0)
        {
            avatar.Width = size;
            avatar.Height = size;
        }
    }
}
