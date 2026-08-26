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

`1.1.0-Alpha-9` implements S09 and closes the planned 1.1 source-language
implementation tranches. The checked-in source corpus covers System V-style
entries, ncurses extended capabilities, unusual escapes, cancellation,
inheritance, malformed input, duplicate lookup identities, and bounded resource
attacks. Duplicate canonical names and aliases produce stable warning diagnostics
while document lookup remains deterministic and source-order based. A fixed,
deterministic mutation corpus exercises parser and resolver robustness without a
host `tic` or `infocmp` dependency. The checked-in T29 source/compiled fixture
pairs continue to provide offline semantic compatibility coverage.

The runtime dependency direction is one-way:

```text
Icod.TermInfo.Source
        |
        v
  Icod.TermInfo
```

`Icod.TermInfo` does not depend on this package.
