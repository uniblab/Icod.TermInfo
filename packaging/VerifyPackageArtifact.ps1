param(
    [Parameter(Mandatory = $true)]
    [string]$ArtifactDirectory,

    [ValidateSet('Debug', 'Staging', 'Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if (-not [System.IO.Path]::IsPathRooted($ArtifactDirectory)) {
    $ArtifactDirectory = Join-Path $repositoryRoot $ArtifactDirectory
}
$ArtifactDirectory = [System.IO.Path]::GetFullPath($ArtifactDirectory)
$runningOnWindows = [System.Environment]::OSVersion.Platform -eq [System.PlatformID]::Win32NT

Push-Location $repositoryRoot
try {
    if ($runningOnWindows) {
        & cmd /d /c .github\scripts\verify-release-package.cmd $ArtifactDirectory $Configuration
    } else {
        & bash .github/scripts/verify-release-package.sh $ArtifactDirectory $Configuration
    }
    if (0 -ne $LASTEXITCODE) {
        throw "Package artifact verification exited with status $LASTEXITCODE."
    }

    & ./.github/scripts/smoke-rl07-package-consumer.ps1 `
        -ArtifactDirectory $ArtifactDirectory `
        -Configuration $Configuration
    if (0 -ne $LASTEXITCODE) {
        throw "RL07 package-only lifecycle consumer exited with status $LASTEXITCODE."
    }

    & ./.github/scripts/smoke-pg07-placement-interop.ps1 `
        -ArtifactDirectory $ArtifactDirectory `
        -Configuration $Configuration
    if (0 -ne $LASTEXITCODE) {
        throw "PG07 package-only placement interoperability consumer exited with status $LASTEXITCODE."
    }

    $lifecycleSampleProject = Join-Path `
        $repositoryRoot `
        'samples/Icod.TermInfo.PersistentRasterLifecycle.Sample/Icod.TermInfo.PersistentRasterLifecycle.Sample.csproj'
    & dotnet restore $lifecycleSampleProject
    if (0 -ne $LASTEXITCODE) {
        throw 'RL07 persistent-raster lifecycle sample restore failed.'
    }

    foreach ($framework in @('net8.0', 'net9.0', 'net10.0')) {
        & dotnet run `
            --project $lifecycleSampleProject `
            -c $Configuration `
            -f $framework `
            --no-restore
        if (0 -ne $LASTEXITCODE) {
            throw "RL07 persistent-raster lifecycle sample failed on $framework."
        }
    }

    $placementSampleProject = Join-Path `
        $repositoryRoot `
        'samples/Icod.TermInfo.PersistentRasterPlacement.Sample/Icod.TermInfo.PersistentRasterPlacement.Sample.csproj'
    & dotnet restore $placementSampleProject
    if (0 -ne $LASTEXITCODE) {
        throw 'PG07 persistent-raster placement sample restore failed.'
    }

    foreach ($framework in @('net8.0', 'net9.0', 'net10.0')) {
        & dotnet run `
            --project $placementSampleProject `
            -c $Configuration `
            -f $framework `
            --no-restore
        if (0 -ne $LASTEXITCODE) {
            throw "PG07 persistent-raster placement sample failed on $framework."
        }
    }

    $inspectionApiManifest = Join-Path `
        $ArtifactDirectory `
        'Icod.TermInfo.Inspection.current-public-api.txt'
    & dotnet run `
        --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj `
        -c $Configuration `
        --no-build `
        -- `
        --write `
        $inspectionApiManifest `
        "Icod.TermInfo.Inspection/bin/$Configuration/net10.0/Icod.TermInfo.Inspection.dll"
    if (0 -ne $LASTEXITCODE) {
        throw 'RL08 Inspection public API manifest generation failed.'
    }
} finally {
    Pop-Location
}
