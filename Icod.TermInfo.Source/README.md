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

`1.1.0-Alpha-8` implements S08 `TerminalDescription` materialization on top of
the S07 resolved source state. `TermInfoSourceResolvedEntry.ToTerminalDescription`
projects terminal identity plus all effectively present standard and extended
capabilities into the stable runtime model. Source-only `use=` declarations,
source locations, comments, and cancellation tombstones do not leak into the
runtime description. The authoritative T29 source/compiled fixture pairs verify
semantic parity across legacy, cancellation, high-byte, extended-capability, and
wide-numeric cases. S09 remains the final 1.1 corpus, fuzzing, and compatibility
tranche.

The runtime dependency direction is one-way:

```text
Icod.TermInfo.Source
        |
        v
  Icod.TermInfo
```

`Icod.TermInfo` does not depend on this package.
