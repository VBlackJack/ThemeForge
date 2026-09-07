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
using ThemeForge.Studio.Services;
using ThemeForge.Studio.ViewModels;
using ThemeForge.Theme;
using ThemeForge.Theme.Palettes;

namespace ThemeForge.Quality;

internal static class DocumentScenario
{
    internal static async Task<object> RunAsync(Application application, ThemeService service, QualityOptions options, string output)
    {
        string path = Path.Combine(output, "palette.json");
        FixtureDialogs dialogs = new FixtureDialogs(path);
        service.ApplyTheme(ThemeNames.Folio);
        using (PaletteEditorViewModel editor = new PaletteEditorViewModel(service, application.Resources, dialogs))
        {
            editor.PaletteName = options.EditedPaletteName;
            editor.CanonicalSlots.First(slot => slot.Name == "Background").Hex = options.EditedBackground;
            await editor.SaveCommand.ExecuteAsync(null);
            if (editor.IsDirty || !File.Exists(path)) { throw new InvalidOperationException("Studio save failed."); }
        }
        service.ApplyTheme(ThemeNames.Dracula);
        using (PaletteEditorViewModel reopened = new PaletteEditorViewModel(service, application.Resources, dialogs))
        {
            await reopened.OpenCommand.ExecuteAsync(null);
            ThemePalette palette = reopened.ExportDocument();
            if (palette.Name != options.EditedPaletteName || palette.Colors["Background"] != options.EditedBackground || reopened.IsDirty)
            { throw new InvalidOperationException("Studio reopen did not preserve the edited palette."); }
        }
        return new { Palette = path, Name = options.EditedPaletteName, Background = options.EditedBackground,
            SavedThroughStudioCommand = true, EditorDisposedThenRecreated = true, ReopenedThroughStudioCommand = true };
    }
    private sealed class FixtureDialogs(string path) : IPaletteFileDialogs
    {
        public string? Open() => path;
        public string? Save(string name) => path;
        public bool ConfirmDiscard() => true;
    }
}
