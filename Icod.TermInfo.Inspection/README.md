# Icod.TermInfo.Inspection

`Icod.TermInfo.Inspection` is the optional managed inspection and semantic-
comparison layer for the `Icod.TermInfo` package family.

The 1.3 line provides the reusable API engine underneath future
`infocmp`-style tooling while preserving the already-frozen Runtime 1.0, Source
1.1, and Compiler 1.2 public contracts.

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

During I02 development:

```text
dotnet add package Icod.TermInfo.Inspection --version 1.3.0-Alpha-2
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
