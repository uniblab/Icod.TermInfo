# Releasing Icod.TermInfo

This document describes the current validation and publication procedure for the
`Icod.TermInfo` package family built from this repository.

## Release principles

- `<Version />` and `<PackageVersion />` must be present and identical in
  `Icod.TermInfo.csproj`, `Icod.TermInfo.Source/Icod.TermInfo.Source.csproj`,
  `Icod.TermInfo.Compiler/Icod.TermInfo.Compiler.csproj`, and
  `Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj`.
- Beginning with I01, Runtime, Source, Compiler, and Inspection package versions
  must match.
- Runtime, Source, Compiler, and Inspection retain 1.x assembly version
  `1.0.0.0` and remain unsigned.
- Supported consumer targets for the 1.3 line are `net8.0`, `net9.0`, and `net10.0`.
- A release tag must be exactly `v<PackageVersion>` and is the only repository
  event which may publish packages.
- Release validation must pass on Windows, Linux, and macOS on `main` before a
  release tag is created. The tag workflow repeats the Release gate on the exact
  tagged commit before publication.
- Release validation must pass the frozen Runtime, Source, and Compiler API
  baselines, the developing Inspection API baseline, and net8/net9/net10
  API-equivalence gates.
- Release builds treat missing public XML documentation as an error.
- All four packages must pass the coordinated release verifier before publication.
  Use `.github/scripts/verify-release-package.sh` on a Bash-capable host or
  `.github/scripts/verify-release-package.cmd` from Windows Command Prompt.
- Release packages must retain deterministic build metadata, repository commit
  metadata, portable symbols, Source Link, README, icon metadata, and all three
  framework XML-documentation assets.
- The eight `.nupkg` / `.snupkg` artifacts produced for a version are immutable
  release artifacts. If package contents change, increment the version rather
  than replacing a published package.
- Publication is downstream of tag/version validation and the complete
  build/test/package gate. Pull requests and ordinary pushes to `main` must not
  authenticate to or push to package registries.

## Repository CI

### Pull requests

`.github/workflows/pr-build-and-test.yaml` runs the solution in the repository
`Staging` configuration on:

- `windows-latest`;
- `ubuntu-latest`;
- `macos-latest`.

Each matrix job cleans, restores, builds, and tests the whole solution, including
all four package projects, Source, Compiler, and Inspection tests, repository sample
executables, and solution-contained maintenance tools.

The Ubuntu matrix leg continues after the shared Staging build/test steps and:

1. packs `Icod.TermInfo.csproj`,
   `Icod.TermInfo.Source/Icod.TermInfo.Source.csproj`,
   `Icod.TermInfo.Compiler/Icod.TermInfo.Compiler.csproj`, and
   `Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj` into a runner-local
   `artifacts` directory;
2. runs `.github/scripts/verify-release-package.sh artifacts Staging`;
3. uploads the validated `.nupkg` and `.snupkg` files as the
   `icod-terminfo-pr-packages` Actions artifact for seven days.

There is no second checkout/restore/build/test package-validation job; all four
packages are produced from the same Staging outputs which just passed the Ubuntu
matrix tests.

That verifier covers generated capability metadata, the frozen runtime API
baseline, the reviewed Source and Compiler API baselines, the Inspection API
baseline, net8/net9/net10 API equivalence for all four assemblies, Runtime,
Compiler, and Inspection package structure, metadata, XML, symbols, all four
fresh-package consumers, and the
non-interactive repository sample.

The PR artifact is uploaded only after verification succeeds. It is intended for
inspection, installation, and testing and is not a registry publication.

Packing and uploading a GitHub Actions artifact on a pull request is validation,
not publication. The pull-request workflow has only `contents: read` permission
and must not request OIDC or package-write permission, authenticate to a package
registry, push a package, or contain a deployment job.

### Pushes to main

`.github/workflows/push-main.yaml` runs only for pushes to `main`. It executes
the Release build/test matrix on:

- `windows-latest`;
- `ubuntu-latest`;
- `macos-latest`.

Each matrix leg packs all four package projects and runs the platform-appropriate
Release verifier. The Windows leg uploads the canonical eight `.nupkg` / `.snupkg`
artifacts for seven days.

The main-branch workflow stops after validation and artifact upload. It has only
`contents: read` permission and never authenticates to or pushes to a package
registry.

### Release tags

`.github/workflows/release.yaml` runs for pushed tags matching `v*`. Before the
release build, it requires the tagged commit to be contained in `main`, requires
Runtime, Source, Compiler, and Inspection `Version` / `PackageVersion` values to match, and
requires the tag version to match that coordinated package version exactly.

The tag workflow reruns the complete Release matrix on Windows, Linux, and macOS.
After all three legs pass, the canonical validated packages are published to
NuGet.org and GitHub Packages. Finally, the workflow creates a GitHub Release
containing all four package files, all four symbol packages, and a SHA-256
checksum manifest. Prerelease package versions create GitHub prereleases.

## What the release verifier checks

The Bash and CMD entry points perform equivalent validation. They:

1. run the deterministic standard-capability metadata generator in `--check`
   mode;
2. require the frozen runtime 1.0 public API baseline to match;
3. require exact runtime public API equivalence across the built `net8.0`,
   `net9.0`, and `net10.0` assemblies;
4. require exact Source public API equivalence across `net8.0`, `net9.0`, and
   `net10.0`;
5. require the reviewed `docs/1.1.0-SOURCE-PUBLIC-API-BASELINE.txt` to match the
   built Source assembly;
6. require exact Compiler public API equivalence across `net8.0`, `net9.0`, and
   `net10.0` and require `docs/1.2.0-COMPILER-PUBLIC-API-BASELINE.txt` to match;
7. require exact Inspection public API equivalence across `net8.0`, `net9.0`, and
   `net10.0` and require `docs/1.3.0-INSPECTION-PUBLIC-API-BASELINE.txt` to match;
8. run the Runtime, Compiler, and Inspection package verifiers for package
   structure, dependency closure, metadata, XML documentation, Source Link, and
   portable symbols;
9. require Source, Compiler, and Inspection `.nupkg` / `.snupkg` artifacts at the
   same package version as Runtime;
10. restore and execute the isolated Runtime package consumer on all three TFMs;
11. restore and execute the isolated Source package consumer on all three TFMs;
12. restore and execute the isolated Compiler package consumer on all three TFMs;
13. restore and execute the isolated Inspection package consumer on all three TFMs;
14. run the general repository sample through its non-interactive
    `--describe-only` path.

Both repository sample executables are solution projects and therefore compile
in every CI matrix. The focused acquisition sample is not automatically run
against the host database because its `system` command intentionally inspects
host-specific terminfo state; the isolated runtime package-smoke consumer
supplies the deterministic acquisition acceptance test instead.

The checked-in Runtime, Source, Compiler, and Inspection package-smoke projects are
intentionally not part of the solution and contain no project references to the
packages they consume.

The runtime smoke consumer creates a conventional compiled entry at runtime and
proves the packed package can:

- parse caller-supplied compiled bytes;
- load an explicit conventional directory tree;
- construct a fully restricted system provider;
- load through the public system provider from a snapshotted `TERMINFO` root;
- compose system lookup with `TerminalDatabase.BuiltIn` fallback.

The Source smoke consumer proves the separately packed source-language package
can restore through its NuGet dependency on the matching runtime package and
execute on all three supported target frameworks. The Compiler smoke consumer
likewise proves the Compiler package restores through its Runtime dependency and
can write and reparse a C01 legacy entry on all three frameworks. The Inspection
smoke consumer proves the fourth package restores with matching Runtime and Source
dependencies while retaining the I01 empty-public-surface contract.

No checked-in runtime fixture is copied into the smoke project, so those checks
prove the public package surface rather than repository-only outputs.

## Local release validation

Build, test, and pack Release first:

```text
dotnet restore Icod.TermInfo.sln
dotnet build Icod.TermInfo.sln -c Release
dotnet test Icod.TermInfo.sln -c Release
dotnet pack Icod.TermInfo.csproj -c Release --output artifacts
dotnet pack Icod.TermInfo.Source/Icod.TermInfo.Source.csproj -c Release --output artifacts
dotnet pack Icod.TermInfo.Compiler/Icod.TermInfo.Compiler.csproj -c Release --output artifacts
dotnet pack Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj -c Release --output artifacts
```

Then run the coordinated verifier with the same configuration used to build and
pack.

For Staging:

```text
.github\scripts\verify-release-package.cmd artifacts Staging
bash .github/scripts/verify-release-package.sh artifacts Staging
```

For final Release validation:

```text
.github\scripts\verify-release-package.cmd artifacts Release
bash .github/scripts/verify-release-package.sh artifacts Release
```

Both wrappers accept `Debug`, `Staging`, or `Release` and reject other
configuration names. They otherwise provide the same validation contract.

## Automated publication

`push-main.yaml` validates release candidates but never publishes them.
`release.yaml` is the sole automated publication workflow. A pushed `v*` tag
must identify a commit contained in `main`, and its version must exactly match
the coordinated Runtime, Source, Compiler, and Inspection `Version` / `PackageVersion`
values.

The tag workflow rebuilds, retests, repacks, and reverifies the tagged commit.
NuGet.org publication, GitHub Packages publication, and GitHub Release creation
all consume the canonical validated Actions artifact rather than repacking the
repository.

Before merging or pushing a release-ready commit to `main`:

1. confirm `<Version />` and `<PackageVersion />` are the intended version in
   all four package projects and that all eight values match;
2. confirm all four assemblies still declare `AssemblyVersion` `1.0.0.0`;
3. ensure the NuGet.org trusted-publishing policy authorizes this repository,
   `release.yaml`, the `Release` environment, and all four package IDs:
   `Icod.TermInfo`, `Icod.TermInfo.Source`, `Icod.TermInfo.Compiler`, and
   `Icod.TermInfo.Inspection`;
4. ensure the `NUGET_USER` repository secret identifies the intended NuGet.org
   account;
5. preserve `packages: write` permission for GitHub Packages;
6. remember that a source/package change after validation creates a new release
   candidate and requires validation again.

After the release-ready commit has passed the `main` Release workflow, create
and push tag `v<PackageVersion>` for that exact commit. Pushing the tag starts
publication; do not publish the packages manually first.

If the `Release` environment restricts deployment branches or tags, configure
it to permit the intended `v*` release tags before the first tag-driven release.

## NuGet.org authentication

NuGet.org publication uses trusted publishing. The workflow obtains a temporary
API key through GitHub OIDC and does not require a committed or long-lived NuGet
API key. The trusted-publishing package scope must authorize all package IDs participating
in the coordinated release.

Do not commit NuGet credentials to the repository.

## GitHub Packages authentication

GitHub Actions publication uses the repository `GITHUB_TOKEN` with
`packages: write`. Manual publication, whenever required, should use an
appropriately scoped credential and must never place that credential in source
control.

## After publication

After a final version is published:

- confirm all four package IDs and all four symbol packages are visible on
  NuGet.org;
- confirm the same package version for all four IDs is visible in GitHub
  Packages;
- confirm fresh Runtime, Source, Compiler, and Inspection consumers can restore
  the final version;
- verify the published version came from the expected immutable release tag;
- confirm the GitHub Release contains all eight package artifacts plus
  `SHA256SUMS.txt`;
- treat subsequent public API or package-content changes as changes for the next
  version.

Historical completion evidence for 0.6.0 through 1.0.0 remains under the
versioned roadmap and contract-audit documents. Consumer-facing compiled-database
usage remains consolidated in `docs/0.9.0-ACQUISITION-GUIDE.md`.

For 1.1, use:

- `Icod.TermInfo-Post-1.0-Development-Roadmap.md` for the S01-S09 contract;
- `docs/1.1.0-S01-SOURCE-PACKAGE-FOUNDATION.md` through
  `docs/1.1.0-S09-CORPUS-FUZZING-COMPATIBILITY.md` for tranche evidence;
- `docs/1.1.0-SOURCE-PUBLIC-API-BASELINE.txt` for the frozen Source API;
- `docs/1.1.0-RELEASE-AUDIT.md` for final release sign-off requirements;
- `docs/1.0.0-PUBLIC-API-BASELINE.txt` for the unchanged runtime API;
- `docs/VERSIONING.md` and `docs/COMPATIBILITY.md` for the stable 1.x promises.

For the completed 1.2 line, use `docs/1.2.0-PRE-C01-CONTRACT-AUDIT.md` for the
frozen compiler architecture, `docs/1.2.0-C01-COMPILER-PACKAGE-FOUNDATION.md`
for the C01 implementation record, and
`docs/1.2.0-COMPILER-PUBLIC-API-BASELINE.txt` for the frozen Compiler API.

For 1.3 development, use
`Icod.TermInfo-1.3.0-Inspection-and-Comparison-Roadmap.md` for the I01-I07
contract, `docs/1.3.0-PRE-I01-CONTRACT-AUDIT.md` for the package/layer freeze,
and `docs/1.3.0-INSPECTION-PUBLIC-API-BASELINE.txt` for the developing Inspection
API.

The final `v<PackageVersion>` tag must identify the exact validated and published `main`
commit. Do not edit the audit or any other source/package content after that
validation without rerunning the release gate.