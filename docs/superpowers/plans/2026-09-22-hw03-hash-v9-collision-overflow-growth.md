# HW03 Hash-v9 Collision, Overflow, and Bounded Growth Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use
> superpowers:executing-plans to implement this plan task-by-task. Steps use
> checkbox (`- [ ]`) syntax for tracking. Execute inline; do not delegate tasks
> or reviews to subagents.

**Goal:** Extend the internal Hash-v9 image builder with deterministic off-page
payloads, bounded power-of-two bucket growth, type-13 continuation chains, and
cross-host interoperability evidence while preserving every accepted HW02 byte.

**Architecture:** A new internal layout planner converts canonical Hash records
into a fully numbered immutable page plan before allocation. The existing image
builder becomes a deterministic emitter for that plan. Tests advance through
three compiling RED/GREEN gates: overflow storage, bucket/continuation layout,
and native qualification.

**Tech Stack:** C# 13; .NET 8.0, 9.0, and 10.0; xUnit; GitHub Actions;
PowerShell 5.1-compatible PowerShell; cmd/sh; existing Berkeley DB 5 tooling on
Linux and macOS.

**Spec:**
`docs/superpowers/specs/2026-09-22-hw03-hash-v9-collision-overflow-growth-design.md`

## Global Constraints

- Work on `1.16.0-berkeley-db-hash-writer` and keep PR #47 as the integration
  surface.
- Execute inline with `superpowers:executing-plans`; do not use subagents.
- Use strict compiling RED, publish that exact head, observe the expected CI
  failure, then implement the smallest GREEN.
- Advance `<IcodTermInfoSuiteVersion>` and therefore `<Version>` and
  `<PackageVersion>` to exactly `1.16.0-Alpha-3` with the first RED commit.
- Keep every reusable `<AssemblyVersion>` exactly `1.0.0.0`.
- Target `net8.0`, `net9.0`, and `net10.0`; keep C# at `13.0`.
- Keep `Icod.TermInfo.BerkeleyDb` dependent only on Runtime. Add no Compiler,
  Inspection, native, or third-party production dependency.
- Keep the package pure managed, IL-only, and free of native assets or P/Invoke.
- Add no public API. `BerkeleyDbTerminalDatabaseWriter.Write` must retain the
  exact HW01/HW02 `NotSupportedException` until HW04.
- Preserve byte-for-byte every accepted HW02 two-bucket inline image.
- Preserve the accepted HW00 two-bucket overflow image when no bucket growth or
  continuation page is required.
- Use only C#, PowerShell 5.1-compatible PowerShell, cmd/sh, XML/MSBuild, YAML,
  and Markdown changes. Add no Python, C, or C++ source.
- Reuse the existing native C lookup probe unchanged as an external oracle.
- Native Berkeley DB qualification runs on Linux and macOS. Windows remains
  managed-only and reads the transported Linux artifacts.
- Local `dotnet` is unavailable in the current workspace. GitHub Actions is the
  executable RED/GREEN authority; never claim a test result without its exact
  workflow head.
- Documentation-only commits are published without waiting for their jobs.

## Review Focus

The following five easy-to-miss cases must have explicit permanent tests:

1. **A maximum size between page boundaries:**
   `ExactPageLimitSucceedsAndOneByteBelowFailsBeforeAllocation` in Task 3 must
   prove that the limit is inclusive and no partial page is emitted.
2. **An overflow payload that is an exact multiple of 4,070 bytes:**
   `ExactOverflowMultipleDoesNotAllocateAnEmptyTailPage` in Task 1 must prove
   that 8,140 bytes uses exactly two full pages.
3. **Exact-hash keys that differ and sit near the inline limit:**
   `ExactHashCollisionUsesLinkedContinuationPage` in Task 3 must recover both
   1,016/1,017-byte keys and their values without merging or rejecting them.
4. **Cancellation during a repeated sizing pass:**
   `CancellationDuringCandidateEvaluationEscapesWithoutAnImage` in Task 3 must
   cancel from an instrumented read-only list and report
   `OperationCanceledException`.
5. **A larger limit after the natural layout already fits:**
   `LargerCeilingDoesNotChangeNaturalImage` in Task 3 must compare complete
   arrays, proving the ceiling does not cause gratuitous bucket growth.

---

## File Structure

### Production files

- Create `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashV9LayoutPlan.cs` — immutable
  record, item, Hash-page, overflow-page, and whole-image plan types.
- Create `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashV9LayoutPlanner.cs` — item
  classification, candidate evaluation, bounded sizing, page packing, page
  numbering, and cancellation checks.
- Modify `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashV9ImageBuilder.cs` — retain
  hashing and deterministic file-ID behavior; emit metadata and pages from the
  completed layout plan.
- Modify `Icod.TermInfo.BerkeleyDb/src/Properties/AssemblyInfo.cs` only in the
  native-qualification task to grant the repository-owned C# probe internal
  access.
- Modify `Directory.Build.props` — coordinated `1.16.0-Alpha-3` version.

### Managed test files

- Create
  `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw03WriterTestSupport.cs` — test-only
  record construction, exact collision keys, page slicing, field reads, and an
  instrumented cancelling list.
- Create
  `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw03WriterOverflowTests.cs` — inline
  boundary, type-3 descriptors, type-7 chains, key/value overflow, and byte
  preservation.
- Create
  `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw03WriterGrowthTests.cs` — natural
  growth, continuation chains, capped fallback, metadata, bounds,
  determinism, cancellation, and managed-reader round trips.
- Modify `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb01ContractTests.cs` — exact
  prerelease version assertion.
- Create
  `tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/src/Hw03ProductionWriterProofTests.cs`
  — complete production/HW00 overflow byte comparison.
- Modify
  `tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/src/NativeOracleTests.cs` —
  managed reading of native-verified HW03 output and transported Linux images.

### Native-qualification tooling and automation

- Create `tools/hdb00/hw03-writer-probe/Hw03.ManagedWriterProbe.csproj` —
  repository-only net10.0 C# executable referencing BerkeleyDb.
- Create `tools/hdb00/hw03-writer-probe/Program.cs` — emit deterministic raw
  overflow, growth, and exact-collision databases plus expected key/value
  files.
- Modify `.github/workflows/hdb00-interoperability.yml` — build the production
  fixtures, run native verification/dump/lookup on Linux and macOS, and include
  Linux artifacts for Windows managed-only validation.

### Acceptance documentation

- Create `docs/1.16.0-HW03-COLLISION-OVERFLOW-BOUNDED-GROWTH.md` — accepted
  exact code head, workflow runs, storage profile, bounds, and next gate.
- Modify `Icod.TermInfo-1.16.0-Berkeley-DB-Hash-V9-Writer-Roadmap.md` — mark HW03
  complete/accepted and HW04 next.
- Modify `Icod.TermInfo-Post-1.0-Development-Roadmap.md` — advance the current
  1.16.0 checkpoint without expanding release scope.

---

### Task 1: Define the compiling RED for overflow storage

**Files:**

- Create:
  `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw03WriterTestSupport.cs`
- Create:
  `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw03WriterOverflowTests.cs`
- Create:
  `tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/src/Hw03ProductionWriterProofTests.cs`
- Modify: `Directory.Build.props`
- Modify: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb01ContractTests.cs`

**Interfaces:**

- Consumes:
  `BerkeleyDbHashV9ImageBuilder.Build(IReadOnlyList<BerkeleyDbHashRecord>, int,
  CancellationToken)` and the existing HW00 oracle.
- Produces: a compiling test contract for type-3/type-7 storage and reusable
  HW03 fixture helpers; no production behavior.

- [ ] **Step 1: Advance the coordinated prerelease version**

Change the central value and its exact contract assertion:

```xml
<IcodTermInfoSuiteVersion>1.16.0-Alpha-3</IcodTermInfoSuiteVersion>
```

```csharp
Assert.Contains(
	"<IcodTermInfoSuiteVersion>1.16.0-Alpha-3</IcodTermInfoSuiteVersion>",
	props,
	StringComparison.Ordinal
);
```

- [ ] **Step 2: Add deterministic HW03 test support**

Define the qualified exact-collision keys and common helpers:

```csharp
using System.Buffers.Binary;
using System.Text;
using Xunit;

internal static class Hw03WriterTestSupport {
	internal const int PageSize = 4096;
	internal const int HeaderSize = 26;
	internal readonly record struct ItemLocation(
		int PageNumber,
		int Offset
	);

	internal static byte[] FirstExactCollisionKey() =>
		Encoding.ASCII.GetBytes(
			"hw03-0c5ny4k-da6" + new string( 'x', 1000 )
		)
	;

	internal static byte[] SecondExactCollisionKey() =>
		Encoding.ASCII.GetBytes(
			"hw03-0fpxptj-1j8p" + new string( 'x', 1000 )
		)
	;

	internal static BerkeleyDbHashRecord Record(
		string key,
		int valueLength,
		byte fill
	) => new(
		Encoding.ASCII.GetBytes( key ),
		Enumerable.Repeat( fill, valueLength ).ToArray()
	);

	internal static ReadOnlySpan<byte> Page( byte[] image, int pageNumber ) =>
		image.AsSpan( checked( pageNumber * PageSize ), PageSize )
	;

	internal static uint ReadUInt32( byte[] image, int offset ) =>
		BinaryPrimitives.ReadUInt32LittleEndian(
			image.AsSpan( offset, sizeof( uint ) )
		)
	;

	internal static uint ReadPageNumber( byte[] image, int pageNumber ) =>
		ReadUInt32( image, checked( ( pageNumber * PageSize ) + 8 ) )
	;

	internal static uint ReadPagePrevious( byte[] image, int pageNumber ) =>
		ReadUInt32( image, checked( ( pageNumber * PageSize ) + 12 ) )
	;

	internal static uint ReadPageNext( byte[] image, int pageNumber ) =>
		ReadUInt32( image, checked( ( pageNumber * PageSize ) + 16 ) )
	;

	internal static BerkeleyDbHashRecord[] Sort(
		params BerkeleyDbHashRecord[] records
	) {
		Array.Sort(
			records,
			static ( left, right ) =>
				BerkeleyDbHashV9WriterKeyComparer.Instance.Compare(
					left.Key.Span,
					right.Key.Span
				)
		);
		return records;
	}

	internal static byte[] Build(
		IReadOnlyList<BerkeleyDbHashRecord> records,
		int maximumDatabaseSize = 64 * 1024 * 1024,
		CancellationToken cancellationToken = default
	) => BerkeleyDbHashV9ImageBuilder.Build(
		records,
		maximumDatabaseSize,
		cancellationToken
	);

	internal static byte[] BuildSingleRecord( int valueLength ) =>
		Build( Sort( Record( "hw03-overflow", valueLength, 0x5A ) ) )
	;

	internal static ItemLocation LocateOnlyValue( byte[] image ) {
		for ( int pageNumber = 1; pageNumber <= 2; pageNumber++ ) {
			ReadOnlySpan<byte> page = Page( image, pageNumber );
			ushort itemCount = BinaryPrimitives.ReadUInt16LittleEndian(
				page[20..22]
			);
			if ( itemCount == 0 ) {
				continue;
			}
			Assert.Equal( 2, itemCount );
			ushort valueOffset = BinaryPrimitives.ReadUInt16LittleEndian(
				page[28..30]
			);
			return new ItemLocation( pageNumber, valueOffset );
		}
		throw new Xunit.Sdk.XunitException( "The only record was not found." );
	}

	internal static void AssertOverflowPage(
		byte[] image,
		int pageNumber,
		int previous,
		int next,
		int chunkLength
	) {
		ReadOnlySpan<byte> page = Page( image, pageNumber );
		Assert.Equal( (uint)pageNumber, ReadPageNumber( image, pageNumber ) );
		Assert.Equal( (uint)previous, ReadPagePrevious( image, pageNumber ) );
		Assert.Equal( (uint)next, ReadPageNext( image, pageNumber ) );
		Assert.Equal( (ushort)1, BinaryPrimitives.ReadUInt16LittleEndian( page[20..22] ) );
		Assert.Equal( (ushort)chunkLength, BinaryPrimitives.ReadUInt16LittleEndian( page[22..24] ) );
		Assert.Equal( (byte)7, page[25] );
	}

	internal static BerkeleyDbHashRecord[] GrowthRecords() => Sort(
		Record( "hw03-growth-003", 1024, 0x03 ),
		Record( "hw03-growth-007", 1024, 0x07 ),
		Record( "hw03-growth-001", 1024, 0x01 ),
		Record( "hw03-growth-005", 1024, 0x05 )
	);

	internal static byte[] BuildGrowthRecords( int maximumDatabaseSize ) =>
		Build( GrowthRecords(), maximumDatabaseSize )
	;

	internal static BerkeleyDbHashRecord[] ExactCollisionRecords() => Sort(
		new BerkeleyDbHashRecord(
			FirstExactCollisionKey(),
			Enumerable.Repeat( (byte)0x31, 1024 ).ToArray()
		),
		new BerkeleyDbHashRecord(
			SecondExactCollisionKey(),
			Enumerable.Repeat( (byte)0x32, 1024 ).ToArray()
		)
	);
}

internal sealed class CancelAfterReadsList : IReadOnlyList<BerkeleyDbHashRecord> {
	private readonly IReadOnlyList<BerkeleyDbHashRecord> _records;
	private readonly CancellationTokenSource _source;
	private readonly int _cancelOnRead;
	private int _readCount;

	internal CancelAfterReadsList(
		IReadOnlyList<BerkeleyDbHashRecord> records,
		CancellationTokenSource source,
		int cancelOnRead
	) {
		_records = records;
		_source = source;
		_cancelOnRead = cancelOnRead;
	}

	public int Count => _records.Count;
	public BerkeleyDbHashRecord this[int index] {
		get {
			if ( ++_readCount == _cancelOnRead ) {
				_source.Cancel();
			}
			return _records[index];
		}
	}

	public IEnumerator<BerkeleyDbHashRecord> GetEnumerator() {
		for ( int index = 0; index < Count; index++ ) {
			yield return this[index];
		}
	}

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
		GetEnumerator()
	;
}
```

Each HW03 test file imports the helpers with:

```csharp
using static Icod.TermInfo.BerkeleyDb.Tests.Hw03WriterTestSupport;
```

Add an instrumented `IReadOnlyList<BerkeleyDbHashRecord>` whose indexer cancels
the supplied `CancellationTokenSource` after a constructor-selected read count;
Task 3 uses it to prove cancellation inside candidate evaluation.

- [ ] **Step 3: Add failing overflow geometry tests**

The tests must inspect the complete descriptor and chain, not only reader
output. Include these exact cases:

```csharp
[Theory]
[InlineData( 1024, 1, 0 )]
[InlineData( 1025, 3, 1 )]
[InlineData( 4070, 3, 1 )]
[InlineData( 4071, 3, 2 )]
[InlineData( 8140, 3, 2 )]
public void SelectsCanonicalItemShape(
	int payloadLength,
	byte expectedItemType,
	int expectedOverflowPages
) {
	byte[] image = BuildSingleRecord( payloadLength );
	ItemLocation value = LocateOnlyValue( image );
	Assert.Equal(
		expectedItemType,
		Page( image, value.PageNumber )[value.Offset]
	);
	Assert.Equal(
		expectedOverflowPages,
		checked( image.Length / 4096 ) - 3
	);
}

[Fact]
public void ExactOverflowMultipleDoesNotAllocateAnEmptyTailPage() {
	byte[] image = BuildSingleRecord( 8140 );
	Assert.Equal( 5 * 4096, image.Length );
	AssertOverflowPage( image, 3, previous: 0, next: 4, chunkLength: 4070 );
	AssertOverflowPage( image, 4, previous: 3, next: 0, chunkLength: 4070 );
}
```

Also add:

- `WritesExactTwelveByteOffPageDescriptor`;
- `WritesLargeKeyThenLargeValueAsIndependentChains`;
- `ManagedReaderReconstructsMultiPagePayloadExactly`;
- `TwoBucketInlineImageRemainsByteIdenticalToHw02ExpectedBytes`; and
- `UnusedFinalOverflowBytesRemainZero`.

- [ ] **Step 4: Add the failing complete-byte HW00 overflow proof**

Create a 3,000-byte compiled entry and compare complete arrays:

```csharp
[Fact]
public void TwoBucketOverflowProductionImageExactlyMatchesHw00Oracle() {
	byte[] compiled = CreateCompiledEntry(
		"hw03-overflow",
		"HW03 overflow byte proof"
	);
	Array.Resize( ref compiled, 3000 );

	Assert.Equal(
		WriteOracle( compiled ),
		WriteProduction(
			new BerkeleyDbTerminalDatabaseEntry(
				"hw03-overflow",
				Array.Empty<string>(),
				compiled
			)
		)
	);
}
```

Copy the small `CreateCompiledEntry`, `WriteOracle`, and `WriteProduction`
helpers from `Hw02ProductionWriterProofTests`; do not call another test class.

- [ ] **Step 5: Verify and publish the RED**

Run when .NET is available:

```bash
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release -f net10.0 --filter FullyQualifiedName~Hw03WriterOverflowTests
dotnet test tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/Icod.TermInfo.BerkeleyDb.Interop.Tests.csproj -c Release -f net10.0 --filter FullyQualifiedName~Hw03ProductionWriterProofTests
```

In the current workspace, publish the exact RED head and inspect GitHub Actions.
Expected failures are the existing
`HW02 supports at most 1024-byte inline payloads` rejection. Existing HW02 tests
must still compile.

- [ ] **Step 6: Commit the RED**

```bash
git add Directory.Build.props \
  tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb01ContractTests.cs \
  tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw03WriterTestSupport.cs \
  tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw03WriterOverflowTests.cs \
  tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/src/Hw03ProductionWriterProofTests.cs
git commit -m "test: define HW03 overflow image contract"
```

---

### Task 2: Implement deterministic off-page items and overflow chains

**Files:**

- Create: `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashV9LayoutPlan.cs`
- Create: `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashV9LayoutPlanner.cs`
- Modify: `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashV9ImageBuilder.cs`
- Test:
  `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw03WriterOverflowTests.cs`
- Test:
  `tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/src/Hw03ProductionWriterProofTests.cs`

**Interfaces:**

- Consumes: canonical ordered `BerkeleyDbHashRecord` instances.
- Produces:
  `BerkeleyDbHashV9LayoutPlanner.Create(IReadOnlyList<BerkeleyDbHashRecord>,
  int, CancellationToken)` returning a completed
  `BerkeleyDbHashV9LayoutPlan`; `Build` emits its bytes.

- [ ] **Step 1: Define focused immutable plan types**

Use these exact type names and property types:

```csharp
internal readonly record struct BerkeleyDbHashV9ItemPlan(
	ReadOnlyMemory<byte> Payload,
	int FirstOverflowPageNumber,
	int OverflowPageCount
) {
	internal bool IsOffPage => OverflowPageCount != 0;
	internal int EncodedLength => IsOffPage
		? 12
		: checked( Payload.Length + 1 )
	;
}

internal sealed record BerkeleyDbHashV9RecordPlan(
	uint Hash,
	BerkeleyDbHashV9ItemPlan Key,
	BerkeleyDbHashV9ItemPlan Value
);

internal sealed record BerkeleyDbHashV9HashPagePlan(
	int PageNumber,
	int BucketNumber,
	int PreviousPageNumber,
	int NextPageNumber,
	IReadOnlyList<BerkeleyDbHashV9RecordPlan> Records
);

internal sealed record BerkeleyDbHashV9OverflowPagePlan(
	int PageNumber,
	int PreviousPageNumber,
	int NextPageNumber,
	ReadOnlyMemory<byte> Payload
);

internal sealed class BerkeleyDbHashV9LayoutPlan {
	internal required int BucketCount { get; init; }
	internal required int PageCount { get; init; }
	internal required int ImageSize { get; init; }
	internal required IReadOnlyList<BerkeleyDbHashV9HashPagePlan> HashPages { get; init; }
	internal required IReadOnlyList<BerkeleyDbHashV9OverflowPagePlan> OverflowPages { get; init; }
}
```

Keep constructors/internal properties internal. Clone collections into arrays or
read-only wrappers before returning the plan.

- [ ] **Step 2: Implement item classification and overflow counts**

In the planner, freeze these constants and calculation:

```csharp
private const int PageSize = 4096;
private const int PageHeaderSize = 26;
private const int BigItemThreshold = PageSize / 4;
private const int OverflowPayloadSize = PageSize - PageHeaderSize;

private static int GetOverflowPageCount( int payloadLength ) =>
	( payloadLength <= BigItemThreshold )
		? 0
		: checked( 1 + ( ( payloadLength - 1 ) / OverflowPayloadSize ) )
;
```

For this GREEN, retain two primary buckets and one Hash page per bucket. Count
all overflow pages first, require `3 + overflowPageCount` pages to fit the
inclusive limit, then assign overflow pages from page 3 in bucket, record, key,
value order.

- [ ] **Step 3: Refactor `Build` into plan then emit**

The entry point becomes:

```csharp
BerkeleyDbHashV9LayoutPlan plan =
	BerkeleyDbHashV9LayoutPlanner.Create(
		records,
		maximumDatabaseSize,
		cancellationToken
	);
byte[] image = new byte[plan.ImageSize];
WriteMetadata( image, records, plan );
foreach ( BerkeleyDbHashV9HashPagePlan page in plan.HashPages ) {
	WriteHashPage( image, page );
}
foreach ( BerkeleyDbHashV9OverflowPagePlan page in plan.OverflowPages ) {
	WriteOverflowPage( image, page );
}
return image;
```

Remove the HW02-only `ValidateInlinePayload` rejection. Do not change `Hash` or
`CreateFileId`.

- [ ] **Step 4: Emit exact type-3 and type-7 structures**

Add emission branches:

```csharp
if ( !item.IsOffPage ) {
	page[offset] = 1;
	item.Payload.Span.CopyTo( page[( offset + 1 )..] );
} else {
	page[offset] = 3;
	WriteUInt32( page, offset + 4, checked( (uint)item.FirstOverflowPageNumber ) );
	WriteUInt32( page, offset + 8, checked( (uint)item.Payload.Length ) );
}
```

```csharp
private static void WriteOverflowPage(
	byte[] image,
	BerkeleyDbHashV9OverflowPagePlan source
) {
	Span<byte> page = image.AsSpan( source.PageNumber * PageSize, PageSize );
	WriteNotLoggedLsn( page );
	WriteUInt32( page, 8, checked( (uint)source.PageNumber ) );
	WriteUInt32( page, 12, checked( (uint)source.PreviousPageNumber ) );
	WriteUInt32( page, 16, checked( (uint)source.NextPageNumber ) );
	WriteUInt16( page, 20, 1 );
	WriteUInt16( page, 22, checked( (ushort)source.Payload.Length ) );
	page[25] = 7;
	source.Payload.Span.CopyTo( page[26..] );
}
```

Use zero for absent links. Nonfinal chunks are exactly 4,070 bytes; the final
chunk is positive.

- [ ] **Step 5: Verify the overflow GREEN and all HW02 regressions**

Run through exact-head Actions:

```bash
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release
dotnet test tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/Icod.TermInfo.BerkeleyDb.Interop.Tests.csproj -c Release
```

Expected: every new overflow test passes; every existing HW02 byte test passes;
the complete production overflow image equals the HW00 oracle.

- [ ] **Step 6: Commit the overflow GREEN**

```bash
git add Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashV9LayoutPlan.cs \
  Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashV9LayoutPlanner.cs \
  Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashV9ImageBuilder.cs
git commit -m "feat: write deterministic Hash-v9 overflow chains"
```

---

### Task 3: Define the compiling RED for growth, chains, and bounds

**Files:**

- Create:
  `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw03WriterGrowthTests.cs`
- Modify:
  `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw03WriterTestSupport.cs`

**Interfaces:**

- Consumes: the Task 2 two-bucket overflow-capable builder.
- Produces: the complete managed contract for Task 4's sizing and continuation
  implementation.

Decorate `Hw03WriterGrowthTests` with
`[Collection(Hdb07CultureCollection.Name)]` so its temporary process-culture
changes cannot run in parallel with HDB07's culture-sensitive cases.

- [ ] **Step 1: Add the four-record reducible-collision fixture**

Use these checked-in ASCII keys and assert their qualified hashes:

```csharp
private static readonly ( string Key, uint Hash )[] GrowthKeys = [
	( "hw03-growth-003", 0x2B73EB10U ),
	( "hw03-growth-007", 0x2B73EB14U ),
	( "hw03-growth-001", 0x2B73EB12U ),
	( "hw03-growth-005", 0x2B73EB16U ),
];
```

Give every record a 1,024-byte inline value. All four hashes map to bucket 0 at
two buckets; two map to bucket 0 and two to bucket 2 at four buckets.

Add a second fixed array for long capped chains. Every hash has low two bits
zero, and the third bit contains both values:

```csharp
private static readonly ( string Key, uint Hash )[] LongChainKeys = [
	( "hw03-chain-003", 0xDB6BFDF4U ),
	( "hw03-chain-007", 0xDB6BFDF0U ),
	( "hw03-chain-010", 0xDA6BFC04U ),
	( "hw03-chain-014", 0xDA6BFC00U ),
	( "hw03-chain-018", 0xDA6BFC0CU ),
	( "hw03-chain-021", 0xDD6C00DCU ),
	( "hw03-chain-025", 0xDD6C00D8U ),
	( "hw03-chain-029", 0xDD6C00D4U ),
	( "hw03-chain-032", 0xDC6BFF68U ),
	( "hw03-chain-036", 0xDC6BFF6CU ),
];
```

`LongChainRecords` gives each key a 1,024-byte inline value and canonicalizes
with the writer comparer.

```csharp
private static BerkeleyDbHashRecord[] LongChainRecords() => Sort(
	LongChainKeys.Select(
		( item, index ) => new BerkeleyDbHashRecord(
			Encoding.ASCII.GetBytes( item.Key ),
			Enumerable.Repeat( checked( (byte)( index + 1 ) ), 1024 ).ToArray()
		)
	).ToArray()
);
```

- [ ] **Step 2: Add natural-growth and metadata tests**

```csharp
[Fact]
public void ReducibleCollisionSelectsFourPrimaryBuckets() {
	byte[] image = BuildGrowthRecords( maximumDatabaseSize: 64 * 1024 * 1024 );
	Assert.Equal( 5 * 4096, image.Length );
	Assert.Equal( 3U, ReadUInt32( image, 72 ) );
	Assert.Equal( 3U, ReadUInt32( image, 76 ) );
	Assert.Equal( 1U, ReadUInt32( image, 80 ) );
	Assert.Equal( 1U, ReadUInt32( image, 96 ) );
	Assert.Equal( 1U, ReadUInt32( image, 100 ) );
	Assert.Equal( 1U, ReadUInt32( image, 104 ) );
	Assert.All( Enumerable.Range( 1, 4 ), page =>
		Assert.Equal( (uint)page, ReadPageNumber( image, page ) )
	);
}
```

Also assert bucket pages 1 and 3 contain two records each and pages 2 and 4 are
valid empty type-13 pages.

- [ ] **Step 3: Add exact-hash continuation-chain tests**

The two base keys have hash `0xC071CA1D`. After appending the same 1,000 `x`
bytes, the 1,016/1,017-byte keys both have hash `0x6DBEE87D`. Give each a
1,024-byte value so both records cannot share one Hash page.

```csharp
[Fact]
public void ExactHashCollisionUsesLinkedContinuationPage() {
	BerkeleyDbHashRecord[] records = ExactCollisionRecords();
	Assert.Equal( 0x6DBEE87DU, BerkeleyDbHashV9ImageBuilder.Hash( records[0].Key.Span ) );
	Assert.Equal( 0x6DBEE87DU, BerkeleyDbHashV9ImageBuilder.Hash( records[1].Key.Span ) );

	byte[] image = Build( records );
	int bucketPage = checked( (int)( 0x6DBEE87DU & 1U ) + 1 );
	int continuationPage = 3;
	Assert.Equal( (uint)continuationPage, ReadPageNext( image, bucketPage ) );
	Assert.Equal( (uint)bucketPage, ReadPagePrevious( image, continuationPage ) );
	Assert.Equal( 0U, ReadPageNext( image, continuationPage ) );

	foreach ( BerkeleyDbHashRecord record in records ) {
		Assert.True( BerkeleyDbHashReader.TryReadValue(
			image,
			record.Key.Span,
			out byte[] actual,
			maximumItemSize: 2048
		) );
		Assert.Equal( record.Value.ToArray(), actual );
	}
}
```

- [ ] **Step 4: Add capped fallback and limit tests**

```csharp
[Fact]
public void GrowthCapUsesTwoBucketsAndOneContinuationPage() {
	byte[] image = BuildGrowthRecords( maximumDatabaseSize: 4 * 4096 );
	Assert.Equal( 4 * 4096, image.Length );
	Assert.Equal( 1U, ReadUInt32( image, 72 ) );
	Assert.Equal( 1U, ReadUInt32( image, 76 ) );
	Assert.Equal( 0U, ReadUInt32( image, 80 ) );
	Assert.Equal( 3U, ReadPageNext( image, 1 ) );
}

[Fact]
public void ExactPageLimitSucceedsAndOneByteBelowFailsBeforeAllocation() {
	Assert.Equal( 4 * 4096, BuildGrowthRecords( 4 * 4096 ).Length );
	Assert.Throws<InvalidOperationException>(
		() => BuildGrowthRecords( ( 4 * 4096 ) - 1 )
	);
}
```

Add two physical-allocation facts:

```csharp
[Fact]
public void CappedGrowthBuildsMultipleContinuationPages() {
	byte[] image = Build( LongChainRecords(), 8 * 4096 );
	Assert.Equal( 8 * 4096, image.Length );
	Assert.Equal( 3U, ReadUInt32( image, 72 ) );
	Assert.Equal( 5U, ReadPageNext( image, 1 ) );
	Assert.Equal( 6U, ReadPageNext( image, 5 ) );
	Assert.Equal( 7U, ReadPageNext( image, 6 ) );
	Assert.Equal( 0U, ReadPageNext( image, 7 ) );
}

[Fact]
public void ContinuationPagesPrecedeEveryPayloadOverflowPage() {
	BerkeleyDbHashRecord[] records = LongChainRecords()
		.Append( Record( "hw03-overflow-order", 1025, 0x6A ) )
		.ToArray()
	;
	records = Sort( records );
	byte[] image = Build( records, 9 * 4096 );
	Assert.Equal( (byte)13, Page( image, 5 )[25] );
	Assert.Equal( (byte)13, Page( image, 6 )[25] );
	Assert.Equal( (byte)13, Page( image, 7 )[25] );
	Assert.Equal( (byte)7, Page( image, 8 )[25] );
}
```

- [ ] **Step 5: Add determinism, cancellation, and ceiling tests**

Add these exact facts:

- `RepeatedAndReversedInputsProduceEqualBytes`;
- `ContrastingCulturesProduceEqualBytes` using `en-US` and `tr-TR` with
  restoration in `finally`;
- `LargerCeilingDoesNotChangeNaturalImage`, comparing 20,480-byte and 64-MiB
  ceilings for complete equality;
- `SameRecordsHaveSameFileIdAcrossNaturalAndCappedLayouts`, comparing metadata
  bytes 52 through 71;
- `PreCancelledTokenEscapesWithoutAnImage`;
- `CancellationDuringCandidateEvaluationEscapesWithoutAnImage` using the
  instrumented list; and
- `IntMaxValueCeilingDoesNotCauseGratuitousAllocation`, asserting the natural
  five-page image.

Implement them with complete-array, metadata-slice, and cancellation
assertions:

```csharp
[Fact]
public void RepeatedAndReversedInputsProduceEqualBytes() {
	BerkeleyDbHashRecord[] forward = GrowthRecords();
	BerkeleyDbHashRecord[] reversed = Sort( forward.Reverse().ToArray() );
	Assert.Equal( Build( forward ), Build( forward ) );
	Assert.Equal( Build( forward ), Build( reversed ) );
}

[Fact]
public void ContrastingCulturesProduceEqualBytes() {
	CultureInfo original = CultureInfo.CurrentCulture;
	CultureInfo originalUi = CultureInfo.CurrentUICulture;
	try {
		CultureInfo.CurrentCulture = new CultureInfo( "en-US" );
		CultureInfo.CurrentUICulture = new CultureInfo( "en-US" );
		byte[] expected = Build( GrowthRecords() );
		CultureInfo.CurrentCulture = new CultureInfo( "tr-TR" );
		CultureInfo.CurrentUICulture = new CultureInfo( "tr-TR" );
		Assert.Equal( expected, Build( GrowthRecords() ) );
	} finally {
		CultureInfo.CurrentCulture = original;
		CultureInfo.CurrentUICulture = originalUi;
	}
}

[Fact]
public void LargerCeilingDoesNotChangeNaturalImage() {
	BerkeleyDbHashRecord[] records = GrowthRecords();
	Assert.Equal(
		Build( records, 5 * 4096 ),
		Build( records, 64 * 1024 * 1024 )
	);
}

[Fact]
public void SameRecordsHaveSameFileIdAcrossNaturalAndCappedLayouts() {
	byte[] natural = Build( GrowthRecords(), 5 * 4096 );
	byte[] capped = Build( GrowthRecords(), 4 * 4096 );
	Assert.Equal( natural.AsSpan( 52, 20 ).ToArray(), capped.AsSpan( 52, 20 ).ToArray() );
}

[Fact]
public void PreCancelledTokenEscapesWithoutAnImage() {
	using var source = new CancellationTokenSource();
	source.Cancel();
	Assert.Throws<OperationCanceledException>(
		() => Build( GrowthRecords(), cancellationToken: source.Token )
	);
}

[Fact]
public void CancellationDuringCandidateEvaluationEscapesWithoutAnImage() {
	using var source = new CancellationTokenSource();
	var records = new CancelAfterReadsList(
		GrowthRecords(),
		source,
		cancelOnRead: 2
	);
	Assert.Throws<OperationCanceledException>(
		() => Build( records, cancellationToken: source.Token )
	);
}

[Fact]
public void IntMaxValueCeilingDoesNotCauseGratuitousAllocation() {
	Assert.Equal( 5 * 4096, Build( GrowthRecords(), int.MaxValue ).Length );
}
```

- [ ] **Step 6: Verify and publish the growth RED**

Run/publish:

```bash
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release -f net10.0 --filter FullyQualifiedName~Hw03WriterGrowthTests
```

Expected failures: the Task 2 planner still reports that the two-bucket Hash
page does not fit. No compilation errors and no HW02/overflow regression are
acceptable.

- [ ] **Step 7: Commit the growth RED**

```bash
git add tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw03WriterGrowthTests.cs \
  tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw03WriterTestSupport.cs
git commit -m "test: define HW03 bounded growth and collision chains"
```

---

### Task 4: Implement bounded growth and type-13 continuation chains

**Files:**

- Modify: `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashV9LayoutPlan.cs`
- Modify: `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashV9LayoutPlanner.cs`
- Modify: `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashV9ImageBuilder.cs`
- Test:
  `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw03WriterGrowthTests.cs`

**Interfaces:**

- Consumes: the Task 2 item shapes and Task 3 collision fixtures.
- Produces: final canonical bucket count, complete linked Hash page plans, exact
  metadata masks/spares, and bounded fallback behavior.

- [ ] **Step 1: Represent candidate layouts explicitly**

Add an internal planner-only candidate with these fields:

```csharp
private sealed record CandidateLayout(
	int BucketCount,
	IReadOnlyList<IReadOnlyList<IReadOnlyList<RecordShape>>> BucketPages,
	int ContinuationPageCount,
	int TotalPageCount,
	bool HasReducibleCollision,
	bool IsFeasible
);
```

`RecordShape` caches `uint Hash`, key/value payload, encoded lengths, and
overflow page counts so candidate passes never re-hash or copy payloads.

- [ ] **Step 2: Pack one candidate deterministically**

Use the exact incremental fit predicate:

```csharp
private static bool Fits(
	int currentRecordCount,
	int currentItemBytes,
	RecordShape next
) => checked(
	PageHeaderSize
		+ ( ( currentRecordCount + 1 ) * 2 * sizeof( ushort ) )
		+ currentItemBytes
		+ next.KeyEncodedLength
		+ next.ValueEncodedLength
) <= PageSize;
```

Partition by `record.Hash & (uint)(bucketCount - 1)`, preserving incoming
canonical order. Greedily pack whole record pairs. Mark a candidate reducible
when an overfull bucket contains more than one distinct exact `uint` hash.

- [ ] **Step 3: Select the bounded candidate**

Implement the approved search literally:

```csharp
CandidateLayout? greatestFeasible = null;
for ( int bucketCount = 2; ; bucketCount = checked( bucketCount * 2 ) ) {
	cancellationToken.ThrowIfCancellationRequested();
	CandidateLayout candidate = Evaluate( shapes, bucketCount, pageBudget );
	if ( candidate.IsFeasible ) {
		greatestFeasible = candidate;
		if ( !candidate.HasReducibleCollision ) {
			return Finalize( candidate, shapes, cancellationToken );
		}
	} else if ( !candidate.HasReducibleCollision ) {
		break;
	}

	if ( !CanEvaluateNextBucketCount(
		bucketCount,
		pageBudget,
		overflowPageCount
	) ) {
		break;
	}
}

if ( greatestFeasible is null ) {
	throw new InvalidOperationException(
		"The Berkeley DB records do not fit the configured database limit."
	);
}
return Finalize( greatestFeasible, shapes, cancellationToken );
```

`CanEvaluateNextBucketCount` uses checked arithmetic and rejects the next
candidate when metadata + primary pages + unavoidable payload pages exceed
`maximumDatabaseSize / 4096`.

- [ ] **Step 4: Assign physical pages in the frozen order**

Assign:

```text
0                         metadata
1 .. N                    primary buckets
N + 1 .. hashPageEnd      continuation pages by bucket and chain position
hashPageEnd + 1 .. end    payload chains by bucket/page/record/key/value
```

Build previous/next links only after every Hash page number is known. Then walk
Hash pages in physical order, replace item shapes with final item plans, and
assign each off-page item's independent overflow chain.

- [ ] **Step 5: Emit links and grown metadata**

Change Hash-page emission to write:

```csharp
WriteUInt32( page, 12, checked( (uint)source.PreviousPageNumber ) );
WriteUInt32( page, 16, checked( (uint)source.NextPageNumber ) );
```

Change metadata to:

```csharp
WriteUInt32( image, 32, checked( (uint)( plan.PageCount - 1 ) ) );
WriteUInt32( image, 72, checked( (uint)( plan.BucketCount - 1 ) ) );
WriteUInt32( image, 76, checked( (uint)( plan.BucketCount - 1 ) ) );
WriteUInt32( image, 80, checked( (uint)( ( plan.BucketCount / 2 ) - 1 ) ) );

int spareCount = BitOperations.Log2( checked( (uint)plan.BucketCount ) ) + 1;
for ( int index = 0; index < spareCount; index++ ) {
	WriteUInt32( image, 96 + ( index * sizeof( uint ) ), 1 );
}
```

Add `using System.Numerics;`. Leave unused spares zero because the image begins
zero-filled.

- [ ] **Step 6: Check cancellation and narrowing at every loop boundary**

Place `ThrowIfCancellationRequested` before record shaping, during every
candidate record pass, between candidates, during final page assignment, and
before each emitted page. Use `checked` for page totals, byte totals, offset
tables, item counts, page numbers, and all `ushort`/`uint` conversions.

- [ ] **Step 7: Verify the full managed GREEN**

Publish the code head and require the normal PR workflow. Expected evidence:

```bash
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release
dotnet test tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/Icod.TermInfo.BerkeleyDb.Interop.Tests.csproj -c Release
```

All three target frameworks must pass. Confirm complete HW02 byte tests,
complete HW00 overflow equality, growth, exact collision, capped fallback,
bounds, determinism, cancellation, reader round trips, API baseline, package,
IL-only, and Runtime-only checks.

- [ ] **Step 8: Commit the managed GREEN**

```bash
git add Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashV9LayoutPlan.cs \
  Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashV9LayoutPlanner.cs \
  Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashV9ImageBuilder.cs
git commit -m "feat: add bounded Hash-v9 collision growth"
```

---

### Task 5: Define the compiling RED for native HW03 fixtures

**Files:**

- Modify:
  `tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/src/NativeOracleTests.cs`

**Interfaces:**

- Consumes: files named `hw03-overflow`, `hw03-growth`, and `hw03-collision`
  from `ICOD_HDB02_FIXTURE_ROOT`.
- Produces: failing executable requirements for Task 6's fixture generator and
  workflow integration.

- [ ] **Step 1: Require native dumps for all three production layouts**

Extend `ProductionReaderMatchesEveryNativeRecord` data:

```csharp
[InlineData( "hw03-overflow", 1 )]
[InlineData( "hw03-growth", 4 )]
[InlineData( "hw03-collision", 2 )]
```

Extend the clean-miss theory with all three names. The existing helper will
load each `.dump`, enumerate every key/value pair, and confirm the production
reader returns identical bytes from the corresponding `.db`.

- [ ] **Step 2: Require exact large and collision lookup values**

Add a theory that reads the expected key and value files without decoding the
key:

```csharp
[Theory]
[InlineData( "hw03-overflow", "hw03-overflow.key", "hw03-overflow.value" )]
[InlineData( "hw03-collision", "hw03-collision-first.key", "hw03-collision-first.value" )]
[InlineData( "hw03-collision", "hw03-collision-second.key", "hw03-collision-second.value" )]
public void ProductionReaderRecoversExactHw03RawValue(
	string database,
	string keyFile,
	string valueFile
) {
	byte[] key = File.ReadAllBytes( FixturePath( keyFile ) );
	byte[] expected = File.ReadAllBytes( FixturePath( valueFile ) );
	Assert.True( BerkeleyDbHashReader.TryReadValue(
		FixturePath( database + ".db" ),
		key,
		out byte[] actual
	) );
	Assert.Equal( expected, actual );
}
```

- [ ] **Step 3: Publish and observe the native-fixture RED**

The interop project must compile. HDB00 must fail because the three new `.dump`
and `.db` files do not exist yet. Record the exact failing job and missing path;
do not weaken the test or add a skip.

- [ ] **Step 4: Commit the native RED**

```bash
git add tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/src/NativeOracleTests.cs
git commit -m "test: require native HW03 writer fixtures"
```

---

### Task 6: Generate and qualify production HW03 images natively

**Files:**

- Create: `tools/hdb00/hw03-writer-probe/Hw03.ManagedWriterProbe.csproj`
- Create: `tools/hdb00/hw03-writer-probe/Program.cs`
- Modify: `Icod.TermInfo.BerkeleyDb/src/Properties/AssemblyInfo.cs`
- Modify: `.github/workflows/hdb00-interoperability.yml`
- Test:
  `tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/src/NativeOracleTests.cs`

**Interfaces:**

- Consumes: one output-directory argument and the internal image builder.
- Produces: three `.db` files, their exact `.key`/`.value` files, and no package
  asset; the workflow adds native `.dump` files.

- [ ] **Step 1: Add the repository-only C# probe project**

Use a net10.0 executable with no package references:

```xml
<Project Sdk="Microsoft.NET.Sdk">
	<PropertyGroup>
		<OutputType>Exe</OutputType>
		<TargetFramework>net10.0</TargetFramework>
		<LangVersion>13.0</LangVersion>
		<Nullable>enable</Nullable>
		<ImplicitUsings>enable</ImplicitUsings>
		<AssemblyName>Icod.TermInfo.Hw03.ManagedWriterProbe</AssemblyName>
		<RootNamespace>Icod.TermInfo.Hw03.ManagedWriterProbe</RootNamespace>
	</PropertyGroup>
	<ItemGroup>
		<ProjectReference Include="..\..\..\Icod.TermInfo.BerkeleyDb\Icod.TermInfo.BerkeleyDb.csproj" />
	</ItemGroup>
</Project>
```

Grant exactly this friend assembly:

```csharp
[assembly: InternalsVisibleTo( "Icod.TermInfo.Hw03.ManagedWriterProbe" )]
```

- [ ] **Step 2: Emit deterministic qualification fixtures**

`Program.cs` validates one output-directory argument, creates it, and emits:

```csharp
internal sealed record LookupFile(
	string Stem,
	byte[] Key,
	byte[] Value
);

byte[] overflowKey = Encoding.ASCII.GetBytes( "hw03-overflow" );
byte[] overflowValue = Enumerable.Range( 0, 9000 )
	.Select( i => (byte)( i % 251 ) )
	.ToArray()
;
WriteFixture(
	root,
	"hw03-overflow",
	[
		new BerkeleyDbHashRecord( overflowKey, overflowValue ),
	],
	[ new LookupFile( "hw03-overflow", overflowKey, overflowValue ) ]
);

BerkeleyDbHashRecord[] growth = GrowthRecords();
WriteFixture(
	root,
	"hw03-growth",
	growth,
	[
		new LookupFile(
			"hw03-growth-003",
			growth.Single( record => Encoding.ASCII.GetString( record.Key.Span ) == "hw03-growth-003" ).Key.ToArray(),
			growth.Single( record => Encoding.ASCII.GetString( record.Key.Span ) == "hw03-growth-003" ).Value.ToArray()
		),
	]
);

BerkeleyDbHashRecord[] collisions = ExactCollisionRecords();
WriteFixture(
	root,
	"hw03-collision",
	collisions,
	[
		new LookupFile(
			"hw03-collision-first",
			collisions.Single( record => record.Key.Span.SequenceEqual( FirstExactCollisionKey() ) ).Key.ToArray(),
			collisions.Single( record => record.Key.Span.SequenceEqual( FirstExactCollisionKey() ) ).Value.ToArray()
		),
		new LookupFile(
			"hw03-collision-second",
			collisions.Single( record => record.Key.Span.SequenceEqual( SecondExactCollisionKey() ) ).Key.ToArray(),
			collisions.Single( record => record.Key.Span.SequenceEqual( SecondExactCollisionKey() ) ).Value.ToArray()
		),
	]
);

static void WriteFixture(
	string root,
	string databaseName,
	BerkeleyDbHashRecord[] records,
	IReadOnlyList<LookupFile> lookups
) {
	Array.Sort(
		records,
		static ( left, right ) =>
			BerkeleyDbHashV9WriterKeyComparer.Instance.Compare(
				left.Key.Span,
				right.Key.Span
			)
	);
	byte[] database = BerkeleyDbHashV9ImageBuilder.Build(
		records,
		BerkeleyDbTerminalDescriptionProviderOptions.DefaultMaximumDatabaseSize,
		CancellationToken.None
	);
	File.WriteAllBytes( Path.Combine( root, databaseName + ".db" ), database );
	foreach ( LookupFile lookup in lookups ) {
		File.WriteAllBytes( Path.Combine( root, lookup.Stem + ".key" ), lookup.Key );
		File.WriteAllBytes( Path.Combine( root, lookup.Stem + ".value" ), lookup.Value );
	}
}
```

`GrowthRecords` uses the four Task 3 keys with distinct 1,024-byte fill values.
`ExactCollisionRecords` uses the two Task 3 long keys with distinct 1,024-byte
values. Sort each array with `BerkeleyDbHashV9WriterKeyComparer.Instance`, call
the production builder with the default 64-MiB ceiling, and write the database.
Write every lookup key and expected value as separate binary files using the
names required by Task 5.

- [ ] **Step 3: Generate and verify Linux fixtures**

Before the existing production-reader comparison step, add a Linux bash step:

```bash
root="$RUNNER_TEMP/icod-terminfo-hdb00"
dotnet run --project tools/hdb00/hw03-writer-probe/Hw03.ManagedWriterProbe.csproj \
  -c Release -f net10.0 -- "$root"

for name in hw03-overflow hw03-growth hw03-collision; do
  db5.3_verify "$root/$name.db"
  db5.3_dump -k -f "$root/$name.dump" "$root/$name.db"
done

"$root/hdb00-probe" "$root/hw03-overflow.db" hw03-overflow \
  "$root/hw03-overflow.actual"
cmp "$root/hw03-overflow.actual" "$root/hw03-overflow.value"

first_key="$(LC_ALL=C tr -d '\n' < "$root/hw03-collision-first.key")"
second_key="$(LC_ALL=C tr -d '\n' < "$root/hw03-collision-second.key")"
"$root/hdb00-probe" "$root/hw03-collision.db" "$first_key" \
  "$root/hw03-collision-first.actual"
"$root/hdb00-probe" "$root/hw03-collision.db" "$second_key" \
  "$root/hw03-collision-second.actual"
cmp "$root/hw03-collision-first.actual" "$root/hw03-collision-first.value"
cmp "$root/hw03-collision-second.actual" "$root/hw03-collision-second.value"
```

Also probe `hw03-growth-003` and compare its value file. Do not modify the C
probe.

- [ ] **Step 4: Generate and verify macOS fixtures**

Add the equivalent step using:

```bash
db_prefix="$(brew --prefix berkeley-db@5)"
export DYLD_LIBRARY_PATH="$db_prefix/lib${DYLD_LIBRARY_PATH:+:$DYLD_LIBRARY_PATH}"
"$db_prefix/bin/db_verify" "$database"
"$db_prefix/bin/db_dump" -k -f "$dump" "$database"
```

Use the same C# probe inputs, native lookup keys, and byte-for-byte `cmp`
assertions as Linux.

- [ ] **Step 5: Transport all Linux HW03 evidence to Windows**

Add these files to the Linux artifact upload:

```text
hw03-overflow.db
hw03-overflow.dump
hw03-overflow.key
hw03-overflow.value
hw03-growth.db
hw03-growth.dump
hw03-growth-003.key
hw03-growth-003.value
hw03-collision.db
hw03-collision.dump
hw03-collision-first.key
hw03-collision-first.value
hw03-collision-second.key
hw03-collision-second.value
```

The existing Windows `dotnet test` invocation then executes Task 5's managed
reader checks against the transported Linux bytes. Add no native Windows setup.

Add the corresponding HW03 databases, dumps, keys, values, and native lookup
outputs to the macOS artifact list as well so a failed native job retains its
complete diagnostic evidence. Windows continues to download only the Linux
artifact.

- [ ] **Step 6: Verify the native GREEN on the exact code head**

Require all HDB00 jobs to pass:

- Linux: native `db_verify`, native dump, raw lookup comparisons, and managed
  tests;
- macOS: the same native and managed checks through `berkeley-db@5`; and
- Windows: managed reading of every transported Linux database/dump/value.

Also require the normal 12-job PR workflow to remain green, including API,
package, Source Link, symbols, and RID archive checks.

- [ ] **Step 7: Commit the native GREEN**

```bash
git add tools/hdb00/hw03-writer-probe/Hw03.ManagedWriterProbe.csproj \
  tools/hdb00/hw03-writer-probe/Program.cs \
  Icod.TermInfo.BerkeleyDb/src/Properties/AssemblyInfo.cs \
  .github/workflows/hdb00-interoperability.yml
git commit -m "test: qualify HW03 images with native Berkeley DB"
```

---

### Task 7: Accept HW03 and advance the roadmaps

**Files:**

- Create: `docs/1.16.0-HW03-COLLISION-OVERFLOW-BOUNDED-GROWTH.md`
- Modify: `Icod.TermInfo-1.16.0-Berkeley-DB-Hash-V9-Writer-Roadmap.md`
- Modify: `Icod.TermInfo-Post-1.0-Development-Roadmap.md`

**Interfaces:**

- Consumes: the final exact code head and completed normal/HDB00 workflow run
  identifiers.
- Produces: the durable HW03 acceptance record and HW04-next roadmap state.

- [ ] **Step 1: Perform exact-head verification before claiming completion**

For the final code head, record:

```bash
git rev-parse HEAD
git status --short
```

Use GitHub workflow data to confirm that the normal PR run and HDB00 run both
refer to that exact SHA. Record every job conclusion and the net8/net9/net10
BerkeleyDb/Interop test counts from their logs. A run from an earlier head is
not acceptance evidence.

- [ ] **Step 2: Reconfirm frozen package and API boundaries**

From the successful normal run, verify:

- exact 12-type public API baseline;
- API equivalence across net8.0/net9.0/net10.0;
- assembly version `1.0.0.0`;
- coordinated package version `1.16.0-Alpha-3`;
- Runtime-only production dependency;
- IL-only package with no native assets;
- deterministic nupkg/snupkg and portable symbols; and
- unchanged public writer `NotSupportedException` boundary.

- [ ] **Step 3: Write the HW03 acceptance record**

Document the observed exact code head and numeric workflow run IDs, plus:

- selected storage profile and page allocation order;
- 1,024-byte inline and 4,070-byte overflow boundaries;
- power-of-two natural growth and capped fallback;
- exact-hash type-13 continuation chains;
- complete HW02 and HW00 byte preservation;
- managed/native host split and results;
- public/package/dependency invariants; and
- HW04 as the next gate.

Do not write prospective language such as “will pass.” Use only observed run
results.

- [ ] **Step 4: Advance both roadmaps**

Change the 1.16.0 roadmap header to `HW03 COMPLETE / HW04 NEXT`, add the
accepted exact head/run IDs to HW03, and mark its gate passed. Change the main
roadmap's next implementation gate to HW04 public terminfo publication without
adding migration or catalog automation to 1.16.0.

- [ ] **Step 5: Validate and commit documentation**

```bash
rg -n "HW03|HW04|1.16.0-Alpha-3" \
  docs/1.16.0-HW03-COLLISION-OVERFLOW-BOUNDED-GROWTH.md \
  Icod.TermInfo-1.16.0-Berkeley-DB-Hash-V9-Writer-Roadmap.md \
  Icod.TermInfo-Post-1.0-Development-Roadmap.md
git diff --check
git add docs/1.16.0-HW03-COLLISION-OVERFLOW-BOUNDED-GROWTH.md \
  Icod.TermInfo-1.16.0-Berkeley-DB-Hash-V9-Writer-Roadmap.md \
  Icod.TermInfo-Post-1.0-Development-Roadmap.md
git commit -m "docs: accept HW03 collision and overflow support"
```

Publish this documentation-only commit and continue without waiting for its
workflow. Report the accepted code head separately from the later documentation
head.

---

## Execution Order and Stop Conditions

Execute Tasks 1 through 7 in order with `superpowers:executing-plans`.

- Stop if a RED does not fail for the stated missing behavior; diagnose the
  test before changing production.
- Stop if any GREEN requires public API, a native production dependency, a
  different page size, a different hash function, or public-writer connection.
- Stop if native Berkeley DB rejects the approved contiguous-primary plus
  continuation-page layout; inspect metadata and link semantics against the
  native verifier rather than weakening the oracle.
- Stop if the remote branch head changes unexpectedly; fetch and reconcile
  before publishing another commit.
- Do not accept HW03 until both normal and HDB00 workflows are green on the
  exact final code head.
