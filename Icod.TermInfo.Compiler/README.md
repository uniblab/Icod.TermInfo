# Icod.TermInfo.Compiler

`Icod.TermInfo.Compiler` is the optional managed compiled-terminfo writing and
database-layout layer for `Icod.TermInfo`.

C01 establishes the package and pure writer contract. C02 completes standard
capability emission, C03 adds the supported ncurses extended section, and C04
adds deterministic automatic and explicit `0432` / `01036` format selection.
C05 composes the 1.1 Source parser/resolver with that writer. C06 adds
controlled publication into an explicit conventional terminfo directory root.
C07 closes the implementation program with round-trip, determinism, and pinned
ncurses/`tic` differential validation.

## Install

For the 1.3.0 release:

```text
dotnet add package Icod.TermInfo.Compiler --version 1.3.0
```

The package targets `net8.0`, `net9.0`, and `net10.0` and depends on the matching
`Icod.TermInfo` and `Icod.TermInfo.Source` packages. Version 1.3.0 coordinates
Compiler with the Inspection release without changing the frozen 1.2 Compiler
public API. The dependency remains one-way; neither Source nor Runtime depends
on Compiler.

C06 adds `CompiledTermInfoDatabaseWriter`. It never discovers a system database
or installs globally: callers must supply the output root explicitly. It can
publish a successful C05 compilation result or compile resolved
`TerminalDescription` values before publication. The writer publishes canonical
names and aliases in lowercase hexadecimal first-byte directories compatible
with `DirectoryTerminalDescriptionProvider`. Existing files are rejected by
default; replacement requires an explicit
`CompiledTermInfoDatabaseWriterOptions` opt-in.

C07 adds no new public production API. Its validation suite reuses the checked-in
T29 corpus generated with pinned ncurses `tic`, compares Compiler output and the
reference binaries at the semantic `TerminalDescription` level, verifies
byte-for-byte determinism across extended-capability insertion order and culture,
and exercises temporary database output through the existing directory provider.
Normal CI remains independent of a host ncurses installation.

## Source compilation

C05 compiles a complete `.ti` source document without duplicating Source
semantics:

```csharp
using Icod.TermInfo.Compiler;

const string source =
	"""
	example-child|Example child,
		cols#132,
		use=example-base,

	example-base|Example base,
		am,
		lines#40,
	""";

TermInfoSourceCompilationResult result =
	TermInfoSourceCompiler.Compile(
		source,
		"example.ti"
	);

foreach ( CompiledTermInfoSourceEntry entry in result.Entries ) {
	byte[] compiled = entry.Data;
	// Store or load this independently as appropriate for the caller.
}
```

Entries are returned in source-document order. `use=` dependencies may appear
before or after their parents because resolution is delegated to the existing
Source resolver. Parser and resolver diagnostics are returned as the original
`TermInfoSourceDiagnostic` objects, preserving source names, lines, columns,
offsets, and spans.

Source cancellation remains source-only state. After inheritance resolution it
materializes as effective absence in `TerminalDescription`; C05 does not invent
compiled cancellation tombstones.

`CompiledTermInfoWriterOptions` can be supplied to `Compile` to retain the C04
automatic/Legacy/Wide and extended-section policies. If a resolved description
cannot be represented by the requested writer policy, the established C04
`InvalidOperationException` contract is preserved.

## Automatic format selection

The original writer operation now selects the narrowest representation which can
encode the description exactly:

```csharp
using Icod.TermInfo;
using Icod.TermInfo.Compiler;

TerminalDescription description =
	new TerminalDescriptionBuilder( "example" )
		.SetDescription( "Example terminal" )
		.SetNumber( NumericCapability.Colors, 16_777_216 )
		.SetExtendedBoolean( "AX" )
		.SetExtendedNumber( "RGB", 16_777_216 )
		.Build();

byte[] compiled =
	CompiledTermInfoWriter.Write(
		description
	);
```

When every present standard and extended numeric value is in `0..32767`, the
writer emits legacy `0432`. A representable value greater than `32767` selects
wide `01036`, where both standard and extended numeric tables use signed 32-bit
little-endian values. Negative present values remain unrepresentable because
they collide with compiled absent/canceled sentinel semantics.

## Explicit format policy

Use `CompiledTermInfoWriterOptions` when the output representation is part of the
caller's contract:

```csharp
byte[] wide =
	CompiledTermInfoWriter.Write(
		description,
		new CompiledTermInfoWriterOptions(
			CompiledTermInfoFormat.Wide
		)
	);
```

`CompiledTermInfoFormat.Legacy` emits `0432` exactly or fails if any numeric
requires the wide form. `CompiledTermInfoFormat.Wide` emits `01036` exactly even
when legacy would suffice. `Automatic` prefers legacy and upgrades only when
required.

The options also expose `IncludeExtendedCapabilities`. Setting it to `false` is
a representation constraint, not a request to discard data: a description
containing extended capabilities fails rather than being silently truncated.

All identity, capability-name, and capability-string data retain the strict
reversible Latin-1 and NUL-termination rules established by C01-C03. Standard
and extended string/name offsets remain signed 16-bit fields, section counts and
sizes remain checked, and total-entry arithmetic is checked before allocation.

Invalid arguments use normal argument exceptions. A valid `TerminalDescription`
which cannot be represented by the requested policy throws
`InvalidOperationException`. C04 freezes that distinction for the low-level
writer surface.

`CompiledTermInfoWriter` remains pure: it does not inspect environment variables,
access terminfo directories, invoke native `tic`/ncurses, or write database
layouts.
