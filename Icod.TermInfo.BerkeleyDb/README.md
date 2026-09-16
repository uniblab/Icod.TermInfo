# Icod.TermInfo.BerkeleyDb

`Icod.TermInfo.BerkeleyDb` is the optional managed package for read-only acquisition from ncurses-compatible Berkeley DB hashed terminfo stores.

## 1.15 development status

`1.15.0-Alpha-4` completes the accepted explicit and opt-in system terminal-description providers on the accepted managed Hash-v9 reader. It supports bounded exact-key lookup with inline and off-page records, overflow reconstruction, both byte orders, ncurses marker resolution, compiled-entry parsing, exact identity validation, and successful-result caching.

HDB00 selected a dependency-free managed reader for the reviewed Berkeley DB **Hash on-disk format version 9** subset required by ncurses acquisition. Native Berkeley DB remains a CI interoperability oracle and is not a production dependency.

The internal ncurses resolver follows bounded marker-2 index chains over one acquired database image and extracts opaque marker-0 payloads. Empty records, unsupported markers, dangling targets, cycles, and excessive hops fail explicitly. Only an absent initial key is a clean miss.

`BerkeleyDbTerminalDescriptionProvider` reads one caller-selected database path. `BerkeleyDbTerminalDescriptionProviderOptions` snapshots parser, database-size, and index-hop limits. `BerkeleyDbDatabaseFormatException` identifies malformed or unsupported containers and ncurses record envelopes while parser and I/O failures retain their existing exception types.

## Package boundary

The package:

- targets `net8.0`, `net9.0`, and `net10.0`;
- depends only on the matching `Icod.TermInfo` version;
- has no native Berkeley DB dependency;
- does not bundle Berkeley DB binaries;
- does not implement writes, transactions, environments, recovery, or general-purpose Berkeley DB APIs; and
- keeps compiled terminfo semantics in `Icod.TermInfo` Runtime.

The reviewed subset supports unencrypted, non-checksummed Hash-v9 files with sorted Hash pages (type 13), inline items, and overflow items. Other access methods, revisions, duplicate/subdatabase features, and legacy type-2 Hash pages are unsupported. Validation covers encountered records and does not guarantee an atomic snapshot during external writes.

Dedicated CI compares the production reader and public provider with native Berkeley DB stores on Linux and macOS; Windows reads Linux-generated fixtures without Berkeley DB installed. Big-endian coverage uses synthetic fixtures. Lookup keys are encoded as UTF-8, while compiled identity fields retain Runtime's byte-preserving Latin-1 interpretation. Qualified native fixtures use ASCII terminal names; broader non-ASCII producer compatibility is not claimed.

HDB03 qualification includes native-fixture provider parsing on all supported target frameworks and an isolated package-only consumer on `net8.0`, `net9.0`, and `net10.0`.

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

HDB04 qualification passed 253 unit cases and 18 native-store interoperability
cases per target framework on Windows, Linux, and macOS. The isolated
package-only consumer exercised the system provider on net8.0, net9.0, and
net10.0. HDB05 catalog enumeration is next.
