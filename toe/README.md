# toe

`toe` is the managed conventional terminfo database-listing command in the
`Icod.TermInfo` tool suite.

## T09 status

Version `1.4.0-Alpha-9` retains T08 conventional database listing and adds
forward/reverse terminfo source dependency reports plus optional semantic
duplicate markers. Source parsing/resolution is delegated to
`Icod.TermInfo.Source`; duplicate equality is delegated to
`TerminalDescriptionComparer`.

Supported forms are:

```text
toe [options] [directory ...]
toe -u file
toe -U file
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

When `-a` and `-s` expose a canonical name in more than one database, the first
entry in database order becomes the comparison reference and each later root is
marked as either semantically equal or semantically different. The marker is
explicitly Icod-defined and equality comes from `TerminalDescriptionComparer`,
not compiled-file byte equality.

Source dependency modes are standalone:

```text
toe -u file    child<TAB>parent, preserving source use= order
toe -U file    parent<TAB>child, grouped deterministically by source identity
```

Alias references resolve to canonical source identities. Missing parents and
inheritance cycles are diagnosed through the existing Source resolver; safely
parsed dependency edges are still emitted before the command returns status 1.

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

The command targets .NET 10. The reusable `Icod.TermInfo` package family
continues to target `net8.0`, `net9.0`, and `net10.0`.
