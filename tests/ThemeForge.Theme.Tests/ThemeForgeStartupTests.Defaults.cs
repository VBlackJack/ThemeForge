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
using FluentAssertions;
using ThemeForge.Theme.DependencyInjection;
using ThemeForge.Theme.Persistence;
using Xunit;

namespace ThemeForge.Theme.Tests;

public sealed partial class ThemeForgeStartupTests
{
    [Fact]
    public void Run_NoStore_AppliesDefaultThemeAndAccent()
    {
        FakeThemeService service = new FakeThemeService();
        ThemeForgeOptions options = new ThemeForgeOptions
        {
            DefaultTheme = ThemeNames.Carmilla,
            DefaultAccentTint = AccentTint.Cyan,
        };
        ThemeForgeStartup startup = Create(service, options, store: null);

        startup.Run();

        service.CurrentTheme.Should().Be(ThemeNames.Carmilla);
        service.CurrentAccentTint.Should().Be(AccentTint.Cyan);
    }

}
