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
using Microsoft.Win32;
using ThemeForge.Studio.Resources;

namespace ThemeForge.Studio.Services;

/// <summary>Native Windows palette dialogs, localized independently of the view model.</summary>
public sealed class PaletteFileDialogs : IPaletteFileDialogs
{
    /// <inheritdoc/>
    public string? Open()
    {
        OpenFileDialog dialog = new OpenFileDialog { Filter = EditorText.Filter, Title = EditorText.DialogTitle, CheckFileExists = true };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
    /// <inheritdoc/>
    public string? Save(string name)
    {
        SaveFileDialog dialog = new SaveFileDialog
        { Filter = EditorText.Filter, Title = EditorText.DialogTitle, FileName = name, DefaultExt = ".json", AddExtension = true, OverwritePrompt = true };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
    /// <inheritdoc/>
    public bool ConfirmDiscard() => MessageBox.Show(EditorText.Discard, EditorText.DialogTitle,
        MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes;
}
