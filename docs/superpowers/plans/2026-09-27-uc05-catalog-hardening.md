# UC05 Unified Catalog Hardening Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:executing-plans`
> inline, task by task. Do not use subagents. Steps use checkboxes.
> **Status:** Proposed for user review. UC04 is accepted at
> `168862eb6935ab10597f378a372e393591c9b7b2`; its documentation follow-up is
> `cc3b3ceca21324ac6ae5aa68dc08445e8c659f2e`.

**Goal:** Harden the approved one-source catalog reader against boundary inputs,
interrupted acquisition, filesystem changes, and compatibility regressions as
`1.17.0-Alpha-5`, without enlarging its public API.

**Architecture:** Exercise the public reader with bounded Compiler-generated
directory and managed Hash-v9 fixtures. Use the existing internal acquisition
seams for deterministic cancellation and enumeration failures, and the existing
BerkeleyDb fixture builder for physical-record and overflow boundaries. Fix only
behavior demonstrated incorrect by a failing test; retain the approved UC00
dispatch, status, budget, and exception contracts.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit, PowerShell 5.1, cmd/sh; no Python.

**Spec:** [Approved UC00 contract](../../1.17.0-UC00-UNIFIED-CATALOG-CONTRACT.md),
especially sections 4–7; [1.17 roadmap](../../../Icod.TermInfo-1.17.0-Unified-Directory-Hashed-Catalogs-Roadmap.md), UC05.

## Global Constraints

- Implement inline in an isolated checkout from draft PR #48 after the user
  reviews this plan. Non-force pushes only; do not merge, tag, or publish.
- Preserve the eleven-type Catalogs API, its exact UC04 addition and UC01
  ten-type reconstruction, all earlier public baselines, JSON v1–v6 schemas,
  assembly identities, package dependency directions, and command semantics.
- Preserve explicit single-source dispatch and fresh reads. Do not add provider
  discovery, native dependencies, source sets, migration, JSON features, or
  CLI options. Compiler and the BerkeleyDb writer remain test-only fixtures.
- Keep limits inclusive, cancellation ahead of allocation/retention, typed
  limit exceptions without partial results, and storage-specific diagnostics.
  Synchronous OS calls and concurrent source replacement are not atomic.
- Reuse the accepted bounded Inspection/BerkeleyDb paths. Add a narrow internal
  seam only if an otherwise untestable race requires it; never create a second
  parser, decoder, or production friend assembly.
- Raise all eight coordinated package versions together to
  `1.17.0-Alpha-5` only after behavioral tasks pass. Keep stable `1.16.0`
  install examples and historical fixture version strings unchanged.
- Before editing implementation, provision .NET SDKs 8/9/10 and PowerShell in
  the execution workspace, verify a clean baseline, and use strict RED→GREEN
  tests. This plan-only checkout has no `dotnet` executable on PATH.

## Review Focus

1. Thousands of ignored directory children still consume candidate budget
   before retention; the next yielded child raises a typed limit. Task 1.
2. Malformed candidates, invalid placements, and duplicate groups consume
   parsed/issue/entry budgets without refunds or a partial result. Task 1.
3. A shared overflow payload or orphan hashed record consumes physical,
   decoded, and parsed budgets at the specified accounting points. Task 2.
4. Cancellation or a disappearing source during an enumeration/read disposes
   owned resources and preserves Missing/Partial/Unavailable distinctions.
   Task 3.
5. The hardening release must leave old directory/database-set/JSON and CLI
   behavior unchanged while preserving generated and packaged APIs. Task 4.

---

## Task 1: Directory Aggregate Boundaries

**Create:** `tests/Icod.TermInfo.Catalogs.Tests/src/UC05DirectoryHardeningTests.cs`.
**Inspect if RED:** `Icod.TermInfo.Inspection/src/CatalogReadBudget.cs`,
`TermInfoDatabaseInspector.Catalog.Bounded.cs`, and
`TermInfoDatabaseInspector.Catalog.cs`; change only the defective path.

**Interfaces:** Exercise `TerminalCatalogReader.Read()` with
`TerminalCatalogReadOptions` and `CompiledTermInfoWriter.Write`; compare the
existing `TermInfoDatabaseInspector.InspectDirectory` physical behavior where
relevant. No new public type or method is produced.

- [ ] Write parameterized public-reader cases for one eligible directory plus
  many ignored root children, nested directories, malformed siblings, and
  valid publications. Count every yielded child in the fixture; exact
  `MaximumCandidateCount` succeeds and one fewer throws
  `TerminalCatalogLimitException` with `MaximumCandidateCount`, the configured
  value and the same source, before retaining the over-budget child.
- [ ] Use Compiler-generated large-description bytes and two physical copies:
  exact parser entry size and aggregate `MaximumParsedBytes` succeed, one byte
  less fails with the respective typed name. Include malformed bytes and a
  valid but misplaced parse to show charged input is not refunded. Repeat the
  read to prove its budget is fresh.
- [ ] Combine malformed acquisition issues with two duplicate publication
  groups. Exact `MaximumIssueCount` accepts the complete Partial result; one
  fewer throws, with no returned catalog. Test exact and one-beyond
  `MaximumEntryCount`, including a parsed entry filtered for invalid placement.
- [ ] Run the focused class on net10.0 with a watched RED where a defect exists;
  change only the offending acquisition/accounting code, then run the class on
  net8.0/net9.0/net10.0. Keep the legacy inspector's observed rows and issues
  intact. Commit the tested boundary work.

## Task 2: Hashed Physical and Logical Boundaries

**Create:** `tests/Icod.TermInfo.Catalogs.Tests/src/UC05HashedHardeningTests.cs`.
**Extend only for a lower-layer defect:**
`tests/Icod.TermInfo.BerkeleyDb.Tests/src/UC03AcquisitionBudgetTests.cs` or
`UC03BoundedCatalogTests.cs`, and the corresponding existing
`BerkeleyDbCatalogReadBudget.cs`, `BerkeleyDbHashReader.cs`, or
`NcursesCatalogReader.cs` implementation.

**Interfaces:** Build Hash-v9 images with `Hdb07HashV9FixtureBuilder` and
`HashedCatalogFixture`; exercise `TerminalCatalogReader.Read()` and its
`TerminalCatalogLimitException` cause. Use the existing `ReadBounded` fixture
seam for accounting that cannot be observed from a completed catalog.

- [ ] For one canonical publication, aliases and an orphan storage record,
  record actual image length, physical records, decoded key/value bytes,
  distinct parsed payload lengths and followed index links. Assert exact
  `MaximumDatabaseSize`, `MaximumRecordCount`, `MaximumDecodedBytes`,
  `MaximumParsedBytes`, `MaximumEntryCount`, parser `MaximumEntrySize`, and
  `MaximumIndexHops` pass independently; one fewer fails with the documented
  common limit name and the lower typed inner cause. Hops may be zero.
- [ ] Include a shared overflow tail, many aliases, a malformed orphan, and a
  valid-length item exceeding a configured budget. Assert physical/decoded
  work is charged before extraction, the orphan's compiled bytes before parse,
  malformed data returns `InvalidStore` only when no earlier limit was crossed,
  and no partial hashed rows escape. Cover `long.MaxValue` aggregate arithmetic
  at the lower budget seam without allocating a huge image.
- [ ] Watch a focused RED for any newly demonstrated defect, fix the narrow
  existing reader/budget path, and rerun the new cases and applicable UC03 tests
  on all three TFMs. Verify legacy `Read` still has its original exception
  family. Commit the tested hashed hardening.

## Task 3: Cancellation, Links, Permissions and Replacement

**Create:** `tests/Icod.TermInfo.Catalogs.Tests/src/UC05SourceFailureTests.cs`.
**Extend if needed:** `tests/Icod.TermInfo.Inspection.Tests/src/UC01BoundedCatalogTests.cs`,
`tests/Icod.TermInfo.BerkeleyDb.Tests/src/UC03AcquisitionBudgetTests.cs`.
**Inspect if RED:** the existing bounded acquisition and Catalogs adapters;
avoid changing legacy methods.

**Interfaces:** Use `InspectDirectoryBoundedCore(..., enumerate)` and
`HashedTerminalCatalogAdapter.ReadCore(..., inspect, acquire)` to inject
deterministic progress/failure points. Exercise the public reader separately
with actual temporary files and links.

- [ ] Inject cancellation before root inspection, after a yielded child, during
  directory file reading, in each hashed image/stability pass, and after the
  final normalized row. Assert `OperationCanceledException`, zero returned
  result, disposed enumerators/owned streams and successful fresh retry.
- [ ] Inject root enumeration failure before versus after progress and child
  failure after a valid sibling; assert Missing, Unavailable or Partial and
  typed permission/I/O issues per UC00. Use a deterministic deletion hook at
  the existing seam; do not write sleep-based or timing-sensitive race tests.
- [ ] On hosts that allow them, create an explicit linked root and linked file/
  child directory: root is readable, children are skipped with `LinkSkipped`,
  and no alias row is manufactured. Probe permission denial only if the host
  can actually deny access (including under elevated CI accounts); otherwise
  log a specific skip and rely on injected failure classification.
- [ ] Run the focused cases across all three TFMs and the applicable lower-layer
  tests. Correct only observed regressions; document the lack of atomic
  replacement snapshots and synchronous-call interruption. Commit.

## Task 4: Compatibility and Frozen Surface

**Create:** `tests/Icod.TermInfo.Catalogs.Tests/src/UC05CompatibilityTests.cs`.
**Modify only if needed:** existing frozen API verifier/tests, without editing
historical baselines or JSON schemas.

**Interfaces:** Compare the new reader with existing `InspectDirectory` and
`BerkeleyDbTerminalCatalogReader.Read` on shared fixture bytes. Run the current
package verifier reconstruction on compiled and isolated packaged assemblies.

- [ ] Pin an old directory physical catalog, a database-set plan and one
  Hash-v9 legacy read to their previously accepted publication/diagnostic
  semantics while unified reads retain UC00 normalization. Reuse the existing
  JSON v1–v6 fixture/schema tests and `toe`, `infocmp`, `tic` command tests;
  add a new assertion only for a concrete uncovered behavior.
- [ ] Regenerate current API snapshots on net8.0/net9.0/net10.0; assert exactly
  eleven Catalogs public types and byte-exact reconstruction to the unchanged
  UC01 ten-type baseline. Assert the Inspection and BerkeleyDb historical
  reconstruction chains and unchanged schema files. Fail the task for any
  unapproved public addition, dependency edge, or command output change.
- [ ] Run targeted compatibility filters plus the established package-only
  consumers; commit tests or minimal fixes with evidence. Do not regenerate a
  frozen manifest to accommodate an accidental change.

## Task 5: Alpha-5 Release Qualification

**Modify:** `Directory.Build.props`, all eight coordinated project release
notes, active version assertions, root and Catalogs READMEs, `CHANGELOG.md`,
`docs/VERSIONING.md`, both roadmaps, and the draft PR body. Keep this plan's
status current.

- [ ] Add a failing compiled metadata assertion for `1.17.0-Alpha-5`, stable
  `1.0.0.0` assembly identity, eleven Catalogs types and approved dependencies.
  Bump all eight package versions and current notes together, preserve
  historical Alpha fixture strings and stable 1.16 install commands; pass the
  metadata tests on all three TFMs.
- [ ] Run a fresh Release solution build with warnings as errors, serial MSBuild
  and `UseSharedCompilation=false`; run the complete solution tests (all TFMs)
  and separately rebuilt complete BerkeleyDb suite. Record actual counts,
  failures and warnings, and `git diff --check`.
- [ ] Pack eight nupkg and seven snupkg with `PackPackages.ps1`. Run
  `VerifyPackageArtifact.ps1` in Release against packages built from the same
  exact source/PDB commit. Require APIs, dependencies, isolated consumers,
  samples, managed-only payloads, symbols and Source Link to pass.
- [ ] Self-review source boundaries and UC00 matrix inline. Commit, non-force
  push to draft PR #48 and update the PR body. Observe all 12 platform/package/
  tool/archive PR jobs and the applicable native Linux/macOS plus managed
  Windows interoperability jobs on the implementation code. If a path-filtered
  interoperability run skips native probes, cite the last full native run on
  unchanged production code explicitly; do not call skipped probes passing.
- [ ] Only after exact-code qualification, record the code SHA/tree, counts,
  workflow links, platform skips, and remaining limitations in the roadmaps and
  PR. Mark UC05 accepted and UC06 next. Push a documentation-only acceptance
  commit without awaiting its CI; leave the PR draft, unmerged and unpublished.

## Review and Handoff

UC00 already approves the architecture, public signatures and budget/status
contract. This plan narrows UC05 to adversarial proof and repairs demonstrated
by RED tests; it does not pre-authorize additional public behavior. The user
selected inline execution without subagents for the previous tranches. Review
this plan before implementation under the 1.17 roadmap's per-tranche gate.
