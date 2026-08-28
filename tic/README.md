# tic

`tic` is part of the `Icod.TermInfo` managed terminfo tool suite.

## T04 status

Version `1.4.0-Alpha-4` introduces the first operational `tic` tranche as a
managed, non-mutating terminfo source validator.

Supported through T04:

```text
tic -c [options] file
tic -D
tic -V
tic --version
tic --help
```

`file` may be `-` to read UTF-8 source from standard input. `-c` parses the
complete document through `Icod.TermInfo.Source`, preserves source diagnostics
and locations, resolves the selected entries and their `use=` inheritance, and
runs each successfully resolved entry through the Compiler's in-memory
representation writer. No conventional database files are created in T04.

Selection is available through:

```text
tic -c -e name,alias file
```

Selected identities are matched case-sensitively against canonical names and
aliases. Selection does not change lexical parsing of the complete source, but
resolver and representation validation are limited to selected entries and the
parents they inherit.

Known extended capabilities are accepted normally. Syntactically valid unknown
extended capability names require `-x`:

```text
tic -c -x file
```

`-D` prints the ordered Runtime database-location discovery model supplied by
`Icod.TermInfo.Inspection`; encoded `TERMINFO` values are identified without
printing their encoded payload.

Exit status follows the command-suite contract:

```text
0    validation succeeded, including warnings-only source
1    source/input/representation failure
2    command usage error
130  cancellation
```

Database destination selection, writing, overwrite policy, summaries, and
`--force` are deliberately deferred to T05.

The command targets .NET 10. The reusable `Icod.TermInfo` libraries remain
available for `net8.0`, `net9.0`, and `net10.0`.
