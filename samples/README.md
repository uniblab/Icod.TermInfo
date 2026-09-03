# Icod.TermInfo Samples

The repository contains four executable API samples and one command-suite
walkthrough. The API samples remain separate so acquisition, terminal-control,
toolchain, and multi-database examples stay easy to copy without mixing unrelated
concerns.

The 1.10 addition is `Icod.TermInfo.DatabaseSet.Sample`, an executable public-API
walkthrough for ordered explicit database sets, precedence, semantic shadow and
alias evidence, set comparison, multi-database planning, and all three version-2
JSON document kinds. Its normalized JSON fixtures are checked in and verified on
`net8.0`, `net9.0`, and `net10.0` by the permanent release gate.

The existing Toolchain sample demonstrates the reusable Source -> Compiler ->
Runtime -> Inspection flow, including 1.8 parent planning, 1.7 relative-source
synthesis, and the frozen 1.9 source-plan JSON contract. ToolSuite demonstrates
the coordinated five-command suite: `tic`, `infocmp`, `toe`, `captoinfo`, and
`infotocap`, including both the frozen 1.9 version-1 JSON forms and the additive
1.10 database-set automation forms.

All four executable API sample projects target `net8.0`, `net9.0`, and
`net10.0`. Every `dotnet run` example therefore specifies a framework; substitute
`-f net8.0` or `-f net9.0` when exercising those consumer targets.

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
demonstration introduced in 0.9 and retained through 1.10. It never emits
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
demonstration introduced for 1.5 and extended through 1.9. It parses and resolves
controlled `.ti` source, selects a useful parent from an explicit candidate set,
compiles and publishes the planned source into a temporary conventional
database, reloads the child entry through the Runtime provider, verifies the
acquired description through Inspection, and renders the immutable plan as a
version-1 JSON document. It does not depend on the host `TERM` value or installed
terminfo database.

Run it with:

```text
dotnet run --project samples/Icod.TermInfo.Toolchain.Sample/Icod.TermInfo.Toolchain.Sample.csproj -f net10.0
```

See `Icod.TermInfo.Toolchain.Sample/README.md`.

## Icod.TermInfo.DatabaseSet.Sample

`Icod.TermInfo.DatabaseSet.Sample` is the focused 1.10 reusable-library example.
It creates controlled compiled databases through the public Compiler API and
then exercises the public Inspection API without command parsing or ambient
database discovery.

The sample demonstrates:

- ordered `InspectSet(...)` construction;
- canonical lookup and first-root precedence;
- conflicting shadow classification;
- alias-collision evidence;
- structural/effective set comparison;
- conflict-free multi-database parent planning;
- `databaseSet`, `databaseSetComparison`, and `databaseSetPlan` JSON.

Run it with:

```text
dotnet run --project samples/Icod.TermInfo.DatabaseSet.Sample/Icod.TermInfo.DatabaseSet.Sample.csproj -f net10.0
```

See `Icod.TermInfo.DatabaseSet.Sample/README.md` and
`../docs/1.10.0-MULTI-DATABASE-GUIDE.md`.

## ToolSuite

`ToolSuite` is a data-and-command walkthrough for the managed command suite. It
uses controlled terminfo and termcap source files plus explicit local database
roots, so the example does not depend on the host's installed terminfo or termcap
databases.

The walkthrough covers validation, publication, effective rendering, relative
synthesis through `infocmp -u`, explicit-candidate parent planning through
`infocmp --plan-use`, direct and routed planning equivalence, generated-source
validation, semantic comparison, conventional database enumeration,
forward/reverse `use=` dependency reports, termcap-to-terminfo conversion,
terminfo-to-termcap round trips, all four frozen version-1 JSON document kinds,
and the three additive 1.10 database-set JSON document kinds.

See `ToolSuite/README.md`.

## Acquisition guide

For the complete consumer-facing explanation of supported compiled formats,
directory layout, discovery precedence, options, errors, caching, and refresh,
see `../docs/0.9.0-ACQUISITION-GUIDE.md`.
