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

`1.1.0-Alpha-6` implements S06 cancellation semantics on top of the S05
classified unresolved model. An internal semantic state records explicit values
and cancellation tombstones for standard and extended capabilities so a
higher-priority cancellation cannot be undone by lower-priority inheritance.
S06 distinguishes right-to-left parent overlays from the final lower-priority
inheritance beneath local fields, providing the precedence primitives required
for S07. The `use=` graph itself, including cycle, missing-parent, and depth
diagnostics, remains S07 work.

The runtime dependency direction is one-way:

```text
Icod.TermInfo.Source
        |
        v
  Icod.TermInfo
```

`Icod.TermInfo` does not depend on this package.
