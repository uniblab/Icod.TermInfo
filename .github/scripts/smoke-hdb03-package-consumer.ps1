param(
	[Parameter( Mandatory = $true, Position = 0 )]
	[string] $ArtifactDirectory,

	[ValidateSet( 'Debug', 'Staging', 'Release' )]
	[string] $Configuration = 'Staging'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryRoot = [System.IO.Path]::GetFullPath(
	[System.IO.Path]::Combine( $scriptDirectory, '..', '..' )
)
$artifactRoot = if ( [System.IO.Path]::IsPathRooted( $ArtifactDirectory ) ) {
	[System.IO.Path]::GetFullPath( $ArtifactDirectory )
} else {
	[System.IO.Path]::GetFullPath(
		[System.IO.Path]::Combine( $repositoryRoot, $ArtifactDirectory )
	)
}
[xml] $buildProperties = Get-Content -LiteralPath (
	Join-Path $repositoryRoot 'Directory.Build.props'
) -Raw
$versionNode = $buildProperties.SelectSingleNode(
	'/Project/PropertyGroup/IcodTermInfoSuiteVersion'
)
if ( $null -eq $versionNode ) {
	throw 'Directory.Build.props does not declare IcodTermInfoSuiteVersion.'
}
$version = $versionNode.InnerText
$workRoot = Join-Path (
	[System.IO.Path]::GetTempPath()
) (
	'Icod.TermInfo.HDB03PackageSmoke.' + [System.Guid]::NewGuid().ToString( 'N' )
)
$previousNugetPackages = $env:NUGET_PACKAGES

try {
	New-Item -ItemType Directory -Path $workRoot -Force | Out-Null
	Copy-Item -LiteralPath (
		Join-Path $repositoryRoot 'tools/berkeleydb-package-smoke/Icod.TermInfo.BerkeleyDb.PackageSmoke.csproj'
	) -Destination $workRoot
	Copy-Item -LiteralPath (
		Join-Path $repositoryRoot 'tools/berkeleydb-package-smoke/Program.cs'
	) -Destination $workRoot

	$projectPath = Join-Path $workRoot 'Icod.TermInfo.BerkeleyDb.PackageSmoke.csproj'
	$configPath = Join-Path $workRoot 'NuGet.Config'
	$escapedArtifactRoot = [System.Security.SecurityElement]::Escape( $artifactRoot )
	$config = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="artifacts" value="$escapedArtifactRoot" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="artifacts">
      <package pattern="Icod.TermInfo*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
"@
	[System.IO.File]::WriteAllText(
		$configPath,
		$config,
		[System.Text.UTF8Encoding]::new( $false )
	)
	$env:NUGET_PACKAGES = Join-Path $workRoot 'packages'

	& dotnet restore $projectPath --configfile $configPath "-p:IcodTermInfoBerkeleyDbPackageVersion=$version"
	if ( 0 -ne $LASTEXITCODE ) {
		throw "HDB03 package consumer restore failed for Icod.TermInfo.BerkeleyDb $version."
	}

	foreach ( $framework in @( 'net8.0', 'net9.0', 'net10.0' ) ) {
		& dotnet run --project $projectPath -c $Configuration -f $framework --no-restore "-p:IcodTermInfoBerkeleyDbPackageVersion=$version"
		if ( 0 -ne $LASTEXITCODE ) {
			throw "HDB03 package consumer failed on $framework."
		}
	}

	Write-Host "HDB03 isolated BerkeleyDb package consumer passed on net8.0, net9.0, and net10.0 for $version."
}
finally {
	$env:NUGET_PACKAGES = $previousNugetPackages
	if ( Test-Path -LiteralPath $workRoot ) {
		Remove-Item -LiteralPath $workRoot -Recurse -Force
	}
}
