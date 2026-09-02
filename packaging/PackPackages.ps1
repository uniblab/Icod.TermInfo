param(
    [ValidateSet('Debug', 'Staging', 'Release')]
    [string]$Configuration = 'Debug',

    [string]$OutputDirectory = 'artifacts'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
Import-Module (Join-Path $PSScriptRoot 'RepositoryTools.psm1') -Force

if (-not [System.IO.Path]::IsPathRooted($OutputDirectory)) {
    $OutputDirectory = Join-Path $repositoryRoot $OutputDirectory
}
$OutputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

$projects = @(
    'Icod.TermInfo.csproj',
    'Icod.TermInfo.Source/Icod.TermInfo.Source.csproj',
    'Icod.TermInfo.Termcap/Icod.TermInfo.Termcap.csproj',
    'Icod.TermInfo.Compiler/Icod.TermInfo.Compiler.csproj',
    'Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj',
    'icod-terminfo/Icod.TermInfo.Router.csproj'
)

Push-Location $repositoryRoot
try {
    foreach ($project in $projects) {
        Invoke-DotNet -Arguments @(
            'pack', $project,
            '-c', $Configuration,
            '--no-build',
            '--no-restore',
            '-o', $OutputDirectory,
            '-p:ContinuousIntegrationBuild=true'
        )
    }
} finally {
    Pop-Location
}
