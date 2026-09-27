# Icod.TermInfo

![Icod TUI Toolchain](https://raw.githubusercontent.com/uniblab/Icod.TermInfo/v1.4.1/icod_tui_toolchain.jpg)

[![PR Staging build](https://github.com/uniblab/Icod.TermInfo/actions/workflows/pull-request.yaml/badge.svg)](https://github.com/uniblab/Icod.TermInfo/actions/workflows/pull-request.yaml)
[![Main Release validation](https://github.com/uniblab/Icod.TermInfo/actions/workflows/main.yaml/badge.svg?branch=main)](https://github.com/uniblab/Icod.TermInfo/actions/workflows/main.yaml)

`Icod.TermInfo` is a managed, cross-platform .NET implementation of the low-level terminal-capability model traditionally supplied by `libtinfo`. The runtime package is dependency-free; optional sibling packages add terminfo source parsing, compilation, termcap interoperability, inspection, comparison, planning, and automation without enlarging the core runtime contract.

## Status

Current release line: `Icod.TermInfo 1.17.0-Alpha-4`.

The 1.17 development branch is building **Unified directory/hashed catalogs**.
Alpha-4 adds `TerminalCatalogReader` in `Icod.TermInfo.Catalogs`: choose a
conventional directory or Hash-v9 file explicitly, then read actual publications
through one bounded, read-only result model. Directory and hashed adapters retain
parsed terminal identity, provenance, typed diagnostics, and inclusive resource
budgets. See the [Catalogs guide](Icod.TermInfo.Catalogs/README.md) for usage.
See the [1.17 roadmap](Icod.TermInfo-1.17.0-Unified-Directory-Hashed-Catalogs-Roadmap.md)
and [approved contract](docs/1.17.0-UC00-UNIFIED-CATALOG-CONTRACT.md).

The published stable release is **1.16.0**. Installation examples below use that
published version. Alpha-1 through Alpha-3 are accepted; Alpha-4 qualification
is tracked in the roadmap.

Version 1.16 adds deterministic, pure-managed Berkeley DB Hash-v9 publication
to `Icod.TermInfo.BerkeleyDb`, alongside exact lookup, logical catalogs, and
opt-in hashed-aware discovery. The public surface is the unchanged nine-type
1.15 reader plus three writer types, with a Runtime-only dependency and
equivalent APIs on .NET 8, 9, and 10.

Publish compiled entries with `BerkeleyDbTerminalDatabaseWriter.Write`, or
compile source directly with:

```sh
icod-terminfo tic --database-format hashed -o ./terminfo.db source.ti
```

Directory publication remains the default. Hashed output requires an exact
destination file and an existing parent directory; `--force` permits whole-file
replacement. Publication uses a persistent cooperative lock and verifies staged
bytes before commit. See the [writing guide](docs/1.16.0-BERKELEY-DB-WRITING-GUIDE.md)
for limits, cancellation, errors, and filesystem guarantees.

Runtime discovery, sibling public APIs, and JSON contracts remain unchanged.
Native Berkeley DB and ncurses are qualification tools, never production
dependencies. Unified catalogs are the selected 1.17 scope; migration and broader
catalog automation remain deferred without an assigned release.
The [release audit](docs/1.16.0-RELEASE-AUDIT.md) records exact qualification
and publication status.

## Support the Project


`Icod.TermInfo` and its ecosystem packages (`Icod.Terminal` and `Icod.DCurses`) are built and maintained by a solo developer. If these packages save you or your team time, please consider supporting their continued development and maintenance.

[![GitHub Sponsors](https://img.shields.io/badge/GitHub-Sponsor?logo=githubsponsors)](https://github.com/sponsors/uniblab)
[![Ko-fi](https://img.shields.io/badge/Ko--fi-Support?logo=kofi)](https://ko-fi.com/TimothyBruce)
[![PayPal](https://img.shields.io/badge/PayPal-Support?logo=paypal)](https://paypal.me/uniblab)

## Architecture

`Icod.TermInfo` is the capability-data foundation of the Icod terminal stack. Higher layers may stop at whichever abstraction they need:

```text
higher-level terminal applications
             |
        Icod.DCurses
             |
        Icod.Terminal
             |
     Icod.TermInfo family
             |
 terminal capability databases
```

- `Icod.TermInfo` owns immutable terminal descriptions, capability metadata, compiled terminfo acquisition, expansion, color semantics, padding-aware output, and built-in terminal profiles.
- `Icod.Terminal` owns the live terminal conversation: input, lifecycle, active queries, semantic protocols, terminal state, raster execution, and reversible session ownership.
- `Icod.DCurses` owns higher-level cells, windows, pads, retained panels, layout, composition, refresh/damage policy, and curses-style interaction abstractions.

The reusable TermInfo package family is deliberately layered:

```text
Icod.TermInfo                    runtime / capability authority
├── Icod.TermInfo.Source         .ti parsing and resolution
├── Icod.TermInfo.Termcap        termcap interoperability
├── Icod.TermInfo.Compiler       compilation / conventional database writing
├── Icod.TermInfo.Inspection     inspection / comparison / planning
└── Icod.TermInfo.BerkeleyDb     optional hashed acquisition and publication

Icod.TermInfo.Tools              command distribution / routing
```

Dependency boundaries are explicit: Runtime has no production package dependencies; Source, Termcap, and BerkeleyDb each depend on Runtime; Compiler and Inspection each depend on Runtime and Source. Inspection does not depend on BerkeleyDb, Compiler, Termcap, `Icod.Terminal`, or `Icod.DCurses`. The `infocmp` and `toe` executables reference BerkeleyDb for explicit hashed acquisition; `tic` composes Compiler and BerkeleyDb for explicit hashed publication.

The 1.17 `Icod.TermInfo.Catalogs` foundation sits above Runtime, Inspection,
and BerkeleyDb. Existing libraries and command tools do not reference it.

## Quick Start

Install the runtime package:

```text
dotnet add package Icod.TermInfo --version 1.16.0
```

Resolve the current terminal through conventional system discovery with immutable built-in fallback:

```csharp
using Icod.TermInfo;

TerminalDatabase database = new(
	new ITerminalDescriptionProvider[] {
		new SystemTerminalDescriptionProvider(),
		TerminalDatabase.BuiltIn,
	}
);

TerminalDescription terminal = TerminalEnvironment.Resolve(
	database,
	TerminalProfiles.Dumb
);

Console.WriteLine( $"Terminal: {terminal.Name}" );

string? clear = terminal.GetString( StringCapability.ClearScreen );
if ( clear is not null ) {
	TermInfoOutput.PutP( clear, Console.Out );
}
```

Applications that only need compiled terminfo acquisition, immutable `TerminalDescription` values, capability lookup, expansion, or output continue to reference `Icod.TermInfo` alone. Add the optional packages only for the higher-level source, compiler, termcap, or planning workflows described below.

For explicit ncurses-compatible hashed acquisition:

```text
dotnet add package Icod.TermInfo.BerkeleyDb --version 1.16.0
```

```csharp
using Icod.TermInfo;
using Icod.TermInfo.BerkeleyDb;

var provider = new BerkeleyDbTerminalDescriptionProvider(
	"/path/to/terminfo.db"
);
if (
	provider.TryLoad(
		"xterm-256color",
		out TerminalDescription? terminal
	)
) {
	Console.WriteLine( terminal.Name );
}
```

See the [deterministic BerkeleyDb sample](samples/Icod.TermInfo.BerkeleyDb.Sample/README.md)
for a complete controlled example and the
[Berkeley DB hashed acquisition guide](docs/1.15.0-BERKELEY-DB-HASHED-ACQUISITION-GUIDE.md)
for provider, catalog, discovery, command, ownership, and error contracts.

## Feature Inventory

The root README describes the current product by capability rather than by the release in which each feature first appeared.

- **Terminal capability model** — immutable terminal descriptions; standard and extended Boolean, numeric, and string capabilities; canonical capability metadata; aliases and descriptions; built-in `dumb`, ANSI, DEC VT100/VT102/VT220, xterm, Windows Console, and Windows Terminal profiles.
- **Compiled terminfo acquisition** — bounded parsing of conventional compiled entries, explicit directory providers, deterministic system discovery, provider composition, successful-entry caching, and built-in fallback.
- **Expansion and output** — reusable parsed parameter programs, bounded parameter evaluation, extended-string expansion, reversible 8-bit capability-string semantics, and padding-aware `tputs`/`putp`-style output.
- **Color and environment semantics** — monochrome, indexed-color, and direct-RGB inspection and selector expansion; terminal-size queries; explicit Windows virtual-terminal output enablement kept separate from profile selection.
- **Terminfo source language** — `Icod.TermInfo.Source` provides `.ti` lexing, parsing, diagnostics, capability classification, cancellation semantics, `use=` inheritance resolution, and materialization into ordinary `TerminalDescription` values.
- **Compilation and publication** — `Icod.TermInfo.Compiler` writes deterministic legacy and wide compiled entries, validates representability, compiles resolved descriptions or `.ti` source, and publishes explicit conventional terminfo directory layouts.
- **Termcap interoperability** — `Icod.TermInfo.Termcap` provides bounded termcap parsing, capability classification, `tc=` resolution, semantic conversion, reverse representability/rendering, and explicit historical `TERMCAP` / `TERMPATH` acquisition.
- **Hashed terminfo acquisition and publication** — `Icod.TermInfo.BerkeleyDb` provides pure-managed exact lookup, opt-in hashed-aware discovery, logical catalog enumeration, and deterministic whole-file Hash-v9 writing.
- **Inspection, comparison, and planning** — `Icod.TermInfo.Inspection` provides canonical effective-source rendering, semantic comparison, database catalogs and ordered database-set analysis, relative-source synthesis and parent planning, machine-readable JSON automation, persistent-raster lifecycle/placement/runtime-evidence planning, and raster-backend availability and selection.
- **Managed command toolchain** — `tic`, `infocmp`, `toe`, `captoinfo`, and `infotocap` expose the reusable engines as traditional command-line workflows; `Icod.TermInfo.Tools` provides the non-colliding `icod-terminfo` router.

## Packages and Tools

| Package | Purpose |
| --- | --- |
| `Icod.TermInfo` | Runtime capability model, compiled acquisition, expansion, profiles, and output |
| `Icod.TermInfo.Source` | Terminfo `.ti` source parsing and `use=` resolution |
| `Icod.TermInfo.Termcap` | Termcap parsing, conversion, rendering, and explicit acquisition |
| `Icod.TermInfo.Compiler` | Deterministic compiled terminfo writing and database publication |
| `Icod.TermInfo.Inspection` | Rendering, comparison, database analysis, planning, and JSON automation |
| `Icod.TermInfo.BerkeleyDb` | Optional pure-managed Hash-v9 acquisition, catalog enumeration, and deterministic publication |
| `Icod.TermInfo.Tools` | Installable `icod-terminfo` multi-command router |

Install an optional package only when its capability is needed:

```text
dotnet add package Icod.TermInfo.Source --version 1.16.0
dotnet add package Icod.TermInfo.Termcap --version 1.16.0
dotnet add package Icod.TermInfo.Compiler --version 1.16.0
dotnet add package Icod.TermInfo.Inspection --version 1.16.0
```

Install the command router with:

```text
dotnet tool install --global Icod.TermInfo.Tools --version 1.16.0

icod-terminfo tic -V
icod-terminfo infocmp -V
icod-terminfo toe -V
icod-terminfo captoinfo -V
icod-terminfo infotocap -V
```

The standalone release archives expose the traditional command names directly for `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `osx-x64`, and `osx-arm64`. They are framework-dependent .NET 10 distributions; the user supplies the .NET 10 runtime and controls installation and `PATH` placement.

## Platforms and Targets

The reusable package family targets:

```text
net8.0
net9.0
net10.0
```

The repository uses C# 13. The command projects and router target .NET 10. Release validation covers Windows, Linux, and macOS, with x64 and ARM64 standalone tool archives for each operating-system family.

The managed packages contain no native ncurses or system terminfo payload. Runtime discovery consumes caller-selected or conventional host databases and can fall back to immutable built-in profiles.

## Design Boundaries and Guarantees

- Runtime capability data is immutable and reusable; live terminal-session ownership is intentionally outside TermInfo.
- TermInfo does not install a competing terminal input reader, own application event loops, or perform general live terminal probing.
- Persistent-raster planning is protocol-neutral. TermInfo does not allocate terminal-side image/resource/placement identities, execute Sixel or Kitty Graphics, process acknowledgements, or own terminal cleanup; those responsibilities belong to live-session layers such as `Icod.Terminal`.
- Backend planning does not use terminal-brand heuristics or hidden backend ranking. Caller preference is explicit, and ambiguity remains visible rather than being resolved by enum value or input order.
- Curses-style cells, windows, layout, retained presentation state, refresh/damage policy, and interaction routing belong to `Icod.DCurses`.
- PTY/process hosting and terminal emulation are separate concerns.
- Parsing, compilation, comparison, synthesis, planning, and machine-readable output are designed to be deterministic and bounded for equivalent caller input.
- `TermInfoOutput` can emit resolved capability strings and honor terminfo padding semantics without turning the library into the owner of a live terminal conversation.

## Samples and Documentation

The [`samples`](samples/README.md) directory contains focused examples for runtime and controlled hashed publication/acquisition, reusable Termcap parsing/conversion/acquisition, the source/compiler toolchain, database-set analysis, persistent-raster lifecycle and placement planning, runtime-evidence integration, raster-backend selection, and command-tool workflows.

Recommended documentation entry points:

- [`CHANGELOG.md`](CHANGELOG.md) — release-by-release feature history;
- [`docs/VERSIONING.md`](docs/VERSIONING.md) — versioning and compatibility policy;
- [`docs/COMPATIBILITY.md`](docs/COMPATIBILITY.md) — public and binary compatibility commitments;
- [`docs/1.15.0-BERKELEY-DB-HASHED-ACQUISITION-GUIDE.md`](docs/1.15.0-BERKELEY-DB-HASHED-ACQUISITION-GUIDE.md) — explicit provider, catalog, discovery, command, and error contracts;
- [`docs/1.15.0-BERKELEY-DB-HASH-V9-COMPATIBILITY.md`](docs/1.15.0-BERKELEY-DB-HASH-V9-COMPATIBILITY.md) — exact supported Berkeley DB/ncurses subset;
- [`docs/1.16.0-BERKELEY-DB-WRITING-GUIDE.md`](docs/1.16.0-BERKELEY-DB-WRITING-GUIDE.md) — public writer and command contracts;
- [`docs/1.16.0-RELEASE-AUDIT.md`](docs/1.16.0-RELEASE-AUDIT.md) — exact 1.16 qualification and release evidence;
- [`Icod.TermInfo-1.16.0-Berkeley-DB-Hash-V9-Writer-Roadmap.md`](Icod.TermInfo-1.16.0-Berkeley-DB-Hash-V9-Writer-Roadmap.md) — active focused writer-only plan, with migration and catalog automation reserved for 1.17;
- [`docs/1.14.0-RASTER-BACKEND-SELECTION-GUIDE.md`](docs/1.14.0-RASTER-BACKEND-SELECTION-GUIDE.md) — current raster-backend evidence and selection model;
- [`docs/1.14.0-RELEASE-AUDIT.md`](docs/1.14.0-RELEASE-AUDIT.md) — exact 1.14 qualification and release evidence;
- [`Icod.TermInfo-Post-1.0-Development-Roadmap.md`](Icod.TermInfo-Post-1.0-Development-Roadmap.md) — longer-range development direction.

Release audits, public-API baselines, schema freezes, tranche records, and historical roadmaps remain in the repository as engineering evidence. They are intentionally not repeated in this README.

## Compatibility and Versioning

The 1.x line keeps reusable assembly identity at version `1.0.0.0`; the assemblies remain unsigned. The current reusable packages support `net8.0`, `net9.0`, and `net10.0`.

The runtime 1.0 public API remains frozen. Later reusable capabilities were added through sibling packages or compatible additive surfaces rather than by turning the dependency-free Runtime package into a monolith. Public API, binary/package compatibility, deprecation, and target-framework policy are maintained in [`docs/VERSIONING.md`](docs/VERSIONING.md) and [`docs/COMPATIBILITY.md`](docs/COMPATIBILITY.md).

This README is maintained as a current product and contributor entry point. Release-by-release chronology belongs in [`CHANGELOG.md`](CHANGELOG.md), release notes, versioned roadmaps, API/schema baselines, and release audits rather than accumulating here.

## Authors

Inspired by original work from Bill Joy, author of the original `termcap`; Ken Arnold, for his work on `termcap` and `curses`; Mary Ann (born Mark) Horton, author of `terminfo`; Pavel Curtis, author of `pcurses`; and Zeyd Ben-Halim, Eric S. Raymond, and Thomas Dickey, whose work developed and maintained `libtinfo` and `ncurses`.

Managed .NET implementation by Timothy J. Bruce <uniblab@hotmail.com>.

## Copyright

Copyright (c) 2026 Timothy J. Bruce

## License

The reusable `Icod.TermInfo`, `Icod.TermInfo.Source`,
`Icod.TermInfo.Termcap`, `Icod.TermInfo.Compiler`,
`Icod.TermInfo.Inspection`, and `Icod.TermInfo.BerkeleyDb` library
projects are licensed under the GNU Lesser General Public License, version 3 or
later.

Executable command, sample, and repository tooling projects are licensed under the GNU General Public License, version 3 or later, as stated in their project and source declarations.

See `LICENSE` and the per-project/source declarations for the applicable terms.
