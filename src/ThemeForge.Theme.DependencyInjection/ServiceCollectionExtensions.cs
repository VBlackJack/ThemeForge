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
using ThemeForge.Theme.Persistence;

namespace ThemeForge.Theme.DependencyInjection;

/// <summary>
/// Dependency injection helpers that register the ThemeForge theming engine in a
/// <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="ThemeService"/> as the backing singleton and exposes
    /// it through the <see cref="IThemeService"/>, <see cref="ISystemThemeFollower"/>,
    /// <see cref="ISystemAccentFollower"/> and <see cref="IWindowsThemeFollower"/>
    /// interfaces, all resolving to the same instance. Consumers never cast to the
    /// concrete type to reach the follow capabilities, and the concrete type is not
    /// registered as a resolvable service.
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
        services.TryAddSingleton<IWindowsThemeFollower>(
            static sp => (IWindowsThemeFollower)sp.GetRequiredService<IThemeService>());

        return services;
    }

    /// <summary>
    /// Registers the theming engine and the bootstrap orchestrator from a
    /// <see cref="ThemeForgeOptions"/> configuration. The four theming interfaces
    /// resolve to one shared <see cref="ThemeService"/> singleton, as with the
    /// list-based overload. Persistence is opt-in: a preference store is registered
    /// only when <see cref="ThemeForgeOptions.PreferenceStore"/> or
    /// <see cref="ThemeForgeOptions.ApplicationName"/> is supplied. Call
    /// <see cref="ServiceProviderExtensions.UseThemeForge"/> from <c>OnStartup</c> to
    /// run the bootstrap.
    /// </summary>
    /// <param name="services">The service collection to add the registrations to.</param>
    /// <param name="application">The hosting WPF application.</param>
    /// <param name="configure">Callback that populates the options.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Any argument is null.</exception>
    /// <exception cref="ArgumentException">
    /// The options provide neither a usable <see cref="ThemeForgeOptions.DefaultTheme"/>
    /// nor a Follow Windows default, or the default theme is not in the available set.
    /// </exception>
    public static IServiceCollection AddThemeForge(
        this IServiceCollection services,
        Application application,
        Action<ThemeForgeOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(application);
        ArgumentNullException.ThrowIfNull(configure);

        ThemeForgeOptions options = new ThemeForgeOptions();
        configure(options);
        ValidateOptions(options);

        services.TryAddSingleton(options);
        services.TryAddSingleton<IThemeService>(_ => new ThemeService(application, options.AvailableThemes));
        services.TryAddSingleton<ISystemThemeFollower>(
            static sp => (ISystemThemeFollower)sp.GetRequiredService<IThemeService>());
        services.TryAddSingleton<ISystemAccentFollower>(
            static sp => (ISystemAccentFollower)sp.GetRequiredService<IThemeService>());
        services.TryAddSingleton<IWindowsThemeFollower>(
            static sp => (IWindowsThemeFollower)sp.GetRequiredService<IThemeService>());

        IThemePreferenceStore? store = ResolveStore(options);
        if (store is not null)
        {
            services.TryAddSingleton(store);
        }

        services.TryAddSingleton(static sp => new ThemeForgeStartup(
            sp.GetRequiredService<IThemeService>(),
            sp.GetRequiredService<IWindowsThemeFollower>(),
            sp.GetRequiredService<ISystemThemeFollower>(),
            sp.GetRequiredService<ThemeForgeOptions>(),
            sp.GetService<IThemePreferenceStore>()));

        return services;
    }

    private static void ValidateOptions(ThemeForgeOptions options)
    {
        bool hasFollowDefault = options.FollowWindowsByDefault && options.WindowsFollow is not null;
        bool hasThemeDefault = !string.IsNullOrWhiteSpace(options.DefaultTheme);
        if (!hasThemeDefault && !hasFollowDefault)
        {
            throw new ArgumentException(
                "ThemeForgeOptions requires a DefaultTheme, or FollowWindowsByDefault with a WindowsFollow mapping.",
                nameof(options));
        }

        if (hasThemeDefault)
        {
            IReadOnlyList<string> available = options.AvailableThemes ?? ThemeNames.All;
            if (!available.Contains(options.DefaultTheme!, StringComparer.Ordinal))
            {
                throw new ArgumentException(
                    $"DefaultTheme '{options.DefaultTheme}' is not in AvailableThemes.", nameof(options));
            }
        }
    }

    private static IThemePreferenceStore? ResolveStore(ThemeForgeOptions options)
    {
        if (options.PreferenceStore is not null)
        {
            return options.PreferenceStore;
        }

        if (!string.IsNullOrWhiteSpace(options.ApplicationName))
        {
            return JsonThemePreferenceStore.ForApplicationData(options.ApplicationName, onError: options.OnError);
        }

        return null;
    }
}
