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

	foreach ( $command in @( 'tic', 'infocmp', 'toe', 'captoinfo', 'infotocap' ) ) {
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

	$planningSourcePath = Join-Path $workRoot 'release-planning.ti'
	[System.IO.File]::WriteAllText(
		$planningSourcePath,
		@"
release-plan-decoy|Icod.TermInfo release planning decoy,
	lines#12,

release-plan-useful|release-plan-parent|Icod.TermInfo release planning parent,
	am,
	cols#80,
	lines#24,

release-plan-target|Icod.TermInfo release planning target,
	am,
	cols#80,
	lines#24,
	clear=\E[H\E[2J,
"@,
		[System.Text.UTF8Encoding]::new( $false )
	)

	[void] (
		Invoke-Router -Arguments @(
			'tic',
			'-o',
			$databaseRoot,
			$planningSourcePath
		)
	)

	$planningOutput = Invoke-Router -Arguments @(
		'infocmp',
		'-A',
		$databaseRoot,
		'-B',
		$databaseRoot,
		'--max-parents',
		'1',
		'--require-exhaustive',
		'--plan-use',
		'release-plan-target',
		'release-plan-decoy',
		'release-plan-parent'
	)
	if ( -not $planningOutput.Contains( 'use=release-plan-parent' ) ) {
		throw 'Routed infocmp planning did not select the useful alias candidate.'
	}
	if ( $planningOutput.Contains( 'use=release-plan-decoy' ) ) {
		throw 'Routed infocmp planning selected the decoy candidate.'
	}
	$plannedSourcePath = Join-Path $workRoot 'release-planned-output.ti'
	$planningValidationSource = $planningOutput + @"

release-plan-useful|release-plan-parent|Icod.TermInfo release planning parent,
	am,
	cols#80,
	lines#24,
"@
	[System.IO.File]::WriteAllText(
		$plannedSourcePath,
		$planningValidationSource,
		[System.Text.UTF8Encoding]::new( $false )
	)
	[void] (
		Invoke-Router -Arguments @(
			'tic',
			'-c',
			$plannedSourcePath
		)
	)
	$plannedDatabaseRoot = Join-Path $workRoot 'planned-terminfo'
	[System.IO.Directory]::CreateDirectory( $plannedDatabaseRoot ) | Out-Null
	[void] (
		Invoke-Router -Arguments @(
			'tic',
			'-o',
			$plannedDatabaseRoot,
			$plannedSourcePath
		)
	)
	$originalTarget = Invoke-Router -Arguments @(
		'infocmp',
		'-A',
		$databaseRoot,
		'release-plan-target'
	)
	$plannedTarget = Invoke-Router -Arguments @(
		'infocmp',
		'-A',
		$plannedDatabaseRoot,
		'release-plan-target'
	)
	if ( $originalTarget -cne $plannedTarget ) {
		throw 'Routed infocmp planning output did not reproduce the target semantics.'
	}

	$descriptionDocument = Invoke-Router -Arguments @(
		'infocmp',
		'--json',
		'-A',
		$databaseRoot,
		'release-smoke'
	) | ConvertFrom-Json -Depth 100
	if ( 'terminalDescription' -cne $descriptionDocument.documentKind ) {
		throw 'Routed infocmp JSON did not emit a terminalDescription document.'
	}

	$comparisonDocument = Invoke-Router -Arguments @(
		'infocmp',
		'--json',
		'-d',
		'-A',
		$databaseRoot,
		'-B',
		$databaseRoot,
		'release-smoke',
		'release-plan-target'
	) | ConvertFrom-Json -Depth 100
	if ( 'comparison' -cne $comparisonDocument.documentKind ) {
		throw 'Routed infocmp JSON did not emit a comparison document.'
	}

	$automationSourcePath = Join-Path $workRoot 'release-json-planning.ti'
	$automationDatabaseRoot = Join-Path $workRoot 'json-terminfo'
	[System.IO.Directory]::CreateDirectory( $automationDatabaseRoot ) | Out-Null
	[System.IO.File]::WriteAllText(
		$automationSourcePath,
		@"
release-json-parent|Icod.TermInfo release JSON planning parent,
	cols#80,

release-json-target|Icod.TermInfo release JSON planning target,
	am,
	cols#80,
"@,
		[System.Text.UTF8Encoding]::new( $false )
	)
	[void] (
		Invoke-Router -Arguments @(
			'tic',
			'-o',
			$automationDatabaseRoot,
			$automationSourcePath
		)
	)

	$planDocument = Invoke-Router -Arguments @(
		'infocmp',
		'--json',
		'--plan-use',
		'--all-candidates',
		'--max-parents',
		'1',
		'-A',
		$automationDatabaseRoot,
		'-B',
		$automationDatabaseRoot,
		'release-json-target'
	) | ConvertFrom-Json -Depth 100
	if ( 'sourcePlan' -cne $planDocument.documentKind ) {
		throw 'Routed infocmp all-candidates JSON did not emit a sourcePlan document.'
	}
	if ( $planDocument.data.source.Contains( 'use=release-json-target' ) ) {
		throw 'Routed infocmp all-candidates planning failed to exclude the target.'
	}

	$toeOutput = Invoke-Router -Arguments @(
		'toe',
		'-s',
		$databaseRoot
	)
	if ( -not $toeOutput.Contains( 'release-smoke' ) ) {
		throw 'Routed toe did not enumerate the entry published by routed tic.'
	}

	$catalogDocument = Invoke-Router -Arguments @(
		'toe',
		'--json',
		$databaseRoot
	) | ConvertFrom-Json -Depth 100
	if ( 'databaseCatalog' -cne $catalogDocument.documentKind ) {
		throw 'Routed toe JSON did not emit a databaseCatalog document.'
	}
	if ( 'conventionalDirectory' -cne $catalogDocument.data.kind ) {
		throw 'Routed toe JSON did not report a conventional directory.'
	}

	$termcapPath = Join-Path $workRoot 'release-smoke.cap'
	[System.IO.File]::WriteAllText(
		$termcapPath,
		"release-cap|Release cap terminal:am:co#80:`n",
		[System.Text.UTF8Encoding]::new( $false )
	)

	$capToInfoOutput = Invoke-Router -Arguments @(
		'captoinfo',
		$termcapPath
	)
	if ( -not $capToInfoOutput.Contains( 'cols#80' ) ) {
		throw 'Routed captoinfo did not convert the release smoke termcap entry.'
	}

	$infoToCapOutput = Invoke-Router -Arguments @(
		'infotocap',
		$sourcePath
	)
	if ( -not $infoToCapOutput.Contains( 'co#80' ) ) {
		throw 'Routed infotocap did not convert the release smoke terminfo entry.'
	}

	Write-Host "Smoke-tested Icod.TermInfo.Tools $version successfully."
}
finally {
	$env:NUGET_PACKAGES = $previousNugetPackages
	if ( Test-Path -LiteralPath $workRoot ) {
		Remove-Item -LiteralPath $workRoot -Recurse -Force
	}
}
