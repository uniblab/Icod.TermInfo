# HDB04 Hashed-aware System Discovery Plan

**Goal:** Add an opt-in system provider in the optional BerkeleyDb package while preserving Runtime's frozen directory provider and discovery precedence.

**Architecture:** Runtime remains the single owner of environment capture, path-list expansion, platform defaults, and logical-location deduplication. It grants internal friend access to the optional package; no Runtime public API changes. The optional provider consumes Runtime's immutable discovery snapshot and ordered logical locations, then selects an encoded entry, exact directory, exact hashed file, or ncurses `.db` companion for each location. Existing explicit directory and BerkeleyDb providers perform parsing, identity checks, caching, and format-specific failures.

**Tech stack:** C# 13, net8.0/net9.0/net10.0, xUnit, pure managed package.

**Spec:** `Icod.TermInfo-1.15.0-Berkeley-DB-Hashed-Terminfo-Acquisition-Roadmap.md`, HDB04.

## Public API

```csharp
public sealed class BerkeleyDbSystemTerminalDescriptionProvider
	: ITerminalDescriptionProvider
public sealed class BerkeleyDbSystemTerminalDescriptionProviderOptions
```

Options snapshot Runtime parser limits, Berkeley database/index limits, and the existing independent environment/user/system policy flags.

## Discovery contract

1. Encoded `TERMINFO`.
2. `TERMINFO` logical location.
3. Non-Windows user `.terminfo` logical location.
4. `TERMINFO_DIRS`, including in-place empty-component defaults.
5. Final platform defaults.

At a logical location, an existing directory uses the Runtime directory provider; an existing regular file uses the explicit BerkeleyDb provider. When the exact location is absent, a regular `.db` companion is tried, matching pinned ncurses `check_existence` behavior in `ncurses/tinfo/db_iterator.c`. Exact locations take precedence over companions. Reached malformed/unsupported sources fail explicitly; clean missing locations continue.

## RED

Declaration/test head `798ae5694f69a6187e8f62818f1cdad16cdf86f4` produced 15 expected failures and 235 passes, 250 total per target framework, on Linux and macOS in PR run 35033288142. Provider behaviors failed with the declaration-only `NotImplementedException`; options snapshot and limit-validation cases failed because the stubs retained references and accepted invalid bounds. No discovery behavior was implemented before this RED was observed.

## GREEN

Implement the smallest adapter over Runtime's internal discovery policy. Exact locations are inspected at lookup time so later-created sources remain retryable; an existing exact directory or file wins before its `.db` companion. Preserve exact Lazy removal and successful-only caching. Keep Runtime's public API and existing `SystemTerminalDescriptionProvider` behavior unchanged. Then add concurrency, location deduplication, native system-provider fixtures, and package-only consumer qualification before HDB04 acceptance.
