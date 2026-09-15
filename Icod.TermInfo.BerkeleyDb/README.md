# Icod.TermInfo.BerkeleyDb

`Icod.TermInfo.BerkeleyDb` is the optional managed package for read-only acquisition from ncurses-compatible Berkeley DB hashed terminfo stores.

## 1.15 development status

`1.15.0-Alpha-4` builds on the accepted explicit public terminal-description provider on the accepted managed Hash-v9 reader. It supports bounded exact-key lookup with inline and off-page records, overflow reconstruction, both byte orders, ncurses marker resolution, compiled-entry parsing, exact identity validation, and successful-result caching.

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
