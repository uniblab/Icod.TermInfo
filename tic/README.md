# tic

`tic` is part of the `Icod.TermInfo` managed terminfo tool suite.

## T09 status

Version `1.4.0-Alpha-9` retains T05 safe conventional database publication and
the T04 managed source-validation path while T09 work remains isolated to
`toe`.

Supported through T09:

```text
tic [options] file
tic -c [options] file
tic -D
tic -V
tic --version
tic --help
```

`file` may be `-` to read strict UTF-8 source from standard input. The complete
source document is parsed through `Icod.TermInfo.Source`; selected entries are
resolved with their `use=` inheritance and checked for compiled representability
before publication begins.

Use `-c` for the non-mutating T04 validation path:

```text
tic -c file
tic -c -e name,alias file
tic -c -x file
```

Without `-c`, successful validation is followed by publication through
`CompiledTermInfoDatabaseWriter`:

```text
tic -o ./terminfo file
tic -e xterm,xterm-256color -o ./terminfo file
tic --force -o ./terminfo file
tic -s -o ./terminfo file
```

`-o` chooses an explicit conventional database root. When `-o` is absent, the
command selects only these safe writable candidates, in this order:

```text
1. directory-valued TERMINFO
2. the Runtime-defined user database
3. otherwise fail and require -o
```

Encoded `TERMINFO`, `TERMINFO_DIRS`, and platform system/default roots are never
selected implicitly for writes.

Existing destinations are rejected by default. `--force` opts into the
Compiler writer's existing overwrite policy. The writer preflights the complete
publication plan, stages temporary files with write-through, and then commits the
canonical and alias destinations. The command does not duplicate that path or
transaction logic.

`-s` writes a successful publication summary to standard error containing the
normalized destination root, number of compiled source entries, and warning
count. Ordinary successful publication remains quiet.

Known extended capabilities are accepted normally. Syntactically valid unknown
extended capability names require `-x`.

`-D` prints the ordered Runtime database-location discovery model supplied by
`Icod.TermInfo.Inspection`; encoded `TERMINFO` values are identified without
printing their encoded payload.

Exit status follows the command-suite contract:

```text
0    validation/publication succeeded, including warnings-only source
1    source/input/destination/publication failure
2    command usage error
130  cancellation before the publication commit begins
```

Publication through the frozen synchronous Compiler writer is treated as one
non-interruptible commit boundary. Cancellation is checked before that boundary;
once publication begins, the writer is allowed to finish so the command does not
report cancellation after files have actually been committed.

The command targets .NET 10. The reusable `Icod.TermInfo` libraries remain
available for `net8.0`, `net9.0`, and `net10.0`.
