# Local pre-push hook

English | [Français](fr/pre-push-hook.md)

ThemeForge includes a versioned `pre-push` hook that runs the local equivalents of the GitHub CI checks to detect analyzer violations, missing license headers, and build or test failures before pushing.

## Enable the hook

Configure each clone:

```pwsh
git config core.hooksPath .githooks
```

Verify the setting:

```pwsh
git config --get core.hooksPath
```

Expected output: `.githooks`.

## Run manually

```pwsh
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File tools/ci-explicit-types.ps1
```

The script checks the .NET 10 SDK, Apache 2.0 headers on C# files, and explicit types in `src/` and `tests/` (`var` is prohibited). It then runs:

```pwsh
dotnet restore ThemeForge.slnx
dotnet build ThemeForge.slnx --configuration Release --no-restore
dotnet test ThemeForge.slnx -m:1 --configuration Release --no-build --verbosity normal
```

The hook also calls `tools/ci-xaml-headers.ps1` to check palette attribution in XAML variants.

## Emergency bypass

```pwsh
git push --no-verify
```

Use this only exceptionally. Run the scripts manually afterwards so GitHub Actions is not the first place a local regression is detected.

## Maintenance

Pull requests, main and tagged builds call `tools/ci-verify.ps1`, which reuses the hook scripts and adds package validation, an isolated generated application, a Studio document round trip and performance budgets. The local hook runs the source, build, test and attribution subset. See [quality checks](quality.md) to run the full pipeline locally.

## Publication gates

The same source, build and test gates run on the tagged source before packaging. Source
files (`.cs` and `.xaml`, excluding build output) must stay within 200 lines. Tests run
with `-m:1` to keep the WPF test assemblies sequential.

After packing the solution and template, `tools/ci-packages.ps1 -PackageDirectory artifacts/packages`
checks the exact NOTICE, LICENSE and `licenses/Dracula-MIT.txt` bytes in every package.
The publication workflow uses `tools/push-packages.ps1` to stop on the first failed package.
