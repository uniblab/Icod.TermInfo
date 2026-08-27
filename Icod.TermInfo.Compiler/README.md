# Icod.TermInfo.Compiler

`Icod.TermInfo.Compiler` is the optional managed compiled-terminfo writing layer
for `Icod.TermInfo`.

C01 establishes the package and the pure inverse of the runtime compiled-entry
parser. C02 completes standard Boolean, numeric, and string capability emission.
C03 adds the supported ncurses extended Boolean, numeric, string, name, offset,
and alignment representation. Wide-numeric format selection remains C04.

## Install

For the C03 development package:

```text
dotnet add package Icod.TermInfo.Compiler --version 1.2.0-Alpha-3
```

The package targets `net8.0`, `net9.0`, and `net10.0` and depends on the matching
`Icod.TermInfo` package. C03 does not depend on `Icod.TermInfo.Source`.

## Standard and extended compiled writer

```csharp
using Icod.TermInfo;
using Icod.TermInfo.Compiler;

TerminalDescription description =
	new TerminalDescriptionBuilder( "example" )
		.SetDescription( "Example terminal" )
		.SetBoolean( BooleanCapability.AutoRightMargin )
		.SetNumber( NumericCapability.Columns, 80 )
		.SetString(
			StringCapability.ClearScreen,
			"\u001b[H\u001b[2J"
		)
		.SetExtendedBoolean( "AX" )
		.SetExtendedNumber( "RGB", 8 )
		.SetExtendedString(
			"Smulx",
			"\u001b[4:%p1%dm"
		)
		.Build();

byte[] compiled =
	CompiledTermInfoWriter.Write(
		description
	);

TerminalDescription parsed =
	CompiledTermInfoParser.Parse(
		compiled
	);
```

The C02 standard-table contract remains unchanged: standard positions derive
only from `StandardCapabilityCatalog.BinaryIndex`, trailing absent positions are
omitted, and interior absent positions use conventional sentinels.

C03 appends an ncurses extended section only when extended capabilities are
present. Extended Boolean, numeric, and string capabilities are grouped by kind
and ordered within each kind by ordinal, case-sensitive name. Extended string
values precede capability names in the shared string table; value offsets are
relative to the string-table start, while name offsets are relative to the name
portion, matching the runtime parser contract.

All names and string values use strict reversible Latin-1 and reject embedded
NUL. Legacy `0432` numerics remain restricted to non-negative signed 16-bit
values, and all counts, offsets, alignments, and table sizes are checked before
narrowing. Values which require `01036` remain C04 work rather than being
silently truncated.

The writer is pure: it does not inspect environment variables, access terminfo
directories, invoke native `tic`/ncurses, or write database layouts.