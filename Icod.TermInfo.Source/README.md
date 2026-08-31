# Icod.TermInfo.Source

`Icod.TermInfo.Source` is the optional managed terminfo source-language layer
for `Icod.TermInfo`.

The package is intentionally separate from the stable runtime package. Ordinary
applications that only load compiled terminfo or use `TerminalDescription`
values continue to reference `Icod.TermInfo` alone.

## Install

For the 1.6.0 release:

```text
dotnet add package Icod.TermInfo.Source --version 1.6.0
```

The package depends on the matching `Icod.TermInfo` version and targets
`net8.0`, `net9.0`, and `net10.0`.

Version 1.6.0 participates in the coordinated Termcap/tool release without
changing the frozen 1.1 source-language public API or semantics. The
`infotocap` command consumes Source at the executable-composition layer; Source
does not acquire a Termcap dependency.

## What the 1.1 line provides

The completed 1.1 source-language path includes:

- deterministic `.ti` lexical analysis with source spans and diagnostics;
- terminfo string and numeric source-value semantics;
- unresolved documents, entries, fields, aliases, and descriptions;
- standard and extended capability classification against the runtime catalog;
- cancellation and `use=` inheritance;
- bounded inheritance-depth and source-size handling;
- materialization into the same immutable `TerminalDescription` model used by
  compiled acquisition;
- duplicate source-name and alias warnings with deterministic first-source-order
  lookup;
- a checked-in System V/ncurses-oriented source corpus, deterministic mutation
  fuzzing, and offline T29 source/compiled compatibility fixtures.

No host `tic`, `infocmp`, ncurses library, or native payload is required at
runtime or by normal CI.

## Typical flow

Parse source, resolve a named entry, and materialize it into the runtime model:

```csharp
using Icod.TermInfo;
using Icod.TermInfo.Source;

TermInfoSourceParseResult parsed = TermInfoSourceParser.Parse(
    source,
    "example.ti"
);

if ( parsed.HasErrors ) {
    throw new InvalidOperationException(
        "The terminfo source contains errors."
    );
}

TermInfoSourceResolveResult resolved = TermInfoSourceResolver.Resolve(
    parsed.Document,
    "example"
);

if ( resolved.Entry is null ) {
    throw new InvalidOperationException(
        "The terminfo entry could not be resolved."
    );
}

TerminalDescription terminal = resolved.Entry.ToTerminalDescription();
```

For source sets that are not already held in one parsed document, use the
`ITermInfoSourceEntryProvider` resolver overload. Provider misses become source
diagnostics; provider failures propagate rather than being collapsed into clean
misses.

The runtime dependency direction is one-way:

```text
Icod.TermInfo.Source
        |
        v
  Icod.TermInfo
```

`Icod.TermInfo` does not depend on this package.
