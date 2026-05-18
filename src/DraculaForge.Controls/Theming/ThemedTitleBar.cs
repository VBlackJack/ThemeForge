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

using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DraculaForge.Controls.Theming;

/// <summary>
/// Attached property that switches the Win32 title bar of a WPF
/// <see cref="Window"/> into the OS immersive dark mode, so the chrome
/// painted by DWM matches a dark Dracula theme.
/// </summary>
/// <remarks>
/// Usage in XAML:
/// <code>
///   xmlns:dft="clr-namespace:DraculaForge.Controls.Theming;assembly=DraculaForge.Controls"
///   ...
///   &lt;Window dft:ThemedTitleBar.IsEnabled="True" ...&gt;
/// </code>
/// The attribute call is a no-op on Windows builds older than 10 1809
/// (build 17763), so it is safe to enable unconditionally.
/// </remarks>
public static class ThemedTitleBar
{
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_PRE_20H1 = 19;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(ThemedTitleBar),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Window window || e.NewValue is not bool enabled)
        {
            return;
        }

        if (window.IsLoaded)
        {
            Apply(window, enabled);
        }
        else
        {
            window.SourceInitialized += (_, _) => Apply(window, enabled);
        }
    }

    private static void Apply(Window window, bool enabled)
    {
        var handle = new WindowInteropHelper(window).Handle;
        if (handle == IntPtr.Zero)
        {
            return;
        }

        int useImmersiveDarkMode = enabled ? 1 : 0;
        // Try the modern attribute first; fall back to the pre-20H1 alias for
        // older Windows 10 builds. Both calls are no-ops on Windows < 1809.
        int hr = DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE,
            ref useImmersiveDarkMode, sizeof(int));
        if (hr != 0)
        {
            _ = DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE_PRE_20H1,
                ref useImmersiveDarkMode, sizeof(int));
        }
    }

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd, int attribute, ref int pvAttribute, int cbAttribute);
}
