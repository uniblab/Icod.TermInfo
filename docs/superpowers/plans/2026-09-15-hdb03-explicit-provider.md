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
3 interoperability jobs, and independent review. The implementation commit `f704454a7280eb8c20ef713386b490c66d5699f4` passed 234 BerkeleyDb tests per target framework on every host and the original 9 native-oracle cases per target framework on every host.

## Remaining HDB03 closure work

The qualification checkpoint exercises the public provider against canonical,
alias, overflow, clean-miss, and unsupported native inputs, raising the native
oracle suite from 9 to 15 cases per target framework. An isolated consumer
restores only the packed BerkeleyDb package and verifies provider construction,
options snapshotting, alias parsing, successful caching, and a clean miss on
net8.0, net9.0, and net10.0. Native fixtures use ASCII identities; UTF-8 database
keys with non-ASCII producer identities remain outside the qualified
interoperability claim.

Accepted exact head: `e2b55290f97014086b1de89f5b466e8c083ed1d3`.
PR workflow 35031640798 passed all 12 jobs. HDB00 workflow 35031640883
passed all 3 jobs. The final suite passed 234 unit cases and 15 native-store
interoperability cases per target framework on Windows, Linux, and macOS. The
isolated packed-package consumer passed net8.0, net9.0, and net10.0. Independent
review found no blocking findings. HDB03 is complete; system discovery remains
HDB04.
