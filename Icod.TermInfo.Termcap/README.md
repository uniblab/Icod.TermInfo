# Icod.TermInfo.Termcap

`Icod.TermInfo.Termcap` is the optional managed termcap interoperability layer for
the Icod.TermInfo package family.

## 1.14 release status

Version `1.14.0` preserves the frozen 1.6 Termcap public API, Runtime-only
production dependency, parsing/resolution/conversion/rendering/acquisition
semantics, `net8.0` / `net9.0` / `net10.0` support, and assembly identity
`1.0.0.0`. The coordinated 1.14 raster-backend work is isolated to Inspection;
Termcap acquires no Source, Compiler, Inspection, Terminal, or command dependency.

## Install

```text
dotnet add package Icod.TermInfo.Termcap --version 1.14.0
```

The package depends only on the matching `Icod.TermInfo` version.

## What Termcap provides

The frozen 1.6 contract includes:

- bounded termcap source parsing with source spans and diagnostics;
- capability classification derived from Runtime's standard capability catalog;
- `tc=` inheritance resolution;
- semantic conversion into immutable Runtime `TerminalDescription` values;
- representability analysis and deterministic reverse termcap rendering;
- explicit opt-in `TERMCAP` / `TERMPATH` and file/provider acquisition; and
- differential, hostile-input, mutation, round-trip, and package-only validation.

Command-only composition with Source or Inspection belongs to the executable
`captoinfo` / `infotocap` projects and does not change the Termcap package graph.

## Parsing and classification

```csharp
using Icod.TermInfo.Termcap;

TermcapSourceParseResult result = TermcapSourceParser.Parse(
	"vt|vt100|DEC VT100:am:co#80:cl=\\E[H\\E[2J:"
);

if ( !result.HasErrors ) {
	TermcapSourceEntry entry = result.Document.Entries[0];
	TermcapCapabilityClassificationResult classification =
		TermcapCapabilityClassifier.Classify( entry.Fields[1] );

	Console.WriteLine( classification.Mapping?.TermInfoLongName );
}
```

`TermcapCapabilityCatalog` derives standard mappings from Runtime metadata rather
than maintaining a second standard capability table. Unmapped/vendor values and
historical aliases remain explicit classification results.

## Resolution and conversion

Termcap inheritance and conversion remain opt-in and deterministic. Resolve a
parsed entry by one of its header names, then convert the resolved semantic state
into Runtime's immutable terminal-description model. Conversion does not mutate
Runtime discovery or install anything globally.

## Reverse rendering

Reverse conversion reports representation loss instead of silently inventing a
termcap encoding. Deterministic rendering and representability are part of the
frozen 1.6 public contract.

## Acquisition

Acquisition is explicit. The Termcap package does not join Runtime's terminfo
discovery path and does not make host termcap state an implicit dependency.
Callers choose the acquisition source/provider intentionally.

## Compatibility

The Termcap public surface frozen by TC08 remains immutable through the
coordinated 1.x line unless a later compatible release deliberately adds reviewed
API. Version 1.14 changes package/release identity only for Termcap and does not
change its semantics.

See `../docs/VERSIONING.md`, `../docs/COMPATIBILITY.md`, and the root
`../README.md` for the coordinated release contract.
