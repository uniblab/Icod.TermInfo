$ErrorActionPreference = 'Stop'

function Assert-ScopeResult {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Expected,

        [Parameter(Mandatory = $true)]
        [hashtable] $Arguments,

        [Parameter(Mandatory = $true)]
        [string] $CaseName
    )

    $actual = & "$PSScriptRoot/Get-Hdb00ChangeScope.ps1" @Arguments
    if ($actual -cne $Expected) {
        throw "$CaseName expected '$Expected', got '$actual'."
    }
}

$cases = @(
    @{ Paths = @('Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashReader.cs'); Expected = 'true' },
    @{ Paths = @('tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/src/NativeOracleTests.cs'); Expected = 'true' },
    @{ Paths = @('tools/hdb00/run-linux.sh'); Expected = 'true' },
    @{ Paths = @('.github/workflows/hdb00-interoperability.yml'); Expected = 'true' },
    @{ Paths = @('Directory.Build.props'); Expected = 'true' },
    @{ Paths = @('tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb08PackagingQualificationTests.cs'); Expected = 'true' },
    @{ Paths = @('docs/1.15.0-HDB08-PACKAGING-AND-CROSS-PLATFORM-QUALIFICATION.md'); Expected = 'true' },
    @{ Paths = @('tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb09ReleaseClosureTests.cs'); Expected = 'true' },
    @{ Paths = @('docs/1.15.0-BERKELEYDB-PUBLIC-API-BASELINE.txt'); Expected = 'true' },
    @{ Paths = @('docs/1.15.0-BERKELEY-DB-PUBLIC-API-FREEZE.md'); Expected = 'true' },
    @{ Paths = @('docs/1.15.0-RELEASE-AUDIT.md'); Expected = 'true' },
    @{ Paths = @('samples/Icod.TermInfo.BerkeleyDb.Sample/Program.cs'); Expected = 'true' },
    @{ Paths = @('tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb07StorageHardeningTests.cs'); Expected = 'false' },
    @{ Paths = @('docs/superpowers/plans/2026-09-16-hdb07-adversarial-compatibility-hardening.md'); Expected = 'false' },
    @{ Paths = @('README.md'); Expected = 'false' },
    @{ Paths = @('Icod.TermInfo.BerkeleyDb/README.md'); Expected = 'false' },
    @{ Paths = @('infocmp/README.md'); Expected = 'false' }
)

foreach ($case in $cases) {
    $displayPaths = $case.Paths -join ','
    Assert-ScopeResult -Expected $case.Expected -CaseName "Explicit paths '$displayPaths'" -Arguments @{
        ChangedPaths = $case.Paths
    }
}

Assert-ScopeResult -Expected 'true' -CaseName 'Mixed documentation and production paths' -Arguments @{
    ChangedPaths = @('README.md', 'Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashReader.cs')
}

Assert-ScopeResult -Expected 'false' -CaseName 'Empty explicit path list' -Arguments @{
    ChangedPaths = @()
}

Assert-ScopeResult -Expected 'true' -CaseName 'Manual dispatch' -Arguments @{
    EventName = 'workflow_dispatch'
}

Assert-ScopeResult -Expected 'true' -CaseName 'Non-synchronize pull request action' -Arguments @{
    EventName = 'pull_request'
    Action = 'opened'
}

Write-Host 'HDB00 change-scope classification passed.'
