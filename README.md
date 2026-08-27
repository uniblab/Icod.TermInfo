# Icod.TermInfo

`Icod.TermInfo` is a managed, dependency-free .NET implementation of the low-level terminal-capability model traditionally supplied by `libtinfo`.

Version 1.1.1 is the current maintenance release of the 1.1 line. It preserves the frozen 1.0 runtime contract and the 1.1 source-language feature set provided by the optional `Icod.TermInfo.Source` package.

The published 1.1.1 packages target `net8.0` and `net10.0`. Beginning with the 1.2
development line, the supported consumer matrix expands to `net8.0`, `net9.0`,
and `net10.0`; the packages use C# 13, contain no native ncurses/terminfo
payload, and are intended to run on Windows, Linux, and macOS.

## Install

For the 1.1.1 release, runtime-only consumers use:

```text
dotnet add package Icod.TermInfo --version 1.1.1
```

Applications which need terminfo source-language support use:

```text
dotnet add package Icod.TermInfo.Source --version 1.1.1
```

`Icod.TermInfo.Source` depends on the matching `Icod.TermInfo` package. Applications which only load compiled terminfo or consume `TerminalDescription` values continue to reference `Icod.TermInfo` alone.

The same validated package artifacts are intended for NuGet.org and GitHub Packages. Repository development can reference the corresponding project directly.

## 1.x stability contract

The 1.x line keeps runtime assembly identity `Icod.TermInfo, Version=1.0.0.0` and
remains unsigned. The frozen 1.0 and 1.1 releases support `net8.0` and
`net10.0`; beginning with 1.2, the supported consumer targets are `net8.0`,
`net9.0`, and `net10.0`. `Icod.TermInfo.Source` and `Icod.TermInfo.Compiler`
retain assembly version `1.0.0.0` throughout their 1.x lines. Public API,
binary/package compatibility,
deprecation, and target-framework policy are documented in `docs/VERSIONING.md`
and `docs/COMPATIBILITY.md`.

The runtime 1.0 public API remains frozen. Version 1.1 adds source-language functionality in the separate `Icod.TermInfo.Source` package rather than making the runtime package depend on parser/front-end code. The 1.2 line adds deterministic compiled-entry writing in the separate `Icod.TermInfo.Compiler` package. Live terminal sessions, PTYs, curses/UI, terminal emulation, command-line `tic`/`infocmp`/`toe` tooling, termcap conversion, and active protocol negotiation remain later or sibling work.

## What 1.0 provides

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
- pure parsing of supported conventional compiled terminfo entries from caller-supplied bytes;
- caller-owned explicit conventional-directory providers with literal/hex first-character lookup and identity verification;
- deterministic system discovery through encoded or directory `TERMINFO`, user `.terminfo`, ordered `TERMINFO_DIRS`, and frozen platform defaults;
- provider-local successful-entry caching with retryable clean misses/failures and new-provider refresh semantics;
- explicit provider composition, including system lookup followed by immutable built-in fallback.

The 0.6.0 behavior of `dumb`, `ansi`, and `vt100` remains intentionally conservative: `dumb` is minimal, `ansi` is the traditional eight-color profile, and `vt100` remains monochrome.

## What 1.1 adds

The optional `Icod.TermInfo.Source` package adds the source-language path without changing the runtime package contract:

- deterministic `.ti` lexical analysis with source spans and diagnostics;
- terminfo string and numeric source-value semantics;
- unresolved documents, entries, fields, aliases, and descriptions;
- classification of standard and extended capabilities against the runtime catalog;
- cancellation semantics and `use=` inheritance resolution;
- materialization of resolved source entries into ordinary immutable `TerminalDescription` values;
- stable duplicate source-name and alias diagnostics;
- bounded source and inheritance processing, deterministic mutation fuzzing, and checked-in source/compiled compatibility fixtures.

The source package is optional. Compiled-database users and higher-level terminal consumers do not acquire it transitively through `Icod.TermInfo`.

## Getting started

Terminal resolution remains explicit and conservative. A normal application can
search the host database first and then fall back to the immutable built-ins:

```csharp
using Icod.TermInfo;

TerminalDatabase database =
    new(
        new ITerminalDescriptionProvider[]
        {
            new SystemTerminalDescriptionProvider(),
            TerminalDatabase.BuiltIn,
        }
    );

TerminalDescription terminal =
    TerminalEnvironment.Resolve(
        database,
        TerminalProfiles.Dumb
    );

Console.WriteLine($"Terminal profile: {terminal.Name}");
```

`TerminalDatabase.BuiltIn` remains environment-independent and I/O-free.
Unknown names are not silently coerced to ANSI, VT100, or xterm.

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
        StringCapability.CursorAddress
    );

Console.WriteLine(
    $"{cupMetadata.ShortName}: binary index {cupMetadata.BinaryIndex}"
);

foreach (StandardCapabilityMetadata<NumericCapability> metadata
    in StandardCapabilityCatalog.NumericCapabilities)
{
    Console.WriteLine(
        $"{metadata.ShortName} / {metadata.LongName}"
    );
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
        196
    );

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
        purple
    );
```

The selected xterm direct profiles use packed 8/8/8 RGB semantics and retain 8, 16, or 256 indexed entries according to their `CO` metadata. The library validates collisions between packed RGB values and that retained indexed prefix instead of guessing.

## Cursor positioning and full-screen primitives

Parameterized standard capabilities use the same terminfo expansion engine:

```csharp
string move =
    xterm.Expand(
        StringCapability.CursorAddress,
        10,
        20
    );
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
        20
    );

// move contains ESC[11;21H$<5>
```

Applications should emit capability strings through the output layer. Modern terminals normally use the default `PaddingMode.Ignore`, which removes delay annotations without writing them literally:

```csharp
TermInfoOutput.TPuts(
    move,
    affectedLines: 1,
    Console.Out
);
```

Physical or serial terminals can opt into delays:

```csharp
TermInfoOutput.TPuts(
    move,
    affectedLines: 1,
    Console.Out,
    PaddingMode.Delay
);
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
    Encoding.Latin1
);

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
        paddingMode: PaddingMode.Delay
    );

TermInfoOutput.TPuts(
    move,
    affectedLines: 1,
    Console.Out,
    options
);
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
        new[] { example }
    );

TerminalDatabase database =
    new(new[] { provider });
```

Provider ordering is explicit and deterministic; the first provider that resolves a name wins.

## Compiled terminfo acquisition

For a task-oriented explanation of parser formats, directory layout, discovery
precedence, option boundaries, errors, caching, and refresh, see
`docs/0.9.0-ACQUISITION-GUIDE.md`. The focused
`samples/Icod.TermInfo.Acquisition.Sample` executable demonstrates the same
public acquisition paths without emitting terminal-control strings.

### Parse caller-supplied bytes

The parser is independently usable and has no filesystem or environment
dependency:

```csharp
byte[] entry = File.ReadAllBytes("xterm.compiled");

TerminalDescription parsed =
    CompiledTermInfoParser.Parse(entry);
```

`CompiledTermInfoParserOptions` bounds accepted entry size. Malformed or
unsupported compiled data throws `CompiledTermInfoFormatException`; it is not
reported as a clean provider miss.

### Load an explicit directory

When an application owns a conventional terminfo directory tree, use
`DirectoryTerminalDescriptionProvider`:

```csharp
ITerminalDescriptionProvider applicationTermInfo =
    new DirectoryTerminalDescriptionProvider(
        "/opt/myapp/share/terminfo"
    );

TerminalDescription terminal =
    new TerminalDatabase(
        new[] { applicationTermInfo }
    )
        .Load("my-terminal");
```

The provider performs exact-name lookup only and propagates malformed-entry,
permission, and I/O failures.

### Construct a restricted system provider

Each system discovery category can be disabled independently:

```csharp
SystemTerminalDescriptionProvider restricted =
    new(
        new SystemTerminalDescriptionProviderOptions(
            useEnvironment: false,
            useUserDatabase: false,
            useSystemDatabases: false
        )
    );
```

That provider has no enabled acquisition source and therefore returns a clean
miss for valid terminal names.

### Use normal system discovery

The default system provider snapshots its permitted discovery inputs at
construction:

```csharp
SystemTerminalDescriptionProvider system =
    new();
```

On non-Windows systems this can search encoded/directory `TERMINFO`, the
user-local `.terminfo` database, `TERMINFO_DIRS`, and frozen platform defaults.
Windows does not invent Unix-style implicit roots; explicit `TERMINFO`,
`TERMINFO_DIRS`, and `DirectoryTerminalDescriptionProvider` remain available.

### Compose system and built-in fallback

`TerminalDatabase` is itself an `ITerminalDescriptionProvider`, so built-in
fallback remains explicit:

```csharp
TerminalDatabase database =
    new(
        new ITerminalDescriptionProvider[]
        {
            new SystemTerminalDescriptionProvider(),
            TerminalDatabase.BuiltIn,
        }
    );
```

The first provider which resolves the requested name wins.
`TerminalDatabase.BuiltIn` is never mutated by system discovery.

## Sample applications

The repository contains two executable samples with deliberately different
purposes.

### General terminal API sample

`samples/Icod.TermInfo.Sample` demonstrates:

- conservative environment resolution with an explicit `dumb` fallback;
- system-to-built-in provider composition for ordinary resolution;
- verbose description plus standard catalog/per-description enumeration;
- reusable standard and extended parameterized-string expansion;
- exact Latin-1 capability-byte output;
- terminal-aware padding with explicit terminal facts;
- semantic indexed/direct color inspection and expansion;
- Windows Console and Windows Terminal profile selection without side effects;
- full-screen/cursor-visibility capability discovery without taking ownership of
  a full-screen session;
- live/configured/profile size selection;
- redirection handling and explicit Windows VT enablement;
- a custom provider implementation.

Both sample projects in the 1.2 development line target `net8.0`, `net9.0`,
and `net10.0`; `dotnet run` therefore needs an explicit framework. Run the
ordinary demonstration with:

```text
dotnet run --project samples/Icod.TermInfo.Sample/Icod.TermInfo.Sample.csproj -f net10.0
```

For CI, documentation checks, or any environment where terminal-control output
is inappropriate, use the non-interactive descriptive mode:

```text
dotnet run --project samples/Icod.TermInfo.Sample/Icod.TermInfo.Sample.csproj -f net10.0 -- --describe-only --profile xterm-direct256
```

Use `-f net8.0` or `-f net9.0` instead when validating those consumer targets.

`--profile <name>` selects an exact built-in profile instead of consulting
`TERM`. `--describe-only` exercises metadata/enumeration, expansion, byte-output,
padding, profile, color, and extended-capability APIs but emits no
terminal-control strings to the active terminal.

### Compiled terminfo acquisition sample

`samples/Icod.TermInfo.Acquisition.Sample` is the focused, non-interactive
compiled-database acquisition sample introduced in 0.9 and retained for 1.0. It
demonstrates:

```text
parse <compiled-file>
directory <root> <terminal-name>
system <terminal-name>
restricted <terminal-name>
fallback <terminal-name>
```

For example:

```text
dotnet run --project samples/Icod.TermInfo.Acquisition.Sample/Icod.TermInfo.Acquisition.Sample.csproj -f net10.0 -- system xterm-256color
```

and:

```text
dotnet run --project samples/Icod.TermInfo.Acquisition.Sample/Icod.TermInfo.Acquisition.Sample.csproj -f net10.0 -- directory /usr/share/terminfo xterm
```

The sample prints the resolved terminal identity, aliases, selected numeric
facts, and standard/extended capability counts. It does not write any capability
string to the terminal.

See `samples/README.md`,
`samples/Icod.TermInfo.Acquisition.Sample/README.md`, and
`docs/0.9.0-ACQUISITION-GUIDE.md` for the complete examples.

## Project-family boundary

`Icod.TermInfo` owns immutable terminal-description data, acquisition of that data, and pure transformations required to interpret, expand, and output terminal capabilities. `Icod.TermInfo.Source` owns optional source-language parsing and inheritance resolution. Neither package owns a live terminal session, a child pseudo-terminal, or a virtual screen.

The intended family boundary is now explicit:

- **`Icod.TermInfo`** — descriptions, compiled-database acquisition, capability semantics, parameter expansion, and output transformation;
- **`Icod.TermInfo.Source`** — `.ti` lexical analysis, source diagnostics, unresolved entries, cancellation, `use=` inheritance, and materialization into `TerminalDescription`;
- **future `Icod.TermInfo.Compiler` / tools** — compiled-entry writing, `tic`/`infocmp`/`toe` engines and commands, termcap conversion, and optional database-maintenance functionality;
- **future `Icod.Terminal`** — raw/cooked session ownership, input decoding, keyboard/mouse/paste/focus events, active probing/negotiation, full-screen/cursor lifecycle, clipboard/hyperlink operations, and progress helpers;
- **future `Icod.Pty`** — Unix PTY and Windows ConPTY creation, resize propagation, and child-process plumbing;
- **future `Icod.Curses`** — Unicode cell/grid state, damage/refresh optimization, windows, pads, panels, menus, forms, and widgets.

The broader dependency inventory is recorded in `docs/FUTURE-WORK-INVENTORY.md`.

## Acquisition foundation inherited from 0.9.0

Version 0.8 completed **terminfo semantics in memory**. Version 0.9 added the
acquisition layer without redesigning that semantic model. Version 1.0 freezes
that combined low-level contract rather than replacing it.

The implemented acquisition dependency chain is:

```text
pure compiled-byte parser
    -> explicit directory provider
    -> TERMINFO / TERMINFO_DIRS / user / platform discovery
    -> provider-local cache and refresh semantics
    -> frozen API/package contract
```

The parser independently accepts caller-supplied bytes and supports the frozen
conventional `0432`, ncurses extended-section, and `01036` / signed-32-bit
formats. Directory and system providers reuse that parser rather than embedding
their own binary logic. Encoded `TERMINFO=hex:...` and
`TERMINFO=b64:...` entries use the same parser path.

0.9 deliberately does **not** include `.ti` source parsing, `tic`/`infocmp`,
termcap, Berkeley-DB hashed terminfo stores, divergent historical vendor binary
formats, live input/session management, active probing, PTYs, curses, terminal
emulation, or graphics protocols.

See `docs/0.9.0-ACQUISITION-GUIDE.md` for the consumer-facing acquisition
guide, `Icod.TermInfo-Development-Roadmap-0.9.0.md` for the detailed frozen
tranche contract, `docs/0.9.0-CONTRACT-AUDIT.md` for the final completion
evidence, `docs/0.9.0-T40-API-PACKAGE-FREEZE.md` for the release-candidate
API/package freeze, and `docs/FUTURE-WORK-INVENTORY.md` for the post-0.9
dependency map.

## Build, test, and pack

```text
dotnet restore Icod.TermInfo.sln

dotnet build Icod.TermInfo.sln -c Debug
dotnet test Icod.TermInfo.sln -c Debug

dotnet build Icod.TermInfo.sln -c Staging
dotnet test Icod.TermInfo.sln -c Staging
dotnet pack Icod.TermInfo.csproj -c Staging --output artifacts
dotnet pack Icod.TermInfo.Source/Icod.TermInfo.Source.csproj -c Staging --output artifacts

dotnet build Icod.TermInfo.sln -c Release
dotnet test Icod.TermInfo.sln -c Release
dotnet pack Icod.TermInfo.csproj -c Release --output artifacts
dotnet pack Icod.TermInfo.Source/Icod.TermInfo.Source.csproj -c Release --output artifacts
```

Use the verifier with the same configuration used to build and pack.

For Staging validation:

```text
.github\scripts\verify-release-package.cmd artifacts Staging
bash .github/scripts/verify-release-package.sh artifacts Staging
```

For final Release validation:

```text
.github\scripts\verify-release-package.cmd artifacts Release
bash .github/scripts/verify-release-package.sh artifacts Release
```

Both wrappers run the same capability-metadata check; exact runtime and Source
public-API baseline checks; net8/net9/net10 API-equivalence checks; runtime
package/XML/symbol validation; isolated runtime and Source package-reference-only
smoke consumers on all three target frameworks; and the sample's non-interactive
`--describe-only` path. Windows package validation does not require Bash or
Python.

Pull requests use Staging throughout, may upload the verified `.nupkg` and
`.snupkg` artifacts for both packages, and never publish packages. Only pushes to
`main` run the Release build/test/package-validation/publication workflow.

See `docs/RELEASING.md` for the release procedure,
`Icod.TermInfo-Development-Roadmap-1.0.0.md` for the frozen runtime contract,
`Icod.TermInfo-Post-1.0-Development-Roadmap.md` for the 1.1 source-language
program, and `docs/1.1.0-RELEASE-AUDIT.md` for the final release gate. Tag
`v1.1.1` only on the exact `main` commit whose complete Release validation and
publication succeeded; no source or package content may change between that
validation and tagging.

## Scope

`Icod.TermInfo` is not curses, a terminal emulator, a PTY implementation, a termios session manager, an input-event parser, or a general terminal UI toolkit. It intentionally carries low-level descriptive data which those higher-level systems may consume. `Icod.TermInfo.Source` is an optional parser/resolver layer and does not change those runtime boundaries.

See `Icod.TermInfo-Development-Roadmap-0.9.0.md` for the frozen acquisition
contract, `Icod.TermInfo-Development-Roadmap-1.0.0.md` for the 1.0 runtime
stability contract, `Icod.TermInfo-Post-1.0-Development-Roadmap.md` for the 1.1
source-language program, `docs/VERSIONING.md` and `docs/COMPATIBILITY.md` for the
1.x promises, and `docs/FUTURE-WORK-INVENTORY.md` for the broader terminal-system
dependency map. The 0.6.0 through 1.0.0 roadmaps remain historical frozen
contracts.

## Authors

Inspired by original work from Bill Joy, author of the original `termcap`; Mary Ann (born Mark) Horton, author of `terminfo`; Pavel Curtis, author of `pcurses`; and Zeyd Ben-Halim, Eric S. Raymond, and Thomas Dickey, whose work developed and maintained `libtinfo` and ncurses.

Managed .NET implementation by Timothy J. Bruce <uniblab@hotmail.com>.

## Copyright

Copyright (c) 2026 Timothy J. Bruce

## License

Licensed under the GNU Lesser General Public License v3.0 or later. See `LICENSE`.
