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
using System.Windows.Interop;
using System.Windows.Media;

namespace ThemeForge.Theme.Interop;

/// <summary>
/// Applies the title bar plan to one window and keeps it synchronized with theme
/// changes. Defers to <see cref="Window.SourceInitialized"/> when the native handle
/// is not yet created. Per-window Win32 timing, not Application lifecycle coupling.
/// </summary>
internal sealed class TitleBarThemer : IDisposable
{
    private const string BackgroundColorKey = "BackgroundColor";
    private const string ForegroundColorKey = "ForegroundColor";

    private readonly Window _window;
    private readonly IThemeService _themeService;
    private readonly TitleBarOptions _options;
    private readonly INativeWindowChrome _chrome;
    private readonly Version _osVersion;

    private bool _hasRun;
    private bool _sourceHooked;
    private bool _disposed;

    public TitleBarThemer(
        Window window,
        IThemeService themeService,
        TitleBarOptions options,
        INativeWindowChrome chrome,
        Version osVersion)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(themeService);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(chrome);
        ArgumentNullException.ThrowIfNull(osVersion);
        _window = window;
        _themeService = themeService;
        _options = options;
        _chrome = chrome;
        _osVersion = osVersion;
    }

    /// <summary>
    /// Runs once: subscribes to theme changes and the window's lifetime, then applies
    /// the plan immediately when the handle exists or defers to SourceInitialized.
    /// A second call is a no-op.
    /// </summary>
    public void Apply()
    {
        if (_hasRun)
        {
            return;
        }

        _hasRun = true;
        _themeService.ThemeChanged += OnThemeChanged;
        _window.Closed += OnWindowClosed;

        IntPtr handle = new WindowInteropHelper(_window).Handle;
        if (handle == IntPtr.Zero)
        {
            if (!_sourceHooked)
            {
                _sourceHooked = true;
                _window.SourceInitialized += OnSourceInitialized;
            }

            return;
        }

        ApplyToHandle(handle);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _themeService.ThemeChanged -= OnThemeChanged;
        _window.SourceInitialized -= OnSourceInitialized;
        _window.Closed -= OnWindowClosed;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        _window.SourceInitialized -= OnSourceInitialized;
        IntPtr handle = new WindowInteropHelper(_window).Handle;
        if (handle != IntPtr.Zero)
        {
            ApplyToHandle(handle);
        }
    }

    private void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
    {
        IntPtr handle = new WindowInteropHelper(_window).Handle;
        if (handle != IntPtr.Zero)
        {
            ApplyToHandle(handle);
        }
    }

    private void OnWindowClosed(object? sender, EventArgs e) => Dispose();

    private void ApplyToHandle(IntPtr handle)
    {
        Color? background = ReadColor(BackgroundColorKey);
        if (background is not Color backgroundColor)
        {
            return; // Best-effort: nothing to derive the caption mode from.
        }

        Color foreground = ReadColor(ForegroundColorKey) ?? backgroundColor;
        (Color caption, Color text, Color? border) =
            TitleBarPlanner.ResolveColors(backgroundColor, foreground, _options);
        IReadOnlyList<TitleBarPlanner.DwmInstruction> plan =
            TitleBarPlanner.BuildPlan(backgroundColor, caption, text, border, _osVersion);

        foreach (TitleBarPlanner.DwmInstruction instruction in plan)
        {
            PushAttribute(handle, instruction);
        }
    }

    private void PushAttribute(IntPtr handle, TitleBarPlanner.DwmInstruction instruction)
    {
        try
        {
            // The plan is pre-gated by OS build, so a failure here is unexpected and
            // worth reporting; theming stays best-effort and never throws.
            if (!_chrome.TrySetAttribute(handle, instruction.Attribute, instruction.Value))
            {
                _options.OnError?.Invoke(new InvalidOperationException(
                    $"Failed to set window attribute {instruction.Attribute}."));
            }
        }
        catch (Exception ex)
        {
            _options.OnError?.Invoke(ex);
        }
    }

    private Color? ReadColor(string key)
        => _window.TryFindResource(key) is Color color ? color : null;
}
