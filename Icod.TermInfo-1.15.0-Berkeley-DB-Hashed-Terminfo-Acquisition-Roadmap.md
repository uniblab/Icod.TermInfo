# Icod.TermInfo 1.15.0 — Berkeley DB / Hashed Terminfo Acquisition Roadmap

**Development line:** `1.15.0`  
**Theme:** Berkeley DB / Hashed Terminfo Acquisition  
**Primary package family:** `Icod.TermInfo`  
**Optional package:** `Icod.TermInfo.BerkeleyDb`  
**Language:** C# 13  
**Reusable target frameworks:** `net8.0`; `net9.0`; `net10.0`  
**Status:** HDB00–HDB08 accepted; HDB09 Alpha-8 closure candidate
**Current coordinated prerelease:** `1.15.0-Alpha-8`

---

## 1. Release definition

Icod.TermInfo 1.15 closes the principal remaining modern acquisition-format gap
in the runtime package family: ncurses-compatible hashed terminfo databases
stored in the Berkeley DB Hash on-disk format used by current ncurses hashed
terminfo configurations.

The release is deliberately **acquisition-first, read-only, and managed**.
It does not add a second compiled-term parser, a general-purpose Berkeley DB
engine, a native Berkeley DB runtime dependency, or hashed-database writing to
`tic`.

The governing architecture is:

```text
Berkeley DB / hashed store
          |
          v
managed Hash-v9 reader
          |
          v
optional hashed-store provider
          |
          v
compiled entry bytes
          |
          v
existing Icod.TermInfo parser
          |
          v
TerminalDescription
```

The most important architectural requirement remains unchanged: the hashed-store
layer stops at **compiled entry bytes**. Everything after that boundary remains
owned by the existing `Icod.TermInfo` runtime.

The HDB00 interoperability gate proved that this boundary is real rather than
aspirational. A managed reader can recover the exact compiled bytes produced by
ncurses from Berkeley DB Hash-v9 stores, including alias indirection and actual
overflow-page records, and those bytes are accepted unchanged by the existing
`CompiledTermInfoParser`.

---

## 2. Goals

Icod.TermInfo 1.15 shall:

1. acquire compiled terminfo entries from supported ncurses-compatible Berkeley
   DB Hash-v9 stores;
2. preserve the existing `TerminalDescription` semantic model unchanged;
3. reuse `CompiledTermInfoParser` as the only compiled-entry semantic parser;
4. keep the base `Icod.TermInfo` package free of Berkeley DB-specific code and
   dependencies;
5. keep `Icod.TermInfo.BerkeleyDb` free of native Berkeley DB runtime binaries,
   P/Invoke bindings, or a mandatory third-party database package;
6. expose an explicit reusable provider for caller-selected hashed databases;
7. provide an opt-in system-discovery provider capable of recognizing both
   conventional directory databases and supported hashed stores;
8. preserve existing provider caching, retry, identity-validation,
   resource-bound, and error semantics wherever applicable;
9. distinguish clean misses, malformed terminfo data, malformed database
   containers, unsupported Berkeley DB access methods or revisions, and ordinary
   filesystem/I/O failures;
10. remain deterministic and safe under hostile database contents;
11. support `net8.0`, `net9.0`, and `net10.0` with the same public API;
12. integrate naturally with `TerminalDatabase` and existing provider
    composition;
13. maintain fixture-based differential interoperability tests against real
    ncurses/Berkeley DB-produced stores; and
14. preserve all previously frozen Runtime, Source, Compiler, Termcap,
    Inspection, JSON, command, package, and archive contracts unless an additive
    1.15 change is explicitly reviewed and frozen.

---

## 3. Non-goals

Version 1.15 does **not** include:

- a general-purpose Berkeley DB API;
- a general-purpose Berkeley DB database engine;
- arbitrary Berkeley DB application tables unrelated to terminfo;
- Btree, Recno, Queue, Heap, or other access methods except enough recognition
  to reject them safely;
- native Berkeley DB runtime loading;
- bundled Oracle Berkeley DB binaries;
- runtime dependence on an installed Berkeley DB library;
- transactional database mutation;
- Berkeley DB environment administration;
- database recovery or repair;
- hashed-database creation or updates;
- `tic` hashed-database publication;
- directory-to-hashed or hashed-to-directory migration;
- historical vendor terminfo binary dialects;
- generic JSON import/deserialization;
- live terminal probing;
- graphics transport;
- terminal/session ownership; or
- silent fallback from malformed hashed data to an unrelated storage format.

A successful 1.15 release establishes a trustworthy reader foundation that may
support hashed writing in a later release, but writing is a separate project
with different atomicity, locking, transaction, crash-recovery, alias-publication,
and compatibility requirements.

---

## 4. Frozen architectural principles

### 4.1 The existing parser remains authoritative

The Berkeley DB package SHALL NOT parse terminfo capability tables.

It may understand only enough of the Berkeley DB container and ncurses record
envelope to recover the opaque compiled entry bytes associated with a requested
terminal identity.

The semantic pipeline is:

```text
requested terminal name
          |
          v
Hash-v9 exact-key search
          |
          v
ncurses index record(s), marker 2
          |
          v
ncurses data record, marker 0
          |
          v
opaque compiled-entry bytes
          |
          v
CompiledTermInfoParser.Parse(...)
          |
          v
TerminalDescription
          |
          v
canonical-name / alias identity verification
```

No second terminfo parser is permitted.

### 4.2 The package remains optional

The dependency direction is:

```text
Icod.TermInfo.BerkeleyDb
        |
        v
Icod.TermInfo
```

The reverse dependency is forbidden:

```text
Icod.TermInfo
      X
      |
      v
Icod.TermInfo.BerkeleyDb
```

Consumers which do not need hashed-store acquisition remain unaffected.

### 4.3 Production support is pure managed

HDB00 selected a narrowly scoped managed reader for the reviewed Berkeley DB
Hash on-disk format v9 subset required by ncurses.

Production 1.15 therefore has:

- no native Berkeley DB loader;
- no native handles;
- no ABI probing;
- no host-library discovery;
- no `runtimes/<rid>/native` package assets;
- no runtime "backend unavailable" state caused by an absent native library.

Native Berkeley DB 5.3 remains useful only as a **development and CI oracle**
for producing authoritative fixtures and performing differential comparisons.

### 4.4 Read-only first

The package reads existing database files only.

It does not mutate pages, update hash metadata, allocate pages, acquire write
locks, create database environments, or attempt recovery.

### 4.5 Public API follows validated internals

HDB00 froze the storage strategy, but it did not freeze public provider names or
options.

HDB01 establishes the package with no new public acquisition surface.
HDB02 productionizes the internal reader under tests.
HDB03 is the first tranche allowed to freeze the public provider API.

---

## 5. Backend decision

HDB00 evaluated three strategies.

### Approach A — dynamically load Berkeley DB

This was the provisional roadmap preference before experimentation.

It was rejected for production because it would introduce platform-dependent
library discovery, ABI/version qualification, native lifetime management,
distribution complications, and undesirable licensing/deployment coupling for a
feature that only needs bounded read-only record extraction.

Native Berkeley DB remains an interoperability oracle in CI, not a runtime
backend.

### Approach B — managed read-only Hash-v9 reader

**Selected by HDB00.**

The managed implementation is intentionally narrower than Berkeley DB itself.
It supports only the reviewed on-disk structures required to recover ncurses
terminfo records:

- generic Berkeley DB metadata fields needed for safe recognition;
- Hash metadata version/access-method validation;
- Hash page headers and paired key/data offsets;
- inline Hash key/data items;
- off-page key/data references;
- overflow-page chains;
- byte-order handling where the format permits it;
- ncurses marker-2 index records;
- ncurses marker-0 compiled-entry records;
- deterministic exact-key matching;
- bounded scanning/enumeration sufficient for terminfo acquisition.

It does **not** implement Berkeley DB's write path, transactions, environments,
recovery, locking, hash-table growth, page allocation, general duplicate-record
semantics, or arbitrary access methods.

HDB00 demonstrated that the reader need not reimplement Berkeley DB's hash
function in order to satisfy our read-only use case: a bounded validated page
scan/index can find exact records deterministically.

### Approach C — bundle Berkeley DB binaries

Rejected for 1.15.

Bundling would add RID-specific native packaging, third-party security-update
responsibility, larger artifacts, licensing/redistribution complexity, and an
unnecessary runtime dependency.

---

## 6. HDB00 — accepted interoperability decision

**Status:** COMPLETE / ACCEPTED

**Accepted exact head:**

```text
15fd7dab601c6b8d0d69fe973d1faa5404c32c88
```

**Accepted qualification:**

- HDB00 interoperability workflow run `34997185592` — all three jobs green;
- normal PR workflow run `34997185765` — all twelve jobs green.

The accepted experiment uses pinned ncurses source and Berkeley DB 5.3 as the
external oracle and proves:

1. ncurses canonical/alias keys resolve through marker `2` index records;
2. marker `2` ultimately reaches a marker `0` compiled-data record;
3. canonical and alias requests reach the same compiled entry;
4. the marker-0 payload is byte-for-byte identical to conventional-directory
   `tic` output;
5. `CompiledTermInfoParser` accepts those bytes unchanged;
6. clean missing keys are distinguishable from malformed/wrong database
   containers;
7. a deliberately large valid entry forces an actual Berkeley DB overflow page;
8. the managed reader reconstructs that overflow value byte-for-byte identically
   to the native Berkeley DB oracle;
9. Linux and macOS managed extraction match Berkeley DB 5.3; and
10. Windows reads the Linux-generated Hash-v9 store, including the overflow
    case, with only .NET installed and no Berkeley DB runtime library.

The full evidence and rationale are recorded in:

```text
docs/1.15.0-HDB00-BERKELEY-DB-INTEROPERABILITY-AND-BACKEND-DECISION.md
docs/1.15.0-HDB05-HASHED-CATALOG-ENUMERATION.md
```

---

## 7. Package and public API direction

The optional package is:

```text
Icod.TermInfo.BerkeleyDb
```

HDB01 deliberately exports no acquisition API. HDB03 through HDB05 accepted the
following public surface, limited to terminfo acquisition and catalog inspection:

```text
BerkeleyDbTerminalDescriptionProvider
BerkeleyDbTerminalDescriptionProviderOptions
BerkeleyDbSystemTerminalDescriptionProvider
BerkeleyDbSystemTerminalDescriptionProviderOptions
BerkeleyDbTerminalCatalogReader
BerkeleyDbTerminalCatalogReaderOptions
BerkeleyDbTerminalCatalogEntry
BerkeleyDbTerminalCatalogEntryKind
BerkeleyDbDatabaseFormatException
```

The explicit provider, its immutable options, and the format exception were
accepted in HDB03. The opt-in system provider and its immutable options were
accepted in HDB04. The fresh-snapshot catalog reader, immutable catalog options,
logical entry, and canonical/alias kind were accepted in HDB05.

There is intentionally no `BerkeleyDbBackendAvailability` or
`BerkeleyDbBackendUnavailableException`: production acquisition has no native
backend to discover.

The package must not expose raw pages, hash buckets, database cursors,
environments, transactions, Berkeley DB handles, or other general database
concepts.

### 7.1 Explicit provider

The primary reusable provider is conceptually:

```csharp
ITerminalDescriptionProvider provider =
    new BerkeleyDbTerminalDescriptionProvider(
        "/usr/share/terminfo.db"
    );
```

It owns:

- one canonical database path;
- snapshotted parser/resource options;
- successful-description caching;
- exact-name lookup;
- ncurses record-envelope resolution;
- compiled-entry parsing; and
- requested-name identity verification.

It does not inspect environment variables or platform search paths.

### 7.2 System provider

The optional package provides the accepted hashed-aware system provider for
callers that want ncurses-like discovery across both conventional and hashed
storage shapes.

Its conceptual search model is:

```text
encoded TERMINFO
        |
        v
TERMINFO path
   |          |
directory    hashed file
   |          |
   +-----+----+
         |
user database
         |
TERMINFO_DIRS
         |
platform defaults
         |
clean miss
```

It preserves the existing snapshot-at-construction philosophy and reuses
Runtime's existing internal discovery policy rather than creating a second
precedence model.

The frozen existing `SystemTerminalDescriptionProvider` does not silently gain
new file-valued path semantics during 1.x.

---

## 8. Provider semantics

### 8.1 Exact-name lookup

Lookups are ordinal.

A request for `xterm-256color` must not match a case-folded, normalized,
truncated, or approximate key.

### 8.2 Ncurses record-envelope resolution

HDB00 observed two record classes relevant to terminfo:

```text
marker 2 -> index/alias target key
marker 0 -> compiled terminfo bytes
```

Index resolution must be bounded. Cycles, empty targets, unexpected markers, and
excessive hop counts are malformed data, not clean misses.

### 8.3 Identity validation

After marker-0 bytes are parsed:

1. compare the requested name with `TerminalDescription.Name`;
2. if unequal, compare against `TerminalDescription.Aliases`; and
3. reject the result if neither matches.

A database key alone is not sufficient proof of terminal identity.

### 8.4 Caching

The provider should retain the established Icod model:

- successful descriptions are cached per exact requested name;
- clean misses are retryable;
- malformed-container failures are retryable;
- malformed compiled-entry failures are retryable;
- I/O failures are retryable; and
- a new provider deliberately refreshes previously successful cached entries.

### 8.5 Managed file lifetime

The initial production reader should prefer simple bounded managed ownership:

```text
lookup
  |
read/open database read-only
  |
validate metadata/pages under resource bounds
  |
find exact key and reconstruct bounded value
  |
resolve ncurses record envelope
  |
parse compiled entry
```

The implementation may later optimize file access if profiling demonstrates a
need, but HDB02 correctness is more important than persistent file handles or
memory mapping.

---

## 9. Error model

The public provider must distinguish at least these states.

### Clean miss

A valid readable supported database contains no requested key.

```text
TryLoad(...) == false
```

No exception.

### Unsupported database access method or revision

The file is a recognizable Berkeley DB container but is not the supported
Hash-v9 shape required by 1.15.

This is a format/compatibility failure, not a clean miss.

### Malformed database container

Metadata, page structure, item offsets, off-page references, overflow chains, or
record envelopes violate the validated format and safety rules.

The reviewed HDB03 public API should expose an acquisition-specific format
exception rather than leaking internal page-parser details.

### Malformed terminfo value

The database lookup succeeds and resolves to marker-0 bytes, but those bytes are
not a valid supported compiled terminfo entry.

The existing `CompiledTermInfoFormatException` remains authoritative.

### Identity mismatch

The key returns a valid compiled entry that does not identify the requested
canonical name or alias.

The result uses the established directory-provider identity-failure semantics.

### Filesystem/I/O failure

Missing files where an explicit path is required, permissions, sharing failures,
short reads caused by concurrent replacement, and other operational failures are
failures and are never converted into clean misses unless an existing provider
contract explicitly defines otherwise.

---

## 10. Resource and security bounds

Hashed database support creates a hostile-input boundary.

Mandatory rules are:

1. `CompiledTermInfoParserOptions.MaximumEntrySize` remains authoritative for
   recovered compiled entries.
2. Database file size, page size, page count, item offsets, off-page declared
   lengths, and overflow-chain lengths are validated before allocation/copy.
3. Arithmetic uses overflow-safe checks before offsets are converted to managed
   indices.
4. Page references outside the validated file are rejected.
5. Overflow chains are cycle-detected and bounded.
6. Unsupported encryption/checksum/layout features are rejected unless separately
   implemented and qualified.
7. Terminal names are database keys, never filesystem paths in the hashed
   provider.
8. A malformed hashed database is never reinterpreted as termcap, terminfo
   source, a conventional directory, encoded `TERMINFO`, or another database
   type.
9. Every recovered marker-0 value is parsed and identity-checked before it is
   returned.
10. Physical Hash page order is never exposed as a stable public ordering
    contract.

Permanent adversarial coverage must include:

- random database bytes;
- truncated metadata and data pages;
- invalid page sizes;
- impossible page counts;
- invalid item-offset tables;
- unsupported access methods;
- unsupported Hash revisions;
- inline records;
- off-page records;
- truncated overflow chains;
- overflow cycles;
- oversized declared values;
- malformed ncurses markers;
- excessive index chains;
- malformed terminfo values;
- valid terminfo under the wrong key;
- aliases;
- very long requested names;
- supported byte-order variants where fixtures can be established;
- concurrent lookups;
- permission failures; and
- file replacement during acquisition.

---

## 11. Tool-suite impact

The 1.15 focus is reusable acquisition. Tool changes remain thin consequences of
that capability.

### `infocmp`

`infocmp` should be able to inspect a terminal from an explicitly selected
supported hashed store once provider integration is accepted.

The command must not contain Berkeley DB page parsing logic.

### `toe`

Hashed-database enumeration is useful but is not required before exact-key
provider acquisition is stable.

If HDB05 proves safe deterministic iteration, `toe` may expose explicit hashed
catalog listing in 1.15.

### `tic`

No hashed-store output in 1.15.

`tic` continues to publish through the existing conventional database writer.
A file-valued hashed target remains unsupported rather than silently changing
storage format.

### `captoinfo` / `infotocap`

No semantic changes are required.

### `Icod.TermInfo.Tools`

The router owns no Berkeley DB semantics. It routes any accepted `infocmp`/`toe`
additions exactly as it routes current commands.

---

## 12. Tranche plan

### HDB00 — Interoperability Research and Backend Decision

**Status:** COMPLETE / ACCEPTED

Deliverables completed:

- authoritative ncurses hashed-store fixture generation;
- Berkeley DB 5.3 oracle qualification;
- Linux/macOS native-vs-managed comparison;
- Windows managed-only cross-host comparison;
- canonical/alias and marker-envelope observations;
- actual overflow-page proof;
- clean-miss/wrong-container differentiation;
- licensing/deployment analysis; and
- managed Hash-v9 production decision.

Acceptance evidence is frozen in the HDB00 decision record and PR history.

### HDB01 — Optional Package Foundation

**Status:** COMPLETE / ACCEPTED

Accepted exact head: `407fcea3a0a40e43c654a6a5ae365ee2c814ecd5`.
Closure evidence: `docs/1.15.0-HDB01-OPTIONAL-PACKAGE-FOUNDATION.md`.

Create and qualify:

```text
Icod.TermInfo.BerkeleyDb
tests/Icod.TermInfo.BerkeleyDb.Tests
```

Requirements:

- `net8.0;net9.0;net10.0`;
- C# 13;
- coordinated `1.15.0-Alpha-1` version;
- reusable assembly identity `1.0.0.0`;
- LGPL-3.0-or-later licensing consistent with reusable Icod library packages;
- direct dependency on `Icod.TermInfo` only;
- no third-party runtime package dependency;
- no native assets;
- package README and release metadata;
- explicit PR testing on Windows, Linux, and macOS;
- coordinated pack inclusion;
- exact package artifact verification;
- cross-TFM public API equivalence; and
- no exported production acquisition API yet.

HDB01 establishes packaging and dependency boundaries only. It does not expose a
provider prematurely.

### HDB02 — Managed Read-only Berkeley DB Hash-v9 Reader

**Status:** COMPLETE / ACCEPTED (`1.15.0-Alpha-2`)

Accepted implementation head: `0495addc76c0655ab99d716b19bcc303b05a6f0c`.
Closure evidence and supported-subset limits:
`docs/1.15.0-HDB02-MANAGED-HASH-V9-READER.md`.

Qualification: PR workflow #1049 / 35024125144, all 12 jobs; HDB00 workflow
#78 / 35024125102, all 3 jobs. The production reader passed 165 unit cases and
6 native-oracle cases per TFM on Windows, Linux, and macOS.

Productionize the HDB00 research findings as internal package code using
red-green TDD.

The internal reader must cover:

```text
open/read bounded file
validate Hash-v9 metadata
validate page geometry
scan Hash pages
read paired key/data items
read inline items
follow off-page items
reconstruct overflow chains
find exact key
return bounded opaque value bytes
```

Requirements:

- no native code;
- no P/Invoke;
- no Berkeley DB runtime package;
- no writes;
- no transactions or environments;
- no public database API;
- explicit unsupported-access-method/version failures;
- overflow-safe arithmetic;
- cycle/bounds validation;
- deterministic exact-key behavior;
- thread-safe independent reads;
- differential fixtures retained against the HDB00 oracle; and
- internal implementation only.

The HDB00 exploratory managed probe is not copied blindly into production.
Production code is re-established from tests and reviewed package conventions.

### HDB03 — Explicit Hashed Terminal Provider

**Status:** COMPLETE / ACCEPTED (`1.15.0-Alpha-3`)

Accepted exact head: `e2b55290f97014086b1de89f5b466e8c083ed1d3`.
Closure record:
`docs/1.15.0-HDB03-EXPLICIT-HASHED-TERMINAL-PROVIDER.md`.

The accepted public explicit provider owns one canonical database path, snapshots
all parser and resource options, validates requested terminal names, resolves
bounded marker-2/marker-0 records through the HDB02 reader, reuses
`CompiledTermInfoParser`, verifies canonical/alias identity, caches successful
results, and retries misses and failures.

Qualification: PR workflow #1057 / 35031640798, all 12 jobs; HDB00 workflow
#86 / 35031640883, all 3 jobs. The package passed 234 unit cases and 15 native
interoperability cases per TFM on Windows, Linux, and macOS, plus package-only
consumption on net8/net9/net10.

This tranche completes the central architecture:

```text
Hash-v9 store
   -> ncurses record envelope
   -> compiled bytes
   -> CompiledTermInfoParser
   -> TerminalDescription
```

### HDB04 — System Discovery Integration

**Status:** COMPLETE / ACCEPTED (`1.15.0-Alpha-4`)

Accepted exact head: `c6e05caa4cc5158f1200050e3ba3c97e4f7aa486`.
Closure record:
`docs/1.15.0-HDB04-HASHED-AWARE-SYSTEM-DISCOVERY.md`.
Plan and TDD history:
`docs/superpowers/plans/2026-09-15-hdb04-system-discovery.md`.

The accepted opt-in system provider reuses Runtime's internal discovery policy,
including construction-time environment snapshots, precedence, empty
`TERMINFO_DIRS` default components, and path deduplication. At each logical
location, an existing exact directory or supported exact hashed file wins; only
an absent exact location permits the ncurses-compatible `.db` companion.
Encoded `TERMINFO` remains first. Runtime's public API and existing
`SystemTerminalDescriptionProvider` behavior are unchanged.

Reached malformed sources fail explicitly; missing sources remain clean misses.
Only successful outer and underlying provider results are cached, so later-created
sources and replacement after failures remain retryable. Parser and resource
options are snapshotted.

Qualification: PR workflow #1060 / 35041557637, all 12 jobs; HDB00 workflow
#89 / 35041557669, all 3 jobs. The package passed 253 unit cases and 18 native
interoperability cases per TFM on Windows, Linux, and macOS. The isolated
package-only consumer loaded the public system provider and alias on
net8/net9/net10.

### HDB05 — Hashed Catalog Enumeration

**Status:** COMPLETE / ACCEPTED

Accepted implementation/qualification head:
`819ca194b51baa78f52a6464a2e64c45418eebc6`.

Coordinated prerelease: `1.15.0-Alpha-5`.

- Normal PR workflow run `35049904561`: 12/12 jobs passed.
- HDB00 interoperability run `35049904277`: 3/3 jobs passed.
- BerkeleyDb unit suite: 308/308 per TFM on Windows, Linux, and macOS.
- Native-store interoperability suite: 20/20 per TFM on all three hosts.
- The isolated package-only consumer enumerated canonical and alias
  publications and verified fresh snapshots on net8/net9/net10.
- Exact package, dependency/native-asset, cross-TFM API, installed-tool, and
  six-RID archive gates passed.

The accepted public catalog enumerates marker-2 logical publications, classifies
canonical names and aliases against Runtime-parsed identity, validates every
marker-0 storage record including orphans, and never exposes storage keys.
Each read acquires one fresh bounded image and returns an immutable ordinal
snapshot. Physical Hash-page order is not an API contract.

Container/envelope failures use `BerkeleyDbDatabaseFormatException`; compiled
parser, identity, I/O, and cancellation failures preserve their distinct
contracts. Record count, database size, parser-derived stored-item size, index
hops, duplicate keys, cycles, and cancellation are bounded.

Closure record:
`docs/1.15.0-HDB05-HASHED-CATALOG-ENUMERATION.md`.

HDB06 Inspection/tool integration was subsequently accepted under
`1.15.0-Alpha-6`. PR #45 remains open, draft, and unmerged.

### HDB06 — Inspection and Tool Integration — COMPLETE / ACCEPTED

Accepted implementation/qualification head:
`c273b99df920e8a71cd31e0e23bc6700ef9ce456`.

Coordinated prerelease: `1.15.0-Alpha-6`.

HDB06 composes the accepted BerkeleyDb provider with provider-neutral
Inspection and adds command-owned path-shape dispatch without adding reusable
public API:

- existing-file `infocmp -A/-B` paths use
  `BerkeleyDbTerminalDescriptionProvider`; existing directories and
  unclassified paths retain conventional behavior;
- explicit human `toe` file operands use
  `BerkeleyDbTerminalCatalogReader` and may be mixed with conventional
  directories in caller order;
- direct commands and the installed `icod-terminfo` routes are equivalent;
- `infocmp --all-candidates` remains conventional-directory-only;
- operand-free `toe`, `toe -a`, `toe -D`, and all frozen JSON schemas
  remain unchanged; and
- `tic` remains conventional-directory-write-only.

Qualification passed the complete 12-job PR workflow and 3-job HDB00 workflow
on Windows, Linux, and macOS. The exact head passed 108 `infocmp` cases,
62 `toe` cases, 47 router cases, and 312 BerkeleyDb cases per host/TFM where
applicable, plus 20 native-store interoperability cases per TFM on all three
hosts. Installed package smoke passed on Windows/Linux/macOS; direct-command
smoke passed for all six standalone archive RIDs. Production remains pure
managed and contains no native Berkeley DB asset or dependency.

Closure record:
`docs/1.15.0-HDB06-INSPECTION-AND-TOOL-INTEGRATION.md`.

HDB07 adversarial and compatibility hardening was subsequently accepted under
`1.15.0-Alpha-7`. PR #45 remains open, draft, and unmerged.

### HDB07 — Adversarial and Compatibility Hardening — COMPLETE / ACCEPTED

Accepted implementation/qualification head:
`ab059acf27a5bc7fb1e8cef390d47085be33b5af`.

Coordinated prerelease: `1.15.0-Alpha-7`.

Permanent tests cover valid stores, aliases, misses, unsupported access
methods, unsupported revisions, malformed metadata, truncated files, invalid
page/item geometry, off-page records, overflow chains/cycles, malformed compiled
values, wrong-key valid entries, oversized values, culture independence,
repeated/concurrent lookup, failure retry, provider refresh, file replacement,
and permission failures.

Differential fixtures continue to prove:

```text
ncurses hashed record
        vs
conventional compiled entry
        |
        v
same compiled bytes
        |
        v
same TerminalDescription semantics
```

Qualification passed normal run `35145593802` with 12/12 jobs and HDB00 run
`35145593852` with 3/3 jobs. The exact head passed 404 BerkeleyDb unit cases
and 34 native-store interoperability cases per TFM on all three hosts, 115
`infocmp` cases, 66 `toe` cases, and 47 router cases per host. Linux and macOS
each generated a native 64-entry ASCII matrix containing 192 database records
and 128 logical publications; Windows verified the transported Linux fixture
without Berkeley DB installed. Real permission denial and restoration passed on
all three hosts, as did installed-package and all six archive-RID smoke gates.

HDB07 adds no public API, package dependency, JSON schema, native runtime, or
write path. Native big-endian production, broader non-ASCII producer
compatibility, and stable reads during arbitrary concurrent replacement remain
outside its accepted claims.

Closure record:
`docs/1.15.0-HDB07-ADVERSARIAL-COMPATIBILITY-HARDENING.md`.

The separately reviewed HDB07C compatibility-expansion tranche was subsequently
accepted without changing HDB07's accepted head.
PR #45 remains open, draft, and unmerged.

### HDB07C — Compatibility Expansion — COMPLETE / ACCEPTED

Accepted implementation/qualification head:
`2c122abd7e4e63397b474f248d51273a1b7fc006`.

Coordinated prerelease: `1.15.0-Alpha-7`.

HDB07C adds exact evidence and bounded behavior in three areas:

- production path acquisition requires two complete byte-identical
  observations through one open handle and rejects unequal content or length;
- Linux and macOS independently build and natively verify a big-endian
  Berkeley DB Hash-v9 container containing byte-exact ncurses-produced records,
  while Windows reads the transported Linux fixture without Berkeley DB; and
- provider lookup and catalog enumeration implement exact UTF-8-first,
  representable-Latin-1 fallback behavior for the qualified ncurses producer
  subset, including logical-name ambiguity rejection.

Qualification passed normal run `35161853347` with 12/12 jobs and HDB00 run
`35161853443` with 3/3 jobs. The exact head passed 422 BerkeleyDb unit cases
per TFM and host, 50 native-store interoperability cases per TFM and host, 115
`infocmp` cases, 66 `toe` cases, and 47 router cases per host. Package-only
net8/net9/net10, installed-tool, six archive-RID, API, dependency, assembly,
and no-native-asset gates passed.

HDB07C adds no public API, package dependency, JSON schema, native production
runtime, or write path. Its two observations detect unequal reads but do not
provide an atomic snapshot or arbitrary writer coordination. The exact
Latin-1 subset does not imply general encoding detection, normalization, or
transliteration, and the big-endian claim does not imply a big-endian host.

Closure record:
`docs/1.15.0-HDB07C-COMPATIBILITY-EXPANSION.md`.

HDB08 advanced the coordinated prerelease to Alpha-8 and is accepted. HDB07C
remains the accepted behavior boundary; HDB08 adds qualification infrastructure
and evidence only. PR #45 remains open, draft, and unmerged.

### HDB08 — Packaging and Cross-platform Qualification — COMPLETE / ACCEPTED

Accepted implementation/qualification head:
`b25733851c963585b56fcf064e9e27c6fdd47ee5`.

Coordinated prerelease: `1.15.0-Alpha-8`.

Qualification covers:

```text
Windows
Linux
macOS
```

Production qualification must not require Berkeley DB to be installed.

Native Berkeley DB may remain in the dedicated HDB interoperability workflow as
a fixture producer/oracle on qualified Unix hosts, but ordinary consumers and
ordinary package validation use only managed .NET artifacts.

CI must include:

1. exact package dependency verification;
2. no native runtime assets;
3. package-only consumers on net8/net9/net10;
4. normal coordinated package verification;
5. installed `Icod.TermInfo.Tools` smoke where applicable;
6. all six standalone tool archive RIDs; and
7. the existing complete Windows/Linux/macOS PR matrix.

Qualification passed normal run `35171310997` with 12/12 jobs and HDB00 run
`35171310971` with 3/3 jobs. Seven nupkg and six snupkg files were produced.
The managed verifier proved exact BerkeleyDb package/dependency identity,
unsigned IL-only assembly version `1.0.0.0`, portable symbols with Source Link,
cross-TFM public-API equivalence, and no native/runtime assets. One canonical
package set passed the isolated net8/net9/net10 consumer on Windows, Linux, and
macOS. Installed-tool smoke and all six matching-host archive RIDs passed.

HDB08 changes no public API, dependency direction, acquisition behavior, JSON
schema, command contract, native-production boundary, or read-only boundary.
It created no tag or publication and did not merge or mark PR #45 ready.

Closure record:
`docs/1.15.0-HDB08-PACKAGING-AND-CROSS-PLATFORM-QUALIFICATION.md`.

HDB09 API freeze, documentation, audits, and stable promotion are next. Alpha-8
remains the current coordinated prerelease.

### HDB09 — API Freeze, Documentation, and Stable Promotion

Final release closure includes:

- exact public API manifest;
- package dependency verification;
- cross-TFM API equivalence;
- Runtime frozen-API reconstruction;
- dependency-direction tests;
- acquisition guide;
- Hash-v9 compatibility documentation;
- security/resource-bound audit;
- ecosystem/dependency audit;
- release audit;
- README feature-inventory update;
- samples update;
- CHANGELOG entry; and
- exact complete CI qualification.

Stable `1.15.0` promotion adds no behavior beyond the accepted final prerelease
contract.

HDB09 is implemented as a staged closeout. The first RED witness is exact head
`1525096d1a5925c2816159f7f7b7c826dfd7013b`: normal run `35176822414`
compiled the new closure suite and failed on the absent freeze/sample/guide/audit
authorities, while HDB00 run `35176822401` failed the intended new sensitivity
case. The Alpha-8 GREEN candidate adds:

- the complete 9-type BerkeleyDb reflection baseline with normalized-LF
  SHA-256 `f519600aa4085d07c2d20bd8dc7a32c4dc06a43f4e361554b205ce2f97a8bf36`;
- exact baseline and existing cross-TFM checks in package verification;
- Runtime frozen-API and dependency-direction closure tests;
- the controlled all-TFM `Icod.TermInfo.BerkeleyDb.Sample`;
- acquisition, Hash-v9 compatibility, security/resource, ecosystem, and release
  authorities; and
- a root changelog plus synchronized Alpha-8 release-facing documentation.

Alpha-8 closure qualification is pending. Stable promotion remains a separate
version/status-only checkpoint after that exact candidate passes the complete
normal and genuine HDB00 matrices. PR #45 remains draft and unmerged; HDB09
does not create a tag or publication.

---

## 13. Fixture and interoperability policy

The production package contains no Berkeley DB binary and need not contain large
opaque test databases merely for convenience.

The test strategy distinguishes:

### Authoritative generated fixtures

The dedicated HDB interoperability workflow may build/use qualified Berkeley DB
and ncurses versions to generate real hashed stores and compare native and
managed extraction.

### Checked-in minimal fixtures

Small reviewed binary fixtures may be checked in when they materially improve
unit/adversarial coverage and have clear provenance.

### Synthetic malformed fixtures

Tests may construct minimal malformed page images for precise bounds/error
coverage, but synthetic fixtures do not replace real ncurses differential
qualification.

---

## 14. Package policy

`Icod.TermInfo.BerkeleyDb` is an Icod implementation of a narrowly scoped
read-only file-format reader. It is **not** a redistribution or managed wrapper
of Oracle Berkeley DB.

The package should therefore contain only:

- Icod-managed assemblies;
- XML documentation;
- package metadata/README/icon/license;
- symbols and Source Link artifacts consistent with the coordinated family.

It must not contain:

- Oracle Berkeley DB binaries;
- Berkeley DB headers/source copied into the package;
- RID-specific native libraries;
- hidden runtime downloads;
- dynamic library probing code.

The package's LGPL license applies to Icod's own implementation. Third-party
Berkeley DB licensing remains relevant to CI/oracle use and to any future design
that would redistribute or link Berkeley DB itself, but not because the 1.15
production package embeds that library.

---

## 15. Samples

Once HDB03 is accepted, add a deterministic non-interactive sample such as:

```text
samples/Icod.TermInfo.BerkeleyDb.Sample
```

It should demonstrate:

```text
explicit hashed database path
          |
          v
BerkeleyDbTerminalDescriptionProvider
          |
          v
TerminalDescription
          |
          v
selected capability inspection
```

The sample must use a controlled fixture rather than depending on the host's
ambient terminfo database.

It should run on all reusable target frameworks.

---

## 16. Documentation set

The 1.15 line should maintain at least:

```text
Icod.TermInfo-1.15.0-Berkeley-DB-Hashed-Terminfo-Acquisition-Roadmap.md

docs/1.15.0-HDB00-BERKELEY-DB-INTEROPERABILITY-AND-BACKEND-DECISION.md
docs/1.15.0-BERKELEY-DB-ECOSYSTEM-AUDIT.md
docs/1.15.0-HASHED-DATABASE-ACQUISITION-GUIDE.md
docs/1.15.0-BERKELEY-DB-HASH-V9-COMPATIBILITY.md
docs/1.15.0-BERKELEY-DB-SECURITY-AND-RESOURCE-AUDIT.md
docs/1.15.0-RELEASE-AUDIT.md
```

The compatibility document must distinguish:

- Icod package/API support;
- supported Berkeley DB on-disk access method/revision;
- tested ncurses/database producers;
- byte-order/page-layout coverage;
- explicitly unsupported database features;
- operating-system support.

---

## 17. Compatibility requirements

The 1.15 line must preserve:

- Runtime's frozen 1.0 public API unless explicitly exempted;
- reusable assembly identity `1.0.0.0`;
- net8/net9/net10 API equivalence;
- dependency direction;
- existing conventional directory acquisition;
- existing encoded `TERMINFO` acquisition;
- existing built-in fallback;
- existing compiled parser semantics;
- existing provider cache semantics;
- existing Source/Compiler/Termcap/Inspection APIs;
- all frozen JSON schemas;
- all current five-command behavior except reviewed additive hashed-store
  acquisition features.

A caller which does not install or instantiate `Icod.TermInfo.BerkeleyDb` should
observe no semantic change from adding the package family member.

---

## 18. Success criteria

Icod.TermInfo 1.15 is successful when all of the following are true.

### Acquisition

A real ncurses-generated supported Hash-v9 database can be queried by exact
terminal name and returns a `TerminalDescription` semantically identical to the
corresponding conventional compiled entry.

### Aliases

Canonical and alias requests resolve correctly and identity validation prevents a
valid-but-wrong compiled entry from being accepted under an unrelated key.

### Isolation

`Icod.TermInfo` remains free of any Berkeley DB dependency.

### Managed deployment

`Icod.TermInfo.BerkeleyDb` runs on qualified Windows/Linux/macOS hosts without an
installed Berkeley DB library and ships no native database assets.

### Parser reuse

No Berkeley DB code parses terminfo capability tables.

### Safety

Malformed database metadata/pages, malformed ncurses envelopes, malformed
compiled values, identity mismatches, unsupported database containers, and clean
misses remain distinguishable.

### Determinism

Lookup, diagnostics, catalog output, and tool behavior are deterministic.

### Portability

Reusable projects build/test on net8/net9/net10 across the established platform
matrix.

### Distribution

NuGet packages, installed tools, and standalone archives continue to satisfy the
established coordinated release gates.

### Compatibility

All previously frozen 1.x contracts remain green.

---

## 19. Explicit deferred work after 1.15

Successful read-only acquisition may establish the foundation for later work:

```text
1.15 managed hashed acquisition
      |
      +--> future broader historical Berkeley DB read compatibility, if demanded
      |
      +--> future hashed catalog tooling expansion
      |
      +--> future hashed database writer
      |
      +--> future tic hashed publication
      |
      +--> future directory <-> hashed migration
```

Any future writer must independently design and test:

- hash placement/growth rules;
- metadata mutation;
- overwrite policy;
- alias publication;
- atomicity;
- file locking;
- concurrent readers/writers;
- transactions or equivalent crash safety;
- recovery behavior;
- backup/replacement policy;
- `tic` destination selection; and
- licensing/distribution implications if any third-party implementation is
  introduced.

These concerns are intentionally excluded from 1.15.

---

## 20. Release north star

> **Icod.TermInfo 1.15.0 adds optional, read-only, pure-managed acquisition of
> supported ncurses-compatible Berkeley DB Hash-v9 terminfo stores. The optional
> package recovers opaque compiled-entry bytes and delegates all terminfo
> semantics to the existing frozen Runtime parser. The release preserves the
> dependency-free Runtime, avoids native Berkeley DB deployment, and establishes
> a bounded, deterministic, cross-platform foundation for hashed terminfo
> interoperability.**

If a proposed 1.15 feature does not directly improve:

```text
Hash-v9 store
    -> bounded managed record recovery
    -> compiled terminfo bytes
    -> existing parser
    -> TerminalDescription
```

it belongs in a later release.
