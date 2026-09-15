# Icod.TermInfo.BerkeleyDb

`Icod.TermInfo.BerkeleyDb` is the optional managed package for read-only acquisition from ncurses-compatible Berkeley DB hashed terminfo stores.

## 1.15 development status

`1.15.0-Alpha-1` establishes the package boundary only. It intentionally exposes no hashed-store provider API yet.

HDB00 selected a dependency-free managed reader for the reviewed Berkeley DB **Hash on-disk format version 9** subset required by ncurses acquisition. Native Berkeley DB remains a CI interoperability oracle and is not a production dependency.

The planned data flow is:

```text
Berkeley DB Hash-v9 store
          |
          v
managed bounded container reader
          |
          v
ncurses marker-0 compiled bytes
          |
          v
Icod.TermInfo.CompiledTermInfoParser
          |
          v
TerminalDescription
```

## Package boundary

The package:

- targets `net8.0`, `net9.0`, and `net10.0`;
- depends only on the matching `Icod.TermInfo` version;
- has no native Berkeley DB dependency;
- does not bundle Berkeley DB binaries;
- does not implement writes, transactions, environments, recovery, or general-purpose Berkeley DB APIs; and
- keeps compiled terminfo semantics in `Icod.TermInfo` Runtime.

The managed Hash-v9 reader is scheduled for HDB02. The explicit public terminal-description provider is scheduled for HDB03.
