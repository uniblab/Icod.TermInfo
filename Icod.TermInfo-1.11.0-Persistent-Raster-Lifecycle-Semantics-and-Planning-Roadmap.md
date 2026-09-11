# Icod.TermInfo 1.11.0 — Persistent Raster Lifecycle Semantics and Planning Roadmap

**Project:** `Icod.TermInfo`  
**Release:** `1.11.0`  
**Development branch:** `1.11.0`  
**Theme:** Persistent Raster Lifecycle Semantics and Deterministic Planning  
**Primary package:** `Icod.TermInfo.Inspection`  
**Baseline:** stable `1.10.0`  
**Downstream integration target:** `Icod.Terminal 1.11.x`  
**Frozen lower layers:** existing 1.x parsing, compiled-database, source, comparison, synthesis, catalog, and 1.10 database-set contracts except for unavoidable defect corrections

---

## 1. Release objective

`Icod.TermInfo 1.11.0` SHALL introduce a protocol-neutral semantic model for describing and planning the lifecycle of terminal-resident raster resources.

The release SHALL make the following questions answerable through reusable managed APIs and deterministic automation:

- does the available evidence establish only ephemeral raster display, or persistent terminal-resident raster resources;
- can raster data be uploaded independently of display;
- can an uploaded resource have one or more placements;
- can an existing placement be replaced or updated;
- can an individual placement be removed;
- can terminal-resident resource data be removed independently;
- is acknowledgement available or required for a lifecycle operation;
- which conclusions are declared, inferred, externally supplied, verified, unknown, or contradicted;
- which lifecycle operation sequence is admissible for a requested task;
- why did the planner select, reject, or weaken a particular operation;
- what assumptions must a runtime library verify before execution.

The release SHALL NOT create or own live terminal raster resources. It describes and plans lifecycle semantics. `Icod.Terminal` remains responsible for live protocol verification, execution, identity, generation invalidation, and cleanup.

```text
terminfo / explicit semantic evidence
        |
        v
persistent-raster lifecycle profile
        |
        v
requested lifecycle intent
        |
        v
deterministic lifecycle plan
        |
        v
runtime verification + execution
        |
        v
Icod.Terminal resource / placement ownership
```

The library boundary is:

```text
Icod.TermInfo
    describes
    classifies
    preserves evidence
    expresses uncertainty
    plans

Icod.Terminal
    verifies live terminal behavior
    transmits protocol operations
    owns resource identity
    owns placement identity
    handles lifecycle-generation invalidation
    performs cleanup

Icod.DCurses
    owns layout
    owns virtual-screen policy
    owns higher-level scene/application state
```

---

## 2. Non-negotiable architectural constraints

### 2.1 TermInfo does not become a terminal-session library

No public 1.11 API SHALL allocate terminal image identifiers, allocate placement identifiers, transmit Kitty APC, transmit Sixel, wait for terminal acknowledgements, own a terminal input reader, dispose terminal resources, retain live terminal ownership state, or depend on `Icod.Terminal`.

### 2.2 Persistent and ephemeral raster support are distinct

The model SHALL distinguish at least:

```text
no known raster support

ephemeral raster display

persistent raster resource support
    upload resource
    create placement
    update/replace placement
    delete placement
    delete resource
```

Support for ordinary raster output SHALL NOT imply persistent-resource support. Sixel may establish ephemeral raster capability without establishing a persistent lifecycle.

### 2.3 Unknown is not unsupported

Static terminfo data cannot prove every modern terminal extension. The foundational support-state vocabulary SHALL therefore distinguish:

```text
Unknown
Supported
Unsupported
Contradicted
```

Absence of a terminfo capability SHALL NOT automatically mean that a live terminal cannot perform an operation.

### 2.4 Evidence is first-class

Every lifecycle conclusion SHALL retain deterministic evidence sufficient to identify the conclusion, its source, its provenance, and whether live verification remains necessary.

Potential evidence includes standard capabilities, extended capabilities, terminal-description metadata, explicit caller-supplied semantic facts, deterministic protocol-family declarations, database/catalog provenance, and contradictions between evidence sources.

Terminal-brand heuristics SHALL NOT silently become capability truth.

### 2.5 Planning is advisory, not execution authority

A TermInfo lifecycle plan means that, given the available evidence and requested semantic outcome, an ordered set of lifecycle operations appears admissible. It SHALL NOT mean that those operations are guaranteed to succeed against the live terminal.

Plans MAY require runtime verification explicitly.

### 2.6 Existing 1.x contracts remain frozen

1.11 SHALL build on `TerminalDescription`, the standard and extended capability models, semantic comparison, catalog inspection, database-set inspection and precedence, existing deterministic planning foundations, and existing versioned JSON automation.

Existing serialized document contracts SHALL NOT change meaning. Additive document kinds or a new explicitly versioned schema SHALL be used where necessary.

### 2.7 Determinism and bounds remain public policy

Culture, filesystem enumeration, hash ordering, thread scheduling, and platform-specific ambient state SHALL NOT affect semantic ordering or serialized output.

Evidence counts, plan steps, database composition, and serialized output SHALL remain explicitly bounded.

---

## 3. Foundational semantic vocabulary

RL01 SHALL freeze public concepts equivalent in responsibility to:

```text
PersistentRasterLifecycleOperation
PersistentRasterLifecycleSupportStatus
PersistentRasterLifecycleEvidence
PersistentRasterLifecycleEvidenceKind
PersistentRasterLifecycleProfile
PersistentRasterLifecycleRequest
PersistentRasterLifecyclePlan
PersistentRasterLifecyclePlanStep
PersistentRasterLifecyclePlanIssue
PersistentRasterLifecyclePlanner
PersistentRasterLifecycleOptions
```

The initial semantic operation vocabulary SHALL be:

```text
DisplayEphemeral
UploadResource
CreatePlacement
UpdatePlacement
DeletePlacement
DeleteResource
```

Public TermInfo APIs SHALL NOT encode Kitty image IDs, image numbers, placement IDs, raw APC fields, or other protocol-specific numeric ownership identifiers.

---

## 4. Profile classification

A lifecycle profile SHOULD independently classify:

```text
RasterDisplay
PersistentUpload
AcknowledgedUpload
PlacementCreation
MultiplePlacements
PlacementUpdate
PlacementDeletion
ResourceDeletion
```

Classification SHALL preserve unknown and contradictory evidence rather than forcing a Boolean answer.

---

## 5. Planning model

A caller SHOULD be able to request semantic outcomes equivalent to:

```text
display once
upload without display
upload and place once
one resource with several placements
create then update a placement
targeted placement cleanup
deterministic resource cleanup
```

The planner SHALL return either an ordered semantic plan or a structured inability/uncertainty result. Ordinary unsupported or unknown combinations SHALL NOT require exception-driven control flow.

An ephemeral-only terminal SHALL not satisfy a persistent-resource plan merely because it can display raster pixels once.

---

## 6. Evidence precedence and contradictions

The default evidence ordering SHOULD prefer:

```text
explicit verified semantic evidence
        >
explicit declared semantic evidence
        >
recognized capability-derived evidence
        >
absence / unknown
```

Contradictory positive and negative evidence SHALL be preserved. The classifier SHALL not silently discard contradictory evidence to produce a convenient answer.

---

## 7. Database-set integration

The 1.10 ordered database-set model remains authoritative for effective terminal-definition precedence.

Lifecycle inspection over a database set SHALL operate on the effective description selected by existing precedence rules and retain enough provenance to report the source database, shadowed definitions, differing lifecycle evidence, incomplete earlier databases, and indeterminate effective results.

A later database SHALL NOT silently establish lifecycle certainty when incomplete earlier evidence prevents a definitive effective-definition conclusion.

---

## 8. Machine-readable automation

Persistent-raster lifecycle information SHALL be available without parsing human-oriented text.

Automation SHOULD eventually support additive document kinds equivalent to:

```text
persistentRasterLifecycleProfile
persistentRasterLifecyclePlan
```

Documents SHALL include explicit schema/document versioning, semantic states, deterministic evidence, plan steps, runtime-verification requirements, issues, and database provenance where applicable.

Existing 1.9 version-1 and 1.10 version-2 documents SHALL remain compatible for equivalent existing requests.

---

## 9. Explicit exclusions

Version 1.11 SHALL NOT include:

- terminal-side image IDs;
- Kitty image numbers or placement IDs;
- raw Kitty APC command dictionaries;
- raw Sixel encoding;
- raster codecs or pixel buffers;
- image caching;
- terminal resource/placement registries;
- hidden replay after reset;
- terminal lifecycle-generation ownership;
- terminal cleanup execution;
- cursor positioning or placement coordinates;
- virtual-screen integration;
- scene graphs;
- animation;
- Unicode placeholder rendering;
- z-order policy;
- live terminal probing owned by TermInfo;
- terminal-brand guessing.

---

## 10. Versioning and tranche plan

Development SHALL proceed through eight tranches:

```text
RL01 -> 1.11.0-Alpha-1
RL02 -> 1.11.0-Alpha-2
RL03 -> 1.11.0-Alpha-3
RL04 -> 1.11.0-Alpha-4
RL05 -> 1.11.0-Alpha-5
RL06 -> 1.11.0-Alpha-6
RL07 -> 1.11.0-Alpha-7
RL08 -> 1.11.0-Alpha-8
```

Each completed tranche SHALL update the coordinated suite version before the tranche is considered complete. Stable `1.11.0` SHALL promote the validated Alpha-8 surface without adding new semantics.

### RL01 — Semantic contract and public API regret gate

Freeze the lifecycle operation vocabulary, support-state model, evidence taxonomy, persistent-versus-ephemeral distinction, caller-supplied evidence boundary, contradiction semantics, package placement, and initial public API shape.

**Gate:** both TermInfo and Terminal can independently implement against the semantic vocabulary without sharing private protocol state.

### RL02 — Lifecycle evidence model

Introduce immutable deterministic evidence with provenance, positive/negative facts, explicit caller evidence, stable ordering, snapshotting, bounds, and culture independence.

**Gate:** callers can construct and inspect deterministic persistent-raster evidence without receiving a forced final judgment.

### RL03 — Lifecycle profile classification

Classify evidence into an immutable protocol-neutral profile for ephemeral display, persistent upload, acknowledgement, placement creation/multiplicity/update/deletion, and resource deletion.

**Gate:** callers can ask what lifecycle semantics are known without requesting a plan.

### RL04 — Deterministic lifecycle planner

Plan bounded ordered lifecycle steps from a semantic request and profile. Distinguish impossible from unknown/indeterminate outcomes and identify required runtime verification.

**Gate:** downstream code can make policy decisions from a reusable plan without knowing Kitty or Sixel wire syntax.

### RL05 — Terminal-description and database-set composition

Derive recognized evidence from `TerminalDescription`, compose with effective database-set precedence, and retain catalog/root incompleteness and provenance.

**Gate:** lifecycle semantics participate naturally in the 1.10 inspection model without changing 1.10 precedence rules.

### RL06 — Machine-readable profile and plan automation

Add deterministic versioned lifecycle-profile and lifecycle-plan automation with bounded output while preserving existing JSON documents unchanged.

**Gate:** CI and external tooling can consume lifecycle information without parsing human-readable output.

### RL07 — Downstream integration and loose-coupling qualification

Prove package-only consumption and semantic interoperability with `Icod.Terminal` without project/package dependency inversion. Demonstrate that consumer-owned live verification can strengthen otherwise unknown static knowledge.

**Gate:** TermInfo and Terminal agree semantically while remaining independently versionable.

### RL08 — Hardening, documentation, compatibility, and release closure

Complete public API baselines, XML docs, README/guide/sample work, hostile-input and bounds coverage, deterministic repetition/culture tests, multi-target package validation, package-only consumer qualification, changelog/release notes, and stable promotion.

**Gate:** the feature is documented, deterministic, bounded, package-qualified, downstream-qualified, and ready for stable promotion.

---

## 11. Cross-library acceptance contract

The principal integration shape is:

```text
Icod.TermInfo

    inspect terminal information
        |
        v
    PersistentRasterLifecycleProfile
        |
        v
    PersistentRasterLifecyclePlanner
        |
        v
    PersistentRasterLifecyclePlan

Icod.Terminal

    consume semantic requirements
        |
        v
    perform required live verification
        |
        v
    create terminal-owned raster resource
        |
        v
    create/update/dispose terminal-owned placement(s)
        |
        v
    dispose terminal-owned resource
```

TermInfo never needs a terminal image or placement identifier. Terminal never needs TermInfo's private evidence implementation.

---

## 12. Stable-release acceptance criteria

`Icod.TermInfo 1.11.0` is ready for stable release when:

1. persistent raster lifecycle has a stable protocol-neutral semantic vocabulary;
2. ephemeral and persistent raster support are not conflated;
3. unknown capability is not treated as unsupported;
4. evidence/provenance are immutable, bounded, and deterministic;
5. contradictions remain visible;
6. profiles can be generated from existing terminal descriptions;
7. plans can be generated without runtime side effects;
8. plans explicitly identify live-verification requirements;
9. database-set precedence and incompleteness remain intact;
10. machine-readable automation is deterministic and versioned;
11. existing 1.x JSON/API contracts remain compatible;
12. `Icod.TermInfo` has no dependency on `Icod.Terminal`;
13. a package-only downstream consumer proves loose coupling;
14. the model maps cleanly to `Icod.Terminal 1.11` resource/placement ownership;
15. documentation identifies which library owns inspection, planning, execution, identity, lifetime, and layout.

---

## 13. Deferred work

Potential later work includes advanced placement semantics, source rectangles, z-order, Unicode placeholders, animation, richer geometry, protocol-preference policy, multi-protocol negotiation, and machine-readable live-evidence interchange.

These SHALL NOT be added to 1.11 merely because one graphics protocol can express them. The purpose of 1.11 is a small durable lifecycle foundation.
