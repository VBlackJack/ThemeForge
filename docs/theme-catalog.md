# Theme catalogue

English | [Français](fr/theme-catalog.md)

[Back to README](../README.md)

The current source catalogue contains 17 variants. The original 16-theme set consists of canonical Dracula, Drakul, and 14 original palettes. Magellan adds a brand-derived dark variant.

| Family | Variants |
|---|---|
| Root | Dracula, Drakul |
| Dark | Striga, Cinder, Bracken, Tarn, Mortis, Slate, Magellan, Voivode, Carmilla, Whitby, Vesper |
| Light | Parchment, Folio |
| Alternative | Wormwood, Sconce |

## Dracula and Drakul

Dracula preserves Zeno Rocha's canonical MIT palette. Its Comment colour `#6272A4` has a 1.94:1 contrast ratio against CurrentLine, below the WCAG AA text threshold.

Drakul is Julien Bombled's Apache 2.0 adaptation, changing Comment to `#B3BBD6` with a 4.79:1 ratio against CurrentLine. The other palette slots remain identical. Its name honours Vlad II Dracul of the Order of the Dragon.

Use Dracula for the canonical palette or Drakul for the adjusted text contrast.

## Geometric colour design

The 14 original palettes derive their hexadecimal values from declared Oklab targets:

- The 10 original dark variants share seven accents and vary the background hue.
- The two light variants share darker accents for light surfaces.
- The alternative variants reuse the dark band and change one signature accent.
- Runtime gates check Foreground/Background, Comment/Background, and Comment/CurrentLine. Canonical Dracula retains its documented historical exception.

Magellan adapts the background hue and signature accent from the Magellan corporate identity, while retaining the functional dark accent band.

## Attribution

- Dracula: canonical MIT palette by Zeno Rocha, [Dracula Theme](https://draculatheme.com).
- Drakul: Apache 2.0 adaptation by Julien Bombled.
- Striga, Cinder, Bracken, Tarn, Mortis, Slate, Voivode, Carmilla, Whitby, Vesper, Parchment, Folio, Wormwood, and Sconce: original Apache 2.0 palettes by Julien Bombled.
- Magellan: Apache 2.0 XAML authoring and contrast engineering by Julien Bombled; the brand and its colours remain the property of their owner.

No commercial Dracula scheme names or RGB palettes are reproduced. [NOTICE](../NOTICE) is the canonical source for attribution, palette methodology, and trademark limitations.
