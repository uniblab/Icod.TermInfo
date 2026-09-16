#!/usr/bin/env pwsh
# Proves public HDB07 acquisition propagates a real operating-system access denial.

[CmdletBinding()]
param(
	[Parameter( Mandatory = $true )]
	[string] $DatabasePath,

	[Parameter( Mandatory = $true )]
	[string] $TerminalName
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$database = (Resolve-Path -LiteralPath $DatabasePath).Path
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..' '..')).Path
$probeProject = Join-Path $repositoryRoot 'tools/hdb07-permission-probe/Hdb07.PermissionProbe.csproj'

function Assert-DatabaseReadable {
	$share = [System.IO.FileShare]::ReadWrite -bor [System.IO.FileShare]::Delete
	$stream = [System.IO.File]::Open(
		$database,
		[System.IO.FileMode]::Open,
		[System.IO.FileAccess]::Read,
		$share
	)
	try {
		if ($stream.ReadByte() -lt 0) {
			throw 'The HDB07 permission fixture is unexpectedly empty.'
		}
	} finally {
		$stream.Dispose()
	}
}

function Invoke-PermissionProbeProcess {
	param(
		[Parameter( Mandatory = $true )]
		[bool] $NoBuild
	)

	$arguments = @(
		'run',
		'--project',
		$probeProject,
		'-c',
		'Release'
	)
	if ($NoBuild) {
		$arguments += '--no-build'
	}
	$arguments += @('--', $database, $TerminalName)

	$PSNativeCommandUseErrorActionPreference = $false
	$output = & dotnet @arguments 2>&1
	return [pscustomobject] @{
		ExitCode = $LASTEXITCODE
		Output = $output
	}
}

function Assert-PublicAcquisitionReadable {
	param(
		[Parameter( Mandatory = $true )]
		[bool] $NoBuild
	)

	$result = Invoke-PermissionProbeProcess -NoBuild $NoBuild
	if ($result.ExitCode -ne 10) {
		$result.Output | Out-Host
		throw "The public HDB07 acquisition path was not readable (exit $($result.ExitCode))."
	}
}

function Assert-PermissionDenied {
	$result = Invoke-PermissionProbeProcess -NoBuild $true
	if ($result.ExitCode -ne 0) {
		$result.Output | Out-Host
		throw "The HDB07 permission probe failed with exit code $($result.ExitCode)."
	}
}

Assert-DatabaseReadable
Assert-PublicAcquisitionReadable -NoBuild $false

if ([System.OperatingSystem]::IsWindows()) {
	$originalAcl = Get-Acl -LiteralPath $database
	$sid = [System.Security.Principal.WindowsIdentity]::GetCurrent().User.Value
	$sidArgument = "*$sid"
	try {
		& icacls $database '/inheritance:r' | Out-Host
		if ($LASTEXITCODE -ne 0) {
			throw "icacls could not disable inheritance (exit $LASTEXITCODE)."
		}

		& icacls $database '/deny' "${sidArgument}:(R)" | Out-Host
		if ($LASTEXITCODE -ne 0) {
			throw "icacls could not establish a read denial (exit $LASTEXITCODE)."
		}

		Assert-PermissionDenied
	} finally {
		& icacls $database '/remove:d' $sidArgument | Out-Host
		Set-Acl -LiteralPath $database -AclObject $originalAcl
	}
} else {
	$originalMode = [System.IO.File]::GetUnixFileMode( $database )
	try {
		[System.IO.File]::SetUnixFileMode(
			$database,
			[System.IO.UnixFileMode]::None
		)
		Assert-PermissionDenied
	} finally {
		[System.IO.File]::SetUnixFileMode( $database, $originalMode )
	}
}

Assert-DatabaseReadable
Assert-PublicAcquisitionReadable -NoBuild $true
Write-Host 'HDB07 permission propagation passed.'
