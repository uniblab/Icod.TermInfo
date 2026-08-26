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

`1.1.0-Alpha-2` implements the S02 lexical and source-location foundation. It
can tokenize source from a `string` or `TextReader`, retain exact raw field text,
identify entry names/aliases/descriptions and capability forms, preserve
comments, distinguish `use=` and cancellation, and report deterministic
`TISdddd` diagnostics with source spans. String/numeric value decoding remains
S03 work.

The runtime dependency direction is one-way:

```text
Icod.TermInfo.Source
        |
        v
  Icod.TermInfo
```

`Icod.TermInfo` does not depend on this package.
