# Icod.TermInfo Samples

The repository contains **eight executable API samples** plus one command-suite
walkthrough. The samples stay separate so acquisition, reusable toolchain,
multi-database automation, persistent-raster semantics, runtime-evidence
integration, and raster-backend selection can be copied without mixing unrelated
concerns.

All eight executable API sample projects target `net8.0`, `net9.0`, and
`net10.0`; every `dotnet run` example therefore specifies a framework. Substitute
`-f net8.0` or `-f net9.0` when exercising those consumer targets.

## Icod.TermInfo.RasterBackendSelection.Sample

`Icod.TermInfo.RasterBackendSelection.Sample` is the focused **1.14**
raster-backend selection example. It demonstrates the intended boundary:

```text
static TermInfo metadata
    -> explicit Sixel availability evidence
    -> Sixel candidate

caller-owned Terminal verification
    -> backend-scoped 1.13 observations/integration
    -> explicit Kitty Graphics availability evidence
    -> Kitty candidate

Sixel + Kitty Graphics candidates
    -> RasterBackendPlanner
    -> explicit caller preference
    -> JSON v6 selection-plan audit
```

The sample keeps backend availability separate from lifecycle and placement
support, never infers a backend from emulator names, and keeps all live result
mapping in consumer code. The default mode is deterministic and CI-safe.
`--live` uses published `Icod.Terminal 1.13.0` at the application boundary.
Production `Icod.TermInfo.Inspection` still has no Terminal dependency.

Run the deterministic form with:

```text
dotnet run --project samples/Icod.TermInfo.RasterBackendSelection.Sample/Icod.TermInfo.RasterBackendSelection.Sample.csproj -f net10.0
```

For interactive verification:

```text
dotnet run --project samples/Icod.TermInfo.RasterBackendSelection.Sample/Icod.TermInfo.RasterBackendSelection.Sample.csproj -f net10.0 -- --live
```

See `Icod.TermInfo.RasterBackendSelection.Sample/README.md` and
`../docs/1.14.0-RASTER-BACKEND-SELECTION-GUIDE.md`.

## Icod.TermInfo.PersistentRasterRuntimeIntegration.Sample

The **1.13** `Icod.TermInfo.PersistentRasterRuntimeIntegration.Sample` shows the
static-plan -> caller-owned live verification -> runtime observation -> evidence
integration -> replan boundary. The sample's observations are backend-neutral;
1.14 consumers maintain separate integration contexts when verification is scoped
to separate raster backends.

Its default mode is deterministic. `--live` invokes the published Terminal
semantic verification API and keeps the coarse-result-to-TermInfo-subject mapping
explicit in consumer code.

```text
dotnet run --project samples/Icod.TermInfo.PersistentRasterRuntimeIntegration.Sample/Icod.TermInfo.PersistentRasterRuntimeIntegration.Sample.csproj -f net10.0
```

See `Icod.TermInfo.PersistentRasterRuntimeIntegration.Sample/README.md` and
`../docs/1.13.0-PERSISTENT-RASTER-RUNTIME-EVIDENCE-GUIDE.md`.

## Icod.TermInfo.PersistentRasterPlacement.Sample

The **1.12** `Icod.TermInfo.PersistentRasterPlacement.Sample` demonstrates
`SourceRectangle` and `SignedZOrder` support planning. It uses the frozen 1.13
runtime observation/integration API to strengthen placement evidence and then
replans through the frozen placement planner. Concrete rectangle coordinates and
signed z-index values remain downstream execution values.

```text
dotnet run --project samples/Icod.TermInfo.PersistentRasterPlacement.Sample/Icod.TermInfo.PersistentRasterPlacement.Sample.csproj -f net10.0
```

See `Icod.TermInfo.PersistentRasterPlacement.Sample/README.md` and
`../docs/1.12.0-ADVANCED-PERSISTENT-RASTER-PLACEMENT-GUIDE.md`.

## Icod.TermInfo.PersistentRasterLifecycle.Sample

The **1.11** `Icod.TermInfo.PersistentRasterLifecycle.Sample` demonstrates
protocol-neutral persistent-raster lifecycle evidence and planning. It performs
no terminal I/O and has no `Icod.Terminal` dependency. The current sample uses
the 1.13 runtime-integration path for caller-owned verification evidence while
retaining the frozen 1.11 planner semantics.

```text
dotnet run --project samples/Icod.TermInfo.PersistentRasterLifecycle.Sample/Icod.TermInfo.PersistentRasterLifecycle.Sample.csproj -f net10.0
```

See `Icod.TermInfo.PersistentRasterLifecycle.Sample/README.md` and
`../docs/1.11.0-PERSISTENT-RASTER-LIFECYCLE-GUIDE.md`.

## Icod.TermInfo.DatabaseSet.Sample

`Icod.TermInfo.DatabaseSet.Sample` is the focused **1.10** reusable-library
example. It creates controlled compiled databases through the public Compiler API
and exercises:

- ordered `InspectSet(...)` construction;
- canonical lookup and first-root precedence;
- conflicting shadow classification;
- alias-collision evidence;
- structural/effective set comparison;
- conflict-free multi-database parent planning; and
- `databaseSet`, `databaseSetComparison`, and `databaseSetPlan` JSON.

```text
dotnet run --project samples/Icod.TermInfo.DatabaseSet.Sample/Icod.TermInfo.DatabaseSet.Sample.csproj -f net10.0
```

See `Icod.TermInfo.DatabaseSet.Sample/README.md` and
`../docs/1.10.0-MULTI-DATABASE-GUIDE.md`.

## Icod.TermInfo.Toolchain.Sample

`Icod.TermInfo.Toolchain.Sample` demonstrates the reusable Source -> Compiler ->
Runtime -> Inspection flow without invoking command parsing. It parses and
resolves controlled `.ti` source, performs deterministic parent planning and
relative synthesis, compiles and publishes into a temporary conventional
database, reloads the result through Runtime, verifies semantic equality through
Inspection, and renders the immutable source plan as JSON.

```text
dotnet run --project samples/Icod.TermInfo.Toolchain.Sample/Icod.TermInfo.Toolchain.Sample.csproj -f net10.0
```

See `Icod.TermInfo.Toolchain.Sample/README.md`.

## Icod.TermInfo.Acquisition.Sample

`Icod.TermInfo.Acquisition.Sample` is the focused compiled-database acquisition
sample introduced in 0.9. It never emits terminal-control strings.

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

## Icod.TermInfo.Sample

`Icod.TermInfo.Sample` is the general Runtime API demonstration. It covers
terminal profiles, explicit environment resolution, standard and extended
capabilities, parameter expansion, color, padding, terminal size, custom
providers, and optional Windows virtual-terminal output enablement.

Use `--describe-only` when no terminal-control strings should be emitted:

```text
dotnet run --project samples/Icod.TermInfo.Sample/Icod.TermInfo.Sample.csproj -f net10.0 -- --describe-only --profile xterm-256color
```

See `Icod.TermInfo.Sample/README.md`.

## ToolSuite

`samples/ToolSuite` is the data-and-command walkthrough for the coordinated five
command applications:

```text
tic
infocmp
toe
captoinfo
infotocap
```

It uses controlled terminfo and termcap source plus an explicit local database
root. The walkthrough covers validation, publication, rendering, comparison,
relative synthesis, explicit parent planning, enumeration, forward/reverse
`use=` dependency reporting, bidirectional termcap conversion, and the frozen
command JSON automation paths without depending on host-installed databases.

See `ToolSuite/README.md`.

## Release validation

The release pipeline executes the deterministic reusable samples and package-only
consumers on their supported target frameworks. The 1.14 qualification adds the
raster-backend selection sample and a package-reference-only consumer that uses a
freshly packed `Icod.TermInfo.Inspection` candidate beside published
`Icod.Terminal 1.13.0`.

Live verification is never required by CI. Interactive `--live` modes exist only
to demonstrate the caller/sibling-layer boundary.
