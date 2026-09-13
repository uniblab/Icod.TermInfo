# Icod.TermInfo 1.13.0 — Persistent-Raster Runtime Evidence Interchange and Integration Roadmap

**Project:** `Icod.TermInfo`  
**Release:** `1.13.0`  
**Development branch:** `1.13.0`  
**Theme:** Persistent-Raster Runtime Evidence Interchange and Deterministic Integration  
**Primary package:** `Icod.TermInfo.Inspection`  
**Baseline:** stable `1.12.0`  
**Downstream qualification target:** `Icod.Terminal` semantic capability inspection and verification  
**Frozen contracts:** existing 1.x Runtime/Source/Compiler/Termcap APIs, Inspection contracts through 1.12, JSON schemas v1-v4, database-set precedence, persistent-raster lifecycle semantics, and advanced persistent-raster placement semantics except for unavoidable defect corrections  
**Status:** Planning approved; implementation not yet started  
**Tranche prefix:** `RE`  
**Release audit:** `docs/1.13.0-RELEASE-AUDIT.md` when created

---

## 1. Release objective

`Icod.TermInfo 1.13.0` SHALL provide a protocol-neutral, bounded, deterministic way for caller-owned runtime verification results to cross into the existing persistent-raster evidence, classification, and planning system.

The current architecture is correct but mechanically awkward for consumers:

```text
TermInfo static evidence
    -> classify
    -> plan
    -> runtime verification required
    -> consumer performs live verification
    -> consumer manually creates TermInfo evidence
    -> consumer calculates source ordinals
    -> consumer merges evidence
    -> reclassify
    -> replan
```

Version 1.13 SHALL make that handoff explicit:

```text
TermInfo static evidence
    -> classify / plan
    -> runtime verification required
    -> external verifier
    -> protocol-neutral runtime observations
    -> deterministic evidence integration
    -> existing 1.11 / 1.12 classifiers
    -> existing planners
```

TermInfo SHALL still perform no terminal I/O.

The purpose of 1.13 is not to invent another planning model. It is to make verified runtime facts portable, inspectable, replayable, bounded, and safely integrable with the frozen lifecycle and placement models.

---

## 2. Architectural boundary

### 2.1 Preserve the existing planners

The following remain authoritative and frozen except for unavoidable defect correction:

- 1.11 persistent-raster lifecycle evidence, profile, request, classification, and planner semantics;
- 1.12 persistent-raster placement evidence, profile, request, classification, and planner semantics.

1.13 SHALL NOT replace, reinterpret, widen, or silently bypass those models.

Runtime integration ultimately SHALL produce ordinary existing:

```text
PersistentRasterLifecycleEvidence
PersistentRasterPlacementEvidence
```

which are then consumed by the frozen classifiers and planners.

### 2.2 TermInfo does not own verification

TermInfo SHALL NOT:

- open a terminal session;
- send probe traffic;
- select a protocol backend;
- inspect PTY/TTY handles;
- determine current endpoint availability;
- retain terminal generation/session identity;
- decide whether a live capability is presently usable;
- execute persistent-raster operations.

Those responsibilities remain with consumers such as `Icod.Terminal`.

### 2.3 Runtime observation is not static evidence

A new runtime-observation model SHALL describe facts supplied by an external verifier.

It SHALL distinguish at least:

```text
Supported
Unsupported
Inconclusive
```

`Supported` and `Unsupported` may become existing `Verified` evidence.

`Inconclusive` SHALL remain visible in integration/audit results but SHALL NOT be silently converted into either positive or negative evidence.

### 2.4 No backend identities

The interchange model SHALL remain semantic.

It SHALL NOT expose public protocol/backend values such as:

```text
Kitty
Sixel
iTerm
OSC
CSI
protocol backend ID
terminal brand
```

A runtime verifier may internally use any protocol necessary to establish a semantic result.

### 2.5 Deterministic provenance

Runtime observations SHALL retain bounded provenance sufficient for deterministic auditing:

- stable caller-supplied source label;
- deterministic source-local ordinal;
- semantic subject;
- outcome.

They SHALL NOT require timestamps, host names, process IDs, session IDs, generation IDs, or other nondeterministic environmental metadata.

---

## 3. Proposed public vocabulary

RE01 SHALL freeze public concepts equivalent in responsibility to:

```text
PersistentRasterRuntimeObservationOutcome

PersistentRasterLifecycleRuntimeObservation
PersistentRasterPlacementRuntimeObservation

PersistentRasterRuntimeObservationSet

PersistentRasterRuntimeIntegrationIssueKind
PersistentRasterRuntimeIntegrationIssue
PersistentRasterRuntimeIntegrationResult

PersistentRasterRuntimeEvidenceIntegrator
```

Exact names may be adjusted before RE01 freezes the public surface, but responsibilities SHALL remain bounded to this architecture.

### 3.1 Observation outcome

The initial runtime-observation outcome vocabulary SHALL contain exactly:

```text
Supported
Unsupported
Inconclusive
```

No `Advertised` state belongs in runtime interchange. Advertised/static evidence already has an existing representation.

### 3.2 Subject reuse

Lifecycle runtime observations SHALL target the existing:

```text
PersistentRasterLifecycleEvidenceSubject
```

Placement runtime observations SHALL target the existing:

```text
PersistentRasterPlacementSubject
```

1.13 SHALL NOT introduce a third duplicate unified persistent-raster subject enumeration merely for interchange.

---

## 4. Observation-set semantics

A `PersistentRasterRuntimeObservationSet` SHALL be:

- immutable;
- bounded;
- snapshot-based;
- culture-independent;
- deterministic;
- explicit about lifecycle versus placement observations.

The set SHALL permit:

- positive observations;
- negative observations;
- inconclusive observations;
- multiple observations for one semantic subject;
- contradictory observations supplied by the caller.

Contradiction is evidence, not an exception.

Invalid enum values, negative source-local ordinals, null elements, excessive counts, malformed source labels, and other programming errors SHALL fail through ordinary argument validation.

---

## 5. Deterministic evidence integration

The integration layer SHALL solve the mechanical work consumers currently perform themselves.

Given:

```text
existing lifecycle profile/evidence
existing placement profile/evidence
runtime observation set
```

the integrator SHALL:

1. validate and snapshot the incoming observations;
2. preserve the complete existing evidence snapshots;
3. retain inconclusive observations without strengthening support;
4. map supported observations to positive existing `Verified` evidence;
5. map unsupported observations to negative existing `Verified` evidence;
6. append mapped evidence deterministically;
7. assign safe final source ordinals;
8. enforce resulting combined-evidence bounds;
9. detect ordinal-space exhaustion rather than overflowing;
10. re-run the frozen lifecycle and placement classifiers;
11. expose strengthened resulting profiles;
12. expose structured integration issues and audit evidence.

The integrator SHALL NOT hide contradictions.

For example:

```text
static CapabilityDerived support
    +
runtime Unsupported observation
    ->
Verified negative evidence
    ->
Unsupported or Contradicted according to the frozen classifier rules
```

1.13 SHALL not invent a second support-precedence system.

---

## 6. Source ordinal handling

Consumers SHALL no longer need application-specific code equivalent to:

```text
max(existing.SourceOrdinal) + 1
```

Runtime observations SHALL carry **source-local ordering**, not assumed final classifier ordinals.

The integration layer SHALL assign safe final ordinals after the existing evidence snapshot while preserving the deterministic relative ordering of imported observations.

If a safe append cannot be represented, integration SHALL fail deterministically through a structured integration outcome/issue rather than integer overflow, wrapping, arbitrary renumbering, or evidence loss.

---

## 7. Replanning boundary

1.13 SHALL deliberately reuse the existing planners rather than introduce a new aggregate support/status model.

The intended flow is:

```text
integrationResult =
    integrate(existingProfiles, runtimeObservations)

lifecyclePlan =
    PersistentRasterLifecyclePlanner.Plan(
        integrationResult.LifecycleProfile,
        lifecycleRequest
    )

placementPlan =
    PersistentRasterPlacementPlanner.Plan(
        lifecyclePlan,
        integrationResult.PlacementProfile,
        placementRequest
    )
```

Convenience APIs MAY perform this orchestration if they delegate to the frozen planners and do not reinterpret their outcomes.

A new third persistent-raster planner or a new combined plan-status enumeration is explicitly disfavored.

---

## 8. Machine-readable automation — JSON version 5

JSON versions 1 through 4 SHALL remain immutable historical contracts.

Version 1.13 SHALL add JSON schema **version 5**.

The preferred version-5 document kinds are exactly:

```text
persistentRasterRuntimeObservationSet
persistentRasterRuntimeIntegration
```

### 8.1 Observation document

The observation document SHALL expose deterministic bounded representations of:

- source label;
- lifecycle observations;
- placement observations;
- semantic subjects;
- observation outcomes;
- source-local ordinals.

### 8.2 Integration document

The integration document SHALL expose deterministic bounded representations of:

- imported observations;
- mapped verified lifecycle evidence;
- mapped verified placement evidence;
- inconclusive observations;
- structured integration issues;
- resulting lifecycle support states;
- resulting placement support states.

It SHALL NOT expose:

- protocol/backend identifiers;
- terminal resource IDs;
- terminal placement IDs;
- timestamps;
- current terminal endpoint availability;
- current session usability;
- raw probe responses.

`TermInfoJsonRenderer` SHALL remain deterministic, bounded, cancelable, and culture-independent.

### 8.3 JSON input is deferred

1.13 SHALL NOT introduce a general-purpose JSON deserializer for historical Inspection document kinds.

The CLR runtime-observation model SHALL be the authoritative ingestion surface.

Version-5 JSON is a deterministic interchange/audit representation. A future release may add carefully bounded parsing only if a concrete cross-process consumer justifies it.

---

## 9. Terminal interoperability

A package-only qualification consumer SHALL demonstrate loose coupling with `Icod.Terminal`.

The intended downstream mapping is:

```text
Terminal-owned live verification
        |
        v
semantic live result
        |
        v
consumer adapter
        |
        v
PersistentRasterRuntimeObservationSet
        |
        v
TermInfo integration
        |
        v
existing lifecycle / placement classifiers
        |
        v
existing planners
```

The adapter SHALL remain outside production `Icod.TermInfo`.

No production TermInfo API SHALL reference:

```text
TerminalCapabilityStatus
TerminalSession
Icod.Terminal
```

The downstream qualification sample SHOULD replace the hand-written evidence-factory and ordinal-merging pattern currently required by consumers.

---

## 10. Bounds

1.13 SHALL reuse existing Inspection-wide defensive bounds wherever they serve the same denial-of-service purpose.

The runtime-observation model SHALL explicitly bound:

- observation count;
- source-label size;
- resulting combined evidence count;
- JSON UTF-8 output size.

The evidence count MUST remain compatible with the existing lifecycle and placement classifier limits.

No unbounded provenance strings, arbitrary nested metadata dictionaries, opaque extension bags, or unbounded caller payloads SHALL be introduced.

---

## 11. Error model

Programming errors SHALL throw ordinary argument exceptions:

- null required arguments;
- invalid enum values;
- negative source-local ordinal;
- malformed source labels;
- observation count above the configured maximum.

Normal semantic/integration outcomes SHALL be represented, not thrown:

- inconclusive verification;
- verified support;
- verified non-support;
- contradiction;
- evidence which cannot strengthen the current conclusion;
- ordinal-space exhaustion;
- resulting combined-evidence limit exhaustion.

Cancellation remains ordinary `OperationCanceledException` for applicable rendering/API operations accepting cancellation tokens.

---

## 12. Tranche plan

Development SHALL proceed through eight tranches:

```text
RE01 -> 1.13.0-Alpha-1
RE02 -> 1.13.0-Alpha-2
RE03 -> 1.13.0-Alpha-3
RE04 -> 1.13.0-Alpha-4
RE05 -> 1.13.0-Alpha-5
RE06 -> 1.13.0-Alpha-6
RE07 -> 1.13.0-Alpha-7
RE08 -> 1.13.0-Alpha-8
```

Each completed tranche SHALL update the coordinated suite version before the tranche is considered complete. Stable `1.13.0` SHALL promote the validated Alpha-8 surface without adding new semantics.

### RE01 — Architecture, vocabulary, and public API regret gate

Freeze:

- runtime-observation versus static-evidence distinction;
- subject reuse;
- observation outcome vocabulary;
- deterministic provenance;
- bounds;
- integration responsibilities;
- JSON v5 decision;
- Terminal ownership boundary;
- explicit exclusions.

**Gate:** the public model can carry external verified facts without introducing Terminal/protocol dependencies or mutating frozen 1.11/1.12 evidence models.

### RE02 — Immutable runtime observations

Implement immutable lifecycle and placement runtime-observation values plus bounded observation sets.

Cover:

- Supported;
- Unsupported;
- Inconclusive;
- deterministic source-local ordering;
- snapshotting;
- validation;
- equality expectations;
- culture independence.

**Gate:** a consumer can capture a complete bounded runtime-verification result without constructing classifier evidence itself.

### RE03 — Deterministic evidence integration

Implement conversion of conclusive runtime observations into existing `Verified` lifecycle/placement evidence.

Add:

- safe ordinal assignment;
- complete existing-evidence preservation;
- combined-evidence limits;
- deterministic append ordering;
- overflow handling;
- inconclusive retention.

**Gate:** consumers no longer need custom evidence factories or `GetNextSourceOrdinal` logic.

### RE04 — Classification and contradiction integration

Apply integrated evidence through the frozen lifecycle and placement classifiers.

Exercise:

- static-positive + verified-negative;
- static-negative + verified-positive;
- verified-positive + verified-negative;
- repeated compatible runtime observations;
- inconclusive-only observation sets;
- mixed lifecycle/placement observations.

**Gate:** runtime evidence strengthens or contradicts knowledge exclusively according to the frozen classifier precedence rules.

### RE05 — Replanning composition and audit result

Complete the orchestration boundary above the integration result.

Provide ergonomic APIs for:

```text
integrate
classify
replan
inspect resulting issues
```

without introducing a replacement planner or new aggregate plan status.

Preserve both resulting profiles and the runtime observation/integration audit trail.

**Gate:** the normal static-plan → runtime-verification → replan workflow can be expressed without application-specific evidence-merging boilerplate.

### RE06 — JSON v5 runtime-evidence automation

Add deterministic rendering and schema for exactly:

```text
persistentRasterRuntimeObservationSet
persistentRasterRuntimeIntegration
```

Freeze v1-v4 fingerprints unchanged.

Add normalized fixtures, UTF-8 bounds, cancellation, culture, repetition, and schema validation.

**Gate:** CI and tooling can persist and inspect the runtime-evidence handoff without parsing prose.

### RE07 — Terminal interoperability and sample qualification

Add a package-only consumer against stable `Icod.Terminal`.

Demonstrate:

- Terminal-owned verification;
- caller-owned adapter;
- TermInfo runtime observation set;
- deterministic integration;
- reclassification;
- replanning;
- no production TermInfo → Terminal dependency.

Update persistent-raster samples to remove hand-written evidence-merging and ordinal-management where the 1.13 API supersedes it.

Execute package/sample consumers on:

```text
net8.0
net9.0
net10.0
```

**Gate:** a real downstream consumer can perform the complete handoff using stable packages rather than repository-internal assumptions.

### RE08 — Hardening, exact freeze, documentation, and release closure

Complete:

- adversarial bounds;
- duplicate and contradictory observation stress;
- ordinal boundary tests;
- deterministic cross-culture/cross-process tests;
- exact Inspection public API fingerprint;
- exact v1-v5 schema fingerprints;
- package-only consumers;
- samples;
- README/versioning/compatibility updates;
- release audit;
- Windows PowerShell verification;
- Windows/Linux/macOS Build/Test;
- installed-tool smoke;
- six archive RIDs.

**Gate:** Alpha-8 is releasable without semantic changes during stable promotion.

---

## 13. Stable-release acceptance criteria

`Icod.TermInfo 1.13.0` is ready for stable release when:

1. callers can represent supported, unsupported, and inconclusive runtime observations;
2. lifecycle and placement subjects reuse the frozen 1.11/1.12 semantic enumerations;
3. no protocol/backend enumeration enters the TermInfo public API;
4. no TermInfo production project references `Icod.Terminal`;
5. conclusive observations map deterministically to existing `Verified` evidence;
6. inconclusive observations remain visible without becoming support or non-support;
7. consumers no longer calculate final evidence ordinals manually;
8. existing evidence is not rewritten or discarded;
9. contradictions remain visible;
10. frozen classifiers remain authoritative;
11. frozen planners remain authoritative;
12. JSON v1-v4 are unchanged;
13. JSON v5 contains exactly the runtime observation and integration documents;
14. observation and resulting evidence counts are bounded;
15. package-only interoperability with Terminal is proven;
16. multi-TFM consumers and samples pass;
17. Windows PowerShell 5.1 release verification remains green;
18. Windows/Linux/macOS package validation and all six archive RIDs pass.

---

## 14. Explicit exclusions

1.13 SHALL NOT include:

- live terminal probing implemented by TermInfo;
- Terminal session or endpoint types;
- public Kitty/Sixel/iTerm identifiers;
- backend selection;
- backend preference ordering;
- multi-protocol negotiation;
- terminal-brand heuristics;
- relative placement graphs;
- animation/frame lifecycle;
- Unicode placeholders;
- scene graphs;
- image codecs;
- raster pixel transport;
- terminal resource/placement identities;
- arbitrary timestamps/host provenance;
- generic JSON deserialization;
- changes to database-set precedence;
- changes to 1.11 lifecycle semantics;
- changes to 1.12 placement semantics.

---

## 15. Post-1.13 handoff

If 1.13 succeeds, it creates the foundation for the previously discussed multi-protocol preference/negotiation track.

A later release—likely 1.14 or beyond—can then reason over:

```text
static semantic knowledge
+
runtime evidence
+
multiple genuinely viable execution backends
+
caller preference policy
```

without conflating evidence gathering, backend discovery, and preference policy.

That later track SHOULD begin only when at least two meaningful implementations exist for the same semantic operation and choosing between them has real behavioral value.

Until then, backend ranking remains intentionally out of TermInfo.
