# HW01 Hash-v9 Writer Public API Design

**Date:** 2026-09-18

**Release:** Icod.TermInfo 1.16.0

**Tranche:** HW01 / Alpha-1

**Status:** Approved design; implementation pending

## 1. Purpose

HW01 freezes the smallest reusable public contract needed to publish a complete
ncurses-compatible Berkeley DB Hash-v9 terminfo database. It does not implement
a general Berkeley DB library, incremental mutation, catalog migration, system
database discovery, or catalog automation. Those broader concerns remain outside
1.16.0 or are deferred to 1.17.0.

The design builds on the accepted HW00 proof at
`5d3cea771d6fb0187caab191632bc1ef84d6a705`, which demonstrated that the fixed
managed output profile is readable by native Berkeley DB and ncurses on Linux
and macOS and by the existing managed reader on Windows.

## 2. Decision

Icod.TermInfo.BerkeleyDb adds exactly three public types:

1. `BerkeleyDbTerminalDatabaseEntry`;
2. `BerkeleyDbTerminalDatabaseWriterOptions`; and
3. `BerkeleyDbTerminalDatabaseWriter`.

The writer is a static, whole-database publisher. It accepts terminfo identities
and compiled-entry bytes, constructs one complete deterministic image, verifies
that image through the existing managed reader, and commits it to one explicit
destination path.

HW01 adds no result type and no writer-specific exception type. The operation
returns `void` and uses the established .NET, compiled-terminfo, and Berkeley DB
exception vocabulary.

## 3. Alternatives considered

### 3.1 Selected: static whole-database writer

This shape matches `CompiledTermInfoDatabaseWriter`, makes the atomic publication
boundary explicit, and permits the complete logical input to be validated and
ordered before any destination change. Three types are sufficient: immutable
entry, immutable options, and static writer.

### 3.2 Rejected: immutable publication plan plus writer

A public plan would duplicate the writer's required input snapshot without
providing a second supported execution policy. It would introduce a fourth type
and permanent plan-versioning obligations before 1.17 migration or automation
has demonstrated a need for one.

### 3.3 Rejected: streaming builder

A stateful builder would make duplicate detection, identity agreement,
deterministic ordering, bounded-size calculation, cancellation, and safe commit
dependent on call order. It would also imply incremental publication even though
1.16 always replaces a complete database image.

### 3.4 Rejected: generic key/value writer

A generic surface would expose Berkeley DB record and envelope semantics that
the package does not otherwise support. It would allow callers to create stores
outside the reviewed ncurses terminfo profile and would turn a focused terminfo
feature into an unsupported database API.

### 3.5 Rejected: public result and writer-specific exception

Publication counts can be derived from the supplied immutable entries, and the
1.16 reusable API has no warnings or partial-success state. Standard exceptions
already distinguish programmer errors, logical conflicts, malformed compiled
payloads, cancellation, I/O failures, and invalid generated images. Result and
exception types would therefore be speculative compatibility commitments.

## 4. Exact public surface

The following source-equivalent signatures are the HW01 contract. XML
documentation and the generated public API snapshot are authoritative for
nullability and default values.

```csharp
namespace Icod.TermInfo.BerkeleyDb;

public sealed class BerkeleyDbTerminalDatabaseEntry {
	public BerkeleyDbTerminalDatabaseEntry(
		string canonicalName,
		IEnumerable<string> aliases,
		byte[] data
	);

	public string CanonicalName { get; }
	public IReadOnlyList<string> Aliases { get; }
	public byte[] Data { get; }
}

public sealed class BerkeleyDbTerminalDatabaseWriterOptions {
	public const int DefaultMaximumRecordCount = 65_536;

	public BerkeleyDbTerminalDatabaseWriterOptions(
		CompiledTermInfoParserOptions? parserOptions = null,
		int maximumDatabaseSize =
			BerkeleyDbTerminalDescriptionProviderOptions.DefaultMaximumDatabaseSize,
		int maximumRecordCount = DefaultMaximumRecordCount,
		bool overwriteExisting = false
	);

	public CompiledTermInfoParserOptions ParserOptions { get; }
	public int MaximumDatabaseSize { get; }
	public int MaximumRecordCount { get; }
	public bool OverwriteExisting { get; }
}

public static class BerkeleyDbTerminalDatabaseWriter {
	public static void Write(
		string databasePath,
		IEnumerable<BerkeleyDbTerminalDatabaseEntry> entries,
		BerkeleyDbTerminalDatabaseWriterOptions? options = null,
		CancellationToken cancellationToken = default
	);
}
```

No convenience overload is included in HW01. Additional overloads would be
reviewed as additive API in a later tranche only if a concrete caller cannot use
this operation without one.

## 5. Immutable input contracts

### 5.1 Entry

The entry constructor rejects null arguments, enumerates and snapshots the alias
sequence, and clones the supplied payload. Every `Data` access returns a new
clone, following `CompiledTermInfoSourceEntry`. `Aliases` exposes a read-only
snapshot preserving caller order.

The constructor performs the validation needed to create a valid immutable
value: the canonical name, every alias, and the payload must be non-null. The
writer owns cross-entry, safe-identity, parser, and resource validation so all
failures that depend on the complete database are evaluated consistently.

### 5.2 Options

Options are immutable. The constructor validates positive database and record
limits and snapshots `CompiledTermInfoParserOptions` rather than retaining the
caller's instance. The default database-size limit remains 64 MiB, matching the
1.15 reader. The default maximum record count remains 65,536, matching catalog
enumeration.

`MaximumRecordCount` counts physical Hash key/value records. One logical
canonical publication consumes the ncurses canonical index and data records;
each alias consumes its index record. All calculations use checked arithmetic.

### 5.3 Writer snapshot

`Write` materializes the finite `entries` sequence exactly once before semantic
validation or filesystem mutation. A null element is rejected. Because every
entry and the options object are immutable snapshots, later caller mutation of
the original alias collections, payload arrays, parser options, or entry
enumerable cannot affect the in-progress publication.

An empty entry sequence is rejected. Empty input does not create an empty
Berkeley DB file and does not alter an existing destination.

## 6. Identity and payload validation

Validation is fail-closed and completes before the destination is committed.
The writer:

1. validates the destination path as a non-empty explicit file path;
2. validates canonical names and aliases with the existing safe terminfo
   identity rules;
3. rejects embedded NULs, invalid surrogate input, unsafe path identities, and
   any name that cannot be encoded by strict UTF-8;
4. encodes keys as exact UTF-8 without normalization, transliteration,
   replacement bytes, or culture-sensitive conversion;
5. rejects a repeated canonical name;
6. rejects a repeated alias within one entry or across entries;
7. rejects a canonical name used as any alias, including its own;
8. parses every payload with the snapshotted parser options;
9. requires the parsed canonical name to equal `CanonicalName` by ordinal
   comparison;
10. requires parsed aliases to equal `Aliases` in the same order by ordinal
    comparison;
11. rejects unsupported, malformed, oversized, or trailing compiled data under
    the existing parser contract; and
12. enforces record-count, payload-size, database-size, page-count, and checked
    arithmetic limits before commit.

Enumeration order does not affect output. Exact payload identity remains
caller-visible through deterministic reader round trips.

## 7. Output and data flow

The operation has five internal phases:

1. **Snapshot:** materialize the entries and immutable option values.
2. **Validate:** validate identities, conflicts, compiled payloads, and resource
   bounds without changing the destination.
3. **Construct:** produce the fixed 4096-byte, little-endian Berkeley DB Hash-v9
   image using deterministic UTF-8 key ordering and the HW00-qualified envelope
   rules.
4. **Verify:** write and flush a uniquely named sibling temporary file, reopen it
   through the existing managed reader, and verify every canonical name, alias,
   and compiled payload.
5. **Commit:** use a same-filesystem move or replacement honoring
   `OverwriteExisting`, then cease observing cancellation.

The writer never merges with or mutates an existing database. Overwrite means
complete replacement after the new image has been closed and verified.

The temporary name and destination path do not participate in generated
database bytes. Equivalent logical input and options produce byte-identical
output across enumeration order, culture, operating system, process
architecture, destination path, and repeated runs.

## 8. Failure and cancellation model

- `ArgumentNullException` reports null public arguments.
- `ArgumentException` reports an empty or whitespace path, invalid individual
  entry values, or an empty entry sequence.
- `ArgumentOutOfRangeException` reports invalid option limits.
- `InvalidOperationException` reports duplicate or conflicting logical
  identities and parsed identity disagreement.
- Existing compiled parser exceptions report malformed or unsupported compiled
  payloads.
- `IOException`, `UnauthorizedAccessException`, and other platform filesystem
  exceptions retain their normal meaning.
- `OperationCanceledException` reports cancellation observed before commit.
- `BerkeleyDbDatabaseFormatException` is reserved for a constructed or reopened
  image that violates the supported Hash-v9 database format.

No exception contains compiled payload bytes, unrelated environment values, or
an uncontrolled absolute temporary path.

Cancellation is checked during snapshot, validation, construction, write, and
verification. Cancellation is not observed after the irreversible commit phase
begins. A pre-commit failure leaves prior destination bytes unchanged and removes
the writer's temporary artifact on a best-effort basis without hiding the
primary exception.

## 9. Package and compatibility boundary

Only `Icod.TermInfo.BerkeleyDb` gains public API. It retains exactly one project
dependency on `Icod.TermInfo` Runtime and gains no package or native dependency.
Runtime, Source, Compiler, Termcap, Inspection, command, and JSON schema public
contracts remain unchanged.

The BerkeleyDb assembly version remains `1.0.0.0`. The three new types and their
members must have equivalent public API snapshots on net8.0, net9.0, and
net10.0. The coordinated package version advances to `1.16.0-Alpha-1` only with
the HW01 contract-test checkpoint.

The implementation remains pure managed. Native Berkeley DB and ncurses tools
are interoperability oracles in dedicated CI only and are not production
dependencies or package assets.

## 10. HW01 contract tests

HW01 begins with an intentional RED checkpoint. Contract tests must prove:

- the exact three exported type additions and no fourth writer type;
- exact constructor, property, method, nullability, and default-value shapes;
- static/sealed type characteristics;
- defensive alias, payload, parser-option, and sequence snapshots;
- the no-result and no-writer-specific-exception decisions;
- null, empty-sequence, duplicate, alias-conflict, identity, strict UTF-8, and
  record-limit rules;
- cancellation-token presence and pre-commit behavior;
- the Runtime-only dependency boundary;
- unchanged public surfaces outside Icod.TermInfo.BerkeleyDb;
- assembly-version stability and cross-target API equivalence; and
- an additive 1.16 BerkeleyDb public API baseline generated by
  `Icod.TermInfo.PublicApiSnapshot`.

HW01 does not claim byte construction, collisions, overflow, or native
interoperability complete. Those implementation guarantees belong to HW02 and
HW03 and remain guarded by their own RED, GREEN, and cross-host acceptance
checkpoints.

## 11. Acceptance boundary

The HW01 design is accepted when the contract tests and checked-in API baseline
demonstrate the exact three-type terminfo-specific surface, immutable snapshots,
reviewed validation and error semantics, Runtime-only dependency direction, and
no generic Berkeley DB API. Production image construction may remain incomplete
until the later writer tranches, but no later tranche may enlarge or reinterpret
this public surface without a separately reviewed API decision.
