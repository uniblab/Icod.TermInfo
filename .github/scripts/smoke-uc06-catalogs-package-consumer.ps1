param(
    [Parameter(Mandatory = $true)]
    [string]$ArtifactDirectory,

    [ValidateSet('Debug', 'Staging', 'Release')]
    [string]$Configuration = 'Staging'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$artifactRoot = if ([System.IO.Path]::IsPathRooted($ArtifactDirectory)) {
    [System.IO.Path]::GetFullPath($ArtifactDirectory)
} else {
    [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $ArtifactDirectory))
}
if (-not [System.IO.Directory]::Exists($artifactRoot)) {
    throw "UC06 package directory does not exist: $artifactRoot"
}
[xml]$buildProperties = Get-Content -LiteralPath (Join-Path $repositoryRoot 'Directory.Build.props') -Raw
$versionNode = $buildProperties.SelectSingleNode('/Project/PropertyGroup/IcodTermInfoSuiteVersion')
if ($null -eq $versionNode) { throw 'IcodTermInfoSuiteVersion is missing.' }
$version = $versionNode.InnerText
$workRoot = Join-Path ([System.IO.Path]::GetTempPath()) `
    ('Icod.TermInfo.UC06PackageSmoke.' + [System.Guid]::NewGuid().ToString('N'))
$previousNugetPackages = $env:NUGET_PACKAGES
try {
    New-Item -ItemType Directory -Path $workRoot -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $repositoryRoot 'tools/catalogs-package-smoke/Icod.TermInfo.Catalogs.PackageSmoke.csproj') -Destination $workRoot
    Copy-Item -LiteralPath (Join-Path $repositoryRoot 'tools/catalogs-package-smoke/Program.cs') -Destination $workRoot
    $project = Join-Path $workRoot 'Icod.TermInfo.Catalogs.PackageSmoke.csproj'
    $config = Join-Path $workRoot 'NuGet.Config'
    $escaped = [System.Security.SecurityElement]::Escape($artifactRoot)
    $xml = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources><clear /><add key="artifacts" value="$escaped" /></packageSources>
  <packageSourceMapping>
    <packageSource key="artifacts"><package pattern="Icod.TermInfo*" /></packageSource>
  </packageSourceMapping>
</configuration>
"@
    [System.IO.File]::WriteAllText($config, $xml, [System.Text.UTF8Encoding]::new($false))
    $env:NUGET_PACKAGES = Join-Path $workRoot 'packages'
    & dotnet restore $project --configfile $config "-p:IcodTermInfoCatalogsPackageVersion=$version"
    if (0 -ne $LASTEXITCODE) { throw "UC06 isolated package restore failed for $version." }
    & dotnet build $project -c $Configuration --no-restore -m:1 -p:UseSharedCompilation=false "-p:IcodTermInfoCatalogsPackageVersion=$version"
    if (0 -ne $LASTEXITCODE) { throw 'UC06 isolated package build failed.' }
    foreach ($framework in @('net8.0', 'net9.0', 'net10.0')) {
        & dotnet run --project $project -c $Configuration -f $framework --no-build --no-restore "-p:IcodTermInfoCatalogsPackageVersion=$version"
        if (0 -ne $LASTEXITCODE) { throw "UC06 isolated package consumer failed on $framework." }
    }
    Write-Host "UC06 isolated Catalogs package consumer passed on all three frameworks for $version."
} finally {
    $env:NUGET_PACKAGES = $previousNugetPackages
    if (Test-Path -LiteralPath $workRoot) { Remove-Item -LiteralPath $workRoot -Recurse -Force }
}
