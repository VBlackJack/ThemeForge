# Changelog

All notable changes to ThemeForge are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.3.0] - 2026-06-14

### Added
- `ThemeForge.Theme.DependencyInjection` package with a single
  `AddThemeForge(IServiceCollection, Application, IReadOnlyList<string>?)`
  extension. It registers `ThemeService` as one shared singleton and exposes it
  through `IThemeService`, `ISystemThemeFollower` and `ISystemAccentFollower`,
  all resolving to the same instance. Consumers no longer cast `IThemeService` to
  the concrete type to reach the system-follow capabilities. The core
  `ThemeForge.Theme` package stays dependency-free; only the new package carries
  `Microsoft.Extensions.DependencyInjection.Abstractions`.

### Notes
- Registrations use `TryAddSingleton`, so a consumer's own prior registration of
  any of these services is respected. The `Application` is captured in the
  singleton factory: no `Application.Current` global lookup.

## [1.2.0] - 2026-06-14

### Added
- Windows accent-color auto-follow for WPF apps through the opt-in
  `ISystemAccentFollower` capability. `IThemeService` remains unchanged for
  SemVer compatibility; the arbitrary Windows accent color stays separate from
  the discrete `AccentTint` palette.
- `ISystemAccentProvider` and `RegistrySystemAccentProvider` to read the DWM
  `AccentColor` registry value and react to Windows color preference changes
  without changing the project TFM or adding WinRT dependencies.
- Studio toggle to follow the Windows accent color while disabling the discrete
  accent tint picker until the user takes manual control again.

## [1.1.0] - 2026-06-13

### Added
- Windows light/dark auto-follow for WPF apps through the opt-in
  `ISystemThemeFollower` capability. `IThemeService` remains unchanged for
  SemVer compatibility; consumers map their own light and dark ThemeForge
  variants.
- `ISystemThemeProvider` and `RegistrySystemThemeProvider` to read
  `AppsUseLightTheme` and react to Windows preference changes without changing
  the project TFM or adding WinRT dependencies.
- Studio toggle to follow the Windows app theme mode, using `Folio` for light
  mode and `Drakul` for dark mode.

## [1.0.0] - 2026-06-13

First stable release of the ThemeForge WPF theming framework (.NET 10).

### Added
- Theme engine (`ThemeForge.Theme`): `IThemeService` / `ThemeService` with runtime
  `ResourceDictionary` swap, `ThemeRevision`, and `ThemeChanged`.
- 16 themes (v6): canonical `Dracula` plus `Drakul` and 14 original Apache-2.0
  palettes, WCAG AA verified (except documented historical Dracula slots).
- AccentTint axis: 9 orthogonal tints with Oklab-derived hover/pressed, persisted
  across theme switches.
- Shared design tokens (`DesignTokens.xaml`): spacing, radius, font sizes.
- 23 styled native WPF controls (`ThemeForge.Controls`).
- Composite controls: Card, IconButton, Badge, Chip, ToggleSwitch, Avatar,
  SearchBox, Toast/ToastHost, themed ToolTip, Dialog, NumericUpDown,
  SegmentedControl, Breadcrumb -- with custom UI Automation peers.
- Studio demo app: runtime theme/accent switching and a live palette editor.
- NuGet packaging for both `ThemeForge.Theme` and `ThemeForge.Controls`, published
  to GitHub Packages on tag push.

### Build
- Deterministic build: SDK pinned via `global.json` (10.0.103, `latestPatch`);
  `var` rejected at build time as `error IDE0008`.
- CI on `windows-2025-vs2026`; 146 tests (39 Theme + 107 Controls).

[1.3.0]: https://github.com/VBlackJack/ThemeForge/releases/tag/v1.3.0
[1.2.0]: https://github.com/VBlackJack/ThemeForge/releases/tag/v1.2.0
[1.1.0]: https://github.com/VBlackJack/ThemeForge/releases/tag/v1.1.0
[1.0.0]: https://github.com/VBlackJack/ThemeForge/releases/tag/v1.0.0
