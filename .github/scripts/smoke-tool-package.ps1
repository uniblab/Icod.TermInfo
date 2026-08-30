param(
	[Parameter( Mandatory = $true, Position = 0 )]
	[string] $ArtifactDirectory
)

$ErrorActionPreference = 'Stop'

$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryRoot = [System.IO.Path]::GetFullPath(
	[System.IO.Path]::Combine(
		$scriptDirectory,
		'..',
		'..'
	)
)
$artifactRoot = [System.IO.Path]::GetFullPath(
	$ArtifactDirectory
)

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
$packagePath = Join-Path (
	$artifactRoot
) (
	"Icod.TermInfo.Tools.$version.nupkg"
)
if ( -not ( Test-Path -LiteralPath $packagePath -PathType Leaf ) ) {
	throw "Expected router package '$packagePath' was not found."
}

$workRoot = Join-Path (
	[System.IO.Path]::GetTempPath()
) (
	'Icod.TermInfo.ToolPackageSmoke.' + [System.Guid]::NewGuid().ToString( 'N' )
)
$previousNugetPackages = $env:NUGET_PACKAGES

try {
	$toolRoot = Join-Path $workRoot 'tool'
	$configPath = Join-Path $workRoot 'NuGet.Config'
	$env:NUGET_PACKAGES = Join-Path $workRoot 'packages'
	New-Item -ItemType Directory -Path $toolRoot -Force | Out-Null

	$escapedArtifactRoot = [System.Security.SecurityElement]::Escape(
		$artifactRoot
	)
	$config = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="artifacts" value="$escapedArtifactRoot" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="artifacts">
      <package pattern="Icod.TermInfo*" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="Icod.CommandFramework" />
      <package pattern="Microsoft.*" />
      <package pattern="System.*" />
      <package pattern="runtime.*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
"@
	[System.IO.File]::WriteAllText(
		$configPath,
		$config,
		[System.Text.UTF8Encoding]::new( $false )
	)

	& dotnet tool install Icod.TermInfo.Tools `
		--tool-path $toolRoot `
		--version $version `
		--configfile $configPath `
		--no-cache
	if ( 0 -ne $LASTEXITCODE ) {
		throw "Could not install Icod.TermInfo.Tools $version from '$artifactRoot'."
	}

	$launcherName = if ( $IsWindows ) {
		'icod-terminfo.exe'
	}
	else {
		'icod-terminfo'
	}
	$launcher = Join-Path $toolRoot $launcherName
	if ( -not ( Test-Path -LiteralPath $launcher -PathType Leaf ) ) {
		throw "Installed tool is missing '$launcherName'."
	}

	function Invoke-Router {
		param(
			[Parameter( Mandatory = $true )]
			[string[]] $Arguments
		)

		$output = @(
			& $launcher @Arguments 2>&1
		)
		if ( 0 -ne $LASTEXITCODE ) {
			throw "'icod-terminfo $($Arguments -join ' ')' failed with status $LASTEXITCODE.`n$($output -join [Environment]::NewLine)"
		}

		return $output -join [Environment]::NewLine
	}

	$routerVersion = Invoke-Router -Arguments @(
		'--version'
	)
	if ( -not $routerVersion.Contains( $version ) ) {
		throw "'icod-terminfo --version' did not report '$version'."
	}

	foreach ( $command in @( 'tic', 'infocmp', 'toe' ) ) {
		$versionOutput = Invoke-Router -Arguments @(
			$command,
			'-V'
		)
		if ( -not $versionOutput.Contains( $version ) ) {
			throw "'icod-terminfo $command -V' did not report '$version'."
		}
	}

	$sourcePath = Join-Path $workRoot 'release-smoke.ti'
	$databaseRoot = Join-Path $workRoot 'terminfo'
	[System.IO.Directory]::CreateDirectory( $databaseRoot ) | Out-Null
	[System.IO.File]::WriteAllText(
		$sourcePath,
		@"
release-smoke|Icod.TermInfo release smoke terminal,
	am,
	cols#80,
"@,
		[System.Text.UTF8Encoding]::new( $false )
	)

	[void] (
		Invoke-Router -Arguments @(
			'tic',
			'-o',
			$databaseRoot,
			$sourcePath
		)
	)

	$infocmpOutput = Invoke-Router -Arguments @(
		'infocmp',
		'-A',
		$databaseRoot,
		'release-smoke'
	)
	if ( -not $infocmpOutput.Contains( 'release-smoke' ) ) {
		throw 'Routed infocmp did not acquire the entry published by routed tic.'
	}

	$toeOutput = Invoke-Router -Arguments @(
		'toe',
		'-s',
		$databaseRoot
	)
	if ( -not $toeOutput.Contains( 'release-smoke' ) ) {
		throw 'Routed toe did not enumerate the entry published by routed tic.'
	}

	Write-Host "Smoke-tested Icod.TermInfo.Tools $version successfully."
}
finally {
	$env:NUGET_PACKAGES = $previousNugetPackages
	if ( Test-Path -LiteralPath $workRoot ) {
		Remove-Item -LiteralPath $workRoot -Recurse -Force
	}
}
