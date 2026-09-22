# Icod.TermInfo 1.16.0 — Berkeley DB Hash-v9 Writer Roadmap

**Development line:** `1.16.0`
**Theme:** Deterministic Managed Berkeley DB Hash-v9 Publication
**Primary optional package:** `Icod.TermInfo.BerkeleyDb`
**Command integration:** `tic` and `Icod.TermInfo.Tools`
**Language:** C# 13
**Reusable target frameworks:** `net8.0`; `net9.0`; `net10.0`
**Status:** ACTIVE — HW03 COMPLETE / HW04 NEXT
**Stable predecessor:** `1.15.0`
**Initial development version:** `1.16.0-Alpha-1`

---

## 1. Release definition

Icod.TermInfo 1.16 completes the narrow write-side counterpart to the read-only
Berkeley DB work delivered in 1.15. It adds a bounded, deterministic,
pure-managed writer for the ncurses-compatible Berkeley DB Hash on-disk format
version 9 subset used for hashed terminfo databases.

The release is deliberately a **hashed-writer-only** line. It creates complete
Hash-v9 database images from validated compiled terminfo records and integrates
that publication path with managed `tic`. It does not add database migration,
multi-database catalog automation, JSON schema changes, in-place mutation, or a
general-purpose Berkeley DB API. Those broader administration and automation
features are assigned to 1.17.

The north-star pipeline is:

```text
.ti source or TerminalDescription
          |
          v
existing Icod.TermInfo.Compiler writer
          |
          v
validated compiled terminfo bytes
          |
          v
Icod.TermInfo.BerkeleyDb Hash-v9 writer
          |
          v
complete temporary database image
          |
          v
existing 1.15 reader verification
          |
          v
same-filesystem destination commit
```

The BerkeleyDb layer owns only the Hash-v9 container and ncurses record-envelope
publication. `Icod.TermInfo.Compiler` remains the only owner of compiled terminfo
binary encoding, and Runtime remains the only owner of compiled-entry semantic
parsing.

---

## 2. Approved scope decision

The approved release split is:

| Release | Owned outcome |
| --- | --- |
| `1.16.0` | Pure-managed deterministic Hash-v9 writer, safe whole-file publication, and explicit `tic` hashed output |
| `1.17.0` | Directory/hashed migration, unified catalog automation, cross-container comparison/planning, and any additive machine-readable schema |

This split is normative. A feature does not enter 1.16 merely because the writer
makes it possible. Migration and catalog automation require independent public
contracts, precedence rules, diagnostics, and compatibility review.

---

## 3. Goals

Icod.TermInfo 1.16 shall:

1. create ncurses-compatible Berkeley DB Hash-v9 terminfo stores without loading,
   bundling, or invoking a native Berkeley DB runtime in production;
2. publish canonical terminal names and aliases through the record-envelope model
   already qualified by the 1.15 reader;
3. accept validated opaque compiled-entry bytes rather than implement a second
   terminfo compiler;
4. validate every compiled entry through existing Runtime semantics before any
   destination is committed;
5. produce byte-identical database images for equivalent ordered-independent
   logical input and writer options;
6. use a documented canonical output profile: Hash-v9, little-endian metadata,
   fixed 4096-byte pages, and exact ordinal UTF-8 keys;
7. support hash collisions, bucket growth, inline key/data pairs, overflow pages,
   and compiled records spanning overflow storage;
8. reject duplicate keys, conflicting aliases, identity mismatches, unsafe names,
   malformed compiled entries, and resource-limit violations before publication;
9. create the complete database at a unique sibling temporary path, verify it
   through the shipped reader, and commit it using a same-filesystem rename or
   replacement primitive;
10. leave an existing destination unchanged when validation, construction,
    verification, cancellation, or pre-commit I/O fails;
11. expose explicit overwrite policy and deterministic publication results;
12. add explicit managed `tic` selection between conventional directory output
    and hashed-file output while preserving directory output as the default;
13. preserve the Runtime-only production dependency of
    `Icod.TermInfo.BerkeleyDb`;
14. preserve all frozen Runtime, Source, Compiler, Termcap, Inspection, JSON v1-v6,
    command, package, and archive contracts except for the reviewed additive 1.16
    surface;
15. maintain equivalent reusable API on `net8.0`, `net9.0`, and `net10.0`; and
16. qualify output against native ncurses/Berkeley DB readers on Linux and macOS,
    with managed-only writer/reader qualification on Windows.

---

## 4. Non-goals

Version 1.16 does **not** include:

- directory-to-hashed migration;
- hashed-to-directory migration;
- cross-container synchronization or mirroring;
- unified directory/hashed catalog automation;
- `toe --json` support for hashed inputs;
- Inspection JSON version 7;
- cross-database comparison or migration planning;
- public repair, salvage, vacuum, or reorganization APIs;
- in-place insert, update, or delete operations;
- transactional logs, Berkeley DB environments, recovery, or replication;
- Btree, Recno, Queue, Heap, or other Berkeley DB access methods;
- arbitrary non-terminfo key/value storage;
- native Berkeley DB P/Invoke or bundled native binaries;
- big-endian database emission;
- historical pre-v9 or vendor-specific Berkeley DB write compatibility;
- implicit output-format selection from a filename suffix;
- automatic system-database installation;
- ambient replacement of `/usr/share/terminfo.db`;
- new `infocmp` or `toe` behavior;
- generic JSON import/deserialization;
- live terminal probing, terminal-session ownership, or graphics execution; or
- unrelated command-parity work such as `tput`, `tack`, `toe -u/-U`, or C
  initializer generation.

The 1.15 reader continues to accept its frozen qualified input family, including
both metadata byte orders. A write-side restriction does not narrow the reader.

---

## 5. Frozen package and dependency boundaries

### 5.1 Production dependencies

`Icod.TermInfo.BerkeleyDb` remains optional and retains its Runtime-only
dependency:

```text
Icod.TermInfo.BerkeleyDb
          |
          v
    Icod.TermInfo
```

The following production arrows remain forbidden:

```text
Icod.TermInfo -> Icod.TermInfo.BerkeleyDb
Icod.TermInfo.BerkeleyDb -> Icod.TermInfo.Compiler
Icod.TermInfo.Inspection -> Icod.TermInfo.BerkeleyDb
```

The command layer may compose optional siblings without changing reusable
package ownership:

```text
tic
 |---> Icod.TermInfo.Compiler ---> Icod.TermInfo.Source ---> Icod.TermInfo
 |
 +---> Icod.TermInfo.BerkeleyDb ---------------------------> Icod.TermInfo
```

### 5.2 Semantic ownership

- `Icod.TermInfo.Compiler` turns `TerminalDescription` values into compiled
  terminfo bytes.
- `Icod.TermInfo.BerkeleyDb` validates and places those bytes into the selected
  Hash-v9/ncurses envelope.
- `CompiledTermInfoParser` validates recovered bytes and remains the semantic
  authority.
- `tic` owns command-line destination selection, diagnostics, summaries, and
  process exit policy.

No layer may duplicate another layer's parser or capability metadata.

---

## 6. Writer contract

### 6.1 Terminfo-specific publication input

The reusable writer accepts a finite caller-owned sequence of logical terminfo
publications. Each publication contains:

- one canonical terminal name;
- zero or more aliases;
- one immutable compiled-entry byte payload; and
- no caller-controlled raw Berkeley DB page, bucket, address, or envelope state.

The target additive public surface is exactly three types:

```text
BerkeleyDbTerminalDatabaseEntry
BerkeleyDbTerminalDatabaseWriterOptions
BerkeleyDbTerminalDatabaseWriter
```

`BerkeleyDbTerminalDatabaseEntry` snapshots one canonical name, its aliases, and
its compiled-entry bytes. `BerkeleyDbTerminalDatabaseWriterOptions` snapshots
parser options, the maximum database size, the maximum total Hash record count,
and `OverwriteExisting`. `BerkeleyDbTerminalDatabaseWriter` is static and exposes
a whole-database `Write` operation accepting the destination path, a finite entry
sequence, optional writer options, and a `CancellationToken`.

HW01 must remove or reshape one of these types if the regret gate proves it
unnecessary, but it may not enlarge this target without a separately recorded
justification. The contract shall not expose a generic `Put(key, value)` API.

The writer snapshots caller collections before validation. Later caller mutation
cannot change an in-progress write.

### 6.2 Input validation

Before allocating the complete output image, the writer shall:

1. reject null publications, null names, null aliases, and null payloads;
2. require at least one publication;
3. validate canonical and alias names against the existing safe terminfo identity
   rules;
4. encode keys as exact UTF-8 without normalization, transliteration, replacement
   bytes, or locale-dependent conversion;
5. reject duplicate canonical keys;
6. reject an alias claimed by more than one canonical entry;
7. reject a canonical key that is also another entry's alias;
8. parse each compiled payload with `CompiledTermInfoParser`;
9. require the parsed canonical name and aliases to agree with the publication;
10. reject unsupported or trailing payload data under the existing parser
    contract; and
11. enforce every configured count, length, size, and work bound before commit.

Failure is fail-closed. A found malformed record never degrades into a skipped
entry or clean miss.

### 6.3 Canonical output profile

The 1.16 writer emits exactly one reviewed profile:

| Property | Required output |
| --- | --- |
| Access method | Berkeley DB Hash |
| On-disk version | 9 |
| Metadata byte order | little-endian |
| Page size | 4096 bytes |
| Key encoding | exact UTF-8 |
| Hash function | qualified Hash-v9 function used by the 1.15 reader/oracle |
| Entry order | canonical ordinal order over encoded key bytes |
| Free-list state | canonical empty/fully-accounted state for a newly built image |
| Mutation history | none; output depends only on logical input and options |

Writer options do not expose arbitrary page size, byte order, hash function, or
Berkeley DB tuning values in 1.16. Those knobs would enlarge the compatibility
surface without a demonstrated terminfo requirement.

### 6.4 Determinism

Equivalent logical input shall produce byte-identical output regardless of:

- caller enumeration order;
- current culture or UI culture;
- operating system;
- process architecture;
- temporary filename;
- destination path; or
- repeated execution.

Deterministic ordering uses encoded key bytes and explicit numeric tie-breakers,
never culture-sensitive string comparison or dictionary enumeration.

---

## 7. Hash-v9 construction requirements

The implementation must derive every stored offset and page relationship through
checked arithmetic. It shall cover:

- metadata/header initialization;
- bucket count and mask selection;
- hash-to-bucket placement;
- multiple keys in one bucket;
- deliberate hash collisions;
- deterministic pair packing;
- key/data pair offset tables;
- inline ncurses index and data records;
- overflow chains;
- large compiled-entry payloads;
- page allocation and page-number bounds;
- bucket growth thresholds;
- exact final file length; and
- reader-visible record-envelope identity.

The implementation shall not copy opaque page images from fixtures or invoke
`db_load` to create production output. Native tools remain test oracles only.

Construction may use bounded intermediate state, but it may not require holding
an unbounded multiple of the final database in memory. Exact default and maximum
resource limits are frozen in HW01/HW02 before the public writer is exposed.

---

## 8. Whole-file publication and failure atomicity

### 8.1 Prepare, verify, commit

Filesystem publication follows three phases:

```text
prepare complete sibling temporary file
               |
               v
reopen with 1.15 reader and verify every canonical name/alias/payload
               |
               v
commit with same-filesystem move/replacement
```

The temporary file is created in the destination directory so the final commit
does not cross filesystems. Its name is unique and is never derived solely from
the target filename.

### 8.2 Overwrite policy

The default refuses an existing destination. An explicit overwrite option allows
replacement only after the new image has been completely constructed, flushed,
closed, reopened, and verified.

The writer shall not silently merge with an existing hashed database. Replacement
always means replacement of the complete logical database.

### 8.3 Guarantees and limits

The writer guarantees:

- pre-commit failure leaves the prior destination bytes unchanged;
- an absent destination does not become visible as a partial database;
- cancellation before the commit boundary removes the temporary artifact;
- cancellation is not observed after the irreversible commit begins;
- temporary cleanup failure does not hide the primary exception; and
- unsupported filesystem replacement behavior fails explicitly.

Atomic visibility ultimately depends on the same-filesystem rename/replacement
semantics supplied by the host filesystem. The public documentation shall state
that boundary precisely rather than promise transactional durability on every
filesystem or network share.

### 8.4 Concurrency

Independent writer instances may prepare images concurrently, but only one may
successfully commit under a non-overwrite policy. Under explicit overwrite,
commit serialization and destination identity checks must prevent partial files,
temporary-file collisions, and accidental deletion of another writer's artifact.

1.16 does not promise coordination with external native Berkeley DB writers and
does not mutate a database held open for native write access.

---

## 9. `tic` integration

### 9.1 Explicit format selection

Managed `tic` gains:

```text
--database-format directory
--database-format hashed
```

The default remains `directory`, preserving every existing command invocation.
The option is valid only for compiled publication; check-only and source-rendering
paths reject combinations that cannot publish.

`-o` retains destination ownership:

- directory format: `-o` names the conventional database root;
- hashed format: `-o` names the exact database file.

No `.db` suffix is appended and no format is inferred from the path.

### 9.2 Existing policy composition

- `-f` controls whether an existing hashed destination may be replaced.
- `-s` reports the selected `hashed` format, exact destination, canonical entry
  count, alias key count, and warning count.
- `-c` performs validation only and writes no temporary or destination file.
- cancellation and injected-stream command testing retain their existing
  behavior.
- failures use controlled `tic` diagnostics rather than stack traces.

Hashed output requires an explicit `-o` path in 1.16. Ambient `TERMINFO`, user
home, and system locations are never selected as hashed write destinations.

### 9.3 Router and archives

The `icod-terminfo tic ...` route and standalone `tic` archives expose identical
behavior. Existing `infocmp`, `toe`, `captoinfo`, and `infotocap` command behavior
does not change.

---

## 10. Error model

Programmer-contract violations use the established argument exception vocabulary.
Malformed compiled terminfo uses the existing compiled-format exception.
Invalid publication identity or conflicting logical input uses a reviewed
writer-specific exception or `InvalidOperationException` decision frozen by
HW01. Hash-v9 construction/verification failures use
`BerkeleyDbDatabaseFormatException` only when the database image itself violates
the supported format.

Filesystem and authorization failures retain their platform exception types in
the reusable API. `tic` converts expected failures into stable diagnostics and a
failure exit status.

Exceptions and diagnostics shall not disclose recovered payload bytes, unrelated
environment values, or uncontrolled absolute temporary paths.

---

## 11. Security and resource requirements

The writer is a parser-adjacent file generator and receives the same hostile-input
treatment as the 1.15 reader. Testing shall cover:

- integer overflow and narrowing boundaries;
- maximum and one-over-maximum entry counts;
- maximum and one-over-maximum key and payload sizes;
- adversarial collision sets;
- maximum overflow-chain and database-size calculations;
- duplicate and prefix-related UTF-8 keys;
- invalid surrogate input and embedded NULs;
- aliases differing only by culture-sensitive comparison;
- path traversal names;
- destination files, directories, symlinks, and reparse points;
- read-only destinations and permission denial;
- disk-full and truncated-write simulation through injectable internal seams;
- cancellation at every prepare/verify boundary;
- concurrent publication attempts; and
- cleanup after every controlled failure.

No test may rely on the developer machine's ambient terminfo database. Checked-in
fixtures and generated temporary stores remain deterministic.

---

## 12. Interoperability strategy

Normal CI remains pure managed and independent of native ncurses or Berkeley DB.
A dedicated interoperability workflow may install native tools on Linux and
macOS to prove:

1. native Berkeley DB utilities can inspect the managed image;
2. an ncurses configuration using hashed terminfo can acquire managed-written
   canonical names and aliases;
3. managed output survives native dump/reload without semantic changes;
4. native-written and managed-written stores expose equivalent compiled payloads;
5. collision and overflow fixtures remain interoperable; and
6. Windows can create and consume the same managed image without Berkeley DB
   installed.

Native programs are test oracles only. Their licenses, headers, libraries, and
binaries are not incorporated into production assemblies or NuGet packages.

---

## 13. Public API and compatibility freeze

HW01 starts with a public-API regret gate. It must compare at least these shapes:

1. a static whole-database writer;
2. an immutable publication plan plus writer;
3. a streaming builder; and
4. a generic key/value writer.

The selected API must prefer the smallest terminfo-specific whole-database
surface. A streaming or generic database API is rejected unless the gate proves
that it is required for bounded publication.

The release may add public types only to `Icod.TermInfo.BerkeleyDb`. Runtime,
Source, Compiler, Termcap, and Inspection public APIs remain frozen. Reusable
assembly versions remain `1.0.0.0`, and API must remain equivalent across all
three target frameworks.

JSON schemas v1-v6 remain byte-for-byte frozen. No schema v7 file is introduced
in 1.16.

---

## 14. Development sequence

Each tranche follows a strict RED → GREEN → cross-host qualification cycle. A
later tranche may not silently absorb an unaccepted earlier tranche.

### HW00 / pre-Alpha — Native interoperability and backend proof

**Status:** COMPLETE / ACCEPTED  
**Accepted exact head:** `5d3cea771d6fb0187caab191632bc1ef84d6a705`  
**Accepted workflow:** `hdb00-interoperability` run `35376556264`  
**Decision record:** `docs/1.16.0-HW00-HASH-V9-WRITER-INTEROPERABILITY-AND-BACKEND-DECISION.md`

- generate a minimal managed candidate Hash-v9 image from independently reviewed
  format facts;
- prove native Berkeley DB inspection and ncurses lookup;
- freeze the canonical 4096-byte/little-endian output profile;
- prove canonical, alias, collision, and overflow record-envelope shapes;
- document licensing and production-dependency consequences; and
- reject the release design if a native runtime would be required.

**Gate:** Exact managed bytes are readable by the existing 1.15 reader and the
native oracle on Linux and macOS; the proposed production path remains pure
managed.

### HW01 / Alpha-1 — Writer contract and public-API regret gate

**Status:** COMPLETE / ACCEPTED
**Accepted exact head:** `36a71697b7c8db4b5aa9004b4cfa60f005bf4bf8`
**Accepted workflow:** `pull-request` run `35383543026` (12/12 jobs)
**Contract record:** `docs/1.16.0-HW01-BERKELEY-DB-WRITER-CONTRACT.md`

- add failing contract tests for terminfo-specific whole-database publication;
- freeze publication, options, and writer type shapes while rejecting public
  writer result and writer-specific exception types;
- freeze input snapshot, duplicate, alias, identity, encoding, and resource rules;
- preserve the Runtime-only BerkeleyDb dependency;
- add exact API-baseline tooling for the additive surface; and
- advance the coordinated development version to `1.16.0-Alpha-1` only when the
  contract tests exist.

**Gate:** PASSED — API review demonstrates no generic Berkeley DB surface and no
dependency reversal. Exact preflight, cross-target API, package, command, and
archive verification passed at the accepted head.

### HW02 / Alpha-2 — Deterministic metadata, buckets, and inline records

**Status:** COMPLETE / ACCEPTED
**Accepted exact head:** `a31125898e7ab169457a9f9060a134cfcdbe4af6`
**Accepted pull-request workflow:** run `35394326963` (12/12 jobs)
**Accepted HDB00 workflow:** run `35394327002` (3/3 jobs)
**Acceptance record:** `docs/1.16.0-HW02-DETERMINISTIC-HASH-V9-INLINE-IMAGE.md`

- implement checked metadata and page construction;
- implement canonical bucket sizing and hash placement;
- emit inline ncurses index/data records;
- canonicalize all ordering;
- add byte-exact fixtures and repeated/cross-culture determinism tests; and
- prove reader round trips for canonical names and aliases.

**Gate:** PASSED — equivalent logical input produces byte-identical three-page
inline stores across order and culture changes; the 1.15 reader recovers exact
canonical, alias, and compiled payload data; and complete production images
match the independent HW00 byte oracle for compact and both-bucket catalogs.

### HW03 / Alpha-3 — Collisions, overflow, and bounded growth

**Status:** COMPLETE / ACCEPTED
**Accepted exact code head:** `cd55a1099f72f1ce970f55eb113a3acddf532464`
**Accepted pull-request workflow:** run `35785978854` (12/12 jobs)
**Accepted HDB00 workflow:** run `35785978928` (3/3 jobs)
**Acceptance record:** `docs/1.16.0-HW03-COLLISION-OVERFLOW-BOUNDED-GROWTH.md`

- implement deterministic collision packing;
- implement overflow pages and chains;
- support large compiled-entry payloads;
- enforce page-count, chain, file-size, and work bounds;
- add adversarial collision and arithmetic tests; and
- qualify native reading of overflow fixtures.

**Gate:** PASSED — boundary, collision, and overflow stores are deterministic,
bounded, and interoperable across native Linux/macOS verification and
managed-only transported-fixture Windows qualification.

### HW04 / Alpha-4 — Public terminfo publication engine

**Status:** PLANNED

- connect the internal image builder to the frozen public writer API;
- snapshot and validate publications;
- parse every payload through Runtime;
- enforce canonical/alias identity equality;
- return deterministic publication counts/results; and
- verify net8.0/net9.0/net10.0 API equivalence.

**Gate:** A package consumer can create a complete caller-selected Hash-v9
terminfo database without Compiler or native dependencies.

### HW05 / Alpha-5 — Safe filesystem commit

**Status:** PLANNED

- implement sibling temporary-file preparation;
- flush, close, reopen, and verify through the production reader;
- implement refuse-existing and explicit-overwrite policies;
- define the cancellation commit boundary;
- harden concurrent writer behavior and cleanup; and
- test file/directory/symlink/reparse/permission/fault boundaries on all hosts.

**Gate:** Every injected pre-commit failure preserves the original destination;
successful publication exposes only a completely verified database.

### HW06 / Alpha-6 — Explicit `tic` hashed publication

**Status:** PLANNED

- add the BerkeleyDb project/package dependency to `tic`, not Compiler;
- implement `--database-format directory|hashed`;
- require explicit `-o` for hashed output;
- compose existing source resolution and compiled-byte writing with the hashed
  publisher;
- preserve `-c`, `-f`, `-s`, diagnostics, cancellation, and default directory
  behavior; and
- prove direct, routed, package-installed, and archive command parity.

**Gate:** Controlled `.ti` input published by managed `tic` is readable by
managed and native consumers, while every historical directory invocation is
unchanged.

### HW07 / Alpha-7 — Interoperability, security, and pathological hardening

**Status:** PLANNED

- expand native oracle coverage across representative ncurses entries;
- perform generated collision/overflow differential testing;
- exercise non-ASCII exact UTF-8 names without fallback manufacture;
- complete security and resource audits;
- run concurrency, cancellation, disk/permission, and mutation-boundary tests;
- verify no native assets or unexpected dependencies enter packages; and
- qualify Windows/Linux/macOS Release builds with warnings as errors.

**Gate:** Normal CI and the native interoperability workflow are green at the
same exact head.

### HW08 / Alpha-8 — Package, documentation, freeze, and stable promotion

**Status:** PLANNED

- add a deterministic writer sample and package-only consumer;
- document API, `tic`, error, filesystem, and interoperability contracts;
- freeze the additive BerkeleyDb public API manifest;
- prove removal of the 1.16 delta reconstructs the frozen 1.15 nine-type surface;
- verify package contents, symbols, licenses, dependency groups, and README;
- verify all tool packages and six RID archives;
- update changelog, versioning, compatibility, main roadmap, and release audit;
- pass the full three-host Release and dedicated native-oracle matrices; and
- promote the accepted Alpha-8 contract to stable `1.16.0` without feature or API
  changes.

**Gate:** Exact release-candidate head, artifacts, API freeze, and workflow
evidence are recorded before stable tagging.

---

## 15. Required verification matrix

The release candidate must pass:

| Dimension | Required evidence |
| --- | --- |
| Target frameworks | BerkeleyDb writer tests and package consumer on net8.0, net9.0, net10.0 |
| Operating systems | Windows, Linux, macOS |
| Configurations | Release with warnings as errors; Staging distribution gates where established |
| Existing contracts | Full solution plus explicit BerkeleyDb tests |
| Determinism | repeated process, reordered input, changed culture, all hosts |
| Managed round trip | writer → 1.15 reader → exact compiled payload and identity |
| Native round trip | managed writer → native reader/dump on Linux and macOS |
| Failure safety | injected validation, I/O, cancellation, overwrite, concurrency failures |
| Packages | exact nupkg/snupkg count, dependency groups, license, README, no native assets |
| Commands | direct `tic`, `icod-terminfo tic`, installed tool, six RID archives |
| Compatibility | frozen pre-1.16 APIs, JSON v1-v6, and default directory publication |

No normal test depends on host `/usr/share/terminfo`, ambient `TERM`, or an
installed native Berkeley DB library.

---

## 16. Documentation deliverables

Before stable promotion, 1.16 shall include:

- a Hash-v9 writer and `tic` publication guide;
- a canonical output-profile and native interoperability record;
- a security, resource, and filesystem-atomicity audit;
- an exact BerkeleyDb public-API freeze and baseline;
- a deterministic writer sample README;
- package README examples using both the reusable writer and `tic`;
- a stable release audit with exact commits and workflow runs; and
- explicit 1.17 handoff documentation for migration and catalog automation.

Documentation shall say “whole-database replacement,” not imply in-place
transactions, and shall state the host-filesystem boundary of atomic visibility.

---

## 17. Explicit 1.17 handoff

The following work is reserved for a separate 1.17 roadmap:

```text
1.16 deterministic Hash-v9 writer
          |
          +--> directory -> hashed migration
          |
          +--> hashed -> directory migration
          |
          +--> cross-container comparison and synchronization planning
          |
          +--> unified conventional/hashed catalog automation
          |
          +--> hashed-aware machine-readable output / JSON v7, if justified
```

The 1.17 design must consume the frozen 1.15 reader and 1.16 writer rather than
reimplement either. It must independently define precedence, overwrite,
partial-source, provenance, dry-run, and automation-schema rules.

---

## 18. Release north star

> **Icod.TermInfo 1.16.0 adds deterministic, bounded, pure-managed creation and
> whole-file publication of the reviewed ncurses-compatible Berkeley DB Hash-v9
> terminfo subset. It composes existing compiled-entry bytes into canonical
> hashed stores, verifies them through the frozen 1.15 reader, and adds explicit
> managed `tic` hashed output while preserving the dependency-free Runtime, the
> Runtime-only BerkeleyDb package boundary, existing directory publication, and
> every frozen 1.x API and JSON contract. Migration and catalog automation remain
> assigned to 1.17.**

If a proposed 1.16 feature does not directly improve:

```text
compiled terminfo bytes
    -> deterministic Hash-v9 image
    -> verified whole-file publication
    -> explicit tic hashed output
```

it belongs in 1.17 or a later release.
