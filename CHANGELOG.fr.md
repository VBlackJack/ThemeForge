# Historique des versions

[English](CHANGELOG.md) | Français

Les changements notables de ThemeForge sont consignés ici.
Le format suit [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) et le projet utilise le [versionnement sémantique](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.2.0] - 2026-09-07

### Ajouts

- Palettes JSON externes versionnées et validées, intégrées au bootstrap et aux préférences.
- Studio : ouvrir/enregistrer/exporter, historique, protection du brouillon, comparaison,
  validation immédiate, diagnostics de contraste et journal quotidien asynchrone.
- Contrôles CI communs pour PR/main/tags, aller-retour Studio et consommateur de packages
  isolé avec restauration des préférences après redémarrage.
- Matrice de rendu/accessibilité et mesures de performance avec budgets configurables.


### Corrections

- Rejet des saisies numériques non finies et limitation des dépassements avant affectation.
- Validation des préférences d’accent et préparation des palettes avant modification de l’état actif.
- Suppression des surcharges Studio au changement de thème ; réinitialisation sur la palette active.
- Mémorisation du suivi Windows même si le thème résolu reste identique, et arrêt du suivi
  d’accent existant lorsque `FollowAccent` vaut false.
- Contraste du texte des boutons adapté aux états d’accent, fermeture des notifications
  nommée et accessible au clavier, redimensionnement des colonnes DataGrid rétabli.
- Respect des préférences d’animation Windows et de `Motion.ReduceMotion` hérité.
- Publication des tags conditionnée aux contrôles de sources, compilation, tests et attributions ;
  arrêt au premier échec d’envoi. Inclusion de NOTICE et des textes de licence dans les packages.
- Découpage des fichiers C# trop longs et contrôle CI de la limite de 200 lignes de source.

## [2.1.0] - 2026-06-20

### Ajouts

- Thème sombre Magellan dérivé de la marque, avec attribution et limites d’usage dans NOTICE.

## [2.0.0] - 2026-06-14

Version « Drop-in » : une nouvelle application WPF entièrement thémée en une commande et un appel, avec suivi Windows, mémorisation du choix et barre de titre intégrée.

### Ajouts

- Suivi Windows combiné : interface optionnelle `IWindowsThemeFollower` avec un appel `FollowWindows(WindowsFollowOptions)` activant les suivis clair/sombre et accent. `IThemeService` reste inchangée ; cette capacité possède sa propre interface.
- Persistance : enregistrement `ThemePreference`, contrat `IThemePreferenceStore` et implémentation `JsonThemePreferenceStore` dans l’espace de noms `Persistence`. La génération de source intégrée à `System.Text.Json` préserve l’absence de dépendances NuGet du moteur. `Load()` renvoie null pour un fichier absent, mal formé ou de schéma inconnu, sans lever d’exception ; `Save()` écrit atomiquement.
- Initialisation dans `ThemeForge.Theme.DependencyInjection` : `AddThemeForge(Application, Action<ThemeForgeOptions>)` et `UseThemeForge()`. Restauration de la préférence ou du défaut, activation du suivi Windows et sauvegarde automatique des changements. Le bootstrap possède le thème de bout en bout ; `App.xaml` fusionne uniquement les styles, ce qui évite les conflits d’ordre des dictionnaires.
- Barre de titre optionnelle : `window.ApplyThemeForgeTitleBar(themeService, options?)` applique le mode clair/sombre et les couleurs à la zone non cliente via `DwmSetWindowAttribute`, suit les changements de thème et s’adapte aux anciennes versions de Windows.
- Package `ThemeForge.Templates` : `dotnet new tf-wpf -n App` crée une application WPF avec suivi Windows, persistance, barre de titre thémée et sélecteur de thème.

### Notes

- L’ancienne surcharge `AddThemeForge(IServiceCollection, Application, IReadOnlyList<string>?)` reste inchangée. Tous les ajouts sont compatibles ; le changement de version majeure marque le jalon de la campagne, pas une rupture de compatibilité.

## [1.3.0] - 2026-06-14

### Ajouts

- Package `ThemeForge.Theme.DependencyInjection` et extension `AddThemeForge(IServiceCollection, Application, IReadOnlyList<string>?)`. Un singleton `ThemeService` partagé est exposé via `IThemeService`, `ISystemThemeFollower` et `ISystemAccentFollower`, qui résolvent la même instance. Plus besoin de convertir `IThemeService` vers le type concret pour utiliser les suivis système. Le moteur reste sans dépendances ; seul le nouveau package référence `Microsoft.Extensions.DependencyInjection.Abstractions`.

### Notes

- `TryAddSingleton` respecte les services enregistrés au préalable par l’application. `Application` est capturée par la fabrique du singleton, sans recherche globale via `Application.Current`.

## [1.2.0] - 2026-06-14

### Ajouts

- Suivi automatique optionnel de la couleur d’accent Windows via `ISystemAccentFollower`. `IThemeService` reste inchangée pour préserver la compatibilité SemVer. La couleur Windows reste distincte de la palette discrète `AccentTint`.
- `ISystemAccentProvider` et `RegistrySystemAccentProvider` lisent la valeur DWM `AccentColor` et réagissent aux changements de préférences Windows, sans modification du TFM ni dépendance WinRT.
- Interrupteur Studio pour suivre l’accent Windows ; le sélecteur de teinte est désactivé jusqu’à la reprise du contrôle manuel.

## [1.1.0] - 2026-06-13

### Ajouts

- Suivi automatique optionnel du mode clair/sombre Windows via `ISystemThemeFollower`. `IThemeService` reste inchangée pour préserver la compatibilité SemVer. L’application définit ses variantes claires et sombres.
- `ISystemThemeProvider` et `RegistrySystemThemeProvider` lisent `AppsUseLightTheme` et réagissent aux préférences Windows, sans modification du TFM ni dépendance WinRT.
- Interrupteur Studio utilisant `Folio` en mode clair et `Drakul` en mode sombre.

## [1.0.0] - 2026-06-13

Première version stable du framework de thèmes WPF ThemeForge (.NET 10).

### Ajouts

- Moteur `ThemeForge.Theme` : `IThemeService` et `ThemeService`, remplacement de `ResourceDictionary` à l’exécution, `ThemeRevision` et `ThemeChanged`.
- 16 thèmes v6 : `Dracula` canonique, `Drakul` et 14 palettes originales Apache 2.0. Vérification WCAG AA, sauf les exceptions historiques documentées de Dracula.
- Axe `AccentTint` : neuf teintes indépendantes, états de survol et de pression dérivés en Oklab, conservées lors des changements de thème.
- Tokens partagés `DesignTokens.xaml` : espacements, rayons et tailles de police.
- 23 contrôles WPF natifs stylés dans `ThemeForge.Controls`.
- Composites : Card, IconButton, Badge, Chip, ToggleSwitch, Avatar, SearchBox, Toast/ToastHost, ToolTip thémé, Dialog, NumericUpDown, SegmentedControl et Breadcrumb, avec pairs UI Automation personnalisés.
- Studio : changement de thème et d’accent à l’exécution, édition de palette en direct.
- Packages NuGet `ThemeForge.Theme` et `ThemeForge.Controls`, publiés sur GitHub Packages à la poussée d’un tag.

### Compilation

- Build déterministe : SDK fixé par `global.json` à `10.0.103` avec `latestPatch`. `var` rejeté à la compilation avec `error IDE0008`.
- CI sur `windows-2025-vs2026` ; 146 tests à cette version (39 Theme et 107 Controls).

[2.0.0]: https://github.com/VBlackJack/ThemeForge/releases/tag/v2.0.0
[1.3.0]: https://github.com/VBlackJack/ThemeForge/releases/tag/v1.3.0
[1.2.0]: https://github.com/VBlackJack/ThemeForge/releases/tag/v1.2.0
[1.1.0]: https://github.com/VBlackJack/ThemeForge/releases/tag/v1.1.0
[1.0.0]: https://github.com/VBlackJack/ThemeForge/releases/tag/v1.0.0
