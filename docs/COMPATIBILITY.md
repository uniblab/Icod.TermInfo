# Icod.TermInfo Compatibility Policy

This document defines the supported 1.x compatibility boundary for
`Icod.TermInfo`, the optional `Icod.TermInfo.Source` and
`Icod.TermInfo.Compiler` packages, and, beginning with 1.3, the optional
`Icod.TermInfo.Inspection` package.

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

For 1.2 and later, all three target frameworks are first-class package targets.
Release validation requires equivalent public API manifests between target
frameworks and fresh-package execution for each target for every package present
in that release.

Beginning with T01 in 1.4, the `tic`, `infocmp`, and `toe` command layer targets
`net10.0`. This command-host choice follows `Icod.CommandFramework 2.0.0` and
does not remove `net8.0` or `net9.0` from any reusable TermInfo library package.

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

The Compiler 1.2 public API is frozen through
`docs/1.2.0-COMPILER-PUBLIC-API-BASELINE.txt` and its compiler-contract tests.

The Inspection 1.3 public API is independently frozen by
`docs/1.3.0-INSPECTION-PUBLIC-API-BASELINE.txt` and Inspection contract tests.
I01 started with an empty public surface, I02-I06 established the reviewed API,
and I07 froze that contract for release.

The active 1.4 Inspection development baseline is
`docs/1.4.0-INSPECTION-PUBLIC-API-BASELINE.txt`. At T01 it is byte-for-byte
identical to the frozen 1.3 baseline. Any later 1.4 addition must be compatible,
reviewed deliberately, and recorded in the 1.4 baseline rather than rewriting
the released 1.3 contract.

Within 1.x:

- existing public signatures remain source/binary compatible;
- enum names and numeric values remain stable;
- nullability and optional/default parameter contracts are treated as public
  contract;
- additions must be compatible and documented;
- behavior changes must preserve documented semantic contracts unless they
  correct an acknowledged defect.

Runtime, Source, Compiler, and Inspection assemblies retain version `1.0.0.0`
and remain unsigned throughout 1.x.

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

## Inspection compatibility

`Icod.TermInfo.Inspection` 1.3 is the optional human-readable inspection and
semantic-comparison layer. It is deliberately separate from Runtime, Source, and
Compiler so those frozen public contracts do not acquire tooling-oriented API.

The 1.3 architectural contract distinguishes two domains:

- effective inspection/comparison over `TerminalDescription`;
- source-aware inspection/comparison over unresolved Source models.

Effective inspection SHALL NOT invent `use=` relationships, cancellation
tombstones, duplicate-source history, comments, or provenance that
`TerminalDescription` does not retain. Source-aware operations SHALL preserve
field order where order is semantically significant and likewise shall not
invent source information the parsed model does not retain.

The released 1.3 contract includes canonical effective rendering, normalized
unresolved-source rendering, structured effective and source-aware comparison,
and provider-aware inspection orchestration. Those behaviors are frozen through
the independent Inspection baseline and its semantic tests.

Beginning with T02 in the 1.4 line, Inspection additionally exposes a read-only
snapshot of the ordered system database locations considered by Runtime
discovery. The API distinguishes encoded `TERMINFO`, directory `TERMINFO`, the
user database, `TERMINFO_DIRS`, and final platform defaults. Encoded payload bytes
are not exposed. Directory paths are normalized, Runtime precedence and
platform-specific duplicate handling are preserved, and no database contents are
enumerated until the separate T03 catalog tranche. Runtime public API remains
unchanged.

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

Beginning with 1.3, `Icod.TermInfo.Inspection` contains corresponding three-target
managed/XML and symbol assets and depends directly on the matching Runtime and
Source packages. Inspection does not depend on Compiler. Runtime, Source, and
Compiler do not depend on Inspection.

Beginning with T01, the three command executables sit above this package family.
They may use `Icod.CommandFramework` and the appropriate TermInfo libraries, but
no dependency flows back from Runtime, Source, Compiler, or Inspection into the
command layer. The command projects are non-packable in T01; command distribution
is a later 1.4 release concern.

The same validated artifacts for a release are used for NuGet.org and GitHub
Packages.

## Explicit non-goals

The 1.3 package family does not promise:

- `tic`, `infocmp`, or `toe` command-line applications;
- termcap parsing/conversion;
- Berkeley DB/hashed terminfo stores;
- divergent undocumented vendor binary dialects;
- live raw/cooked terminal session ownership;
- input-event decoding or active probing;
- PTY/ConPTY lifecycle;
- curses/virtual-screen behavior;
- terminal emulation or graphics protocols.

For 1.4, the first item above becomes active tranche-by-tranche: `tic`,
`infocmp`, and `toe` are introduced as managed command projects beginning with
the T01 shell contract. T01 does not yet implement their operational semantics.
The remaining items continue to be future or sibling-system work. See
`FUTURE-WORK-INVENTORY.md`.
