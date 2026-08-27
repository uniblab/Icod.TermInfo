# Icod.TermInfo Post-1.0 Development Roadmap

**Project:** `Icod.TermInfo`  
**Stable runtime package:** `Icod.TermInfo`  
**Optional source package:** `Icod.TermInfo.Source`
**Optional compiler package:** `Icod.TermInfo.Compiler`
**Language:** C# 13  
**Target frameworks:** `net8.0`; `net9.0`; `net10.0`
**Frozen runtime contract:** `1.0.0`
**Current release version:** `1.2.0`
**Current development version:** `1.2.0`
**Next development line:** `1.3.0`
**Status:** 1.2.0 release closure
**Current tranche:** Final 1.2.0 release closure
**Primary objective:** Extend the terminfo ecosystem beyond runtime capability acquisition without destabilizing the frozen 1.0 runtime contract.

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
                         |
                         v
              Icod.TermInfo.Compiler
                    /           \
                   v             v
      Icod.TermInfo.Source ---> Icod.TermInfo
                               stable runtime
```

The arrows are dependency arrows. `Icod.TermInfo` remains dependency-free.
`Icod.TermInfo.Source` depends on `Icod.TermInfo`. The compiler depends directly
on `Icod.TermInfo`; its Source dependency is introduced only when source
compilation enters the compiler in C05. No dependency may point from the runtime
package back toward Source, Compiler, or Tools.

Live-terminal state, terminal input, probing, PTYs, curses presentation, and terminal emulation remain outside this roadmap and belong to their respective packages.

---

## 2. Version Sequence

| Version | Theme | Outcome |
|---|---|---|
| **1.1.0** | Terminfo source language | Parse and resolve `.ti` source into `TerminalDescription` |
| **1.2.0** | Terminfo compiler | Write conventional compiled terminfo entries; provide the `tic` engine |
| **1.3.0** | Inspection/comparison | `infocmp` engine, canonical source rendering, semantic comparison |
| **1.4.0** | Tool suite | Actual `tic`, `infocmp`, and `toe` command projects |
| **1.5.0** | Termcap interoperability | Parse, resolve, and convert termcap and terminfo |
| **later** | Exotic storage/formats | Berkeley DB provider and historical Unix dialects as justified |

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

## 7.1 Release objective

`1.4.0` SHALL expose the source/compiler/inspection functionality as Unix-style command-line utilities.

Likely projects:

```text
tools/
    Icod.TermInfo.Tic
    Icod.TermInfo.InfoCmp
    Icod.TermInfo.Toe
```

Assemblies/executables SHOULD expose conventional command names where packaging permits:

```text
tic
infocmp
toe
```

The tools SHOULD use the established Icod command-entry-point conventions.

---

## 7.2 T01 — Shared tooling foundation

If substantial CLI behavior is shared, introduce a narrowly scoped tooling library rather than putting command policy into the runtime packages.

Potential package/project:

```text
Icod.TermInfo.Tools
```

It MAY depend on:

```text
Icod.CommandFramework
Icod.TermInfo
Icod.TermInfo.Source
Icod.TermInfo.Compiler
```

The lower-level TermInfo packages SHALL NOT depend on it.

---

## 7.3 T02 — `tic`

Implement the command-line compiler.

Initial scope SHOULD include:

- one or more input files;
- standard input;
- output-directory selection;
- syntax/semantic diagnostics;
- compile-only validation where appropriate;
- deterministic exit statuses;
- safe overwrite behavior;
- extended capability support.

Command-line compatibility SHALL be documented explicitly rather than claimed wholesale.

**Gate:** useful mainstream `tic` workflows operate without native ncurses.

---

## 7.4 T03 — `infocmp`

Implement inspection and comparison.

Initial scope SHOULD include:

- inspect named terminal;
- render canonical source;
- compare two terminals;
- choose explicit provider/database roots;
- show extended capabilities;
- deterministic output ordering.

Additional ncurses-compatible switches may be introduced tranche by tranche.

**Gate:** common `infocmp` diagnostic workflows operate entirely on managed TermInfo APIs.

---

## 7.5 T04 — `toe`

Implement terminal-table enumeration.

The tool SHALL enumerate descriptions from provider/database sources without duplicating provider discovery logic.

Potential output:

- canonical names;
- aliases;
- descriptions;
- source/database identity where requested.

**Gate:** conventional directory-based terminfo stores can be enumerated reliably.

---

## 7.6 T05 — Cross-platform packaging

Validate tools on:

- Windows;
- Linux;
- macOS.

Tools SHALL avoid assuming:

- `/usr/share/terminfo`;
- POSIX path syntax;
- a native ncurses installation.

**1.4 completion gate:** `tic`, `infocmp`, and `toe` are useful standalone managed utilities backed by the same libraries used by application consumers.

---

# 8. Version 1.5.0 — Termcap Interoperability

## 8.1 Release objective

`1.5.0` SHALL add historical termcap compatibility without contaminating the primary terminfo model.

Termcap SHALL be treated as an interoperability/source family.

---

## 8.2 TC01 — Termcap source parser

Support conventional termcap entry syntax, including:

- names and aliases;
- Boolean values;
- numeric values;
- string values;
- cancellation;
- continuation;
- inheritance/reference mechanisms;
- source diagnostics.

Input limits and recursion limits SHALL match the defensive posture of the terminfo source parser.

---

## 8.3 TC02 — Termcap semantic model

Do not force termcap's unresolved syntax directly into `TerminalDescription`.

Use an intermediate representation where necessary, followed by explicit conversion.

Capability mapping SHALL be centralized and testable.

---

## 8.4 TC03 — Termcap → terminfo conversion

Implement controlled semantic conversion.

The converter SHALL identify:

- exact mappings;
- approximations;
- unsupported termcap capabilities;
- terminfo capabilities with no termcap equivalent;
- lossy conversion.

Loss SHALL NOT be silently hidden.

---

## 8.5 TC04 — Terminfo → termcap conversion

Provide reverse conversion where representable.

The API SHALL report when a `TerminalDescription` cannot be faithfully expressed using termcap.

---

## 8.6 TC05 — Environment/search compatibility

Optionally support explicit termcap-style acquisition semantics such as:

- `TERMCAP`;
- `TERMPATH`;

without changing the default 1.0 terminfo discovery contract.

Any runtime provider SHOULD be explicitly opt-in.

---

## 8.7 TC06 — Conversion tools

Add command functionality equivalent in purpose to:

```text
captoinfo
infotocap
```

These may be separate executable projects or modes over the shared tooling engine.

**1.5 completion gate:** common termcap databases can be parsed and converted into the canonical Icod terminfo semantic model with loss explicitly reported.

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
6. 1.5 termcap parser.

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
1.1 S01  source package foundation
      |
      v
1.1 S02-S04
lexer + values + unresolved model
      |
      v
1.1 S05-S07
capability mapping + cancellation + use=
      |
      v
1.1 S08-S09
TerminalDescription materialization + corpus
      |
      v
1.1.0
      |
      v
1.2 binary writer / tic engine
      |
      v
1.3 inspection / infocmp engine
      |
      v
1.4 executable tool suite
      |
      v
1.5 termcap interoperability
```

The first implementation target should therefore be **1.1 S01 — `Icod.TermInfo.Source` package foundation**, followed by the lexer/source-location layer. This gives the project a clean new development line while leaving the stable runtime and the ongoing Terminal/DCurses work untouched.
