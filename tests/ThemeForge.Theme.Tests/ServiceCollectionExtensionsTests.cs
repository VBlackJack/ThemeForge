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
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ThemeForge.Theme.DependencyInjection;
using Xunit;

namespace ThemeForge.Theme.Tests;

/// <summary>
/// Verifies that <see cref="ServiceCollectionExtensions.AddThemeForge"/> exposes
/// the theming capabilities through a single shared singleton without leaking the
/// concrete <see cref="ThemeService"/> type.
/// </summary>
public sealed class ServiceCollectionExtensionsTests
{
    [StaFact]
    public void AddThemeForge_ResolvesAllInterfacesToTheSameInstance()
    {
        ServiceCollection services = new ServiceCollection();
        services.AddThemeForge(TestApplication.Instance);

        using ServiceProvider provider = services.BuildServiceProvider();
        IThemeService themeService = provider.GetRequiredService<IThemeService>();
        ISystemThemeFollower themeFollower = provider.GetRequiredService<ISystemThemeFollower>();
        ISystemAccentFollower accentFollower = provider.GetRequiredService<ISystemAccentFollower>();
        IWindowsThemeFollower windowsFollower = provider.GetRequiredService<IWindowsThemeFollower>();

        ReferenceEquals(themeService, themeFollower).Should().BeTrue();
        ReferenceEquals(themeService, accentFollower).Should().BeTrue();
        ReferenceEquals(themeService, windowsFollower).Should().BeTrue();
    }

    [StaFact]
    public void AddThemeForge_RegistersThemeServiceAsSingleton()
    {
        ServiceCollection services = new ServiceCollection();
        services.AddThemeForge(TestApplication.Instance);

        using ServiceProvider provider = services.BuildServiceProvider();
        IThemeService first = provider.GetRequiredService<IThemeService>();
        IThemeService second = provider.GetRequiredService<IThemeService>();

        ReferenceEquals(first, second).Should().BeTrue();
    }

    [StaFact]
    public void AddThemeForge_DoesNotExposeConcreteType()
    {
        ServiceCollection services = new ServiceCollection();
        services.AddThemeForge(TestApplication.Instance);

        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetService<ThemeService>().Should().BeNull();
    }

    [StaFact]
    public void AddThemeForge_ForwardsAvailableThemes()
    {
        IReadOnlyList<string> themes = new[] { ThemeNames.Folio, ThemeNames.Drakul };
        ServiceCollection services = new ServiceCollection();
        services.AddThemeForge(TestApplication.Instance, themes);

        using ServiceProvider provider = services.BuildServiceProvider();
        IThemeService themeService = provider.GetRequiredService<IThemeService>();

        themeService.AvailableThemes.Should().Equal(themes);
    }

    [StaFact]
    public void AddThemeForge_WithNullServices_Throws()
    {
        Action act = () => ServiceCollectionExtensions.AddThemeForge(null!, TestApplication.Instance);

        act.Should().Throw<ArgumentNullException>();
    }

    [StaFact]
    public void AddThemeForge_WithNullApplication_Throws()
    {
        ServiceCollection services = new ServiceCollection();

        Action act = () => services.AddThemeForge(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
