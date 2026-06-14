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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ThemeForge.Theme.DependencyInjection;

/// <summary>
/// Dependency injection helpers that register the ThemeForge theming engine in a
/// <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="ThemeService"/> as the backing singleton and exposes
    /// it through the <see cref="IThemeService"/>, <see cref="ISystemThemeFollower"/>
    /// and <see cref="ISystemAccentFollower"/> interfaces, all resolving to the same
    /// instance. Consumers never cast to the concrete type to reach the follow
    /// capabilities, and the concrete type is not registered as a resolvable service.
    /// </summary>
    /// <remarks>
    /// Registrations use <c>TryAddSingleton</c> so a consumer's own prior
    /// registration of any of these services wins and is left untouched. The
    /// <paramref name="application"/> is captured in the singleton factory: no
    /// <see cref="Application.Current"/> global lookup is performed.
    /// </remarks>
    /// <param name="services">The service collection to add the registrations to.</param>
    /// <param name="application">The hosting WPF application whose resources the
    /// theme service swaps at runtime.</param>
    /// <param name="availableThemes">The optional list of theme names the service can
    /// apply. When null, the canonical ThemeForge theme set is used.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/> or <paramref name="application"/> is null.
    /// </exception>
    public static IServiceCollection AddThemeForge(
        this IServiceCollection services,
        Application application,
        IReadOnlyList<string>? availableThemes = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(application);

        services.TryAddSingleton<IThemeService>(_ => new ThemeService(application, availableThemes));
        services.TryAddSingleton<ISystemThemeFollower>(
            static sp => (ISystemThemeFollower)sp.GetRequiredService<IThemeService>());
        services.TryAddSingleton<ISystemAccentFollower>(
            static sp => (ISystemAccentFollower)sp.GetRequiredService<IThemeService>());

        return services;
    }
}
