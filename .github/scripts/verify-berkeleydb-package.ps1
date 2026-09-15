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
    $runtimeVersion = (& dotnet msbuild Icod.TermInfo.csproj -nologo -getProperty:PackageVersion).Trim()
    if (0 -ne $LASTEXITCODE -or [string]::IsNullOrWhiteSpace($runtimeVersion)) {
        throw 'Unable to determine Icod.TermInfo PackageVersion.'
    }

    $berkeleyDbVersion = (& dotnet msbuild Icod.TermInfo.BerkeleyDb/Icod.TermInfo.BerkeleyDb.csproj -nologo -getProperty:PackageVersion).Trim()
    if (0 -ne $LASTEXITCODE -or [string]::IsNullOrWhiteSpace($berkeleyDbVersion)) {
        throw 'Unable to determine Icod.TermInfo.BerkeleyDb PackageVersion.'
    }

    if ($berkeleyDbVersion -cne $runtimeVersion) {
        throw "Icod.TermInfo.BerkeleyDb PackageVersion '$berkeleyDbVersion' does not match Runtime '$runtimeVersion'."
    }

    $packagePath = Join-Path $ArtifactDirectory "Icod.TermInfo.BerkeleyDb.$berkeleyDbVersion.nupkg"
    $symbolsPath = Join-Path $ArtifactDirectory "Icod.TermInfo.BerkeleyDb.$berkeleyDbVersion.snupkg"
    if (-not (Test-Path -LiteralPath $packagePath -PathType Leaf)) {
        throw "Icod.TermInfo.BerkeleyDb package not found: $packagePath"
    }
    if (-not (Test-Path -LiteralPath $symbolsPath -PathType Leaf)) {
        throw "Icod.TermInfo.BerkeleyDb symbol package not found: $symbolsPath"
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

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [System.IO.Compression.ZipFile]::OpenRead($packagePath)
    try {
        $entryNames = @($archive.Entries | ForEach-Object FullName)
        foreach ($framework in @('net8.0', 'net9.0', 'net10.0')) {
            $expected = "lib/$framework/Icod.TermInfo.BerkeleyDb.dll"
            if ($entryNames -cnotcontains $expected) {
                throw "Package is missing '$expected'."
            }
        }

        if ($entryNames | Where-Object { $_ -like 'runtimes/*' }) {
            throw 'HDB01 package unexpectedly contains native/runtime-specific assets.'
        }

        $nuspecEntry = $archive.Entries | Where-Object { $_.FullName -like '*.nuspec' } | Select-Object -First 1
        if ($null -eq $nuspecEntry) {
            throw 'HDB01 package does not contain a nuspec.'
        }

        $reader = New-Object System.IO.StreamReader($nuspecEntry.Open())
        try {
            [xml]$nuspec = $reader.ReadToEnd()
        } finally {
            $reader.Dispose()
        }

        $metadata = $nuspec.package.metadata
        if ([string]$metadata.id -cne 'Icod.TermInfo.BerkeleyDb') {
            throw "Unexpected package id '$($metadata.id)'."
        }
        if ([string]$metadata.version -cne $berkeleyDbVersion) {
            throw "Unexpected package version '$($metadata.version)'."
        }
        if ([string]$metadata.license.'#text' -cne 'LGPL-3.0-or-later') {
            throw "Unexpected package license '$($metadata.license.'#text')'."
        }

        $dependencies = @($metadata.dependencies.group.dependency)
        if (1 -ne $dependencies.Count) {
            throw "Expected exactly one package dependency, found $($dependencies.Count)."
        }
        if ([string]$dependencies[0].id -cne 'Icod.TermInfo') {
            throw "Unexpected HDB01 package dependency '$($dependencies[0].id)'."
        }
        if (-not ([string]$dependencies[0].version).Contains($runtimeVersion, [System.StringComparison]::Ordinal)) {
            throw "Runtime dependency version '$($dependencies[0].version)' does not reference '$runtimeVersion'."
        }
    } finally {
        $archive.Dispose()
    }

    Write-Host "Icod.TermInfo.BerkeleyDb $berkeleyDbVersion HDB01 package verification passed."
} finally {
    Pop-Location
}
