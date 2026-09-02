# Icod.TermInfo.Tools

`Icod.TermInfo.Tools` is the installable .NET tool router for the managed
`Icod.TermInfo` command suite.

The tool targets `net10.0` and therefore requires a .NET 10 runtime.

## 1.9 JSON automation

Version `1.9.0-Alpha-7` freezes the same machine-readable command contracts as
the standalone commands:

```text
icod-terminfo infocmp --json target
icod-terminfo infocmp --json -d left right
icod-terminfo infocmp --json --plan-use target candidate
icod-terminfo infocmp --json --plan-use --all-candidates -B directory target
icod-terminfo toe --json directory
```

The router adds no JSON or planning semantics. It forwards arguments, streams,
cancellation, diagnostics, and exit status, so routed and direct output is
byte-for-byte identical.

MI07 adds no router behavior. It freezes that dispatch contract, fresh installed
tool-package evidence, and every matching standalone archive on Windows, Linux,
and macOS as part of the complete 1.9 release gate.

## 1.8 release status

Version 1.8.0 adds routed relative-source planning without adding router-owned
semantics:

```text
icod-terminfo infocmp -A ./target-db -B ./candidate-db --max-parents 2 --require-exhaustive --plan-use target decoy useful
```

The router forwards the exact `--plan-use` arguments, streams, cancellation
token, diagnostics, and exit status to `Icod.TermInfo.InfoCmp.Command`. Direct
`infocmp` and routed `icod-terminfo infocmp` planning are therefore required to
produce byte-for-byte identical source. The installable package smoke and every
matching standalone archive smoke execute the same controlled planning case.

Version 1.8.0 freezes that command and distribution surface. The stable release
adds no router-owned option or dispatch behavior beyond the validated planning
composition.

## 1.7 release status

Version 1.7.0 adds `infocmp -u` relative-source synthesis to the coordinated
tool distribution. The router still owns no terminfo semantics; it forwards the
operation to the same standalone command implementation and retains the
five-command dispatch topology.

## Install

```text
dotnet tool install --global Icod.TermInfo.Tools --version 1.8.0
```

Update or remove the global tool with:

```text
dotnet tool update --global Icod.TermInfo.Tools --version 1.8.0
dotnet tool uninstall --global Icod.TermInfo.Tools
```

For repository-local or application-local use, install through a tool manifest:

```text
dotnet new tool-manifest
dotnet tool install Icod.TermInfo.Tools --version 1.8.0
dotnet tool run icod-terminfo --version
```

The package installs one unambiguous command:

```text
icod-terminfo
```

It does not install global commands named `tic`, `infocmp`, `toe`, `captoinfo`,
or `infotocap`. Those traditional command names belong to separately downloaded
standalone release archives.

Version 1.8.0 retains all five coordinated commands introduced in 1.6.0,
preserves 1.7 relative synthesis, and adds relative-source planning through the
existing `infocmp` command:

```text
icod-terminfo tic -V
icod-terminfo infocmp -V
icod-terminfo toe -V
icod-terminfo captoinfo -V
icod-terminfo infotocap -V
icod-terminfo infocmp -u target parent
icod-terminfo infocmp --plan-use target candidate
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
icod-terminfo captoinfo --help
icod-terminfo infotocap --help
```

## Standalone distribution

The router package complements rather than replaces the release archives. Each
of the six framework-dependent 1.8.0 tool-suite archives contains standalone
executables named exactly:

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
