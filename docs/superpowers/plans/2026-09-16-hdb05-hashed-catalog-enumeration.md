# HDB05 Hashed Catalog Enumeration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [x]`) syntax for tracking.

**Goal:** Add bounded, deterministic, read-only enumeration of logical canonical and alias publications in one supported ncurses Berkeley DB Hash-v9 terminfo store.

**Architecture:** Extend the internal Hash-v9 reader with complete key/value enumeration over one acquired image, then build a separate public catalog reader which validates ncurses marker records, delegates compiled semantics to Runtime, and returns immutable logical publications in ordinal order. Exact lookup and system discovery remain unchanged.

**Tech Stack:** C# 13; .NET 8/9/10; xUnit; pure-managed `Icod.TermInfo.BerkeleyDb`; native Berkeley DB/ncurses only as CI fixture producers.

**Spec:** `docs/superpowers/specs/2026-09-16-hdb05-hashed-catalog-enumeration-design.md`

**Execution status:** COMPLETE / ACCEPTED at qualification head
`819ca194b51baa78f52a6464a2e64c45418eebc6`.

- Normal PR workflow run `35049904561`: 12/12 jobs passed.
- HDB00 workflow run `35049904277`: 3/3 jobs passed.
- Unit suite: 308/308 per TFM on Windows/Linux/macOS.
- Native suite: 20/20 per TFM on Windows/Linux/macOS.
- Package-only catalog consumer: passed on net8/net9/net10.
- Closure record: `docs/1.15.0-HDB05-HASHED-CATALOG-ENUMERATION.md`.

## Global Constraints

- Advance the coordinated suite to exactly `1.15.0-Alpha-5`.
- Preserve reusable `AssemblyVersion` `1.0.0.0`.
- Preserve Runtime's frozen public API and dependency-free package boundary.
- Production remains read-only, pure managed, and free of P/Invoke/native assets.
- No production behavior is written before its failing test is observed.
- Use tabs, 1TBS braces, mandatory braces for `if`/`else`, and the repository's multiline-call and ternary conventions.
- Parse compiled entries only through `CompiledTermInfoParser`.
- Physical Hash-page order is never a public ordering contract.
- Keep PR #45 open, draft, and unmerged.

---

### Task 1: Establish Alpha-5 and the internal enumeration RED

**Files:**
- Modify: `Directory.Build.props`
- Create: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb05HashEnumerationTests.cs`
- Modify after compile RED: `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashReader.cs`
- Create after compile RED: `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashRecord.cs`
- Create after compile RED: `Icod.TermInfo.BerkeleyDb/src/ByteArrayComparer.cs`

**Interfaces:**
- Consumes: existing `BerkeleyDbHashReader.ReadDatabase`, metadata/page/item validation, and synthetic Hash-v9 fixture conventions.
- Produces:

```csharp
internal static IReadOnlyList<BerkeleyDbHashRecord> ReadRecords(
	byte[] database,
	int maximumItemSize,
	int maximumRecordCount,
	CancellationToken cancellationToken
);

internal sealed class BerkeleyDbHashRecord {
	internal BerkeleyDbHashRecord( byte[] key, byte[] value );
	internal ReadOnlyMemory<byte> Key { get; }
	internal ReadOnlyMemory<byte> Value { get; }
}

internal sealed class ByteArrayComparer
	: IComparer<byte[]>, IEqualityComparer<byte[]> {
	internal static ByteArrayComparer Instance { get; }
}
```

- [x] **Step 1: bump the coordinated version**

Change only:

```xml
<IcodTermInfoSuiteVersion>1.15.0-Alpha-5</IcodTermInfoSuiteVersion>
```

- [x] **Step 2: write the missing-interface tests**

Create real synthetic Hash-v9 images and call the wished-for API directly. Begin with independently derived literal expectations:

```csharp
[Fact]
public void ReadRecordsReturnsEveryPairInOrdinalByteKeyOrder() {
	byte[] database = CreateDatabase(
		( new byte[] { 0x7A }, new byte[] { 0x02 } ),
		( new byte[] { 0x61 }, new byte[] { 0x01 } )
	);

	IReadOnlyList<BerkeleyDbHashRecord> records =
		BerkeleyDbHashReader.ReadRecords(
			database,
			maximumItemSize: 16,
			maximumRecordCount: 2,
			CancellationToken.None
		);

	Assert.Equal( 2, records.Count );
	Assert.Equal( new byte[] { 0x61 }, records[0].Key.ToArray() );
	Assert.Equal( new byte[] { 0x01 }, records[0].Value.ToArray() );
	Assert.Equal( new byte[] { 0x7A }, records[1].Key.ToArray() );
	Assert.Equal( new byte[] { 0x02 }, records[1].Value.ToArray() );
}
```

The file must also cover:

- multiple pairs on one Hash page;
- multiple Hash pages in reverse key order;
- inline/off-page keys and values;
- little- and big-endian images;
- zero records;
- record-limit acceptance at the exact boundary;
- rejection of the next record beyond the boundary;
- duplicate byte-key rejection;
- cancellation before and during enumeration;
- unsupported/malformed encountered pages and items; and
- a regression proving existing exact lookup still stops successfully before an unrelated later malformed page.

For every test, name the production mutation it catches in a comment only when the test name cannot state it clearly.

- [x] **Step 3: commit and verify compile RED**

Commit:

```text
test: define HDB05 hash enumeration
```

Expected CI failure: `CS0117` for missing
`BerkeleyDbHashReader.ReadRecords` and/or `CS0246` for missing
`BerkeleyDbHashRecord`, on every exercised TFM. No production enumeration
exists at this head.

- [x] **Step 4: add declaration-only production surface**

Add the exact internal types/signature above. `ReadRecords` must throw
`NotImplementedException`; no page enumeration behavior is added.

```csharp
internal static IReadOnlyList<BerkeleyDbHashRecord> ReadRecords(
	byte[] database,
	int maximumItemSize,
	int maximumRecordCount,
	CancellationToken cancellationToken
) {
	throw new NotImplementedException();
}
```

Implement `ByteArrayComparer` only far enough for compilation; the behavior
tests must still fail because `ReadRecords` is unimplemented.

- [x] **Step 5: commit and verify behavioral RED**

Commit:

```text
test: expose HDB05 enumeration red
```

Expected: every new enumeration behavior test reaches the declaration-only
method and fails with `NotImplementedException`; existing HDB02–HDB04 tests
remain green. Record exact run IDs and per-TFM pass/fail totals before Task 2.

---

### Task 2: Implement complete bounded Hash-v9 record enumeration

**Files:**
- Modify: `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashReader.cs`
- Modify: `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashRecord.cs`
- Modify: `Icod.TermInfo.BerkeleyDb/src/ByteArrayComparer.cs`
- Test unchanged: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb05HashEnumerationTests.cs`

**Interfaces:**
- Consumes: the declaration-only Task 1 interfaces.
- Produces: validated, byte-key-sorted immutable internal records over one caller-owned image.

- [x] **Step 1: implement immutable record ownership**

Clone constructor inputs and expose read-only memory:

```csharp
internal BerkeleyDbHashRecord( byte[] key, byte[] value ) {
	ArgumentNullException.ThrowIfNull( key );
	ArgumentNullException.ThrowIfNull( value );

	Key = key.ToArray();
	Value = value.ToArray();
}
```

If double copying is measurable in tests, transfer ownership through a private
factory rather than exposing mutable arrays; do not weaken the immutable
contract.

- [x] **Step 2: implement byte-array equality, hashing, and ordering**

Ordering is unsigned lexicographic byte order, with a shorter equal prefix first.
Equality and hash codes consume byte content, never array identity.

- [x] **Step 3: implement the bounded scan**

Validate null/positive arguments and cancellation before metadata parsing. Reuse
`ReadMetadata`, `GetPage`, `ValidatePageIdentity`,
`ValidateIndexTable`, and `ReadHashItem`.

For each ascending page number and paired item index:

1. check cancellation;
2. reject unsupported encountered page types exactly as lookup does;
3. reject an odd item count;
4. check `records.Count >= maximumRecordCount` before reading the next pair;
5. read key and value with `maximumItemSize`;
6. reject duplicate exact byte keys with `ByteArrayComparer.Instance`;
7. add the owned record.

Sort the completed array using `ByteArrayComparer.Instance` on `Key`.

- [x] **Step 4: run the dedicated suite**

Run:

```text
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release --filter FullyQualifiedName~Hdb05HashEnumerationTests
```

Expected: all HDB05 enumeration tests pass on net8/net9/net10.

- [x] **Step 5: run all BerkeleyDb unit tests**

Run the complete project in Release. Expected: all prior HDB02–HDB04 cases plus
the new HDB05 cases pass on all three TFMs.

- [x] **Step 6: commit GREEN**

Commit:

```text
feat: enumerate bounded Hash-v9 records
```

Push and require both the normal PR workflow and HDB00 workflow green before
starting the public catalog RED. Record counts and exact run IDs.

---

### Task 3: Establish the public catalog declaration and behavioral RED

**Files:**
- Create: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb05CatalogReaderTests.cs`
- Create after compile RED:
  - `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalCatalogReader.cs`
  - `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalCatalogReaderOptions.cs`
  - `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalCatalogEntry.cs`
  - `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalCatalogEntryKind.cs`

**Interfaces:**
- Consumes: Task 2 `ReadRecords` and approved HDB05 specification.
- Produces: the exact four-type public API frozen in the specification.

- [x] **Step 1: write public behavior tests before declarations**

Use complete compiled fixtures with literal expected names/kinds. Representative
test:

```csharp
[Fact]
public void ReadReturnsCanonicalAndAliasPublicationsInOrdinalOrder() {
	WithDatabase(
		CreateCatalogStore( "sample", "z-alias", "a-alias" ),
		path => {
			BerkeleyDbTerminalCatalogReader reader = new( path );

			IReadOnlyList<BerkeleyDbTerminalCatalogEntry> entries =
				reader.Read();

			Assert.Equal(
				new[] { "a-alias", "sample", "z-alias" },
				entries.Select( entry => entry.Name )
			);
			Assert.Equal(
				new[] {
					BerkeleyDbTerminalCatalogEntryKind.Alias,
					BerkeleyDbTerminalCatalogEntryKind.Canonical,
					BerkeleyDbTerminalCatalogEntryKind.Alias,
				},
				entries.Select( entry => entry.Kind )
			);
			Assert.All(
				entries,
				entry => Assert.Same( entries[0].Terminal, entry.Terminal )
			);
		}
	);
}
```

Include tests for every Section 10 Checkpoint B behavior in the specification.
Expected values must be literal or derived from fixture inputs, never from
production helpers.

- [x] **Step 2: commit and verify compile RED**

Commit:

```text
test: define HDB05 public catalog
```

Expected: missing-type `CS0246` failures on every host/TFM.

- [x] **Step 3: add declaration-only public types**

Add XML documentation, immutable constructor/property snapshots, enum values,
and public method signatures exactly as approved. Both `Read` methods must
reach a private method that throws `NotImplementedException`.

Options validation and snapshot tests should pass; catalog behavior tests must
fail with the declaration-only exception.

- [x] **Step 4: commit and verify behavioral RED**

Commit:

```text
test: expose HDB05 catalog red
```

Expected failures must be limited to unimplemented catalog reads. Fix formatting
or declaration defects without adding catalog behavior. Record exact RED totals
before Task 4.

---

### Task 4: Implement logical ncurses catalog resolution

**Files:**
- Create: `Icod.TermInfo.BerkeleyDb/src/NcursesCatalogReader.cs`
- Create: `Icod.TermInfo.BerkeleyDb/src/TerminalNameValidator.cs`
- Modify: `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalCatalogReader.cs`
- Modify: `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalDescriptionProvider.cs`
- Test unchanged:
  - `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb05CatalogReaderTests.cs`
  - `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb03ProviderTests.cs`

**Interfaces:**
- Consumes: ordered internal Hash records and the parser/options snapshots.
- Produces:

```csharp
internal static IReadOnlyList<BerkeleyDbTerminalCatalogEntry> Read(
	IReadOnlyList<BerkeleyDbHashRecord> records,
	CompiledTermInfoParserOptions parserOptions,
	int maximumIndexHops,
	CancellationToken cancellationToken
);
```

- [x] **Step 1: extract shared exact-name validation**

Move HDB03's validation rules into
`TerminalNameValidator.Validate(string name)`. Keep exception types, parameter
name `name`, and messages unchanged. Call it from the explicit provider and
new catalog reader. Run HDB03 provider tests before continuing.

- [x] **Step 2: acquire one fresh image per public read**

`BerkeleyDbTerminalCatalogReader.Read()` delegates to
`Read(CancellationToken.None)`. The token overload:

1. checks cancellation;
2. reads `DatabasePath` once through `BerkeleyDbHashReader.ReadDatabase`;
3. calls Task 2 `ReadRecords` with the parser maximum plus marker byte;
4. maps only storage `InvalidDataException` to
   `BerkeleyDbDatabaseFormatException`; and
5. passes ordered records to `NcursesCatalogReader`.

No result cache is added.

- [x] **Step 3: index exact raw keys**

Build a content-keyed dictionary from record keys. Duplicate keys should already
have failed in Task 2; keep a defensive format failure if this invariant is
violated.

- [x] **Step 4: validate and resolve records in byte-key order**

For each record in the already ordered list:

- empty values are package-format failures;
- marker `0` is parsed once and memoized by exact storage key;
- marker `2` publication keys are strict UTF-8 and pass
  `TerminalNameValidator`;
- marker-2 targets are followed with content-based cycle detection and the
  inclusive hop bound;
- a missing target, empty target, unsupported marker, cycle, or excessive hop is
  a package-format failure;
- the final marker-0 payload is parsed by `CompiledTermInfoParser`;
- an orphan marker-0 record is parsed but emits no entry.

Envelope failures should be constructed as
`BerkeleyDbDatabaseFormatException` at this layer so parser exceptions remain
unwrapped.

- [x] **Step 5: verify identity and classify**

Use ordinal comparison. The publication is canonical only when it equals
`Terminal.Name`; otherwise it is an alias only when present in
`Terminal.Aliases`. Throw `InvalidDataException` for a valid parsed entry
which does not declare the publication.

Reuse the memoized `TerminalDescription` instance for all publications which
reach the same marker-0 key.

- [x] **Step 6: sort and freeze output**

Order entries by ordinal `Name`, then enum `Kind`, then
`Terminal.Name`. Return `Array.AsReadOnly(entries.ToArray())`.

- [x] **Step 7: verify GREEN**

Run the HDB05 catalog tests, HDB03 provider tests, and complete BerkeleyDb unit
project. All must pass on net8/net9/net10 with no warnings.

- [x] **Step 8: commit GREEN**

Commit:

```text
feat: add hashed terminal catalog reader
```

Push and require the normal and HDB00 workflows green. Record exact head, run
IDs, and per-TFM totals.

---

### Task 5: Add adversarial ordering and resource qualification

**Files:**
- Modify: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb05HashEnumerationTests.cs`
- Modify: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb05CatalogReaderTests.cs`
- Modify only after a demonstrated failure, if required:
  - `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashReader.cs`
  - `Icod.TermInfo.BerkeleyDb/src/NcursesCatalogReader.cs`
  - `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalCatalogReader.cs`

**Interfaces:**
- Consumes: Task 4 public catalog.
- Produces: permanent adversarial guarantees required by the HDB05 acceptance gate.

- [x] **Step 1: add mutation-focused tests**

Add physical page permutations which must return identical entries, plus two
different malformed logical records whose lowest byte key must fail first.

Add exact boundary cases for:

- `MaximumRecordCount`;
- parser maximum entry size plus marker;
- `MaximumDatabaseSize`;
- zero and maximum supported index hops;
- pre-canceled and mid-enumeration tokens;
- repeated independent reads after replacement;
- concurrent reads with different reader option snapshots; and
- file-handle release after success and each failure family.

- [x] **Step 2: observe RED for any uncovered production defect**

If all additions pass, record them as characterization/qualification coverage
and do not alter production. If a test exposes a real defect, commit the failing
test alone, observe the exact failure, then make the smallest production
correction and rerun the complete suite.

- [x] **Step 3: commit qualification coverage**

Commit:

```text
test: harden HDB05 catalog enumeration
```

Record which cases were new RED behavior and which were already-green
characterization.

---

### Task 6: Qualify native stores and packed-package consumption

**Files:**
- Modify: `tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/src/NativeOracleTests.cs`
- Modify: `tools/berkeleydb-package-smoke/Program.cs`
- Modify only if fixture artifacts are insufficient:
  - `.github/workflows/hdb00-interoperability.yml`
  - `tools/hdb00/run-linux.sh`
  - `tools/hdb00/run-macos.sh`

**Interfaces:**
- Consumes: native `hashed-db.db` and `overflow-hashed-db.db` fixtures already generated by HDB00.
- Produces: cross-host native-oracle and isolated-NuGet evidence for the public catalog.

- [x] **Step 1: add native catalog assertions**

For the primary store, assert ordinal publication names and canonical/alias
kinds using literals from the HDB00 fixture. Assert that all publications share
the expected parsed canonical identity.

For the forced-overflow store, assert the published canonical identity and
successful parsing of the overflow-backed entry.

Run on net8/net9/net10 on Linux and macOS native-generated stores and Windows'
downloaded Linux store.

- [x] **Step 2: extend the isolated package consumer**

Construct `BerkeleyDbTerminalCatalogReader` only from packed
`Icod.TermInfo.BerkeleyDb` and Runtime artifacts. Assert:

- canonical absolute `DatabasePath`;
- canonical and alias names/kinds;
- shared parsed terminal identity;
- immutable option snapshots;
- fresh read behavior; and
- successful execution on net8/net9/net10.

No project reference or repository output path may satisfy the consumer.

- [x] **Step 3: run qualification workflows**

Require:

- normal PR workflow: 12/12 jobs;
- HDB00 workflow: 3/3 jobs;
- unit and native counts identical across all three TFMs and hosts;
- exact Alpha-5 package dependency/native-asset/API checks;
- package-only consumer on net8/net9/net10;
- installed-tool smoke on Windows/Linux/macOS; and
- all six standalone archive RIDs.

- [x] **Step 4: commit qualification**

Commit:

```text
test: qualify HDB05 catalog enumeration
```

Do not accept HDB05 until both workflows and their complete job sets are green.

---

### Task 7: Record HDB05 acceptance

**Files:**
- Create: `docs/1.15.0-HDB05-HASHED-CATALOG-ENUMERATION.md`
- Modify: `Icod.TermInfo-1.15.0-Berkeley-DB-Hashed-Terminfo-Acquisition-Roadmap.md`
- Modify: `Icod.TermInfo.BerkeleyDb/README.md`
- Modify: `docs/superpowers/plans/2026-09-16-hdb05-hashed-catalog-enumeration.md`
- Update: PR #45 body

**Interfaces:**
- Consumes: exact RED/GREEN/qualification heads and workflow logs.
- Produces: auditable HDB05 closure and an explicit HDB06 next step.

- [x] **Step 1: perform verification-before-completion**

Review the exact public API, dependency direction, native/package evidence,
exception boundaries, ordering, resource bounds, and all workflow conclusions.
Do not infer success from individual green jobs while a run remains active.

- [x] **Step 2: write the closure record**

Record:

- compile RED and behavioral RED heads/results;
- accepted implementation and qualification heads;
- all workflow IDs/job counts;
- per-TFM unit/native totals on every host;
- package-only consumer results;
- public API;
- supported marker semantics;
- deterministic ordering contract;
- resource and snapshot limits;
- explicit non-goals; and
- any already-green characterization cases.

- [x] **Step 3: update release-facing documentation**

Mark HDB05 complete/accepted, retain `1.15.0-Alpha-5`, and make HDB06
Inspection/tool integration next. Do not claim HDB06 behavior.

- [x] **Step 4: commit and qualify documentation closure**

Commit:

```text
docs: accept HDB05 catalog enumeration
```

Require the closure head's normal and HDB00 workflows green, then add their run
IDs to PR #45. Leave the PR open, draft, and unmerged.
