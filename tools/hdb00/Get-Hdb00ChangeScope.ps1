[CmdletBinding()]
param(
    [string] $EventName,
    [string] $Action,
    [string] $Before,
    [string] $After,
    [string] $RepositoryRoot = (Get-Location).Path,
    [AllowEmptyCollection()]
    [string[]] $ChangedPaths
)

$ErrorActionPreference = 'Stop'

$patterns = @(
    '^\.github/workflows/hdb00-interoperability\.yml$',
    '^tools/hdb00/',
    '^tools/hdb07-permission-probe/',
    '^\.github/scripts/verify-hdb07-permissions\.ps1$',
    '^\.github/scripts/verify-berkeleydb-package\.ps1$',
    '^Icod\.TermInfo\.BerkeleyDb/(src/|Icod\.TermInfo\.BerkeleyDb\.csproj$)',
    '^tests/Icod\.TermInfo\.BerkeleyDb\.Tests/src/Hdb08PackagingQualificationTests\.cs$',
    '^tests/Icod\.TermInfo\.BerkeleyDb\.Tests/src/Hdb09ReleaseClosureTests\.cs$',
    '^tests/Icod\.TermInfo\.BerkeleyDb\.Interop\.Tests/',
    '^docs/1\.15\.0-HDB08-PACKAGING-AND-CROSS-PLATFORM-QUALIFICATION\.md$',
    '^docs/1\.15\.0-BERKELEYDB-PUBLIC-API-BASELINE\.txt$',
    '^docs/1\.15\.0-BERKELEY-DB-PUBLIC-API-FREEZE\.md$',
    '^docs/1\.15\.0-RELEASE-AUDIT\.md$',
    '^samples/Icod\.TermInfo\.BerkeleyDb\.Sample/',
    '^(infocmp|toe|icod-terminfo)/(src/|[^/]+\.csproj$)',
    '^\.github/scripts/(new-hdb06-test-store|smoke-tool-package|smoke-tool-archive)\.ps1$',
    '^Directory\.Build\.(props|targets)$',
    '^Icod\.TermInfo/(src/|Icod\.TermInfo\.csproj$)'
)

function Test-Hdb00SensitivePath {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    $normalizedPath = $Path.Replace('\\', '/')
    $options = [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
        [System.Text.RegularExpressions.RegexOptions]::CultureInvariant

    foreach ($pattern in $patterns) {
        if ([System.Text.RegularExpressions.Regex]::IsMatch($normalizedPath, $pattern, $options)) {
            return $true
        }
    }

    return $false
}

function Write-Hdb00Scope {
    param(
        [AllowEmptyCollection()]
        [string[]] $Paths
    )

    foreach ($path in $Paths) {
        if (Test-Hdb00SensitivePath -Path $path) {
            Write-Output 'true'
            return
        }
    }

    Write-Output 'false'
}

if ($PSBoundParameters.ContainsKey('ChangedPaths')) {
    Write-Hdb00Scope -Paths $ChangedPaths
    return
}

if (($EventName -cne 'pull_request') -or ($Action -cne 'synchronize')) {
    Write-Output 'true'
    return
}

if ([string]::IsNullOrWhiteSpace($Before) -or [string]::IsNullOrWhiteSpace($After)) {
    throw 'A pull-request synchronization requires both before and after commit identifiers.'
}

$gitOutput = @(
    & git -C $RepositoryRoot diff --name-only --diff-filter=ACMR $Before $After 2>&1
)
if ($LASTEXITCODE -ne 0) {
    throw "Unable to calculate the HDB00 synchronization delta: $($gitOutput -join [Environment]::NewLine)"
}

Write-Hdb00Scope -Paths $gitOutput
