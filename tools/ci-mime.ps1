# Copyright 2026 Julien Bombled
# Licensed under the Apache License, Version 2.0

#Requires -Version 7.0
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$script:StepCount = 0
$script:StartedAt = Get-Date
$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$SolutionPath = Join-Path $RepositoryRoot 'ThemeForge.slnx'

function Write-Section {
    param([Parameter(Mandatory)][string]$Name)

    $script:StepCount++
    Write-Host ''
    Write-Host "[$script:StepCount/7] $Name"
}

function Invoke-Native {
    param(
        [Parameter(Mandatory)][string]$FilePath,
        [Parameter(ValueFromRemainingArguments)][string[]]$Arguments
    )

    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "'$FilePath $($Arguments -join ' ')' failed with exit code $LASTEXITCODE."
    }
}

function Assert-DotNetSdk {
    Write-Section 'Verify .NET 10 SDK'

    $sdks = & dotnet --list-sdks
    if ($LASTEXITCODE -ne 0) {
        throw 'dotnet --list-sdks failed.'
    }

    if (-not ($sdks | Where-Object { $_ -match '^10\.0\.' })) {
        throw 'The .NET 10 SDK is required. Install an SDK matching 10.0.x.'
    }

    Write-Host 'Found .NET 10 SDK.'
}

function Assert-LicenseHeaders {
    Write-Section 'Verify Apache 2.0 headers (.cs files)'

    $missing = @()
    Get-ChildItem -Path (Join-Path $RepositoryRoot 'src'), (Join-Path $RepositoryRoot 'tests') -Recurse -Filter *.cs |
        Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } |
        ForEach-Object {
            $content = Get-Content $_.FullName -Raw
            if ($content -notmatch 'Copyright \d{4} Julien Bombled' -or
                $content -notmatch 'Apache License, Version 2\.0') {
                $missing += $_.FullName
            }
        }

    if ($missing.Count -gt 0) {
        Write-Host "Missing Apache 2.0 header in $($missing.Count) file(s):"
        $missing | ForEach-Object { Write-Host "  - $_" }
        throw 'Apache 2.0 header gate failed.'
    }

    Write-Host 'All .cs files have proper Apache 2.0 headers.'
}

function Assert-ExplicitTypes {
    Write-Section 'Verify explicit type declarations (.cs files)'

    $violations = @()
    Get-ChildItem -Path (Join-Path $RepositoryRoot 'src'), (Join-Path $RepositoryRoot 'tests') -Recurse -Filter *.cs |
        Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } |
        ForEach-Object {
            $lineNumber = 0
            foreach ($line in Get-Content $_.FullName) {
                $lineNumber++
                if ($line -match '^\s*var\s+' -or
                    $line -match 'foreach\s*\(\s*var\s+' -or
                    $line -match '\bout\s+var\s+') {
                    $violations += [PSCustomObject]@{
                        Path = $_.FullName
                        Line = $lineNumber
                        Text = $line.Trim()
                    }
                }
            }
        }

    if ($violations.Count -gt 0) {
        Write-Host "Explicit type declaration gate found $($violations.Count) var usage(s):"
        foreach ($violation in $violations) {
            Write-Host "  - $($violation.Path):$($violation.Line): $($violation.Text)"
        }
        throw 'Explicit type declaration gate failed.'
    }

    Write-Host 'No var declarations found in src/ or tests/.'
}

function Assert-PaletteHeaders {
    Write-Section 'Verify palette attribution headers (theme XAML variants)'

    $invalid = @()
    $themeRoot = Join-Path $RepositoryRoot 'src/ThemeForge.Theme/Themes'

    Get-ChildItem -Path $themeRoot -Filter *.xaml -File | ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        $headerMatch = [regex]::Match($content, '^\s*<!--([\s\S]*?)-->')
        $errors = @()

        if (-not $headerMatch.Success) {
            $errors += 'missing leading XML header comment'
        }
        else {
            $header = $headerMatch.Value
            if ($header -notmatch 'Copyright \d{4} Julien Bombled') {
                $errors += 'missing Julien Bombled copyright'
            }
            if ($header -notmatch 'Palette attribution:') {
                $errors += 'missing Palette attribution marker'
            }
            if ($header -notmatch 'NOTICE') {
                $errors += 'missing NOTICE reference'
            }

            switch ($_.Name) {
                'Dracula.xaml' {
                    if ($header -notmatch 'Zeno Rocha' -or
                        $header -notmatch 'https://draculatheme\.com' -or
                        $header -notmatch 'MIT License') {
                        $errors += 'Dracula must attribute Zeno Rocha, draculatheme.com, and MIT'
                    }
                }
                'Drakul.xaml' {
                    if ($header -notmatch 'derived from the canonical Dracula Theme palette' -or
                        $header -notmatch 'Zeno Rocha' -or
                        $header -notmatch 'Comment slot lifted') {
                        $errors += 'Drakul must document its Dracula-derived palette origin'
                    }
                }
                default {
                    if ($header -notmatch 'original palette by Julien Bombled, Apache 2\.0') {
                        $errors += 'original variants must attribute Julien Bombled under Apache 2.0'
                    }
                }
            }
        }

        if ($errors.Count -gt 0) {
            $invalid += [PSCustomObject]@{
                Path = $_.FullName
                Errors = $errors
            }
        }
    }

    if ($invalid.Count -gt 0) {
        Write-Host "Invalid palette attribution header in $($invalid.Count) theme XAML file(s):"
        foreach ($entry in $invalid) {
            Write-Host "  - $($entry.Path): $($entry.Errors -join '; ')"
        }
        throw 'Palette attribution header gate failed.'
    }

    Write-Host 'All theme XAML variants have explicit palette attribution headers.'
}

try {
    Push-Location $RepositoryRoot
    Assert-DotNetSdk
    Assert-LicenseHeaders
    Assert-ExplicitTypes
    Assert-PaletteHeaders

    Write-Section 'Restore'
    Invoke-Native dotnet restore $SolutionPath

    Write-Section 'Build (Release)'
    Invoke-Native dotnet build $SolutionPath --configuration Release --no-restore

    Write-Section 'Test (Release)'
    Invoke-Native dotnet test $SolutionPath --configuration Release --no-build --verbosity normal

    $elapsed = (Get-Date) - $script:StartedAt
    Write-Host ''
    Write-Host ("CI mimic passed in {0:mm\:ss}." -f $elapsed)
}
finally {
    Pop-Location
}
