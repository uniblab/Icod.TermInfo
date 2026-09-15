# Icod.TermInfo.Termcap.Sample

This sample demonstrates the reusable managed `Icod.TermInfo.Termcap` API without invoking `captoinfo` or `infotocap` command parsing and without consulting ambient host termcap state.

The deterministic flow is:

```text
controlled termcap source
    -> TermcapSourceParser
    -> TermcapCapabilityClassifier
    -> TermcapSourceResolver (tc= inheritance)
    -> TermcapConverter
    -> immutable Runtime TerminalDescription
    -> TermcapRenderer

controlled inline source
    -> TermcapAcquirer
    -> immutable Runtime TerminalDescription
```

The source contains a base entry with `am`, `co#80`, `li#24`, and `cl`, plus a child that overrides columns with `co#132`, adds cursor addressing, and inherits the base through `tc=`. The sample verifies that parsing, classification, inheritance, conversion, reverse rendering, and explicit inline acquisition preserve the expected semantics.

The project references only `Icod.TermInfo.Termcap`; `Icod.TermInfo` Runtime arrives through the Termcap package's normal transitive dependency. It does not depend on Source, Compiler, Inspection, Terminal, or the command projects.

Run it on any supported reusable target framework:

```text
dotnet run --project samples/Icod.TermInfo.Termcap.Sample/Icod.TermInfo.Termcap.Sample.csproj -f net8.0
dotnet run --project samples/Icod.TermInfo.Termcap.Sample/Icod.TermInfo.Termcap.Sample.csproj -f net9.0
dotnet run --project samples/Icod.TermInfo.Termcap.Sample/Icod.TermInfo.Termcap.Sample.csproj -f net10.0
```

The sample is deterministic and safe for CI. It does not read `TERMCAP`, `TERMPATH`, host files, or the system terminfo database.

For command-level conversion examples, see `../ToolSuite/README.md`. For the reusable package contract, see `../../Icod.TermInfo.Termcap/README.md`.
