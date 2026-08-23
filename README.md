# Icod.TermInfo

`Icod.TermInfo` is a managed, dependency-free .NET implementation of the low-level terminal-capability model traditionally supplied by `libtinfo`.

Version 0.8.0 is the semantic-completion release: it finishes the in-memory capability model, parameter/runtime safety, exact capability-byte behavior, terminal-aware padding, profile composition/cancellation fidelity, and authoritative Windows Console/Windows Terminal built-ins while preserving conservative resolution and no process-global current terminal.

The package targets `net10.0`, uses C# 13, contains no native ncurses/terminfo payload, and is intended to run on Windows, Linux, and macOS.

## Install

For the 0.8.0 release:

```text
dotnet add package Icod.TermInfo --version 0.8.0
```

The same package contents are intended for NuGet.org and GitHub Packages. Repository development can reference `Icod.TermInfo.csproj` directly, as the sample project does.

## What 0.8.0 provides

- immutable terminal descriptions with canonical name, aliases, and a separate verbose `Description`;
- a complete ncurses/System V-compatible standard capability catalog: 44 Boolean, 39 numeric, and 414 string table positions;
- canonical standard-capability metadata including fixed future binary index, short name, long/variable name, termcap code, and managed enum identity;
- deterministic read-only enumeration of the standard catalog and of effectively present standard capabilities on each description;
- signed 32-bit standard and extended numeric semantics;
- generic extended Boolean, numeric, and string capabilities with exact case-sensitive names;
- reusable parsed terminfo parameter programs, hardened parsing/evaluation, and per-description bounded lazy expansion caches;
- explicit `ExpandExtendedString` symmetry for parameterized extended strings;
- reversible 8-bit capability-string semantics: bytes `0x01`-`0xFF` map one-to-one through .NET strings and round-trip with `Encoding.Latin1`;
- simple and terminal-aware `tputs`/`putp`-style output, including `xon`, `pb`, `npc`, `pad`, affected-line multiplication, and caller-supplied baud-rate semantics;
- semantic monochrome, indexed-color, and direct-RGB inspection and selector expansion;
- built-in `dumb`, ANSI, DEC VT100/VT102/VT220, xterm indexed/direct-color, `winconsole`, `ms-terminal`, and `ms-terminal-direct` profiles;
- descriptive mouse, focus, bracketed-paste, modified-key, cursor-style, reporting, and clipboard metadata where the selected profile advertises it;
- live terminal-size queries kept distinct from environment/profile defaults;
- explicit and reversible Windows virtual-terminal output enablement, separate from profile selection;
- a frozen, deterministic 0.9 compiled-terminfo binary/provider target and checked-in parser-readiness fixture corpus, without a production external database loader in 0.8.

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

TerminalDescription winConsole =
    TerminalDatabase.BuiltIn.Load("winconsole");

TerminalDescription windowsTerminal =
    TerminalDatabase.BuiltIn.Load("ms-terminal");

TerminalDescription windowsTerminalDirect =
    TerminalDatabase.BuiltIn.Load("ms-terminal-direct");
```

Aliases remain exact and intentional. For example, `vt100-am` resolves to `vt100`, and `vt200` resolves to `vt220`. Windows identities are not aliases for ANSI or xterm.

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

The complete standard catalog is inspectable in compiled-table order. Managed enum values are deliberately independent from those binary indices:

```csharp
StandardCapabilityMetadata<StringCapability> cupMetadata =
    StandardCapabilityCatalog.GetMetadata(
        StringCapability.CursorAddress);

Console.WriteLine(
    $"{cupMetadata.ShortName}: binary index {cupMetadata.BinaryIndex}");

foreach (StandardCapabilityMetadata<NumericCapability> metadata
    in StandardCapabilityCatalog.NumericCapabilities)
{
    Console.WriteLine(
        $"{metadata.ShortName} / {metadata.LongName}");
}
```

A terminal description also exposes its effective standard capabilities in the same deterministic order:

```csharp
Console.WriteLine(terminal.Description ?? "(no verbose description)");

foreach (KeyValuePair<NumericCapability, int> capability
    in terminal.NumericCapabilities)
{
    Console.WriteLine($"{capability.Key} = {capability.Value}");
}
```

Absent and internally canceled capabilities do not appear as effective present values. Extended capabilities remain separately enumerable through `ExtendedCapabilities`.

Modern capabilities which are not part of the fixed standard terminfo vocabulary are carried through the extended-capability store:

```csharp
if (xterm.TryGetExtendedString("BE", out string? enablePaste))
{
    Console.WriteLine("Bracketed-paste enable metadata is present.");
}

if (xterm.TryGetExtendedString("XM", out _))
{
    string enableMouse =
        xterm.ExpandExtendedString("XM", 1);
}
```

Extended names are case-sensitive. Standard capability names cannot be silently shadowed by extended capabilities.

Reusable arbitrary-source parameter programs can be parsed once and expanded repeatedly. Structural/type analysis remains internal safety machinery rather than a second public model:

```csharp
TermInfoParameterProgram program =
    TermInfoParameterProgram.Parse("%p1%{1}%+%d");

Console.WriteLine(program.Source);     // %p1%{1}%+%d
Console.WriteLine(program.Expand(41)); // 42
```

Per-description standard and extended expansion use bounded lazy caches owned by the immutable description. There is no process-global arbitrary-string cache.

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

### Exact capability bytes

Capability strings are protocol byte data, not application text. For data originating in conventional compiled terminfo, `Icod.TermInfo` uses a one-to-one Latin-1 bridge: byte `0x80` is represented by `\u0080`, byte `0xFF` by `\u00FF`, and so on. Use `Encoding.Latin1` when exact capability bytes must be emitted:

```csharp
using MemoryStream stream = new();

TermInfoOutput.TPuts(
    "\u0080",
    affectedLines: 1,
    stream,
    Encoding.Latin1);

byte[] bytes = stream.ToArray(); // { 0x80 }
```

This does **not** prescribe the encoding of application text. Text encoding remains caller-owned.

### Terminal-aware padding

When padding policy needs terminal facts, pass immutable `TermInfoOutputOptions` explicitly:

```csharp
TermInfoOutputOptions options =
    new(
        vt100,
        baudRate: 9600,
        paddingMode: PaddingMode.Delay);

TermInfoOutput.TPuts(
    move,
    affectedLines: 1,
    Console.Out,
    options);
```

The library never discovers baud rate and never owns a tty/file descriptor. Advisory padding is suppressed according to the terminal's `xon` and `pb` capabilities; mandatory padding remains mandatory unless the caller explicitly chooses `PaddingMode.Ignore`. `PaddingMode.PadCharacters` also honors `npc` and `pad`.

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

Windows profile selection is separate and side-effect free:

```csharp
TerminalDescription console =
    TerminalProfiles.WinConsole;

TerminalDescription wt =
    TerminalProfiles.MsTerminal;

TerminalDescription wtDirect =
    TerminalProfiles.MsTerminalDirect;
```

`winconsole` describes the authoritative modern Windows Console terminfo identity. `ms-terminal` is the indexed-color Windows Terminal identity, while `ms-terminal-direct` advertises direct RGB through the same generic color engine used by other profiles. `WT_SESSION`, `WT_PROFILE_ID`, and `COLORTERM` do not silently select or mutate any profile.

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
- verbose description plus standard catalog/per-description enumeration;
- reusable standard and extended parameterized-string expansion;
- exact Latin-1 capability-byte output;
- terminal-aware padding with explicit terminal facts;
- semantic indexed/direct color inspection and expansion;
- Windows Console and Windows Terminal profile selection without side effects;
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

`--profile <name>` selects an exact built-in profile instead of consulting `TERM`. `--describe-only` exercises metadata/enumeration, expansion, byte-output, padding, profile, color, and extended-capability APIs but emits no terminal-control strings to the active terminal.

## Project-family boundary

`Icod.TermInfo` owns terminal-description data and pure transformations of that data. It does not own a live terminal session.

A future `Icod.Terminal`-style layer may own raw/cooked mode changes, input decoding, keyboard/mouse/paste/focus events, probing, full-screen lifecycle, cursor lifecycle, clipboard operations, and progress helpers. A future curses-style library may own virtual-screen state, windows, pads, panels, menus, forms, and refresh optimization. PTY creation/process plumbing belongs elsewhere as well.

## 0.9.0 arbitrary-terminal boundary

Version 0.8 deliberately completes **terminfo semantics in memory** but does not acquire arbitrary terminal descriptions from the host.

The required 0.9 work is already frozen and fixture-backed. It includes:

- conventional `0432` compiled entries;
- ncurses extended sections;
- `01036` / signed 32-bit numeric entries;
- absent/canceled binary handling;
- explicit directory-tree providers;
- `TERMINFO`, `TERMINFO_DIRS`, `$HOME/.terminfo`, and platform default roots;
- encoded `TERMINFO=hex:...` / `TERMINFO=b64:...`;
- safe malformed-entry diagnostics and provider-instance caching.

Version 0.8 therefore contains **no production compiled terminfo parser**, no `/usr/share/terminfo` loader, no `TERMINFO`/`TERMINFO_DIRS` discovery, and no host-database-dependent automatic profile selection. The checked-in T29 compiled fixtures are test/provenance assets for 0.9 and are not runtime package data.

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

The package verifier checks the `.nupkg`/`.snupkg` structure, dependency closure, Source Link metadata, exclusion of test-only compiled fixtures, and a fresh `net10.0` consumer restored from only the local package directory. The fresh consumer exercises the intended new 0.8 metadata/enumeration, expansion, exact-byte, terminal-aware padding, and Windows-profile APIs. It also runs the sample's `--describe-only` mode as a non-interactive consumer check.

GitHub pull requests validate both Debug and Release on Windows, Linux, and macOS. Pushes to `main` and the `0.8.0` release branch run the same matrix, then pack and verify the package and upload the exact package artifacts; validation may also be started manually. These workflows do not publish packages automatically.

See `docs/RELEASING.md` for the release procedure and `docs/0.8.0-CONTRACT-AUDIT.md` for the final T31 evidence map. Tag `v0.8.0` only after the exact final candidate passes the complete workflow described there; no source/package content should change between that successful validation and tagging.

## Scope

`Icod.TermInfo` is not curses, a terminal emulator, a PTY implementation, a termios session manager, an input-event parser, or a general terminal UI toolkit. It intentionally carries low-level descriptive data which those higher-level systems may consume.

See `Icod.TermInfo-Development-Roadmap-0.8.0.md` for the complete 0.8.0 contract. The 0.6.0 and 0.7.0 roadmaps remain historical frozen contracts.

## License

Licensed under the GNU Lesser General Public License v3.0 or later. See `LICENSE`.
