# Icod.TermInfo.Tools

`Icod.TermInfo.Tools` is the installable .NET tool router for the managed
`Icod.TermInfo` command suite.

The tool targets `net10.0` and therefore requires a .NET 10 runtime.

## Install

```text
dotnet tool install --global Icod.TermInfo.Tools --version 1.5.0
```

Update or remove the global tool with:

```text
dotnet tool update --global Icod.TermInfo.Tools --version 1.5.0
dotnet tool uninstall --global Icod.TermInfo.Tools
```

For repository-local or application-local use, install through a tool manifest:

```text
dotnet new tool-manifest
dotnet tool install Icod.TermInfo.Tools --version 1.5.0
dotnet tool run icod-terminfo --version
```

The package installs one unambiguous command:

```text
icod-terminfo
```

It does not install global commands named `tic`, `infocmp`, `toe`, `captoinfo`,
or `infotocap`. Those traditional command names belong to separately downloaded
standalone release archives.

Route the existing commands through it:

```text
icod-terminfo tic -V
icod-terminfo infocmp -V
icod-terminfo toe -V
```

Beginning with TC07 on the 1.6 development line, the same router additionally
exposes the conversion commands:

```text
icod-terminfo captoinfo -V
icod-terminfo infotocap -V
```

The router owns no terminfo semantics and does not reparse command-specific
options. It removes the first command operand and calls the existing
`Icod.TermInfo.Tic.Command`, `Icod.TermInfo.InfoCmp.Command`,
`Icod.TermInfo.Toe.Command`, `Icod.TermInfo.CapToInfo.Command`, or
`Icod.TermInfo.InfoToCap.Command` implementation in-process, preserving the selected
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

TC07 development builds also support:

```text
icod-terminfo captoinfo --help
icod-terminfo infotocap --help
```

## Standalone distribution

The router package complements rather than replaces the release archives. At
TC07 on the 1.6 development line, each of the six framework-dependent
tool-suite archives contains standalone executables named exactly:

```text
tic
infocmp
toe
captoinfo
infotocap
```

Users control where those archives are unpacked and whether their directory is
placed on `PATH`. This keeps intentional traditional-name installation separate
from the globally installable `icod-terminfo` router.

All five reusable libraries, all five standalone commands, and this router
consume the single `IcodTermInfoSuiteVersion` value in `Directory.Build.props`.
