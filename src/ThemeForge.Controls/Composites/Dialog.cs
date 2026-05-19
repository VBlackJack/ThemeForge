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
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ThemeForge.Controls.Composites;

/// <summary>
/// Content dialog surface with optional header, body, footer, severity accent,
/// and close command plumbing. The surrounding host owns modal behavior.
/// </summary>
public sealed class Dialog : HeaderedContentControl
{
    private const string PartCloseButton = "PART_CloseButton";

    private ButtonBase? _closeButton;
    private bool _isClosed;

    static Dialog()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Dialog),
            new FrameworkPropertyMetadata(typeof(Dialog)));
    }

    /// <summary>Identifies the Footer dependency property.</summary>
    public static readonly DependencyProperty FooterProperty =
        DependencyProperty.Register(
            nameof(Footer),
            typeof(object),
            typeof(Dialog),
            new PropertyMetadata(null));

    /// <summary>Gets or sets optional trailing content for actions or status.</summary>
    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    /// <summary>Identifies the FooterTemplate dependency property.</summary>
    public static readonly DependencyProperty FooterTemplateProperty =
        DependencyProperty.Register(
            nameof(FooterTemplate),
            typeof(DataTemplate),
            typeof(Dialog),
            new PropertyMetadata(null));

    /// <summary>Gets or sets an optional template for the footer content.</summary>
    public DataTemplate? FooterTemplate
    {
        get => (DataTemplate?)GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }

    /// <summary>Identifies the Severity dependency property.</summary>
    public static readonly DependencyProperty SeverityProperty =
        DependencyProperty.Register(
            nameof(Severity),
            typeof(DialogSeverity),
            typeof(Dialog),
            new PropertyMetadata(DialogSeverity.Default));

    /// <summary>Gets or sets the semantic accent displayed by the dialog.</summary>
    public DialogSeverity Severity
    {
        get => (DialogSeverity)GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    /// <summary>Identifies the IconGeometry dependency property.</summary>
    public static readonly DependencyProperty IconGeometryProperty =
        DependencyProperty.Register(
            nameof(IconGeometry),
            typeof(Geometry),
            typeof(Dialog),
            new PropertyMetadata(null));

    /// <summary>Gets or sets an optional geometry rendered before the header.</summary>
    public Geometry? IconGeometry
    {
        get => (Geometry?)GetValue(IconGeometryProperty);
        set => SetValue(IconGeometryProperty, value);
    }

    /// <summary>Identifies the IsClosable dependency property.</summary>
    public static readonly DependencyProperty IsClosableProperty =
        DependencyProperty.Register(
            nameof(IsClosable),
            typeof(bool),
            typeof(Dialog),
            new PropertyMetadata(true));

    /// <summary>Gets or sets whether the template close button is visible.</summary>
    public bool IsClosable
    {
        get => (bool)GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    /// <summary>Identifies the CloseCommand dependency property.</summary>
    public static readonly DependencyProperty CloseCommandProperty =
        DependencyProperty.Register(
            nameof(CloseCommand),
            typeof(ICommand),
            typeof(Dialog),
            new PropertyMetadata(null));

    /// <summary>Gets or sets the command invoked by <see cref="Close"/>.</summary>
    public ICommand? CloseCommand
    {
        get => (ICommand?)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    /// <summary>Raised once when the dialog closes.</summary>
    public event EventHandler? Closed;

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (_closeButton is not null)
        {
            _closeButton.Click -= OnCloseClicked;
        }

        _closeButton = GetTemplateChild(PartCloseButton) as ButtonBase;
        if (_closeButton is not null)
        {
            _closeButton.Click += OnCloseClicked;
        }
    }

    /// <summary>
    /// Closes the dialog once, executes <see cref="CloseCommand"/> when
    /// allowed, then raises <see cref="Closed"/>.
    /// </summary>
    public void Close()
    {
        if (_isClosed)
        {
            return;
        }

        _isClosed = true;
        ICommand? closeCommand = CloseCommand;
        if (closeCommand?.CanExecute(null) == true)
        {
            closeCommand.Execute(null);
        }

        Closed?.Invoke(this, EventArgs.Empty);
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e)
        => Close();
}
