# HDB09 API Freeze, Documentation, and Stable Promotion Plan

> Execute this plan in the existing PR #45 workspace. Use test-first
> RED -> GREEN commits and exact-head GitHub Actions evidence. Do not merge,
> tag, publish, or mark the PR ready for review.

**Goal:** Freeze the accepted 1.15 BerkeleyDb contract, complete release-facing
documentation and the deterministic sample, and promote the coordinated family
to stable `1.15.0` without behavior changes.

**Design:**
`docs/superpowers/specs/2026-09-17-hdb09-api-freeze-documentation-stable-promotion-design.md`

**Starting head:** `78168f06a357034315fbe3de472375af1c7b0fbb`

## Constraints

- C#, Windows PowerShell 5.1-compatible PowerShell, cmd, and sh only.
- No new Python, C, or C++.
- Preserve public API, dependency, JSON, command, pure-managed, and read-only
  boundaries.
- Keep `1.15.0-Alpha-8` until the Alpha-8 closure is independently green.
- Use the accepted Alpha-8 assembly to generate the complete API baseline.
- Require exact-head normal 12-job and genuine HDB00 three-host qualification
  for accepted Alpha-8 closure and stable promotion.

## Task 1: Permanent closure tests (RED)

- [x] Add
  `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb09ReleaseClosureTests.cs`.
- [x] Assert the exact API baseline/freeze wiring, package/dependency boundary,
  Runtime historical reconstruction, sample, docs/audits/changelog,
  release-authority structure, and HDB00 classifier coverage. Keep the
  Alpha-8 checkpoint satisfiable without claiming stable identity.
- [x] Add HDB09-sensitive paths and classifier self-tests before adding the
  authorities they name.
- [x] Commit and push the RED witness.
- [x] Verify the exact workflow fails for the intended missing HDB09 contract,
  with unrelated jobs remaining healthy.

## Task 2: Exact Alpha-8 API freeze (GREEN)

- [x] Generate
  `docs/1.15.0-BERKELEYDB-PUBLIC-API-BASELINE.txt` from the accepted Alpha-8
  net10.0 assembly with `tools/public-api-snapshot`.
- [x] Record its normalized-LF SHA-256 and exported-type count in
  `docs/1.15.0-BERKELEY-DB-PUBLIC-API-FREEZE.md`.
- [x] Extend `.github/scripts/verify-berkeleydb-package.ps1` to check the exact
  baseline in addition to its existing cross-TFM comparisons.
- [x] Keep the Runtime `docs/1.0.0-PUBLIC-API-BASELINE.txt` check and existing
  Source/Compiler/Termcap/Inspection compatibility chain unchanged.
- [x] Run the focused remote test and package verification until green.

## Task 3: Deterministic BerkeleyDb sample (GREEN)

- [x] Add `samples/Icod.TermInfo.BerkeleyDb.Sample` targeting
  `net8.0;net9.0;net10.0` with one project reference to BerkeleyDb.
- [x] Construct a controlled minimal Hash-v9 store in a temporary directory.
- [x] Resolve a controlled alias through the public provider and print stable
  selected capability output.
- [x] Add sample README and solution membership.
- [x] Wire deterministic all-TFM execution into the existing verification
  path without native dependencies or ambient host state.

## Task 4: Guides and audits (GREEN)

- [x] Add the acquisition guide.
- [x] Add the exact Hash-v9 compatibility document.
- [x] Add the security/resource-bound audit.
- [x] Finalize the existing ecosystem/dependency audit against the accepted
  implementation and qualification evidence.
- [x] Add `CHANGELOG.md` with a 1.15 entry and links to older release audits.
- [x] Add the initial 1.15 release audit while still identifying Alpha-8 as
  the accepted feature/API source.

## Task 5: Release-facing synchronization (GREEN)

- [x] Update root README feature inventory/status/quick start.
- [x] Update BerkeleyDb, command, router, packaging, and samples READMEs where
  their current-release language changes.
- [x] Update `docs/VERSIONING.md`, `docs/COMPATIBILITY.md`, the 1.15 roadmap,
  and the post-1.0 roadmap.
- [x] Preserve exact historical documents and user-authored tag edits.
- [x] Keep the coordinated version at Alpha-8 for this checkpoint.

## Task 6: Alpha-8 closure qualification

- [x] Push the complete Alpha-8 closure head.
- [x] Require 12/12 normal PR jobs green.
- [x] Require 3/3 genuine HDB00 jobs green with native Linux/macOS and
  transported Windows evidence executed, not skipped.
- [x] Record the exact head, run IDs, API fingerprint, package counts, and
  archive counts.

## Task 7: Stable `1.15.0` promotion

- [x] Add a focused stable-promotion test first and prove that it fails while
  the coordinated version and release authorities still identify Alpha-8.
- [x] Change only coordinated version/release-facing identity from
  `1.15.0-Alpha-8` to `1.15.0`.
- [x] Update all seven coordinated package release notes for the 1.15 stable
  contract, without production behavior changes.
- [x] Update release audit/roadmaps/readmes/changelog to stable state.
- [x] Commit and push the promotion.
- [x] Require fresh exact-head 12/12 normal and genuine 3/3 HDB00 green runs.

## Task 8: Final evidence and review

- [x] Update the release audit and 1.15 roadmap with final exact evidence.
- [ ] If evidence edits touch qualified paths, rerun exact complete gates.
- [ ] Review the full diff against this plan and address all critical or
  important findings.
- [ ] Update the PR body with HDB09 RED/GREEN/stable evidence.
- [ ] Fetch PR #45 and verify: open, draft, unmerged, stable head exact.
- [ ] Verify no `v1.15.0` tag or package publication was created.

## Completion evidence

Record here during execution:

- RED head/run: `1525096d1a5925c2816159f7f7b7c826dfd7013b` / normal
  `35176822414`, HDB00 `35176822401`
- Alpha-8 GREEN head/run: `406891bff059bafb4dba82b1de64e33861c78773` /
  normal `35178539117`; genuine HDB00 implementation run `35178105916`
- Stable promotion head/run:
  `10851c380b2df7eba35ea2151ecb7b982a8a7a2d` / normal
  `35182018598`, genuine HDB00 `35182018597`
- Final documentation head/run: pending
- BerkeleyDb API SHA-256: `f519600aa4085d07c2d20bd8dc7a32c4dc06a43f4e361554b205ce2f97a8bf36`
- Final PR state: pending
