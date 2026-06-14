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

namespace ThemeForge.Theme.Persistence;

/// <summary>
/// Persists and restores the end user's <see cref="ThemePreference"/>. Opt-in:
/// the theming engine never reads or writes a store on its own.
/// </summary>
/// <remarks>
/// Implement this interface to persist elsewhere (registry, settings service,
/// cloud) with no file dependency. The default file-based implementation is
/// <see cref="JsonThemePreferenceStore"/>.
/// </remarks>
public interface IThemePreferenceStore
{
    /// <summary>
    /// Reads the stored preference. Returns <see langword="null"/> when there is
    /// no usable preference, that is when the backing store is absent, malformed,
    /// or carries an unknown schema version. Never throws on read.
    /// </summary>
    /// <returns>The stored preference, or <see langword="null"/>.</returns>
    ThemePreference? Load();

    /// <summary>Writes the supplied preference to the backing store.</summary>
    /// <param name="preference">The preference to persist.</param>
    void Save(ThemePreference preference);
}
