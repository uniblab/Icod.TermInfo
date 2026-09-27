# UC02 Conventional-Directory Adapter Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:executing-plans`
> inline, task by task. Do not use subagents. Steps use checkboxes.
> **Status:** Approved by the user on 2026-09-27 UTC; implementation in progress.

**Goal:** Deliver the internal conventional-directory adapter for
`1.17.0-Alpha-2`, translating bounded physical observations into actual terminal
publications without changing legacy Inspection behavior.

**Architecture:** One internal Catalogs adapter calls the public bounded
Inspection method and normalizes its immutable result. Inspection remains the
only directory traversal/layout owner and Runtime remains the only binary parser.
The existing Catalogs constructors enforce result invariants and final ordering.
The public reader is still UC04, not an Alpha-2 stub.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit, PowerShell 5.1, cmd/sh; no Python.

**Spec:** [Approved UC00 contract](../../1.17.0-UC00-UNIFIED-CATALOG-CONTRACT.md),
especially sections 4-6. Baseline: accepted UC01 code `656acf9`, recorded by
documentation head `68181c0b933fbd4b89c2715d15fd200c068a117f` in draft PR #48.

## Global Constraints

- User reviews this plan before implementation; execution is inline, no subagents.
- Catalogs keeps exactly ten public types until UC04. Preserve its UC01 manifest.
- Assembly version stays `1.0.0.0`; targets stay `net8.0;net9.0;net10.0`.
- Catalogs production references only Runtime, Inspection, and BerkeleyDb.
- Inspection retains Runtime/Source dependencies. No production friend assembly,
  second parser, directory-layout implementation, or native dependency is added.
- Names and invalid-placement path matching are ordinal. Do not normalize Unicode
  or rewrite the spelling of physical paths supplied by Inspection.
- Budgets are inclusive. Cancellation/limits throw without returning a result.
  Acquisition issues and duplicate-publication issues share MaximumIssueCount.
- No migration, source precedence, comparison, JSON/schema, or command changes.
- Keep published installation examples on 1.16.0; change only active development
  metadata to Alpha-2 during task 2. Preserve historical evidence and baselines.
- Use the existing worktree and PR #48. Do not merge, tag, or publish packages.
- Do not wait on documentation-only CI. Qualify the implementation code head.

## Review Focus

1. Canonical files with several aliases must not manufacture missing publications
   or turn duplicate canonical identities into duplicate publication groups.
2. Literal/hex copies and case-sensitive names must retain every occurrence,
   classify ordinally, and sort identically under English and Turkish cultures.
3. Misplaced parses and malformed candidates still consume lower-layer budgets;
   filtering must not refund work or hide the original diagnostics.
4. Failure before root observations differs from failure after observations;
   neither may be reported as a complete empty result.
5. A duplicate issue can exhaust a budget already consumed by acquisition issues;
   cancellation and unexpected exceptions must not become Partial results.

## Task 1: Directory Publication Adapter

**Create:**
- `Icod.TermInfo.Catalogs/src/ConventionalTerminalCatalogAdapter.cs`
- `tests/Icod.TermInfo.Catalogs.Tests/src/UC02DirectoryAdapterTests.cs`
- `tests/Icod.TermInfo.Catalogs.Tests/src/UC02DirectoryAdapterBoundaryTests.cs`
- `tests/Icod.TermInfo.Catalogs.Tests/src/DirectoryCatalogFixture.cs`

**Modify:**
- `tests/Icod.TermInfo.Catalogs.Tests/Icod.TermInfo.Catalogs.Tests.csproj`: add
  Compiler as a test-only ProjectReference for compiled fixture generation.
- `Icod.TermInfo.Inspection/src/Properties/AssemblyInfo.cs`: add only
  `InternalsVisibleTo("Icod.TermInfo.Catalogs.Tests")`. This test-only grant allows
  immutable physical observations to be constructed for deterministic mapping
  tests; it does not grant Catalogs production access to Inspection internals.

**Interfaces:** All new adapter members are internal, in
`Icod.TermInfo.Catalogs`. The static class exposes:

```csharp
internal static TerminalCatalog Read(
    TerminalCatalogSource source, TerminalCatalogReadOptions options,
    CancellationToken cancellationToken);

internal static TerminalCatalog ReadCore(
    TerminalCatalogSource source, TerminalCatalogReadOptions options,
    CancellationToken cancellationToken,
    Func<string, TermInfoDatabaseCatalogReadOptions, CancellationToken,
        TermInfoDatabaseCatalog> acquire);

internal static TerminalCatalog Normalize(
    TerminalCatalogSource source, TerminalCatalogReadOptions options,
    TermInfoDatabaseCatalog physical, CancellationToken cancellationToken);
```

`Read` delegates to `ReadCore` with `InspectDirectoryBounded`. `ReadCore` maps
options, invokes acquisition, translates only its typed limit exception, then
calls `Normalize`. The delegate is a deterministic test boundary, not a public
extension mechanism. Guard null arguments, require ConventionalDirectory source
kind, and require physical.Root to equal source.Path ordinally. Invalid internal
inputs fail explicitly rather than producing misleading provenance.

- [x] Write failing publication tests using real temporary directories and
  Compiler-generated bytes. `CanonicalDeclarationsDoNotManufactureAliases`
  asserts one canonical row when sample declares a/b but only s/sample exists.
  `SeparateAliasFilesArePublicationsNotDuplicates` adds a/a and b/b and asserts
  three rows, canonical/alias/alias, Complete, and no duplicate publication names,
  even though Inspection reports a duplicate canonical identity.
- [x] Add `LiteralAndHexCopiesRetainEveryOccurrence`: s/sample and 73/sample
  produce two rows, duplicate names [sample], one DuplicatePublication issue
  with null EntryPath and PublicationName sample, and Partial. Include repeated
  alias publications and three copies in synthetic observations: exactly one
  issue per repeated name, not one per pair. Different descriptions do not select
  a winner. Assert terminal object identity is preserved from physical inputs.
- [x] Add `MisplacedFilesRemainOnlyAsIssues`: exclude the physical parse with an
  InvalidPlacement issue at the exact same path; retain its valid sibling and
  original issue. Assert legacy bounded/unbounded Inspection still exposes the
  misplaced parse. Synthetic differently cased paths prove exact ordinal matching
  even on case-insensitive hosts. Name classification is ordinal, not cultural.
- [x] Add root/status and provenance tests using both real directories and
  constructed physical observations. Cover all rows in the mapping table below,
  all five physical issue kinds, root versus subdirectory locators, malformed
  siblings, and a Partial catalog containing zero valid rows. Preserve issue
  messages without asserting OS-specific prose. Acquisition issues have null
  PublicationName; only duplicate issues supply a publication name.
- [x] Add `OrderingIsOrdinalAcrossCulturesAndInputOrder`: reverse supplied entry
  and issue sequences under en-US and tr-TR; assert identical sorted rows,
  issues, and duplicate names according to UC00 section 4. Restore culture after
  the test. Include I/i names without requiring both files on a case-insensitive
  filesystem.
- [x] Add real-filesystem tests for child file/directory links being skipped,
  explicit linked roots being allowed, empty roots, missing paths, and file roots.
  Follow existing link-test availability conventions; do not count a skipped
  host capability as an exercised link case.
- [x] Write boundary tests: ReadCore forwards parser and all four directory
  budgets and the exact token; five lower limit names map unchanged with their
  values, original source object, and inner exception. Unexpected
  InvalidOperationException/ArgumentException and cancellation propagate unchanged.
  Pre-cancellation never invokes acquisition; cancellation inside acquisition
  before returning a physical catalog prevents normalization/result publication.
- [x] Add shared issue-capacity tests with one malformed acquisition issue and
  one duplicate group: maximumIssueCount 2 succeeds, 1 throws the common limit
  exception. Cover duplicate-only exhaustion, multiple duplicate groups, and
  acquisition exhaustion. Synthetic missing/unsupported roots each require one
  generated issue, even though the lower empty result contains none.
- [x] Add real boundary fixtures for exact candidate, physical entry, parsed-byte,
  and per-entry maxima and the next item/byte overrun. Include ignored candidates,
  copied aliases, malformed bytes and a misplaced physical parse; assert no
  budget refund during normalization. Reuse UC01 expectations rather than
  duplicating the traversal algorithm. Add pre-canceled Normalize coverage and
  inspect every normalization/grouping loop for cancellation checks.
- [x] Run `dotnet test tests/Icod.TermInfo.Catalogs.Tests/Icod.TermInfo.Catalogs.Tests.csproj -c Release -f net10.0 -m:1 -p:UseSharedCompilation=false --filter FullyQualifiedName~UC02`.
  Confirm failure is the missing adapter, not restore/tooling failure.
- [x] Implement the adapter. First map acquisition issues and collect exact
  invalid-placement paths, then map surviving physical entries using basename
  and canonical/alias membership. Never consume DuplicateCanonicalNames.
  Count actual publication names with an ordinal dictionary, retaining all rows.
  Reserve shared issue capacity before constructing/appending each generated
  issue; use remaining-capacity checks, not overflow-prone arithmetic.
- [x] Check cancellation at entry, each issue/entry/group, before reservation,
  and immediately before and after catalog construction/sorting. Keep acquisition
  handling narrowly scoped to TermInfoDatabaseCatalogLimitException; do not
  reinterpret programming errors or parse exception messages. Lower parsing and
  traversal behavior is unchanged.
- [x] Run the UC02 filter again, then all Catalogs and Inspection tests on every
  target framework by omitting `-f`; require zero failures and warnings. Run the
  existing exact API checks; the ten-type Catalogs manifest must be unchanged.
- [x] Self-review the diff against all five Review Focus items and UC00 sections
  4-6, including explicit switch mappings and every cancellation/budget boundary.
  Commit as `Add bounded conventional-directory publication adapter`.

| Physical observation | Unified result |
| --- | --- |
| ConventionalDirectory, no issues or duplicates | Complete |
| ConventionalDirectory with issues or duplicates | Partial, even with zero rows |
| Missing | Missing, no rows, one generated MissingSource issue |
| UnsupportedStore | UnsupportedSource, no rows, one generated UnsupportedSource issue |
| Unavailable | Unavailable, no rows, preserve permission/I/O diagnostics |
| Root issue path equals source.Path ordinally | Issue EntryPath null |
| Child file/subdirectory issue | Preserve original absolute EntryPath |

## Task 2: Alpha-2 Integration and Qualification

**Modify:** `Directory.Build.props`, active release notes in the eight package
projects (`Icod.TermInfo.csproj`, Source, Compiler, Inspection, Termcap, BerkeleyDb,
Catalogs, and `icod-terminfo/Icod.TermInfo.Router.csproj`), `README.md`,
`Icod.TermInfo.Catalogs/README.md`, `CHANGELOG.md`, `docs/VERSIONING.md`, and the
two root roadmaps. Update only active-version assertions found by
`rg -n '1\.17\.0-Alpha-1' tests`; keep deliberately synthetic package-version
fixtures and frozen historical evidence intact.

**Interfaces:** Consume task 1's internal adapter. No new public entry point or
distribution payload. Eight nupkg/seven snupkg; existing tools and six archive
RIDs remain unchanged.

- [x] Add `UC02DevelopmentMetadataTests.cs` in the Catalogs test src directory,
  asserting compiled Alpha-2 identity, stable assembly version and no production
  Compiler reference. Retain the existing exact ten-type API checks; review
  adapter-complete/reader-pending documentation directly. Run the named test filter and observe failure on Alpha-1 metadata.
- [x] Advance active coordinated metadata/assertions to Alpha-2. Add a new
  changelog section rather than rewriting UC01 history. Document publication
  semantics and internal-only adapter status; do not publish examples using the
  future reader. Run the new test and all existing metadata contract tests.
- [x] Run full Release solution build and tests, plus the separately invoked
  BerkeleyDb test project, across all TFMs with warnings as errors and serial
  MSBuild. Record actual counts, not copied UC01 totals.
- [x] Run `packaging/PackPackages.ps1 -Configuration Release -OutputDirectory artifacts/uc02-release`
  and `packaging/VerifyPackageArtifact.ps1 -ArtifactDirectory artifacts/uc02-release -Configuration Release`
  with the available PowerShell host and dotnet on PATH. Use
  DOTNET_PROCESSOR_COUNT=1 locally if needed. Require all package/API,
  compatibility, consumer, sample, symbol and managed-payload checks to pass.
- [x] Verify `git diff --check`, unchanged API baselines and JSON schemas, and
  unchanged production dependency directions. Self-review implementation and
  tests inline; commit as `Integrate and qualify UC02 Alpha-2`.
- [ ] Push to PR #48 without force and update its body with actual evidence.
  Require code-head Windows/Linux/macOS build/test/package checks to pass;
  diagnose any failure before accepting UC02. Do not claim native checks ran
  when a path-filtered workflow did not exercise them.
- [ ] Record accepted code SHA, test counts and CI links in both roadmaps; mark
  UC02 complete only after qualification and identify UC03 as next. Commit/push
  that documentation-only acceptance without waiting on its CI.

## Review and Handoff

This plan follows the approved architecture without adding a public API. The
test-only Inspection friend grant and internal acquisition delegate make status,
option forwarding and failure translation tests deterministic on privileged CI
hosts; no filesystem permission race or production internal access is required.

Plan self-review covers UC02's roadmap checklist and the directory requirements
of UC00. Hash-v9 decoding/bounds remain UC03; common reader dispatch/parity remain
UC04; broader race/adversarial qualification remains UC05. No implementation or
version change is authorized by the existence of this plan alone.
