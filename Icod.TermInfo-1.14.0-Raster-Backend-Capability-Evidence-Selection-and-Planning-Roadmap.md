# Icod.TermInfo 1.14.0 — Raster Backend Capability Evidence, Selection, and Planning Roadmap

**Project:** `Icod.TermInfo`  
**Release:** `1.14.0`  
**Development branch:** `1.14.0`  
**Theme:** Raster Backend Capability Evidence, Selection, and Deterministic Advisory Planning  
**Primary package:** `Icod.TermInfo.Inspection`  
**Baseline:** stable `1.13.0`  
**Downstream qualification target:** published stable `Icod.Terminal 1.13.0`  
**Frozen contracts:** existing 1.x Runtime/Source/Compiler/Termcap APIs, Inspection contracts through 1.13, JSON schemas v1-v5, database-set precedence, 1.11 persistent-raster lifecycle semantics, 1.12 advanced-placement semantics, and 1.13 runtime-observation/integration semantics except for unavoidable defect corrections  
**Status:** planning approved; design/spec recorded; implementation not yet started  
**Tranche prefix:** `RB`  
**Planned final prerelease:** `1.14.0-Alpha-8`  
**Design authority:** `docs/superpowers/specs/2026-09-14-1.14.0-raster-backend-evidence-selection-design.md`  
**Release audit:** `docs/1.14.0-RELEASE-AUDIT.md` when created

---

## 1. Release objective

`Icod.TermInfo 1.14.0` SHALL add a protocol-aware but execution-free advisory layer for evaluating concrete raster backends against the semantic requirements already modeled by 1.11-1.13.

The completed stack before 1.14 is:

```text
1.11  persistent-raster lifecycle evidence / classification / planning
1.12  advanced placement evidence / classification / planning
1.13  backend-neutral runtime observations and evidence integration
```

Version 1.14 adds the next layer:

```text
backend availability evidence
        +
per-backend lifecycle profile
        +
per-backend placement profile
        |
        v
backend candidate evaluation
        |
        +---- explicit caller preference policy
        |
        v
deterministic backend selection plan
```

The initial concrete backend vocabulary is Sixel and Kitty Graphics because both are meaningful real downstream backends today. Their wire protocols remain outside TermInfo.

TermInfo SHALL answer questions such as:

- Is this backend known supported, unsupported, unknown, or contradicted?
- Can this backend satisfy the requested lifecycle semantics?
- Can this backend satisfy the requested advanced placement semantics?
- Which candidate is selected under the caller's explicit preference order?
- Is runtime verification still required before the preferred backend can be used?
- Is a choice impossible, or merely missing caller preference?

TermInfo SHALL NOT send graphics, probe the terminal, allocate terminal-side identities, or own backend execution.

---

## 2. Architectural rules

### 2.1 Preserve 1.11-1.13 semantics

The following remain authoritative:

- `PersistentRasterLifecycleProfile` and the 1.11 lifecycle classifier/planner;
- `PersistentRasterPlacementProfile` and the 1.12 placement classifier/planner; and
- `PersistentRasterRuntimeObservationSet`, `PersistentRasterRuntimeIntegrationResult`, and the 1.13 evidence integrator.

1.14 SHALL compose those contracts rather than replacing or widening them.

A backend candidate therefore carries existing lifecycle and placement profiles. Backend identity does not create a second copy of lifecycle/placement semantics.

### 2.2 Backend availability is a separate semantic question

A backend being usable for ordinary raster display does not imply persistent upload, acknowledgement, placement creation, multiple placements, placement update/deletion, resource deletion, source rectangles, or signed z-order.

Backend availability SHALL have its own evidence/profile model.

Lifecycle and placement facts remain independently classified.

### 2.3 1.13 observations remain backend-neutral

The 1.13 interchange contract deliberately omitted protocol/backend identity.

1.14 SHALL NOT modify those frozen types.

A caller that verifies multiple backends maintains independent 1.13 integration contexts and then composes each strengthened lifecycle/placement result into a 1.14 backend candidate.

### 2.4 No live terminal ownership

Production TermInfo SHALL NOT:

- open terminal sessions;
- send Sixel DCS;
- send Kitty Graphics APC;
- wait for acknowledgements;
- call Terminal probing APIs;
- inspect PTY/TTY handles;
- own resource IDs or placement IDs;
- retain terminal generations;
- transmit image payloads;
- manage cleanup; or
- depend on `Icod.Terminal`.

### 2.5 No terminal-brand heuristics

1.14 SHALL NOT infer backend support from terminal names, emulator names, environment variables, process ancestry, or profile-brand heuristics.

Explicit capability metadata and explicit caller evidence are authoritative.

### 2.6 No hidden backend ranking

Sixel and Kitty Graphics SHALL NOT have an implicit quality ordering.

Backend enum numeric identity, canonical output ordering, and caller input order SHALL NOT select a winner.

If more than one candidate remains viable and no complete explicit preference policy resolves the choice, the planner SHALL return a preference-required result.

---

## 3. Proposed public responsibilities

Exact names and enum numerics are frozen by RB01, but the release SHALL provide responsibilities equivalent to:

```text
RasterBackendKind
RasterBackendEvidenceKind
RasterBackendEvidence
RasterBackendSupportStatus
RasterBackendProfile
RasterBackendEvidenceOptions
RasterBackendInspector
RasterBackendClassifier

RasterBackendCandidate
RasterBackendSelectionRequest
RasterBackendSelectionOptions
RasterBackendCandidateStatus
RasterBackendCandidateEvaluation
RasterBackendSelectionStatus
RasterBackendSelectionPlan
RasterBackendPlanner
```

The planned initial backend identities are:

```text
Sixel
KittyGraphics
```

The planned availability statuses are:

```text
Unknown
Supported
Unsupported
Contradicted
```

The planned evidence provenance categories are:

```text
CapabilityDerived
Declared
Verified
```

The planned candidate statuses are:

```text
Satisfied
RequiresRuntimeVerification
Impossible
```

The planned overall selection statuses are:

```text
Selected
RequiresRuntimeVerification
RequiresPreference
Impossible
```

No additional public status or backend value SHALL be added casually after RB01.

---

# 4. RB01 / Alpha-1 — Contract and public API regret gate

## 4.1 Objective

Freeze the minimum public vocabulary, bounds, validation rules, and selection-policy invariants before implementing behavior.

## 4.2 Required work

RB01 SHALL:

- add test-first API/regret-gate coverage for the proposed 1.14 public responsibilities;
- freeze numeric identities for backend/status/evidence enums;
- define immutable backend availability evidence;
- define immutable backend profile shape;
- define immutable backend candidate shape;
- define the selection request/options/result vocabulary;
- freeze source-label and evidence-count bounds by reusing existing Inspection safety conventions where possible;
- require exactly one candidate per backend in a selection operation;
- define complete-preference validation;
- establish that no preference means no hidden ranking;
- prove that existing 1.13 public types remain unchanged; and
- advance the coordinated development version to `1.14.0-Alpha-1` only after the RED contract is reviewed.

## 4.3 Evidence shape

Backend availability evidence SHALL be equivalent in responsibility to:

```text
Backend
IsPositive
Kind
SourceLabel
SourceOrdinal
```

It SHALL NOT carry lifecycle or placement subjects.

## 4.4 Candidate shape

A backend candidate SHALL compose:

```text
backend availability profile
existing PersistentRasterLifecycleProfile
existing PersistentRasterPlacementProfile
```

No backend-specific lifecycle enum is introduced.

## 4.5 Preference invariant

A non-empty preference order SHALL be a unique complete ordering of the candidate backends being planned.

Partial preferences SHALL fail argument validation instead of being interpreted as hidden fallback policy.

## 4.6 Gate

**RB01 gate:** tests prove the proposed public contract, validation, bounds, enum numerics, frozen 1.13 surface, and no-hidden-preference rule before behavioral implementation proceeds.

**Alpha checkpoint:** `1.14.0-Alpha-1` only after the contract is green on Windows, Linux, and macOS.

---

# 5. RB02 / Alpha-2 — Backend evidence, classification, and conservative static inspection

## 5.1 Objective

Implement deterministic backend-availability evidence classification and the minimal safe `TerminalDescription` inspection path.

## 5.2 Classifier semantics

Backend evidence precedence SHALL mirror the proven lifecycle precedence shape:

```text
Verified > Declared > CapabilityDerived
```

Within the highest present precedence:

- positive only -> `Supported`;
- negative only -> `Unsupported`;
- both polarities -> `Contradicted`;
- no evidence -> `Unknown`.

Lower-precedence evidence SHALL NOT override a higher-precedence conclusion.

## 5.3 Static Sixel inspection

`RasterBackendInspector` SHALL recognize explicit positive Sixel capability metadata already present in `TerminalDescription`.

The existing extended Boolean `Sixel` may produce positive `CapabilityDerived` evidence for backend availability.

Absence SHALL remain `Unknown`.

## 5.4 Kitty static inspection

RB02 SHALL NOT infer Kitty Graphics from:

- terminal name;
- built-in profile name;
- Windows Terminal identity;
- xterm identity;
- environment variables; or
- unrelated graphics metadata.

Without explicit authoritative capability metadata or caller evidence, Kitty availability remains `Unknown`.

## 5.5 Determinism

Evidence snapshots and classified profiles SHALL be deterministic across:

- evidence input order;
- collection implementation;
- current culture; and
- process execution.

## 5.6 Gate

**RB02 gate:** static Sixel advertisement is supported, absence stays unknown, Kitty is never inferred by brand/name, evidence precedence and contradictions are fully characterized, and immutable snapshots are proven.

**Alpha checkpoint:** `1.14.0-Alpha-2`.

---

# 6. RB03 / Alpha-3 — Backend candidate composition and frozen-planner evaluation

## 6.1 Objective

Evaluate each backend candidate by delegating to the frozen lifecycle and placement planners.

## 6.2 Selection request

The 1.14 request SHALL compose existing request types:

```text
PersistentRasterLifecycleRequest
PersistentRasterPlacementRequest
```

1.14 SHALL NOT duplicate their semantic fields.

## 6.3 Candidate evaluation algorithm

For each candidate:

1. classify backend availability;
2. if backend availability is `Unsupported`, candidate -> `Impossible`;
3. if backend availability is `Unknown` or `Contradicted`, candidate -> `RequiresRuntimeVerification`;
4. if backend availability is `Supported`, call the existing lifecycle planner;
5. lifecycle `Impossible` -> candidate `Impossible`;
6. lifecycle `Indeterminate` -> candidate `RequiresRuntimeVerification`;
7. lifecycle `Success` -> call the existing placement planner;
8. placement `Impossible` -> candidate `Impossible`;
9. placement `RequiresRuntimeVerification` or `Indeterminate` -> candidate `RequiresRuntimeVerification`; and
10. placement `Satisfied` -> candidate `Satisfied`.

The candidate evaluation SHALL retain the actual lifecycle and placement plans used to reach the status.

## 6.4 No backend inference

The evaluator SHALL NOT special-case Sixel or Kitty Graphics capabilities.

Backend kind identifies the candidate only; lifecycle/placement truth comes from the candidate's profiles.

## 6.5 Gate

**RB03 gate:** candidate status is a pure deterministic composition of backend availability plus the frozen 1.11/1.12 planners, with no duplicated semantic rules.

**Alpha checkpoint:** `1.14.0-Alpha-3`.

---

# 7. RB04 / Alpha-4 — Deterministic preference-aware backend selection

## 7.1 Objective

Turn candidate evaluations into one explicit overall advisory decision without hidden ranking.

## 7.2 Explicit preference semantics

When a complete caller preference order is supplied, candidates are considered in that order:

- `Impossible` candidates are skipped;
- the first `RequiresRuntimeVerification` candidate stops selection and yields overall `RequiresRuntimeVerification` for that preferred candidate;
- the first `Satisfied` candidate is selected; and
- if all candidates are impossible, the plan is `Impossible`.

This intentionally prevents silent fallback past an unverified preferred candidate.

## 7.3 No-preference semantics

When no preference order is supplied:

- all candidates impossible -> `Impossible`;
- exactly one non-impossible candidate -> reflect that candidate as `Selected` or `RequiresRuntimeVerification`;
- more than one non-impossible candidate -> `RequiresPreference`.

Canonical evaluation ordering SHALL NOT choose a backend.

## 7.4 Selected backend

`SelectedBackend` SHALL be populated only when overall status is `Selected`.

All other statuses expose an explicit null/absent selection and retain candidate evaluations explaining why.

## 7.5 Gate

**RB04 gate:** permutation tests prove input-order independence, explicit preference controls fallback deterministically, and the no-preference path never chooses between multiple viable candidates.

**Alpha checkpoint:** `1.14.0-Alpha-4`.

---

# 8. RB05 / Alpha-5 — 1.13 runtime-integration composition and orchestration

## 8.1 Objective

Make per-backend runtime verification convenient without changing the frozen 1.13 observation/integration model.

## 8.2 Composition helper

RB05 MAY add a convenience composition responsibility equivalent to:

```text
RasterBackendProfile
+ PersistentRasterRuntimeIntegrationResult
    -> RasterBackendCandidate
```

The integration result already contains strengthened lifecycle and placement profiles.

The helper SHALL package those profiles; it SHALL NOT re-integrate evidence.

## 8.3 Backend-specific verification contexts

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

## 8.4 No trust elevation

RB05 SHALL NOT:

- turn `Inconclusive` observations into evidence;
- convert backend availability from lifecycle observations;
- assign new source ordinals;
- weaken evidence capacity/ordinal failure behavior; or
- manufacture `Verified` evidence outside the existing 1.13 integrator.

## 8.5 Gate

**RB05 gate:** strengthened 1.13 results can feed 1.14 candidates without manual lifecycle/placement evidence reconstruction and without any change to the 1.13 public surface or behavior.

**Alpha checkpoint:** `1.14.0-Alpha-5`.

---

# 9. RB06 / Alpha-6 — JSON version 6 backend automation

## 9.1 Objective

Add deterministic machine-readable backend profile and selection-plan output while preserving JSON versions 1-5.

## 9.2 Schema version

RB06 SHALL add:

```text
urn:icod:terminfo:inspection:json:6
```

with exactly two new document kinds:

```text
rasterBackendProfile
rasterBackendSelectionPlan
```

## 9.3 Backend profile document

The profile document SHALL preserve:

- backend identity;
- support status;
- canonical availability evidence;
- evidence kind/polarity/source label/source ordinal; and
- deterministic ordering.

## 9.4 Selection-plan document

The plan document SHALL preserve:

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

## 9.5 Compatibility

JSON versions 1 through 5 SHALL remain byte-for-byte stable for their existing inputs.

No JSON input/deserialization is introduced.

## 9.6 Bounds

The renderer SHALL continue using existing `TermInfoJsonRendererOptions` output-byte bounds and cancellation semantics.

## 9.7 Gate

**RB06 gate:** version-6 output is deterministic, schema-valid, culture-independent, bounded, repeatable across processes, and additive to frozen v1-v5.

**Alpha checkpoint:** `1.14.0-Alpha-6`.

---

# 10. RB07 / Alpha-7 — Terminal 1.13 qualification and focused samples

## 10.1 Objective

Prove the loose-coupling contract against real published downstream packages without adding a production Terminal dependency.

## 10.2 Package-only consumer

Add an isolated consumer that references:

- freshly packed `Icod.TermInfo`/`Icod.TermInfo.Inspection` 1.14 candidate packages; and
- published stable `Icod.Terminal 1.13.0`.

It SHALL use no project reference to production TermInfo projects.

## 10.3 Adapter boundary

The consumer SHALL make Terminal-to-TermInfo mapping policy explicit.

A suitable qualification flow is:

```text
TermInfo TerminalDescription
    -> explicit Sixel capability-derived backend evidence
    -> Sixel candidate

Terminal session / semantic verification
    -> caller-owned conclusive result
    -> explicit Kitty backend availability policy
    -> backend-specific 1.13 observations/integration
    -> Kitty candidate

Sixel + Kitty candidates
    -> 1.14 backend planner
    -> explicit preference-aware plan
```

If a Terminal semantic capability is coarser than the TermInfo semantic vocabulary, the expansion SHALL remain visible caller policy just as in 1.13 qualification.

The sample SHALL NOT inspect Terminal internal backend resolver types.

## 10.4 Focused sample

Add a dedicated sample responsibility-equivalent to:

```text
samples/Icod.TermInfo.RasterBackendSelection.Sample
```

The default mode SHALL be deterministic and CI-safe.

A live mode MAY use published Terminal APIs, but the sample must remain explicit about which facts are real live observations and which mappings are caller policy.

## 10.5 Existing samples

Existing lifecycle, placement, and runtime-integration samples SHALL remain valid and SHALL NOT be rewritten merely to advertise 1.14.

Only targeted documentation cross-links should be added where useful.

## 10.6 Production dependency gate

Permanent tests SHALL prove production `Icod.TermInfo.Inspection` has no `Icod.Terminal` package or assembly dependency.

## 10.7 Gate

**RB07 gate:** package-only net8/net9/net10 consumers, focused sample, published Terminal 1.13 interop, tool package smoke, and all six archive RIDs remain green without production coupling.

**Alpha checkpoint:** `1.14.0-Alpha-7`.

---

# 11. RB08 / Alpha-8 — Adversarial hardening, exact freeze, documentation, and release closure

## 11.1 Objective

Freeze 1.14 only after adversarial proof that backend selection is deterministic, bounded, non-heuristic, and backward-compatible.

## 11.2 Required adversarial cases

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

## 11.3 Public API freeze

Generate the exact complete Inspection reflection manifest from a validated package artifact and freeze its normalized-LF SHA-256.

The freeze SHALL prove the 1.14 surface is additive to the accepted 1.13 surface.

## 11.4 JSON freeze

Freeze normalized-LF fingerprints for JSON schemas v1-v6 and verify historical versions exactly.

## 11.5 Documentation

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

## 11.6 Release qualification

The exact Alpha-8 head SHALL pass the complete 12-job Staging-equivalent matrix:

- Windows Build/Test + compatibility reconstruction;
- Linux Build/Test + package verification;
- macOS Build/Test;
- three installed-tool package smokes; and
- six archive RID smokes.

Stable `1.14.0` promotion SHALL then be version/status-only unless an explicitly reviewed release defect requires correction.

## 11.7 Gate

**RB08 gate:** exact API/schema fingerprints are frozen, all adversarial and distribution gates are green, release-facing documentation is internally consistent, and stable promotion can occur without semantic changes.

**Alpha checkpoint:** `1.14.0-Alpha-8` intended final prerelease.

---

# 12. Release-wide invariants

Every 1.14 tranche SHALL preserve:

1. `Icod.TermInfo` runtime remains dependency-free.
2. `Icod.TermInfo.Inspection` has no production dependency on `Icod.Terminal`.
3. Runtime/Source/Compiler/Termcap public APIs remain frozen.
4. Inspection 1.13 contracts remain source/binary compatible.
5. 1.13 runtime observations remain backend-neutral.
6. 1.11 lifecycle semantics remain authoritative.
7. 1.12 placement semantics remain authoritative.
8. JSON v1-v5 remain stable.
9. Backend availability does not imply lifecycle/placement support.
10. Static absence is not negative evidence.
11. Terminal names and brands are not evidence.
12. Backend enum ordering is not preference policy.
13. No preference means no hidden selection among multiple viable candidates.
14. Preference order, when supplied, is explicit and semantic.
15. Selection remains advisory; execution stays downstream.
16. All caller collections are bounded and snapshotted.
17. Deterministic output is culture-independent.
18. New package/sample qualification uses published dependencies rather than sibling source coupling.

---

# 13. Explicit exclusions from 1.14

The following are not part of this release:

- iTerm image protocol support;
- Sixel encoder/transmitter implementation;
- Kitty encoder/transmitter implementation;
- graphics payload abstraction;
- alpha-channel semantic planning;
- pixel-format negotiation;
- compression negotiation;
- image dimensions or scaling policy;
- performance/latency/bandwidth scoring;
- automatic quality ranking;
- terminal-brand backend preference;
- session endpoint health;
- resource/placement lifetime ownership;
- scene/layout policy;
- JSON deserialization/import;
- Berkeley DB/hashed terminfo acquisition; or
- historical vendor binary formats.

Those remain candidates for later releases when independently justified.

---

# 14. Acceptance definition

`Icod.TermInfo 1.14.0` is complete when a consumer can:

1. represent Sixel/Kitty backend availability evidence without using terminal-name heuristics;
2. combine each backend with independently established lifecycle/placement profiles;
3. reuse 1.13 runtime integration per backend without changing 1.13 observations;
4. ask one deterministic planner to evaluate every backend against the same semantic request;
5. provide explicit backend preference when desired;
6. receive `Selected`, `RequiresRuntimeVerification`, `RequiresPreference`, or `Impossible` with complete candidate evidence;
7. serialize backend profiles and selection plans through JSON v6; and
8. consume the feature from ordinary NuGet packages without introducing `Icod.Terminal` into production TermInfo dependencies.

The release SHALL make uncertainty visible rather than guess, make caller policy explicit rather than hidden, and keep all live graphics execution downstream.