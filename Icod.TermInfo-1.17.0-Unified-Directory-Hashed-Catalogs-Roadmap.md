# Icod.TermInfo 1.17.0 — Unified Directory/Hashed Catalogs Roadmap

**Development line:** `1.17.0`

**Theme:** Unified directory/hashed catalogs

**Status:** DEVELOPING — UC00 through UC03 accepted; UC04 is next

**Stable predecessor:** `1.16.0` (published)

**Initial implementation version:** `1.17.0-Alpha-1`

**Current coordinated version:** `1.17.0-Alpha-3`

**Language / targets:** C# 13; `net8.0`, `net9.0`, `net10.0`

**Execution:** sequential, inline development; no subagents

**Goal:** Let a caller enumerate one explicitly selected conventional terminfo
directory or supported ncurses Hash-v9 file through one immutable, read-only
catalog contract, with honest publication identities, provenance, and diagnostics.

**Architecture:** Compose the existing directory inspector, managed hashed
catalog reader, and Runtime parser. The approved optional composition package sits above Inspection and BerkeleyDb.
UC00 fixes this boundary and opt-in bounded acquisition.

**Specification:** This roadmap defines release scope and acceptance criteria.
The [main roadmap](Icod.TermInfo-Post-1.0-Development-Roadmap.md) owns release
sequencing. The approved detailed design is the
[UC00 contract](docs/1.17.0-UC00-UNIFIED-CATALOG-CONTRACT.md), with a
[UC01 implementation plan](docs/superpowers/plans/2026-09-27-uc01-unified-catalog-foundation.md).

**Global constraints:** Preserve released API behavior, JSON v1–v6, command
output, assembly identity `1.0.0.0`, and existing dependency directions. Reuse
managed Hash-v9 support; introduce no native production dependency. Keep LGPL
licensing and attribution intact. Use C#, PowerShell 5.1, and shell tooling.

**Review focus:** Physical files versus logical publication names; truthful
alias reporting; partial results versus failed acquisition; limits enforced
before allocation; deterministic output; package and compatibility impact.

## 1. Release boundary

The approved feature is **Unified directory/hashed catalogs**. This supersedes
the broader migration/catalog-automation assignment in the 1.16 roadmap.

| Included in 1.17 | Deferred; no release assigned |
| --- | --- |
| One explicit directory or Hash-v9 source per read | Ordered mixed-source database sets and precedence |
| Common immutable entry and issue model | Cross-container comparison and conflict-resolution policy |
| Published canonical/alias identities and provenance | Directory/hashed conversion, migration, and synchronization |
| Deterministic ordering, cancellation, resource limits | Hashed-aware JSON, JSON v7, or new automation CLI switches |
| Library sample, package consumer, compatibility qualification | In-place mutation, repair, new Berkeley DB formats or native bindings |

Existing directory-only database-set, comparison, planning, and JSON facilities
continue unchanged. A unified catalog does not automatically become an input to
those APIs. Existing `toe` listing behavior also remains unchanged; command
integration needs a separate scope decision if it would add public behavior.

This release reuses the difficult storage work completed in 1.15 and 1.16. It
does not reopen hash functions, bucket/page layout, overflow chains, writer
publication, or native compatibility decisions.

## 2. Current implementation and proposed ownership

| Existing authority | Contract to preserve | Work needed |
| --- | --- | --- |
| `Icod.TermInfo.Inspection/src/TermInfoDatabaseInspector.Catalog.cs` | Conventional layout inspection, Runtime parsing, file issues, cancellation | Map physical occurrences into publication identities; resolve aggregate traversal bounds |
| `Icod.TermInfo.Inspection/src/TermInfoDatabaseCatalog*.cs` | Physical compiled files and duplicate canonical identities | Preserve these types; do not reinterpret their entries as hashed keys |
| `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalCatalogReader.cs` | Bounded managed hashed enumeration | Adapt actual canonical/alias keys and parsed descriptions |
| `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalCatalogReaderOptions.cs` | Database bytes, physical record count, index hops, parser limits | Preserve these distinct budgets in the composed read |
| `Icod.TermInfo.BerkeleyDb/src/BerkeleyDbTerminalCatalogEntry.cs` | Actual key, entry kind, parsed `TerminalDescription` | Retain key identity; do not claim access to raw bytes through this API |
| Runtime `CompiledTermInfoParser` | Compiled-entry semantics | Remain the sole semantic parser |

The approved package is named **`Icod.TermInfo.Catalogs`**. It
references Runtime, Inspection, and BerkeleyDb; Inspection retains its
Runtime/Source dependencies and BerkeleyDb retains its Runtime-only dependency.
No existing library references the new package. Compiler is only needed by
tests/samples that construct fixtures, not by production catalog acquisition.

This costs an additional package, API baseline, verifier, consumer, and solution
entry. The user approved that cost in UC00. Putting the facade in Inspection
would add a BerkeleyDb dependency to existing consumers; putting it in BerkeleyDb
would violate its Runtime-only boundary. Neither is the default plan.

Approved contract roles are an explicit source descriptor, immutable read
options, a reader with a cancellation overload, immutable catalog/entry/issue
results, and typed source/entry/status classifications. The authoritative UC00 contract records exact signatures, defaults,
validation order, enum values, exception behavior, and the reviewed type count.

## 3. Required observable behavior

### 3.1 Explicit source and publication identity

- A caller supplies the source path and chooses conventional directory or
  supported hashed file. No ambient provider search, suffix guessing, or hidden
  fallback to another source is part of this release.
- Normalize source paths to absolute paths once per read. Preserve storage kind
  and the distinction between an absent source and an empty readable source.
- Each successful row represents an **observed publication occurrence**. Keep
  the lookup/publication name separate from `TerminalDescription.Name` and its
  declared aliases. A declaration alone does not prove an alias was published.
- For a directory, derive the publication name from the inspected file's name
  and validate it against the parsed canonical name or declared aliases and
  supported layout rules. Preserve its actual file path. Invalid placement
  produces an issue, not an invented usable entry.
- For a hashed source, use the reader's actual key and canonical/alias kind.
  Provenance is the database file plus key; do not invent a per-entry file path
  or expose an unstable physical page number as public identity.
- Preserve multiple directory occurrences of the same publication name. Report
  duplication without choosing a winner, merging descriptions, or introducing
  cross-source precedence. Existing duplicate-*canonical*-name behavior remains
  unchanged in the old Inspection API.
- Names use exact ordinal comparison, without case folding or Unicode
  normalization. Native filesystem access still follows platform semantics.

### 3.2 Results, ordering, and failures

- Results and options are immutable snapshots. Sort entries by publication name,
  then storage locator with ordinal comparison and an explicit final kind tie
  breaker. Sort issues by locator, stable kind, and ordinal message text.
  Guarantee ordering for equivalent observations, not identical OS error text
  across operating systems.
- Distinguish complete, partial, missing, unsupported, and unavailable reads.
  UC00 freezes the exact representation and issue mapping. Do not label a
  partially inspected directory as a complete catalog.
- Recoverable directory candidate failures retain valid sibling observations
  and typed issues. Preserve skipped-link evidence and the current policy of
  not traversing child links/junctions/reparse points.
- Corrupt or unsupported hashed containers fail closed; never expose a partially
  salvaged hashed catalog. Missing sources, permission failures, source-kind
  mismatch, invalid hashed data, and configured limits remain distinguishable.
  UC00 groups malformed/unsupported hashed content under one stable issue code
  because the existing reader does not expose a reliable finer classification.
- Caller argument errors and cancellation propagate as exceptions. A catalog
  budget overrun fails the operation explicitly; it must not return a silently
  truncated success. UC00 defines how known lower-layer failures map to these
  rules without catching arbitrary programming errors.
- Each read acquires fresh observations. There is no hidden cache and no promise
  of an atomic directory snapshot or cross-process consistency. Document source
  replacement/races, handle lifetime, and the guarantees inherited from readers.

### 3.3 Bounds and cancellation: a mandatory design gate

The existing directory inspector uses `Directory.GetDirectories` and
`Directory.GetFiles` and returns a materialized catalog. Its per-entry parser
limit is **not** a whole-directory resource bound. Checking the result count
after calling that API cannot enforce a pre-allocation traversal limit.

UC00 specifies budgets for discovered filesystem candidates, retained entries,
retained diagnostics, and aggregate parsed bytes, as well as the existing hashed
database-byte, physical-record, index-hop, and per-entry limits. Count rejected
candidates and aliases where they consume resources. Specify defaults, inclusive
boundary behavior, checked arithmetic, and cancellation checkpoints before reads,
during traversal/normalization, and before returning a result.

The approved solution is a narrowly additive bounded Inspection acquisition
entry point backed by shared traversal internals. The UC00 audit also found that
typed hashed limit failures, cumulative decoded-byte accounting, and cancellation
during image reads require an opt-in bounded BerkeleyDb method in UC03. Existing
methods retain their behavior; historical APIs are reconstructed after removing
only the reviewed additions. UC00 approves these deltas and the allocation
accounting. If implementation
cannot preserve existing behavior within this scope, stop this
gate and revise the design explicitly. Do not copy a second directory parser or
claim that a wrapper around eager enumeration solves this problem.

## 4. Development sequence

Every tranche requires a reviewed task plan with exact files, interfaces, focused
tests, and commands before implementation. Execute inline with the
`superpowers:executing-plans` workflow. Start behavior changes with meaningful
failing tests, verify the focused behavior, then commit. Never mark a tranche
accepted based only on a plan or an unobserved CI run.

| Tranche | Planned version | Deliverable | Depends on | Status |
| --- | --- | --- | --- | --- |
| UC00 | Planning; retain `1.16.0` build identity | Contract, package decision, bounded-acquisition design | Published 1.16 | Approved by user, 2026-09-27 UTC |
| UC01 | `1.17.0-Alpha-1` | Package/model foundation and approved bounded acquisition seam | UC00 | Accepted at `656acf9` |
| UC02 | `1.17.0-Alpha-2` | Conventional-directory adapter | UC01 | Accepted at `965c8d2` |
| UC03 | `1.17.0-Alpha-3` | Hash-v9 adapter | UC01, UC02 contract fixtures | Accepted at `e4dcf7e` |
| UC04 | `1.17.0-Alpha-4` | Unified reader and cross-format behavioral qualification | UC02, UC03 | Pending |
| UC05 | `1.17.0-Alpha-5` | Resource, failure, cancellation, and compatibility hardening | UC04 | Pending |
| UC06 | `1.17.0-Alpha-6` | Samples, package consumers, distribution and guide | UC05 | Pending |
| UC07 | `1.17.0` after accepted Alpha-6 | Exact API freeze and stable release audit | UC06 | Pending |

### UC00 — contract and architecture decision

- [x] Audit the concrete reader/model files listed in section 2 and existing
  `I03DatabaseCatalogTests`, `Hdb05CatalogReaderTests`, and HDB hardening fixtures.
- [x] Write `docs/1.17.0-UC00-UNIFIED-CATALOG-CONTRACT.md`: dependency graph,
  exact candidate API, source/identity examples, duplicate policy, failure table,
  link/root policy, deterministic ordering, numeric limits, and allocation audit.
- [ ] Accept the proposed additive Inspection/BerkeleyDb seams and package name. Identify every
  frozen API/schema/dependency check affected; no blanket baseline replacement.
- [x] Specify fixture expectations for canonical-only entries, published aliases,
  declared-but-unpublished aliases, duplicate layouts, malformed siblings, and
  invalid hashed indexes. Separate format parity from intentional storage
  differences such as skipped directory links.
- [x] Draft the UC01 plan under `docs/superpowers/plans/`, including
  exact signatures and test commands.
- [x] User approved the UC00 contract and UC01 implementation plan on 2026-09-27 UTC.

**Exit:** The public model and acquisition limits are reviewable and implementable
without reopening storage engineering. No unresolved package/bounds decision is
carried into production implementation.

### UC01 — package and immutable model foundation

- [x] Add `Icod.TermInfo.Catalogs/`, its
  test project, explicit solution entries, and package/API verification scaffolding.
- [x] Implement the approved immutable source/options/result model and validate
  nulls, names, paths, enum values, option ranges, and defensive copying.
- [x] Add the reviewed bounded Inspection seam with focused traversal-budget
  tests; preserve old overload outputs, diagnostics, and cancellation contracts.
- [x] Introduce Alpha-1 through the existing centralized versioning process;
  preserve reusable assembly identity and equivalent APIs on all three TFMs.

**Exit:** Package dependency checks pass; model invariants and bounded discovery
are tested; existing Inspection and BerkeleyDb baselines remain reconstructible.

### UC02 — directory adapter

The [UC02 implementation plan](docs/superpowers/plans/2026-09-27-uc02-conventional-directory-adapter.md)
was approved on 2026-09-27 UTC. The internal adapter, 31 behavioral/boundary cases,
and one compiled identity check are complete. Alpha-2 is accepted at `965c8d2`
after local and code-head CI qualification. Execution remains inline, without
subagents.

- [x] Adapt bounded Inspection observations into the new publication model,
  reusing Runtime parsing and shared directory-layout validation.
- [x] Cover literal/hex directories, canonical and alias files, misplaced files,
  duplicate occurrences, links, empty/missing roots, and malformed siblings.
- [x] Verify partial status, original physical provenance, deterministic ordering,
  cancellation, and budgets during discovery, parsing, and issue accumulation.

**Exit:** A directory can be read through the proposed common contract without
inventing aliases, choosing duplicate winners, or changing legacy catalogs.

### UC03 — hashed adapter

The [UC03 implementation plan](docs/superpowers/plans/2026-09-27-uc03-bounded-hashed-catalog-adapter.md)
was approved on 2026-09-27 and is complete. Physical acquisition, logical budgets
and ReadBounded, the internal adapter, exact API compatibility, and Alpha-3
integration are accepted at `e4dcf7e` after local and code-commit CI qualification.
Execution and author self-review remained inline without subagents.

- [x] Add the UC00-specified opt-in `ReadBounded` method and two supporting types
  to `BerkeleyDbTerminalCatalogReader`, sharing existing reader/decoder internals.
  Preserve the old `Read` methods and reconstruct the frozen 1.16 API exactly.
- [x] Compose bounded acquisition and immutable options; map actual keys, kinds,
  parsed terminals, and database/key provenance.
- [x] Exercise canonical records, aliases/indexes, non-ASCII identities, chained
  buckets, and large records with existing qualified fixtures/writer output.
- [x] Verify missing files, malformed/unsupported images, index failures, each
  resource budget, cancellation, and fail-closed result semantics.

**Exit:** No duplicate Hash-v9 decoder or raw-byte API is introduced; bounded
hashed acquisition produces the agreed common model and diagnostics.

### UC04 — unified reader and parity

- [ ] Add the reviewed single-source dispatch and public cancellation overloads.
  Test explicit format mismatch; do not silently autodetect or fall back.
- [ ] Build equivalent directory/hashed fixtures from the same compiled records
  using Compiler and BerkeleyDb writer only in test infrastructure.
- [ ] Assert equal observed publication names, kinds, canonical identities, and
  capability semantics where publication sets match. Assert intentionally
  different provenance and storage-specific issues separately.
- [ ] Verify repeat reads, caller option snapshots, cultures, input order, and
  replacement behavior; document the absence of an atomic snapshot guarantee.

**Exit:** A consumer processes either storage format through the same API without
storage-specific branching over successful rows. No migration or JSON feature
is required to demonstrate the release's value.

### UC05 — adversarial and compatibility hardening

- [ ] Test each budget at its boundary and one beyond; include many rejected
  files/issues, large descriptions, aliases, early cancellation, and checked
  size arithmetic. Demonstrate enforcement before unbounded materialization.
- [ ] Test permission/race/link behavior with platform-aware fixtures and explicit
  skips only where the host cannot exercise the condition.
- [ ] Verify old directory catalogs, database sets, JSON v1–v6, `toe`, `infocmp`,
  and `tic` behavior remains unchanged; retain all 1.15/1.16 storage coverage.
- [ ] Run Windows/Linux/macOS qualification on all three TFMs. Reuse the existing
  native oracle on Linux/macOS for acquisition regression evidence; do not
  require native Berkeley DB in production or Windows consumer tests.

**Exit:** The failure/bounds matrix and compatibility evidence identify exact
commits, commands, outcomes, and any remaining limitations.

### UC06 — usable package, sample, and documentation

- [ ] Add `samples/Icod.TermInfo.Catalogs.Sample/` (or the UC00-approved name)
  showing the same read/print workflow for a directory and hashed file, including
  aliases, provenance, issues, cancellation, and custom limits.
- [ ] Add `docs/1.17.0-UNIFIED-CATALOG-GUIDE.md`; explain publication names versus
  declarations, skipped links, partial results, duplicate handling, fresh reads,
  and why unified catalogs do not yet imply migration or mixed-source sets.
- [ ] Add a package-reference-only consumer and verifier. Update the solution,
  package inventory, restore/dependency checks, symbol/license checks, and release
  scripts. If the new package is accepted, the family grows from seven nupkg/six
  library snupkg to eight nupkg/seven library snupkg; record this explicitly.
- [ ] Update root/package/sample READMEs, changelog, versioning/compatibility
  documentation, and roadmap status using actual supported behavior.
- [ ] Verify Tools and all six existing platform archives remain valid; do not
  ship the new library in Tools merely because it exists.

**Exit:** A clean package-only consumer and the documented sample work on all
three TFMs; published-artifact composition matches the approved dependency graph.

### UC07 — freeze and stable closure

- [ ] Freeze the complete new API and reviewed Inspection delta; retain historical
  reconstructions and unchanged JSON schema fingerprints.
- [ ] Produce `docs/1.17.0-RELEASE-AUDIT.md` with exact accepted commit/tree,
  qualification runs, test counts, packages/consumers, and known limitations.
- [ ] Promote only the accepted feature/API source to stable `1.17.0`; verify
  release metadata, warnings-as-errors builds, all frameworks/platforms, exact
  package consumers, and existing command archives.
- [ ] Update both roadmaps to accepted status only after evidence is recorded.
  Maintainer merge, tag, and NuGet publication remain separate authorized actions.

**Exit:** Stable artifacts are reviewable and qualified. Stable promotion does
not add behavior or enlarge the catalog scope.

## 5. Progress and change control

### Accepted UC03 evidence

UC03 implements bounded physical acquisition, `ReadBounded`, the internal hashed
adapter, exact additive API verification, and coordinated Alpha-3 metadata.
The implementation adds 59 BerkeleyDb and 40 Catalogs cases per framework.

- Full Release solution build: **zero warnings and errors**, with warnings as
  errors, serial MSBuild, and shared compilation disabled.
- Fresh complete solution test run: **5,586 passed** across 22 test runs.
- Separate complete BerkeleyDb suite: **1,995 passed**, 665 per framework.
- Total: **7,581 passed**; Catalogs has 97 cases per framework.
- The existing repository formatting gate found multiline closing parentheses;
  whitespace-only corrections passed all six convention checks, all three
  Runtime suites, and the subsequent fresh complete solution run.
- Historical BerkeleyDb 1.15/1.16 baselines and all JSON schemas are unchanged.
  Catalogs retains its ten-type API. The compiled fourteen-type BerkeleyDb API
  reconstructs exactly to twelve types and then the historical nine-type reader.

- Eight nupkg and seven snupkg built. Full Release distribution verification
  passed: exact APIs, historical reconstruction, dependency closure, isolated
  package consumers, samples, managed payloads, symbols, and Source Link.
- The unchanged RE07 package-only fixture reports CS8321 for its unused
  `MapPersistentRasterStatus` helper; all three framework runs pass.

UC03 is accepted at code commit
`e4dcf7e25f174d07702186d5bd9e4ea1a5ec7742`, tree
`8ed81360d94bebc98e5b81abbf09455806c34655`. This exactly matches the locally
qualified implementation tree.

| Code-head CI check | Result |
| --- | --- |
| [PR workflow 36304428682](https://github.com/uniblab/Icod.TermInfo/actions/runs/36304428682) | All 12 jobs passed: Windows/Linux/macOS Staging and Release qualification, installed-tool checks on three hosts, and all six archives |
| [Interoperability workflow 36304428688](https://github.com/uniblab/Icod.TermInfo/actions/runs/36304428688) | All three jobs passed: Linux/macOS native Berkeley DB/ncurses and Windows managed-only transported fixture; native probes ran and were not path-filtered out |

Author self-review was inline, without subagents. Full package qualification from implementation
task 4 was combined with task 5's integrated Alpha-3 distribution gate. This
could have exposed package defects later in the session; the complete gate
passed. UC04 remains responsible for the public unified reader and cross-format
parity. The acceptance follow-up changes documentation only; its CI is not
awaited. The release PR remains a draft; no merge, tag, or NuGet publication.

### Accepted UC02 evidence

The approved directory adapter is implemented, with 31 behavioral/boundary cases
and one compiled Alpha-2 identity check. It maps only observed publications,
retains all duplicate occurrences, excludes exact-path invalid placements, and
preserves acquisition diagnostics and typed limit causes. The test-only
Inspection friend grant supports deterministic mapping fixtures; production
dependency directions and the ten-type Catalogs API remain unchanged.

Local Alpha-2 Release qualification:

- Full solution build: zero warnings/errors, warnings treated as errors.
- Full solution tests: **5,466 passed**, across 22 test runs.
- Separate BerkeleyDb suite: **1,818 passed**, 606 per framework.
- Catalogs: **57 per framework**, including all UC01 tests and 32 UC02 cases.
- Exact Catalogs API fingerprint remains
  `9b7479953a060de94aadf994ddd5c1946660e8f1f2341a3c26291e2a3c9e0b3f`.

- Eight nupkg and seven snupkg built; full Release distribution verification
  passed, including existing package consumers and samples, exact APIs,
  historical Inspection reconstruction, metadata, symbols and Source Link.

UC02 is accepted at `965c8d2ee91a6b3515a33c64cb9f702566f3943a`, tree
`a00b37d8a4e7db913e5d1f6af399e941a410ed67`. The adapter commit is
`142c6377b68435b2e0984a909862d016d40c3e25`.

| Code-head CI check | Result |
| --- | --- |
| [PR workflow 36298484406](https://github.com/uniblab/Icod.TermInfo/actions/runs/36298484406) | All 12 jobs passed: Windows/Linux/macOS Staging and Release qualification, installed-tool checks on three hosts, and all six archives |
| [Interoperability workflow 36298484409](https://github.com/uniblab/Icod.TermInfo/actions/runs/36298484409) | All three jobs passed: Linux/macOS native Berkeley DB/ncurses and Windows managed-only transported fixture |

UC03 bounded hashed acquisition and adaptation is accepted as recorded above;
UC04 public unified acquisition and cross-format parity is next.
The UC02 acceptance follow-up changed documentation only; its CI was not awaited.
UC02 review was performed inline by the author, without subagents;
documentation is reviewed directly, while compiled identity and package contracts
are checked automatically. Published 1.16 installation examples remain unchanged.

### Accepted UC01 evidence

UC00 and the UC01 plan are approved. UC01 implementation comprises:
- `af93967`: opt-in bounded Inspection acquisition.
- `e61e9c6`: immutable Catalogs model package and explicit solution entries.
- `62eb787`: exact additive compatibility and package-verifier fixtures.
- `6850dd5`: coordinated Alpha-1 integration and repository style corrections.

Local Release evidence before coordinated integration: 36 bounded/legacy catalog
checks, 24 Catalogs checks, 661 complete Inspection checks, and 7 BerkeleyDb
API-freeze checks per framework. Catalogs API equality is confirmed across all
three frameworks; both Inspection compatibility entry points reconstruct 1.14
through 1.10. All six JSON schema fingerprints remain unchanged.

UC01 is accepted at `656acf952c286ccd24b3819e85faf2bc598e2bcd` after successful
code-head qualification. UC02 directory adaptation is accepted as recorded above;
UC03 adds bounded hashed acquisition, and UC04 adds the public reader. No 1.17
release has been tagged or published.

The integrated production code is `6850dd56ca0643279f55d8037e2fe4dc46b44445`.
The qualification candidate is `656acf952c286ccd24b3819e85faf2bc598e2bcd`,
tree `f27ce743c726638200b59a983d50fa4de91db5c2`. It changes only the Windows
fixture assertion to compare the exact supplied absolute path: a valid `/` in
Windows input must not be compared against a newly constructed `\` spelling.
The model preserves provenance correctly. All 25 Catalogs tests passed again
on each local framework after the assertion fix.
Local qualification on Linux used SDK 10.0.100 and .NET 8/9/10 runtimes:

| Command / check | Result |
| --- | --- |
| `dotnet build Icod.TermInfo.sln -c Release --no-restore -m:1 -p:UseSharedCompilation=false -warnaserror` | Passed; zero warnings/errors |
| `dotnet test Icod.TermInfo.sln -c Release --no-restore -m:1 -p:UseSharedCompilation=false` | 5,370 passed across 22 test runs |
| `dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release --no-restore -m:1 -p:UseSharedCompilation=false` | 606 per framework; 1,818 passed |
| `packaging/PackPackages.ps1 -Configuration Release -OutputDirectory artifacts/uc01-release` | Eight nupkg and seven snupkg artifacts |
| Catalogs package verifier plus three-TFM API baseline checks | Passed; exact dependencies, managed payload, XML docs, symbols and Source Link |
| `packaging/VerifyPackageArtifact.ps1 -ArtifactDirectory artifacts/uc01-release -Configuration Release` | Passed, including existing package consumers and samples |
| [Initial PR workflow 36292829043](https://github.com/uniblab/Icod.TermInfo/actions/runs/36292829043) | macOS passed; Windows exposed the fixture separator assertion; superseded |
| [PR workflow 36293112745](https://github.com/uniblab/Icod.TermInfo/actions/runs/36293112745) | Passed: all 12 jobs, including Windows/Linux/macOS Staging and Release qualification, three installed-tool package checks and all six tool archives |
| [Interoperability workflow 36292829036](https://github.com/uniblab/Icod.TermInfo/actions/runs/36292829036) | Passed on production head `6850dd5`: Linux/macOS native oracle and Windows managed fixture |

The local package script runs used `DOTNET_PROCESSOR_COUNT=1` to keep MSBuild
within this workspace's process limits. The initial full test run caught the
new files' brace/multiline-parenthesis convention violations; these were corrected
and all six convention checks passed before the successful full run. No frozen
API/schema authority was changed to accommodate a failure.

Record each accepted tranche's commit, tests, qualification run, and remaining
risks here and in its contract/audit document. Documentation-only follow-ups
receive local relevant checks; do not stop development waiting for their CI jobs.

If a proposed change introduces migration, mixed-source precedence, JSON, command
switches, a second parser, new native requirements, or broader Hash-v9 support,
revise the release scope explicitly before implementing it. Do not let those
features enter through an adapter or sample as incidental work.
