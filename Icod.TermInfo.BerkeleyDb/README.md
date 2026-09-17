# Icod.TermInfo.BerkeleyDb

`Icod.TermInfo.BerkeleyDb` is the optional managed package for read-only acquisition from ncurses-compatible Berkeley DB hashed terminfo stores.

## 1.15 release closure status

`1.15.0` is the stable coordinated release. The complete public surface is
frozen at nine exported types, dependency direction remains Runtime-only, and
the deterministic sample plus acquisition, compatibility, security/resource,
ecosystem, and release authorities are complete.
The stable promotion changed no acquisition behavior or public API.

HDB00 selected a dependency-free managed reader for the reviewed Berkeley DB **Hash on-disk format version 9** subset required by ncurses acquisition. Native Berkeley DB remains a CI interoperability oracle and is not a production dependency.

The internal ncurses resolver follows bounded marker-2 index chains over one acquired database image and extracts opaque marker-0 payloads. Empty records, unsupported markers, dangling targets, cycles, and excessive hops fail explicitly. Only an absent initial key is a clean miss.

`BerkeleyDbTerminalDescriptionProvider` reads one caller-selected database path. Lookup tries the exact ordinal UTF-8 key first and, only after a clean miss, one distinct exact Latin-1 key when every requested character is representable. A found malformed or identity-invalid UTF-8 record never falls through. `BerkeleyDbTerminalDescriptionProviderOptions` snapshots parser, database-size, and index-hop limits. `BerkeleyDbDatabaseFormatException` identifies malformed or unsupported containers and ncurses record envelopes while parser and I/O failures retain their existing exception types.


## Hashed terminal catalog

`BerkeleyDbTerminalCatalogReader` reads one explicit database path and returns
a fresh immutable snapshot on every call. Its options snapshot parser,
database-size, record-count, and index-hop limits.

The catalog emits only marker-2 logical publications. It classifies each
publication as `Canonical` or `Alias` by decoding a valid UTF-8 key first and
using Latin-1 only when the raw key is not valid UTF-8. Distinct raw keys that
decode to the same ordinal logical name are rejected. Entries resolving to the same
marker-0 record share one parsed terminal instance within the snapshot.
Marker-0 storage records, including unreferenced orphans, are still parsed and
validated but their internal keys are not emitted.

Raw records are validated in unsigned byte-key order. Successful entries are
ordered by ordinal publication name, then kind, then canonical terminal name.
Physical Hash-page order is never exposed. Reads are uncached, bounded,
cancellable, independent, and release the database handle before parsing.
Production path reads compare two complete observations through one open handle
and reject unequal content or length. This detects mutation between observations
but does not provide an atomic snapshot or arbitrary writer coordination.

Malformed Berkeley DB structures and ncurses envelopes use
`BerkeleyDbDatabaseFormatException`. Malformed compiled entries retain
`CompiledTermInfoFormatException`; identity mismatches, I/O failures, and
cancellation retain their established exception types.

## Package boundary

The package:

- targets `net8.0`, `net9.0`, and `net10.0`;
- depends only on the matching `Icod.TermInfo` version;
- has no native Berkeley DB dependency;
- does not bundle Berkeley DB binaries;
- does not implement writes, transactions, environments, recovery, or general-purpose Berkeley DB APIs; and
- keeps compiled terminfo semantics in `Icod.TermInfo` Runtime.

The reviewed subset supports unencrypted, non-checksummed Hash-v9 files with sorted Hash pages (type 13), inline items, and overflow items. Other access methods, revisions, duplicate/subdatabase features, and legacy type-2 Hash pages are unsupported. Validation covers encountered records and does not guarantee an atomic snapshot during external writes.

Dedicated CI compares the production reader, providers, catalog, and tools with native Berkeley DB stores on Linux and macOS; Windows reads Linux-generated fixtures without Berkeley DB installed. Native big-endian Hash-v9 containers are produced by reloading byte-exact ncurses records with big-endian metadata. Native `tic` also produces exact Latin-1 canonical and alias keys. Lookup is UTF-8-first with the bounded clean-miss-only Latin-1 fallback above, while compiled identity fields retain Runtime's byte-preserving Latin-1 interpretation. Universal non-ASCII encodings, normalization, transliteration, and best-fit mapping are not claimed.

HDB08 qualification requires an isolated package-only consumer on `net8.0`, `net9.0`, and `net10.0` on Windows, Linux, and macOS. A managed verifier enforces the exact package identity, dependency, assembly, symbol, Source Link, and no-native-asset contract.

## Hashed-aware system discovery

`BerkeleyDbSystemTerminalDescriptionProvider` is the opt-in HDB04 system
provider. It snapshots Runtime's existing discovery inputs and policy at
construction while leaving Runtime's frozen
`SystemTerminalDescriptionProvider` unchanged. Discovery preserves encoded
`TERMINFO`, `TERMINFO`, user, `TERMINFO_DIRS`, and platform-default
precedence, including empty default components and equivalent-location
deduplication.

For each logical location, an existing exact directory or exact hashed file wins.
When the exact location is absent, a regular `.db` companion is considered,
matching ncurses discovery behavior. Missing sources continue; malformed reached
sources fail explicitly. Successful results are cached, while misses and
failures remain retryable.

HDB07C qualification passed 422 BerkeleyDb unit cases and 50 native-store
interoperability cases per target framework on Windows, Linux, and macOS. In
addition to HDB07's native 64-entry ASCII matrix, Linux and macOS independently
verified a big-endian Hash-v9 container with byte-exact ncurses records and an
exact Latin-1 canonical/alias fixture; Windows consumed the Linux fixtures
without Berkeley DB installed. Real permission denial and restoration also
passed on all three hosts. The
optional package's public API remains unchanged: command executables classify
explicit paths and then call the accepted provider or catalog reader. Explicit
`infocmp -A/-B` files and explicit human `toe` file operands are supported;
Inspection stays provider-neutral, ambient `toe` discovery and JSON stay
conventional, and `tic` remains directory-write-only. Installed-tool smoke
passed on all three hosts and direct-command smoke passed for all six archive
RIDs. The accepted Alpha-8 feature/API source and its HDB09 evidence-record head
passed the normal 12-job and genuine HDB00 3-job workflows, including the
managed exact-package verifier and three-host net8/net9/net10 package
consumption. Atomic snapshots, arbitrary writer coordination, and
non-UTF-8/non-Latin-1 producer encodings remain outside the qualified contract.
Stable `1.15.0` preserves that exact behavior and surface.
