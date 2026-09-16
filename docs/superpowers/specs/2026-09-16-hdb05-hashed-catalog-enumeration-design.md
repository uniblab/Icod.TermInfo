# HDB05 Hashed Catalog Enumeration Design

**Status:** APPROVED DESIGN BASELINE  
**Development line:** `Icod.TermInfo 1.15.0`  
**Coordinated prerelease:** `1.15.0-Alpha-5`  
**Tranche:** HDB05  
**Package:** `Icod.TermInfo.BerkeleyDb`

## 1. Objective

HDB05 adds bounded, deterministic, read-only catalog enumeration for one
explicit ncurses-compatible Berkeley DB Hash-v9 terminfo store.

The catalog is a snapshot for tooling and diagnostics. It exposes logical
terminal-name publications and parsed `TerminalDescription` values. It does not
expose raw Berkeley DB records, pages, buckets, cursors, environments, or
transactions.

HDB05 does not integrate `infocmp`, `toe`, Inspection, or system-wide
multi-database discovery. Those remain HDB06 responsibilities.

## 2. Selected approach

A dedicated catalog reader is added alongside the accepted explicit and system
terminal-description providers.

This is preferred over adding enumeration to
`BerkeleyDbTerminalDescriptionProvider` because exact lookup and full-store
inspection have different caching, error, ordering, cancellation, and resource
contracts. It is preferred over keeping enumeration internal because HDB06 tools
need a reviewed reusable boundary without owning database parsing.

The reader owns one canonical database path and immutable option snapshot. Every
read acquires a fresh database image and returns a new immutable catalog
snapshot. Catalog results are not cached.

## 3. Public API

HDB05 adds four public types:

```csharp
public sealed class BerkeleyDbTerminalCatalogReader
public sealed class BerkeleyDbTerminalCatalogReaderOptions
public sealed class BerkeleyDbTerminalCatalogEntry
public enum BerkeleyDbTerminalCatalogEntryKind
```

The intended surface is:

```csharp
public sealed class BerkeleyDbTerminalCatalogReader {
	public BerkeleyDbTerminalCatalogReader(
		string databasePath,
		BerkeleyDbTerminalCatalogReaderOptions? options = null
	);

	public string DatabasePath { get; }

	public IReadOnlyList<BerkeleyDbTerminalCatalogEntry> Read();

	public IReadOnlyList<BerkeleyDbTerminalCatalogEntry> Read(
		CancellationToken cancellationToken
	);
}

public sealed class BerkeleyDbTerminalCatalogReaderOptions {
	public const int DefaultMaximumRecordCount = 65_536;

	public BerkeleyDbTerminalCatalogReaderOptions(
		CompiledTermInfoParserOptions? parserOptions = null,
		int maximumDatabaseSize =
			BerkeleyDbTerminalDescriptionProviderOptions.DefaultMaximumDatabaseSize,
		int maximumRecordCount = DefaultMaximumRecordCount,
		int maximumIndexHops =
			BerkeleyDbTerminalDescriptionProviderOptions.DefaultMaximumIndexHops
	);

	public CompiledTermInfoParserOptions ParserOptions { get; }
	public int MaximumDatabaseSize { get; }
	public int MaximumRecordCount { get; }
	public int MaximumIndexHops { get; }
}

public sealed class BerkeleyDbTerminalCatalogEntry {
	internal BerkeleyDbTerminalCatalogEntry(
		string name,
		BerkeleyDbTerminalCatalogEntryKind kind,
		TerminalDescription terminal
	);

	public string Name { get; }
	public BerkeleyDbTerminalCatalogEntryKind Kind { get; }
	public TerminalDescription Terminal { get; }
}

public enum BerkeleyDbTerminalCatalogEntryKind {
	Canonical = 0,
	Alias = 1,
}
```

The options constructor snapshots the supplied parser limit. Database-size and
record-count limits must be positive. The index-hop limit uses the same inclusive
range, zero through
`BerkeleyDbTerminalDescriptionProviderOptions.MaximumSupportedIndexHops`, as
the explicit provider.

The returned list and entries are immutable. The same parsed
`TerminalDescription` instance is shared by entries that resolve to the same
marker-0 payload within one snapshot.

## 4. ncurses logical-record model

The pinned ncurses writer stores two kinds of records:

1. marker `0`: compiled terminfo bytes keyed by the complete names field;
2. marker `2`: a canonical or alias terminal-name key whose value targets the
   marker-0 key.

Catalog entries represent marker-2 logical publications, not marker-0 storage
keys.

For each marker-2 publication, the reader:

1. decodes the publication key as strict UTF-8;
2. applies the existing exact terminal-name safety rules;
3. follows marker-2 targets within the configured hop limit;
4. rejects missing targets and cycles;
5. parses the terminal marker-0 payload through
   `CompiledTermInfoParser`;
6. classifies the publication as `Canonical` when its name equals
   `TerminalDescription.Name`;
7. classifies it as `Alias` when its name equals one of
   `TerminalDescription.Aliases`; and
8. rejects a publication that is not declared by the parsed terminal identity.

Every marker-0 record is parsed and validated even when no marker-2 publication
references it. Such an orphan storage record is not emitted because its key is
not evidence of a published terminal name.

Direct marker-0 exact lookup remains supported by the HDB03 lookup machinery,
but a marker-0 key without a marker-2 publication is intentionally not
enumerated. This is the meaning of distinguishing canonical entries and aliases
"where the ncurses envelope permits."

## 5. Internal Hash-v9 enumeration

`BerkeleyDbHashReader` gains an internal complete-record operation that reuses
its existing metadata, page, item, off-page, overflow, byte-order, and corruption
validation.

One acquired byte array is used for the complete read. The operation:

- scans page numbers from one through the declared last page;
- validates every encountered page and paired key/value item;
- counts each Hash key/value pair before materializing it;
- rejects the next pair when the inclusive configured record limit is already
  exhausted;
- applies the configured stored-item bound to every key and value;
- supports cancellation between pages and records;
- rejects duplicate exact byte keys; and
- returns internal immutable key/value records.

The stored-item bound is the snapshotted parser entry limit plus the one-byte
ncurses marker. This bounds data values, complete names-field keys, index targets,
and publication keys consistently with the accepted explicit provider.

Existing exact-key lookup behavior remains unchanged. Refactoring shared page
walking must preserve HDB02's early-success exact lookup and encountered-page
validation contract.

## 6. Determinism

Physical Hash-page order is not an API contract.

Structural database failures are discovered in ascending page and item order.
After safe extraction, raw records are ordered by ordinal lexicographic byte-key
comparison before ncurses-envelope validation. If multiple logical records are
malformed, the same byte key therefore fails first regardless of their physical
Hash-page placement.

Successful catalog entries are ordered by:

1. `Name`, ordinal;
2. `Kind`, with canonical before alias; and
3. parsed canonical terminal name, ordinal.

Exact Berkeley DB keys are unique in the supported subset, so the secondary
ordering rules are defensive and deterministic rather than a duplicate-key
contract.

## 7. Resource and ownership policy

The reader validates options before opening the database.

A read is bounded by:

- `MaximumDatabaseSize`;
- `MaximumRecordCount`;
- the parser-derived maximum stored-item size;
- `MaximumIndexHops`;
- the existing supported Hash-v9 page-size and geometry limits; and
- cancellation.

The database file is opened read-only, acquired once, and closed before
enumeration and parsing continue. No file handle survives success or failure.

Independent concurrent `Read` calls use independent images and limit state.
The reader does not guarantee an atomic snapshot against an external writer
which changes bytes without changing the observed file length.

## 8. Error contract

The public reader preserves the HDB03 exception boundary:

- unsupported or malformed Berkeley DB structures and ncurses record envelopes,
  including invalid UTF-8 publication keys, throw
  `BerkeleyDbDatabaseFormatException`;
- malformed compiled terminfo bytes throw
  `CompiledTermInfoFormatException`;
- a compiled entry which does not declare its marker-2 publication key throws
  `InvalidDataException`;
- filesystem and permission failures retain their I/O exception types;
- invalid constructor/options arguments retain argument exception types; and
- cancellation throws `OperationCanceledException`.

Enumeration is all-or-nothing. HDB05 does not add a partial-results issue model.
HDB06 may translate a catalog failure into command diagnostics, but it does not
recover or print entries from the rejected snapshot.

## 9. Dependency and compatibility boundaries

Production dependency direction remains:

```text
Icod.TermInfo.BerkeleyDb
        |
        v
Icod.TermInfo
```

Runtime gains no public API and no Berkeley DB dependency. The optional package
remains pure managed, read-only, and free of P/Invoke, native assets, database
writes, transactions, recovery, and general-purpose database APIs.

The accepted HDB03 and HDB04 provider surfaces remain behaviorally unchanged.
Reusable assembly identity remains `1.0.0.0`. Public API must remain equivalent
across net8.0, net9.0, and net10.0.

## 10. TDD sequence

### Checkpoint A — internal complete-record enumeration

Start with tests for:

- multiple records on one and multiple Hash pages;
- inline and off-page keys and values;
- both byte orders;
- deterministic byte-key ordering;
- inclusive record-count bounds;
- duplicate byte-key rejection;
- cancellation;
- malformed page/item/overflow propagation; and
- unchanged exact-key early-success behavior.

Commit and observe RED before adding production enumeration.

### Checkpoint B — public logical catalog

Add declaration-only public types and behavioral tests for:

- canonical and alias classification;
- shared terminal object identity;
- marker-2 chains;
- orphan marker-0 validation without emission;
- strict UTF-8 publication keys;
- safe terminal-name validation;
- missing targets, cycles, hop limits, and unsupported markers;
- parser, identity, format, I/O, and cancellation exception boundaries;
- fresh snapshots on repeated reads;
- concurrent independent reads;
- physical-page-order independence; and
- options/path snapshots.

Observe the expected declaration/behavior RED before implementing the reader.

### Checkpoint C — native and package qualification

Extend native interoperability coverage to enumerate real ncurses-generated
canonical, alias, and forced-overflow stores on Linux and macOS. Windows consumes
the Linux-generated stores without Berkeley DB installed.

Extend the isolated package-only consumer to construct the public catalog reader
and enumerate canonical and alias publications on net8.0, net9.0, and net10.0.

## 11. Acceptance gate

HDB05 is accepted only when:

1. all new RED states were observed before their production behavior;
2. real native stores enumerate canonical, alias, and overflow-backed entries;
3. ordering is independent of physical Hash-page placement;
4. record count, stored-item size, database size, hop count, and cancellation are
   enforced;
5. all records in the snapshot are deterministically validated;
6. compiled semantics are parsed only by Runtime;
7. exact lookup and HDB04 system discovery remain unchanged;
8. package-only consumption passes on net8/net9/net10;
9. the complete Windows/Linux/macOS normal matrix is green;
10. the three-job HDB00 interoperability workflow is green;
11. exact package/dependency/native-asset verification is green;
12. installed-tool smoke and all six archive RID jobs are green; and
13. PR #45 remains open, draft, and unmerged.

## 12. Explicit non-goals

HDB05 does not add:

- catalog caching;
- system-wide or multi-database catalog composition;
- partial catalogs or issue collections;
- raw marker-0/storage-key exposure;
- physical page, bucket, or record-order exposure;
- Inspection or command integration;
- JSON schema changes;
- conventional-directory behavior changes;
- Berkeley DB writes;
- native Berkeley DB runtime loading; or
- support for additional Berkeley DB access methods, revisions, or feature
  flags.
