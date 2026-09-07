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

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using ThemeForge.Theme;
using ThemeForge.Studio.ViewModels;
using ThemeForge.Studio.Views;

namespace ThemeForge.Quality;

internal static class PerformanceScenario
{
    internal static object Run(Application application, ThemeService service, QualityOptions options, bool recordOnly)
    {
        Stopwatch startup = Stopwatch.StartNew();
        service.ApplyTheme(options.Themes[0]);
        Window initial = QualityWindow.Open(QualityGallery.Create(options), options);
        startup.Stop(); QualityWindow.Close(initial);
        for (int index = 0; index < options.Themes.Length; index++) { service.ApplyTheme(options.Themes[index]); }
        _ = OpenAndClose(application, service, options);
        application.Dispatcher.Invoke(static () => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        long before = GC.GetTotalMemory(true);
        long allocatedBefore = GC.GetTotalAllocatedBytes(true);
        double[] times = new double[options.Iterations];
        for (int index = 0; index < times.Length; index++)
        {
            long start = Stopwatch.GetTimestamp();
            service.ApplyTheme(options.Themes[index % options.Themes.Length]);
            times[index] = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        }
        long allocated = (GC.GetTotalAllocatedBytes(true) - allocatedBefore) / options.Iterations;
        List<WeakReference> windows = new List<WeakReference>();
        for (int index = 0; index < options.WindowCycles; index++) { windows.AddRange(OpenAndClose(application, service, options)); }
        application.Dispatcher.Invoke(static () => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        long retained = Math.Max(0, GC.GetTotalMemory(true) - before);
        Array.Sort(times);
        double p95 = times[(int)Math.Ceiling(times.Length * 0.95) - 1];
        int alive = windows.Count(reference => reference.IsAlive);
        bool passed = startup.Elapsed.TotalMilliseconds <= options.MaximumFirstGalleryMs && p95 <= options.MaximumSwitchP95Ms && allocated <= options.MaximumAllocatedBytesPerSwitch &&
            retained <= options.MaximumRetainedBytes && alive == 0;
        object result = new { Runtime = RuntimeInformation.FrameworkDescription, Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            Iterations = options.Iterations, WindowCycles = options.WindowCycles, FirstGalleryMs = startup.Elapsed.TotalMilliseconds,
            SwitchMedianMs = times[times.Length / 2], SwitchP95Ms = p95, AllocatedBytesPerSwitch = allocated,
            RetainedManagedBytes = retained, PrivateBytes = Process.GetCurrentProcess().PrivateMemorySize64,
            WarmupEditorCycles = 1, LiveClosedWindowsOrEditors = alive, BudgetPassed = passed, RecordOnly = recordOnly };
        if (!passed && !recordOnly) { throw new InvalidOperationException("Performance budget exceeded: " + System.Text.Json.JsonSerializer.Serialize(result)); }
        return result;
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference[] OpenAndClose(Application application, ThemeService service, QualityOptions options)
    {
        using PaletteEditorViewModel editor = new PaletteEditorViewModel(service, application.Resources);
        Window window = QualityWindow.Open(new PaletteEditorView(editor), options);
        WeakReference reference = new WeakReference(window);
        QualityWindow.Close(window); return new[] { reference, new WeakReference(editor) };
    }
}
