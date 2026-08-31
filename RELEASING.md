# Releasing Icod.TermInfo

This document describes the current validation and publication procedure for the
`Icod.TermInfo` package family built from this repository.

## Release principles

- `Directory.Build.props:IcodTermInfoSuiteVersion` is the sole coordinated
  release-version literal.
- Runtime, Source, Termcap, Compiler, and Inspection must consume that property
  for `<Version />` and `<PackageVersion />`; `tic`, `infocmp`, `toe`,
  `captoinfo`, and `infotocap` consume it for `<Version />`; the
  `Icod.TermInfo.Tools` router consumes it for both `<Version />` and
  `<PackageVersion />`.
- Runtime, Source, Termcap, Compiler, Inspection, and the router package versions
  must match the centralized suite version.
- Runtime, Source, Termcap, Compiler, and Inspection retain 1.x assembly version
  `1.0.0.0` and remain unsigned.
- Supported consumer targets for the 1.6 release are `net8.0`, `net9.0`, and `net10.0`.
- Beginning with T01 in 1.4, `tic`, `infocmp`, and `toe` target `net10.0`; TC07
  adds `captoinfo` and `infotocap` on `net10.0`. The five reusable library
  packages retain all three target frameworks.
- All five command projects remain non-packable solution executables. Command
  distribution uses six framework-dependent .NET 10 suite archives and the
  `Icod.TermInfo.Tools` .NET tool package exposing the `icod-terminfo` router.
- A release tag must be exactly `v<PackageVersion>` and is the only repository
  event which may publish packages.
- Release validation must pass on Windows, Linux, and macOS on `main` before a
  release tag is created. The tag workflow repeats the Release gate on the exact
  tagged commit before publication.
- Release validation must pass the frozen Runtime 1.0, Source 1.1, Compiler 1.2,
  Inspection 1.4, and Termcap 1.6 API baselines while retaining the historical
  Inspection 1.3 baseline and the net8/net9/net10 API-equivalence gates.
- Reusable-library Release builds treat missing public XML documentation as an
  error. Command and router projects generate XML documentation while retaining
  their explicit `CS1591` exemption.
- All five reusable library packages must pass the coordinated release verifier
  before publication. Termcap additionally has a dedicated structural verifier
  and package-reference-only smoke consumer. Use
  `.github/scripts/verify-release-package.sh` on a Bash-capable host or
  `.github/scripts/verify-release-package.cmd` from Windows Command Prompt.
  `Icod.TermInfo.Tools` must pass its structural package verifier and separately
  pass the isolated install-and-route smoke on Windows, Linux, and macOS.
- Release packages must retain deterministic build metadata, repository commit
  metadata, portable symbols, Source Link, README, icon metadata, and all three
  framework XML-documentation assets.
- The six `.nupkg` artifacts and five reusable-library `.snupkg` artifacts
  produced for a version are immutable release artifacts. If package contents
  change, increment the version rather than replacing a published package.
- The 1.6.0 release audit records maintainer confirmation that NuGet.org trusted
  publishing authorizes all six coordinated package IDs for this repository,
  `release.yaml`, and the `Release` environment. This external permission cannot
  be established by repository tests and must be reconfirmed if the policy
  changes before tagging.
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
all five reusable package projects, command/router tests, repository sample
executables, solution-contained maintenance tools, the five standalone command
projects, and the `Icod.TermInfo.Router` project.

The Ubuntu matrix leg continues after the shared Staging build/test steps and:

1. packs `Icod.TermInfo.csproj`,
   `Icod.TermInfo.Source/Icod.TermInfo.Source.csproj`,
   `Icod.TermInfo.Termcap/Icod.TermInfo.Termcap.csproj`,
   `Icod.TermInfo.Compiler/Icod.TermInfo.Compiler.csproj`,
   `Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj`, and the
   `Icod.TermInfo.Tools` router package into a runner-local `artifacts`
   directory;
2. runs `.github/scripts/verify-release-package.sh artifacts Staging`;
3. uploads the validated `.nupkg` and `.snupkg` files as the
   `icod-terminfo-pr-packages` Actions artifact for seven days.

There is no second checkout/restore/build/test package-validation job; all
packages are produced from the same Staging outputs which just passed the Ubuntu
matrix tests.

Beginning with T10, the Ubuntu PR leg also runs
`.github/scripts/build-tool-archives.sh Staging artifacts/tools`, requires all six
framework-dependent suite archives, and uploads them as the
`icod-terminfo-pr-tools` validation artifact. Beginning with T11, the same leg
runs `.github/scripts/verify-tool-archives.sh artifacts/tools` before upload so
the six archive names, manifests, command launchers, managed dependencies,
documentation payload, path safety, and absence of development-only project/PDB
files are checked. This is still validation, not publication.

The library verifier covers generated capability metadata, the frozen Runtime,
Source, Termcap, Compiler, and Inspection API baselines, net8/net9/net10 API
equivalence, package structure, metadata, XML, symbols, all five
fresh-library-package consumers, and the non-interactive repository sample. A
separate `smoke-tool-package.ps1` job installs and exercises
`Icod.TermInfo.Tools` on all three host families.

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

Each matrix leg packs all five reusable package projects plus
`Icod.TermInfo.Tools` and runs the platform-appropriate library Release verifier.
The Windows leg uploads the canonical six `.nupkg` and five `.snupkg` artifacts
for seven days. A separate three-host job installs and exercises the router
package from that canonical artifact set.

The main-branch workflow stops after validation and artifact upload. It has only
`contents: read` permission and never authenticates to or pushes to a package
registry.

Beginning with T10, the Ubuntu main-validation leg also builds and uploads the
six canonical framework-dependent tool-suite archives as
`icod-terminfo-main-tools`. Beginning with T11, those archives must pass the
structural tool-archive verifier before upload. The workflow remains
validation-only.

### Release tags

`.github/workflows/release.yaml` runs for pushed tags matching `v*`. Before the
release build, it fetches `origin/main` and requires the tagged commit to equal
the exact current `main` HEAD. This prevents an older already-merged ancestor
from being released after a newer validation commit exists. It also requires the
tag to match `Directory.Build.props:IcodTermInfoSuiteVersion` and verifies
that all coordinated library, command, and router projects consume the suite
version through their appropriate `Version` / `PackageVersion` fields.

The tag workflow reruns the complete Release matrix on Windows, Linux, and macOS.
After all three legs pass and both tool-distribution smoke gates succeed, the
canonical validated packages are published to NuGet.org and GitHub Packages.
Finally, the workflow creates a GitHub Release containing all six `.nupkg` files,
all five reusable-library symbol packages, the six framework-dependent .NET 10
tool-suite archives, and a SHA-256 checksum manifest. The 17 package/archive files
become 18 release assets after `SHA256SUMS.txt` is added. Prerelease package
versions create GitHub prereleases.

The archives retain the traditional command names and pass structural plus
matching-host execution gates. `Icod.TermInfo.Tools` is the separate
registry-published .NET tool package and must pass its own matching-host
install-and-route smoke before publication.

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
6. require exact Termcap public API equivalence across `net8.0`, `net9.0`, and
   `net10.0`, require the frozen `PublicApiSnapshot/v1` reflection-manifest
   fingerprint, and require the packed XML surfaces to match
   `docs/1.6.0-TERMCAP-PUBLIC-API-BASELINE.txt`;
7. require exact Compiler public API equivalence across `net8.0`, `net9.0`, and
   `net10.0` and require `docs/1.2.0-COMPILER-PUBLIC-API-BASELINE.txt` to match;
8. require exact Inspection public API equivalence across `net8.0`, `net9.0`, and
   `net10.0` and require `docs/1.4.0-INSPECTION-PUBLIC-API-BASELINE.txt` to match;
9. run the Runtime, Termcap, Compiler, and Inspection package verifiers for
   package structure, dependency closure, metadata, XML documentation, Source
   Link, and portable symbols;
10. structurally verify `Icod.TermInfo.Tools`, including its single router command
    and the absence of host-specific command apphosts from its `any` payload;
11. require Source, Termcap, Compiler, and Inspection `.nupkg` / `.snupkg` artifacts
    at the same package version as Runtime;
12. restore and execute the isolated Runtime package consumer on all three TFMs;
13. restore and execute the isolated Source package consumer on all three TFMs;
14. restore and execute the isolated Termcap package consumer on all three TFMs;
15. restore and execute the isolated Compiler package consumer on all three TFMs;
16. restore and execute the isolated Inspection package consumer on all three TFMs;
17. run the general repository sample through its non-interactive
    `--describe-only` path;
18. run the deterministic Source -> Compiler -> database acquisition -> Inspection
    toolchain sample.

All three executable API samples are solution projects and therefore compile in
every CI matrix. The deterministic toolchain sample is also executed by the
release verifier. The focused acquisition sample is not automatically run against
the host database because its `system` command intentionally inspects host-specific
terminfo state; the isolated runtime package-smoke consumer supplies the
deterministic acquisition acceptance test instead.

The checked-in Runtime, Source, Termcap, Compiler, and Inspection package-smoke
projects are intentionally not part of the solution and contain no project
references to the packages they consume.

The runtime smoke consumer creates a conventional compiled entry at runtime and
proves the packed package can:

- parse caller-supplied compiled bytes;
- load an explicit conventional directory tree;
- construct a fully restricted system provider;
- load through the public system provider from a snapshotted `TERMINFO` root;
- compose system lookup with `TerminalDatabase.BuiltIn` fallback.

The Source smoke consumer proves the separately packed source-language package
can restore through its NuGet dependency on the matching Runtime package and
execute on all three supported target frameworks. The Termcap smoke consumer
proves the fifth reusable package restores through its Runtime-only dependency
and executes parsing, `tc=` resolution, Runtime conversion, reverse rendering,
and explicit inline acquisition on all three frameworks. The Compiler smoke
consumer likewise proves the Compiler package restores through its Runtime and
Source dependencies and can write and reparse a C01 legacy entry on all three
frameworks. The Inspection smoke consumer restores with matching Runtime and
Source dependencies and exercises the reviewed 1.4 Inspection public surface,
including T02 system database-location inspection and T03 conventional database
catalog enumeration, without a production Compiler dependency.

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
dotnet pack Icod.TermInfo.Termcap/Icod.TermInfo.Termcap.csproj -c Release --output artifacts
dotnet pack Icod.TermInfo.Compiler/Icod.TermInfo.Compiler.csproj -c Release --output artifacts
dotnet pack Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj -c Release --output artifacts
dotnet pack icod-terminfo/Icod.TermInfo.Router.csproj -c Release --output artifacts
```

The solution restore/build/test commands above also build and test `tic`,
`infocmp`, `toe`, `captoinfo`, `infotocap`, the router, and their tests. All five
standalone command projects remain non-packable.

Run the packed router installation smoke with:

```text
pwsh -File .github/scripts/smoke-tool-package.ps1 artifacts
```

Build the six framework-dependent standalone suite archives on a Bash-capable
host with the .NET 10 SDK plus `zip`, GNU `tar`, and `gzip`:

```text
bash .github/scripts/build-tool-archives.sh Release artifacts/tools
```

The builder verifies the centralized coordinated version, publishes all five
commands for the six supported RIDs with `--self-contained false`, normalizes
archive ordering/timestamps, and requires exactly six output archives.

Beginning with T11, immediately validate the archive structure:

```text
bash .github/scripts/verify-tool-archives.sh artifacts/tools
```

The verifier checks all six canonical names, archive path safety, suite manifests,
command launchers/runtime metadata, reusable managed dependencies, documentation,
and the absence of `.pdb`, `.csproj`, and `.sln` payload files.

Then run the coordinated package verifier with the same configuration used to
build and pack.

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
the centralized `IcodTermInfoSuiteVersion`.

The tag workflow rebuilds, retests, repacks, and reverifies the tagged commit.
NuGet.org publication, GitHub Packages publication, and GitHub Release creation
all consume the canonical validated Actions artifact rather than repacking the
repository.

For 1.6.0, maintainer confirmation of the required NuGet.org trusted-publishing
scope is recorded in `docs/1.6.0-RELEASE-AUDIT.md`: all six coordinated package
IDs are authorized for this repository, `release.yaml`, and the `Release`
environment. Reconfirm the policy if it changes before the stable tag.

Before merging or pushing a release-ready commit to `main`:

1. confirm `Directory.Build.props:IcodTermInfoSuiteVersion` is the intended
   release version and all coordinated projects consume it;
2. confirm all five reusable assemblies still declare `AssemblyVersion`
   `1.0.0.0`;
3. ensure the NuGet.org trusted-publishing policy authorizes this repository,
   `release.yaml`, the `Release` environment, and all six package IDs:
   `Icod.TermInfo`, `Icod.TermInfo.Source`, `Icod.TermInfo.Termcap`,
   `Icod.TermInfo.Compiler`, `Icod.TermInfo.Inspection`, and `Icod.TermInfo.Tools`;
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
API key. The trusted-publishing package scope must authorize all package IDs
participating in the coordinated release.

Do not commit NuGet credentials to the repository.

## GitHub Packages authentication

GitHub Actions publication uses the repository `GITHUB_TOKEN` with
`packages: write`. Manual publication, whenever required, should use an
appropriately scoped credential and must never place that credential in source
control.

## After publication

After a final version is published:

- confirm all six package IDs and all five reusable-library symbol packages are
  visible on NuGet.org;
- confirm the same package version for all six IDs is visible in GitHub Packages;
- confirm fresh Runtime, Source, Termcap, Compiler, and Inspection consumers can
  restore the final version;
- verify the published version came from the expected immutable release tag;
- confirm `Icod.TermInfo.Tools` installs and routes all five commands;
- confirm the GitHub Release contains all eleven package artifacts, all six
  framework-dependent tool-suite archives, and `SHA256SUMS.txt`;
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

For the completed 1.3 line, use
`Icod.TermInfo-1.3.0-Inspection-and-Comparison-Roadmap.md` for the I01-I07
contract, `docs/1.3.0-PRE-I01-CONTRACT-AUDIT.md` for the package/layer freeze,
`docs/1.3.0-INSPECTION-PUBLIC-API-BASELINE.txt` for the frozen Inspection API,
and `docs/1.3.0-RELEASE-AUDIT.md` for final release sign-off requirements.

For the completed 1.4 line, use `Icod.TermInfo-1.4.0-Tool-Suite-Roadmap.md` for
the T01-T11 contract, `docs/1.4.0-PRE-T01-CONTRACT-AUDIT.md` for the command-layer
foundation, `docs/1.4.0-T02-SYSTEM-DATABASE-LOCATION-INSPECTION.md` for the T02
discovery seam, `docs/1.4.0-T03-CONVENTIONAL-DATABASE-CATALOG.md` for T03 catalog
enumeration, `docs/1.4.0-T04-TIC-VALIDATION-AND-CHECK-ONLY.md` for the first
operational validation contract,
`docs/1.4.0-T05-TIC-COMPILATION-AND-DATABASE-PUBLICATION.md` for the first
filesystem-mutating command contract,
`docs/1.4.0-T06-INFOCMP-ONE-TERMINAL-INSPECTION-AND-RENDERER-CONTROLS.md` for the
first operational `infocmp` contract,
`docs/1.4.0-T07-INFOCMP-SEMANTIC-COMPARISON.md` for managed semantic comparison,
`docs/1.4.0-T08-TOE-CONVENTIONAL-DATABASE-LISTING.md` for conventional database
listing, `docs/1.4.0-T09-TOE-SOURCE-DEPENDENCY-AND-DUPLICATE-SEMANTICS.md` for
source dependency analysis,
`docs/1.4.0-T10-CLI-COMPATIBILITY-PRESENTATION-AND-DISTRIBUTION-HARDENING.md`
for the hardened suite/distribution contract, and
`docs/1.4.0-T11-DIFFERENTIAL-VALIDATION-HOSTILE-INPUT-AND-FREEZE.md` for the
Alpha-11 release-readiness gate. The original stable 1.4.0 release gate is
recorded in `docs/1.4.0-RELEASE-AUDIT.md`. Patch release 1.4.1 corrects
release-facing documentation and metadata without changing the frozen
`docs/1.4.0-INSPECTION-PUBLIC-API-BASELINE.txt` or 1.4 command semantics; its
release gate is `docs/1.4.1-RELEASE-AUDIT.md`.

For 1.5, centralized versioning and the installable router are frozen by
`docs/1.5.0-RELEASE-AUDIT.md`. The 1.4 command semantic contract and the frozen
`docs/1.4.0-INSPECTION-PUBLIC-API-BASELINE.txt` remain unchanged.

For 1.6, use `Icod.TermInfo-1.6.0-Termcap-Interoperability-Roadmap.md` for the
TC01-TC08 contract, `docs/1.6.0-TC01-TERMCAP-PACKAGE-AND-PARSER-FOUNDATION.md`
through `docs/1.6.0-TC08-DIFFERENTIAL-VALIDATION-FUZZING-AND-FREEZE.md` for
tranche evidence, `docs/1.6.0-TERMCAP-PUBLIC-API-BASELINE.txt` for the frozen
Termcap surface, and `docs/1.6.0-RELEASE-AUDIT.md` for stable release closure.

The final `v<IcodTermInfoSuiteVersion>` tag must identify the exact validated and
published `main` commit. Do not edit the audit or any other source/package
content after that validation without rerunning the release gate.
