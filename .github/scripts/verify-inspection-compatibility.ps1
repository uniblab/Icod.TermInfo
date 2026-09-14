param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Debug', 'Staging', 'Release')]
    [string]$Configuration,

    [Parameter(Mandatory = $true)]
    [string]$AssemblyPath
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = [System.IO.Path]::GetFullPath(
    (Join-Path (Join-Path $PSScriptRoot '..') '..')
)
$freezePath = Join-Path $repositoryRoot 'docs/1.13.0-INSPECTION-PUBLIC-API-FREEZE.md'
$historyVerifierPath = Join-Path $PSScriptRoot 'verify-inspection-compatibility-history.ps1'
$oneThirteenApiSha256 = 'fd827a25abafb8e9ff3915567f45f9f2ec51b332bfd8a82dd4e4bca20290e764'
$assemblyFullPath = if ([System.IO.Path]::IsPathRooted($AssemblyPath)) {
    [System.IO.Path]::GetFullPath($AssemblyPath)
} else {
    [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $AssemblyPath))
}

foreach ($requiredPath in @($freezePath, $historyVerifierPath, $assemblyFullPath)) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Required Inspection compatibility input not found: $requiredPath"
    }
}

function Normalize-Text {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text
    )

    return (($Text -replace "`r`n", "`n" -replace "`r", "`n").TrimEnd("`n") + "`n")
}

function Get-NormalizedSha256 {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text
    )

    $normalized = Normalize-Text -Text $Text
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($normalized)
    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        $digest = $sha256.ComputeHash($bytes)
    } finally {
        $sha256.Dispose()
    }

    return [System.BitConverter]::ToString($digest).Replace('-', '').ToLowerInvariant()
}

Push-Location $repositoryRoot
try {
    $temporaryManifest = Join-Path (
        [System.IO.Path]::GetTempPath()
    ) ("Icod.TermInfo.Inspection-1.13-api-{0}.txt" -f [Guid]::NewGuid().ToString('N'))
    try {
        & dotnet run `
            --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj `
            -c $Configuration `
            --no-build `
            -- `
            --write `
            $temporaryManifest `
            $assemblyFullPath
        if (0 -ne $LASTEXITCODE) {
            throw "Public API snapshot generation exited with status $LASTEXITCODE."
        }

        $current = [System.IO.File]::ReadAllText($temporaryManifest)
        $currentSha256 = Get-NormalizedSha256 -Text $current
        if (-not [string]::Equals(
            $oneThirteenApiSha256,
            $currentSha256,
            [System.StringComparison]::Ordinal
        )) {
            throw "Icod.TermInfo.Inspection 1.13 public API fingerprint changed. Expected $oneThirteenApiSha256, actual $currentSha256."
        }

        $freeze = [System.IO.File]::ReadAllText($freezePath)
        if ($freeze.IndexOf($oneThirteenApiSha256, [System.StringComparison]::Ordinal) -lt 0) {
            throw '1.13.0-INSPECTION-PUBLIC-API-FREEZE.md does not record the expected whole-surface fingerprint.'
        }

        Write-Host "Verified exact 1.13 Inspection public API SHA-256 $currentSha256."
    } finally {
        if (Test-Path -LiteralPath $temporaryManifest) {
            Remove-Item -LiteralPath $temporaryManifest -Force
        }
    }

    & $historyVerifierPath `
        -Configuration $Configuration `
        -AssemblyPath $assemblyFullPath
    if (0 -ne $LASTEXITCODE) {
        throw "Historical Inspection compatibility verification exited with status $LASTEXITCODE."
    }
} finally {
    Pop-Location
}
