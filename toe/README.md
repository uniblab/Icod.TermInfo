# toe

`toe` is the managed conventional terminfo database-listing command in the
`Icod.TermInfo` tool suite.

## T08 status

Version `1.4.0-Alpha-8` implements the T08 listing tranche by composing the
existing Inspection database-location and conventional catalog APIs. It does
not duplicate Runtime discovery policy or compiled-entry parsing.

Supported forms are:

```text
toe [options] [directory ...]
toe -D
toe -V
toe --version
toe --help
```

Supported listing options are:

```text
-a    inspect all discovered conventional databases
-h    identify each conventional database before its entries
-s    sort entries by canonical terminal name
```

With explicit directory operands, `toe` inspects exactly those roots in operand
order. `-a` does not change explicit-operand processing.

Without directory operands, `toe` uses the Runtime discovery snapshot exposed by
`Icod.TermInfo.Inspection`. Encoded `TERMINFO` is not a directory catalog and is
skipped. By default the first applicable conventional database is listed.
`-a` lists every applicable conventional directory in discovery order.

Each successfully parsed physical catalog entry is written as:

```text
canonical-name<TAB>description
```

No listing identity is inferred from a filename. Alias publications therefore
retain the canonical name parsed from the compiled entry. Duplicate canonical
names are not globally collapsed.

With `-h`, each conventional database is introduced by:

```text
# <absolute-database-root>
```

Malformed, oversized, unreadable, misplaced, or deliberately skipped linked
catalog candidates are reported on standard error. Enumeration continues when
the Inspection catalog says that it is safe to continue, and the command
returns status `1` after all safely obtainable entries have been emitted.
Missing roots discovered through the normal Runtime search plan are clean
misses; an explicitly requested missing root is an operational failure.

Exit status follows `Icod.CommandFramework`:

```text
0    success
1    operational/database failure
2    usage error
130  cancellation
```

`-u` and `-U` source dependency analysis belong to T09 and are deliberately
rejected by T08 rather than approximated.

The command targets .NET 10. The reusable `Icod.TermInfo` package family
continues to target `net8.0`, `net9.0`, and `net10.0`.
