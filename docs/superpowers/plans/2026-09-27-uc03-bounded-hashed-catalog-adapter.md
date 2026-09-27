# UC03 Bounded Hashed Catalog Adapter Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:executing-plans`
> inline, task by task. Do not use subagents. Steps use checkboxes.
> **Status:** Proposed implementation plan for user review. UC00's architecture
> and public signatures are approved; UC03 implementation has not started.

**Goal:** Deliver opt-in bounded BerkeleyDb catalog acquisition and the internal
hashed Catalogs adapter for `1.17.0-Alpha-3`.

**Architecture:** A per-call budget context threads through the existing image,
Hash-v9 item, and ncurses readers. Legacy callers retain their existing policy
and exceptions. Catalogs composes the new public bounded method, translates
known acquisition failures, and retains actual publication keys and descriptions.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit, PowerShell 5.1, cmd/sh; no Python.

**Spec:** [Approved UC00 contract](../../1.17.0-UC00-UNIFIED-CATALOG-CONTRACT.md),
sections 2, 4, 5, 6, and 7. Baseline: accepted UC02 code
`965c8d2ee91a6b3515a33c64cb9f702566f3943a`, recorded by documentation head
`2922bce12f43c1cde8d6c08e0eb548092f8d7ea3` in draft PR #48.

## Global Constraints

- Execute inline in the existing worktree and PR #48; no subagents.
- Assembly version stays `1.0.0.0`; targets stay `net8.0;net9.0;net10.0`.
- Catalogs references only Runtime, Inspection, and BerkeleyDb. BerkeleyDb
  references only Runtime. Compiler remains a test/fixture dependency.
- Add exactly two public BerkeleyDb types and one method specified by UC00.
  BerkeleyDb becomes fourteen public types; Catalogs stays ten until UC04.
- Preserve legacy reads, exact lookup, writer verification, historical API
  baselines, and JSON schemas. No second decoder or production friend grant.
- All maxima are inclusive: check cancellation, reserve, then allocate/retain.
  Check remaining capacity before addition, including `long.MaxValue` limits.
- Decoded bytes count extracted key/value payloads, including ncurses markers
  and repeated overflow references. They do not count every defensive CLR copy.
- Parsed bytes count each distinct storage key once, including orphan storage;
  publication count counts actual marker-2 rows before resolution or retention.
- Cancellation and resource overruns throw without a result. Invalid hashed
  data yields no partial catalog. No retries, refunds, or exception-message codes.
- Source selection is explicit; preserve ordinal names, actual provenance, and
  Runtime terminal objects. Do not synthesize aliases or choose duplicate winners.
- No migration, precedence, comparison, command, or public unified reader work.
- Advance active metadata to Alpha-3 only at integration. Keep published install
  examples on 1.16.0. Do not merge, tag, or publish NuGet packages.
- Do not wait on documentation-only CI; qualify the implementation code head.

## Review Focus

1. A stream canceled after its length is observed, during the second stability
   read, or during overflow traversal must stop before the next allocation/work
   unit and release the owned file handle (tasks 1 and 2).
2. Two records referencing the same overflow payload must charge two extractions;
   aliases sharing one storage key must charge one parse (tasks 1 and 2).
3. A malformed orphan following valid publications must reject the entire
   hashed result, including through Catalogs (tasks 2 and 3).
4. An explicit directory, an inaccessible file, and a disappearing file must
   produce distinct source outcomes without broadly swallowing errors (task 3).
5. API checking must reject modified additions as well as unrelated legacy
   changes; merely subtracting two type names is insufficient (task 4).

## Task 1: Bounded Physical Acquisition

**Create:**
- `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalCatalogReadLimits.cs`
- `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbCatalogLimitException.cs`
- `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbCatalogReadBudget.cs`
- `tests/Icod.TermInfo.BerkeleyDb.Tests/src/UC03AcquisitionBudgetTests.cs`

**Modify:** `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbHashReader.cs` and the
historical exported-type assertion in
`tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb01ContractTests.cs`.

**Interfaces:** Implement the exact two public constructors/properties in UC00.
Defaults are publication count `65_536`, decoded bytes `67_108_864L`, parsed
bytes `67_108_864L`. All three must be positive. The exception constructor is
`(string sourcePath, string limitName, long limit, Exception? innerException = null)`;
properties are `SourcePath`, `LimitName`, `Limit`. Validate absolute nonblank
source, the eight UC00 BerkeleyDb limit names, and positive limits except hops
may be zero. Add XML documentation; no public constants or setters.

The internal sealed `BerkeleyDbCatalogReadBudget` constructor takes
`(string sourcePath, BerkeleyDbTerminalCatalogReaderOptions options,
BerkeleyDbTerminalCatalogReadLimits limits, CancellationToken cancellationToken)`.
It exposes internal get-only `CancellationToken`, and internal void methods
`CheckImage(long length)`, `CheckStoredItem(long length)`,
`CheckEntry(long length)`, `CheckIndexHop(int followedLinks)`,
`ReserveRecord()`, `ReserveDecoded(long length)`, `ReservePublication()`, and
`ReserveParsed(long length)`. Each checks cancellation first. Checks do not
charge aggregate counters; reservation methods do. Hop checking rejects the
next link when `followedLinks >= MaximumIndexHops`.

Append optional `BerkeleyDbCatalogReadBudget? budget = null` to the internal
`ReadDatabase(string, int)`, `ReadDatabase(Stream, int)`,
`ReadStableDatabase(Stream, int)`, and `ReadRecords(byte[], int, int,
CancellationToken)` methods. Thread it through private stability, item, and
overflow helpers. Null preserves the legacy branches and error selection.
Existing provider, lookup, and writer callers remain on that null policy.

- [ ] Write constructor tests asserting exact defaults, get-only properties,
  parameter-order validation, no cross-budget inequality, accepted long.MaxValue,
  and exception path/name/value/inner validation (all eight names; hops zero).
  Keep the historical twelve-type assertion after removing exactly the two
  approved new types, and add an explicit current fourteen-type assertion.
- [ ] Write `ImageLimitPrecedesAllocation`: an instrumented stream of length
  512 with max 511 throws the typed database limit before any Read; length 512
  with max 512 passes acquisition. Test cancellation triggered by Length before
  allocation, pre-cancellation, first-read and stability-read cancellation,
  same-length mutation, truncation, and growth. Borrowed streams stay open;
  reader-owned files must be reopenable exclusively after failure.
- [ ] Use the existing `Hdb07HashV9FixtureBuilder` for exact and next-over limits:
  N physical records accept N and reject N-1 before extracting the next pair;
  decoded budget equals the sum of extracted key/value lengths, and sum-1 fails.
  Include little/big endian, inline and overflow keys/values, shared overflow
  tails referenced twice, and stored-item size M versus M+1. Assert typed source,
  name, and value rather than English messages. Test budget arithmetic directly
  with long.MaxValue reservations without allocating large buffers.
- [ ] Run `dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release -f net10.0 -m:1 -p:UseSharedCompilation=false --filter FullyQualifiedName~UC03AcquisitionBudgetTests`.
  Confirm failure is the missing bounded contract, not tooling/restore failure.
- [ ] Implement the public types, budget, and optional shared-reader seams.
  Reuse `BerkeleyDbCancellationReadStream` for bounded image and stability reads;
  it borrows the inner stream. Explicitly check cancellation after Length and
  before image/stability-buffer allocation. Check every overflow page and both
  sides of record sorting. Reserve a record before key extraction and decoded
  bytes before each inline ToArray or overflow output allocation.
- [ ] Keep structural validation needed to trust an encoded length ahead of
  reservation. Impossible overflow lengths exceeding image capacity remain
  format errors; valid lengths exceeding configured bounds use typed limits.
  Preserve null-policy ordering. Do not promise corruption has global priority
  over an earlier valid budget failure. Do not charge record defensive clones
  or dictionary key copies as additional physical extractions.
- [ ] Rerun the new filter and existing Hdb02AcquisitionTests,
  Hdb05HashEnumerationTests, Hdb07StorageHardeningTests,
  Hdb07OverflowHardeningTests, Hdb07cAcquisitionStabilityTests, and
  Hw05PublicationVerificationTests on net10.0. Require no failures; inspect all
  allocation sites and null-policy paths. Commit as `Add bounded BerkeleyDb physical acquisition`.

## Task 2: Bounded Logical Catalog and Public Method

**Modify:** `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalCatalogReader.cs`
and `Icod.TermInfo.BerkeleyDb/src/NcursesCatalogReader.cs`.
**Create:** `tests/Icod.TermInfo.BerkeleyDb.Tests/src/UC03BoundedCatalogTests.cs`.

**Interfaces:** Add the exact public instance method:

```csharp
public IReadOnlyList<BerkeleyDbTerminalCatalogEntry> ReadBounded(
	BerkeleyDbTerminalCatalogReadLimits? limits = null,
	CancellationToken cancellationToken = default
);
```

Create one task-1 budget per call using normalized `DatabasePath`, the reader's
existing options snapshot, and default limits when null. Pass it through the
shared ReadCore pipeline. Append optional `BerkeleyDbCatalogReadBudget? budget
= null` to internal `NcursesCatalogReader.Read`; pass it through resolution and
storage parsing helpers. Legacy Read overloads retain their current exception
wrapping boundaries and default-null policy.

- [ ] Write real bounded-reader fixtures for canonical-only publication,
  separately published aliases, multi-hop indexes, valid empty database,
  non-ASCII UTF-8/Latin-1 identities, chained bucket pages, and large overflow
  records. Reuse qualified Hdb07/Hw07 fixtures or writer output. Assert actual
  names/kinds, shared terminal reference for aliases, and ordinal ordering under
  en-US/tr-TR with restored test culture. Do not add another binary builder.
- [ ] Add `AliasesChargeOneParseAndEachPublication`: for one payload of P bytes
  and three publication rows, parsed P and publication 3 succeed; parsed P-1 or
  publication 2 throws. Add another orphan payload Q: P+Q succeeds and P+Q-1
  throws, even when Q has no publication. Independent calls have fresh budgets.
- [ ] Test exact hop count and next-link failure, including max hops zero.
  Test per-entry size at the internal logical seam: public image decoding may
  reach the derived stored-item cap first. Do not assert every limit is reachable
  through every pipeline when an earlier applicable limit necessarily wins.
- [ ] Add malformed orphan, missing/cyclic target, unknown marker, duplicate
  raw/decoded publication name, and identity-mismatch fixtures. Assert bounded
  reads fail wholly and legacy Read retains its format/Runtime/InvalidData
  exception families. Include malformed orphan after a valid publication.
  Test pre-cancel and deterministic cancellation during discovery using a
  test-controlled record list; audit resolution and sort checkpoints.
- [ ] Run the task-1 command with `--filter FullyQualifiedName~UC03BoundedCatalogTests`
  and observe the expected missing-method/budget behavior failure.
- [ ] Implement budget propagation. Reserve publication capacity during the
  publication discovery pass before decoding/retaining the next publication
  name; do not charge it again during resolution. Before parsing an uncached
  storage key, check entry size then reserve payload bytes (excluding marker).
  Cache only successful parses. Check hops before following each target.
  Check cancellation in both dictionary-building loops, per record/link, before
  parsing, and before/after sorting and final immutable publication.
- [ ] Rerun new tests, then all BerkeleyDb tests on all three TFMs by omitting
  `-f`. Require no failures, including legacy lookup/writer suites. Commit as
  `Add opt-in bounded BerkeleyDb catalog reads`.

## Task 3: Internal Hashed Catalogs Adapter

**Create:**
- `Icod.TermInfo.Catalogs/src/HashedTerminalCatalogAdapter.cs`
- `tests/Icod.TermInfo.Catalogs.Tests/src/UC03HashedAdapterTests.cs`
- `tests/Icod.TermInfo.Catalogs.Tests/src/UC03HashedAdapterBoundaryTests.cs`
- `tests/Icod.TermInfo.Catalogs.Tests/src/HashedCatalogFixture.cs`

**Modify:** `tests/Icod.TermInfo.Catalogs.Tests/Icod.TermInfo.Catalogs.Tests.csproj`
to link `tests/Shared/Hdb07HashV9FixtureBuilder.cs` for malformed fixtures.

**Interfaces:** The internal static adapter exposes:

```csharp
internal static TerminalCatalog Read(
	TerminalCatalogSource source, TerminalCatalogReadOptions options,
	CancellationToken cancellationToken
);
internal static TerminalCatalog ReadCore(
	TerminalCatalogSource source, TerminalCatalogReadOptions options,
	CancellationToken cancellationToken, Func<string, FileAttributes> inspect,
	Func<string, BerkeleyDbTerminalCatalogReaderOptions,
		BerkeleyDbTerminalCatalogReadLimits, CancellationToken,
		IReadOnlyList<BerkeleyDbTerminalCatalogEntry>> acquire
);
```

Read supplies File.GetAttributes and a delegate constructing the existing
BerkeleyDb reader then calling task 2's ReadBounded. ReadCore validates non-null
inputs and BerkeleyDbHash source kind. Injected delegates are internal test
seams only. Test valid lower rows by reading actual fixtures; no BDB internals
grant is needed. Cancellation precedes inspect, acquisition, and normalization.

- [ ] Write real adapter tests for canonical-only, actual aliases/index chains,
  empty, non-ASCII, chained buckets, and large-record stores. Assert Complete,
  exact publication names/kinds, retained terminal object identity, source file
  provenance, null EntryPath, no manufactured aliases, and no duplicate groups.
  Check ordering with reversed lower rows under en-US/tr-TR.
- [ ] Test the table below using real missing/directory paths and deterministic
  inspect/acquire failures. Valid rows followed by malformed orphan, missing or
  cyclic target, and identity mismatch all yield zero rows and exactly one
  InvalidHashedStore issue. Source-wide issues have null EntryPath and name.
  Do not assert OS-specific message prose or put capability bytes in messages.
- [ ] Verify exact parser/database/record/hop option forwarding, limits
  publication=MaximumEntryCount and decoded/parsed byte forwarding, and token
  identity. For all eight typed lower limits assert the original source object,
  unchanged value, and same inner exception; only MaximumPublicationCount maps
  to MaximumEntryCount. Exercise real exact/overrun integration fixtures too.
- [ ] Test cancellation before inspection (neither delegate invoked), during
  acquisition, and before result return. Unexpected ArgumentException and
  InvalidOperationException propagate. A format exception from inspection is
  not misclassified as invalid hashed data. Test file disappearance between
  inspection and acquisition and PermissionFailure versus IoFailure mapping.
- [ ] Run `dotnet test tests/Icod.TermInfo.Catalogs.Tests/Icod.TermInfo.Catalogs.Tests.csproj -c Release -f net10.0 -m:1 -p:UseSharedCompilation=false --filter FullyQualifiedName~UC03Hashed`;
  confirm the missing adapter is the failure.
- [ ] Implement the adapter using task 2's public API only. Scope known format
  catches to acquisition, before the general IOException handling where relevant;
  catch the typed budget exception first. Check cancellation before creating any
  issue/result and each mapped entry, and before/after constructor sorting.
  Reserve issue capacity before its single source issue. Do not catch Exception,
  ArgumentException, allocation errors, or failures from normalization.
- [ ] Rerun the filter, then all Catalogs tests on all TFMs; preserve UC01/UC02
  behavior and the ten-type manifest. Self-review every table row and budget
  translation. Commit as `Add bounded hashed publication catalog adapter`.

| Observation | Result |
| --- | --- |
| Valid store, including zero publications | Complete, no issues |
| Directory attributes for explicit hashed source | UnsupportedSource, one UnsupportedSource issue |
| FileNotFoundException / DirectoryNotFoundException while inspecting or opening the source | Missing, one MissingSource issue |
| UnauthorizedAccessException from inspection/acquisition | Unavailable, one PermissionFailure issue |
| Other acquisition/inspection IOException | Unavailable, one IoFailure issue |
| BerkeleyDbDatabaseFormatException, CompiledTermInfoFormatException, or InvalidDataException from acquisition | InvalidStore, one InvalidHashedStore issue |
| Typed limit or cancellation | Throw; no catalog |

## Task 4: Exact Additive API and Package Compatibility

**Create:**
- `docs/1.17.0-UC03-BERKELEYDB-PUBLIC-API-ADDITIONS.txt`
- `docs/1.17.0-UC03-BERKELEYDB-PUBLIC-API-ADDITIVE-MEMBERS.txt`
- `tools/berkeleydb-package-verifier/BerkeleyDbUc03Compatibility.cs`
- `tests/Icod.TermInfo.BerkeleyDb.Tests/src/UC03ApiCompatibilityTests.cs`

**Modify:** the BerkeleyDb package verifier `Program.cs` and csproj,
`.github/scripts/verify-berkeleydb-package.ps1`, and the BDB test csproj to link
the new compatibility helper and public-api-snapshot source. Review existing
Hdb01/Hw01/Hdb09/Hw08 contract assertions; retain their historical intent.

**Interfaces:** Internal static helper method
`string Reconstruct(string current, string approvedTypeBlocks,
string approvedMemberLines)` follows InspectionUc01Compatibility's exact-delta
pattern. Remove exactly the two complete approved type blocks and exactly one
ReadBounded member on its owning reader, rejecting missing/changed/duplicate
additions. Require the reconstructed normalized 1.16 manifest SHA-256
`01a84c409fb222324009272de0213d1b1a8dc3df1ccac46ce66341f17d4baa81`.
Then invoke existing BerkeleyDbApiFreeze.VerifyReaderReconstruction against the
unchanged 1.15 baseline (SHA-256
`f519600aa4085d07c2d20bd8dc7a32c4dc06a43f4e361554b205ce2f97a8bf36`).

- [ ] Add tests for exact fourteen-to-twelve-to-nine reconstruction; corrupt
  each approved addition, duplicate/remove the method/type, add an unapproved
  member, and modify a legacy member. Every corruption must fail. Obtain the
  current manifest from the actual assembly; do not compare old text to itself.
- [ ] Run the BDB test command filtered to UC03ApiCompatibilityTests and
  observe the missing helper/manifest failure.
- [ ] Freeze and review the exact two type blocks and method line from the
  generated manifest against UC00. Implement the helper and a verifier mode
  `--reconstruct-uc03 <input-manifest> <output-manifest>`. Keep its existing
  artifact-directory mode; that mode must also check the actual packaged BDB
  assembly for each TFM using public-api-snapshot and isolated assembly loading,
  following the Catalogs package verifier's established pattern.
- [ ] Update the PowerShell gate to generate the current net10 manifest into
  artifacts, reconstruct it, and compare it with the unchanged
  `docs/1.16.0-BERKELEYDB-PUBLIC-API-BASELINE.txt`. Keep cross-TFM comparisons,
  historical reader reconstruction, metadata/symbol/payload gates and samples.
  Link source as needed rather than adding production dependencies.
- [ ] Run new compatibility tests and existing Hdb01/Hw01/Hdb09/Hw08 tests;
  verify both frozen hashes and the unchanged Catalogs manifest. Full package
  gates become mandatory at this task; earlier incremental tasks run focused
  suites while the old exact-current-surface gate awaits this approved update.
  Commit as `Verify exact UC03 BerkeleyDb API additions`.

## Task 5: Alpha-3 Integration and Qualification

**Modify:** `Directory.Build.props`, active release notes in all eight package
projects, `README.md`, `Icod.TermInfo.BerkeleyDb/README.md`,
`Icod.TermInfo.Catalogs/README.md`, `CHANGELOG.md`, `docs/VERSIONING.md`, this
plan, and both root roadmaps. Add
`tests/Icod.TermInfo.Catalogs.Tests/src/UC03DevelopmentMetadataTests.cs`;
update only active-version assertions located by `rg -n '1\.17\.0-Alpha-2' tests`.

**Interfaces:** Consume tasks 1–4, with no new public surface. Distribution stays
eight nupkg/seven snupkg and the same tools/six archive RIDs. UC04 owns common
reader dispatch and cross-format parity; UC05 broadens adversarial/race coverage.

- [ ] Add compiled Alpha-3 identity, stable assembly version, and dependency
  assertions following UC02DevelopmentMetadataTests; run the new test and see
  the expected Alpha-2 identity failure.
- [ ] Advance active metadata and assertions to Alpha-3; preserve synthetic
  fixtures and accepted UC01/UC02 evidence. Document ReadBounded defaults, typed
  failures, accounting, cancellation limits, fail-closed hashed semantics, and
  internal adapter status. Add a changelog entry; do not advertise the pending
  public unified reader as available. Rerun metadata/contract tests.
- [ ] Run the full Release solution build and tests with warnings as errors,
  serial MSBuild and `-p:UseSharedCompilation=false`, plus the separately
  invoked BerkeleyDb test project, on all TFMs. Record actual test counts.
- [ ] Run `packaging/PackPackages.ps1 -Configuration Release -OutputDirectory artifacts/uc03-release`
  and `packaging/VerifyPackageArtifact.ps1 -ArtifactDirectory artifacts/uc03-release -Configuration Release`
  with PowerShell and dotnet on PATH. Use DOTNET_PROCESSOR_COUNT=1 locally if
  needed. Require API, reconstruction, dependency, consumer, sample, symbol,
  SourceLink and managed-payload gates to pass.
- [ ] Run `git diff --check`; verify unchanged historical baselines/schemas and
  production dependency directions. Self-review the complete diff against UC00
  and all Review Focus cases. Commit as `Integrate and qualify UC03 Alpha-3`.
- [ ] Push to PR #48 without force and update its body with actual evidence.
  Require Windows/Linux/macOS build/test/package and relevant native/managed
  interoperability jobs at that exact code head. Diagnose failures before
  acceptance; report any path-filtered/skipped native checks accurately.
- [ ] Record accepted code SHA, counts and CI links; mark UC03 complete only
  after those checks pass. Identify UC04 as next. Commit/push the documentation
  acceptance without waiting on documentation-only CI.

## Review and Handoff

Plan self-review covers every UC03 roadmap item and the applicable UC00
requirements. The implementation reuses the existing decoder, cancellation
stream, logical storage cache, qualified fixture builder, and package API
machinery. Legacy exception priority stays on the null-budget path. Public
defaults and budgets are fixed by the approved contract, not new design choices.

The plan is ready for user review before task 1, following the writing-plans
workflow. Execution remains inline with author self-review and no subagents.
This documentation commit does not advance the version or claim UC03 is built.
