# Icod.TermInfo Post-1.0 Development Roadmap

**Project:** `Icod.TermInfo`  
**Stable runtime package:** `Icod.TermInfo`  
**Optional source package:** `Icod.TermInfo.Source`  
**Optional compiler package:** `Icod.TermInfo.Compiler`  
**Optional inspection package:** `Icod.TermInfo.Inspection`  
**Optional termcap package:** `Icod.TermInfo.Termcap`  
**Installable tool package:** `Icod.TermInfo.Tools`  
**Language:** C# 13  
**Target frameworks:** `net8.0`; `net9.0`; `net10.0`  
**Frozen runtime contract:** `1.0.0`  
**Current coordinated version:** `1.14.0`  
**Latest completed line:** `1.14.0` - Raster Backend Capability Evidence, Selection, and Planning  
**Latest completed prerelease:** `1.14.0-Alpha-8`  
**Status:** 1.14 feature development is complete; stable `1.14.0` is undergoing final PR qualification before merge/publication  
**Active release roadmap:** `Icod.TermInfo-1.14.0-Raster-Backend-Capability-Evidence-Selection-and-Planning-Roadmap.md`  
**Latest completed release audit:** `docs/1.14.0-RELEASE-AUDIT.md`  
**Next development line:** not yet selected; future work requires a separate design/roadmap decision

---

## 1. Purpose

`Icod.TermInfo 1.0.0` froze the managed runtime foundation: immutable terminal
descriptions, standard/extended capability metadata, compiled terminfo parsing,
database discovery/provider composition, built-in profiles, parameter expansion,
padding-aware output, color semantics, and stable reusable assembly identity.

Post-1.0 development builds outward from that foundation rather than repeatedly
enlarging Runtime. Optional layers and the command distribution remain separate:

```text
                    Icod.TermInfo.Tools
                    /                 \
                   v                   v
      Icod.TermInfo.Compiler   Icod.TermInfo.Inspection
                 |  \             /  |             Icod.TermInfo.Termcap
                 |   \           /   |                      |
                 v    v         v    v                      v
          Icod.TermInfo.Source ---> Icod.TermInfo <----------+
                                   stable runtime
```

The arrows are production dependency arrows. `Icod.TermInfo` remains
dependency-free. Source and Termcap depend on Runtime; Compiler and Inspection
depend on Runtime and Source. Runtime never depends upward on optional layers.

Live-terminal state, probing, input decoding, PTYs, curses presentation, graphics
protocol execution, terminal-side raster identities, and terminal emulation remain
outside this reusable package-family roadmap and belong to sibling systems.

## 1.1 Roadmap authority

This document is the authoritative high-level roadmap after the frozen 1.0
Runtime contract. Version-specific roadmaps and release audits are the detailed
authorities for completed release lines.

`docs/FUTURE-WORK-INVENTORY.md` is retained only for historical links. New work,
ownership decisions, and release sequencing must be recorded here or in a new
version-specific roadmap.

---

## 2. Version sequence

| Version | Theme | Outcome |
|---|---|---|
| **1.1.0** | Terminfo source language | Parse and resolve `.ti` source into `TerminalDescription` |
| **1.2.0** | Terminfo compiler | Write conventional compiled terminfo entries and provide the reusable `tic` engine |
| **1.3.0** | Inspection/comparison | Canonical rendering and semantic comparison for `infocmp`-style consumers |
| **1.4.0** | Tool suite | Managed `tic`, `infocmp`, and `toe` command projects |
| **1.4.1** | Documentation correction | Release-facing metadata/documentation fix with no semantic/API change |
| **1.5.0** | Coordinated distribution | Centralized suite versioning and installable `icod-terminfo` router |
| **1.6.0** | Termcap interoperability | Parse, resolve, convert, render, and explicitly acquire termcap; add `captoinfo` / `infotocap` |
| **1.6.1** | Release-verifier hotfix | Restore caller NuGet-cache state; no public API or command-semantic change |
| **1.7.0** | Relative terminfo source synthesis | Deterministic relative `.ti` synthesis and `infocmp -u` |
| **1.8.0** | Relative source planning | Bounded deterministic ordered parent selection for the frozen 1.7 synthesizer |
| **1.9.0** | Machine-readable automation | Versioned deterministic Inspection JSON and explicit command automation |
| **1.10.0** | Multi-database automation | Ordered explicit catalogs, precedence, conflict analysis, comparison, planning, JSON v2 |
| **1.11.0** | Persistent-raster lifecycle | Protocol-neutral lifecycle evidence/classification/planning, JSON v3 |
| **1.12.0** | Advanced raster placement | Source-rectangle and signed-z-order evidence/planning, JSON v4 |
| **1.13.0** | Runtime evidence interchange | Caller-owned runtime observations/integration and replanning, JSON v5 |
| **1.14.0** | Raster backend selection | Backend availability evidence/classification, candidate evaluation, explicit preference selection, JSON v6 |
| **later** | Explicitly planned deferred work | Additional backends, richer graphics policy, exotic storage/formats, or other justified tracks |

No 1.15 theme is implied by completion of 1.14. A later development line starts
only after a separate design review selects and scopes it.

---

## 3. Completed 1.14 line

Version 1.14 is governed by
`Icod.TermInfo-1.14.0-Raster-Backend-Capability-Evidence-Selection-and-Planning-Roadmap.md`.
Its RB01-RB08 sequence is complete.

### RB01 — backend evidence/model foundation

Adds the bounded immutable raster-backend vocabulary and first 13 reviewed
`RasterBackend*` public types.

Accepted head: `7f43c4ad27f1858f8648237cbcb26e6808142458`  
Qualification: workflow #865 / `34862203053`

### RB02 — deterministic classification and conservative inspection

Adds `Verified > Declared > CapabilityDerived` backend classification and exact
Boolean Sixel static inspection. Kitty Graphics is never inferred from terminal
or profile names.

Accepted head: `6321de472a6e54a8382dd214996c22a4cf62e816`  
Qualification: workflow #875 / `34865731077`

### RB03 — candidate evaluation through frozen planners

Composes backend availability with the frozen 1.11 lifecycle and 1.12 placement
planners without duplicating their semantics.

Accepted head: `22f38e9c6516bec9d5f6b8662f00e9b5bace7c2b`  
Qualification: workflow #885 / `34871582203`

### RB04 — explicit preference-aware selection

Adds deterministic selection with no hidden ranking. Multiple viable candidates
without caller preference yield `RequiresPreference`; complete explicit
preference acts as fallback policy.

Accepted head: `d202fd2e452b083de92b84521f1845a30354aede`  
Qualification: workflow #896 / `34877834128`

### RB05 — frozen 1.13 integration composition

Allows a backend profile to be composed directly with the lifecycle/placement
profiles already retained by a frozen 1.13
`PersistentRasterRuntimeIntegrationResult` without re-integrating evidence or
elevating trust.

Accepted head: `a4aab5e7af4d554cad7c978cef20491744100c80`  
Qualification: workflow #900 / `34880872852`

### RB06 — JSON v6 automation

Adds deterministic bounded `rasterBackendProfile` and
`rasterBackendSelectionPlan` documents while preserving JSON v1-v5 exactly.

Accepted head: `d20d1c7b9a727b20dbcb931f1ac980a0e961b372`  
Qualification: workflow #904 / `34893643336`

### RB07 — published Terminal 1.13 qualification

Adds a package-reference-only consumer and focused raster-backend sample beside
published `Icod.Terminal 1.13.0`, preserving the production no-Terminal-
dependency boundary.

Accepted head: `0ce8df7f08064de807db17a2662f38252a814ce5`  
Qualification: workflow #910 / `34898618294`

### RB08 — adversarial hardening and exact freeze

Freezes the exact complete 106-type Inspection surface and JSON v1-v6
fingerprints, adds adversarial selection/evidence/bounds/culture/process coverage,
and closes release documentation.

Accepted Alpha-8 head: `911e8d44428a26b588193a27e07696c2489bcb93`  
Qualification: workflow #924 / `34903967732`, all 12 jobs green

The stable 1.14.0 transition is version/status/documentation-only and must pass an
independent complete qualification matrix before merge/publication.

---

## 4. Frozen 1.14 contract

The complete `Icod.TermInfo.Inspection` 1.14 reflection manifest contains
**106 exported public types** and has normalized-LF SHA-256:

```text
e9f240a562aec5274d64fb2ec3647862fe4ef5684582af2b442ba3c55e189497
```

The v6 schema contains exactly:

```text
rasterBackendProfile
rasterBackendSelectionPlan
```

and has normalized-LF SHA-256:

```text
9d51ec6659f8978c867408881eefb106f3c4976dfcf2da12b23c374a222665c2
```

Removing only the reviewed 1.14 API delta reconstructs the frozen 1.13 Inspection
surface with SHA-256
`fd827a25abafb8e9ff3915567f45f9f2ec51b332bfd8a82dd4e4bca20290e764`.
The historical reconstruction chain through 1.12, 1.11, and 1.10 remains
unchanged.

---

## 5. Project-family ownership

### `Icod.TermInfo`

Owns immutable terminal descriptions, capability metadata/semantics, compiled
terminfo acquisition, provider composition, parameter expansion, padding-aware
output, color semantics, and built-in terminal profiles. It remains dependency-
free.

### `Icod.TermInfo.Source`

Owns `.ti` lexical analysis, diagnostics, unresolved source state, cancellation,
`use=` inheritance, and materialization into Runtime descriptions.

### `Icod.TermInfo.Compiler`

Owns deterministic compiled-entry writing, source compilation, and explicit
conventional database publication.

### `Icod.TermInfo.Inspection`

Owns deterministic canonical rendering, semantic comparison, relative-source
synthesis/planning, explicit database inspection/automation, persistent-raster
lifecycle/placement evidence and planning, runtime-evidence integration,
raster-backend availability/selection planning, and versioned machine-readable
views through JSON v6.

Inspection does not own live probing or graphics protocol execution and does not
have a production dependency on `Icod.Terminal`.

### `Icod.TermInfo.Termcap`

Owns optional bounded termcap parsing, mapping/classification, `tc=` resolution,
Runtime conversion, reverse rendering, and explicit acquisition.

### Command layer

`tic`, `infocmp`, `toe`, `captoinfo`, and `infotocap` compose reusable libraries
and own command-line policy. `Icod.TermInfo.Tools` is the distribution-only
`icod-terminfo` router.

### Sibling ownership

`Icod.Terminal` owns live terminal/session behavior, input decoding, active
verification/negotiation, and persistent-raster protocol/resource/placement
execution. `Icod.DCurses` owns curses-style virtual-screen/window policy.
PTY/ConPTY and terminal emulation remain separate sibling/future-system concerns.

---

## 6. Compatibility principles

Post-1.0 work must preserve these rules unless a new major release explicitly
changes them:

1. Runtime's frozen 1.0 public API is not expanded casually.
2. Reusable assembly identities stay `1.0.0.0` throughout compatible 1.x.
3. Public API must be equivalent across `net8.0`, `net9.0`, and `net10.0`.
4. Dependency arrows do not reverse.
5. Released JSON schema versions remain immutable.
6. Live terminal/session ownership does not migrate into TermInfo merely because
   Inspection gains descriptive evidence or advisory planners.
7. Stable promotion of an accepted prerelease is not an opportunity to add
   feature/API/schema behavior.

See `docs/VERSIONING.md` and `docs/COMPATIBILITY.md` for the complete policy.

---

## 7. Deferred work after 1.14

The following remain explicitly outside the completed 1.14 scope and require a
future design/roadmap before implementation:

- additional concrete raster backends;
- alpha/pixel-format capability planning;
- performance/latency scoring;
- scene/layout/animation policy;
- terminal-brand preference policy;
- graphics transport or image codecs;
- generic JSON import/deserialization;
- Berkeley DB terminfo storage;
- divergent historical vendor binary formats; and
- any migration of live-session/protocol execution into TermInfo.

The next release theme should be selected based on downstream needs and API-regret
analysis rather than inferred automatically from this list.

---

## 8. Historical authorities

Each completed line retains its own version-specific roadmap and/or release audit.
Those files are the detailed historical record for tranche-level requirements,
accepted exact heads, public API freezes, schema fingerprints, and qualification
evidence.

The current completed line is documented by:

```text
Icod.TermInfo-1.14.0-Raster-Backend-Capability-Evidence-Selection-and-Planning-Roadmap.md
docs/1.14.0-RASTER-BACKEND-SELECTION-GUIDE.md
docs/1.14.0-INSPECTION-PUBLIC-API-FREEZE.md
docs/1.14.0-RB08-FREEZE-FINGERPRINTS.txt
docs/1.14.0-RELEASE-AUDIT.md
```
