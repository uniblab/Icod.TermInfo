param(
    [ValidateSet('Debug', 'Staging', 'Release')]
    [string]$Configuration = 'Release',

    [switch]$BuildToolArchives
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

& (Join-Path $PSScriptRoot 'Invoke-Build.ps1') `
    -Section all `
    -Configuration $Configuration

if ($BuildToolArchives) {
    & (Join-Path $PSScriptRoot 'BuildToolArchives.ps1') `
        -Configuration $Configuration `
        -OutputDirectory 'artifacts/tools'
}
