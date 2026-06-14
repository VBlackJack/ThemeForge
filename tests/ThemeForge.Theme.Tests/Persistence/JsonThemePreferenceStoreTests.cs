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
using ThemeForge.Theme.Persistence;
using Xunit;

namespace ThemeForge.Theme.Tests.Persistence;

public sealed class JsonThemePreferenceStoreTests : IDisposable
{
    private readonly string _directory;
    private readonly string _filePath;

    public JsonThemePreferenceStoreTests()
    {
        _directory = Path.Combine(Path.GetTempPath(), "ThemeForgeTests", Guid.NewGuid().ToString("N"));
        _filePath = Path.Combine(_directory, "preferences.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public void SaveThenLoad_RoundTripsAllFields()
    {
        JsonThemePreferenceStore store = new JsonThemePreferenceStore(_filePath);
        ThemePreference saved = new ThemePreference
        {
            ThemeName = ThemeNames.Carmilla,
            AccentTint = AccentTint.Purple,
            FollowWindows = true,
        };

        store.Save(saved);
        ThemePreference? loaded = store.Load();

        loaded.Should().NotBeNull();
        loaded!.ThemeName.Should().Be(ThemeNames.Carmilla);
        loaded.AccentTint.Should().Be(AccentTint.Purple);
        loaded.FollowWindows.Should().BeTrue();
        loaded.Version.Should().Be(ThemePreference.SchemaVersion);
    }

    [Fact]
    public void Save_SerializesAccentTintAsName()
    {
        JsonThemePreferenceStore store = new JsonThemePreferenceStore(_filePath);

        store.Save(new ThemePreference { AccentTint = AccentTint.Purple });

        string json = File.ReadAllText(_filePath);
        json.Should().Contain("\"Purple\"");
        json.Should().NotContain("\"AccentTint\": 6");
    }

    [Fact]
    public void Load_MissingFile_ReturnsNullWithoutInvokingOnError()
    {
        bool errorRaised = false;
        JsonThemePreferenceStore store = new JsonThemePreferenceStore(_filePath, _ => errorRaised = true);

        ThemePreference? loaded = store.Load();

        loaded.Should().BeNull();
        errorRaised.Should().BeFalse();
    }

    [Fact]
    public void Load_MalformedJson_ReturnsNullAndInvokesOnErrorOnce()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(_filePath, "{ this is not valid json ");
        int errorCount = 0;
        JsonThemePreferenceStore store = new JsonThemePreferenceStore(_filePath, _ => errorCount++);

        ThemePreference? loaded = store.Load();

        loaded.Should().BeNull();
        errorCount.Should().Be(1);
    }

    [Fact]
    public void Load_UnknownEnumValue_ReturnsNull()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(_filePath, "{ \"ThemeName\": \"Dracula\", \"AccentTint\": \"NotAColor\", \"Version\": 1 }");
        JsonThemePreferenceStore store = new JsonThemePreferenceStore(_filePath);

        ThemePreference? loaded = store.Load();

        loaded.Should().BeNull();
    }

    [Fact]
    public void Load_UnknownSchemaVersion_ReturnsNull()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(_filePath, "{ \"ThemeName\": \"Dracula\", \"AccentTint\": \"Default\", \"Version\": 999 }");
        JsonThemePreferenceStore store = new JsonThemePreferenceStore(_filePath);

        ThemePreference? loaded = store.Load();

        loaded.Should().BeNull();
    }

    [Fact]
    public void Save_CreatesParentDirectoryWhenAbsent()
    {
        Directory.Exists(_directory).Should().BeFalse();
        JsonThemePreferenceStore store = new JsonThemePreferenceStore(_filePath);

        store.Save(new ThemePreference { ThemeName = ThemeNames.Folio });

        File.Exists(_filePath).Should().BeTrue();
    }

    [Fact]
    public void Save_LeavesNoTempFileAndOverwritesAtomically()
    {
        JsonThemePreferenceStore store = new JsonThemePreferenceStore(_filePath);

        store.Save(new ThemePreference { ThemeName = ThemeNames.Folio });
        store.Save(new ThemePreference { ThemeName = ThemeNames.Drakul });

        Directory.GetFiles(_directory).Should().ContainSingle().Which.Should().Be(_filePath);
        store.Load()!.ThemeName.Should().Be(ThemeNames.Drakul);
    }

    [Fact]
    public void ForApplicationData_ComposesPathUnderApplicationData()
    {
        string root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        JsonThemePreferenceStore store = JsonThemePreferenceStore.ForApplicationData("ThemeForgeSampleApp");

        store.FilePath.Should().Be(Path.Combine(root, "ThemeForgeSampleApp", "preferences.json"));
    }
}
