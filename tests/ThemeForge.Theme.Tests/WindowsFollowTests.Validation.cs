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
    [StaFact]
    public void FollowWindows_WithNullOptions_Throws()
    {
        FakeSystemThemeProvider themeProvider = new FakeSystemThemeProvider(SystemThemeMode.Dark);
        FakeSystemAccentProvider accentProvider = new FakeSystemAccentProvider(Color.FromRgb(17, 34, 51));
        using ThemeService service = new ThemeService(
            TestApplication.Instance, themeProvider, accentProvider);

        Action act = () => service.FollowWindows(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [StaFact]
    public void FollowWindows_WithUnknownTheme_Throws()
    {
        FakeSystemThemeProvider themeProvider = new FakeSystemThemeProvider(SystemThemeMode.Dark);
        FakeSystemAccentProvider accentProvider = new FakeSystemAccentProvider(Color.FromRgb(17, 34, 51));
        using ThemeService service = new ThemeService(
            TestApplication.Instance, themeProvider, accentProvider);

        Action act = () => service.FollowWindows(new WindowsFollowOptions
        {
            LightTheme = "NotARealTheme",
            DarkTheme = ThemeNames.Drakul,
        });

        act.Should().Throw<ArgumentException>();
    }

}
