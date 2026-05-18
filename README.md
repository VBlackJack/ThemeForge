# ThemeForge

Framework de thématisation Dracula pour applications **.NET 10 WPF**.

> Pose un thème Dracula propre sur une nouvelle app WPF en 5 minutes,
> pas en 2 jours.

## Pourquoi ?

Chaque nouveau projet WPF redémarre de zéro côté style : couleurs hardcodées,
`ResourceDictionary` recopiés à la main, design tokens incohérents, pas de
mécanique de bascule de thème, et trois mois plus tard chaque app a son propre
dialecte visuel.

ThemeForge fournit un moteur de theming réutilisable, 15 variantes Dracula
prêtes à l'emploi, et une app **Studio** pour les prévisualiser et les ajuster
en direct avant de les exporter dans ton projet cible.

## Composants

| Projet | Rôle |
|---|---|
| `ThemeForge.Theme` | Moteur de theming (`IThemeService`, `ThemeRevision`), 15 ResourceDictionaries Dracula, design tokens partagés |
| `ThemeForge.Controls` | Styles WPF pour les contrôles standard + composites (`Card`, `IconButton`, `ToggleSwitch`) |
| `ThemeForge.Studio` | App WPF de démonstration et d'édition live des thèmes |

## Variantes incluses

**16 variantes** — 1 historique MIT (Dracula) + 1 sibling AA-compliant (Drakul)
+ 14 originales Apache 2.0, groupées en 3 familles. Set v6 — résultat de **5
passes de cross-review indépendantes** (independent reviewers
) avec uniform-Oklab-L* color space. **Convergence finale SHIP** sur
pass 5.

### La paire Dracula / Drakul

Deux variantes Root sont livrées intentionnellement :

- **Dracula** : palette MIT canonique de Zeno Rocha, **préservée à l'identique**
  pour fidélité historique. Le Comment `#6272A4` ne clear pas WCAG AA
  (Comment/CL = 1.94:1) — un défaut accessibilité du design original de 2013.
- **Drakul** : sibling Apache 2.0 du Dracula canonique, **AA-compliant** par
  un simple lift du Comment vers `#B3BBD6` (Comment/CL = 4.79:1). Tout le reste
  est byte-identique. Le nom honore Vlad II Dracul (~1395-1447), membre de
  l'Ordre du Dragon (1408) et père historique de Vlad III Țepeș dont Stoker
  dériva "Dracula".

Tu choisis Dracula si tu veux la palette canonique du roi des thèmes ; tu
choisis Drakul si tu as besoin d'accessibilité AA stricte sans sacrifier
l'esthétique Dracula DNA.

- **Dark** (10) — Striga, Cinder, Bracken, Tarn, Mortis, Slate, Voivode, Carmilla,
  Whitby, Vesper (+ `Dracula` MIT comme racine)
- **Light** (2) — Parchment, Folio
- **Alt** (2) — Wormwood (signature vert viridian), Sconce (signature ambre)

### Geometric Color Palette

Les 14 palettes originales sont **ingénierées**, pas intuitives. Hex valeurs
**calculées depuis des cibles HSL déclarées** (pas reverse-engineered), conformes
à la philosophie *Geometric Color Palette* :

- **Bande Dark uniforme** : les 10 variantes Dark partagent **les mêmes 7 accents
  exactement** (HSL L 0.68 / S 0.65, hues fixés à 186°/135°/27°/333°/282°/0°/60°).
  Seul le **Hue du Background** change entre variantes (rotation 12° → 340° avec
  espacement minimum 20° pour éviter les clumps visuels).
- **Bande Light uniforme** : les 2 Light partagent un second jeu d'accents
  (L 0.32 / S 0.78), assombri par rapport à la v2 pour clear WCAG AA sur les
  fonds clairs L 0.95.
- **Alt** : les 2 Alt héritent de la bande Dark et brisent **exactement un**
  accent à L 0.68 + S 0.82 — même luminance que la base, chroma seule
  différencie. Choix de design préférable à un bump de L qui ferait sortir
  visuellement.
- **WCAG 2.1 AA** : tous les pairs Foreground/Bg, Comment/Bg et Comment/CurrentLine
  vérifiés ≥ 4.5:1. Audit triple an independent reviewer + an independent reviewer + an independent reviewer.

### Attribution des palettes

**`Dracula`** : palette MIT canonique de **Zeno Rocha** (Dracula Theme,
https://draculatheme.com) — racine historique du projet.

**Les 12 autres** (Nosferatu, Carmilla, Whitby, Vesper, Striga, Belfry,
Sepulcher, Mireille, Parchment, Lucent, Wormwood, Gaslight) : créations
originales de **Julien Bombled** sous Apache 2.0. Noms tirés du folklore
vampirique, de la littérature gothique du domaine public (Carmilla 1872 de
Le Fanu, Nosferatu 1922 de Murnau, Dracula 1897 de Stoker), de termes
atmosphériques/architecturaux/latins, ou de l'œuvre publique de Frédéric
Mistral (1859).

**Important** — aucune valeur RGB ni aucun nom n'est repris des schemes
commerciaux Dracula PRO (Pro, Alucard, Blade, Buffy, Lincoln, Morbius, Van
Helsing). Le framework s'inscrit dans l'**ADN Dracula** (palette pastel/néon
sur fond désaturé) sans dépendance juridique avec le produit commercial.

## Utilisation rapide

```xml
<!-- App.xaml du projet consommateur -->
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <ResourceDictionary Source="pack://application:,,,/ThemeForge.Theme;component/Themes/Dracula.xaml" />
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

```csharp
// Au runtime, basculer de thème
themeService.ApplyTheme("Helsing");
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

Pre-alpha. MVP en construction (Sem 20, mai 2026).

## Licence

- **Code et infrastructure** : Apache 2.0 — Julien Bombled (voir `LICENSE`)
- **Code et infrastructure + 12 palettes originales** : Apache 2.0 —
  Julien Bombled
- **Palette `Dracula`** : MIT — Zeno Rocha (racine historique conservée)

Voir `NOTICE` pour le détail complet de la philosophie Geometric Color
Palette et la table d'attribution.
