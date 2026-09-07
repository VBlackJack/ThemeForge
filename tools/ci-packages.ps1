# Copyright 2026 Julien Bombled
# Licensed under the Apache License, Version 2.0
#Requires -Version 7.0
<#
.SYNOPSIS
Verifies that each NuGet package contains the exact repository attribution files.
#>
[CmdletBinding()]
param([Parameter(Mandatory)][ValidateNotNullOrEmpty()][string]$PackageDirectory)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
[string]$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
[string[]]$requiredPaths = @('NOTICE', 'LICENSE', 'licenses/Dracula-MIT.txt')
[System.IO.FileInfo[]]$packages = @(Get-ChildItem -LiteralPath $PackageDirectory -Filter *.nupkg)
if ($packages.Count -eq 0) { throw 'No NuGet package found.' }
foreach ($package in $packages) {
    $archive = [System.IO.Compression.ZipFile]::OpenRead($package.FullName)
    try {
        foreach ($entryPath in $requiredPaths) {
            $entry = $archive.GetEntry($entryPath)
            if ($null -eq $entry) { throw "Missing '$entryPath' in '$($package.Name)'." }
            $stream = $entry.Open()
            try {
                [string]$actual = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
            }
            finally { $stream.Dispose() }
            [string]$expected = (Get-FileHash -LiteralPath (Join-Path $repositoryRoot $entryPath) -Algorithm SHA256).Hash
            if ($actual -ne $expected) { throw "Attribution mismatch '$entryPath' in '$($package.Name)'." }
        }
    }
    finally { $archive.Dispose() }
}
Write-Output "Verified attribution in $($packages.Count) packages."
