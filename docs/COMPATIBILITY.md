# Icod.TermInfo Compatibility Policy

This document defines the supported 1.x compatibility boundary.

## Supported target frameworks

The 1.x package supports:

```text
net8.0
net10.0
```

Both are first-class package targets. Release validation requires equivalent
public API manifests and fresh-package execution for each target.

Dropping a supported target framework is considered a breaking support-contract
change and normally requires a new major version.

## Supported host families

The repository validates on:

```text
Windows
Linux
macOS
```

The library is predominantly managed and platform-neutral. Narrow
platform-specific functionality, such as Windows virtual-terminal mode
enablement, remains explicitly isolated and must fail gracefully when it is not
applicable.

Support means the package is expected to operate on platform/runtime
combinations supported by the corresponding .NET target. It does not promise
every historical OS release.

## Public API compatibility

The 1.0 public API is frozen by the reviewed API baseline and the semantic
surface tests.

Within 1.x:

- existing public signatures remain source/binary compatible;
- enum names and numeric values remain stable;
- nullability and optional/default parameter contracts are treated as public
  contract;
- additions must be compatible and documented;
- behavior changes must preserve the documented semantic contracts unless they
  correct an acknowledged defect.

Assembly identity remains `1.0.0.0` and unsigned throughout 1.x.

## Terminfo semantic compatibility

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

## Discovery and failure compatibility

Discovery precedence, clean-miss behavior, parser failures, I/O/permission
propagation, terminal-name validation, and provider-local cache/refresh rules are
part of the compatibility contract.

`TerminalDatabase.BuiltIn` remains environment-independent and I/O-free.

## Package compatibility

A release package contains managed/XML assets for both supported target
frameworks and portable symbols for both. It has no runtime NuGet dependency and
no native ncurses/terminfo payload.

The same validated package artifact is used for NuGet.org and GitHub Packages.

## Explicit non-goals

The stable 1.0 contract does not promise:

- `.ti` source parsing or `use=` inheritance;
- `tic`/`infocmp`-class tooling;
- termcap parsing/conversion;
- Berkeley DB/hashed terminfo stores;
- divergent undocumented vendor binary dialects;
- live raw/cooked terminal session ownership;
- input-event decoding or active probing;
- PTY/ConPTY lifecycle;
- curses/virtual-screen behavior;
- terminal emulation or graphics protocols.

Those remain future or sibling-system work. See `FUTURE-WORK-INVENTORY.md`.
