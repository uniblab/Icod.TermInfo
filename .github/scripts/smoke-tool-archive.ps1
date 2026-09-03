param(
	[Parameter( Mandatory = $true, Position = 0 )]
	[string] $ArchiveDirectory
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

$architecture = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture
switch ( $architecture ) {
	'X64' {
		$architectureName = 'x64'
	}
	'Arm64' {
		$architectureName = 'arm64'
	}
	default {
		throw "Unsupported smoke-test architecture '$architecture'."
	}
}

if ( $IsWindows ) {
	$platform = 'win'
	$extension = '.zip'
}
elseif ( $IsLinux ) {
	$platform = 'linux'
	$extension = '.tar.gz'
}
elseif ( $IsMacOS ) {
	$platform = 'osx'
	$extension = '.tar.gz'
}
else {
	throw 'Unsupported smoke-test operating system.'
}

$rid = "$platform-$architectureName"
$archiveName = "Icod.TermInfo.Tools.$version.$rid$extension"
$archivePath = Join-Path $ArchiveDirectory $archiveName
if ( -not ( Test-Path -LiteralPath $archivePath -PathType Leaf ) ) {
	throw "Expected matching tool archive '$archivePath' was not found."
}

$workRoot = Join-Path (
	[System.IO.Path]::GetTempPath()
) (
	'Icod.TermInfo.ToolSmoke.' + [System.Guid]::NewGuid().ToString( 'N' )
)

try {
	New-Item -ItemType Directory -Path $workRoot | Out-Null

	if ( $IsWindows ) {
		Expand-Archive -LiteralPath $archivePath -DestinationPath $workRoot
	}
	else {
		& tar -xzf $archivePath -C $workRoot
		if ( 0 -ne $LASTEXITCODE ) {
			throw "Could not extract '$archiveName'."
		}
	}

	function Invoke-ReleaseTool {
		param(
			[Parameter( Mandatory = $true )]
			[string] $Name,

			[Parameter( Mandatory = $true )]
			[string[]] $Arguments
		)

		$launcherName = if ( $IsWindows ) {
			"$Name.exe"
		}
		else {
			$Name
		}
		$launcher = Join-Path $workRoot $launcherName
		if ( -not ( Test-Path -LiteralPath $launcher -PathType Leaf ) ) {
			throw "Archive '$archiveName' is missing launcher '$launcherName'."
		}

		$output = @(
			& $launcher @Arguments 2>&1
		)
		if ( 0 -ne $LASTEXITCODE ) {
			throw "'$Name $($Arguments -join ' ')' failed with status $LASTEXITCODE.`n$($output -join [Environment]::NewLine)"
		}

		return $output -join [Environment]::NewLine
	}

	foreach ( $command in @( 'tic', 'infocmp', 'toe', 'captoinfo', 'infotocap' ) ) {
		$versionOutput = Invoke-ReleaseTool -Name $command -Arguments @(
			'--version'
		)
		if ( -not $versionOutput.Contains( $version ) ) {
			throw "'$command --version' did not report '$version'."
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
		Invoke-ReleaseTool -Name 'tic' -Arguments @(
			'-o',
			$databaseRoot,
			$sourcePath
		)
	)

	$infocmpOutput = Invoke-ReleaseTool -Name 'infocmp' -Arguments @(
		'-A',
		$databaseRoot,
		'release-smoke'
	)
	if ( -not $infocmpOutput.Contains( 'release-smoke' ) ) {
		throw 'infocmp did not acquire the entry published by tic.'
	}

	$relativeSourcePath = Join-Path $workRoot 'release-relative.ti'
	[System.IO.File]::WriteAllText(
		$relativeSourcePath,
		@"
release-base|Icod.TermInfo release base,
	am,
	lines#24,

release-child|Icod.TermInfo release child,
	cols#120,
	clear=\E[H\E[2J,
	use=release-base,
"@,
		[System.Text.UTF8Encoding]::new( $false )
	)

	[void] (
		Invoke-ReleaseTool -Name 'tic' -Arguments @(
			'-o',
			$databaseRoot,
			$relativeSourcePath
		)
	)

	$relativeOutput = Invoke-ReleaseTool -Name 'infocmp' -Arguments @(
		'-A',
		$databaseRoot,
		'-B',
		$databaseRoot,
		'-u',
		'release-child',
		'release-base'
	)
	if ( -not $relativeOutput.Contains( 'use=release-base' ) ) {
		throw 'infocmp -u did not emit the expected release-base reference.'
	}
	if ( -not $relativeOutput.Contains( 'cols#120' ) ) {
		throw 'infocmp -u did not preserve the release-child local override.'
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
		Invoke-ReleaseTool -Name 'tic' -Arguments @(
			'-o',
			$databaseRoot,
			$planningSourcePath
		)
	)

	$planningOutput = Invoke-ReleaseTool -Name 'infocmp' -Arguments @(
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
		throw 'infocmp planning did not select the useful alias candidate.'
	}
	if ( $planningOutput.Contains( 'use=release-plan-decoy' ) ) {
		throw 'infocmp planning selected the decoy candidate.'
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
		Invoke-ReleaseTool -Name 'tic' -Arguments @(
			'-c',
			$plannedSourcePath
		)
	)
	$plannedDatabaseRoot = Join-Path $workRoot 'planned-terminfo'
	[System.IO.Directory]::CreateDirectory( $plannedDatabaseRoot ) | Out-Null
	[void] (
		Invoke-ReleaseTool -Name 'tic' -Arguments @(
			'-o',
			$plannedDatabaseRoot,
			$plannedSourcePath
		)
	)
	$originalTarget = Invoke-ReleaseTool -Name 'infocmp' -Arguments @(
		'-A',
		$databaseRoot,
		'release-plan-target'
	)
	$plannedTarget = Invoke-ReleaseTool -Name 'infocmp' -Arguments @(
		'-A',
		$plannedDatabaseRoot,
		'release-plan-target'
	)
	if ( $originalTarget -cne $plannedTarget ) {
		throw 'infocmp planning output did not reproduce the target semantics.'
	}

	$descriptionDocument = Invoke-ReleaseTool -Name 'infocmp' -Arguments @(
		'--json',
		'-A',
		$databaseRoot,
		'release-smoke'
	) | ConvertFrom-Json -Depth 100
	if ( 'terminalDescription' -cne $descriptionDocument.documentKind ) {
		throw 'infocmp JSON did not emit a terminalDescription document.'
	}

	$comparisonDocument = Invoke-ReleaseTool -Name 'infocmp' -Arguments @(
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
		throw 'infocmp JSON did not emit a comparison document.'
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
		Invoke-ReleaseTool -Name 'tic' -Arguments @(
			'-o',
			$automationDatabaseRoot,
			$automationSourcePath
		)
	)

	$planDocument = Invoke-ReleaseTool -Name 'infocmp' -Arguments @(
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
		throw 'infocmp all-candidates JSON did not emit a sourcePlan document.'
	}
	if ( $planDocument.data.source.Contains( 'use=release-json-target' ) ) {
		throw 'infocmp all-candidates planning failed to exclude the target.'
	}

	$toeOutput = Invoke-ReleaseTool -Name 'toe' -Arguments @(
		'-s',
		$databaseRoot
	)
	if ( -not $toeOutput.Contains( 'release-smoke' ) ) {
		throw 'toe did not enumerate the entry published by tic.'
	}

	$catalogDocument = Invoke-ReleaseTool -Name 'toe' -Arguments @(
		'--json',
		$databaseRoot
	) | ConvertFrom-Json -Depth 100
	if ( 'databaseCatalog' -cne $catalogDocument.documentKind ) {
		throw 'toe JSON did not emit a databaseCatalog document.'
	}
	if ( 'conventionalDirectory' -cne $catalogDocument.data.kind ) {
		throw 'toe JSON did not report a conventional directory.'
	}

	$termcapPath = Join-Path $workRoot 'release-smoke.cap'
	[System.IO.File]::WriteAllText(
		$termcapPath,
		"release-cap|Release cap terminal:am:co#80:`n",
		[System.Text.UTF8Encoding]::new( $false )
	)

	$capToInfoOutput = Invoke-ReleaseTool -Name 'captoinfo' -Arguments @(
		$termcapPath
	)
	if ( -not $capToInfoOutput.Contains( 'cols#80' ) ) {
		throw 'captoinfo did not convert the release smoke termcap entry.'
	}

	$infoToCapOutput = Invoke-ReleaseTool -Name 'infotocap' -Arguments @(
		$sourcePath
	)
	if ( -not $infoToCapOutput.Contains( 'co#80' ) ) {
		throw 'infotocap did not convert the release smoke terminfo entry.'
	}

	Write-Host "Smoke-tested $archiveName successfully."
}
finally {
	if ( Test-Path -LiteralPath $workRoot ) {
		Remove-Item -LiteralPath $workRoot -Recurse -Force
	}
}
