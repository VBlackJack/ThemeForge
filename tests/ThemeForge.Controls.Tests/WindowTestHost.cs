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

using System;
using System.Windows;
using System.Windows.Threading;

namespace ThemeForge.Controls.Tests;

/// <summary>
/// Hosts a <see cref="FrameworkElement"/> in an off-screen <see cref="Window"/>
/// and forces a layout pass for generated containers and automation peers.
/// </summary>
internal static class WindowTestHost
{
    public static void Render(FrameworkElement content, Action<Window> assert)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(assert);

        Application? application = Application.Current;
        ShutdownMode originalShutdownMode = ShutdownMode.OnLastWindowClose;
        bool restoreShutdownMode = false;
        if (application is not null && application.Dispatcher.CheckAccess())
        {
            originalShutdownMode = application.ShutdownMode;
            application.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            restoreShutdownMode = true;
        }

        Window window = new Window
        {
            Width = 400,
            Height = 120,
            Left = -10000,
            Top = -10000,
            ShowInTaskbar = false,
            ShowActivated = false,
            WindowStyle = WindowStyle.None,
            Content = content,
        };

        try
        {
            window.Show();
            window.UpdateLayout();
            window.Dispatcher.Invoke(static () => { }, DispatcherPriority.Loaded);
            content.UpdateLayout();
            assert(window);
        }
        finally
        {
            window.Content = null;
            window.Close();
            if (restoreShutdownMode)
            {
                application!.ShutdownMode = originalShutdownMode;
            }
        }
    }
}
