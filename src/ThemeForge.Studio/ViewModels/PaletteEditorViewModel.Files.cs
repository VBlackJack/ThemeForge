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

using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ThemeForge.Studio.Resources;
using ThemeForge.Theme.Palettes;

namespace ThemeForge.Studio.ViewModels;

public sealed partial class PaletteEditorViewModel
{
    /// <summary>Protects an unsaved document when the owning window closes.</summary>
    public bool ConfirmClose() => !IsBusy && (!IsDirty || _dialogs?.ConfirmDiscard() == true);

    /// <summary>Called by the shell before a deliberate theme/accents change.</summary>
    public bool PrepareThemeChange()
    {
        if (!ConfirmClose()) { return false; }
        _discardOnThemeChange = true;
        _resources.MergedDictionaries.Remove(_overrides);
        return true;
    }
    private bool CanSave()
    {
        if (IsBusy || AllSlots.Any(slot => !slot.IsValid)) { return false; }
        try { PaletteValidation.Normalize(_current); return true; }
        catch (ArgumentException) { return false; }
    }
    [RelayCommand(CanExecute = nameof(CanEdit))]
    private async Task OpenAsync(CancellationToken cancellationToken)
    {
        if (!ConfirmClose()) { return; }
        string? path = _dialogs?.Open();
        if (path is null) { return; }
        IsBusy = true;
        try
        {
            ThemePalette palette = await PaletteJson.LoadAsync(path, cancellationToken);
            LoadDocument(palette, path); Status = EditorText.Opened;
            _logger?.LogInformation("Opened palette {PaletteName}", palette.Name);
        }
        catch (OperationCanceledException) { Status = EditorText.Clean; }
        catch (Exception exception)
        { _logger?.LogError(exception, "Unable to open palette"); Status = EditorText.Failed; }
        finally { IsBusy = false; }
    }
    [RelayCommand(CanExecute = nameof(CanSave))]
    private Task SaveAsync(CancellationToken cancellationToken) => SaveToAsync(_filePath ?? _dialogs?.Save(PaletteName), cancellationToken);
    [RelayCommand(CanExecute = nameof(CanSave))]
    private Task ExportAsync(CancellationToken cancellationToken) => SaveToAsync(_dialogs?.Save(PaletteName), cancellationToken);

    private async Task SaveToAsync(string? path, CancellationToken cancellationToken)
    {
        if (path is null) { return; }
        IsBusy = true;
        try
        {
            ThemePalette snapshot = ExportDocument();
            await PaletteJson.SaveAsync(path, snapshot, cancellationToken);
            _filePath = path; _saved = snapshot; Status = EditorText.Saved;
            _logger?.LogInformation("Saved palette {PaletteName} with {ColorCount} colours", snapshot.Name, snapshot.Colors.Count);
        }
        catch (OperationCanceledException) { Status = EditorText.Modified; }
        catch (Exception exception)
        { _logger?.LogError(exception, "Unable to save palette"); Status = EditorText.Failed; }
        finally { IsBusy = false; }
    }
}
