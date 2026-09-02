# Releasing Icod.TermInfo

This document describes the current validation and publication procedure for the
coordinated `Icod.TermInfo` package and tool family.

The repository follows the canonical Icod C#/.NET development lifecycle:

```text
local development
    -> Debug

pull request
    -> Staging

push to main
    -> Release

tagged release
    -> Release publication
```

The product-specific implementation of that lifecycle lives under
[`packaging/`](packaging/README.md). GitHub workflow YAML owns lifecycle and job
dependencies; `/packaging` owns coordinated package, archive, and verification
policy.

## Release invariants

`Directory.Build.props:IcodTermInfoSuiteVersion` is the sole coordinated suite
version.

The coordinated registry package set is:

```text
Icod.TermInfo
Icod.TermInfo.Source
Icod.TermInfo.Termcap
Icod.TermInfo.Compiler
Icod.TermInfo.Inspection
Icod.TermInfo.Tools
```

Runtime, Source, Termcap, Compiler, Inspection, and the router package must use
the same package version. The five reusable libraries retain the frozen 1.x
assembly version `1.0.0.0` and remain unsigned.

The reusable library packages target:

```text
net8.0
net9.0
net10.0
```

The standalone command projects and router target .NET 10. The standalone
commands remain non-packable:

```text
tic
infocmp
toe
captoinfo
infotocap
```

They are distributed through six framework-dependent archives:

```text
win-x64
win-arm64
linux-x64
linux-arm64
osx-x64
osx-arm64
```

`Icod.TermInfo.Tools` remains the separately published .NET tool package exposing
`icod-terminfo`.

Package contents are immutable after publication. If a package or release asset
changes, increment the version rather than replacing an already-published
version.

## Local development

Local development uses `Debug`.

Windows:

```text
build.cmd
```

Unix-like hosts:

```text
./build.sh
```

Both entry points delegate to `packaging/Invoke-Build.ps1` and run the canonical
local sequence:

```text
clean -> restore -> build -> test -> pack -> validate
```

The local build scripts do not silently elevate to `Staging` or `Release`.

For targeted package validation after an existing build, use:

```text
pwsh -File packaging/PackPackages.ps1 -Configuration Debug -OutputDirectory artifacts
pwsh -File packaging/VerifyPackageArtifact.ps1 -ArtifactDirectory artifacts -Configuration Debug
```

For a deliberate Release diagnostic, use:

```text
pwsh -File packaging/VerifyDistribution.ps1 -Configuration Release
```

Do not treat a local Release run as a substitute for the authoritative GitHub
`main` validation gate.

## Pull requests: Staging

`.github/workflows/pull-request.yaml` is the pull-request workflow.

It restores, builds, and tests the complete solution in `Staging` on:

```text
Windows
Linux
macOS
```

The Linux job is the distribution host. After its Staging build/test succeeds,
it reuses those outputs to:

1. pack the six coordinated registry packages;
2. verify the exact `.nupkg` / `.snupkg` artifacts through
   `packaging/VerifyPackageArtifact.ps1`;
3. build and structurally verify all six standalone tool-suite archives; and
4. upload the exact validated package and archive artifacts.

Separate Windows, Linux, and macOS jobs then smoke:

- installation and routing of the exact `Icod.TermInfo.Tools` package; and
- the matching-host standalone tool-suite archive.

The PR workflow is validation-only. It has no package-registry publication
permission and must never publish.

A pull request answers:

> Is this proposed source state safe to merge?

## Pushes to main: Release

`.github/workflows/main.yaml` is the authoritative Release validation workflow.

A push to `main` builds and tests `Release` on six OS/architecture runners:

```text
Windows x64
Windows ARM64
Linux x64
Linux ARM64
macOS x64
macOS ARM64
```

The Linux x64 matrix member is the Release distribution host. It reuses the
Release build that just passed on that runner to:

1. pack and verify the six coordinated package artifacts;
2. build and structurally verify the six standalone archives; and
3. upload both validated distribution forms.

Windows, Linux, and macOS smoke jobs execute the exact package and archive
artifacts produced by that Release gate.

The `main` workflow is validation-only. It does not authenticate to NuGet.org or
publish to GitHub Packages.

A successful PR Staging run is not a substitute for this Release gate.

## Manual distribution validation

`.github/workflows/distribution-validation.yaml` is a manual diagnostic workflow.
It is not another automatic branch or PR gate.

Use it when a selected Debug, Staging, or Release distribution check is useful
without changing the normal lifecycle.

## Tagged releases

`.github/workflows/release.yaml` is the sole automated publication workflow.

A release tag must have the form:

```text
v<semver>
```

and the tag version must exactly match
`Directory.Build.props:IcodTermInfoSuiteVersion`.

The tagged commit must be contained in `main`. It does not have to remain the
current `main` HEAD after later commits have landed.

Do not create a release tag until the intended source commit has passed the
authoritative `main` Release validation.

### Release artifact production

The tag workflow independently produces:

- the exact Release package set; and
- the exact six-RID standalone archive set.

Package production and archive production do not depend on each other.

The package path restores, builds, tests, packs, and runs the complete exact
package verifier. The archive path builds and structurally verifies the six
standalone distributions.

Before publication, matching-host Windows/Linux/macOS jobs smoke both:

- `Icod.TermInfo.Tools`; and
- the standalone archive distribution.

### Registry publication

After the exact package and both execution-smoke gates pass, NuGet.org and
GitHub Packages publish the same validated `.nupkg` artifacts in parallel.

NuGet.org uses Trusted Publishing through GitHub OIDC. The repository must have:

- a GitHub environment named `Release`;
- a `NUGET_USER` secret identifying the authorized NuGet.org account; and
- NuGet.org Trusted Publishing policy covering this repository,
  `.github/workflows/release.yaml`, the `Release` environment, and all six
  coordinated package IDs.

GitHub Packages uses the repository `GITHUB_TOKEN` with `packages: write`.

Both registry paths use immutable package versions and `--skip-duplicate` so a
recoverable partial publication can be rerun safely.

### GitHub Release

GitHub Release creation is the final rendezvous. It waits for:

- package production and verification;
- archive production and verification;
- NuGet.org publication; and
- GitHub Packages publication.

The release contains:

- six `.nupkg` files;
- five reusable-library `.snupkg` files;
- six standalone tool-suite archives; and
- `SHA256SUMS.txt`.

That is 17 package/archive files before checksumming and 18 GitHub Release assets
after the checksum manifest is added.

Prerelease package versions create GitHub prereleases.

## What the coordinated package verifier preserves

`packaging/VerifyPackageArtifact.ps1` delegates to the repository's established
deep verifier. The migration of CI/CD orchestration does not weaken the existing
TermInfo release contract.

Among other checks, the coordinated verifier retains:

- deterministic generated capability metadata;
- the frozen Runtime public API baseline;
- Source public API baseline and cross-framework equality;
- Termcap public API baseline, reflection fingerprint, and structural verifier;
- Compiler public API baseline and cross-framework equality;
- Inspection historical/current public API gates and cross-framework equality;
- package dependency and metadata validation;
- XML documentation, portable symbols, and Source Link validation;
- isolated package-reference consumers for Runtime, Source, Termcap, Compiler,
  and Inspection on all supported target frameworks;
- structural validation of `Icod.TermInfo.Tools`;
- the non-interactive general sample; and
- the deterministic Source -> Compiler -> database -> Inspection toolchain
  sample.

Matching-host router and standalone-archive execution smoke remain separate from
structural package verification so both distribution forms are exercised as
users receive them.

## Release checklist

Before creating a release tag:

1. synchronize to the intended `main` commit;
2. confirm `IcodTermInfoSuiteVersion` is the intended version;
3. confirm all coordinated projects consume that version correctly;
4. confirm the reusable assemblies retain `AssemblyVersion` `1.0.0.0`;
5. confirm the exact `main` commit has a successful `.github/workflows/main.yaml`
   Release run;
6. confirm NuGet Trusted Publishing still authorizes all six package IDs;
7. confirm the `Release` environment and `NUGET_USER` configuration are valid;
8. create tag `v<version>` for that exact intended commit; and
9. push the tag once.

After publication:

1. confirm all six package IDs are visible at the expected version on
   NuGet.org;
2. confirm all six package IDs are visible at the expected version in GitHub
   Packages;
3. confirm the GitHub Release contains all 18 expected assets;
4. confirm `SHA256SUMS.txt` covers the 17 package/archive files;
5. confirm the release came from the expected immutable tag; and
6. record release-specific evidence in the appropriate versioned release audit.

## Historical release records

Historical roadmaps, contract audits, and release audits remain historical
evidence and should not be rewritten merely because the current CI/CD machinery
changes.

`docs/RELEASING.md` intentionally preserves the older pre-1.5 procedure for
historical links. This root `RELEASING.md` is the authoritative current
procedure.

For implementation details of the current build/distribution tooling, see
[`packaging/README.md`](packaging/README.md).
