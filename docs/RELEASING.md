# Releasing Icod.TermInfo

This document describes the current validation and publication procedure for
`Icod.TermInfo` releases built from this repository.

## Release principles

- `<Version />` and `<PackageVersion />` in `Icod.TermInfo.csproj` must always be
  present and identical.
- The 1.x assembly identity remains `Icod.TermInfo, Version=1.0.0.0` and remains
  unsigned.
- Supported consumer targets are `net8.0` and `net10.0`.
- A final release tag must be exactly `v<PackageVersion>`.
- Release validation must pass on Windows, Linux, and macOS before a final tag is
  created.
- Release validation must pass the approved public API baseline and net8/net10
  API-equivalence gates.
- Release builds treat missing public XML documentation as an error.
- The package must pass the release verifier before publication. Use
  `.github/scripts/verify-release-package.sh` on a Bash-capable host or
  `.github/scripts/verify-release-package.cmd` from Windows Command Prompt.
- Release packages must retain deterministic build metadata, repository commit
  metadata, portable symbols, Source Link, README, icon metadata, and both
  framework XML-documentation assets.
- The `.nupkg` and `.snupkg` produced for a version are immutable release
  artifacts. If package contents change, increment the version rather than
  replacing a published package.
- Publication is downstream of build/test/package validation. A package must not
  be pushed if any validation stage fails.

## Repository CI

### Pull requests

`.github/workflows/pr-build-and-test.yaml` currently runs the solution in the
repository `Staging` configuration on:

- `windows-latest`;
- `ubuntu-latest`;
- `macos-latest`.

Each matrix job cleans, restores, builds, and tests the whole solution, including
both repository sample executables and solution-contained maintenance tools.

The pull-request workflow is validation-only. It must not pack or publish
packages, request publication credentials, or contain a deployment job.

### Pushes to main

`.github/workflows/push-main.yaml` runs only for pushes to `main`. It executes
the Release build/test matrix on:

- `windows-latest`;
- `ubuntu-latest`;
- `macos-latest`.

After that matrix succeeds, the Ubuntu package-validation job:

1. restores and builds Release with `ContinuousIntegrationBuild=true`;
2. runs the Release test suite;
3. packs `Icod.TermInfo.csproj` into `artifacts`;
4. runs `.github/scripts/verify-release-package.sh artifacts`;
5. uploads the exact `.nupkg` and `.snupkg` as workflow artifacts.

After package validation succeeds, the `Release` deployment job downloads those
exact artifacts and publishes the `.nupkg` to NuGet.org and GitHub Packages. The
NuGet.org push uses trusted publishing/OIDC through `NuGet/login`; GitHub Packages
uses the repository `GITHUB_TOKEN`. Both pushes use `--skip-duplicate`.

## What the release verifier checks

The Bash and CMD entry points perform equivalent validation. They:

1. run the deterministic standard-capability metadata generator in `--check`
   mode;
2. require the approved 1.0 public API baseline to match;
3. require exact public API equivalence between the built `net8.0` and
   `net10.0` assemblies;
4. run the C# package verifier for package structure, dependency closure,
   README/icon/license metadata, XML documentation, Source Link, portable
   symbols, and the generic-parameterization architecture guard;
5. copy the package-reference-only smoke consumer to a temporary directory;
6. restore that consumer from the local artifact directory with an isolated
   NuGet package cache;
7. execute the smoke consumer separately against `net8.0` and `net10.0`;
8. run the general repository sample through its non-interactive
   `--describe-only` path.

Both repository sample executables are solution projects and therefore compile
in every CI matrix. The focused acquisition sample is not automatically run
against the host database because its `system` command intentionally inspects
host-specific terminfo state; the isolated package-smoke consumer supplies the
deterministic acquisition acceptance test instead.

The checked-in package smoke project is intentionally not part of the solution
and contains no project reference to `Icod.TermInfo`.

The smoke consumer creates a conventional compiled entry at runtime and proves
the packed package can:

- parse caller-supplied compiled bytes;
- load an explicit conventional directory tree;
- construct a fully restricted system provider;
- load through the public system provider from a snapshotted `TERMINFO` root;
- compose system lookup with `TerminalDatabase.BuiltIn` fallback.

No checked-in fixture is copied into the smoke project, so those checks prove the
public package surface rather than repository-only test assets.

## Local release validation

Build, test, and pack Release first:

```text
dotnet restore Icod.TermInfo.sln
dotnet build Icod.TermInfo.sln -c Release
dotnet test Icod.TermInfo.sln -c Release
dotnet pack Icod.TermInfo.csproj -c Release --output artifacts
```

Then run the package verifier appropriate to the host.

On Windows Command Prompt:

```text
.github\scripts\verify-release-package.cmd artifacts
```

On a Bash-capable host:

```text
bash .github/scripts/verify-release-package.sh artifacts
```

Both wrappers are intended to provide the same validation contract.

## Automated publication

The current `push-main.yaml` workflow watches only `main` and publishes only
after the Release matrix and package-validation job succeed. Pull-request and
development-branch pushes do not publish packages. The deploy job consumes the
package artifact uploaded by package validation rather than repacking the
repository.

Before merging or pushing a release-ready commit to `main`:

1. confirm `<Version />` and `<PackageVersion />` are the intended version;
2. ensure the NuGet.org trusted-publishing policy authorizes this repository,
   workflow, and `Release` environment;
3. ensure the `NUGET_USER` repository secret identifies the intended NuGet.org
   account;
4. preserve `packages: write` permission for GitHub Packages;
5. remember that a source/package change after validation creates a new release
   candidate and requires validation again.

For a final release, create tag `v<PackageVersion>` only for the exact commit
whose release validation and publication succeeded.

## NuGet.org authentication

NuGet.org publication uses trusted publishing. The workflow obtains a temporary
API key through GitHub OIDC and does not require a committed or long-lived NuGet
API key.

Do not commit NuGet credentials to the repository.

## GitHub Packages authentication

GitHub Actions publication uses the repository `GITHUB_TOKEN` with
`packages: write`. Manual publication, whenever required, should use an
appropriately scoped credential and must never place that credential in source
control.

## After publication

After a final version is published:

- confirm the package and symbols are visible on NuGet.org;
- confirm the same package version is visible in GitHub Packages;
- confirm a fresh public-package consumer can restore the version;
- create/verify the exact final tag;
- treat subsequent public API or package-content changes as changes for the next
  version.

Historical completion evidence for 0.6.0 through 0.9.0 remains under the
versioned roadmap and contract-audit documents. Consumer-facing compiled-database
usage remains consolidated in `docs/0.9.0-ACQUISITION-GUIDE.md`.

For 1.0, use:

- `Icod.TermInfo-Development-Roadmap-1.0.0.md` for the final gate contract;
- `docs/1.0.0-T42-CONTRACT-API-AUDIT.md` for assembly/support/API-baseline policy;
- `docs/1.0.0-T43-ROBUSTNESS-COMPATIBILITY.md` for robustness/package metadata;
- `docs/1.0.0-T44-DOCUMENTATION-PACKAGE-FREEZE.md` for the RC freeze;
- `docs/1.0.0-CONTRACT-AUDIT.md` for final T45 release sign-off requirements;
- `docs/VERSIONING.md` and `docs/COMPATIBILITY.md` for the stable 1.x promises.

The final `v1.0.0` tag must identify the exact validated and published `main`
commit; do not edit the audit or any other source/package content after that
validation without rerunning the completion gate.
