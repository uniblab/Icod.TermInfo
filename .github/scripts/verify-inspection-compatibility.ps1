param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Debug', 'Staging', 'Release')]
    [string]$Configuration,

    [Parameter(Mandatory = $true)]
    [string]$AssemblyPath
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$baselinePath = Join-Path $repositoryRoot 'docs\1.10.0-INSPECTION-PUBLIC-API-BASELINE.txt'
$assemblyFullPath = if ([System.IO.Path]::IsPathRooted($AssemblyPath)) {
    [System.IO.Path]::GetFullPath($AssemblyPath)
} else {
    [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $AssemblyPath))
}

if (-not (Test-Path -LiteralPath $baselinePath -PathType Leaf)) {
    throw "Frozen 1.10 Inspection API baseline not found: $baselinePath"
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

function Remove-ApprovedOneElevenTypes {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Manifest
    )

    $lines = (Normalize-Text -Text $Manifest).Split("`n")
    $result = [System.Collections.Generic.List[string]]::new()
    $skipBlock = $false
    $skipTrailingBlank = $false
    $removedTypeCount = 0

    foreach ($line in $lines) {
        if ($skipTrailingBlank) {
            if ($line.Length -eq 0) {
                $skipTrailingBlank = $false
                continue
            }
            $skipTrailingBlank = $false
        }

        if (-not $skipBlock -and $line.StartsWith('TYPE ', [System.StringComparison]::Ordinal)) {
            $skipBlock = $line -match '^TYPE\s+\S+\s+Icod\.TermInfo\.Inspection\.PersistentRasterLifecycle'
            if ($skipBlock) {
                $removedTypeCount++
                continue
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

    return [PSCustomObject]@{
        Manifest = Normalize-Text -Text ($result -join "`n")
        RemovedTypeCount = $removedTypeCount
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
        $filtered = Remove-ApprovedOneElevenTypes -Manifest $current

        if (-not [string]::Equals($frozen, $filtered.Manifest, [System.StringComparison]::Ordinal)) {
            throw 'Icod.TermInfo.Inspection changed the frozen 1.10 public API outside approved PersistentRasterLifecycle* additions.'
        }

        Write-Host ("Verified frozen 1.10 Inspection API compatibility after excluding {0} approved 1.11 PersistentRasterLifecycle* type block(s)." -f $filtered.RemovedTypeCount)
    } finally {
        if (Test-Path -LiteralPath $temporaryManifest) {
            Remove-Item -LiteralPath $temporaryManifest -Force
        }
    }
} finally {
    Pop-Location
}
