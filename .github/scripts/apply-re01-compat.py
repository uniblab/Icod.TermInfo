from pathlib import Path


PATH = Path('.github/scripts/verify-inspection-compatibility.ps1')


def replace_once(text: str, old: str, new: str) -> str:
    count = text.count(old)
    if count != 1:
        raise RuntimeError(
            f'Expected exactly one compatibility-script match, found {count}: {old[:120]!r}'
        )
    return text.replace(old, new, 1)


text = PATH.read_text(encoding='utf-8')

text = replace_once(
    text,
    "$oneTwelveMembersPath = Join-Path $repositoryRoot 'docs/1.12.0-PG06-INSPECTION-PUBLIC-API-ADDITIVE-MEMBERS.txt'\n$oneElevenApiSha256",
    "$oneTwelveMembersPath = Join-Path $repositoryRoot 'docs/1.12.0-PG06-INSPECTION-PUBLIC-API-ADDITIVE-MEMBERS.txt'\n$oneThirteenTypesPath = Join-Path $repositoryRoot 'docs/1.13.0-RE01-INSPECTION-PUBLIC-API-ADDITIONS.txt'\n$oneElevenApiSha256",
)

text = replace_once(
    text,
    "    $oneTwelveTypesPath,\n    $oneTwelveMembersPath,\n    $assemblyFullPath",
    "    $oneTwelveTypesPath,\n    $oneTwelveMembersPath,\n    $oneThirteenTypesPath,\n    $assemblyFullPath",
)

text = replace_once(
    text,
    """        $frozen = Normalize-Text -Text ([System.IO.File]::ReadAllText($baselinePath))
        $current = [System.IO.File]::ReadAllText($temporaryManifest)
        $currentSha256 = Get-NormalizedSha256 -Text $current
        if (-not [string]::Equals($oneTwelveApiSha256, $currentSha256, [System.StringComparison]::Ordinal)) {
            throw "Icod.TermInfo.Inspection exact 1.12 public API fingerprint changed. Expected $oneTwelveApiSha256, actual $currentSha256."
        }

        $approvedOneTwelveMembers = Read-ApprovedRendererMembers `
""",
    """        $frozen = Normalize-Text -Text ([System.IO.File]::ReadAllText($baselinePath))
        $current = [System.IO.File]::ReadAllText($temporaryManifest)

        $approvedOneThirteenTypes = Read-ApprovedTypes `
            -Path $oneThirteenTypesPath `
            -RequiredPrefix 'Icod.TermInfo.Inspection.PersistentRasterRuntime' `
            -ReleaseLabel '1.13'
        $oneTwelveCandidate = Remove-ApprovedTypes `
            -Manifest $current `
            -ApprovedTypes $approvedOneThirteenTypes `
            -RequiredPrefix 'Icod.TermInfo.Inspection.PersistentRasterRuntime' `
            -ReleaseLabel '1.13'
        $oneTwelveCandidateSha256 = Get-NormalizedSha256 -Text $oneTwelveCandidate.Manifest
        if (-not [string]::Equals($oneTwelveApiSha256, $oneTwelveCandidateSha256, [System.StringComparison]::Ordinal)) {
            throw "Icod.TermInfo.Inspection reconstructed 1.12 public API fingerprint changed. Expected $oneTwelveApiSha256, actual $oneTwelveCandidateSha256."
        }

        $approvedOneTwelveMembers = Read-ApprovedRendererMembers `
""",
)

text = replace_once(
    text,
    """        $oneTwelveMemberFiltered = Remove-ApprovedRendererMembers `
            -Manifest $current `
""",
    """        $oneTwelveMemberFiltered = Remove-ApprovedRendererMembers `
            -Manifest $oneTwelveCandidate.Manifest `
""",
)

text = replace_once(
    text,
    """        Write-Host "Verified exact 1.12 Inspection public API SHA-256 $currentSha256."
        Write-Host (
            "Verified reconstructed exact 1.11 Inspection public API SHA-256 {0} after excluding {1} approved 1.12 type block(s) and {2} PG06 renderer member(s)." -f `
""",
    """        Write-Host (
            "Verified reconstructed exact 1.12 Inspection public API SHA-256 {0} after excluding {1} approved 1.13 type block(s)." -f `
                $oneTwelveCandidateSha256, `
                $oneTwelveCandidate.RemovedTypeCount
        )
        Write-Host (
            "Verified reconstructed exact 1.11 Inspection public API SHA-256 {0} after excluding {1} approved 1.12 type block(s) and {2} PG06 renderer member(s)." -f `
""",
)

PATH.write_text(text, encoding='utf-8', newline='\n')
