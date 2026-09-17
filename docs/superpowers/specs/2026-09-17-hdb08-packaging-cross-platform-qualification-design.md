# HDB08 Packaging and Cross-platform Qualification Design

**Status:** COMPLETE / ACCEPTED
**Target prerelease:** `1.15.0-Alpha-8`
**Accepted implementation/qualification head:**
`b25733851c963585b56fcf064e9e27c6fdd47ee5`
**Accepted qualification runs:** normal `35171310997` (12/12), HDB00
`35171310971` (3/3)
**Accepted HDB07C implementation/qualification head:**
`2c122abd7e4e63397b474f248d51273a1b7fc006`
**Accepted HDB07C final closure head:**
`048aaeba39e5659cc4b549dc06d4fdc7c85a08fe`

## 1. Purpose

HDB08 is the packaging and cross-platform qualification tranche for the
accepted Icod.TermInfo 1.15 Berkeley DB acquisition contract. It closes the
distribution gap between Linux-only isolated BerkeleyDb package consumption
and the required Windows, Linux, and macOS package evidence.

HDB08 freezes production behavior. It does not add another acquisition
feature, public API, command option, JSON contract, native runtime, or write
path. A product correction is permitted only when a new HDB08 distribution
gate exposes a concrete defect, and that correction must be independently
reviewed through a narrow RED-to-GREEN checkpoint.

## 2. Current evidence and identified gap

The accepted HDB07C head already proves:

- Windows, Linux, and macOS build and unit-test coverage;
- 422 BerkeleyDb unit tests per target framework and host;
- 50 native-store interoperability tests per target framework and host;
- Linux and macOS native-store production with Windows transported-fixture
  consumption;
- canonical Linux package creation and structural verification;
- one isolated BerkeleyDb package consumer on net8, net9, and net10;
- installed-tool package smoke on Windows, Linux, and macOS; and
- matching-host smoke for all six archive RIDs.

The isolated BerkeleyDb package consumer currently runs only on the Linux
packaging host in the pull-request workflow. It does not run in the
cross-platform package-smoke jobs, and the `main` and tagged-release workflows
do not invoke it. The existing PowerShell package verifier also proves the
essential HDB01 structure but is less exact than the repository's mature C#
package verifiers for Runtime, Compiler, Inspection, and Termcap.

HDB08 closes these two qualification gaps without duplicating native HDB00
work or packing independently on every operating system.

## 3. Scope decision

The approved approach is **one canonical package set, exact managed package
verification, and three-host consumption**.

Linux remains the canonical package producer in ordinary PR and `main`
workflows. Tagged release likewise produces one package set. The exact same
artifacts are downloaded and consumed on Windows, Linux, and macOS. This
separates package production from consumer portability and avoids treating
three independently packed archives as equivalent artifacts.

The six existing standalone tool archives remain RID-specific and continue to
run on matching Windows x64/ARM64, Linux x64/ARM64, and macOS x64/ARM64 hosts.

Rejected alternatives are:

1. **workflow-only wiring with the current verifier** — cheaper, but it leaves
   BerkeleyDb package validation weaker than the rest of the coordinated
   package family; and
2. **independent package production on every host** — more expensive and less
   precise because it creates multiple candidate package sets rather than one
   byte-authoritative set consumed everywhere.

## 4. Version and contract boundaries

HDB08 advances the coordinated suite version from `1.15.0-Alpha-7` to
`1.15.0-Alpha-8`. `Directory.Build.props` remains the single version
authority, and every package continues to consume
`$(IcodTermInfoSuiteVersion)` for both `<Version>` and `<PackageVersion>`.

The BerkeleyDb package release notes become:

> 1.15.0-Alpha-8 completes exact package and cross-platform qualification for
> managed read-only Berkeley DB Hash-v9 acquisition. Public API, dependencies,
> acquisition behavior, pure-managed deployment, and the read-only boundary
> remain unchanged.

Reusable assembly versions remain exactly `1.0.0.0`. Target frameworks remain
`net8.0`, `net9.0`, and `net10.0`.

HDB08 adds no:

- public type, member, option, or exception;
- command switch or diagnostic identifier;
- JSON field, document kind, or schema revision;
- package dependency;
- production native asset, P/Invoke, or dynamic native load;
- Berkeley DB writer, environment, transaction, recovery, or repair path; or
- change to Runtime, Inspection, Source, Compiler, or Termcap dependency
  direction.

The accepted HDB07C acquisition semantics, limitations, and exception
boundaries remain authoritative.

## 5. Exact BerkeleyDb package verifier

### 5.1 Ownership

HDB08 adds a focused C# verification tool under
`tools/berkeleydb-package-verifier/`. It follows the existing package-verifier
pattern, targets `net10.0`, has no package dependency, and is built through
`Icod.TermInfo.sln`. It is repository maintenance tooling, not a packaged
product.

Its command contract is:

```text
dotnet run --project tools/berkeleydb-package-verifier/Icod.TermInfo.BerkeleyDb.PackageVerifier.csproj -- [artifact-directory]
```

The artifact directory defaults to `artifacts`. More than one argument returns
status 2 with usage text. A validation failure returns status 1 with one exact
diagnostic on stderr. Success returns status 0 and one summary on stdout.

`.github/scripts/verify-berkeleydb-package.ps1` remains the stable orchestration
entry point used by `packaging/VerifyPackageArtifact.ps1`. The PowerShell
wrapper invokes the C# verifier and the existing public-API equivalence tool;
it does not reimplement archive parsing.

### 5.2 Primary package requirements

For `Icod.TermInfo.BerkeleyDb.<version>.nupkg`, the verifier requires:

- the package filename and nuspec id/version match the coordinated suite
  version exactly;
- package metadata is exactly title/id `Icod.TermInfo.BerkeleyDb`, author
  `Timothy J. Bruce`, license `LGPL-3.0-or-later`, project/repository URL
  `https://github.com/uniblab/Icod.TermInfo`, repository type `git`, README
  `README.md`, icon `icon.png`, license acceptance `true`, description
  `Managed read-only acquisition support for ncurses-compatible Berkeley DB
  Hash-v9 terminfo stores.`, and nuspec tags
  `terminfo libtinfo terminal berkeleydb hash ncurses database dotnet csharp`;
- the project retains the semicolon-separated MSBuild tag authority
  `terminfo;libtinfo;terminal;berkeleydb;hash;ncurses;database;dotnet;csharp`;
- root package entries include `README.md`, `icon.png`, and `LICENSE`;
- exactly the `net8.0`, `net9.0`, and `net10.0` library groups are present;
- each library group contains the managed BerkeleyDb assembly and XML
  documentation with no additional product binary;
- each dependency group contains exactly one dependency on `Icod.TermInfo` at
  the coordinated package version;
- each managed assembly has simple name `Icod.TermInfo.BerkeleyDb`, assembly
  version `1.0.0.0`, and no unmanaged module;
- no `runtimes/` payload, native library extension, executable native payload,
  or native package dependency is present; and
- no unexpected target framework or package dependency is present.

The verifier compares exact values. Substring matching is insufficient for
dependency versions or metadata identity.

### 5.3 Symbol package requirements

For `Icod.TermInfo.BerkeleyDb.<version>.snupkg`, the verifier requires:

- the matching id and coordinated version;
- one portable PDB for each supported target framework;
- embedded Source Link metadata pointing at the repository and exact commit;
- no DLL, runtime-specific, or native payload; and
- no unexpected PDB target framework.

The verifier may reuse repository patterns and framework libraries already
used by the other package verifiers. It adds no NuGet dependency solely for
verification.

### 5.4 API equivalence

The existing `Icod.TermInfo.PublicApiSnapshot` tool remains authoritative for
cross-TFM public-API equivalence. The PowerShell wrapper compares net8 to net9
and net8 to net10 after the C# verifier accepts the package structure and
identity.

HDB09, not HDB08, owns the final checked-in 1.15 public API manifest and stable
release freeze.

## 6. Three-host package consumption

### 6.1 Artifact flow

The ordinary workflow data flow is:

```text
Linux canonical pack + exact verification
                  |
                  v
       one uploaded package set
                  |
          +-------+-------+
          |       |       |
          v       v       v
       Windows  Linux   macOS
       net8/9/10 package-only consumer
```

The same topology applies to the `main` and tagged-release workflows. No
consumer job installs Berkeley DB or ncurses.

### 6.2 Consumer contract

The existing `.github/scripts/smoke-hdb03-package-consumer.ps1` remains the
single package-only entry point. It copies only the checked-in C# consumer
project and program into a temporary directory, configures the package artifact
directory as the only `Icod.TermInfo*` source, and restores the candidate
packages without project references.

On each of net8, net9, and net10, the consumer exercises:

- explicit provider canonical and alias lookup;
- clean miss and successful-result caching;
- catalog canonical/alias publication and shared terminal identity;
- opt-in system-provider acquisition;
- exact UTF-8 behavior; and
- the HDB07C Latin-1 canonical/alias provider, catalog, and system-provider
  paths.

The consumer uses deterministic C#-generated Hash-v9 fixtures. Native fixture
production remains in HDB00 and is not duplicated in ordinary package smoke.

### 6.3 Workflow integration

The package-smoke jobs in these workflows install .NET SDKs 8, 9, and 10 and
invoke the isolated BerkeleyDb consumer on all three operating systems:

- `.github/workflows/pull-request.yaml`;
- `.github/workflows/main.yaml`; and
- `.github/workflows/release.yaml`.

The pull-request workflow removes the redundant Linux-only invocation from the
canonical packaging job after the cross-host package-smoke jobs own the check.
Each artifact is still structurally verified before upload.

The tagged-release publication jobs continue to depend on successful package
and archive smoke. No release package is published unless all Windows, Linux,
and macOS package-consumer jobs and all six archive jobs succeed.

## 7. Archive qualification

The archive topology remains unchanged:

- `win-x64` on Windows x64;
- `win-arm64` on Windows ARM64;
- `linux-x64` on Linux x64;
- `linux-arm64` on Linux ARM64;
- `osx-x64` on macOS x64; and
- `osx-arm64` on macOS ARM64.

Each archive smoke retains direct executable and routed `icod-terminfo`
coverage. Existing controlled Hash-v9 fixtures continue to prove explicit
`infocmp` provider lookup and `toe` logical catalog enumeration from the
archive without Berkeley DB installed.

HDB08 adds permanent workflow-contract assertions that the three package-smoke
hosts and six archive-RID hosts remain wired in PR, `main`, and release
workflows. It does not add a seventh archive or a native Berkeley DB payload.

## 8. Test-first checkpoints

HDB08 uses independently reviewable RED-to-GREEN checkpoints.

### Checkpoint A — exact package verification

Contract tests first require the C# verifier project, its stable PowerShell
entry point, exact dependency checks, assembly identity, symbol/Source Link
validation, and no-native-asset rejection. The RED must fail because the new
verifier does not yet exist or because the old substring dependency check is
insufficient.

GREEN adds the C# verifier and routes the existing orchestration entry point to
it. The complete normal workflow must pass before proceeding.

### Checkpoint B — three-host packaged consumption

Contract tests first require all three workflow families to install SDKs 8, 9,
and 10 in package-smoke jobs and invoke the isolated BerkeleyDb consumer. The
RED must identify the missing cross-host/main/release wiring.

GREEN updates the workflows and removes only the now-redundant Linux-only PR
invocation. The pull-request workflow must pass all 12 jobs, including three
package-smoke hosts and six matching archive RIDs.

### Checkpoint C — Alpha-8 authority and closure

Tests first require the exact `1.15.0-Alpha-8` version authority, package
release notes, HDB08 roadmap state, and closure documents. GREEN advances the
version and synchronizes package-facing descriptions without changing product
behavior.

The exact Alpha-8 candidate then receives complete normal and HDB00
qualification. Documentation closure occurs only after those workflows pass.

## 9. CI authority and cadence

The normal pull-request workflow remains the authoritative executable HDB08
qualification gate because PR development does not create a `main` push or a
release tag.

Static contract tests freeze equivalent HDB08 wiring in `main.yaml` and
`release.yaml`. Those workflows reuse the same package verifier and consumer
scripts exercised by PR CI; they do not contain independent package semantics.

Every pushed HDB08 checkpoint runs the normal PR workflow. HDB00 retains its
existing synchronize-delta classifier. Changes limited to packaging tools,
ordinary workflows, version authority, and HDB08 documentation do not require
native fixture regeneration unless the existing classifier identifies a
sensitive path. The exact final Alpha-8 implementation head must nevertheless
have successful normal 12-job and HDB00 3-job workflow results.

## 10. Failure boundaries

Package-verifier failures must identify the exact unexpected or missing
package entry, metadata value, dependency, assembly identity, symbol, or native
payload. They fail before package artifacts are uploaded for cross-host smoke.

Package-consumer failures identify the operating system and target framework
through the matrix job and script diagnostic. A failed consumer does not fall
back to source-tree project references, a globally installed Icod package, or
an installed Berkeley DB library.

Artifact download, extraction, restore, build, or execution failures remain
ordinary CI failures. HDB08 adds no retry that could conceal a deterministic
package defect.

## 11. Technology and repository constraints

New HDB08 work is limited to:

- C# 13 and .NET 8/9/10;
- Windows PowerShell 5.1-compatible PowerShell; and
- cmd/sh and GitHub Actions YAML.

HDB08 adds no Python, C, C++, native source, inline native program, or new
runtime/build dependency. Existing historical tooling outside HDB08 is not
rewritten merely for language uniformity.

Generated packages, symbols, archives, extracted assemblies, and native
fixtures remain workflow artifacts or temporary files. They are not committed
to the repository.

## 12. Documentation and acceptance record

HDB08 updates:

- the 1.15 roadmap;
- the root README;
- `Icod.TermInfo.BerkeleyDb/README.md`;
- `packaging/README.md` and relevant tooling documentation;
- the BerkeleyDb package release notes;
- the HDB08 implementation plan and closure record; and
- PR #45's body.

The closure record distinguishes:

- the accepted HDB07C behavior head from the HDB08 distribution head;
- canonical package production from three-host consumption;
- ordinary managed package qualification from native HDB00 evidence; and
- Alpha-8 acceptance from later stable release publication.

## 13. Acceptance criteria

HDB08 is accepted only when:

1. the coordinated version is exactly `1.15.0-Alpha-8`;
2. all seven coordinated `.nupkg` files and six reusable-library `.snupkg`
   files pass exact verification;
3. the BerkeleyDb package has exactly one Runtime dependency per supported
   target framework at the coordinated version;
4. the BerkeleyDb package contains no native or runtime-specific asset;
5. its assembly identity is `Icod.TermInfo.BerkeleyDb`, version `1.0.0.0`, on
   net8, net9, and net10;
6. its symbol package has one portable PDB with valid Source Link per target
   framework;
7. the isolated BerkeleyDb package consumer passes net8, net9, and net10 on
   Windows, Linux, and macOS against one canonical package set;
8. ordinary consumer jobs install no Berkeley DB or ncurses dependency;
9. installed-tool package smoke passes on Windows, Linux, and macOS;
10. matching-host smoke passes for all six archive RIDs;
11. PR, `main`, and release workflow contracts retain the required package and
    archive topology;
12. reusable public APIs remain equivalent across target frameworks and
    unchanged from HDB07C;
13. production dependency direction, JSON, command, native-asset, and read-only
    boundaries remain unchanged;
14. the normal PR workflow passes all 12 jobs on the exact final Alpha-8 head;
15. HDB00 passes all 3 jobs on that exact head; and
16. PR #45 remains open, draft, and unmerged.

## 14. Explicit non-goals

HDB08 does not:

- publish a package, create a tag, merge PR #45, or mark it ready for review;
- perform stable `1.15.0` promotion;
- create the final checked-in 1.15 public API manifest;
- add acquisition behavior, encoding support, or Berkeley DB format support;
- add package signing or a new registry;
- make ordinary consumers install Berkeley DB or ncurses;
- independently pack three candidate package sets;
- add an archive RID;
- claim an atomic database snapshot or arbitrary writer coordination; or
- broaden the HDB07C native big-endian or Latin-1 compatibility claims.

## 15. Next tranche

After HDB08 acceptance, HDB09 performs final API/dependency freeze,
acquisition and compatibility documentation, security/resource audit,
ecosystem/release audit, stable-version promotion, and release-candidate
closure.

Stable `1.15.0` adds no feature semantics beyond the accepted Alpha-8
contract. Publication still requires explicit user direction after merge,
post-merge Release validation, and a separate tag.
