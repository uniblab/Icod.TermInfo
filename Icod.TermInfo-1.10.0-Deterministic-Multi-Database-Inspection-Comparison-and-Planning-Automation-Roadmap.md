# Icod.TermInfo 1.10.0 — Deterministic Multi-Database Inspection, Comparison, and Planning Automation Roadmap

**Project:** `Icod.TermInfo`  
**Release:** `1.10.0`  
**Development branch:** `1.10.0`  
**Theme:** Deterministic Multi-Database Inspection, Comparison, and Planning Automation  
**Primary package:** `Icod.TermInfo.Inspection`  
**Command composition:** `toe`, `infocmp`, `icod-terminfo`  
**Frozen lower layers:** `Icod.TermInfo`, `Icod.TermInfo.Source`, `Icod.TermInfo.Compiler`, `Icod.TermInfo.Termcap` except for unavoidable defect corrections  
**Baseline:** stable `1.9.0`  
**Status:** DA08 and consumer documentation/sample closure complete and validated; stable `1.10.0` promotion applied; publication pending

---

## 1. Release objective

`Icod.TermInfo 1.10.0` SHALL extend the 1.9 machine-readable inspection and planning foundation from one explicit conventional database at a time to deterministic analysis of an **ordered set of explicit conventional terminfo databases**.

The release SHALL make the following questions answerable through reusable managed APIs and stable command automation without parsing human-oriented output:

- what entries are present across an ordered database set;
- which canonical identities occur in more than one database;
- which duplicates are semantically equal and which conflict;
- which physical definition wins under ordered search precedence;
- which definitions are shadowed by earlier databases;
- which aliases collide or resolve ambiguously across roots;
- which roots are complete, incomplete, missing, unavailable, or unsupported;
- how two database sets differ semantically;
- which candidate parents should be selected for relative-source planning when candidates are drawn from one or more explicit databases;
- what exact evidence produced each result.

The release SHALL build on the frozen 1.8 planner and frozen 1.9 JSON/catalog contracts rather than introducing another terminfo parser, comparison engine, planner, or database-discovery implementation.

The conceptual progression is:

```text
1.7  target + ordered parents
      -> deterministic relative source

1.8  target + explicit candidates
      -> deterministic best ordered parents
      -> deterministic relative source

1.9  descriptions / comparisons / plans / explicit catalogs
      -> bounded versioned JSON automation

1.10 ordered explicit database sets
      -> deterministic aggregate inspection
      -> shadow/conflict analysis
      -> set comparison
      -> catalog-backed parent planning
      -> bounded versioned automation
```

---

## 2. Non-negotiable architectural constraints

### 2.1 Preserve the frozen runtime contract

`Icod.TermInfo` remains the dependency-free runtime package. 1.10 SHALL NOT move database-set orchestration, aggregate inspection, cross-catalog comparison, or planning policy into Runtime.

The primary implementation home is `Icod.TermInfo.Inspection` because the work is a deterministic transformation over existing Runtime descriptions, 1.4 catalog inspection, 1.3 comparison semantics, 1.7 synthesis, 1.8 planning, and 1.9 JSON rendering.

### 2.2 Explicit roots first

The foundational 1.10 database-set APIs SHALL operate on caller-supplied **ordered explicit roots or already-inspected catalogs**.

They SHALL NOT depend on ambient `TERMINFO`, `TERMINFO_DIRS`, home-directory discovery, platform defaults, process-global current-terminal state, or mutable environment state.

Host discovery MAY be composed later through the already-frozen Runtime/Inspection discovery model, but discovery SHALL remain visibly distinct from deterministic explicit-root analysis.

### 2.3 Ordered search semantics are evidence, not an implementation accident

Input root order SHALL be retained exactly. When the same terminal identity is available from more than one applicable database, earlier roots have higher precedence for effective lookup semantics.

The aggregate result SHALL retain enough evidence to distinguish:

```text
winning definition
semantically equal shadowed definition
semantically different shadowed definition
unreadable/incomplete candidate
alias-mediated duplicate
physical duplicate within one root
cross-root duplicate
```

No global sorting step may erase the caller-selected root order.

### 2.4 Reuse the canonical semantic engines

The following remain authoritative:

- `TerminalDescription` for resolved terminal semantics;
- `TerminalDescriptionComparer` for semantic equality/difference;
- `TermInfoDatabaseInspector` and the frozen catalog model for explicit conventional-directory inspection;
- `TerminalDescriptionSourceSynthesizer` for relative-source synthesis;
- `TerminalDescriptionSourcePlanner` for bounded deterministic parent selection;
- `TermInfoJsonRenderer` and its frozen 1.9 contracts for existing JSON document kinds.

1.10 SHALL compose these engines rather than duplicate their logic.

### 2.5 JSON version 1 is immutable

The 1.9 JSON version-1 envelope, existing document kinds, schema, ordering, field spelling, bounds, and semantics are frozen.

1.10 SHALL NOT add fields to existing version-1 document shapes or reinterpret them.

If database-set or database-set-comparison documents cannot be represented as existing frozen document kinds, 1.10 SHALL introduce a clearly versioned additive JSON contract. Existing 1.9 invocations MUST continue to emit byte-for-byte compatible version-1 documents for equivalent inputs.

### 2.6 Determinism over convenience

All aggregate ordering SHALL be explicitly specified. Culture, filesystem enumeration order, dictionary hashing, thread scheduling, platform path casing behavior, and process locale SHALL NOT affect semantic ordering or serialized output.

### 2.7 Hostile and partial input remain first-class

A database set may contain missing, unavailable, malformed, oversized, linked, misplaced, unsupported, or otherwise incomplete roots/entries.

The aggregate API SHALL preserve this evidence. It SHALL NOT silently convert incomplete input into a complete-looking result.

### 2.8 Bounds are part of the public contract

Database counts, entry counts, duplicate groups, comparison work, candidate expansion, serialized output, and planning search SHALL remain bounded. Where existing 1.8/1.9 bounds already govern a composed operation, 1.10 SHALL reuse them rather than invent parallel meanings.

---

## 3. Versioning policy

Development SHALL proceed in eight tranches:

```text
DA01 -> 1.10.0-Alpha-1
DA02 -> 1.10.0-Alpha-2
DA03 -> 1.10.0-Alpha-3
DA04 -> 1.10.0-Alpha-4
DA05 -> 1.10.0-Alpha-5
DA06 -> 1.10.0-Alpha-6
DA07 -> 1.10.0-Alpha-7
DA08 -> 1.10.0-Alpha-8
```

Each completed tranche SHALL update the repository's coordinated `<Version />` and `<PackageVersion />` to the tranche version before the tranche is considered complete.

A tranche version bump SHALL be committed together with that tranche's implementation/documentation or as the final closure commit for that tranche. The version SHALL never be advanced merely because work has begun.

After DA08 is frozen and all release gates are green, stable `1.10.0` SHALL promote the validated Alpha-8 surface without adding new feature semantics.

---

# 4. DA01 — Database-set model and contract foundation

**Development version:** `1.10.0-Alpha-1`

## 4.1 Objective

Introduce the minimum immutable Inspection-layer model required to represent an ordered set of explicitly inspected conventional database roots while preserving every individual 1.9 catalog unchanged.

## 4.2 Required design

The preferred public concepts are equivalent in responsibility to:

```text
TermInfoDatabaseSet
TermInfoDatabaseSetEntry
TermInfoDatabaseSetIdentity
TermInfoDatabaseSetOccurrence
TermInfoDatabaseSetIssue
TermInfoDatabaseSetOptions
```

Exact names are subject to implementation review, but the model SHALL provide:

- a snapshot of input roots/catalogs in caller order;
- an immutable ordered list of per-root catalog evidence;
- deterministic aggregate canonical-identity indexing;
- occurrence evidence identifying source catalog index and entry index;
- distinction between canonical identity and aliases;
- distinction between complete and incomplete constituent catalogs;
- an aggregate completeness contract that cannot claim completeness if required input evidence is incomplete;
- cancellation and explicit resource bounds where aggregation work can be large.

## 4.3 Construction paths

The reusable API SHOULD support both:

```text
explicit roots -> inspect each once -> database set
```

and:

```text
already-inspected frozen 1.9 catalogs -> database set
```

The latter is important so callers can control I/O, caching, testing, and orchestration without forced reinspection.

## 4.4 Identity rules

Canonical terminal names SHALL be compared using the same ordinal rules already used by the package family.

Aliases SHALL NOT be silently promoted to independent canonical entries. The aggregate model SHALL retain enough information to answer which canonical entry owns each alias occurrence.

## 4.5 Tests

DA01 SHALL include tests for:

- zero roots;
- one root equivalence with the existing 1.9 catalog contract;
- multiple complete roots;
- missing/incomplete roots;
- duplicate canonical names within and across roots;
- alias overlap;
- stable input snapshotting;
- culture independence;
- deterministic repeated construction;
- cancellation and configured bounds;
- public API baseline growth limited to the intended Inspection additions.

**Gate DA01:** an application can construct an immutable deterministic ordered database-set value from explicit roots or frozen catalogs, and no comparison/shadow/planning policy has yet been layered onto that foundation.

---

# 5. DA02 — Deterministic multi-catalog inspection and precedence

**Development version:** `1.10.0-Alpha-2`

## 5.1 Objective

Make the database-set model operational for aggregate lookup and ordered precedence without yet classifying semantic conflicts.

## 5.2 Required behavior

For every canonical identity, the aggregate SHALL identify:

- all occurrences in deterministic root/entry order;
- the first applicable occurrence under ordered search precedence;
- later shadowed occurrences;
- aliases associated with each occurrence;
- whether an apparent occurrence came from an incomplete catalog whose evidence prevents a complete conclusion.

The aggregate SHALL expose an unambiguous distinction between:

```text
not observed
observed once
observed multiple times
winner known
winner indeterminate because earlier input is incomplete
```

## 5.3 Lookup contract

A reusable lookup API MAY return a structured result rather than `bool` so that clean absence, successful precedence resolution, and indeterminate/incomplete state are not conflated.

No API SHALL silently skip an earlier incomplete database and claim that a later occurrence is definitively the effective winner.

## 5.4 Path handling

Physical root paths and entry paths SHALL retain the normalization behavior of the frozen catalog layer. 1.10 SHALL NOT introduce a second path-normalization policy.

**Gate DA02:** callers can deterministically ask what definition would win across a complete explicit ordered set and receive explicit indeterminate evidence when incomplete earlier roots prevent that conclusion.

---

# 6. DA03 — Semantic duplicate, conflict, alias, and shadow analysis

**Development version:** `1.10.0-Alpha-3`

## 6.1 Objective

Classify repeated identities using the frozen semantic comparison engine.

## 6.2 Duplicate classification

For each canonical identity with multiple usable occurrences, classify later occurrences relative to the precedence winner as at least:

```text
semanticallyEqual
semanticallyDifferent
indeterminate
```

Semantic equality SHALL come from `TerminalDescriptionComparer`, never compiled-file byte equality.

The aggregate SHOULD retain the structured comparison result for conflicts or provide a bounded route to obtain it without reacquiring the entries.

## 6.3 Alias analysis

DA03 SHALL explicitly model collisions such as:

- the same alias owned by semantically equal canonical identities;
- the same alias owned by different canonical identities;
- an alias matching another entry's canonical name;
- cross-root alias ownership changes caused by precedence;
- incomplete evidence preventing a definitive alias result.

The result SHALL distinguish an alias collision from a canonical-name duplicate.

## 6.4 Shadow analysis

The model SHALL make it possible to report:

- winner root;
- shadowed root(s);
- equal shadows;
- conflicting shadows;
- identities whose winning status is indeterminate.

This analysis is the reusable semantic engine for later `toe` presentation and JSON automation.

## 6.5 Scale and laziness

Comparison work can grow with duplicates. The implementation SHALL avoid all-pairs comparison when winner-versus-shadow classification suffices. Any expanded comparison surface SHALL document its work bound.

**Gate DA03:** a caller can explain every repeated canonical identity and alias across an explicit ordered database set as equal, conflicting, or indeterminate, with deterministic evidence and no byte-level semantic shortcuts.

---

# 7. DA04 — Database-set semantic comparison

**Development version:** `1.10.0-Alpha-4`

## 7.1 Objective

Compare two ordered database sets as semantic collections and as effective precedence views.

## 7.2 Comparison dimensions

The reusable comparison SHALL distinguish at least:

```text
root topology differences
identity only in left
identity only in right
same identity / equal effective winner
same identity / different effective winner
same effective semantics / different provenance
alias ownership difference
shadow-set difference
completeness/issue difference
indeterminate comparison due to incomplete evidence
```

## 7.3 Effective versus structural comparison

The API SHOULD separate or clearly classify:

- **effective semantic difference** — the terminal description selected by precedence differs;
- **structural/provenance difference** — effective description is equal but root placement, shadow copies, aliases, or issues differ.

This distinction is important for deployment auditing: two hosts may behave identically for a terminal while having materially different database provenance.

## 7.4 Deterministic ordering

Difference ordering SHALL be frozen. A preferred order is identity-ordinal within a stable difference-kind hierarchy, with root/occurrence indices used as final deterministic evidence rather than host path collation.

## 7.5 Independent oracle tests

Tests SHALL include independently constructed small-set truth tables rather than validating the comparer only against itself.

**Gate DA04:** two explicit ordered database sets can be compared with a stable structured result that distinguishes effective semantic changes from topology/provenance changes.

---

# 8. DA05 — Multi-database candidate planning

**Development version:** `1.10.0-Alpha-5`

## 8.1 Objective

Generalize the 1.9 explicit-directory all-candidates planning composition so candidate parents may be drawn deterministically from an ordered explicit database set.

The frozen 1.8 `TerminalDescriptionSourcePlanner` remains the planner. DA05 owns only deterministic candidate discovery, normalization, de-duplication, eligibility, and evidence.

## 8.2 Candidate construction

Candidate enumeration SHALL:

- inspect each explicit root once;
- preserve root order and catalog order as deterministic source order;
- exclude the target according to the frozen planner identity rule;
- collapse semantically redundant physical publications only under a documented rule;
- retain enough evidence to identify the original root and canonical entry for every planner candidate;
- reject or explicitly handle conflicting duplicate candidates rather than arbitrarily selecting one;
- propagate incomplete catalog evidence rather than planning from a falsely complete candidate universe.

## 8.3 Planning policy

The existing bounds remain authoritative:

```text
max parents
max plans
require exhaustive
allow bounded
```

DA05 SHALL NOT change the planner score ordering, parent permutation semantics, synthesis semantics, or rendering semantics.

## 8.4 Result evidence

The composed result SHALL permit a caller to map each selected parent back to:

```text
input database index
catalog entry index
canonical name
use= spelling selected for emission
semantic identity
```

## 8.5 Explicit roots only at the core

The reusable DA05 engine SHALL accept explicit roots/catalogs/database-set values. Ambient system discovery is deferred to DA06 command composition.

**Gate DA05:** a target can be planned against candidates drawn from multiple explicit ordered databases without changing the frozen 1.8 planner or 1.7 synthesizer semantics.

---

# 9. DA06 — Command and machine-readable automation composition

**Development version:** `1.10.0-Alpha-6`

## 9.1 Objective

Expose the reusable 1.10 engines through thin command adapters and a versioned machine-readable representation.

## 9.2 `toe`

The preferred new explicit-root automation form is conceptually:

```text
toe --json directory [directory ...]
```

where one directory SHALL retain the frozen 1.9 output contract and multiple explicit directories SHALL produce the new database-set document contract.

Human-oriented multi-root listing behavior already exists and SHALL remain compatible.

An additional focused human analysis mode MAY be introduced for shadow/conflict reporting if it can be made unambiguous and useful; JSON automation is the primary requirement.

## 9.3 `infocmp`

The preferred new planning composition SHALL permit catalog-backed planning across more than one explicit candidate database without requiring every candidate terminal name as a positional operand.

Exact CLI syntax SHALL be frozen during DA06 after reviewing ambiguity with existing `-A`, `-B`, `--plan-use`, and `--all-candidates` semantics.

The command layer SHALL NOT reimplement candidate discovery or planning.

## 9.4 Discovery composition

DA06 MAY add a clearly explicit switch that converts the already-frozen Runtime discovery snapshot into an ordered database set. If added:

- it SHALL visibly preserve the snapshot's ordered locations;
- it SHALL NOT change the Runtime discovery rules;
- explicit-root command forms SHALL remain available for reproducible automation independent of host discovery.

Ambient discovery is useful for operator convenience, but explicit roots remain the primary automation contract.

## 9.5 New JSON contract

A new machine-readable document SHALL use a deliberate versioned contract appropriate for database-set semantics.

The document SHALL retain at least:

```text
ordered input roots
root completeness/status
canonical identity
winning occurrence
shadowed occurrence(s)
equal/conflicting/indeterminate classification
alias collision evidence
constituent catalog issues
comparison/planning evidence when applicable
configured bounds affecting the answer
```

Version selection SHALL be deterministic from the command/API mode. The command SHALL NOT guess a schema from output contents after execution.

The version-1 existing document kinds remain unchanged.

## 9.6 Output framing

Successful JSON automation SHALL retain the established contract:

```text
one JSON document
one LF
no prose
no BOM
```

Operational failure or cancellation SHALL NOT emit partial JSON.

**Gate DA06:** commands can consume deterministic multi-database inspection, comparison, and planning results without parsing human output, while every frozen 1.9 invocation remains byte-compatible.

---

# 10. DA07 — Generated-state, cross-host, package, and pathological hardening

**Development version:** `1.10.0-Alpha-7`

## 10.1 Objective

Prove determinism and robustness where orchestration is most likely to fail: culture, root ordering, filesystem shape, incomplete evidence, package consumption, command framing, and cross-host execution.

## 10.2 Generated-state tests

Generate controlled temporary databases containing combinations such as:

```text
same canonical name / equal semantics / two roots
same canonical name / different semantics / two roots
same alias / different canonical names
alias matching another canonical name
physical duplicate inside one root
missing earlier root / valid later root
malformed earlier root / valid later root
same root listed twice
equivalent databases with different physical paths
large duplicate groups
large candidate universes
```

Tests SHALL generate compiled entries through the repository's own compiler/database-writer path where practical, then inspect them through the public database-set APIs.

## 10.3 Culture and host determinism

Run the same database-set operations under cultures including:

```text
ar-SA
tr-TR
```

and compare output across Windows, Linux, and macOS.

Where path bytes inherently differ because the host path syntax differs, normalize the fixture's intentional path evidence before comparing semantic projections; do not hide real path/provenance differences in product output merely to make tests equal.

## 10.4 Package-reference-only consumer

Extend the fresh-package consumer so it can:

- construct/inspect a database set;
- classify at least one duplicate/shadow case;
- compare two sets;
- perform multi-database planning;
- render the new machine-readable document;
- reject an intentionally insufficient bound.

The consumer SHALL run on `net8.0`, `net9.0`, and `net10.0`.

## 10.5 Command/package/archive smoke

Staging verification SHALL exercise the new automation through:

```text
installed package tools
win-x64 archive
win-arm64 archive
linux-x64 archive
linux-arm64 archive
osx-x64 archive
osx-arm64 archive
```

At minimum, one database-set inspection and one database-backed planning path SHALL execute outside the source tree.

**Gate DA07:** the new 1.10 functionality is deterministic and usable through project references, package references, installed tools, and every supported standalone archive RID.

---

# 11. DA08 — API, schema, packaging, documentation, and release closure

**Development version:** `1.10.0-Alpha-8`

## 11.1 Freeze exact public API

Freeze the complete 1.10 Inspection public surface in a new exact baseline artifact.

The release verifier SHALL then require exact Inspection API equality across:

```text
net8.0
net9.0
net10.0
```

The frozen 1.9 baseline remains historical evidence but is no longer the current expected 1.10 Inspection surface after DA08.

## 11.2 Freeze exact schema

Freeze the complete new database-set JSON schema/document contract and package it as part of `Icod.TermInfo.Inspection` alongside the frozen 1.9 schema where appropriate.

Validate:

- compact output;
- indented output;
- exact UTF-8 byte bounds;
- optional and null fields;
- enum/string spellings;
- ordering;
- schema identifier and version;
- pathological escaped text;
- duplicate groups;
- incomplete evidence;
- comparison and planning forms.

## 11.3 Package verification

Update exact package verification for the 1.10 Inspection public API and schema artifacts while preserving every lower-layer package contract.

## 11.4 Command closure

Freeze help, usage, exit codes, stdout/stderr framing, JSON mode combinations, and router dispatch.

Historical exact command forms from 1.9 SHALL remain green and byte-compatible.

## 11.5 Documentation closure

Update:

- root README;
- Inspection README;
- `toe` README;
- `infocmp` README;
- compatibility notes;
- versioning notes;
- package release notes;
- samples/consumer documentation;
- 1.10 release audit.

## 11.6 Stable promotion

After all DA08 gates are green:

1. promote the coordinated repository version from `1.10.0-Alpha-8` to `1.10.0`;
2. rerun the exact release verifier;
3. rerun package and archive smoke;
4. verify command versions and JSON compatibility;
5. merge only after all gates are green.

**Gate DA08:** the complete 1.10 API, schema, command, package, documentation, and compatibility surface is frozen and ready for stable promotion.

---

# 12. Cross-tranche mandatory test matrix

Every tranche SHALL keep the following green where applicable:

```text
Build
  net8.0
  net9.0
  net10.0
  zero warnings in closure configuration

Runtime
  existing full suite
  unchanged public API

Source
  existing full suite
  unchanged public API

Compiler
  existing full suite
  unchanged public API

Inspection
  existing full suite
  additive API only until DA08 freeze
  deterministic aggregate ordering
  bounded work

Termcap
  existing full suite
  unchanged public API

Commands
  tic
  infocmp
  toe
  captoinfo
  infotocap
  icod-terminfo
  existing behavior unchanged unless explicitly extended

Packages
  coordinated versions
  exact lower-layer artifacts
  package-reference-only consumer

Archives
  win-x64
  win-arm64
  linux-x64
  linux-arm64
  osx-x64
  osx-arm64
```

---

# 13. Explicit non-goals for 1.10

Unless unavoidable for compatibility or correctness, 1.10 SHALL NOT add:

- DBM storage support;
- hashed database formats;
- zip-backed databases;
- remote terminfo databases;
- registry-backed database policy;
- automatic filesystem watchers;
- mutable catalog caches;
- command-output scraping APIs;
- a second semantic comparison engine;
- a second relative-source planner;
- a second discovery engine;
- a generic JSON object serializer for terminfo state;
- ambient environment discovery inside foundational explicit-root APIs;
- parallel traversal whose scheduling can affect ordering.

Those remain post-1.10 work.

---

# 14. Stable `1.10.0` definition of done

Stable `1.10.0` is complete only when all of the following are true:

- DA01 through DA08 are complete and frozen;
- explicit ordered database sets are represented immutably;
- aggregate precedence is deterministic and evidence-preserving;
- equal/conflicting/indeterminate duplicates are classified semantically;
- alias collisions are explicit;
- two database sets can be compared structurally and semantically;
- multi-database candidate planning composes the frozen 1.8 planner;
- machine-readable automation is versioned, bounded, deterministic, and schema-backed;
- incomplete roots never produce falsely complete answers;
- existing 1.9 JSON invocations remain byte-compatible;
- package-reference-only consumers work on net8/net9/net10;
- installed tools work on Windows/Linux/macOS;
- all six standalone archive RIDs pass smoke;
- exact 1.10 Inspection API and schema baselines are frozen;
- lower-layer public contracts remain unchanged;
- package and archive artifacts are release-verified;
- documentation matches the shipped behavior;
- stable promotion adds no new feature semantics.

At that point the release may be promoted to `1.10.0` and merged/published through the standard repository release workflow.
