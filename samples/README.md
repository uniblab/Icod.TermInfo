# Icod.TermInfo Samples

The repository contains three executable API samples and one command-suite
walkthrough. The API samples remain separate so acquisition examples stay easy
to copy without mixing them with interactive terminal-control output. The
Toolchain sample demonstrates the reusable Source -> Compiler -> Runtime ->
Inspection flow, including 1.7 relative-source synthesis, while ToolSuite
demonstrates the coordinated five-command 1.7 suite: `tic`, `infocmp`, `toe`,
`captoinfo`, and `infotocap`.

All three executable API sample projects target `net8.0`, `net9.0`, and
`net10.0`. Every
`dotnet run` example therefore specifies a framework; substitute `-f net8.0` or
`-f net9.0` when exercising those consumer targets.

## Icod.TermInfo.Sample

`Icod.TermInfo.Sample` is the general API demonstration. It covers terminal
profiles, environment resolution, standard and extended capabilities, parameter
expansion, color, padding, terminal size, custom providers, and optional Windows
virtual-terminal output enablement.

Use `--describe-only` when no terminal-control strings should be emitted.

```text
dotnet run --project samples/Icod.TermInfo.Sample/Icod.TermInfo.Sample.csproj -f net10.0 -- --describe-only --profile xterm-256color
```

See `Icod.TermInfo.Sample/README.md`.

## Icod.TermInfo.Acquisition.Sample

`Icod.TermInfo.Acquisition.Sample` is the focused compiled-database acquisition
demonstration introduced in 0.9 and retained for 1.0. It never emits
terminal-control strings.

Commands:

```text
parse <compiled-file>
directory <root> <terminal-name>
system <terminal-name>
restricted <terminal-name>
fallback <terminal-name>
```

Examples:

```text
dotnet run --project samples/Icod.TermInfo.Acquisition.Sample/Icod.TermInfo.Acquisition.Sample.csproj -f net10.0 -- system xterm-256color
```

```text
dotnet run --project samples/Icod.TermInfo.Acquisition.Sample/Icod.TermInfo.Acquisition.Sample.csproj -f net10.0 -- directory /usr/share/terminfo xterm
```

See `Icod.TermInfo.Acquisition.Sample/README.md`.

## Icod.TermInfo.Toolchain.Sample

`Icod.TermInfo.Toolchain.Sample` is the deterministic reusable-library toolchain
demonstration introduced for 1.5. It parses and resolves controlled `.ti`
source, compiles and publishes it into a temporary conventional database,
reloads the child entry through the Runtime provider, and verifies the acquired
description through Inspection. It does not depend on the host `TERM` value or
installed terminfo database.

Run it with:

```text
dotnet run --project samples/Icod.TermInfo.Toolchain.Sample/Icod.TermInfo.Toolchain.Sample.csproj -f net10.0
```

See `Icod.TermInfo.Toolchain.Sample/README.md`.

## ToolSuite

`ToolSuite` is a data-and-command walkthrough for the managed 1.8 command suite.
It uses controlled terminfo and termcap source files plus an explicit local
database root, so the example does not depend on the host's installed terminfo or
termcap databases.

The walkthrough covers validation, publication, effective rendering, relative
synthesis through `infocmp -u`, explicit-candidate parent planning through
`infocmp --plan-use`, direct and routed planning equivalence, generated-source
validation, semantic comparison, conventional database enumeration,
forward/reverse `use=` dependency reports, termcap-to-terminfo conversion, and
terminfo-to-termcap round trips.

See `ToolSuite/README.md`.

## Acquisition guide

For the complete consumer-facing explanation of supported compiled formats,
directory layout, discovery precedence, options, errors, caching, and refresh,
see `../docs/0.9.0-ACQUISITION-GUIDE.md`.
