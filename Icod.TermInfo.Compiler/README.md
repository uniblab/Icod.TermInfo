# Icod.TermInfo.Compiler

`Icod.TermInfo.Compiler` is the optional managed compiled-terminfo writing layer
for `Icod.TermInfo`.

C01 establishes the package and the pure inverse of the runtime compiled-entry
parser. The initial writer emits deterministic minimal legacy `0432` entries
from representable `TerminalDescription` values. Standard capability tables,
extended sections, and explicit format-selection policy are added by later 1.2
tranches.

## Install

For the C01 development package:

```text
dotnet add package Icod.TermInfo.Compiler --version 1.2.0-Alpha-1
```

The package targets `net8.0`, `net9.0`, and `net10.0` and depends on the matching
`Icod.TermInfo` package. C01 does not depend on `Icod.TermInfo.Source`.

## Minimal writer

```csharp
using Icod.TermInfo;
using Icod.TermInfo.Compiler;

TerminalDescription description =
	new TerminalDescriptionBuilder( "example" )
		.SetDescription( "Example terminal" )
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

The C01 writer is intentionally narrow. It rejects capability-bearing
descriptions until C02/C03 implement the corresponding tables. It also rejects
identity data that cannot be represented exactly by the conventional names
section, including missing verbose descriptions, embedded NULs, `|` separators,
and characters outside Latin-1.

The writer is pure: it does not inspect environment variables, access terminfo
directories, invoke native `tic`/ncurses, or write database layouts.