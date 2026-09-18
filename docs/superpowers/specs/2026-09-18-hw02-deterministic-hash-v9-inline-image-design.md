# HW02 Deterministic Hash-v9 Inline Image Design

**Date:** 2026-09-18

**Release:** Icod.TermInfo 1.16.0

**Tranche:** HW02 / Alpha-2

**Status:** Approved design; implementation pending

## 1. Purpose

HW02 implements the first production Hash-v9 image builder for the writer
contract accepted in HW01. It converts fully validated terminfo publications
into a deterministic, pure-managed Berkeley DB Hash-v9 image containing
metadata, two initial bucket pages, and inline ncurses index and data records.

This tranche deliberately stops before bucket growth, collision-chain policy,
overflow pages, large payloads, public writer integration, filesystem
publication, or native qualification. Those responsibilities remain assigned to
HW03 through HW07.

The design builds on:

- the accepted HW00 output profile and native proof at
  `5d3cea771d6fb0187caab191632bc1ef84d6a705`;
- the accepted HW01 writer contract at
  `36a71697b7c8db4b5aa9004b4cfa60f005bf4bf8`; and
- the HW01 documentation acceptance head at
  `275f239ef37551f4259225f29dd09f32e7d0282b`.

## 2. Decisions

HW02 adds one internal image-building path to `Icod.TermInfo.BerkeleyDb`. The
path has three responsibilities:

1. preserve the exact compiled terminfo identity bytes needed for the ncurses
   storage key;
2. derive and canonically order the ncurses Hash records; and
3. serialize those records into the frozen three-page Hash-v9 profile.

The builder returns an in-memory `byte[]`. It does not open a stream, inspect a
destination path, write a file, or alter the public writer's HW01 behavior.
`BerkeleyDbTerminalDatabaseWriter.Write` continues to complete preflight and
throw its exact HW01 `NotSupportedException` until HW04 connects the internal
builder to the public operation.

The initial bucket count remains exactly two. An input that requires a larger
bucket, an overflow item, or an overflow page is rejected at the internal HW02
boundary. HW03 replaces those rejections with bounded bucket growth, collision
packing, and overflow construction.

## 3. Alternatives considered

### 3.1 Selected: a clean internal image builder

A focused production builder is distilled from the accepted HW00 facts rather
than from its research-only API. It accepts prepared immutable publication
state, returns exact image bytes, and has no filesystem responsibility. This
keeps terminfo validation, ncurses record planning, page serialization, and
later filesystem commit independently testable.

### 3.2 Rejected: move the HW00 research probe into production

The probe reparses compiled entries, writes directly to a stream, and already
contains overflow behavior assigned to HW03. Moving it would couple production
code to a research project, duplicate Runtime validation, and obscure tranche
boundaries.

### 3.3 Rejected: extend the reader into a read/write codec

The reader is deliberately fail-closed over a broader historical input family,
including byte-order and page-layout variations that the writer never emits.
Combining it with the single canonical output profile would mix validation and
construction responsibilities and increase regression risk to the accepted
1.15 reader.

### 3.4 Rejected: connect the public writer during HW02

Public orchestration belongs to HW04 and safe filesystem commit belongs to
HW05. Connecting either now would make the HW02 deterministic-image gate depend
on unrelated destination and lifecycle behavior.

## 4. Internal architecture

The implementation uses focused internal components rather than adding public
API:

- **Prepared publication state** retains canonical and alias UTF-8 keys, the
  exact compiled storage key, and a cloned compiled payload after HW01
  validation.
- **Ncurses record planning** creates immutable `BerkeleyDbHashRecord` values
  for publication indexes and compiled data, detects exact byte-key conflicts,
  and applies writer-specific canonical ordering.
- **Hash-v9 image construction** hashes planned keys into two buckets, checks
  inline and page bounds, creates deterministic metadata, and serializes the
  three pages.

The existing `BerkeleyDbHashRecord` remains the immutable key/value carrier.
The existing reader-oriented `ByteArrayComparer` is not changed: it uses normal
shorter-before-prefix ordering and serves accepted read paths. HW02 adds a
writer-specific comparer whose exact-prefix rule is the Berkeley DB rule proven
in HW00: compare bytes ascending, but place the longer key before its exact
prefix.

The implementation seam is explicit:

- the writer's existing `PreparedIdentity` and `PreparedPublication` nested
  records become internal immutable implementation types;
- `PreparedPublication` gains the exact `StorageKey` snapshot;
- an internal ncurses record planner consumes prepared publications and returns
  an ordered read-only collection of `BerkeleyDbHashRecord` values; and
- an internal `BerkeleyDbHashV9ImageBuilder` consumes those ordered records,
  the maximum database size, and a cancellation token and returns the image.

These internal seams are used by HW04 rather than existing solely for tests.
The existing test assemblies may exercise them through their current
`InternalsVisibleTo` grants. No testing-only public hook is added.

## 5. Exact storage-key rule

After `CompiledTermInfoParser` accepts a cloned payload and HW01 confirms its
canonical name and ordered aliases, preflight reads the compiled header's
little-endian names-section length. The storage key is an exact copy of that
names section excluding only its terminating NUL byte.

For an entry whose compiled identity is:

```text
canonical|alias-1|alias-2|description\0
```

the storage key is the exact byte sequence for:

```text
canonical|alias-1|alias-2|description
```

The storage key is never reconstructed from .NET strings. In particular, the
description bytes and their original encoding remain opaque and unchanged.
HW01's declared canonical and alias values remain strict UTF-8 publication keys
and must still agree ordinally with the Runtime parser. The already successful
Runtime parse is authoritative for format validity; storage-key extraction does
not become a second compiled terminfo parser.

## 6. Ncurses record derivation

Each prepared publication produces these physical Hash records:

| Logical item | Hash key | Hash value |
| --- | --- | --- |
| Canonical publication | Exact canonical UTF-8 bytes | Byte `2`, then the exact storage key |
| Each alias publication | Exact alias UTF-8 bytes | Byte `2`, then the exact storage key |
| Compiled data | Exact storage key | Byte `0`, then the complete compiled payload |

The physical record count therefore remains `2 + alias count` per logical
publication, as frozen in HW01.

Record planning rejects an exact duplicate byte key even if a future internal
caller bypasses HW01's logical-name ownership check. It then sorts all records
with the writer-specific byte comparer. Bucket membership never depends on
dictionary enumeration or caller entry order.

## 7. Hashing and bucket placement

HW02 uses the HW00-qualified Hash-v9 function:

```text
hash = 0
for each key byte:
    hash = unchecked(hash * 16777619)
    hash = hash XOR byte
```

The two initial buckets are numbered zero and one and stored on physical pages
one and two. Placement is `hash(key) & 1`. Records retain canonical writer order
within each bucket. An empty bucket is emitted as a valid empty Hash page.

The fixed two-bucket shape is the only HW02 sizing policy. It is sufficient for
the accepted minimal profile and permits both gate cases:

- a **single-bucket store**, where all records occupy one bucket and the other
  Hash page is empty; and
- a **multi-bucket store**, where records are distributed across both pages.

Bucket-count growth and any alternate high-mask/low-mask state remain HW03.

## 8. Exact three-page image profile

Every successful HW02 image is exactly 12,288 bytes:

| Page | Purpose | Page type |
| --- | --- | --- |
| 0 | Hash metadata | `8` |
| 1 | Bucket 0 | `13` |
| 2 | Bucket 1 | `13` |

All multibyte integers are little-endian and every page is 4,096 bytes. Newly
allocated image bytes begin as zero; only defined fields are written.

### 8.1 Metadata page

The metadata page uses these frozen HW00 values:

| Offset | Width | Value |
| --- | ---: | --- |
| 4 | 4 | Not-logged LSN marker `1` |
| 12 | 4 | Hash magic `0x00061561` |
| 16 | 4 | Hash version `9` |
| 20 | 4 | Page size `4096` |
| 25 | 1 | Metadata page type `8` |
| 32 | 4 | Last page number `2` |
| 52 | 20 | Deterministic file ID |
| 72 | 4 | `1` |
| 76 | 4 | `1` |
| 80 | 4 | `0` |
| 84 | 4 | `0` |
| 88 | 4 | Physical Hash record count |
| 92 | 4 | Hash of `%$sniglet^&\0`, `0x5E688DD1` |
| 96 | 4 | `1` |
| 100 | 4 | `1` |

The file ID is the first 20 bytes of SHA-256 over the canonically ordered
records. For every record, the hash input appends the key length as a
little-endian signed 32-bit integer, the key bytes, the value length in the same
form, and the value bytes.

### 8.2 Bucket pages

Each Hash page writes the not-logged LSN marker, its physical page number,
twice the bucket's record count as the item count, the final high free offset,
and page type `13`. The offset table begins at byte 26 and contains one
little-endian `ushort` per item.

Each record contributes key then value. Each item is stored from the end of the
page downward as byte `1` followed by the complete key or value payload. Offset
table order follows record order; item bytes may not overlap the page header or
offset table.

The HW00 big-item threshold is one quarter of a page. HW02 therefore accepts
only key and value payloads of at most 1,024 bytes before the Berkeley DB inline
item marker is added. Larger payloads require an off-page item and are deferred
to HW03.

## 9. Bounds and failure behavior

Construction uses checked arithmetic for record counts, item counts, table
sizes, offsets, lengths, allocation sizes, and numeric field conversions. It
checks the fixed 12,288-byte result against `MaximumDatabaseSize` before
allocating the image.

The internal builder fails with `InvalidOperationException` when otherwise
valid input:

- contains a key or value payload over the 1,024-byte inline threshold;
- cannot fit all records assigned to a bucket page;
- requires an image larger than the configured maximum; or
- exceeds a checked Hash-v9 field or allocation bound.

Null or structurally invalid internal arguments retain normal
`ArgumentNullException`, `ArgumentException`, or `ArgumentOutOfRangeException`
semantics. Exact duplicate byte keys are logical conflicts and use
`InvalidOperationException`. Cancellation is checked during record planning,
hash placement, and page serialization and reports `OperationCanceledException`.

No HW02 failure performs destination I/O or mutates the public caller's entry,
alias, payload, option, or sequence state. Error messages identify the violated
bound without embedding compiled payload bytes.

## 10. Determinism contract

Equivalent logical input produces byte-identical output regardless of:

- publication enumeration order;
- current culture or UI culture;
- dictionary implementation or enumeration order;
- repeated execution;
- target framework; or
- operating system and process architecture.

HW02 has no path, timestamp, random-number, process-ID, machine-ID, environment,
or ambient locale input. The exact compiled payload is part of logical input;
two payloads that differ in compiled identity, description bytes, padding, or
capability bytes are not equivalent even if Runtime renders similar terminal
descriptions.

## 11. Test and evidence design

HW02 follows strict RED to GREEN checkpoints and advances the coordinated
development version to `1.16.0-Alpha-2` with its first compiling RED tests.

Tests must cover:

1. exact compiled storage-key extraction, including description bytes that
   would be lost or changed by string reconstruction;
2. canonical publication, alias publication, and compiled-data marker envelopes;
3. the qualified Hash-v9 function and both bucket placements;
4. writer ordering, including the longer-key-before-exact-prefix case, without
   changing reader comparer behavior;
5. every frozen metadata field and page header;
6. complete byte-for-byte comparison with an independent HW00-derived test
   oracle for representative single-bucket and multi-bucket inputs;
7. input permutation and repeated-run equality;
8. equality under at least two contrasting cultures;
9. managed-reader round trips that recover each canonical name, alias, and
   exact compiled payload;
10. the 1,024/1,025 payload boundary, exact bucket-fit boundary, insufficient
    maximum database size, checked arithmetic, duplicate byte keys, and
    cancellation;
11. unchanged public API snapshots and Runtime-only production dependency; and
12. unchanged HW01 public behavior, including no destination creation and the
    exact construction-boundary `NotSupportedException`.

The independent byte oracle belongs only to tests. Production code does not
copy opaque fixture pages and does not invoke the HW00 writer, `db_load`, native
Berkeley DB, or ncurses. Tests and tooling use only C#, PowerShell 5.1-compatible
PowerShell, cmd/sh, XML/MSBuild, and Markdown changes; HW02 adds no Python, C, or
C++.

Targeted tests run on net8.0, net9.0, and net10.0. The normal pull-request matrix
is the acceptance workflow for HW02. Native overflow qualification remains an
HW03 gate and is not required to accept this inline-only tranche.

## 12. Scope boundaries

HW02 does not implement or claim:

- bucket growth or more than two bucket pages;
- adversarial collision policy or collision-driven growth; ordinary records
  that hash to the same bucket are emitted in canonical order when they fit;
- off-page items, overflow pages, or overflow chains;
- compiled payloads whose ncurses data envelope exceeds the inline threshold;
- public writer image construction or destination publication;
- stream APIs, temporary files, overwrite behavior, atomic commit, or cleanup;
- native runtime or package dependencies;
- native interoperability acceptance beyond the previously accepted HW00
  profile proof; or
- migration, catalog automation, or generic Berkeley DB writing.

HW03 owns bounded growth, adversarial collisions, overflow construction, and
native overflow evidence. HW04 connects the completed internal builder to the
frozen public API. HW05 owns filesystem verification and commit.

## 13. Acceptance boundary

HW02 is accepted when exact-head CI proves that the internal builder creates
byte-identical three-page single-bucket and multi-bucket images from equivalent
logical input; those images match the independent byte oracle and round-trip
every canonical name, alias, and compiled payload through the existing managed
reader; boundary inputs fail deterministically without destination access; and
the public API and Runtime-only dependency boundary remain unchanged.
