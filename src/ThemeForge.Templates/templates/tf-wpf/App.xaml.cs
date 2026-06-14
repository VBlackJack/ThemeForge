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
using Microsoft.Extensions.DependencyInjection;
using ThemeForge.Theme;
using ThemeForge.Theme.DependencyInjection;

namespace ThemeForge.WpfApp;

/// <summary>
/// Application entry point. Wires the full ThemeForge set-and-forget stack in one
/// place: register the engine, run the bootstrap, then show the main window.
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _services;
    private IDisposable? _themeForge;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        AppThemeConfig themeConfig = new AppThemeConfig();
        ServiceCollection services = new ServiceCollection();
        services.AddThemeForge(this, options =>
        {
            options.DefaultTheme = themeConfig.DarkTheme;
            options.WindowsFollow = themeConfig.Follow;
            options.FollowWindowsByDefault = true;
            options.ApplicationName = "ThemeForge.WpfApp";
        });
        services.AddSingleton(themeConfig);
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();

        _services = services.BuildServiceProvider();

        // Restores the persisted choice (or the default), arms Follow Windows, and
        // wires auto-save. The bootstrap owns the theme end to end.
        _themeForge = _services.UseThemeForge();
        _services.GetRequiredService<MainWindow>().Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _themeForge?.Dispose();
        _services?.Dispose();
        base.OnExit(e);
    }
}
