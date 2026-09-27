# UC06 Unified Catalog Consumer and Guide Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:executing-plans` inline, task by task; do not use subagents. Steps use checkbox (`- [ ]`) syntax for tracking.
> **Status:** Proposed for review. UC05 accepted at implementation commit `09f69208d9240c755e4015cf20364a386963703b`; documentation head `fc3b43195762471e00bc66e037853974e86aa189`.

**Goal:** Make the existing bounded directory/Hash-v9 catalog reader usable from a documented, runnable sample and an isolated package-only consumer, then qualify the coordinated `1.17.0-Alpha-6` distribution.

**Architecture:** Keep the eleven-type `Icod.TermInfo.Catalogs` API and production dependency graph unchanged. A deterministic sample creates disposable conventional and hashed fixtures with public writers, then presents both through the same catalog read/print routine. A separate temporary consumer restores only the `Icod.TermInfo.Catalogs` nupkg and its transitive dependencies from locally built artifacts, executes on three TFMs, and is invoked by the established package gate.

**Tech Stack:** C# 13, .NET 8/9/10, PowerShell 5.1-compatible packaging script, cmd/sh for existing verification, no Python.

**Spec:** [Approved UC00 contract](../../1.17.0-UC00-UNIFIED-CATALOG-CONTRACT.md) and [1.17 roadmap, UC06](../../../Icod.TermInfo-1.17.0-Unified-Directory-Hashed-Catalogs-Roadmap.md). The approved roadmap requires review of this task plan before implementation.

## Global constraints

- Single explicit source per read; no migration, mixed-source precedence, new JSON, command switch, or native production dependency.
- Existing APIs, JSON v1–v6, command behavior, `1.0.0.0` assembly identity, LGPL attribution, and dependency directions remain unchanged.
- Preserve eight coordinated nupkg and seven reusable-library snupkg; do not add Catalogs to the Tools package or any of six archives.
- Bump all eight coordinated packages to `1.17.0-Alpha-6` together; preserve historical version fixtures and the published stable `1.16.0` installation examples.
- Build and test serially in the restricted local MSBuild environment (`-m:1 -p:UseSharedCompilation=false`); platform CI provides independent Windows/Linux/macOS evidence.

## Review focus

1. A declared alias with no published file/key must not appear in either sample or package consumer output; Task 1 tests the distinct identity fields.
2. An issue-bearing directory retains valid siblings as `Partial`, with actual paths and diagnostics, while an invalid hashed store returns no rows; Tasks 1 and 2 check both.
3. An isolated consumer can silently resolve repository project outputs or a stale NuGet cache; Task 2 checks copied files, local-only source mapping, isolated package cache, and the exact version.
4. A sample can accidentally turn cancellation into a partial catalog or confuse an inclusive limit with truncation; Task 1 verifies thrown exceptions and a fresh successful retry.
5. A release script can pass library packages yet change installed Tools/archive payloads; Task 4 verifies the existing six archive and tool-package gates and the unchanged CLI inventory.

---

## Task 1: Deterministic unified sample

**Create:** `samples/Icod.TermInfo.Catalogs.Sample/Icod.TermInfo.Catalogs.Sample.csproj`, `Program.cs`, `README.md`.
**Modify:** `Icod.TermInfo.sln`, `packaging/VerifyPackageArtifact.ps1` (sample loop only).

**Interfaces:** Use `TerminalCatalogReader(TerminalCatalogSource, TerminalCatalogReadOptions?)`, `Read()` / `Read(CancellationToken)`, `TerminalCatalog.Entries/Issues/Status`, and public `CompiledTermInfoDatabaseWriter.Write` / `BerkeleyDbTerminalDatabaseWriter.Write` to create fixtures. No new library API.

- [ ] Add a sample project targeting `net8.0;net9.0;net10.0` with project references to Catalogs and the fixture-producing Compiler and BerkeleyDb libraries; add the normal sample solution configurations. It is not packable and never probes ambient databases.
- [ ] Implement deterministic `Program`: in a disposable temp directory publish one canonical `uc06-sample` and separately published `uc06-alias` in both formats, while leaving one declared alias unpublished. Use one `PrintCatalog(TerminalCatalog)` routine for both sources that shows source kind, status, actual publication name/kind, declared canonical name, source/entry path, and issue kind/locator. Normalize only temporary path prefixes in stable test output.
- [ ] Exercise one intentionally issue-bearing directory alongside a valid sibling (Partial), cancellation before a read (`OperationCanceledException`), and a custom inclusive budget with exact count success and one-below typed `TerminalCatalogLimitException`; then read again successfully. Handle these demonstrations in the sample, without silently catching unexpected exceptions.
- [ ] Add a `--verify` sample mode that asserts publication/alias/provenance/status/issue, cancellation, limit and retry observations and returns a nonzero exit code on failure; run `dotnet run --project samples/Icod.TermInfo.Catalogs.Sample/Icod.TermInfo.Catalogs.Sample.csproj -c Release -f net10.0 -- --verify` first against an intentionally wrong expectation to observe RED, then implement/repair the fixture and run it on all three TFMs. Add those three runs to `VerifyPackageArtifact.ps1` after its existing sample gates.
- [ ] Document both human output and the `--verify` command in the sample README; commit the tested sample and integration.

## Task 2: Installed package-only consumer

**Create:** `tools/catalogs-package-smoke/Icod.TermInfo.Catalogs.PackageSmoke.csproj`, `Program.cs`, `.github/scripts/smoke-uc06-catalogs-package-consumer.ps1`.
**Modify:** `packaging/VerifyPackageArtifact.ps1` (invoke the consumer after existing catalog artifact verification).

**Interfaces:** The smoke project has exactly one direct `<PackageReference Include="Icod.TermInfo.Catalogs" Version="$(IcodTermInfoCatalogsPackageVersion)" />`. Its public Catalogs API read checks use self-contained minimal compiled bytes and the transitively restored public BerkeleyDb writer for the hashed fixture; no project references or ambient database. The script accepts mandatory `-ArtifactDirectory` and optional `-Configuration` (Debug/Staging/Release), like HDB03.

- [ ] Add a package-only executable targeting three TFMs and a copied-outside-repo PowerShell runner. The runner reads `IcodTermInfoSuiteVersion`, creates a fresh temp project and `NUGET_PACKAGES`, writes a `NuGet.Config` with only the artifact source and an `Icod.TermInfo*` mapping, restores with the exact property, runs each framework, and cleans/restores environment in `finally`.
- [ ] Implement consumer assertions for canonical and published alias rows from both source formats, unpublished declaration absence, directory entry path versus hashed null `EntryPath`, fresh retry, bounded `MaximumEntryCount` failure, Partial from malformed directory sibling, and failed hashed image with zero entries; avoid any production reference to Compiler or native libraries.
- [ ] Start with a deliberate failed assertion to prove the isolated consumer gate detects a bad expected publication. Then fix it, pack the current coherent package version, and run `.github/scripts/smoke-uc06-catalogs-package-consumer.ps1 -ArtifactDirectory <same-commit-packages> -Configuration Release` on net8.0/net9.0/net10.0.
- [ ] Invoke the new runner in `VerifyPackageArtifact.ps1` and verify the full gate still checks the Catalogs nupkg/snupkg, API reconstruction, dependencies, licenses, symbols and Source Link. Commit the independently qualified consumer.

## Task 3: User guide and cross-links

**Create:** `docs/1.17.0-UNIFIED-CATALOG-GUIDE.md`.
**Modify:** root `README.md`, `Icod.TermInfo.Catalogs/README.md`, sample README if needed, `CHANGELOG.md`, `docs/VERSIONING.md`, and both roadmaps for in-progress UC06 status.

**Interfaces:** Copy the approved UC00 public constructor names and source/status enum names as compiled by the Task 1 sample; use verified sample output rather than a hypothetical CLI.

- [ ] Explain explicit source selection and same read/print loop, actual publication names versus `TerminalDescription` declarations, canonical/alias and duplicate directory occurrences, absolute paths versus hashed key provenance, and sorted issues/entries. Explain Complete/Partial/Missing/UnsupportedSource/Unavailable/InvalidStore, skipped child links, malformed hashed fail-closed, inclusive resource budgets, cancellation, fresh reads, and non-atomic source replacement.
- [ ] State supported .NET TFMs, managed-only dependencies, eight nupkg/seven snupkg topology, stable published `1.16.0` installation versus Alpha-6 development builds, and deferred mixed-source sets, conversion/migration, CLI integration and JSON changes. Cross-link the runnable sample, package README, UC00 and roadmap.
- [ ] Check every guide code snippet against the compiling sample, all relative links against actual files, and the command/JSON claims against existing tests and roadmap. Commit the reviewed guide and cross-links.

## Task 4: Coordinated Alpha-6 and qualification

**Modify:** `Directory.Build.props`, all eight package `PackageReleaseNotes` in csproj files, current-version test assertions (including `UC05DevelopmentMetadataTests`, `UC01CatalogPackageContractTests`, command/version tests), root/package/sample README, `CHANGELOG.md`, `docs/VERSIONING.md`, both roadmaps and PR body. Keep historical `Alpha-5` fixtures outside the active 1.17 identity untouched.

**Interfaces:** Reuse `PackPackages.ps1`, `VerifyPackageArtifact.ps1`, `.github/scripts/verify-release-package.{sh,cmd}`, `VerifyDistribution.ps1`, six `BuildToolArchives.ps1` RIDs and existing HDB00 interoperability workflow.

- [ ] Add a failing `UC06DevelopmentMetadataTests` assertion for `1.17.0-Alpha-6` on all three TFMs, `1.0.0.0` assembly identity, eleven Catalogs types, exact three Catalogs dependency edges, and eight coordinated package versions. Change the central version and eight notes together; update only active 1.17 assertions. Run focused metadata tests across .NET 8/9/10, then `git diff --check`.
- [ ] Run fresh Release solution build with warnings as errors, complete solution tests on every TFM, and separately rebuilt BerkeleyDb suite. Pack eight nupkg/seven snupkg from one exact source/PDB commit and run the unchanged full package verification plus the new sample/consumer checks. Record exact test counts, warnings, API reconstruction and isolated consumer results.
- [ ] Verify existing Tools package, installed-tool smoke, and all six platform archive compositions and command inventories; keep their source and binary payload free of Catalogs. Exercise Windows/Linux/macOS PR jobs and applicable native Linux/macOS and managed Windows interoperability jobs on the implementation commit. Do not claim skipped steps passed.
- [ ] Self-review the complete UC06 diff inline, non-force push the implementation to draft PR #48, and update its body with SHA/tree and evidence. Only after exact-code CI is green, record accepted run links, skips and limitations in both roadmaps and PR; push a documentation-only acceptance commit. Leave the PR draft, unmerged, untagged and unpublished, with UC07 next.

## Review and handoff

UC00 fixes the public contract and the roadmap fixes UC06 deliverables. This plan only adds a sample, isolated consumer and guide around that accepted behavior. Review it before implementation under the roadmap's per-tranche gate; execute inline without subagents after approval.
