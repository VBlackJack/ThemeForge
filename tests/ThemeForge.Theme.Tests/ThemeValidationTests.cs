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
using System.Windows;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ThemeForge.Theme.DependencyInjection;
using ThemeForge.Theme.Persistence;

namespace ThemeForge.Theme.Tests;

[Collection("ThemeService resource dictionary tests")]
public sealed class ThemeValidationTests
{
    [StaFact]
    public void InvalidTint_LeavesThemeResourcesRevisionAndFollowUntouched()
    {
        Application app = TestApplication.Instance;
        using ThemeService service = new ThemeService(app, new FixedThemeProvider());
        service.ApplyTheme(ThemeNames.Dracula);
        service.ApplyAccentTint(AccentTint.Blue);
        service.EnableSystemFollow(ThemeNames.Folio, ThemeNames.Dracula);
        object accent = app.Resources["AccentBrush"];
        int revision = service.ThemeRevision;

        Action apply = () => service.ApplyAccentTint((AccentTint)999);

        apply.Should().Throw<ArgumentOutOfRangeException>();
        service.CurrentAccentTint.Should().Be(AccentTint.Blue);
        service.ThemeRevision.Should().Be(revision);
        service.IsFollowingSystem.Should().BeTrue();
        app.Resources["AccentBrush"].Should().BeSameAs(accent);
        Action switchTheme = () => service.ApplyTheme(ThemeNames.Folio);
        switchTheme.Should().NotThrow();
    }

    [StaFact]
    public void InvalidPersistedNumericTint_FallsBackToDefault()
    {
        string path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, "{\"Version\":1,\"ThemeName\":\"Dracula\",\"AccentTint\":999}");
            JsonThemePreferenceStore store = new JsonThemePreferenceStore(path);
            store.Load().Should().BeNull();
            ServiceCollection services = new ServiceCollection();
            services.AddThemeForge(TestApplication.Instance, options =>
            {
                options.DefaultTheme = ThemeNames.Folio;
                options.PreferenceStore = store;
            });
            using ServiceProvider provider = services.BuildServiceProvider();
            using IDisposable startup = provider.UseThemeForge();
            provider.GetRequiredService<IThemeService>().CurrentTheme.Should().Be(ThemeNames.Folio);
        }
        finally { File.Delete(path); }
    }

    [StaFact]
    public void InvalidDefaultTint_IsRejectedBeforeRegistration()
    {
        ServiceCollection services = new ServiceCollection();
        Action register = () => services.AddThemeForge(TestApplication.Instance, options =>
        {
            options.DefaultTheme = ThemeNames.Dracula;
            options.DefaultAccentTint = (AccentTint)999;
        });
        register.Should().Throw<ArgumentOutOfRangeException>();
        services.Should().BeEmpty();
    }

    [StaFact]
    public void MissingTintBrush_PreservesExistingOverride()
    {
        Application app = TestApplication.Instance;
        using ThemeService service = new ThemeService(app);
        service.ApplyTheme(ThemeNames.Dracula);
        service.ApplyAccentTint(AccentTint.Blue);
        object previous = app.Resources["AccentBrush"];
        app.Resources["CyanBrush"] = "invalid host override";
        try
        {
            Action apply = () => service.ApplyAccentTint(AccentTint.Cyan);
            apply.Should().Throw<InvalidOperationException>();
            service.CurrentAccentTint.Should().Be(AccentTint.Blue);
            app.Resources["AccentBrush"].Should().BeSameAs(previous);
        }
        finally { app.Resources.Remove("CyanBrush"); }
    }

    private sealed class FixedThemeProvider : ISystemThemeProvider
    {
        public SystemThemeMode GetCurrentMode() => SystemThemeMode.Dark;
        public event EventHandler? Changed { add { } remove { } }
    }
}
