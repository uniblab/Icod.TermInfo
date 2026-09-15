# Icod.TermInfo 1.15.0 — Berkeley DB / Hashed Terminfo Acquisition Roadmap

**Development line:** `1.15.0`  
**Theme:** Berkeley DB / Hashed Terminfo Acquisition  
**Primary package family:** `Icod.TermInfo`  
**Proposed optional package:** `Icod.TermInfo.BerkeleyDb`  
**Language:** C# 13  
**Reusable target frameworks:** `net8.0`; `net9.0`; `net10.0`  
**Status:** Approved for development; HDB00 interoperability research is the first implementation gate

---

## 1. Release definition

Icod.TermInfo 1.15 closes the principal remaining acquisition-format gap in the
runtime package family: ncurses-compatible hashed terminfo databases backed by
Berkeley DB.

The release is deliberately **acquisition-first and read-only**. It does not add
a second compiled-term parser, a general-purpose Berkeley DB implementation, or
hashed-database writing to `tic`.

The governing architecture is:

```text
Berkeley DB / hashed store
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

The most important architectural requirement is that the hashed-store layer
stops at **compiled entry bytes**. Everything after that boundary remains owned
by the existing `Icod.TermInfo` runtime.

Current ncurses documentation explicitly permits terminal databases to be
configured as directory trees or hashed databases, and a hashed configuration
may use a path such as `/usr/share/terminfo.db`. 1.15 targets that storage shape
without changing the existing terminfo semantic model.

---

## 2. Goals

Icod.TermInfo 1.15 shall:

1. acquire compiled terminfo entries from supported ncurses-compatible Berkeley
   DB / hashed stores;
2. preserve the existing `TerminalDescription` semantic model unchanged;
3. reuse `CompiledTermInfoParser` as the only compiled-entry semantic parser;
4. keep the base `Icod.TermInfo` package free of a mandatory Berkeley DB package
   or native-library dependency;
5. expose an explicit reusable provider for caller-selected hashed databases;
6. provide an opt-in system-discovery provider capable of recognizing both
   conventional directory databases and supported hashed stores;
7. preserve existing provider caching, retry, identity-validation,
   resource-bound, and error semantics wherever applicable;
8. distinguish clean misses, malformed terminfo data, malformed or unsupported
   database containers, I/O failures, and unavailable Berkeley DB backends;
9. remain deterministic and safe under hostile database contents;
10. support the existing reusable target matrix: `net8.0`, `net9.0`, and
    `net10.0`;
11. integrate naturally with `TerminalDatabase` and existing provider
    composition;
12. provide fixture-based interoperability tests against authoritative
    ncurses/Berkeley DB-generated databases; and
13. preserve all previously frozen Runtime, Source, Compiler, Termcap, and
    Inspection contracts unless an additive 1.15 API is explicitly reviewed and
    frozen.

---

## 3. Non-goals

Version 1.15 does **not** include:

- a general-purpose Berkeley DB API;
- arbitrary Berkeley DB application tables unrelated to terminfo;
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
- terminal/session ownership;
- bundled contemporary Oracle Berkeley DB binaries; or
- silent fallback from malformed hashed data to an unrelated storage format.

A successful 1.15 release should make future hashed-database writing easier, but
writing remains a separate design problem with different atomicity, locking,
transaction, and distribution concerns.

---

## 4. Architectural principles

### 4.1 The existing parser remains authoritative

The hashed-store implementation SHALL NOT parse terminfo capability tables.

It may understand only enough of the database container to locate the value
associated with a requested terminal name. That value is then passed unchanged
to:

```text
CompiledTermInfoParser.Parse(...)
```

The data flow is therefore:

```text
requested terminal name
          |
          v
hashed-store exact-key lookup
          |
          v
opaque stored value
          |
          v
compiled-entry size/bounds validation
          |
          v
CompiledTermInfoParser
          |
          v
TerminalDescription
          |
          v
canonical-name / alias identity verification
```

No second terminfo parser is permitted.

### 4.2 Berkeley DB support is optional

The preferred package structure is:

```text
Icod.TermInfo.BerkeleyDb
        |
        v
Icod.TermInfo
```

`Icod.TermInfo` remains dependency-free. Consumers which do not need hashed
stores do not acquire Berkeley DB-specific code or dependencies.

The reverse dependency is forbidden:

```text
Icod.TermInfo
      |
      v
Icod.TermInfo.BerkeleyDb
```

### 4.3 Read-only first

The 1.15 provider opens databases read-only. There is no mutation path.

This intentionally avoids write locking, transaction, crash-consistency,
recovery, alias-publication, and atomic replacement questions until the storage
reader contract is proven.

### 4.4 Explicit backend availability

Installing the optional package must not imply that a compatible native Berkeley
DB backend is present on the host.

Backend absence becomes observable only when a hashed store is explicitly
requested or encountered by the hashed-aware provider.

### 4.5 Public API follows interoperability evidence

No production provider API is frozen before HDB00 demonstrates the exact
ncurses/Berkeley DB interoperability contract. Public type names in this roadmap
are provisional until that gate is accepted.

---

## 5. Backend strategy

Berkeley DB support presents three broad implementation strategies.

### Approach A — dynamic Berkeley DB compatibility API

Use an installed Berkeley DB compatibility API through a small native interop
layer and bind it dynamically at runtime.

Advantages:

- delegates Berkeley DB file-version details to Berkeley DB itself;
- keeps terminfo-specific code small;
- matches the architecture historically used by ncurses hashed-database support;
- preserves a clean `key -> value bytes` boundary.

Disadvantages:

- compatible library availability varies by platform;
- library names and ABI details require qualification;
- Windows and macOS cannot assume a suitable backend exists.

**This is the preferred initial strategy, subject to HDB00 evidence.**

### Approach B — pure managed hashed-database reader

Implement only the Berkeley DB hash-file subset needed for read-only exact-key
lookup.

Advantages:

- no native dependency;
- strongest deterministic deployment story;
- all resource controls remain managed.

Disadvantages:

- much larger implementation and attack surface;
- multiple Berkeley DB revisions and byte-order details;
- substantial risk of accidentally becoming a general Berkeley DB project;
- greater long-term maintenance burden than the terminfo feature warrants.

This remains a fallback or future direction, not the default 1.15 assumption.

### Approach C — bundle Berkeley DB native binaries

Advantages:

- predictable backend availability.

Disadvantages:

- per-RID native packaging;
- larger artifacts;
- security-update responsibility;
- licensing and redistribution complexity;
- undesirable coupling of Icod release cadence to third-party native binaries.

This approach is **not planned for 1.15**.

---

## 6. HDB00 decision gate

HDB00 must answer, with executable evidence:

- which Berkeley DB implementations/releases can open the selected current
  ncurses hashed terminfo fixture;
- which library names and ABI shapes are required on Linux;
- what support is practical on macOS;
- what support is practical on Windows;
- how canonical names and aliases are represented as keys;
- exactly what bytes are returned for a terminal record;
- whether any ncurses-specific wrapper bytes exist around the compiled entry;
- how a clean missing key is represented;
- how the backend reports wrong access method or unsupported file revision;
- whether read-only access can avoid Berkeley DB environment/transaction
  infrastructure; and
- what native resources must be closed after lookup.

If those questions cannot be answered reliably, public API work stops after
HDB00 rather than freezing an assumed ABI.

The decisive acceptance proof is:

```text
ncurses-generated hashed database
          |
          v
exact terminal-name lookup
          |
          v
returned value bytes
          |
          v
existing CompiledTermInfoParser
          |
          v
expected TerminalDescription
```

---

## 7. Proposed package and API shape

Subject to HDB00, the intended package is:

```text
Icod.TermInfo.BerkeleyDb
```

Likely public concepts are:

```text
BerkeleyDbTerminalDescriptionProvider
BerkeleyDbTerminalDescriptionProviderOptions
BerkeleyDbSystemTerminalDescriptionProvider
BerkeleyDbSystemTerminalDescriptionProviderOptions
BerkeleyDbBackendAvailability
BerkeleyDbBackendUnavailableException
BerkeleyDbDatabaseFormatException
```

The package exposes **terminfo acquisition concepts**, not a general Berkeley DB
wrapper. Raw database handles, cursors, transactions, pages, hash tables, and
native ABI structures remain internal.

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
- snapshotted parser options;
- backend-selection policy;
- successful-description caching;
- exact-name lookup;
- compiled-entry parsing; and
- requested-name identity verification.

It does not inspect environment variables or platform search paths.

### 7.2 System provider

The optional package should also provide a hashed-aware system provider for
callers that want ncurses-like discovery across both storage shapes.

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

It preserves the existing snapshot-at-construction philosophy and reuses the
existing discovery policy instead of creating a second independent precedence
implementation.

---

## 8. Provider semantics

### 8.1 Exact-name lookup

Lookups are ordinal. A request for `xterm-256color` must not match a case-folded,
normalized, truncated, or approximate key.

### 8.2 Identity validation

After the store returns a value:

1. parse it with `CompiledTermInfoParser`;
2. compare the requested name with `TerminalDescription.Name`;
3. if unequal, compare against `TerminalDescription.Aliases`; and
4. reject the result if neither matches.

A database key alone is not sufficient proof of terminal identity.

### 8.3 Caching

The provider should retain the established Icod model:

- successful descriptions are cached per exact requested name;
- clean misses are retryable;
- malformed-data failures are retryable;
- I/O failures are retryable;
- backend-unavailable failures are retryable; and
- a new provider deliberately refreshes previously successful cached entries.

### 8.4 Native handle lifetime

The first implementation should prefer bounded native handle lifetime over
connection caching:

```text
lookup
  |
open DB read-only
  |
get key
  |
copy bounded value to managed memory
  |
close DB
  |
parse value
```

Long-lived native connection caching may be revisited only if profiling proves
it necessary.

---

## 9. Discovery and compatibility semantics

The existing `SystemTerminalDescriptionProvider` explicitly treats reached
non-directory/hashed paths as unsupported. That is part of its frozen 1.x
behavior.

Version 1.15 therefore introduces a separate hashed-aware provider rather than
silently redefining the existing provider:

```text
SystemTerminalDescriptionProvider
    existing 1.x behavior

BerkeleyDbSystemTerminalDescriptionProvider
    encoded + directory + supported hashed-store behavior
```

Consumers explicitly opt into the new capability.

A future major version may reconsider whether hashed support should become
intrinsic to the default system provider.

---

## 10. Error model

The provider must distinguish at least these states.

### Clean miss

A valid readable database contains no matching key.

```text
TryLoad(...) == false
```

No exception.

### Backend unavailable

A hashed store is requested but no compatible backend can be loaded.

Provisional result:

```text
BerkeleyDbBackendUnavailableException
```

### Unsupported or malformed database container

The path is not a supported readable hashed terminfo database.

Provisional result:

```text
BerkeleyDbDatabaseFormatException
```

### Malformed terminfo value

Lookup succeeds but the returned value is not a valid supported compiled terminfo
entry.

The existing `CompiledTermInfoFormatException` remains authoritative.

### Identity mismatch

The key returns a valid compiled entry that does not identify the requested
canonical name or alias.

The result uses the existing directory-provider identity-failure semantics.

### Filesystem/native I/O failure

Permission failures, sharing failures, corruption reported by the backend, and
other operational failures propagate as failures and are never converted into a
clean miss.

---

## 11. Resource and security bounds

Hashed database support creates a new hostile-input boundary.

Mandatory rules are:

1. `CompiledTermInfoParserOptions.MaximumEntrySize` remains authoritative.
2. Native result lengths are validated before managed allocation/copy.
3. Negative, overflowing, impossible, or excessive lengths are fatal failures.
4. Terminal names are database keys, never filesystem paths in the hashed
   provider.
5. A malformed hashed database is never reinterpreted as termcap, terminfo
   source, a conventional directory, encoded `TERMINFO`, or another database
   type.
6. Every retrieved value is reparsed and identity-checked before it is returned.

Permanent adversarial coverage must include:

- random database bytes;
- truncated headers/pages;
- oversized records;
- invalid page references;
- wrong database access method;
- unsupported versions;
- malformed keys;
- malformed terminfo values;
- valid terminfo under the wrong key;
- aliases;
- very long requested names;
- unusual byte order where applicable;
- concurrent lookups;
- backend absence;
- permission failures; and
- file replacement during acquisition.

---

## 12. Tool-suite impact

The 1.15 focus is reusable acquisition. Tool changes are thin consequences of
that capability.

### `infocmp`

`infocmp` should be able to inspect a terminal from an explicitly selected
supported hashed store once provider integration is accepted.

The command must not contain Berkeley DB parsing logic.

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
additions exactly as it routes their existing command behavior.

---

## 13. Tranche plan

### HDB00 — Interoperability Research and Backend Decision

**Purpose:** remove format/ABI uncertainty before public API design.

Deliverables:

- authoritative ncurses hashed-store fixture corpus;
- Berkeley DB compatibility/version matrix;
- platform/backend availability matrix;
- exact key/value observations;
- canonical-name and alias observations;
- read-only exact-key prototype;
- iteration prototype;
- malformed/wrong-format behavior report;
- licensing/distribution decision record; and
- selected backend strategy.

Acceptance gate: at least one authoritative ncurses-generated hashed database can
be opened and its retrieved compiled bytes are accepted unchanged by the existing
`CompiledTermInfoParser`.

### HDB01 — Optional Package Foundation

Create:

```text
Icod.TermInfo.BerkeleyDb
tests/Icod.TermInfo.BerkeleyDb.Tests
```

Requirements:

- `net8.0;net9.0;net10.0`;
- C# 13;
- coordinated 1.15 prerelease version;
- reusable assembly identity `1.0.0.0`;
- LGPL-3.0-or-later licensing consistent with reusable library packages;
- direct dependency on `Icod.TermInfo` only;
- no dependency from Runtime back to this package;
- no bundled Berkeley DB native binary;
- complete public XML documentation;
- API/package baseline infrastructure; and
- basic backend-availability reporting.

No terminal acquisition is required in HDB01.

### HDB02 — Read-only Berkeley DB Adapter

Implement the smallest internal primitive needed by terminfo:

```text
open
get exact key
copy bounded value
clean miss
close
```

Requirements:

- read-only access;
- no writes or transactions;
- deterministic backend probing;
- explicit unavailable-backend behavior;
- safe native lifetime management;
- exact cleanup under success and exceptions; and
- thread-safe independent operations.

The adapter remains internal and is not a general Berkeley DB API.

### HDB03 — Explicit Hashed Terminal Provider

Implement the reviewed explicit provider.

Requirements:

- canonical absolute database path;
- parser-option snapshot;
- terminal-name validation;
- exact-key lookup;
- compiled-size bounds;
- existing parser reuse;
- canonical/alias identity verification;
- successful-result caching;
- retryable misses/failures;
- concurrency tests; and
- package-only consumer validation.

This tranche completes the central architecture:

```text
Berkeley DB
   -> bytes
   -> CompiledTermInfoParser
   -> TerminalDescription
```

### HDB04 — System Discovery Integration

Implement the reviewed hashed-aware system provider.

Requirements:

- preserve existing discovery precedence;
- preserve snapshot-at-construction behavior;
- distinguish encoded `TERMINFO`, directories, and supported hashed files;
- deduplicate equivalent locations;
- preserve clean-miss semantics;
- never alter the frozen behavior of `SystemTerminalDescriptionProvider`; and
- reuse existing discovery-policy implementation instead of duplicating it.

Any Runtime refactor needed for this should remain internal unless a separate
public API addition is independently justified.

### HDB05 — Hashed Catalog Enumeration

Add deterministic read-only enumeration needed by tooling and diagnostics.

Requirements:

- enumerate logical terminal records safely;
- distinguish canonical entries and aliases where the authoritative store format
  permits;
- validate every emitted compiled entry through Runtime;
- bound record counts and record sizes; and
- impose deterministic ordering independent of physical Berkeley DB hash order.

Physical database iteration order is not an Icod API contract.

### HDB06 — Inspection and Tool Integration

At minimum, integrate explicit hashed acquisition with `infocmp`.

If HDB05 is accepted, integrate hashed listing/discovery with `toe`.

Requirements:

- direct and routed command equivalence;
- existing directory behavior unchanged;
- no duplicated database parser;
- no command-owned Berkeley DB interop;
- deterministic stdout/stderr;
- clear backend-unavailable diagnostics; and
- existing exit-status conventions retained.

`tic` remains directory-write-only.

### HDB07 — Adversarial and Compatibility Hardening

Permanent tests shall cover valid stores, aliases, missing terminals, malformed
containers, unsupported access methods, truncated files, corrupt records,
malformed compiled values, wrong-key valid entries, oversized values, culture
independence, repeated/concurrent lookup, failure retry, provider refresh, file
replacement, permission failures, and backend absence.

Differential fixtures should compare equivalent directory and hashed acquisition:

```text
ncurses hashed record
        vs
conventional compiled entry
        |
        v
same TerminalDescription semantics
```

### HDB08 — Packaging and Cross-platform Qualification

Qualification must cover Windows, Linux, and macOS while clearly distinguishing:

```text
managed package supported
```

from:

```text
compatible native Berkeley DB backend present
```

CI must include:

1. at least one host with a real compatible backend and real hashed-store
   interoperability;
2. backend-absent hosts verifying deterministic unavailable-backend behavior;
3. package-only consumers for `net8.0`, `net9.0`, and `net10.0`;
4. coordinated package validation;
5. installed `Icod.TermInfo.Tools` smoke where applicable; and
6. all six existing standalone tool archive RIDs.

No Berkeley DB binary may accidentally leak into an artifact.

### HDB09 — API Freeze, Documentation, and Stable Promotion

Final closure includes:

- exact public API manifest;
- package dependency verification;
- cross-TFM API equivalence;
- Runtime frozen-API reconstruction;
- dependency-direction tests;
- acquisition guide;
- backend compatibility guide;
- security/resource-bound audit;
- release audit;
- root README and sample-index updates;
- CHANGELOG entry; and
- exact complete CI qualification.

Stable `1.15.0` promotion adds no new behavior beyond the accepted final
prerelease contract.

---

## 14. Sample

Add a deterministic non-interactive reusable sample:

```text
samples/Icod.TermInfo.BerkeleyDb.Sample
```

It demonstrates:

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

It should also demonstrate provider composition with `TerminalDatabase` and a
built-in fallback. The sample must not depend on the user's ambient host database;
a checked-in or CI-generated fixture is required.

---

## 15. Documentation set

The 1.15 line should produce at least:

```text
Icod.TermInfo-1.15.0-Berkeley-DB-Hashed-Terminfo-Acquisition-Roadmap.md

docs/1.15.0-HASHED-DATABASE-ACQUISITION-GUIDE.md
docs/1.15.0-BERKELEY-DB-BACKEND-COMPATIBILITY.md
docs/1.15.0-BERKELEY-DB-SECURITY-AND-RESOURCE-AUDIT.md
docs/1.15.0-RELEASE-AUDIT.md
```

The compatibility guide must distinguish Icod API support, operating-system
support, backend availability, tested Berkeley DB implementations, tested
ncurses producers, and unsupported database types/versions.

---

## 16. Compatibility requirements

The 1.15 line must preserve:

- Runtime's frozen 1.0 public API unless explicitly exempted;
- reusable assembly identity `1.0.0.0`;
- `net8.0`/`net9.0`/`net10.0` API equivalence;
- dependency direction;
- existing directory acquisition;
- existing encoded `TERMINFO` behavior;
- existing built-in fallback;
- existing parser semantics;
- existing provider cache semantics;
- existing Source/Compiler/Termcap/Inspection APIs;
- all frozen JSON schemas; and
- all current five-command behavior except reviewed additive hashed-store
  acquisition paths.

A caller which does not install or instantiate the Berkeley DB package should
observe no semantic change.

---

## 17. Success criteria

Icod.TermInfo 1.15 is successful when all of the following are true.

### Acquisition

A real ncurses-generated hashed database can be queried by exact terminal name
and yields a `TerminalDescription` semantically identical to the corresponding
conventional compiled entry.

### Aliases

Canonical and alias requests return the correct immutable semantic description
when the authoritative fixture declares those identities.

### Isolation

`Icod.TermInfo` remains free of a mandatory Berkeley DB dependency.

### Parser reuse

No Berkeley DB code parses terminfo capability tables.

### Safety

Malformed databases, malformed values, identity mismatches, unsupported
containers, backend absence, and ordinary clean misses remain distinguishable.

### Determinism

Lookup, diagnostics, catalog output, and command behavior are deterministic.

### Portability

All managed projects build and test on Windows, Linux, and macOS; backend-specific
support is explicitly qualified rather than guessed.

### Distribution

NuGet packages, installed tools, and standalone archives continue to satisfy the
established coordinated release gates.

### Compatibility

All previously frozen 1.x contracts remain green.

---

## 18. Explicit deferred work after 1.15

Successful read-only acquisition establishes a foundation for later work:

```text
1.15 hashed acquisition
      |
      +--> future hashed catalog/tooling expansion
      |
      +--> future hashed database writer
      |
      +--> future tic hashed publication
      |
      +--> future directory <-> hashed migration
```

A future writer release must independently design overwrite policy, atomicity,
locking, transactions, crash recovery, alias publication, concurrent
readers/writers, file replacement, backup policy, destination selection, and
license/distribution implications.

---

## 19. North-star rule

The release definition is:

> **Icod.TermInfo 1.15.0 adds optional, read-only acquisition of supported
> ncurses-compatible Berkeley DB / hashed terminfo stores. The optional provider
> retrieves opaque compiled-entry bytes and delegates all terminfo interpretation
> to the existing frozen Runtime parser. The release preserves the
> dependency-free core runtime, conventional directory acquisition, provider
> semantics, and package-family boundaries while establishing the
> interoperability and safety foundation required for possible future
> hashed-store writing.**

If a proposed 1.15 feature does not directly improve:

```text
hashed store
    -> compiled bytes
    -> existing parser
    -> TerminalDescription
```

it belongs in a later release.
