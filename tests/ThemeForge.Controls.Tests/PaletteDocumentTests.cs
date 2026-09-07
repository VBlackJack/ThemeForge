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
using ThemeForge.Studio.Services;
using ThemeForge.Studio.ViewModels;
using ThemeForge.Theme;
using ThemeForge.Theme.Palettes;

namespace ThemeForge.Controls.Tests;

[Collection("Controls application resources")]
public sealed class PaletteDocumentTests
{
    [StaFact]
    public void EditUndoRedoResetAndComparison_PreserveDocumentState()
    {
        Application app = TestApplication.Instance;
        using ThemeService service = new ThemeService(app); service.ApplyTheme(ThemeNames.Folio);
        using PaletteEditorViewModel editor = new PaletteEditorViewModel(service, app.Resources);
        string original = editor.ExportDocument().Colors["Background"];
        editor.CanonicalSlots.First(slot => slot.Name == "Background").Hex = "#010203";
        editor.IsDirty.Should().BeTrue();
        editor.ShowOriginal = true;
        PaletteValidation.ToHex(((SolidColorBrush)app.Resources["BackgroundBrush"]).Color).Should().Be(original);
        editor.ExportDocument().Colors["Background"].Should().Be("#010203");
        editor.ShowOriginal = false;
        editor.UndoCommand.Execute(null); editor.IsDirty.Should().BeFalse();
        editor.RedoCommand.Execute(null); editor.IsDirty.Should().BeTrue();
        editor.ResetAllCommand.Execute(null); editor.IsDirty.Should().BeFalse();
        editor.Contrasts.Should().HaveCount(4);
    }
    [StaFact]
    public void InvalidInputAndDiscardCancellation_ProtectDraft()
    {
        Application app = TestApplication.Instance;
        using ThemeService service = new ThemeService(app); service.ApplyTheme(ThemeNames.Folio);
        using PaletteEditorViewModel editor = new PaletteEditorViewModel(service, app.Resources, new DecliningDialogs());
        editor.CanonicalSlots.First().Hex = "#broken";
        editor.IsDirty.Should().BeTrue(); editor.SaveCommand.CanExecute(null).Should().BeFalse();
        editor.ConfirmClose().Should().BeFalse(); editor.PrepareThemeChange().Should().BeFalse();
        editor.CanonicalSlots.First().Hex = "#010203";
        service.ApplyTheme(ThemeNames.Dracula);
        editor.ExportDocument().Colors["Background"].Should().Be("#010203");
        PaletteValidation.ToHex(((SolidColorBrush)app.Resources["BackgroundBrush"]).Color).Should().Be("#010203");
    }
    [StaFact]
    public void LoadInvalidPalette_DoesNotReplaceCurrentDocument()
    {
        Application app = TestApplication.Instance;
        using ThemeService service = new ThemeService(app); service.ApplyTheme(ThemeNames.Folio);
        using PaletteEditorViewModel editor = new PaletteEditorViewModel(service, app.Resources);
        ThemePalette original = editor.ExportDocument();
        Action invalid = () => editor.LoadDocument(original with { Version = 99 }, null);
        invalid.Should().Throw<ArgumentException>(); editor.ExportDocument().Should().BeEquivalentTo(original);
    }
    private sealed class DecliningDialogs : IPaletteFileDialogs
    {
        public string? Open() => null;
        public string? Save(string name) => null;
        public bool ConfirmDiscard() => false;
    }
}
