# Icod.TermInfo Development Roadmap — 1.0.0

**Project:** `Icod.TermInfo`
**Package:** `Icod.TermInfo`
**Supported target frameworks:** `net8.0`, `net10.0`
**Language:** C# 13
**Status:** Active — T42 contract and API-regret audit
**Previous contract:** `0.9.0` — arbitrary compiled terminfo acquisition
**Contract target:** `1.0.0`
**Initial development version:** `1.0.0-alpha.1`

---

## 1. Purpose

Version 1.0 does not add another terminal family or another acquisition format.

The purpose of 1.0 is to declare that the low-level `Icod.TermInfo` job is
stable enough to carry a long-lived public contract:

- terminal-description semantics are complete for the supported terminfo model;
- conventional compiled-database acquisition is complete for the frozen 0.9
  format/discovery scope;
- public API and assembly identity are intentionally reviewed before stability;
- supported target frameworks and operating-system families are explicit;
- package structure, symbols, documentation, and compatibility gates are
  enforceable;
- future terminal-system families remain outside this package unless they fit
  the established low-level description boundary.

No T42-T45 tranche should add runtime functionality merely to make 1.0 appear
larger.

---

## 2. Frozen 1.x platform and identity policy

The 1.x support contract is:

```text
Target frameworks:
    net8.0
    net10.0

CI host families:
    Windows
    Linux
    macOS
```

Both target frameworks must compile and execute the complete test suite on the
three-host CI matrix.

The NuGet package must contain first-class managed/XML assets for both target
frameworks. The fresh-package consumer must restore and execute against each
framework independently.

Repository-only maintenance tools may target `net10.0`; they are not shipped
consumer assets.

### 2.1 Assembly identity

Beginning with T42:

```text
AssemblyName       Icod.TermInfo
AssemblyVersion    1.0.0.0
Strong name        no
```

`AssemblyVersion` remains `1.0.0.0` throughout the 1.x package line unless a
future compatibility review deliberately changes that policy.

Package/informational versions continue to carry normal semantic versions and
prerelease labels.

The 1.x line remains unsigned. Strong naming is not a security boundary and is
not added without a concrete consumer requirement. Adding a strong name later
changes assembly identity and therefore belongs at a deliberate major-version
boundary unless compatibility evidence proves otherwise.

---

## 3. Public API stability rule

T42 is the last intentionally cheap breaking-change window before 1.0.

The repository retains the existing semantic `PublicApiSurfaceTests` and adds
`Icod.TermInfo.PublicApiSnapshot`, which emits a deterministic reflection
manifest covering every exported type and declared public/protected member.

The first 1.0 baseline is generated and manually reviewed during T42:

```text
dotnet run --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj -c Release -- --write
```

The candidate baseline is:

```text
docs/1.0.0-PUBLIC-API-BASELINE.txt
```

That file must not be accepted by reflexively running `--write`. A maintainer
must read the manifest and explicitly answer:

> If source/binary compatibility did not matter, is there anything in this
> exported API that we would rename, remove, reshape, or hide?

Any regretted API is corrected in T42 before the baseline is approved.

After approval, later tranches use `--check` rather than rewriting the baseline.

---

## T42 — 1.0 Contract and API Regret Audit

**Development version:** `1.0.0-alpha.1`
**Status:** Implementation in progress
**Implementation record:** `docs/1.0.0-T42-CONTRACT-API-AUDIT.md`

T42 SHALL:

- move `<Version>` and `<PackageVersion>` to `1.0.0-alpha.1`;
- set `<AssemblyVersion>` explicitly to `1.0.0.0`;
- freeze the unsigned 1.x assembly policy;
- freeze `net8.0;net10.0` as the supported package target set;
- freeze Windows/Linux/macOS as the CI host-family set;
- require both supported SDK/runtime lines in build/test and package-validation
  CI jobs;
- retain `push-main.yaml` as main-only publication and
  `pr-build-and-test.yaml` as non-publishing PR validation;
- normalize repository-only maintenance executables to `net10.0`;
- make the isolated fresh-package consumer execute on both supported target
  frameworks;
- make the package verifier require both framework payloads, XML docs, and
  symbol PDBs;
- make the package verifier assert stable assembly version and unsigned identity
  for both packaged assemblies;
- add the exhaustive public API snapshot tool;
- remove the obsolete T41 current-version assertion and move current-version
  ownership to T42;
- add T42 contract tests;
- add no production runtime feature.

### T42 acceptance

Before T42 is marked complete:

1. Debug/Staging/Release solution builds pass as appropriate.
2. Tests pass for both `net8.0` and `net10.0`.
3. Release package validation passes.
4. Fresh package smoke passes separately for both frameworks.
5. The package verifier sees exactly the intended `net8.0` and `net10.0`
   managed assets.
6. The candidate API baseline is generated.
7. The complete baseline is manually reviewed for API regret.
8. Any approved corrections are applied and the baseline is regenerated.
9. The final T42 baseline is committed and `--check` passes.

---

## T43 — 1.0 Robustness and Compatibility Gate

**Development version:** `1.0.0-beta.1`

T43 SHALL concentrate on confidence rather than feature expansion:

- expanded compiled-entry corpus;
- larger deterministic mutation/fuzz campaign;
- pinned ncurses differential campaign;
- culture-independence audit;
- concurrency/cache stress audit;
- resource/bounds audit;
- package compatibility baseline against published `0.9.0`;
- net8/net10 API-equivalence gate;
- package icon metadata correction;
- final package metadata audit.

T43 must not weaken or casually rewrite the approved T42 API baseline.

---

## T44 — 1.0 Documentation and Package Freeze

**Development version:** `1.0.0-rc.1`

T44 SHALL:

- complete XML documentation for every public member;
- remove the Release `CS1591` exemption;
- add stable versioning/compatibility policy documentation;
- document the 1.x assembly-version and unsigned policy;
- document the support lifecycle and target framework contract;
- modernize `CONTRIBUTING.md`;
- modernize `docs/FUTURE-WORK-INVENTORY.md`;
- audit the root README and sample documentation;
- freeze final 1.0 package contents.

---

## T45 — 1.0.0 Completion Gate

**Final version:** `1.0.0`

Before tagging `v1.0.0`, require:

- Windows/Linux/macOS Release CI passes;
- both `net8.0` and `net10.0` tests pass;
- exact approved public API baseline passes;
- 0.9 compatibility audit passes;
- semantic compatibility audit passes;
- package verifier passes;
- fresh package consumer passes independently on both frameworks;
- Source Link and both symbol assets pass;
- package-content/dependency purity passes;
- final documentation audit passes;
- final package identifies version `1.0.0` while assembly identity remains
  `1.0.0.0`.

No source or package-content change is permitted between the successful final
validation and the `v1.0.0` tag without rerunning the completion gate.

---

## 4. Explicitly outside 1.0 readiness scope

The following are not prerequisites for `Icod.TermInfo` 1.0:

- `.ti` source parsing;
- `use=` source inheritance;
- `tic`/`infocmp`-class tooling;
- termcap parsing/conversion;
- Berkeley DB/hashed terminfo stores;
- divergent historical vendor binary formats outside the frozen parser family;
- live keyboard/mouse/paste/focus decoding;
- active terminal probing/negotiation;
- raw/cooked terminal session ownership;
- PTY/ConPTY management;
- curses/windows/pads/panels/widgets;
- terminal emulation;
- graphics protocols.

Those remain mapped in `docs/FUTURE-WORK-INVENTORY.md`.
