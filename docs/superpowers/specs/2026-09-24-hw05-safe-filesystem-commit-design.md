# HW05 Safe Filesystem Commit Design

**Date:** 2026-09-24

**Release:** Icod.TermInfo 1.16.0

**Tranche:** HW05 / Alpha-5

**Status:** Design approved and locked; implementation explicitly not started

**Accepted baseline:** HW04 documentation head
`4603e9e41d0f52520a942222e03e8cd71d1864e6`

## 1. Purpose

HW05 replaces HW04's direct destination write with verified whole-file
publication. The public writer must never expose a partial Hash-v9 database and
must preserve an existing destination whenever construction, staging,
verification, cancellation, or another pre-commit operation fails.

This tranche changes filesystem publication only. It does not change the
accepted Hash-v9 bytes, public writer surface, package dependency graph, reader
contract, or the release split that defers migration and catalog automation to
1.17.0.

## 2. Governing decisions

The approved and normative decisions are:

1. The writer builds the complete deterministic image in memory before entering
   destination-specific filesystem publication.
2. Publication uses a persistent destination-specific sibling lock file.
3. A contending Icod writer waits until it acquires the lock or its existing
   cancellation token is signaled. HW05 adds no public timeout option.
4. The lock file remains on disk after publication. Deleting it could allow two
   processes to acquire locks on different filesystem objects for the same
   destination.
5. The complete image is written to a unique sibling temporary file with
   exclusive creation and write-through behavior.
6. The temporary file is flushed to durable storage, closed, reopened through
   the accepted 1.15 reader, and completely verified before commit.
7. Commit uses one same-directory move. Non-overwrite publication refuses an
   existing destination; explicit overwrite replaces it.
8. Overwrite is a complete replacement. The new destination keeps the staged
   file's fresh filesystem metadata inherited through ordinary directory
   creation rules; the old destination's ACLs, attributes, or other metadata are
   not copied.
9. Cancellation is observed through the last pre-commit check and is not
   observed after the irreversible commit operation begins.
10. Cleanup is limited to the writer's unique temporary artifact. Cleanup
    failure never hides the primary failure.
11. The atomicity claim is visibility atomicity supplied by a host filesystem's
    same-directory rename/replacement semantics. HW05 does not claim
    transactional or power-loss durability for every local, network, or virtual
    filesystem.

## 3. Alternatives considered

### 3.1 Selected: persistent sibling lock and staged atomic move

Each destination has a deterministic sibling lock path. The writer holds that
file open with exclusive sharing while it validates destination policy, stages,
verifies, and commits a unique sibling temporary file.

This coordinates independent Icod writer processes on Windows, Linux, and
macOS, keeps every publication artifact on the destination filesystem, requires
no public API expansion, and lets a crashed process release ownership through
normal operating-system handle cleanup.

### 3.2 Rejected: named operating-system mutex

A named mutex would avoid a sidecar file but would introduce platform-specific
name namespaces, path canonicalization, permission, and abandoned-owner
behavior. Those differences are unnecessary for a file publication protocol.

### 3.3 Rejected: directory-wide lock

One lock for an entire directory would be simpler but would serialize unrelated
database destinations. HW05 requires destination-specific coordination.

### 3.4 Rejected: deleting the lock file after release

Deleting a lock file is unsafe. Another writer may already hold the old file
open while a third writer creates and locks a new file at the same pathname.
The two writers would then proceed concurrently. The persistent zero-length
sidecar is therefore part of the approved protocol.

## 4. Component boundaries

The accepted public types and members remain unchanged.

`BerkeleyDbTerminalDatabaseWriter` continues to own:

- argument and option snapshots;
- entry snapshots and compiled-payload validation;
- canonical and alias identity validation;
- ncurses record planning; and
- deterministic Hash-v9 image construction.

An internal `BerkeleyDbDatabasePublisher` owns:

- lock acquisition and release;
- destination and path-state validation;
- temporary-file lifecycle;
- reopened-image verification;
- cancellation and commit boundaries;
- final move/replacement; and
- failure cleanup.

An internal `BerkeleyDbDatabasePublicationFileSystem` isolates the filesystem
operations required by the publisher. Its production implementation uses
`System.IO`. Internal injection permits deterministic failure and concurrency
tests without adding a public extension point or test-only public hook.

These are responsibility boundaries, not frozen internal type signatures. The
implementation plan may refine private method and type names while preserving
the responsibilities and observable contract in this specification.

## 5. Artifact names and lifecycle

For a destination named `terminfo.db`, the conceptual sibling names are:

```text
.terminfo.db.icod-terminfo.lock
.terminfo.db.icod-terminfo-<unique identifier>.tmp
terminfo.db
```

The lock name is deterministic for the destination and uses the destination
directory's own filename-comparison behavior. The temporary name contains a
unique per-attempt identifier and is opened with `FileMode.CreateNew`. An
existing temporary pathname is never followed, truncated, or reused.

The lock file is an ordinary zero-length implementation artifact. Its continued
presence does not mean a writer is active; exclusive ownership of its open
handle represents the active lock. A process crash releases that ownership even
though the pathname remains.

## 6. Publication flow

The complete flow is:

1. Resolve the caller path with `Path.GetFullPath` and complete all existing
   HW01-HW04 input snapshots, validation, planning, and image construction.
2. Derive the fixed sibling lock path and unique sibling temporary path.
3. Acquire the lock file with exclusive sharing. Retry only recognized lock or
   sharing contention, checking cancellation between attempts. Propagate
   permission, invalid-path, unsupported-filesystem, and unrelated I/O failures.
4. While holding the lock, validate the immediate parent, destination, lock
   file, and overwrite policy.
5. Create the temporary file exclusively, write the complete image with
   write-through behavior, flush it with `flushToDisk: true`, and close it.
6. Reopen and verify the temporary file as specified in section 8.
7. Revalidate the destination type and overwrite policy.
8. Perform the final cancellation check.
9. Commit through one same-directory `File.Move` operation. Use non-overwrite
   move by default and overwrite move only when `OverwriteExisting` is true.
10. Treat the move invocation as the irreversible boundary. Do not observe
    cancellation after it begins.
11. Release the lock after commit or failure. Leave the fixed lock file in
    place.

In-memory image construction may proceed concurrently for different writer
instances. All filesystem publication for one destination, from path validation
through cleanup or commit, is serialized by its sibling lock.

## 7. Path and object safety

Before staging and again where relevant before commit, the publisher shall:

- require an existing ordinary immediate parent directory;
- reject an immediate parent that is a symlink or reparse point;
- reject an existing destination that is a directory, symlink, or reparse
  point;
- reject an existing lock path that is a directory, symlink, or reparse point;
- refuse any existing destination when overwrite is false; and
- ensure that only its uniquely named temporary artifact is eligible for
  cleanup.

Exclusive temporary creation prevents following a pre-existing temporary
object. The destination checks prevent the supported operation from replacing a
link target or directory.

Portable managed APIs cannot make every path check and subsequent operation one
indivisible no-follow transaction. HW05 therefore does not claim resistance to
a hostile process actively replacing directory entries between checks. It
coordinates cooperative Icod writers and handles ordinary filesystem races
without exposing partial database content.

## 8. Reopened verification

Closing the temporary stream is not sufficient. Before commit, the publisher
reopens the path through the production Berkeley DB reader and verifies all
three levels of the publication:

1. **Complete image:** reopened bytes equal the deterministic image byte for
   byte and remain within `MaximumDatabaseSize`.
2. **Physical records:** the reader returns exactly the planned ncurses records,
   including exact key and value bytes, with no missing, duplicate, or extra
   record.
3. **Logical catalog:** catalog resolution returns every prepared canonical
   identity and alias with the exact compiled payload and no additional logical
   publication.

Verification uses the accepted parser and record-count bounds. It does not
reimplement Hash-v9 parsing and does not call the public path provider once per
name. A verification discrepancy is a pre-commit failure and leaves the
destination untouched.

## 9. Overwrite and concurrency contract

With `OverwriteExisting == false`:

- destination existence is checked under the lock;
- a present destination fails without staging or modifying it; and
- the final non-overwrite move remains authoritative if an uncoordinated process
  creates the destination after the check.

With `OverwriteExisting == true`:

- each cooperating Icod writer completes its stage, verification, and commit
  while holding the same destination lock;
- a later writer begins filesystem publication only after the earlier writer
  releases the lock;
- each visible destination state is a complete verified image; and
- successful replacement uses fresh metadata from the staged file rather than
  preserving metadata from the prior destination.

HW05 does not coordinate with native Berkeley DB writers or unrelated tools.
Destination state is revalidated before commit, but the library does not claim
a portable compare-and-swap identity guarantee against hostile or
non-cooperating processes.

## 10. Cancellation and failure handling

Cancellation is observed during:

- existing input preparation and image construction;
- lock acquisition retries;
- staging writes;
- reopened reading and verification; and
- the final check immediately before commit.

Cancellation before commit produces `OperationCanceledException`, removes the
unique temporary artifact when possible, preserves the destination, and releases
the lock. Cancellation requested after the final check does not interrupt or
reinterpret the commit result.

The primary operation exception is always preserved. Temporary cleanup is
best-effort; an `IOException` or `UnauthorizedAccessException` raised only by
cleanup is suppressed when another failure is already active. The persistent
lock file is not a cleanup target.

Argument, compiled-format, publication-identity, database-format, filesystem,
authorization, and cancellation failures retain the established exception
vocabulary. HW05 adds no public result or exception type.

## 11. Filesystem guarantee boundary

The temporary file and destination are siblings, so the requested move never
intentionally crosses filesystems. On a filesystem that provides atomic
same-directory rename or replacement, readers see either the complete prior
database or the complete verified replacement.

The writer requests write-through staging and explicitly flushes file content
before close. Managed .NET does not provide a uniform portable directory-sync
contract, and network or virtual filesystems may weaken rename, cache, or
power-loss semantics. Consequently:

- HW05 promises atomic visibility where the host supplies same-directory atomic
  move/replacement;
- it does not promise survival of every acknowledged directory-entry change
  after sudden power loss; and
- unsupported or refused host operations surface as their filesystem or
  platform exceptions rather than falling back to copy-and-delete publication.

## 12. Test design

HW05 begins with failing tests and preserves the existing test suite. The new
coverage shall include:

- absent-destination publication and explicit replacement;
- default refusal of an existing destination;
- byte, physical-record, and logical-catalog verification;
- deterministic failures during create, write, flush, close, reopen, read,
  verification, and move;
- preservation of prior bytes for every injected pre-commit failure;
- no partial destination when the destination was initially absent;
- primary-exception preservation when temporary cleanup also fails;
- cancellation while waiting for the lock and at each pre-commit phase;
- no cancellation observation after commit begins;
- two non-overwrite writers, where exactly one commits;
- serialized overwrite writers, with every observed and final database complete;
- file, directory, symlink, reparse-point, read-only, and permission-denied
  boundaries;
- unique temporary naming and cleanup without cross-writer deletion;
- intentional persistence and safe reuse of the sibling lock file; and
- net8.0, net9.0, and net10.0 qualification on Windows, Linux, and macOS.

The internal filesystem seam may decorate real temporary-directory operations
to inject failures and synchronization barriers. Tests must not use timing alone
to prove serialization and must not depend on the developer machine's ambient
terminfo database.

## 13. Compatibility and scope boundaries

HW05 adds no public type or member, changes no accepted Hash-v9 byte, and adds no
production dependency. `Icod.TermInfo.BerkeleyDb` remains pure managed,
Runtime-only, IL-only, and free of native Berkeley DB assets or P/Invoke.

The following remain outside HW05:

- `tic` hashed-output integration, owned by HW06;
- migration and catalog automation, deferred to 1.17.0;
- in-place or incremental database mutation;
- coordination with native Berkeley DB writers;
- metadata-preserving destination replacement;
- public lock, retry, timeout, or filesystem-abstraction APIs; and
- transactional logging, recovery, or universal power-loss durability.

## 14. Acceptance boundary

HW05 is acceptable only when exact-head cross-host evidence proves that:

- every controlled pre-commit failure leaves an existing destination
  byte-for-byte unchanged;
- an initially absent destination is never exposed partially;
- every successful destination reopens as the exact verified logical catalog;
- non-overwrite and overwrite writers obey the approved lock protocol;
- cancellation and cleanup follow the frozen commit boundary;
- unsupported filesystem behavior fails without copy-and-delete fallback;
- the public API, package graph, and accepted Hash-v9 bytes remain frozen; and
- normal and native-interoperability workflows pass at the same exact code
  head.

This document freezes the design only. No implementation plan or product-code
authorization is implied by its approval or commit.
