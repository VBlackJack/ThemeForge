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
    Write-Host "[$script:StepCount/6] $Name"
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

try {
    Push-Location $RepositoryRoot
    Assert-DotNetSdk
    Assert-LicenseHeaders
    Assert-ExplicitTypes

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
