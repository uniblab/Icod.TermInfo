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
	throw "RE07 artifact directory does not exist: $artifactRoot"
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
	'Icod.TermInfo.RE07RuntimeEvidence.' + [System.Guid]::NewGuid().ToString( 'N' )
)
$projectPath = Join-Path $workRoot 'Icod.TermInfo.RuntimeEvidence.PackageSmoke.csproj'
$configPath = Join-Path $repositoryRoot '.github/scripts/package-smoke-re07.NuGet.Config'
$previousNugetPackages = $env:NUGET_PACKAGES
$previousArtifactDirectory = $env:ICOD_TERMINFO_ARTIFACT_DIR

try {
	New-Item -ItemType Directory -Path $workRoot -Force | Out-Null
	Copy-Item -LiteralPath (
		Join-Path $repositoryRoot 'tools/runtime-evidence-package-smoke/Icod.TermInfo.RuntimeEvidence.PackageSmoke.csproj'
	) -Destination $projectPath
	Copy-Item -LiteralPath (
		Join-Path $repositoryRoot 'tools/runtime-evidence-package-smoke/Program.cs'
	) -Destination $workRoot

	$env:ICOD_TERMINFO_ARTIFACT_DIR = $artifactRoot
	$env:NUGET_PACKAGES = Join-Path $workRoot 'packages'

	& dotnet restore $projectPath `
		--configfile $configPath `
		-p:IcodTermInfoInspectionPackageVersion=$version
	if ( 0 -ne $LASTEXITCODE ) {
		throw "RE07 package-only interoperability restore failed for Icod.TermInfo.Inspection $version and Icod.Terminal 1.12.0."
	}

	foreach ( $framework in @( 'net8.0', 'net9.0', 'net10.0' ) ) {
		& dotnet run `
			--project $projectPath `
			-c $Configuration `
			-f $framework `
			--no-restore `
			-p:IcodTermInfoInspectionPackageVersion=$version
		if ( 0 -ne $LASTEXITCODE ) {
			throw "RE07 package-only runtime-evidence interoperability consumer failed on $framework."
		}
	}

	Write-Host "RE07 package-only runtime-evidence interoperability consumer passed on net8.0, net9.0, and net10.0 for Icod.TermInfo.Inspection $version with Icod.Terminal 1.12.0."
} finally {
	$env:NUGET_PACKAGES = $previousNugetPackages
	$env:ICOD_TERMINFO_ARTIFACT_DIR = $previousArtifactDirectory
	if ( Test-Path -LiteralPath $workRoot ) {
		Remove-Item -LiteralPath $workRoot -Recurse -Force
	}
}
