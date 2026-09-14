param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Debug', 'Staging', 'Release')]
    [string]$Configuration,

    [Parameter(Mandatory = $true, ParameterSetName = 'Assembly')]
    [string]$AssemblyPath,

    [Parameter(Mandatory = $true, ParameterSetName = 'Manifest')]
    [string]$ManifestPath
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = [System.IO.Path]::GetFullPath(
    (Join-Path (Join-Path $PSScriptRoot '..') '..')
)
$baselinePath = Join-Path $repositoryRoot 'docs/1.10.0-INSPECTION-PUBLIC-API-BASELINE.txt'
$oneElevenTypesPath = Join-Path $repositoryRoot 'docs/1.11.0-INSPECTION-PUBLIC-API-ADDITIONS.txt'
$oneElevenMembersPath = Join-Path $repositoryRoot 'docs/1.11.0-INSPECTION-PUBLIC-API-ADDITIVE-MEMBERS.txt'
$oneTwelveTypesPath = Join-Path $repositoryRoot 'docs/1.12.0-PG01-INSPECTION-PUBLIC-API-ADDITIONS.txt'
$oneTwelveMembersPath = Join-Path $repositoryRoot 'docs/1.12.0-PG06-INSPECTION-PUBLIC-API-ADDITIVE-MEMBERS.txt'
$oneThirteenTypesPath = Join-Path $repositoryRoot 'docs/1.13.0-RE01-INSPECTION-PUBLIC-API-ADDITIONS.txt'
$oneThirteenMembersPath = Join-Path $repositoryRoot 'docs/1.13.0-RE06-INSPECTION-PUBLIC-API-ADDITIVE-MEMBERS.txt'
$oneElevenApiSha256 = '69c7350d5d44d502ecf1698c8fe1c1336f03d38eb1a36e36219f50ac33585a86'
$oneTwelveApiSha256 = 'f71501dcd27a530051c1a02083325144ced2b6173b6b967b9571c620815198f0'
$inputFullPath = if ($PSCmdlet.ParameterSetName -eq 'Assembly') {
    if ([System.IO.Path]::IsPathRooted($AssemblyPath)) {
        [System.IO.Path]::GetFullPath($AssemblyPath)
    } else {
        [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $AssemblyPath))
    }
} else {
    if ([System.IO.Path]::IsPathRooted($ManifestPath)) {
        [System.IO.Path]::GetFullPath($ManifestPath)
    } else {
        [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $ManifestPath))
    }
}

foreach ($requiredPath in @(
    $baselinePath,
    $oneElevenTypesPath,
    $oneElevenMembersPath,
    $oneTwelveTypesPath,
    $oneTwelveMembersPath,
    $oneThirteenTypesPath,
    $oneThirteenMembersPath,
    $inputFullPath
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
        [string]$RequiredPrefix,

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
        if (-not $candidate.StartsWith($RequiredPrefix, [System.StringComparison]::Ordinal)) {
            throw "Approved $ReleaseLabel Inspection public type is outside the required prefix: $candidate"
        }
        if (-not $approved.Add($candidate)) {
            throw "Approved $ReleaseLabel Inspection public types file contains a duplicate: $candidate"
        }
    }

    if ($approved.Count -eq 0) {
        throw "Approved $ReleaseLabel Inspection public types file is empty."
    }

    return $approved
}

function Read-ApprovedRendererMembers {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$RequiredToken,

        [Parameter(Mandatory = $true)]
        [string]$ReleaseLabel
    )

    $approved = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::Ordinal
    )
    foreach ($line in [System.IO.File]::ReadAllLines($Path)) {
        $candidate = $line.TrimEnd()
        $classification = $candidate.Trim()
        if ($classification.Length -eq 0 -or $classification.StartsWith('#', [System.StringComparison]::Ordinal)) {
            continue
        }

        $isField = $candidate.StartsWith('  FIELD ', [System.StringComparison]::Ordinal)
        $isMethod = $candidate.StartsWith('  METHOD ', [System.StringComparison]::Ordinal)
        if (-not $isField -and -not $isMethod) {
            throw "Approved $ReleaseLabel additive API member is not a public API manifest field or method line: $candidate"
        }
        if ($candidate.IndexOf($RequiredToken, [System.StringComparison]::Ordinal) -lt 0) {
            throw "Approved $ReleaseLabel additive API member is outside the required renderer surface: $candidate"
        }
        if (-not $approved.Add($candidate)) {
            throw "Approved $ReleaseLabel additive API members file contains a duplicate member: $candidate"
        }
    }

    if ($approved.Count -eq 0) {
        throw "Approved $ReleaseLabel additive API members file is empty."
    }

    return $approved
}

function Remove-ApprovedRendererMembers {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Manifest,

        [Parameter(Mandatory = $true)]
        [System.Collections.Generic.HashSet[string]]$ApprovedMembers,

        [Parameter(Mandatory = $true)]
        [string]$RequiredToken,

        [Parameter(Mandatory = $true)]
        [string]$ReleaseLabel
    )

    $rendererTypeHeader = 'TYPE class Icod.TermInfo.Inspection.TermInfoJsonRenderer [static]'
    $lines = (Normalize-Text -Text $Manifest).Split("`n")
    $result = [System.Collections.Generic.List[string]]::new()
    $removedMembers = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::Ordinal
    )
    $insideRenderer = $false

    foreach ($line in $lines) {
        if ($line -eq $rendererTypeHeader) {
            $insideRenderer = $true
            $result.Add($line)
            continue
        }

        if ($insideRenderer -and $line -eq 'END') {
            $insideRenderer = $false
            $result.Add($line)
            continue
        }

        $matchesToken = $line.IndexOf($RequiredToken, [System.StringComparison]::Ordinal) -ge 0
        if ($insideRenderer -and $matchesToken) {
            if (-not $ApprovedMembers.Contains($line)) {
                throw "Unapproved $ReleaseLabel additive public member on TermInfoJsonRenderer: $line"
            }
            if (-not $removedMembers.Add($line)) {
                throw "Inspection API manifest contains duplicate approved $ReleaseLabel member lines: $line"
            }
            continue
        }

        $result.Add($line)
    }

    foreach ($approvedMember in $ApprovedMembers) {
        if (-not $removedMembers.Contains($approvedMember)) {
            throw "Approved $ReleaseLabel additive public API member is missing from the current assembly: $approvedMember"
        }
    }

    return [PSCustomObject]@{
        Manifest = Normalize-Text -Text ($result -join "`n")
        RemovedMemberCount = $removedMembers.Count
    }
}

function Remove-ApprovedTypes {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Manifest,

        [Parameter(Mandatory = $true)]
        [System.Collections.Generic.HashSet[string]]$ApprovedTypes,

        [Parameter(Mandatory = $true)]
        [string]$RequiredPrefix,

        [Parameter(Mandatory = $true)]
        [string]$ReleaseLabel
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
                if ($typeName.StartsWith($RequiredPrefix, [System.StringComparison]::Ordinal)) {
                    if (-not $ApprovedTypes.Contains($typeName)) {
                        throw "Unapproved $ReleaseLabel Inspection public API addition: $typeName"
                    }
                    if (-not $removedTypes.Add($typeName)) {
                        throw "Inspection API manifest contains duplicate public type blocks: $typeName"
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
        throw "Inspection API manifest ended inside an approved $ReleaseLabel type block."
    }

    foreach ($approvedType in $ApprovedTypes) {
        if (-not $removedTypes.Contains($approvedType)) {
            throw "Approved $ReleaseLabel Inspection public API type is missing from the current assembly: $approvedType"
        }
    }

    return [PSCustomObject]@{
        Manifest = Normalize-Text -Text ($result -join "`n")
        RemovedTypeCount = $removedTypes.Count
    }
}

Push-Location $repositoryRoot
try {
    $temporaryManifest = $null
    try {
        if ($PSCmdlet.ParameterSetName -eq 'Assembly') {
            $temporaryManifest = Join-Path (
                [System.IO.Path]::GetTempPath()
            ) ("Icod.TermInfo.Inspection-api-{0}.txt" -f [Guid]::NewGuid().ToString('N'))
            & dotnet run `
                --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj `
                -c $Configuration `
                --no-build `
                -- `
                --write `
                $temporaryManifest `
                $inputFullPath
            if (0 -ne $LASTEXITCODE) {
                throw "Public API snapshot generation exited with status $LASTEXITCODE."
            }

            $current = [System.IO.File]::ReadAllText($temporaryManifest)
        } else {
            $current = [System.IO.File]::ReadAllText($inputFullPath)
        }

        $frozen = Normalize-Text -Text ([System.IO.File]::ReadAllText($baselinePath))

        $approvedOneThirteenMembers = Read-ApprovedRendererMembers `
            -Path $oneThirteenMembersPath `
            -RequiredToken 'PersistentRasterRuntime' `
            -ReleaseLabel '1.13 RE06'
        $oneThirteenMemberFiltered = Remove-ApprovedRendererMembers `
            -Manifest $current `
            -ApprovedMembers $approvedOneThirteenMembers `
            -RequiredToken 'PersistentRasterRuntime' `
            -ReleaseLabel '1.13 RE06'

        $approvedOneThirteenTypes = Read-ApprovedTypes `
            -Path $oneThirteenTypesPath `
            -RequiredPrefix 'Icod.TermInfo.Inspection.PersistentRasterRuntime' `
            -ReleaseLabel '1.13'
        $oneTwelveCandidate = Remove-ApprovedTypes `
            -Manifest $oneThirteenMemberFiltered.Manifest `
            -ApprovedTypes $approvedOneThirteenTypes `
            -RequiredPrefix 'Icod.TermInfo.Inspection.PersistentRasterRuntime' `
            -ReleaseLabel '1.13'
        $oneTwelveCandidateSha256 = Get-NormalizedSha256 -Text $oneTwelveCandidate.Manifest
        if (-not [string]::Equals($oneTwelveApiSha256, $oneTwelveCandidateSha256, [System.StringComparison]::Ordinal)) {
            throw "Icod.TermInfo.Inspection reconstructed 1.12 public API fingerprint changed. Expected $oneTwelveApiSha256, actual $oneTwelveCandidateSha256."
        }

        $approvedOneTwelveMembers = Read-ApprovedRendererMembers `
            -Path $oneTwelveMembersPath `
            -RequiredToken 'PersistentRasterPlacement' `
            -ReleaseLabel '1.12 PG06'
        $oneTwelveMemberFiltered = Remove-ApprovedRendererMembers `
            -Manifest $oneTwelveCandidate.Manifest `
            -ApprovedMembers $approvedOneTwelveMembers `
            -RequiredToken 'PersistentRasterPlacement' `
            -ReleaseLabel '1.12 PG06'

        $approvedOneTwelveTypes = Read-ApprovedTypes `
            -Path $oneTwelveTypesPath `
            -RequiredPrefix 'Icod.TermInfo.Inspection.PersistentRasterPlacement' `
            -ReleaseLabel '1.12'
        $oneElevenCandidate = Remove-ApprovedTypes `
            -Manifest $oneTwelveMemberFiltered.Manifest `
            -ApprovedTypes $approvedOneTwelveTypes `
            -RequiredPrefix 'Icod.TermInfo.Inspection.PersistentRasterPlacement' `
            -ReleaseLabel '1.12'
        $oneElevenCandidateSha256 = Get-NormalizedSha256 -Text $oneElevenCandidate.Manifest
        if (-not [string]::Equals($oneElevenApiSha256, $oneElevenCandidateSha256, [System.StringComparison]::Ordinal)) {
            throw "Icod.TermInfo.Inspection reconstructed 1.11 public API fingerprint changed. Expected $oneElevenApiSha256, actual $oneElevenCandidateSha256."
        }

        $approvedOneElevenMembers = Read-ApprovedRendererMembers `
            -Path $oneElevenMembersPath `
            -RequiredToken 'PersistentRasterLifecycle' `
            -ReleaseLabel '1.11'
        $oneElevenMemberFiltered = Remove-ApprovedRendererMembers `
            -Manifest $oneElevenCandidate.Manifest `
            -ApprovedMembers $approvedOneElevenMembers `
            -RequiredToken 'PersistentRasterLifecycle' `
            -ReleaseLabel '1.11'
        $approvedOneElevenTypes = Read-ApprovedTypes `
            -Path $oneElevenTypesPath `
            -RequiredPrefix 'Icod.TermInfo.Inspection.PersistentRasterLifecycle' `
            -ReleaseLabel '1.11'
        $filtered = Remove-ApprovedTypes `
            -Manifest $oneElevenMemberFiltered.Manifest `
            -ApprovedTypes $approvedOneElevenTypes `
            -RequiredPrefix 'Icod.TermInfo.Inspection.PersistentRasterLifecycle' `
            -ReleaseLabel '1.11'

        if (-not [string]::Equals($frozen, $filtered.Manifest, [System.StringComparison]::Ordinal)) {
            throw 'Icod.TermInfo.Inspection changed the frozen 1.10 public API outside explicitly approved 1.11, 1.12, and 1.13 additions.'
        }

        Write-Host (
            "Verified reconstructed exact 1.12 Inspection public API SHA-256 {0} after excluding {1} approved 1.13 type block(s) and {2} RE06 renderer member(s)." -f `
                $oneTwelveCandidateSha256, `
                $oneTwelveCandidate.RemovedTypeCount, `
                $oneThirteenMemberFiltered.RemovedMemberCount
        )
        Write-Host (
            "Verified reconstructed exact 1.11 Inspection public API SHA-256 {0} after excluding {1} approved 1.12 type block(s) and {2} PG06 renderer member(s)." -f `
                $oneElevenCandidateSha256, `
                $oneElevenCandidate.RemovedTypeCount, `
                $oneTwelveMemberFiltered.RemovedMemberCount
        )
        Write-Host (
            "Verified frozen 1.10 Inspection API compatibility after excluding {0} explicitly approved 1.11 public type block(s) and {1} additive member(s)." -f `
                $filtered.RemovedTypeCount, `
                $oneElevenMemberFiltered.RemovedMemberCount
        )
    } finally {
        if ($null -ne $temporaryManifest) {
            if (Test-Path -LiteralPath $temporaryManifest) {
                Remove-Item -LiteralPath $temporaryManifest -Force
            }
        }
    }
} finally {
    Pop-Location
}
