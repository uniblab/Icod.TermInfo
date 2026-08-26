# Icod.TermInfo.Source

`Icod.TermInfo.Source` is the optional terminfo source-language layer for
`Icod.TermInfo`.

The package is intentionally separate from the stable runtime package. Ordinary
applications that only load or use `TerminalDescription` values continue to
reference `Icod.TermInfo` alone.

## 1.1.0 development line

The 1.1 line adds managed parsing and resolution of `.ti` source, including
source diagnostics, cancellation, extended capabilities, and `use=`
inheritance. Resolved source entries materialize into the same immutable
`TerminalDescription` model used by compiled terminfo acquisition.

`1.1.0-Alpha-4` implements S04 unresolved source parsing on top of the S02
lexer and S03 value semantics. `TermInfoSourceParser` produces immutable
`TermInfoSourceDocument` and `TermInfoSourceEntry` values whose ordered fields
retain capability names, decoded numeric/string values, cancellations, `use=`
references, disabled fields, exact lexical text, and source spans. Capability
names intentionally remain unclassified until S05, and no inheritance or
`TerminalDescription` materialization occurs in S04.

The runtime dependency direction is one-way:

```text
Icod.TermInfo.Source
        |
        v
  Icod.TermInfo
```

`Icod.TermInfo` does not depend on this package.
