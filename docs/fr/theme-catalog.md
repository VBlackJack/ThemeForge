# Catalogue de thèmes

[English](../theme-catalog.md) | [README français](../../README.fr.md)

## Le catalogue de thèmes

Le moteur est agnostique ; le catalogue livré est d'esthétique Dracula, dont
Dracula est l'héritage historique et la variante de référence.

**17 variantes dans les sources actuelles** : `Dracula` MIT canonique, `Drakul` sibling AA-compliant Apache
2.0, puis 14 palettes originales Apache 2.0 de Julien Bombled.

- **Root** (2) : Dracula, Drakul
- **Dark** (11) : Striga, Cinder, Bracken, Tarn, Mortis, Slate, Magellan, Voivode,
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


## Magellan

Magellan ajoute une variante sombre dérivée de l’identité visuelle Magellan au catalogue initial de 16 thèmes. Le fond et l’accent de signature sont adaptés à la marque ; les sept accents fonctionnels conservent la bande sombre. L’écriture XAML et l’adaptation du contraste sont sous Apache 2.0 par Julien Bombled. La marque et ses couleurs restent la propriété de leur titulaire. Voir [NOTICE](../../NOTICE).
