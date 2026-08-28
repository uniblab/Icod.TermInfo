# Icod.TermInfo.Inspection

`Icod.TermInfo.Inspection` is the optional managed inspection and semantic-
comparison layer for the `Icod.TermInfo` package family.

The 1.3 line provides the reusable API engine underneath future
`infocmp`-style tooling while preserving the already-frozen Runtime 1.0, Source
1.1, and Compiler 1.2 public contracts. Version 1.3.0 is the first stable
release of this optional package.

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
TermInfoInspectionTarget target =
	new(
		provider,
		"xterm",
		"system xterm"
	);

TermInfoInspectionResult inspected =
	TermInfoInspectionEngine.Inspect(
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
TermInfoInspectionComparison comparison =
	TermInfoInspectionEngine.Compare(
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
TermInfoComparisonResult sourceComparison =
	TermInfoSourceComparer.Compare(
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
TermInfoComparisonResult comparison =
	TerminalDescriptionComparer.Compare(
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
string normalized =
	TermInfoSourceRenderer.Render(
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
string source =
	TerminalDescriptionSourceRenderer.Render(
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

## Install

For the 1.3.0 release:

```text
dotnet add package Icod.TermInfo.Inspection --version 1.3.0
```

The package targets `net8.0`, `net9.0`, and `net10.0`, uses C# 13, remains
unsigned, and retains assembly version `1.0.0.0` throughout the 1.x line.

## Ownership boundary

- `Icod.TermInfo` owns immutable effective terminal descriptions and acquisition.
- `Icod.TermInfo.Source` owns `.ti` lexical, parsing, and inheritance semantics.
- `Icod.TermInfo.Compiler` owns deterministic compiled-entry/database writing.
- `Icod.TermInfo.Inspection` owns canonical human-readable representation,
  semantic comparison, and inspection orchestration.

Command-line parsing and `infocmp` executable policy remain outside this package.
