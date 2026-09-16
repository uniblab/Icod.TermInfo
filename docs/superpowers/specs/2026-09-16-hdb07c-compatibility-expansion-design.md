# HDB07C Compatibility Expansion Design

**Status:** COMPLETE / ACCEPTED
**Target prerelease:** `1.15.0-Alpha-7`  
**Accepted HDB07 implementation head:**
`ab059acf27a5bc7fb1e8cef390d47085be33b5af`  
**Accepted HDB07 documentation head:**
`3b113450f1c17b349bd9f470da5f48e4d9c9387a`

**Accepted HDB07C implementation/qualification head:**
`2c122abd7e4e63397b474f248d51273a1b7fc006`

**Accepted qualification runs:** normal `35161853347` (12/12), HDB00
`35161853443` (3/3)

## 1. Purpose

HDB07C is the separately reviewed compatibility-expansion tranche approved
after HDB07. It strengthens the read-only Berkeley DB Hash-v9 acquisition
contract in exactly three areas:

1. detected concurrent file mutation;
2. native Berkeley DB big-endian container evidence; and
3. a bounded non-ASCII ncurses producer subset.

HDB07C completes before HDB08 packaging and release qualification. It does not
reopen HDB07 or broaden the supported Berkeley DB family.

## 2. Scope decision

The governing approach is **bounded compatibility expansion with exact failure**.

HDB07C does not infer an encoding from the current culture, normalize terminal
names, retry a changing file, or claim an atomic filesystem snapshot. It adds
only behavior that can be specified as exact bytes and proved by deterministic
tests plus native producer evidence.

The three workstreams are independent RED-to-GREEN checkpoints. A production
change in one workstream is not permitted to absorb unrelated corrections from
another.

## 3. Approved compatibility claims

HDB07C may claim all of the following after qualification:

- production path acquisition accepts a database image only after two
  consecutive complete observations of the same open file object are
  byte-identical;
- an observed length or content change causes a stable I/O failure without a
  hidden retry;
- the managed reader accepts a natively created big-endian Berkeley DB Hash-v9
  container whose ncurses key/value records were copied byte-for-byte from a
  native `tic`-produced hashed store;
- exact provider lookup and catalog enumeration accept the qualified
  single-byte Latin-1 ncurses name subset defined in this specification; and
- those behaviors remain pure managed and read-only in production.

HDB07C does not claim an atomic snapshot, arbitrary concurrent-writer safety, a
native big-endian host, universal text-encoding detection, or general Unicode
normalization.

## 4. Architecture and dependency invariants

The accepted dependency graph remains unchanged:

```text
Icod.TermInfo.BerkeleyDb --> Icod.TermInfo

Icod.TermInfo.Inspection --> Icod.TermInfo + Icod.TermInfo.Source

infocmp --> Inspection + BerkeleyDb
toe     --> Inspection + BerkeleyDb
```

Runtime and Inspection do not reference BerkeleyDb. BerkeleyDb continues to
reference Runtime only. No production assembly acquires a Berkeley DB native
dependency.

The semantic boundary also remains unchanged:

```text
Hash-v9 container
       |
       v
ncurses marker records
       |
       v
opaque compiled-entry bytes
       |
       v
existing CompiledTermInfoParser
       |
       v
TerminalDescription
```

HDB07C does not add a second compiled-entry parser or rewrite a parsed
`TerminalDescription` to conceal an encoding mismatch.

## 5. Versioning and public surface

HDB07C retains the coordinated version `1.15.0-Alpha-7`. HDB08 remains the
planned Alpha-8 packaging and cross-platform qualification tranche.

HDB07C adds no public type, member, option, exception type, command switch,
diagnostic identifier, JSON field, schema revision, or package dependency.
Reusable assembly identities remain `1.0.0.0`, and public APIs remain
equivalent across net8/net9/net10.

Existing XML and README descriptions may be corrected to state the expanded
behavior. Such documentation changes do not alter the public metadata shape.

## 6. Observed-stability acquisition

### 6.1 Production path contract

`BerkeleyDbHashReader.ReadDatabase(string, int)`, which supplies the production
provider and catalog acquisition path, opens one file handle and performs two
bounded complete observations through that same handle.

The first observation preserves the accepted acquisition behavior:

1. read and validate the reported length against the configured maximum and
   `Array.MaxLength`;
2. reject a file smaller than one metadata page;
3. allocate exactly one database-sized result array;
4. combine partial reads until that array is full; and
5. reject trailing growth beyond the observed length.

The reader then rewinds the same seekable file handle and verifies a second
complete observation against the acquired array. Verification uses a bounded
scratch buffer rather than allocating a second database-sized array.

The acquired image is returned only when:

- the second reported length equals the first;
- every second-pass byte equals the acquired image;
- the second pass reaches the expected end exactly; and
- no trailing byte is present.

The exact stable mutation failure is an `IOException` whose message is:

```text
The Berkeley DB file changed while it was being read.
```

There is no retry. A found format error is not reclassified as a mutation, and
an unrelated I/O exception retains its original type and detail.

### 6.2 Borrowed stream contract

The existing internal `ReadDatabase(Stream, int)` helper continues to borrow a
readable, length-reporting stream positioned at byte zero. It remains
single-pass and continues to support non-seekable deterministic test streams.

Observed-stability verification belongs to the production path wrapper. HDB07C
must not silently add seekability, rewind, ownership, or double-read
requirements to the borrowed-stream contract.

### 6.3 Filesystem semantics

The existing open mode and sharing policy remain unchanged. On platforms where
that policy prevents a concurrent writer, the operating system supplies the
stronger exclusion. On platforms where writes can occur through another
handle, unequal observations are detected and rejected.

Using the same open handle is deliberate:

- same-path replacement does not splice two different file objects into one
  acquisition;
- an already-open object remains the object being verified; and
- a later provider/catalog call can observe a replacement according to the
  existing refresh and caching contracts.

HDB07C does not claim detection when an external actor changes bytes and
restores the identical image between observations, nor does it prevent a
change after verification has completed. The returned byte array itself is an
owned immutable-by-convention acquisition image.

### 6.4 Public failure mapping

The mutation `IOException` remains an I/O failure at public provider, catalog,
system-provider, Inspection, and command boundaries. It is not wrapped as
`BerkeleyDbDatabaseFormatException`.

Existing command mappings therefore remain authoritative:

- `infocmp` reports `INFOCMP0003` and status 1 with no partial stdout; and
- `toe` reports `TOE0005` and status 1 for the affected explicit hashed root,
  emits no entries from that root, and preserves existing later-root behavior.

## 7. Native big-endian container qualification

### 7.1 Producer definition

The HDB07C native fixture is produced in three stages:

1. the pinned ncurses `tic` build creates a normal hashed terminfo database;
2. installed Berkeley DB 5.3 `db_dump -k` exports its exact application
   records and `db_load -c db_lorder=4321` reloads those records into a newly
   created big-endian Hash database; and
3. a package-free C# verifier checks the metadata magic and exact record-dump
   parity independently of the production reader.

The Berkeley DB utilities and verifier are development and CI evidence only.
They are never loaded by or distributed with a production package.

### 7.2 Required evidence

Linux and macOS independently prove that:

- the source store was produced by the pinned hashed `tic`;
- the destination metadata is big-endian rather than merely relabeled;
- native Berkeley DB can reopen and dump the destination;
- source and destination dumps contain the same exact application key/value
  records;
- the managed reader enumerates the same records;
- canonical and alias provider lookup returns the expected compiled payload;
- the catalog classifies the expected canonical and alias publications; and
- the existing direct and routed command surfaces consume the store.

Windows downloads the Linux-generated big-endian fixture and exercises the
same managed package/provider/catalog/command paths without Berkeley DB
installed.

### 7.3 Claim boundary

The accepted wording is:

> a native big-endian Berkeley DB Hash-v9 container containing byte-exact
> ncurses-produced records

The evidence is not described as ncurses running on a big-endian CPU. Berkeley
DB changes the integer byte order of its metadata and access-method structures;
the ncurses application key/value bytes remain application-owned and are copied
unchanged.

No production change is expected for this checkpoint unless native evidence
exposes a minimized defect in the already accepted endian-aware reader.

## 8. Qualified non-ASCII producer subset

### 8.1 Encoding policy

Provider lookup remains exact and deterministic:

1. validate the requested terminal name under the existing safety rules;
2. encode and try the strict UTF-8 key first;
3. if and only if that initial key is a clean miss, determine whether every
   requested character is exactly representable in Latin-1;
4. if the Latin-1 bytes differ from the UTF-8 bytes, try that exact key once;
   and
5. return a clean miss only when both permitted candidates are absent.

A found UTF-8 record that is malformed, unsupported, or fails compiled identity
validation is a failure. The reader must not fall through to Latin-1 and mask
that failure.

No current-culture encoding, platform code page, replacement fallback,
case-folding, normalization, transliteration, or best-fit mapping is permitted.
Malformed UTF-16 input, including an unpaired surrogate, retains the accepted
failure behavior.

### 8.2 Catalog decoding

Catalog publication keys continue to use strict UTF-8 when the raw bytes form a
valid UTF-8 sequence. Only a key rejected by strict UTF-8 may be decoded
one-to-one as Latin-1.

After decoding, the existing terminal-name safety validation and exact
canonical/alias identity rules apply. The parsed compiled identity remains
owned by `CompiledTermInfoParser`, whose byte-preserving Latin-1 contract is not
changed.

If distinct raw publication keys decode to the same logical .NET name under the
UTF-8-first/Latin-1-fallback policy, catalog acquisition fails deterministically
as an ambiguous database rather than emitting duplicate logical publications.

### 8.3 Qualified native fixture

The native fixture contains at least:

- one canonical terminal name with a non-ASCII Latin-1 byte that is invalid as
  strict UTF-8;
- one non-ASCII alias governed by the same rule;
- a marker-0 compiled entry whose names section contains those exact bytes; and
- marker-2 keys and targets whose bytes agree exactly with the compiled
  identity.

The fixture source is written as explicit bytes so its encoding does not depend
on the shell, locale, editor, checkout conversion, or workflow YAML parser.
Native dumps and managed assertions record the exact hexadecimal key bytes.

Linux and macOS independently run `tic` and prove native canonical/alias
lookup. Windows consumes the Linux artifact and proves Unicode caller strings
resolve through the managed Latin-1 fallback without an installed native
library.

### 8.4 Claim boundary

HDB07C qualifies the exact single-byte Latin-1 producer subset above. It does
not claim:

- every Latin-1 name whose bytes also form a different valid UTF-8 string;
- UTF-8-encoded compiled names being rewritten into Unicode identities;
- locale-specific multibyte encodings;
- Windows code pages;
- Unicode normalization equivalence; or
- arbitrary non-ASCII terminal-name portability across other terminfo
  implementations.

Strict UTF-8 synthetic-key coverage from HDB07 remains permanent, but it is not
relabelled as native UTF-8 compiled-identity interoperability.

## 9. Deterministic failure precedence

HDB07C preserves the following order:

1. terminal-name argument and safety validation;
2. acquisition I/O and observed-stability verification;
3. Berkeley DB structural validation;
4. exact initial UTF-8 lookup;
5. optional exact Latin-1 lookup after an initial clean miss only;
6. ncurses marker resolution;
7. compiled-entry parsing; and
8. exact canonical/alias identity validation.

Catalog acquisition remains all-or-nothing. Structural order remains raw
unsigned byte-key order, while successful publications remain sorted by logical
ordinal name, kind, and canonical terminal name.

An encoding fallback never suppresses corruption, parser failure, identity
failure, cancellation, permission denial, sharing denial, or mutation.

## 10. Test-driven checkpoints

### Checkpoint A — observed-stability acquisition

Add deterministic test-only streams or file seams that present two different
same-length observations. First commit and observe the failure under current
single-observation production path acquisition. Then add the narrow two-pass
verification and prove:

- identical partial-read observations succeed;
- same-length content mutation fails with the exact `IOException`;
- length growth and truncation during verification fail;
- unrelated second-pass I/O failures propagate;
- no retry occurs;
- borrowed non-seekable stream behavior remains unchanged; and
- handles are released after every result.

### Checkpoint B — native big-endian evidence

Add the native repacker and workflow assertions as a test-only RED. Observe the
missing fixture/evidence failure before adding generation. If the resulting
valid database exposes a production defect, reduce it to an independent managed
regression before correcting production.

### Checkpoint C — native Latin-1 producer evidence

Add a byte-authored source fixture and native producer assertions. First prove
that the current UTF-8-only provider/catalog behavior does not satisfy the
approved native fixture. Then add the narrow fallback and ambiguity handling.

Provider, catalog, explicit system-provider, Inspection, direct command, routed
command, package-only, and cross-host cases are included where those surfaces
already consume hashed databases.

### Checkpoint D — qualification and closure

Run the complete normal PR workflow and a full three-job HDB00 qualification on
the exact final implementation head. Record exact heads, workflow IDs, job
counts, per-host/TFM counts, native byte-order evidence, non-ASCII key bytes,
and concurrency outcomes.

## 11. Fixture and tool ownership

HDB07C extends `tools/hdb00` for native generation and verification rather than
adding a second interoperability harness.

The installed Berkeley DB utilities are narrowly limited to dumping exact
source records and reloading them with the selected destination byte order.
The package-free C# verifier checks magic and dump parity; it does not become a
reusable product library.

Non-ASCII source generation uses an existing repository scripting language or
a focused helper capable of explicit byte output. Locale-sensitive shell text
literals are prohibited.

Generated database binaries remain workflow artifacts with short retention;
they are not committed to the repository. Permanent managed regressions use
deterministic in-test builders when native provenance is not required.

## 12. CI cadence

The normal twelve-job PR workflow runs for every pushed HDB07C checkpoint.

The existing HDB00 synchronize-delta gate remains authoritative. Expensive
native work runs when a checkpoint changes BerkeleyDb production, native tools,
interop tests, relevant shared fixtures, workflow logic, or package composition.
Unit-test-only, specification, plan, README, and closure-document changes use
the inexpensive classifier jobs unless they also touch a sensitive path.

HDB00 runs all three jobs once more on the exact final implementation head.
Linux and macOS generate independently; Windows consumes the Linux artifacts.

## 13. Documentation and compatibility language

The roadmap, package README, root README, HDB07 closure, and PR body are updated
only after behavior is qualified. They must distinguish:

- detected two-observation stability from an atomic snapshot;
- native big-endian container production from execution on a big-endian host;
- the qualified Latin-1 subset from universal non-ASCII support; and
- CI-only native tooling from the pure-managed production package.

No document may shorten these distinctions into a broader compatibility claim.

## 14. Acceptance criteria

HDB07C is accepted only when:

1. each behavioral correction has an independently committed and observed RED;
2. production path acquisition requires two byte-identical complete
   observations without retry or a second database-sized allocation;
3. the borrowed stream contract remains single-pass and non-owning;
4. mutation failures remain ordinary I/O failures at every public boundary;
5. Linux and macOS independently create and natively read the big-endian
   container;
6. native and managed record dumps are byte-equivalent;
7. Windows reads the Linux big-endian artifact without Berkeley DB installed;
8. Linux and macOS independently produce and natively read the exact Latin-1
   canonical and alias fixture;
9. provider and catalog behavior follows the exact UTF-8-first fallback and
   ambiguity rules;
10. Runtime parsing and returned `TerminalDescription` semantics are unchanged;
11. all reusable public APIs and dependency directions remain unchanged;
12. production contains no native asset, P/Invoke, runtime download, writer,
    transaction, environment, or recovery path;
13. BerkeleyDb tests pass on net8/net9/net10 on Windows, Linux, and macOS;
14. relevant command, package-only, installed-tool, and archive verification
    remains green;
15. the normal PR workflow passes all 12 jobs on the final implementation
    head;
16. HDB00 passes all 3 jobs on that exact head; and
17. PR #45 remains open, draft, and unmerged.

## 15. Explicit non-goals

HDB07C does not add:

- a general-purpose stable-snapshot or file-locking API;
- retries, temporary copies, filesystem transactions, or writer coordination;
- new Berkeley DB access methods, revisions, checksums, encryption,
  subdatabases, duplicate semantics, environments, transactions, recovery, or
  writes;
- a native production backend;
- a native big-endian runner claim;
- automatic encoding detection beyond the exact two-candidate policy;
- UTF-8 reinterpretation of Runtime's byte-preserving compiled names;
- Unicode normalization, case folding, transliteration, or locale code pages;
- hashed publication from `tic`;
- public raw-record access;
- new command switches or JSON contracts; or
- unrelated refactoring.

## 16. Next tranche

After HDB07C acceptance, HDB08 performs packaging, cross-platform
qualification, final API/dependency verification, documentation freeze, and
release-candidate closure for `1.15.0`.

Any broader encoding family, actual big-endian-host evidence, or coordinated
writer/snapshot protocol requires a future separately reviewed release.
