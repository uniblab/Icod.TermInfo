# UC04 Unified Catalog Reader Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:executing-plans`
> inline, task by task. Do not use subagents. Steps use checkboxes.
> **Status:** Proposed for user review. UC03 is accepted at
> `e4dcf7e25f174d07702186d5bd9e4ea1a5ec7742`; its documentation follow-up is
> `a3ae098c5a096a1102d3dbe7555def1090e0ad0e`.

**Goal:** Deliver the single public reader for an explicitly selected conventional
directory or ncurses Hash-v9 file, and qualify equivalent observable publications
and capabilities across both storage formats in `1.17.0-Alpha-4`.

**Architecture:** Add one sealed reader in Catalogs; it snapshots its options
and delegates each read to the existing bounded directory or hashed adapter
according to the source kind. Use the approved UC00 result model and lower-layer
limits unchanged. Independently build equivalent fixtures with Compiler and the
BerkeleyDb writer in tests, then freeze exactly the one additive public type in
the generated and packaged Catalogs API.

**Tech Stack:** C# 13, .NET 8/9/10, xUnit, PowerShell 5.1, cmd/sh; no Python.

**Spec:** [Approved UC00 contract](../../1.17.0-UC00-UNIFIED-CATALOG-CONTRACT.md),
especially sections 3, 4, 6 and 7; [1.17 roadmap](../../../Icod.TermInfo-1.17.0-Unified-Directory-Hashed-Catalogs-Roadmap.md), UC04.

## Global Constraints

- Execute inline with author self-review and no subagents, on existing draft PR
  #48 and its isolated worktree. Do not merge, tag, or publish to NuGet.
- Source kind is explicit; no autodetection, environment/provider search, suffix
  guessing, source sets, fallback, migration, or cross-source precedence.
- Catalogs adds only the approved `TerminalCatalogReader` public class: eleven
  public types total, with unchanged UC01 ten-type baseline and exact assembly
  version `1.0.0.0` on net8.0/net9.0/net10.0.
- Production Catalogs references Runtime, Inspection and BerkeleyDb only;
  Compiler remains test-only. Preserve all earlier public API and JSON baselines.
- Never duplicate layout traversal, Hash-v9 decoding, compiled parsing, budget
  accounting, or status normalization. New tests may use the existing fixture
  builders and writer, but new production code delegates to accepted adapters.
- Constructor validation checks source first. Snapshots of read options and
  parser options must not share mutable caller state. `Read()` and
  `Read(CancellationToken)` invoke fresh acquisition without a cache.
- Cancellation and typed limits throw with no catalog. No attempt is made to
  interrupt a blocking OS call or promise an atomic source snapshot.
- Version all eight coordinated packages as `1.17.0-Alpha-4`, retain stable
  `1.16.0` install examples and historical evidence. Qualification uses a
  non-force push and exact implementation-code commit CI; do not wait on
  documentation-only acceptance CI.

## Review Focus

1. A requested directory that is a file, or a requested hashed file that is a
   directory, yields `UnsupportedSource` for the explicit kind; never probe and
   silently dispatch to the other kind. Pin in task 1.
2. Declared but unpublished aliases contribute no rows in either format, even
   when compiled terminal descriptions are semantically equal. Pin in task 2.
3. Two directory placements of one name retain two rows and a duplicate issue;
   Hash-v9 keys have no equivalent duplicated-key success. Pin in task 2.
4. A late cancellation or limit failure must escape the public reader without
   returning the adapter's partially built result; repeat reads use fresh
   budgets and source observations. Pin in task 1.
5. A packaged assembly with an extra public type or an altered member of the
   ten-type UC01 model must fail API reconstruction on every TFM, even if its
   eleven-type count is unchanged. Pin in task 3.

---

## Task 1: Public Reader and Explicit Dispatch

**Create:** `Icod.TermInfo.Catalogs/src/TerminalCatalogReader.cs`,
`tests/Icod.TermInfo.Catalogs.Tests/src/UC04UnifiedReaderTests.cs`.

**Interfaces:**

```csharp
public sealed class TerminalCatalogReader {
    public TerminalCatalogReader(TerminalCatalogSource source,
        TerminalCatalogReadOptions? options = null);
    public TerminalCatalogSource Source { get; }
    public TerminalCatalog Read();
    public TerminalCatalog Read(CancellationToken cancellationToken);
}
```

The reader keeps the original immutable source instance and creates an
independent `TerminalCatalogReadOptions` snapshot (including a fresh
`CompiledTermInfoParserOptions`) in the constructor. It checks cancellation
before dispatching. The two overloads call the corresponding internal adapter's
`Read(source, options, token)` on every invocation; `Read()` passes the default
token. A source of an undefined kind cannot be publicly constructed, so an
impossible internal switch value fails explicitly.

- [ ] Write failing tests for a null source, null/default and supplied option
  snapshots, `Source` object identity, and both public overloads. Use a real
  empty directory and the existing empty Hash-v9 fixture builder; both return Complete.
  Confirm the supplied parser and options objects do not become mutable reader
  state, while constructor validation reports `source` before considering options.
- [ ] Run the focused net10.0 Catalogs class filter. Expect failure because
  `TerminalCatalogReader` is absent.
- [ ] Implement the exact class/interface above with only the two adapter calls;
  do not expose an adapter selector, test hook, async API or fallback.
- [ ] Run the focused tests. Add tests for directory-as-file and hash-as-directory
  (`UnsupportedSource`), missing sources (`Missing`), malformed Hash-v9
  (`InvalidStore`) and malformed directory sibling (`Partial`); assert their
  issue kinds and zero/retained rows as UC00 requires.
- [ ] Test pre-canceled token and typed `MaximumEntryCount` failures for both
  source kinds, and a second read after replacing the underlying data. Rely on
  the already qualified adapter seams for deterministic mid-read cancellation;
  the public path forwards that token unchanged. Ensure no result on exception
  and no persisted budget/cache across calls.
- [ ] Run `dotnet test tests/Icod.TermInfo.Catalogs.Tests/Icod.TermInfo.Catalogs.Tests.csproj -c Release -f net10.0 -m:1 -p:UseSharedCompilation=false --filter FullyQualifiedName~UC04UnifiedReaderTests`; commit the reader and focused tests.

## Task 2: Cross-Format Behavioral Qualification

**Create:** `tests/Icod.TermInfo.Catalogs.Tests/src/UC04CatalogParityTests.cs`.
**Modify:** `tests/Icod.TermInfo.Catalogs.Tests/src/DirectoryCatalogFixture.cs`
and `tests/Icod.TermInfo.Catalogs.Tests/src/HashedCatalogFixture.cs` only to
share the same generated `TerminalDescription`/compiled bytes where needed.

**Interfaces:** Consumer tests instantiate the public `TerminalCatalogReader`
from task 1 for both source kinds. Compiler, the existing Hash-v9 fixture builder,
and the BerkeleyDb writer are test-only dependencies already present. The writer
publishes all declared aliases, so use the record fixture builder for a canonical
key with declared but unpublished aliases; use the writer when all aliases are
published. Compare semantic values and actual publication sets, not object
reference equality across independent parses. Preserve each format's distinct
`SourcePath`/`EntryPath` and file-versus-key identity.

- [ ] Write one failing paired-fixture test for a canonical terminal declaring
  two aliases but publishing only `sample`. Build one compiled payload and write
  a conventional `s/sample` file and an existing fixture-builder Hash-v9
  canonical key pointing to the same payload.
  Both readers return exactly one Canonical row with matching terminal name,
  description, aliases, and representative capability values; no invented
  aliases or issues.
- [ ] Add actual separately published `a` and `b` entries/keys; assert identical
  sorted publication names and Canonical/Alias kinds, equivalent parsed
  capability semantics, Complete status, and empty duplicates. Directory
  `EntryPath` is each absolute file path; hashed `EntryPath` is null, with the
  single database path as `SourcePath`. Use the public BerkeleyDb writer here.
- [ ] Add asymmetric observations: literal/hex duplicate directory placement
  retains both occurrences and a Partial duplicate issue while hashed keys
  remain unique; an invalid placement, malformed sibling, or skipped child link
  affects only directory issues. Malformed hashed orphan/index rejects the
  whole store with InvalidStore and no rows. Assert these deliberate differences
  separately from matching semantic rows.
- [ ] Prove stable ordinal row ordering under `en-US` and `tr-TR`, reverse
  fixture input order where the writer allows it, and repeat the two reads.
  Restore process culture after tests and use host-independent names/paths.
- [ ] Run the focused parity class on net8.0/net9.0/net10.0 and commit fixture
  improvements and tests. Avoid new generic comparison or fixture framework.

## Task 3: Exact Additive Catalogs API and Packages

**Create:** `docs/1.17.0-UC04-CATALOGS-PUBLIC-API-ADDITIONS.txt`,
`tools/catalogs-package-verifier/CatalogsUc04Compatibility.cs`,
`tests/Icod.TermInfo.Catalogs.Tests/src/UC04ApiCompatibilityTests.cs`.

**Modify:** `tools/catalogs-package-verifier/Program.cs`,
`.github/scripts/verify-release-package.sh`,
`.github/scripts/verify-release-package.cmd`,
`tests/Icod.TermInfo.Catalogs.Tests/src/UC01CatalogPackageContractTests.cs`,
`tests/Icod.TermInfo.Catalogs.Tests/src/UC01CatalogPackageVerifierTests.cs`.

**Interfaces:** The additions file contains exactly the generated public
`TerminalCatalogReader` type block, including its two overloads, constructor,
and get-only `Source`. `CatalogsUc04Compatibility.Reconstruct(current, addition)`
normalizes line endings, requires precisely one matching reader block, removes
it, and compares the result byte-for-byte with the untouched
`docs/1.17.0-UC01-CATALOGS-PUBLIC-API-BASELINE.txt` (SHA-256
`9b7479953a060de94aadf994ddd5c1946660e8f1f2341a3c26291e2a3c9e0b3f`).
The package verifier applies this to each packaged assembly's generated
manifest. A verifier command `--reconstruct-uc04 input-manifest output-manifest`
does the same for the compiled gate on each TFM. Do not use a raw type count or
replace the historical ten-type baseline.

- [ ] Write failing tests generating the actual current compiled Catalogs
  manifest: exact eleven public types, one approved reader block, unchanged
  ten-type reconstruction, stable `1.0.0.0` identity. Reject a missing, repeated,
  or modified reader block, an unexpected twelfth type, and one changed member
  in an existing model type. Verify package-loaded assemblies per TFM through
  the existing isolated load context.
- [ ] Generate the exact additions block from the built assembly, review the
  signature against UC00 section 3, implement the reconstruction helper and
  verifier mode, and wire `.sh`/`.cmd` package gates to reconstruct then check
  the UC01 baseline for each framework. Update the older foundation test to
  assert ten unchanged types after reconstruction and one reader in the current
  assembly; preserve the remaining dependency tests and their semantics.
- [ ] Run focused API and package-verifier mutation tests, then compile on all
  three TFMs and invoke both available API-gate paths. Confirm the existing
  UC01 baseline, lower-layer historical baselines, and JSON schemas have no
  diffs. Commit the manifest, verifier, scripts and tests.

## Task 4: Alpha-4 Integration and Qualification

**Modify:** `Directory.Build.props`, coordinated package release notes,
`Icod.TermInfo.Catalogs/README.md`, root `README.md`, `CHANGELOG.md`,
`docs/VERSIONING.md`, both roadmaps, and active metadata assertions in tests.
Include `tests/Icod.TermInfo.Catalogs.Tests/src/UC04DevelopmentMetadataTests.cs`
for compiled version/assembly/dependency checks.

- [ ] Set the coordinated version to `1.17.0-Alpha-4`. Explain public reader
  usage with explicit source kind, immutable options, cancellation, statuses,
  publication-versus-alias distinction, duplicate semantics and provenance.
  Keep stable 1.16 install commands and the older historical manifests intact.
- [ ] Update active source tests that assert `Alpha-3` metadata or prose;
  retain fixture-only historical Alpha-1 package version strings. Assert Alpha-4
  informational versions, eleven public Catalogs types, and the approved
  production dependency direction without introducing Compiler/native assets.
- [ ] Run a full Release solution build (warnings as errors, serial MSBuild,
  `UseSharedCompilation=false`), full solution tests on all three frameworks,
  and the separate complete BerkeleyDb suite. Record actual counts, zero failures,
  and any warning in existing package-only consumers precisely.
- [ ] Build eight nupkg and seven snupkg with `packaging/PackPackages.ps1`;
  run `packaging/VerifyPackageArtifact.ps1` in Release with dotnet and PowerShell
  on PATH. Require API reconstruction, dependencies, isolated consumers,
  sample runs, symbols, Source Link, and managed-only payload checks to pass.
- [ ] Review `git diff --check`, production/source compatibility and parity
  assertions against UC00. Commit, non-force push to PR #48, and update the PR
  body with actual counts. Observe Windows/Linux/macOS PR and applicable native
  Linux/macOS plus managed Windows interoperability on the exact code commit;
  diagnose failures before acceptance.
- [ ] Once exact-code CI is green, record code SHA/tree, local counts and both
  workflow links in roadmaps and PR, mark UC04 accepted, and identify UC05 as
  next. Commit/push the documentation-only acceptance record without waiting
  for its CI. Leave the release PR draft, unmerged and unpublished.

## Review and Handoff

UC00 has already approved the architecture and public signature; this plan
locks down the four remaining implementation gates. Task 1 publishes the reader,
task 2 proves semantic parity and deliberate source differences, task 3 guards
the one additive API type in real artifacts, and task 4 qualifies Alpha-4.
The user selected inline execution without subagents; review this plan before
implementation under the 1.17 roadmap's per-tranche gate.
