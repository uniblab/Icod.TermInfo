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

`1.1.0-Alpha-7` implements S07 `use=` inheritance resolution on top of the
S06 cancellation state. The resolver supports canonical and alias lookup,
recursive and multiple-parent inheritance, caller-supplied source-entry
providers, deterministic missing-entry and cycle diagnostics, and a bounded
inheritance depth. Parent composition follows terminfo's right-to-left `use=`
processing while explicit child fields remain higher priority than all inherited
state. Resolved source remains separate from `TerminalDescription`; conversion
to the stable runtime model remains S08 work.

The runtime dependency direction is one-way:

```text
Icod.TermInfo.Source
        |
        v
  Icod.TermInfo
```

`Icod.TermInfo` does not depend on this package.
