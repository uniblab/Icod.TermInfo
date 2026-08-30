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

	foreach ( $command in @( 'tic', 'infocmp', 'toe' ) ) {
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

	$toeOutput = Invoke-ReleaseTool -Name 'toe' -Arguments @(
		'-s',
		$databaseRoot
	)
	if ( -not $toeOutput.Contains( 'release-smoke' ) ) {
		throw 'toe did not enumerate the entry published by tic.'
	}

	Write-Host "Smoke-tested $archiveName successfully."
}
finally {
	if ( Test-Path -LiteralPath $workRoot ) {
		Remove-Item -LiteralPath $workRoot -Recurse -Force
	}
}
