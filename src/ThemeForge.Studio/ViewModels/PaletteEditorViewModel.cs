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
using Microsoft.Extensions.Logging;
using ThemeForge.Studio.Resources;
using ThemeForge.Studio.Services;
using ThemeForge.Theme;
using ThemeForge.Theme.Palettes;

namespace ThemeForge.Studio.ViewModels;

/// <summary>Owns a portable palette document, its preview overlay and editing history.</summary>
public sealed partial class PaletteEditorViewModel : ObservableObject, IDisposable
{
    private readonly IThemeService _themeService;
    private readonly ResourceDictionary _resources;
    private readonly ResourceDictionary _overrides = new ResourceDictionary();
    private readonly IPaletteFileDialogs? _dialogs;
    private readonly ILogger<PaletteEditorViewModel>? _logger;
    private ThemePalette _current = new ThemePalette();
    private ThemePalette _original = new ThemePalette();
    private ThemePalette _saved = new ThemePalette();
    private bool _updating;
    private bool _discardOnThemeChange;
    private bool _disposed;
    private string? _filePath;

    /// <summary>Dialogs and logging are supplied by the host; tests can exercise the editor without either.</summary>
    public PaletteEditorViewModel(IThemeService themeService, ResourceDictionary resources,
        IPaletteFileDialogs? dialogs = null, ILogger<PaletteEditorViewModel>? logger = null)
    {
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _resources = resources ?? throw new ArgumentNullException(nameof(resources));
        _dialogs = dialogs; _logger = logger;
        ReloadFromTheme();
        themeService.ThemeChanged += OnThemeChanged;
    }
    /// <summary>Canonical colour rows.</summary>
    public ObservableCollection<SlotViewModel> CanonicalSlots { get; } = new ObservableCollection<SlotViewModel>();
    /// <summary>Semantic colour rows.</summary>
    public ObservableCollection<SlotViewModel> SemanticSlots { get; } = new ObservableCollection<SlotViewModel>();
    /// <summary>Extended colour rows.</summary>
    public ObservableCollection<SlotViewModel> ExtendedSlots { get; } = new ObservableCollection<SlotViewModel>();
    private IEnumerable<SlotViewModel> AllSlots => CanonicalSlots.Concat(SemanticSlots).Concat(ExtendedSlots);
    [ObservableProperty] private string _paletteName = string.Empty;
    [ObservableProperty] private bool _isDirty;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _showOriginal;
    [ObservableProperty] private string _status = string.Empty;
    /// <summary>Original provenance retained in every exported document.</summary>
    public string Attribution => _current.Attribution;
    /// <summary>Localized dirty state.</summary>
    public string DocumentState => IsDirty ? EditorText.Modified : EditorText.Clean;
    /// <summary>Editing is disabled during file operations.</summary>
    public bool CanEdit => !IsBusy;

    private void OnThemeChanged(object? sender, ThemeChangedEventArgs args)
    {
        if ((IsDirty || IsBusy) && _dialogs is not null && !_discardOnThemeChange)
        { ApplyPreview(); Status = EditorText.Kept; return; }
        _discardOnThemeChange = false;
        ReloadFromTheme();
    }
    private void ReloadFromTheme()
    {
        _resources.MergedDictionaries.Remove(_overrides);
        string name = "Custom" + _themeService.CurrentTheme;
        string attribution = _resources["ThemeAttribution"] as string ?? BuiltInPaletteAttribution.Describe(_themeService.CurrentTheme);
        LoadDocument(PaletteResources.Capture(_resources, name, attribution), null);
    }
    private void PopulateRows()
    {
        _updating = true;
        try
        {
            PaletteName = _current.Name;
            Populate(CanonicalSlots, PaletteSlots.Canonical);
            Populate(SemanticSlots, PaletteSlots.Semantic);
            Populate(ExtendedSlots, PaletteSlots.Extended);
        }
        finally { _updating = false; }
        OnPropertyChanged(nameof(Attribution));
        ApplyPreview(); RefreshState();
    }
    private void Populate(ObservableCollection<SlotViewModel> rows, IReadOnlyList<string> names)
    {
        rows.Clear();
        foreach (string name in names)
        {
            PaletteValidation.TryParseColor(_original.Colors[name], out Color original);
            SlotViewModel slot = new SlotViewModel(name, name + "Brush", original, ApplyOverride, RefreshState);
            slot.Hex = _current.Colors[name];
            rows.Add(slot);
        }
    }
    private void ApplyPreview()
    {
        _resources.MergedDictionaries.Remove(_overrides);
        _overrides.Clear();
        ThemePalette preview = ShowOriginal ? _original : _current;
        foreach (KeyValuePair<string, string> entry in preview.Colors)
        {
            PaletteValidation.TryParseColor(entry.Value, out Color color);
            SolidColorBrush brush = new SolidColorBrush(color); brush.Freeze();
            _overrides[entry.Key + "Brush"] = brush;
            _overrides[entry.Key + "Color"] = color;
        }
        _resources.MergedDictionaries.Add(_overrides);
        RefreshDiagnostics();
    }
    partial void OnShowOriginalChanged(bool value) => ApplyPreview();
    partial void OnIsBusyChanged(bool value) { OnPropertyChanged(nameof(CanEdit)); RefreshState(); }
    /// <summary>Releases the event subscription and only the editor-owned resources.</summary>
    public void Dispose()
    {
        if (_disposed) { return; }
        _disposed = true;
        _themeService.ThemeChanged -= OnThemeChanged;
        _resources.MergedDictionaries.Remove(_overrides);
    }
}
