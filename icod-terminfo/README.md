# Icod.TermInfo.Tools

`Icod.TermInfo.Tools` is the installable .NET tool router for the managed
`Icod.TermInfo` command suite.

## Install

```text
dotnet tool install --global Icod.TermInfo.Tools --version 1.5.0
```

The package installs one unambiguous command:

```text
icod-terminfo
```

Route the existing commands through it:

```text
icod-terminfo tic -V
icod-terminfo infocmp -V
icod-terminfo toe -V
```

The router owns no terminfo semantics and does not reparse command-specific
options. It removes the first command operand and calls the existing
`Icod.TermInfo.Tic.Command`, `Icod.TermInfo.InfoCmp.Command`, or
`Icod.TermInfo.Toe.Command` implementation in-process, preserving the selected
command's standard streams, cancellation behavior, diagnostics, and exit status.

## Router options

```text
icod-terminfo --help
icod-terminfo -h
icod-terminfo --version
icod-terminfo -V
```

For command-specific help, route the command's normal help option:

```text
icod-terminfo tic --help
icod-terminfo infocmp --help
icod-terminfo toe --help
```

## Standalone distribution

The router package complements rather than replaces the release archives. The
six framework-dependent tool-suite archives continue to contain standalone
executables named exactly:

```text
tic
infocmp
toe
```

Users control where those archives are unpacked and whether their directory is
placed on `PATH`. This keeps intentional traditional-name installation separate
from the globally installable `icod-terminfo` router.

All four reusable libraries, all three standalone commands, and this router
consume the single `IcodTermInfoSuiteVersion` value in `Directory.Build.props`.
