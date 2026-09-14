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
$oneFourteenRb01TypesPath = Join-Path $repositoryRoot 'docs/1.14.0-RB01-INSPECTION-PUBLIC-API-ADDITIONS.txt'
$oneFourteenRb02TypesPath = Join-Path $repositoryRoot 'docs/1.14.0-RB02-INSPECTION-PUBLIC-API-ADDITIONS.txt'
$oneFourteenRb03TypesPath = Join-Path $repositoryRoot 'docs/1.14.0-RB03-INSPECTION-PUBLIC-API-ADDITIONS.txt'
$historyVerifierPath = Join-Path $PSScriptRoot 'verify-inspection-compatibility-history.ps1'

# Keep historical authorities explicit at the public verifier entry point. Exact
# reconstruction to the frozen whole-1.13 fingerprint is followed by the
# established 1.13 -> 1.12 -> 1.11 -> 1.10 reconstruction.
$oneTenBaselinePath = Join-Path $repositoryRoot 'docs/1.10.0-INSPECTION-PUBLIC-API-BASELINE.txt'
$oneElevenTypesPath = Join-Path $repositoryRoot 'docs/1.11.0-INSPECTION-PUBLIC-API-ADDITIONS.txt'
$oneElevenMembersPath = Join-Path $repositoryRoot 'docs/1.11.0-INSPECTION-PUBLIC-API-ADDITIVE-MEMBERS.txt'
$oneTwelveTypesPath = Join-Path $repositoryRoot 'docs/1.12.0-PG01-INSPECTION-PUBLIC-API-ADDITIONS.txt'
$oneTwelveMembersPath = Join-Path $repositoryRoot 'docs/1.12.0-PG06-INSPECTION-PUBLIC-API-ADDITIVE-MEMBERS.txt'
$oneThirteenTypesPath = Join-Path $repositoryRoot 'docs/1.13.0-RE01-INSPECTION-PUBLIC-API-ADDITIONS.txt'
$oneThirteenMembersPath = Join-Path $repositoryRoot 'docs/1.13.0-RE06-INSPECTION-PUBLIC-API-ADDITIVE-MEMBERS.txt'
$oneElevenApiSha256 = '69c7350d5d44d502ecf1698c8fe1c1336f03d38eb1a36e36219f50ac33585a86'
$oneTwelveApiSha256 = 'f71501dcd27a530051c1a02083325144ced2b6173b6b967b9571c620815198f0'
$oneThirteenApiSha256 = 'fd827a25abafb8e9ff3915567f45f9f2ec51b332bfd8a82dd4e4bca20290e764'
$assemblyFullPath = if ([System.IO.Path]::IsPathRooted($AssemblyPath)) {
    [System.IO.Path]::GetFullPath($AssemblyPath)
} else {
    [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $AssemblyPath))
}

foreach ($requiredPath in @(
    $freezePath,
    $oneFourteenRb01TypesPath,
    $oneFourteenRb02TypesPath,
    $oneFourteenRb03TypesPath,
    $historyVerifierPath,
    $oneTenBaselinePath,
    $oneElevenTypesPath,
    $oneElevenMembersPath,
    $oneTwelveTypesPath,
    $oneTwelveMembersPath,
    $oneThirteenTypesPath,
    $oneThirteenMembersPath,
    $assemblyFullPath
)) {
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

function Read-ApprovedTypes {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [int]$ExpectedCount,

        [Parameter(Mandatory = $true)]
        [string]$ReleaseLabel
    )

    $approved = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::Ordinal
    )
    foreach ($line in [System.IO.File]::ReadAllLines($Path)) {
        $candidate = $line.Trim()
        if ($candidate.Length -eq 0 -or $candidate.StartsWith('#', [System.StringComparison]::Ordinal)) {
            continue
        }
        if (-not $candidate.StartsWith('Icod.TermInfo.Inspection.RasterBackend', [System.StringComparison]::Ordinal)) {
            throw "Approved $ReleaseLabel Inspection public type is outside the RasterBackend prefix: $candidate"
        }
        if (-not $approved.Add($candidate)) {
            throw "Approved $ReleaseLabel Inspection public types file contains a duplicate: $candidate"
        }
    }

    if ($approved.Count -ne $ExpectedCount) {
        throw "Approved $ReleaseLabel Inspection public types file must contain exactly $ExpectedCount types; found $($approved.Count)."
    }

    # PowerShell enumerates collection return values. Preserve the HashSet as
    # one object so a one-entry tranche ledger does not collapse to a scalar.
    Write-Output -NoEnumerate $approved
}

function Remove-ApprovedTypes {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Manifest,

        [Parameter(Mandatory = $true)]
        [System.Collections.Generic.HashSet[string]]$ApprovedTypes
    )

    $lines = (Normalize-Text -Text $Manifest).Split("`n")
    $result = [System.Collections.Generic.List[string]]::new()
    $removedTypes = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::Ordinal
    )
    $skipBlock = $false
    $skipTrailingBlank = $false

    foreach ($line in $lines) {
        if ($skipTrailingBlank) {
            if ($line.Length -eq 0) {
                $skipTrailingBlank = $false
                continue
            }
            $skipTrailingBlank = $false
        }

        if (-not $skipBlock -and $line.StartsWith('TYPE ', [System.StringComparison]::Ordinal)) {
            if ($line -match '^TYPE\s+\S+\s+(\S+)\s+\[') {
                $typeName = $Matches[1]
                if ($typeName.StartsWith('Icod.TermInfo.Inspection.RasterBackend', [System.StringComparison]::Ordinal)) {
                    if (-not $ApprovedTypes.Contains($typeName)) {
                        throw "Unapproved 1.14 Inspection public API addition: $typeName"
                    }
                    if (-not $removedTypes.Add($typeName)) {
                        throw "Inspection API manifest contains duplicate 1.14 public type blocks: $typeName"
                    }
                    $skipBlock = $true
                    continue
                }
            }
        }

        if ($skipBlock) {
            if ($line -eq 'END') {
                $skipBlock = $false
                $skipTrailingBlank = $true
            }
            continue
        }

        $result.Add($line)
    }

    if ($skipBlock) {
        throw 'Inspection API manifest ended inside an approved 1.14 type block.'
    }

    foreach ($approvedType in $ApprovedTypes) {
        if (-not $removedTypes.Contains($approvedType)) {
            throw "Approved 1.14 Inspection public API type is missing from the current assembly: $approvedType"
        }
    }

    return [PSCustomObject]@{
        Manifest = Normalize-Text -Text ($result -join "`n")
        RemovedTypeCount = $removedTypes.Count
    }
}

Push-Location $repositoryRoot
try {
    $temporaryManifest = Join-Path (
        [System.IO.Path]::GetTempPath()
    ) ("Icod.TermInfo.Inspection-1.14-api-{0}.txt" -f [Guid]::NewGuid().ToString('N'))
    $reconstructedOneThirteenManifestPath = Join-Path (
        [System.IO.Path]::GetTempPath()
    ) ("Icod.TermInfo.Inspection-1.13-reconstructed-{0}.txt" -f [Guid]::NewGuid().ToString('N'))
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
        if ($current.IndexOf('Icod.Terminal', [System.StringComparison]::Ordinal) -ge 0) {
            throw 'Icod.TermInfo.Inspection public API unexpectedly references Icod.Terminal.'
        }

        $approvedRb01Types = Read-ApprovedTypes `
            -Path $oneFourteenRb01TypesPath `
            -ExpectedCount 13 `
            -ReleaseLabel '1.14 RB01'
        $approvedRb02Types = Read-ApprovedTypes `
            -Path $oneFourteenRb02TypesPath `
            -ExpectedCount 2 `
            -ReleaseLabel '1.14 RB02'
        $approvedRb03Types = Read-ApprovedTypes `
            -Path $oneFourteenRb03TypesPath `
            -ExpectedCount 1 `
            -ReleaseLabel '1.14 RB03'
        $approvedOneFourteenTypes = [System.Collections.Generic.HashSet[string]]::new(
            [System.StringComparer]::Ordinal
        )
        foreach ($approvedType in $approvedRb01Types) {
            if (-not $approvedOneFourteenTypes.Add($approvedType)) {
                throw "Duplicate reviewed 1.14 Inspection public type across tranche ledgers: $approvedType"
            }
        }
        foreach ($approvedType in $approvedRb02Types) {
            if (-not $approvedOneFourteenTypes.Add($approvedType)) {
                throw "Duplicate reviewed 1.14 Inspection public type across tranche ledgers: $approvedType"
            }
        }
        foreach ($approvedType in $approvedRb03Types) {
            if (-not $approvedOneFourteenTypes.Add($approvedType)) {
                throw "Duplicate reviewed 1.14 Inspection public type across tranche ledgers: $approvedType"
            }
        }
        if ($approvedOneFourteenTypes.Count -ne 16) {
            throw "Reviewed 1.14 Inspection public type set must contain exactly 16 types through RB03; found $($approvedOneFourteenTypes.Count)."
        }

        $oneThirteenCandidate = Remove-ApprovedTypes `
            -Manifest $current `
            -ApprovedTypes $approvedOneFourteenTypes
        $oneThirteenCandidateSha256 = Get-NormalizedSha256 -Text $oneThirteenCandidate.Manifest
        if (-not [string]::Equals(
            $oneThirteenApiSha256,
            $oneThirteenCandidateSha256,
            [System.StringComparison]::Ordinal
        )) {
            throw "Icod.TermInfo.Inspection reconstructed 1.13 public API fingerprint changed. Expected $oneThirteenApiSha256, actual $oneThirteenCandidateSha256."
        }

        $freeze = [System.IO.File]::ReadAllText($freezePath)
        if ($freeze.IndexOf($oneThirteenApiSha256, [System.StringComparison]::Ordinal) -lt 0) {
            throw '1.13.0-INSPECTION-PUBLIC-API-FREEZE.md does not record the expected whole-surface fingerprint.'
        }

        Write-Host (
            "Verified exact 1.13 Inspection public API SHA-256 {0} after excluding {1} approved 1.14 type block(s): {2} RB01, {3} RB02, and {4} RB03." -f `
                $oneThirteenCandidateSha256, `
                $oneThirteenCandidate.RemovedTypeCount, `
                $approvedRb01Types.Count, `
                $approvedRb02Types.Count, `
                $approvedRb03Types.Count
        )

        [System.IO.File]::WriteAllText(
            $reconstructedOneThirteenManifestPath,
            (Normalize-Text -Text $oneThirteenCandidate.Manifest)
        )
        & $historyVerifierPath `
            -Configuration $Configuration `
            -ManifestPath $reconstructedOneThirteenManifestPath
        if (0 -ne $LASTEXITCODE) {
            throw "Historical Inspection compatibility verification exited with status $LASTEXITCODE."
        }

        Write-Host "Verified the established historical Inspection reconstruction through frozen 1.10 from exact reconstructed 1.13."
    } finally {
        if (Test-Path -LiteralPath $temporaryManifest) {
            Remove-Item -LiteralPath $temporaryManifest -Force
        }
        if (Test-Path -LiteralPath $reconstructedOneThirteenManifestPath) {
            Remove-Item -LiteralPath $reconstructedOneThirteenManifestPath -Force
        }
    }
} finally {
    Pop-Location
}
