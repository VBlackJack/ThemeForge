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
using System.Windows.Media;
using FluentAssertions;
using Xunit;

namespace ThemeForge.Theme.Tests;

public sealed partial class WindowsFollowTests
{
    private static Color ReadBrushColor(string key) =>
        (TestApplication.Instance.Resources[key] as SolidColorBrush
            ?? throw new InvalidOperationException($"Resource '{key}' must resolve to a SolidColorBrush.")).Color;

    private static int CountMarked(string markerKey) =>
        TestApplication.Instance.Resources.MergedDictionaries.Count(dict => dict.Contains(markerKey));

    private static void ClearTaggedDictionaries()
    {
        Application? app = Application.Current;
        if (app is null)
        {
            return;
        }

        IList<ResourceDictionary> merged = app.Resources.MergedDictionaries;
        for (int i = merged.Count - 1; i >= 0; i--)
        {
            if (merged[i].Contains(ThemeMarkerKey) ||
                merged[i].Contains(AccentTintMarkerKey) ||
                merged[i].Contains(SystemAccentMarkerKey))
            {
                merged.RemoveAt(i);
            }
        }
    }

    private sealed class FakeSystemThemeProvider : ISystemThemeProvider, IDisposable
    {
        private EventHandler? _changed;

        public FakeSystemThemeProvider(SystemThemeMode mode)
        {
            Mode = mode;
        }

        public SystemThemeMode Mode { get; private set; }
        public bool IsDisposed { get; private set; }
        public int SubscriberCount => _changed?.GetInvocationList().Length ?? 0;

        public event EventHandler? Changed
        {
            add => _changed += value;
            remove => _changed -= value;
        }

        public SystemThemeMode GetCurrentMode() => Mode;

        public void Raise(SystemThemeMode mode)
        {
            Mode = mode;
            _changed?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose() => IsDisposed = true;
    }

    private sealed class FakeSystemAccentProvider : ISystemAccentProvider, IDisposable
    {
        private EventHandler? _changed;

        public FakeSystemAccentProvider(Color? accent)
        {
            Accent = accent;
        }

        public Color? Accent { get; private set; }
        public bool IsDisposed { get; private set; }
        public int SubscriberCount => _changed?.GetInvocationList().Length ?? 0;

        public event EventHandler? Changed
        {
            add => _changed += value;
            remove => _changed -= value;
        }

        public Color? GetCurrentAccent() => Accent;

        public void Raise(Color? accent)
        {
            Accent = accent;
            _changed?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose() => IsDisposed = true;
    }
}
