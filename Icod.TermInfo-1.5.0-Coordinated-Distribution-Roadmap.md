# Icod.TermInfo 1.5.0 — Coordinated Distribution Roadmap

**Release version:** `1.5.0`
**Stable assembly version:** `1.0.0.0`
**Status:** Release finalization
**Primary change:** Centralized suite versioning and installable command routing
**API policy:** No reusable-library public API changes
**Command policy:** No changes to the frozen 1.4 `tic`, `infocmp`, or `toe` semantics

---

## 1. Release objective

Version 1.5.0 is a distribution and release-engineering release.

It SHALL:

- establish one repository-level version authority;
- coordinate Runtime, Source, Compiler, Inspection, and command versions;
- add the installable `Icod.TermInfo.Tools` .NET tool package;
- expose the router command `icod-terminfo`;
- route `tic`, `infocmp`, and `toe` in-process through their existing command
  implementations;
- retain the six framework-dependent standalone tool-suite archives;
- preserve all frozen reusable-library public APIs;
- preserve the released 1.4 command semantics.

Termcap interoperability is deferred to the 1.6 development line.

---

## 2. Frozen lower-layer contracts

The following reusable-library contracts SHALL remain unchanged:

```text
Icod.TermInfo
Icod.TermInfo.Source
Icod.TermInfo.Compiler
Icod.TermInfo.Inspection
```

Version 1.5 SHALL NOT introduce a new reusable-library public API baseline.

The existing public API baselines and exact `net8.0` / `net9.0` / `net10.0`
API-equivalence gates remain authoritative.

The dependency graph remains:

```text
Icod.TermInfo

Icod.TermInfo.Source
    -> Icod.TermInfo

Icod.TermInfo.Compiler
    -> Icod.TermInfo.Source
    -> Icod.TermInfo

Icod.TermInfo.Inspection
    -> Icod.TermInfo.Source
    -> Icod.TermInfo
```

Inspection SHALL NOT acquire a Compiler dependency.

---

## 3. Coordinated version authority

`Directory.Build.props` SHALL declare:

```text
IcodTermInfoSuiteVersion = 1.5.0
```

The four reusable libraries SHALL consume that value for both `Version` and
`PackageVersion`.

The standalone `tic`, `infocmp`, and `toe` projects SHALL consume the same value
for `Version` while remaining non-packable.

The router SHALL consume the same value for `Version` and `PackageVersion`.

No coordinated project SHALL carry an independent current-release version
literal.

The four reusable assemblies SHALL retain `AssemblyVersion` `1.0.0.0`.

---

## 4. Installable router

The distribution-only router project is:

```text
icod-terminfo/Icod.TermInfo.Router.csproj
```

It SHALL:

- target `net10.0`;
- package as `Icod.TermInfo.Tools`;
- set `PackAsTool` to `true`;
- expose `ToolCommandName` `icod-terminfo`;
- reference `tic`, `infocmp`, and `toe`;
- contain no duplicated terminfo engine;
- contain no duplicated command-specific option parser.

The supported routing surface is:

```text
icod-terminfo --help
icod-terminfo --version
icod-terminfo tic ...
icod-terminfo infocmp ...
icod-terminfo toe ...
```

The router SHALL strip only the selected command name and forward the remaining
arguments to the existing command `RunAsync` entry point.

It SHALL preserve caller-owned streams, cancellation, and the selected command's
exit status.

---

## 5. Distribution artifacts

The registry artifact set is:

```text
Icod.TermInfo
Icod.TermInfo.Source
Icod.TermInfo.Compiler
Icod.TermInfo.Inspection
Icod.TermInfo.Tools
```

The four reusable libraries produce `.nupkg` and `.snupkg` artifacts. The router
produces one `.nupkg`.

The router package SHALL be host-neutral. It may contain the managed command
assemblies required for in-process dispatch, but it SHALL NOT contain
RID-specific `tic`, `infocmp`, or `toe` apphosts in its `any` payload.

The package artifact count is therefore:

```text
5 .nupkg
4 .snupkg
```

The six existing framework-dependent standalone archives remain part of the
release. They continue to expose the traditional `tic`, `infocmp`, and `toe`
command names and do not need to contain the router.

Before checksumming, the GitHub Release input set SHALL contain fifteen files:
nine package artifacts plus six standalone archives.

`SHA256SUMS.txt` is the sixteenth and final release asset.

---

## 6. Dual execution gates

Both distribution forms SHALL be executed before publication.

### 6.1 Standalone archive smoke

On matching Windows, Linux, and macOS hosts:

1. unpack the appropriate archive;
2. require `tic -V`, `infocmp -V`, and `toe -V` to report `1.5.0`;
3. publish a controlled entry with `tic`;
4. acquire it with `infocmp`;
5. enumerate it with `toe`.

### 6.2 Installable router smoke

On Windows, Linux, and macOS:

1. install the freshly produced local `Icod.TermInfo.Tools.1.5.0.nupkg`;
2. require `icod-terminfo --version` to report `1.5.0`;
3. require `icod-terminfo tic -V` to report `1.5.0`;
4. require `icod-terminfo infocmp -V` to report `1.5.0`;
5. require `icod-terminfo toe -V` to report `1.5.0`;
6. publish a controlled entry through routed `tic`;
7. acquire it through routed `infocmp`;
8. enumerate it through routed `toe`.

The install smoke SHALL NOT resolve the router from a previously published
registry package.

---

## 7. Release closure

Before merge/tag publication, the exact release-finalization commit SHALL pass:

- Release restore/build/test on Windows, Linux, and macOS;
- all frozen reusable-library API-baseline checks;
- exact cross-target API equivalence;
- all four reusable-library package verifiers and fresh consumers;
- Router unit and contract tests;
- structural verification of the `Icod.TermInfo.Tools` package;
- structural verification of all six standalone archives;
- matching-host standalone archive smoke;
- matching-host installed-router smoke.

The immutable publication contract is defined by
[`docs/1.5.0-RELEASE-AUDIT.md`](docs/1.5.0-RELEASE-AUDIT.md).

After the branch is merged to `main`, the exact merge commit SHALL pass the main
Release validation before `v1.5.0` is created.

No source, package, archive, documentation, command-contract, or API-baseline
change may occur between final validation and tag creation without rerunning the
complete release gate.

---

## 8. Completion gate

Version 1.5.0 is complete only when:

- the exact validated `main` commit is tagged `v1.5.0`;
- all five `.nupkg` packages are published to NuGet.org;
- the same five packages are published to GitHub Packages;
- the GitHub Release contains exactly sixteen expected assets;
- fresh consumers restore all four reusable libraries;
- the installed router dispatches all three commands;
- all six standalone archives remain downloadable and checksummed.

After 1.5.0 is immutable, termcap interoperability proceeds on the 1.6
development line.
