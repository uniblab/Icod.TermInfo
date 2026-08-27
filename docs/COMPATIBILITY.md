# Icod.TermInfo Compatibility Policy

This document defines the supported 1.x compatibility boundary for
`Icod.TermInfo`, the optional `Icod.TermInfo.Source` package, and, beginning
with 1.2, the optional `Icod.TermInfo.Compiler` package.

## Supported target frameworks

The frozen 1.0 and 1.1 package lines support:

```text
net8.0
net10.0
```

Beginning with 1.2.0, every package in the coordinated family supports:

```text
net8.0
net9.0
net10.0
```

For 1.2 and later, all three are first-class package targets. Release validation
requires equivalent public API manifests between target frameworks and
fresh-package execution for each target for every package present in that
release.

Dropping a supported target framework is considered a breaking support-contract
change and normally requires a new major version.

## Supported host families

The repository validates on:

```text
Windows
Linux
macOS
```

The package family is predominantly managed and platform-neutral. Narrow
platform-specific runtime functionality, such as Windows virtual-terminal mode
enablement, remains explicitly isolated and must fail gracefully when it is not
applicable.

Support means a package is expected to operate on platform/runtime combinations
supported by the corresponding .NET target. It does not promise every historical
OS release.

## Public API compatibility

The runtime 1.0 public API is frozen by
`docs/1.0.0-PUBLIC-API-BASELINE.txt` and its semantic surface tests.

The Source 1.1 public API is independently frozen by
`docs/1.1.0-SOURCE-PUBLIC-API-BASELINE.txt` and its source-contract tests.

The Compiler 1.2 public API is developed and frozen through
`docs/1.2.0-COMPILER-PUBLIC-API-BASELINE.txt` and its compiler-contract tests
beginning with C01.

Within 1.x:

- existing public signatures remain source/binary compatible;
- enum names and numeric values remain stable;
- nullability and optional/default parameter contracts are treated as public
  contract;
- additions must be compatible and documented;
- behavior changes must preserve documented semantic contracts unless they
  correct an acknowledged defect.

Runtime, Source, and Compiler assemblies retain version `1.0.0.0` and remain
unsigned throughout 1.x.

## Runtime terminfo semantic compatibility

The stable runtime responsibility includes:

- immutable terminal descriptions;
- complete standard and extended capability representation;
- signed 32-bit numeric semantics;
- reversible 8-bit compiled capability strings;
- parameter expansion;
- padding-aware output;
- built-in terminal profiles;
- conventional compiled terminfo parsing;
- explicit directory acquisition;
- deterministic environment/user/system discovery;
- provider-local caching and explicit new-provider refresh;
- explicit provider composition.

The supported compiled family is the frozen conventional System V/ncurses
contract documented by the 0.9 acquisition records: legacy `0432`, ncurses
extended sections, and `01036` 32-bit numerics.

## Source-language compatibility

`Icod.TermInfo.Source` 1.1 adds the optional source-language path:

- `.ti` lexical analysis and source locations;
- deterministic diagnostics;
- Boolean, numeric, string, cancellation, and `use=` source forms;
- standard and extended capability classification;
- unresolved source documents and entries;
- bounded inheritance resolution with cycle and depth diagnostics;
- deterministic duplicate source-name/alias warnings;
- materialization into the existing immutable `TerminalDescription` model.

The Source package does not redefine runtime capability semantics. A resolved
source entry is required to enter the same runtime model used by compiled
acquisition.

## Compiler compatibility

`Icod.TermInfo.Compiler` 1.2 adds the optional compiled-output path without
moving compiler responsibilities into the runtime package.

The compiler contract includes:

- deterministic conventional compiled-entry writing;
- legacy `0432` output;
- `01036` wide-numeric output;
- ncurses extended sections;
- standard ordering through the runtime capability catalog;
- strict reversible Latin-1 byte semantics;
- checked count, offset, and total-size arithmetic;
- explicit representation failure rather than silent truncation;
- source compilation through the existing Source parser/resolver;
- controlled conventional database-layout output;
- semantic round-trip validation through the existing runtime parser.

The low-level binary writer is pure. It does not read environment variables,
discover system databases, invoke native ncurses tools, or write filesystem
layouts. Filesystem output belongs to the later database-layout layer.

`TerminalDescription` represents effective runtime state and does not retain
source cancellation tombstones. A writer receiving only a
`TerminalDescription` therefore emits absence for absent capabilities and does
not invent cancellation.

Compiled output is byte-oriented. Identity strings, capability names, and
capability values which cannot be represented under the selected conventional
format fail deterministically. The compiler does not silently replace Unicode,
truncate numeric values, wrap offsets, or synthesize missing identity metadata.

For deterministic output, standard capabilities use canonical binary metadata
and extended capability names are ordered ordinally within their value kinds.

## Discovery and failure compatibility

Runtime discovery precedence, clean-miss behavior, parser failures,
I/O/permission propagation, terminal-name validation, and provider-local
cache/refresh rules are part of the compatibility contract.

For source resolution, a clean `ITermInfoSourceEntryProvider` miss becomes a
source diagnostic. Provider failures propagate. Resolver diagnostics and
duplicate-identity lookup remain deterministic and ordinal/case-sensitive.

`TerminalDatabase.BuiltIn` remains environment-independent and I/O-free.

## Package compatibility

Beginning with 1.2, `Icod.TermInfo` contains managed/XML assets and portable
symbols for all three supported target frameworks. It has no runtime NuGet
dependency and no native ncurses/terminfo payload.

`Icod.TermInfo.Source` likewise contains corresponding three-target managed/XML
and symbol assets and depends on the matching `Icod.TermInfo` package. The
dependency direction is one-way: `Icod.TermInfo` never depends on Source.

Beginning with 1.2, `Icod.TermInfo.Compiler` contains corresponding three-target
managed/XML and symbol assets. It depends directly on the matching runtime
package and may depend on the matching Source package for source compilation.
Runtime and Source never depend on Compiler.

The same validated artifacts for a release are used for NuGet.org and GitHub
Packages.

## Explicit non-goals

The 1.2 package family does not promise:

- `tic`, `infocmp`, or `toe` command-line applications;
- `infocmp`-class canonical rendering or semantic-comparison tooling;
- termcap parsing/conversion;
- Berkeley DB/hashed terminfo stores;
- divergent undocumented vendor binary dialects;
- live raw/cooked terminal session ownership;
- input-event decoding or active probing;
- PTY/ConPTY lifecycle;
- curses/virtual-screen behavior;
- terminal emulation or graphics protocols.

Those remain future or sibling-system work. See `FUTURE-WORK-INVENTORY.md`.
