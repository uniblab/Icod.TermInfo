# Icod.TermInfo.Compiler

`Icod.TermInfo.Compiler` is the optional managed compiled-terminfo writing and
explicit database-publication layer for `Icod.TermInfo`.

## 1.14 release status

Version `1.14.0` preserves the frozen Compiler 1.2 public API, deterministic
compiled-entry and database-publication semantics, Runtime-and-Source dependency
graph, `net8.0` / `net9.0` / `net10.0` support, and assembly identity
`1.0.0.0`. The coordinated 1.14 feature work is isolated to
`Icod.TermInfo.Inspection`; Compiler does not acquire an Inspection, Termcap,
Terminal, or command-layer dependency.

## Install

```text
dotnet add package Icod.TermInfo.Compiler --version 1.14.0
```

The package depends on matching `Icod.TermInfo` and `Icod.TermInfo.Source`
versions.

## What Compiler provides

The frozen 1.2 contract includes:

- deterministic conventional compiled terminfo writing;
- legacy `0432` and wide `01036` output;
- standard and ncurses extended-capability sections;
- deterministic automatic or explicit format selection;
- strict representation validation rather than silent narrowing;
- direct compilation from immutable `TerminalDescription` values;
- `.ti` source compilation through the existing Source parser/resolver;
- explicit publication into caller-selected conventional database roots;
- canonical-name and alias publication in compatible first-byte layouts;
- explicit overwrite policy; and
- semantic round-trip, byte-determinism, and pinned ncurses/`tic` differential
  validation.

Compiler never discovers or modifies an implicit system terminfo database.
Callers must provide any publication root explicitly.

## Source compilation

Compiler composes the Source package rather than duplicating parser or `use=`
resolution semantics:

```csharp
using Icod.TermInfo.Compiler;

TermInfoCompilationResult result = TermInfoCompiler.Compile(
	source,
	"example-child"
);

if ( result.Succeeded ) {
	byte[] compiled = result.Bytes;
	Console.WriteLine( $"Compiled {compiled.Length} bytes." );
}
```

Use the public overloads appropriate to the caller's source/document or resolved
`TerminalDescription`; diagnostics and representation failures remain explicit.

## Database publication

`CompiledTermInfoDatabaseWriter` writes only to an explicit root. Existing files
are rejected by default; replacement requires an explicit options opt-in. The
resulting layout is compatible with `DirectoryTerminalDescriptionProvider`.

Compiler owns output bytes and database publication only. Inspection may use
Compiler in tests/samples for semantic round trips, but production
`Icod.TermInfo.Inspection` deliberately has no Compiler dependency.

## Compatibility

The Compiler public contract was frozen at 1.2 and remains compatible throughout
the coordinated 1.x line. Version 1.14 changes package/release identity only for
Compiler; it does not change Compiler semantics or public API.

See `../docs/VERSIONING.md`, `../docs/COMPATIBILITY.md`, and the root
`../README.md` for the coordinated release contract.
