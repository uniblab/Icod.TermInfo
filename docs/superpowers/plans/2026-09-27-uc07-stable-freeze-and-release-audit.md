# UC07 Stable API Freeze and Release Audit Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:executing-plans` inline, task by task; do not use subagents. Steps use checkbox (`- [ ]`) syntax for tracking.
> **Status:** Proposed for review. UC06 Alpha-6 implementation `3327871eb22524fd38c9337b9dd41a41b232b814` and documentation head `f2b619c5376fb37b75e4418b53d1b984e86ad4c5` are accepted; stable `1.17.0` has not been built or qualified.

**Goal:** Freeze the accepted unified Catalogs API and reviewed Inspection/BerkeleyDb additions, promote the same feature source to coordinated stable `1.17.0`, and record a verifiable release audit without merging, tagging or publishing.

**Architecture:** Capture complete rich reflection manifests from the accepted Alpha-6 assemblies, pin them independently of the existing additive reconstruction checks, and run those checks against every supported TFM and packaged assembly. Promote metadata and release-facing documentation together while retaining the accepted reader/parser behavior. Qualify one exact stable candidate locally and in the established cross-host PR/native workflows, then record its observed evidence in a documentation-only acceptance commit.

**Tech Stack:** C# 13, .NET 8/9/10, PowerShell 5.1-compatible scripts, cmd/sh, `Icod.TermInfo.PublicApiSnapshot`, existing packaging/CI tooling; no Python.

**Spec:** [Approved UC00 contract](../../1.17.0-UC00-UNIFIED-CATALOG-CONTRACT.md), [1.17 roadmap, UC07](../../../Icod.TermInfo-1.17.0-Unified-Directory-Hashed-Catalogs-Roadmap.md), and accepted [UC06 qualification](../../../Icod.TermInfo-1.17.0-Unified-Directory-Hashed-Catalogs-Roadmap.md#accepted-uc06-evidence). The roadmap requires review of this task plan before implementation.

## Global constraints

- Keep the UC06 feature/API source unchanged. No mixed-source sets, migration, JSON v7, new command switches, native production dependencies or changes to the five existing commands.
- Preserve `1.0.0.0` reusable assembly identity, C# 13, `net8.0;net9.0;net10.0`, existing Runtime/Source/Inspection/BerkeleyDb dependency directions, LGPL metadata, eight nupkg, seven snupkg and six tool archives. Tools must not acquire Catalogs.
- Preserve frozen 1.0–1.16 manifests, historical fixture versions, JSON v1–v6 fingerprints and additive UC01/UC03/UC04 reconstruction. Pin new complete 1.17 surfaces separately; never bless an accidental API delta by regenerating historical baselines.
- Coordinate **all eight** package versions and release notes at exactly `1.17.0`. Update only assertions that describe the current build; leave Alpha-1 through Alpha-6 historical fixtures and accepted run records intact.
- Keep install examples on the actually published `1.16.0` until the maintainer publishes 1.17; label `1.17.0` as a qualified stable **candidate** wherever a reader could confuse build identity with NuGet availability.
- Do not merge, tag, create a GitHub Release, publish to NuGet/GitHub Packages or turn PR #48 ready for review. Maintainer authorization for those actions is separate.
- Build locally with serial MSBuild (`-m:1 -p:UseSharedCompilation=false`) in the restricted workspace; CI provides independent Windows/Linux/macOS and applicable native evidence.

## Review focus

1. A mutable additive manifest can reconstruct an old API while the complete current API drifts. Task 1 pins the full Catalogs/Inspection/BerkeleyDb manifest and its independent fingerprint, and mutation tests reject an extra or changed member.
2. Stable promotion can replace `Alpha-6` inside a historical fixture or leave an active version assertion stale. Task 2 distinguishes current-version assertions from historical release evidence and runs the complete solution after the coordinated change.
3. A stable version string can make an unpublished package appear installable. Task 2 checks published `1.16.0` install examples separately from the `1.17.0` candidate identity.
4. A successful interoperability workflow can skip all native probes because of its change-scope classifier. Task 4 reads job **steps**, records the actual skips, and uses the last exact unchanged-production-code native run only when appropriate.
5. A green library package gate can miss the Tools payload or one RID. Tasks 3 and 4 check eight nupkg/seven snupkg, the installed tool on three hosts, all six archive jobs, and the unchanged command inventory.

---

## Task 1: Freeze complete 1.17 public surfaces

**Create:** `docs/1.17.0-CATALOGS-PUBLIC-API-BASELINE.txt`, `docs/1.17.0-INSPECTION-PUBLIC-API-BASELINE.txt`, `docs/1.17.0-BERKELEYDB-PUBLIC-API-BASELINE.txt`, `docs/1.17.0-PUBLIC-API-FREEZE.md`, `tests/Icod.TermInfo.Catalogs.Tests/src/UC07ApiFreezeTests.cs`.
**Modify:** `.github/scripts/verify-release-package.sh`, `.github/scripts/verify-release-package.cmd`, `.github/scripts/verify-berkeleydb-package.ps1`, `tools/catalogs-package-verifier/Program.cs`, `tools/inspection-package-verifier/Program.cs`, `tools/berkeleydb-package-verifier/Program.cs` only where needed to enforce the new complete baselines against compiled and packaged assemblies. Keep the established historical reconstruction in each gate.

**Interfaces:** `Icod.TermInfo.PublicApiSnapshot` supports `--write <baseline-path> <assembly-path>`, `--check <baseline-path> <assembly-path>` and `--compare <assembly-a> <assembly-b>`. A normalized-LF SHA-256 of each complete manifest is the 1.17 fingerprint. Expected exported types: Catalogs **11**, Inspection **108**, BerkeleyDb **14**; verify these against the live accepted Alpha-6 assemblies before freezing.

- [ ] Write `UC07ApiFreezeTests` to require the three complete 1.17 files, correct type counts/assembly identity, equality of each live reflection manifest with its baseline, and fingerprints recorded in the freeze document. Run on net10.0 before adding the files; observe a focused RED because the 1.17 freezes are absent, not because existing code is broken.
- [ ] Generate all three manifests from the accepted UC06 Alpha-6 assemblies with `public-api-snapshot --write`; compare the generated Catalogs manifest with UC01 plus the exact UC04 addition, Inspection with the exact UC01 removal and frozen 1.14 reconstruction, and BerkeleyDb with the exact UC03 removal and frozen 1.16/1.15 reconstruction. Record the observed complete hashes and approved additive blocks in `docs/1.17.0-PUBLIC-API-FREEZE.md`. Do not hand-edit generated baseline contents.
- [ ] Add focused mutations to `UC07ApiFreezeTests`: an extra type or changed member in a copy of any complete manifest must fail the exact live/baseline comparison even when older reconstruction fixtures are still checked. Test CRLF versus LF normalization on a copy without loosening type/member equality. Run the focused tests across .NET 8/9/10; expect GREEN and identical complete fingerprints.
- [ ] Extend the existing sh/cmd (and BerkeleyDb PowerShell) checks so Staging and Release run `--check` for each TFM's live assembly. In the three package verifiers, compare `PublicApiSnapshot.Program.CreateManifest` from each `lib/net{8,9,10}.0/*.dll` against its complete 1.17 baseline before retaining the UC01/UC03/UC04 historical reconstruction and JSON checks. Prove a deliberately altered *temporary* baseline or package input is rejected before restoring normal checks. Run `git diff --check` and commit the freeze with its verification gates.

## Task 2: Promote coordinated stable candidate identity

**Create:** `tests/Icod.TermInfo.Catalogs.Tests/src/UC07StableMetadataTests.cs`.
**Modify:** `Directory.Build.props`, all eight coordinated `.csproj` `PackageReleaseNotes`, only current-build version assertions in `tests/` (including UC01–UC06 Catalogs metadata, HDB release checks and command/version checks), `README.md`, `Icod.TermInfo.Catalogs/README.md`, `CHANGELOG.md`, `docs/VERSIONING.md`, `docs/1.17.0-UNIFIED-CATALOG-GUIDE.md` as release-facing wording requires. Preserve published-install instructions and historical snapshots.

**Interfaces:** `IcodTermInfoSuiteVersion` is the sole coordinated version property. The stable candidate is `1.17.0`, with eight nupkg/seven snupkg, Catalogs' exact Runtime/Inspection/BerkeleyDb package dependencies, `1.0.0.0` reusable assembly identities and the Task 1 complete API freezes.

- [ ] Add `UC07StableMetadataTests`: assert the central version and all eight package versions/release notes equal or name `1.17.0`, all reusable assembly identities remain `1.0.0.0`, Catalogs still exports eleven types and has exactly its three approved dependency edges, and published installation examples still say `1.16.0`. Run on .NET 8/9/10 before promotion; observe RED on the current Alpha-6 identity.
- [ ] Advance the central property, eight package notes, and **only active** 1.17 identity assertions together. Retain historical Alpha fixtures, accepted UC06 SHA/test counts, and published `1.16.0` install examples. Update README/package README/guide/changelog/versioning language to distinguish the qualified candidate from published stable. Do not change production `.cs` implementation files or public interfaces.
- [ ] Run focused stable metadata tests and all currently failing version/command tests, then `dotnet build Icod.TermInfo.sln -t:Rebuild -c Release -m:1 -p:UseSharedCompilation=false -p:ContinuousIntegrationBuild=true -warnaserror` and `dotnet test Icod.TermInfo.sln -c Release --no-build --no-restore -m:1 -p:UseSharedCompilation=false`. Verify no production `.cs` diff from accepted UC06 (`git diff --name-only 3327871..HEAD` restricted to product source directories). Commit the coherent stable-candidate identity.

## Task 3: Draft audit and qualify local artifacts

**Create:** `docs/1.17.0-RELEASE-AUDIT.md`.
**Modify:** release-facing cross-links in `README.md`, `docs/VERSIONING.md` and both roadmaps for **candidate/in progress** status; no acceptance marking yet.

**Interfaces:** Use the complete frozen manifests from Task 1 and stable `1.17.0` metadata from Task 2. The audit records observed commit/tree, commands, test counts, warnings/errors, eight package/seven symbol identities, three .NET targets, dependency topology, source/command/JSON compatibility, consumer and archive results, exact CI links/artifact hashes, limitations and publication state.

- [ ] Draft the audit from the actual freeze and accepted UC06 evidence, clearly marking unrun stable qualification as pending. Record feature scope and deferred migration/JSON/CLI work, eleven/108/14 type counts, API hashes, historical reconstruction, source provenance semantics, cancellation and non-atomic-read limits. Link UC00, public guide, sample, frozen manifests and active roadmap. Commit the pending audit so qualification uses a clean exact source/PDB commit.
- [ ] Freshly rebuild the Release solution with warnings as errors, then rebuild the BerkeleyDb tests separately and run all three frameworks; record precise passed/skipped/failed counts. Pack **eight** nupkg and **seven** snupkg from that same clean stable source/PDB commit using `packaging/PackPackages.ps1 -Configuration Release -OutputDirectory <fresh-directory>`; run `packaging/VerifyPackageArtifact.ps1 -ArtifactDirectory <same-directory> -Configuration Release`. Verify complete current and reconstructed APIs, sample, isolated Catalogs consumer, historical compatibility, licenses, symbols, Source Link, and managed-only payloads.
- [ ] Check the Tools nupkg has the established five commands and no Catalogs dependency. Run the existing six-RID `BuildToolArchives.ps1` composition/command checks locally where supported; rely on matching-host CI archive jobs for cross-host execution, and distinguish archive composition from an actual executed host smoke. Add observed local commands, counts, and limitations to the audit as a documentation-only commit; describe the local artifact commit separately from this documentation head. Self-review before PR qualification.

## Task 4: Exact stable candidate qualification and acceptance record

**Modify:** `docs/1.17.0-RELEASE-AUDIT.md`, both roadmaps, `CHANGELOG.md` or README only if observed release evidence requires corrections, and draft PR #48 body. No product change after the exact code/artifact candidate is qualified; if a correction is necessary, create a new candidate and rerun its gates.

**Interfaces:** Existing [pull-request workflow](../../../.github/workflows/pull-request.yaml) covers three platform build/test jobs, Linux Staging/Release package gates, three installed tool jobs and six matching archive jobs. [HDB00 interoperability workflow](../../../.github/workflows/hdb00-interoperability.yml) has native Linux/macOS plus Windows managed readback, subject to scope filters. The audit references the exact stable candidate commit/tree and observed workflow run IDs, artifact IDs/hashes and skips.

- [ ] Push the stable candidate to the existing **draft** PR by fast-forward only; record exact SHA/tree. Review all job and step conclusions on that commit: warnings-as-errors builds and solution tests on Windows/Linux/macOS, Staging/Release artifacts on Linux, three installed-tool smokes, six archives, Windows Inspection PowerShell, and any applicable native/managed interoperability steps. Never label a skipped step passed. If CI fails, diagnose the actual cause, repair and rerun on a new exact commit.
- [ ] Complete the audit with observed local and CI counts, complete manifest fingerprints, immutable 1.0–1.16 reconstruction, JSON v1–v6/command stability, actual package/consumer/archive evidence, exact artifact hashes if obtainable, platform-specific skips and known limitations. Mark UC07 accepted in **both** roadmaps only once exact-code qualification is green; retain a distinction between a qualified stable candidate and published `1.17.0`.
- [ ] Perform one whole-diff author review inline without subagents, update the PR body with exact SHA/tree and linked results, and fast-forward a documentation-only acceptance commit. Confirm its changed paths contain only documentation and that PR #48 remains open and draft. If acceptance-head CI runs, record its actual result and any change-scope skips separately from the exact-code evidence.
- [ ] Leave merge, tag, GitHub Release, NuGet/GitHub Packages publication and changing draft status for separately authorized maintainer actions. Hand off the qualified stable candidate and remaining publication boundary; never perform a release by implication.

## Review and handoff

UC07 freezes and qualifies the accepted behavior; it does not add another catalog feature. This plan needs the roadmap's per-tranche review before implementation. Execution remains sequential and inline without subagents after approval.
