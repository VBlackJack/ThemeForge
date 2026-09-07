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
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThemeForge.Theme.Palettes;

namespace ThemeForge.Studio.ViewModels;

/// <summary>A palette colour with immediate validation and an original swatch.</summary>
public sealed partial class SlotViewModel : ObservableObject
{
    private readonly string _resourceKey;
    private readonly Action<string, Color?> _applyOverride;
    private readonly Action? _validationChanged;
    /// <summary>Creates an editable opaque colour row.</summary>
    public SlotViewModel(string name, string resourceKey, Color initial, Action<string, Color?> applyOverride, Action? validationChanged = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceKey);
        ArgumentNullException.ThrowIfNull(applyOverride);
        Name = name; _resourceKey = resourceKey; OriginalColor = initial;
        _color = initial; _hex = PaletteValidation.ToHex(initial);
        _applyOverride = applyOverride; _validationChanged = validationChanged;
    }
    /// <summary>Slot identifier and accessible name.</summary>
    public string Name { get; }
    /// <summary>Colour at the start of the document.</summary>
    public Color OriginalColor { get; }
    [ObservableProperty] private Color _color;
    [ObservableProperty] private string _hex;
    [ObservableProperty] private bool _isValid = true;

    partial void OnHexChanged(string value)
    {
        IsValid = PaletteValidation.TryParseColor(value, out Color parsed);
        if (IsValid) { Color = parsed; _applyOverride(_resourceKey, parsed); }
        _validationChanged?.Invoke();
    }
    [RelayCommand]
    private void Reset() => Hex = PaletteValidation.ToHex(OriginalColor);
}
