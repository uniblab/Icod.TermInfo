# Icod.TermInfo.Compiler

`Icod.TermInfo.Compiler` is the optional managed compiled-terminfo writing layer
for `Icod.TermInfo`.

C01 establishes the package and the pure inverse of the runtime compiled-entry
parser. C02 completes standard Boolean, numeric, and string capability emission
for deterministic legacy `0432` entries. Extended sections and wide-numeric
format selection remain later 1.2 tranches.

## Install

For the C02 development package:

```text
dotnet add package Icod.TermInfo.Compiler --version 1.2.0-Alpha-2
```

The package targets `net8.0`, `net9.0`, and `net10.0` and depends on the matching
`Icod.TermInfo` package. C02 does not depend on `Icod.TermInfo.Source`.

## Standard compiled writer

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

The C02 writer emits standard tables only through
`StandardCapabilityCatalog.BinaryIndex`. Trailing absent positions are omitted,
interior absent positions use conventional sentinels, numerics must fit the
legacy non-negative 16-bit range, and strings use strict reversible Latin-1 with
checked signed 16-bit offsets and an unsigned 16-bit table-size field.

Extended capabilities are still rejected until C03. Values which require the
`01036` wide-numeric format are rejected until C04 rather than truncated.

The writer is pure: it does not inspect environment variables, access terminfo
directories, invoke native `tic`/ncurses, or write database layouts.