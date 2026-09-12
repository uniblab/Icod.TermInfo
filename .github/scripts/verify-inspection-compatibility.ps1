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
$baselinePath = Join-Path $repositoryRoot 'docs/1.10.0-INSPECTION-PUBLIC-API-BASELINE.txt'
$approvedAdditionsPath = Join-Path $repositoryRoot 'docs/1.11.0-INSPECTION-PUBLIC-API-ADDITIONS.txt'
$approvedMembersPath = Join-Path $repositoryRoot 'docs/1.11.0-INSPECTION-PUBLIC-API-ADDITIVE-MEMBERS.txt'
$oneElevenApiSha256 = '69c7350d5d44d502ecf1698c8fe1c1336f03d38eb1a36e36219f50ac33585a86'
$assemblyFullPath = if ([System.IO.Path]::IsPathRooted($AssemblyPath)) {
    [System.IO.Path]::GetFullPath($AssemblyPath)
} else {
    [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $AssemblyPath))
}

if (-not (Test-Path -LiteralPath $baselinePath -PathType Leaf)) {
    throw "Frozen 1.10 Inspection API baseline not found: $baselinePath"
}
if (-not (Test-Path -LiteralPath $approvedAdditionsPath -PathType Leaf)) {
    throw "Approved 1.11 Inspection API additions file not found: $approvedAdditionsPath"
}
if (-not (Test-Path -LiteralPath $approvedMembersPath -PathType Leaf)) {
    throw "Approved 1.11 Inspection API additive members file not found: $approvedMembersPath"
}
if (-not (Test-Path -LiteralPath $assemblyFullPath -PathType Leaf)) {
    throw "Inspection assembly not found: $assemblyFullPath"
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
    return [Convert]::ToHexString(
        [System.Security.Cryptography.SHA256]::HashData($bytes)
    ).ToLowerInvariant()
}

function Read-ApprovedOneElevenTypes {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $approved = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::Ordinal
    )
    foreach ($line in [System.IO.File]::ReadAllLines($Path)) {
        $candidate = $line.Trim()
        if ($candidate.Length -eq 0 -or $candidate.StartsWith('#', [System.StringComparison]::Ordinal)) {
            continue
        }
        if (
            -not $candidate.StartsWith(
                'Icod.TermInfo.Inspection.PersistentRasterLifecycle',
                [System.StringComparison]::Ordinal
            )
        ) {
            throw "Approved 1.11 Inspection API addition is outside the persistent-raster lifecycle namespace: $candidate"
        }
        if (-not $approved.Add($candidate)) {
            throw "Approved 1.11 Inspection API additions file contains a duplicate type: $candidate"
        }
    }

    if ($approved.Count -eq 0) {
        throw 'Approved 1.11 Inspection API additions file is empty.'
    }

    return $approved
}

function Read-ApprovedOneElevenMembers {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
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
        if (-not $candidate.StartsWith('  FIELD ', [System.StringComparison]::Ordinal) -and -not $candidate.StartsWith('  METHOD ', [System.StringComparison]::Ordinal)) {
            throw "Approved 1.11 additive API member is not a public API manifest field or method line: $candidate"
        }
        if (
            -not $candidate.Contains(
                'PersistentRasterLifecycle',
                [System.StringComparison]::Ordinal
            )
        ) {
            throw "Approved 1.11 additive API member is outside the persistent-raster lifecycle surface: $candidate"
        }
        if (-not $approved.Add($candidate)) {
            throw "Approved 1.11 additive API members file contains a duplicate member: $candidate"
        }
    }

    if ($approved.Count -eq 0) {
        throw 'Approved 1.11 Inspection API additive members file is empty.'
    }

    return $approved
}

function Remove-ApprovedOneElevenMembers {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Manifest,

        [Parameter(Mandatory = $true)]
        [System.Collections.Generic.HashSet[string]]$ApprovedMembers
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

        if (
            $insideRenderer -and $line.Contains(
                'PersistentRasterLifecycle',
                [System.StringComparison]::Ordinal
            )
        ) {
            if (-not $ApprovedMembers.Contains($line)) {
                throw "Unapproved 1.11 additive public member on TermInfoJsonRenderer: $line"
            }
            if (-not $removedMembers.Add($line)) {
                throw "Inspection API manifest contains duplicate approved additive member lines: $line"
            }
            continue
        }

        $result.Add($line)
    }

    foreach ($approvedMember in $ApprovedMembers) {
        if (-not $removedMembers.Contains($approvedMember)) {
            throw "Approved 1.11 additive public API member is missing from the current assembly: $approvedMember"
        }
    }

    return [PSCustomObject]@{
        Manifest = Normalize-Text -Text ($result -join "`n")
        RemovedMemberCount = $removedMembers.Count
    }
}

function Remove-ApprovedOneElevenTypes {
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
                if (
                    $typeName.StartsWith(
                        'Icod.TermInfo.Inspection.PersistentRasterLifecycle',
                        [System.StringComparison]::Ordinal
                    )
                ) {
                    if (-not $ApprovedTypes.Contains($typeName)) {
                        throw "Unapproved 1.11 Inspection public API addition: $typeName"
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
        throw 'Inspection API manifest ended inside an approved 1.11 type block.'
    }

    foreach ($approvedType in $ApprovedTypes) {
        if (-not $removedTypes.Contains($approvedType)) {
            throw "Approved 1.11 Inspection public API type is missing from the current assembly: $approvedType"
        }
    }

    return [PSCustomObject]@{
        Manifest = Normalize-Text -Text ($result -join "`n")
        RemovedTypeCount = $removedTypes.Count
    }
}

Push-Location $repositoryRoot
try {
    $temporaryManifest = Join-Path ([System.IO.Path]::GetTempPath()) ("Icod.TermInfo.Inspection-api-{0}.txt" -f [Guid]::NewGuid().ToString('N'))
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

        $frozen = Normalize-Text -Text ([System.IO.File]::ReadAllText($baselinePath))
        $current = [System.IO.File]::ReadAllText($temporaryManifest)
        $currentSha256 = Get-NormalizedSha256 -Text $current
        if (-not [string]::Equals($oneElevenApiSha256, $currentSha256, [System.StringComparison]::Ordinal)) {
            throw "Icod.TermInfo.Inspection current 1.11 public API fingerprint changed. Expected $oneElevenApiSha256, actual $currentSha256."
        }

        $approvedMembers = Read-ApprovedOneElevenMembers -Path $approvedMembersPath
        $memberFiltered = Remove-ApprovedOneElevenMembers `
            -Manifest $current `
            -ApprovedMembers $approvedMembers
        $approvedTypes = Read-ApprovedOneElevenTypes -Path $approvedAdditionsPath
        $filtered = Remove-ApprovedOneElevenTypes `
            -Manifest $memberFiltered.Manifest `
            -ApprovedTypes $approvedTypes

        if (-not [string]::Equals($frozen, $filtered.Manifest, [System.StringComparison]::Ordinal)) {
            throw 'Icod.TermInfo.Inspection changed the frozen 1.10 public API outside explicitly approved 1.11 additions.'
        }

        Write-Host "Verified exact 1.11 Inspection public API SHA-256 $currentSha256."
        Write-Host (
            "Verified frozen 1.10 Inspection API compatibility after excluding {0} explicitly approved 1.11 public type block(s) and {1} additive member(s)." -f `
                $filtered.RemovedTypeCount, `
                $memberFiltered.RemovedMemberCount
        )
    } finally {
        if (Test-Path -LiteralPath $temporaryManifest) {
            Remove-Item -LiteralPath $temporaryManifest -Force
        }
    }
} finally {
    Pop-Location
}
