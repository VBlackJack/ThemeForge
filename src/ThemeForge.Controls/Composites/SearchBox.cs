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
using System.Windows.Input;
using System.Windows.Media;

namespace ThemeForge.Controls.Composites;

/// <summary>
/// Text input optimized for search: placeholder text when empty, optional
/// clear button when filled, leading search icon, and a <see cref="SearchCommand"/>
/// invoked on Enter. Inherits <see cref="TextBox"/> to keep the native editor
/// semantics (caret, selection, input method).
/// </summary>
public sealed class SearchBox : TextBox
{
    private const string PartClearButton = "PART_ClearButton";

    static SearchBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SearchBox),
            new FrameworkPropertyMetadata(typeof(SearchBox)));
    }

    /// <summary>Hint text shown when Text is empty.</summary>
    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(SearchBox),
            new PropertyMetadata(string.Empty));

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    /// <summary>When true, a clear (×) button appears once Text is non-empty.</summary>
    public static readonly DependencyProperty IsClearableProperty =
        DependencyProperty.Register(
            nameof(IsClearable),
            typeof(bool),
            typeof(SearchBox),
            new PropertyMetadata(true));

    public bool IsClearable
    {
        get => (bool)GetValue(IsClearableProperty);
        set => SetValue(IsClearableProperty, value);
    }

    /// <summary>Command invoked when the user presses Enter while focus is in the box.</summary>
    public static readonly DependencyProperty SearchCommandProperty =
        DependencyProperty.Register(
            nameof(SearchCommand),
            typeof(ICommand),
            typeof(SearchBox),
            new PropertyMetadata(null));

    public ICommand? SearchCommand
    {
        get => (ICommand?)GetValue(SearchCommandProperty);
        set => SetValue(SearchCommandProperty, value);
    }

    /// <summary>Parameter passed to <see cref="SearchCommand"/>; falls back to current Text when null.</summary>
    public static readonly DependencyProperty SearchCommandParameterProperty =
        DependencyProperty.Register(
            nameof(SearchCommandParameter),
            typeof(object),
            typeof(SearchBox),
            new PropertyMetadata(null));

    public object? SearchCommandParameter
    {
        get => GetValue(SearchCommandParameterProperty);
        set => SetValue(SearchCommandParameterProperty, value);
    }

    /// <summary>Leading icon geometry (default: magnifier glyph from the theme style).</summary>
    public static readonly DependencyProperty IconGeometryProperty =
        DependencyProperty.Register(
            nameof(IconGeometry),
            typeof(Geometry),
            typeof(SearchBox),
            new PropertyMetadata(null));

    public Geometry? IconGeometry
    {
        get => (Geometry?)GetValue(IconGeometryProperty);
        set => SetValue(IconGeometryProperty, value);
    }

    /// <summary>True when Text is non-empty. Drives Placeholder / ClearButton visibility triggers.</summary>
    private static readonly DependencyPropertyKey HasTextPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(HasText),
            typeof(bool),
            typeof(SearchBox),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HasTextProperty = HasTextPropertyKey.DependencyProperty;

    public bool HasText => (bool)GetValue(HasTextProperty);

    private Button? _clearButton;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_clearButton is not null)
        {
            _clearButton.Click -= OnClearClicked;
        }

        _clearButton = GetTemplateChild(PartClearButton) as Button;

        if (_clearButton is not null)
        {
            _clearButton.Click += OnClearClicked;
        }
    }

    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        base.OnTextChanged(e);
        SetValue(HasTextPropertyKey, !string.IsNullOrEmpty(Text));
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Enter && SearchCommand is not null)
        {
            var parameter = SearchCommandParameter ?? Text;
            if (SearchCommand.CanExecute(parameter))
            {
                SearchCommand.Execute(parameter);
                e.Handled = true;
            }
        }
    }

    protected override AutomationPeer OnCreateAutomationPeer()
        => new SearchBoxAutomationPeer(this);

    private void OnClearClicked(object sender, RoutedEventArgs e)
    {
        Clear();
        Focus();
    }
}
