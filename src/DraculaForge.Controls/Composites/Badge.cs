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

namespace DraculaForge.Controls.Composites;

/// <summary>
/// Severity signal carried by a <see cref="Badge"/>. Drives both the
/// background fill and the foreground contrast of the rendered pill.
/// </summary>
public enum BadgeSeverity
{
    /// <summary>Neutral outline badge — no semantic signal.</summary>
    Default,
    /// <summary>Positive / completed signal — maps to SuccessBrush.</summary>
    Success,
    /// <summary>Caution signal — maps to WarningBrush.</summary>
    Warning,
    /// <summary>Failure / blocking signal — maps to ErrorBrush.</summary>
    Error,
    /// <summary>Informational signal — maps to InfoBrush.</summary>
    Info,
}

/// <summary>
/// Small pill control that carries a short textual signal (count, status,
/// label). The visual treatment is driven by <see cref="Severity"/>; the
/// payload is a single string-like <see cref="Content"/>.
/// </summary>
/// <remarks>
/// Usage:
/// <code>
/// &lt;dfc:Badge Content="42"      Severity="Default"/&gt;
/// &lt;dfc:Badge Content="Shipped" Severity="Success"/&gt;
/// </code>
/// Inherits <see cref="Control"/> directly rather than
/// <see cref="ContentControl"/> to keep the surface tight: a Badge is a
/// signal, not a content host. The <see cref="Content"/> DP is declared
/// explicitly and rendered via a <c>ContentPresenter</c> in the template.
/// </remarks>
public sealed class Badge : Control
{
    static Badge()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Badge),
            new FrameworkPropertyMetadata(typeof(Badge)));
    }

    /// <summary>Payload rendered inside the pill (typically a string).</summary>
    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(
            nameof(Content),
            typeof(object),
            typeof(Badge),
            new PropertyMetadata(null));

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>Severity signal that drives the pill's color treatment.</summary>
    public static readonly DependencyProperty SeverityProperty =
        DependencyProperty.Register(
            nameof(Severity),
            typeof(BadgeSeverity),
            typeof(Badge),
            new PropertyMetadata(BadgeSeverity.Default));

    public BadgeSeverity Severity
    {
        get => (BadgeSeverity)GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    /// <summary>
    /// Exposes a UIA peer whose Name surfaces <see cref="Content"/> as text,
    /// so screen readers and automation harnesses pick up the badge payload.
    /// </summary>
    protected override AutomationPeer OnCreateAutomationPeer() => new BadgeAutomationPeer(this);
}

/// <summary>
/// UIA peer for <see cref="Badge"/>. Reports the control as <c>Text</c>
/// and projects <see cref="Badge.Content"/> as the accessible Name.
/// </summary>
internal sealed class BadgeAutomationPeer : FrameworkElementAutomationPeer
{
    public BadgeAutomationPeer(Badge owner) : base(owner)
    {
    }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Text;

    protected override string GetNameCore()
    {
        var owner = (Badge)Owner;
        return owner.Content?.ToString() ?? base.GetNameCore();
    }

    protected override string GetClassNameCore() => nameof(Badge);
}
