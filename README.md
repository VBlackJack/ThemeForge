# ThemeForge

Framework de théming pour applications **.NET 10 WPF**.

> Un nouveau projet WPF entièrement thémé en une commande et un appel : suivi
> automatique de Windows, mémoire du choix utilisateur, barre de titre comprise.
> Et tu n'y reviens plus.

## Pourquoi ?

Chaque nouveau projet WPF redémarre de zéro côté style : couleurs hardcodées,
`ResourceDictionary` recopiés à la main, design tokens incohérents, pas de
mécanique de bascule de thème, et trois mois plus tard chaque app a son propre
dialecte visuel.

ThemeForge est un moteur de théming **réutilisable et agnostique de l'app** :
swap de thème au runtime, suivi du clair/sombre et de l'accent Windows,
persistance du choix, theming de la barre de titre, plus un catalogue de 16
variantes prêtes à l'emploi et des styles WPF prêts à merger. Le moteur est le
produit ; le catalogue est livré avec.

## Démarrer en une commande

La voie la plus rapide : le template. Il génère une app WPF déjà entièrement
câblée (suivi Windows, persistance, barre de titre), prête à lancer.

```pwsh
dotnet new install ThemeForge.Templates
dotnet new tf-wpf -n MonApp
```

Les packages se restaurent depuis nuget.org en accès anonyme, sans aucune
authentification.

## Intégrer dans une app existante

### 1. Référencer les packages

```xml
<ItemGroup>
  <PackageReference Include="ThemeForge.Theme" Version="2.0.0" />
  <PackageReference Include="ThemeForge.Controls" Version="2.0.0" />
  <PackageReference Include="ThemeForge.Theme.DependencyInjection" Version="2.0.0" />
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

Le détail complet est dans [`docs/integration-guide.md`](docs/integration-guide.md).

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
- **Catalogue** : 16 variantes, 23 contrôles WPF natifs stylés, 13 composites.

## Composants

| Package | Rôle |
|---|---|
| `ThemeForge.Theme` | Moteur : `IThemeService`, `IWindowsThemeFollower`, `IThemePreferenceStore`, theming barre de titre, 16 thèmes. Zéro dépendance NuGet. |
| `ThemeForge.Controls` | 23 contrôles WPF natifs stylés + 13 composites (`Card`, `IconButton`, `Badge`, `Chip`, `ToggleSwitch`, `Avatar`, `SearchBox`, `Toast`, `ToastHost`, `Breadcrumb`, `Dialog`, `NumericUpDown`, `SegmentedControl`) |
| `ThemeForge.Theme.DependencyInjection` | Wiring DI : `AddThemeForge(...)` et `UseThemeForge()` |
| `ThemeForge.Templates` | Template `dotnet new tf-wpf` |

Les styles natifs sont agrégés par
`ThemeForge.Controls;component/Styles/Studio.xaml`. Les composites suivent la
convention WPF `Themes/Generic.xaml`.

## Le catalogue de thèmes

Le moteur est agnostique ; le catalogue livré est d'esthétique Dracula, dont
Dracula est l'héritage historique et la variante de référence.

**16 variantes** : `Dracula` MIT canonique, `Drakul` sibling AA-compliant Apache
2.0, puis 14 palettes originales Apache 2.0 de Julien Bombled.

- **Root** (2) : Dracula, Drakul
- **Dark** (10) : Striga, Cinder, Bracken, Tarn, Mortis, Slate, Voivode,
  Carmilla, Whitby, Vesper
- **Light** (2) : Parchment, Folio
- **Alt** (2) : Wormwood, Sconce

### La paire Dracula / Drakul

- **Dracula** : palette MIT canonique de Zeno Rocha, préservée à l'identique pour
  fidélité historique. Le Comment `#6272A4` ne clear pas WCAG AA
  (Comment/CurrentLine = 1.94:1) : c'est un défaut accessibilité du design
  d'origine.
- **Drakul** : sibling Apache 2.0, AA-compliant par un lift du Comment vers
  `#B3BBD6` (Comment/CurrentLine = 4.79:1). Tout le reste est byte-identique. Le
  nom honore Vlad II Dracul, de l'Ordre du Dragon.

Choisis Dracula pour la palette canonique, Drakul pour l'accessibilité AA stricte
sans perdre l'ADN visuel.

### Geometric Color Palette

Les 14 palettes originales sont ingénierées, pas intuitives : les valeurs hex
sont calculées depuis des cibles déclarées en Oklab, puis auditées.

- **Bande Dark uniforme** : les 10 variantes Dark partagent les mêmes 7 accents,
  seul le hue du background change.
- **Bande Light uniforme** : les 2 variantes Light partagent un jeu d'accents
  plus sombre, lisible sur surfaces claires.
- **Alt** : héritent de la bande Dark et brisent exactement un accent pour leur
  signature.
- **WCAG 2.1 AA** : les couples Foreground/Background, Comment/Background et
  Comment/CurrentLine sont vérifiés par des gates runtime. Dracula reste
  l'exception historique ; le reste du catalogue est AA.

### Attribution des palettes

- **`Dracula`** : palette MIT canonique de **Zeno Rocha** (Dracula Theme,
  https://draculatheme.com), racine historique du catalogue.
- **`Drakul`** : variante Apache 2.0 de **Julien Bombled**, dérivée de Dracula
  avec le seul slot Comment ajusté pour clear WCAG AA.
- **Les 14 originales** : `Striga`, `Cinder`, `Bracken`, `Tarn`, `Mortis`,
  `Slate`, `Voivode`, `Carmilla`, `Whitby`, `Vesper`, `Parchment`, `Folio`,
  `Wormwood`, `Sconce`, par **Julien Bombled** sous Apache 2.0.

Aucun nom ni aucune valeur RGB d'un scheme commercial Dracula n'est repris.
ThemeForge reprend l'ADN visuel, pas un produit payant. Voir `NOTICE` pour la
table d'attribution complète.

## Studio

Studio prévisualise les 16 variantes, teste les contrôles et édite les slots hex
en direct.

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

## État

**v2.0.0 "Drop-in"** publiée sur nuget.org et GitHub Packages. Quatre packages :
`ThemeForge.Theme`, `ThemeForge.Controls`, `ThemeForge.Theme.DependencyInjection`,
`ThemeForge.Templates`. Suite de tests : 219 verts (108 pour `ThemeForge.Theme`,
111 pour `ThemeForge.Controls`). CI GitHub Actions verte, publication par Trusted
Publishing OIDC.

## Licence

- Code, styles, framework : Apache 2.0 — Julien Bombled
- 14 palettes originales : Apache 2.0 — Julien Bombled
- Palette Drakul : Apache 2.0 — Julien Bombled
- Palette Dracula : MIT — Zeno Rocha (racine historique)

Voir `NOTICE` pour le détail de la philosophie Geometric Color Palette et la table
d'attribution.

## Pour aller plus loin

- [`docs/integration-guide.md`](docs/integration-guide.md) : guide d'intégration WPF complet.
- [`NOTICE`](NOTICE) : attributions des palettes.
