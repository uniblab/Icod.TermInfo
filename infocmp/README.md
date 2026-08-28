# infocmp

`infocmp` is part of the `Icod.TermInfo` managed terminfo tool suite.

## T07 status

Version `1.4.0-Alpha-7` adds deterministic semantic comparison to the T06
one-terminal acquisition and effective-source rendering path.

Supported through T07:

```text
infocmp [options] [terminal ...]
infocmp -D
infocmp -V
infocmp --version
infocmp --help
```

Operand behavior is:

```text
0 terminals     use TERM and render one effective description
1 terminal      render that effective description
2+ terminals    compare the first terminal with each subsequent terminal
```

With two or more terminals and no explicit comparison selector, difference mode
(`-d`) is the default. Semantic differences are command output, not a failure, and
therefore return status `0`.

Database selection is:

```text
-A <directory>    use this explicit database for the first terminal
-B <directory>    use this explicit database for subsequent terminals
```

Neither option mutates `TERMINFO` or other process environment variables. Without
the matching explicit root, that side uses the normal Runtime
`SystemTerminalDescriptionProvider` search policy.

One-terminal presentation options remain:

```text
-0                emit one logical source line
-1                emit one capability per line
-w <width>        request canonical wrapping width
-s d|i|l|c        order standard capabilities by database, short name,
                  long name, or termcap code
-x                include effective extended capabilities
-D                report Runtime database discovery locations
```

Comparison modes are:

```text
-d                report structured semantic differences
-c                report equal effective capabilities
-n                report standard capabilities absent from all operands
-q                use short comparison presentation
-x                include extended capabilities in -d/-c reports
```

`-d` delegates semantic comparison to `TerminalDescriptionComparer`. `-c` uses
the already-acquired immutable descriptions and Runtime capability metadata.
`-n` deliberately walks only the closed standard capability catalog; arbitrary
extended names have no defined absent-name universe, so `-x` does not enlarge
`-n`.

Default one-terminal output contains standard capabilities only. `-x` includes
effective extended capabilities. Rendered text represents effective
`TerminalDescription` state; it does not reconstruct original comments,
whitespace, `use=` history, cancellations, disabled fields, or source provenance.

The command targets .NET 10. The reusable `Icod.TermInfo` libraries remain
available for `net8.0`, `net9.0`, and `net10.0`.
