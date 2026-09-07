# Copyright 2026 Julien Bombled
# Licensed under the Apache License, Version 2.0
#Requires -Version 7.0
<#
.SYNOPSIS
Publishes each validated package, stopping immediately on the first native failure.
.DESCRIPTION
Invoked only by the publication workflow after its build, test and package gates.
Never includes the API key in failure messages.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string]$PackageDirectory,
    [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string]$Source,
    [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string]$ApiKey
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
[System.IO.FileInfo[]]$packages = @(Get-ChildItem -LiteralPath $PackageDirectory -Filter *.nupkg | Sort-Object Name)
if ($packages.Count -eq 0) { throw 'No NuGet package found.' }
foreach ($package in $packages) {
    & dotnet nuget push $package.FullName --source $Source --api-key $ApiKey --skip-duplicate
    if ($LASTEXITCODE -ne 0) {
        throw "Package '$($package.Name)' failed to publish (exit $LASTEXITCODE)."
    }
}
