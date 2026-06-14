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
using System.Text.Json;

namespace ThemeForge.Theme.Persistence;

/// <summary>
/// File-based <see cref="IThemePreferenceStore"/> backed by a JSON document.
/// Uses in-box <see cref="System.Text.Json"/> source generation, so the engine
/// keeps zero external dependencies.
/// </summary>
public sealed class JsonThemePreferenceStore : IThemePreferenceStore
{
    private const string DefaultPreferenceFileName = "preferences.json";
    private const string TempSuffix = ".tmp";

    private readonly string _filePath;
    private readonly Action<Exception>? _onError;

    /// <summary>Initializes a store backed by the file at <paramref name="filePath"/>.</summary>
    /// <param name="filePath">Fully resolved path of the preference file.</param>
    /// <param name="onError">
    /// Optional sink invoked when a present file cannot be read or parsed. The core
    /// imposes no logger; a host wires its own logging here. Defaults to none.
    /// </param>
    /// <exception cref="ArgumentException"><paramref name="filePath"/> is null or blank.</exception>
    public JsonThemePreferenceStore(string filePath, Action<Exception>? onError = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = filePath;
        _onError = onError;
    }

    /// <summary>The resolved path of the backing preference file.</summary>
    public string FilePath => _filePath;

    /// <summary>
    /// Composes a store under the per-user roaming application data folder:
    /// <c>%AppData%/{appName}/{fileName}</c>. The path is built from the supplied
    /// application name, never hardcoded.
    /// </summary>
    /// <param name="appName">The hosting application's folder name.</param>
    /// <param name="fileName">The preference file name. Defaults to a standard name.</param>
    /// <param name="onError">Optional read/parse error sink forwarded to the store.</param>
    /// <returns>A store rooted under the application data folder.</returns>
    /// <exception cref="ArgumentException"><paramref name="appName"/> or
    /// <paramref name="fileName"/> is null or blank.</exception>
    public static JsonThemePreferenceStore ForApplicationData(
        string appName,
        string fileName = DefaultPreferenceFileName,
        Action<Exception>? onError = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appName);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        string root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string path = Path.Combine(root, appName, fileName);
        return new JsonThemePreferenceStore(path, onError);
    }

    /// <inheritdoc/>
    public ThemePreference? Load()
    {
        if (!File.Exists(_filePath))
        {
            return null; // Normal first run: absence is not an error.
        }

        try
        {
            string json = File.ReadAllText(_filePath);
            ThemePreference? preference =
                JsonSerializer.Deserialize(json, ThemePreferenceJsonContext.Default.ThemePreference);
            if (preference is null || preference.Version != ThemePreference.SchemaVersion)
            {
                return null; // Empty payload or a schema this build cannot interpret.
            }

            return preference;
        }
        catch (Exception ex)
        {
            // Deliberate graceful degradation. A corrupt or unreadable preference
            // file must never crash startup, so the project's "log + rethrow" rule
            // is consciously replaced here by reporting through the optional onError
            // hook and returning null; the caller then falls back to its defaults.
            _onError?.Invoke(ex);
            return null;
        }
    }

    /// <inheritdoc/>
    public void Save(ThemePreference preference)
    {
        ArgumentNullException.ThrowIfNull(preference);

        string? directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(
            preference, ThemePreferenceJsonContext.Default.ThemePreference);
        string tempPath = _filePath + TempSuffix;

        // Write to a sibling temp file then replace, so a crash mid-write can never
        // leave a partially written (corrupt) preference file behind.
        try
        {
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, _filePath, overwrite: true);
        }
        catch
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            throw;
        }
    }
}
