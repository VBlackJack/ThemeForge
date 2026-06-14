# ThemeForge

Framework de thématisation Dracula pour applications **.NET 10 WPF**.

> Pose un thème Dracula propre sur une nouvelle app WPF en 5 minutes,
> pas en 2 jours.

## Pourquoi ?

Chaque nouveau projet WPF redémarre de zéro côté style : couleurs hardcodées,
`ResourceDictionary` recopiés à la main, design tokens incohérents, pas de
mécanique de bascule de thème, et trois mois plus tard chaque app a son propre
dialecte visuel.

ThemeForge fournit un moteur de theming réutilisable, 16 variantes Dracula
prêtes à l'emploi, des styles WPF prêts à merger, et une app **Studio** pour
les prévisualiser et les ajuster en direct avant de les exporter dans ton
projet cible.

## Composants

| Projet | Rôle |
|---|---|
| `ThemeForge.Theme` | Moteur de theming (`IThemeService`, `ThemeRevision`), suivis Windows opt-in (`ISystemThemeFollower`, `ISystemAccentFollower`), 16 `ResourceDictionary` de thèmes |
| `ThemeForge.Controls` | 23 contrôles WPF natifs stylés + 9 composites (`Card`, `IconButton`, `Badge`, `Chip`, `ToggleSwitch`, `Avatar`, `SearchBox`, `Toast`, `ToastHost`) |
| `ThemeForge.Studio` | App WPF de démonstration et d'édition live des thèmes |

Les styles natifs sont agrégés par
`ThemeForge.Controls;component/Styles/Studio.xaml`. Les composites suivent la
convention WPF `Themes/Generic.xaml`.

## Variantes incluses

**16 variantes** — `Dracula` MIT canonique, `Drakul` sibling AA-compliant
Apache 2.0, puis 14 palettes originales Apache 2.0 de Julien Bombled. Le set
v6 a été audité en cross-review à trois voix indépendantes sur la palette et
sur les noms.

- **Root** (2) : Dracula, Drakul
- **Dark** (10) : Striga, Cinder, Bracken, Tarn, Mortis, Slate, Voivode,
  Carmilla, Whitby, Vesper
- **Light** (2) : Parchment, Folio
- **Alt** (2) : Wormwood, Sconce

### La paire Dracula / Drakul

Deux variantes Root sont livrées intentionnellement :

- **Dracula** : palette MIT canonique de Zeno Rocha, préservée à l'identique
  pour fidélité historique. Le Comment `#6272A4` ne clear pas WCAG AA
  (Comment/CurrentLine = 1.94:1). C'est un défaut accessibilité du design
  original.
- **Drakul** : sibling Apache 2.0 du Dracula canonique, AA-compliant par un
  lift du Comment vers `#B3BBD6` (Comment/CurrentLine = 4.79:1). Tout le reste
  est byte-identique. Le nom honore Vlad II Dracul, membre de l'Ordre du
  Dragon et père historique de Vlad III.

Tu choisis Dracula si tu veux la palette canonique. Tu choisis Drakul si tu as
besoin d'accessibilité AA stricte sans perdre l'ADN Dracula.

### Geometric Color Palette

Les 14 palettes originales sont **ingénierées**, pas intuitives. Les valeurs
hex sont calculées depuis des cibles déclarées, puis auditées. L'objectif :
garder une identité Dracula pastel/néon, tout en rendant les variantes
prévisibles.

- **Bande Dark uniforme** : les 10 variantes Dark partagent les mêmes 7
  accents. Seul le hue du background change entre variantes.
- **Bande Light uniforme** : les 2 variantes Light partagent un jeu d'accents
  plus sombre, lisible sur des surfaces claires.
- **Alt** : les 2 variantes Alt héritent de la bande Dark et brisent exactement
  un accent pour créer leur signature.
- **WCAG 2.1 AA** : les couples Foreground/Background, Comment/Background et
  Comment/CurrentLine sont vérifiés. Dracula reste la racine historique ; Drakul
  est la réponse AA.

### Attribution des palettes

**`Dracula`** : palette MIT canonique de **Zeno Rocha** (Dracula Theme,
https://draculatheme.com). Elle sert de racine historique au projet.

**`Drakul`** : variante Apache 2.0 de **Julien Bombled**, dérivée de Dracula
avec le seul slot Comment ajusté pour clear WCAG AA.

**Les 14 originales** : `Striga`, `Cinder`, `Bracken`, `Tarn`, `Mortis`,
`Slate`, `Voivode`, `Carmilla`, `Whitby`, `Vesper`, `Parchment`, `Folio`,
`Wormwood`, `Sconce`. Elles sont créées par **Julien Bombled** sous Apache 2.0.

Important : aucun nom ni aucune valeur RGB d'un scheme commercial Dracula n'est
repris. ThemeForge reprend l'ADN visuel Dracula, pas un produit payant.

## Utilisation rapide

### Référencer les projets

Aucun package NuGet ThemeForge n'est publié pour l'instant. Référence les deux
projets depuis ton app WPF consommatrice.

```xml
<ItemGroup>
  <ProjectReference Include="..\ThemeForge\src\ThemeForge.Theme\ThemeForge.Theme.csproj" />
  <ProjectReference Include="..\ThemeForge\src\ThemeForge.Controls\ThemeForge.Controls.csproj" />
</ItemGroup>
```

### Charger un thème au démarrage

Merge les styles de contrôles, puis un thème par défaut. `ThemeService` pourra
ensuite remplacer le thème actif au runtime.

```xml
<ResourceDictionary.MergedDictionaries>
  <ResourceDictionary Source="pack://application:,,,/ThemeForge.Controls;component/Styles/Studio.xaml"/>
  <ResourceDictionary Source="pack://application:,,,/ThemeForge.Theme;component/Themes/Dracula.xaml"/>
</ResourceDictionary.MergedDictionaries>
```

Dans `App.xaml.cs`, enregistre le service avec `Microsoft.Extensions.DependencyInjection`.

```csharp
private ServiceProvider? _services;

protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);
    var services = new ServiceCollection();
    services.AddSingleton<IThemeService>(_ => new ThemeService(this));
    _services = services.BuildServiceProvider();
    _services.GetRequiredService<IThemeService>().ApplyTheme(ThemeNames.Dracula);
}
```

### Basculer de thème au runtime

`ApplyTheme` est idempotent. Appliquer le thème déjà actif ne déclenche rien.

```csharp
themeService.ThemeChanged += (_, e) =>
    Debug.WriteLine($"{e.CurrentTheme} rev {e.Revision}");

themeService.ApplyTheme(ThemeNames.Drakul);
```

### Suivre Windows

Les suivis Windows sont opt-in et ne modifient pas le contrat `IThemeService`.
Expose le même `ThemeService` sous les interfaces nécessaires, puis choisis
toi-même le mapping clair/sombre adapté à ton app.

```csharp
services.AddSingleton<ThemeService>(_ => new ThemeService(this));
services.AddSingleton<IThemeService>(sp => sp.GetRequiredService<ThemeService>());
services.AddSingleton<ISystemThemeFollower>(sp => sp.GetRequiredService<ThemeService>());
services.AddSingleton<ISystemAccentFollower>(sp => sp.GetRequiredService<ThemeService>());
```

```csharp
ISystemThemeFollower follower = _services.GetRequiredService<ISystemThemeFollower>();
follower.EnableSystemFollow(ThemeNames.Folio, ThemeNames.Dracula);

ISystemAccentFollower accentFollower =
    _services.GetRequiredService<ISystemAccentFollower>();
accentFollower.EnableSystemAccentFollow();
```

Un appel manuel à `ApplyTheme` désactive le suivi pour que le choix explicite de
l'utilisateur reste prioritaire. Si Windows ne fournit pas d'état clair/sombre,
ThemeForge conserve le thème courant.

Le suivi de l'accent Windows est séparé de `AccentTint` : la couleur Windows est
arbitraire, alors que `AccentTint` reste une palette discrète ThemeForge. Quand
le suivi d'accent est actif, un appel manuel à `ApplyAccentTint` reprend la
main et désactive ce suivi.

## Lancer Studio

Studio te permet de prévisualiser les 16 variantes, de tester les contrôles, et
d'éditer 24 slots hex : 12 canoniques + 12 sémantiques.

```pwsh
dotnet run --project src/ThemeForge.Studio
```

## Architecture

Inspirée du moteur de theming de [Heimdall.Next](https://github.com/jbombled/Heimdall.Next),
mais construite de zéro pour rester **agnostique de toute app spécifique** :
pas de brushes applicatifs, juste les slots Dracula canoniques + tokens
sémantiques (Background, Surface, Accent, TextPrimary, etc.).

Le swap de `ResourceDictionary` passe par un point unique
(`IThemeService.ApplyTheme`) qui incrémente un compteur `ThemeRevision` **avant**
de lever l'événement `ThemeChanged` — pattern utilisé par les converters et
liaisons one-way pour se ré-évaluer.

## État

Moteur stable v6. ThemeForge shippe aujourd'hui 16 variantes, 23 contrôles WPF
natifs stylés et 9 composites. Le Studio a été audité côté UI Automation après
correctifs, avec les sections, le picker de thèmes et l'éditeur de palette
adressables.

La suite locale compte 164 tests verts : 53 pour `ThemeForge.Theme` et 111 pour
`ThemeForge.Controls`. La CI GitHub Actions est committée ; elle s'activera au
premier push sur une remote GitHub.

Pas encore de package NuGet publié.

## Licence

- Code, styles, framework : Apache 2.0 — Julien Bombled
- 14 palettes originales : Apache 2.0 — Julien Bombled
- Palette Drakul : Apache 2.0 — Julien Bombled
- Palette Dracula : MIT — Zeno Rocha (racine historique)

Voir `NOTICE` pour le détail complet de la philosophie Geometric Color Palette
et la table d'attribution.

## Pour aller plus loin

- `docs/integration-guide.md` : guide d'intégration WPF tiers à venir, voir
  issue à suivre.
