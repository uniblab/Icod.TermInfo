# HDB03 Explicit Provider Implementation Plan

**Goal:** Freeze and qualify the explicit public provider for one caller-selected ncurses-compatible Hash-v9 database.

**Architecture:** A sealed provider implements the existing Runtime interface. It validates an exact terminal name, resolves compiled bytes through the accepted internal reader, parses only with CompiledTermInfoParser, verifies canonical/alias identity, and caches only successful immutable descriptions. Immutable provider options snapshot parser and database bounds. A package-specific format exception maps container/envelope failures while preserving parser and I/O exception identities.

**Tech stack:** C# 13, net8.0/net9.0/net10.0, xUnit, pure managed package.

**Spec:** `Icod.TermInfo-1.15.0-Berkeley-DB-Hashed-Terminfo-Acquisition-Roadmap.md`, HDB03.

## Public API

```csharp
public sealed class BerkeleyDbTerminalDescriptionProvider
	: ITerminalDescriptionProvider
public sealed class BerkeleyDbTerminalDescriptionProviderOptions
public sealed class BerkeleyDbDatabaseFormatException : FormatException
```

The provider constructor accepts a database path and optional provider options.
`DatabasePath` is canonical and absolute. Options expose snapshotted
`CompiledTermInfoParserOptions`, maximum database bytes, and maximum index hops.
No raw database API becomes public.

## Behavior

- Validate construction and lookup arguments before file access.
- Terminal names remain exact ordinal UTF-8 keys; reject empty, dot, path
  separators, control characters, and surrogates.
- Resolve using the accepted internal marker reader.
- Map internal InvalidDataException from the container/envelope boundary to
  BerkeleyDbDatabaseFormatException with the original failure as InnerException.
- Propagate I/O failures and CompiledTermInfoFormatException unchanged.
- Parse with the snapshotted parser options and verify requested name against
  canonical name and aliases using ordinal comparison.
- Cache successful descriptions per exact name with publication-once semantics.
- Remove clean misses and exceptions from the cache so later calls retry.
- A new provider observes later valid database content.
- MaximumEntrySize remains authoritative at the parser. The internal item bound
  is the Runtime parser's maximum supported entry size plus the envelope marker.

## RED

The initial declaration-only commit `7dae422925b17d05c5d958eb94d876c5093549f7`
produced 32 expected provider failures with 199 existing passes (231 total) per
target framework on Linux in PR run 35029787548. The strengthened test-only
commit `2100a83c0c639eefba302cb1fe809afd6afacc77` added changed-content refresh,
surrogate rejection, I/O retry, and the exact parser-limit boundary. It produced
35 expected failures with 199 existing passes (234 total) per target framework
on both Linux and macOS in PR run 35030136042. No production implementation was
written before these behavioral failures were observed.

## GREEN

Implement the smallest provider/options/exception behavior satisfying the tests.
Failed Lazy instances are removed only by exact key/value pair. Exception mapping
is limited to InvalidDataException from record resolution; parser, identity, and
I/O failures retain their types. Key validation is platform-independent because
the requested name is a database key. The reader item cap is the Runtime parser's
maximum supported entry size plus the ncurses marker byte; the parser's configured
limit remains authoritative. The database-size limit is an aggregate acquisition
bound rather than a per-record storage limit.

Keep existing HDB02 and record-resolution tests unchanged. Require public API
equivalence across all TFMs, package-only consumer validation, 12 normal CI jobs,
3 interoperability jobs, and independent review. GREEN qualification is pending.

## Remaining HDB03 closure work

After this checkpoint, review native-fixture provider parsing and package-only
consumer coverage, then complete HDB03 documentation and acceptance. System
discovery remains HDB04.
