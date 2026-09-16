# HDB07C Compatibility Expansion Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add detected-mutation rejection, native big-endian Hash-v9 evidence, and a bounded exact Latin-1 ncurses producer fallback without changing the public API or production dependency/write boundaries.

**Architecture:** Production path acquisition verifies two complete byte-identical observations through one open handle while the borrowed stream helper remains single-pass. Native CI repacks ncurses-produced records through Berkeley DB's C API into a big-endian Hash container, and terminal-name handling uses strict UTF-8 first with a Latin-1 candidate only after a clean miss or invalid UTF-8 publication key. Each workstream has an independently committed RED, narrow GREEN, and exact CI checkpoint.

**Tech Stack:** C# 13; .NET 8/9/10; xUnit 2.9; Berkeley DB 5.3 C API; pinned ncurses `tic`; Bash; PowerShell; GitHub Actions.

**Spec:** `docs/superpowers/specs/2026-09-16-hdb07c-compatibility-expansion-design.md`

## Global Constraints

- Keep the coordinated version exactly `1.15.0-Alpha-7`; HDB08 owns Alpha-8.
- Keep reusable assembly versions exactly `1.0.0.0` and public API equivalent across net8/net9/net10.
- Add no public type/member/option/exception, command switch, diagnostic ID, JSON field/schema, or package dependency.
- Keep Runtime and Inspection independent of BerkeleyDb; dependency direction remains `Icod.TermInfo.BerkeleyDb --> Icod.TermInfo`.
- Keep production pure managed and read-only: no P/Invoke, native asset, runtime download, database writer, environment, transaction, recovery, or repair path.
- Keep `CompiledTermInfoParser` authoritative and preserve its byte-preserving Latin-1 semantics.
- Do not add retry, encoding normalization, culture/code-page inference, case folding, transliteration, or best-fit mapping.
- A UTF-8 record that is found but malformed or identity-invalid must fail; Latin-1 fallback occurs only after a clean UTF-8 miss.
- Generated native databases remain short-lived workflow artifacts and are not committed.
- Add no Python source, script, inline program, or Python-dependent verification step.
- Run the ordinary PR workflow for every pushed checkpoint. Let the existing synchronize-delta classifier decide whether HDB00 performs expensive work.
- Run complete normal 12-job and HDB00 3-job qualification on the exact final implementation head.
- Keep PR #45 open, draft, and unmerged.

## File structure and ownership

| Path | Responsibility |
| --- | --- |
| `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashReader.cs` | Single-pass borrowed acquisition plus production-path two-observation verification. |
| `Icod.TermInfo.BerkeleyDb/src/TerminalNameEncoding.cs` | Internal strict UTF-8 and exact Latin-1 key conversion/decoding policy. |
| `Icod.TermInfo.BerkeleyDb/src/NcursesRecordReader.cs` | Resolve one or more exact key candidates against one acquired image. |
| `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalDescriptionProvider.cs` | UTF-8-first provider lookup, clean-miss-only Latin-1 fallback, and unchanged identity/caching boundaries. |
| `Icod.TermInfo.BerkeleyDb/src/NcursesCatalogReader.cs` | Publication decoding, logical-name ambiguity rejection, resolution, and classification. |
| `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb07cAcquisitionStabilityTests.cs` | Deterministic two-observation acquisition and ownership tests. |
| `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb07cEncodingCompatibilityTests.cs` | Synthetic provider/catalog encoding, precedence, ambiguity, and system-provider tests. |
| `tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/src/NativeOracleTests.cs` | Native dump, big-endian, Latin-1, provider, system-provider, and catalog comparisons. |
| `tools/hdb00/hdb07c_repack.c` | CI-only byte-exact record copier with selectable Berkeley DB metadata order. |
| `tools/hdb00/native-verifier/` | Managed metadata-magic, dump-parity, and Latin-1 native-evidence verification. |
| `tools/hdb00/hdb00_probe.c` | Native probe with exact text-key and explicit hex-key lookup modes. |
| `tools/hdb00/run-linux.sh` | Linux native big-endian and Latin-1 fixture generation/evidence. |
| `tools/hdb00/run-macos.sh` | macOS native big-endian and Latin-1 fixture generation/evidence. |
| `tools/hdb00/verify-managed.sh` | Managed probe verification for generated big-endian fixtures. |
| `tools/hdb00/verify-hdb06-commands.ps1` | Direct/routed `infocmp` and `toe` evidence for new fixtures. |
| `.github/workflows/hdb00-interoperability.yml` | Dumps, transports, and verifies the new native artifacts on all three hosts. |
| `tools/berkeleydb-package-smoke/Program.cs` | Package-only Latin-1 provider/catalog/system-provider evidence. |
| Roadmap, README, HDB07 record, HDB07C closure, and PR body | Precise accepted claims, residual limits, and HDB08 handoff. |

---

### Task 1: Two-observation production acquisition

**Files:**
- Create: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb07cAcquisitionStabilityTests.cs`
- Modify: `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashReader.cs`

**Interfaces:**
- Consumes: existing `ReadDatabase(Stream stream, int maximumDatabaseSize)` single-pass borrowed-stream contract.
- Produces: `internal static byte[] ReadStableDatabase(Stream stream, int maximumDatabaseSize)` for a seekable borrowed stream; production `ReadDatabase(string, int)` delegates to it.
- Preserves: `ReadDatabase(Stream, int)` remains single-pass, non-owning, and usable with non-seekable streams.

- [ ] **Step 1: Add the behavior-neutral stable-acquisition seam and path routing**

In `BerkeleyDbHashReader.cs`, change only the path wrapper and add this temporary behavior-neutral seam:

```csharp
internal static byte[] ReadDatabase(
	string databasePath,
	int maximumDatabaseSize
) {
	using FileStream stream = new FileStream(
		databasePath,
		FileMode.Open,
		FileAccess.Read,
		FileShare.Read,
		4096,
		FileOptions.SequentialScan
	);
	return ReadStableDatabase( stream, maximumDatabaseSize );
}

internal static byte[] ReadStableDatabase(
	Stream stream,
	int maximumDatabaseSize
) {
	return ReadDatabase( stream, maximumDatabaseSize );
}
```

Do not alter the existing stream overload. This seam makes the behavioral RED deterministic without requiring a filesystem race.

- [ ] **Step 2: Add deterministic two-observation tests**

Create `Hdb07cAcquisitionStabilityTests.cs` with a private seekable `ObservationStream` that:

- owns two byte arrays and two reported lengths;
- serves the first observation until EOF;
- advances to observation two only when `Position` is set back to zero;
- applies a configured maximum chunk size to every `Read`;
- optionally throws one caller-supplied `IOException` at a second-observation offset;
- counts complete observation starts; and
- records whether the stream was disposed.

Add these exact cases:

```csharp
[Theory]
[InlineData( 1 )]
[InlineData( 7 )]
[InlineData( 512 )]
public void StableAcquisitionAcceptsTwoIdenticalPartialReadObservations(
	int chunkSize
)

[Fact]
public void StableAcquisitionRejectsSameLengthContentChangeWithoutRetry()

[Theory]
[InlineData( 511 )]
[InlineData( 513 )]
public void StableAcquisitionRejectsSecondObservationLengthChange(
	int secondLength
)

[Fact]
public void StableAcquisitionPropagatesSecondObservationIoFailure()

[Fact]
public void BorrowedSinglePassAcquisitionStillAcceptsNonSeekableStream()
```

For mutation and second-length failures assert:

```csharp
IOException error = Assert.Throws<IOException>(
	() => BerkeleyDbHashReader.ReadStableDatabase( stream, 512 )
);
Assert.Equal(
	"The Berkeley DB file changed while it was being read.",
	error.Message
);
Assert.Equal( 2, stream.ObservationCount );
Assert.True( stream.CanRead );
```

For the injected I/O case use `Assert.Same( expected, actual )`. For the borrowed stream case call the existing `ReadDatabase(Stream, int)` and assert only one observation was consumed.

- [ ] **Step 3: Run the focused test and verify the behavioral RED**

Run when a local SDK is available:

```bash
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release --filter FullyQualifiedName~Hdb07cAcquisitionStabilityTests
```

Expected: the identical-observation cases fail because only one observation occurs; same-length mutation and second-length changes fail because no exception is thrown; second-pass I/O fails because the second observation is never read. The borrowed single-pass case passes.

In this environment, push the test/seam commit and require the same focused failures on all three TFMs before implementation.

- [ ] **Step 4: Commit the RED**

```bash
git add Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashReader.cs tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb07cAcquisitionStabilityTests.cs
git commit -m "test: require HDB07C stable acquisition"
```

Record the exact commit, workflow run, per-TFM failure count, and exception mismatch. Do not edit implementation until the Linux RED is observed.

- [ ] **Step 5: Implement bounded verification without a second database-sized array**

Replace the seam body with validation plus a private verifier:

```csharp
internal static byte[] ReadStableDatabase(
	Stream stream,
	int maximumDatabaseSize
) {
	ArgumentNullException.ThrowIfNull( stream );
	ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumDatabaseSize );

	byte[] database = ReadDatabase( stream, maximumDatabaseSize );
	VerifyStableObservation( stream, database );
	return database;
}

private static void VerifyStableObservation(
	Stream stream,
	byte[] expected
) {
	stream.Position = 0;
	if ( stream.Length != expected.LongLength ) {
		throw CreateChangedWhileReadingException();
	}

	byte[] buffer = new byte[Math.Min( 81920, expected.Length )];
	int offset = 0;
	while ( offset < expected.Length ) {
		int count = stream.Read(
			buffer,
			0,
			Math.Min( buffer.Length, expected.Length - offset )
		);
		if (
			count == 0
			|| !expected.AsSpan( offset, count ).SequenceEqual(
				buffer.AsSpan( 0, count )
			)
		) {
			throw CreateChangedWhileReadingException();
		}
		offset += count;
	}

	if (
		stream.ReadByte() != -1
		|| stream.Length != expected.LongLength
	) {
		throw CreateChangedWhileReadingException();
	}
}

private static IOException CreateChangedWhileReadingException() {
	return new IOException(
		"The Berkeley DB file changed while it was being read."
	);
}
```

If `expected.Length` could be zero, the existing minimum-length validation already rejects it before verification. Do not catch an `IOException` thrown by `Read`; it must retain identity. Do not change `FileShare.Read` or add retry.

- [ ] **Step 6: Run focused and complete BerkeleyDb tests**

```bash
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release --filter FullyQualifiedName~Hdb07cAcquisitionStabilityTests
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release
```

Expected: PASS on net8/net9/net10. Existing HDB02 non-seekable borrowed-stream tests remain green.

- [ ] **Step 7: Commit the GREEN and qualify the checkpoint**

```bash
git add Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashReader.cs
git commit -m "fix: reject changed HDB07C acquisitions"
```

Push and require normal CI 12/12 plus HDB00 3/3 because production acquisition changed. Record exact counts and verify permission/handle-release evidence remains green.

---

### Task 2: Native big-endian Berkeley DB container

**Files:**
- Create: `tools/hdb00/hdb07c_repack.c`
- Create: `tools/hdb00/native-verifier/Hdb07c.NativeVerifier.csproj`
- Create: `tools/hdb00/native-verifier/Program.cs`
- Modify: `tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/src/NativeOracleTests.cs`
- Modify: `tools/hdb00/run-linux.sh`
- Modify: `tools/hdb00/run-macos.sh`
- Modify: `tools/hdb00/verify-managed.sh`
- Modify: `tools/hdb00/verify-hdb06-commands.ps1`
- Modify: `.github/workflows/hdb00-interoperability.yml`

**Interfaces:**
- Consumes: `hashed-db.db`, the three-record ncurses native source store.
- Produces: `big-endian-hashed-db.db` and `big-endian-hashed-db.dump` with identical application records and big-endian Berkeley DB metadata.
- Produces: CI-only executable interface `hdb07c_repack SOURCE_DATABASE DESTINATION_DATABASE 4321`.

- [ ] **Step 1: Add fixture expectations before generation**

Extend `NativeOracleTests.ProductionReaderMatchesEveryNativeRecord`:

```csharp
[InlineData( "big-endian-hashed-db", 3 )]
```

Add provider and catalog cases using `hdb00-primary` and `hdb00-alias`, and assert every record from `big-endian-hashed-db.dump` matches `BerkeleyDbHashReader`.

In the HDB00 workflow:

- run native `db_dump -k` for `big-endian-hashed-db.db`;
- add both new files to Linux and macOS artifacts;
- add them to the Windows download checks; and
- pass the same fixture root to the interop project.

In `verify-managed.sh`, require `big-endian-hashed-db.db`, extract `hdb00-primary`, compare it with `hdb00-primary.bin`, and assert output contains `Byte order: big-endian`.

In `verify-hdb06-commands.ps1`, require the new database and add direct/routed `infocmp` and `toe` assertions equal to the primary little-endian store.

Do not generate the file in this step.

- [ ] **Step 2: Commit and observe the native-evidence RED**

```bash
git add tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/src/NativeOracleTests.cs tools/hdb00/verify-managed.sh tools/hdb00/verify-hdb06-commands.ps1 .github/workflows/hdb00-interoperability.yml
git commit -m "test: require native HDB07C big-endian evidence"
```

Expected HDB00 result: Linux and macOS fail on the missing `big-endian-hashed-db.db`; Windows skips because Linux did not publish the required artifact. The ordinary workflow may remain green.

- [ ] **Step 3: Implement the CI-only native repacker**

Create `hdb07c_repack.c` with this exact operating sequence:

```c
DB *source = NULL;
DB *destination = NULL;
DBC *cursor = NULL;
DBT key;
DBT value;
int result = 1;

db_create(&source, NULL, 0);
source->open(source, NULL, argv[1], NULL, DB_UNKNOWN, DB_RDONLY, 0);

db_create(&destination, NULL, 0);
destination->set_lorder(destination, atoi(argv[3]));
destination->open(
	destination,
	NULL,
	argv[2],
	NULL,
	DB_HASH,
	DB_CREATE | DB_TRUNCATE,
	0600
);

source->cursor(source, NULL, &cursor, 0);
memset(&key, 0, sizeof(key));
memset(&value, 0, sizeof(value));
while ((status = cursor->get(cursor, &key, &value, DB_NEXT)) == 0) {
	status = destination->put(destination, NULL, &key, &value, DB_NOOVERWRITE);
	if (status != 0) {
		fprintf(stderr, "destination put: %s\n", db_strerror(status));
		goto cleanup;
	}
}
if (status != DB_NOTFOUND) {
	fprintf(stderr, "source cursor get: %s\n", db_strerror(status));
	goto cleanup;
}
result = 0;

cleanup:
if (cursor != NULL) {
	status = cursor->close(cursor);
	if (status != 0) {
		fprintf(stderr, "cursor close: %s\n", db_strerror(status));
		result = 1;
	}
}
if (destination != NULL) {
	status = destination->close(destination, 0);
	if (status != 0) {
		fprintf(stderr, "destination close: %s\n", db_strerror(status));
		result = 1;
	}
}
if (source != NULL) {
	status = source->close(source, 0);
	if (status != 0) {
		fprintf(stderr, "source close: %s\n", db_strerror(status));
		result = 1;
	}
}
return result;
```

The completed file must:

- require exactly three arguments;
- accept only `1234` or `4321`;
- check every `db_create`, method, cursor, and close return value;
- close cursor before databases;
- close the destination before source;
- report failures to stderr with the failing operation and `db_strerror`;
- print copied record count and selected order; and
- compile with `-std=c11 -Wall -Wextra -Werror` on Linux and macOS.

- [ ] **Step 4: Add an independent managed native-evidence verifier**

Create a package-free net8.0 console project under `tools/hdb00/native-verifier`.
Its initial interface is:

```text
Hdb07c.NativeVerifier DATABASE SOURCE_DUMP DESTINATION_DUMP
```

The verifier must independently require the big-endian Hash magic bytes
`00 06 15 61` at metadata offset 12, parse the bytevalue dump lines after
`HEADER=END`, require `DATA=END`, build sorted exact key/value hex tuples,
require source and destination tuples to be identical, require exactly three
records, and print:

```text
HDB07C byte order: big-endian
HDB07C native big-endian records: 3
```

- [ ] **Step 5: Generate and verify the big-endian database on Linux and macOS**

In each native script:

1. define the repacker executable and destination paths;
2. compile `hdb07c_repack.c` with the same Berkeley DB include/library settings as `hdb00_probe.c`;
3. run `hdb07c_repack "$hashed_db" "$big_endian_db" 4321` under the macOS `DYLD_LIBRARY_PATH` wrapper when required;
4. dump source and destination with `-k`;
5. run the managed native-evidence verifier to assert metadata order and exact parsed `(key,value)` parity rather than textual header equality;
6. use the native probe for canonical and alias extraction and compare both with `hdb00-primary.bin`; and
7. print exactly `HDB07C native big-endian records: 3`.

The record comparator must parse the bytevalue dump lines after `HEADER=END`, require `DATA=END`, build sorted `(key,value)` byte tuples, and fail unless the tuples are exactly equal.

- [ ] **Step 6: Run static and native checks**

```bash
bash -n tools/hdb00/run-linux.sh
bash -n tools/hdb00/run-macos.sh
dotnet build tools/hdb00/native-verifier/Hdb07c.NativeVerifier.csproj -c Release
```

On CI, require native creation/dump/probe success on Linux and macOS, managed interop tests on all three TFMs/hosts, command verification on all hosts, and Windows reading the transported Linux fixture.

- [ ] **Step 7: Commit the GREEN**

```bash
git add tools/hdb00/hdb07c_repack.c tools/hdb00/native-verifier tools/hdb00/run-linux.sh tools/hdb00/run-macos.sh
git commit -m "test: produce native HDB07C big-endian stores"
```

Record the exact HDB00 and normal workflow results. If native evidence exposes a production failure, stop, reduce it to a synthetic managed RED in a separate commit, and correct only that demonstrated defect.

---

### Task 3: UTF-8-first provider lookup with Latin-1 fallback

**Files:**
- Create: `Icod.TermInfo.BerkeleyDb/src/TerminalNameEncoding.cs`
- Create: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb07cEncodingCompatibilityTests.cs`
- Modify: `Icod.TermInfo.BerkeleyDb/src/NcursesRecordReader.cs`
- Modify: `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalDescriptionProvider.cs`

**Interfaces:**
- Produces: `TerminalNameEncoding.EncodeUtf8(string) -> byte[]`.
- Produces: `TerminalNameEncoding.TryEncodeDistinctLatin1(string, ReadOnlySpan<byte>, out byte[]) -> bool`.
- Produces: `NcursesRecordReader.TryReadCompiledEntry(byte[] database, ReadOnlySpan<byte> requestedKey, out byte[] compiledEntry, int maximumItemSize, int maximumIndexHops) -> bool`.
- Preserves: the path overload and every existing default/limit/exception boundary.

- [ ] **Step 1: Add provider and precedence RED tests**

Use `Hdb07HashV9FixtureBuilder` to create exact Latin-1 key bytes and compiled names for:

```csharp
private const string Canonical = "hdb07c-caf\u00E9";
private const string Alias = "hdb07c-ali\u00E9";
```

Add:

```csharp
[Theory]
[InlineData( Canonical )]
[InlineData( Alias )]
public void ProviderLoadsExactLatin1CanonicalAndAliasAfterUtf8Miss(
	string requestedName
)

[Theory]
[InlineData( Canonical )]
[InlineData( Alias )]
public void SystemProviderLoadsExactLatin1CanonicalAndAlias(
	string requestedName
)

[Fact]
public void ProviderDoesNotUseLatin1AfterFoundUtf8EnvelopeFailure()

[Fact]
public void ProviderReturnsCleanMissWhenBothPermittedCandidatesAreAbsent()

[Fact]
public void NonLatin1NameUsesOnlyUtf8Candidate()
```

For the precedence test, create a marker-7 value under `Encoding.UTF8.GetBytes(Canonical)` and a valid Latin-1 record under `Encoding.Latin1.GetBytes(Canonical)`. Assert `BerkeleyDbDatabaseFormatException` with an inner `InvalidDataException` whose message is `Ncurses hashed-term marker 7 is not supported.`

The clean-miss and non-Latin-1 tests remain characterization; canonical/alias provider and system-provider cases are the expected RED.

- [ ] **Step 2: Run and commit the provider RED**

```bash
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release --filter FullyQualifiedName~Hdb07cEncodingCompatibilityTests
git add tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb07cEncodingCompatibilityTests.cs
git commit -m "test: require HDB07C Latin-1 provider fallback"
```

Expected: exact Latin-1 provider and system-provider cases return clean misses. Precedence and clean-miss controls pass. Observe the RED on Linux before production edits.

- [ ] **Step 3: Add the internal encoding policy**

Create `TerminalNameEncoding.cs` with a private strict UTF-8 encoder:

```csharp
private static readonly UTF8Encoding StrictUtf8 =
	new(
		encoderShouldEmitUTF8Identifier: false,
		throwOnInvalidBytes: true
	);

internal static byte[] EncodeUtf8( string name ) {
	ArgumentNullException.ThrowIfNull( name );
	return StrictUtf8.GetBytes( name );
}

internal static bool TryEncodeDistinctLatin1(
	string name,
	ReadOnlySpan<byte> utf8,
	out byte[] latin1
) {
	ArgumentNullException.ThrowIfNull( name );
	latin1 = new byte[name.Length];
	for ( int index = 0; index < name.Length; index++ ) {
		if ( name[index] > '\u00FF' ) {
			latin1 = [];
			return false;
		}
		latin1[index] = (byte)name[index];
	}

	if ( utf8.SequenceEqual( latin1 ) ) {
		latin1 = [];
		return false;
	}
	return true;
}
```

The validator already rejects surrogate code units before encoding. Do not use `Encoding.Latin1.GetBytes` with replacement for candidate eligibility.

- [ ] **Step 4: Extract one-image record resolution**

Move the existing marker loop into the new `byte[] database` overload. Keep the path overload as:

```csharp
byte[] database = BerkeleyDbHashReader.ReadDatabase(
	databasePath,
	maximumDatabaseSize
);
return TryReadCompiledEntry(
	database,
	requestedKey,
	out compiledEntry,
	maximumItemSize,
	maximumIndexHops
);
```

The array overload validates null/positive limits exactly as the path overload does and does not copy the database. Preserve cycle tracking, clean initial miss, hop limits, and marker errors byte-for-byte.

- [ ] **Step 5: Implement one-acquisition UTF-8-first provider lookup**

In `LoadUncached`:

1. acquire one database image through `BerkeleyDbHashReader.ReadDatabase`;
2. encode UTF-8 with `TerminalNameEncoding.EncodeUtf8`;
3. try that key once;
4. only after `false`, obtain a distinct exact Latin-1 candidate;
5. try the Latin-1 key against the same database image; and
6. preserve the current parse and string identity verification.

Use this decision shape:

```csharp
byte[] database = BerkeleyDbHashReader.ReadDatabase(
	DatabasePath,
	_maximumDatabaseSize
);
byte[] utf8 = TerminalNameEncoding.EncodeUtf8( name );
bool found = NcursesRecordReader.TryReadCompiledEntry(
	database,
	utf8,
	out compiledEntry,
	MaximumStoredItemSize,
	_maximumIndexHops
);
if (
	!found
	&& TerminalNameEncoding.TryEncodeDistinctLatin1(
		name,
		utf8,
		out byte[] latin1
	)
) {
	found = NcursesRecordReader.TryReadCompiledEntry(
		database,
		latin1,
		out compiledEntry,
		MaximumStoredItemSize,
		_maximumIndexHops
	);
}
if ( !found ) {
	return null;
}
```

Keep acquisition and resolver `InvalidDataException` wrapping unchanged. Update the XML remarks to say exact ordinal UTF-8 first and exact Latin-1 after a clean miss.

- [ ] **Step 6: Run provider, resolver, and full BerkeleyDb tests**

```bash
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release --filter "FullyQualifiedName~Hdb07cEncodingCompatibilityTests|FullyQualifiedName~Hdb03|FullyQualifiedName~Hdb07LogicalHardeningTests"
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release
```

Expected: all net8/net9/net10 tests pass; found UTF-8 failures do not fall through; one acquired image serves both candidates.

- [ ] **Step 7: Commit the provider GREEN**

```bash
git add Icod.TermInfo.BerkeleyDb/src/TerminalNameEncoding.cs Icod.TermInfo.BerkeleyDb/src/NcursesRecordReader.cs Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalDescriptionProvider.cs
git commit -m "feat: add bounded HDB07C name fallback"
```

Require normal 12/12 and HDB00 3/3 because BerkeleyDb production changed.

---

### Task 4: Catalog Latin-1 decoding and ambiguity rejection

**Files:**
- Modify: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb07cEncodingCompatibilityTests.cs`
- Modify: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb07LogicalHardeningTests.cs`
- Modify: `Icod.TermInfo.BerkeleyDb/src/TerminalNameEncoding.cs`
- Modify: `Icod.TermInfo.BerkeleyDb/src/NcursesCatalogReader.cs`

**Interfaces:**
- Produces: `TerminalNameEncoding.DecodePublicationName(ReadOnlySpan<byte>) -> string`, strict UTF-8 first and Latin-1 only after decoder failure.
- Produces: a preflight map from each marker-2 `BerkeleyDbHashRecord` to one validated logical name.
- Preserves: raw-byte structural order and final ordinal name/kind/canonical ordering.

- [ ] **Step 1: Add catalog RED cases**

Add:

```csharp
[Fact]
public void CatalogEnumeratesExactLatin1CanonicalAndAlias()

[Fact]
public void CatalogRejectsDistinctRawKeysWithOneDecodedLogicalName()

[Fact]
public void CatalogRejectsUnsafeNameAfterLatin1Fallback()
```

The first database uses Latin-1 canonical/alias marker-2 keys and a Latin-1 storage target. Assert two entries, canonical/alias kinds, ordinal names, and shared `TerminalDescription` identity.

The ambiguity database contains raw UTF-8 `Encoding.UTF8.GetBytes(Canonical)` and raw Latin-1 `Encoding.Latin1.GetBytes(Canonical)` marker-2 keys. Assert:

```csharp
BerkeleyDbDatabaseFormatException error = Assert.Throws<BerkeleyDbDatabaseFormatException>(
	() => reader.Read()
);
Assert.Equal(
	"The ncurses catalog contains more than one exact byte key for logical publication 'hdb07c-caf\u00E9'.",
	error.Message
);
```

For unsafe fallback use invalid UTF-8 bytes containing a NUL after Latin-1 decoding, such as `[ 0xE9, 0x00 ]`, and expect:

```text
The ncurses publication key is not a safe exact UTF-8 or Latin-1 terminal name.
```

Update HDB07's prior invalid-UTF-8 case to this new unsafe control. Do not retain `[ 0xC3, 0x28 ]` as categorically invalid: under the approved policy it is valid Latin-1 text and must proceed to identity checking.

- [ ] **Step 2: Run and commit the catalog RED**

```bash
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release --filter "FullyQualifiedName~Hdb07cEncodingCompatibilityTests|FullyQualifiedName~Hdb07LogicalHardeningTests"
git add tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb07cEncodingCompatibilityTests.cs tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb07LogicalHardeningTests.cs
git commit -m "test: require HDB07C Latin-1 catalogs"
```

Expected: Latin-1 catalog success and ambiguity cases fail; unsafe-name control may fail by message. Observe the Linux RED before implementation.

- [ ] **Step 3: Implement strict UTF-8-first decoding**

In `TerminalNameEncoding` add:

```csharp
internal static string DecodePublicationName(
	ReadOnlySpan<byte> bytes
) {
	try {
		return StrictUtf8.GetString( bytes );
	} catch ( DecoderFallbackException ) {
		return Encoding.Latin1.GetString( bytes );
	}
}
```

Catch only `DecoderFallbackException`; safety validation remains the catalog's responsibility.

- [ ] **Step 4: Preflight publication names and reject ambiguity**

Before logical resolution, scan records in their existing raw-byte order. For each nonempty marker-2 value:

1. decode via `TerminalNameEncoding.DecodePublicationName`;
2. call `TerminalNameValidator.Validate`;
3. convert validation failures to the exact UTF-8-or-Latin-1 format message;
4. add the logical name to `Dictionary<string, byte[]>(StringComparer.Ordinal)`; and
5. if the logical name already maps to a different raw key, throw the exact ambiguity exception.

Store the validated name in `Dictionary<BerkeleyDbHashRecord, string>` and use that name during the existing main record loop. Do not resolve targets or parse compiled entries during preflight.

Remove the private strict UTF-8 field and decoder from `NcursesCatalogReader`; the encoding policy has one owner.

- [ ] **Step 5: Run focused and complete tests**

```bash
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release --filter "FullyQualifiedName~Hdb07cEncodingCompatibilityTests|FullyQualifiedName~Hdb07LogicalHardeningTests|FullyQualifiedName~Hdb05CatalogReaderTests"
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release
```

Expected: all pass across net8/net9/net10. Existing culture-order tests remain ordinal and restored.

- [ ] **Step 6: Commit the catalog GREEN**

```bash
git add Icod.TermInfo.BerkeleyDb/src/TerminalNameEncoding.cs Icod.TermInfo.BerkeleyDb/src/NcursesCatalogReader.cs
git commit -m "feat: read bounded HDB07C Latin-1 catalogs"
```

Require the complete normal and HDB00 workflows green.

---

### Task 5: Native Latin-1 ncurses producer evidence

**Files:**
- Modify: `tools/hdb00/hdb00_probe.c`
- Modify: `tools/hdb00/native-verifier/Program.cs`
- Modify: `tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/src/NativeOracleTests.cs`
- Modify: `tools/hdb00/run-linux.sh`
- Modify: `tools/hdb00/run-macos.sh`
- Modify: `tools/hdb00/verify-hdb06-commands.ps1`
- Modify: `.github/workflows/hdb00-interoperability.yml`

**Interfaces:**
- Produces: `latin1-hashed-db.db`, `latin1-hashed-db.dump`, and `hdb07c-latin1.bin`.
- Uses exact canonical bytes `6864623037632d636166e9` (`hdb07c-caf` + `E9`).
- Uses exact alias bytes `6864623037632d616c69e9` (`hdb07c-ali` + `E9`).

- [ ] **Step 1: Add native fixture expectations before generation**

Extend interop tests with:

```csharp
[InlineData( "latin1-hashed-db", 3 )]
```

Add tests asserting:

- native dump contains the exact canonical and alias bytes above;
- public provider loads `hdb07c-caf\u00E9` and `hdb07c-ali\u00E9`;
- system provider loads both through `TERMINFO`;
- catalog emits both logical names and the correct kinds; and
- canonical and alias share the same terminal instance.

Add the new files to workflow dumps, Linux/macOS uploads, and Windows downloads. Extend command verification to require the database and construct names without source-file encoding dependence:

```powershell
$latin1Canonical = 'hdb07c-caf' + [char]0x00E9
$latin1Alias = 'hdb07c-ali' + [char]0x00E9
```

Assert direct/routed `infocmp` succeeds for both and `toe -s` emits exactly both publications.

Do not generate the fixture in this step.

- [ ] **Step 2: Commit and observe the native Latin-1 evidence RED**

```bash
git add tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/src/NativeOracleTests.cs tools/hdb00/verify-hdb06-commands.ps1 .github/workflows/hdb00-interoperability.yml
git commit -m "test: require native HDB07C Latin-1 evidence"
```

Expected HDB00 result: Linux/macOS fail because `latin1-hashed-db.db` is missing; Windows skips. This evidence RED is separate from the already observed managed behavior RED.

- [ ] **Step 3: Add explicit arbitrary-byte lookup to the native probe**

Extend `hdb00_probe.c` without changing its existing interface. Add:

```text
hdb00_probe DATABASE --key-hex KEY_HEX OUTPUT
```

The hex mode must require nonempty even-length ASCII hex, reject invalid digits
and decoded NUL, allocate the exact decoded bytes, use their explicit length for
Berkeley DB lookup, retain the existing marker/hop behavior, and print the exact
lookup key hex. Existing text-key calls and output remain unchanged.

- [ ] **Step 4: Generate the source as exact bytes and compile it with native tic**

In each Bash native script write the source as exact bytes without a new
language dependency: emit the ASCII fragments with `printf '%s'` and the two
Latin-1 bytes with `printf '\351'`, then emit the remaining capability lines as
ASCII. Compile it with the already built hashed `tic` into
`latin1-hashed-db.db`. Use the native probe's `--key-hex` mode with the
canonical and alias hex keys, compare both payloads, and save the canonical
output as `hdb07c-latin1.bin`.

Dump the database with `db_dump -k`. Extend the managed native-evidence verifier
with a Latin-1 dump mode and require:

- exactly three records;
- both exact publication keys;
- one marker-0 value and two marker-2 values; and
- each marker-2 target equals the exact storage key byte sequence.

Print exactly:

```text
HDB07C native Latin-1 records: 3
HDB07C native Latin-1 publications: 2
```

- [ ] **Step 5: Run static checks and qualify all hosts**

```bash
bash -n tools/hdb00/run-linux.sh
bash -n tools/hdb00/run-macos.sh
dotnet build tools/hdb00/native-verifier/Hdb07c.NativeVerifier.csproj -c Release
```

Require Linux/macOS native creation, byte-key probe, dump, and managed interop success. Require Windows provider/system-provider/catalog/command success against the Linux artifact without Berkeley DB installed.

- [ ] **Step 6: Commit the native Latin-1 GREEN**

```bash
git add tools/hdb00/hdb00_probe.c tools/hdb00/native-verifier/Program.cs tools/hdb00/run-linux.sh tools/hdb00/run-macos.sh
git commit -m "test: produce native HDB07C Latin-1 stores"
```

Record exact test counts, key hex, workflow IDs, and heads.

---

### Task 6: Package-only and distribution-boundary evidence

**Files:**
- Modify: `tools/berkeleydb-package-smoke/Program.cs`
- Modify: `Icod.TermInfo.BerkeleyDb/README.md`
- Modify: `Icod.TermInfo.BerkeleyDb/Icod.TermInfo.BerkeleyDb.csproj`

**Interfaces:**
- Consumes: packed `Icod.TermInfo.BerkeleyDb` plus Runtime packages only.
- Produces: package-only Latin-1 provider/catalog/system-provider evidence on net8/net9/net10.
- Preserves: no native package asset and unchanged public API/dependency graph.

- [ ] **Step 1: Extend the package smoke with a synthetic Latin-1 store**

Change `CreateStore` to accept an exact key encoding:

```csharp
static byte[] CreateStore(
	string canonical,
	string alias,
	string description,
	Encoding keyEncoding
)
```

Use `keyEncoding.GetBytes` for canonical, alias, and storage target keys. Continue to use `Encoding.Latin1` for the compiled names section. Update existing calls to pass `Encoding.UTF8`.

Create a second temporary database using:

```csharp
const string latin1Canonical = "hdb07c-package-caf\u00E9";
const string latin1Alias = "hdb07c-package-ali\u00E9";
```

and `Encoding.Latin1`. Prove:

- provider canonical and alias load;
- returned identity is exact;
- catalog contains exactly canonical and alias with correct kinds/shared identity;
- the opt-in system provider resolves the alias; and
- the file can be deleted in `finally`.

- [ ] **Step 2: Update package-facing descriptions precisely**

Update provider remarks, package README, and package release notes to state:

- strict UTF-8 exact lookup first;
- exact Latin-1 lookup only after a clean miss when representable;
- strict UTF-8 catalog decoding first and Latin-1 only for invalid UTF-8 bytes;
- two-observation production acquisition detects unequal reads but is not an atomic snapshot; and
- native tools remain CI-only.

Do not claim universal non-ASCII support or execution on a big-endian host.

- [ ] **Step 3: Run package and structural verification**

Use the repository's ordinary PR pipeline to run:

- exact Staging package construction;
- package-only consumer on net8/net9/net10;
- cross-TFM API comparison;
- project-reference/dependency verification;
- native-asset absence verification;
- installed tool smoke on Windows/Linux/macOS; and
- all six archive RID smokes.

Expected: no public API or dependency delta, no `runtimes/*/native` asset, and package consumer success on every TFM.

- [ ] **Step 4: Commit distribution evidence**

```bash
git add tools/berkeleydb-package-smoke/Program.cs Icod.TermInfo.BerkeleyDb/README.md Icod.TermInfo.BerkeleyDb/Icod.TermInfo.BerkeleyDb.csproj
git commit -m "test: qualify HDB07C package behavior"
```

Require normal 12/12. HDB00 runs because package/README paths are interoperability-sensitive under the current classifier; require 3/3 if enabled.

---

### Task 7: Final qualification and HDB07C closure

**Files:**
- Create: `docs/1.15.0-HDB07C-COMPATIBILITY-EXPANSION.md`
- Modify: `docs/superpowers/specs/2026-09-16-hdb07c-compatibility-expansion-design.md`
- Modify: `docs/superpowers/plans/2026-09-16-hdb07c-compatibility-expansion.md`
- Modify: `Icod.TermInfo-1.15.0-Berkeley-DB-Hashed-Terminfo-Acquisition-Roadmap.md`
- Modify: `docs/1.15.0-HDB07-ADVERSARIAL-COMPATIBILITY-HARDENING.md`
- Modify: `README.md`
- Modify: `Icod.TermInfo.BerkeleyDb/README.md`
- Modify: `infocmp/README.md`
- Modify: `toe/README.md`
- Update: PR #45 body

**Interfaces:**
- Consumes: exact accepted heads and workflow evidence from Tasks 1–6.
- Produces: auditable HDB07C acceptance and HDB08 / Alpha-8 as the next tranche.

- [ ] **Step 1: Run final implementation qualification**

On the exact implementation head run or observe:

```text
normal pull-request workflow: 12/12 jobs
hdb00-interoperability: 3/3 jobs
```

Capture:

- exact head SHA;
- workflow IDs and URLs;
- BerkeleyDb unit count per TFM and host;
- native interop count per TFM and host;
- `infocmp`, `toe`, and router counts per host;
- big-endian source/destination record counts and byte-order assertion;
- Latin-1 key hex and publication counts;
- Windows transported-fixture results;
- package-only net8/net9/net10 results;
- installed-tool and six-RID archive results; and
- API, dependency, assembly-version, JSON, no-native-asset, and read-only boundaries.

Do not begin closure edits until both workflows are green on the same exact implementation head.

- [ ] **Step 2: Write the closure record**

Create `docs/1.15.0-HDB07C-COMPATIBILITY-EXPANSION.md` with:

- purpose and accepted scope;
- RED and GREEN commit/run evidence for each checkpoint;
- exact observed-stability contract and residual race boundary;
- exact big-endian producer wording;
- exact UTF-8-first/Latin-1-fallback policy and ambiguity behavior;
- three-host native/cross-host evidence;
- package/distribution evidence;
- unchanged public/dependency/JSON/native/write contracts;
- accepted implementation head; and
- HDB08 as next.

- [ ] **Step 3: Synchronize roadmap and READMEs**

Mark HDB07C complete without rewriting HDB07's accepted head. Replace only the residual limitations actually closed:

- native big-endian container evidence is now qualified under the exact wording;
- the bounded Latin-1 subset is now qualified;
- unequal two-observation file reads are rejected;
- atomic snapshot and arbitrary writer coordination remain unclaimed.

State that the suite remains `1.15.0-Alpha-7` and HDB08 advances to Alpha-8.

- [ ] **Step 4: Mark plan/spec acceptance and commit closure**

Mark completed plan checkboxes `[x]`, change the design status to accepted, and commit:

```bash
git add docs/1.15.0-HDB07C-COMPATIBILITY-EXPANSION.md docs/superpowers/specs/2026-09-16-hdb07c-compatibility-expansion-design.md docs/superpowers/plans/2026-09-16-hdb07c-compatibility-expansion.md Icod.TermInfo-1.15.0-Berkeley-DB-Hashed-Terminfo-Acquisition-Roadmap.md docs/1.15.0-HDB07-ADVERSARIAL-COMPATIBILITY-HARDENING.md README.md Icod.TermInfo.BerkeleyDb/README.md infocmp/README.md toe/README.md
git commit -m "docs: accept HDB07C compatibility expansion"
```

- [ ] **Step 5: Verify the documentation head**

Require the normal PR workflow 12/12. The HDB00 classifier should perform only inexpensive gate jobs for a documentation-only delta; verify that no required gate fails.

Run:

```bash
git diff --check HEAD^
git status --short --branch
```

Expected: no whitespace errors and a clean branch synchronized with origin.

- [ ] **Step 6: Update and verify PR #45**

Update the PR body with accepted HDB07C scope, exact implementation/documentation heads, RED/GREEN evidence, workflow links, counts, preserved boundaries, residual limitations, and HDB08 next.

Re-fetch PR metadata and require:

```text
state: open
draft: true
merged: false
merged_at: null
head_sha: exact documentation head
```

Do not merge or mark ready for review.

---

## Plan self-review

- **Spec coverage:** Sections 6–9 map to Tasks 1, 3, and 4; native big-endian Sections 7/11 map to Task 2; native Latin-1 Sections 8/11 map to Task 5; package/CI Sections 12–14 map to Tasks 6–7; every explicit non-goal is copied into global constraints or closure checks.
- **Placeholder scan:** The plan contains no deferred implementation markers; every created interface, fixture filename, key byte sequence, exception message, command, and commit boundary is specified.
- **Type consistency:** `ReadStableDatabase(Stream,int)`, `TerminalNameEncoding` methods, the array `TryReadCompiledEntry` overload, fixture filenames, Unicode names, and raw key hex are consistent across all consuming tasks.
- **Execution mode:** The user previously selected inline execution. Use `superpowers:executing-plans`; do not dispatch subagents.
