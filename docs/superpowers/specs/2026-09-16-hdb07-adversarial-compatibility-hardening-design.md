# HDB07 Adversarial and Compatibility Hardening Design

**Status:** DESIGN APPROVED / SPEC REVIEW  
**Target prerelease:** `1.15.0-Alpha-7`  
**Accepted baseline:** HDB06 closure head
`37fe736b01096a2629d1fb5fdbba35c123469ddf`

## 1. Purpose

HDB07 hardens the accepted Berkeley DB Hash-v9 acquisition subset before any
compatibility expansion. It adds deterministic adversarial, lifecycle, command,
and native-valid evidence around the HDB02–HDB06 implementation. Production
code changes only when an independently committed test RED proves a defect.

HDB07 does not broaden the accepted storage format, public API, dependency
graph, discovery policy, JSON schemas, or write behavior.

## 2. Scope decision

The governing approach is a deterministic contract matrix.

Synthetic fixtures are explicit, reviewed, and reproducible. Random mutation or
fuzzing may be used offline to discover candidate cases, but generated failures
must be reduced to deterministic fixtures before they enter the permanent suite
or affect acceptance.

A validator refactor is not a prerequisite. Existing production structure is
changed only when the narrowest demonstrated defect requires it.

## 3. Architecture invariants

Hardening is divided into three independently testable layers:

1. **Hash-v9 storage validation**
   - file acquisition;
   - metadata and byte order;
   - page and item geometry;
   - inline and off-page reconstruction;
   - overflow traversal;
   - exact-key lookup and complete-record enumeration; and
   - resource and cancellation boundaries.
2. **Ncurses and Runtime semantics**
   - marker-0 payloads and marker-2 links;
   - strict publication-key decoding;
   - link limits and cycles;
   - Runtime-owned compiled-entry parsing;
   - canonical/alias identity; and
   - deterministic logical ordering.
3. **Consumers**
   - explicit provider;
   - opt-in system provider;
   - catalog reader;
   - provider-neutral Inspection composition;
   - `infocmp`, `toe`, and routed commands; and
   - package/archive execution.

The accepted dependency graph does not change:

```text
Icod.TermInfo.BerkeleyDb --> Icod.TermInfo

Icod.TermInfo.Inspection --> Icod.TermInfo + Icod.TermInfo.Source

infocmp --> Inspection + BerkeleyDb
toe     --> Inspection + BerkeleyDb
```

Runtime and Inspection do not reference BerkeleyDb. BerkeleyDb continues to
reference Runtime only. Production remains pure managed and read-only.

## 4. Versioning and public surface

HDB07 advances the coordinated suite version to exactly
`1.15.0-Alpha-7`.

Reusable assembly identities remain `1.0.0.0`. Runtime, Source, Compiler,
Termcap, Inspection, and BerkeleyDb public APIs remain unchanged and equivalent
across net8/net9/net10.

No new option, exception type, raw-record API, command switch, JSON schema,
native dependency, P/Invoke entry point, transaction API, or write path is
introduced.

## 5. Failure contracts

Existing exception boundaries remain authoritative:

- malformed or unsupported Berkeley DB structures and ncurses envelopes are
  internal `InvalidDataException` failures and become
  `BerkeleyDbDatabaseFormatException` at public BerkeleyDb boundaries;
- malformed compiled terminfo remains
  `CompiledTermInfoFormatException`;
- a valid compiled entry that does not declare the requested or published
  identity remains `InvalidDataException`;
- filesystem, sharing, and permission failures retain their original exception
  types;
- cancellation remains `OperationCanceledException`; and
- only an absent initial exact key is a clean miss.

A public provider lookup or catalog read is all-or-nothing. No partially parsed
terminal or partial catalog escapes.

`infocmp` preserves:

- `INFOCMP0002` and status 1 for a clean acquisition miss;
- `INFOCMP0003` and status 1 for construction, I/O, container, envelope,
  parser, or identity failure;
- no partial stdout after a failed acquisition;
- status 2 for usage errors; and
- status 130 for cancellation.

`toe` preserves:

- `TOE0002` and status 1 for an explicitly requested missing root;
- `TOE0005` and status 1 for a failed hashed catalog;
- no entries from the failed root;
- safely completed output from earlier roots and continued processing of later
  explicit roots; and
- status 130 for cancellation.

## 6. Deterministic failure precedence

Storage failures are reported in the accepted traversal order. Exact lookup may
stop on a match and need not validate unrelated unvisited pages or values.
Complete enumeration discovers structural failures in page/item order and
logical envelope failures in unsigned raw-byte-key order.

Catalog publication order remains ordinal publication name, then kind, then
canonical terminal name. Culture, physical page placement, hash bucket
population, or host filesystem enumeration must not affect that order.

Command root order remains caller order. A failed root does not reorder,
collapse, or suppress safely completed results from other roots.

Hardening may correct an inconsistent precedence only when a committed RED
states the intended deterministic order. It must not replace specific accepted
boundaries with a generic failure.

## 7. Storage adversarial matrix

Permanent tests cover the accepted Hash-v9 subset at exact boundaries and one
step beyond them:

- metadata magic, format, revision, byte order, page size, page count, key
  count, flags, and high-mask geometry;
- index-table bounds and bucket page references;
- page identities, page types, item counts, free-area boundaries, offset
  ordering, odd key/value pairs, and sparse/non-record pages;
- inline empty, exact-limit, and oversized keys and values;
- off-page headers, declared lengths, first-page references, noncontiguous
  overflow chains, shared overflow tails, repeated references, missing pages,
  wrong page identities, cycles, premature termination, trailing data, and
  exact accumulated lengths;
- checked arithmetic at every offset, length, page-count, and allocation
  boundary;
- duplicate exact binary keys during complete enumeration;
- exact database, item, and record-count limits;
- pre-cancellation and deterministic cancellation between pages/records;
- handle release after success, miss, corruption, I/O failure, and
  cancellation; and
- independent repeated and concurrent calls with different limits.

Repeated references are accepted only when each referenced structure is valid.
If a deterministic test demonstrates avoidable reconstruction amplification,
the correction must remain internal and must not add a public tuning option.

## 8. Logical and parser matrix

Permanent tests cover:

- marker-0 direct payloads and marker-2 link chains;
- empty records, unsupported markers, dangling targets, exact repeated-key
  cycles, multi-key cycles, and inclusive/exceeded hop limits;
- binary and embedded-NUL link targets;
- strict UTF-8 publication keys, invalid byte sequences, surrogate input, unsafe
  names, and ordinal exact matching;
- valid compiled payloads declaring the canonical name, a requested alias,
  neither, or conflicting alias evidence;
- wrong-key but otherwise valid compiled entries;
- malformed compiled entries at parser size boundaries;
- orphan marker-0 validation without publication;
- publication identity sharing for entries resolving to one marker-0 record;
- deterministic canonical/alias classification; and
- invariant behavior under representative non-default current cultures and UI
  cultures.

Culture tests restore process/thread state in `finally` blocks and do not run
in parallel with other culture-mutating tests.

## 9. Lifecycle and filesystem matrix

Provider and catalog tests cover:

- missing files, directories supplied as files, sharing denial, read-permission
  denial, deletion, and replacement;
- retry after clean miss, I/O failure, permission failure, malformed container,
  malformed envelope, parser failure, and identity failure;
- successful per-name provider caching and publication-once concurrency;
- fresh catalog snapshots and new-provider observation of replacements;
- system-provider source precedence, equivalent-location deduplication, cached
  success, and retryable miss/failure;
- independent cancellation and resource limits across concurrent calls; and
- file-handle release across all success and failure families.

Permission evidence uses isolated temporary paths and platform-native access
controls on Windows, Linux, and macOS. Tests must restore access before cleanup
in `finally`. A runner that cannot reliably enforce the denial fails the
permission-fixture setup instead of silently skipping or substituting timing,
sleeps, or a weaker sharing test. Portable exception-propagation coverage
remains separate from the real host denial.

HDB07 preserves the accepted race boundary:

- classification and opening are separate point-in-time operations;
- a selected reader/provider does not fall back to another storage shape after
  failure;
- later calls reclassify or reacquire according to their existing contract; and
- no atomic snapshot is promised against same-length concurrent external
  writes.

## 10. Command and Inspection matrix

Provider-neutral Inspection characterization proves canonical and alias
acquisition, clean misses, and exception propagation without adding a BerkeleyDb
reference.

Command tests prove:

- exact diagnostic identifiers and exit statuses;
- no partial `infocmp` stdout on acquisition failure;
- all-or-nothing output for each failed `toe` root;
- later-root continuation and caller-root order;
- existing conventional behavior;
- unchanged frozen JSON documents;
- `infocmp --all-candidates` remaining directory-only;
- operand-free `toe`, `toe -a`, and `toe -D` remaining conventional;
- no internal page, record, or stack-trace detail in diagnostics; and
- exact direct/routed stdout, stderr, and status equivalence.

## 11. Native-valid compatibility matrix

The native oracle remains a fixture producer and differential authority, not a
production dependency.

Linux and macOS generate at least three independently identified valid fixture
families:

1. a default-layout store with multiple canonical terminals and aliases;
2. a forced-overflow store containing off-page keys or values and multiple
   overflow lengths; and
3. a multi-terminal store that spans multiple populated hash pages/buckets.

The oracle also retains clean-miss checks and an unsupported access-method input
for rejection evidence. Native page-size variants are added only when the
qualified Berkeley DB producer exposes a deterministic documented control; they
are not synthesized and described as native.

The production reader, record resolver, explicit provider, system provider, and
catalog reader are compared with native extraction. Windows consumes the
Linux-generated fixtures without Berkeley DB installed.

Native big-endian producer compatibility and broader non-ASCII producer
compatibility remain outside HDB07. Existing synthetic big-endian and strict
encoding coverage remain permanent.

## 12. Fixture ownership

Synthetic adversarial fixtures are generated in test code by a focused HDB07
builder. The builder exposes reviewed layout concepts needed by tests without
copying production parsing or validation logic.

Fixtures use literal format values and independent byte writing. They do not
call production serializers, because production has no writer and the fixture
must be capable of constructing invalid geometry.

Large opaque binary fixtures are not checked in unless a minimized case cannot
be expressed clearly and reproducibly by the builder. Any checked-in binary
requires provenance and a textual assertion of the exact property it proves.

Native fixtures continue to be generated by the dedicated HDB00 scripts and
transported to Windows as workflow artifacts.

## 13. TDD checkpoints

### Checkpoint A — storage validation

Add test-only deterministic vectors for metadata, geometry, arithmetic,
off-page/overflow, limits, ordering, cancellation, and ownership.

Commit and observe the RED for each defect family before changing production.
Already-green cases are recorded as characterization and require no production
commit.

### Checkpoint B — logical semantics

Add marker, identity, parser-boundary, wrong-key, strict-encoding, culture, and
logical-order tests. Correct only independently observed REDs.

### Checkpoint C — lifecycle and filesystem behavior

Add provider/catalog/system-provider retry, caching, replacement, permission,
concurrency, cancellation, and handle-ownership tests. Timing-dependent races
are prohibited.

### Checkpoint D — commands and Inspection

Add command-boundary and provider-neutral Inspection regression cases. A
production command correction requires its own behavioral RED.

### Checkpoint E — native-valid differential expansion

Extend native generation and managed comparisons for additional valid layouts.
Any new incompatibility receives a minimized deterministic regression before a
production change.

### Checkpoint F — qualification and closure

Run exact package/API/dependency/native-asset verification, isolated consumers,
installed-tool smoke, all six archives, the complete normal PR workflow, and
the complete HDB00 workflow. Record exact heads, workflow IDs, job counts, and
per-host/TFM test counts.

## 14. CI cadence

The normal PR workflow runs for every pushed HDB07 checkpoint.

GitHub evaluates a pull request workflow's top-level `paths` filter against
the cumulative PR diff. Because PR #45 already contains
interoperability-sensitive files, later documentation-only synchronizations
still invoke the HDB00 workflow. HDB07 therefore adds an in-workflow
synchronize-delta gate based on the event's `before` and `after` commits.

The gate enables expensive HDB00 work when the latest synchronization changes:

- BerkeleyDb production code;
- interop tests;
- native generation or verification scripts;
- shared fixtures consumed by interoperability;
- package/distribution composition relevant to BerkeleyDb; or
- the HDB00 workflow or gate itself.

For `workflow_dispatch` and non-`synchronize` pull request actions, the gate
defaults to enabled. Missing or invalid synchronize commit identifiers fail the
gate rather than silently skipping qualification.

BerkeleyDb unit-test-only, design/plan, closure-document, README, and PR-body
deltas produce only the inexpensive gate jobs. Linux and macOS skip native
installation and test steps, and Windows is skipped when the Linux gate output
is false. HDB00 runs all three qualification jobs once more on the final
implementation head. A documentation-only closure synchronization may create a
cheap gated workflow run, but it is not a second interoperability
qualification.

Changing the HDB00 workflow or gate is itself sensitive and therefore receives
one full qualifying run.

## 15. Acceptance criteria

HDB07 is accepted only when:

1. every new defect correction has an independently observed RED and narrow
   GREEN;
2. characterization cases are distinguished from defect-driven cases;
3. BerkeleyDb reusable tests pass on net8/net9/net10 on Windows, Linux, and
   macOS; command tests pass on their .NET 10 target on all three hosts;
4. real permission-denial evidence passes on Windows, Linux, and macOS and is
   cleanup-safe;
5. direct/routed command equivalence remains exact;
6. Runtime, Inspection, and BerkeleyDb public API and dependency boundaries are
   unchanged;
7. production contains no native Berkeley DB asset, P/Invoke, runtime download,
   or third-party Berkeley DB package;
8. the normal PR workflow passes all 12 jobs;
9. HDB00 passes all 3 jobs on the exact final implementation head;
10. installed-tool smoke passes on Windows, Linux, and macOS;
11. all six standalone archive RIDs pass; and
12. PR #45 remains open, draft, and unmerged.

## 16. Explicit non-goals

HDB07 does not add:

- other Berkeley DB access methods, revisions, checksums, encryption,
  transactions, recovery, cursors, or writes;
- hashed publication from `tic`;
- new system-wide or heterogeneous JSON catalogs;
- hashed ambient `toe` discovery;
- general-purpose mutation fuzzing as a CI acceptance gate;
- aggregate concurrent-writer snapshot guarantees;
- native big-endian producer qualification;
- broader non-ASCII producer compatibility; or
- unrelated validator or command refactoring.

## 17. Follow-on compatibility expansion

After HDB07 acceptance, a separate reviewed design will address the requested
compatibility expansion. Candidate claims include native big-endian production,
broader non-ASCII producer interoperability, and stronger concurrent-writer
behavior.

That follow-on tranche must define its own fixtures, public compatibility
language, RED/GREEN plan, and qualification evidence. If included in
`1.15.0`, it completes before HDB08 packaging qualification. HDB07 acceptance
does not imply any of those expanded claims.
