# Icod.TermInfo.Toolchain.Sample

This sample demonstrates the reusable managed toolchain without invoking
`tic`, `infocmp`, `toe`, or the `icod-terminfo` router.

It composes:

```text
Icod.TermInfo.Source
    parse + resolve .ti source
        |
        v
Icod.TermInfo.Inspection
    plan and synthesize child relative to candidates
        |
        v
Icod.TermInfo.Source
    reparse + resolve synthesized source
        |
        v
Icod.TermInfo.Compiler
    compile + publish a temporary conventional database
        |
        v
Icod.TermInfo
    acquire the compiled child entry
        |
        v
Icod.TermInfo.Inspection
    compare acquired state with the original target
```

The source contains a useful base, a decoy candidate, and a child using `use=`
inheritance. The sample resolves all three entries, asks the 1.8 planner to
select zero or one parent, requires the useful base to win an exhaustive search,
and consumes the planner's exact source. It then reparses and resolves that
source, compiles the planned form, publishes it into a unique temporary
database, reloads the child through `DirectoryTerminalDescriptionProvider`, and
requires each stage to remain semantically equal to the original resolved
child.

The sample retains the explicit five-argument
`TerminalDescriptionSourceSynthesisOptions` constructor which is part of the
frozen `1.7.0` Inspection API baseline, then composes it through
`TerminalDescriptionSourcePlanningOptions` and
`TerminalDescriptionSourcePlanner.Plan`.

Run it with:

```text
dotnet run --project samples/Icod.TermInfo.Toolchain.Sample/Icod.TermInfo.Toolchain.Sample.csproj -f net10.0
```

The project also targets `net8.0` and `net9.0`.

The sample is deterministic:

- it does not inspect `TERMINFO`, `TERMINFO_DIRS`, or the host system database;
- it does not invoke native ncurses tools;
- it writes only beneath a unique temporary directory;
- it deletes that directory before exit;
- it produces identical output across repeated process executions;
- release validation executes the `net10.0` path on Windows, Linux, and macOS.

The planned child entry is written to standard output only after the complete
plan -> synthesize -> compile -> publish -> reacquire -> compare path succeeds.
