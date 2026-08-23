# Releasing Icod.TermInfo

This document describes the current validation and publication procedure for
`Icod.TermInfo` releases built from this repository.

## Release principles

- `<Version />` and `<PackageVersion />` in `Icod.TermInfo.csproj` must always be
  present and identical.
- A final release tag must be exactly `v<PackageVersion>`.
- Release validation must pass on Windows, Linux, and macOS before a final tag is
  created.
- The package must pass the release verifier before publication. Use
  `.github/scripts/verify-release-package.sh` on a Bash-capable host or
  `.github/scripts/verify-release-package.cmd` from Windows Command Prompt.
- Release packages must retain deterministic build metadata, repository commit
  metadata, portable symbols, and Source Link information.
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
the sample and solution-contained maintenance tools.

### Pushes to main and the active 0.9.0 release branch

`.github/workflows/push-main.yaml` runs for pushes to `main` and the active
`0.9.0` release branch. It executes the Release build/test matrix on:

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
2. run the C# package verifier for package structure, dependency closure, package
   metadata, Source Link, portable symbols, and the generic-parameterization
   architecture guard;
3. copy the package-reference-only smoke consumer to a temporary directory;
4. restore that consumer from the local artifact directory with an isolated
   NuGet package cache;
5. execute the smoke consumer against the packed `Icod.TermInfo` package;
6. run the repository sample through its non-interactive `--describe-only` path.

The checked-in package smoke project is intentionally not part of the solution
and contains no project reference to `Icod.TermInfo`.

For the 0.9 release line, the smoke consumer also creates a conventional
compiled entry at runtime and proves the packed package can:

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

The current `push-main.yaml` workflow publishes only after the Release matrix and
package-validation job succeed. It watches both `main` and the active `0.9.0`
release branch. The deploy job consumes the package artifact uploaded by package
validation rather than repacking the repository.

Before pushing a release-ready commit to `main` or `0.9.0`:

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

Historical completion evidence for 0.6.0, 0.7.0, and 0.8.0 remains under
`docs/*-CONTRACT-AUDIT.md`. The 0.9 release-candidate API/package freeze is
recorded in `docs/0.9.0-T40-API-PACKAGE-FREEZE.md`; T41 owns the final 0.9.0
completion evidence.
