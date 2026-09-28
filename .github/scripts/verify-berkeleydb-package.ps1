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
    foreach ($framework in @('net8.0', 'net9.0', 'net10.0')) {
        & dotnet run --project $publicApiProject -c $Configuration --no-build -- --check `
            'docs/1.17.0-BERKELEYDB-PUBLIC-API-BASELINE.txt' `
            "Icod.TermInfo.BerkeleyDb/bin/$Configuration/$framework/Icod.TermInfo.BerkeleyDb.dll"
        if (0 -ne $LASTEXITCODE) { throw "UC07 BerkeleyDb $framework complete API freeze failed." }
    }
    & dotnet run `
        --project $publicApiProject `
        -c $Configuration `
        --no-build `
        -- `
        --write `
        (Join-Path $ArtifactDirectory 'uc03-berkeleydb-current-api.txt') `
        "Icod.TermInfo.BerkeleyDb/bin/$Configuration/net10.0/Icod.TermInfo.BerkeleyDb.dll"
    if (0 -ne $LASTEXITCODE) { throw 'UC03 BerkeleyDb current API generation failed.' }
    & dotnet run --project tools/berkeleydb-package-verifier/Icod.TermInfo.BerkeleyDb.PackageVerifier.csproj `
        -c $Configuration --no-build -- --reconstruct-uc03 `
        (Join-Path $ArtifactDirectory 'uc03-berkeleydb-current-api.txt') `
        (Join-Path $ArtifactDirectory 'uc03-berkeleydb-reconstructed-api.txt')
    if (0 -ne $LASTEXITCODE) { throw 'UC03 BerkeleyDb exact additive API reconstruction failed.' }
    $normalizeApi = { param($text) $text.Replace("`r`n", "`n").Replace("`r", "`n").TrimEnd("`n") + "`n" }
    $historicalApi = & $normalizeApi ([System.IO.File]::ReadAllText((Join-Path $repositoryRoot 'docs/1.16.0-BERKELEYDB-PUBLIC-API-BASELINE.txt')))
    $reconstructedApi = & $normalizeApi ([System.IO.File]::ReadAllText((Join-Path $ArtifactDirectory 'uc03-berkeleydb-reconstructed-api.txt')))
    if ($historicalApi -cne $reconstructedApi) { throw 'Icod.TermInfo.BerkeleyDb public API differs from the frozen 1.16 manifest.' }

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

    $sampleProject =
        'samples/Icod.TermInfo.BerkeleyDb.Sample/Icod.TermInfo.BerkeleyDb.Sample.csproj'
    foreach ($framework in @('net8.0', 'net9.0', 'net10.0')) {
        & dotnet run `
            --project $sampleProject `
            -c $Configuration `
            -f $framework `
            --no-build
        if (0 -ne $LASTEXITCODE) {
            throw "Icod.TermInfo.BerkeleyDb deterministic sample failed on $framework."
        }
    }
} finally {
    Pop-Location
}
