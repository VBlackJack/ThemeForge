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
using DraculaForge.Studio.ViewModels;
using DraculaForge.Studio.Views;
using DraculaForge.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace DraculaForge.Studio;

/// <summary>
/// Studio application entry point. Builds the DI container, wires the
/// IThemeService, registers gallery sections, and shows MainWindow.
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _services;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var collection = new ServiceCollection();
        ConfigureServices(collection);
        _services = collection.BuildServiceProvider();

        // Apply the default theme before any window is shown so the first
        // measure pass already has the correct brushes available.
        var themeService = _services.GetRequiredService<IThemeService>();
        themeService.ApplyTheme(ThemeNames.Dracula);

        var mainWindow = _services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // ThemeService is bound to the live Application so it can mutate
        // Application.Resources.MergedDictionaries at runtime.
        services.AddSingleton<IThemeService>(_ => new ThemeService(this));

        // Each section eagerly instantiates its UserControl. The cost is
        // negligible (7 controls, no heavy resources) and keeps the
        // navigation snappy.
        services.AddSingleton<MainViewModel>(sp =>
        {
            var themeService = sp.GetRequiredService<IThemeService>();
            return new MainViewModel(
                themeService,
                new[]
                {
                    new GallerySectionViewModel("Palette",     new PaletteView()),
                    new GallerySectionViewModel("Buttons",     new ButtonsView()),
                    new GallerySectionViewModel("Inputs",      new InputsView()),
                    new GallerySectionViewModel("Lists",       new ListsView()),
                    new GallerySectionViewModel("Feedback",    new FeedbackView()),
                    new GallerySectionViewModel("Containers",  new ContainersView()),
                    new GallerySectionViewModel("Composites",  new CompositesView()),
                    new GallerySectionViewModel("Edit",        new PaletteEditorView(new PaletteEditorViewModel(themeService))),
                });
        });

        services.AddSingleton<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.Dispose();
        base.OnExit(e);
    }
}
