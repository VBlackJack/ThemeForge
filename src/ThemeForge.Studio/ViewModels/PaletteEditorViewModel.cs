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

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThemeForge.Theme;

namespace ThemeForge.Studio.ViewModels;

/// <summary>
/// View model of the "Edit" section. Snapshots the current theme's slot
/// colors into editable rows, exposes a global reset command, and reloads
/// itself when the theme changes so the editor stays in sync with the
/// active ResourceDictionary.
/// </summary>
public sealed partial class PaletteEditorViewModel : ObservableObject
{
    private static readonly string[] CanonicalSlotNames =
    {
        "Background", "CurrentLine", "Selection", "Foreground", "Comment",
        "Cyan", "Green", "Orange", "Pink", "Purple", "Red", "Yellow",
    };

    private static readonly string[] SemanticSlotNames =
    {
        "Surface", "SurfaceAlt", "Border",
        "Accent", "AccentHover", "AccentPressed",
        "TextPrimary", "TextSecondary",
        "Success", "Warning", "Error", "Info",
    };

    private static readonly string[] ExtendedSlotNames =
    {
        "Blue",
    };

    public PaletteEditorViewModel(IThemeService themeService)
    {
        ArgumentNullException.ThrowIfNull(themeService);
        CanonicalSlots = new ObservableCollection<SlotViewModel>();
        SemanticSlots = new ObservableCollection<SlotViewModel>();
        ExtendedSlots = new ObservableCollection<SlotViewModel>();
        ReloadFromTheme();
        themeService.ThemeChanged += (_, _) => ReloadFromTheme();
    }

    /// <summary>The 12 canonical palette slots (Background/Cyan/Purple...).</summary>
    public ObservableCollection<SlotViewModel> CanonicalSlots { get; }

    /// <summary>The 12 semantic tokens (Surface/Accent/TextPrimary/Success...).</summary>
    public ObservableCollection<SlotViewModel> SemanticSlots { get; }

    /// <summary>The extended accent slots (Blue).</summary>
    public ObservableCollection<SlotViewModel> ExtendedSlots { get; }

    [RelayCommand]
    private void ResetAll()
    {
        foreach (SlotViewModel slot in CanonicalSlots)
        {
            slot.ResetCommand.Execute(null);
        }
        foreach (SlotViewModel slot in SemanticSlots)
        {
            slot.ResetCommand.Execute(null);
        }
        foreach (SlotViewModel slot in ExtendedSlots)
        {
            slot.ResetCommand.Execute(null);
        }
    }

    private void ReloadFromTheme()
    {
        CanonicalSlots.Clear();
        SemanticSlots.Clear();
        ExtendedSlots.Clear();
        Populate(CanonicalSlots, CanonicalSlotNames);
        Populate(SemanticSlots, SemanticSlotNames);
        Populate(ExtendedSlots, ExtendedSlotNames);
    }

    private static void Populate(ObservableCollection<SlotViewModel> target, string[] names)
    {
        foreach (string name in names)
        {
            string resourceKey = name + "Brush";
            if (Application.Current.Resources[resourceKey] is SolidColorBrush brush)
            {
                target.Add(new SlotViewModel(name, resourceKey, brush.Color));
            }
        }
    }
}
