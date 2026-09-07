# Hook pre-push local

[English](../pre-push-hook.md) | Français

ThemeForge versionne un hook `pre-push` qui reproduit le workflow CI GitHub
avant chaque push. Il sert à attraper localement les écarts d'analyseurs .NET,
les headers manquants et les régressions de build/test avant que GitHub Actions
ne les bloque.

## Activation

Chaque clone doit activer le chemin de hooks versionné :

```pwsh
git config core.hooksPath .githooks
```

Vérification :

```pwsh
git config --get core.hooksPath
```

La sortie attendue est `.githooks`.

## Exécution manuelle

Le hook appelle d'abord le gate local principal :

```pwsh
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File tools/ci-explicit-types.ps1
```

Ce script exécute les checks locaux suivants :

- vérification du SDK .NET 10
- headers Apache 2.0 sur les fichiers `.cs`
- types explicites dans `src/` et `tests/` (`var` interdit)
- `dotnet restore ThemeForge.slnx`
- `dotnet build ThemeForge.slnx --configuration Release --no-restore`
- `dotnet test ThemeForge.slnx -m:1 --configuration Release --no-build --verbosity normal`

Le hook appelle ensuite `tools/ci-xaml-headers.ps1` pour vérifier
l'attribution des palettes dans les variants XAML.

## Bypass d'urgence

Un push peut contourner le hook avec :

```pwsh
git push --no-verify
```

Ce bypass doit rester exceptionnel. Relance le script manuellement ensuite pour
ne pas laisser GitHub Actions découvrir seul une régression locale.

## Maintenance

Les pull requests, main et les tags appellent `tools/ci-verify.ps1`, qui réutilise
les scripts du hook et ajoute la validation des packages, une application générée
isolée, un aller-retour de document Studio et les budgets de performance. Le hook
local exécute les contrôles de sources, compilation, tests et attribution.
Voir les [contrôles qualité](quality.md) pour lancer toute la chaîne localement.

## Contrôles de publication

Les mêmes contrôles de sources, compilation et tests portent sur le tag avant création
des packages. Les fichiers source (`.cs` et `.xaml`, hors sorties de compilation) doivent
rester dans la limite de 200 lignes. Les tests utilisent `-m:1` pour séquencer les assemblies WPF.

Après création des packages de la solution et du modèle,
`tools/ci-packages.ps1 -PackageDirectory artifacts/packages` vérifie les octets exacts de NOTICE,
LICENSE et `licenses/Dracula-MIT.txt` dans chaque package. Le workflow utilise
`tools/push-packages.ps1` pour s’arrêter au premier échec d’envoi.
