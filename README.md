# Icod.TermInfo

`Icod.TermInfo` is a managed, dependency-free .NET implementation of the low-level terminal-capability model traditionally supplied by `libtinfo`.

Version 0.7.0 expands the deliberately small 0.6.0 ANSI/VT100 contract into a modern terminal-description library while preserving the same capability-driven architecture: immutable descriptions, generic parameter expansion, padding-aware output, conservative terminal resolution, and no process-global current terminal.

The package targets `net10.0`, uses C# 13, contains no native ncurses/terminfo payload, and is intended to run on Windows, Linux, and macOS.

## Install

During 0.7 development, use the prerelease version that matches the branch/package you are testing:

```text
dotnet add package Icod.TermInfo --version 0.7.0-alpha.11
```

After the final `v0.7.0` release is published:

```text
dotnet add package Icod.TermInfo --version 0.7.0
```

For repository development, reference `Icod.TermInfo.csproj` directly as the sample project does.

## What 0.7.0 provides

- immutable terminal descriptions and deterministic provider composition;
- typed standard capability lookup plus generic extended Boolean, numeric, and string capabilities;
- a stack-oriented terminfo parameter-expansion engine shared by every built-in profile;
- padding-aware `tputs`/`putp`-style output;
- conservative `TERM` resolution with explicit caller-selected fallback behavior;
- built-in `dumb`, `ansi`, `vt100`/`vt100-am`, `vt102`, and `vt220`/`vt200` profiles;
- built-in modern `xterm`, `xterm-16color`, `xterm-88color`, and `xterm-256color` profiles;
- built-in `xterm-direct`, `xterm-direct16`, and `xterm-direct256` true-color profiles;
- semantic monochrome, indexed-color, and direct-RGB inspection;
- safe indexed and direct-RGB foreground/background expansion helpers;
- full-screen/cursor-addressing and cursor-visibility primitives where a profile advertises them;
- descriptive xterm mouse, focus, bracketed-paste, modified-key, cursor-style, reporting, and clipboard metadata;
- live terminal-size queries on Windows, Linux, and macOS, kept distinct from environment/profile defaults;
- explicit and reversible Windows virtual-terminal output enablement.

The 0.6.0 behavior of `dumb`, `ansi`, and `vt100` remains intentionally conservative: `dumb` is minimal, `ansi` is the traditional eight-color profile, and `vt100` remains monochrome.

## Getting started

Terminal resolution is intentionally conservative. Unknown `TERM` values do not silently become ANSI, VT100, or xterm:

```csharp
using Icod.TermInfo;

TerminalDescription terminal =
    TerminalEnvironment.Resolve(
        TerminalDatabase.BuiltIn,
        TerminalProfiles.Dumb);

Console.WriteLine($"Terminal profile: {terminal.Name}");
```

To select a known modern profile explicitly:

```csharp
TerminalDescription xterm =
    TerminalDatabase.BuiltIn.Load("xterm");

TerminalDescription xterm256 =
    TerminalDatabase.BuiltIn.Load("xterm-256color");

TerminalDescription xtermDirect =
    TerminalDatabase.BuiltIn.Load("xterm-direct256");
```

Aliases remain exact and intentional. For example, `vt100-am` resolves to `vt100`, and `vt200` resolves to `vt220`.

## Standard and extended capabilities

Typed lookup is the preferred API for standard capabilities:

```csharp
bool automaticMargins =
    terminal.GetBoolean(BooleanCapability.AutoRightMargin);

int? columns =
    terminal.GetNumber(NumericCapability.Columns);

string? clear =
    terminal.GetString(StringCapability.ClearScreen);
```

Traditional short-name lookup remains available:

```csharp
bool hasColors = terminal.TryGetNumber("colors", out int colors);
bool hasClear = terminal.TryGetString("clear", out string? clear);
```

Modern capabilities which are not part of the fixed standard terminfo vocabulary are carried through the extended-capability store:

```csharp
if (xterm.TryGetExtendedString("BE", out string? enablePaste))
{
    Console.WriteLine("Bracketed-paste enable metadata is present.");
}

if (xterm.TryGetExtendedString("XM", out string? mouseMode))
{
    string enableMouse =
        TermInfoParameterExpander.Expand(mouseMode, 1);
}
```

Extended names are case-sensitive. Standard capability names cannot be silently shadowed by extended capabilities.

## Color inspection

Color semantics are derived from raw terminfo data rather than from terminal-name checks:

```csharp
TerminalColorSupport support =
    TerminalColors.GetColorSupport(xterm256);

Console.WriteLine(support.Model);             // Indexed
Console.WriteLine(support.Tier);              // Color256
Console.WriteLine(support.IndexedColorCount); // 256
```

Raw `colors`, `pairs`, `ncv`, selectors, `bce`, `ccc`, `hls`, `initc`, `op`, `oc`, and extended `RGB`/`CO` metadata remain authoritative. `pairs` is never synthesized from `colors`.

### Indexed color

Use the semantic helper rather than embedding ANSI escape strings:

```csharp
string foreground =
    TerminalColors.ExpandForeground(
        TerminalProfiles.Xterm256Color,
        196);

TermInfoOutput.PutP(foreground, Console.Out);
```

The helper validates the terminal's advertised indexed range and expands the terminal's own `setaf` capability through the shared parameter engine.

### Direct RGB color

Direct profiles expose an RGB layout and any retained indexed prefix:

```csharp
TerminalDescription direct =
    TerminalProfiles.XtermDirect256;

TerminalColorSupport support =
    TerminalColors.GetColorSupport(direct);

TerminalRgbColor purple =
    new(0x80, 0x40, 0xC0);

string foreground =
    TerminalColors.ExpandForeground(
        direct,
        purple);
```

The selected xterm direct profiles use packed 8/8/8 RGB semantics and retain 8, 16, or 256 indexed entries according to their `CO` metadata. The library validates collisions between packed RGB values and that retained indexed prefix instead of guessing.

## Cursor positioning and full-screen primitives

Parameterized standard capabilities use the same terminfo expansion engine:

```csharp
string move =
    xterm.Expand(
        StringCapability.CursorAddress,
        10,
        20);
```

Profiles can also advertise cursor-addressing lifecycle and cursor-visibility primitives:

```csharp
string? enter =
    xterm.GetString(StringCapability.EnterCursorAddressingMode);
string? leave =
    xterm.GetString(StringCapability.ExitCursorAddressingMode);
string? hideCursor =
    xterm.GetString(StringCapability.CursorInvisible);
string? normalCursor =
    xterm.GetString(StringCapability.CursorNormal);
```

These are capability strings, not a session manager. `Icod.TermInfo` does not decide when to enter full-screen mode, hide the cursor, recover from exceptions, or restore terminal state. A caller or future higher-level terminal library owns that lifecycle.

## Mouse, focus, paste, and clipboard metadata

The modern xterm profiles carry descriptive protocol metadata such as:

- standard `kmous` plus extended `XM`/`xm` mouse strings;
- focus enable/disable and focus-in/focus-out strings;
- bracketed-paste enable/disable and begin/end strings;
- modified-key strings;
- cursor-style and terminal-reporting strings;
- OSC 52 clipboard/selection metadata where present in the selected profile.

This package does **not** decode mouse events, focus events, keys, or paste payloads. It also does not perform clipboard operations or terminal probing. The metadata is intentionally available so a future `Icod.Terminal`-style layer can consume it without teaching `Icod.TermInfo` about live input state.

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

The output API also supports asynchronous `TextWriter` output, byte streams with a caller-selected encoding, character callbacks, and an injectable `ITermInfoDelayProvider`.

## Compatibility-shaped API

`TermInfoCompatibility` provides familiar terminfo operation names while retaining managed semantics and explicit terminal ownership:

```csharp
bool am =
    TermInfoCompatibility.TiGetFlag(xterm, "am");

int? colorCount =
    TermInfoCompatibility.TiGetNum(xterm, "colors");

string? cup =
    TermInfoCompatibility.TiGetStr(xterm, "cup");
```

There is no process-global `cur_term`, no sentinel-pointer result, and no hidden persistent expansion state. Persistent uppercase `%P/%g` variables require an explicit caller-owned `TermInfoExpansionContext`.

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

A failed live query never substitutes `COLUMNS`/`LINES` or a profile default. Fallback order belongs to the caller.

## Windows virtual-terminal output

Windows VT output mode is always opt-in:

```csharp
using IDisposable? mode =
    WindowsVirtualTerminal.TryEnableOutput();
```

The helper returns `null` on non-Windows systems, redirected output, non-console handles, or when Windows refuses the mode change. When it changes console mode, disposing the returned lease restores the exact previous mode. Loading a terminal profile never changes console state.

This helper is not a Windows Console terminal profile. Explicit Windows Console and Windows Terminal profile work is reserved for 0.8.0.

## Custom terminal providers

Applications can add descriptions without changing the built-in database or generic engines:

```csharp
TerminalDescription example =
    new TerminalDescriptionBuilder("example-terminal")
        .SetBoolean(BooleanCapability.AutoRightMargin)
        .SetNumber(NumericCapability.Columns, 80)
        .SetNumber(NumericCapability.Lines, 24)
        .SetExtendedBoolean("exampleFlag")
        .SetExtendedString("exampleString", "value")
        .Build();

ITerminalDescriptionProvider provider =
    new InMemoryTerminalDescriptionProvider(
        new[] { example });

TerminalDatabase database =
    new(new[] { provider });
```

Provider ordering is explicit and deterministic; the first provider that resolves a name wins.

## Sample application

`samples/Icod.TermInfo.Sample` demonstrates:

- conservative environment resolution with an explicit `dumb` fallback;
- semantic indexed/direct color inspection and expansion;
- extended-capability discovery;
- full-screen/cursor-visibility capability discovery without taking ownership of a full-screen session;
- live/configured/profile size selection;
- redirection handling and explicit Windows VT enablement;
- a custom provider implementation.

Run the ordinary demonstration with:

```text
dotnet run --project samples/Icod.TermInfo.Sample/Icod.TermInfo.Sample.csproj
```

For CI, documentation checks, or any environment where terminal-control output is inappropriate, use the non-interactive descriptive mode:

```text
dotnet run --project samples/Icod.TermInfo.Sample/Icod.TermInfo.Sample.csproj -- --describe-only --profile xterm-direct256
```

`--profile <name>` selects an exact built-in profile instead of consulting `TERM`. `--describe-only` exercises profile/color/extended-capability APIs but emits no terminal-control strings.

## Project-family boundary

`Icod.TermInfo` owns terminal-description data and pure transformations of that data. It does not own a live terminal session.

A future `Icod.Terminal`-style layer may own raw/cooked mode changes, input decoding, keyboard/mouse/paste/focus events, probing, full-screen lifecycle, cursor lifecycle, clipboard operations, and progress helpers. A future curses-style library may own virtual-screen state, windows, pads, panels, menus, forms, and refresh optimization. PTY creation/process plumbing belongs elsewhere as well.

## 0.8.0 reservation

Version 0.8.0 is reserved for three major additions:

1. classic Windows Console support modeled honestly rather than aliased to ANSI/xterm;
2. explicit Windows Terminal profiles/support;
3. arbitrary/system compiled terminfo database loading, including discovery/provider-precedence rules.

Version 0.7.0 deliberately contains no `/usr/share/terminfo` loader, `TERMINFO`/`TERMINFO_DIRS` discovery, or host-database-dependent profile selection.

## Build, test, and pack

```text
dotnet restore Icod.TermInfo.sln

dotnet build Icod.TermInfo.sln -c Debug
dotnet test Icod.TermInfo.sln -c Debug

dotnet build Icod.TermInfo.sln -c Release
dotnet test Icod.TermInfo.sln -c Release

dotnet pack Icod.TermInfo.csproj -c Release --output artifacts
bash .github/scripts/verify-release-package.sh artifacts
```

The package verifier checks the `.nupkg`/`.snupkg` structure, dependency closure, Source Link metadata, and a fresh `net10.0` consumer restored from only the local package directory. T19 also runs the sample's `--describe-only` mode as a non-interactive consumer check.

GitHub pull requests and pushes to `main` validate both Debug and Release on Windows, Linux, and macOS. The `main` workflow additionally packs and verifies the package and uploads the package artifacts; it does not publish them automatically.

See `docs/RELEASING.md` for the current release procedure and `docs/0.7.0-CONTRACT-AUDIT.md` for the T19 pre-completion audit. T20 remains the final 0.7.0 release gate.

## Scope

`Icod.TermInfo` is not curses, a terminal emulator, a PTY implementation, a termios session manager, an input-event parser, or a general terminal UI toolkit. It intentionally carries low-level descriptive data which those higher-level systems may consume.

See `Icod.TermInfo-Development-Roadmap-0.7.0.md` for the complete 0.7.0 contract.

## License

Licensed under the GNU Lesser General Public License v3.0 or later. See `LICENSE`.
