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
using System.Windows.Threading;

namespace ThemeForge.Controls.Composites;

/// <summary>
/// Ephemeral notification with title, message, severity tone, and auto-dismiss
/// after <see cref="Duration"/>. Raises <see cref="Dismissed"/> when the user
/// closes it or the timer elapses; the hosting <see cref="ToastHost"/>
/// observes that event to remove the toast from its items.
/// </summary>
public sealed class Toast : Control
{
    private const string PartCloseButton = "PART_CloseButton";

    static Toast()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Toast),
            new FrameworkPropertyMetadata(typeof(Toast)));
    }

    public Toast()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(Toast),
            new PropertyMetadata(string.Empty));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(Toast),
            new PropertyMetadata(string.Empty));

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly DependencyProperty SeverityProperty =
        DependencyProperty.Register(
            nameof(Severity),
            typeof(ToastSeverity),
            typeof(Toast),
            new PropertyMetadata(ToastSeverity.Info));

    public ToastSeverity Severity
    {
        get => (ToastSeverity)GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    /// <summary>Auto-dismiss delay. <see cref="TimeSpan.Zero"/> disables auto-dismiss.</summary>
    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(
            nameof(Duration),
            typeof(TimeSpan),
            typeof(Toast),
            new PropertyMetadata(TimeSpan.FromSeconds(3)));

    public TimeSpan Duration
    {
        get => (TimeSpan)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    /// <summary>Raised when the toast is dismissed (timer elapsed or close button clicked).</summary>
    public event EventHandler? Dismissed;

    private DispatcherTimer? _dismissTimer;
    private bool _isDismissed;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (GetTemplateChild(PartCloseButton) is Button closeButton)
        {
            closeButton.Click -= OnCloseClicked;
            closeButton.Click += OnCloseClicked;
        }
    }

    /// <summary>
    /// Dismiss the toast. Raises <see cref="Dismissed"/> the first time it
    /// is called; subsequent calls are no-ops (single-shot contract).
    /// </summary>
    public void Dismiss()
    {
        if (_isDismissed)
        {
            return;
        }

        _isDismissed = true;
        StopTimer();
        Dismissed?.Invoke(this, EventArgs.Empty);
    }

    protected override AutomationPeer OnCreateAutomationPeer()
        => new ToastAutomationPeer(this);

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (Duration > TimeSpan.Zero)
        {
            _dismissTimer = new DispatcherTimer { Interval = Duration };
            _dismissTimer.Tick += OnDismissTimerTick;
            _dismissTimer.Start();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
        => StopTimer();

    private void OnDismissTimerTick(object? sender, EventArgs e)
    {
        StopTimer();
        Dismiss();
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e)
        => Dismiss();

    private void StopTimer()
    {
        if (_dismissTimer is not null)
        {
            _dismissTimer.Tick -= OnDismissTimerTick;
            _dismissTimer.Stop();
            _dismissTimer = null;
        }
    }
}
