param(
    [Parameter(Mandatory = $true)]
    [string]$ArtifactDirectory,

    [ValidateSet('Debug', 'Staging', 'Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if (-not [System.IO.Path]::IsPathRooted($ArtifactDirectory)) {
    $ArtifactDirectory = Join-Path $repositoryRoot $ArtifactDirectory
}
$ArtifactDirectory = [System.IO.Path]::GetFullPath($ArtifactDirectory)
$runningOnWindows = [System.Environment]::OSVersion.Platform -eq [System.PlatformID]::Win32NT

Push-Location $repositoryRoot
try {
    if ($runningOnWindows) {
        & cmd /d /c .github\scripts\verify-release-package.cmd $ArtifactDirectory $Configuration
    } else {
        & bash .github/scripts/verify-release-package.sh $ArtifactDirectory $Configuration
    }
    if (0 -ne $LASTEXITCODE) {
        throw "Package artifact verification exited with status $LASTEXITCODE."
    }
} finally {
    Pop-Location
}
