# Icod.TermInfo 1.8.0 - Relative Source Planning and Parent Selection Roadmap

**Project:** `Icod.TermInfo`
**Release:** `1.8.0`
**Theme:** Relative Source Planning and Parent Selection
**Published baseline:** `1.7.0`
**Repository baseline:** `main` at `9f4913a07de58e20e58fec4128da32f4a73742d5` (`v1.7.0`)
**Primary reusable package:** `Icod.TermInfo.Inspection`
**Primary command:** `infocmp`
**Target frameworks:** reusable libraries `net8.0`; `net9.0`; `net10.0`; commands `net10.0`
**Reusable assembly identity:** retain `1.0.0.0`
**Planned development sequence:** `1.8.0-Alpha-1` through `1.8.0-Alpha-8`, then stable `1.8.0`
**Status:** Planning contract proposed
**Primary objective:** select a deterministic, bounded, semantically valid ordered parent plan from caller-supplied candidates, then delegate source production unchanged to the frozen 1.7 relative-source synthesizer.

---

## 1. Release thesis

`Icod.TermInfo 1.7.0` completed deterministic relative terminfo source
synthesis when the caller already knows the exact ordered parents:

```text
effective target
+ exact ordered parents
    -> TerminalDescriptionSourceSynthesizer
    -> local additions and overrides
    -> inherited-state cancellations
    -> ordered use= references
    -> deterministic relative terminfo source
```

That explicit contract is correct and remains frozen. Parent order is semantic.
The synthesizer does not discover, rank, reorder, prune, or replace any parent
supplied by the caller.

The missing higher-level operation is planning:

```text
effective target
+ ordered candidate set
+ bounded planning policy
    -> evaluate zero-, one-, and multi-parent plans
    -> rank semantically valid plans deterministically
    -> select the best plan under the frozen cost contract
    -> invoke the existing 1.7 synthesizer unchanged
    -> return selected parents, source, score, and search evidence
```

Version 1.8.0 shall add that operation as a separate, opt-in Inspection layer.
It shall not change what the 1.7 synthesizer means.

The central promise is:

```text
Plan(target, candidates, policy)
    -> selected ordered parents
    -> existing 1.7 Synthesize(target, selectedParents)
    -> combine with source representations of selected effective parents
    -> TermInfoSourceParser
    -> TermInfoSourceResolver
    -> TerminalDescriptionComparer.Compare(target, resolved)
    -> AreEqual == true
```

The planner selects among representations. It does not change terminal
semantics and does not invent capabilities, parent identities, intermediate
entries, or source ancestry.

---

## 2. Architectural ownership

Relative-source planning belongs in `Icod.TermInfo.Inspection`.

Inspection already owns:

- effective-description rendering;
- source-aware rendering;
- structured semantic comparison;
- provider-aware inspection;
- conventional database catalog inspection;
- deterministic relative-source synthesis;
- source presentation controls used by `infocmp`.

Planning is an inspection and representation-selection operation over immutable
effective descriptions. It is not Runtime acquisition, Source parsing, Compiler
output, or Termcap conversion.

The production dependency graph shall remain:

```text
Icod.TermInfo.Inspection
        |          |
        v          v
Icod.TermInfo.Source
        |
        v
Icod.TermInfo

Icod.TermInfo.Compiler       test/sample use only
Icod.TermInfo.Termcap        no planning dependency
```

The following boundaries are frozen for 1.8:

1. `Icod.TermInfo` remains dependency-free.
2. `Icod.TermInfo.Source` continues to depend only on Runtime.
3. `Icod.TermInfo.Compiler` continues to depend only on Runtime and Source.
4. `Icod.TermInfo.Inspection` continues to depend only on Runtime and Source.
5. `Icod.TermInfo.Termcap` continues to depend only on Runtime.
6. No reusable package depends on `Icod.CommandFramework`.
7. No production Inspection dependency on Compiler or Termcap is introduced.
8. `infocmp` remains a thin command adapter over reusable Inspection behavior.
9. The router continues to dispatch the existing `infocmp` implementation
   without duplicating planning logic.

---

## 3. Relationship to the frozen 1.7 contract

The 1.7 public types remain source-, binary-, and semantically compatible:

```text
TerminalDescriptionSourceSynthesisParent
TerminalDescriptionSourceSynthesisOptions
TerminalDescriptionSourceSynthesizer
```

In particular:

- `TerminalDescriptionSourceSynthesizer` shall never inspect candidates which
  were not passed as selected parents;
- it shall preserve supplied `UseName` values exactly;
- it shall preserve supplied parent order exactly;
- it shall not remove repeated or equivalent supplied parents;
- it shall not apply a planning score;
- it shall not perform database discovery;
- it shall retain the 1.7 exception and output contracts;
- every existing 1.7 test shall remain unchanged unless a version assertion or
  release record must advance.

Planning shall be exposed through new API rather than through a behavioral
change to an existing 1.7 method.

The planner may reuse `TerminalDescriptionSourceSynthesisParent` as its
candidate representation because that type already contains the exact two
inputs required for a selectable parent:

```text
UseName
Description
```

A new public candidate type shall be introduced only if RP01 proves that
planning-specific immutable data cannot be represented safely through the
existing synthesis-parent type.

---

## 4. Planning terminology

### 4.1 Target

The target is the effective `TerminalDescription` which generated source must
reproduce.

### 4.2 Candidate

A candidate is one caller-supplied effective parent description paired with the
exact `UseName` which would appear in generated source if selected.

Candidate identity is its position in the snapshotted candidate list. Two
candidates may:

- refer to semantically equal descriptions;
- use different aliases for the same effective description;
- use equal descriptions obtained from different providers;
- repeat the same `UseName` if the caller deliberately supplied it.

The planner shall not canonicalize, merge, or silently discard candidates before
the planning contract explicitly permits doing so.

### 4.3 Plan

A plan is an ordered selection of zero or more distinct candidate positions.

Selected order is semantic because the existing Source resolver gives earlier
`use=` references higher parent-to-parent precedence through its established
right-to-left resolution process.

The same candidate position shall not appear twice in one plan. Distinct
candidate positions remain distinct even if their names or descriptions are
equal.

### 4.4 Baseline plan

The zero-parent plan is always considered when the active synthesis options can
render the complete target safely. It represents ordinary complete effective
source with no `use=` fields.

Including this baseline guarantees that selection is measured against a real,
semantically complete alternative rather than against an assumed benefit from
inheritance.

### 4.5 Valid plan

A plan is valid only when the existing 1.7 synthesizer can produce source under
the requested options and that source reproduces the target semantics when its
selected effective parents are represented consistently.

A candidate plan which violates extended-capability suppression, representation
bounds, parent-count limits, or another frozen synthesis rule is not silently
approximated.

### 4.6 Best plan

The best plan is the valid evaluated plan with the lexicographically smallest
frozen planning score. Candidate input order supplies the final tie-break.

"Best" means best under the documented 1.8 score and search contract. It does
not claim that the selected source resembles a human author's preferred
ancestry.

---

## 5. Deterministic cost contract

RP01 shall freeze a transparent lexicographic score. The default score should
prefer semantic simplicity before textual presentation.

The proposed order is:

1. fewer emitted local capability directives;
2. fewer emitted cancellations;
3. fewer selected parents;
4. fewer UTF-8 bytes in the rendered source;
5. lexicographically earlier selected candidate-index sequence.

For this score:

- a Boolean, numeric, or string capability declaration counts as one local
  capability directive;
- a standard or extended cancellation counts both as a local directive and as
  one cancellation;
- identity header fields do not count as capability directives;
- every emitted `use=` reference counts through the parent-count component;
- UTF-8 byte length is calculated without a byte-order mark;
- line endings are the deterministic LF output already frozen in 1.7;
- candidate-index comparison is ordinal integer comparison and is independent
  of culture;
- standard and extended capability insertion order shall not change the score.

RP01 may adjust the exact public shape of the score after implementation
evidence, but the final Alpha-1 contract must be explicit. Later tranches shall
not quietly change score precedence.

Alternative objectives, configurable weights, or application-supplied scoring
callbacks are outside the initial 1.8 contract. They may be considered later
only after the deterministic default is proven useful.

---

## 6. Search and resource-bounds contract

Ordered multi-parent planning is combinatorial. External inputs and caller-owned
candidate sequences must not cause uncontrolled CPU time, allocation, recursion,
or source growth.

Planning options shall bound at least:

- maximum accepted candidate count;
- maximum selected parent count;
- maximum evaluated plan count;
- maximum generated source length inherited from or coordinated with synthesis
  policy;
- cancellation observation;
- caller cancellation through `CancellationToken` where an operation may be
  meaningfully long-running.

Exact defaults and supported maxima shall be frozen by RP01 after focused
performance and API-regret review.

The implementation shall:

1. validate scalar options before enumerating candidates;
2. snapshot a candidate sequence at most once;
3. reject null candidates and invalid names deterministically;
4. stop candidate enumeration at the configured bound plus the minimum required
   evidence of overflow;
5. use checked arithmetic for plan-space calculations and score counters;
6. avoid recursive depth proportional to uncontrolled input;
7. check cancellation at deterministic boundaries;
8. avoid retaining generated source for every rejected plan;
9. retain enough evidence to explain the selected result;
10. produce the same result across Windows, Linux, macOS, culture, process, and
    supported target framework.

### 6.1 Exhaustive and bounded search

The result shall distinguish an exhaustive search from a budget-limited search.

If every legal ordered plan within the configured candidate and parent limits
was evaluated, the result may claim exhaustive selection under those limits.

If the plan-evaluation budget stops the search first, the result shall identify
that fact. It must not claim global optimality.

The default configuration should be chosen so common zero-, one-, and two-parent
planning over a practical candidate set can be exhaustive. Larger searches may
require an explicit caller opt-in to bounded non-exhaustive planning.

### 6.2 Stable enumeration

Exhaustive plan enumeration shall be deterministic:

```text
zero-parent plan
one-parent plans in candidate order
two-parent ordered plans in lexicographic candidate-index order
later depths in the same defined order
```

If a bounded search uses pruning or a frontier, its ordering and tie-breaking
shall be specified and tested. Hash-table iteration order, task scheduling, and
wall-clock time shall never decide the result.

### 6.3 Safe pruning

Pruning is permitted only when it cannot remove a plan which could outrank the
best retained plan under the frozen score, or when the result is explicitly
reported as non-exhaustive.

Names, aliases, reference equality, and superficial capability counts are not
by themselves sufficient evidence that two candidates are interchangeable.

---

## 7. Identity, aliases, and source-graph limits

The planner operates on effective descriptions. Effective
`TerminalDescription` values do not retain original source ancestry.

Consequently, the planner shall not claim to reconstruct historical authorial
intent or a complete preexisting source graph.

The planner shall:

- preserve every selected `UseName` exactly;
- treat names and aliases with ordinal comparison where identity comparison is
  required;
- preserve candidate order as the final deterministic tie-break;
- exclude an obvious self-reference when a candidate `UseName` equals the
  target's canonical name or one of its aliases under the adopted source-name
  comparison contract;
- report or reject ambiguous catalog-derived self-reference rather than guessing;
- document that caller-supplied external source graphs remain caller-owned.

The planner shall not:

- invent a new parent entry;
- rename a parent;
- replace an alias with a canonical name;
- create an intermediate factoring entry;
- inspect an uncontrolled host source database;
- infer cycles from information absent from effective descriptions;
- claim provenance not supplied by the caller or an explicit catalog.

Source-set factoring and automatic creation of shared intermediate entries are
separate future work.

---

## 8. Proposed reusable API direction

RP01 shall perform the final API-regret audit. The preferred minimal shape is:

```text
TerminalDescriptionSourcePlanningOptions
TerminalDescriptionSourcePlanningScore
TerminalDescriptionSourcePlan
TerminalDescriptionSourcePlanner
```

The existing `TerminalDescriptionSourceSynthesisParent` should be reused for
candidate input and selected-parent output unless evidence requires otherwise.

### 8.1 Planning options

Planning options should contain immutable snapshots of:

- the existing synthesis options or equivalent copied values;
- maximum candidate count;
- maximum selected parent count;
- maximum evaluated plan count;
- whether a non-exhaustive result is permitted;
- any other resource bound proven necessary by RP01.

Options shall reject unsupported enum values and invalid ranges at construction.
Mutable collections, delegates, provider instances, and process-global policy do
not belong in the options object.

### 8.2 Planning score

The score shall expose the components necessary to understand and reproduce the
frozen comparison:

- local directive count;
- cancellation count;
- parent count;
- rendered UTF-8 byte count;
- selected candidate-index sequence or equivalent stable final tie-break data.

The public type should be immutable. Equality and ordering semantics shall be
reviewed deliberately rather than inherited accidentally from implementation
containers.

### 8.3 Plan result

The selected plan should expose:

- selected ordered parents;
- deterministic generated source;
- score;
- evaluated plan count;
- whether search was exhaustive under configured limits;
- candidate count considered;
- immutable planning evidence required for diagnostics and tests.

The result shall not expose mutable internal arrays or implementation-specific
search nodes.

### 8.4 Planner operations

The reusable planner should support:

```text
Plan(target, candidates)
Plan(target, candidates, options)
Plan(target, candidates, options, cancellationToken)
```

The exact overload set shall be minimized during RP01. Optional values should
not produce an overload explosion.

The planner shall return a plan or throw a documented argument, bounds,
cancellation, or representation exception. Normal candidate inferiority is not
an exception.

### 8.5 Catalog orchestration

Catalog-wide candidate planning may be exposed through a separate Inspection
orchestration operation or through carefully reviewed planner overloads.

It shall accept an explicit `TermInfoDatabaseCatalog` or explicit directory
root. It shall not make the generic planner discover the host system database.

Catalog-derived planning shall define:

- deterministic candidate ordering;
- canonical-name versus alias candidate policy;
- target exclusion;
- duplicate physical-entry handling;
- malformed or incomplete catalog behavior;
- cancellation and parser resource limits.

---

## 9. Command contract direction

The proposed command form is:

```text
infocmp --plan-use [options] target candidate [candidate ...]
```

The first operand is the target. Later operands are the ordered candidate set.

Database policy shall remain explicit:

```text
-A directory    target database
-B directory    candidate database for every explicit candidate operand
```

The exact candidate operand spelling becomes its candidate `UseName`. Candidate
operand order supplies the final planning tie-break. Acquisition may resolve an
alias to an effective entry, but emitted source preserves the operand spelling
if that candidate is selected.

The command shall write only the selected deterministic source to standard
output on success. Diagnostics and an explicitly requested planning report may
use standard error without contaminating source output.

### 9.1 Presentation controls

The existing synthesis presentation controls should apply:

```text
-0
-1
-w width
-s d|i|l|c
-x
```

They shall be mapped into the reusable planning and synthesis options rather
than reimplemented by the command.

### 9.2 Planning controls

Long-form controls may expose reviewed resource policy, for example:

```text
--max-parents count
--max-plans count
--require-exhaustive
--allow-bounded
```

Names and defaults remain subject to RP06 command-regret review. The command
shall not expose an option which the reusable planner cannot enforce.

### 9.3 Explicit catalog mode

RP05 and RP06 may add an explicit catalog form such as:

```text
infocmp --plan-use --all-candidates -B directory target
```

This form shall require an explicit candidate directory. It shall not scan all
host discovery locations implicitly.

Canonical catalog entries should be the default candidate identities. Alias
expansion, if supported, must be explicit and deterministic.

An incomplete catalog caused by malformed entries, permission failures, or I/O
failures shall not silently masquerade as a complete candidate universe.

### 9.4 Option interaction

Planning mode is distinct from explicit 1.7 synthesis and from comparison.

The command shall define and test interactions with:

```text
-u
-c
-d
-n
-q
-D
```

At minimum:

- `--plan-use` with `-u` is a usage error;
- comparison-only selectors with `--plan-use` are usage errors;
- database-location listing is not combined with planning;
- presentation controls retain their synthesis meanings;
- no existing command form changes behavior.

### 9.5 Exit statuses

The established command exit statuses remain:

```text
0    planning and synthesis succeeded
1    acquisition, catalog, bounds, or operational planning failure
2    usage error
130  cancellation
```

No suitable nonzero-parent plan is not necessarily failure because the
zero-parent baseline may be the best valid plan.

---

## 10. Package and distribution contract

Version 1.8.0 shall retain the coordinated package family:

```text
Icod.TermInfo
Icod.TermInfo.Source
Icod.TermInfo.Compiler
Icod.TermInfo.Inspection
Icod.TermInfo.Termcap
Icod.TermInfo.Tools
```

No new package is required for relative-source planning.

The five reusable libraries continue to target:

```text
net8.0
net9.0
net10.0
```

The five standalone commands and the router continue to target `net10.0`.

The six framework-dependent standalone suite archives remain:

```text
win-x64
win-arm64
linux-x64
linux-arm64
osx-x64
osx-arm64
```

`Icod.TermInfo.Tools` continues to install only the `icod-terminfo` router. The
router continues to expose the same five command names:

```text
tic
infocmp
toe
captoinfo
infotocap
```

Planning enters the distributions through the existing `infocmp` command. No
new standalone executable or router command is introduced.

The release artifact count shall remain unchanged unless an independently
reviewed release-infrastructure change requires otherwise.

---

## 11. Permanent correctness evidence

Every 1.8 tranche shall preserve all 1.7 synthesis tests and add planning
evidence without weakening older gates.

### 11.1 Semantic round trip

For every selected plan:

```text
selected source
+ complete source for selected effective parents
    -> TermInfoSourceParser
    -> TermInfoSourceResolver
    -> TerminalDescriptionComparer
    -> target equality
```

### 11.2 Brute-force planning oracle

Small candidate sets shall be evaluated by a deliberately simple independent
test oracle which enumerates every legal ordered plan and applies the frozen
score.

Production planner results shall match that oracle exactly for exhaustive test
spaces.

The oracle shall not share production search or pruning code.

### 11.3 Generated-state testing

Seeded deterministic generation shall cover:

- empty and dense targets;
- standard Boolean, numeric, and string capabilities;
- ordinal case-sensitive extended capabilities;
- parent additions, overrides, and conflicts;
- cancellation-heavy candidates;
- semantically equal candidates under different references;
- repeated names and aliases where legal;
- candidate order permutations;
- zero-, one-, and multi-parent winners;
- ties at every score component;
- exhaustive and budget-limited search;
- all supported presentation layouts and ordering controls;
- extended-capability suppression success and failure;
- culture and insertion-order changes.

Every generated failure shall print the complete reproducible seed and case
identity.

### 11.4 Pathological boundaries

Focused tests shall cover:

- candidate-count boundary and one-past-boundary;
- parent-count boundary and one-past-boundary;
- plan-budget exhaustion;
- checked combinatorial arithmetic overflow;
- very long names and descriptions;
- maximum supported extended-capability unions;
- cancellation storms;
- early and late cancellation tokens;
- stateful or single-use candidate enumerables;
- duplicate object instances and equal effective descriptions;
- invalid options and unsupported enum values;
- no partial mutable result after failure.

### 11.5 Existing differential corpus

The pinned 1.7 ncurses semantic differential corpus remains permanent.

Because ncurses does not define this new general planning API, 1.8 shall not
claim textual differential equivalence for automatic parent selection. Selected
ordered parents may still be passed through the existing explicit synthesis
path and compared semantically with the checked-in ncurses evidence where
applicable.

### 11.6 Cross-target and cross-host determinism

The same target, candidates, and options shall select the same plan and emit the
same LF source on:

- `net8.0`;
- `net9.0`;
- `net10.0`;
- Windows;
- Linux;
- macOS;
- invariant and non-invariant cultures.

---

## 12. Samples and documentation

### 12.1 Toolchain sample

`Icod.TermInfo.Toolchain.Sample` shall add a deterministic planning path:

1. construct or resolve one target;
2. provide one useful parent plus controlled decoy candidates;
3. plan under explicit bounds;
4. require the expected selected parent order and score;
5. synthesize through the returned plan;
6. reparse and resolve combined source;
7. compile and publish the result;
8. reacquire it through Runtime;
9. require semantic equality with the original target.

The sample shall remain independent of host terminfo state and native ncurses.

### 12.2 ToolSuite sample

The controlled command walkthrough shall demonstrate:

- explicit candidate planning;
- a decoy candidate which is not selected;
- the selected `use=` reference;
- validation of emitted source through `tic -c`;
- direct and routed command forms;
- explicit catalog mode if RP06 includes it.

### 12.3 Package READMEs

The Inspection README shall explain:

- the difference between explicit synthesis and planning;
- candidate identity and order;
- the cost tuple;
- exhaustive versus bounded results;
- zero-parent fallback;
- limits of ancestry reconstruction;
- deterministic and hostile-input guarantees.

The `infocmp` and router READMEs shall document command forms, options,
interactions, output, exit statuses, and controlled database policy.

### 12.4 Release audit

RP08 shall create:

```text
docs/1.8.0-RELEASE-AUDIT.md
```

It shall record:

- exact additive public API;
- unchanged 1.7 synthesis API and semantics;
- frozen score and search contract;
- package dependency direction;
- command behavior;
- samples and smoke evidence;
- generated-state and brute-force oracle evidence;
- resource bounds;
- package, router, and archive topology;
- exact stable release commit and tag after publication.

---

## 13. Development tranche sequence

## RP01 - Planning Contract, Cost Model, and API Foundation

**Development version:** `1.8.0-Alpha-1`

### Goals

Freeze the distinction between explicit synthesis and planning before search
implementation spreads across the codebase.

### Required work

- perform a pre-RP01 API and dependency audit;
- confirm the exact 1.7 Inspection baseline before any additive API;
- finalize planner, options, score, and result type names;
- decide whether the existing synthesis-parent type is sufficient for candidates;
- freeze the lexicographic score components and comparison order;
- freeze candidate snapshot and validation semantics;
- freeze zero-parent baseline behavior;
- freeze exhaustive versus bounded result reporting;
- select defaults and supported maxima from measured evidence;
- define exception and cancellation behavior;
- advance the coordinated version to `1.8.0-Alpha-1`;
- add contract tests for every public member and dependency boundary.

### Gate

RP01 is complete when callers can construct a validated immutable planning
request and the repository contains no search implementation whose behavior is
not covered by the written contract.

### Documentation

Create:

```text
docs/1.8.0-RP01-PLANNING-CONTRACT-AND-API-FOUNDATION.md
```

---

## RP02 - Zero- and Single-Parent Planning

**Development version:** `1.8.0-Alpha-2`

### Goals

Implement the smallest complete planning operation and prove the score against
an independent oracle.

### Required work

- evaluate the zero-parent baseline;
- evaluate every legal single candidate exactly once;
- delegate source production to the existing 1.7 synthesizer;
- compute the frozen score without reparsing human-readable command output;
- select the deterministic best plan;
- preserve exact selected `UseName` spelling;
- expose immutable result evidence;
- handle candidates which do not improve the baseline;
- handle invalid plans without approximating target semantics;
- add brute-force oracle tests for zero and one parent;
- add standard and extended capability cases;
- advance the coordinated version to `1.8.0-Alpha-2`.

### Gate

RP02 is complete when every single-parent result matches the independent oracle,
round-trips semantically, and remains deterministic across candidate order,
culture, and supported target framework.

### Documentation

Create:

```text
docs/1.8.0-RP02-ZERO-AND-SINGLE-PARENT-PLANNING.md
```

---

## RP03 - Ordered Multi-Parent Planning

**Development version:** `1.8.0-Alpha-3`

### Goals

Evaluate ordered parent combinations without weakening the 1.7 precedence
contract.

### Required work

- enumerate legal ordered plans up to the configured parent bound;
- treat different orders as different semantic plans;
- prohibit repeated use of one candidate position within a plan;
- preserve distinct equal candidates and aliases;
- evaluate additions, overrides, collisions, and cancellations across parents;
- implement deterministic lexicographic plan enumeration;
- compare every exhaustive small-space result with the brute-force oracle;
- prove that selected order is passed unchanged to the synthesizer;
- advance the coordinated version to `1.8.0-Alpha-3`.

### Gate

RP03 is complete when generated and hand-authored cases demonstrate one-parent,
two-parent, and higher configured-depth winners, and every exhaustive result
matches the independent oracle exactly.

### Documentation

Create:

```text
docs/1.8.0-RP03-ORDERED-MULTI-PARENT-PLANNING.md
```

---

## RP04 - Bounded Search, Cancellation, and Planning Evidence

**Development version:** `1.8.0-Alpha-4`

### Goals

Make planning predictably safe for large or adversarial candidate sets.

### Required work

- enforce candidate, parent, plan-count, and generated-size bounds;
- use checked plan-space arithmetic;
- distinguish exhaustive from budget-limited results;
- define deterministic bounded-search order and any safe pruning;
- add cancellation-token support at stable boundaries;
- avoid retaining source for every rejected plan;
- expose evaluated-plan and search-completeness evidence;
- prove that partial internal state never escapes on failure;
- add allocation and runtime guard tests appropriate for CI;
- advance the coordinated version to `1.8.0-Alpha-4`.

### Gate

RP04 is complete when hostile candidate sets terminate within documented bounds,
exhaustive results remain exact, bounded results never claim global optimality,
and cancellation leaves no externally visible partial plan.

### Documentation

Create:

```text
docs/1.8.0-RP04-BOUNDED-SEARCH-CANCELLATION-AND-EVIDENCE.md
```

---

## RP05 - Explicit Database Catalog Planning

**Development version:** `1.8.0-Alpha-5`

### Goals

Compose planning with the existing Inspection database catalog without adding
implicit host discovery.

### Required work

- accept an explicit catalog or explicit conventional directory root;
- define deterministic catalog candidate ordering;
- exclude the target and obvious self-references safely;
- define canonical-name and alias policy;
- define duplicate physical-entry policy;
- reject or report incomplete catalogs without claiming completeness;
- preserve parser options, cancellation, and resource bounds;
- add temporary controlled database tests;
- retain the existing catalog public contract where possible;
- advance the coordinated version to `1.8.0-Alpha-5`.

### Gate

RP05 is complete when a controlled database can supply deterministic planning
candidates without environment mutation, hidden system search, duplicate
instability, or false completeness claims.

### Documentation

Create:

```text
docs/1.8.0-RP05-EXPLICIT-DATABASE-CATALOG-PLANNING.md
```

---

## RP06 - infocmp Planning Command and Distribution Composition

**Development version:** `1.8.0-Alpha-6`

### Goals

Expose reusable planning through `infocmp` and both distribution forms without
moving planning semantics into the command layer.

### Required work

- add the reviewed `--plan-use` command form;
- map target and candidate acquisition through existing `-A` and `-B` policy;
- preserve candidate operand spelling and order;
- map existing presentation controls into reusable options;
- add reviewed planning-bound controls;
- define every selector interaction and usage diagnostic;
- write only selected source to standard output on success;
- retain established exit statuses and cancellation behavior;
- prove direct and routed behavior are identical;
- update tool-package and archive smoke to execute a real planning path;
- add the controlled ToolSuite walkthrough;
- advance the coordinated version to `1.8.0-Alpha-6`.

### Gate

RP06 is complete when standalone `infocmp`, routed `icod-terminfo infocmp`, the
NuGet tool package, and all six standalone archive forms execute the same
controlled planning operation and emit semantically equivalent source.

### Documentation

Create:

```text
docs/1.8.0-RP06-INFOCMP-PLANNING-COMMAND-AND-DISTRIBUTION.md
```

---

## RP07 - Generated-State Validation, Oracle Comparison, and Hardening

**Development version:** `1.8.0-Alpha-7`

### Goals

Establish permanent evidence that planning remains correct, deterministic, and
bounded beyond hand-authored examples.

### Required work

- add seeded generated target and candidate universes;
- compare exhaustive production results with the independent brute-force oracle;
- cover ties at every score component;
- cover candidate permutations and equivalent descriptions;
- cover standard and extended capabilities, kind changes, and cancellations;
- cover exhaustive and budget-limited searches;
- cover all synthesis layouts and ordering modes;
- add maximum-bound and one-past-boundary tests;
- add culture, insertion-order, and repeated-process determinism checks;
- extend the Toolchain sample through plan, synthesize, compile, publish,
  reacquire, and compare;
- retain and reuse the pinned 1.7 differential corpus where applicable;
- advance the coordinated version to `1.8.0-Alpha-7`.

### Gate

RP07 is complete when every exhaustive generated case agrees with the oracle,
every selected source round-trips to the target, every failure is reproducible
from a seed, and pathological inputs remain within frozen limits.

### Documentation

Create:

```text
docs/1.8.0-RP07-GENERATED-STATE-ORACLE-AND-HARDENING.md
```

---

## RP08 - API Freeze, Packaging, and Release Closure

**Development version:** `1.8.0-Alpha-8`

### Goals

Freeze the 1.8 planning contract and prepare stable publication without adding
another feature tranche.

### Required work

- freeze the additive Inspection planning public API;
- generate and review the 1.8 Inspection public API baseline;
- prove the 1.7 synthesis surface remains unchanged;
- prove Runtime, Source, Compiler, and Termcap public APIs remain unchanged;
- prove all reusable assembly versions remain `1.0.0.0`;
- freeze score, bounds, search-completeness, and candidate-order semantics;
- update package release notes and package-facing READMEs;
- update root documentation and active post-1.0 roadmap;
- update package-smoke coverage for planning API;
- update deterministic Toolchain and ToolSuite samples;
- update release-verifier expectations;
- validate direct command, router package, and six archives;
- create the 1.8 release audit;
- run cross-platform build, test, pack, install, archive, and smoke gates;
- advance the coordinated version to `1.8.0-Alpha-8`.

### New frozen baseline

Create:

```text
docs/1.8.0-INSPECTION-PUBLIC-API-BASELINE.txt
```

The 1.7 baseline remains an immutable historical record.

### Gate

RP08 is complete when Alpha-8 contains the complete stable-intended API,
behavior, documentation, package graph, command semantics, samples, fixtures,
and release evidence, with no known semantic, bounds, or distribution gap.

### Documentation

Create:

```text
docs/1.8.0-RP08-API-PACKAGING-AND-RELEASE-CLOSURE.md
docs/1.8.0-RELEASE-AUDIT.md
```

---

## 14. Version and package policy

Every tranche shall update the single coordinated version authority:

```xml
<IcodTermInfoSuiteVersion>1.8.0-Alpha-N</IcodTermInfoSuiteVersion>
```

All five reusable packages and `Icod.TermInfo.Tools` shall consume that value
for package versioning. Standalone commands shall consume it for command version
reporting and remain non-packable.

All five reusable assemblies shall retain:

```text
AssemblyVersion 1.0.0.0
```

The stable transition is:

```text
1.8.0-Alpha-8
    -> 1.8.0
```

No feature, API, score, search, or command change enters between successful
Alpha-8 validation and stable publication.

---

## 15. Release verification

The complete release gate shall retain all existing checks and add planning
coverage.

Before stable tagging, the exact intended release commit shall pass:

- solution clean, restore, build, and test on Windows, Linux, and macOS;
- Runtime 1.0 public API baseline;
- Source 1.1 public API baseline;
- Compiler 1.2 public API baseline;
- Termcap 1.6 public API and reflection baseline;
- Inspection 1.8 public API baseline;
- exact net8/net9/net10 public API equivalence for reusable packages;
- deterministic planner oracle comparisons;
- semantic source and compiled round trips;
- package structure, metadata, XML, symbols, Source Link, and dependency checks;
- isolated package-reference smoke for all reusable packages;
- deterministic Toolchain sample execution;
- direct and routed `infocmp --plan-use` execution;
- router package installation and smoke on all three host families;
- six standalone archive structure checks;
- matching-host archive smoke on all three host families;
- release artifact count and checksum verification.

The stable tag shall be exactly:

```text
v1.8.0
```

It shall identify the exact current validated `main` HEAD when the tag workflow
starts and shall match the centralized suite version.

---

## 16. Non-goals for 1.8

Version 1.8 shall not include:

- behavioral changes to explicit 1.7 synthesis;
- automatic creation of intermediate shared parent entries;
- source-set factoring across multiple targets;
- reconstruction of historical author intent;
- unbounded or wall-clock-limited planning;
- application-supplied arbitrary scoring delegates;
- implicit host-wide candidate discovery;
- JSON schema or database-manifest publication;
- Berkeley DB or hashed terminfo stores;
- AIX, HP-UX, OSF/1, or other historical binary dialects;
- new termcap conversion behavior;
- live terminal sessions, input decoding, probing, or negotiation;
- PTY or ConPTY support;
- curses, screen, window, or widget behavior;
- terminal emulation or graphics protocols;
- a new command executable or package family.

These exclusions keep 1.8 focused and preserve ownership by `Icod.Terminal`,
future `Icod.Pty`, `Icod.DCurses`, and later demand-driven TermInfo releases.

---

## 17. Risks and mitigations

### 17.1 Combinatorial growth

**Risk:** ordered parent combinations grow rapidly.

**Mitigation:** explicit candidate, parent, and evaluation bounds; checked
arithmetic; exhaustive-result evidence; deterministic budget-limited reporting;
no claim of optimality after incomplete search.

### 17.2 Accidental change to 1.7 semantics

**Risk:** planning logic leaks into the explicit synthesizer.

**Mitigation:** separate public entry point, immutable 1.7 API baseline, permanent
1.7 regression tests, and command modes which remain distinct.

### 17.3 Unstable cost behavior

**Risk:** implementation details decide the selected parent.

**Mitigation:** frozen lexicographic score, explicit UTF-8 length rule, ordinal
candidate-index tie-break, independent brute-force oracle, culture and insertion
order tests.

### 17.4 False ancestry claims

**Risk:** users interpret the selected plan as recovered original source.

**Mitigation:** document that planning selects a valid deterministic
representation from supplied candidates; it does not recover provenance or
authorial intent.

### 17.5 Alias and duplicate ambiguity

**Risk:** catalog aliases or duplicate entries create unstable plans.

**Mitigation:** exact reference preservation, deterministic catalog policy,
canonical-only defaults for catalog planning, explicit alias opt-in if added,
and target/self-reference exclusion.

### 17.6 Incomplete candidate catalogs

**Risk:** I/O or parsing issues make a partial catalog appear complete.

**Mitigation:** propagate catalog issues into failure or explicit incompleteness;
never label a partial universe exhaustive.

### 17.7 Public API regret

**Risk:** planning exposes search internals which become permanent.

**Mitigation:** RP01 API-regret audit, immutable result abstractions, minimal
overloads, no public search-node types, and no configurable delegate surface.

---

## 18. Completion gate

Version 1.8.0 is complete when all of the following are true:

1. Callers can supply an effective target and a bounded ordered candidate set.
2. The planner considers the zero-parent baseline and legal ordered parent
   plans under explicit limits.
3. The selected result follows the frozen deterministic score and final
   candidate-order tie-break.
4. Exhaustive results match an independent brute-force oracle.
5. Budget-limited results identify that they are not globally exhaustive.
6. Selected `UseName` values and selected parent order pass unchanged to the
   frozen 1.7 synthesizer.
7. Generated source resolves semantically to the target.
8. Standard and ordinal case-sensitive extended capabilities participate in
   planning, including additions, overrides, kind changes, and cancellations.
9. Hostile and pathological candidate sets remain within frozen resource bounds.
10. Direct and routed `infocmp --plan-use` behavior is equivalent.
11. The NuGet tool package and all six standalone archives execute a real
    planning path on matching hosts.
12. Runtime, Source, Compiler, and Termcap public APIs remain unchanged.
13. Inspection retains Runtime-and-Source-only production dependencies.
14. All reusable assemblies retain version `1.0.0.0`.
15. The deterministic Toolchain sample proves plan, synthesize, source resolve,
    compile, publish, reacquire, and compare composition.
16. Windows, Linux, and macOS release gates pass on the exact stable-intended
    commit.

The concise release promise is:

> Given an effective target and an explicit bounded candidate set,
> `Icod.TermInfo.Inspection` can deterministically select and explain the best
> valid ordered parent plan under its frozen score, then produce semantically
> equivalent relative source through the unchanged 1.7 synthesizer.

---

## 19. Post-1.8 candidates

The following remain plausible later releases but are not prerequisites for
1.8:

- machine-readable Inspection JSON and database manifests;
- source-set factoring and intermediate shared-parent creation;
- alternative reviewed planning objectives;
- hashed ncurses database acquisition through an optional provider;
- documented historical Unix binary dialects;
- additional vendor source dialect compatibility;
- further command automation over explicit planning results.

All later work shall preserve the frozen explicit-synthesis and planning
contracts unless an independently justified compatibility review requires a new
major version.
