# Hook pre-push local

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

Le hook appelle le script principal :

```pwsh
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File tools/ci-mime.ps1
```

Le script exécute les mêmes gates que la CI :

- vérification du SDK .NET 10
- headers Apache 2.0 sur les fichiers `.cs`
- types explicites dans `src/` et `tests/` (`var` interdit)
- attribution des palettes dans les variants XAML
- `dotnet restore ThemeForge.slnx`
- `dotnet build ThemeForge.slnx --configuration Release --no-restore`
- `dotnet test ThemeForge.slnx --configuration Release --no-build --verbosity normal`

## Bypass d'urgence

Un push peut contourner le hook avec :

```pwsh
git push --no-verify
```

Ce bypass doit rester exceptionnel. Relance le script manuellement ensuite pour
ne pas laisser GitHub Actions découvrir seul une régression locale.

## Limite connue

Le script duplique actuellement les checks PowerShell inline du workflow GitHub
Actions. Si les gates évoluent, il faut maintenir `.github/workflows/build.yml`
et `tools/ci-mime.ps1` ensemble. Une future refactorisation pourra extraire les
checks dans des scripts partagés sous `tools/checks/`.
