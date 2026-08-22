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
- historical VT100 terminfo padding annotations preserved for the output layer;
- a reusable terminfo parameter-expansion engine, including stack operations, formatting, variables, `%i`, and conditionals.

The next milestone is the padding-aware output layer. Until then, VT100 strings retain their `$<...>` annotations so no timing information is lost.

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

The `$<5>` suffix is a terminfo padding annotation. T4 deliberately preserves
it; the T5 output layer is responsible for removing or honoring padding.

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
