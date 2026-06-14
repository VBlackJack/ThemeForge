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

using Microsoft.Extensions.DependencyInjection;

namespace ThemeForge.Theme.DependencyInjection;

/// <summary>
/// Bootstrap entry point resolved from a built <see cref="IServiceProvider"/>.
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Runs the ThemeForge bootstrap: restores the persisted preference or applies
    /// the configured default, arms Follow Windows when intended, and wires
    /// auto-save. Call once from <c>OnStartup</c> after <c>base.OnStartup</c>, so the
    /// application resources are loaded. Idempotent: a second call is a no-op.
    /// </summary>
    /// <param name="provider">The built service provider.</param>
    /// <returns>
    /// A handle that unsubscribes the auto-save wiring when disposed. The container
    /// also disposes it on shutdown; disposal is idempotent.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="provider"/> is null.</exception>
    public static IDisposable UseThemeForge(this IServiceProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        ThemeForgeStartup startup = provider.GetRequiredService<ThemeForgeStartup>();
        startup.Run();
        return startup;
    }
}
