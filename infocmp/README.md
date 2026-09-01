# infocmp

`infocmp` is part of the `Icod.TermInfo` managed terminfo tool suite.

## 1.7 RS06 relative synthesis

`1.7.0-Alpha-6` exposes the reusable Inspection relative-source synthesizer
through `infocmp -u`.

The command form is:

```text
infocmp -u [options] target parent [parent ...]
```

The first operand is acquired through the normal first-terminal path (`-A` when
supplied). Every later operand is an ordered parent acquired through the
subsequent-terminal path (`-B` when supplied). Parent operand spelling and order
are preserved exactly in the emitted `use=` references, even when an operand is
an alias for a canonical compiled entry.

`-c -u` is accepted as an ncurses-compatible synonym for `-u`. `-d -u` and
`-n -u` are usage errors. `-q` remains comparison-only and is therefore also a
usage error with `-u`.

The existing source-presentation controls apply to synthesis:

```text
-0
-1
-w <width>
-s d|i|l|c
```

`-x` permits required local extended-capability declarations and cancellations.
Without `-x`, synthesis succeeds only when suppressing local extended directives
still reproduces the target's extended state. Otherwise the command fails rather
than emitting source that would resolve to different semantics.

The command layer owns only argument parsing, acquisition, diagnostics, and
presentation-option mapping. Delta, cancellation, ordered-parent, and rendering
semantics remain owned by `Icod.TermInfo.Inspection`.

## 1.6.x status

Version `1.6.0` retains the frozen T06/T07 acquisition, rendering, and semantic
comparison engines plus the T10 CLI/distribution and T11 validation gates. The
1.6 release adds the separate Termcap package and conversion commands without
changing `infocmp` comparison or command semantics.

Version `1.6.1` is a release-infrastructure hotfix over that frozen 1.6.0
contract. It does not change `infocmp` acquisition, rendering, comparison,
or command semantics.

Supported as either `infocmp ...` from a release archive or
`icod-terminfo infocmp ...` from the .NET tool:

```text
infocmp [options] [terminal ...]
infocmp -u [options] target parent [parent ...]
infocmp -D
infocmp -V
infocmp --version
infocmp --help
```

Operand behavior is:

```text
0 terminals             use TERM and render one effective description
1 terminal              render that effective description
2+ terminals            compare first with each later terminal
-u target parent [...]  synthesize target relative to ordered parents
```

With two or more terminals and no explicit comparison selector or `-u`,
difference mode (`-d`) is the default. Semantic differences are command output,
not a failure, and therefore return status `0`.

Database selection is:

```text
-A <directory>    use this explicit database for the first terminal
-B <directory>    use this explicit database for subsequent terminals
```

Neither option mutates `TERMINFO` or other process environment variables. Without
the matching explicit root, that side uses the normal Runtime
`SystemTerminalDescriptionProvider` search policy.

Source-presentation options are:

```text
-0                emit one logical source line
-1                emit one capability per line
-w <width>        request canonical wrapping width
-s d|i|l|c        order standard capabilities by database, short name,
                  long name, or termcap code
-x                include/permit extended capabilities where defined
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
infocmp -u [options] target parent [parent ...]
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
-u              synthesize first terminal relative to ordered parents
-d              semantic differences
-c              common effective capabilities; with -u, synonym for -u
-n              standard capabilities absent from all operands
-q              short comparison presentation
-x              include/permit extended capabilities where defined
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
terminal unless `-u` selects relative synthesis. In synthesis mode, the first
operand is the target and every later operand is an ordered parent. Use `--`
before a terminal name beginning with `-`.

## Environment

`TERM` is read only for zero-operand one-terminal inspection. `-A` and `-B`
construct explicit directory providers and do not mutate `TERMINFO` or other
process environment variables. Without an explicit database for a side, normal
Runtime system discovery is used.

## Exit statuses

```text
0    successful rendering/comparison/synthesis, including semantic differences
1    acquisition/database/operational synthesis failure
2    usage error
130  cancellation
```

## Examples

```text
infocmp xterm
infocmp -1 -xd xterm xterm-256color
infocmp -w120 xterm
infocmp -u xterm-256color xterm
infocmp -1 -x -u child base
infocmp -A./target-db -B./parent-db -u child base1 base2
```

## Compatibility

Icod uses `TerminalDescriptionComparer`, `TerminalDescriptionSourceRenderer`,
and `TerminalDescriptionSourceSynthesizer` as the authoritative semantic
engines. Current ncurses behavior is followed for `-c -u`, while the existing
Icod Source resolver remains authoritative for parent precedence. Exact ncurses
comments, whitespace, provenance, or original source reconstruction are not
claimed. Unsupported switches are reported as usage errors rather than ignored.

## Non-goals

RS06 does not add C initializer generation, initialization-string analysis,
vendor subsets, padding-insensitive comparison, Compiler-backed `-Q` output, or
parent discovery/reordering/minimization.
