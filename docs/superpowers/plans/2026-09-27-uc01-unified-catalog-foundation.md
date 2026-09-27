# UC01 unified catalog foundation implementation plan

> **For agentic workers:** Use `superpowers:executing-plans` inline, task by task.
> Do not use subagents. Steps use checkboxes. This is a draft implementation
> handoff accompanying the proposed UC00 contract; neither is approved for
> production implementation merely by being committed.

**Goal:** Deliver `1.17.0-Alpha-1` with the optional Catalogs model package and
an opt-in bounded conventional-directory inspection API.

**Architecture:** Catalogs composes existing optional layers from above.
Inspection gains a bounded acquisition method that shares semantic helpers with
its existing inspector while retaining legacy behavior. BerkeleyDb additions and
the public unified reader remain UC03 and UC04 respectively.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit, PowerShell 5.1, cmd/sh; no Python.

**Spec:** [UC00 contract](../../1.17.0-UC00-UNIFIED-CATALOG-CONTRACT.md).

## Global constraints

- Execute only after review of the UC00 contract and this plan; execution method
  is already selected: inline, no subagents.
- Reusable assembly identity is `1.0.0.0`; TFMs are `net8.0;net9.0;net10.0`.
- Catalogs production references: Runtime, Inspection, BerkeleyDb only.
- Inspection keeps Runtime/Source dependencies; BerkeleyDb keeps Runtime only.
- Runtime API, JSON v1–v6, existing reader/writer methods, and command output stay
  unchanged. Reconstruct old APIs after removing precisely the reviewed additions.
- New library license is `LGPL-3.0-or-later`; preserve attribution and repository
  packaging conventions. Build identity changes to `1.17.0-Alpha-1` only in task 4.
- Use serial MSBuild (`-m:1 -p:UseSharedCompilation=false`) in the current workspace.
  Do not change repository feed policy to accommodate a local restore problem.

## Review focus

1. Millions of ignored children: candidate budget applies before filtering and
   before sorting/materialization (task 1).
2. Malformed or misplaced entries: aggregate bytes are not refunded; invalid
   placement remains visible in legacy physical catalogs (task 1).
3. Cancellation and enumeration failure after partial progress: release resources
   and retain only documented observations, never label incomplete reads complete
   (task 1; unified status mapping remains UC02).
4. Relative paths, undefined enums, mutable input collections, and extreme numeric
   budgets: validate/snapshot once without overflow (tasks 1 and 2).
5. Existing null-overload calls and frozen API checks: the new distinct method must
   preserve compilation and reconstruct the exact old surface (task 3).

## Task 1 — bounded Inspection acquisition

**Create:**

- `Icod.TermInfo.Inspection/src/TermInfoDatabaseCatalogReadOptions.cs`
- `Icod.TermInfo.Inspection/src/TermInfoDatabaseCatalogLimitException.cs`
- `Icod.TermInfo.Inspection/src/TermInfoDatabaseInspector.Catalog.Bounded.cs`
- `Icod.TermInfo.Inspection/src/CatalogReadBudget.cs` (internal accounting only)
- `tests/Icod.TermInfo.Inspection.Tests/src/UC01BoundedCatalogTests.cs`

**Modify:** `Icod.TermInfo.Inspection/src/TermInfoDatabaseInspector.Catalog.cs`
only to share parsing, placement, issue, and finalization helpers. Keep legacy
entry points on their existing policy. Existing glob compile items include the
new files; do not add duplicate explicit compile entries.

**Interfaces:** Produce exactly the two types and `InspectDirectoryBounded`
signature in UC00 section 2. Consume Runtime parser/options and existing
`TermInfoDatabaseCatalog`, entry and issue types. The internal budget takes
`(string sourcePath, TermInfoDatabaseCatalogReadOptions options)` and provides
`ReserveCandidate()`, `EnsureEntryCapacity()`, `ReserveEntry()`, `ReserveIssue()`,
and `ReserveParsedBytes(long length)`; all return `void`. Its fields are private.
Limit names and inclusive defaults are exactly UC00 section 5.

- [ ] Write option and budget tests first. Assert defaults
  `(131072, 65536, 4096, 67108864L)`, parser maximum `1048576`, defensive parser
  copying, rejection of zero/negative numbers, acceptance of positive maxima,
  and remaining-capacity arithmetic at `long.MaxValue` without overflow.
- [ ] Add `DistinctMethodPreservesLegacyNullCall`: compile and call both
  `InspectDirectory(root, null)` and `InspectDirectory(root, null, token)`.
  Add boundary tests on the internal budget with no filesystem timing dependency:

```csharp
var budget = new CatalogReadBudget(root,
    new TermInfoDatabaseCatalogReadOptions(maximumCandidateCount: 2));
budget.ReserveCandidate();
budget.ReserveCandidate();
var error = Assert.Throws<TermInfoDatabaseCatalogLimitException>(
    () => budget.ReserveCandidate());
Assert.Equal("MaximumCandidateCount", error.LimitName);
Assert.Equal(2L, error.Limit);
Assert.Equal(root, error.SourcePath);
```

- [ ] Run `dotnet test tests/Icod.TermInfo.Inspection.Tests/Icod.TermInfo.Inspection.Tests.csproj -c Release -f net10.0 -m:1 -p:UseSharedCompilation=false --filter FullyQualifiedName~UC01BoundedCatalogTests`.
  Expected initial failure: new types/method absent, not restore/environment failure.
- [ ] Implement options and exception validation, then internal budget accounting.
  Reserve using subtraction before addition. `EnsureEntryCapacity` does not
  increment; `ReserveEntry` increments only after a successful parse.
- [ ] Write filesystem tests with unique temporary roots and Compiler-generated
  fixtures: ignored root files count; ignored nested directories count; exact
  candidate count succeeds; next candidate throws; no recursion; links skipped;
  two canonical copies consume two physical entry slots; malformed bytes consume
  aggregate budget; per-entry oversize throws a typed limit only in the new path.
  For an exact issue boundary, two malformed candidates with limit 2 yield two
  issues; a third with limit 2 throws `MaximumIssueCount`.
- [ ] Add deterministic cancellation/accounting tests using internal seams, not
  sleeps. Exercise cancellation before traversal, after a yielded candidate,
  before buffer allocation, and after reading. A test enumerator records disposal
  and can throw after one yield; assert partial observations and an I/O issue.
  Keep any enumeration seam internal/test-only, with production using lazy .NET
  enumeration; do not expose a public filesystem abstraction.
- [ ] Implement UC00's lazy two-level traversal. Collect/sort only budgeted paths;
  catch exceptions during enumerator creation and `MoveNext`. Allow explicit
  linked roots; report/skip child links. Ignore ordinary root files and child
  subdirectories after counting them. Root enumeration failure before yielding
  returns the existing Missing/Unavailable physical kind; after yielding,
  return ConventionalDirectory with an issue and observed candidates.
- [ ] Share the file-read helper using an optional internal budget/context. For
  the new path: per-entry size check, capacity check, aggregate reservation,
  allocation/read, Runtime parse, successful-entry charge, placement diagnostic.
  Legacy null context preserves old exception types and physical-entry behavior.
- [ ] Run the task filter plus `FullyQualifiedName~I03DatabaseCatalogTests`.
  Assert bounded generous-limit results equal legacy physical entries, kinds,
  aliases, issue kinds/paths, and duplicate canonical names on unchanged fixtures.
  Repeat on net8.0/net9.0; expected all pass. Existing whole-surface freeze tests
  need task 3's precise additive reconstruction before the full suite can pass.
- [ ] Commit the bounded acquisition API and its focused tests together.

## Task 2 — Catalogs immutable model package

**Create:** `Icod.TermInfo.Catalogs/Icod.TermInfo.Catalogs.csproj`, its `README.md`,
`src/Properties/AssemblyInfo.cs`, and one source file per following type:

`TerminalCatalogSourceKind`, `TerminalCatalogEntryKind`, `TerminalCatalogStatus`,
`TerminalCatalogIssueKind`, `TerminalCatalogSource`, `TerminalCatalogReadOptions`,
`TerminalCatalog`, `TerminalCatalogEntry`, `TerminalCatalogIssue`,
`TerminalCatalogLimitException`.

**Create:** `tests/Icod.TermInfo.Catalogs.Tests/Icod.TermInfo.Catalogs.Tests.csproj`,
`src/UC01CatalogModelTests.cs`, `src/UC01CatalogPackageContractTests.cs`.

**Modify:** `Icod.TermInfo.sln` to display the library at the root and tests under
the existing tests solution folder, with Debug/Staging/Release configurations.

**Interfaces:** The ten types, constructors, enum values and get-only properties
are exactly UC00 section 3. The reader is intentionally absent in Alpha-1.
Internal result constructors use the signatures there; test-only internals
access is granted solely to `Icod.TermInfo.Catalogs.Tests`.

- [ ] Write tests for path normalization before current-directory changes,
  argument validation order, undefined enums, every option boundary, parser
  snapshotting, and all exact defaults. Avoid mutating process current directory
  concurrently; use the repository's existing isolation convention.

```csharp
var options = new TerminalCatalogReadOptions();
Assert.Equal(131_072, options.MaximumCandidateCount);
Assert.Equal(65_536, options.MaximumEntryCount);
Assert.Equal(4_096, options.MaximumIssueCount);
Assert.Equal(67_108_864L, options.MaximumParsedBytes);
Assert.Equal(67_108_864, options.MaximumDatabaseSize);
Assert.Equal(65_536, options.MaximumRecordCount);
Assert.Equal(16, options.MaximumIndexHops);
Assert.Equal(67_108_864L, options.MaximumDecodedBytes);
Assert.Throws<ArgumentOutOfRangeException>(() =>
    new TerminalCatalogSource("sample", (TerminalCatalogSourceKind)99));
```

- [ ] Add result tests: changing caller lists after construction cannot change
  entries/issues/duplicate names; exposed lists reject mutation; null elements
  and undefined enums fail; hashed entry path is null; directory entry path is
  absolute; source/entry path and publication identity invariants are enforced.
  Assert sorted entries/issues/duplicate names exactly match UC00's ordinal keys
  under invariant and Turkish cultures. `HasIssues` derives solely from issue count.
- [ ] Run the new test project and observe the expected absent-type failure.
- [ ] Create the library/test projects using existing package/test conventions:
  version property from `Directory.Build.props`, assembly `1.0.0.0`, explicit
  source glob, XML documentation, license/icon/readme, exact three downward
  references. Test SDK/xUnit versions match the existing Inspection test project.
- [ ] Implement the model only. Constructors enforce the contract and snapshot
  collections; no filesystem reads, dispatch stub, `NotImplementedException`, or
  fabricated success reader is added. The README describes the Alpha foundation.
- [ ] Run model/package-contract tests across net8.0/net9.0/net10.0 and build
  Release warnings-as-errors. Assert ten exported types and no production
  references to Compiler, Terminal, native BDB, or test assemblies. Commit.

## Task 3 — exact additive compatibility and package verification

**Create:**

- `docs/1.17.0-UC01-INSPECTION-PUBLIC-API-ADDITIONS.txt`
- `docs/1.17.0-UC01-INSPECTION-PUBLIC-API-ADDITIVE-MEMBERS.txt`
- `docs/1.17.0-UC01-CATALOGS-PUBLIC-API-BASELINE.txt`
- `tests/Icod.TermInfo.Inspection.Tests/src/UC01InspectionCompatibilityTests.cs`
- `tools/catalogs-package-verifier/Icod.TermInfo.Catalogs.PackageVerifier.csproj`
- `tools/catalogs-package-verifier/Program.cs`

**Modify:** `.github/scripts/verify-inspection-compatibility.ps1`,
`.github/scripts/verify-inspection-compatibility-history.ps1`,
`tests/Icod.TermInfo.Inspection.Tests/src/RB08ReleaseClosureTests.cs`,
`Icod.TermInfo.sln`, and the model test project for exact-surface checks.

**Interfaces:** Reuse `tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj`
normalization. Catalogs verifier accepts one artifact-directory argument, exits
zero on valid Catalogs nupkg/snupkg and nonzero with a useful diagnostic otherwise.
Inspection reconstruction removes only the two exact UC00 types and the exact
`InspectDirectoryBounded` method; no namespace-wide filtering is permitted.

- [ ] Add failing tests that the current Inspection surface is 108 types and
  subtracting exactly the approved delta reproduces the frozen 106-type 1.14
  manifest hash `e9f240a562aec5274d64fb2ec3647862fe4ef5684582af2b442ba3c55e189497`.
  Assert unexpected extra types/methods fail reconstruction. Keep old manifest
  contents and schema fingerprints immutable.
- [ ] Snapshot only the new reviewed signatures and Catalogs ten-type baseline.
  Update verifier preprocessing so the reconstructed 1.14 view feeds the
  established 1.13→1.12→1.11→1.10 checks. Both verifier entry points must account
  for the delta exactly once. Historical test assertions about live type counts
  must use the reconstructed view, with a separate assertion for current 108.
- [ ] Audit additional whole-assembly assertions with
  `rg -n 'GetExportedTypes|106|1.14.0-INSPECTION' tests/Icod.TermInfo.Inspection.Tests .github/scripts tools/inspection-package-verifier`.
  Change only assertions that incorrectly treat the historical surface as the
  current whole assembly; retain explicit historical values and behavioral tests.
- [ ] Add package-verifier rejection fixtures for a missing TFM, wrong dependency,
  missing XML docs/license/icon/readme, unexpected native assets, or wrong
  assembly/API identity. Implement checks following the existing library verifiers.
- [ ] Build all TFMs; compare Catalogs public APIs net8↔net9 and net8↔net10 using
  the snapshot tool. Run Inspection compatibility scripts and the full Inspection
  tests. Run BerkeleyDb API-freeze tests: its source/API is untouched in UC01.
  Expected all pass and all six JSON fingerprints unchanged. Commit.

## Task 4 — coordinated Alpha-1 integration and qualification

**Modify:** `Directory.Build.props`, package release notes in the seven existing
packable project files plus the new Catalogs project, `CHANGELOG.md`, root and
Catalogs READMEs, `docs/VERSIONING.md`, `docs/COMPATIBILITY.md`, both active roadmaps.

**Distribution files:** `.github/scripts/verify-release-package.sh`,
`.github/scripts/verify-release-package.cmd`, `.github/workflows/release.yaml`,
`.github/workflows/pull-request.yaml`, `.github/workflows/main.yaml`,
`.github/workflows/distribution-validation.yaml`, and related package inventory
assertions found by `rg -n 'seven|six reusable|packages.Count|BerkeleyDb' .github tests`.
Only active coordinated counts/version checks change; historical counts remain.

**Test metadata:** Update current-version expectations in
`tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb09ReleaseClosureTests.cs` and other
coordinated-version checks exposed by the full suite. Keep stable 1.15/1.16
historical assertions unchanged. Add `UC01CatalogPackageContractTests` checks
for solution registration, coordinated version, and eight/seven artifact counts.

**Interfaces:** Introduce `1.17.0-Alpha-1`; register the new tests and verifier
with existing serial/local and CI build flows. Tools/archive dependency graphs
do not gain Catalogs. The new package must be explicitly packed, verified, and
collected wherever the coordinated family is assembled. UC06 later adds the
full reader sample and package-only functional consumer, not basic registration.

- [ ] Write failing metadata/distribution assertions before changing the version
  and package registration. Verify new Catalogs projects appear in Solution
  Explorer; merely adding a ProjectReference is not sufficient.
- [ ] Update centralized version and active release-facing notes, register
  packing/verifying/testing and exact package inventory, then run targeted checks.
  Describe Alpha-1 as models plus bounded Inspection, not completed unified reads.
- [ ] Run the full solution tests and Release warnings-as-errors build with
  serial MSBuild. Run existing Staging/Release distribution verification in its
  supported shell, preserving all existing package and archive checks. Record
  exact commands/results; do not replace them with unverified predicted counts.
- [ ] Qualify the code head on Windows/Linux/macOS and all three TFMs through
  existing workflows. Resolve concrete failures; do not wait on a later
  documentation-only commit. Do not tag, merge, or publish NuGet packages.
- [ ] Record UC01 evidence and remaining UC02 work in the release roadmap;
  mark accepted only after the required code-head qualification. Commit and
  update PR #48 with the bounded-acquisition/model delivery and its evidence.

## Handoff

UC02 consumes the bounded Inspection method and Catalogs models; it adds the
directory adapter and publication mapping. UC03 implements the separately
specified BerkeleyDb bounded method and hashed adapter. UC04 introduces the
reader facade. None of those APIs should be stubbed into UC01 to claim that the
whole release is already usable.

**Current evidence:** the user approved this plan on 2026-09-27 UTC. Tasks 1–3
are implemented and locally verified; task 4 distribution integration and
cross-host qualification are in progress. The release roadmap records evidence
and acceptance; this task checklist remains the original execution prescription.
