# HDB09 API Freeze, Documentation, and Stable Promotion Design

**Status:** APPROVED

**Date:** 2026-09-17

## Purpose

HDB09 closes the Icod.TermInfo 1.15 line without adding behavior beyond the
accepted `1.15.0-Alpha-8` contract. It freezes the optional Berkeley DB
package's exact public API and dependency boundary, completes the user-facing
acquisition and compatibility documentation, adds the roadmap-promised
deterministic sample, audits security/resource/ecosystem/release boundaries,
and promotes the coordinated package family to stable `1.15.0`.

PR #45 remains draft, open, unmerged, untagged, and unpublished throughout
this tranche. Stable promotion changes package identity and release-facing
authorities only; it is not permission to merge, tag, or publish.

## Accepted starting point

HDB09 begins at exact HDB08 head
`78168f06a357034315fbe3de472375af1c7b0fbb`. That head passed the complete
12-job pull-request workflow and the genuine three-host HDB00 workflow. The
accepted implementation is pure managed, read only, Hash-v9 scoped, bounded,
cross-target equivalent, and package-qualified on Windows, Linux, and macOS.

## Release-freeze contract

The final freeze has four independent layers.

1. **BerkeleyDb API identity.** Check in the complete
   `Icod.TermInfo.PublicApiSnapshot/v1` manifest produced from the accepted
   Alpha-8 assembly. Every candidate must match it exactly on net8.0,
   net9.0, and net10.0.
2. **Coordinated historical identity.** Runtime, Source, Compiler, Termcap,
   and Inspection keep their existing frozen API reconstruction gates. HDB09
   explicitly verifies that BerkeleyDb adds no member to Runtime and that the
   current Runtime still reconstructs the frozen 1.0 manifest.
3. **Dependency direction.** BerkeleyDb depends only on matching-version
   Runtime; Runtime and the reusable sibling packages do not depend on
   BerkeleyDb. `infocmp` and `toe` remain the only direct command consumers.
   No native assets or third-party production package dependencies are added.
4. **Distribution identity.** The exact seven nupkg/seven snupkg family,
   package-only all-TFM consumers, installed-tool smoke, and six RID archives
   remain authoritative. Stable promotion must pass a fresh complete matrix.

The final API manifest is generated and reviewed from the accepted prerelease
artifact, never reconstructed manually from source declarations.

## Permanent HDB09 closure tests

`Hdb09ReleaseClosureTests` lives with the BerkeleyDb tests and locks:

- the complete API baseline and its normalized-LF SHA-256;
- the baseline check in the managed/package verification path;
- net8.0/net9.0/net10.0 equivalence;
- exact production project references and absence of native assets;
- Runtime's unchanged 1.0 baseline gate;
- the deterministic sample topology and controlled fixture generation;
- acquisition, Hash-v9 compatibility, security/resource, ecosystem, and
  release audit authorities;
- stable release notes, README/versioning/compatibility/roadmap status, and
  the new changelog entry; and
- HDB00 sensitivity for every HDB09 file capable of changing the qualified
  BerkeleyDb contract.

Tests are introduced before their authorities. The first exact-head workflow
must fail for the intended missing HDB09 artifacts. The minimal implementation
then makes the same suite green.

## Sample design

Add `samples/Icod.TermInfo.BerkeleyDb.Sample` as a deterministic,
non-interactive, all-TFM executable. It references only
`Icod.TermInfo.BerkeleyDb`, creates a small controlled Hash-v9 store in a
temporary directory, resolves an alias with
`BerkeleyDbTerminalDescriptionProvider`, and prints selected parsed
capabilities. It does not inspect ambient terminfo state, invoke native
Berkeley DB, write through the production package, or add a package dependency.

The fixture writer is sample-owned demonstration code. Production remains
strictly read only. CI executes the sample on all reusable target frameworks
through the established verification path.

## Documentation authorities

HDB09 adds:

- `docs/1.15.0-BERKELEY-DB-HASHED-ACQUISITION-GUIDE.md` for provider,
  catalog, discovery, command, error, and ownership contracts;
- `docs/1.15.0-BERKELEY-DB-HASH-V9-COMPATIBILITY.md` for the precise accepted
  file-format/record-envelope subset and explicit exclusions;
- `docs/1.15.0-BERKELEY-DB-SECURITY-AND-RESOURCE-AUDIT.md` for all bounds,
  mutation detection, cancellation, permissions, parsing, and non-atomicity;
- a finalized ecosystem/dependency audit tied to the accepted implementation;
- `docs/1.15.0-BERKELEY-DB-PUBLIC-API-FREEZE.md` and the complete baseline;
- `docs/1.15.0-RELEASE-AUDIT.md` for tranche evidence and stable state; and
- `CHANGELOG.md`, beginning with 1.15 and linking older versioned audits rather
  than inventing unrecoverable historical prose.

The root, package, command, packaging, samples, versioning, compatibility, and
roadmap authorities are synchronized to stable 1.15. HDB09 preserves frozen
JSON schemas and all command semantics outside the accepted explicit hashed
paths.

## Delivery stages

### Stage A: RED freeze witness

Add only the permanent HDB09 closure tests and HDB00 classifier expectations.
Push the exact head and require an intentional workflow failure caused by the
missing API baseline, sample, documentation, and stable authorities. Record the
run and failing assertions.

### Stage B: Alpha-8 closure implementation

Generate the exact BerkeleyDb API manifest from the accepted Alpha-8 assembly,
wire it into verification, add the sample and documentation/audits/changelog,
and make the closure suite green while the coordinated version remains
`1.15.0-Alpha-8`. Require the normal 12 jobs and genuine HDB00 three-host jobs.

### Stage C: stable promotion

Change the coordinated version to exactly `1.15.0`, update all coordinated
package release notes and stable release authorities, and change no production
behavior or public API. Require a fresh exact-head normal 12-job qualification
and genuine three-host HDB00 qualification.

### Stage D: evidence closure

Record exact commits, workflow IDs, counts, API fingerprint, and preserved
boundaries in the release audit/roadmap/PR body. Requalify that documentation
head if any qualified path changes. Verify PR #45 remains draft, open,
unmerged, untagged, and unpublished.

## Rejected alternatives

- A documentation-only freeze is insufficient because it cannot reject API or
  dependency drift.
- Extending the conventional acquisition sample would introduce an optional
  package dependency into a sample whose purpose is Runtime-only acquisition.
- A single large stable commit would obscure the RED witness, accepted
  prerelease API derivation, and stable-only identity change.
- Tagging or publishing during HDB09 would exceed the authorized scope.
