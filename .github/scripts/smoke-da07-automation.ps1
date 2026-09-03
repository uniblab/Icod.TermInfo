param(
	[Parameter( Mandatory = $true, Position = 0 )]
	[ValidateSet( 'package', 'archive' )]
	[string] $Kind,

	[Parameter( Mandatory = $true, Position = 1 )]
	[string] $ArtifactDirectory
)

$ErrorActionPreference = 'Stop'

$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryRoot = [System.IO.Path]::GetFullPath(
	[System.IO.Path]::Combine( $scriptDirectory, '..', '..' )
)
$artifactRoot = [System.IO.Path]::GetFullPath( $ArtifactDirectory )
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
	'Icod.TermInfo.DA07Smoke.' + [System.Guid]::NewGuid().ToString( 'N' )
)
$previousNugetPackages = $env:NUGET_PACKAGES

try {
	New-Item -ItemType Directory -Path $workRoot -Force | Out-Null
	$toolRoot = Join-Path $workRoot 'tools'
	New-Item -ItemType Directory -Path $toolRoot -Force | Out-Null

	if ( 'package' -ceq $Kind ) {
		$configPath = Join-Path $workRoot 'NuGet.Config'
		$env:NUGET_PACKAGES = Join-Path $workRoot 'packages'
		$escapedArtifactRoot = [System.Security.SecurityElement]::Escape( $artifactRoot )
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
	}
	else {
		$architecture = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture
		switch ( $architecture ) {
			'X64' { $architectureName = 'x64' }
			'Arm64' { $architectureName = 'arm64' }
			default { throw "Unsupported DA07 smoke architecture '$architecture'." }
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
			throw 'Unsupported DA07 smoke operating system.'
		}
		$rid = "$platform-$architectureName"
		$archiveName = "Icod.TermInfo.Tools.$version.$rid$extension"
		$archivePath = Join-Path $artifactRoot $archiveName
		if ( -not ( Test-Path -LiteralPath $archivePath -PathType Leaf ) ) {
			throw "Expected matching DA07 archive '$archivePath' was not found."
		}
		if ( $IsWindows ) {
			Expand-Archive -LiteralPath $archivePath -DestinationPath $toolRoot
		}
		else {
			& tar -xzf $archivePath -C $toolRoot
			if ( 0 -ne $LASTEXITCODE ) {
				throw "Could not extract '$archiveName'."
			}
		}
	}

	function Invoke-TermInfoTool {
		param(
			[Parameter( Mandatory = $true )]
			[string] $Name,

			[Parameter( Mandatory = $true )]
			[string[]] $Arguments
		)

		if ( 'package' -ceq $Kind ) {
			$launcherName = if ( $IsWindows ) { 'icod-terminfo.exe' } else { 'icod-terminfo' }
			$launcherArguments = @( $Name ) + $Arguments
		}
		else {
			$launcherName = if ( $IsWindows ) { "$Name.exe" } else { $Name }
			$launcherArguments = $Arguments
		}
		$launcher = Join-Path $toolRoot $launcherName
		if ( -not ( Test-Path -LiteralPath $launcher -PathType Leaf ) ) {
			throw "DA07 $Kind smoke is missing '$launcherName'."
		}
		$output = @(
			& $launcher @launcherArguments 2>&1
		)
		if ( 0 -ne $LASTEXITCODE ) {
			throw "'$Name $($Arguments -join ' ')' failed with status $LASTEXITCODE.`n$($output -join [Environment]::NewLine)"
		}
		return $output -join [Environment]::NewLine
	}

	$firstRoot = Join-Path $workRoot 'first-db'
	$secondRoot = Join-Path $workRoot 'second-db'
	$planningFirstRoot = Join-Path $workRoot 'planning-first-db'
	$planningSecondRoot = Join-Path $workRoot 'planning-second-db'
	New-Item -ItemType Directory -Path $firstRoot -Force | Out-Null
	New-Item -ItemType Directory -Path $secondRoot -Force | Out-Null
	New-Item -ItemType Directory -Path $planningFirstRoot -Force | Out-Null
	New-Item -ItemType Directory -Path $planningSecondRoot -Force | Out-Null
	$firstSource = Join-Path $workRoot 'first.ti'
	$secondSource = Join-Path $workRoot 'second.ti'
	$planningFirstSource = Join-Path $workRoot 'planning-first.ti'
	$planningSecondSource = Join-Path $workRoot 'planning-second.ti'
	[System.IO.File]::WriteAllText(
		$firstSource,
		@"
da07-shared|DA07 first shared,
	cols#80,

da07-target|DA07 planning target,
	am,
	cols#80,
"@,
		[System.Text.UTF8Encoding]::new( $false )
	)
	[System.IO.File]::WriteAllText(
		$secondSource,
		@"
da07-shared|DA07 second shared,
	cols#132,
"@,
		[System.Text.UTF8Encoding]::new( $false )
	)
	[System.IO.File]::WriteAllText(
		$planningFirstSource,
		@"
da07-parent-a|DA07 first parent,
	am,
"@,
		[System.Text.UTF8Encoding]::new( $false )
	)
	[System.IO.File]::WriteAllText(
		$planningSecondSource,
		@"
da07-parent-b|DA07 second parent,
	cols#80,
"@,
		[System.Text.UTF8Encoding]::new( $false )
	)
	[void] ( Invoke-TermInfoTool -Name 'tic' -Arguments @( '-o', $firstRoot, $firstSource ) )
	[void] ( Invoke-TermInfoTool -Name 'tic' -Arguments @( '-o', $secondRoot, $secondSource ) )
	[void] ( Invoke-TermInfoTool -Name 'tic' -Arguments @( '-o', $planningFirstRoot, $planningFirstSource ) )
	[void] ( Invoke-TermInfoTool -Name 'tic' -Arguments @( '-o', $planningSecondRoot, $planningSecondSource ) )

	$setText = Invoke-TermInfoTool -Name 'toe' -Arguments @(
		'--json',
		$firstRoot,
		$secondRoot
	)
	$setDocument = $setText | ConvertFrom-Json -Depth 100
	if ( 2 -ne $setDocument.schemaVersion -or 'databaseSet' -cne $setDocument.documentKind ) {
		throw 'DA07 distribution smoke did not emit a v2 databaseSet document.'
	}
	if ( 2 -ne $setDocument.data.databases.Count ) {
		throw 'DA07 distribution smoke did not retain both ordered database roots.'
	}
	$shared = @(
		$setDocument.data.semanticAnalysis.repeatedIdentities |
			Where-Object { 'da07-shared' -ceq $_.name }
	)
	if ( 1 -ne $shared.Count -or 'semanticallyDifferent' -cne $shared[ 0 ].relationship ) {
		throw 'DA07 distribution smoke did not classify the conflicting shared identity.'
	}

	$planText = Invoke-TermInfoTool -Name 'infocmp' -Arguments @(
		'--json',
		'--plan-use',
		'--all-candidates',
		'--max-parents',
		'2',
		'-A',
		$firstRoot,
		'--candidate-root',
		$planningFirstRoot,
		'--candidate-root',
		$planningSecondRoot,
		'da07-target'
	)
	$planDocument = $planText | ConvertFrom-Json -Depth 100
	if ( 2 -ne $planDocument.schemaVersion -or 'databaseSetPlan' -cne $planDocument.documentKind ) {
		throw 'DA07 distribution smoke did not emit a v2 databaseSetPlan document.'
	}
	if ( $planDocument.data.candidates.Count -lt 2 ) {
		throw 'DA07 distribution smoke did not inspect candidates from both database roots.'
	}
	if ( $planDocument.data.source.Contains( 'use=da07-target' ) ) {
		throw 'DA07 distribution smoke failed to exclude the target identity from planning.'
	}

	Write-Host "DA07 $Kind automation smoke passed for Icod.TermInfo.Tools $version."
}
finally {
	$env:NUGET_PACKAGES = $previousNugetPackages
	if ( Test-Path -LiteralPath $workRoot ) {
		Remove-Item -LiteralPath $workRoot -Recurse -Force
	}
}
