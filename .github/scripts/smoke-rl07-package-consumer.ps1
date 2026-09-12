param(
	[Parameter( Mandatory = $true )]
	[string] $ArtifactDirectory,

	[ValidateSet( 'Debug', 'Staging', 'Release' )]
	[string] $Configuration = 'Staging'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = [System.IO.Path]::GetFullPath(
	[System.IO.Path]::Combine( $PSScriptRoot, '..', '..' )
)
$artifactRoot = if ( [System.IO.Path]::IsPathRooted( $ArtifactDirectory ) ) {
	[System.IO.Path]::GetFullPath( $ArtifactDirectory )
} else {
	[System.IO.Path]::GetFullPath(
		[System.IO.Path]::Combine( $repositoryRoot, $ArtifactDirectory )
	)
}
if ( ![System.IO.Directory]::Exists( $artifactRoot ) ) {
	throw "RL07 artifact directory does not exist: $artifactRoot"
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
	'Icod.TermInfo.RL07PackageSmoke.' + [System.Guid]::NewGuid().ToString( 'N' )
)
$projectPath = Join-Path $workRoot 'Icod.TermInfo.Inspection.PackageSmoke.csproj'
$configPath = Join-Path $repositoryRoot '.github/scripts/package-smoke.NuGet.Config'
$previousNugetPackages = $env:NUGET_PACKAGES
$previousArtifactDirectory = $env:ICOD_TERMINFO_ARTIFACT_DIR

try {
	New-Item -ItemType Directory -Path $workRoot -Force | Out-Null
	Copy-Item -LiteralPath (
		Join-Path $repositoryRoot 'tools/inspection-package-smoke/Icod.TermInfo.Inspection.PackageSmoke.csproj'
	) -Destination $projectPath
	Copy-Item -LiteralPath (
		Join-Path $repositoryRoot 'tools/inspection-package-smoke/Program.cs'
	) -Destination $workRoot
	Copy-Item -LiteralPath (
		Join-Path $repositoryRoot 'tools/inspection-package-smoke/RL07LifecyclePackageSmoke.cs'
	) -Destination $workRoot

	$env:ICOD_TERMINFO_ARTIFACT_DIR = $artifactRoot
	$env:NUGET_PACKAGES = Join-Path $workRoot 'packages'

	& dotnet restore $projectPath `
		--configfile $configPath `
		-p:IcodTermInfoInspectionPackageVersion=$version
	if ( 0 -ne $LASTEXITCODE ) {
		throw "RL07 package-only consumer restore failed for Icod.TermInfo.Inspection $version."
	}

	foreach ( $framework in @( 'net8.0', 'net9.0', 'net10.0' ) ) {
		& dotnet run `
			--project $projectPath `
			-c $Configuration `
			-f $framework `
			--no-restore `
			-p:IcodTermInfoInspectionPackageVersion=$version
		if ( 0 -ne $LASTEXITCODE ) {
			throw "RL07 package-only lifecycle consumer failed on $framework."
		}
	}

	Write-Host "RL07 package-only lifecycle consumer passed on net8.0, net9.0, and net10.0 for $version."
} finally {
	$env:NUGET_PACKAGES = $previousNugetPackages
	$env:ICOD_TERMINFO_ARTIFACT_DIR = $previousArtifactDirectory
	if ( Test-Path -LiteralPath $workRoot ) {
		Remove-Item -LiteralPath $workRoot -Recurse -Force
	}
}
