# Icod.TermInfo.Compiler

`Icod.TermInfo.Compiler` is the optional managed compiled-terminfo writing layer
for `Icod.TermInfo`.

C01 establishes the package and pure writer contract. C02 completes standard
capability emission, C03 adds the supported ncurses extended section, and C04
adds deterministic automatic and explicit `0432` / `01036` format selection.

## Install

For the C04 development package:

```text
dotnet add package Icod.TermInfo.Compiler --version 1.2.0-Alpha-4
```

The package targets `net8.0`, `net9.0`, and `net10.0` and depends on the matching
`Icod.TermInfo` package. C04 still does not depend on `Icod.TermInfo.Source`.

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

The writer remains pure: it does not inspect environment variables, access
terminfo directories, invoke native `tic`/ncurses, or write database layouts.