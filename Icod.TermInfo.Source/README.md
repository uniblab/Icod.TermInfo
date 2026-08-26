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

`1.1.0-Alpha-3` implements S03 string and numeric source-value semantics on top
of the S02 lexer. Decimal, octal, and hexadecimal numeric spellings decode to
the stable signed 32-bit model. String values support terminfo control notation,
backslash escapes, octal byte escapes, multiline continuations, and historical
zero/high-byte behavior while retaining deterministic `TISdddd` diagnostics.
The unresolved source-entry model remains S04 work.

The runtime dependency direction is one-way:

```text
Icod.TermInfo.Source
        |
        v
  Icod.TermInfo
```

`Icod.TermInfo` does not depend on this package.
