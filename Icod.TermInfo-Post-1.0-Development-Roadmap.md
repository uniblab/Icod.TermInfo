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
**Current coordinated version:** `1.9.0-Alpha-3`
**Final 1.6 prerelease:** `1.6.0-Alpha-8`
**Final 1.7 prerelease:** `1.7.0-Alpha-8`
**Final 1.8 prerelease:** `1.8.0-Alpha-8`
**Planned final 1.9 prerelease:** `1.9.0-Alpha-7`
**Next development line:** `1.9.0` - Machine-Readable Inspection and Planning Automation
**Status:** 1.9.0 development active
**Current tranche:** MI03 - Comparison and Planning Evidence JSON
**Primary objective:** Render effective descriptions, comparisons, plans, and explicit catalogs as deterministic bounded versioned JSON, then compose that reusable representation through `infocmp` and `toe` without changing frozen lower-layer semantics.

---

## 1. Purpose

`Icod.TermInfo 1.0.0` establishes the stable managed runtime foundation:

- immutable `TerminalDescription` values;
- complete standard and extended capability metadata;
- compiled terminfo parsing;
- system/environment/database discovery;
- provider composition and caching;
- built-in terminal profiles;
- parameter expansion;
- padding-aware output;
- indexed and direct-color semantics;
- stable `net8.0` / `net10.0` support.

Post-1.0 development SHALL build outward from that foundation rather than repeatedly enlarging the runtime package.

The principal new family is **terminfo source and tooling**:

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

The arrows are dependency arrows. `Icod.TermInfo` remains dependency-free.
`Icod.TermInfo.Source` and `Icod.TermInfo.Termcap` each depend on
`Icod.TermInfo`. Compiler and Inspection each depend on matching Runtime and
Source packages. No dependency may point from the runtime package back toward
Source, Compiler, Inspection, Termcap, or Tools.

Live-terminal state, terminal input, probing, PTYs, curses presentation, and terminal emulation remain outside this roadmap and belong to their respective packages.

---

## 1.1 Roadmap authority

This document is the authoritative active roadmap for work after the frozen 1.0
Runtime contract. Version-specific roadmaps and release audits remain the
authoritative historical records for completed releases.

`docs/FUTURE-WORK-INVENTORY.md` is retired as an active planning document. It is
retained only so historical 0.9 through 1.3 records which link to it do not break.
New work, ownership decisions, and release sequencing SHALL be recorded here or
in a new version-specific roadmap, not in the retired inventory.

---

## 2. Version Sequence

| Version | Theme | Outcome |
|---|---|---|
| **1.1.0** | Terminfo source language | Parse and resolve `.ti` source into `TerminalDescription` |
| **1.2.0** | Terminfo compiler | Write conventional compiled terminfo entries; provide the `tic` engine |
| **1.3.0** | Inspection/comparison | `infocmp` engine, canonical source rendering, semantic comparison |
| **1.4.0** | Tool suite | Actual `tic`, `infocmp`, and `toe` command projects |
| **1.5.0** | Coordinated distribution | Centralize suite versioning and add the installable command router without changing frozen library APIs or 1.4 command semantics |
| **1.6.0** | Termcap interoperability | Parse, resolve, and convert termcap and terminfo |
| **1.6.1** | Release-verifier hotfix | Restore caller NuGet-cache state before repository sample/toolchain validation; no public API or command-semantic changes |
| **1.7.0** | Relative terminfo source synthesis | Synthesize deterministic relative `.ti` source in Inspection and expose it through `infocmp -u` |
| **1.8.0** | Relative source planning | Select deterministic bounded ordered parents for the frozen 1.7 relative-source synthesizer |
| **1.9.0** | Machine-readable inspection and planning automation | Render versioned deterministic JSON for Inspection values and expose explicit command automation without parsing human output |
| **later** | Exotic storage/formats | Berkeley DB provider and historical Unix dialects as justified |

The completed 1.5 release contract is recorded in
[`Icod.TermInfo-1.5.0-Coordinated-Distribution-Roadmap.md`](Icod.TermInfo-1.5.0-Coordinated-Distribution-Roadmap.md)
and its immutable release requirements and final sign-off are recorded by
[`docs/1.5.0-RELEASE-AUDIT.md`](docs/1.5.0-RELEASE-AUDIT.md).

Version 1.5.0 completed the coordinated distribution tranche. The published
`Icod.TermInfo.Tools` package installs the `icod-terminfo` multi-command router,
which multiplexes the frozen `tic`, `infocmp`, and `toe` implementations while
the standalone archives retain the traditional command names.

Version 1.6.0 adds the optional `Icod.TermInfo.Termcap` package plus the
`captoinfo` and `infotocap` command implementations. The `icod-terminfo` router
and standalone suite archives therefore expose five commands.

Version 1.6.1 preserves that frozen package/API/command surface and corrects a
release-verifier environment leak: package-smoke validation must restore the
caller's `NUGET_PACKAGES` state before repository sample/toolchain builds.

Version 1.7.0 is governed by
[`Icod.TermInfo 1.7.0 — Relative Terminfo Source Synthesis Roadmap.md`](Icod.TermInfo%201.7.0%20-%20Relative%20Terminfo%20Source%20Synthesis%20Roadmap.md).
RS01 established the additive synthesis contract in Inspection. RS02 implements
the standard Boolean, numeric, and string parent aggregate, local delta, and
cancellation engine. RS03 extends the same engine to ordinal case-sensitive
extended capabilities, kind changes, inherited cancellation, deterministic
ordering, and semantically safe extended-output filtering while preserving the
frozen Runtime, Source, Compiler, and Termcap APIs. RS04 freezes exact ordered
multi-parent composition and source-reference fidelity: `UseName` spelling is
preserved independently of effective parent identity, repeated/equivalent
parents remain legal under distinct references, and Source-backed cross-checks
confirm leftmost-parent precedence across the complete capability universe.
RS05 freezes deterministic relative rendering plus Source and Compiler semantic
round trips. RS06 exposes the reusable engine through `infocmp -u`; RS07 adds
seeded generated-state, pathological-boundary, and pinned ncurses differential
validation. RS08 freezes the additive Inspection API, package and distribution
topology, release-verifier gates, documentation, and release audit. The
stable 1.7.0 release promotes that validated Alpha-8 surface without semantic or
public-API changes.

Version 1.8.0 is governed by
[`Icod.TermInfo-1.8.0-Relative-Source-Planning-and-Parent-Selection-Roadmap.md`](Icod.TermInfo-1.8.0-Relative-Source-Planning-and-Parent-Selection-Roadmap.md).
RP01 adds the immutable planning options, lexicographic score, result evidence,
and planner API foundation to Inspection. Candidate inputs reuse the frozen 1.7
synthesis-parent type, are snapshotted once, retain distinct input positions, and
are bounded independently from deterministic search. RP02 implements exhaustive
zero- and single-parent evaluation, obtains frozen score evidence directly from
the synthesis renderer, rejects unrepresentable plans without approximation, and
selects the deterministic best result against an independent Source-based oracle.

RP03 adds exhaustive ordered multi-parent permutations; RP04 freezes bounded
search, cancellation, and result evidence; RP05 adds explicit complete catalog
and conventional-directory orchestration without host discovery; RP06 composes
the planner through direct and routed `infocmp --plan-use` plus all six archive
RIDs; RP07 adds generated-state, independent-oracle, boundary, corpus, culture,
and repeated-process hardening. RP08 freezes the complete additive Inspection
API, package and distribution topology, samples, release verifiers, and 1.8
release audit at `1.8.0-Alpha-8` without adding another feature tranche. The
stable 1.8.0 release promotes that validated Alpha-8 surface without semantic or
public-API changes.

Version 1.9.0 is governed by
[`Icod.TermInfo-1.9.0-Machine-Readable-Inspection-and-Planning-Automation-Roadmap.md`](Icod.TermInfo-1.9.0-Machine-Readable-Inspection-and-Planning-Automation-Roadmap.md).
MI01 adds the immutable bounded JSON options and typed renderer foundation,
freezes the version-1 envelope and deterministic text contract, and leaves each
payload deliberately non-operational until its owning tranche. MI02 renders
effective descriptions. MI03 renders structured comparisons and complete
relative-source planning evidence. MI04 renders explicit database-catalog
manifests and publishes the completed version-1 JSON Schema. MI05 composes JSON
through `infocmp` and `toe` and adds explicit-directory all-candidates planning.
MI06 hardens samples, package consumers, router/archive execution, bounds, and
cross-host determinism. MI07 freezes the final 1.9 Inspection API, JSON Schema,
commands, package graph, distribution evidence, and release audit without adding
another feature tranche.

The sequence is cumulative but intentionally modular. Applications which only need runtime terminfo SHALL continue to depend on `Icod.TermInfo` alone.

---

## 3. Architectural Rules

### 3.1 Preserve the 1.0 runtime contract

`Icod.TermInfo` SHALL remain the low-level runtime package.

New source/compiler/tooling functionality SHALL NOT require ordinary consumers such as `Icod.Terminal` to acquire compiler-front-end dependencies.

Existing 1.x APIs SHALL remain source- and binary-compatible except where an unavoidable defect requires correction under normal semantic-versioning rules.

### 3.2 One semantic model

There SHALL NOT be separate semantic representations for:

- built-in terminal descriptions;
- compiled terminal descriptions;
- source terminal descriptions;
- tool-produced terminal descriptions.

All resolved inputs ultimately produce the existing immutable:

```csharp
TerminalDescription
```

The existing capability metadata SHALL remain authoritative.

### 3.3 Source state is distinct from resolved state

Terminfo source contains concepts which do not exist in a resolved compiled entry:

- `use=` inheritance;
- cancellation;
- source ordering;
- comments;
- source spans;
- unresolved references.

These SHALL be represented by a source-layer model and SHALL NOT be forced into `TerminalDescription`.

### 3.4 No ncurses runtime dependency

Testing MAY compare against ncurses tools where useful.

No production package SHALL require:

- `libtinfo`;
- `ncurses`;
- `tic`;
- `infocmp`;
- Berkeley DB;

unless a future explicitly optional provider is designed around such a dependency.

### 3.5 External input is hostile input

All parsers, resolvers, and binary writers SHALL employ:

- checked arithmetic;
- input-size limits;
- inheritance-depth limits;
- deterministic failure;
- cycle detection;
- bounded allocation;
- clear diagnostics.

---

# 4. Version 1.1.0 — Terminfo Source Language

## 4.1 Release objective

`1.1.0` SHALL introduce a managed implementation of the terminfo source language capable of parsing one or more `.ti` entries and resolving them into ordinary `TerminalDescription` instances.

The preferred new package is:

```text
Icod.TermInfo.Source
    -> Icod.TermInfo
```

The stable `Icod.TermInfo` runtime package SHALL NOT acquire a dependency on `Icod.TermInfo.Source`.

---

## 4.2 S01 — Source package foundation

Create the source-language package and test infrastructure.

Required work:

- add `Icod.TermInfo.Source`;
- target `net8.0;net10.0`;
- use C# 13;
- reference `Icod.TermInfo`;
- add source-specific tests;
- establish package metadata;
- establish coordinated repository build/test/pack;
- add public API snapshotting;
- establish source diagnostic conventions.

**Gate S01:** a fresh consumer can reference `Icod.TermInfo.Source` without changing the `Icod.TermInfo` runtime package.

**S01 implementation record:** [`docs/1.1.0-S01-SOURCE-PACKAGE-FOUNDATION.md`](docs/1.1.0-S01-SOURCE-PACKAGE-FOUNDATION.md).

---

## 4.3 S02 — Lexical and source-location model

Implement the source reader and lexical foundation.

It SHALL recognize the adopted System V/ncurses-compatible source syntax for:

- entry headers;
- aliases;
- descriptive names;
- capability separators;
- whitespace;
- comments;
- continued/multiline entries;
- Boolean capability forms;
- numeric capability forms;
- string capability forms;
- cancellation;
- `use=` references.

Every parsed construct SHOULD retain useful source position information.

Introduce source-location concepts equivalent to:

```text
file/source name
line
column
span
```

Diagnostics SHALL be able to identify the relevant source location.

**Gate S02:** representative `.ti` files can be tokenized deterministically with precise diagnostics for malformed lexical input.

**S02 implementation record:** [`docs/1.1.0-S02-LEXICAL-SOURCE-LOCATION.md`](docs/1.1.0-S02-LEXICAL-SOURCE-LOCATION.md).

---

## 4.4 S03 — String and numeric source semantics

Implement terminfo source value interpretation.

String support SHALL include the adopted terminfo escape language, including:

- escaped punctuation;
- control-character notation;
- octal/escaped byte forms where defined;
- escaped whitespace where applicable;
- deterministic handling of malformed escapes.

Source strings SHALL preserve the byte semantics required by the existing `Icod.TermInfo` model.

Numeric capabilities SHALL use the same signed 32-bit semantic model frozen in 1.0.

**Gate S03:** source capability values round-trip through known authoritative fixtures without semantic corruption.

**S03 implementation record:** [`docs/1.1.0-S03-STRING-NUMERIC-SOURCE-SEMANTICS.md`](docs/1.1.0-S03-STRING-NUMERIC-SOURCE-SEMANTICS.md).

---

## 4.5 S04 — Unresolved source-entry model

Introduce an immutable or controlled source representation for one parsed entry.

The model SHALL preserve:

- canonical name;
- aliases;
- description;
- standard Boolean capabilities;
- standard numeric capabilities;
- standard string capabilities;
- extended capabilities;
- cancellation declarations;
- `use=` declarations;
- source ordering where semantically significant;
- source locations.

The unresolved source model SHALL NOT pretend that inheritance has already occurred.

**Gate S04:** parsing a source entry requires no `TerminalDescription` construction and loses no information required for later resolution or diagnostics.

**S04 implementation record:** [`docs/1.1.0-S04-UNRESOLVED-SOURCE-ENTRY-MODEL.md`](docs/1.1.0-S04-UNRESOLVED-SOURCE-ENTRY-MODEL.md).

---

## 4.6 S05 — Capability classification

Map known source capability names through the existing canonical capability catalog.

The parser/resolver SHALL distinguish:

```text
known standard capability
known extended capability
unknown extended capability
invalid/reserved name
```

No duplicate standard capability table SHALL be introduced in `Icod.TermInfo.Source`.

Capability aliases and historical names SHALL follow an explicitly documented compatibility policy.

**Gate S05:** every standard capability accepted from source maps to the same semantic identifier used by compiled parsing and built-in profiles.

**S05 implementation record:** [`docs/1.1.0-S05-CAPABILITY-CLASSIFICATION.md`](docs/1.1.0-S05-CAPABILITY-CLASSIFICATION.md).

---

## 4.7 S06 — Cancellation semantics

Implement source cancellation.

The resolver SHALL correctly support forms equivalent to:

```text
capability@
```

for:

- Boolean capabilities;
- numeric capabilities;
- string capabilities;
- extended capabilities.

Cancellation SHALL prevent an inherited value from reappearing contrary to the adopted terminfo precedence rules.

The existing builder's cancellation/inheritance machinery SHOULD be reused or generalized rather than reimplemented independently.

**Gate S06:** cancellation tests cover local values, inherited values, multiple parents, and extended capabilities.

**S06 implementation record:** [`docs/1.1.0-S06-CANCELLATION-SEMANTICS.md`](docs/1.1.0-S06-CANCELLATION-SEMANTICS.md).

---

## 4.8 S07 — `use=` inheritance resolver

Implement full source inheritance.

Required behavior:

- resolve a named parent;
- resolve multiple parents;
- recursively resolve parent entries;
- apply authoritative terminfo precedence rules;
- preserve local overrides;
- preserve cancellation semantics;
- detect missing parents;
- detect direct cycles;
- detect indirect cycles;
- limit inheritance depth;
- produce deterministic diagnostics;
- resolve entries independent of dictionary iteration order.

Resolver APIs SHOULD permit both:

- a complete parsed source database;
- a caller-supplied resolution provider.

This allows inheritance to refer eventually to source entries obtained from multiple files or providers.

**Gate S07:** complicated multi-level inheritance graphs resolve reproducibly and cyclic graphs fail cleanly.

**S07 implementation record:** [`docs/1.1.0-S07-USE-INHERITANCE-RESOLVER.md`](docs/1.1.0-S07-USE-INHERITANCE-RESOLVER.md).

---

## 4.9 S08 — `TerminalDescription` materialization

Convert a resolved source entry into the stable runtime model.

The resulting `TerminalDescription` SHALL behave equivalently to one acquired from a compiled database containing the same resolved entry.

No source-only concepts SHALL leak into `TerminalDescription`.

Useful APIs may include concepts equivalent to:

```csharp
TermInfoSourceParser
TermInfoSourceDocument
TermInfoSourceEntry
TermInfoSourceResolver
TermInfoSourceDiagnostic
```

Exact public names SHALL receive an API-regret review before release.

**Gate S08:** source and compiled forms of equivalent fixtures produce semantically equivalent `TerminalDescription` instances.

**S08 implementation record:** [`docs/1.1.0-S08-TERMINAL-DESCRIPTION-MATERIALIZATION.md`](docs/1.1.0-S08-TERMINAL-DESCRIPTION-MATERIALIZATION.md).

---

## 4.10 S09 — Corpus, fuzzing, and compatibility

Build an authoritative source corpus.

Tests SHOULD include:

- simple System V entries;
- ncurses source entries;
- extended capabilities;
- unusual escapes;
- cancellation;
- deep but valid inheritance;
- malformed inheritance;
- duplicate entries;
- duplicate aliases;
- malformed source;
- resource-limit attacks.

Optional differential tests MAY compare resolution with `tic`/`infocmp`.

Normal CI SHALL remain deterministic and SHALL NOT require host ncurses installation.

**1.1 completion gate:** arbitrary supported `.ti` source can be parsed and resolved into the same semantic runtime model used by the existing 1.0 compiled-term acquisition path.

**S09 implementation record:** [`docs/1.1.0-S09-CORPUS-FUZZING-COMPATIBILITY.md`](docs/1.1.0-S09-CORPUS-FUZZING-COMPATIBILITY.md).

## 4.11 1.1.0 release closure

S01-S09 constitute the complete planned implementation program for 1.1.0. The
final release step is deliberately a closure gate rather than another feature
tranche.

Release closure SHALL:

- set both package versions to exactly `1.1.0`;
- retain assembly version `1.0.0.0` for both assemblies;
- preserve the frozen runtime 1.0 public API baseline;
- freeze the reviewed Source public API baseline;
- pass `net8.0` / `net10.0` API-equivalence checks;
- pass the complete Windows/Linux/macOS solution matrix;
- pack and validate both NuGet packages and symbol packages;
- execute fresh runtime and Source package consumers for both targets;
- review final package README, license, repository, and dependency metadata;
- publish the exact validated artifacts to NuGet.org and GitHub Packages;
- tag the exact validated/published commit as `v1.1.0`.

**Release audit:** [`docs/1.1.0-RELEASE-AUDIT.md`](docs/1.1.0-RELEASE-AUDIT.md).

---

# 5. Version 1.2.0 — Terminfo Compiler

## 5.1 Release objective

`1.2.0` SHALL implement the inverse of the existing compiled-entry reader.

Introduce:

```text
Icod.TermInfo.Compiler
    -> Icod.TermInfo
```

At C01, the Compiler package depends only on `Icod.TermInfo`. C05 MAY add:

```text
Icod.TermInfo.Compiler
    -> Icod.TermInfo.Source
        -> Icod.TermInfo
```

when source compilation becomes a compiler responsibility. The runtime package
SHALL remain dependency-free and SHALL NOT acquire a dependency on Source or
Compiler.

The core writer SHOULD also be capable of compiling an already-resolved `TerminalDescription`.

The pre-C01 architectural and representation contract is frozen by
[`docs/1.2.0-PRE-C01-CONTRACT-AUDIT.md`](docs/1.2.0-PRE-C01-CONTRACT-AUDIT.md).

---

## 5.2 C01 — Compiler package foundation and binary writer contract

**Development version:** `1.2.0-Alpha-1`

Create the compiler package and establish the deterministic low-level writer.

Required package-foundation work:

- add `Icod.TermInfo.Compiler`;
- add `tests/Icod.TermInfo.Compiler.Tests`;
- target `net8.0;net9.0;net10.0`;
- use C# 13;
- reference `Icod.TermInfo`;
- add the projects to `Icod.TermInfo.sln`;
- establish Compiler package metadata, README, icon, license, and Source Link;
- establish `docs/1.2.0-COMPILER-PUBLIC-API-BASELINE.txt`;
- extend public-API snapshotting and cross-target API comparison to Compiler;
- add a package-reference-only Compiler smoke consumer;
- extend package verification and both CI workflows to the Compiler package;
- coordinate all three package versions at `1.2.0-Alpha-1`;
- retain assembly version `1.0.0.0` for all three assemblies.

Before the first C01 merge to `main`, the NuGet.org trusted-publishing policy
SHALL authorize the `Icod.TermInfo.Compiler` package ID in addition to the
existing runtime and Source package IDs.

Define a deterministic compiled-entry writer.

The preferred low-level ownership is conceptually symmetrical with the reader:

```text
CompiledTermInfoParser
    bytes -> TerminalDescription

CompiledTermInfoWriter
    TerminalDescription -> bytes
```

Exact public signatures SHALL receive an API-regret review during C01. A broad
format-policy public surface SHOULD NOT be frozen before C04 requires it.

The C01 writer SHALL be pure:

- no filesystem access;
- no environment-variable discovery;
- no process-global state;
- no native `tic`, `infocmp`, or ncurses dependency;
- no database-layout policy.

C01 SHALL implement the minimal conventional legacy `0432` representation
needed to prove the writer contract. The public design SHALL leave room for the
already-supported reader family:

- legacy `0432`;
- wide-numeric `01036`;
- ncurses extended sections.

All compiled integers SHALL be emitted little-endian according to the frozen
0.9 reader contract.

Identity and capability strings SHALL use a strict reversible Latin-1 mapping.
The writer SHALL reject managed characters which cannot be represented as one
byte and SHALL reject embedded NUL where the binary format uses NUL
termination. It SHALL NOT silently replace characters.

The writer SHALL NOT synthesize identity metadata merely to make a description
encodable. In particular, the currently supported compiled names contract
requires a verbose description. A `TerminalDescription` without one is not
losslessly representable and SHALL fail deterministically unless authoritative
format evidence later establishes an equivalent representation.

`TerminalDescription` exposes effective values, not source cancellation
tombstones. The writer SHALL therefore emit absence for absent effective
capabilities and SHALL NOT invent compiled cancellation sentinels.

No unsupported format SHALL be emitted by guesswork.

**Gate C01:** a minimal representable `TerminalDescription` can be serialized
as deterministic legacy `0432` bytes and parsed by the existing
`CompiledTermInfoParser` into a semantically equivalent description, while the
new package/API/package-consumer gates pass on all three target frameworks.

**Implementation record:** [`docs/1.2.0-C01-COMPILER-PACKAGE-FOUNDATION.md`](docs/1.2.0-C01-COMPILER-PACKAGE-FOUNDATION.md)

---

## 5.3 C02 — Standard capability emission

**Development version:** `1.2.0-Alpha-2`

Implement:

- names section;
- Boolean table;
- numeric table;
- string offset table;
- string table;
- required alignment;
- absent values;
- deterministic omission/truncation of trailing absent standard positions;
- strict representation validation;
- correct legacy integer width and little-endian encoding.

Capability ordering SHALL derive from the canonical 1.0 metadata.

Standard tables SHALL derive exclusively from
`StandardCapabilityCatalog.BinaryIndex`; managed enum ordinal values SHALL NOT
be used as compiled positions.

Present numeric values which collide with compiled sentinel semantics or fall
outside the selected representation SHALL fail deterministically. String-table
offsets and section sizes SHALL be checked before narrowing to binary field
widths. Silent truncation and wraparound are forbidden.

**Gate C02:** standard-only entries round-trip exactly at the semantic level.

**Implementation record:** [`docs/1.2.0-C02-STANDARD-CAPABILITY-EMISSION.md`](docs/1.2.0-C02-STANDARD-CAPABILITY-EMISSION.md)

---

## 5.4 C03 — Extended capability emission

**Development version:** `1.2.0-Alpha-3`

Write the supported ncurses extended-capability representation.

Include:

- extended Booleans;
- extended numbers;
- extended strings;
- extended names;
- offsets;
- alignment;
- overflow validation.

Extended capabilities SHALL be emitted deterministically. Within each
Boolean/numeric/string kind, names SHALL be ordered using ordinal,
case-sensitive comparison rather than dictionary enumeration order.

Extended capability names and string values SHALL follow the same strict
Latin-1, NUL-termination, offset-width, and overflow rules as the conventional
sections.

**Gate C03:** all extended capability kinds survive writer → parser round-trip.

**Implementation record:** [`docs/1.2.0-C03-EXTENDED-CAPABILITY-EMISSION.md`](docs/1.2.0-C03-EXTENDED-CAPABILITY-EMISSION.md)

---

## 5.5 C04 — Format selection

**Development version:** `1.2.0-Alpha-4`

Introduce explicit writer policy.

The compiler SHALL determine or accept:

- legacy format where sufficient;
- wide numeric format where required;
- extended section inclusion;
- deterministic failure if requested representation cannot encode the description.

Silent truncation SHALL be forbidden.

Automatic selection SHALL prefer the narrow conventional representation when it
is sufficient and select `01036` only when a representable numeric value
requires the wide form. Explicit format requests SHALL either produce that
format exactly or fail; they SHALL NOT silently upgrade or downgrade.

Representation validation SHALL include:

- strict Latin-1 identity, aliases, descriptions, capability names, and strings;
- NUL-terminated-field constraints;
- names-section separator constraints;
- legacy versus wide numeric range;
- compiled absent/canceled sentinel collisions;
- signed 16-bit string/name offset limits;
- extended section count and byte-size limits;
- checked total-entry-size arithmetic.

The writer SHALL distinguish an invalid `TerminalDescription` argument from a
valid description which cannot be represented by the requested compiled format.
The exact public exception/options surface SHALL be frozen here if C01-C03 have
not already required it.

**Gate C04:** boundary numeric/string/count cases select or reject formats predictably.

**Implementation record:** [`docs/1.2.0-C04-FORMAT-SELECTION.md`](docs/1.2.0-C04-FORMAT-SELECTION.md)

---

## 5.6 C05 — Source compiler engine

**Development version:** `1.2.0-Alpha-5`

Compose:

```text
.ti source
    ↓
source parser
    ↓
inheritance resolution
    ↓
TerminalDescription
    ↓
compiled writer
```

The engine SHALL support multiple source entries and dependency ordering.

Compilation diagnostics SHALL preserve source locations from the 1.1 parser.

This is the earliest tranche at which `Icod.TermInfo.Compiler` SHOULD acquire a
dependency on `Icod.TermInfo.Source`. The dependency SHALL remain one-way:

```text
Compiler -> Source -> Runtime
Compiler ------------> Runtime
```

Source parsing and inheritance SHALL NOT be duplicated in Compiler.

Because `TerminalDescription` contains only effective values, source
cancellation tombstones SHALL NOT be reconstructed after materialization.
Compiler output is required to preserve effective terminal semantics, not
incidental source-only state.

**Gate C05:** a multi-entry source file with `use=` inheritance compiles into independently loadable entries.

---

## 5.7 C06 — Database-layout writer

**Development version:** `1.2.0-Alpha-6`

Add controlled output suitable for conventional terminfo directory layouts.

The writer SHALL:

- derive canonical destination paths safely;
- prevent path traversal;
- support explicit output roots;
- create required directory hierarchy;
- define overwrite behavior;
- avoid partial files on failed compilation;
- use atomic replacement where practical.

No process-global installation shall occur merely by compiling a description.

**Gate C06:** a temporary database produced by the compiler can be consumed by the existing directory provider.

---

## 5.8 C07 — Round-trip and differential validation

**Development version:** `1.2.0-Alpha-7`

The principal invariant becomes:

```text
source
   ↓
parse/resolve
   ↓
TerminalDescription A
   ↓
write compiled
   ↓
read compiled
   ↓
TerminalDescription B

A == B semantically
```

The writer SHALL additionally be deterministic: the same semantic input and
writer options SHALL produce byte-for-byte identical output independent of
dictionary iteration order, process execution, host operating system, or
culture.

Validation SHALL reuse the checked-in 0.9 binary corpus and its pinned
ncurses/`tic` provenance. Ordinary CI SHALL remain independent of a host ncurses
installation.

Tests SHALL include:

- legacy `0432` entries;
- `01036` wide numerics;
- ncurses extended sections;
- alignment boundaries;
- absent capabilities and sparse standard tables;
- high Latin-1 bytes;
- unrepresentable Unicode;
- embedded NUL rejection;
- sentinel numeric collisions;
- string/name offset boundaries;
- oversized and overflow-producing descriptions;
- deterministic extended-capability ordering;
- source → resolve → write → parse semantic equivalence;
- temporary database output consumed through the existing directory provider.

Optional ncurses comparison MAY additionally verify emitted data against `tic`.

**1.2 completion gate:** Icod can compile supported terminfo source into conventional compiled database entries which its existing 1.0 runtime parser reads without semantic loss.

**Implementation record:** [`docs/1.2.0-C07-ROUND-TRIP-DIFFERENTIAL-VALIDATION.md`](docs/1.2.0-C07-ROUND-TRIP-DIFFERENTIAL-VALIDATION.md)

## 5.9 1.2.0 release closure

C01-C07 constitute the planned 1.2 implementation program. Publication is a
separate closure gate rather than an additional feature tranche.

Release closure SHALL:

- set `Icod.TermInfo`, `Icod.TermInfo.Source`, and
  `Icod.TermInfo.Compiler` package versions to exactly `1.2.0`;
- retain assembly version `1.0.0.0` for all three assemblies;
- preserve the frozen runtime 1.0 public API baseline;
- preserve the frozen Source 1.1 public API baseline;
- freeze the reviewed Compiler public API baseline;
- pass `net8.0` / `net9.0` / `net10.0` API-equivalence checks for all three packages;
- pass the complete Windows/Linux/macOS solution matrix;
- pack and structurally validate all three NuGet and symbol packages;
- execute fresh runtime, Source, and Compiler package consumers on all three targets;
- verify the one-way dependency graph;
- pass the non-publishing Release validation workflow on the exact `main` commit;
- create and push immutable tag `v1.2.0` for that validated commit;
- require the tag workflow to repeat the complete Release gate on the tagged
  commit before publishing the validated artifacts to NuGet.org and GitHub
  Packages and creating the GitHub Release.

**Release audit:** [`docs/1.2.0-RELEASE-AUDIT.md`](docs/1.2.0-RELEASE-AUDIT.md)

---

# 6. Version 1.3.0 — Inspection and Comparison

## 6.1 Release objective

`1.3.0` SHALL provide the reusable engine underlying `infocmp`-style diagnostics.

It is an API/tooling release, not yet principally a command-line release.

---

## 6.2 I01 — Canonical source renderer

Render a `TerminalDescription` into deterministic `.ti`-style source.

Required considerations:

- canonical name;
- aliases;
- description;
- canonical capability ordering;
- extended capability ordering;
- correct source escaping;
- stable wrapping;
- deterministic output.

The renderer SHALL favor reproducibility over preserving incidental formatting from the original source.

**Gate I01:** parse → resolve → canonical-render → parse produces semantic equivalence.

---

## 6.3 I02 — Unresolved-source rendering

Where a `TermInfoSourceEntry` is available, support rendering source while preserving meaningful unresolved constructs such as:

- `use=`;
- cancellation;
- aliases;
- descriptions.

Exact comment/whitespace preservation is optional unless deliberately adopted.

**Gate I02:** inheritance-bearing source can be inspected without forcing it into a flattened representation.

---

## 6.4 I03 — Semantic comparison engine

Introduce comparison independent of textual representation.

Differences SHALL identify categories such as:

```text
only in left
only in right
same capability / different value
cancelled versus present
standard versus extended
alias/name metadata difference
```

Comparison SHOULD operate directly on `TerminalDescription`.

**Gate I03:** two descriptions can be compared without converting either to source text.

---

## 6.5 I04 — Structured difference model

Expose differences in a machine-readable representation suitable for:

- command output;
- tests;
- diagnostics;
- future JSON output;
- IDE/tool integration.

Formatting SHALL remain separate from comparison.

**Gate I04:** tests assert semantic differences without parsing human-readable text.

---

## 6.6 I05 — Provider-aware inspection

Allow inspection of descriptions acquired through the existing provider model.

Useful scenarios include:

```text
built-in xterm vs system xterm
system xterm vs supplied database xterm
xterm-256color vs screen-256color
two separate database roots
```

**Gate I05:** comparison does not depend on a particular acquisition source.

---

## 6.7 I06 — `infocmp` engine

Compose acquisition, rendering, and comparison into a reusable service layer.

The engine SHALL remain usable independently of a console application.

**1.3 completion gate:** callers can inspect, decompile, and semantically compare terminfo descriptions through managed APIs.

---

# 7. Version 1.4.0 — Tool Suite

The detailed 1.4 tranche contract is maintained in
[`Icod.TermInfo-1.4.0-Tool-Suite-Roadmap.md`](Icod.TermInfo-1.4.0-Tool-Suite-Roadmap.md).

## 7.1 Release objective

`1.4.0` SHALL expose the source/compiler/inspection functionality as Unix-style command-line utilities.

Command projects:

```text
tic/Icod.TermInfo.Tic.csproj
infocmp/Icod.TermInfo.InfoCmp.csproj
toe/Icod.TermInfo.Toe.csproj
```

Assemblies/executables SHOULD expose conventional command names where packaging permits:

```text
tic
infocmp
toe
```

The tools SHOULD use the established Icod command-entry-point conventions.

---

## 7.2 Tranche sequence

The earlier coarse T01-T05 sketch is superseded by the detailed 1.4 roadmap.
The active implementation sequence is:

| Tranche | Development version | Gate |
|---|---|---|
| T01 | `1.4.0-Alpha-1` | command shells, command contracts, tests, version/CI integration |
| T02 | `1.4.0-Alpha-2` | reusable database-location discovery seam |
| T03 | `1.4.0-Alpha-3` | deterministic conventional database catalog |
| T04 | `1.4.0-Alpha-4` | `tic` validation/check-only path |
| T05 | `1.4.0-Alpha-5` | `tic` compiled database publication |
| T06 | `1.4.0-Alpha-6` | `infocmp` one-entry inspection/rendering |
| T07 | `1.4.0-Alpha-7` | `infocmp` semantic comparison |
| T08 | `1.4.0-Alpha-8` | `toe` conventional database listing |
| T09 | `1.4.0-Alpha-9` | `toe` source dependency analysis |
| T10 | `1.4.0-Alpha-10` | CLI compatibility and distribution hardening |
| T11 | `1.4.0-Alpha-11` | differential validation, hostile-input audit, API/command freeze |

The detailed requirements, compatibility decisions, and gates for every tranche
are authoritative in `Icod.TermInfo-1.4.0-Tool-Suite-Roadmap.md`.

T01 specifically SHALL remain structural: it creates the three command projects
and their tests, adopts the `Icod.CommandFramework 2.0.0` command-host contract,
coordinates the four library packages at `1.4.0-Alpha-1`, and proves dependency
direction without implementing operational terminfo command behavior.

T02 and T03 establish the reusable Inspection discovery/catalog foundations. T04
introduces the first operational command semantics: `tic -c` reads strict UTF-8
terminfo source, parses the complete document, applies optional source-entry
selection, resolves selected inheritance graphs, enforces the command's `-x`
unknown-extension policy, and validates compiled representability entirely in
memory. T05 adds the filesystem write path through the frozen
`CompiledTermInfoDatabaseWriter`, keeps destination and overwrite policy in the
command layer, and preserves the T04 check-only path as non-mutating. T06 makes
`infocmp` operational for zero/one-terminal acquisition and adds reviewed additive
Inspection renderer controls for layout, width, metadata ordering, and extended-
capability filtering while preserving the frozen 1.3 renderer overload output.
T07 adds first-versus-each-subsequent semantic comparison to `infocmp`, delegates
difference detection to the frozen `TerminalDescriptionComparer`, adds common and
absent-standard capability reports, and keeps all comparison policy in the command
layer without changing the active Inspection public API baseline.

**1.4 completion gate:** `tic`, `infocmp`, and `toe` are useful standalone
managed utilities on Windows, Linux, and macOS, backed by the same reusable
libraries used by application consumers, with the frozen lower-layer contracts
and explicitly reviewed 1.4 Inspection additions preserved.

---

# 8. Version 1.6.0 — Termcap Interoperability

The detailed and authoritative 1.6 tranche contract is maintained in
[`Icod.TermInfo-1.6.0-Termcap-Interoperability-Roadmap.md`](Icod.TermInfo-1.6.0-Termcap-Interoperability-Roadmap.md).

## 8.1 Release objective

`1.6.0` adds historical termcap compatibility as a separate opt-in reusable
package without contaminating the primary terminfo model or enlarging the frozen
Runtime, Source, Compiler, or Inspection APIs.

The stable package boundary is:

```text
Icod.TermInfo.Termcap
    -> Icod.TermInfo
```

## 8.2 Completed tranche sequence

| Tranche | Development version | Gate |
|---|---|---|
| TC01 | `1.6.0-Alpha-1` | package foundation, unresolved model, bounded parser |
| TC02 | `1.6.0-Alpha-2` | two-character capability metadata and classification |
| TC03 | `1.6.0-Alpha-3` | `tc=` inheritance, cancellation, cycle/depth handling |
| TC04 | `1.6.0-Alpha-4` | termcap → `TerminalDescription` conversion with explicit loss |
| TC05 | `1.6.0-Alpha-5` | reverse representability and deterministic termcap rendering |
| TC06 | `1.6.0-Alpha-6` | explicit opt-in `TERMCAP` / `TERMPATH` acquisition |
| TC07 | `1.6.0-Alpha-7` | conversion tools and coordinated router/archive distribution |
| TC08 | `1.6.0-Alpha-8` | corpus, fuzzing, hostile-input audit, API/package/CLI freeze |

TC01-TC08 are complete. The Termcap public API baseline, Runtime-only dependency,
conversion-command composition, router/archive topology, package verification,
and hostile-input/differential validation are frozen for 1.6.0.

**1.6 completion gate:** common conventional termcap databases can be parsed,
resolved, converted into the canonical Icod terminfo semantic model, rendered
back where representable, acquired explicitly, and exercised through conversion
tools with loss reported rather than hidden.

**Release audit:** [`docs/1.6.0-RELEASE-AUDIT.md`](docs/1.6.0-RELEASE-AUDIT.md).

---

# 9. Later Work — Exotic Storage and Historical Dialects

These features SHALL remain demand-driven.

## 9.1 Berkeley DB / hashed ncurses storage

Implement an optional provider capable of acquiring compiled entry bytes from ncurses hashed databases.

The provider SHOULD feed those bytes into the existing compiled parser.

It SHALL NOT create an independent terminal-description decoder.

Packaging should avoid forcing a Berkeley DB dependency on ordinary consumers.

## 9.2 Historical Unix binary formats

Possible future targets include documented formats from:

- AIX;
- HP-UX;
- OSF/1;
- other commercial Unix variants.

Each format requires:

- authoritative documentation;
- real fixtures;
- explicit format detection;
- independent tests.

Heuristic interpretation of unknown formats is prohibited.

## 9.3 Additional source dialect compatibility

Vendor-specific source extensions MAY be added where:

- real source files exist;
- behavior is documented;
- compatibility does not compromise standard semantics.

---

# 10. Continuous Quality Work

Quality work proceeds throughout all releases rather than waiting for a dedicated version.

## 10.1 Fuzzing targets

Highest-value targets:

1. existing compiled terminfo parser;
2. 1.1 source lexer/parser;
3. 1.1 inheritance resolver;
4. 1.2 compiled writer;
5. parameter expansion;
6. 1.6 termcap parser.

Fuzz failures SHALL become deterministic regression tests.

## 10.2 Differential testing

Where available, development/optional tests MAY compare with:

- ncurses `tic`;
- ncurses `infocmp`;
- known system databases.

Ordinary CI SHALL remain independent of those external programs.

## 10.3 Corpus growth

Maintain test fixtures covering:

- Linux distributions;
- macOS;
- BSDs where available;
- screen;
- tmux;
- xterm families;
- rxvt families;
- Linux console;
- unusual historical descriptions;
- deliberately malformed input.

## 10.4 Resource/security audit

Each release SHALL review:

- integer overflow;
- recursive inheritance;
- allocation bounds;
- malformed offsets;
- string-table limits;
- path handling;
- partial-file writes;
- malicious capability names;
- denial-of-service through pathological source.

---

# 11. Compatibility Policy

The post-1.0 line SHALL preserve this dependency rule:

```text
Application needing terminal descriptions
        |
        v
   Icod.TermInfo
```

Source/tooling functionality is opt-in:

```text
Source consumer
      |
      v
Icod.TermInfo.Source
      |
      v
Icod.TermInfo
```

Compilation is opt-in:

```text
Compiler/tool
      |
      v
Icod.TermInfo.Compiler
      |
      +--> Icod.TermInfo.Source
      |
      +--> Icod.TermInfo
```

No future tooling release SHALL make `Icod.Terminal`, `Icod.DCurses`, or ordinary command-line applications depend on source/compiler packages merely to retrieve terminal capabilities.

---

# 12. Recommended Immediate Sequence

Development should proceed:

```text
1.0 stable runtime
      |
      v
1.1 terminfo source language
      |
      v
1.2 compiled writer / compiler
      |
      v
1.3 inspection / comparison
      |
      v
1.4 executable tic / infocmp / toe suite
      |
      v
1.5 coordinated distribution / icod-terminfo router
      |
      v
1.6 TC01-TC06 termcap interoperability
```

The 1.1 Source, 1.2 Compiler, 1.3 Inspection, 1.4 Tool Suite, and 1.5
Coordinated Distribution lines are complete. The next development target is
**1.6.0 — Termcap Interoperability**, beginning with the TC01 parser foundation
and preserving the frozen Runtime and previously released reusable-library
contracts.
