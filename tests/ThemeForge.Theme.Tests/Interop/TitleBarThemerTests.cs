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
using FluentAssertions;
using ThemeForge.Theme.Interop;
using Xunit;

namespace ThemeForge.Theme.Tests.Interop;

/// <summary>
/// Window-bound behavior of the title bar themer. Uses a real off-screen window for
/// the HWND and lifetime, with the native call and theme service faked.
/// </summary>
public sealed class TitleBarThemerTests
{
    private static readonly Version Win11 = new Version(10, 0, 22631);

    [StaFact]
    public void Apply_WithReadyHandle_PushesDerivedAttributes()
    {
        RunWindowTest(window =>
        {
            new WindowInteropHelper(window).EnsureHandle();
            RecordingChrome chrome = new RecordingChrome();
            using TitleBarThemer themer = Create(window, new FakeThemeService(), chrome);

            themer.Apply();

            chrome.Calls.Should().Contain(c => c.Attribute == TitleBarPlanner.DwmwaUseImmersiveDarkMode);
            chrome.Calls.Should().Contain(c => c.Attribute == TitleBarPlanner.DwmwaCaptionColor);
        });
    }

    [StaFact]
    public void Apply_FailingHResult_RoutesToOnErrorWithoutThrowing()
    {
        RunWindowTest(window =>
        {
            new WindowInteropHelper(window).EnsureHandle();
            RecordingChrome chrome = new RecordingChrome { ReturnValue = false };
            int errorCount = 0;
            TitleBarOptions options = new TitleBarOptions { OnError = _ => errorCount++ };
            using TitleBarThemer themer = Create(window, new FakeThemeService(), chrome, options);

            Action act = themer.Apply;

            act.Should().NotThrow();
            errorCount.Should().Be(chrome.Calls.Count);
            errorCount.Should().BeGreaterThan(0);
        });
    }

    [StaFact]
    public void ThemeChanged_ReAppliesAttributes()
    {
        RunWindowTest(window =>
        {
            new WindowInteropHelper(window).EnsureHandle();
            RecordingChrome chrome = new RecordingChrome();
            FakeThemeService service = new FakeThemeService();
            using TitleBarThemer themer = Create(window, service, chrome);
            themer.Apply();
            chrome.Calls.Clear();

            service.RaiseThemeChanged();

            chrome.Calls.Should().NotBeEmpty();
        });
    }

    [StaFact]
    public void Dispose_Unsubscribes_NoReapply()
    {
        RunWindowTest(window =>
        {
            new WindowInteropHelper(window).EnsureHandle();
            RecordingChrome chrome = new RecordingChrome();
            FakeThemeService service = new FakeThemeService();
            TitleBarThemer themer = Create(window, service, chrome);
            themer.Apply();
            chrome.Calls.Clear();

            themer.Dispose();
            themer.Dispose();
            service.RaiseThemeChanged();

            chrome.Calls.Should().BeEmpty();
        });
    }

    [StaFact]
    public void Apply_BeforeHandleExists_AppliesOnSourceInitialized()
    {
        RunWindowTest(window =>
        {
            RecordingChrome chrome = new RecordingChrome();
            using TitleBarThemer themer = Create(window, new FakeThemeService(), chrome);

            themer.Apply();
            chrome.Calls.Should().BeEmpty();

            new WindowInteropHelper(window).EnsureHandle();

            chrome.Calls.Should().NotBeEmpty();
        });
    }

    private static TitleBarThemer Create(
        Window window, FakeThemeService service, INativeWindowChrome chrome, TitleBarOptions? options = null) =>
        new TitleBarThemer(window, service, options ?? new TitleBarOptions(), chrome, Win11);

    private static void RunWindowTest(Action<Window> body)
    {
        _ = TestApplication.Instance;
        Window window = new Window
        {
            Width = 200,
            Height = 100,
            Left = -10000,
            Top = -10000,
            ShowInTaskbar = false,
            ShowActivated = false,
            WindowStyle = WindowStyle.None,
        };
        window.Resources["BackgroundColor"] = Color.FromRgb(0x28, 0x2A, 0x36);
        window.Resources["ForegroundColor"] = Color.FromRgb(0xF8, 0xF8, 0xF2);

        try
        {
            body(window);
        }
        finally
        {
            window.Close();
        }
    }

    private sealed class RecordingChrome : INativeWindowChrome
    {
        public List<(int Attribute, int Value)> Calls { get; } = new();
        public bool ReturnValue { get; set; } = true;

        public bool TrySetAttribute(IntPtr handle, int attribute, int value)
        {
            Calls.Add((attribute, value));
            return ReturnValue;
        }
    }

    private sealed class FakeThemeService : IThemeService
    {
        public string CurrentTheme => ThemeNames.Dracula;
        public int ThemeRevision => 1;
        public IReadOnlyList<string> AvailableThemes => ThemeNames.All;
        public IReadOnlyList<AccentTint> AvailableAccentTints => AccentTints.All;
        public AccentTint CurrentAccentTint => AccentTint.Default;

        public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

        public void ApplyTheme(string name)
        {
        }

        public void ApplyAccentTint(AccentTint tint)
        {
        }

        public void RaiseThemeChanged() =>
            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(ThemeNames.Dracula, ThemeNames.Dracula, 2));
    }
}
