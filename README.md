# Icod.TermInfo

`Icod.TermInfo` is a managed, dependency-free .NET implementation of the low-level terminal-capability model traditionally supplied by `libtinfo`.

The 0.6.0 contract is intentionally narrow: ANSI/ECMA-48, DEC VT100, and a safe `dumb` profile. Broader terminal families can be added later through the provider architecture without changing the generic capability, parameter-expansion, padding, or output engines.

The package targets `net10.0`, uses C# 13, contains no native ncurses/terminfo payload, and is intended to run on Windows, Linux, and macOS.

## Install

NuGet.org (after the `v0.6.0` release is published):

```text
dotnet add package Icod.TermInfo --version 0.6.0
```

GitHub Packages uses the same package ID and package contents. Configure the `uniblab` GitHub NuGet feed and authenticate according to your GitHub Packages policy, then install `Icod.TermInfo` normally.

For repository development, reference `Icod.TermInfo.csproj` directly as the sample project does.

## What 0.6.0 provides

- immutable terminal descriptions;
- typed and traditional short-name capability lookup;
- `TerminalDescriptionBuilder` and pluggable terminal-description providers;
- built-in `ansi`, `vt100`/`vt100-am`, and `dumb` profiles;
- classic eight-color ANSI capabilities;
- DEC VT100 advanced-video, alternate-character-set, keypad, cursor-key, PF-key, scrolling, and historical padding capabilities;
- a real stack-oriented terminfo parameter-expansion engine;
- padding-aware `tputs`/`putp`-style output;
- managed `tigetflag`, `tigetnum`, `tigetstr`, `tparm`/`tiparm`, `tputs`, and `putp`-shaped compatibility operations;
- conservative `TERM` resolution and explicit fallback behavior;
- redirection inspection;
- live terminal-size queries on Windows, Linux, and macOS;
- explicit environment/profile size fallbacks;
- explicit and reversible Windows virtual-terminal output enablement.

## Getting started

Terminal resolution is intentionally conservative. An unknown `TERM` value never silently becomes ANSI or VT100, so applications should choose their fallback explicitly:

```csharp
using Icod.TermInfo;

TerminalDescription terminal =
    TerminalEnvironment.Resolve(
        TerminalDatabase.BuiltIn,
        TerminalProfiles.Dumb);

Console.WriteLine($"Terminal profile: {terminal.Name}");
```

To require a specific built-in profile instead:

```csharp
TerminalDescription ansi = TerminalDatabase.BuiltIn.Load("ansi");
TerminalDescription vt100 = TerminalDatabase.BuiltIn.Load("vt100");
```

The historical `vt100-am` alias resolves to the same immutable VT100 description.

## Capabilities

Typed lookup is the normal managed API:

```csharp
bool automaticMargins =
    terminal.GetBoolean(BooleanCapability.AutoRightMargin);

int? columns =
    terminal.GetNumber(NumericCapability.Columns);

string? clear =
    terminal.GetString(StringCapability.ClearScreen);
```

Traditional short-name lookup is also available:

```csharp
bool hasColors = terminal.TryGetNumber("colors", out int colors);
bool hasClear = terminal.TryGetString("clear", out string? clear);
```

A recognized but absent capability returns the managed absent result appropriate to its type. An unknown capability short name is rejected rather than silently treated as absent.

## Cursor positioning and clearing

Parameterized capabilities use the shared terminfo expansion engine:

```csharp
TerminalDescription ansi = TerminalProfiles.Ansi;

string clear =
    ansi.GetRequiredString(StringCapability.ClearScreen);

string move =
    ansi.Expand(
        StringCapability.CursorAddress,
        10,
        20);

TermInfoOutput.PutP(clear, Console.Out);
TermInfoOutput.PutP(move, Console.Out);
```

The cursor coordinates passed to `Expand` are zero-based terminfo parameters. The ANSI/VT100 `cup` capability performs the required `%i` adjustment itself.

## ANSI attributes and color

The built-in ANSI profile deliberately stops at the traditional eight colors:

```csharp
TerminalDescription ansi = TerminalProfiles.Ansi;

string red =
    ansi.Expand(StringCapability.SetForegroundColor, 1);

string bold =
    ansi.GetRequiredString(StringCapability.EnterBoldMode);

string normal =
    ansi.GetRequiredString(StringCapability.ExitAttributeMode);

TermInfoOutput.PutP(red, Console.Out);
TermInfoOutput.PutP(bold, Console.Out);
Console.Write("important");
TermInfoOutput.PutP(normal, Console.Out);
```

The 0.6.0 contract does not advertise 16-color, 256-color, or true-color extensions.

## VT100 and padding

VT100 strings preserve their historical terminfo padding annotations through parameter expansion:

```csharp
TerminalDescription vt100 = TerminalProfiles.Vt100;

string move =
    vt100.Expand(
        StringCapability.CursorAddress,
        10,
        20);

// move contains ESC[11;21H$<5>
```

Applications should emit capability strings through the output layer. Modern terminals normally use the default `PaddingMode.Ignore`, which removes delay annotations without writing them literally:

```csharp
TermInfoOutput.TPuts(
    move,
    affectedLines: 1,
    Console.Out);
```

Physical or serial terminals can opt into delays:

```csharp
TermInfoOutput.TPuts(
    move,
    affectedLines: 1,
    Console.Out,
    PaddingMode.Delay);
```

The output API also supports asynchronous `TextWriter` output, byte streams with a caller-selected encoding, character callbacks, and an injectable `ITermInfoDelayProvider` for deterministic applications and tests.

## Compatibility-shaped API

`TermInfoCompatibility` provides familiar terminfo operation names while retaining managed semantics and explicit terminal ownership:

```csharp
bool am =
    TermInfoCompatibility.TiGetFlag(ansi, "am");

int? colorCount =
    TermInfoCompatibility.TiGetNum(ansi, "colors");

string? cup =
    TermInfoCompatibility.TiGetStr(ansi, "cup");

string expanded =
    TermInfoCompatibility.TParm(
        "\x1b[%i%p1%d;%p2%dH",
        4,
        12);
```

There is no process-global `cur_term`, no sentinel pointer result, and no hidden persistent expansion state. Persistent uppercase `%P/%g` variables require an explicit caller-owned `TermInfoExpansionContext`.

## Terminal size

Live dimensions are distinct from configured and profile-default dimensions:

```csharp
TerminalSize size;

if (TerminalEnvironment.TryGetLiveSize(out size))
{
    Console.WriteLine($"Live: {size.Columns}x{size.Rows}");
}
else if (TerminalEnvironment.TryGetEnvironmentSize(out size))
{
    Console.WriteLine($"Configured: {size.Columns}x{size.Rows}");
}
else if (TerminalEnvironment.TryGetProfileSize(terminal, out size))
{
    Console.WriteLine($"Profile default: {size.Columns}x{size.Rows}");
}
```

A failed live query never substitutes `COLUMNS`/`LINES` or a profile's default dimensions. The fallback order belongs to the caller.

## Windows virtual-terminal output

Windows VT mode is always opt-in:

```csharp
using IDisposable? mode =
    WindowsVirtualTerminal.TryEnableOutput();

if (mode is not null)
{
    TermInfoOutput.PutP(
        TerminalProfiles.Ansi.GetRequiredString(
            StringCapability.ClearScreen),
        Console.Out);
}
```

The helper returns `null` on non-Windows systems, redirected output, non-console handles, or when Windows refuses the mode change. When it changes console mode, disposing the returned lease restores the exact previous mode. Merely loading a terminal profile never changes console state.

## Custom terminal providers

Applications can add terminal descriptions without changing the built-in database or generic engines:

```csharp
TerminalDescription example =
    new TerminalDescriptionBuilder("example-terminal")
        .SetBoolean(BooleanCapability.AutoRightMargin)
        .SetNumber(NumericCapability.Columns, 80)
        .SetNumber(NumericCapability.Lines, 24)
        .SetString(
            StringCapability.CursorAddress,
            "\x1b[%i%p1%d;%p2%dH")
        .Build();

ITerminalDescriptionProvider provider =
    new InMemoryTerminalDescriptionProvider(
        new[] { example });

TerminalDatabase database =
    new(new[] { provider });
```

For larger integrations, implement `ITerminalDescriptionProvider` directly. Provider ordering is explicit and deterministic; the first provider that resolves a name wins.

## Sample application

`samples/Icod.TermInfo.Sample` demonstrates:

- conservative environment resolution with an explicit `dumb` fallback;
- live/configured/profile size selection;
- redirection handling;
- explicit Windows VT enablement;
- clearing, cursor movement, attributes, and ANSI color when the selected profile supports them;
- a custom provider implementation.

Run it from the repository with:

```text
dotnet run --project samples/Icod.TermInfo.Sample/Icod.TermInfo.Sample.csproj
```

## Build, test, and pack

```text
dotnet restore Icod.TermInfo.sln

dotnet build Icod.TermInfo.sln -c Debug
dotnet test Icod.TermInfo.sln -c Debug

dotnet build Icod.TermInfo.sln -c Release
dotnet test Icod.TermInfo.sln -c Release

dotnet pack Icod.TermInfo.csproj -c Release --output artifacts
```

Release packages produce both `.nupkg` and `.snupkg` artifacts. The .NET SDK supplies Source Link support, and GitHub Actions builds set `ContinuousIntegrationBuild` so repository/commit information and deterministic source mapping are emitted for package debugging. The T10 package job also inspects the packed artifacts and installs the package into a fresh `net10.0` application using only the local artifact directory as a NuGet source.

## Publishing

The repository has two publication paths:

- `publish-github-packages.yml` is a manually invoked GitHub Packages workflow;
- `release.yml` is tag-driven and validates on Windows, Linux, and macOS, packs once, publishes the same `.nupkg` to GitHub Packages and NuGet.org, and creates a GitHub Release containing the package and symbol package.

See `docs/RELEASING.md` for the one-time NuGet trusted-publishing setup and release procedure, and `docs/0.6.0-CONTRACT-AUDIT.md` for the final T10 contract evidence.

## Scope

`Icod.TermInfo` is not curses, a terminal emulator, a PTY implementation, a termios wrapper, a keyboard event parser, or a general terminal UI toolkit. Version 0.6.0 also deliberately excludes loading the host operating system's compiled terminfo database and modern terminal-family aliases such as `xterm-256color`, `screen`, `tmux`, or `linux`.

See `Icod.TermInfo-Development-Roadmap.md` for the complete 0.6.0 contract.

## License

Licensed under the GNU Lesser General Public License v3.0 or later. See `LICENSE`.
