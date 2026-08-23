# Icod.TermInfo Acquisition Sample

This executable is the focused 0.9 sample for compiled terminfo acquisition. It
is intentionally non-interactive and never emits terminal-control strings.

It demonstrates the public API at five ownership levels:

```text
caller-supplied bytes
explicit directory root
normal system discovery
fully restricted system discovery
system discovery with immutable built-in fallback
```

## Parse a compiled file directly

```text
dotnet run --project samples/Icod.TermInfo.Acquisition.Sample/Icod.TermInfo.Acquisition.Sample.csproj -- parse ./xterm.compiled
```

This path calls `CompiledTermInfoParser.Parse` directly. No filesystem search or
environment discovery occurs beyond reading the file requested by the sample
itself.

## Load one explicit conventional root

```text
dotnet run --project samples/Icod.TermInfo.Acquisition.Sample/Icod.TermInfo.Acquisition.Sample.csproj -- directory /usr/share/terminfo xterm
```

This uses `DirectoryTerminalDescriptionProvider`. The root is caller-owned and
the provider performs conventional exact-name lookup beneath that root only.

## Use normal system discovery

```text
dotnet run --project samples/Icod.TermInfo.Acquisition.Sample/Icod.TermInfo.Acquisition.Sample.csproj -- system xterm-256color
```

This constructs `SystemTerminalDescriptionProvider` with default options. Its
permitted environment/home/platform inputs are snapshotted at construction.

## Demonstrate a fully restricted provider

```text
dotnet run --project samples/Icod.TermInfo.Acquisition.Sample/Icod.TermInfo.Acquisition.Sample.csproj -- restricted xterm
```

The sample disables environment, user, and platform-system sources. A clean miss
is therefore the expected result for a valid terminal name.

## Compose system lookup with built-in fallback

```text
dotnet run --project samples/Icod.TermInfo.Acquisition.Sample/Icod.TermInfo.Acquisition.Sample.csproj -- fallback xterm
```

This constructs:

```csharp
new TerminalDatabase(
    new ITerminalDescriptionProvider[]
    {
        new SystemTerminalDescriptionProvider(),
        TerminalDatabase.BuiltIn,
    });
```

The first provider which resolves the name wins. System acquisition never
mutates `TerminalDatabase.BuiltIn`.

## Output

On success the sample prints:

- acquisition source;
- canonical terminal name;
- description;
- aliases;
- columns, lines, and colors when present;
- counts of effective standard Boolean, numeric, and string capabilities;
- extended capability count.

A clean provider miss exits with code 1. Invalid arguments, malformed entries,
unsupported database storage, and propagated acquisition errors exit with code
2 after printing the exception category and message.

For the complete discovery and error contract, see
`../../docs/0.9.0-ACQUISITION-GUIDE.md`.
