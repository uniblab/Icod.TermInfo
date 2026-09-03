# infocmp

## 1.9 JSON automation

Version `1.9.0` publishes the frozen direct machine-readable projections of the
frozen Inspection values:

```text
infocmp --json target
infocmp --json -d left right
infocmp --json --plan-use target candidate [candidate ...]
infocmp --json --plan-use --all-candidates -B directory target
```

The forms emit, respectively, one `terminalDescription`, `comparison`, or
`sourcePlan` version-1 document followed by exactly one LF. JSON mode rejects
human-only source-layout and comparison-presentation combinations when they are
irrelevant or ambiguous. Failures emit no partial stdout document.

`--all-candidates` requires `--plan-use`, one explicit `-B` conventional
directory, and exactly one target. The directory is inspected once through the
frozen catalog contract; candidates retain canonical catalog order, semantic
duplicates collapse to one canonical candidate, conflicting physical copies or
incomplete catalogs are rejected, and the target is excluded by the frozen
planner identity rule. No system database discovery occurs. Without `--json`,
the same form emits only the selected terminfo source.

MI06 changes no option or document semantics. It adds large escaped-input and
culture hardening plus real tool-package, archive, and cross-host execution.
The stable release adds no command behavior and publishes these validated forms
as the stable 1.9 command contract.

## 1.8 relative-source planning

Version 1.8.0 adds deterministic bounded parent selection without changing the
frozen 1.7 `-u` synthesis contract:

```text
infocmp --plan-use [options] target candidate [candidate ...]
```

The first operand is acquired through `-A`; every explicit candidate is acquired
through `-B`. Candidate operand spelling becomes the possible emitted `use=`
name, and candidate order is the final planning tie-break. The command does not
discover additional candidates or enumerate a catalog.

Existing `-0`, `-1`, `-w`, `-s`, and `-x` source controls apply. Planning bounds
are:

```text
--max-parents count   selected-parent limit, default 2, range 0..64
--max-plans count     evaluated-plan limit, default 4097, range 1..1000000
--require-exhaustive  reject a plan space larger than the budget; default
--allow-bounded       return the best deterministic evaluated prefix
```

`-u`, `-d`, `-c`, `-n`, `-q`, and `-D` cannot be combined with `--plan-use`.
Planning-bound controls require planning mode. Successful planning writes only
the selected source to stdout and leaves stderr empty.

The stable 1.8 contract is frozen by the exact Inspection API baseline,
generated-state oracle, package consumers, direct and routed command tests, and
all six matching archive smokes.

## 1.7 relative synthesis

Version 1.7.0 adds the frozen RS06 `infocmp -u` command contract. The RS07
differential and pathological validation remains permanent release evidence.
The command remains a thin adapter over the frozen Inspection synthesis API.

`infocmp` is part of the `Icod.TermInfo` managed terminfo tool suite.

## 1.7 RS07 validation and hardening

`1.7.0-Alpha-7` adds no new `infocmp` command semantics. The RS06 `-u`
contract is hardened by deterministic generated-state round trips, a checked-in
semantic differential corpus captured from pinned ncurses `infocmp -u`, and
pathological boundary tests for parent counts, large extended unions, long
strings, alias-mediated references, culture changes, and cancellation.

Normal CI consumes the checked-in corpus and does not require host ncurses.
The command remains a thin adapter over `Icod.TermInfo.Inspection`.

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
infocmp --plan-use [options] target candidate [candidate ...]
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
--plan-use target candidate [...]  select parents from explicit candidates
```

With two or more terminals and no explicit comparison selector, `-u`, or
`--plan-use`, difference mode (`-d`) is the default. Semantic differences are
command output, not a failure, and therefore return status `0`.

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
infocmp --plan-use [options] target candidate [candidate ...]
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
--plan-use      select ordered use= parents from explicit candidates
--max-parents count
                limit selected parents; default 2, range 0..64
--max-plans count
                limit evaluated plans; default 4097, range 1..1000000
--require-exhaustive
                reject a budget smaller than the complete plan space; default
--allow-bounded return the best deterministic evaluated prefix
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
`--require-exhaustive` is mutually exclusive with `--allow-bounded`. Planning-
bound controls are rejected outside `--plan-use` mode.

## Operands

With no terminal operand, `TERM` supplies the one-terminal name. One operand is
rendered. Two or more operands compare the first terminal against each later
terminal unless `-u` selects relative synthesis. In synthesis mode, the first
operand is the target and every later operand is an ordered parent. Use `--`
before a terminal name beginning with `-`.

In planning mode, the first operand is the target and every later operand is one
ordered candidate position. Exact operand spelling is preserved when that
candidate is selected. Duplicate candidate spellings are usage errors.

## Environment

`TERM` is read only for zero-operand one-terminal inspection. `-A` and `-B`
construct explicit directory providers and do not mutate `TERMINFO` or other
process environment variables. Without an explicit database for a side, normal
Runtime system discovery is used.

## Exit statuses

```text
0    successful rendering/comparison/synthesis/planning, including differences
1    acquisition/database/operational synthesis or planning failure
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
infocmp -A./target-db -B./candidate-db --max-parents 2 --plan-use child decoy base
```

## Compatibility

Icod uses `TerminalDescriptionComparer`, `TerminalDescriptionSourceRenderer`,
`TerminalDescriptionSourceSynthesizer`, and `TerminalDescriptionSourcePlanner`
as the authoritative semantic engines. Current ncurses behavior is followed for
`-c -u`, while the existing
Icod Source resolver remains authoritative for parent precedence. Exact ncurses
comments, whitespace, provenance, or original source reconstruction are not
claimed. Unsupported switches are reported as usage errors rather than ignored.

## Non-goals

RP06 does not add C initializer generation, initialization-string analysis,
vendor subsets, padding-insensitive comparison, Compiler-backed `-Q` output,
implicit system candidate discovery, or command-level catalog-wide planning.
