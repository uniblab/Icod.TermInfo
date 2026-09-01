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
    synthesize child relative to base
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

The source contains a base entry and a child using `use=` inheritance. The
sample resolves both entries, synthesizes deterministic child source relative to
the base through the frozen 1.7 Inspection API, reparses and resolves that source,
compiles the synthesized form, publishes it into a unique temporary database,
reloads the child through `DirectoryTerminalDescriptionProvider`, and requires
each stage to remain semantically equal to the original resolved child.

`1.7.0-Alpha-8` also exercises the explicit five-argument
`TerminalDescriptionSourceSynthesisOptions` constructor which is part of the
frozen `1.7.0` Inspection API baseline.

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
- release validation executes the `net10.0` path on Windows, Linux, and macOS.

The rendered child entry is written to standard output only after the complete
source -> compile -> acquire -> compare path succeeds.
