# HW02 Deterministic Hash-v9 Inline Image Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the internal deterministic three-page Berkeley DB Hash-v9 image path for validated inline ncurses terminfo publications without changing the frozen public writer behavior.

**Architecture:** HW01 preflight preserves exact compiled identity bytes in internal prepared publications. An internal ncurses planner converts those publications into immutable, canonically ordered Hash records, and an internal image builder hashes them into two fixed 4096-byte buckets plus deterministic metadata. Public writer integration, bucket growth, overflow pages, filesystem publication, and native overflow qualification remain in later tranches.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit 2.9, `System.Buffers.Binary`, `System.Security.Cryptography`, PowerShell 5.1-compatible verification, and GitHub Actions.

**Spec:** `docs/superpowers/specs/2026-09-18-hw02-deterministic-hash-v9-inline-image-design.md`

## Global Constraints

- Keep the accepted HW01 public API exactly unchanged at 12 exported BerkeleyDb types.
- Keep `BerkeleyDbTerminalDatabaseWriter.Write` at its exact HW01 `NotSupportedException` construction boundary through HW02.
- Keep `Icod.TermInfo.BerkeleyDb` dependent only on `Icod.TermInfo`; add no package, P/Invoke, or native production dependency.
- Keep reusable assembly versions at `1.0.0.0` and API equivalent on net8.0, net9.0, and net10.0.
- Advance the coordinated package version to exactly `1.16.0-Alpha-2` with the first compiling HW02 RED checkpoint.
- Emit only little-endian Hash-v9 images with 4096-byte pages, exactly two buckets, and exactly 12,288 total bytes.
- Preserve the compiled names section excluding its terminal NUL as the exact storage key; never reconstruct it from strings.
- Use exact UTF-8 publication keys and the HW00-qualified Hash-v9 function.
- Use a writer-only longer-before-exact-prefix comparer; do not change the accepted reader `ByteArrayComparer`.
- Reject payloads over 1,024 bytes, overfull buckets, and insufficient maximum database size; do not add overflow behavior in HW02.
- Preserve strict RED -> GREEN commits and exact-head CI evidence.
- Add or modify only C#, PowerShell 5.1-compatible, cmd/sh, XML/MSBuild, and Markdown files; do not add Python, C, or C++.
- Develop inline in the current isolated feature branch; do not dispatch subagents.

---

## File Structure

- Create `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw02WriterImageTests.cs`: storage-key, record-planning, layout, bounds, determinism, and managed round-trip tests.
- Create `tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/src/Hw02ProductionWriterProofTests.cs`: complete byte equality between production HW02 output and the accepted HW00 research oracle.
- Modify `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalDatabaseWriter.Preflight.cs`: internal prepared-state seam and exact storage-key extraction.
- Create `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbNcursesRecordPlanner.cs`: ncurses marker envelopes, duplicate protection, and deterministic record ordering.
- Create `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashV9WriterKeyComparer.cs`: Hash-v9 writer ordering without changing reader ordering.
- Create `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashV9ImageBuilder.cs`: hashing, deterministic file ID, metadata, two buckets, inline items, and checked bounds.
- Modify `Directory.Build.props`: advance the active coordinated version to `1.16.0-Alpha-2`.
- Modify `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb01ContractTests.cs`: active development-version expectation only.
- Create `docs/1.16.0-HW02-DETERMINISTIC-HASH-V9-INLINE-IMAGE.md`: accepted exact-head evidence after GREEN.
- Modify `Icod.TermInfo-1.16.0-Berkeley-DB-Hash-V9-Writer-Roadmap.md`: HW02 completion and HW03 next gate after acceptance.
- Modify `Icod.TermInfo-Post-1.0-Development-Roadmap.md`: coordinated 1.16 progress after acceptance.

---

### Task 1: HW02 RED Version Gate and Exact Storage-Key Preparation

**Files:**
- Create: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw02WriterImageTests.cs`
- Modify: `Directory.Build.props`
- Modify: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb01ContractTests.cs`
- Modify: `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalDatabaseWriter.Preflight.cs`

**Interfaces:**
- Consumes: HW01 `PreparePublications`, `PreparedIdentity`, and `PreparedPublication` implementation.
- Produces: internal `PreparePublications(IReadOnlyList<BerkeleyDbTerminalDatabaseEntry>, BerkeleyDbTerminalDatabaseWriterOptions, CancellationToken)`, internal immutable prepared records, and `PreparedPublication.StorageKey` containing the exact compiled names section without its terminal NUL.

- [ ] **Step 1: Add a compiling reflection-first RED test**

Create `Hw02WriterImageTests` with a helper that invokes the nonpublic static
`PreparePublications` method. The first test must remain compilable before the
internal visibility change and require a `StorageKey` property:

```csharp
[Fact]
public void PreparedPublicationPreservesExactCompiledNamesSection() {
	byte[] compiled = Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
		"hw02-primary",
		"HW02 caf\u00E9 description",
		"hw02-alias"
	);
	var entry = new BerkeleyDbTerminalDatabaseEntry(
		"hw02-primary",
		[ "hw02-alias" ],
		compiled
	);

	Array prepared = InvokePreparePublications( [ entry ] );
	object publication = Assert.Single( prepared.Cast<object>() );
	PropertyInfo? property = publication.GetType().GetProperty(
		"StorageKey",
		BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
	);

	Assert.NotNull( property );
	Assert.Equal(
		Encoding.Latin1.GetBytes(
			"hw02-primary|hw02-alias|HW02 caf\u00E9 description"
		),
		Assert.IsType<byte[]>( property!.GetValue( publication ) )
	);
}
```

`InvokePreparePublications` locates the method with
`BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public`, passes a
`BerkeleyDbTerminalDatabaseEntry[]`, default writer options, and
`CancellationToken.None`, and unwraps `TargetInvocationException` so existing
preflight failures retain their original exception types.

- [ ] **Step 2: Advance the active development version in the same RED commit**

Set:

```xml
<IcodTermInfoSuiteVersion>1.16.0-Alpha-2</IcodTermInfoSuiteVersion>
```

Update only the active assertion in `Hdb01ContractTests` from
`1.16.0-Alpha-1` to `1.16.0-Alpha-2`. Preserve the historical 1.15 marker,
the HW01 Alpha-1 acceptance record in `Hdb09ReleaseClosureTests`, and accepted
stable-version assertions.

- [ ] **Step 3: Run the focused suite and verify intentional RED**

Run:

```text
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release -f net10.0 --filter "FullyQualifiedName~Hw02WriterImageTests|FullyQualifiedName~Hdb01ContractTests"
```

Expected: the project compiles and only the new storage-key assertion fails
because `PreparedPublication` has no `StorageKey` property. Version assertions
pass.

- [ ] **Step 4: Commit and publish RED**

```bash
git add Directory.Build.props tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb01ContractTests.cs tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw02WriterImageTests.cs
git commit -m "test: define HW02 prepared publication identity"
```

Publish through the GitHub Git Data or contents API and record the exact remote
head plus the expected failing pull-request job.

- [ ] **Step 5: Implement exact storage-key extraction**

Add `using System.Buffers.Binary;`. After successful Runtime parsing and
identity agreement, extract the already validated names section:

```csharp
private static byte[] ExtractStorageKey( byte[] data ) {
	int namesLength = BinaryPrimitives.ReadUInt16LittleEndian(
		data.AsSpan( 2, sizeof( ushort ) )
	);
	return data.AsSpan( 12, namesLength - 1 ).ToArray();
}
```

Make the existing prepared records and preparation method internal so later
production components and both friend test assemblies share the same state:

```csharp
internal static PreparedPublication[] PreparePublications(
	IReadOnlyList<BerkeleyDbTerminalDatabaseEntry> entries,
	BerkeleyDbTerminalDatabaseWriterOptions options,
	CancellationToken cancellationToken
)

internal sealed record PreparedIdentity(
	string Name,
	byte[] Utf8
);

internal sealed record PreparedPublication(
	PreparedIdentity Canonical,
	IReadOnlyList<PreparedIdentity> Aliases,
	byte[] StorageKey,
	byte[] Data
);
```

Construct `PreparedPublication` with `ExtractStorageKey(data)` after
`RequireIdentityAgreement`. Do not parse delimiters or reconstruct description
text.

- [ ] **Step 6: Run focused and full BerkeleyDb unit tests**

Run the focused command from Step 3, then:

```text
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release -f net10.0
```

Expected: PASS, including every existing HW01 preflight test and the new exact
storage-key test.

- [ ] **Step 7: Commit and publish GREEN**

```bash
git add Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalDatabaseWriter.Preflight.cs tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw02WriterImageTests.cs
git commit -m "feat: preserve HW02 compiled storage keys"
```

Publish and require the normal exact-head pull-request matrix to pass before
accepting Task 1.

---

### Task 2: Deterministic Ncurses Record Planning

**Files:**
- Modify: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw02WriterImageTests.cs`
- Create: `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbNcursesRecordPlanner.cs`
- Create: `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashV9WriterKeyComparer.cs`

**Interfaces:**
- Consumes: `IReadOnlyList<BerkeleyDbTerminalDatabaseWriter.PreparedPublication>` from Task 1.
- Produces: `BerkeleyDbNcursesRecordPlanner.CreateRecords(IReadOnlyList<PreparedPublication>, CancellationToken)` returning an ordered `IReadOnlyList<BerkeleyDbHashRecord>` and `BerkeleyDbHashV9WriterKeyComparer.Instance`.

- [ ] **Step 1: Add reflection-first RED envelope and ordering tests**

Extend `Hw02WriterImageTests` so it locates
`Icod.TermInfo.BerkeleyDb.BerkeleyDbNcursesRecordPlanner` and its static
`CreateRecords` method at runtime. Prepare one entry with canonical
`hw02-primary`, alias `hw02-alias`, and description `HW02 record planner`.
Require exactly these three key/value pairs, regardless of returned order:

```text
key = UTF8("hw02-primary")
value = 0x02 + storageKey

key = UTF8("hw02-alias")
value = 0x02 + storageKey

key = storageKey
value = 0x00 + complete compiled bytes
```

Add a separate reflection-first comparer test with raw keys `a`, `aa`, and `b`;
require `aa`, `a`, `b`. Add a planner duplicate-byte-key test and a
pre-cancelled-token test.

- [ ] **Step 2: Run targeted tests and verify intentional RED**

Run:

```text
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release -f net10.0 --filter "FullyQualifiedName~Hw02WriterImageTests"
```

Expected: the Task 1 tests pass and planner tests fail because the planner type
does not exist.

- [ ] **Step 3: Commit and publish RED**

```bash
git add tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw02WriterImageTests.cs
git commit -m "test: define HW02 ncurses record planning"
```

- [ ] **Step 4: Implement writer-only key ordering**

Create an internal sealed comparer implementing `IComparer<byte[]>` and an
internal span overload. Compare common bytes ascending, then return
`right.Length.CompareTo(left.Length)` so an exact longer key precedes its
prefix. Do not alter `ByteArrayComparer`.

```csharp
internal sealed class BerkeleyDbHashV9WriterKeyComparer : IComparer<byte[]> {
	internal static BerkeleyDbHashV9WriterKeyComparer Instance { get; } = new();

	private BerkeleyDbHashV9WriterKeyComparer() {
	}

	public int Compare( byte[]? left, byte[]? right ) {
		// Preserve normal null/reference handling, compare shared bytes, then:
		return right.Length.CompareTo( left.Length );
	}
}
```

- [ ] **Step 5: Implement ncurses record derivation**

Create an internal static planner with the exact signature:

```csharp
internal static IReadOnlyList<BerkeleyDbHashRecord> CreateRecords(
	IReadOnlyList<BerkeleyDbTerminalDatabaseWriter.PreparedPublication> publications,
	CancellationToken cancellationToken
)
```

For canonical and every alias, add a record whose key is its prepared UTF-8
bytes and whose value is `PrependMarker(storageKey, 2)`. Add one data record
whose key is the storage key and whose value is `PrependMarker(data, 0)`.
Use a `HashSet<byte[]>(ByteArrayComparer.Instance)` to reject exact duplicate
keys with `InvalidOperationException`. Sort the finished array with
`BerkeleyDbHashV9WriterKeyComparer.Instance`, check cancellation per
publication and during record emission, and return `Array.AsReadOnly(records)`.

- [ ] **Step 6: Replace reflection assertions with direct internal calls**

Now that Task 2 types exist, simplify the tests to call internal types through
the existing friend assembly. Keep all exact envelope, prefix-order, duplicate,
and cancellation assertions.

- [ ] **Step 7: Run targeted and full BerkeleyDb tests**

Run the targeted command from Step 2 followed by the complete net10.0
BerkeleyDb test project. Expected: PASS.

- [ ] **Step 8: Commit and publish GREEN**

```bash
git add Icod.TermInfo.BerkeleyDb/src/BerkeleyDbNcursesRecordPlanner.cs Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashV9WriterKeyComparer.cs tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw02WriterImageTests.cs
git commit -m "feat: plan deterministic ncurses hash records"
```

Require exact-head pull-request CI before accepting Task 2.

---

### Task 3: Checked Metadata, Hashing, and Inline Bucket Serialization

**Files:**
- Modify: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw02WriterImageTests.cs`
- Create: `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashV9ImageBuilder.cs`

**Interfaces:**
- Consumes: canonically ordered `IReadOnlyList<BerkeleyDbHashRecord>`, positive maximum database size, and cancellation token.
- Produces: `BerkeleyDbHashV9ImageBuilder.Build(IReadOnlyList<BerkeleyDbHashRecord>, int, CancellationToken)` returning the exact 12,288-byte image and `BerkeleyDbHashV9ImageBuilder.Hash(ReadOnlySpan<byte>)` for internal verification.

- [ ] **Step 1: Add reflection-first RED layout tests**

Require an internal static `BerkeleyDbHashV9ImageBuilder.Build` method. Supply
one planned publication and assert the complete output shape:

```csharp
Assert.Equal( 3 * 4096, image.Length );
Assert.Equal( 0x00061561U, ReadUInt32( image, 12 ) );
Assert.Equal( 9U, ReadUInt32( image, 16 ) );
Assert.Equal( 4096U, ReadUInt32( image, 20 ) );
Assert.Equal( (byte)8, image[25] );
Assert.Equal( 2U, ReadUInt32( image, 32 ) );
Assert.Equal( 3U, ReadUInt32( image, 88 ) );
Assert.Equal( 0x5E688DD1U, ReadUInt32( image, 92 ) );
Assert.Equal( (byte)13, image[4096 + 25] );
Assert.Equal( (byte)13, image[( 2 * 4096 ) + 25] );
```

Also assert metadata offsets 4, 72, 76, 80, 84, 96, and 100; physical bucket
page numbers; even item counts; page high-free offsets; every item marker `1`;
and key/value payload recovery through the offset tables.

- [ ] **Step 2: Verify RED and commit**

Run only `Hw02WriterImageTests`. Expected: existing storage/planner tests pass;
layout tests fail because the image builder is absent.

```bash
git add tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw02WriterImageTests.cs
git commit -m "test: define HW02 inline Hash-v9 image layout"
```

- [ ] **Step 3: Implement constants and the qualified hash**

Create `BerkeleyDbHashV9ImageBuilder` with private constants:

```csharp
private const uint HashMagic = 0x00061561;
private const uint HashVersion = 9;
private const byte MetadataPage = 8;
private const byte HashPage = 13;
private const byte InlineItem = 1;
private const int PageHeaderSize = 26;
private const int BucketCount = 2;
private const int BigItemThreshold = PageSize / 4;
internal const int PageSize = 4096;
```

Implement unchecked Hash-v9 arithmetic exactly:

```csharp
internal static uint Hash( ReadOnlySpan<byte> key ) {
	uint result = 0;
	foreach ( byte value in key ) {
		result = unchecked( result * 16777619 );
		result ^= value;
	}
	return result;
}
```

- [ ] **Step 4: Implement checked build orchestration**

Use this exact internal entry point:

```csharp
internal static byte[] Build(
	IReadOnlyList<BerkeleyDbHashRecord> records,
	int maximumDatabaseSize,
	CancellationToken cancellationToken
)
```

Reject null/empty records, nonpositive maximum size, and a maximum below
`3 * PageSize`. Validate that each key and value length is at most 1,024 before
allocating. Place each record into bucket `Hash(record.Key.Span) & 1`, retaining
the input order. Allocate exactly `3 * PageSize`, write metadata, then pages one
and two. Use checked arithmetic for every size, count, offset, and conversion.

- [ ] **Step 5: Implement deterministic metadata and file ID**

Use `IncrementalHash.CreateHash(HashAlgorithmName.SHA256)`. Append each ordered
record's key length as a little-endian signed 32-bit integer, key bytes, value
length in the same form, and value bytes. Copy only the first 20 digest bytes to
metadata offsets 52 through 71.

Write the exact metadata fields from specification section 8.1, including the
not-logged LSN marker and character hash. Do not write timestamps, paths,
randomness, process state, or unused bytes.

- [ ] **Step 6: Implement inline page packing**

For each bucket, create key then value items as `0x01 + payload`. Write the
item count at offset 20. Starting at offset 4096, subtract each item length,
reject when the new offset is below `26 + itemCount * 2`, copy the item, and
write its `ushort` offset into the table beginning at byte 26. Write the final
offset at byte 22. Empty pages retain offset 4096, item count zero, page type
13, and their physical page number.

- [ ] **Step 7: Replace reflection use and run GREEN verification**

Call `Build` directly from tests. Run the targeted suite, then the complete
BerkeleyDb net10.0 project. Expected: PASS.

- [ ] **Step 8: Commit and publish GREEN**

```bash
git add Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashV9ImageBuilder.cs tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw02WriterImageTests.cs
git commit -m "feat: build deterministic inline Hash-v9 images"
```

Require exact-head pull-request CI before accepting Task 3.

---

### Task 4: Determinism, Boundary, and Managed Reader Proofs

**Files:**
- Modify: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw02WriterImageTests.cs`

**Interfaces:**
- Consumes: Task 1 preparation, Task 2 planner, Task 3 builder, `BerkeleyDbHashReader.ReadRecords`, and `NcursesCatalogReader.Read`.
- Produces: the complete managed HW02 gate for deterministic equivalent input, reader round trips, and explicit HW03 rejection boundaries.

- [ ] **Step 1: Add RED determinism and round-trip tests**

Build two or more publications in forward and reverse order and assert complete
byte-array equality. Repeat under `en-US` and `tr-TR`, restoring both
`CurrentCulture` and `CurrentUICulture` in `finally`. Build twice under the same
culture and assert equality.

Pass the produced bytes to:

```csharp
IReadOnlyList<BerkeleyDbHashRecord> records =
	BerkeleyDbHashReader.ReadRecords(
		image,
		maximumItemSize: 1025,
		maximumRecordCount: 64,
		CancellationToken.None
	);
IReadOnlyList<BerkeleyDbTerminalCatalogEntry> catalog =
	NcursesCatalogReader.Read(
		records,
		new CompiledTermInfoParserOptions(),
		maximumIndexHops: 16,
		CancellationToken.None
	);
```

Require each canonical and alias publication, and locate each marker-0 record
to prove its bytes after the marker exactly equal the original compiled payload.

- [ ] **Step 2: Add RED bound tests**

Call the image builder directly with controlled `BerkeleyDbHashRecord` values:

- key and value payloads of exactly 1,024 bytes succeed;
- either payload at 1,025 bytes throws `InvalidOperationException`;
- maximum database size 12,287 throws and 12,288 succeeds;
- a pre-cancelled token throws `OperationCanceledException`;
- two same-bucket records whose four payload lengths total 4,058 bytes fit
  exactly because `26 + 8 table bytes + 4 inline markers + 4,058 = 4,096`;
- increasing the final payload by one byte throws the bucket-capacity
  `InvalidOperationException`.

Use two distinct 1,024-byte even-parity keys so `Hash(key) & 1` selects the same
bucket. Use payload lengths `1024, 1024, 1024, 986` for the exact-fit case and
`987` for the failure case.

- [ ] **Step 3: Run tests and confirm any missing boundary behavior is RED**

Run only `Hw02WriterImageTests`. If Task 3 already satisfies a test, retain it
as characterization; at least the integrated permutation/culture/round-trip
test must demonstrate the complete path before the GREEN commit.

- [ ] **Step 4: Make the smallest production corrections**

Correct only violations exposed by Step 3. Keep all corrections inside the
three HW02 production components; do not connect `Write`, add overflow pages,
or change reader behavior.

- [ ] **Step 5: Run all three target frameworks**

```text
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release -f net8.0
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release -f net9.0
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release -f net10.0
```

Expected: PASS with identical public API and deterministic image assertions.

- [ ] **Step 6: Commit and publish the managed proof**

```bash
git add Icod.TermInfo.BerkeleyDb/src tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw02WriterImageTests.cs
git commit -m "test: harden HW02 deterministic image boundaries"
```

Require exact-head pull-request CI before accepting Task 4.

---

### Task 5: Independent HW00 Byte Oracle and HW02 Acceptance

**Files:**
- Create: `tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/src/Hw02ProductionWriterProofTests.cs`
- Create after GREEN: `docs/1.16.0-HW02-DETERMINISTIC-HASH-V9-INLINE-IMAGE.md`
- Modify after GREEN: `Icod.TermInfo-1.16.0-Berkeley-DB-Hash-V9-Writer-Roadmap.md`
- Modify after GREEN: `Icod.TermInfo-Post-1.0-Development-Roadmap.md`

**Interfaces:**
- Consumes: production prepare/plan/build path and research-only `Hw00HashV9Writer.WriteNcursesCatalog`.
- Produces: complete byte equality for single-bucket and multi-bucket stores, exact-head CI evidence, and the accepted HW02 checkpoint.

- [ ] **Step 1: Add the failing production-oracle equality test**

Create helpers:

```csharp
private static byte[] WriteProduction(
	params BerkeleyDbTerminalDatabaseEntry[] entries
) {
	var options = new BerkeleyDbTerminalDatabaseWriterOptions();
	BerkeleyDbTerminalDatabaseWriter.PreparedPublication[] prepared =
		BerkeleyDbTerminalDatabaseWriter.PreparePublications(
			entries,
			options,
			CancellationToken.None
		);
	IReadOnlyList<BerkeleyDbHashRecord> records =
		BerkeleyDbNcursesRecordPlanner.CreateRecords(
			prepared,
			CancellationToken.None
		);
	return BerkeleyDbHashV9ImageBuilder.Build(
		records,
		options.MaximumDatabaseSize,
		CancellationToken.None
	);
}
```

`WriteOracle` writes the same compiled byte arrays through
`Hw00HashV9Writer.WriteNcursesCatalog` into a `MemoryStream`.

Use `hw00-primary-0` with alias `hw00-alias-0` for a compact fixture and
`hw00-bucket-00` through `hw00-bucket-07` for a both-bucket fixture. Assert
`Assert.Equal(oracle, production)` over the complete byte arrays, not selected
fields or digests. Assert both bucket item counts are nonzero in the second
case.

- [ ] **Step 2: Run the interop test and preserve RED if bytes differ**

```text
dotnet test tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/Icod.TermInfo.BerkeleyDb.Interop.Tests.csproj -c Release -f net10.0 --filter "FullyQualifiedName~Hw02ProductionWriterProofTests"
```

Expected before any needed correction: either PASS immediately because Tasks
1-4 exactly match HW00, or FAIL with the first differing offset. A compile
failure is not accepted RED.

- [ ] **Step 3: Correct production bytes, never the accepted oracle**

If RED, identify the first differing page and field, compare it with the HW00
accepted profile, and change only production HW02 code. Do not weaken equality,
change the research oracle, or copy an opaque fixture into production.

- [ ] **Step 4: Run targeted, full interop, and full unit verification**

Run the targeted command, the complete Interop project on net10.0, and the
complete BerkeleyDb unit project on net10.0. Expected: PASS.

- [ ] **Step 5: Commit and publish the byte-oracle proof**

```bash
git add Icod.TermInfo.BerkeleyDb/src tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/src/Hw02ProductionWriterProofTests.cs
git commit -m "test: prove HW02 byte equality with HW00"
```

Publish the exact head. Require the normal pull-request workflow across all 12
jobs. HDB00/native overflow CI is not an HW02 acceptance requirement because
HW00 already qualified this exact inline profile and HW03 owns overflow.

- [ ] **Step 6: Verify unchanged API and dependency boundaries**

Confirm package verification still reports the accepted 12-type 1.16
BerkeleyDb API baseline, assembly version `1.0.0.0`, and only the Runtime project
dependency. Confirm `BerkeleyDbTerminalDatabaseWriter.Write` still performs no
destination mutation and throws the exact HW01 construction-boundary message.

- [ ] **Step 7: Record HW02 acceptance after exact-head GREEN**

Create the HW02 acceptance record with:

- accepted code head;
- pull-request workflow run ID, URL, and 12/12 result;
- net8.0/net9.0/net10.0 test counts;
- complete HW00 byte-oracle equality evidence;
- unchanged public API/dependency evidence;
- explicit remaining HW03 growth/overflow boundary; and
- next gate `HW03 / Alpha-3`.

Update the 1.16 roadmap to `HW02 COMPLETE / HW03 NEXT`, mark HW02
`COMPLETE / ACCEPTED`, and add exact-head evidence. Update the main roadmap's
1.16 status consistently. Do not change the historical stable 1.15 release
record.

- [ ] **Step 8: Commit and publish documentation without waiting for CI**

```bash
git add docs/1.16.0-HW02-DETERMINISTIC-HASH-V9-INLINE-IMAGE.md Icod.TermInfo-1.16.0-Berkeley-DB-Hash-V9-Writer-Roadmap.md Icod.TermInfo-Post-1.0-Development-Roadmap.md
git commit -m "docs: accept HW02 deterministic inline images"
```

Publish the documentation-only commit and proceed directly to HW03 design work
without waiting on a documentation-only workflow, following the user's standing
instruction.

---

## Execution Order and Checkpoints

Execute Tasks 1 through 5 in order. Each RED and GREEN commit is independently
published and reviewed through exact-head evidence. Do not combine Tasks 2 and
3: record-envelope mistakes and page-layout mistakes must remain independently
diagnosable. Do not accept HW02 until the production builder matches complete
HW00 output bytes and the normal pull-request matrix is green at the exact code
head.
