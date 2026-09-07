# Copyright 2026 Julien Bombled
# Licensed under the Apache License, Version 2.0
#Requires -Version 7.0
<#
.SYNOPSIS
Runs the shared source, build, test, package and consumer gates for PRs and tags.
.PARAMETER OutputDirectory
Fresh artifacts directory used by packaging and the isolated consumer fixture.
.NOTES
Returns normally on success; any gate failure terminates execution. Never publishes.
#>
[CmdletBinding()]
param([Parameter(Mandatory)][ValidateNotNullOrEmpty()][string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
[string]$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
[string]$output = [IO.Path]::GetFullPath($OutputDirectory)
[string]$packages = Join-Path $output 'packages'
if (Test-Path -LiteralPath $packages) { throw 'Use a fresh output directory to avoid mixing package versions.' }
& (Join-Path $PSScriptRoot 'ci-explicit-types.ps1')
& (Join-Path $PSScriptRoot 'ci-xaml-headers.ps1')
& dotnet pack (Join-Path $root 'ThemeForge.slnx') -c Release --no-build --output $packages
if ($LASTEXITCODE -ne 0) { throw 'Solution packaging failed.' }
& dotnet pack (Join-Path $root 'src/ThemeForge.Templates/ThemeForge.Templates.csproj') -c Release --output $packages
if ($LASTEXITCODE -ne 0) { throw 'Template packaging failed.' }
& (Join-Path $PSScriptRoot 'ci-packages.ps1') -PackageDirectory $packages
& dotnet (Join-Path $root 'tests/ThemeForge.Quality/bin/Release/net10.0-windows/ThemeForge.Quality.dll') document `
    (Join-Path $PSScriptRoot 'quality-settings.json') (Join-Path $output 'document')
if ($LASTEXITCODE -ne 0) { throw 'Studio document round trip failed.' }
& (Join-Path $PSScriptRoot 'test-package-consumer.ps1') -PackageDirectory $packages -OutputDirectory $output `
    -PalettePath (Join-Path $output 'document/palette.json')
& dotnet (Join-Path $root 'tests/ThemeForge.Quality/bin/Release/net10.0-windows/ThemeForge.Quality.dll') performance `
    (Join-Path $PSScriptRoot 'quality-settings.json') (Join-Path $output 'performance')
if ($LASTEXITCODE -ne 0) { throw 'Performance budget failed.' }
