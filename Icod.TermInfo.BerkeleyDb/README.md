# Icod.TermInfo.BerkeleyDb

`Icod.TermInfo.BerkeleyDb` is the optional managed package for read-only acquisition from ncurses-compatible Berkeley DB hashed terminfo stores.

## 1.15 development status

`1.15.0-Alpha-3` builds on the accepted internal managed Hash-v9 reader. It supports bounded exact-key lookup with inline and off-page records, overflow reconstruction, and both byte orders. No public hashed-store provider API is exposed yet.

HDB00 selected a dependency-free managed reader for the reviewed Berkeley DB **Hash on-disk format version 9** subset required by ncurses acquisition. Native Berkeley DB remains a CI interoperability oracle and is not a production dependency.

The internal ncurses resolver follows bounded marker-2 index chains over one acquired database image and extracts opaque marker-0 payloads. Empty records, unsupported markers, dangling targets, cycles, and excessive hops fail explicitly. Only an absent initial key is a clean miss.

HDB03 remains in progress. Its public provider will enforce compiled-entry parser limits, reuse `Icod.TermInfo.CompiledTermInfoParser`, validate requested identity, and implement successful-result caching and retry semantics.

## Package boundary

The package:

- targets `net8.0`, `net9.0`, and `net10.0`;
- depends only on the matching `Icod.TermInfo` version;
- has no native Berkeley DB dependency;
- does not bundle Berkeley DB binaries;
- does not implement writes, transactions, environments, recovery, or general-purpose Berkeley DB APIs; and
- keeps compiled terminfo semantics in `Icod.TermInfo` Runtime.

The reviewed subset supports unencrypted, non-checksummed Hash-v9 files with sorted Hash pages (type 13), inline items, and overflow items. Other access methods, revisions, duplicate/subdatabase features, and legacy type-2 Hash pages are unsupported. Validation covers encountered records and does not guarantee an atomic snapshot during external writes.

Dedicated CI compares the production reader directly with native Berkeley DB dumps on Linux and macOS; Windows reads Linux-generated fixtures without Berkeley DB installed. Big-endian coverage uses synthetic fixtures.

The explicit public terminal-description provider is scheduled for HDB03.
