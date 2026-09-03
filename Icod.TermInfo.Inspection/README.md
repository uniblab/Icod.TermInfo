# Icod.TermInfo.Inspection

`Icod.TermInfo.Inspection` is the optional managed inspection and semantic-
comparison layer for the `Icod.TermInfo` package family.

## 1.10 DA05 multi-database candidate planning

`1.10.0-Alpha-5` composes the frozen 1.8 planner over canonical candidates
discovered from a complete explicit ordered database set. Candidate order follows
physical database/catalog order; target identities are excluded by the frozen
RP05 rule; semantically equal duplicate publications collapse behind the first
representative; conflicting duplicates and incomplete sets are rejected before
planning. The composed result maps frozen planner candidate indices and selected
parents back to exact database, catalog-entry, canonical-name, `use=`, and
`TerminalDescription` evidence.

See `docs/1.10.0-DA05-MULTI-DATABASE-CANDIDATE-PLANNING.md`.

## 1.10 DA04 database-set semantic and structural comparison

`1.10.0-Alpha-4` adds deterministic comparison of two ordered database sets as
both effective precedence views and physical/provenance collections. The result
separates effective winner/membership/alias changes from root topology, winner
provenance, shadow-set, completeness, and issue differences, while incomplete
inputs remain explicitly indeterminate. Cross-set terminal semantics continue to
use `TerminalDescriptionComparer`; alias scanning reuses the DA03 bound.

See
`docs/1.10.0-DA04-DATABASE-SET-SEMANTIC-AND-STRUCTURAL-COMPARISON.md`.

## 1.10 DA03 semantic duplicate, alias, and shadow analysis

`1.10.0-Alpha-3` adds bounded winner-versus-shadow semantic classification for
repeated canonical identities and deterministic alias collision analysis.
Observed conflicts retain the frozen `TermInfoComparisonResult`; alias ownership
collisions distinguish multiple canonical owners and alias-to-canonical-name
collisions, while incomplete input remains explicitly indeterminate. No all-pairs
comparison, compiled-byte equality, command output, or new JSON document kind is
introduced.

See
`docs/1.10.0-DA03-SEMANTIC-DUPLICATE-CONFLICT-ALIAS-AND-SHADOW-ANALYSIS.md`.

## 1.10 DA02 deterministic database-set precedence

`1.10.0-Alpha-2` makes the DA01 ordered database-set model operational for exact
canonical-name precedence. `LookupCanonicalName` returns structured
`NotObserved`, `WinnerKnown`, or `Indeterminate` evidence, retains every observed
occurrence, exposes later observed shadows only when a winner is conclusive, and
records incomplete databases which prevent a reliable winner or clean absence.
Aliases remain occurrence evidence rather than canonical lookup keys; semantic
equal/conflicting shadow classification remains assigned to DA03.

See `docs/1.10.0-DA02-DETERMINISTIC-MULTI-CATALOG-PRECEDENCE.md`.

## 1.10 DA01 database-set foundation

`1.10.0-Alpha-1` introduced immutable caller-ordered explicit catalog sets,
canonical occurrence indexing, constituent issue/completeness evidence, bounds,
and explicit-root or already-inspected-catalog construction.

See `docs/1.10.0-DA01-DATABASE-SET-MODEL-AND-CONTRACT-FOUNDATION.md`.

## 1.9 release status

Version `1.9.0` publishes the complete 31-type Inspection surface frozen in
`docs/1.9.0-INSPECTION-PUBLIC-API-BASELINE.txt`, the complete version-1 JSON
Schema and its packaged copy, and the validated command, package-consumer,
sample, fixture, router, and six-archive evidence.

The only public types added after 1.8 are `TermInfoJsonRenderer` and
`TermInfoJsonRendererOptions`. The four document kinds, schema identifier,
deterministic compact and indented representations, exact UTF-8 bounds, and
cancellation behavior are stable. The stable release promotes the validated
Alpha-7 contract without feature, API, schema, or command-semantic changes. See
`docs/1.9.0-RELEASE-AUDIT.md`.

## 1.9 MI06 consumer and cross-host hardening

`1.9.0-Alpha-6` leaves the MI04 public API, version-1 JSON Schema, and MI05
command semantics unchanged. The Toolchain sample now renders an exact
checked-in source-plan fixture after completing its semantic round trip. The
fresh package-reference-only consumer renders all four document kinds on
`net8.0`, `net9.0`, and `net10.0`, and hardens large escaped text, culture
independence, and exact UTF-8 bounds. Tool-package and six-archive smoke continue
to execute real `infocmp` and `toe` JSON workflows on Windows, Linux, and macOS.

See
`docs/1.9.0-MI06-SAMPLES-PACKAGE-CONSUMERS-AND-CROSS-HOST-HARDENING.md`.

The 1.3 line established the reusable inspection/comparison engine while
preserving the already-frozen Runtime 1.0, Source 1.1, and Compiler 1.2 public
contracts. Version 1.4.0 froze the reviewed additive database-inspection and
renderer-control APIs used by the managed tool suite. Version 1.7.0 adds the
frozen relative-source synthesis API while preserving all earlier Inspection
contracts. `captoinfo` consumes Inspection only at the executable-composition
layer.

## 1.8 release status

Version 1.8.0 freezes the complete additive planning public API in
`docs/1.8.0-INSPECTION-PUBLIC-API-BASELINE.txt`, retains the immutable 1.7
synthesis baseline, and makes exact API, package-consumer, deterministic sample,
direct command, router package, and six-archive validation part of the release
gate. The stable score, bounds, candidate order, exhaustive and explicitly
bounded search, cancellation evidence, and explicit catalog semantics are
recorded in `docs/1.8.0-RELEASE-AUDIT.md`. Stable 1.8.0 promotes the validated
Alpha-8 surface without changing planning or synthesis semantics.

## 1.9 MI04 database-catalog manifests and JSON Schema

`1.9.0-Alpha-4` makes the existing `TermInfoDatabaseCatalog` renderer overload
operational without adding public API. The `databaseCatalog` payload preserves
the normalized root, explicit catalog kind, derived `isComplete` evidence,
ordered entries and issues, and ordered `duplicateCanonicalNames` evidence.

```csharp
TermInfoDatabaseCatalog catalog =
	TermInfoDatabaseInspector.InspectDirectory( explicitDatabaseRoot );
string catalogJson =
	TermInfoJsonRenderer.Render(
		catalog,
		new TermInfoJsonRendererOptions(
			maximumOutputByteCount: 65_536,
			writeIndented: true
		)
	);
```

Completeness is true only for a conventional directory with no inspection
issues. Duplicate canonical names remain explicit ambiguity evidence and do not
erase otherwise complete inspection evidence. Missing, unsupported, unavailable,
malformed, permission-failed, and partial states never claim completeness.

The complete draft 2020-12 version-1 schema is published as
`docs/Icod.TermInfo.Inspection.schema.json`, included in the NuGet package, and
covered by checked-in compact and indented fixtures for all four document kinds.
Command JSON was deliberately deferred to, and is now implemented by, MI05. See
`docs/1.9.0-MI04-DATABASE-CATALOG-MANIFESTS-AND-JSON-SCHEMA.md`.

## 1.9 MI03 comparison and planning evidence JSON

`1.9.0-Alpha-3` makes the existing comparison and source-plan renderer
overloads operational without adding public API. Comparison documents preserve
the existing deterministic difference order and expose exact kind strings,
capability identity, typed side values, and retained source entry, field, index,
and span evidence.

```csharp
TermInfoComparisonResult comparison =
	TerminalDescriptionComparer.Compare( left, right );
string comparisonJson =
	TermInfoJsonRenderer.Render( comparison );

TerminalDescriptionSourcePlan plan =
	TerminalDescriptionSourcePlanner.Plan( target, candidates );
string planJson =
	TermInfoJsonRenderer.Render( plan );
```

Each comparison side has one stable shape. Inapplicable text, aliases,
capability values, source entries, indices, fields, and spans are explicit JSON
null. Source-field summaries retain classification, canonical identity, decoded
value, lexical text, and span without recursively embedding unrelated fields.

Plan documents retain selected parent count and ordered `UseName` values,
generated LF source, all score components, selected candidate indices,
evaluated-plan count, `isExhaustive`, and accepted candidate count. Rendering
reports the supplied immutable result and does not recompute a comparison or
rerun planning. MI04 has since activated catalog payloads and published the
complete schema. See
`docs/1.9.0-MI03-COMPARISON-AND-PLANNING-EVIDENCE-JSON.md`.

## 1.9 MI02 effective-description JSON

`1.9.0-Alpha-2` makes `TermInfoJsonRenderer.Render(TerminalDescription)`
operational. The version-1 payload preserves the exact terminal name, immutable
alias order, and nullable description, followed by typed standard and extended
capability arrays.

```csharp
TerminalDescription terminal =
	TerminalDatabase.BuiltIn.Load( "xterm-256color" );
string json =
	TermInfoJsonRenderer.Render(
		terminal,
		new TermInfoJsonRendererOptions(
			maximumOutputByteCount: 65_536,
			writeIndented: true
		)
	);
```

Standard capabilities use canonical terminfo short names and compiled database
order. Extended capabilities are grouped by Boolean, number, and string kind,
then ordered by exact ordinal name. Compact output is canonical; indented output
uses LF, two spaces, and no trailing line terminator. The bound is applied to the
exact final UTF-8 byte count, and control characters use the default safe JSON
escaping policy. MI03 has since activated comparison and planning payloads, and
MI04 has activated catalog payloads. See
`docs/1.9.0-MI02-EFFECTIVE-DESCRIPTION-JSON.md`.

## 1.9 MI01 machine-readable renderer foundation

`1.9.0-Alpha-1` adds `TermInfoJsonRendererOptions` and
`TermInfoJsonRenderer`. The immutable default policy permits up to 4,194,304
UTF-8 output bytes, uses compact presentation, and exposes no mutable
`JsonSerializerOptions`, converter, naming policy, encoder, or callback.

```csharp
TermInfoJsonRendererOptions options =
	new(
		maximumOutputByteCount: 8_192,
		writeIndented: true
	);
```

The exact schema identifier is `urn:icod:terminfo:inspection:json:1`, with
schema version `1`. The renderer has typed entry points for
`TerminalDescription`, `TermInfoComparisonResult`,
`TerminalDescriptionSourcePlan`, and `TermInfoDatabaseCatalog`.

MI01 validated arguments, bounds, and pre-cancellation, then deliberately threw
`NotSupportedException` for all payloads. MI02 activated description rendering,
MI03 activated comparison and plan rendering, and MI04 activated catalog
rendering and published the complete version-1 schema. See
`docs/1.9.0-MI01-JSON-CONTRACT-AND-RENDERER-FOUNDATION.md`.

## 1.8 RP07 generated-state oracle and hardening

`1.8.0-Alpha-7` leaves the planner and public Inspection API unchanged while
adding seeded generated target/candidate universes, an independent brute-force
oracle, exhaustive and budget-prefix comparison, score-tie and permutation
coverage, exact-boundary tests, culture and insertion-order determinism, and
repeated-process output comparison. Every selected generated source is resolved
and compared with its original target.

RP07 also reuses the pinned `ncurses 6.5.20250216` effective-state corpus and
extends the managed Toolchain sample through explicit candidate planning,
compilation, publication, Runtime reacquisition, and semantic comparison. See
`docs/1.8.0-RP07-GENERATED-STATE-ORACLE-AND-HARDENING.md` for the complete
evidence contract.

## 1.8 RP06 command and distribution composition

`1.8.0-Alpha-6` exposes the existing bounded planner through
`infocmp --plan-use`, the `icod-terminfo infocmp` route, the installable tool
package, and all six standalone archive RIDs. RP06 adds no Inspection API and
changes no RP01 through RP05 semantics. The command maps explicit acquisition,
presentation, and bounds into the reusable immutable planning options.

See `docs/1.8.0-RP06-INFOCMP-PLANNING-COMMAND-AND-DISTRIBUTION.md` for the
reviewed command and distribution contract.

## 1.8 RP05 explicit database catalog planning

`1.8.0-Alpha-5` composes the bounded planner with an explicit
`TermInfoDatabaseCatalog` or one explicit conventional database directory. It
does not consult environment discovery or platform default database locations.

```csharp
TerminalDescriptionSourcePlan plan =
	TerminalDescriptionSourcePlanner.PlanFromDirectory(
		target,
		explicitDatabaseRoot,
		planningOptions,
		parserOptions,
		cancellationToken
	);
```

Use `PlanFromCatalog` when the caller already owns an immutable catalog
snapshot. Catalog planning requires a conventional, issue-free catalog. Missing,
unsupported, unavailable, malformed, misplaced, inaccessible, or link-skipping
catalogs are rejected before plan evaluation so partial candidates never produce
false exhaustive evidence.

Candidates use canonical names only and retain the catalog's ordinal canonical-
name order. Alias publications and equivalent physical copies collapse to one
candidate; conflicting copies of the same canonical name are rejected. Any
catalog entry whose canonical name or aliases intersects the target name or
aliases is excluded as an obvious self-reference.

Parser limits, planning bounds, cancellation, the frozen score, and exhaustive
versus bounded result semantics remain unchanged. See
`docs/1.8.0-RP05-EXPLICIT-DATABASE-CATALOG-PLANNING.md` for the complete policy.

## 1.8 RP04 bounded search, cancellation, and evidence

`1.8.0-Alpha-4` freezes the planner's hostile-input behavior without changing
the RP01 public API. Candidate count, selected-parent depth, evaluated-plan
count, and generated-source length are enforced independently. Plan-space
arithmetic is checked and budget-aware, so even the supported 256-candidate and
256-parent maxima cannot wrap an integer or materialize a factorial plan list.

Exhaustive planning rejects a request before source evaluation when its complete
legal space exceeds `MaximumEvaluatedPlanCount`. A caller that explicitly sets
`AllowNonExhaustiveResult` receives the best plan from the deterministic
increasing-depth lexicographic prefix ending at that budget. Such a result always
reports `IsExhaustive` as `false`.

```csharp
TerminalDescriptionSourcePlanningOptions options =
	new(
		new TerminalDescriptionSourceSynthesisOptions(
			80,
			maximumParentCount: 3
		),
		maximumCandidateCount: 64,
		maximumSelectedParentCount: 3,
		maximumEvaluatedPlanCount: 10_000,
		allowNonExhaustiveResult: true
	);

TerminalDescriptionSourcePlan plan =
	TerminalDescriptionSourcePlanner.Plan(
		target,
		candidates,
		options,
		cancellationToken
	);
```

Cancellation is observed while candidates are snapshotted, throughout ordered
enumeration, and immediately before and after each synchronous synthesis call.
A cancellation or bounds failure returns no partial plan. The immutable
`EvaluatedPlanCount`, `IsExhaustive`, and `CandidateCount` properties explain
the completed search without exposing mutable internal state.

## 1.8 RP03 ordered multi-parent planning

`1.8.0-Alpha-3` evaluates the zero-parent baseline and every ordered
permutation of distinct candidate positions through the configured parent
depth. Different parent orders are distinct plans because the frozen 1.7
synthesizer resolves collisions using leftmost precedence.

```csharp
TerminalDescriptionSourcePlan plan =
	TerminalDescriptionSourcePlanner.Plan(
		target,
		new[] {
			new TerminalDescriptionSourceSynthesisParent(
				"preferred-base",
				preferredBase
			),
			new TerminalDescriptionSourceSynthesisParent(
				"supplemental-base",
				supplementalBase
			),
		},
		new TerminalDescriptionSourcePlanningOptions(
			new TerminalDescriptionSourceSynthesisOptions(
				80,
				maximumParentCount: 2
			),
			maximumCandidateCount: 2,
			maximumSelectedParentCount: 2
		)
	);
```

Enumeration starts with the baseline, then proceeds by increasing depth and
lexicographic candidate-index sequence. One position cannot repeat within a
plan, but equal descriptions and aliases at different caller positions remain
distinct candidates. The winning `SelectedParents`, score indices, and emitted
`use=` directives all retain the exact selected order.

The default limit of 4,097 evaluations exactly covers the baseline, 64 single-
parent plans, and 4,032 ordered two-parent plans. A larger admitted space is
rejected before evaluation unless `AllowNonExhaustiveResult` is enabled; an
opted-in prefix result reports `IsExhaustive` as `false`.

## 1.8 RP02 zero- and single-parent planning

`1.8.0-Alpha-2` makes the RP01 planner operational for the zero-parent baseline
and every legal single candidate position. Planning snapshots candidates once,
delegates every source candidate to the frozen 1.7 synthesizer, scores semantic
emission evidence without parsing generated text, and returns the deterministic
best valid plan.

```csharp
using Icod.TermInfo;
using Icod.TermInfo.Inspection;

TerminalDescription target =
	TerminalDatabase.BuiltIn.Load( "xterm-256color" );
TerminalDescription parent =
	TerminalDatabase.BuiltIn.Load( "xterm" );

TerminalDescriptionSourcePlan plan =
	TerminalDescriptionSourcePlanner.Plan(
		target,
		new[] {
			new TerminalDescriptionSourceSynthesisParent(
				"xterm",
				parent
			),
		},
		new TerminalDescriptionSourcePlanningOptions(
			new TerminalDescriptionSourceSynthesisOptions(),
			maximumSelectedParentCount: 1
		)
	);
```

`SelectedParents` retains the exact winning `UseName` spelling. `Score`,
`EvaluatedPlanCount`, `IsExhaustive`, and `CandidateCount` provide immutable
selection evidence. A candidate is rejected if synthesis cannot reproduce the
target under the active policy or its source exceeds the configured length; the
planner never substitutes an approximate result.

RP02's complete search depth was zero and one. A selected-parent limit of zero
still restricts planning to the baseline, and an explicit limit of one retains
the RP02 search domain. RP03 implements the larger ordered multi-parent legal
space. `IsExhaustive` retains its RP01 meaning across all plans admitted by the
active limits.

## 1.8 RP01 relative-source planning contract

`1.8.0-Alpha-1` begins additive relative-source planning in Inspection. RP01
introduces `TerminalDescriptionSourcePlanningOptions`,
`TerminalDescriptionSourcePlanningScore`, `TerminalDescriptionSourcePlan`, and
`TerminalDescriptionSourcePlanner`. Candidate inputs and selected outputs reuse
the frozen `TerminalDescriptionSourceSynthesisParent` type.

The canonical immutable policy accepts 64 candidate positions, considers up to
two selected ordered parents, and budgets 4,097 evaluations. That budget exactly
covers the zero-parent plan, all 64 single-parent plans, and all `64 * 63`
ordered two-parent plans. The score prefers fewer local directives, fewer
cancellations, fewer parents, fewer rendered UTF-8 bytes, and then earlier
candidate-index sequences.

RP01 validates and snapshots candidate input once, preserves equivalent positions,
excludes ordinal target-name and target-alias self-references, and freezes
exhaustive versus budget-limited result evidence. RP02 supplies operational zero-
and single-parent planning; RP03 extends it to ordered multi-parent plans. The
frozen 1.7 synthesizer public contract is
unchanged, and Inspection retains only Runtime and Source production dependencies.

## 1.7 synthesis contract

Version 1.7.0 freezes the additive relative-source synthesis surface in
`docs/1.7.0-INSPECTION-PUBLIC-API-BASELINE.txt`. The frozen additions are
`TerminalDescriptionSourceSynthesisParent`,
`TerminalDescriptionSourceSynthesisOptions`, and
`TerminalDescriptionSourceSynthesizer`. Their standard/extended delta,
cancellation, exact ordered-parent reference, deterministic LF rendering, and
semantic round-trip contracts are the stable 1.7 compatibility boundary.

Inspection continues to target `net8.0`, `net9.0`, and `net10.0`, retains
assembly version `1.0.0.0`, and depends in production only on matching Runtime
and Source packages.

## Install

```text
dotnet add package Icod.TermInfo.Inspection --version 1.9.0
```

The package targets `net8.0`, `net9.0`, and `net10.0`, depends on matching
Runtime and Source packages, and retains no production Compiler or Termcap
dependency.

## Relative-source synthesis

The synthesizer accepts already effective terminal descriptions. Acquisition,
parent selection, and the exact reference spelling to emit remain caller-owned:

```csharp
using Icod.TermInfo;
using Icod.TermInfo.Inspection;

TerminalDescription target =
	TerminalDatabase.BuiltIn.Load( "xterm-256color" );
TerminalDescription parent =
	TerminalDatabase.BuiltIn.Load( "xterm" );

string source = TerminalDescriptionSourceSynthesizer.Synthesize(
	target,
	new[] {
		new TerminalDescriptionSourceSynthesisParent(
			"xterm",
			parent
		),
	}
);
```

For multiple parents, array order is the exact emitted `use=` order and is part
of the semantic input:

```csharp
TerminalDescription primary =
	TerminalDatabase.BuiltIn.Load( "xterm" );
TerminalDescription fallback =
	TerminalDatabase.BuiltIn.Load( "vt100" );

string source = TerminalDescriptionSourceSynthesizer.Synthesize(
	target,
	new[] {
		new TerminalDescriptionSourceSynthesisParent( "primary", primary ),
		new TerminalDescriptionSourceSynthesisParent( "fallback", fallback ),
	},
	new TerminalDescriptionSourceSynthesisOptions(
		100,
		TerminalDescriptionSourceLayout.Canonical,
		TerminalDescriptionSourceCapabilityOrder.TermInfoName,
		maximumParentCount: 64,
		includeExtendedCapabilities: true
	)
);
```

The leftmost parent has highest parent-to-parent priority, while explicit local
target state outranks every parent. Parents are never reordered or pruned.
`UseName` may intentionally be an alias and is preserved exactly.

Setting `IncludeExtendedCapabilities` to `false` is accepted only when no local
extended declaration or cancellation is required to reproduce the target. The
synthesizer throws `InvalidOperationException` rather than emitting source with
different effective semantics.

## 1.7 RS04 ordered multi-parent/reference fidelity

`1.7.0-Alpha-4` freezes caller-supplied parent order and exact `UseName`
spelling across the complete standard and extended capability universe.
Parent aggregation continues right-to-left so present values from leftward
parents win collisions, while emitted `use=` fields preserve the original
left-to-right parent sequence without sorting, canonicalization, or pruning.

`UseName` is source-reference identity and may intentionally be an alias rather
than `Description.Name`. The same effective `TerminalDescription` may also be
supplied more than once under distinct valid references; duplicate `UseName`
values remain rejected under the ordinal, case-sensitive RS01 policy.

RS04 adds no public API. Source-backed fixtures independently resolve use-only
multi-parent entries to verify that synthesis assumptions match the frozen
Source precedence contract.

## 1.7 RS03 extended capability synthesis

`1.7.0-Alpha-3` extends the relative synthesis engine across the complete
`TerminalDescription` capability universe. Extended names use ordinal,
case-sensitive identity; target-only values are declared, equal inherited values
are omitted, inherited removals produce `name@`, and target overrides may change
Boolean, numeric, and string value kind without a separate cancellation.

`TerminalDescriptionSourceSynthesisOptions.IncludeExtendedCapabilities` defaults
to `true`. The existing constructor remains available, and a new additive
five-argument overload can disable local extended directives. Disabling them is
accepted only when the target already matches the ordered-parent extended
aggregate; otherwise synthesis fails explicitly rather than emitting source with
false round-trip semantics.

## 1.7 RS02 standard capability delta and cancellation

`1.7.0-Alpha-2` makes the RS01 parented synthesis contract operational for every
standard Boolean, numeric, and string capability. The synthesizer computes the
effective ordered-parent baseline, omits inherited values which already match
the target, emits target-local additions and overrides, and emits `cap@`
cancellations when inherited state must be removed.

Parent order is preserved exactly and follows the existing Source precedence
contract: the leftmost parent has the highest parent-to-parent priority. The
target header remains authoritative, and existing layout and capability-order
options remain deterministic.

Extended-capability relative synthesis remains RS03 work. A parented request
containing target or parent extended capabilities fails explicitly rather than
emitting source whose effective semantics could differ from the target.

## 1.7 RS01 relative-source synthesis contract

`1.7.0-Alpha-1` begins additive relative terminfo source synthesis in Inspection.
RS01 introduces `TerminalDescriptionSourceSynthesisParent`,
`TerminalDescriptionSourceSynthesisOptions`, and
`TerminalDescriptionSourceSynthesizer`. Parent references are explicit and
ordered, reference names are unique under ordinal comparison, and synthesis is
bounded to 64 parents by default with a hard supported maximum of 256.

The zero-parent form already delegates to the existing effective source renderer.
Relative capability delta and cancellation execution for one or more parents is
reserved for RS02 so Alpha-1 does not emit semantically incomplete `use=` source.
Runtime, Source, Compiler, and Termcap public APIs remain unchanged, and
Inspection retains no production Compiler or Termcap dependency.

## 1.4 T07 semantic-comparison composition

`1.4.0-Alpha-7` advances the coordinated package family while the managed
`infocmp` command composes the existing `TerminalDescriptionComparer` for
difference reporting. Common-capability and absent-standard-capability reporting
remain command-layer policy over already-acquired immutable descriptions.

T07 adds no Inspection public API. The frozen
`docs/1.4.0-INSPECTION-PUBLIC-API-BASELINE.txt` therefore remains unchanged from
the reviewed T06 surface and remains the frozen Inspection contract in 1.5.0.

## 1.4 T06 effective-source renderer controls

`1.4.0-Alpha-6` adds reviewed additive presentation controls used by the managed
`infocmp` command while preserving the frozen 1.3 renderer overload output.

```csharp
TerminalDescriptionSourceRendererOptions options = new(
	100,
	TerminalDescriptionSourceLayout.Canonical,
	TerminalDescriptionSourceCapabilityOrder.TermInfoName,
	includeExtendedCapabilities: false
);

string source = TerminalDescriptionSourceRenderer.Render(
	description,
	options
);
```

The configurable renderer supports canonical wrapping at a caller-selected width,
a single logical line, one capability per line, standard-capability ordering by
compiled-table position, terminfo short name, long variable name, or termcap
code, and explicit inclusion/exclusion of effective extended capabilities.
Ordering is ordinal and deterministic.

A parameterless `TerminalDescriptionSourceRendererOptions` value represents the
frozen canonical policy: width 80, canonical layout, compiled-table ordering, and
extended capabilities included. The renderer routes that exact policy through the
existing implementation so the released 1.3 `Render`/`Write` behavior remains
unchanged.

T06 adds no Runtime, Source, or Compiler public API and does not add a production
Compiler dependency to Inspection. The reviewed additive surface is recorded in
`docs/1.4.0-INSPECTION-PUBLIC-API-BASELINE.txt`.

## 1.4 T03 conventional database catalog enumeration

`1.4.0-Alpha-3` adds safe read-only enumeration of one explicit conventional
terminfo directory root. The catalog parses candidate files through the Runtime
`CompiledTermInfoParser` rather than trusting filenames, and returns immutable
physical-entry metadata together with deterministic non-fatal issues:

```csharp
TermInfoDatabaseCatalog catalog = TermInfoDatabaseInspector.InspectDirectory(
	"./terminfo"
);

foreach ( TermInfoDatabaseCatalogEntry entry in catalog.Entries ) {
	Console.WriteLine(
		$"{entry.Name}: {entry.Description}"
	);
}
```

T03 recognizes only immediate literal first-character and two-digit hexadecimal
subdirectories. It does not recursively crawl arbitrary trees. Successfully
parsed entries retain their absolute physical path, canonical name, aliases,
description, and immutable `TerminalDescription`. Duplicate canonical identities
are reported separately from the physical entries which produced them.

Malformed, misplaced, inaccessible, and skipped link/reparse candidates are
reported through deterministic catalog issues so callers such as the later
`toe` command can continue through mixed-quality databases without silently
losing failures. Missing roots, conventional directories, unsupported
non-directory stores, and unavailable roots are distinguished explicitly.

Parser resource limits are snapshotted for the inspection, cancellation is
supported by an explicit overload, and no filesystem mutation occurs. T03 adds
no Runtime, Source, or Compiler public API and does not add a production Compiler
dependency to Inspection.

## 1.4 T02 system database-location inspection

`1.4.0-Alpha-2` adds read-only inspection of the ordered system database
locations a newly created Runtime system provider would consider. The API is
intended for later `tic -D`, `infocmp -D`, and `toe` composition:

```csharp
IReadOnlyList<TermInfoDatabaseLocation> locations = TermInfoDatabaseInspector.GetSystemLocations();
```

Each location identifies whether it came from encoded `TERMINFO`, directory
`TERMINFO`, the user database, `TERMINFO_DIRS`, or a final platform default.
Directory paths are normalized and preserve Runtime precedence and duplicate-root
semantics. Encoded `TERMINFO` is reported without exposing its payload. T02 does
not enumerate database contents; conventional catalog enumeration remains T03.

The Runtime 1.0 public API remains unchanged. Inspection consumes a narrow
internal Runtime discovery seam and continues to have no production dependency
on `Icod.TermInfo.Compiler`.

## I07 differential validation, robustness, and API/package freeze

`1.3.0-Alpha-7` closes the 1.3 implementation program without adding another
production API surface. The reviewed I02-I06 Inspection API is now the candidate
1.3 contract, and the existing public API baseline is treated as frozen for
release closure.

I07 adds cross-layer validation which deliberately uses
`Icod.TermInfo.Compiler` only from the Inspection test project. Effective terminal
descriptions are rendered through Inspection, compiled from the resulting Source,
parsed back through Runtime, and compared semantically. The production package
continues to depend only on Source and Runtime:

```text
Inspection -> Source -> Runtime
Inspection ----------> Runtime
```

The validation corpus covers every built-in profile, the pinned T29 compiled
fixtures, and the checked-in Source corpus. It also locks exact wrapping
boundaries, culture-independent and insertion-order-independent comparison
ordering, source cancellation/disabled/`use=`/duplicate sequencing, and the
four-package release boundary.

Ordinary CI remains independent of a host ncurses installation. Differential
evidence is semantic: I07 does not claim byte-for-byte formatting identity with
`infocmp`, and it does not change the existing Runtime, Source, or Compiler public
contracts.

## I06 provider-aware inspection and reusable `infocmp` engine

`1.3.0-Alpha-6` composes the existing Runtime acquisition contract with the I02
canonical renderer and I04 effective comparer. An inspection target contains an
explicit `ITerminalDescriptionProvider`, the exact requested terminal name, and
an optional caller-owned display label:

```csharp
TermInfoInspectionTarget target = new(
	provider,
	"xterm",
	"system xterm"
);

TermInfoInspectionResult inspected = TermInfoInspectionEngine.Inspect(
	target
);
```

`TryInspect` preserves the Runtime provider contract's clean-miss semantics;
provider exceptions continue to propagate. Successful results retain both the
requested target identity and the provider-returned canonical
`TerminalDescription`, so aliases do not erase what the caller actually asked
for.

The engine can render a target or an already acquired result and can compare two
targets or two acquired results:

```csharp
TermInfoInspectionComparison comparison = TermInfoInspectionEngine.Compare(
	leftTarget,
	rightTarget
);
```

The comparison retains both target/result identities together with the I04
`TermInfoComparisonResult`. Already acquired results are never reacquired when
rendered or compared.

The optional display label is caller-owned diagnostic context only. I06 does not
enumerate providers, expose private system-discovery internals, infer the exact
compiled database path used by a provider, or add command-line/console-output
policy. `SystemTerminalDescriptionProvider`, separate
`DirectoryTerminalDescriptionProvider` roots, `TerminalDatabase.BuiltIn`, and
caller-defined providers all participate through the same frozen Runtime
interface.

## I05 source-aware comparison

`1.3.0-Alpha-5` adds deterministic comparison of unresolved Source 1.1
entries and documents:

```csharp
TermInfoComparisonResult sourceComparison = TermInfoSourceComparer.Compare(
	leftEntry,
	rightEntry
);
```

The same comparer accepts `TermInfoSourceDocument` values and compares entries
in document order. Entry identity metadata is compared separately from ordered
fields. Field comparison keeps duplicate declarations and position observable,
distinguishes `use=` reference changes, local value changes, one-sided fields,
and field-kind changes such as present versus cancelled or disabled.

`TermInfoDifference` now carries the retained Source entry/field objects, their
zero-based document/field indexes when available, and the most specific retained
source spans for actual differences. Source spans themselves are not treated as
semantic differences. Comments, incidental whitespace, and equivalent lexical
spellings of successfully decoded values likewise do not make two source models
different.

Effective comparison remains deliberately separate: two source programs may
differ structurally while resolving to identical `TerminalDescription` values.
Call `TermInfoSourceComparer` when source program structure matters and
`TerminalDescriptionComparer` when only effective terminal semantics matter.

## I04 effective semantic comparison

`1.3.0-Alpha-4` adds deterministic, machine-readable comparison of effective
`TerminalDescription` values:

```csharp
TermInfoComparisonResult comparison = TerminalDescriptionComparer.Compare(
	left,
	right
);
```

`TermInfoComparisonResult.Differences` contains structured
`TermInfoDifference` values. Identity metadata is reported separately from
capabilities; standard capabilities are compared in canonical Runtime metadata
order; and extended capabilities are matched by exact ordinal, case-sensitive
name. Extended value-kind mismatches remain distinct from ordinary value
differences.

The effective comparer reports left-only, right-only, value, and value-kind
differences without rendering either terminal to text. It does not invent source
cancellation, disabled-field, `use=`, or provenance information because those
facts are not retained by `TerminalDescription`. I05 source-aware comparison is
therefore a separate operation over the unresolved Source model.

## I03 normalized unresolved-source rendering

`1.3.0-Alpha-3` adds normalized rendering for the unresolved Source 1.1 model:

```csharp
string normalized = TermInfoSourceRenderer.Render(
	parsed.Document
);
```

The same API accepts a single `TermInfoSourceEntry`, and both entry/document
forms have caller-owned `TextWriter` overloads.

I03 preserves entry order and field order, including duplicate declarations,
`use=` placement, cancellation, and disabled fields. It does not flatten
inheritance. Boolean, numeric, and string fields are regenerated from structured
Source values with invariant numeric spelling, canonical source escaping, LF line
endings, and deterministic wrapping.

The renderer intentionally does not reproduce comments, original whitespace,
source spans, or equivalent lexical spellings such as hexadecimal versus decimal
numbers. Disabled operands are not structured Source state, so a declaration such
as `.clear=\E[H` normalizes to `.clear` while retaining its ordered disabled-field
semantics. A numeric or string field with no successfully decoded value fails with
`InvalidOperationException` instead of substituting opaque malformed text.

## I02 canonical effective source rendering

`1.3.0-Alpha-2` introduces the first public Inspection API:

```csharp
string source = TerminalDescriptionSourceRenderer.Render(
	terminal
);
```

The same canonical representation can be written to a caller-owned
`TextWriter`:

```csharp
TerminalDescriptionSourceRenderer.Write(
	writer,
	terminal
);
```

The renderer operates only on effective `TerminalDescription` state. It emits
canonical name, aliases, description, standard capabilities, and extended
capabilities in deterministic order. Standard capabilities use the Runtime
metadata catalog; extended capabilities are ordered by value kind and then by
ordinal, case-sensitive name.

Output uses LF line endings, four-space capability indentation, deterministic
80-character wrapping for string values, invariant-culture numeric spelling,
and reversible terminfo source escapes for the supported Latin-1 byte model.

Effective absence is omitted. The renderer does not invent `use=` inheritance,
cancellation tombstones, disabled fields, comments, or source locations because
those facts are no longer present in a `TerminalDescription`.

Some effective states cannot be represented losslessly by the frozen Source 1.1
grammar. Those cases fail with `InvalidOperationException` rather than silently
changing semantics. Examples include negative numeric values, embedded NUL,
non-Latin-1 string characters, and identity headers whose alias/description
shape would be reinterpreted by the Source parser.

## Dependency graph

```text
Inspection -> Source -> Runtime
Inspection ----------> Runtime

Compiler   -> Source -> Runtime
Compiler   ------------> Runtime
```

There is no production dependency between Inspection and Compiler.

## Historical 1.3 package contract

For the 1.3.0 release, the package installed as:

```text
dotnet add package Icod.TermInfo.Inspection --version 1.3.0
```

The package targeted `net8.0`, `net9.0`, and `net10.0`, used C# 13, remained
unsigned, and retained assembly version `1.0.0.0`; those 1.x identity and target
framework guarantees continue through 1.6.0.

## Ownership boundary

- `Icod.TermInfo` owns immutable effective terminal descriptions and acquisition.
- `Icod.TermInfo.Source` owns `.ti` lexical, parsing, and inheritance semantics.
- `Icod.TermInfo.Compiler` owns deterministic compiled-entry/database writing.
- `Icod.TermInfo.Inspection` owns canonical human-readable representation,
  relative-source synthesis, semantic comparison, inspection orchestration, and
  read-only database catalog inspection.

Command-line parsing and `infocmp` executable policy remain outside this package.
