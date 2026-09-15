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

Add declaration stubs and behavioral tests for options snapshot/validation, encoded precedence, exact hashed files, directory-versus-companion precedence, user companions, mixed TERMINFO_DIRS order, in-place defaults, policy controls, malformed-source propagation, retryable misses, and successful caching. Observe the behavioral RED across supported TFMs before implementation.

## GREEN

Implement the smallest adapter over Runtime's internal discovery policy. Preserve exact Lazy removal and successful-only caching. Keep Runtime's public API and existing `SystemTerminalDescriptionProvider` behavior unchanged. Then add concurrency, location deduplication, native system-provider fixtures, and package-only consumer qualification before HDB04 acceptance.
