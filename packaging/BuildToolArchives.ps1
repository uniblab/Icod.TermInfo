param(
    [ValidateSet('Debug', 'Staging', 'Release')]
    [string]$Configuration = 'Release',

    [string]$OutputDirectory = 'artifacts/tools'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
Push-Location $repositoryRoot
try {
    & bash .github/scripts/build-tool-archives.sh $Configuration $OutputDirectory
    if (0 -ne $LASTEXITCODE) {
        throw "Tool archive production exited with status $LASTEXITCODE."
    }

    & bash .github/scripts/verify-tool-archives.sh $OutputDirectory
    if (0 -ne $LASTEXITCODE) {
        throw "Tool archive verification exited with status $LASTEXITCODE."
    }
} finally {
    Pop-Location
}
