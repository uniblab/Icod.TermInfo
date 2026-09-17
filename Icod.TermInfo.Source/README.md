# Icod.TermInfo.Source

`Icod.TermInfo.Source` is the optional managed terminfo source-language layer for
`Icod.TermInfo`.

The package is intentionally separate from Runtime. Applications that only load
compiled terminfo or consume `TerminalDescription` values continue to reference
`Icod.TermInfo` alone.

## 1.15 release status

Version `1.15.0` preserves the frozen Source 1.1 public API, parser/resolver
semantics, Runtime-only dependency, `net8.0` / `net9.0` / `net10.0` support, and
assembly identity `1.0.0.0`. Hashed acquisition is isolated to the optional
`Icod.TermInfo.BerkeleyDb` package; Source does not acquire BerkeleyDb,
Inspection, Terminal, Termcap, or command-layer dependencies.

## Install

```text
dotnet add package Icod.TermInfo.Source --version 1.15.0
```

The package depends on the matching `Icod.TermInfo` version.

## What Source provides

The frozen 1.1 source-language contract includes:

- deterministic `.ti` lexical analysis with source spans and diagnostics;
- terminfo string and numeric source-value semantics;
- unresolved documents, entries, fields, aliases, and descriptions;
- standard and extended capability classification against Runtime metadata;
- cancellation and `use=` inheritance;
- bounded source size and inheritance depth;
- materialization into immutable `TerminalDescription` values;
- deterministic duplicate source-name and alias diagnostics; and
- checked-in source/compiled compatibility and mutation coverage.

No host `tic`, `infocmp`, ncurses library, or native payload is required by the
package or normal CI.

## Typical flow

```csharp
using Icod.TermInfo;
using Icod.TermInfo.Source;

TermInfoSourceParseResult parsed = TermInfoSourceParser.Parse(
	 source,
	 "example.ti"
);

if ( parsed.HasErrors ) {
	foreach ( TermInfoSourceDiagnostic diagnostic in parsed.Diagnostics ) {
		Console.Error.WriteLine( diagnostic );
	}
	return;
}

TermInfoSourceResolveResult resolved = TermInfoSourceResolver.Resolve(
	parsed.Document,
	"example"
);

if ( resolved.Description is not null ) {
	TerminalDescription terminal = resolved.Description;
	Console.WriteLine( terminal.Name );
}
```

Source owns parsing and inheritance resolution only. Compiled output belongs to
`Icod.TermInfo.Compiler`; canonical rendering/comparison/planning belongs to
`Icod.TermInfo.Inspection`; termcap syntax belongs to `Icod.TermInfo.Termcap`.

## Historical release record

Version `1.9.0` was the first coordinated line whose machine-readable Inspection
release closure required package-facing documentation across the full suite. That
historical marker is retained here for the corresponding release-closure gate; it
does not change the current install version above.

## Compatibility

The Source public contract was frozen at 1.1 and remains compatible throughout
the coordinated 1.x line. Version 1.15 changes package/release identity only for
Source; it does not change Source semantics or public API.

See `../docs/VERSIONING.md`, `../docs/COMPATIBILITY.md`, and the root
`../README.md` for the coordinated release contract.
