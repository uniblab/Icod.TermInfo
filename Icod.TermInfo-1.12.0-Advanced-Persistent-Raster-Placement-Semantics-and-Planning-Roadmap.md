# Icod.TermInfo 1.12.0 — Advanced Persistent-Raster Placement Semantics and Planning Roadmap

**Project:** `Icod.TermInfo`  
**Release:** `1.12.0`  
**Development branch:** `1.12.0`  
**Theme:** Advanced Persistent-Raster Placement Semantics and Deterministic Planning  
**Primary package:** `Icod.TermInfo.Inspection`  
**Baseline:** stable `1.11.0`  
**Downstream integration target:** `Icod.Terminal 1.12.0`  
**Frozen contracts:** existing 1.x Runtime/Source/Compiler/Termcap APIs, Inspection contracts through 1.11, JSON schemas v1-v3, database-set precedence, and persistent-raster lifecycle semantics except for unavoidable defect corrections  
**Status:** Completed; stable `1.12.0` promotion validated  
**Alpha-8 witness:** workflow #725 / `34763187115`  
**Stable promotion witness:** workflow #731 / `34764253699`  
**Release audit:** `docs/1.12.0-RELEASE-AUDIT.md`

---

## 1. Release objective

`Icod.TermInfo 1.12.0` SHALL add a protocol-neutral semantic layer for advanced persistent-raster placement requirements that build on, but do not mutate, the frozen 1.11 lifecycle model.

The release SHALL answer reusable semantic questions equivalent to:

- is pixel-space source-rectangle placement known to be supported;
- is signed z-order / stacking order known to be supported;
- what evidence establishes, denies, leaves unknown, or contradicts those placement features;
- can a requested persistent-raster lifecycle plan also satisfy source cropping and/or signed stacking requirements;
- which placement requirements require live runtime verification;
- how do placement conclusions compose with an effective `TerminalDescription` or ordered database set;
- how can CI and external automation consume placement profile/plan results without parsing human-readable output.

The release SHALL NOT carry actual rectangle coordinates or z-order values as TermInfo planning state. Those values remain execution inputs owned by application/Terminal code. TermInfo describes whether the semantics are available and whether verification is required.

```text
terminfo / explicit semantic evidence
        |
        v
persistent-raster lifecycle profile (1.11, frozen)
        +
advanced placement profile (1.12)
        |
        v
lifecycle intent + placement requirements
        |
        v
deterministic semantic plan
        |
        v
consumer-owned live verification + execution
        |
        v
Icod.Terminal placement options
```

---

## 2. Architectural boundary

### 2.1 Preserve the 1.11 lifecycle contract

The 1.11 public types, lifecycle evidence-subject enumeration, lifecycle profile shape, request shape, planner semantics, and JSON v3 document meanings are frozen.

1.12 SHALL NOT add new values to `PersistentRasterLifecycleEvidenceSubject`, SHALL NOT reinterpret existing 1.11 profile properties, and SHALL NOT extend JSON v3 in place.

Advanced placement semantics SHALL be represented by a parallel additive family in `Icod.TermInfo.Inspection`.

### 2.2 TermInfo describes semantics; Terminal executes them

`Icod.TermInfo` SHALL classify evidence and produce deterministic semantic plans.

`Icod.Terminal` SHALL continue to own:

- actual source-rectangle coordinates;
- actual signed z-order values;
- terminal-side resource and placement identities;
- wire encoding;
- live verification;
- execution;
- generation invalidation;
- cleanup.

No 1.12 TermInfo API SHALL depend on `Icod.Terminal`.

### 2.3 Unknown is not unsupported

The placement model SHALL reuse the 1.11 four-state support semantics:

```text
Unknown
Supported
Unsupported
Contradicted
```

Absence of static evidence SHALL NOT be interpreted as terminal non-support.

### 2.4 Evidence is immutable and deterministic

Placement evidence SHALL preserve source, provenance, polarity, ordering, and contradiction information using the same precedence philosophy established in 1.11.

Evidence and serialized output SHALL be bounded and culture-independent.

---

## 3. Initial semantic vocabulary

PG01 SHALL freeze public concepts equivalent in responsibility to:

```text
PersistentRasterPlacementFeature
PersistentRasterPlacementEvidence
PersistentRasterPlacementEvidenceKind
PersistentRasterPlacementProfile
PersistentRasterPlacementRequirements
PersistentRasterPlacementPlan
PersistentRasterPlacementPlanIssue
PersistentRasterPlacementPlanner
PersistentRasterPlacementOptions
```

The initial feature vocabulary SHALL contain exactly:

```text
SourceRectangle
SignedZOrder
```

No protocol-family names, Kitty command fields, image identifiers, placement identifiers, raster pixels, coordinates, or z-order values SHALL appear in this semantic feature enumeration.

---

## 4. Placement profile semantics

A `PersistentRasterPlacementProfile` SHALL independently classify:

```text
SourceRectangle
SignedZOrder
```

Each classification SHALL use `PersistentRasterLifecycleSupportStatus` rather than inventing a second four-state support enum.

The profile SHALL retain the complete canonical placement-evidence snapshot, including lower-precedence and contradictory evidence.

Placement support SHALL not imply persistent upload or placement creation support. Consumers requiring persistent placement must combine the placement profile with the frozen 1.11 lifecycle profile/plan.

---

## 5. Placement requirements and planning

A placement request SHALL express semantic requirements only, for example:

```text
require source cropping
require signed stacking order
require both
```

The request SHALL NOT contain rectangle coordinates or a z-order integer.

Planning SHALL consume:

- a frozen 1.11 `PersistentRasterLifecycleProfile` or lifecycle plan where appropriate;
- one 1.12 placement profile;
- one validated placement-requirements value.

Planning SHALL distinguish:

```text
possible from current evidence
possible but runtime verification required
impossible
contradicted / indeterminate
```

The planner SHALL NOT perform terminal I/O.

---

## 6. TerminalDescription and database-set composition

1.12 placement inspection SHALL support the same composition boundaries used by 1.11 lifecycle inspection:

- direct `TerminalDescription` inspection;
- caller-supplied semantic evidence;
- ordered database-set effective-definition precedence;
- provenance of the effective source;
- incomplete earlier roots;
- shadowing and indeterminate effective results.

1.12 SHALL NOT modify 1.10 database-set precedence rules.

---

## 7. Machine-readable automation

JSON schemas v1, v2, and v3 remain immutable historical contracts.

1.12 SHALL introduce a new explicitly versioned schema contract, version 4, with exactly two new document kinds:

```text
persistentRasterPlacementProfile
persistentRasterPlacementPlan
```

Version-4 documents SHALL include deterministic bounded representations of:

- feature support states;
- evidence and provenance;
- requirements;
- plan status;
- runtime-verification requirements;
- structured issues;
- database provenance where applicable.

Existing command forms and v1-v3 outputs SHALL remain byte/semantic compatible for equivalent historical requests.

---

## 8. Cross-library interoperability target

The intended loose-coupling contract with `Icod.Terminal 1.12.0` is:

```text
Icod.TermInfo
    classify SourceRectangle / SignedZOrder support
        |
        v
    plan semantic requirements
        |
        v
    identify live-verification requirements

Icod.Terminal
    verify live endpoint behavior as necessary
        |
        v
    accept concrete TerminalRasterSourceRectangle / ZIndex values
        |
        v
    encode, execute, acknowledge, own, update, and clean up placement state
```

A package-only qualification consumer SHALL prove this semantic agreement without introducing a production dependency from TermInfo to Terminal.

---

## 9. Explicit exclusions

Version 1.12 SHALL NOT include:

- relative-placement graphs;
- parent/child placement relationships;
- Unicode placeholder rendering;
- animation or frame lifecycle;
- absolute screen-coordinate placement;
- scene graphs;
- raster pixel buffers or codecs;
- image decoding/transcoding;
- raw Kitty or Sixel wire fields;
- public protocol/backend identifiers;
- terminal image or placement identifiers;
- protocol preference or negotiation;
- backend ranking;
- terminal-brand heuristics;
- live terminal probing owned by TermInfo;
- machine-readable live-evidence interchange as a generalized subsystem.

These remain later-track candidates.

---

## 10. Tranche plan

Development SHALL proceed through eight tranches:

```text
PG01 -> 1.12.0-Alpha-1
PG02 -> 1.12.0-Alpha-2
PG03 -> 1.12.0-Alpha-3
PG04 -> 1.12.0-Alpha-4
PG05 -> 1.12.0-Alpha-5
PG06 -> 1.12.0-Alpha-6
PG07 -> 1.12.0-Alpha-7
PG08 -> 1.12.0-Alpha-8
```

Each completed tranche SHALL update the coordinated suite version before the tranche is considered complete. Stable `1.12.0` SHALL promote the validated Alpha-8 surface without adding new semantics.

### PG01 — Architecture, vocabulary, and API regret gate

Freeze the feature vocabulary, reuse of the 1.11 support-state type, evidence taxonomy, immutable API shape, lifecycle-composition boundary, JSON versioning decision, bounds, exclusions, and downstream ownership boundary.

**Gate:** the public contract can represent source-rectangle and signed-z-order semantics without mutating frozen 1.11 lifecycle or JSON-v3 contracts and without importing Terminal execution state.

### PG02 — Immutable placement evidence

Implement bounded immutable evidence values with provenance, positive/negative facts, explicit caller-supplied facts, canonical ordering, snapshotting, equality expectations, invalid-enum rejection, and culture independence.

**Gate:** callers can construct deterministic placement evidence without receiving a forced final support judgment.

### PG03 — Placement profile classification

Classify placement evidence into an immutable profile for `SourceRectangle` and `SignedZOrder`, preserving contradictions and lower-precedence evidence.

**Gate:** callers can inspect advanced placement semantic knowledge independently from planning.

### PG04 — Deterministic placement-requirement planner

Introduce bounded requirements and deterministic planning that composes with the frozen 1.11 lifecycle model. Distinguish possible, runtime-verification-required, impossible, and indeterminate outcomes.

**Gate:** downstream consumers can make policy decisions without knowing protocol wire syntax or concrete geometry values.

### PG05 — Terminal-description and database-set composition

Derive recognized placement evidence from `TerminalDescription` where defensible, merge caller evidence, and compose with ordered database-set effective-definition precedence and incompleteness semantics.

**Gate:** placement semantics participate naturally in the existing inspection/database-set model without changing precedence rules.

### PG06 — Version-4 machine-readable automation

Add deterministic bounded JSON v4 profile and plan rendering plus schemas/fixtures while leaving v1-v3 unchanged.

**Gate:** CI and external automation can consume advanced placement semantics without parsing prose.

### PG07 — Terminal 1.12 interoperability and package qualification

Add a package-only downstream proof against `Icod.Terminal 1.12.0`, an executable public sample, XML/package documentation checks, and multi-TFM qualification.

**Gate:** TermInfo placement semantics map cleanly to Terminal's concrete source-rectangle and signed-z-order execution inputs while both packages remain independently versionable.

### PG08 — Hardening, exact freeze, documentation, and release closure

Complete adversarial input/bounds coverage, deterministic repetition/culture testing, exact public-API and v1-v4 schema fingerprints, release-facing guides, samples, changelog/release notes, package/archive verification, release audit, and stable promotion readiness.

**Gate:** the 1.12 contract is documented, deterministic, bounded, compatibility-frozen, package-qualified, downstream-qualified, and ready for stable promotion.

---

## 11. Stable-release acceptance criteria

`Icod.TermInfo 1.12.0` is ready for stable release when:

1. source-rectangle and signed-z-order semantics have a protocol-neutral public vocabulary;
2. the 1.11 lifecycle public surface is unchanged except for unavoidable defect correction;
3. JSON v1-v3 contracts remain immutable;
4. JSON v4 contains exactly the 1.12 placement profile and placement plan document kinds;
5. unknown support is not collapsed to unsupported;
6. evidence and contradictions remain visible, bounded, immutable, and deterministic;
7. profiles can be produced from explicit evidence and supported description/database-set inputs;
8. planning composes with the 1.11 lifecycle model without carrying concrete geometry values;
9. runtime-verification requirements are explicit;
10. database-set precedence and incompleteness semantics remain unchanged;
11. `Icod.TermInfo` has no dependency on `Icod.Terminal`;
12. a package-only downstream consumer proves semantic interoperability with `Icod.Terminal 1.12.0`;
13. public API and all four JSON schema versions are exactly frozen;
14. samples and package-only consumers pass on `net8.0`, `net9.0`, and `net10.0`;
15. Windows/Linux/macOS package and six-RID archive validation are green.

---

## 12. Later-track handoff

After 1.12, the most likely subsequent semantic tracks are:

- generalized machine-readable live-evidence interchange; and
- multi-protocol preference/negotiation once more than one meaningful execution backend exists and preference policy can be expressed semantically rather than by terminal-brand or protocol-name heuristics.

Neither is required for the 1.12 release contract.
