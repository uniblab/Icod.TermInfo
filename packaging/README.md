# Icod.TermInfo build and distribution tooling

This directory adapts the canonical Icod C#/.NET build-cycle contract to the coordinated `Icod.TermInfo` distribution.

## Lifecycle

| Lifecycle | Configuration | Entry point |
| --- | --- | --- |
| local `build.cmd` / `build.sh` | `Debug` | `packaging/Invoke-Build.ps1` |
| pull request | `Staging` | `.github/workflows/pull-request.yaml` |
| push to `main` | `Release` | `.github/workflows/main.yaml` |
| manual diagnostic | selected | `.github/workflows/distribution-validation.yaml` |
| `v*` tag contained in `main` | `Release` | `.github/workflows/release.yaml` |

## Coordinated package set

`PackPackages.ps1` produces the seven coordinated registry packages from the already-built solution:

```text
Icod.TermInfo
Icod.TermInfo.Source
Icod.TermInfo.Termcap
Icod.TermInfo.Compiler
Icod.TermInfo.Inspection
Icod.TermInfo.BerkeleyDb
Icod.TermInfo.Tools
```

The six reusable libraries also produce symbol packages. Package versions remain authoritative in MSBuild through `IcodTermInfoSuiteVersion`.

`VerifyPackageArtifact.ps1` delegates to the repository's existing deep package contract, including API baselines, cross-target equivalence, fresh package consumers, structural checks, deterministic samples, and router validation.

For HDB08, it also invokes the managed
`Icod.TermInfo.BerkeleyDb.PackageVerifier`. That verifier requires the exact
BerkeleyDb nupkg/snupkg identity, matching Runtime-only dependencies, the
unsigned IL-only `1.0.0.0` assembly, portable PDBs with Source Link, and no
runtime/native payload. The PowerShell entry point is orchestration only; it
does not parse ZIP archives.

## Tool-suite archives

`BuildToolArchives.ps1` delegates to the existing deterministic archive builder and structural verifier. The six framework-dependent archives continue to contain the five traditional commands:

```text
tic
infocmp
toe
captoinfo
infotocap
```

for:

```text
win-x64
win-arm64
linux-x64
linux-arm64
osx-x64
osx-arm64
```

Matching-host execution smoke remains separate from structural archive verification.

## CI/CD dependency model

Pull requests build and test Staging on Windows, Linux, and macOS. Linux x64 produces the canonical Staging packages and archives; matching-host jobs smoke the exact artifacts. Each Windows, Linux, and macOS package-smoke host consumes the same BerkeleyDb package set on net8.0, net9.0, and net10.0 before installed-tool smoke. The six RID jobs retain matching-host archive execution.

`main` builds and tests Release on six OS/architecture runners. The Linux x64 matrix member reuses that validated build to pack and verify the canonical package artifacts and build the archive set rather than performing a second identical Release build.

Tagged releases independently produce the exact Release package and archive artifacts. Both distribution forms are smoke-tested before publication. NuGet.org and GitHub Packages consume the same validated package artifact and publish in parallel. GitHub Release creation remains the final rendezvous after both registries and the archive path succeed.

The tagged commit must be contained in `main`; it does not have to remain the current `main` HEAD after later commits have landed.
