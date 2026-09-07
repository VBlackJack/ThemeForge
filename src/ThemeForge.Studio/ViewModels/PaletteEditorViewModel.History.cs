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

using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using ThemeForge.Studio.Resources;
using ThemeForge.Theme.Palettes;

namespace ThemeForge.Studio.ViewModels;

public sealed partial class PaletteEditorViewModel
{
    private const int MaximumHistory = 100;
    private readonly List<ThemePalette> _undo = new List<ThemePalette>();
    private readonly List<ThemePalette> _redo = new List<ThemePalette>();

    private void ApplyOverride(string key, Color? color)
    {
        if (_updating || color is not Color value) { return; }
        string slot = key[..^"Brush".Length];
        string hex = PaletteValidation.ToHex(value);
        if (_current.Colors[slot] == hex) { return; }
        RecordHistory();
        Dictionary<string, string> colors = new Dictionary<string, string>(_current.Colors, StringComparer.Ordinal) { [slot] = hex };
        _current = _current with { Colors = colors };
        ApplyPreview(); RefreshState();
    }
    partial void OnPaletteNameChanged(string value)
    {
        if (_updating) { return; }
        RecordHistory();
        _current = _current with { Name = value, DisplayName = value };
        RefreshState();
    }
    private void RecordHistory()
    {
        _undo.Add(_current);
        if (_undo.Count > MaximumHistory) { _undo.RemoveAt(0); }
        _redo.Clear();
    }
    private bool CanUndo() => !IsBusy && _undo.Count > 0;
    private bool CanRedo() => !IsBusy && _redo.Count > 0;
    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        _redo.Add(_current); _current = _undo[^1]; _undo.RemoveAt(_undo.Count - 1); PopulateRows();
    }
    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo()
    {
        _undo.Add(_current); _current = _redo[^1]; _redo.RemoveAt(_redo.Count - 1); PopulateRows();
    }
    [RelayCommand(CanExecute = nameof(CanEdit))]
    private void ResetAll()
    {
        RecordHistory(); _current = _original; PopulateRows();
    }
    [RelayCommand(CanExecute = nameof(CanEdit))]
    private void NewPalette()
    {
        if (ConfirmClose()) { ReloadFromTheme(); }
    }
    /// <summary>Loads a validated document; the caller owns any discard confirmation.</summary>
    public void LoadDocument(ThemePalette palette, string? path)
    {
        ThemePalette validated = PaletteValidation.Normalize(palette);
        _current = validated; _original = validated; _saved = validated;
        _undo.Clear(); _redo.Clear(); _filePath = path;
        ShowOriginal = false;
        PopulateRows();
    }
    /// <summary>Returns an immutable, validated snapshot suitable for any ThemeForge consumer.</summary>
    public ThemePalette ExportDocument() => PaletteValidation.Normalize(_current);

    private void RefreshState()
    {
        if (_updating) { return; }
        IsDirty = !Equivalent(_current, _saved) || AllSlots.Any(slot => !slot.IsValid);
        OnPropertyChanged(nameof(DocumentState));
        if (!IsBusy && !CanSave()) { Status = EditorText.InvalidDocument; }
        else if (Status == EditorText.InvalidDocument) { Status = string.Empty; }
        UndoCommand.NotifyCanExecuteChanged(); RedoCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged(); ExportCommand.NotifyCanExecuteChanged();
        OpenCommand.NotifyCanExecuteChanged(); NewPaletteCommand.NotifyCanExecuteChanged(); ResetAllCommand.NotifyCanExecuteChanged();
    }
    private static bool Equivalent(ThemePalette left, ThemePalette right)
        => left.Name == right.Name && left.DisplayName == right.DisplayName && left.Attribution == right.Attribution &&
           left.Family == right.Family && left.Colors.Count == right.Colors.Count &&
           left.Colors.All(pair => right.Colors.TryGetValue(pair.Key, out string? value) && value == pair.Value);
}
