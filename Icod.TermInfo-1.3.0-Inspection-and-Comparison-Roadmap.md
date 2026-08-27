# Icod.TermInfo 1.3.0 — Inspection and Comparison Roadmap

**Repository:** `uniblab/Icod.TermInfo`  
**Audited branch:** `main`  
**Audited main commit:** `e24e5da8b67acb95b42732a4252de0251f012045`  
**Current published release:** `1.2.0` / tag `v1.2.0`  
**Language:** C# 13  
**Target frameworks:** `net8.0`; `net9.0`; `net10.0`  
**Runtime API contract:** frozen at 1.0  
**Source API contract:** frozen at 1.1  
**Compiler API contract:** frozen at 1.2  
**New package:** `Icod.TermInfo.Inspection`
**Current development version:** `1.3.0-Alpha-4`
**Development sequence:** `1.3.0-Alpha-1` through `1.3.0-Alpha-7`
**Status:** I04 implementation candidate — build/test/package validation required
**Current tranche:** I04 — Effective semantic comparison and structured difference model
**Release objective:** reusable managed inspection, canonical rendering, and semantic-comparison APIs underlying future `infocmp`-style tooling without destabilizing the existing Runtime, Source, or Compiler contracts.

---

# 1. Executive decision

Version 1.3.0 should remain an **API/tooling-engine release**, not yet a command-line
release.

The existing post-1.0 roadmap is directionally correct in identifying canonical
rendering, unresolved source inspection, semantic comparison, structured
differences, provider-aware inspection, and an `infocmp` engine as the 1.3 work.
The audit shows, however, that those features should not be added directly to
`Icod.TermInfo`, `Icod.TermInfo.Source`, or `Icod.TermInfo.Compiler`.

The recommended architecture is a fourth coordinated package:

```text
Icod.TermInfo.Inspection
        |             \
        v              \
Icod.TermInfo.Source    \
        |                \
        v                 v
             Icod.TermInfo
```

Equivalently:

```text
Inspection -> Source -> Runtime
Inspection ----------> Runtime

Compiler   -> Source -> Runtime
Compiler   ------------> Runtime
```

There SHALL be no production dependency between Inspection and Compiler.

`Icod.TermInfo.Inspection.Tests` MAY depend on Compiler for round-trip and
differential validation, but `Icod.TermInfo.Inspection` itself SHOULD NOT.

This preserves the already-published ownership model:

- Runtime owns immutable effective terminal descriptions and acquisition.
- Source owns `.ti` lexical/parsing/inheritance semantics.
- Compiler owns deterministic compiled-entry and database-layout writing.
- Inspection owns canonical human-readable representation, semantic comparison,
  and reusable inspection orchestration.
- Future command-line applications remain a later layer.

---

# 2. Audit of the 1.2.0 baseline

## 2.1 Release health

The 1.2.0 baseline is suitable for beginning 1.3 development.

The audited `main` commit is the 1.2.0 release merge:

```text
e24e5da8b67acb95b42732a4252de0251f012045
```

The non-publishing `main validation` workflow completed successfully for that
commit.

The tag-driven release workflow for `v1.2.0` also completed successfully and
created the GitHub Release with:

- `Icod.TermInfo.1.2.0.nupkg`;
- `Icod.TermInfo.1.2.0.snupkg`;
- `Icod.TermInfo.Source.1.2.0.nupkg`;
- `Icod.TermInfo.Source.1.2.0.snupkg`;
- `Icod.TermInfo.Compiler.1.2.0.nupkg`;
- `Icod.TermInfo.Compiler.1.2.0.snupkg`;
- `SHA256SUMS.txt`.

No open GitHub issues were present at the time of this audit.

## 2.2 Existing public-contract boundaries

The repository now has three intentionally independent public API baselines:

```text
docs/1.0.0-PUBLIC-API-BASELINE.txt
docs/1.1.0-SOURCE-PUBLIC-API-BASELINE.txt
docs/1.2.0-COMPILER-PUBLIC-API-BASELINE.txt
```

That separation has become a useful architectural constraint and should be
preserved.

The 1.3 implementation SHOULD NOT enlarge any of those three frozen public
surfaces merely to make inspection convenient.

In particular:

- do not add rendering APIs to Runtime;
- do not add rendering/comparison APIs to Source;
- do not make Compiler double as a decompiler/inspection library;
- do not change provider interfaces to support 1.3 unless a separately reviewed
  compatibility decision proves it necessary.

## 2.3 Runtime already exposes the right effective model

`TerminalDescription` already exposes:

- canonical name;
- description;
- aliases;
- effectively present standard Boolean capabilities;
- effectively present standard numeric capabilities;
- effectively present standard string capabilities;
- extended capabilities.

Standard capability collections are already normalized into compiled-table
order.

That means an effective-state renderer and comparer can be implemented entirely
outside Runtime.

Extended capabilities remain case-sensitive and therefore MUST be ordered
explicitly by Inspection rather than relying on dictionary enumeration order.

## 2.4 Source already exposes the right unresolved model

`Icod.TermInfo.Source` already exposes:

- `TermInfoSourceDocument`;
- `TermInfoSourceEntry`;
- ordered `TermInfoSourceField` collections;
- field kinds for Boolean, numeric, string, cancellation, disabled, and
  `use=` reference forms;
- aliases and descriptions;
- source spans;
- parser and resolver APIs.

That is enough to support normalized unresolved-source rendering and
source-aware comparison without changing Source.

This distinction is important:

> `TerminalDescription` contains effective state. It cannot tell Inspection
> whether an absent capability was never mentioned, locally cancelled, or
> removed through inheritance.

Therefore 1.3 SHALL distinguish **effective comparison** from **source-aware
comparison**.

## 2.5 Provider boundary

The frozen provider contract is intentionally small:

```text
ITerminalDescriptionProvider.TryLoad(name, out terminal)
```

The existing providers do not expose a common enumeration or provenance API.

Version 1.3 SHOULD therefore support provider-aware inspection by accepting an
explicit provider plus terminal name (and, where useful, a caller-supplied
display label).

Version 1.3 SHALL NOT:

- add provider enumeration merely for inspection;
- attempt to expose private System-provider discovery internals;
- pretend it can identify the exact originating database path when the provider
  contract does not expose that fact.

Provider/database enumeration remains naturally associated with the future
`toe` work.

## 2.6 CI and publication are currently three-package specific

The current PR, main, verifier, and tag-release machinery explicitly handles:

```text
Icod.TermInfo
Icod.TermInfo.Source
Icod.TermInfo.Compiler
```

The release workflow currently expects:

```text
3 .nupkg
3 .snupkg
1 SHA256SUMS.txt
```

A 1.3 Inspection package changes that to:

```text
4 .nupkg
4 .snupkg
1 SHA256SUMS.txt
```

The first 1.3 tranche therefore must update package verification and release
plumbing before feature implementation proceeds.

## 2.7 Active-document drift

`docs/FUTURE-WORK-INVENTORY.md` still contains pre-1.2 language describing the
Compiler as planned work and describing the current foundation primarily in
terms of 1.1.

That should be refreshed as part of the 1.3 foundation work.

Historical C01-C07 implementation records should remain historical records.

`docs/1.2.0-RELEASE-AUDIT.md` was intentionally written as the pre-tag release
gate and still describes the candidate state. It need not be rewritten to
pretend it was authored after publication. The active roadmap and versioning
documents can record that 1.2.0 is now published.

---

# 3. Architectural contract proposed for 1.3

## 3.1 New package

Create:

```text
Icod.TermInfo.Inspection/
    Icod.TermInfo.Inspection.csproj
    README.md
    src/
```

and:

```text
tests/
    Icod.TermInfo.Inspection.Tests/
```

The package SHALL:

- target `net8.0`, `net9.0`, and `net10.0`;
- use C# 13;
- retain assembly version `1.0.0.0`;
- remain unsigned;
- produce XML documentation and portable symbols;
- carry normal repository/license/icon/Source Link metadata;
- directly reference matching `Icod.TermInfo`;
- directly reference matching `Icod.TermInfo.Source`;
- not reference `Icod.TermInfo.Compiler`.

Direct Runtime reference is required because Inspection's public API will expose
Runtime types such as `TerminalDescription` and `ITerminalDescriptionProvider`.

## 3.2 Coordinated package versioning

Beginning with I01, all four package projects SHOULD advance together:

```text
Icod.TermInfo
Icod.TermInfo.Source
Icod.TermInfo.Compiler
Icod.TermInfo.Inspection
```

Recommended tranche versions:

```text
I01  1.3.0-Alpha-1
I02  1.3.0-Alpha-2
I03  1.3.0-Alpha-3
I04  1.3.0-Alpha-4
I05  1.3.0-Alpha-5
I06  1.3.0-Alpha-6
I07  1.3.0-Alpha-7
```

Final release closure changes all four to:

```text
1.3.0
```

All four assemblies retain:

```text
AssemblyVersion 1.0.0.0
```

## 3.3 Inspection public API baseline

Introduce:

```text
docs/1.3.0-INSPECTION-PUBLIC-API-BASELINE.txt
```

During I01-I07 it records the deliberately developing Inspection surface.

At release closure it becomes the frozen 1.3 Inspection contract.

The existing Runtime, Source, and Compiler baselines SHALL remain exact.

## 3.4 Effective versus source-aware domains

The package SHALL expose two conceptually distinct inspection domains.

### Effective domain

Input:

```text
TerminalDescription
```

Can know:

- canonical name;
- aliases;
- description;
- effective standard capabilities;
- effective extended capabilities.

Cannot know:

- original `use=` structure;
- source cancellation tombstones;
- duplicate source fields;
- comments;
- whitespace;
- inheritance provenance.

### Source-aware domain

Input:

```text
TermInfoSourceEntry
TermInfoSourceDocument
```

Can know:

- ordered local fields;
- `use=` references;
- local cancellations;
- disabled fields;
- aliases;
- description;
- source spans.

It still SHALL NOT invent provenance or formatting which the parsed model does
not retain.

This distinction should appear in type names, documentation, tests, and
difference categories.

---

# 4. Canonical rendering contract

## 4.1 Effective rendering

Rendering a `TerminalDescription` produces normalized `.ti`-style source.

The renderer SHALL emit:

1. canonical name;
2. aliases;
3. description when present;
4. standard Boolean capabilities;
5. standard numeric capabilities;
6. standard string capabilities;
7. extended capabilities.

Recommended ordering:

- standard capabilities: `StandardCapabilityCatalog` / compiled-table order
  within each value kind;
- extended capabilities: value kind followed by ordinal, case-sensitive name
  ordering.

The output SHALL be culture-independent and platform-independent.

The renderer SHALL use deterministic source escaping and deterministic wrapping.

It SHALL NOT synthesize:

- `use=`;
- cancellations;
- disabled fields;
- comments;
- inheritance relationships.

Effective absence is rendered as absence.

## 4.2 Source-aware rendering

Rendering a `TermInfoSourceEntry` has a different rule:

> **Field order must be preserved.**

Reordering unresolved source fields can change inheritance/cancellation behavior
or change the meaning of duplicate fields.

The source-aware renderer MAY normalize:

- indentation;
- escaping;
- continuation;
- wrapping;
- line endings.

It SHALL preserve the semantic order and kind of:

- Boolean fields;
- numeric fields;
- string fields;
- cancellation fields;
- disabled fields;
- `use=` references.

Exact comments and incidental whitespace are out of scope for 1.3 unless a
separate deliberate contract is adopted.

## 4.3 Round-trip standards

For effective rendering:

```text
TerminalDescription
    ↓
canonical render
    ↓
Source parse
    ↓
resolve
    ↓
TerminalDescription
```

must be semantically equivalent.

For unresolved rendering:

```text
TermInfoSourceEntry
    ↓
normalized render
    ↓
Source parse
```

must preserve the ordered semantic field sequence and produce equivalent
resolution when evaluated against the same dependency set.

---

# 5. Comparison contract

## 5.1 Effective semantic comparison

Effective comparison operates directly on two `TerminalDescription` values.

It SHALL compare:

- canonical name metadata;
- aliases;
- description;
- standard Boolean presence;
- standard numeric presence/value;
- standard string presence/value;
- extended capability presence;
- extended capability value kind;
- extended capability value.

The result SHALL be structural and machine-readable.

Required difference categories include:

```text
identity metadata difference
only in left
only in right
same capability / different value
extended value-kind mismatch
```

Cancellation SHALL NOT be a category in effective comparison because
`TerminalDescription` does not retain that information.

## 5.2 Source-aware comparison

Source-aware comparison operates on unresolved source entries/documents.

It MAY identify categories such as:

```text
field only in left
field only in right
field kind difference
same capability / different local value
present versus cancelled
present versus disabled
different use= reference
different field sequence
alias/name/description difference
```

Duplicate fields and field ordering SHALL remain observable.

A useful 1.3 invariant is:

> Two source entries may have source-aware differences while resolving to
> identical effective `TerminalDescription` values.

The API should make that distinction straightforward rather than treating it as
an error.

## 5.3 Deterministic difference ordering

Difference order SHALL NOT depend on:

- dictionary insertion order;
- culture;
- process execution;
- operating system.

For effective capability differences, use the same canonical standard ordering
as rendering and ordinal ordering for extended names.

Source-aware differences should follow source order where sequence is
semantically relevant.

## 5.4 Presentation separation

The comparison engine returns structured differences.

Human CLI formatting SHALL remain separate.

This keeps the 1.3 engine useful for:

- tests;
- IDEs;
- future JSON output;
- future CLI output;
- diagnostics;
- programmatic policy checks.

---

# 6. Recommended public API shape

Exact public names should be frozen during I01/I02, but the package should
roughly contain these responsibilities:

```text
TerminalDescriptionSourceRenderer
TermInfoSourceRenderer

TerminalDescriptionComparer
TermInfoSourceComparer

TermInfoComparisonResult
TermInfoDifference
TermInfoDifferenceKind

TermInfoInspectionTarget
TermInfoInspectionEngine
```

Options types may be introduced where genuinely needed for:

- line width/wrapping;
- identity-metadata comparison;
- extended capability inclusion;
- source/effective view selection.

Avoid option flags merely for anticipated future ncurses switches.

The 1.3 API should model stable semantic operations, not mirror an `infocmp`
command-line parser.

---

# 7. Implementation tranches

# I01 — Inspection package foundation and contract freeze

**Development version:** `1.3.0-Alpha-1`

Create the package and make the repository genuinely four-package aware before
adding inspection behavior.

Required work:

- add `Icod.TermInfo.Inspection`;
- add `Icod.TermInfo.Inspection.Tests`;
- add both projects to the solution;
- establish direct dependencies on Runtime and Source;
- explicitly prohibit production dependency on Compiler;
- advance Runtime, Source, Compiler, and Inspection package versions together;
- retain `AssemblyVersion` `1.0.0.0`;
- add Inspection README/package metadata/icon/license/Source Link;
- add initial `docs/1.3.0-INSPECTION-PUBLIC-API-BASELINE.txt`;
- add Inspection package-reference-only smoke consumer;
- add Inspection package verification;
- update Windows and POSIX coordinated release verifiers;
- update PR/main workflows to pack and verify all four packages;
- update `release.yaml` to validate and publish all four packages;
- update GitHub Release asset expectations from six package files to eight;
- update release notes/install instructions for four packages;
- update `docs/VERSIONING.md`;
- update `docs/COMPATIBILITY.md`;
- update `docs/FUTURE-WORK-INVENTORY.md`;
- update active roadmap/releasing documentation;
- add `docs/1.3.0-PRE-I01-CONTRACT-AUDIT.md`.

**Gate I01:** a fourth package with no reverse dependency builds, tests, packs,
passes API-equivalence/package smoke gates on all three TFMs, and is included in
the complete non-publishing and tag-release validation path.

---

# I02 — Canonical effective source renderer

**Development version:** `1.3.0-Alpha-2`

Implement deterministic rendering from `TerminalDescription`.

Required behavior:

- render canonical identity header;
- render standard capabilities from the Runtime metadata catalog;
- render extended capabilities deterministically;
- correctly encode Boolean/numeric/string source forms;
- correctly escape source strings;
- use culture-independent numeric formatting;
- use deterministic line endings and wrapping;
- expose string and `TextWriter` entry points where appropriate;
- document that absent effective capabilities are omitted;
- document that cancellation and inheritance are not reconstructible from
  `TerminalDescription`.

Testing SHALL include:

- built-in profiles;
- T29 compiled fixtures;
- high Latin-1 bytes;
- control characters;
- commas/backslashes/carets and other source-sensitive characters;
- empty/maximum practical capability sets;
- deterministic output across culture and repeated execution.

**Gate I02:** effective parse/resolve/render/parse round trips preserve
`TerminalDescription` semantics for the supported corpus.

**Implementation record:** [`docs/1.3.0-I02-CANONICAL-EFFECTIVE-SOURCE-RENDERER.md`](docs/1.3.0-I02-CANONICAL-EFFECTIVE-SOURCE-RENDERER.md)

---

# I03 — Normalized unresolved-source renderer

**Development version:** `1.3.0-Alpha-3`

Render `TermInfoSourceEntry` / document data without flattening inheritance.

Required behavior:

- preserve canonical name/aliases/description;
- preserve field order;
- preserve `use=` placement;
- preserve cancellation;
- preserve disabled fields;
- preserve duplicate-field ordering;
- normalize source escaping and layout;
- avoid claiming comment/whitespace preservation;
- preserve enough information for equivalent re-resolution.

Tests SHALL emphasize cases where reordering would be incorrect:

- multiple `use=` references;
- local override before/after `use=`;
- cancellation around inherited values;
- duplicate local capabilities;
- mixed standard/extended capabilities.

**Gate I03:** normalized unresolved output reparses into an equivalent ordered
source model and resolves equivalently against the same provider/document.

**Implementation record:** [`docs/1.3.0-I03-NORMALIZED-UNRESOLVED-SOURCE-RENDERER.md`](docs/1.3.0-I03-NORMALIZED-UNRESOLVED-SOURCE-RENDERER.md)

---

# I04 — Effective semantic comparison and structured difference model

**Development version:** `1.3.0-Alpha-4`

Introduce the machine-readable effective comparison engine.

Required work:

- define stable difference/result types;
- compare identity metadata separately from capabilities;
- compare standard capabilities by semantic identity;
- compare extended capabilities by exact case-sensitive name and value kind;
- identify left-only/right-only/value-difference conditions;
- deterministically order differences;
- ensure comparison does not mutate either input;
- make self-comparison produce zero differences.

The API SHALL NOT claim cancellation/provenance differences at this layer.

Testing SHOULD include algebraic invariants:

```text
Compare(x, x) = no differences
Reverse(Compare(a, b)) corresponds to Compare(b, a)
```

with left/right categories appropriately reversed.

**Gate I04:** callers can inspect exact semantic differences without parsing
human-readable renderer output.

**Implementation record:** [`docs/1.3.0-I04-EFFECTIVE-SEMANTIC-COMPARISON.md`](docs/1.3.0-I04-EFFECTIVE-SEMANTIC-COMPARISON.md)

---

# I05 — Source-aware comparison

**Development version:** `1.3.0-Alpha-5`

Add comparison for unresolved source structure.

Required work:

- compare local source fields and field kinds;
- expose cancellation/disabled/present differences;
- expose `use=` reference differences;
- retain duplicate fields and sequence-sensitive differences;
- compare aliases/descriptions/name metadata;
- provide source spans where useful and unambiguous;
- avoid reconstructing information not retained by Source.

Add paired tests where:

```text
source comparison != equal
effective comparison == equal
```

and the inverse where useful.

This tranche should make explicit that "same terminal semantics" and "same
source program" are different questions.

**Gate I05:** callers can choose effective or source-aware comparison and receive
a deterministic structured answer appropriate to that domain.

---

# I06 — Provider-aware inspection and reusable `infocmp` engine

**Development version:** `1.3.0-Alpha-6`

Compose acquisition, rendering, and comparison into a reusable service layer.

Required scenarios:

```text
built-in xterm vs system xterm
system xterm vs explicit directory xterm
two separate directory roots
xterm-256color vs screen-256color
caller-supplied providers
```

The service layer SHOULD support explicit inspection targets containing:

- provider;
- requested terminal name;
- optional caller-owned display/source label.

It SHALL NOT require providers to expose enumeration or hidden provenance.

The service SHOULD make it straightforward to:

- load and inspect one terminal;
- render one effective terminal;
- compare two explicitly acquired terminals;
- retain enough target identity for future CLI diagnostics.

No console parsing/output policy belongs here.

**Gate I06:** an application can implement the core of common `infocmp`
inspection/comparison workflows without duplicating acquisition, rendering, or
comparison logic.

---

# I07 — Differential validation, robustness, and API/package freeze

**Development version:** `1.3.0-Alpha-7`

Close the implementation program with hostile-input, determinism, differential,
and package validation.

Required validation:

## Rendering

- every built-in profile;
- checked-in compiled T29 corpus;
- checked-in Source corpus;
- effective render → parse → resolve equivalence;
- unresolved render → parse structural equivalence;
- deterministic output across repeated runs;
- deterministic output across cultures;
- deterministic output across Windows/Linux/macOS;
- exact escaping edge cases;
- stable wrapping boundaries.

## Comparison

- self-comparison;
- left/right reversal;
- metadata-only differences;
- every standard value kind;
- every extended value kind;
- extended kind mismatch;
- case-sensitive extended names;
- source cancellation and disabled-state differences;
- `use=` ordering and duplicate fields;
- deterministic ordering independent of dictionary insertion.

## Differential evidence

Reuse or extend the pinned ncurses corpus.

A checked-in `infocmp`-derived reference corpus MAY be added, but ordinary CI
SHALL remain independent of a host ncurses installation.

Differential validation SHOULD compare semantic results, not claim byte-for-byte
format identity with ncurses unless that exact formatting has deliberately been
made part of the Icod contract.

## Package/release validation

- freeze `docs/1.3.0-INSPECTION-PUBLIC-API-BASELINE.txt`;
- require net8/net9/net10 API equivalence for Inspection;
- preserve exact Runtime/Source/Compiler baselines;
- verify the four-package dependency graph;
- run all four isolated package consumers on all three TFMs;
- verify all four package/symbol artifacts;
- verify PR/main workflows remain non-publishing;
- verify tag workflow publishes all four packages and creates nine release
  assets total: eight package/symbol files plus `SHA256SUMS.txt`.

**Gate I07:** the Inspection API is deterministic, cross-platform, corpus-backed,
package-valid, and ready to freeze for 1.3.0.

---

# 8. 1.3.0 release closure

I01-I07 constitute the planned 1.3 implementation program.

Release closure is a separate finalization step.

Release closure SHALL:

- set Runtime, Source, Compiler, and Inspection versions to exactly `1.3.0`;
- retain assembly version `1.0.0.0` for all four assemblies;
- preserve the exact Runtime 1.0 API baseline;
- preserve the exact Source 1.1 API baseline;
- preserve the exact Compiler 1.2 API baseline;
- freeze the Inspection 1.3 API baseline;
- pass net8/net9/net10 API equivalence for all four packages;
- pass the complete Windows/Linux/macOS Release matrix;
- pack and structurally validate four NuGet packages and four symbol packages;
- execute fresh Runtime, Source, Compiler, and Inspection package consumers on
  all three targets;
- verify dependency direction;
- pass the non-publishing `main` validation workflow on the exact release
  commit;
- create and push immutable tag `v1.3.0`;
- require the tag workflow to repeat the full Release gate before publication;
- publish the exact validated packages to NuGet.org and GitHub Packages;
- create the GitHub Release with eight package/symbol artifacts plus
  `SHA256SUMS.txt`.

The NuGet trusted-publishing policy must include:

```text
Icod.TermInfo
Icod.TermInfo.Source
Icod.TermInfo.Compiler
Icod.TermInfo.Inspection
```

before `v1.3.0` is pushed.

---

# 9. Explicit 1.3 non-goals

Version 1.3 SHALL NOT include:

- `tic` command-line application;
- `infocmp` command-line application;
- `toe` command-line application;
- command-line compatibility switch emulation;
- provider/database enumeration solely for `toe`;
- termcap parsing or conversion;
- hashed/Berkeley DB provider support;
- new historical binary dialects;
- Source parser syntax expansion unrelated to inspection;
- Compiler binary-format expansion unrelated to inspection;
- reconstruction of source inheritance/cancellation from a flattened
  `TerminalDescription`;
- exact comment/whitespace round-trip source editing;
- live terminal session ownership;
- keyboard/mouse/input decoding;
- PTY/ConPTY support;
- curses/virtual-screen behavior;
- terminal emulation.

Those remain later or sibling work.

---

# 10. Relationship to 1.4

A successful 1.3 should make 1.4 substantially mechanical.

Expected 1.4 dependencies:

```text
tic
    -> Icod.TermInfo.Compiler
    -> Icod.TermInfo.Source
    -> Icod.TermInfo

infocmp
    -> Icod.TermInfo.Inspection
    -> Icod.TermInfo.Source
    -> Icod.TermInfo

toe
    -> provider/enumeration work
    -> Icod.TermInfo

shared CLI policy, if needed
    -> Icod.CommandFramework
```

The important boundary is that 1.4 command-line parsing, exit-code policy, and
console formatting should sit above the 1.3 Inspection engine rather than being
embedded into it.

---

# 11. Recommended first implementation step

Create a `1.3.0` working branch from the current `main` release commit and begin
with I01 only.

I01 should deliberately avoid implementing the canonical renderer yet.

Its purpose is to freeze:

1. package ownership;
2. four-package coordinated versioning;
3. dependency direction;
4. public API baseline mechanics;
5. solution/test/package layout;
6. CI/release support for the fourth package;
7. effective-versus-source-aware inspection semantics;
8. the rule that Inspection has no production Compiler dependency.

Once I01 is green locally and in GitHub Actions, proceed to I02.

That keeps 1.3 development incremental and gives every later feature tranche a
stable package and release environment.

---

# 12. Audit basis

This roadmap was prepared from the `main` branch after publication of 1.2.0,
including review of:

- `Icod.TermInfo-Post-1.0-Development-Roadmap.md`;
- `docs/VERSIONING.md`;
- `docs/COMPATIBILITY.md`;
- `docs/FUTURE-WORK-INVENTORY.md`;
- `docs/1.0.0-PUBLIC-API-BASELINE.txt`;
- `docs/1.1.0-SOURCE-PUBLIC-API-BASELINE.txt`;
- `docs/1.2.0-COMPILER-PUBLIC-API-BASELINE.txt`;
- `docs/1.2.0-RELEASE-AUDIT.md`;
- `src/TerminalDescription.cs`;
- `src/Providers/SystemTerminalDescriptionProvider.cs`;
- `Icod.TermInfo.Source/src`;
- `Icod.TermInfo.Compiler/src`;
- `Icod.TermInfo.sln`;
- `.github/workflows/pr-build-and-test.yaml`;
- `.github/workflows/push-main.yaml`;
- `.github/workflows/release.yaml`;
- `.github/scripts/verify-release-package.sh`;
- the successful `main` validation for the 1.2.0 release commit;
- the successful tag-driven `v1.2.0` release workflow and release assets;
- current open GitHub issues.

