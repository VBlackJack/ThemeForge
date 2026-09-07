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

using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using ThemeForge.Theme;
using ThemeForge.Theme.Palettes;

namespace ThemeForge.PackageConsumer;

public partial class App
{
    public App()
    {
        Startup += (_, arguments) => Dispatcher.BeginInvoke(new Action(async () => await ProbeAsync(arguments.Args)));
    }
    private async Task ProbeAsync(string[] arguments)
    {
        try
        {
            string output = arguments[0]; string phase = arguments[1];
            int timeout = int.Parse(arguments[2], System.Globalization.CultureInfo.InvariantCulture);
            using CancellationTokenSource deadline = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));
            while (MainWindow?.IsLoaded != true) { await Task.Delay(TimeSpan.FromMilliseconds(50), deadline.Token); }
            IThemeService service = _services!.GetRequiredService<IThemeService>();
            ThemePalette palette = await PaletteJson.LoadAsync(Path.Combine(output, "palette.json"), deadline.Token);
            if (phase == "restore" && (service.CurrentTheme != palette.Name || service.CurrentAccentTint != AccentTint.Cyan))
            { throw new InvalidOperationException("Bootstrap did not restore the external palette preference."); }
            if (phase == "apply")
            {
                service.ApplyTheme(ThemeNames.Folio);
                service.ApplyTheme(palette.Name);
                service.ApplyAccentTint(AccentTint.Cyan);
            }
            PaletteValidation.TryParseColor(palette.Colors["Background"], out Color expected);
            if (((SolidColorBrush)Resources["BackgroundBrush"]).Color != expected)
            { throw new InvalidOperationException("External palette colour mismatch."); }
            if (MainWindow.DataContext is not MainViewModel) { throw new InvalidOperationException("Template view model did not initialize."); }
            object report = new { Phase = phase, Theme = service.CurrentTheme, Accent = service.CurrentAccentTint.ToString(),
                Background = PaletteValidation.ToHex(expected), StartupWindowLoaded = MainWindow.IsLoaded,
                CoreAssembly = typeof(IThemeService).Assembly.Location };
            await File.WriteAllTextAsync(Path.Combine(output, phase + ".json"), JsonSerializer.Serialize(report), deadline.Token);
            Shutdown(0);
        }
        catch (Exception exception)
        {
            await File.WriteAllTextAsync(Path.Combine(arguments[0], "failure.txt"), exception.ToString()); Shutdown(1);
        }
    }
}
