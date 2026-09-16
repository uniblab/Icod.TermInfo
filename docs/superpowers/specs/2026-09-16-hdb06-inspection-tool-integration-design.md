# HDB06 Inspection and Tool Integration Design

**Status:** APPROVED DESIGN BASELINE  
**Development line:** `Icod.TermInfo 1.15.0`  
**Coordinated prerelease:** `1.15.0-Alpha-6`  
**Tranche:** HDB06  
**Packages/tools:** `Icod.TermInfo.Inspection`, `Icod.TermInfo.BerkeleyDb`, `infocmp`, `toe`, and `Icod.TermInfo.Tools`

## 1. Objective

HDB06 integrates the accepted HDB03 exact-name provider and HDB05 logical
catalog with the existing Inspection and command surfaces.

The integration is additive:

- `infocmp` may acquire explicitly named terminals from one caller-selected
  supported Berkeley DB Hash-v9 file;
- `toe` may list one or more explicitly selected supported hashed stores;
- direct commands and `icod-terminfo` routed commands remain equivalent; and
- existing conventional-directory, Runtime system-discovery, rendering,
  comparison, planning, JSON, diagnostic, and exit-status contracts remain
  unchanged unless this specification explicitly says otherwise.

No command parses Berkeley DB pages or ncurses record envelopes. All hashed
lookup and enumeration remain owned by `Icod.TermInfo.BerkeleyDb`.

## 2. Selected approach

Existing explicit path operands dispatch by observed filesystem shape.

For `infocmp`, the established `-A` and `-B` paths select:

```text
existing directory
    -> DirectoryTerminalDescriptionProvider

existing regular file
    -> BerkeleyDbTerminalDescriptionProvider

missing or otherwise unclassified path
    -> existing DirectoryTerminalDescriptionProvider behavior
```

Treating an unclassified path as a conventional directory preserves the current
missing-root and retry behavior. HDB06 does not infer a hashed store from a
filename suffix.

For human `toe` listing, each explicit root selects:

```text
existing directory
    -> TermInfoDatabaseInspector conventional catalog

existing regular file
    -> BerkeleyDbTerminalCatalogReader logical catalog

missing path
    -> existing explicit-root missing diagnostic
```

This approach is preferred over new hashed-only switches because `-A`, `-B`,
and `toe` roots already mean caller-selected terminfo databases. It is
preferred over making `Icod.TermInfo.Inspection` depend on
`Icod.TermInfo.BerkeleyDb`, because that would make the optional storage
implementation a transitive dependency of every Inspection consumer.

## 3. Dependency architecture

The production dependency graph becomes:

```text
infocmp --------------------+
                            |
toe ------------------------+--> Icod.TermInfo.BerkeleyDb --> Icod.TermInfo
 |                          |
 +--> Icod.TermInfo.Inspection -----------------------------> Icod.TermInfo

icod-terminfo --> infocmp / toe / other commands
```

The following constraints are frozen:

1. `Icod.TermInfo` does not reference BerkeleyDb.
2. `Icod.TermInfo.Inspection` does not reference BerkeleyDb.
3. `Icod.TermInfo.BerkeleyDb` continues to reference Runtime only.
4. Only the executable command projects gain direct BerkeleyDb project
   references.
5. The installable tool and standalone archives carry the managed BerkeleyDb
   assembly required by `infocmp` and `toe`.
6. No native runtime asset, P/Invoke entry point, or third-party Berkeley DB
   package is introduced.

## 4. Inspection integration

The existing public Inspection architecture already consumes
`ITerminalDescriptionProvider` through `TermInfoInspectionTarget`.
HDB06 therefore adds no Inspection public API.

Permanent integration tests compose:

```csharp
var provider =
    new BerkeleyDbTerminalDescriptionProvider( databasePath );
var target =
    new TermInfoInspectionTarget(
        provider,
        requestedName,
        provider.DatabasePath
    );
TermInfoInspectionResult result =
    TermInfoInspectionEngine.Inspect( target );
```

The tests prove canonical and alias acquisition, canonical rendering, clean
misses, and propagation of BerkeleyDb container, compiled-entry, identity, and
I/O boundaries.

This freezes provider-neutral Inspection composition without reversing package
dependencies or teaching Inspection about Berkeley DB.

## 5. `infocmp` contract

### 5.1 Path selection

`-A path` continues to select the first or only terminal source.
`-B path` continues to select later comparison, synthesis-parent, or explicit
planning-candidate sources.

When the selected path is an existing file, `infocmp` constructs
`BerkeleyDbTerminalDescriptionProvider`. When it is an existing directory or
is not currently an existing file, it follows the existing conventional
provider path.

No extension, basename, platform, or file-content pre-probe is used to choose a
provider. The BerkeleyDb provider remains authoritative for validating an
existing file.

### 5.2 Supported modes

Hashed exact-name acquisition composes with the existing storage-neutral modes:

- one-terminal source rendering;
- one-terminal `--json` rendering using the unchanged
  `terminalDescription` document;
- semantic comparison;
- comparison JSON;
- relative synthesis with explicit parents; and
- relative-source planning with explicitly named candidates.

The existing `--plan-use --all-candidates` catalog-discovery forms remain
conventional-directory-only in HDB06. A BerkeleyDb `-B` file in that mode is
rejected deterministically before output. Adding heterogeneous
all-candidate catalogs would require a separately reviewed multi-store planning
contract and is not necessary for explicit hashed acquisition.

### 5.3 Diagnostics and status

A clean hashed-provider miss uses the existing `INFOCMP0002` diagnostic and
status 1.

Construction, I/O, malformed/unsupported container, malformed ncurses envelope,
compiled parser, or identity failures use the existing `INFOCMP0003`
acquisition diagnostic and status 1. The underlying exception message is
preserved; stack traces and internal page details are not emitted.

Usage errors remain status 2. Cancellation remains status 130. Successful output
leaves stderr empty and returns status 0.

## 6. `toe` contract

### 6.1 Explicit human listing

Human listing accepts explicit conventional directories and hashed files in the
same ordered root list. Each root is inspected exactly once.

A conventional entry retains the established publication:

```text
canonical-terminal-name<TAB>description
```

A hashed entry publishes the HDB05 logical name:

```text
canonical-or-alias-publication-name<TAB>description
```

The description comes from the shared parsed `TerminalDescription`. An absent
description is rendered exactly as the existing conventional command renders it.

This deliberately lists aliases as distinct logical publications. Marker-0
storage keys are never printed.

### 6.2 Ordering and headings

Without `-s`, conventional roots retain their existing catalog order and
hashed roots retain the HDB05 deterministic ordinal logical-publication order.

With `-s`, both shapes are ordered by publication name using
`StringComparer.Ordinal`, with deterministic existing tie-breaks.

With `-h`, both shapes use the established heading:

```text
# <canonical-absolute-root>
```

Caller root order is preserved across mixed conventional and hashed roots.

### 6.3 Duplicate analysis

When existing `-a -s` duplicate analysis applies, the key is the displayed
publication name:

- conventional entries contribute their canonical name;
- hashed entries contribute their exact canonical or alias publication name.

Semantic equality continues to use `TerminalDescriptionComparer`, not compiled
bytes or Berkeley DB record identity. The first root in caller/discovery order
remains the comparison reference.

### 6.4 Failure behavior

Hashed catalog reads are all-or-nothing per root, matching HDB05.

A malformed, unsupported, unreadable, or invalid hashed file produces the
existing `TOE0005` inspection diagnostic, marks the command operationally
failed, emits no entries for that root, and continues to later explicit roots.
Any safely completed earlier or later roots remain on stdout. Final status is 1.

An explicitly requested missing root retains `TOE0002` and status 1.
Cancellation remains status 130.

### 6.5 Ambient discovery and JSON

Operand-free `toe`, `toe -a`, and `toe -D` retain their existing Runtime /
Inspection conventional-discovery contract in HDB06. They do not silently change
to the HDB04 opt-in BerkeleyDb system provider.

The frozen `toe --json` version-1 and version-2 schemas remain unchanged.
JSON routes continue through `TermInfoDatabaseInspector` exactly as before.
Consequently an explicit file continues to appear as the existing
`UnsupportedStore` catalog state rather than gaining a new hashed JSON shape.

A future hashed catalog JSON document or heterogeneous system catalog requires a
new schema/version and separate approval.

## 7. Command-layer structure

A small internal path classifier/provider factory is added to `infocmp`.
It owns only path-shape selection and provider construction.

A small internal listing adapter is added to `toe`. It projects conventional
and hashed entries into one command-private record containing:

```csharp
string PublicationName
string Root
TerminalDescription Terminal
```

The adapter does not expose or reinterpret Berkeley DB records. It calls only
`BerkeleyDbTerminalCatalogReader.Read(CancellationToken)`.

Shared behavior should be factored only within each executable project. HDB06
does not create a new reusable bridge package and does not add command concepts
to either reusable library.

## 8. Filesystem and race semantics

Path classification is a point-in-time dispatch decision, not an atomic
filesystem transaction.

If a path changes after classification:

- the selected provider/reader observes its normal I/O or format behavior;
- the command does not fall back from a failed hashed read to a directory, or
  from a failed directory read to a hashed file;
- malformed data is never reinterpreted as another storage shape; and
- a later command invocation performs a new classification.

Symbolic links follow the host `Directory.Exists` / `File.Exists` behavior.
Broken links and other unclassified paths retain conventional compatibility.

## 9. Versioning and compatibility

HDB06 advances the coordinated suite version to exactly
`1.15.0-Alpha-6`.

Reusable assembly identities remain `1.0.0.0`. Runtime and Inspection public
APIs remain unchanged. The accepted BerkeleyDb public API remains unchanged.
No JSON schema is changed.

Existing direct commands, routed commands, package verification, installed-tool
smoke, and all six standalone archive RIDs remain mandatory compatibility gates.

## 10. TDD sequence

### Checkpoint A — Inspection composition

Add tests which compose the accepted BerkeleyDb provider with
`TermInfoInspectionTarget` and `TermInfoInspectionEngine`. These are expected
to pass without production changes and establish characterization evidence.

If any test exposes a real incompatibility, commit the failing test alone,
observe the exact RED, and make the smallest package-boundary correction.

### Checkpoint B — `infocmp` explicit hashed acquisition

Add command tests before production changes for:

- canonical and alias rendering through `-A`;
- first/second hashed comparison through `-A` and `-B`;
- mixed conventional/hashed comparison;
- explicit-parent synthesis and explicit-candidate planning;
- unchanged terminal-description/comparison JSON;
- clean miss;
- malformed/unsupported database;
- malformed compiled entry and identity mismatch;
- an existing conventional directory retaining its old path; and
- direct/routed equivalence.

Commit and observe the behavioral RED. The expected initial failure is the
existing directory provider attempting to consume the file path; no BerkeleyDb
command reference or dispatch exists at the RED head.

Then add only the direct BerkeleyDb reference and path-shape provider factory
needed to make these tests green.

### Checkpoint C — `toe` explicit hashed listing

Add command tests before production changes for:

- canonical and alias logical lines;
- marker-0 storage keys not appearing;
- default and `-s` order;
- `-h` absolute hashed-root heading;
- mixed conventional/hashed caller order;
- duplicate semantic comparison by publication name;
- malformed/unsupported hashed files with later-root continuation;
- explicit missing-root compatibility;
- unchanged conventional listings;
- unchanged JSON UnsupportedStore behavior for files; and
- direct/routed equivalence.

Commit and observe behavioral RED before adding the hashed catalog adapter.

### Checkpoint D — package and cross-host qualification

Extend package/archive smoke to create or consume a controlled Hash-v9 fixture
and execute:

- installed `icod-terminfo infocmp -A <hashed-file> <name>`;
- installed `icod-terminfo toe <hashed-file>`;
- matching direct archive commands where the archive layout exposes them; and
- exact expected canonical/alias output and empty-success stderr.

The dedicated HDB00 workflow exercises the same commands against native
ncurses/Berkeley DB-produced stores on Linux and macOS and the Linux-generated
store on Windows.

## 11. Acceptance gate

HDB06 is accepted only when:

1. the behavioral RED for each command is observed before implementation;
2. Inspection composition is permanently covered without an Inspection
   dependency on BerkeleyDb;
3. direct and routed `infocmp` hashed acquisition are byte-for-byte equivalent;
4. direct and routed `toe` hashed listings are byte-for-byte equivalent;
5. conventional directory command behavior remains green;
6. malformed/unsupported stores produce deterministic diagnostics and statuses;
7. no command contains Berkeley DB page or ncurses-envelope parsing;
8. Runtime and Inspection public API manifests remain unchanged;
9. BerkeleyDb public API remains cross-TFM equivalent;
10. the package-only and installed-tool consumers pass;
11. all six standalone archive RIDs pass;
12. the normal Windows/Linux/macOS PR workflow is completely green;
13. the three-job HDB00 interoperability workflow is completely green; and
14. PR #45 remains open, draft, and unmerged.

## 12. Explicit non-goals

HDB06 does not add:

- a Runtime or Inspection dependency on BerkeleyDb;
- new Inspection public API;
- new BerkeleyDb public API;
- new JSON schema versions or hashed catalog JSON;
- ambient hashed discovery in `toe`;
- heterogeneous all-candidate planning;
- command-owned page, record, marker, or compiled-entry parsing;
- hashed database writes;
- `tic` hashed publication;
- migration between directory and hashed stores;
- partial hashed catalog recovery;
- native Berkeley DB loading; or
- support for access methods or on-disk revisions outside the accepted HDB02
  subset.

## 13. Next tranche

After HDB06 acceptance, HDB07 performs adversarial and compatibility hardening
over the complete provider, catalog, Inspection-composition, and command
surfaces. HDB07 may strengthen tests and diagnostics but must not silently
expand the HDB06 command contract.

