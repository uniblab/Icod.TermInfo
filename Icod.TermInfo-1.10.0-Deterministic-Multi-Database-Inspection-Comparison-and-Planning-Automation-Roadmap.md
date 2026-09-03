# Icod.TermInfo 1.10.0 — Deterministic Multi-Database Inspection, Comparison, and Planning Automation Roadmap

**Project:** `Icod.TermInfo`  
**Release:** `1.10.0`  
**Development branch:** `1.10.0`  
**Theme:** Deterministic Multi-Database Inspection, Comparison, and Planning Automation  
**Primary package:** `Icod.TermInfo.Inspection`  
**Command composition:** `toe`, `infocmp`, `icod-terminfo`  
**Frozen lower layers:** `Icod.TermInfo`, `Icod.TermInfo.Source`, `Icod.TermInfo.Compiler`, `Icod.TermInfo.Termcap` except for unavoidable defect corrections  
**Baseline:** stable `1.9.0`  
**Status:** DA02 implementation complete; Staging validation pending  

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

- the output SHALL record that roots came from discovery;
- encoded/non-directory `TERMINFO` states SHALL remain visible or explicitly skipped according to the frozen discovery/catalog contracts;
- the same reusable database-set engine SHALL execute after discovery;
- explicit-root automation SHALL remain available and preferable for reproducible builds.

Discovery SHALL NOT become implicit in a command form documented as deterministic explicit-root analysis.

## 9.5 JSON contract

New database-set and database-set-comparison document kinds SHALL use a clearly versioned schema extension. The schema SHALL define:

- envelope/version behavior;
- deterministic property ordering;
- root and occurrence evidence;
- completeness state;
- canonical/alias collision representation;
- equal/conflicting shadow classification;
- comparison structure;
- size bounds;
- escaping and UTF-8 behavior;
- exact LF policy for command output.

Frozen 1.9 document kinds SHALL remain byte-compatible.

## 9.6 Router and archives

Direct commands, the `icod-terminfo` router, and all supported release archives SHALL expose identical semantics.

**Gate DA06:** users can automate multi-database inspection/comparison/planning through direct and routed commands without scraping human output and without breaking any frozen 1.9 command or JSON form.

---

# 10. DA07 — Generated-state, cross-host, package, and pathological hardening

**Development version:** `1.10.0-Alpha-7`

## 10.1 Objective

Prove that the database-set contracts remain deterministic, bounded, and portable under realistic and hostile conditions.

## 10.2 Generated-state validation

Create seeded generated database sets containing combinations of:

- unique entries;
- equal duplicates;
- conflicting duplicates;
- aliases;
- alias/canonical collisions;
- extended capabilities;
- large strings;
- many roots;
- many identities;
- missing roots;
- malformed entries;
- misplaced entries;
- linked entries where applicable;
- permission/I/O failures where testable deterministically.

Expected results SHALL be derived by an independent oracle where practical.

## 10.3 Cross-host determinism

Windows, Linux, and macOS validation SHALL prove stable semantic ordering and JSON output independent of:

- path separator;
- filesystem enumeration order;
- culture;
- current UI culture;
- process locale;
- case behavior of the host filesystem;
- repeated process execution.

Where physical path text necessarily differs by host, schema semantics SHALL distinguish host-specific evidence from portable semantic ordering.

## 10.4 Bounds

Pathological tests SHALL cover:

- maximum root count;
- maximum aggregate entry count;
- large duplicate groups;
- large alias sets;
- large structured comparison output;
- JSON UTF-8 byte limits;
- planner candidate/search bounds;
- cancellation before and during expensive analysis;
- no partial JSON stdout on failure.

## 10.5 Package-consumer validation

Fresh consumers SHALL validate the new Inspection APIs from packed artifacts rather than repository project references.

Direct commands, routed commands, and release archives SHALL execute representative multi-database JSON and planning workflows.

**Gate DA07:** the 1.10 implementation has deterministic generated-state/oracle coverage, cross-host evidence, package-consumer validation, artifact smoke, and pathological bound tests with no known nondeterministic ordering or unbounded work path.

---

# 11. DA08 — API, schema, packaging, documentation, and release closure

**Development version:** `1.10.0-Alpha-8`

## 11.1 Objective

Freeze the complete additive 1.10 contract. DA08 SHALL add no new feature behavior.

## 11.2 API freeze

Freeze and review every public Inspection type/member added by DA01–DA07.

The review SHALL verify:

- no accidental mutable collections;
- no accidental exposure of implementation-specific dictionaries/sets;
- nullability contracts;
- argument validation;
- cancellation semantics;
- bounds/options defaults;
- deterministic enumeration guarantees;
- XML documentation where required;
- source and binary compatibility with the frozen 1.x package family.

## 11.3 JSON/schema freeze

Freeze the exact schema bytes/fingerprint for any new versioned machine-readable contract.

Permanent tests SHALL verify that existing 1.9 version-1 fixtures and schema behavior remain unchanged.

## 11.4 Command freeze

Freeze exact help, usage, option compatibility, exit-status, stdout/stderr, LF, cancellation, and failure behavior for each new command form.

## 11.5 Distribution freeze

Validate:

- all coordinated package versions = `1.10.0-Alpha-8`;
- package dependency graph remains directional;
- installable router package;
- direct project execution;
- six release archive RIDs where retained by current release infrastructure;
- package verifier and archive verifier behavior;
- repository build/test/pack from clean state.

## 11.6 Documentation

Update at least:

- repository `README.md`;
- `docs/COMPATIBILITY.md`;
- `Icod.TermInfo-Post-1.0-Development-Roadmap.md`;
- `toe/README.md`;
- `infocmp/README.md`;
- package/readme surfaces for Inspection and Tools as appropriate;
- a `docs/1.10.0-RELEASE-AUDIT.md` final audit.

## 11.7 Stable promotion

After Alpha-8 is validated, stable `1.10.0` SHALL be a promotion-only release. No new public API, schema fields, command switches, ordering rules, planner semantics, or package topology may be introduced between Alpha-8 closure and stable publication.

**Gate DA08:** the complete 1.10 additive API/schema/command/package surface is frozen, documented, validated from packed artifacts, and ready for stable promotion without another feature tranche.

---

# 12. Cross-tranche invariants

Every tranche SHALL preserve all of the following unless this roadmap is explicitly amended before implementation:

1. `Icod.TermInfo` Runtime remains dependency-free.
2. Existing `TerminalDescription` semantics remain authoritative.
3. Existing 1.9 JSON version-1 documents remain compatible.
4. No native `ncurses`, `libtinfo`, Berkeley DB, `tic`, `toe`, or `infocmp` production dependency is introduced.
5. Explicit-root analysis does not mutate process environment variables.
6. Input root order remains observable and semantically meaningful.
7. Filesystem enumeration order never becomes an externally visible semantic order.
8. Semantic equality comes from managed semantic comparison, not binary-file equality.
9. Incomplete evidence remains incomplete; later roots do not erase earlier uncertainty.
10. Planner scoring/search semantics remain frozen from 1.8.
11. Synthesis semantics remain frozen from 1.7.
12. Existing 1.4–1.9 command forms remain compatible.
13. Public APIs snapshot caller collections rather than retaining mutable caller-owned sequences.
14. External input remains bounded and cancellation-aware.
15. Tests SHALL not rely on the host having ncurses installed unless explicitly marked as optional differential evidence.

---

# 13. Explicit 1.10 non-goals

The following are deliberately outside the 1.10 release unless required to fix a defect in an existing contract:

- Berkeley DB / hashed terminfo store reading or writing;
- HP-UX, AIX, OSF/1, or other historical binary dialects;
- native ncurses dependencies;
- changing Runtime discovery precedence;
- writing to multiple database roots as a synchronization/deployment engine;
- automatically repairing conflicting databases;
- deleting shadowed entries;
- modifying system terminfo databases;
- reconstructing original source comments/whitespace/provenance from compiled entries;
- changing 1.7 relative-source semantics;
- changing 1.8 planner score/search semantics;
- silently extending the frozen 1.9 JSON version-1 schema;
- NativeAOT or self-contained distribution work except where already required by current release infrastructure;
- shell completion generation;
- terminal probing, live input, PTYs, curses, or terminal emulation;
- package-family work belonging to `Icod.Terminal`, `Icod.Pty`, or `Icod.DCurses`.

---

# 14. Release success criteria

`Icod.TermInfo 1.10.0` is complete when all of the following are true:

- an ordered explicit set of conventional terminfo databases can be represented immutably;
- aggregate lookup preserves precedence and incomplete evidence;
- canonical duplicates and alias collisions are classified deterministically;
- semantic equal/conflicting shadows use the frozen comparison engine;
- two database sets can be compared structurally and by effective semantics;
- the frozen planner can consume deterministic candidates from multiple explicit catalogs;
- direct and routed commands expose machine-readable multi-database workflows;
- existing 1.9 JSON documents remain compatible;
- any new JSON contract is versioned, bounded, schema-defined, deterministic, and fixture-frozen;
- all APIs and command outputs are culture-independent and repeatable;
- package consumers and release archives execute representative workflows;
- cross-host validation is green;
- resource/cancellation/pathological tests are green;
- the exact public API and schema are frozen at `1.10.0-Alpha-8`;
- stable `1.10.0` requires only coordinated-version promotion and final release publication.

---

# 15. Tranche summary

| Tranche | Version | Theme | Principal outcome |
|---|---|---|---|
| **DA01** | `1.10.0-Alpha-1` | Database-set foundation | Immutable explicit ordered database-set model |
| **DA02** | `1.10.0-Alpha-2` | Aggregate precedence | Winner/shadow/indeterminate lookup semantics |
| **DA03** | `1.10.0-Alpha-3` | Conflict analysis | Semantic duplicate, alias collision, and shadow classification |
| **DA04** | `1.10.0-Alpha-4` | Set comparison | Effective semantic plus structural/provenance comparison |
| **DA05** | `1.10.0-Alpha-5` | Multi-database planning | Deterministic candidate construction over multiple explicit catalogs |
| **DA06** | `1.10.0-Alpha-6` | CLI/JSON automation | `toe`/`infocmp`/router machine-readable composition |
| **DA07** | `1.10.0-Alpha-7` | Hardening | Generated-state, cross-host, package, archive, and pathological validation |
| **DA08** | `1.10.0-Alpha-8` | Release closure | API/schema/command/package/documentation freeze |

The roadmap intentionally makes DA01–DA05 reusable library work first, DA06 command composition second, and DA07–DA08 proof/freeze work last. This preserves the package-family rule that commands remain thin adapters over reusable managed engines.
