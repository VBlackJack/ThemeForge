# ThemeForge integration guide

English | [Français](fr/integration-guide.md)

This guide covers integrating ThemeForge into a third-party WPF desktop application: startup, Windows following, persistence, title bars, native styles, composites, and theme switching.

## 1. Prerequisites

Use the .NET 10 SDK and an SDK-style WPF project targeting `net10.0-windows`.
ThemeForge packages are available from nuget.org without authentication. GitHub Packages is an optional secondary source and requires a GitHub PAT with `read:packages`.

## 2. Start from the template

```pwsh
dotnet new install ThemeForge.Templates
dotnet new tf-wpf -n MyApp
```

The template is the runnable reference for startup, a theme picker, Windows following, and title bar integration. For an existing application, continue below.

## 3. Reference packages

```xml
<ItemGroup>
  <PackageReference Include="ThemeForge.Theme" Version="2.2.0" />
  <PackageReference Include="ThemeForge.Controls" Version="2.2.0" />
  <PackageReference Include="ThemeForge.Theme.DependencyInjection" Version="2.2.0" />
</ItemGroup>
```

`ThemeForge.Theme` contains `IThemeService`, `ThemeService`, `ThemeNames`, the opt-in Windows followers, preference storage, title bar integration, and theme dictionaries. The core has no NuGet dependencies.

`ThemeForge.Controls` contains native styles, composites, and `Themes/Generic.xaml`.
The DI package adds registration and bootstrap extensions, depending on `Microsoft.Extensions.DependencyInjection.Abstractions`.

If the application does not already reference a DI container implementation, add:

```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.0" />
```

## 4. Merge styles in App.xaml

```xml
<Application
    x:Class="MyApp.App"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="pack://application:,,,/ThemeForge.Controls;component/Styles/Studio.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

Merge styles only. The bootstrap owns the theme at runtime. A static theme dictionary is not marked or removed by `ThemeService` and may override runtime brushes because WPF gives later dictionaries priority for duplicate keys.

Remove `StartupUri`: the startup code resolves and shows the main window through DI. Native styles are implicit, so normal WPF controls automatically pick them up.

## Continue

- [5-8. Bootstrap, Windows following, persistence, and title bars](bootstrap.md)
- [9-11. Native controls, composites, and design tokens](controls.md)
- [12-14. Runtime switching, custom variants, and limitations](custom-themes.md)
- [README](../README.md)
- [NOTICE](../NOTICE)
