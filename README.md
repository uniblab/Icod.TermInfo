# Icod.TermInfo

`Icod.TermInfo` is a managed, dependency-free .NET implementation of the low-level terminal-capability model traditionally supplied by `libtinfo`.

The 0.6.0 contract is intentionally narrow: ANSI/ECMA-48, DEC VT100, and a safe `dumb` profile. Broader terminal families can be added later through the provider architecture without changing the generic capability or parameter-expansion engines.

## Current development state

The current prerelease foundation provides:

- immutable terminal descriptions;
- typed and traditional short-name capability lookup;
- a terminal-description builder;
- ordered terminal-description providers;
- built-in `ansi`, `vt100`, and `dumb` terminal profiles;
- classic eight-color ANSI foreground/background capabilities;
- ANSI cursor movement, screen editing, tabs, rendition controls, and cursor-key strings;
- DEC VT100 advanced-video, scroll-region, alternate-character-set, keypad, cursor-key, and PF-key capabilities;
- a reusable terminfo parameter-expansion engine, including stack operations, formatting, variables, `%i`, and conditionals;
- padding-aware `tputs`/`putp`-style output with ignore and delay modes;
- synchronous and asynchronous `TextWriter` output;
- byte-stream output with caller-selected encoding;
- character-callback output;
- injectable delay handling for physical or serial terminals;
- conservative `TERM` resolution and standard-stream redirection inspection;
- live terminal-size queries on Windows, Linux, and macOS;
- explicit `COLUMNS`/`LINES` and profile-dimension fallback APIs;
- explicit, reversible Windows virtual-terminal output enablement.

## Requirements

- .NET 10 SDK
- C# 13

The library targets `net10.0` and is intended to run on Windows, Linux, and macOS.

## Build and test

```text
dotnet restore Icod.TermInfo.sln
dotnet build Icod.TermInfo.sln -c Debug
dotnet test Icod.TermInfo.sln -c Debug
dotnet build Icod.TermInfo.sln -c Release
dotnet test Icod.TermInfo.sln -c Release
```

Create a package with:

```text
dotnet pack Icod.TermInfo.csproj -c Release
```

## Basic capability lookup

```csharp
using Icod.TermInfo;

TerminalDescription terminal = TerminalDatabase.BuiltIn.Load("ansi");

bool automaticMargins =
    terminal.GetBoolean(BooleanCapability.AutoRightMargin);

int? columns =
    terminal.GetNumber(NumericCapability.Columns);

string moveToHome =
    terminal.Expand(StringCapability.CursorAddress, 0, 0);

string redForeground =
    terminal.Expand(StringCapability.SetForegroundColor, 1);
```

The DEC VT100 profile is also available by canonical name or its historical
`vt100-am` alias:

```csharp
TerminalDescription vt100 = TerminalDatabase.BuiltIn.Load("vt100");

string move =
    vt100.Expand(StringCapability.CursorAddress, 10, 20);
// ESC[11;21H$<5>
```

The `$<5>` suffix is a terminfo padding annotation. Parameter expansion preserves
it so the output layer can remove or honor the delay.

## Padding-aware output

Modern terminal emulators normally do not need historical hardware delays.
`PaddingMode.Ignore` is therefore the default and removes padding annotations
without writing them:

```csharp
using StringWriter writer = new();

string move =
    vt100.Expand(StringCapability.CursorAddress, 10, 20);

TermInfoOutput.TPuts(
    move,
    affectedLines: 1,
    writer);

// writer contains ESC[11;21H
```

A physical or serial terminal can opt into real delays:

```csharp
TermInfoOutput.TPuts(
    move,
    affectedLines: 1,
    writer,
    PaddingMode.Delay);
```

Padding directives with `*` are multiplied by the supplied affected-line count.
The `/` suffix is retained as `TermInfoDelay.IsMandatory` when an
`ITermInfoDelayProvider` is used. The library also provides asynchronous,
byte-stream, and character-callback output overloads.

`TermInfoOutput.PutP` is a convenience form equivalent to `TPuts` with one
affected line.

## Terminal environment and size

`TerminalEnvironment` reads `TERM` conservatively. Only names actually present in
the configured `TerminalDatabase` resolve; an unknown name is never silently
treated as ANSI or VT100:

```csharp
if (TerminalEnvironment.TryResolve(
        TerminalDatabase.BuiltIn,
        out TerminalDescription? current))
{
    // current is ansi, vt100/vt100-am, or dumb in the built-in database.
}

TerminalDescription withFallback =
    TerminalEnvironment.Resolve(
        TerminalDatabase.BuiltIn,
        TerminalProfiles.Dumb);
```

Standard-stream redirection is exposed explicitly:

```csharp
bool redirected = TerminalEnvironment.IsOutputRedirected;
```

Live dimensions are separate from configured and profile defaults:

```csharp
if (TerminalEnvironment.TryGetLiveSize(out TerminalSize live))
{
    // live came from the operating system.
}
else if (TerminalEnvironment.TryGetEnvironmentSize(out TerminalSize configured))
{
    // configured came from positive COLUMNS and LINES values.
}
else if (TerminalEnvironment.TryGetProfileSize(vt100, out TerminalSize profile))
{
    // profile is the terminfo definition default, such as 80x24.
}
```

A failed live query never substitutes `COLUMNS`/`LINES` or profile dimensions.
That fallback order remains an explicit caller decision.

## Windows virtual-terminal output

On Windows, applications can explicitly enable console virtual-terminal output
processing without changing unrelated console-mode flags:

```csharp
using IDisposable? mode =
    WindowsVirtualTerminal.TryEnableOutput();

if (mode is not null)
{
    // ANSI/VT control sequences written to stdout are processed by the console.
}
```

Disposing the returned lease restores the mode that was present before the
library changed it. If virtual-terminal processing was already enabled, disposing
the lease leaves the existing mode untouched.

The helper never enables VT mode merely because a terminal profile is loaded.
It returns `null` on non-Windows systems, redirected streams, non-console handles,
or when Windows refuses the mode change. Standard error can be selected
explicitly:

```csharp
using IDisposable? errorMode =
    WindowsVirtualTerminal.TryEnableOutput(
        TerminalStandardStream.Error);
```

Unsupported terminal names do not silently become ANSI, VT100, or `dumb`. A fallback is always an explicit caller decision.

## Parameter expansion

Terminfo parameter strings use a small stack language. They can be expanded directly:

```csharp
string cursorAddress = TermInfoParameterExpander.Expand(
    "\x1b[%i%p1%d;%p2%dH",
    4,
    12);
```

or parsed once and reused:

```csharp
TermInfoParameterProgram program =
    TermInfoParameterProgram.Parse("%p1%{1}%+%d");

string result = program.Expand(41);
```

Lowercase variables (`a-z`) are scoped to one expansion. Uppercase variables (`A-Z`) persist only when the caller explicitly reuses a `TermInfoExpansionContext`; there is no hidden process-global terminfo state.

## Scope

`Icod.TermInfo` is not curses, a terminal emulator, a PTY implementation, or a general terminal UI toolkit. See `Icod.TermInfo-Development-Roadmap.md` for the complete 0.6.0 contract and exclusions.

## License

Licensed under the GNU Lesser General Public License v3.0 or later. See `LICENSE`.
