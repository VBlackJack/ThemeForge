# Copyright 2026 Julien Bombled
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

<#
.SYNOPSIS
    Verifies attribution headers on every versioned XAML file in ThemeForge.

.DESCRIPTION
    Two attribution contracts are enforced:
      - src/ThemeForge.Theme/Themes/Dracula.xaml: MIT Zeno Rocha (canonical palette).
      - Every other XAML under src/ThemeForge.{Theme,Controls}/**: Apache 2.0 Julien Bombled.

    Exit code 0 on full compliance, 1 otherwise. Offenders are listed with a
    copy-paste-ready suggested header.

.EXAMPLE
    pwsh -NoLogo -NoProfile -File tools/ci-xaml-headers.ps1
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

[string] $RepoRoot = Split-Path -Parent $PSScriptRoot
[string] $DraculaFile = Join-Path $RepoRoot 'src/ThemeForge.Theme/Themes/Dracula.xaml'

[string[]] $JulienScopes = @(
    'src/ThemeForge.Theme/Themes/*.xaml',
    'src/ThemeForge.Theme/Themes/Shared/*.xaml',
    'src/ThemeForge.Controls/Themes/*.xaml',
    'src/ThemeForge.Controls/Styles/*.xaml'
)

[regex] $JulienCopyrightPattern = [regex]::new(
    'Copyright\s+\d{4}\s+Julien\s+Bombled',
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
)
[regex] $JulienLicensePattern = [regex]::new(
    'Apache License,\s+Version\s+2\.0',
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
)
[regex] $DraculaAuthorPattern = [regex]::new(
    'Zeno\s+Rocha',
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
)
[regex] $DraculaLicensePattern = [regex]::new(
    'MIT\s+License',
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
)

function Write-Info {
    param([string] $Message)
    Write-Host "[INFO] $Message"
}

function Write-Err {
    param([string] $Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Get-FileContent {
    param([string] $Path)
    return Get-Content -LiteralPath $Path -Raw -ErrorAction Stop
}

function Test-DraculaHeader {
    param([string] $Content)
    return $DraculaAuthorPattern.IsMatch($Content) -and $DraculaLicensePattern.IsMatch($Content)
}

function Test-JulienHeader {
    param([string] $Content)
    return $JulienCopyrightPattern.IsMatch($Content) -and $JulienLicensePattern.IsMatch($Content)
}

[System.Collections.Generic.List[string]] $Offenders = [System.Collections.Generic.List[string]]::new()

if (-not (Test-Path -LiteralPath $DraculaFile)) {
    Write-Err "Missing required file: $DraculaFile"
    exit 1
}

[string] $DraculaContent = Get-FileContent -Path $DraculaFile
if (-not (Test-DraculaHeader -Content $DraculaContent)) {
    $Offenders.Add("$DraculaFile (expected MIT Zeno Rocha header)")
}
Write-Info "Checked Dracula.xaml under MIT contract."

[System.Collections.Generic.HashSet[string]] $JulienFiles =
    [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

foreach ($scope in $JulienScopes) {
    [string] $absoluteScope = Join-Path $RepoRoot $scope
    Get-ChildItem -Path $absoluteScope -File -ErrorAction SilentlyContinue | ForEach-Object {
        [void] $JulienFiles.Add($_.FullName)
    }
}

[string] $DraculaAbsolute = (Resolve-Path -LiteralPath $DraculaFile).Path
[void] $JulienFiles.Remove($DraculaAbsolute)

foreach ($file in $JulienFiles) {
    [string] $content = Get-FileContent -Path $file
    if (-not (Test-JulienHeader -Content $content)) {
        $Offenders.Add("$file (expected Apache 2.0 Julien Bombled header)")
    }
}
Write-Info "Checked $($JulienFiles.Count) XAML file(s) under Apache 2.0 Julien contract."

if ($Offenders.Count -gt 0) {
    Write-Err "XAML attribution gate FAILED. Offending files:"
    foreach ($entry in $Offenders) {
        Write-Err "  - $entry"
    }
    Write-Host ""
    Write-Host "Suggested Apache 2.0 Julien header (paste inside <!-- ... --> at top of XAML):"
    Write-Host @"
<!--
    Copyright $((Get-Date).Year) Julien Bombled

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

        http://www.apache.org/licenses/LICENSE-2.0
-->
"@
    Write-Host ""
    Write-Host "Suggested Dracula.xaml MIT header (must mention Zeno Rocha + MIT License):"
    Write-Host @"
<!--
    Dracula palette (canonical) - Copyright (c) Zeno Rocha, MIT License.
    Source: https://github.com/dracula/dracula-theme/blob/master/LICENSE
-->
"@
    exit 1
}

[int] $totalChecked = $JulienFiles.Count + 1
Write-Info "XAML attribution gate PASSED ($totalChecked file(s) compliant)."
exit 0
