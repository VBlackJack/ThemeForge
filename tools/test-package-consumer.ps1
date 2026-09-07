# Copyright 2026 Julien Bombled
# Licensed under the Apache License, Version 2.0
#Requires -Version 7.0
<#
.SYNOPSIS
Creates a disposable tf-wpf consumer using only freshly packed ThemeForge packages.
.DESCRIPTION
Uses isolated template settings, NuGet cache, configuration and preferences. Runs the
real template startup twice to check palette application and persisted restoration.
.PARAMETER PackageDirectory
Directory containing all four local packages of one version.
.PARAMETER OutputDirectory
Parent directory for a new isolated fixture and its reports. Existing fixtures are preserved.
.PARAMETER NuGetSource
Source used only for non-ThemeForge dependencies.
.NOTES
Exit 0 indicates success; terminating errors fail the CI step. No publication occurs.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string]$PackageDirectory,
    [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string]$OutputDirectory,
    [Parameter()][ValidateNotNullOrEmpty()][string]$NuGetSource = 'https://api.nuget.org/v3/index.json',
    [Parameter()][string]$PalettePath,
    [Parameter()][ValidateRange(10, 300)][int]$TimeoutSeconds = 60
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
function Invoke-Checked {
    param([Parameter(ValueFromRemainingArguments)][string[]]$Arguments)
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet $($Arguments[0]) failed with exit $LASTEXITCODE." }
}
[string]$repository = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
[string]$packages = (Resolve-Path -LiteralPath $PackageDirectory).Path
[string]$fixture = Join-Path ([IO.Path]::GetFullPath($OutputDirectory)) ('consumer-' + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $fixture
[IO.FileInfo[]]$templates = @(Get-ChildItem -LiteralPath $packages -Filter 'ThemeForge.Templates.*.nupkg')
if ($templates.Count -ne 1) { throw 'Exactly one template package is required.' }
[string]$hive = Join-Path $fixture 'template-hive'
[string]$project = Join-Path $fixture 'app'
[string]$originalPackages = $env:NUGET_PACKAGES
$env:NUGET_PACKAGES = Join-Path $fixture 'packages'
try {
    Invoke-Checked -Arguments @('new', 'install', $templates[0].FullName, '--debug:custom-hive', $hive)
    Invoke-Checked -Arguments @('new', 'tf-wpf', '-n', 'ThemeForge.PackageConsumer', '-o', $project, '--debug:custom-hive', $hive)
    [xml]$config = '<configuration><packageSources><clear/></packageSources><packageSourceMapping/></configuration>'
    foreach ($source in @(@{ Key = 'local'; Value = $packages; Pattern = 'ThemeForge.*' }, @{ Key = 'nuget'; Value = $NuGetSource; Pattern = '*' })) {
        $node = $config.CreateElement('add'); $node.SetAttribute('key', $source.Key); $node.SetAttribute('value', $source.Value)
        $null = $config.SelectSingleNode('/configuration/packageSources').AppendChild($node)
        $mapping = $config.CreateElement('packageSource'); $mapping.SetAttribute('key', $source.Key)
        $pattern = $config.CreateElement('package'); $pattern.SetAttribute('pattern', $source.Pattern)
        $null = $mapping.AppendChild($pattern); $null = $config.SelectSingleNode('/configuration/packageSourceMapping').AppendChild($mapping)
    }
    [string]$nugetConfig = Join-Path $fixture 'NuGet.Config'; $config.Save($nugetConfig)
    if ([string]::IsNullOrWhiteSpace($PalettePath)) { $PalettePath = Join-Path $repository 'samples/palettes/SampleFolio.json' }
    Copy-Item -LiteralPath $PalettePath -Destination (Join-Path $fixture 'palette.json')
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'fixtures/ConsumerSmoke.cs') -Destination (Join-Path $project 'ConsumerSmoke.cs')
    [string]$startupPath = Join-Path $project 'App.xaml.cs'
    [string]$startup = [IO.File]::ReadAllText($startupPath)
    $startup = $startup.Replace('protected override void OnStartup', 'protected override async void OnStartup')
    $startup = $startup.Replace('AppThemeConfig themeConfig = new AppThemeConfig();', @'
ThemeForge.Theme.Palettes.ThemePalette palette = await ThemeForge.Theme.Palettes.PaletteJson.LoadAsync(
            System.IO.Path.Combine(e.Args[0], "palette.json"));
        AppThemeConfig themeConfig = new AppThemeConfig();
'@)
    $startup = $startup.Replace('options.ApplicationName = "ThemeForge.PackageConsumer";', @'
options.Palettes = new[] { palette };
            options.PreferenceStore = new ThemeForge.Theme.Persistence.JsonThemePreferenceStore(
                System.IO.Path.Combine(e.Args[0], "preferences.json"));
'@)
    if ($startup -notmatch 'options.Palettes') { throw 'Template bootstrap fixture insertion failed.' }
    [IO.File]::WriteAllText($startupPath, $startup)
    [string]$windowPath = Join-Path $project 'MainWindow.xaml'
    [string]$window = [IO.File]::ReadAllText($windowPath).Replace('WindowStartupLocation="CenterScreen"', 'WindowStartupLocation="Manual"')
    $window = $window.Replace('<Window', '<Window ShowActivated="False" ShowInTaskbar="False" Left="-10000" Top="-10000"')
    [IO.File]::WriteAllText($windowPath, $window)
    [string]$consumer = Join-Path $project 'ThemeForge.PackageConsumer.csproj'
    Invoke-Checked -Arguments @('restore', $consumer, '--configfile', $nugetConfig, '--no-cache')
    Invoke-Checked -Arguments @('build', $consumer, '-c', 'Release', '--no-restore')
    [string]$executable = Join-Path $project 'bin/Release/net10.0-windows/ThemeForge.PackageConsumer.exe'
    foreach ($phase in @('apply', 'restore')) {
        $process = Start-Process -FilePath $executable -ArgumentList @('"' + $fixture + '"', $phase, $TimeoutSeconds) -PassThru -WindowStyle Hidden
        if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
            Stop-Process -Id $process.Id -Force; throw "Consumer phase '$phase' timed out."
        }
        if ($process.ExitCode -ne 0 -or -not (Test-Path -LiteralPath (Join-Path $fixture ($phase + '.json')))) {
            throw "Consumer phase '$phase' failed; inspect $fixture."
        }
    }
    [string[]]$corePackages = @('themeforge.theme', 'themeforge.controls', 'themeforge.theme.dependencyinjection')
    foreach ($id in $corePackages) {
        [IO.FileInfo[]]$metadata = @(Get-ChildItem -LiteralPath (Join-Path $env:NUGET_PACKAGES $id) -Recurse -Force -Filter '.nupkg.metadata')
        if ($metadata.Count -ne 1) { throw "Unexpected restored package set for $id." }
        $origin = Get-Content -LiteralPath $metadata[0].FullName -Raw | ConvertFrom-Json
        if ($origin.source -ne $packages) { throw "$id was not restored from the local package directory." }
    }
    Write-Output "Package consumer passed both startup phases: $fixture"
}
finally { $env:NUGET_PACKAGES = $originalPackages }
