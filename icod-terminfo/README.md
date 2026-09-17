# Icod.TermInfo.Tools

`Icod.TermInfo.Tools` is the installable .NET tool router for the managed
Icod.TermInfo command suite. The package installs one command, `icod-terminfo`,
which dispatches to the existing `tic`, `infocmp`, `toe`, `captoinfo`, and
`infotocap` implementations without duplicating their semantics.

The tool targets `net10.0` and requires a .NET 10 runtime.

## 1.15 release status

Version `1.15.0` preserves the five-command router contract, installed-tool
validation, and six-RID standalone archive topology. It adds reviewed explicit
hashed-file acquisition for `infocmp -A/-B` and human `toe` operands without
changing frozen JSON routes, conventional ambient discovery, or `tic`
publication.

The coordinated tool version advances with the reusable package family so a
single release identity covers Runtime, Source, Compiler, Inspection, Termcap,
and the tool distribution.

## Install

```text
dotnet tool install --global Icod.TermInfo.Tools --version 1.15.0
```

Update or remove the global tool with:

```text
dotnet tool update --global Icod.TermInfo.Tools --version 1.15.0
dotnet tool uninstall --global Icod.TermInfo.Tools
```

For repository-local or application-local use:

```text
dotnet new tool-manifest
dotnet tool install Icod.TermInfo.Tools --version 1.15.0
dotnet tool run icod-terminfo --version
```

The package installs only:

```text
icod-terminfo
```

It does not install global commands named `tic`, `infocmp`, `toe`, `captoinfo`,
or `infotocap`. Those traditional names belong to separately downloaded
standalone release archives.

## Commands

```text
icod-terminfo tic -V
icod-terminfo infocmp -V
icod-terminfo toe -V
icod-terminfo captoinfo -V
icod-terminfo infotocap -V
```

The router strips the first command token and delegates the remaining arguments,
streams, diagnostics, cancellation, and exit status to the corresponding command
implementation.

## Frozen command features

The coordinated command line retains the features added by earlier releases,
including:

- `infocmp -u` deterministic relative-source synthesis from 1.7;
- `infocmp --plan-use` bounded deterministic parent planning from 1.8;
- `infocmp --json` machine-readable description/comparison/plan forms from 1.9;
- `toe --json` machine-readable database catalogs from 1.9; and
- the additive 1.10 multi-database automation forms.

Examples:

```text
icod-terminfo infocmp -u target parent
icod-terminfo infocmp --plan-use target candidate
icod-terminfo infocmp --json target
icod-terminfo infocmp --json -d left right
icod-terminfo infocmp --json --plan-use target candidate
icod-terminfo infocmp --json --plan-use --all-candidates -B directory target
icod-terminfo toe --json directory
```

The router owns no terminfo semantics and does not reparse command-specific
options. Direct and routed forms are required to preserve the same behavior and,
for deterministic machine-readable/source outputs, byte-identical content.

## Standalone archives

The release distribution also provides framework-dependent .NET 10 archives for:

```text
win-x64
win-arm64
linux-x64
linux-arm64
osx-x64
osx-arm64
```

Each archive contains the traditional `tic`, `infocmp`, `toe`, `captoinfo`, and
`infotocap` launcher names plus their managed dependencies. Archive users provide
the .NET 10 runtime and choose the unpack/install location.

## Release validation

PR validation installs the freshly packed `Icod.TermInfo.Tools` package and
executes router smoke on supported hosts. The release pipeline also builds and
structurally verifies all six archive RIDs and executes matching-host archive
smoke. Version 1.14 changes release identity only for the command layer; existing
command semantics remain frozen.

See the root `../README.md`, `../docs/VERSIONING.md`, and
`../docs/COMPATIBILITY.md` for the coordinated release contract.
