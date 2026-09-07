# Runtime switching and custom themes

English | [Français](fr/custom-themes.md)

[Introduction](integration-guide.md) | [Startup](bootstrap.md) | [Controls](controls.md)

## 12. Switch themes at runtime

`IThemeService.ApplyTheme(string name)` applies a known theme and is idempotent. `AvailableThemes` provides the stable list of names.

```csharp
public sealed partial class ShellViewModel : ObservableObject
{
    private readonly IThemeService _themeService;
    public IReadOnlyList<string> Themes => _themeService.AvailableThemes;
    public ShellViewModel(IThemeService themeService) => _themeService = themeService;

    [ObservableProperty]
    private string? _selectedTheme;

    partial void OnSelectedThemeChanged(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            _themeService.ApplyTheme(value);
        }
    }
}
```

```xml
<ComboBox ItemsSource="{Binding Themes}"
          SelectedItem="{Binding SelectedTheme, Mode=TwoWay}"/>
```

`ThemeRevision` increments before `ThemeChanged`, allowing converters, multi-bindings, and visual caches to refresh:

```csharp
_themeService.ThemeChanged += (_, e) =>
{
    CurrentThemeName = e.CurrentTheme;
    CurrentThemeRevision = e.Revision;
};
```

When persistence is enabled, reflect `IsFollowingSystem` in the follow toggle. Selecting a theme manually disables following and saves that choice. The template's `MainWindow` demonstrates this wiring.

## 13. External palettes

Create and export a palette in Studio, then load its JSON without recompiling
ThemeForge. See the [external palette guide](external-palettes.md) for the format,
validation, loading and persistence.

Framework contributors can still add a built-in variant: create its dictionary
under `src/ThemeForge.Theme/Themes/`, retain attribution and add its name to
`ThemeNames`. This procedure extends the bundled catalogue.

## 14. Known limitations

ThemeForge targets WPF desktop, not WinForms, UWP, WinUI 3, Avalonia, or MAUI.

The Visual Studio designer does not fully preview runtime switching. `ThemeService` updates `Application.Resources.MergedDictionaries` at runtime; launch the application to verify rendering when the designer appears to omit a style.

Title bar support depends on the Windows version. See [title bar integration](bootstrap.md#8-theme-the-title-bar).

[README](../README.md) | [NOTICE](../NOTICE)

## Studio edit lifetime

Studio owns a palette document with undo/redo history and a dirty indicator. A manual
theme or accent selection asks before discarding the draft. External changes, including
Windows changes, retain the draft. Closing the window, opening another file and creating
a new palette also protect unsaved work. Reset restores the original document and can
be undone. Unrelated host resources are preserved.
