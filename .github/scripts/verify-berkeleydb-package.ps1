param(
    [Parameter(Mandatory = $true)]
    [string]$ArtifactDirectory,

    [ValidateSet('Debug', 'Staging', 'Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
if (-not [System.IO.Path]::IsPathRooted($ArtifactDirectory)) {
    $ArtifactDirectory = Join-Path $repositoryRoot $ArtifactDirectory
}
$ArtifactDirectory = [System.IO.Path]::GetFullPath($ArtifactDirectory)

Push-Location $repositoryRoot
try {
    & dotnet run `
        --project tools/berkeleydb-package-verifier/Icod.TermInfo.BerkeleyDb.PackageVerifier.csproj `
        -c $Configuration `
        --no-build `
        -- `
        $ArtifactDirectory
    if (0 -ne $LASTEXITCODE) {
        throw "HDB08 BerkeleyDb package verification exited with status $LASTEXITCODE."
    }

    $publicApiProject = 'tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj'
    & dotnet run `
        --project $publicApiProject `
        -c $Configuration `
        --no-build `
        -- `
        --compare `
        "Icod.TermInfo.BerkeleyDb/bin/$Configuration/net8.0/Icod.TermInfo.BerkeleyDb.dll" `
        "Icod.TermInfo.BerkeleyDb/bin/$Configuration/net9.0/Icod.TermInfo.BerkeleyDb.dll"
    if (0 -ne $LASTEXITCODE) {
        throw 'Icod.TermInfo.BerkeleyDb net8.0/net9.0 public API comparison failed.'
    }

    & dotnet run `
        --project $publicApiProject `
        -c $Configuration `
        --no-build `
        -- `
        --compare `
        "Icod.TermInfo.BerkeleyDb/bin/$Configuration/net8.0/Icod.TermInfo.BerkeleyDb.dll" `
        "Icod.TermInfo.BerkeleyDb/bin/$Configuration/net10.0/Icod.TermInfo.BerkeleyDb.dll"
    if (0 -ne $LASTEXITCODE) {
        throw 'Icod.TermInfo.BerkeleyDb net8.0/net10.0 public API comparison failed.'
    }
} finally {
    Pop-Location
}
