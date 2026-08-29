# infocmp

`infocmp` is part of the `Icod.TermInfo` managed terminfo tool suite.

## 1.4.0 status

Version `1.4.0` ships the frozen T06/T07 acquisition, rendering, and semantic
comparison engines with T10 CLI/distribution hardening and the T11 validation
gate. Release closure adds no new comparison engine.

Supported in 1.4.0:

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

## Synopsis

```text
infocmp [options] [terminal ...]
infocmp -D
infocmp -V
infocmp --version
infocmp --help
```

## Options

```text
-A directory    explicit database for the first terminal
-B directory    explicit database for subsequent terminals
-0              one logical source line
-1              one capability per line
-w width        canonical wrapping width
-s d|i|l|c      capability ordering key
-d              semantic differences
-c              common effective capabilities
-n              standard capabilities absent from all operands
-q              short comparison presentation
-x              include effective extended capabilities where defined
-D              report Runtime database discovery locations
-V, --version   print the coordinated tool-suite version
--help          display help
--              end option parsing
```

Unambiguous short options may be clustered. `-A`, `-B`, `-w`, and `-s` accept
separated or attached values. Repeating the same comparison selector is
idempotent; conflicting `-d`, `-c`, and `-n` selectors remain a usage error.

## Operands

With no terminal operand, `TERM` supplies the one-terminal name. One operand is
rendered. Two or more operands compare the first terminal against each later
terminal. Use `--` before a terminal name beginning with `-`.

## Environment

`TERM` is read only for zero-operand one-terminal inspection. `-A` and `-B`
construct explicit directory providers and do not mutate `TERMINFO` or other
process environment variables. Without an explicit database for a side, normal
Runtime system discovery is used.

## Exit statuses

```text
0    successful rendering/comparison, including semantic differences
1    acquisition/database/operational failure
2    usage error
130  cancellation
```

## Examples

```text
infocmp xterm
infocmp -1 -xd xterm xterm-256color
infocmp -w120 xterm
infocmp -A./first -B./second -q terminal terminal
```

## Compatibility

Icod uses `TerminalDescriptionComparer` and the Inspection renderer as the
authoritative semantic engines. Exact ncurses comments, whitespace, provenance,
or source reconstruction are not claimed. Unsupported ncurses switches are
reported as usage errors rather than ignored.

## Non-goals

T10 does not add termcap conversion, relative `use=` synthesis, C initializer
generation, initialization-string analysis, vendor subsets, padding-insensitive
comparison, or Compiler-backed `-Q` output.
