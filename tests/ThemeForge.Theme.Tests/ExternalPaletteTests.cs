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
using Microsoft.Extensions.DependencyInjection;
using ThemeForge.Theme.DependencyInjection;
using ThemeForge.Theme.Palettes;

namespace ThemeForge.Theme.Tests;

[Collection("ThemeService resource dictionary tests")]
public sealed class ExternalPaletteTests
{
    [StaFact]
    public void RegistrationSwitchAndTint_UseExternalResourcesWithoutRecompilation()
    {
        Application app = TestApplication.Instance;
        using ThemeService service = new ThemeService(app);
        ThemePalette palette = PaletteDataTests.Example();
        service.RegisterPalette(palette);
        service.ApplyTheme(palette.Name);
        service.AvailableThemes.Should().Contain(palette.Name);
        ((SolidColorBrush)app.Resources["BackgroundBrush"]).Color.Should().Be(Color.FromRgb(18, 52, 86));
        app.Resources["BackgroundColor"].Should().Be(Color.FromRgb(18, 52, 86));
        app.Resources["SpacingMd"].Should().BeOfType<Thickness>();
        app.Resources["ThemeAttribution"].Should().Be(palette.Attribution);
        service.ApplyAccentTint(AccentTint.Cyan);
        ((SolidColorBrush)app.Resources["AccentBrush"]).Color.Should().Be(Color.FromRgb(18, 52, 86));
        service.ApplyTheme(ThemeNames.Dracula);
        service.ApplyTheme(palette.Name);
        service.CurrentTheme.Should().Be(palette.Name);
    }
    [StaFact]
    public void BuiltInAndActiveReplacement_AreRejectedWithoutMutation()
    {
        using ThemeService service = new ThemeService(TestApplication.Instance);
        ThemePalette palette = PaletteDataTests.Example();
        Action builtIn = () => service.RegisterPalette(palette with { Name = ThemeNames.Dracula });
        builtIn.Should().Throw<ArgumentException>();
        service.RegisterPalette(palette); service.ApplyTheme(palette.Name);
        int revision = service.ThemeRevision;
        Action active = () => service.RegisterPalette(palette);
        active.Should().Throw<ArgumentException>();
        service.ThemeRevision.Should().Be(revision);
    }
    [StaFact]
    public void Bootstrap_AcceptsExternalDefaultAndRejectsDuplicateNamesBeforeRegistration()
    {
        ThemePalette palette = PaletteDataTests.Example();
        ServiceCollection services = new ServiceCollection();
        services.AddThemeForge(TestApplication.Instance, options => { options.Palettes = new[] { palette }; options.DefaultTheme = palette.Name; });
        using ServiceProvider provider = services.BuildServiceProvider();
        using IDisposable startup = provider.UseThemeForge();
        provider.GetRequiredService<IThemeService>().CurrentTheme.Should().Be(palette.Name);
        ServiceCollection invalid = new ServiceCollection();
        Action duplicate = () => invalid.AddThemeForge(TestApplication.Instance, options =>
        { options.Palettes = new[] { palette, palette }; options.DefaultTheme = palette.Name; });
        duplicate.Should().Throw<ArgumentException>(); invalid.Should().BeEmpty();
    }
}
