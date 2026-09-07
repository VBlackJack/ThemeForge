# ThemeForge

English | [Français](README.fr.md)

A theming framework for **.NET 10 WPF** applications.

ThemeForge provides runtime theme switching, Windows light/dark and accent following, user preference persistence, and title bar theming. Reuse one engine across applications instead of maintaining separate resource dictionaries and styling conventions.

## Custom palettes and validation

ThemeForge 2.2.0 includes JSON palette save/export, undo/redo, comparison and contrast
diagnostics in Studio. Applications load these palettes without recompiling the engine.

- [Create and load a palette](docs/external-palettes.md)
- [Consumer, visual and performance checks](docs/quality.md)

## Quick start

Create an application with Windows following, persistence, and title bar theming already wired:

```pwsh
dotnet new install ThemeForge.Templates::2.2.0
dotnet new tf-wpf -n MyApp
```

Packages restore anonymously from nuget.org.

## Integrate into an existing application

Reference the packages:

```xml
<ItemGroup>
  <PackageReference Include="ThemeForge.Theme" Version="2.2.0" />
  <PackageReference Include="ThemeForge.Controls" Version="2.2.0" />
  <PackageReference Include="ThemeForge.Theme.DependencyInjection" Version="2.2.0" />
</ItemGroup>
```

Merge the native styles in `App.xaml`:

```xml
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <ResourceDictionary Source="pack://application:,,,/ThemeForge.Controls;component/Styles/Studio.xaml"/>
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

Let the bootstrap own the theme. A statically merged theme is not managed by the engine and can override the brushes applied at runtime.

Register and start the engine in `OnStartup`:

```csharp
ServiceCollection services = new ServiceCollection();
services.AddThemeForge(this, options =>
{
    options.DefaultTheme = ThemeNames.Drakul;
    options.WindowsFollow = new WindowsFollowOptions
    {
        LightTheme = ThemeNames.Folio,
        DarkTheme = ThemeNames.Drakul,
    };
    options.FollowWindowsByDefault = true;
    options.ApplicationName = "MyApp";
});
_services = services.BuildServiceProvider();
_services.UseThemeForge();
```

`UseThemeForge()` restores the user's preference or applies the default, starts Windows following, and saves subsequent changes. Dispose the service provider when the application exits.

Apply title bar theming after `InitializeComponent()`:

```csharp
this.ApplyThemeForgeTitleBar(themeService);
```

See the [integration guide](docs/integration-guide.md) for startup, dependency injection, controls, and custom themes.

## Features

- Runtime theme switching through `IThemeService.ApplyTheme(...)`.
- `ThemeRevision` increments before `ThemeChanged`, supporting visual cache invalidation.
- Opt-in Windows theme and accent following through `IWindowsThemeFollower`.
- User preferences stored through `IThemePreferenceStore`.
- Title bar colors synchronized with the active theme where Windows supports them.
- Independent accent selection through `AccentTint`.
- 17 themes in the current source catalogue, 23 styled native WPF controls, and 13 composites.

## Packages

| Package | Purpose |
|---|---|
| `ThemeForge.Theme` | Theme engine, Windows following, persistence, title bar integration, and palettes. No NuGet dependencies. |
| `ThemeForge.Controls` | Native WPF styles and composite controls. |
| `ThemeForge.Theme.DependencyInjection` | `AddThemeForge(...)` registration and `UseThemeForge()` startup. |
| `ThemeForge.Templates` | The `dotnet new tf-wpf` application template. |

Native styles are aggregated in `Styles/Studio.xaml`. Composite controls use the standard WPF `Themes/Generic.xaml` convention.

## Theme catalogue

The engine is application agnostic. The bundled catalogue has a Dracula aesthetic, with the canonical Dracula palette as its historical reference. See the [catalogue and attribution guide](docs/theme-catalog.md).

## Studio

Download **ThemeForge.Studio-2.2.0-win-x64.zip** from the [release](https://github.com/VBlackJack/ThemeForge/releases/tag/v2.2.0),
extract the complete archive and launch `ThemeForge.Studio.exe`. The .NET runtime is included.

To run from source with the .NET 10 SDK:

```pwsh
dotnet run --project src/ThemeForge.Studio
```

## Architecture

Inspired by Heimdall.Next's theming architecture and authored from scratch. The framework exposes canonical colour slots and generic semantic tokens, without application-specific brushes or copied Heimdall code.

`IThemeService` remains stable for SemVer compatibility. Additional capabilities use separate opt-in interfaces. Theme dictionary replacement goes through one path that increments `ThemeRevision` before raising `ThemeChanged`.

## Version history

Version **2.2.0** ships four packages and a Windows x64 Studio archive. See the
[release notes](docs/releases/2.2.0.md), [downloads](https://github.com/VBlackJack/ThemeForge/releases/tag/v2.2.0)
and [changelog](CHANGELOG.md) for changes and upgrade guidance.

## Development

```pwsh
dotnet build ThemeForge.slnx
dotnet test ThemeForge.slnx -m:1
```

Enable the [local pre-push checks](docs/pre-push-hook.md) for each clone.

## License

- Framework, styles, and original palettes: Apache 2.0, Julien Bombled.
- Drakul: Apache 2.0 adaptation by Julien Bombled.
- Dracula: MIT, Zeno Rocha.
- Magellan: brand-derived palette; see the attribution and trademark limits in [NOTICE](NOTICE).

README, documentation, and release notes use English by default, with separate French translations.
