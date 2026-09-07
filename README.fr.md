# ThemeForge

[English](README.md) | Français

Framework de théming pour applications **.NET 10 WPF**.

> Un nouveau projet WPF entièrement thémé en une commande et un appel : suivi
> automatique de Windows, mémoire du choix utilisateur, barre de titre comprise.
> Et tu n'y reviens plus.

## Palettes personnalisées et validation

ThemeForge 2.2.0 inclut un éditeur avec sauvegarde/export JSON, annuler/rétablir,
comparaison et diagnostics de contraste. Les palettes se chargent sans recompiler le moteur.

- [Créer et charger une palette](docs/fr/external-palettes.md)
- [Tests consommateurs, rendu et performances](docs/fr/quality.md)

## Pourquoi ?

Chaque nouveau projet WPF redémarre de zéro côté style : couleurs hardcodées,
`ResourceDictionary` recopiés à la main, design tokens incohérents, pas de
mécanique de bascule de thème, et trois mois plus tard chaque app a son propre
dialecte visuel.

ThemeForge est un moteur de théming **réutilisable et agnostique de l'app** :
swap de thème au runtime, suivi du clair/sombre et de l'accent Windows,
persistance du choix, theming de la barre de titre, plus un catalogue de 17
variantes prêtes à l'emploi et des styles WPF prêts à merger. Le moteur est le
produit ; le catalogue est livré avec.

## Démarrer en une commande

La voie la plus rapide : le template. Il génère une app WPF déjà entièrement
câblée (suivi Windows, persistance, barre de titre), prête à lancer.

```pwsh
dotnet new install ThemeForge.Templates::2.2.0
dotnet new tf-wpf -n MonApp
```

Les packages se restaurent depuis nuget.org en accès anonyme, sans aucune
authentification.

## Intégrer dans une app existante

### 1. Référencer les packages

```xml
<ItemGroup>
  <PackageReference Include="ThemeForge.Theme" Version="2.2.0" />
  <PackageReference Include="ThemeForge.Controls" Version="2.2.0" />
  <PackageReference Include="ThemeForge.Theme.DependencyInjection" Version="2.2.0" />
</ItemGroup>
```

### 2. Merger les styles, jamais un thème

Dans `App.xaml`, merge **uniquement** le dictionnaire de styles. Le thème se
pose au runtime, pas en statique.

```xml
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <ResourceDictionary Source="pack://application:,,,/ThemeForge.Controls;component/Styles/Studio.xaml"/>
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

> Important : ne merge jamais un thème en statique dans `App.xaml`. Un thème
> statique n'est pas géré par le moteur, reste en dernière position du merge et
> ses brushes l'emportent sur le thème appliqué au runtime. Le thème est possédé
> de bout en bout par le bootstrap ci-dessous.

### 3. Bootstrap en une ligne

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

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
        options.ApplicationName = "MonApp";
    });

    _services = services.BuildServiceProvider();
    _services.UseThemeForge();
}
```

`UseThemeForge()` restaure le choix persisté de l'utilisateur, ou applique le
défaut, arme le suivi Windows, et sauvegarde automatiquement chaque changement.

### 4. Thématiser la barre de titre

Dans le code-behind de ta fenêtre :

```csharp
public MainWindow(IThemeService themeService)
{
    InitializeComponent();
    this.ApplyThemeForgeTitleBar(themeService);
}
```

Le détail complet est dans [`docs/fr/integration-guide.md`](docs/fr/integration-guide.md).

## Ce que tu obtiens

- **Moteur runtime** : `IThemeService.ApplyTheme(...)` swap le thème actif, avec
  un compteur `ThemeRevision` incrémenté avant l'événement `ThemeChanged`.
- **Suivi Windows opt-in** : `IWindowsThemeFollower.FollowWindows(...)` aligne le
  thème clair/sombre et l'accent sur les réglages Windows en un seul appel.
- **Persistance** : `IThemePreferenceStore` mémorise le choix de l'utilisateur
  (thème, accent, suivi) et le restaure au démarrage.
- **Barre de titre thématisée** : zone non-cliente alignée sur le thème courant,
  best-effort selon la version de Windows.
- **Accent orthogonal** : `AccentTint` recolore l'accent indépendamment du thème.
- **Catalogue** : 17 variantes, 23 contrôles WPF natifs stylés, 13 composites.

## Composants

| Package | Rôle |
|---|---|
| `ThemeForge.Theme` | Moteur : `IThemeService`, `IWindowsThemeFollower`, `IThemePreferenceStore`, theming barre de titre, 17 thèmes. Zéro dépendance NuGet. |
| `ThemeForge.Controls` | 23 contrôles WPF natifs stylés + 13 composites (`Card`, `IconButton`, `Badge`, `Chip`, `ToggleSwitch`, `Avatar`, `SearchBox`, `Toast`, `ToastHost`, `Breadcrumb`, `Dialog`, `NumericUpDown`, `SegmentedControl`) |
| `ThemeForge.Theme.DependencyInjection` | Wiring DI : `AddThemeForge(...)` et `UseThemeForge()` |
| `ThemeForge.Templates` | Template `dotnet new tf-wpf` |

Les styles natifs sont agrégés par
`ThemeForge.Controls;component/Styles/Studio.xaml`. Les composites suivent la
convention WPF `Themes/Generic.xaml`.

## Catalogue de thèmes

Voir le [catalogue et les attributions](docs/fr/theme-catalog.md).

## Studio

Studio prévisualise les 17 variantes, teste les contrôles et édite les slots hex
en direct.

Téléchargez **ThemeForge.Studio-2.2.0-win-x64.zip** depuis la [release](https://github.com/VBlackJack/ThemeForge/releases/tag/v2.2.0),
extrayez toute l’archive et lancez `ThemeForge.Studio.exe`. Le runtime .NET est inclus.

Depuis les sources, avec le SDK .NET 10 :

```pwsh
dotnet run --project src/ThemeForge.Studio
```

## Architecture

Inspirée du moteur de théming de Heimdall.Next, mais construite de zéro pour
rester **agnostique de toute app** : pas de brushes applicatifs, juste les slots
canoniques et les tokens sémantiques (Background, Surface, Accent, TextPrimary,
etc.). `IThemeService` est gelé pour la stabilité SemVer ; chaque capacité
(suivi Windows, persistance, barre de titre) vit sur une interface opt-in
séparée. Le swap de `ResourceDictionary` passe par un point unique qui incrémente
`ThemeRevision` avant de lever `ThemeChanged`.

## Historique des versions

La version **2.2.0** fournit quatre packages et une archive Studio Windows x64. Voir les
[notes de version](docs/fr/releases/2.2.0.md), les [téléchargements](https://github.com/VBlackJack/ThemeForge/releases/tag/v2.2.0)
et le [journal des versions](CHANGELOG.fr.md) pour les changements et la migration.

## Développement

```pwsh
dotnet build ThemeForge.slnx
dotnet test ThemeForge.slnx -m:1
```

Active le [hook pre-push local](docs/fr/pre-push-hook.md) pour chaque clone.

## Licence

- Code, styles, framework : Apache 2.0 - Julien Bombled
- 14 palettes originales : Apache 2.0 - Julien Bombled
- Palette Drakul : Apache 2.0 - Julien Bombled
- Palette Dracula : MIT - Zeno Rocha (racine historique)

Voir `NOTICE` pour le détail de la philosophie Geometric Color Palette et la table
d'attribution.

## Pour aller plus loin

- [`docs/fr/integration-guide.md`](docs/fr/integration-guide.md) : guide d'intégration WPF complet.
- [`NOTICE`](NOTICE) : attributions des palettes.

README, documentation et notes de version sont en anglais par défaut, avec des traductions françaises séparées.
