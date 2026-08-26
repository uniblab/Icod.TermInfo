# Icod.TermInfo Compatibility Policy

This document defines the supported 1.x compatibility boundary for
`Icod.TermInfo` and the optional `Icod.TermInfo.Source` package.

## Supported target frameworks

Both 1.x packages support:

```text
net8.0
net10.0
```

Both are first-class package targets. Release validation requires equivalent
public API manifests between target frameworks and fresh-package execution for
each target.

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

Within 1.x:

- existing public signatures remain source/binary compatible;
- enum names and numeric values remain stable;
- nullability and optional/default parameter contracts are treated as public
  contract;
- additions must be compatible and documented;
- behavior changes must preserve documented semantic contracts unless they
  correct an acknowledged defect.

Both assemblies retain version `1.0.0.0` and remain unsigned throughout 1.x.

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

## Discovery and failure compatibility

Runtime discovery precedence, clean-miss behavior, parser failures,
I/O/permission propagation, terminal-name validation, and provider-local
cache/refresh rules are part of the compatibility contract.

For source resolution, a clean `ITermInfoSourceEntryProvider` miss becomes a
source diagnostic. Provider failures propagate. Resolver diagnostics and
duplicate-identity lookup remain deterministic and ordinal/case-sensitive.

`TerminalDatabase.BuiltIn` remains environment-independent and I/O-free.

## Package compatibility

`Icod.TermInfo` contains managed/XML assets for both supported target frameworks
and portable symbols for both. It has no runtime NuGet dependency and no native
ncurses/terminfo payload.

`Icod.TermInfo.Source` contains the corresponding dual-target managed/XML and
symbol assets and depends on the matching `Icod.TermInfo` package. The dependency
direction is one-way: `Icod.TermInfo` never depends on Source.

The same validated artifacts for a release are used for NuGet.org and GitHub
Packages.

## Explicit non-goals

The 1.1 package family does not promise:

- `tic`/`infocmp`/`toe`-class compiler or command tooling;
- compiled terminfo writing;
- termcap parsing/conversion;
- Berkeley DB/hashed terminfo stores;
- divergent undocumented vendor binary dialects;
- live raw/cooked terminal session ownership;
- input-event decoding or active probing;
- PTY/ConPTY lifecycle;
- curses/virtual-screen behavior;
- terminal emulation or graphics protocols.

Those remain future or sibling-system work. See `FUTURE-WORK-INVENTORY.md`.
