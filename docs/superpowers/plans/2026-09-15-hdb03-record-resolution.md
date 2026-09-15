# HDB03 Ncurses Record Resolution Implementation Plan

> **For agentic workers:** Use superpowers:executing-plans to implement this checkpoint task-by-task. Steps use checkbox syntax for tracking.

**Goal:** Recover opaque compiled-entry bytes from marker-0/marker-2 records in a supported Hash-v9 database.

**Architecture:** The existing Hash reader owns storage validation. A separate internal ncurses resolver acquires one bounded byte array and performs every exact-key lookup against that image. Compiled parsing, identity validation, caching, and the public provider follow in subsequent HDB03 checkpoints.

**Tech Stack:** C# 13; managed .NET net8.0/net9.0/net10.0; xUnit; existing GitHub PR and HDB00 interoperability workflows.

**Spec:** `Icod.TermInfo-1.15.0-Berkeley-DB-Hashed-Terminfo-Acquisition-Roadmap.md`, sections 4, 8, 9, 10, and HDB03.

## Global constraints

- Coordinated prerelease: `1.15.0-Alpha-3`.
- No public API in this checkpoint; HDB03 public contract is not frozen yet.
- No native runtime dependency, P/Invoke, writes, transactions, or environments.
- Runtime remains dependency-free; compiled semantics remain owned by its parser.
- Braces on every if/else, tabs, and existing Icod 1TBS conventions.
- Complete exact-head qualification requires 12 normal PR jobs and 3 interoperability jobs.
- HDB02 remains accepted at implementation head `0495addc76c0655ab99d716b19bcc303b05a6f0c`, closure head `5d569746ffa73304cef89a608326765888e1c727`.

## Design and decisions

A separate resolver keeps Hash storage independent of ncurses. Combining envelopes
into the Hash reader would mix storage with acquisition policy. Reopening the path
for each index link would permit links to cross different file versions. The selected
design reuses one acquired image; it does not promise an atomic read against external
same-length writes.

The internal entry point is:

```csharp
internal static bool TryReadCompiledEntry(
	string databasePath,
	ReadOnlySpan<byte> requestedKey,
	out byte[] compiledEntry,
	int maximumDatabaseSize = 64 * 1024 * 1024,
	int maximumItemSize = 1024 * 1024,
	int maximumIndexHops = 16
);
```

Limits are internal, provisional parameters. File/item limits must be positive;
index hops must be nonnegative. Validate before opening. The initial missing key
returns false and an empty result. A missing referenced key is malformed.

Marker 0 removes exactly its first byte. Its remaining bytes are opaque, even when
empty or invalid compiled data. The later provider invokes CompiledTermInfoParser
and enforces its independently authoritative MaximumEntrySize.

Marker 2 requires a nonempty target, preserving every raw byte. Count followed
marker-2 links, not total lookups: zero permits a direct data record, and N permits
exactly N links. Reject repeated keys by byte content, empty records, unsupported
markers, empty targets, missing targets, and excessive links with InvalidDataException.
File opening/read exceptions propagate unchanged.

The existing path-based Hash lookup delegates to an internal byte-array lookup.
The resolver uses the existing bounded file-read helper once, then only the byte-array
lookup. Per-call state is independent; no cache or file handle survives resolution.

Pinned ncurses references:
- [write_entry.c](https://github.com/mirror/ncurses/blob/87c2c84cbd2332d6d94b12a1dcaf12ad1a51a938/ncurses/tinfo/write_entry.c)
- [read_entry.c](https://github.com/mirror/ncurses/blob/87c2c84cbd2332d6d94b12a1dcaf12ad1a51a938/ncurses/tinfo/read_entry.c)

The writer uses markers 0 and 2 and explicit DBT lengths. The native reader follows
nonzero markers more permissively; accepting only 2 is our deliberate subset policy.

## Task 1: Establish behavioral RED

**Files:**
- Create `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb03RecordReaderTests.cs`.
- Create declaration-only `Icod.TermInfo.BerkeleyDb/src/NcursesRecordReader.cs`.
- Extend `tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/src/NativeOracleTests.cs`.
- Update `Directory.Build.props` and the main roadmap for Alpha-3/HDB03.

**Interfaces:** Tests call the signature above. The declaration throws
NotImplementedException; it implements no envelope behavior.

- [x] Write 31 real-file synthetic cases covering exact opaque data, raw binary
  index keys, inclusive/excessive hops, clean miss, malformed envelopes, cycles,
  dangling links, file/item bounds, argument validation, I/O retry, and file ownership.
- [x] Add three direct production-resolution comparisons with native compiled files:
  hdb00-primary, hdb00-alias, and hdb00-overflow.
- [ ] Commit tests plus the declaration and observe behavioral failure.

Representative assertion:

```csharp
Assert.True( NcursesRecordReader.TryReadCompiledEntry(
	path, new byte[] { 0x6B }, out byte[] actual, maximumIndexHops: 2
) );
Assert.Equal( new byte[] { 0x42 }, actual );
```

Run:

```sh
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Staging
dotnet test tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/Icod.TermInfo.BerkeleyDb.Interop.Tests.csproj -c Release
```

The second command requires ICOD_HDB02_FIXTURE_ROOT from the existing HDB00 workflow.
Expected RED: new behavior throws NotImplementedException; the 165 HDB02 cases and
six raw-record interoperability cases continue passing. Compiler failures or fixture
setup failures do not count as behavioral RED.

## Task 2: Resolve records over one acquired image

**Files:**
- Modify `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashReader.cs`.
- Implement `Icod.TermInfo.BerkeleyDb/src/NcursesRecordReader.cs`.
- Update package README/release notes and this progress record.

**Interfaces:** Add internal
`TryReadValue(byte[] database, ReadOnlySpan<byte> requestedKey, out byte[] value, int maximumItemSize)`.
Make existing `ReadDatabase(string databasePath, int maximumDatabaseSize)` internal.
Both are used by production, with no testing callbacks or storage mocks.

- [ ] Extract byte-array lookup without altering the validated Hash algorithm.
- [ ] Validate resolver arguments before acquisition.
- [ ] Read one image; track byte-content visited keys and followed index links.
- [ ] Handle clean initial miss, marker 0, marker 2, and each malformed outcome
  exactly as specified above.
- [ ] Run the unchanged tests and require 196/196 unit cases and 9/9
  interoperability cases per TFM on all three hosts.
- [ ] Obtain independent implementation review and resolve blocking findings.
- [ ] Require all 15 exact-head CI jobs green and record evidence in PR #45.

## HDB03 work after this checkpoint

Public provider/options/format exception, canonical path and terminal-name validation,
parser-limit enforcement and parser reuse, canonical/alias identity validation,
successful-result caching, retryable misses/failures, concurrency, and package-only
provider consumer validation remain required before HDB03 acceptance. HDB04 discovery
and HDB05 enumeration are separate tranches.
