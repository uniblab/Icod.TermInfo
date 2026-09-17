# HDB07 Adversarial and Compatibility Hardening Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Harden the accepted pure-managed Berkeley DB Hash-v9 acquisition subset with deterministic adversarial, lifecycle, command, and native-valid evidence while preserving every accepted public and dependency boundary.

**Architecture:** A test-owned Hash-v9 fixture builder supplies reviewed deterministic valid and malformed images. Independent RED→GREEN checkpoints harden storage reconstruction before characterizing ncurses semantics, lifecycle, commands, and native stores; production changes are limited to defects proven by committed REDs. HDB00 keeps its three qualification jobs but adds a synchronize-delta gate so documentation-only updates skip native setup and execution.

**Tech Stack:** C# 13, .NET 8/9/10 reusable libraries, .NET 10 command/probe projects, xUnit 2.9.2, PowerShell 7, Bash, GitHub Actions, native ncurses at pinned commit `87c2c84cbd2332d6d94b12a1dcaf12ad1a51a938`, Berkeley DB 5.3.

**Spec:** `docs/superpowers/specs/2026-09-16-hdb07-adversarial-compatibility-hardening-design.md`

## Global Constraints

- Advance the coordinated suite version to exactly `1.15.0-Alpha-7`.
- Keep every reusable assembly identity at `1.0.0.0`.
- Do not add or change Runtime, Source, Compiler, Termcap, Inspection, or BerkeleyDb public API.
- Runtime and Inspection must not reference BerkeleyDb; BerkeleyDb continues to reference Runtime only.
- Production remains pure managed, read-only, and free of P/Invoke, native assets, runtime downloads, or third-party Berkeley DB packages.
- Preserve the accepted Hash-v9 subset; do not add other access methods, revisions, checksums, encryption, transactions, recovery, cursors, or writes.
- Preserve all accepted exception, diagnostic, ordering, caching, retry, cancellation, JSON, discovery, and command-status boundaries.
- Exact lookup may stop on a match and need not validate unrelated unvisited pages or values.
- Do not claim an atomic snapshot against same-length concurrent external writes.
- Native big-endian, broader non-ASCII producer compatibility, and stronger concurrent-writer guarantees remain deferred to the separately reviewed compatibility-expansion tranche.
- Tests and fixtures must be deterministic. Sleeps, timing races, randomized CI gates, and silently skipped permission evidence are prohibited.
- PR #45 remains open, draft, and unmerged.

---

## Task 1: Alpha-7 baseline and change-sensitive HDB00 gate

**Files:**
- Modify: `Directory.Build.props`
- Create: `tools/hdb00/Get-Hdb00ChangeScope.ps1`
- Create: `tools/hdb00/verify-change-scope.ps1`
- Modify: `.github/workflows/hdb00-interoperability.yml`

**Interfaces:**
- Consumes: GitHub `pull_request` event fields `action`, `before`, and `after`, or `workflow_dispatch`.
- Produces: `Get-Hdb00ChangeScope.ps1`, which writes exactly `true` or `false`; Linux job output `hdb00-required`; coordinated suite version `1.15.0-Alpha-7`.

- [x] **Step 1: write deterministic scope-classifier tests**

Create `verify-change-scope.ps1` with literal sensitive and insensitive paths:

```powershell
$cases = @(
    @{ Paths = @('Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashReader.cs'); Expected = 'true' },
    @{ Paths = @('tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/src/NativeOracleTests.cs'); Expected = 'true' },
    @{ Paths = @('tools/hdb00/run-linux.sh'); Expected = 'true' },
    @{ Paths = @('.github/workflows/hdb00-interoperability.yml'); Expected = 'true' },
    @{ Paths = @('Directory.Build.props'); Expected = 'true' },
    @{ Paths = @('tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb07StorageHardeningTests.cs'); Expected = 'false' },
    @{ Paths = @('docs/superpowers/plans/2026-09-16-hdb07-adversarial-compatibility-hardening.md'); Expected = 'false' },
    @{ Paths = @('README.md'); Expected = 'false' },
    @{ Paths = @('Icod.TermInfo.BerkeleyDb/README.md'); Expected = 'false' },
    @{ Paths = @('infocmp/README.md'); Expected = 'false' }
)

foreach ($case in $cases) {
    $actual = & "$PSScriptRoot/Get-Hdb00ChangeScope.ps1" -ChangedPaths $case.Paths
    if ($actual -cne $case.Expected) {
        throw "Expected '$($case.Expected)' for '$($case.Paths -join ',')', got '$actual'."
    }
}
```

Also assert that a mixed documentation/production list returns `true`, an empty explicit path list returns `false`, `workflow_dispatch` returns `true`, and a non-`synchronize` pull-request action returns `true`.

- [x] **Step 2: run the scope tests to verify RED**

Run:

```text
pwsh -NoProfile -File tools/hdb00/verify-change-scope.ps1
```

Expected: FAIL because `Get-Hdb00ChangeScope.ps1` does not exist.

- [x] **Step 3: implement the scope classifier**

The script accepts either explicit test paths or event commits:

```powershell
param(
    [string] $EventName,
    [string] $Action,
    [string] $Before,
    [string] $After,
    [string] $RepositoryRoot = (Get-Location).Path,
    [string[]] $ChangedPaths
)

$patterns = @(
    '^\.github/workflows/hdb00-interoperability\.yml$',
    '^tools/hdb00/',
    '^tools/hdb07-permission-probe/',
    '^\.github/scripts/verify-hdb07-permissions\.ps1$',
    '^Icod\.TermInfo\.BerkeleyDb/(src/|Icod\.TermInfo\.BerkeleyDb\.csproj$)',
    '^tests/Icod\.TermInfo\.BerkeleyDb\.Interop\.Tests/',
    '^(infocmp|toe|icod-terminfo)/(src/|[^/]+\.csproj$)',
    '^\.github/scripts/(new-hdb06-test-store|smoke-tool-package|smoke-tool-archive)\.ps1$',
    '^Directory\.Build\.(props|targets)$',
    '^Icod\.TermInfo/(src/|Icod\.TermInfo\.csproj$)'
)
```

Normalize separators to `/` and use ordinal-ignore-case regex matching. When
`-ChangedPaths` is present, classify only that array. For `workflow_dispatch`
or a pull-request action other than `synchronize`, emit `true`. For
`synchronize`, require nonempty `Before` and `After`, run:

```text
git -C <RepositoryRoot> diff --name-only --diff-filter=ACMR <Before> <After>
```

Fail on a nonzero Git exit; never convert an invalid range into `false`.

- [x] **Step 4: wire the gate into all three HDB00 jobs**

Set checkout `fetch-depth: 0`. Immediately after checkout, call the classifier
with the event values and write `required=<true|false>` to
`$env:GITHUB_OUTPUT`.

The Linux job exposes:

```yaml
outputs:
  hdb00-required: ${{ steps.hdb00-scope.outputs.required }}
```

Add `if: steps.hdb00-scope.outputs.required == 'true'` to every expensive
Linux/macOS setup, native-build, test, command, and artifact step. Keep checkout,
classifier verification on Linux, and scope calculation unconditional.

Keep the Windows dependency on Linux and add:

```yaml
if: needs.linux-berkeley-db-5-3.outputs.hdb00-required == 'true'
```

A sensitive synchronization must still execute exactly the existing three
qualification jobs. A documentation-only synchronization runs the two cheap
gate jobs and skips Windows.

- [x] **Step 5: advance the coordinated version**

Change only the active property:

```xml
<IcodTermInfoSuiteVersion>1.15.0-Alpha-7</IcodTermInfoSuiteVersion>
```

Do not rewrite historical Alpha-1 through Alpha-6 documentation.

- [x] **Step 6: verify and commit the baseline**

Run:

```text
pwsh -NoProfile -File tools/hdb00/verify-change-scope.ps1
dotnet build Icod.TermInfo.sln -c Release
```

Expected: classifier cases pass and the solution builds with zero warnings and
errors. Push, require the normal workflow to pass 12/12, and require this
workflow-changing head to pass all three full HDB00 jobs.

Commit:

```text
ci: gate HDB00 by synchronization delta
```

---

## Task 2: Deterministic HDB07 fixture builder

**Files:**
- Create: `tests/Shared/Hdb07HashV9FixtureBuilder.cs`
- Modify: `tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj`
- Create: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb07FixtureBuilderTests.cs`

**Interfaces:**
- Consumes: literal Berkeley DB Hash-v9 constants and Runtime compiled-entry layout; no production serializer.
- Produces: `Hdb07HashV9FixtureBuilder.CreateDatabase`, `Hdb07ItemSpec.Inline`, `Hdb07ItemSpec.OffPage`, `Hdb07RecordSpec`, and compiled ncurses record helpers used by Tasks 3–7.

- [x] **Step 1: link the absent builder and write compile-RED tests**

Add:

```xml
<Compile Include="..\Shared\Hdb07HashV9FixtureBuilder.cs"
         Link="Shared\Hdb07HashV9FixtureBuilder.cs" />
```

Write tests which reference this exact test-only interface:

```csharp
internal enum Hdb07ByteOrder { LittleEndian, BigEndian }

internal readonly record struct Hdb07ItemSpec {
    internal static Hdb07ItemSpec Inline( byte[] payload );
    internal static Hdb07ItemSpec OffPage(
        byte[] payload,
        int[] chunkLengths,
        int headerTrailingByteCount = 0,
        bool appendEmptyOverflowPage = false
    );
}

internal readonly record struct Hdb07RecordSpec(
    Hdb07ItemSpec Key,
    Hdb07ItemSpec Value
);

internal static class Hdb07HashV9FixtureBuilder {
    internal static byte[] CreateDatabase(
        Hdb07ByteOrder byteOrder,
        int pageSize,
        params Hdb07RecordSpec[] records
    );

    internal static byte[] CreateCompiledEntry(
        string canonical,
        string description,
        params string[] aliases
    );

    internal static byte[] NcursesData( byte[] compiledEntry );
    internal static byte[] NcursesIndex( byte[] targetKey );
}
```

Tests assert exact magic/version/page-size bytes for both byte orders, every page
identity, declared last-page number, exact inline bytes, exact off-page
first-page/length fields, and requested overflow chunk lengths.

- [x] **Step 2: run the builder tests to verify compile RED**

Run:

```text
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release --filter FullyQualifiedName~Hdb07FixtureBuilderTests
```

Expected: FAIL to compile because the HDB07 builder types do not exist.

- [x] **Step 3: implement the independent builder**

Use `BinaryPrimitives` directly and literal values:

```csharp
private const uint HashMagic = 0x00061561;
private const uint HashVersion = 9;
private const byte MetadataPage = 8;
private const byte OverflowPage = 7;
private const byte HashPage = 13;
private const byte InlineItem = 1;
private const byte OffPageItem = 3;
private const int PageHeaderSize = 26;
```

The builder allocates a complete byte array, writes one record pair per hash
page for readable geometry, allocates overflow pages after hash pages, supports
512 through 65536-byte power-of-two pages, and writes a 12-byte off-page header
plus only the explicitly requested trailing bytes. It must not call
`BerkeleyDbHashReader`, `NcursesRecordReader`, or another production parser
while building.

`CreateCompiledEntry` writes only the minimal little-endian compiled terminfo
header and names section already accepted by Runtime.

- [x] **Step 4: verify literal and production round trips**

Run the filtered builder tests, then:

```text
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release
```

Expected: builder tests and all existing tests pass on net8/net9/net10.

- [x] **Step 5: commit the fixture foundation**

Commit:

```text
test: add deterministic HDB07 Hash-v9 fixtures
```

This unit-test-only push should execute the normal workflow and only the cheap
HDB00 synchronize-delta gate.

---

## Task 3: Off-page and overflow trailing-data RED→GREEN

**Files:**
- Create: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb07OverflowHardeningTests.cs`
- Modify: `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashReader.cs`

**Interfaces:**
- Consumes: the HDB07 fixture builder and internal reader.
- Produces: exact rejection of non-12-byte off-page headers and overflow chains which continue after the declared item length.

- [x] **Step 1: write the behavioral REDs**

Add both byte orders for:

```csharp
[Theory]
[InlineData( Hdb07ByteOrder.LittleEndian )]
[InlineData( Hdb07ByteOrder.BigEndian )]
public void ReadRecordsRejectsOffPageHeaderTrailingBytes(
    Hdb07ByteOrder byteOrder
) {
    byte[] database = Hdb07HashV9FixtureBuilder.CreateDatabase(
        byteOrder,
        512,
        new(
            Hdb07ItemSpec.OffPage(
                Encoding.UTF8.GetBytes( "key" ),
                [ 3 ],
                headerTrailingByteCount: 1
            ),
            Hdb07ItemSpec.Inline( [ 0x00 ] )
        )
    );

    InvalidDataException error = Assert.Throws<InvalidDataException>(
        () => BerkeleyDbHashReader.ReadRecords(
            database, 1024, 8, CancellationToken.None
        )
    );
    Assert.Equal(
        "A Berkeley DB off-page item must contain exactly its 12-byte header.",
        error.Message
    );
}
```

Add `TryReadValueRejectsOverflowPagesAfterDeclaredLength` with
`appendEmptyOverflowPage: true`. Assert the exact message
`The Berkeley DB overflow chain continues after its declared length.` Also add a
zero-length off-page item with page zero as an already-green boundary and a
zero-length item with an appended page as the same RED family.

- [x] **Step 2: run and record the exact RED**

Run:

```text
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release --filter FullyQualifiedName~Hdb07OverflowHardeningTests
```

Expected: the valid empty boundary passes; trailing off-page headers and
continued-overflow cases fail because the current reader accepts them. Commit
the tests alone and record exact per-TFM failure counts.

Commit:

```text
test: define HDB07 trailing-data rejection
```

- [x] **Step 3: require an exact off-page header**

In `ReadHashItem`:

```csharp
if ( itemLength != 12 ) {
    throw new InvalidDataException(
        "A Berkeley DB off-page item must contain exactly its 12-byte header."
    );
}
```

- [x] **Step 4: reject overflow continuation after completion**

After copying the chunk:

```csharp
uint nextPageNumber = ReadUInt32(
    page,
    16,
    metadata.IsBigEndian
);
if ( written == result.Length && nextPageNumber != 0 ) {
    throw new InvalidDataException(
        "The Berkeley DB overflow chain continues after its declared length."
    );
}
pageNumber = nextPageNumber;
```

Preserve cycle, page-identity, maximum-item, overrun, and undersupply failures.

- [x] **Step 5: verify GREEN and regressions**

Run:

```text
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release
dotnet test tests/Icod.TermInfo.InfoCmp.Tests/Icod.TermInfo.InfoCmp.Tests.csproj -c Release
dotnet test tests/Icod.TermInfo.Toe.Tests/Icod.TermInfo.Toe.Tests.csproj -c Release
```

Expected: all configured TFMs pass. Push and require normal 12/12 plus full
HDB00 3/3 because production BerkeleyDb code changed.

Commit:

```text
fix: reject trailing Hash-v9 storage data
```

---

## Task 4: Complete storage characterization matrix

**Files:**
- Create: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb07StorageHardeningTests.cs`

**Interfaces:**
- Consumes: the HDB07 builder and accepted internal reader.
- Produces: deterministic characterization of remaining metadata, geometry, arithmetic, ordering, ownership, and concurrency requirements.

- [x] **Step 1: add table-driven storage cases**

Create literal cases for:

- minimum and maximum page sizes in both byte orders;
- last-page exact boundary and one-page-beyond failure;
- index-table end equal to free offset;
- duplicate offsets, ascending offsets, offset into the index table, and odd item count;
- missing, wrong-type, wrong-identity, cyclic, premature, and overlong overflow pages;
- valid shared overflow tails referenced by two records;
- duplicate exact binary keys on separate hash pages;
- exact database/item/record limits and one unit beyond;
- cancellation before metadata and between records;
- file release after success, miss, corruption, and cancellation; and
- concurrent reads using different limits.

Use named `MemberData` cases and assert the existing exception family and
deterministic winning mutation:

```csharp
[Theory]
[MemberData( nameof( InvalidGeometryCases ) )]
public void ReadRecordsRejectsInvalidGeometryInTraversalOrder(
    Action<byte[]> mutate,
    string expectedMessage
) {
    byte[] database = CreateTwoRecordFixture();
    mutate( database );

    InvalidDataException error = Assert.Throws<InvalidDataException>(
        () => BerkeleyDbHashReader.ReadRecords(
            database,
            4096,
            8,
            CancellationToken.None
        )
    );
    Assert.Equal( expectedMessage, error.Message );
}
```

- [x] **Step 2: add exact-lookup scope characterization**

Place a matching record before an unrelated malformed value and assert lookup
succeeds. Place the malformed key before the match and assert failure. This
freezes the accepted rule that exact lookup stops on a match and does not
validate unrelated later values.

- [x] **Step 3: run as characterization**

Run the complete BerkeleyDb test project in Release.

Expected: all new cases pass. If a case fails, stop with the failing test
committed alone, invoke systematic debugging, and add a dedicated RED→GREEN
task to this plan before editing production. Do not weaken the fixture or
change failure precedence.

- [x] **Step 4: verify cross-host CI and commit**

Push the already-green characterization. Require normal 12/12. The HDB00
synchronize-delta gate should skip expensive work because only unit-test code
changed.

Commit:

```text
test: characterize HDB07 storage boundaries
```

---

## Task 5: Ncurses, identity, and culture characterization

**Files:**
- Create: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb07CultureCollection.cs`
- Create: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb07LogicalHardeningTests.cs`

**Interfaces:**
- Consumes: public provider/catalog APIs, the HDB07 builder, Runtime parser, and ordinal accepted semantics.
- Produces: deterministic marker, wrong-key, parser, publication, and culture evidence with no global test leakage.

- [x] **Step 1: isolate culture-changing tests**

Create:

```csharp
[CollectionDefinition( Name, DisableParallelization = true )]
public sealed class Hdb07CultureCollection {
    public const string Name = "HDB07 culture-sensitive process state";
}
```

The logical test class uses
`[Collection( Hdb07CultureCollection.Name )]`. Add a disposable culture scope:

```csharp
private sealed class CultureScope : IDisposable {
    private readonly CultureInfo _culture = CultureInfo.CurrentCulture;
    private readonly CultureInfo _uiCulture = CultureInfo.CurrentUICulture;

    internal CultureScope( string name ) {
        CultureInfo selected = CultureInfo.GetCultureInfo( name );
        CultureInfo.CurrentCulture = selected;
        CultureInfo.CurrentUICulture = selected;
    }

    public void Dispose() {
        CultureInfo.CurrentCulture = _culture;
        CultureInfo.CurrentUICulture = _uiCulture;
    }
}
```

- [x] **Step 2: add logical matrix tests**

Add deterministic tests for marker-0 direct records; marker-2 chains at the
inclusive limit; empty/unsupported markers; dangling targets; repeated-key and
multi-key cycles; embedded-NUL targets; exceeded limits; invalid UTF-8; unsafe
publication names; surrogate lookup input; valid compiled entries declaring
canonical, exact alias, neither, or a different otherwise-valid identity;
parser size boundaries; malformed orphan marker-0 records; shared parsed
identity; and ordinal publication ordering under `tr-TR`, `ar-SA`, and `ja-JP`.

Use literal expected order:

```csharp
using var culture = new CultureScope( cultureName );
IReadOnlyList<BerkeleyDbTerminalCatalogEntry> entries = reader.Read();

Assert.Equal(
    new[] { "I", "i", "\u0130", "\u0131" },
    entries.Select( entry => entry.Name ).ToArray()
);
```

These are strict UTF-8 synthetic keys only; they do not claim native non-ASCII
producer interoperability.

- [x] **Step 3: run as characterization**

Run:

```text
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release --filter FullyQualifiedName~Hdb07LogicalHardeningTests
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release
```

Expected: all cases pass and both culture properties are restored. If a case
fails, stop with the test-only RED and add a narrow corrective task before any
production edit.

- [x] **Step 4: commit logical evidence**

Commit:

```text
test: characterize HDB07 logical boundaries
```

Require normal 12/12; HDB00 expensive steps should be gated off.

---


## Task 6: Characterize lifecycle behavior and prove permission propagation

**Files:**

- Create: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb07LifecycleHardeningTests.cs`
- Create: `tools/hdb07-permission-probe/Hdb07.PermissionProbe.csproj`
- Create: `tools/hdb07-permission-probe/Program.cs`
- Create: `.github/scripts/verify-hdb07-permissions.ps1`
- Modify: `.github/workflows/hdb00-interoperability.yml`

**Interfaces under test:** `BerkeleyDbTerminalDescriptionProvider`,
`BerkeleyDbTerminalCatalogReader`, and the accepted HDB00 `hashed-db` fixture.
The permission probe is test infrastructure only and returns success exclusively
when the public acquisition path propagates `UnauthorizedAccessException`.

- [x] **Step 1: add lifecycle characterization cases**

Add tests covering:

- retry after missing database, sharing violation, malformed storage,
  malformed ncurses envelope, parser failure, and identity mismatch;
- completed-call caching after success;
- a fresh catalog result per call;
- replacement of the database between separate provider instances;
- system-provider precedence, cache, retry, and cancellation behavior;
- the accepted concurrency limits, cancellation boundaries, and handle-release
  behavior.

Use a fresh temporary root for each case. Assert only completed-call semantics:
HDB07 does not claim a stable read across arbitrary concurrent replacement of an
open database.

Run:

```text
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release --filter FullyQualifiedName~Hdb07LifecycleHardeningTests
```

Expected: all characterization cases pass. If a case fails, preserve that
test-only RED and add a dedicated corrective task before editing production.

- [x] **Step 2: add a framework-independent permission probe**

Create a `net10.0` console application with a direct project reference to
`Icod.TermInfo.BerkeleyDb`. Keep its contract small:

```csharp
try
{
    _ = new BerkeleyDbTerminalDescriptionProvider( databasePath ).Get( terminalName );
    Console.Error.WriteLine( "The protected database was unexpectedly readable." );
    return 10;
}
catch ( UnauthorizedAccessException )
{
    return 0;
}
catch ( Exception exception )
{
    Console.Error.WriteLine( exception );
    return 11;
}
```

Return `64` for invalid arguments. Do not add the probe to the shipped solution
or package graph.

- [x] **Step 3: add the cross-platform permission harness**

In `verify-hdb07-permissions.ps1`:

1. prove the native `hashed-db` fixture is readable before changing permissions;
2. on Unix, capture the existing `UnixFileMode`, apply `UnixFileMode.None`, invoke
   the probe, and restore the captured mode in `finally`;
3. on Windows, capture the ACL, use `icacls` to disable inheritance and deny the
   current identity read access, invoke the probe, then remove the deny rule and
   restore inheritance/ACL state in `finally`;
4. prove the database is readable again after restoration;
5. print exactly `HDB07 permission propagation passed.`.

Any inability to establish the denial is a failure, not a skip.

- [x] **Step 4: wire the harness into all three HDB00 jobs**

Add a gated permission step after native fixture generation and before managed
verification in Linux, macOS, and Windows:

```yaml
- name: Verify HDB07 permission propagation
  if: steps.scope.outputs.hdb00-required == 'true'
  shell: pwsh
  run: ./.github/scripts/verify-hdb07-permissions.ps1
```

Use the equivalent Linux output reference already established in Task 1 for
macOS and Windows job-level gating. Do not create another workflow.

- [x] **Step 5: verify locally and in CI**

Run:

```text
dotnet build tools/hdb07-permission-probe/Hdb07.PermissionProbe.csproj -c Release
dotnet test Icod.TermInfo.sln -c Release
```

Push the test-only commit and require:

- normal PR workflow: 12/12;
- HDB00: Linux, macOS, and Windows all green;
- each HDB00 permission step contains
  `HDB07 permission propagation passed.`;
- each job's post-step readability check passes.

If any host cannot produce a real access denial, stop and retain the evidence;
do not replace the test with a mocked exception.

- [x] **Step 6: commit lifecycle and permission evidence**

Commit:

```text
test: harden HDB07 lifecycle and permissions
```

---


## Task 7: Characterize command and router failure boundaries

**Files:**

- Modify: `tests/Icod.TermInfo.InfoCmp.Tests/Icod.TermInfo.InfoCmp.Tests.csproj`
- Create: `tests/Icod.TermInfo.InfoCmp.Tests/src/Hdb07FailureBoundaryTests.cs`
- Modify: `tests/Icod.TermInfo.Toe.Tests/Icod.TermInfo.Toe.Tests.csproj`
- Create: `tests/Icod.TermInfo.Toe.Tests/src/Hdb07FailureBoundaryTests.cs`
- Modify: `tests/Icod.TermInfo.Tests/Icod.TermInfo.Tests.csproj`
- Create: `tests/Icod.TermInfo.Tests/src/Hdb07RouterFailureBoundaryTests.cs`
- Link: `tests/Shared/Hdb07HashV9FixtureBuilder.cs` into all three test
  projects

**Interfaces under test:** existing InfoCmp and Toe command entry points, direct
Berkeley DB providers, and the `TerminalDescriptionProvider` router. No new
production seam is permitted.

- [x] **Step 1: characterize InfoCmp failures**

Create deterministic cases proving:

- a database with the requested key and trailing off-page or overflow data
  returns status `1`, writes no standard output, emits `INFOCMP0003`, and does
  not leak page numbers, implementation type names, or a stack trace;
- a clean exact-key miss remains `INFOCMP0002`;
- cancellation remains status `130`;
- when every candidate file is rejected, the final diagnostic is stable;
- text and JSON output for accepted successful inputs are byte-for-byte
  unchanged.

Use literal diagnostic codes and assert stderr structure instead of local OS
exception prose.

- [x] **Step 2: characterize Toe isolation and ordering**

Create a root sequence containing valid, corrupt, and valid databases. Assert:

- entries from the earlier and later valid roots are emitted;
- no entry from the failed root is emitted;
- `TOE0005` appears exactly once and the command returns status `1`;
- accepted root and entry ordering is unchanged;
- `-h`, `-s`, and culture changes do not change isolation semantics;
- JSON output and ambient-root behavior remain unchanged.

- [x] **Step 3: characterize direct and routed providers**

For both direct and routed acquisition, cover valid, missing, malformed, and
mixed-directory cases. Assert the same accepted result, miss, or public
exception category as the corresponding direct provider call, with no
Berkeley-DB-specific public API added to the router.

- [x] **Step 4: run the focused and full suites**

Run:

```text
dotnet test tests/Icod.TermInfo.InfoCmp.Tests/Icod.TermInfo.InfoCmp.Tests.csproj -c Release --filter FullyQualifiedName~Hdb07FailureBoundaryTests
dotnet test tests/Icod.TermInfo.Toe.Tests/Icod.TermInfo.Toe.Tests.csproj -c Release --filter FullyQualifiedName~Hdb07FailureBoundaryTests
dotnet test tests/Icod.TermInfo.Tests/Icod.TermInfo.Tests.csproj -c Release --filter FullyQualifiedName~Hdb07RouterFailureBoundaryTests
dotnet test Icod.TermInfo.sln -c Release
```

Expected: all cases pass after Task 3. If any case fails, stop with the smallest
reproducer and add a corrective RED→GREEN task before changing production.

- [x] **Step 5: commit command-boundary evidence**

Commit:

```text
test: characterize HDB07 command failures
```

Require normal 12/12; HDB00 expensive steps should be gated off because this
commit changes only test projects and shared test fixtures.

---

## Task 8: Expand the native-valid differential matrix

**Files:**

- Modify: `tools/hdb00/run-linux.sh`
- Modify: `tools/hdb00/run-macos.sh`
- Modify: `tools/hdb00/verify-managed.sh`
- Modify: `.github/workflows/hdb00-interoperability.yml`
- Modify: `tests/Icod.TermInfo.BerkeleyDb.Interop.Tests/src/NativeOracleTests.cs`

**Interfaces under test:** the existing HDB00 native generator and dump oracle,
managed exact lookup/catalog paths, InfoCmp, Toe, router, and packed-package
smoke paths.

- [x] **Step 1: generate the deterministic multi-record native fixture**

Extend both native scripts to compile 64 entries named
`hdb07-multi-000` through `hdb07-multi-063`. Give each entry one distinct alias,
insert all compiled data records into one Hash-v9 database, and add both the
primary and alias ncurses index records.

The result is exactly:

- 64 compiled data records;
- 128 marker-2 index records;
- 192 native database records total.

Produce `multi-hashed-db` plus its native `db_dump` oracle. Keep byte-order
selection native to each host.

- [x] **Step 2: extend managed-oracle assertions**

Add literal count and edge-position assertions:

```csharp
[InlineData( "multi-hashed-db", 192 )]
public void Native_database_matches_dump_record_count(
    string fixtureName,
    int expectedRecordCount )
```

Assert 128 catalog publications and exercise primary/alias lookup at the first,
middle, and last generated entries through:

- direct provider;
- explicit router;
- system router;
- InfoCmp;
- Toe.

Compare exact key bytes and opaque value bytes with the native dump before
parsing terminal descriptions.

- [x] **Step 3: preserve artifact transport and Windows verification**

Publish the new database and dump in the Linux artifact without renaming the
accepted fixtures. Download it in the Windows HDB00 job and run the same managed
verification there. Windows is a consumer of the Linux-native fixture, not a
claim that Windows generated Berkeley DB output.

- [x] **Step 4: run local structural checks**

Run:

```text
bash -n tools/hdb00/run-linux.sh
bash -n tools/hdb00/run-macos.sh
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release --filter FullyQualifiedName~NativeOracleTests
dotnet test Icod.TermInfo.sln -c Release
```

Expected: shell syntax passes. The native-oracle test may require CI fixtures;
when they are absent locally it must use the existing explicit fixture guard,
not silently lower its assertions.

- [x] **Step 5: push and qualify the expanded native matrix**

Require:

- normal PR workflow: 12/12;
- HDB00: Linux, macOS, and Windows all green;
- Linux and macOS logs report 192 native records and 128 catalog publications;
- Windows reports the same counts against the transported Linux artifact;
- package/archive smoke tests continue to pass.

If a differential fails, minimize it into one deterministic fixture-builder
case before considering production changes.

- [x] **Step 6: commit native-valid evidence**

Commit:

```text
test: expand HDB07 native Hash-v9 matrix
```

---

## Task 9: Qualify and close HDB07

**Files:**

- Create: `docs/1.15.0-HDB07-ADVERSARIAL-COMPATIBILITY-HARDENING.md`
- Modify: `Icod.TermInfo-1.15.0-Berkeley-DB-Hashed-Terminfo-Acquisition-Roadmap.md`
- Modify: `README.md`
- Modify: `Icod.TermInfo.BerkeleyDb/README.md`
- Modify: `infocmp/README.md`
- Modify: `toe/README.md`
- Modify: `docs/superpowers/plans/2026-09-16-hdb07-adversarial-compatibility-hardening.md`
- Update: PR #45 description

- [x] **Step 1: run final implementation qualification**

At the exact implementation head, run or require:

```text
dotnet test Icod.TermInfo.sln -c Release
```

Then require one normal PR workflow with all 12 jobs green and one full HDB00
workflow with Linux, macOS, and Windows green. Record exact commit and run IDs.

- [x] **Step 2: inspect qualification logs and artifacts**

Record and verify:

- per-framework BerkeleyDb, InfoCmp, Toe, router, and interop test counts;
- permission-denial and restoration evidence on all three hosts;
- native `multi-hashed-db` record/publication counts on all three hosts;
- Linux/macOS native-generation provenance and Windows artifact provenance;
- public API and package dependency diffs against the HDB06 baseline;
- package smoke results for every produced archive.

No count or artifact may be inferred only from a green job badge.

- [x] **Step 3: write the closure record**

In `docs/1.15.0-HDB07-ADVERSARIAL-COMPATIBILITY-HARDENING.md`, record:

- scope and non-goals;
- the Task 3 RED commits and exact failure messages;
- the production GREEN commit;
- characterization and native-matrix commits;
- final implementation head and workflow run IDs;
- three-host permission evidence;
- native counts and package/archive results;
- unchanged public API/dependency/JSON/write boundaries;
- residual limitations: native big-endian production, broader non-ASCII producer
  compatibility, and stable reads during arbitrary concurrent replacement remain
  outside HDB07.

- [x] **Step 4: update roadmap and package documentation**

Mark HDB07 complete at `1.15.0-Alpha-7`. State that the separately approved
compatibility-expansion tranche is next before HDB08. Update all referenced
READMEs with only externally established behavior; do not describe synthetic
coverage as native producer compatibility.

Mark every completed checkbox in this plan.

- [x] **Step 5: update PR #45 while preserving state**

Update the PR description with:

- HDB07 accepted scope and exact implementation head;
- normal and HDB00 qualification run IDs;
- links to the design, plan, and closure record;
- residual limitations and next tranche.

Verify the PR remains open, draft, and unmerged.

- [x] **Step 6: commit the documentation closure**

Commit:

```text
docs: accept HDB07 adversarial hardening
```

The documentation-only closure head must receive a normal 12/12 workflow. An
HDB00 workflow may launch because of GitHub event filtering, but its expensive
steps must be gated off and it is not a substitute for the recorded
implementation-head HDB00 qualification.

- [x] **Step 7: perform the final state check**

Fetch the committed files and PR metadata from GitHub. Verify:

- closure document content matches the accepted workflow evidence;
- version is still `1.15.0-Alpha-7`;
- no public API, production dependency, JSON contract, native library, P/Invoke,
  or write path was introduced;
- PR #45 is open, draft, and unmerged;
- the next work item is compatibility expansion, not HDB08 implementation.

