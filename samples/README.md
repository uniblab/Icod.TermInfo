# Icod.TermInfo Samples

The repository contains two executable samples. They are intentionally separate
so acquisition examples stay easy to copy without mixing them with interactive
terminal-control output.

Both sample projects target `net8.0`, `net9.0`, and `net10.0`. Every
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

## Acquisition guide

For the complete consumer-facing explanation of supported compiled formats,
directory layout, discovery precedence, options, errors, caching, and refresh,
see `../docs/0.9.0-ACQUISITION-GUIDE.md`.
