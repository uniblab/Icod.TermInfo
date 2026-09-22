# HW03 Hash-v9 Collision, Overflow, and Bounded Growth Design

**Date:** 2026-09-22

**Release:** Icod.TermInfo 1.16.0

**Tranche:** HW03 / Alpha-3

**Status:** Conversational design approved; written specification ready for review

**Accepted baseline:** HW02 documentation head
`f3882e74d43e648bf702d42c58d617f508daf928`

## 1. Purpose

HW03 extends the accepted deterministic Hash-v9 image builder from fixed,
inline, two-bucket images to the complete internal storage shapes needed by the
1.16.0 writer:

- bounded primary-bucket growth;
- deterministic Hash-page continuation chains;
- type-3 off-page key and value items;
- type-7 overflow-page chains; and
- exact preflight of page count, file size, arithmetic, and construction work.

The result remains a pure-managed, in-memory `byte[]`. HW03 does not connect
`BerkeleyDbTerminalDatabaseWriter.Write` to image construction and does not
write a destination file. Public writer integration remains HW04, safe
filesystem publication remains HW05, and migration and catalog automation
remain deferred to 1.17.0.

HW03 must preserve every accepted HW02 byte whenever the records still fit the
two-bucket inline profile. It also preserves the accepted HW00 two-bucket
overflow profile when large payloads require overflow storage but no Hash-page
continuation or bucket growth is necessary.

## 2. Governing decisions

HW03 uses a canonical whole-image plan rather than replaying Berkeley DB's
incremental insertion and split history.

The governing decisions are:

1. Primary-bucket counts are powers of two beginning at two.
2. The builder chooses the smallest feasible bucket count that eliminates
   collision pressure which a larger native mask can separate.
3. Distinct keys with the same exact 32-bit hash are never expected to separate
   through bucket growth. They use deterministic type-13 continuation pages.
4. If the database-size limit prevents another useful growth step, remaining
   collision pressure also uses continuation pages rather than causing
   unbounded growth or rejecting an image that otherwise fits.
5. Physical pages are allocated in three groups: all primary buckets, all
   continuation Hash pages, then all payload overflow pages.
6. The complete layout is validated before the output array is allocated.
7. No public bucket, hash, page, load-factor, or overflow control is added.

The briefly considered prime-bucket policy is not part of this design. Native
Hash-v9 lookup uses `high_mask` and `low_mask`, not prime-modulus addressing;
HW03 retains the canonical power-of-two mask profile.

## 3. Alternatives considered

### 3.1 Selected: canonical upfront sizing with bounded continuation chains

The builder evaluates complete power-of-two layouts from two buckets upward.
It grows while additional mask bits can remove reducible collision pressure,
then uses continuation pages for exact-hash collisions or for pressure that
cannot be separated within the database-size limit.

This approach is independent of caller enumeration order and mutation history,
keeps primary bucket pages contiguous, produces simple metadata, and permits
complete preflight before allocation.

### 3.2 Rejected: simulate Berkeley DB incremental splits

Replaying insertions and linear-hash splits would make page numbering and
`spares` state depend on insertion history. Equivalent final record sets could
produce different images, and reproducing allocator history would add behavior
that ncurses interoperability does not require.

### 3.3 Rejected: retain two buckets and chain every overfull bucket

This would be deterministic but would create unnecessarily long chains for
ordinary separable hashes. It would not satisfy HW03's bounded-growth goal and
would make native lookup costs grow with catalog size despite available hash
bits.

### 3.4 Rejected: reject any bucket that cannot fit one page

Exact 32-bit collisions cannot be separated by any number of buckets. Rejecting
them would exclude valid distinct Berkeley DB keys and leave the writer without
a complete collision policy.

## 4. Internal architecture

`BerkeleyDbHashV9ImageBuilder` remains the internal entry point and retains its
accepted inputs: canonically ordered `BerkeleyDbHashRecord` values,
`MaximumDatabaseSize`, and a cancellation token.

Construction is divided into four internal responsibilities:

1. **Item planning** caches each record's hash and chooses inline or off-page
   representation independently for its key and value.
2. **Layout evaluation** partitions records into candidate primary buckets,
   greedily packs whole record pairs into Hash pages, and calculates the exact
   total page count.
3. **Page assignment** gives every primary, continuation, and overflow page its
   final physical page number and link fields.
4. **Emission** allocates one zero-filled image and writes the already validated
   plan without making new sizing decisions.

The implementation may introduce focused internal immutable plan types. They
must describe data rather than perform I/O, must not escape the BerkeleyDb
assembly, and must not create public API. The accepted
`BerkeleyDbNcursesRecordPlanner`, `BerkeleyDbHashRecord`, and writer-specific
key comparer remain authoritative for record derivation, duplicate detection,
and canonical record ordering.

The existing reader is not refactored into a read/write codec. It continues to
validate and read the broader accepted 1.15 input family while the writer emits
one canonical little-endian profile.

## 5. Item representation

The page size remains exactly 4,096 bytes. The Berkeley DB big-item threshold
remains one quarter page, exactly 1,024 payload bytes.

For each record key and value:

| Payload length | Hash-page representation | Encoded Hash item length |
| ---: | --- | ---: |
| `0` through `1024` | Type `1`, then the complete payload | payload length + `1` |
| `1025` or greater | Type `3` off-page descriptor | exactly `12` |

The type-3 descriptor contains:

| Relative offset | Width | Value |
| --- | ---: | --- |
| `0` | 1 | Item type `3` |
| `1` | 3 | Zero |
| `4` | 4 | First overflow page number |
| `8` | 4 | Complete payload length |

All multibyte fields are little-endian. A payload selected for off-page storage
has positive length and at least one overflow page, so its first-page field is
never zero.

A record's key and value remain an atomic pair on one Hash page. Payload bytes
may span overflow pages, but a key descriptor and its corresponding value item
are never split between separate Hash pages.

## 6. Hash-page capacity and packing

Every Hash page has a 26-byte page header and a two-byte offset-table entry for
each item. Each record therefore consumes:

```text
4 bytes of offset table
+ encoded key item length
+ encoded value item length
```

Records retain the canonical writer order already produced by the ncurses
record planner. Within each primary bucket, the builder greedily appends whole
records to the current Hash page. If the next pair would cross the offset table
or item area, it begins the next continuation page and retries the complete
pair there.

An empty primary bucket is a valid empty type-13 page. Empty continuation pages
are never allocated. Given the frozen inline threshold and 12-byte off-page
descriptor, every individual record pair fits an otherwise empty Hash page.

Greedy packing is deterministic because record order, item representation,
page size, and capacity calculation are all fixed. The emitter does not
re-evaluate fit.

## 7. Bucket sizing algorithm

For a candidate bucket count `N`, where `N` is a power of two and `N >= 2`, a
record is assigned to:

```text
bucket = hash(key) & (N - 1)
```

The qualified Hash-v9 hash remains:

```text
hash = 0
for each key byte:
    hash = unchecked(hash * 16777619)
    hash = hash XOR byte
```

The builder evaluates candidates `2, 4, 8, ...` in ascending order. For each
candidate it performs exact greedy packing and records:

- primary bucket count;
- continuation Hash-page count;
- overflow-page count, which is independent of bucket count;
- exact total page count and image length; and
- whether remaining continuation pressure is irreducible.

A candidate has eliminated reducible collision pressure when every bucket
either fits its primary page or all records in that overfull bucket have the
same exact 32-bit hash. A bucket that is overfull and contains more than one
exact hash value remains reducible because another mask bit may separate its
records.

The selected candidate is:

1. the first feasible candidate with no reducible collision pressure; otherwise
2. the evaluated feasible candidate with the greatest bucket count when the
   next primary-bucket array cannot fit within the page budget or when the first
   natural candidate is infeasible because its irreducible pages exceed the
   complete-image budget.

Feasibility includes metadata, every primary page, every required continuation
page, and every required payload overflow page. A candidate whose complete
image exceeds `MaximumDatabaseSize` is not feasible. Evaluation may continue
past an infeasible smaller candidate because a larger bucket array can replace
enough continuation pages to reduce or preserve the total page count.

Growth stops when a natural candidate is selected, when the next candidate's
metadata plus primary pages plus unavoidable payload overflow pages already
exceed the page budget, when the first natural candidate is itself infeasible,
or when checked numeric limits prevent another doubling. Once a natural
candidate is reached, further doubling cannot remove its exact-hash continuation
pages and can only add primary pages. If no evaluated candidate is feasible,
construction fails before image allocation.

This search is bounded. The positive signed `MaximumDatabaseSize` and fixed
4,096-byte page size permit fewer than 20 candidate counts under the default
64-MiB limit and fewer than 30 for any supported signed 32-bit limit. Each
candidate visits each record a bounded number of times.

## 8. Physical page allocation

After selecting a layout, physical page numbers are assigned exactly in this
order:

1. page `0`: Hash-v9 metadata;
2. pages `1` through `N`: primary buckets `0` through `N - 1`;
3. continuation Hash pages, ordered by primary bucket number and then chain
   position; and
4. payload overflow pages, ordered by primary bucket number, Hash-chain page,
   record order, key before value, and overflow-chain position.

This rule preserves the accepted HW02 image because an inline two-bucket store
has no pages after page 2. It preserves the accepted HW00 two-bucket overflow
profile because, in the absence of continuation Hash pages, payload allocation
begins at page 3 and follows bucket, record, key, and value order.

Primary page `previous_pgno` is zero. Its `next_pgno` is zero when it has no
continuation or the first continuation page number otherwise. Each continuation
page names its preceding and following Hash page, using zero at the end of the
chain. Continuation pages have page type `13` and the same item layout as a
primary Hash page.

Hash-chain links are independent from type-7 payload overflow links. A type-3
descriptor points only to the first page of its own payload chain.

## 9. Overflow-page construction

A type-7 overflow page has 4,070 payload bytes after its 26-byte page header.
The required page count is calculated without overflow-prone addition as:

```text
1 + ((payloadLength - 1) / 4070)
```

Each overflow page writes:

| Offset | Width | Value |
| --- | ---: | --- |
| `4` | 4 | Not-logged LSN marker `1` |
| `8` | 4 | Its physical page number |
| `12` | 4 | Previous overflow page or zero |
| `16` | 4 | Next overflow page or zero |
| `20` | 2 | Item count `1` |
| `22` | 2 | Payload bytes stored on this page |
| `25` | 1 | Page type `7` |
| `26` | variable | The next payload chunk |

All nonfinal pages contain exactly 4,070 payload bytes. The final page contains
the remaining positive byte count. Unused bytes remain zero. Separate keys and
values never share an overflow page or overflow chain.

## 10. Hash-page emission

Primary and continuation Hash pages write the accepted fields:

| Offset | Width | Value |
| --- | ---: | --- |
| `4` | 4 | Not-logged LSN marker `1` |
| `8` | 4 | Physical page number |
| `12` | 4 | Previous Hash page or zero |
| `16` | 4 | Next Hash page or zero |
| `20` | 2 | Twice the record count on this page |
| `22` | 2 | Final high-free offset |
| `25` | 1 | Page type `13` |

The offset table begins at byte 26. Items are emitted key then value and packed
from the end of the page downward in planned record order. Inline item bytes
and type-3 descriptor bytes are written exactly as specified in section 5.

For an unchained primary page, offsets 12 and 16 remain zero. Consequently,
HW02-compatible pages remain byte-identical.

## 11. Metadata for grown images

Metadata remains little-endian and retains the accepted HW02 magic, version,
page size, deterministic file ID, character key, flags, fill factor, and record
count rules.

Layout-dependent fields become:

| Offset | Field | Value |
| --- | --- | --- |
| `32` | Last page number | Total page count minus one |
| `72` | `max_bucket` | `N - 1` |
| `76` | `high_mask` | `N - 1` |
| `80` | `low_mask` | `(N / 2) - 1` |
| `84` | `ffactor` | `0` |
| `88` | `nelem` | Physical Hash record count |
| `96 + 4k` | `spares[k]` | `1` for `0 <= k <= log2(N)` |

Unused `spares` entries remain zero. With the active entries set to one, native
Hash-v9 bucket-to-page translation maps bucket `b` to contiguous primary page
`b + 1`. Continuation and payload overflow pages do not change primary bucket
translation.

The deterministic file ID remains the first 20 bytes of SHA-256 over the full
canonically ordered logical record stream. It does not depend on selected
bucket count or physical page assignment. Thus equivalent records produce the
same file ID even if a caller selects a different valid database-size ceiling.

## 12. Bounds, cancellation, and failure behavior

Before allocating the image, planning validates with checked arithmetic:

- record and item counts;
- item and offset-table lengths;
- per-page free offsets;
- overflow page counts and accumulated payload lengths;
- primary and continuation page counts;
- physical page numbers and link fields;
- total page count and `last_pgno`;
- total image length;
- every narrowing conversion to `ushort`, `uint`, or `int`; and
- the output allocation against both `MaximumDatabaseSize` and the runtime
  maximum array length.

`MaximumDatabaseSize` is interpreted as an inclusive byte ceiling. The emitted
image is always a whole number of 4,096-byte pages and must be less than or
equal to that ceiling.

The builder checks cancellation before planning, while preparing records,
between candidate layouts, while placing records, while assigning pages, and
while emitting Hash and overflow pages. Cancellation reports
`OperationCanceledException` and no partial image escapes.

Otherwise valid input that cannot fit the configured database limit or a
Hash-v9 numeric field fails with `InvalidOperationException` before allocation.
Null and invalid internal arguments retain normal `ArgumentNullException`,
`ArgumentException`, and `ArgumentOutOfRangeException` semantics. Error text
identifies the violated limit or structure without embedding key or compiled
payload bytes.

HW03 performs no destination I/O. Failure cannot create, truncate, replace, or
delete a caller path.

## 13. Determinism contract

Equivalent logical records produce byte-identical output regardless of:

- publication enumeration order;
- current culture or UI culture;
- dictionary implementation or enumeration order;
- repeated execution;
- target framework;
- operating system or process architecture; or
- a `MaximumDatabaseSize` value larger than the selected image requires.

The last rule follows from choosing the first natural layout. A larger ceiling
does not cause gratuitous growth. When the ceiling is too small for that layout
but large enough for a chained fallback, the ceiling is part of the effective
construction input and may intentionally produce a different canonical layout.

No timestamp, path, random number, process identifier, host identifier,
environment value, native library, or ambient locale enters planning or
emission.

## 14. Test and evidence design

HW03 uses strict RED to GREEN checkpoints. The first compiling RED advances the
coordinated development version to exactly `1.16.0-Alpha-3`; reusable assembly
versions remain `1.0.0.0`.

Permanent managed tests cover:

1. complete byte equality with every accepted HW02 two-bucket inline image;
2. complete byte equality with the independent HW00 two-bucket overflow image;
3. the 1,024/1,025 inline-to-off-page boundary;
4. one-page and multi-page payload chains, including 4,070/4,071-byte chunk
   boundaries, exact descriptor lengths, page identities, previous/next links,
   final chunk lengths, and zero unused bytes;
5. off-page keys, off-page values, and records with both items off page;
6. separable low-bit collisions that force deterministic bucket growth;
7. checked-in distinct binary keys with the same exact qualified 32-bit hash,
   proving continuation-page packing and lookup without runtime collision
   searching;
8. multiple continuation pages, empty primary buckets, and exact chain links;
9. the capped-growth fallback where another bucket doubling exceeds the byte
   ceiling but the continuation layout fits;
10. exact-limit acceptance and one-byte-below rejection for complete images;
11. checked page-count, image-size, field-narrowing, and work bounds;
12. pre-cancellation and cancellation during planning and emission;
13. managed-reader exact lookup, complete enumeration, catalog parsing, and
    canonical/alias/compiled-payload recovery;
14. repeated-run, reversed-input, and contrasting-culture byte equality;
15. unchanged public API snapshots, cross-target API equivalence, Runtime-only
    dependency, IL-only package contents, and absence of native assets; and
16. unchanged HW01 public writer behavior and exact temporary
    `NotSupportedException` boundary.

The independent HW00 writer remains a test oracle and research tool only.
Production does not reference or invoke it.

## 15. Native interoperability qualification

The HDB00 interoperability workflow qualifies production-built HW03 images on
Linux, macOS, and Windows using the already established native Berkeley DB
tooling.

For the frozen two-bucket overflow fixture, production bytes must equal the
independent HW00 writer bytes completely.

For grown and continuation-chain images, equality with a native incrementally
mutated database is neither expected nor required because allocator and split
history affect native physical bytes. Instead, each production image must:

- pass native `db_verify`;
- produce the expected complete key/value set through native `db_dump -k`;
- return exact canonical, alias, exact-collision, and large compiled payloads
  through the native lookup probe; and
- produce the same logical records through the managed reader.

These checks are executable test evidence, not a production dependency. The
NuGet package remains pure managed, IL-only, and free of native Berkeley DB
assets or bindings. HW03 adds no Python, C, or C++ source; implementation and
test changes use C# plus the existing PowerShell 5.1-compatible and cmd/sh
automation. The already accepted native probe and installed Berkeley DB tools
may be reused unchanged as external interoperability oracles.

## 16. Scope boundaries

HW03 does not implement or claim:

- public writer image emission;
- destination streams, temporary files, overwrite behavior, atomic commit, or
  cleanup;
- caller-selected page size, byte order, bucket count, load factor, hash
  function, or overflow threshold;
- replay of native insertion, allocator, or split history;
- generic Berkeley DB mutation or support for non-Hash access methods;
- native production dependencies, P/Invoke, or bundled database tools;
- migration, catalog automation, update-in-place behavior, or incremental
  writes; or
- changes to the accepted reader API or supported input family.

HW04 connects the completed internal builder to the frozen public writer API.
HW05 owns verified filesystem publication. Later 1.16.0 tranches own `tic`
integration, package samples, qualification, and release closure.

## 17. Acceptance boundary

HW03 is accepted when exact-head CI proves that:

- every accepted HW02 inline image remains byte-identical;
- the frozen HW00 two-bucket overflow image is reproduced byte-for-byte;
- separable collisions grow to the deterministic canonical primary-bucket
  count;
- exact-hash and size-capped collisions use correct deterministic type-13
  continuation chains;
- large keys and values use correct bounded type-3/type-7 storage;
- managed and native readers recover every exact record and payload;
- native verification accepts the grown and chained images on all three hosts;
- boundary failures occur before allocation or destination access;
- the public writer remains deliberately disconnected; and
- public API, dependency, package, assembly-version, and target-framework
  contracts remain unchanged.
