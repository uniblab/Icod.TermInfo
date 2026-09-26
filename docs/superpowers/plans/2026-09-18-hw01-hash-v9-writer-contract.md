# HW01 Hash-v9 Writer Contract Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Freeze and implement the minimal three-type public contract and bounded preflight behavior for whole-file ncurses-compatible Berkeley DB Hash-v9 terminfo publication.

**Architecture:** `Icod.TermInfo.BerkeleyDb` receives immutable logical publications, snapshots all caller-owned state, validates identities and compiled payloads through Runtime, and stops at a deliberate HW01 construction boundary. HW02/HW03 replace that boundary with deterministic image construction without changing the public API.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit 2.9, PowerShell 5.1-compatible package verification, `Icod.TermInfo.PublicApiSnapshot`, and GitHub Actions.

**Spec:** `docs/superpowers/specs/2026-09-18-hw01-hash-v9-writer-public-api-design.md`

## Global Constraints

- Add exactly `BerkeleyDbTerminalDatabaseEntry`, `BerkeleyDbTerminalDatabaseWriterOptions`, and `BerkeleyDbTerminalDatabaseWriter` as public types.
- Add public API only to `Icod.TermInfo.BerkeleyDb`; all other reusable public surfaces and JSON schemas remain frozen.
- Keep `Icod.TermInfo.BerkeleyDb` dependent only on `Icod.TermInfo`; add no package, P/Invoke, or native runtime dependency.
- Keep reusable assembly versions at `1.0.0.0` and public API equivalent on net8.0, net9.0, and net10.0.
- Advance the coordinated package version to exactly `1.16.0-Alpha-1` with the first HW01 contract-test checkpoint.
- Use exact ordinal identity comparison and strict UTF-8 without normalization, transliteration, or replacement bytes.
- Snapshot entry aliases, payloads, parser options, and the finite publication sequence before filesystem mutation.
- Do not expose generic key/value, page, bucket, hash, overflow, streaming, plan, result, or writer-specific exception APIs.
- Preserve strict RED -> GREEN commits; do not absorb HW02 image construction into HW01.
- Add or modify only C#, PowerShell 5.1-compatible, cmd/sh, XML/MSBuild, and Markdown files; do not add Python.

---

## File Structure

- Create `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw01WriterContractTests.cs`: reflection-first API, immutability, validation, version, and dependency contract tests.
- Create `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalDatabaseEntry.cs`: immutable logical publication.
- Create `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalDatabaseWriterOptions.cs`: immutable resource and overwrite policy.
- Create `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalDatabaseWriter.cs`: public boundary and orchestration.
- Create `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalDatabaseWriter.Preflight.cs`: name encoding, conflict detection, parser agreement, cancellation, and bounds.
- Modify `Directory.Build.props`: coordinated development version.
- Modify `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb01ContractTests.cs`: exact export list and development-line assertion.
- Modify `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb09ReleaseClosureTests.cs`: current 1.16 API freeze paths.
- Create `docs/1.16.0-BERKELEYDB-PUBLIC-API-BASELINE.txt`: exact additive manifest.
- Create `docs/1.16.0-HW01-BERKELEY-DB-WRITER-CONTRACT.md`: acceptance evidence.
- Modify `.github/scripts/verify-berkeleydb-package.ps1`: 1.16 baseline verification.
- Modify both release roadmaps only after the exact code/API head is green.

---

### Task 1: HW01 RED Public Contract and Version Gate

**Files:**
- Create: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw01WriterContractTests.cs`
- Modify: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb01ContractTests.cs`
- Modify: `Directory.Build.props`

**Interfaces:**
- Consumes: accepted 1.15 BerkeleyDb assembly and the approved HW01 design.
- Produces: compiling reflection-based tests for the exact three new types and coordinated version `1.16.0-Alpha-1`.

- [ ] **Step 1: Write the reflection-first failing contract test**

Load `Icod.TermInfo.BerkeleyDb.dll` from `AppContext.BaseDirectory`. Require the exact addition without compile-time references:

```csharp
[Fact]
public void WriterExportsExactlyTheApprovedThreeTypeAddition() {
	Assembly assembly = LoadBerkeleyDbAssembly();
	string[] writerTypes = assembly.GetExportedTypes()
		.Select( static type => type.FullName )
		.Where( static name =>
			name is not null
			&& name.StartsWith(
				"Icod.TermInfo.BerkeleyDb.BerkeleyDbTerminalDatabase",
				StringComparison.Ordinal
			)
		)
		.OrderBy( static name => name, StringComparer.Ordinal )
		.ToArray()!;

	Assert.Equal(
		new[] {
			"Icod.TermInfo.BerkeleyDb.BerkeleyDbTerminalDatabaseEntry",
			"Icod.TermInfo.BerkeleyDb.BerkeleyDbTerminalDatabaseWriter",
			"Icod.TermInfo.BerkeleyDb.BerkeleyDbTerminalDatabaseWriterOptions",
		},
		writerTypes
	);
}
```

Add reflection assertions for these exact shapes:

```text
Entry(string canonicalName, IEnumerable<string> aliases, byte[] data)
Options(CompiledTermInfoParserOptions? parserOptions = null,
        int maximumDatabaseSize = 67108864,
        int maximumRecordCount = 65536,
        bool overwriteExisting = false)
void Writer.Write(string databasePath,
                  IEnumerable<Entry> entries,
                  Options? options = null,
                  CancellationToken cancellationToken = default)
```

Assert Entry and Options are sealed, Writer is abstract+sealed, Write returns void, and no writer result, writer exception, or generic key/value surface exists.

- [ ] **Step 2: Advance the authoritative version with the contract tests**

Set:

```xml
<IcodTermInfoSuiteVersion>1.16.0-Alpha-1</IcodTermInfoSuiteVersion>
```

Keep the historical HDB01 comment. Rename the Hdb01 version test to `CoordinatedDevelopmentLineAndPackagePipelineIncludeBerkeleyDb` and assert the exact value above.

- [ ] **Step 3: Run the targeted test and verify intentional RED**

Run:

```text
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release -f net10.0 --filter "FullyQualifiedName~Hw01WriterContractTests|FullyQualifiedName~Hdb01ContractTests"
```

Expected: FAIL because the three public types do not exist and the Hdb01 export list lacks them. The test project must compile; compiler failure is not accepted RED.

- [ ] **Step 4: Commit and publish RED**

```bash
git add Directory.Build.props tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb01ContractTests.cs tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw01WriterContractTests.cs
git commit -m "test: define HW01 writer public contract"
```

Record the exact remote head and expected failing GitHub Actions job before GREEN.

---

### Task 2: HW01 GREEN Immutable Values and Writer Surface

**Files:**
- Create: `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalDatabaseEntry.cs`
- Create: `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalDatabaseWriterOptions.cs`
- Create: `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalDatabaseWriter.cs`
- Modify: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb01ContractTests.cs`
- Modify: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw01WriterContractTests.cs`

**Interfaces:**
- Consumes: Task 1 signatures.
- Produces: immutable Entry and Options values and the static Write method used by Task 3.

- [ ] **Step 1: Extend RED with reflection-based snapshot tests**

Construct Entry and Options through reflection so the tests still compile before types exist:

```csharp
string[] aliases = [ "sample-alias" ];
byte[] data = [ 1, 2, 3 ];
object entry = entryConstructor.Invoke([ "sample", aliases, data ]);
aliases[0] = "mutated";
data[0] = 255;

Assert.Equal(
	new[] { "sample-alias" },
	Assert.IsAssignableFrom<IEnumerable<string>>(
		entryType.GetProperty( "Aliases" )!.GetValue( entry )
	)
);
byte[] first = Assert.IsType<byte[]>(
	entryType.GetProperty( "Data" )!.GetValue( entry )
);
first[0] = 254;
byte[] second = Assert.IsType<byte[]>(
	entryType.GetProperty( "Data" )!.GetValue( entry )
);
Assert.Equal( new byte[] { 1, 2, 3 }, second );
```

Construct Options with `new CompiledTermInfoParserOptions(4096)`; assert its exposed ParserOptions is a different object with the same limit. Assert nonpositive database and record limits produce `ArgumentOutOfRangeException` through the reflection wrapper.

- [ ] **Step 2: Implement immutable Entry**

```csharp
public sealed class BerkeleyDbTerminalDatabaseEntry {
	private readonly byte[] _data;

	public BerkeleyDbTerminalDatabaseEntry(
		string canonicalName,
		IEnumerable<string> aliases,
		byte[] data
	) {
		ArgumentNullException.ThrowIfNull( canonicalName );
		ArgumentNullException.ThrowIfNull( aliases );
		ArgumentNullException.ThrowIfNull( data );

		string[] aliasSnapshot = aliases.ToArray();
		if ( aliasSnapshot.Any( static alias => alias is null ) ) {
			throw new ArgumentException(
				"The alias sequence cannot contain null.",
				nameof( aliases )
			);
		}

		CanonicalName = canonicalName;
		Aliases = Array.AsReadOnly( aliasSnapshot );
		_data = (byte[])data.Clone();
	}

	public string CanonicalName { get; }
	public IReadOnlyList<string> Aliases { get; }
	public byte[] Data => (byte[])_data.Clone();
}
```

Add complete XML documentation and the repository LGPL header.

- [ ] **Step 3: Implement immutable Options**

```csharp
public sealed class BerkeleyDbTerminalDatabaseWriterOptions {
	public const int DefaultMaximumRecordCount = 65_536;

	public BerkeleyDbTerminalDatabaseWriterOptions(
		CompiledTermInfoParserOptions? parserOptions = null,
		int maximumDatabaseSize =
			BerkeleyDbTerminalDescriptionProviderOptions.DefaultMaximumDatabaseSize,
		int maximumRecordCount = DefaultMaximumRecordCount,
		bool overwriteExisting = false
	) {
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumDatabaseSize );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumRecordCount );

		CompiledTermInfoParserOptions effective =
			parserOptions ?? new CompiledTermInfoParserOptions();
		ParserOptions = new CompiledTermInfoParserOptions(
			effective.MaximumEntrySize
		);
		MaximumDatabaseSize = maximumDatabaseSize;
		MaximumRecordCount = maximumRecordCount;
		OverwriteExisting = overwriteExisting;
	}

	public CompiledTermInfoParserOptions ParserOptions { get; }
	public int MaximumDatabaseSize { get; }
	public int MaximumRecordCount { get; }
	public bool OverwriteExisting { get; }
}
```

- [ ] **Step 4: Add the static writer boundary without stealing HW02**

Implement the exact Write signature, null/path checks, cancellation, option snapshot, and one materialization of entries. Empty input throws `ArgumentException(nameof(entries))`. Nonempty input reaches:

```csharp
throw new NotSupportedException(
	"HW01 freezes writer contracts; Hash-v9 image construction begins in HW02."
);
```

Do not touch the filesystem. `SnapshotOptions` must construct a new Options instance from all four exposed values.

- [ ] **Step 5: Extend the exact Hdb01 export list**

Add the three full type names to the sorted expected array. Do not weaken it to subset matching.

- [ ] **Step 6: Verify GREEN on all target frameworks**

Run the Task 1 command for net10.0, then the same command for net8.0 and net9.0. Expected: all selected tests PASS.

- [ ] **Step 7: Commit and publish GREEN**

```bash
git add Icod.TermInfo.BerkeleyDb/src tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb01ContractTests.cs tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw01WriterContractTests.cs
git commit -m "feat: add HW01 writer public contract"
```

---

### Task 3: HW01 RED Whole-Input Preflight

**Files:**
- Modify: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw01WriterContractTests.cs`

**Interfaces:**
- Consumes: Task 2 API and `Hdb07HashV9FixtureBuilder.CreateCompiledEntry`.
- Produces: executable failing tests for sequence, identity, parser, cancellation, and record bounds.

- [ ] **Step 1: Add a valid entry helper**

```csharp
private static BerkeleyDbTerminalDatabaseEntry CreateEntry(
	string canonical,
	params string[] aliases
) => new(
	canonical,
	aliases,
	Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
		canonical,
		"HW01 fixture",
		aliases
	)
);
```

- [ ] **Step 2: Add exact preflight outcomes**

Use unique paths beneath disposable temporary directories and assert no destination is created:

| Input | Expected |
| --- | --- |
| null element | `ArgumentException(entries)` |
| empty sequence | `ArgumentException(entries)` |
| duplicate canonical | `InvalidOperationException` |
| repeated alias within/across entries | `InvalidOperationException` |
| canonical used as any alias | `InvalidOperationException` |
| `.`, `..`, slash, NUL | `ArgumentException(entries)` |
| `CON`, `sample?`, trailing dot/space on every OS | `ArgumentException(entries)` |
| unpaired surrogate `\uD800` | `ArgumentException(entries)` |
| parsed canonical mismatch | `InvalidOperationException` |
| parsed alias order mismatch | `InvalidOperationException` |
| malformed compiled bytes | `CompiledTermInfoFormatException` |
| one canonical + alias with record limit 2 | `InvalidOperationException` |
| pre-cancelled token | `OperationCanceledException` |
| valid preflight | exact HW01 `NotSupportedException` |

Add an iterator with an incrementing counter and assert it is enumerated exactly once before the valid-boundary exception.

- [ ] **Step 3: Run and verify intentional RED**

```text
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release -f net10.0 --filter FullyQualifiedName~Hw01WriterContractTests
```

Expected: new preflight tests FAIL; reflection and immutable-value tests remain green.

- [ ] **Step 4: Commit and publish RED**

```bash
git add tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw01WriterContractTests.cs
git commit -m "test: define HW01 writer preflight rules"
```

Record the exact head and verify failure is behavioral, not compilation or unrelated regression.

---

### Task 4: HW01 GREEN Bounded Preflight

**Files:**
- Create: `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalDatabaseWriter.Preflight.cs`
- Modify: `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalDatabaseWriter.cs`

**Interfaces:**
- Consumes: Task 2 values, Runtime `CompiledTermInfoParser`, and Task 3 tests.
- Produces: private `PreparedPublication[]` for HW02 without page or Hash construction.

- [ ] **Step 1: Snapshot once with cancellation**

Use an explicit loop, checking cancellation before each element. Reject a null element with `ArgumentException(nameof(entries))`; reject an empty snapshot the same way.

- [ ] **Step 2: Implement deterministic portable identity validation**

Use one `new UTF8Encoding(false, true)`. Reject empty/whitespace, `.`/`..`, control characters, NUL, slash, backslash, `< > : " | ? *`, trailing dot/space, and Windows device stems on every OS. Device stems are `CON`, `PRN`, `AUX`, `NUL`, `CLOCK$`, `COM1`-`COM9`, and `LPT1`-`LPT9`, compared ordinal-ignore-case before the first dot.

Strict `GetBytes` translates `EncoderFallbackException` to `ArgumentException(nameof(entries), innerException)`. Do not use platform-varying `Path.GetInvalidFileNameChars`, `Path.IsPathRooted`, normalization, or culture-sensitive comparison.

Use:

```csharp
private sealed record PreparedIdentity(
	string Name,
	byte[] Utf8
);
```

- [ ] **Step 3: Enforce ownership and physical record limit**

Maintain `Dictionary<string,string>(StringComparer.Ordinal)`. Add canonical then aliases in supplied order; failed `TryAdd` throws `InvalidOperationException`.

Compute:

```csharp
recordCount = checked(
	recordCount + 2 + entry.Aliases.Count
);
if ( recordCount > options.MaximumRecordCount ) {
	throw new InvalidOperationException(
		"The terminal publications exceed the configured Hash record limit."
	);
}
```

Translate `OverflowException` to `InvalidOperationException` with the original inner exception.

- [ ] **Step 4: Parse and require exact identity agreement**

Clone `entry.Data`, call `CompiledTermInfoParser.Parse(data, options.ParserOptions)`, and compare canonical plus ordered aliases with `StringComparison.Ordinal`. Any count or value difference throws `InvalidOperationException`; parser exceptions propagate unchanged.

Store:

```csharp
private sealed record PreparedPublication(
	PreparedIdentity Canonical,
	IReadOnlyList<PreparedIdentity> Aliases,
	byte[] Data
);
```

- [ ] **Step 5: Wire Write and retain the HW01 boundary**

After argument checks, compute `Path.GetFullPath(databasePath)`, snapshot Options, snapshot entries, prepare publications, check cancellation, then throw the exact HW01 `NotSupportedException`. Do not open, create, delete, move, or inspect a destination.

- [ ] **Step 6: Run the complete BerkeleyDb unit suite**

```text
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release -f net8.0
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release -f net9.0
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release -f net10.0
```

Expected: all tests PASS with zero Release warnings.

- [ ] **Step 7: Commit and publish GREEN**

```bash
git add Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalDatabaseWriter.cs Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalDatabaseWriter.Preflight.cs
git commit -m "feat: validate HW01 terminal publications"
```

---

### Task 5: Freeze API Tooling and Accept HW01

**Files:**
- Modify: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw01WriterContractTests.cs`
- Modify: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb09ReleaseClosureTests.cs`
- Create: `docs/1.16.0-BERKELEYDB-PUBLIC-API-BASELINE.txt`
- Create: `docs/1.16.0-HW01-BERKELEY-DB-WRITER-CONTRACT.md`
- Modify: `.github/scripts/verify-berkeleydb-package.ps1`
- Modify: `Icod.TermInfo-1.16.0-Berkeley-DB-Hash-V9-Writer-Roadmap.md`
- Modify: `Icod.TermInfo-Post-1.0-Development-Roadmap.md`

**Interfaces:**
- Consumes: Task 4 green assemblies and exact-head GitHub Actions.
- Produces: additive 1.16 manifest, package verification, acceptance record, and HW02 next gate.

- [ ] **Step 1: Add failing baseline wiring assertions**

Assert the 1.16 baseline exists and `.github/scripts/verify-berkeleydb-package.ps1` contains `docs/1.16.0-BERKELEYDB-PUBLIC-API-BASELINE.txt`. Expected RED: file absent and verifier still names 1.15.

- [ ] **Step 2: Generate and review the complete baseline**

```text
dotnet run --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj -c Release --no-build -- --write docs/1.16.0-BERKELEYDB-PUBLIC-API-BASELINE.txt Icod.TermInfo.BerkeleyDb/bin/Release/net10.0/Icod.TermInfo.BerkeleyDb.dll
```

The diff must equal the 1.15 manifest plus exactly three type blocks. Existing nine blocks, assembly version, nullability, defaults, and member shapes must remain unchanged.

- [ ] **Step 3: Wire current package verification**

Point the PowerShell verifier and `Hdb09ReleaseClosureTests.ApiBaselinePath` at 1.16. Point freeze evidence at `docs/1.16.0-HW01-BERKELEY-DB-WRITER-CONTRACT.md`. Keep both cross-framework `--compare` calls and all dependency/archive/package-count assertions.

Create that contract document in candidate state before running tests. It must
contain the generated normalized-LF baseline SHA-256 and exported type count,
and state that exact-head workflow acceptance has not yet been recorded. Do not
include empty fields or symbolic values. Step 6 replaces the candidate statement
with literal acceptance evidence after the code/API workflow succeeds.

- [ ] **Step 4: Run full API/package verification**

```text
dotnet build Icod.TermInfo.sln -c Release
dotnet run --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj -c Release --no-build -- --check docs/1.16.0-BERKELEYDB-PUBLIC-API-BASELINE.txt Icod.TermInfo.BerkeleyDb/bin/Release/net10.0/Icod.TermInfo.BerkeleyDb.dll
dotnet run --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj -c Release --no-build -- --compare Icod.TermInfo.BerkeleyDb/bin/Release/net8.0/Icod.TermInfo.BerkeleyDb.dll Icod.TermInfo.BerkeleyDb/bin/Release/net9.0/Icod.TermInfo.BerkeleyDb.dll
dotnet run --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj -c Release --no-build -- --compare Icod.TermInfo.BerkeleyDb/bin/Release/net8.0/Icod.TermInfo.BerkeleyDb.dll Icod.TermInfo.BerkeleyDb/bin/Release/net10.0/Icod.TermInfo.BerkeleyDb.dll
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release --no-build
```

Expected: build, baseline, comparisons, and tests all succeed.

- [ ] **Step 5: Publish code/API checkpoint and require exact-head CI**

```bash
git add docs/1.16.0-BERKELEYDB-PUBLIC-API-BASELINE.txt docs/1.16.0-HW01-BERKELEY-DB-WRITER-CONTRACT.md .github/scripts/verify-berkeleydb-package.ps1 tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb09ReleaseClosureTests.cs tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hw01WriterContractTests.cs
git commit -m "test: freeze HW01 BerkeleyDb public API"
```

Wait for the exact-head pull-request workflow because this is code/package acceptance. Require all jobs green and correct any failure before acceptance.

- [ ] **Step 6: Record evidence and advance roadmaps**

The acceptance document records the literal SHA returned for the green code/API
commit and the literal number and URL of its successful pull-request workflow.
It also records `HW01 result: COMPLETE / ACCEPTED`, coordinated version
`1.16.0-Alpha-1`, the exact three-type public addition, normalized-LF baseline
SHA-256, exported type count, and the fact that deterministic pages and inline
records begin in HW02. Do not leave symbolic tokens in the committed record.

Mark the 1.16 roadmap `ACTIVE — HW01 COMPLETE / HW02 NEXT` and advance the main
roadmap next gate to HW02.

- [ ] **Step 7: Publish documentation and continue without waiting**

Run `git diff --check` and scan the three changed documentation files for
unfinished placeholder markers; expected output is empty. Commit with
`docs: accept HW01 writer contract`. Publish and continue to HW02 without
waiting on the documentation-only job.

---

## Execution Handoff

Execute inline in this session with `superpowers:executing-plans`. The user has already selected inline development, and this environment forbids proactive subagent dispatch. Stop only for explicit RED evidence, unexpected CI failure, or a decision that would change the approved public API.
