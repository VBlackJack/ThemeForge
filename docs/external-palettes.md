# External palettes and Studio

English | [Français](fr/external-palettes.md)

These APIs require ThemeForge **2.2.0** or later. Keep all ThemeForge package versions aligned.

## Create a reusable palette

1. Run Studio and select the starting theme, then open **Edit**.
2. Choose a unique identifier beginning with an uppercase ASCII letter, followed by
   letters or digits. A built-in name cannot be replaced.
3. Edit opaque `#RRGGBB` colours. Invalid input is marked and disables saving.
4. Use the original/edited swatches, before/after panels and contrast diagnostics.
5. Use **Enregistrer** (Ctrl+S) or **Exporter** to write a JSON document.
6. Close and reopen it with **Ouvrir** (Ctrl+O). Metadata, attribution and colours persist.

**Annuler/Rétablir** keep the last 100 valid document changes. Reset is also undoable.
The contrast panel reports raw text/background pairs. It does not certify the complete
application or replace keyboard and screen-reader testing. Button templates may adapt
their foreground independently to preserve readable text.

Studio retains imported attribution. Derived built-in documents identify the source
palette and link to [NOTICE](../NOTICE). Review rights and attribution before distributing
a palette. No executable XAML is imported.

## Load in an application

Load the data asynchronously before registering the bootstrap:

```csharp
using ThemeForge.Theme.DependencyInjection;
using ThemeForge.Theme.Palettes;

ThemePalette palette = await PaletteJson.LoadAsync(palettePath, cancellationToken);
services.AddThemeForge(application, options =>
{
    options.Palettes = new[] { palette };
    options.DefaultTheme = palette.Name;
    options.ApplicationName = applicationName;
});
```

Call `UseThemeForge()` on the built service provider as usual. Register the external
palette on every startup, before restoring preferences. A stored name without a
registered palette falls back to the configured default.

For explicit registration without bootstrap:

```csharp
ThemeService service = new ThemeService(application);
service.RegisterPalette(palette);
service.ApplyTheme(palette.Name);
```

`IExternalThemeCatalog` exposes registration through dependency injection. Register
before binding a picker to the available-name snapshot. Registering takes an immutable
copy. Replacing built-in or currently active names is rejected; switch away before
replacing an external palette. Apply resources on the WPF dispatcher.

## Format and failure behaviour

- [JSON schema](palette.schema.json), [complete sample](../samples/palettes/SampleFolio.json).
- Version 1 requires `version`, `name`, `displayName`, `family`, `attribution`, `colors`.
- Exactly 25 slots: 12 canonical, 12 semantic and Blue. Each creates matching `Color`
  and frozen `Brush` resources; shared spacing/font/radius tokens come from ThemeForge.
- Accepted colours: `#RRGGBB` or opaque `#FFRRGGBB`, normalized to `#RRGGBB`.
- Maximum input: 64 KiB, JSON depth 4. Duplicate or unknown properties, missing slots,
  unknown versions/families, transparent values and invalid names are rejected.
- Runtime validation additionally rejects whitespace-only titles and attribution.
- Invalid loads leave the editor document untouched. Saves validate before writing,
  use a sibling temporary file and replace the destination after completion. Cancellation
  before replacement preserves the existing destination. The host handles propagated errors.

Studio reports file errors in its status area and asynchronous daily logs under
`%LocalAppData%/ThemeForge/Studio/logs`. `StudioLogOptions` controls directory, minimum
severity and logging enablement. Normal shutdown drains the log queue.

[Startup](bootstrap.md) | [Controls](controls.md) | [Quality checks](quality.md)
