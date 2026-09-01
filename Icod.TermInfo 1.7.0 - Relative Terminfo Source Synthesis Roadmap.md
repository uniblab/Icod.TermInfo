# Icod.TermInfo 1.7.0 — Relative Terminfo Source Synthesis Roadmap

**Project:** `Icod.TermInfo`  
**Release:** `1.7.0`  
**Theme:** Relative Terminfo Source Synthesis  
**Published baseline:** `1.6.1`  
**Repository baseline:** `main` at `9a1286a3d477a8b0056a3bbfe31502c510c408e7`  
**Primary reusable package:** `Icod.TermInfo.Inspection`  
**Primary command:** `infocmp`  
**Target frameworks:** reusable libraries `net8.0`; `net9.0`; `net10.0`; commands `net10.0`  
**Reusable assembly identity:** retain `1.0.0.0` unless an independently justified compatibility break is discovered  
**Planned development sequence:** `1.7.0-Alpha-1` through `1.7.0-Alpha-8`, then stable `1.7.0`  
**Status:** RS06 implementation in progress — `infocmp -u` relative synthesis
**Primary objective:** synthesize deterministic terminfo source for a target `TerminalDescription` relative to an explicit ordered set of parent descriptions, then expose that capability through `infocmp -u`.

---

## 1. Release Thesis

`Icod.TermInfo 1.7.0` will add the missing inverse operation between effective terminal state and reusable terminfo inheritance.

The suite can already:

```text
terminfo source
    -> TermInfoSourceParser
    -> TermInfoSourceResolver
    -> TerminalDescription

TerminalDescription
    -> TerminalDescriptionSourceRenderer
    -> complete effective terminfo source
```

What it cannot yet do is:

```text
target TerminalDescription
+ ordered parent TerminalDescriptions
    -> relative local delta
    -> cancellations where inherited state must be removed
    -> use= references
    -> deterministic relative terminfo source
```

The 1.7 release closes that gap.

The central semantic promise is:

```text
Synthesize(target, orderedParents)
    -> render relative .ti source
    -> combine with source representations of orderedParents
    -> TermInfoSourceParser
    -> TermInfoSourceResolver
    -> TerminalDescription
    -> TerminalDescriptionComparer.Compare(target, resolved)
    -> AreEqual == true
```

That semantic round trip is the authoritative correctness gate.

Textual similarity with ncurses `infocmp -u` is desirable where it does not conflict with Icod's existing deterministic rendering and architectural contracts, but semantic equivalence is the primary requirement.

---

## 2. Why 1.7 Belongs in Inspection

Relative synthesis is an Inspection concern.

`Icod.TermInfo.Inspection` already owns:

- `TerminalDescriptionSourceRenderer`, which renders complete effective descriptions;
- `TermInfoSourceRenderer`, which renders unresolved Source state;
- `TerminalDescriptionComparer`, which compares effective terminal semantics;
- Source-aware comparison and provider-aware inspection;
- deterministic presentation controls used by `infocmp`.

Inspection already depends on:

```text
Icod.TermInfo.Inspection
    -> Icod.TermInfo.Source
    -> Icod.TermInfo
```

and also directly on `Icod.TermInfo`.

The 1.7 implementation SHALL preserve that layering.

In particular:

```text
Icod.TermInfo
    MUST NOT depend on Source, Inspection, or Compiler

Icod.TermInfo.Source
    MUST NOT depend on Inspection or Compiler

Icod.TermInfo.Compiler
    MUST NOT become a production dependency of Inspection

Icod.TermInfo.Termcap
    MUST NOT become a production dependency of synthesis

infocmp
    SHOULD continue to consume the reusable Inspection engine
    rather than implementing synthesis semantics itself
```

The Compiler may be used by tests for end-to-end validation, but it is not part of the production synthesis dependency graph.

---

## 3. Scope

Version 1.7 SHALL provide all of the following:

1. A reusable deterministic relative-source synthesis engine in `Icod.TermInfo.Inspection`.
2. Standard Boolean, numeric, and string capability delta synthesis.
3. Cancellation synthesis using `cap@` when inherited state must be removed.
4. Extended capability synthesis with deterministic case-sensitive handling.
5. Ordered multi-parent `use=` semantics matching the existing Source resolver.
6. Deterministic rendering of the synthesized entry.
7. A semantic round-trip verifier exercised by tests and release validation.
8. `infocmp -u` backed by the reusable engine.
9. Differential validation against a pinned ncurses `infocmp -u` reference corpus.
10. Fuzz/property tests over generated terminal descriptions and parent sets.
11. Coordinated package, router, archive, documentation, API-baseline, and release-audit updates for stable `1.7.0`.

---

## 4. Explicit Non-Goals

Version 1.7 SHALL NOT expand opportunistically into unrelated compatibility work.

The following remain out of scope unless required to make relative synthesis correct:

- `tput`, `clear`, `tabs`, `reset`, or terminal-session control;
- `infocmp -e` / `-E` C initializer generation;
- `infocmp -Q` compiled hexadecimal/base64 output;
- historical vendor subset filtering such as `-R`;
- initialization/reset-string analysis such as `infocmp -i`;
- Berkeley DB or other hashed terminfo stores;
- historical Unix compiled terminfo dialects;
- termcap-relative synthesis;
- source-comment reconstruction;
- source whitespace reconstruction;
- reconstruction of the original `use=` ancestry of an already resolved `TerminalDescription`;
- automatic discovery of an "optimal" parent set;
- automatic reordering of caller-supplied parents;
- global source-size minimization;
- source rewriting unrelated to the synthesized target entry.

A later release may build on the 1.7 engine to explore parent-set optimization, but 1.7 is deliberately deterministic rather than heuristic.

---

## 5. Terminology

### 5.1 Target

The **target** is the effective `TerminalDescription` whose semantics the synthesized source entry must reproduce.

The target contributes:

- canonical name;
- aliases;
- descriptive text;
- standard Boolean capabilities;
- standard numeric capabilities;
- standard string capabilities;
- extended capabilities.

### 5.2 Parent

A **parent** is an effective `TerminalDescription` intended to be referenced by one emitted `use=` field.

The reusable API must have a way to distinguish:

```text
the effective parent description
```

from:

```text
the exact source reference name to emit in use=
```

because a caller may intentionally refer to a canonical name or to an alias.

The precise public type name will be frozen in RS01. A likely shape is an immutable parent/reference descriptor containing:

```text
UseName
Description
```

### 5.3 Ordered parent list

The ordered parent list is part of the semantic input.

For:

```text
use=left,
use=right,
```

the existing Source resolver processes parents from right to left so that the leftward parent has higher priority in parent-to-parent collisions.

The synthesizer SHALL use the same rule.

### 5.4 Parent aggregate

The **parent aggregate** is the effective capability baseline produced by applying the existing ordered `use=` precedence rule to the supplied parents.

Parent identity metadata is not inherited. Only capability state contributes to the aggregate.

### 5.5 Local delta

The **local delta** is the set of capability declarations and cancellations that must be explicit on the target entry so that:

```text
local delta
over
parent aggregate
```

resolves to the target.

### 5.6 Minimal

For 1.7, **minimal** means:

> no local capability directive is emitted when omitting that directive would leave the resolved result semantically equal to the target, for the exact caller-supplied ordered parent list.

It does **not** mean:

- fewest possible parent references;
- shortest possible source text;
- best parent ordering;
- best subset of candidate parents;
- globally minimal source representation.

---

## 6. Frozen Semantic Direction

The following rules are release-level design commitments.

### 6.1 Parent order is preserved

The synthesizer SHALL:

- accept parents in explicit order;
- compute the parent aggregate using that order;
- emit one `use=` reference for each supplied parent in the same order;
- never reorder parents as an optimization;
- never silently remove a parent because it appears redundant.

This makes output stable and preserves caller intent.

### 6.2 Existing Source precedence is authoritative

The existing Source contract remains authoritative:

```text
parents = empty

for each parent from rightmost to leftmost:
    parents.OverlayHigherPriority(parent)

local = target-local-state
local.Inherit(parents)
```

Consequently:

- leftward parents win parent-to-parent collisions;
- explicit target-local state wins over every parent;
- an explicit target-local cancellation wins over every parent.

The 1.7 synthesizer SHALL not create a second inheritance-precedence model.

### 6.3 Identity comes from the target

The synthesized entry SHALL preserve target identity metadata:

- `Name`;
- `Aliases`, in target order;
- `Description`, including null versus present state where representable by the existing renderer contract.

Parent identity metadata SHALL not alter the synthesized target header.

### 6.4 Standard Boolean rules

For each standard Boolean capability:

| Parent aggregate | Target | Local output |
|---|---|---|
| absent | absent | nothing |
| absent | present | capability |
| present | present | nothing |
| present | absent | `cap@` |

### 6.5 Standard numeric rules

For each standard numeric capability:

| Parent aggregate | Target | Local output |
|---|---|---|
| absent | absent | nothing |
| absent | value | `cap#value` |
| same value | same value | nothing |
| value A | value B | `cap#valueB` |
| present | absent | `cap@` |

Numeric comparison is exact signed 32-bit semantic equality.

### 6.6 Standard string rules

For each standard string capability:

| Parent aggregate | Target | Local output |
|---|---|---|
| absent | absent | nothing |
| absent | value | `cap=value` |
| same value | same value | nothing |
| value A | value B | `cap=valueB` |
| present | absent | `cap@` |

String comparison is exact ordinal value equality.

Padding text, parameter expressions, embedded escapes, and other string content are semantic string data. The synthesizer SHALL not normalize strings merely to make them look similar.

### 6.7 Extended capability rules

Extended capability names remain:

- case-sensitive;
- ordinal;
- unable to shadow standard capability names.

The parent aggregate is computed by name using the same leftward-parent priority.

For each extended name in the union of target and inherited parent state:

- both absent: emit nothing;
- target only: emit the target value;
- inherited only: emit cancellation;
- same kind and same value: emit nothing;
- different value: emit the target value;
- different value kind: emit the target value with the target kind.

A parent-to-parent extended capability kind collision is not itself an error; normal ordered parent precedence determines the inherited winner.

RS03 must freeze the exact interaction between `infocmp -x` and semantic correctness. The release SHALL NOT ship a mode which silently claims to synthesize the target while producing source that resolves to a different effective terminal because extended inherited state was hidden.

### 6.8 Cancellations are semantic, not provenance reconstruction

A cancellation is emitted because it is necessary to suppress inherited state.

The synthesizer does not claim that the original source used a cancellation, nor that the target ever had source provenance containing `cap@`.

### 6.9 `use=` fields are emitted after local capability directives

Canonical Icod relative rendering SHOULD place local capability declarations and cancellations before the emitted `use=` references.

This presentation choice does not alter the frozen Source precedence rule, under which explicit child state outranks inherited state.

RS01/RS05 shall freeze the exact deterministic layout.

---

## 7. Proposed Public API Direction

The precise public names are **provisional until RS01**. The roadmap freezes ownership and semantics, not spelling.

The expected reusable surface is approximately:

```text
TerminalDescriptionSourceSynthesizer
TerminalDescriptionSourceSynthesisOptions
TerminalDescriptionSourceSynthesisParent
```

The preferred public API characteristics are:

- immutable inputs/options;
- no mutable global state;
- no environment-variable dependence;
- no database discovery inside the synthesizer;
- no native ncurses dependency;
- deterministic output;
- string-returning convenience entry point;
- caller-owned `TextWriter` entry point where consistent with existing renderer APIs;
- validation at public method boundaries;
- explicit parent reference names;
- reuse of existing `TerminalDescriptionSourceRendererOptions` concepts where doing so does not conflate complete-source rendering with relative synthesis.

The API SHALL NOT expose an unnecessary second public representation of `TerminalDescription`.

If an intermediate synthesis plan/model is needed internally to represent:

- local Boolean declarations;
- local numeric declarations;
- local string declarations;
- cancellations;
- extended declarations;
- ordered `use=` references;

it should remain internal unless a concrete reusable consumer justifies making it public.

---

## 8. Deterministic Ordering

The output must remain byte-for-byte deterministic for equal semantic inputs and equal options.

The default ordering contract should align with existing Inspection conventions:

1. standard Boolean capability directives;
2. standard numeric capability directives;
3. standard string capability directives;
4. extended capability directives;
5. `use=` references.

Within standard capability classes, the selected `TerminalDescriptionSourceCapabilityOrder` policy should be reused where applicable.

Extended capability names SHALL use ordinal deterministic ordering when no stronger existing renderer rule applies.

Cancellations SHALL sort according to the capability they cancel rather than being placed in an unrelated global cancellation bucket, unless RS05 testing demonstrates a stronger compatibility reason for another deterministic layout.

The same target, the same ordered parents, and the same rendering options SHALL always produce identical text regardless of:

- dictionary insertion order;
- provider insertion order;
- hash randomization;
- operating system;
- target framework;
- current culture.

Canonical synthesized text SHALL use LF line endings, consistent with the existing Inspection renderers. Repository files remain subject to the repository's CRLF policy independently of generated terminfo source text.

---

## 9. Semantic Verification Strategy

The release must verify the synthesizer through the real existing Source semantics rather than through a duplicate test-only merge algorithm.

The core verification pipeline is:

```text
target TerminalDescription
ordered parent TerminalDescriptions
        |
        v
TerminalDescriptionSourceSynthesizer
        |
        v
relative child source
        |
        +-----------------------------+
        |                             |
        | full parent source          |
        | rendered with existing      |
        | TerminalDescriptionSourceRenderer
        |                             |
        +-------------+---------------+
                      |
                      v
             one source document
                      |
             TermInfoSourceParser
                      |
             TermInfoSourceResolver
                      |
        resolved child TerminalDescription
                      |
             TerminalDescriptionComparer
                      |
                 AreEqual
```

The verifier SHALL compare identity metadata and all effective capabilities, because `TerminalDescriptionComparer` already treats both as semantic state.

A second integration path SHOULD validate Compiler interoperability:

```text
synthesized source document
    -> TermInfoSourceCompiler
    -> compiled terminfo
    -> CompiledTermInfoParser
    -> TerminalDescriptionComparer
    -> target equality
```

Compiler use remains test-only from the Inspection package's perspective.

---

## 10. `infocmp -u` Command Contract

RS06 will add the command-level feature only after the reusable engine has passed the semantic round-trip gate.

The core form is:

```text
infocmp -u target parent [parent ...]
```

Semantics:

- the first terminal operand is the target;
- every subsequent terminal operand is an ordered parent;
- at least one parent is required;
- emitted `use=` references preserve operand order;
- target acquisition follows the existing first-terminal database policy;
- parent acquisition follows the existing subsequent-terminal database policy.

### 10.1 Database options

Existing database selection remains meaningful:

```text
-A directory
    target database

-B directory
    parent database
```

For more than one parent, `-B` applies to all parent acquisitions unless a later separately approved feature introduces per-parent databases.

Neither option mutates `TERMINFO` or other process environment variables.

### 10.2 Rendering options

The following existing source-presentation controls should be supported in synthesis mode where their meaning is unambiguous:

```text
-0
-1
-w width
-s d|i|l|c
```

RS06 shall freeze their exact compatibility matrix.

### 10.3 Extended capabilities

`-x` remains the user-facing control associated with extended capability handling.

RS03 and RS06 together must ensure that the chosen `-x` behavior never creates a falsely advertised semantic round trip.

### 10.4 Comparison-mode interaction

`-u` becomes its own relative-synthesis mode.

The preferred compatibility direction is:

```text
-u
    select relative synthesis

-c -u
    MAY be accepted as an ncurses-compatible synonym for relative synthesis

-d -u
-n -u
    usage errors
```

This exact matrix is frozen in RS06 after differential tests against the pinned reference behavior.

### 10.5 Exit statuses

The command shall retain the suite convention:

```text
0    successful synthesis
1    acquisition or operational synthesis failure
2    usage error
130  cancellation
```

Semantic differences are not relevant in `-u` mode because the command is producing a new relative description rather than reporting a comparison.

### 10.6 Command-layer thinness

`infocmp` SHALL:

- parse arguments;
- acquire target and parents;
- construct synthesis inputs/options;
- call the Inspection synthesizer;
- write the result;
- map failures to command diagnostics/status.

It SHALL NOT duplicate the synthesis merge algorithm.

The `icod-terminfo` router automatically receives the feature through its existing `infocmp` dispatch path; no second router implementation is permitted.

---

## 11. ncurses Compatibility Policy

Current ncurses `infocmp -u` is the primary behavioral reference, but it is not a production dependency and not a source-code template.

The reference behavior establishes the broad compatibility target:

- first terminal is rewritten relative to the remaining terminals;
- remaining terminals become `use=` building blocks;
- inherited capabilities absent from the target require `@` cancellation;
- target values that differ from inherited state must be emitted.

Icod may differ textually in:

- comments;
- capability ordering;
- wrapping;
- whitespace;
- reconstruction notices;
- exact escape presentation;

provided the generated Icod source is deterministic and semantically correct.

Where ncurses output and the already-frozen Icod Source precedence model appear to disagree, the discrepancy must be investigated and documented rather than papered over with a compatibility special case.

Pinned differential fixtures SHALL record:

- ncurses version;
- host platform;
- source database provenance where relevant;
- exact invocation;
- expected semantic comparison result.

No normal unit test may require ncurses to be installed.

---

## 12. Hostile Input and Resource Boundaries

Although synthesis operates on already validated `TerminalDescription` values, 1.7 must preserve the repository's hostile-input posture at its public and command boundaries.

RS01 SHALL freeze bounded policies for:

- maximum parent count;
- maximum emitted source length, where needed beyond existing Source limits;
- parent reference name validation;
- cancellation-token checks in command/integration paths.

The implementation SHALL avoid:

- recursion proportional to parent count;
- unbounded repeated string concatenation;
- culture-sensitive sorting;
- dictionary-order dependence;
- quadratic scans over the fixed standard capability catalogs where a direct indexed/set representation is practical.

The number of standard capabilities is bounded by the existing frozen catalogs. Extended capability work should be approximately proportional to the union of target and parent extended names.

---

# 13. Implementation Tranches

## RS01 — Synthesis Contract and Internal Model

**Development version:** `1.7.0-Alpha-1`

### Goals

Freeze the public semantic contract before implementing the full delta engine.

RS01 shall:

- update the coordinated suite version to `1.7.0-Alpha-1`;
- add the 1.7 roadmap to repository planning documents;
- establish Inspection as the owning reusable package;
- define the ordered parent/reference input contract;
- freeze argument validation and resource limits;
- define the internal synthesis-plan representation;
- define target identity behavior;
- define deterministic output categories;
- define how existing renderer options are reused;
- add API-contract tests for the provisional public surface;
- preserve Runtime, Source, Compiler, and Termcap API baselines.

### Required decisions

RS01 must explicitly settle:

- public type names;
- parent descriptor shape;
- duplicate parent/reference policy;
- parent reference name validation;
- exact default rendering layout;
- maximum parent count;
- whether synthesis returns a string directly, a result object, or both;
- whether diagnostics are required for normal API use or invalid inputs remain argument exceptions.

### Gate

RS01 is complete when the reviewed contract can represent all later cases without changing Runtime or Source APIs.

### Documentation

Create:

```text
docs/1.7.0-RS01-SYNTHESIS-CONTRACT-AND-MODEL.md
```

**Implementation record:**
[`docs/1.7.0-RS01-SYNTHESIS-CONTRACT-AND-MODEL.md`](docs/1.7.0-RS01-SYNTHESIS-CONTRACT-AND-MODEL.md)

---

## RS02 — Standard Capability Delta and Cancellation Engine

**Development version:** `1.7.0-Alpha-2`

### Goals

Implement the complete standard-capability synthesis algorithm for one or more ordered parents, excluding extended capabilities.

RS02 shall implement:

- parent aggregate calculation;
- Boolean delta rules;
- numeric delta rules;
- string delta rules;
- cancellation generation;
- target identity preservation;
- no-op elimination;
- deterministic standard capability ordering.

### Required tests

At minimum:

- no parent capabilities;
- target identical to parent;
- target adds capabilities;
- target overrides numeric/string values;
- target removes inherited capabilities;
- mixed declaration/cancellation cases;
- two-parent precedence collisions;
- three-parent precedence collisions;
- leftmost-parent-wins verification;
- local target override over every parent;
- target identity preservation;
- repeated synthesis byte equality.

### Gate

For standard capabilities:

```text
synthesize
-> parse
-> resolve
-> compare
```

must produce semantic equality for every deterministic fixture.

### Documentation

Create:

```text
docs/1.7.0-RS02-STANDARD-DELTA-AND-CANCELLATION.md
```

**Implementation record:**
[`docs/1.7.0-RS02-STANDARD-DELTA-AND-CANCELLATION.md`](docs/1.7.0-RS02-STANDARD-DELTA-AND-CANCELLATION.md)

---

## RS03 — Extended Capability Synthesis

**Development version:** `1.7.0-Alpha-3`

### Goals

Extend the semantic engine to the full `TerminalDescription` capability universe.

RS03 shall implement and freeze:

- ordinal case-sensitive extended-name union;
- Boolean extended values;
- numeric extended values;
- string extended values;
- inherited extended cancellation;
- inherited extended override;
- target/parent value-kind changes;
- deterministic extended ordering;
- interaction with `-x` and reusable options.

### Required tests

Include:

- target-only extended values;
- parent-only extended values requiring cancellation;
- equal values omitted;
- differing values overridden;
- same name with parent-to-parent type differences;
- same name with inherited kind different from target kind;
- names differing only by case;
- opposite dictionary insertion orders;
- standard-name shadow rejection inherited from Runtime validation.

### Gate

The full-capability semantic round trip must succeed with extended capabilities enabled.

No CLI filtering rule may be accepted if it silently generates relative source that does not resolve to the stated target semantics.

### Documentation

Create:

```text
docs/1.7.0-RS03-EXTENDED-CAPABILITY-SYNTHESIS.md
```

**Implementation record:**
[`docs/1.7.0-RS03-EXTENDED-CAPABILITY-SYNTHESIS.md`](docs/1.7.0-RS03-EXTENDED-CAPABILITY-SYNTHESIS.md)

---

## RS04 — Ordered Multi-Parent Semantics and Reference Fidelity

**Development version:** `1.7.0-Alpha-4`

### Goals

Stress and freeze ordered `use=` composition as a first-class contract rather than an incidental loop.

RS04 shall cover:

- one parent;
- many parents;
- exact preservation of supplied parent order;
- parent collisions across all capability kinds;
- alias/reference spelling distinct from acquired canonical identity;
- repeated/equivalent parents according to the RS01 policy;
- deterministic behavior independent of provider or dictionary insertion order.

### Core invariant

For supplied references:

```text
P1, P2, P3
```

the emitted source must contain:

```text
use=P1,
use=P2,
use=P3,
```

and its inherited baseline must agree with the existing Source resolver's right-to-left processing.

The synthesizer SHALL NOT reorder to reduce local directives.

### Cross-check

Tests should independently construct equivalent ordinary source with the same `use=` list and verify that Source resolution agrees with the synthesizer's parent aggregate assumptions.

### Documentation

Create:

```text
docs/1.7.0-RS04-ORDERED-MULTI-PARENT-SEMANTICS.md
```

**Implementation record:**
[`docs/1.7.0-RS04-ORDERED-MULTI-PARENT-SEMANTICS.md`](docs/1.7.0-RS04-ORDERED-MULTI-PARENT-SEMANTICS.md)

---

## RS05 — Relative Source Rendering and Semantic Verifier

**Development version:** `1.7.0-Alpha-5`

### Goals

Turn the semantic plan into production-quality deterministic source text and make the round-trip verifier a permanent regression gate.

RS05 shall:

- finalize canonical relative source rendering;
- support appropriate existing layout/wrapping/order controls;
- preserve target header identity;
- render necessary cancellations;
- append ordered `use=` references;
- enforce deterministic LF source output;
- integrate parser/resolver semantic verification tests;
- integrate Compiler round-trip tests without adding a production Compiler dependency;
- extend the deterministic Toolchain sample to exercise synthesis.

### Required verification

The primary gate is:

```text
relative child source
+ rendered parent source
-> TermInfoSourceParser
-> TermInfoSourceResolver
-> TerminalDescriptionComparer
-> equal target
```

The secondary gate is:

```text
same combined source
-> TermInfoSourceCompiler
-> CompiledTermInfoParser
-> TerminalDescriptionComparer
-> equal target
```

### Rendering tests

Cover:

- canonical layout;
- single-line layout;
- one-capability-per-line layout;
- selected width;
- capability ordering;
- long escaped strings;
- empty/minimal local delta;
- cancellation-heavy entries;
- several `use=` references;
- culture changes;
- Windows/Linux/macOS determinism.

### Documentation

Create:

```text
docs/1.7.0-RS05-RELATIVE-RENDERING-AND-SEMANTIC-VERIFICATION.md
```

---

## RS06 — `infocmp -u`

**Development version:** `1.7.0-Alpha-6`

### Goals

Expose the proven reusable engine through the existing command suite.

RS06 shall:

- add `-u` parsing;
- require a target plus at least one parent;
- acquire the target through the first-terminal database path;
- acquire all parents through the subsequent-terminal database path;
- preserve parent operand order;
- map `-A` and `-B` consistently;
- integrate supported rendering options;
- freeze `-x` behavior;
- freeze the `-c -u` compatibility decision;
- reject incompatible comparison modes clearly;
- retain cancellation support and exit-status conventions;
- update `infocmp --help`;
- update `infocmp/README.md`;
- update router/help/distribution tests where required.

### Required command examples

At minimum:

```text
infocmp -u child base
infocmp -u child base1 base2
infocmp -1 -u child base
infocmp -w 120 -u child base
infocmp -x -u child base
infocmp -A ./target-db -B ./parent-db -u child base
icod-terminfo infocmp -u child base
```

### Gate

CLI output must pass the same parser/resolver/comparer semantic verifier as direct API output.

The CLI must remain a thin adapter over Inspection.

### Documentation

Create:

```text
docs/1.7.0-RS06-INFOCMP-RELATIVE-SYNTHESIS.md
```

---

## RS07 — Differential Validation, Fuzzing, and Hardening

**Development version:** `1.7.0-Alpha-7`

### Goals

Prove the implementation against both generated state space and an external reference.

RS07 shall add:

### Deterministic property/fuzz tests

Generate:

- target descriptions;
- one through several parents;
- standard capability combinations;
- numeric boundary values;
- escaped and parameterized strings;
- extended capabilities;
- parent collisions;
- target removals requiring cancellations.

For every accepted generated case:

```text
Synthesize
-> Parse
-> Resolve
-> Compare
-> equal
```

must hold.

Failures must report or preserve a reproducible seed/case.

### Pinned ncurses differential corpus

On a controlled Linux environment, compare representative cases with a pinned ncurses `infocmp -u`.

The corpus should include mainstream families such as:

- xterm variants;
- screen variants;
- tmux variants where available;
- Linux console;
- VT-family entries;
- entries with color extensions;
- entries with multiple plausible parents;
- cancellation-producing pairs.

The differential gate is primarily semantic, not byte-for-byte textual identity.

### Hostile and pathological cases

Exercise:

- maximum supported parent count;
- very large extended-name unions;
- long strings;
- many cancellations;
- alias-mediated parent references;
- duplicate/repeated parent cases under the RS01 policy;
- culture changes;
- cancellation tokens at command boundaries.

### Gate

No known semantic divergence may remain unexplained.

Any intentional ncurses textual difference must be documented as presentation-only or as a deliberate Icod contract choice.

### Documentation

Create:

```text
docs/1.7.0-RS07-DIFFERENTIAL-FUZZ-AND-HARDENING.md
```

---

## RS08 — API Freeze, Packaging, and Release Closure

**Development version:** `1.7.0-Alpha-8`

### Goals

Freeze the 1.7 contract and prepare stable publication.

RS08 shall:

- freeze the new Inspection public API;
- generate/update the `1.7.0` Inspection public API baseline;
- prove Runtime API remains frozen;
- prove Source API remains frozen;
- prove Compiler API remains frozen;
- prove Termcap API remains frozen;
- retain reusable `AssemblyVersion` values unless separately justified;
- update all coordinated package versions and release notes;
- update root and package READMEs;
- update `infocmp` command documentation;
- update the active post-1.0 roadmap;
- update compatibility documentation;
- update package-smoke coverage for new Inspection API;
- update the deterministic Toolchain sample;
- update release-verifier expectations;
- validate the `icod-terminfo` router;
- validate standalone archives;
- validate `-V` / `--version` for all commands;
- create the 1.7 release audit;
- run cross-platform build/test/package verification.

### New frozen baseline

Create:

```text
docs/1.7.0-INSPECTION-PUBLIC-API-BASELINE.txt
```

The existing historical baselines remain immutable.

### Release audit

Create:

```text
docs/1.7.0-RELEASE-AUDIT.md
```

The audit must explicitly record:

- public API changes;
- unchanged package dependency direction;
- unchanged Runtime/Source/Compiler/Termcap public surfaces;
- `infocmp -u` command contract;
- semantic round-trip evidence;
- differential corpus provenance;
- package topology;
- router topology;
- archive topology;
- supported target frameworks;
- exact stable release commit and tag after publication.

### Stable transition

After RS08 passes all gates:

```text
1.7.0-Alpha-8
    -> 1.7.0
```

No new feature work enters between final validation and stable publication.

---

# 14. Version and Package Policy

All coordinated packages continue to use the central suite version.

Each tranche shall update:

```xml
<IcodTermInfoSuiteVersion>1.7.0-Alpha-N</IcodTermInfoSuiteVersion>
```

and therefore the matching project `<Version />` and `<PackageVersion />` values through the existing central property.

The reusable package family remains:

```text
Icod.TermInfo
Icod.TermInfo.Source
Icod.TermInfo.Compiler
Icod.TermInfo.Inspection
Icod.TermInfo.Termcap
Icod.TermInfo.Tools
```

Expected semantic/API change by package:

| Package | 1.7 change |
|---|---|
| `Icod.TermInfo` | version/release metadata only |
| `Icod.TermInfo.Source` | version/release metadata only |
| `Icod.TermInfo.Compiler` | version/release metadata only; used in integration tests |
| `Icod.TermInfo.Inspection` | new additive synthesis API and implementation |
| `Icod.TermInfo.Termcap` | version/release metadata only |
| `Icod.TermInfo.Tools` | updated `infocmp` behavior through router distribution |

Standalone commands remain .NET 10 applications.

Reusable libraries continue to target `net8.0`, `net9.0`, and `net10.0`.

---

# 15. Testing Matrix

The 1.7 test program must include all of the following layers.

## 15.1 Unit tests

Inspection synthesis engine:

- argument validation;
- parent aggregation;
- every standard capability type;
- extended capability types;
- cancellations;
- ordering;
- deterministic rendering.

## 15.2 Source semantic round-trip tests

```text
Synthesis
-> Source parser
-> Source resolver
-> TerminalDescription
-> comparer
```

These are the most important correctness tests.

## 15.3 Compiler integration tests

```text
Synthesis
-> Source compiler
-> compiled parser
-> comparer
```

These prove generated relative source participates in the complete toolchain.

## 15.4 Command tests

`infocmp -u`:

- normal success;
- multiple parents;
- layout controls;
- database controls;
- extended-capability controls;
- usage errors;
- acquisition failures;
- cancellation;
- router dispatch;
- version/help.

## 15.5 Property/fuzz tests

Generated target/parent states with reproducible failures.

## 15.6 Differential tests

Pinned ncurses reference behavior on representative fixtures.

## 15.7 Package-smoke tests

Fresh package consumer must prove the new Inspection public API is actually present and usable from the packaged artifact.

## 15.8 Release-verifier tests

Release verification must exercise synthesis from already-built artifacts without leaking isolated package-smoke environment state into repository sample execution.

---

# 16. Compatibility Principles

1. Existing 1.6.1 Runtime behavior is unchanged.
2. Existing Source parsing and resolution behavior is unchanged.
3. Existing Compiler output behavior is unchanged.
4. Existing Termcap behavior is unchanged.
5. Existing `infocmp` modes retain their semantics except for the additive `-u` mode and deliberately approved option combinations.
6. Existing router command names remain unchanged.
7. Existing public APIs are preserved; Inspection receives additive API only.
8. Generated relative source must be valid input to Icod Source.
9. Where mainstream ncurses behavior is well-defined and compatible with Icod architecture, Icod should match it.
10. No compatibility behavior may require a production ncurses dependency.

---

# 17. Documentation Plan

The release should accumulate its contract tranche by tranche instead of reconstructing the design at the end.

Planned documents:

```text
Icod.TermInfo-1.7.0-Relative-Terminfo-Source-Synthesis-Roadmap.md

docs/1.7.0-RS01-SYNTHESIS-CONTRACT-AND-MODEL.md
docs/1.7.0-RS02-STANDARD-DELTA-AND-CANCELLATION.md
docs/1.7.0-RS03-EXTENDED-CAPABILITY-SYNTHESIS.md
docs/1.7.0-RS04-ORDERED-MULTI-PARENT-SEMANTICS.md
docs/1.7.0-RS05-RELATIVE-RENDERING-AND-SEMANTIC-VERIFICATION.md
docs/1.7.0-RS06-INFOCMP-RELATIVE-SYNTHESIS.md
docs/1.7.0-RS07-DIFFERENTIAL-FUZZ-AND-HARDENING.md
docs/1.7.0-INSPECTION-PUBLIC-API-BASELINE.txt
docs/1.7.0-RELEASE-AUDIT.md
```

The active:

```text
Icod.TermInfo-Post-1.0-Development-Roadmap.md
```

should be updated when RS01 begins so that 1.7.0 becomes the current active development line.

Historical 1.0 through 1.6 roadmaps, tranche documents, baselines, and release audits remain immutable except for links that are explicitly intended to point to the new active roadmap.

---

# 18. Definition of Done

`Icod.TermInfo 1.7.0` is complete only when all of the following are true:

- the reusable synthesizer lives in Inspection;
- Runtime, Source, Compiler, and Termcap dependency boundaries remain intact;
- target identity is preserved;
- standard Boolean/numeric/string delta synthesis is correct;
- inherited removals generate cancellations;
- extended capabilities are correct;
- ordered parent precedence exactly agrees with Source;
- parent `use=` order is preserved;
- generated output is deterministic;
- direct Source round-trip resolves exactly to the target;
- Compiler round-trip resolves exactly to the target;
- `infocmp -u` is a thin adapter over Inspection;
- router invocation behaves identically;
- command option interactions are documented and tested;
- ncurses differential cases are either semantically equivalent or have documented intentional differences;
- fuzz/property tests find no unresolved semantic divergence;
- package-smoke tests validate the packaged API;
- all supported target frameworks build and test;
- Release configuration passes warnings-as-errors policy;
- package validation succeeds;
- release verification succeeds on Windows and Unix-like hosts;
- version/help output is coordinated at `1.7.0`;
- the 1.7 Inspection API baseline is frozen;
- `docs/1.7.0-RELEASE-AUDIT.md` is complete;
- stable `1.7.0` is tagged from the exact validated release commit.

---

# 19. Post-1.7 Candidates

The following remain intentionally available for later releases:

```text
1.8 candidate
    tput / clear / tabs

later
    focused tic/infocmp compatibility switches
    compiled hexadecimal/base64 export
    C initializer generation
    initialization-string analysis
    Berkeley/hashed database providers
    historical compiled terminfo dialects
    optional parent-set optimization for relative synthesis
```

None of these should be pulled into 1.7 merely because the surrounding code is being touched.

---

# 20. Recommended First Step

Begin RS01 on a dedicated 1.7 working branch.

The first implementation patch should be deliberately modest:

```text
1. bump the coordinated version to 1.7.0-Alpha-1;
2. add this roadmap and update the active post-1.0 roadmap;
3. introduce the reviewed synthesis contract/types in Inspection;
4. introduce the internal synthesis-plan skeleton;
5. add contract/argument-validation tests;
6. add no full delta algorithm beyond what is needed to prove the API shape.
```

The purpose of RS01 is to freeze the right abstraction before capability-delta code makes that abstraction expensive to change.

Once RS01 is reviewed and accepted, RS02 can implement the standard capability engine against a stable contract.
