param(
	[Parameter( Mandatory = $true, Position = 0 )]
	[string] $FixtureRoot,

	[Parameter( Mandatory = $true, Position = 1 )]
	[ValidateSet( 'Direct', 'Routed' )]
	[string] $LaunchMode
)

$ErrorActionPreference = 'Stop'

$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryRoot = [System.IO.Path]::GetFullPath(
	[System.IO.Path]::Combine( $scriptDirectory, '..', '..' )
)
$fixturePath = [System.IO.Path]::GetFullPath( $FixtureRoot )
$primaryDatabase = Join-Path $fixturePath 'hashed-db.db'
$bigEndianDatabase = Join-Path $fixturePath 'big-endian-hashed-db.db'
$latin1Database = Join-Path $fixturePath 'latin1-hashed-db.db'
$overflowDatabase = Join-Path $fixturePath 'overflow-hashed-db.db'
$multiDatabase = Join-Path $fixturePath 'multi-hashed-db.db'
$malformedDatabase = Join-Path $fixturePath 'random.db'
$wrongTypeDatabase = Join-Path $fixturePath 'not-hash.db'

foreach ( $requiredPath in @(
	$primaryDatabase,
	$bigEndianDatabase,
	$latin1Database,
	$overflowDatabase,
	$multiDatabase,
	$malformedDatabase,
	$wrongTypeDatabase
) ) {
	if ( -not ( Test-Path -LiteralPath $requiredPath -PathType Leaf ) ) {
		throw "Required HDB06 command fixture '$requiredPath' was not found."
	}
}

function Invoke-DotNet {
	param(
		[Parameter( Mandatory = $true )]
		[string[]] $Arguments,

		[Parameter( Mandatory = $true )]
		[string] $FailureMessage
	)

	& dotnet @Arguments
	if ( 0 -ne $LASTEXITCODE ) {
		throw "$FailureMessage (exit code $LASTEXITCODE)."
	}
}

if ( 'Direct' -ceq $LaunchMode ) {
	Invoke-DotNet -Arguments @(
		'build',
		( Join-Path $repositoryRoot 'infocmp/Icod.TermInfo.InfoCmp.csproj' ),
		'-c', 'Release',
		'-f', 'net10.0'
	) -FailureMessage 'Could not build direct infocmp'
	Invoke-DotNet -Arguments @(
		'build',
		( Join-Path $repositoryRoot 'toe/Icod.TermInfo.Toe.csproj' ),
		'-c', 'Release',
		'-f', 'net10.0'
	) -FailureMessage 'Could not build direct toe'
}
else {
	Invoke-DotNet -Arguments @(
		'build',
		( Join-Path $repositoryRoot 'icod-terminfo/Icod.TermInfo.Router.csproj' ),
		'-c', 'Release',
		'-f', 'net10.0'
	) -FailureMessage 'Could not build routed tool'
}

$captureRoot = Join-Path (
	[System.IO.Path]::GetTempPath()
) (
	'Icod.TermInfo.Hdb06Commands.' + [System.Guid]::NewGuid().ToString( 'N' )
)
[System.IO.Directory]::CreateDirectory( $captureRoot ) | Out-Null
$captureIndex = 0

function Invoke-TermInfoCommand {
	param(
		[Parameter( Mandatory = $true )]
		[ValidateSet( 'infocmp', 'toe' )]
		[string] $CommandName,

		[Parameter( Mandatory = $true )]
		[string[]] $Arguments
	)

	$script:captureIndex++
	$stdoutPath = Join-Path $captureRoot "$captureIndex.stdout"
	$stderrPath = Join-Path $captureRoot "$captureIndex.stderr"
	if ( 'Routed' -ceq $LaunchMode ) {
		$projectPath = Join-Path $repositoryRoot 'icod-terminfo/Icod.TermInfo.Router.csproj'
		$commandArguments = @( $CommandName ) + $Arguments
	}
	elseif ( 'infocmp' -ceq $CommandName ) {
		$projectPath = Join-Path $repositoryRoot 'infocmp/Icod.TermInfo.InfoCmp.csproj'
		$commandArguments = $Arguments
	}
	else {
		$projectPath = Join-Path $repositoryRoot 'toe/Icod.TermInfo.Toe.csproj'
		$commandArguments = $Arguments
	}

	& dotnet run `
		--no-build `
		--project $projectPath `
		-c Release `
		-f net10.0 `
		-- `
		@commandArguments `
		1> $stdoutPath `
		2> $stderrPath
	$status = $LASTEXITCODE
	$stdout = [System.IO.File]::ReadAllText( $stdoutPath )
	$stderr = [System.IO.File]::ReadAllText( $stderrPath )

	return [pscustomobject]@{
		Status = $status
		Stdout = $stdout
		Stderr = $stderr
	}
}

function Assert-CommandResult {
	param(
		[Parameter( Mandatory = $true )]
		[object] $Result,

		[Parameter( Mandatory = $true )]
		[int] $ExpectedStatus,

		[string] $StdoutContains,

		[string] $StderrContains,

		[switch] $RequireEmptyStdout,

		[switch] $RequireEmptyStderr
	)

	if ( $ExpectedStatus -ne $Result.Status ) {
		throw "Expected status $ExpectedStatus, got $($Result.Status).`nstdout:`n$($Result.Stdout)`nstderr:`n$($Result.Stderr)"
	}
	if (
		-not [string]::IsNullOrEmpty( $StdoutContains ) -and
		-not $Result.Stdout.Contains( $StdoutContains )
	) {
		throw "stdout did not contain '$StdoutContains'.`n$($Result.Stdout)"
	}
	if (
		-not [string]::IsNullOrEmpty( $StderrContains ) -and
		-not $Result.Stderr.Contains( $StderrContains )
	) {
		throw "stderr did not contain '$StderrContains'.`n$($Result.Stderr)"
	}
	if ( $RequireEmptyStdout -and 0 -ne $Result.Stdout.Length ) {
		throw "Expected empty stdout.`n$($Result.Stdout)"
	}
	if ( $RequireEmptyStderr -and 0 -ne $Result.Stderr.Length ) {
		throw "Expected empty stderr.`n$($Result.Stderr)"
	}
}

try {
	foreach ( $name in @( 'hdb00-primary', 'hdb00-alias' ) ) {
		$result = Invoke-TermInfoCommand infocmp @(
			'-A', $primaryDatabase, $name
		)
		Assert-CommandResult `
			-Result $result `
			-ExpectedStatus 0 `
			-StdoutContains 'hdb00-primary|hdb00-alias|Icod HDB00 hashed terminfo fixture,' `
			-RequireEmptyStderr
	}

	$result = Invoke-TermInfoCommand toe @( '-s', $primaryDatabase )
	Assert-CommandResult -Result $result -ExpectedStatus 0 -RequireEmptyStderr
	$primaryToeOutput = $result.Stdout
	$logicalLines = @(
		$result.Stdout -split '\r?\n' |
			Where-Object { 0 -ne $_.Length }
	)
	$expectedLines = @(
		"hdb00-alias`tIcod HDB00 hashed terminfo fixture",
		"hdb00-primary`tIcod HDB00 hashed terminfo fixture"
	)
	if ( 2 -ne $logicalLines.Count ) {
		throw "Expected two logical toe entries, got $($logicalLines.Count).`n$($result.Stdout)"
	}
	for ( $index = 0; $index -lt $expectedLines.Count; $index++ ) {
		if ( $expectedLines[$index] -cne $logicalLines[$index] ) {
			throw "Unexpected toe entry '$($logicalLines[$index])'; expected '$($expectedLines[$index])'."
		}
	}

	foreach ( $name in @( 'hdb00-primary', 'hdb00-alias' ) ) {
		$littleEndian = Invoke-TermInfoCommand infocmp @(
			'-A', $primaryDatabase, $name
		)
		$bigEndian = Invoke-TermInfoCommand infocmp @(
			'-A', $bigEndianDatabase, $name
		)
		Assert-CommandResult `
			-Result $bigEndian `
			-ExpectedStatus 0 `
			-StdoutContains 'hdb00-primary|hdb00-alias|Icod HDB00 hashed terminfo fixture,' `
			-RequireEmptyStderr
		if ( $littleEndian.Stdout -cne $bigEndian.Stdout ) {
			throw "Big-endian infocmp output differs for '$name'."
		}
	}

	$bigEndianToe = Invoke-TermInfoCommand toe @( '-s', $bigEndianDatabase )
	Assert-CommandResult `
		-Result $bigEndianToe `
		-ExpectedStatus 0 `
		-RequireEmptyStderr
	if ( $primaryToeOutput -cne $bigEndianToe.Stdout ) {
		throw 'Big-endian toe output differs from the primary little-endian store.'
	}

	$latin1Canonical = 'hdb07c-caf' + [char]0x00E9
	$latin1Alias = 'hdb07c-ali' + [char]0x00E9
	foreach ( $name in @( $latin1Canonical, $latin1Alias ) ) {
		$result = Invoke-TermInfoCommand infocmp @(
			'-A', $latin1Database, $name
		)
		Assert-CommandResult `
			-Result $result `
			-ExpectedStatus 0 `
			-StdoutContains "$latin1Canonical|$latin1Alias|Icod HDB07C Latin-1 fixture," `
			-RequireEmptyStderr
	}

	$result = Invoke-TermInfoCommand toe @( '-s', $latin1Database )
	Assert-CommandResult -Result $result -ExpectedStatus 0 -RequireEmptyStderr
	$latin1Lines = @(
		$result.Stdout -split '\r?\n' |
			Where-Object { 0 -ne $_.Length }
	)
	$latin1ExpectedLines = @(
		"$latin1Alias`tIcod HDB07C Latin-1 fixture",
		"$latin1Canonical`tIcod HDB07C Latin-1 fixture"
	)
	if ( 2 -ne $latin1Lines.Count ) {
		throw "Expected two Latin-1 toe publications, got $($latin1Lines.Count).`n$($result.Stdout)"
	}
	for ( $index = 0; $index -lt $latin1ExpectedLines.Count; $index++ ) {
		if ( $latin1ExpectedLines[$index] -cne $latin1Lines[$index] ) {
			throw "Unexpected Latin-1 toe entry '$($latin1Lines[$index])'; expected '$($latin1ExpectedLines[$index])'."
		}
	}

	$result = Invoke-TermInfoCommand infocmp @(
		'-A', $overflowDatabase, 'hdb00-overflow'
	)
	Assert-CommandResult `
		-Result $result `
		-ExpectedStatus 0 `
		-StdoutContains 'hdb00-overflow|Icod HDB00 overflow terminfo fixture,' `
		-RequireEmptyStderr

	$result = Invoke-TermInfoCommand toe @( '-s', $overflowDatabase )
	Assert-CommandResult `
		-Result $result `
		-ExpectedStatus 0 `
		-StdoutContains "hdb00-overflow`tIcod HDB00 overflow terminfo fixture" `
		-RequireEmptyStderr

	foreach ( $index in @( 0, 32, 63 ) ) {
		$canonical = 'hdb07-multi-{0:D3}' -f $index
		$alias = $canonical + '-alias'
		foreach ( $name in @( $canonical, $alias ) ) {
			$result = Invoke-TermInfoCommand infocmp @(
				'-A', $multiDatabase, $name
			)
			Assert-CommandResult `
				-Result $result `
				-ExpectedStatus 0 `
				-StdoutContains "$canonical|$alias|Icod HDB07 multi $($index.ToString( 'D3' )) fixture," `
				-RequireEmptyStderr
		}
	}

	$result = Invoke-TermInfoCommand toe @( '-s', $multiDatabase )
	Assert-CommandResult -Result $result -ExpectedStatus 0 -RequireEmptyStderr
	$multiLines = @(
		$result.Stdout -split '\r?\n' |
			Where-Object { 0 -ne $_.Length }
	)
	if ( 128 -ne $multiLines.Count ) {
		throw "Expected 128 multi-record toe publications, got $($multiLines.Count).`n$($result.Stdout)"
	}
	foreach ( $index in @( 0, 32, 63 ) ) {
		$canonical = 'hdb07-multi-{0:D3}' -f $index
		$description = "Icod HDB07 multi $($index.ToString( 'D3' )) fixture"
		foreach ( $name in @( $canonical, ( $canonical + '-alias' ) ) ) {
			$expected = "$name`t$description"
			if ( $expected -cnotin $multiLines ) {
				throw "Expected toe publication '$expected'."
			}
		}
	}
	Write-Host "HDB07 multi command publications: $($multiLines.Count)"

	$result = Invoke-TermInfoCommand infocmp @(
		'-A', $primaryDatabase, 'hdb00-missing'
	)
	Assert-CommandResult `
		-Result $result `
		-ExpectedStatus 1 `
		-StderrContains 'INFOCMP0002 error' `
		-RequireEmptyStdout

	foreach ( $badDatabase in @( $malformedDatabase, $wrongTypeDatabase ) ) {
		$result = Invoke-TermInfoCommand infocmp @(
			'-A', $badDatabase, 'hdb00-primary'
		)
		Assert-CommandResult `
			-Result $result `
			-ExpectedStatus 1 `
			-StderrContains 'INFOCMP0003 error' `
			-RequireEmptyStdout

		$result = Invoke-TermInfoCommand toe @( '-s', $badDatabase )
		Assert-CommandResult `
			-Result $result `
			-ExpectedStatus 1 `
			-StderrContains 'TOE0005 error' `
			-RequireEmptyStdout
	}

	Write-Host "HDB06 $LaunchMode command verification passed for '$fixturePath'."
	$global:LASTEXITCODE = 0
}
finally {
	if ( Test-Path -LiteralPath $captureRoot ) {
		Remove-Item -LiteralPath $captureRoot -Recurse -Force
	}
}
