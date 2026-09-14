# Icod.TermInfo 1.14.0 — Raster Backend Capability Evidence, Selection, and Planning

## 1. Purpose

`Icod.TermInfo 1.14.0` adds a backend-selection layer above the frozen persistent-raster semantic stack. It answers a new question without weakening the existing answers:

```text
1.11  lifecycle evidence / classification / planning
1.12  advanced placement semantics / planning
1.13  backend-neutral runtime observations / integration
1.14  backend availability evidence / candidate evaluation / selection
```

The new layer remains advisory and lives in `Icod.TermInfo.Inspection`. TermInfo does not probe terminals, execute graphics protocols, allocate terminal-side identities, or depend on `Icod.Terminal` in production.

## 2. Release-wide design rules

1. Initial concrete backend identities are Sixel and Kitty Graphics.
2. Backend availability is independent from lifecycle and placement capability truth.
3. Frozen 1.11 lifecycle and 1.12 placement planners remain authoritative.
4. Frozen 1.13 runtime observations remain backend-neutral and unchanged.
5. Callers maintain separate 1.13 integration contexts per backend when performing backend-specific verification.
6. No terminal-name, emulator-brand, profile-name, or enum-order heuristic may infer backend support or preference.
7. No hidden backend ranking is permitted.
8. With no explicit caller preference and multiple viable candidates, planning returns `RequiresPreference`.
9. A complete explicit preference order is semantic fallback policy: an unverified preferred candidate blocks fallback until verified or impossible.
10. Production `Icod.TermInfo.Inspection` remains free of any `Icod.Terminal` package/project dependency.

---

# 3. RB01 / Alpha-1 — Backend evidence and public model foundation

RB01 establishes the reviewed 1.14 backend-selection vocabulary and immutable model.

**Accepted exact head:** `7f43c4ad27f1858f8648237cbcb26e6808142458`

**Qualification:** workflow #865 / run `34862203053`, all 12 jobs green.

RB01 adds exactly 13 reviewed `Icod.TermInfo.Inspection.RasterBackend*` public types and preserves exact historical Inspection compatibility.

---

# 4. RB02 / Alpha-2 — Backend classification and conservative static inspection

RB02 adds deterministic backend availability classification and static inspection.

`RasterBackendClassifier` uses:

```text
Verified > Declared > CapabilityDerived
```

Within the highest present precedence:

- positive only => `Supported`;
- negative only => `Unsupported`;
- both polarities => `Contradicted`;
- no evidence => `Unknown`.

Lower-precedence contradictions do not override stronger conclusions.

`RasterBackendInspector` recognizes only exact Boolean Sixel metadata. Absence remains unknown. Kitty Graphics is never inferred from terminal names, profile identities, unrelated capabilities, xterm/Windows Terminal identity, or Sixel metadata.

**Accepted exact head:** `6321de472a6e54a8382dd214996c22a4cf62e816`

**Qualification:** workflow #875 / run `34865731077`, all 12 jobs green.

RB02 adds exactly two more reviewed public types, bringing the reviewed whole-1.14 type set through RB02 to 15.

---

# 5. RB03 / Alpha-3 — Candidate composition through frozen planners

## 5.1 Objective

Compose backend availability with the already-frozen lifecycle and placement planners without duplicating their semantics.

## 5.2 Selection request

The request composes the existing frozen request types:

```text
PersistentRasterLifecycleRequest
PersistentRasterPlacementRequest?
```

`PlacementRequest = null` means no advanced 1.12 placement semantics are requested. This is required because the frozen `PersistentRasterPlacementRequest` intentionally rejects an empty request.

1.14 SHALL NOT duplicate their semantic fields.

## 5.3 Candidate evaluation algorithm

For each candidate:

1. classify backend availability;
2. if backend availability is `Unsupported`, candidate -> `Impossible`;
3. if backend availability is `Unknown` or `Contradicted`, candidate -> `RequiresRuntimeVerification`;
4. if backend availability is `Supported`, call the existing lifecycle planner;
5. lifecycle `Impossible` -> candidate `Impossible`;
6. lifecycle `Indeterminate` -> candidate `RequiresRuntimeVerification`;
7. lifecycle `Success` with no placement request -> candidate `Satisfied`;
8. lifecycle `Success` with a placement request -> call the existing placement planner;
9. placement `Impossible` -> candidate `Impossible`;
10. placement `RequiresRuntimeVerification` or `Indeterminate` -> candidate `RequiresRuntimeVerification`; and
11. placement `Satisfied` -> candidate `Satisfied`.

The candidate evaluation SHALL retain the actual lifecycle and placement plans used to reach the status.

## 5.4 No backend inference

The evaluator SHALL NOT special-case Sixel or Kitty Graphics capabilities.

Backend kind identifies the candidate only; lifecycle/placement truth comes from the candidate's profiles.

## 5.5 Gate

**RB03 gate:** candidate status is a pure deterministic composition of backend availability plus the frozen 1.11/1.12 planners, with no duplicated semantic rules.

**Accepted exact head:** `22f38e9c6516bec9d5f6b8662f00e9b5bace7c2b`

**Qualification:** workflow #885 / run `34871582203`, all 12 jobs green.

The tranche also hardened the compatibility helper so singleton reviewed-type ledgers retain collection identity under Windows PowerShell and PowerShell 7.

**Alpha checkpoint:** `1.14.0-Alpha-3`.

---

# 6. RB04 / Alpha-4 — Deterministic preference-aware backend selection

## 6.1 Objective

Turn candidate evaluations into one explicit overall advisory decision without hidden ranking.

## 6.2 Explicit preference semantics

When a complete caller preference order is supplied, candidates are considered in that order:

- `Impossible` candidates are skipped;
- the first `RequiresRuntimeVerification` candidate stops selection and yields overall `RequiresRuntimeVerification` for that preferred candidate;
- the first `Satisfied` candidate is selected; and
- if all candidates are impossible, the plan is `Impossible`.

This intentionally prevents silent fallback past an unverified preferred candidate.

## 6.3 No-preference semantics

When no preference order is supplied:

- all candidates impossible -> `Impossible`;
- exactly one non-impossible candidate -> reflect that candidate as `Selected` or `RequiresRuntimeVerification`;
- more than one non-impossible candidate -> `RequiresPreference`.

Canonical evaluation ordering SHALL NOT choose a backend.

## 6.4 Selected backend

`SelectedBackend` SHALL be populated only when overall status is `Selected`.

All other statuses expose an explicit null/absent selection and retain candidate evaluations explaining why.

## 6.5 Gate

**RB04 gate:** permutation tests prove input-order independence, explicit preference controls fallback deterministically, and the no-preference path never chooses between multiple viable candidates.

**Accepted exact head:** `d202fd2e452b083de92b84521f1845a30354aede`

**Qualification:** workflow #896 / run `34877834128`, all 12 jobs green.

**Alpha checkpoint:** `1.14.0-Alpha-4`.

---

# 7. RB05 / Alpha-5 — 1.13 runtime-integration composition and orchestration

## 7.1 Objective

Make per-backend runtime verification convenient without changing the frozen 1.13 observation/integration model.

## 7.2 Composition helper

RB05 adds a convenience composition responsibility equivalent to:

```text
RasterBackendProfile
+ PersistentRasterRuntimeIntegrationResult
    -> RasterBackendCandidate
```

The integration result already contains strengthened lifecycle and placement profiles.

The helper packages those profiles; it does not re-integrate evidence.

## 7.3 Backend-specific verification contexts

Consumers verifying more than one backend SHALL maintain separate 1.13 integration contexts.

Example:

```text
Sixel runtime observations
    -> 1.13 integrator
    -> Sixel lifecycle/placement profiles
    -> Sixel candidate

Kitty runtime observations
    -> 1.13 integrator
    -> Kitty lifecycle/placement profiles
    -> Kitty candidate
```

The same backend-neutral observation type may be reused in each context because backend scope is supplied by the caller's composition, not embedded in the 1.13 type.

## 7.4 No trust elevation

RB05 SHALL NOT:

- turn `Inconclusive` observations into evidence;
- convert backend availability from lifecycle observations;
- assign new source ordinals;
- weaken evidence capacity/ordinal failure behavior; or
- manufacture `Verified` evidence outside the existing 1.13 integrator.

## 7.5 Gate

**RB05 gate:** strengthened 1.13 results can feed 1.14 candidates without manual lifecycle/placement evidence reconstruction and without any change to the 1.13 public surface or behavior.

**Accepted exact head:** `a4aab5e7af4d554cad7c978cef20491744100c80`

**Qualification:** workflow #900 / run `34880872852`, all 12 jobs green.

**Alpha checkpoint:** `1.14.0-Alpha-5`.

---

# 8. RB06 / Alpha-6 — JSON version 6 backend automation

## 8.1 Objective

Add deterministic machine-readable backend profile and selection-plan output while preserving JSON versions 1-5.

## 8.2 Schema version

RB06 adds:

```text
urn:icod:terminfo:inspection:json:6
```

with exactly two new document kinds:

```text
rasterBackendProfile
rasterBackendSelectionPlan
```

## 8.3 Backend profile document

The profile document preserves:

- backend identity;
- support status;
- canonical availability evidence;
- evidence kind/polarity/source label/source ordinal; and
- deterministic ordering.

## 8.4 Selection-plan document

The plan document preserves:

- lifecycle request;
- placement request;
- caller preference order;
- overall status;
- selected backend or explicit null;
- all candidate evaluations in canonical backend order;
- each candidate's backend support status;
- lifecycle plan status/requirements;
- placement plan status/requirements; and
- the explicit remaining blocker: verification, preference, or impossibility.

## 8.5 Compatibility

JSON versions 1 through 5 remain byte-for-byte stable for their existing inputs.

No JSON input/deserialization is introduced.

## 8.6 Bounds

The renderer continues using existing `TermInfoJsonRendererOptions` output-byte bounds and cancellation semantics.

## 8.7 Gate

**RB06 gate:** version-6 output is deterministic, schema-valid, culture-independent, bounded, repeatable across processes, and additive to frozen v1-v5.

**Accepted exact head:** `d20d1c7b9a727b20dbcb931f1ac980a0e961b372`

**Qualification:** workflow #904 / run `34893643336`, all 12 jobs green.

The accepted Alpha-6 head contains the synchronized version authority, exact six-member additive renderer ledger, compatibility reconstruction support, packaged strict v6 schema, and qualified JSON behavior.

**Alpha checkpoint:** `1.14.0-Alpha-6`.

---

# 9. RB07 / Alpha-7 — Terminal 1.13 qualification and focused samples

## 9.1 Objective

Prove the loose-coupling contract against real published downstream packages without adding a production Terminal dependency.

## 9.2 Package-only consumer

The isolated consumer references:

- freshly packed `Icod.TermInfo.Inspection` 1.14 candidate package; and
- published stable `Icod.Terminal 1.13.0`.

It uses no project reference to production TermInfo projects.

## 9.3 Adapter boundary

The consumer makes Terminal-to-TermInfo mapping policy explicit.

The qualification flow is:

```text
TermInfo TerminalDescription
    -> explicit Sixel capability-derived backend evidence
    -> Sixel candidate

Terminal semantic verification
    -> caller-owned conclusive-result mapping
    -> backend-specific 1.13 observations/integration
    -> explicit caller-owned Kitty availability evidence
    -> Kitty candidate

Sixel + Kitty candidates
    -> 1.14 backend planner
    -> explicit preference-aware plan
    -> JSON v6 output
```

If a Terminal semantic capability is coarser than the TermInfo semantic vocabulary, the expansion remains visible caller policy just as in 1.13 qualification.

The sample does not inspect Terminal internal backend resolver types.

## 9.4 Focused sample

RB07 adds:

```text
samples/Icod.TermInfo.RasterBackendSelection.Sample
```

The default mode is deterministic and CI-safe. Live mode uses the published `TerminalSession.VerifyCapabilityAsync(TerminalCapability.PersistentRasterGraphics)` API and keeps real observation facts separate from caller mapping policy.

## 9.5 Existing samples

Existing lifecycle, placement, and runtime-integration samples remain valid and are not rewritten merely to advertise 1.14.

## 9.6 Production dependency gate

Permanent tests prove production `Icod.TermInfo.Inspection` has no `Icod.Terminal` package or assembly dependency.

## 9.7 Gate

**RB07 gate:** package-only net8/net9/net10 consumers, focused sample, published Terminal 1.13 interop, tool package smoke, and all six archive RIDs remain green without production coupling.

**Accepted exact head:** `0ce8df7f08064de807db17a2662f38252a814ce5`

**Qualification:** workflow #910 / run `34898618294`, all 12 jobs green.

The accepted head synchronizes `1.14.0-Alpha-7` accounting on top of the qualified package-only consumer and sample. RB07 adds no Inspection public API.

**Alpha checkpoint:** `1.14.0-Alpha-7`.

---

# 10. RB08 / Alpha-8 — Adversarial hardening, exact freeze, documentation, and release closure

## 10.1 Objective

Freeze 1.14 only after adversarial proof that backend selection is deterministic, bounded, non-heuristic, and backward-compatible.

**Status:** IN PROGRESS from accepted Alpha-7 authority `0ce8df7f08064de807db17a2662f38252a814ce5`.

## 10.2 Required adversarial cases

RB08 SHALL test at least:

- duplicate backend candidates;
- duplicate preference values;
- incomplete preference orders;
- invalid enum values;
- evidence exactly at and just beyond configured bounds;
- maximum source-label length and overflow-by-one;
- compatible evidence duplicates;
- highest-precedence contradictions;
- lower-precedence contradictions shadowed by higher precedence;
- static Sixel positive advertisement;
- static Sixel absence;
- terminal names that contain `kitty`, `sixel`, `xterm`, or other misleading strings but carry no authoritative metadata;
- multiple satisfied candidates with no preference;
- satisfied + verification-required candidates with no preference;
- explicit preferred verification-required candidate before a satisfied fallback;
- preferred impossible candidate followed by satisfied fallback;
- all candidates impossible;
- lifecycle indeterminate/impossible propagation;
- placement verification/indeterminate/impossible propagation;
- candidate input permutations;
- evidence input permutations;
- non-English cultures;
- repeated separate processes;
- cancellation;
- JSON exact byte-bound boundaries; and
- package-only consumption across all target frameworks.

## 10.3 Public API freeze

Generate the exact complete Inspection reflection manifest from a validated package artifact and freeze its normalized-LF SHA-256.

The freeze SHALL prove the 1.14 surface is additive to the accepted 1.13 surface.

## 10.4 JSON freeze

Freeze normalized-LF fingerprints for JSON schemas v1-v6 and verify historical versions exactly.

## 10.5 Documentation

Produce at minimum:

```text
docs/1.14.0-RASTER-BACKEND-SELECTION-GUIDE.md
docs/1.14.0-INSPECTION-PUBLIC-API-FREEZE.md
docs/1.14.0-RB08-FREEZE-FINGERPRINTS.txt
docs/1.14.0-RELEASE-AUDIT.md
```

Update:

- root `README.md`;
- `Icod.TermInfo.Inspection/README.md`;
- `samples/README.md`;
- `docs/VERSIONING.md`;
- `docs/COMPATIBILITY.md`; and
- `Icod.TermInfo-Post-1.0-Development-Roadmap.md`.

## 10.6 Release qualification

The exact Alpha-8 head SHALL pass the complete 12-job Staging-equivalent matrix:

- Windows Build/Test + compatibility reconstruction;
- Linux Build/Test + package verification;
- macOS Build/Test;
- three installed-tool package smokes; and
- six archive RID smokes.

Stable `1.14.0` promotion SHALL then be version/status-only unless an explicitly reviewed release defect requires correction.

## 10.7 Gate

**RB08 gate:** exact API/schema fingerprints are frozen, all adversarial and distribution gates are green, release-facing documentation is internally consistent, and stable promotion can occur without semantic changes.

**Alpha checkpoint:** `1.14.0-Alpha-8` intended final prerelease.

---

# 11. Release-wide invariants

Every 1.14 tranche SHALL preserve:

1. `Icod.TermInfo` runtime remains dependency-free.
2. `Icod.TermInfo.Inspection` has no production dependency on `Icod.Terminal`.
3. Runtime/Source/Compiler/Termcap public APIs remain frozen.
4. Inspection 1.13 contracts remain source/binary compatible.
5. 1.13 runtime observations remain backend-neutral.
6. 1.11 lifecycle semantics remain authoritative.
7. 1.12 placement semantics remain authoritative.
8. Backend availability remains a separate dimension.
9. Preference remains caller-owned policy.
10. Canonical ordering remains output determinism only, never hidden ranking.
11. JSON import/deserialization remains excluded.
12. Stable promotion does not change semantics, public API, schema, dependencies, TFMs, command topology, package-consumer topology, or archive RIDs after accepted Alpha-8.
