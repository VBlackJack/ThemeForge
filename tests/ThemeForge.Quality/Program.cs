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
using ThemeForge.Theme;

namespace ThemeForge.Quality;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length < 3)
        { Console.Error.WriteLine("Usage: ThemeForge.Quality <visual|performance|document> <config.json> <output-directory> [record-only]"); return 1; }
        int exitCode = 1;
        Application application = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        application.Startup += async (_, _) =>
        {
            try
            {
                QualityOptions options = JsonSerializer.Deserialize<QualityOptions>(await File.ReadAllTextAsync(args[1]))
                    ?? throw new ArgumentException("Missing workload configuration.");
                options.Validate(); Directory.CreateDirectory(args[2]);
                application.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(QualityWindow.StylesUri) });
                using ThemeService service = new ThemeService(application);
                object result = args[0] switch
                {
                    "visual" => VisualScenario.Run(application, service, options, args[2]),
                    "performance" => PerformanceScenario.Run(application, service, options, args.Contains("record-only", StringComparer.Ordinal)),
                    "document" => await DocumentScenario.RunAsync(application, service, options, args[2]),
                    _ => throw new ArgumentException("Unknown workload."),
                };
                await File.WriteAllTextAsync(Path.Combine(args[2], args[0] + ".json"), JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
                Console.WriteLine(JsonSerializer.Serialize(result)); exitCode = 0;
            }
            catch (Exception exception) { Console.Error.WriteLine(exception); }
            finally { application.Shutdown(); }
        };
        application.Run(); return exitCode;
    }
}
