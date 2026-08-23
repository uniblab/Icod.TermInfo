# Releasing Icod.TermInfo

This document describes the current validation and publication procedure for the `Icod.TermInfo` 0.8.0 release candidate and later releases built from the same repository structure.

## Release principles

- `<Version />` and `<PackageVersion />` in `Icod.TermInfo.csproj` must always be present and identical.
- A release tag must be exactly `v<PackageVersion>`.
- Debug and Release validation must pass on Windows, Linux, and macOS before a final tag is created.
- The package must pass `.github/scripts/verify-release-package.sh` before publication.
- Release packages must retain deterministic build metadata, repository commit metadata, portable symbols, and Source Link information.
- The `.nupkg` and `.snupkg` produced for a version are immutable release artifacts. If package contents change, increment the version rather than replacing a published package.
- A push to `main` validates and packages the project but does not publish it automatically.

## Repository CI

### Pull requests

`.github/workflows/pr-build-and-test.yaml` runs the solution in both Debug and Release on:

- `windows-latest`;
- `ubuntu-latest`;
- `macos-latest`.

Each matrix job cleans, restores, builds, and tests the whole solution, including the sample project.

### Pushes to main and the 0.8.0 release branch

`.github/workflows/push-main.yaml` runs for pushes to `main` and `0.8.0`, and may also be started with `workflow_dispatch`. It repeats the Debug/Release three-OS build/test matrix. After that matrix succeeds, an Ubuntu package-validation job:

1. restores and builds Release with `ContinuousIntegrationBuild=true`;
2. runs the Release test suite;
3. packs `Icod.TermInfo.csproj` into `artifacts`;
4. runs `.github/scripts/verify-release-package.sh artifacts`;
5. uploads the `.nupkg` and `.snupkg` as workflow artifacts.

The verifier checks package structure, dependency closure, portable symbols, Source Link/repository metadata, a fresh local-package consumer, and the sample's non-interactive `--describe-only` path.

## Local release validation

Before any final release tag, run:

```text
dotnet restore Icod.TermInfo.sln

dotnet build Icod.TermInfo.sln -c Debug
dotnet test Icod.TermInfo.sln -c Debug

dotnet build Icod.TermInfo.sln -c Release
dotnet test Icod.TermInfo.sln -c Release

dotnet pack Icod.TermInfo.csproj -c Release --output artifacts
bash .github/scripts/verify-release-package.sh artifacts
```

On Windows, run the PowerShell/cmd equivalents for the build/test/pack commands. The package verifier itself is a Bash script and is also exercised by the Ubuntu GitHub Actions package job.

## 0.8.0 completion gate

T30 set the release-candidate version to `0.8.0-rc.1`, froze the intended 0.8 public API, updated documentation/samples, and expanded fresh-package validation. T31 sets both version fields to the final `0.8.0` value, adds the semantic-completion release assertions, and records the final repository-side evidence map in `docs/0.8.0-CONTRACT-AUDIT.md`.

The final candidate must pass the exact release workflow before it is tagged. Push the candidate to `0.8.0` (or run the validation workflow manually) and require:

- three-OS Debug/Release CI;
- package validation and portable-symbol/Source Link checks;
- the frozen public API baseline;
- fresh-package consumer smoke tests covering the new 0.8 metadata, enumeration, expansion, byte-output, terminal-aware padding, and Windows profile APIs;
- compatibility checks for all retained 0.7 built-ins and enum values;
- golden `winconsole`, `ms-terminal`, and `ms-terminal-direct` tests;
- parameter/padding hardening and concurrency tests;
- T29 fixture/provenance and provider clean-miss tests;
- architecture guards proving that no production compiled terminfo parser, directory/system provider, `TERMINFO`, or `TERMINFO_DIRS` discovery entered 0.8.

The final repository-side evidence map is `docs/0.8.0-CONTRACT-AUDIT.md`. Do not create `v0.8.0` until the workflow for the exact final release commit is green. No source or package content may change between that successful validation and tagging; any change requires rerunning the gate.

## Publishing

The current repository intentionally separates validation from publication. The committed workflows validate and upload package artifacts but do not push them to package registries automatically.

For the T31 release candidate:

1. confirm both `<Version />` and `<PackageVersion />` are exactly `0.8.0`;
2. push the release-ready commit to `0.8.0` (and merge it through the normal repository process when appropriate);
3. require the six Windows/Linux/macOS Debug/Release jobs and the package-validation job for the exact release commit to finish successfully;
4. download the `.nupkg` and `.snupkg` artifacts produced by that validated commit, or reproduce them from that same commit with the documented deterministic build;
5. create and push tag `v0.8.0` at that exact release-ready commit;
6. publish the same `.nupkg` to NuGet.org and GitHub Packages using the repository owner's normal authenticated package-publishing procedure;
7. publish the `.snupkg` to the NuGet.org symbol server when pushing the NuGet package;
8. optionally attach both artifacts to a GitHub Release for the tag.

Do not publish a package built from a different commit than the one that passed the final T31 validation.

## NuGet.org authentication

If NuGet.org trusted publishing is used, configure the NuGet.org policy and GitHub OIDC workflow before enabling any automated publication job. Do not commit a long-lived NuGet API key to the repository.

If publication remains manual, use a short-lived/appropriately protected credential according to NuGet.org policy and never store it in source control.

## GitHub Packages authentication

For GitHub Actions publication, prefer the repository `GITHUB_TOKEN` with the minimum required `packages: write` permission. For manual publication, use an appropriately scoped credential according to GitHub Packages policy.

## After publication

After `v0.8.0` is published:

- confirm the package and symbols are visible on NuGet.org;
- confirm the same package version is visible in GitHub Packages;
- confirm a fresh consumer can restore the public package;
- mark the 0.8.0 roadmap complete;
- treat subsequent public API changes as deliberate contract changes for the next version.
