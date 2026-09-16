# HDB06 Inspection and Tool Integration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [x]`) syntax for tracking.

**Goal:** Integrate the accepted BerkeleyDb provider and catalog with provider-neutral Inspection, `infocmp`, `toe`, the routed tool, and distribution qualification without changing Runtime, Inspection, BerkeleyDb public APIs, or frozen JSON schemas.

**Architecture:** Executable projects classify explicit paths by current filesystem shape and delegate all hashed semantics to `Icod.TermInfo.BerkeleyDb`. Inspection remains provider-neutral; `infocmp` reuses `TermInfoInspectionEngine`, while `toe` projects conventional and hashed entries into one command-private listing record. Each command is established through an independent behavioral RED before production dispatch is added.

**Tech Stack:** C# 13, .NET 8/9/10 reusable libraries, .NET 10 command projects, xUnit 2.9.2, GitHub Actions Windows/Linux/macOS matrix, native ncurses/Berkeley DB 5.3 interoperability oracle.

**Status:** COMPLETE / ACCEPTED  
**Accepted implementation/qualification head:** `c273b99df920e8a71cd31e0e23bc6700ef9ce456`  
**Accepted normal workflow:** run `35123554495` — 12/12 jobs passed  
**Accepted HDB00 workflow:** run `35123554545` — 3/3 jobs passed

**Spec:** `docs/superpowers/specs/2026-09-16-hdb06-inspection-tool-integration-design.md`

## Global Constraints

- Advance the coordinated suite version to exactly `1.15.0-Alpha-6`.
- Keep reusable assembly identity `1.0.0.0`.
- `Icod.TermInfo` and `Icod.TermInfo.Inspection` must not reference `Icod.TermInfo.BerkeleyDb`.
- `Icod.TermInfo.BerkeleyDb` continues to reference Runtime only.
- Only `infocmp` and `toe` executable projects gain production BerkeleyDb project references.
- Commands use only the accepted public provider/catalog APIs; no page, record, marker, or compiled-entry parsing enters command production code.
- Existing conventional-directory behavior, Runtime discovery, `toe --json` schemas, and ambient `toe` listing remain unchanged.
- Production remains pure managed and read-only with no P/Invoke, native runtime asset, third-party Berkeley DB runtime package, or write path.
- PR #45 remains open, draft, and unmerged.
- Every behavioral production change follows observed RED, minimal GREEN, full regression verification, and an exact commit.

---

### Task 1: Alpha-6 baseline and Inspection composition

**Files:**
- Modify: `Directory.Build.props`
- Modify: `tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj`
- Create: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb06InspectionCompositionTests.cs`

**Interfaces:**
- Consumes: `BerkeleyDbTerminalDescriptionProvider`, `TermInfoInspectionTarget`, `TermInfoInspectionEngine`, and existing HDB test-store builders.
- Produces: permanent proof that Inspection consumes the hashed provider through the frozen `ITerminalDescriptionProvider` contract without a production dependency change.

- [x] **Step 1: bump the coordinated version**

Change only:

```xml
<IcodTermInfoSuiteVersion>1.15.0-Alpha-6</IcodTermInfoSuiteVersion>
```

- [x] **Step 2: add the test-only Inspection reference**

Add to the BerkeleyDb test project:

```xml
<ProjectReference Include="..\..\Icod.TermInfo.Inspection\Icod.TermInfo.Inspection.csproj" />
```

No production project reference changes in this task.

- [x] **Step 3: add provider-neutral composition tests**

Use a real synthetic Hash-v9 store with literal identities `hdb06-main` and
`hdb06-alias`. Exercise:

```csharp
var provider = new BerkeleyDbTerminalDescriptionProvider( path );
var target = new TermInfoInspectionTarget(
    provider,
    "hdb06-alias",
    provider.DatabasePath
);
TermInfoInspectionResult result =
    TermInfoInspectionEngine.Inspect( target );

Assert.Equal( "hdb06-main", result.Terminal.Name );
Assert.Contains( "hdb06-alias", result.Terminal.Aliases );
Assert.Equal(
    TerminalDescriptionSourceRenderer.Render( result.Terminal ),
    TermInfoInspectionEngine.Render( result )
);
```

Also assert a clean miss through `TryInspect`, and exception identity for a
malformed Hash file. Expectations must be literal and the tests must use the real
provider and engine.

- [x] **Step 4: run characterization verification**

Run:

```text
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release
```

Expected: all existing HDB tests plus the new Inspection composition cases pass
on net8.0, net9.0, and net10.0. These are intentionally already-green
characterization tests; no production behavior is added.

- [x] **Step 5: commit the baseline**

Commit:

```text
test: qualify HDB06 Inspection composition
```

Record the exact head, run IDs, per-TFM counts, and any warning. Do not begin
`infocmp` production changes until this exact head is green.

---

### Task 2: `infocmp` behavioral RED

**Files:**
- Create: `tests/Shared/BerkeleyDbHashV9TestStore.cs`
- Modify: `tests/Icod.TermInfo.InfoCmp.Tests/Icod.TermInfo.InfoCmp.Tests.csproj`
- Create: `tests/Icod.TermInfo.InfoCmp.Tests/src/Hdb06HashedAcquisitionCommandTests.cs`

**Interfaces:**
- Consumes: public `InfoCmpCommand.RunAsync` and literal synthetic Hash-v9 files.
- Produces: failing command-level requirements for file-valued `-A`/`-B`; no production command reference or dispatch.

- [x] **Step 1: add the shared test-store builder**

Create a test-only builder included by source link. Its public test interface is:

```csharp
internal static class BerkeleyDbHashV9TestStore {
    internal static byte[] CreateCatalogStore(
        string canonical,
        string description,
        params string[] aliases
    );

    internal static byte[] CreateMalformedStore();
}
```

`CreateCatalogStore` writes a 512-byte-page little-endian Hash-v9 image with
one marker-0 compiled entry and one marker-2 publication per canonical/alias
name. The compiled identity is constructed from literal canonical, aliases, and
description fields. The builder is test code only and must not call production
BerkeleyDb parsing helpers.

Link it into the InfoCmp test project:

```xml
<Compile Include="..\Shared\BerkeleyDbHashV9TestStore.cs"
         Link="Shared\BerkeleyDbHashV9TestStore.cs" />
```

- [x] **Step 2: write real command tests**

Invoke `InfoCmpCommand.RunAsync` with caller-owned streams. Add separate tests
whose names identify these mutations:

- file-valued `-A` wrongly uses `DirectoryTerminalDescriptionProvider`;
- file-valued `-B` wrongly uses the conventional provider;
- alias requests lose exact publication identity;
- mixed conventional/hashed comparison selects the wrong side;
- explicit hashed synthesis/planning candidates are not acquired;
- hashed `--json` changes the frozen storage-neutral document;
- clean misses are not `INFOCMP0002`;
- malformed stores are not `INFOCMP0003`;
- an existing conventional directory is accidentally dispatched as hashed; and
- `--all-candidates -B <file>` is not rejected before stdout.

Use literal expected exit codes, stdout fragments/documents, and diagnostic codes.

- [x] **Step 3: commit and verify behavioral RED**

Commit:

```text
test: define HDB06 infocmp hashed acquisition
```

Require the normal workflow to show the new InfoCmp cases failing because the
existing directory provider attempts the file path. Existing command tests must
remain green. Record exact failures before Task 3.

---

### Task 3: `infocmp` GREEN

**Files:**
- Modify: `infocmp/Icod.TermInfo.InfoCmp.csproj`
- Create: `infocmp/src/InfoCmpTerminalProviderFactory.cs`
- Modify: `infocmp/src/InfoCmpInspector.cs`

**Interfaces:**
- Consumes: file-valued path behavior frozen by Task 2.
- Produces:

```csharp
internal static class InfoCmpTerminalProviderFactory {
    internal static ITerminalDescriptionProvider Create(
        string? databasePath,
        out string displayLabel
    );
}
```

- [x] **Step 1: add the executable dependency**

Add:

```xml
<ProjectReference Include="..\Icod.TermInfo.BerkeleyDb\Icod.TermInfo.BerkeleyDb.csproj" />
```

to `infocmp` only.

- [x] **Step 2: implement minimal path dispatch**

Implement:

```csharp
if ( databasePath is null ) {
    displayLabel = "system terminfo search";
    return new SystemTerminalDescriptionProvider();
}

if ( File.Exists( databasePath ) ) {
    var provider =
        new BerkeleyDbTerminalDescriptionProvider( databasePath );
    displayLabel = provider.DatabasePath;
    return provider;
}

var directory =
    new DirectoryTerminalDescriptionProvider( databasePath );
displayLabel = directory.Root;
return directory;
```

Do not inspect extensions or database bytes.

- [x] **Step 3: route exact-name acquisition through the factory**

Replace only provider construction in `InfoCmpInspector.AcquireAsync`. Keep
`TermInfoInspectionTarget`, `TermInfoInspectionEngine`, diagnostics, rendering,
comparison, synthesis, and planning unchanged.

Before `--all-candidates` directory enumeration, reject an existing file-valued
candidate root with a deterministic `INFOCMP0004` operational diagnostic and
no stdout.

- [x] **Step 4: verify GREEN**

Run the dedicated InfoCmp test project, then the full solution. Expected: every
new HDB06 case and all prior InfoCmp behavior pass with no new warning.

- [x] **Step 5: commit GREEN and qualify**

Commit:

```text
feat: add infocmp hashed acquisition
```

Require normal CI 12/12 and HDB00 3/3 before starting the `toe` RED. Record
exact head, run IDs, and test counts.

---

### Task 4: `toe` behavioral RED

**Files:**
- Modify: `tests/Icod.TermInfo.Toe.Tests/Icod.TermInfo.Toe.Tests.csproj`
- Create: `tests/Icod.TermInfo.Toe.Tests/src/Hdb06HashedCatalogCommandTests.cs`
- Reuse: `tests/Shared/BerkeleyDbHashV9TestStore.cs`

**Interfaces:**
- Consumes: public `ToeCommand.RunAsync`, the shared independent test fixture, and existing conventional-directory helpers.
- Produces: failing human-listing requirements without a production BerkeleyDb reference.

- [x] **Step 1: link the shared fixture**

Add the same linked compile item used by InfoCmp.

- [x] **Step 2: write real human-listing tests**

Add separate tests for:

```text
hdb06-main<TAB>HDB06 terminal
hdb06-alias<TAB>HDB06 terminal
```

and assert:

- logical canonical and alias publications appear;
- the marker-0 key `hdb06-main|hdb06-alias|HDB06 terminal` does not appear;
- default and `-s` ordering are ordinal;
- `-h` prints the canonical absolute file path;
- mixed directory/file roots preserve caller order;
- duplicate analysis keys the displayed publication name and uses semantic comparison;
- a malformed file emits `TOE0005`, emits no entries for that root, continues to a later valid root, and returns 1;
- a missing explicit root retains `TOE0002`;
- conventional human listing is byte-for-byte unchanged; and
- `toe --json <file>` retains the existing `UnsupportedStore` document.

- [x] **Step 3: commit and verify behavioral RED**

Commit:

```text
test: define HDB06 toe hashed listing
```

Require failures only in new human hashed-listing cases. The frozen JSON-file
case and all conventional cases must remain green. Record exact run evidence.

---

### Task 5: `toe` GREEN

**Files:**
- Modify: `toe/Icod.TermInfo.Toe.csproj`
- Create: `toe/src/ToeCatalogAdapter.cs`
- Modify: `toe/src/Command.cs`

**Interfaces:**
- Consumes: Task 4 requirements and accepted public catalog reader.
- Produces:

```csharp
internal sealed class ToeCatalogEntry {
    internal ToeCatalogEntry(
        string publicationName,
        string root,
        TerminalDescription terminal
    );
    internal string PublicationName { get; }
    internal string Root { get; }
    internal TerminalDescription Terminal { get; }
}

internal static class ToeCatalogAdapter {
    internal static IReadOnlyList<ToeCatalogEntry> ReadHashed(
        string databasePath,
        CancellationToken cancellationToken
    );
}
```

- [x] **Step 1: add the executable dependency**

Add the direct BerkeleyDb project reference to `toe` only.

- [x] **Step 2: implement the minimal adapter**

Construct `BerkeleyDbTerminalCatalogReader`, call
`Read(cancellationToken)`, and project each entry using:

```csharp
new ToeCatalogEntry(
    entry.Name,
    reader.DatabasePath,
    entry.Terminal
)
```

No marker or page interpretation is permitted.

- [x] **Step 3: unify command-private listing projection**

In human `BuildListing` only:

- use the existing conventional inspector for directories and unclassified paths;
- use `ToeCatalogAdapter.ReadHashed` for existing files;
- render `PublicationName`, terminal description, and canonical root;
- preserve caller root order;
- sort by ordinal publication name when `-s`;
- key duplicate analysis by `PublicationName`; and
- catch the existing operational exception family into `TOE0005` and continue.

Do not alter `RenderCatalogAsync`, `RenderDatabaseSetAsync`,
`CompareDatabaseSetsAsync`, `GetSystemLocations`, or JSON rendering.

- [x] **Step 4: verify GREEN**

Run the dedicated Toe project, Router tests, and full solution. Expected: all new
human listing tests and every frozen conventional/JSON test pass.

- [x] **Step 5: commit GREEN and qualify**

Commit:

```text
feat: add toe hashed catalog listing
```

Require normal CI 12/12 and HDB00 3/3. Record exact head, runs, and counts.

---

### Task 6: Routed, native, package, and archive qualification

**Files:**
- Modify: `tests/Icod.TermInfo.Router.Tests/Icod.TermInfo.Router.Tests.csproj`
- Create: `tests/Icod.TermInfo.Router.Tests/src/Hdb06HashedCommandRoutingTests.cs`
- Create: `tools/hdb00/verify-hdb06-commands.ps1`
- Modify: `.github/workflows/hdb00-interoperability.yml`
- Create: `.github/scripts/new-hdb06-test-store.ps1`
- Modify: `.github/scripts/smoke-tool-package.ps1`
- Modify: `.github/scripts/smoke-tool-archive.ps1`

**Interfaces:**
- Consumes: direct command behavior from Tasks 3 and 5 and native HDB00 stores.
- Produces: exact direct/routed/distributed equivalence without new production behavior.

- [x] **Step 1: add routed equivalence tests**

For the same controlled file and literal terminal names, run direct and routed
commands with independent streams:

```csharp
int direct = await InfoCmpCommand.RunAsync( ... );
int routed = await RouterCommand.RunAsync(
    new[] { "infocmp", ... },
    ...
);
```

Repeat for `toe`. Assert identical status, stdout bytes, and stderr bytes.
These tests catch argument loss, stream substitution, and exit-status rewriting.

- [x] **Step 2: add native-store command assertions**

Implement `tools/hdb00/verify-hdb06-commands.ps1` with parameters for the
native fixture root and command launch mode. Invoke it from each HDB00 job after
the existing production-reader comparison. On Linux/macOS native-produced
primary and overflow stores, and Windows' downloaded Linux store, assert:

- canonical and alias `infocmp -A` success;
- canonical and alias `toe` logical lines;
- forced-overflow terminal success;
- missing-name status/diagnostic; and
- malformed/wrong-access-method status/diagnostic.

Commands target net10.0; the reusable provider/catalog remain qualified on
net8/net9/net10 by the existing interoperability project.

- [x] **Step 3: extend package-only and installed-tool smoke**

Implement `.github/scripts/new-hdb06-test-store.ps1` as an independent
PowerShell Hash-v9 fixture writer using literal page fields and compiled
identity bytes. In `smoke-tool-package.ps1`, use only the installed
`icod-terminfo` tool against that fixture and assert exact canonical/alias
output. Ensure the smoke cannot resolve project outputs.

- [x] **Step 4: extend all archive smoke paths**

In `smoke-tool-archive.ps1`, generate the same controlled fixture and for
every existing archive RID:

```text
win-x64
win-arm64
linux-x64
linux-arm64
osx-x64
osx-arm64
```

run routed `infocmp` and `toe` hashed commands against the controlled fixture
and compare literal output.

- [x] **Step 5: commit qualification**

Commit:

```text
test: qualify HDB06 command distribution
```

Require exact package contents, no native assets, installed-tool smoke, six-RID
archive smoke, normal CI 12/12, and HDB00 3/3.

---

### Task 7: HDB06 closure

**Files:**
- Create: `docs/1.15.0-HDB06-INSPECTION-AND-TOOL-INTEGRATION.md`
- Modify: `Icod.TermInfo-1.15.0-Berkeley-DB-Hashed-Terminfo-Acquisition-Roadmap.md`
- Modify: `README.md`
- Modify: `Icod.TermInfo.BerkeleyDb/README.md`
- Modify: `infocmp/README.md`
- Modify: `toe/README.md`
- Modify: `docs/superpowers/plans/2026-09-16-hdb06-inspection-tool-integration.md`
- Update: PR #45 body

**Interfaces:**
- Consumes: exact RED, GREEN, native, package, and distribution evidence.
- Produces: auditable HDB06 acceptance and HDB07 next-tranche state.

- [x] **Step 1: run verification-before-completion**

Review exact heads, workflow conclusions, per-host/TFM counts, direct/routed
output, dependency graphs, public API manifests, JSON regression evidence,
package assets, installed tool, archives, and native stores. Do not infer a
complete run from partial green jobs.

- [x] **Step 2: write the closure record**

Record the accepted implementation head, every RED/GREEN head, exact run IDs,
test counts, command contracts, diagnostics, dependency direction, JSON and
ambient-discovery non-changes, platform evidence, and explicit non-goals.

- [x] **Step 3: update release-facing documentation**

Mark HDB06 complete/accepted at `1.15.0-Alpha-6`, document file-valued
`infocmp -A/-B` and human `toe` roots, retain `tic` directory-only writes,
and make HDB07 adversarial/compatibility hardening next.

- [x] **Step 4: mark this plan complete**

Change all completed plan checkboxes to `[x]` and add the accepted exact head,
workflow IDs, and counts near the header.

- [x] **Step 5: commit and qualify documentation closure**

Commit:

```text
docs: accept HDB06 inspection and tool integration
```

Require a fresh normal 12/12 run and HDB00 3/3 run on the documentation-complete
head. Re-read their logs for exact unit/native counts.

- [x] **Step 6: update and verify PR state**

Update PR #45 with the accepted evidence and HDB07 next step. Re-fetch it and
confirm exact head, open state, draft state, and `merged == false`.
