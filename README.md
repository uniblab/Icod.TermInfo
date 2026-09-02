# Icod.TermInfo

![Icod TUI Toolchain](https://raw.githubusercontent.com/uniblab/Icod.TermInfo/v1.4.1/icod_tui_toolchain.jpg)

[![PR Staging build](https://github.com/uniblab/Icod.TermInfo/actions/workflows/pull-request.yaml/badge.svg)](https://github.com/uniblab/Icod.TermInfo/actions/workflows/pull-request.yaml)
[![Main Release validation](https://github.com/uniblab/Icod.TermInfo/actions/workflows/main.yaml/badge.svg?branch=main)](https://github.com/uniblab/Icod.TermInfo/actions/workflows/main.yaml)

`Icod.TermInfo` is a managed, dependency-free .NET implementation of the low-level terminal-capability model traditionally supplied by `libtinfo`.

Version 1.8.0 is the current coordinated release. It adds deterministic,
bounded ordered-parent planning in `Icod.TermInfo.Inspection` and exposes it
through `infocmp --plan-use` while preserving the Runtime, Source, Compiler,
Termcap, and frozen 1.7 synthesis contracts.

The 1.8.0 library package family targets `net8.0`, `net9.0`, and `net10.0`; the
packages use C# 13, contain no native ncurses/terminfo payload, and are intended
to run on Windows, Linux, and macOS.

The additive planning API, lexicographic score, bounds, exhaustive and
explicitly bounded search, explicit catalog orchestration, command composition,
package consumers, samples, and distribution gates are frozen in
`docs/1.8.0-RELEASE-AUDIT.md`.

## Install

Runtime-only consumers use:

```text
dotnet add package Icod.TermInfo --version 1.8.0
```

Applications which need terminfo source-language support use:

```text
dotnet add package Icod.TermInfo.Source --version 1.8.0
```

Applications which need opt-in termcap parsing, conversion, rendering, or
explicit historical termcap acquisition use:

```text
dotnet add package Icod.TermInfo.Termcap --version 1.8.0
```

Applications which compile terminfo source or write conventional compiled
terminfo databases use:

```text
dotnet add package Icod.TermInfo.Compiler --version 1.8.0
```

Applications which need canonical rendering, semantic comparison, or
provider-aware inspection use:

```text
dotnet add package Icod.TermInfo.Inspection --version 1.8.0
```

`Icod.TermInfo.Source` and `Icod.TermInfo.Termcap` each depend on the matching
`Icod.TermInfo` package. `Icod.TermInfo.Compiler` and
`Icod.TermInfo.Inspection` each depend on matching Runtime and Source packages;
Inspection does not depend on Compiler, and no existing reusable package depends
on Termcap. Applications which only load compiled terminfo or consume
`TerminalDescription` values continue to reference `Icod.TermInfo` alone.

The same validated package artifacts are published to NuGet.org and GitHub
Packages. Historical release contracts remain recorded in the versioned release
audits; the 1.8 publication gate is recorded in
`docs/1.8.0-RELEASE-AUDIT.md`.

## Tool Suite

The 1.4 line added three managed .NET 10 command-line tools above the reusable
package family:

```text
tic       validate and publish terminfo source
infocmp   render and semantically compare terminal descriptions
toe       enumerate conventional databases and analyze use= dependencies
```

Version 1.6.0 adds two additional non-packable conversion commands:

```text
captoinfo convert termcap descriptions to effective terminfo source
infotocap convert effective terminfo source to conventional termcap
```

All five standalone command projects remain non-packable and do not introduce
command-to-command dependencies. `Icod.TermInfo.Tools` remains the separate
distribution-only router package.

Install the coordinated router as a .NET tool with:

```text
dotnet tool install --global Icod.TermInfo.Tools --version 1.8.0

icod-terminfo tic -V
icod-terminfo infocmp -V
icod-terminfo toe -V
icod-terminfo captoinfo -V
icod-terminfo infotocap -V
```

The router strips the command name and dispatches in-process to the existing
command implementation. It does not duplicate command option parsing or
terminfo semantics.

The standalone distribution remains a framework-dependent .NET 10 suite archive
for each supported RID:

```text
Icod.TermInfo.Tools.<version>.win-x64.zip
Icod.TermInfo.Tools.<version>.win-arm64.zip
Icod.TermInfo.Tools.<version>.linux-x64.tar.gz
Icod.TermInfo.Tools.<version>.linux-arm64.tar.gz
Icod.TermInfo.Tools.<version>.osx-x64.tar.gz
Icod.TermInfo.Tools.<version>.osx-arm64.tar.gz
```

Each 1.8.0 archive contains the traditional `tic`, `infocmp`, `toe`,
`captoinfo`, and `infotocap` command names and their required managed
dependencies. The user supplies the .NET 10 runtime and controls where the
archive is unpacked and whether that location is placed on `PATH`. The archive
therefore remains suitable for intentional drop-in installation of the
traditional names, while the NuGet tool uses the non-colliding `icod-terminfo`
router name.

## 1.x stability contract

The 1.x line keeps runtime assembly identity `Icod.TermInfo, Version=1.0.0.0` and
remains unsigned. The frozen 1.0 and 1.1 releases support `net8.0` and
`net10.0`; beginning with 1.2, the supported consumer targets are `net8.0`,
`net9.0`, and `net10.0`. `Icod.TermInfo.Source`, `Icod.TermInfo.Compiler`,
`Icod.TermInfo.Inspection`, and `Icod.TermInfo.Termcap` retain assembly version
`1.0.0.0` throughout their
1.x lines. Public API, binary/package compatibility, deprecation, and
target-framework policy are documented in `docs/VERSIONING.md` and
`docs/COMPATIBILITY.md`.

The runtime 1.0 public API remains frozen. Version 1.1 adds source-language functionality in the separate `Icod.TermInfo.Source` package rather than making the runtime package depend on parser/front-end code. The 1.2 line adds deterministic compiled-entry writing in the separate `Icod.TermInfo.Compiler` package. The 1.3 line adds canonical rendering and semantic comparison in the separate `Icod.TermInfo.Inspection` package. The 1.4 line composes those libraries into the separate `tic`, `infocmp`, and `toe` command layer without moving command policy into the reusable packages. Live terminal sessions, input decoding, and active probing belong to the sibling `Icod.Terminal` layer; curses-style screen/window behavior belongs to `Icod.DCurses`. PTYs, terminal emulation, and graphics protocols remain separate later or sibling work.

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

## What 1.2 adds

The optional `Icod.TermInfo.Compiler` package completes the managed inverse of
the existing compiled-term acquisition path:

- deterministic legacy `0432` and wide `01036` compiled-entry writing;
- standard and ncurses extended-capability emission;
- automatic or explicit format selection with strict representation validation;
- direct compilation from resolved `TerminalDescription` values;
- `.ti` source compilation through the existing Source parser and inheritance
  resolver, preserving Source diagnostics;
- controlled publication into explicit conventional terminfo directory layouts;
- safe path derivation, overwrite policy, and failure-resistant database writes;
- semantic source → resolve → write → parse round-trip validation;
- byte-for-byte determinism checks and pinned ncurses/`tic` differential corpus
  coverage.

Compiler remains opt-in. The Runtime package remains dependency-free, Source
depends only on Runtime, and Compiler depends on Runtime and Source.

## What 1.3 adds

The optional `Icod.TermInfo.Inspection` package adds reusable inspection and
comparison engines without enlarging the already-frozen Runtime, Source, or
Compiler public contracts:

- canonical `.ti`-style rendering of effective `TerminalDescription` values;
- normalized rendering of unresolved Source entries and documents while
  preserving semantically significant field order;
- deterministic structured comparison of effective terminal descriptions;
- source-aware comparison which preserves cancellation, disabled fields,
  duplicate declarations, `use=` references, and source ordering;
- provider-aware inspection through explicit provider/name targets;
- deterministic comparison ordering across cultures and extended-capability
  insertion order;
- corpus-backed managed render/compile/parse/compare validation across the
  existing Runtime, Source, and Compiler layers.

Inspection remains opt-in. It depends on matching Runtime and Source packages
and deliberately has no production dependency on Compiler. The 1.4 command
layer consumes Inspection without changing that package dependency boundary.

## What 1.4 adds

The 1.4 line completes the first managed command-toolchain layer above the
reusable package family while keeping command policy out of Runtime, Source,
Compiler, and Inspection:

- Inspection adds read-only system database-location inspection and conventional
  database catalog enumeration without enlarging the frozen Runtime API;
- Inspection adds configurable effective-source renderer layout, width,
  capability ordering, and extended-capability filtering while preserving the
  released 1.3 renderer overload behavior;
- `tic` validates strict UTF-8 `.ti` source, resolves `use=` inheritance, checks
  compiled representability, and publishes explicit conventional databases;
- `infocmp` renders effective descriptions and performs structured semantic
  difference/common/absent-standard reporting over explicit or discovered
  providers;
- `toe` enumerates conventional databases and provides forward/reverse `use=`
  source-dependency reports with deterministic duplicate handling;
- the three commands ship together in six framework-dependent .NET 10 archives
  for Windows, Linux, and macOS, with structural verification and matching-host
  execution smoke.

Version 1.4.1 retained the exact 1.4.0 API and command semantics while
correcting release-facing documentation.

## What 1.5 adds

Version 1.5 changes distribution and release infrastructure rather than terminfo
semantics:

- `Directory.Build.props` contains the single `IcodTermInfoSuiteVersion` source
  used by all four libraries, all three standalone commands, and the router;
- the new `Icod.TermInfo.Tools` NuGet package installs the `icod-terminfo`
  multi-command router;
- `icod-terminfo tic`, `icod-terminfo infocmp`, and `icod-terminfo toe` dispatch
  directly to the existing command implementations;
- standalone `tic`, `infocmp`, and `toe` remain non-packable projects and remain
  available in the six framework-dependent release archives;
- CI installs and executes the packed router tool on Windows, Linux, and macOS
  in addition to continuing matching-host archive smoke tests.

No frozen Runtime, Source, Compiler, or Inspection public API changes in 1.5.0,
and no routed command semantics change.

## What 1.6 adds

Version 1.6.0 adds opt-in historical termcap interoperability while preserving
the existing terminfo-first Runtime discovery contract:

- `Icod.TermInfo.Termcap` is a fifth coordinated reusable library package and
  depends only on Runtime;
- TC01-TC06 provide bounded termcap parsing, Runtime-derived capability
  classification, `tc=` resolution, semantic conversion, reverse
  representability/rendering, and explicit `TERMCAP` / `TERMPATH` acquisition;
- TC07 adds standalone `captoinfo` and `infotocap` commands and routes both
  through `icod-terminfo`;
- `captoinfo` composes Termcap conversion with Inspection's effective terminfo
  source renderer;
- `infotocap` composes the existing terminfo Source parser/resolver with the
  Termcap reverse renderer;
- all six RID archives carry five standalone launchers: `tic`, `infocmp`, `toe`,
  `captoinfo`, and `infotocap`;
- conversion output is effective resolved state; comments, original formatting,
  cancellations/disabled fields, and inheritance ancestry are not reconstructed;
- conversion loss and termcap representability failures are reported instead of
  being silently hidden;
- TC08 provides checked-in differential/hostile-input coverage, bounded seeded
  mutation validation, the frozen Termcap public API baseline, a structural
  Termcap package verifier, and isolated package-reference consumers on
  `net8.0`, `net9.0`, and `net10.0`.

The 1.6.0 code/API/package/CLI contract is frozen. The stable `v1.6.0` release
was published on 2026-08-31 from the exact validated release commit
`4238632f22fce41726f1f94e5621383a9d3303a7`. The frozen release contract and
post-publication record are documented in `docs/1.6.0-RELEASE-AUDIT.md`.

## What 1.7 adds

Version 1.7.0 adds deterministic relative terminfo source synthesis while
preserving the frozen Runtime, Source, Compiler, and Termcap public contracts:

- `Icod.TermInfo.Inspection` adds
  `TerminalDescriptionSourceSynthesisParent`,
  `TerminalDescriptionSourceSynthesisOptions`, and
  `TerminalDescriptionSourceSynthesizer`;
- callers supply an effective target and an explicit ordered parent list whose
  exact `UseName` values become ordered `use=` references;
- the synthesizer omits inherited values already equal to the target, emits
  local additions and overrides, and emits `cap@` cancellations where inherited
  state must be removed;
- standard and ordinal case-sensitive extended capabilities participate in the
  same deterministic semantic model;
- canonical, single-line, and one-capability-per-line layouts retain the existing
  width and standard-capability ordering controls and always produce LF source;
- `infocmp -u target parent [parent ...]` exposes the reusable engine through
  the standalone command and `icod-terminfo` router;
- Source and Compiler round trips, reproducible generated-state tests, a pinned
  ncurses semantic differential corpus, package consumers, router smoke, and all
  six standalone archives permanently validate the feature.

Applications which already have effective target and parent descriptions can
synthesize relative source directly:

```csharp
using Icod.TermInfo;
using Icod.TermInfo.Inspection;

TerminalDescription target =
	TerminalDatabase.BuiltIn.Load( "xterm-256color" );
TerminalDescription parent =
	TerminalDatabase.BuiltIn.Load( "xterm" );

string relativeSource = TerminalDescriptionSourceSynthesizer.Synthesize(
	target,
	new[] {
		new TerminalDescriptionSourceSynthesisParent(
			"xterm",
			parent
		),
	}
);
```

Parent order is semantic and is never optimized, reordered, or pruned. The
generated source resolves to the target when combined with source representations
of the supplied effective parents.

## What 1.8 adds

Version 1.8.0 adds deterministic parent planning above the unchanged 1.7
relative-source synthesizer:

- `TerminalDescriptionSourcePlanner` evaluates zero-, one-, and ordered
  multi-parent plans from an explicit caller-supplied candidate sequence;
- `TerminalDescriptionSourcePlanningOptions` applies independent candidate,
  selected-parent, evaluated-plan, and generated-source bounds;
- `TerminalDescriptionSourcePlanningScore` ranks plans lexicographically by
  local directives, cancellations, parent count, rendered UTF-8 bytes, and
  candidate-index sequence;
- `TerminalDescriptionSourcePlan` returns selected parents, generated source,
  score, evaluated-plan count, and exhaustive-versus-bounded evidence;
- explicit catalog and directory helpers load only caller-selected data and do
  not consult environment or platform-default discovery; and
- `infocmp --plan-use` exposes the same planner through the standalone command,
  `icod-terminfo` router, tool package, and six release archives.

Applications can plan directly from effective descriptions:

```csharp
using Icod.TermInfo;
using Icod.TermInfo.Inspection;

TerminalDescription target =
	TerminalDatabase.BuiltIn.Load( "xterm-256color" );
TerminalDescription candidate =
	TerminalDatabase.BuiltIn.Load( "xterm" );

TerminalDescriptionSourcePlan plan =
	TerminalDescriptionSourcePlanner.Plan(
		target,
		new[] {
			new TerminalDescriptionSourceSynthesisParent(
				"xterm",
				candidate
			),
		}
	);
```

The planner does not infer ancestry or rewrite synthesis semantics. Every
evaluated plan delegates source generation to the frozen 1.7 synthesizer, and
the result reports whether the configured search space was exhausted.

## Getting started

Terminal resolution remains explicit and conservative. A normal application can
search the host database first and then fall back to the immutable built-ins:

```csharp
using Icod.TermInfo;

TerminalDatabase database = new(
    new ITerminalDescriptionProvider[] {
        new SystemTerminalDescriptionProvider(),
        TerminalDatabase.BuiltIn,
    }
);

TerminalDescription terminal = TerminalEnvironment.Resolve(
    database,
    TerminalProfiles.Dumb
);

Console.WriteLine($"Terminal profile: {terminal.Name}");
```

`TerminalDatabase.BuiltIn` remains environment-independent and I/O-free.
Unknown names are not silently coerced to ANSI, VT100, or xterm.

To select a known modern profile explicitly:

```csharp
TerminalDescription xterm = TerminalDatabase.BuiltIn.Load("xterm");
TerminalDescription xterm256 = TerminalDatabase.BuiltIn.Load("xterm-256color");
TerminalDescription xtermDirect = TerminalDatabase.BuiltIn.Load("xterm-direct256");
TerminalDescription winConsole = TerminalDatabase.BuiltIn.Load("winconsole");
TerminalDescription windowsTerminal = TerminalDatabase.BuiltIn.Load("ms-terminal");
TerminalDescription windowsTerminalDirect = TerminalDatabase.BuiltIn.Load("ms-terminal-direct");
```

Aliases remain exact and intentional. For example, `vt100-am` resolves to `vt100`, and `vt200` resolves to `vt220`. Windows identities are not aliases for ANSI or xterm.

## Standard and extended capabilities

Typed lookup is the preferred API for standard capabilities:

```csharp
bool automaticMargins = terminal.GetBoolean(BooleanCapability.AutoRightMargin);
int? columns = terminal.GetNumber(NumericCapability.Columns);
string? clear = terminal.GetString(StringCapability.ClearScreen);
```

Traditional short-name lookup remains available:

```csharp
bool hasColors = terminal.TryGetNumber("colors", out int colors);
bool hasClear = terminal.TryGetString("clear", out string? clear);
```

The complete standard catalog is inspectable in compiled-table order. Managed enum values are deliberately independent from those binary indices:

```csharp
StandardCapabilityMetadata<StringCapability> cupMetadata = StandardCapabilityCatalog.GetMetadata(
    StringCapability.CursorAddress
);

Console.WriteLine(
    $"{cupMetadata.ShortName}: binary index {cupMetadata.BinaryIndex}"
);

foreach (StandardCapabilityMetadata<NumericCapability> metadata
    in StandardCapabilityCatalog.NumericCapabilities) {
    Console.WriteLine(
        $"{metadata.ShortName} / {metadata.LongName}"
    );
}
```

A terminal description also exposes its effective standard capabilities in the same deterministic order:

```csharp
Console.WriteLine(terminal.Description ?? "(no verbose description)");

foreach (KeyValuePair<NumericCapability, int> capability
    in terminal.NumericCapabilities) {
    Console.WriteLine($"{capability.Key} = {capability.Value}");
}
```

Absent and internally canceled capabilities do not appear as effective present values. Extended capabilities remain separately enumerable through `ExtendedCapabilities`.

Modern capabilities which are not part of the fixed standard terminfo vocabulary are carried through the extended-capability store:

```csharp
if (xterm.TryGetExtendedString("BE", out string? enablePaste)) {
    Console.WriteLine("Bracketed-paste enable metadata is present.");
}

if (xterm.TryGetExtendedString("XM", out _)) {
    string enableMouse = xterm.ExpandExtendedString("XM", 1);
}
```

Extended names are case-sensitive. Standard capability names cannot be silently shadowed by extended capabilities.

Reusable arbitrary-source parameter programs can be parsed once and expanded repeatedly. Structural/type analysis remains internal safety machinery rather than a second public model:

```csharp
TermInfoParameterProgram program = TermInfoParameterProgram.Parse("%p1%{1}%+%d");

Console.WriteLine(program.Source);     // %p1%{1}%+%d
Console.WriteLine(program.Expand(41)); // 42
```

Per-description standard and extended expansion use bounded lazy caches owned by the immutable description. There is no process-global arbitrary-string cache.

## Color inspection

Color semantics are derived from raw terminfo data rather than from terminal-name checks:

```csharp
TerminalColorSupport support = TerminalColors.GetColorSupport(xterm256);

Console.WriteLine(support.Model);             // Indexed
Console.WriteLine(support.Tier);              // Color256
Console.WriteLine(support.IndexedColorCount); // 256
```

Raw `colors`, `pairs`, `ncv`, selectors, `bce`, `ccc`, `hls`, `initc`, `op`, `oc`, and extended `RGB`/`CO` metadata remain authoritative. `pairs` is never synthesized from `colors`.

### Indexed color

Use the semantic helper rather than embedding ANSI escape strings:

```csharp
string foreground = TerminalColors.ExpandForeground(
    TerminalProfiles.Xterm256Color,
    196
);

TermInfoOutput.PutP(foreground, Console.Out);
```

The helper validates the terminal's advertised indexed range and expands the terminal's own `setaf` capability through the shared parameter engine.

### Direct RGB color

Direct profiles expose an RGB layout and any retained indexed prefix:

```csharp
TerminalDescription direct = TerminalProfiles.XtermDirect256;
TerminalColorSupport support = TerminalColors.GetColorSupport(direct);
TerminalRgbColor purple = new(0x80, 0x40, 0xC0);

string foreground = TerminalColors.ExpandForeground(
    direct,
    purple
);
```

The selected xterm direct profiles use packed 8/8/8 RGB semantics and retain 8, 16, or 256 indexed entries according to their `CO` metadata. The library validates collisions between packed RGB values and that retained indexed prefix instead of guessing.

## Cursor positioning and full-screen primitives

Parameterized standard capabilities use the same terminfo expansion engine:

```csharp
string move = xterm.Expand(
    StringCapability.CursorAddress,
    10,
    20
);
```

Profiles can also advertise cursor-addressing lifecycle and cursor-visibility primitives:

```csharp
string? enter = xterm.GetString(StringCapability.EnterCursorAddressingMode);
string? leave = xterm.GetString(StringCapability.ExitCursorAddressingMode);
string? hideCursor = xterm.GetString(StringCapability.CursorInvisible);
string? normalCursor = xterm.GetString(StringCapability.CursorNormal);
```

These are capability strings, not a session manager. `Icod.TermInfo` does not decide when to enter full-screen mode, hide the cursor, recover from exceptions, or restore terminal state. A caller or the sibling `Icod.Terminal` session layer owns that lifecycle; `Icod.DCurses` builds higher-level screen/window policy above it.

## Mouse, focus, paste, and clipboard metadata

The modern xterm profiles carry descriptive protocol metadata such as:

- standard `kmous` plus extended `XM`/`xm` mouse strings;
- focus enable/disable and focus-in/focus-out strings;
- bracketed-paste enable/disable and begin/end strings;
- modified-key strings;
- cursor-style and terminal-reporting strings;
- OSC 52 clipboard/selection metadata where present in the selected profile.

This package does **not** decode mouse events, focus events, keys, or paste payloads. It also does not perform clipboard operations or terminal probing. The metadata is intentionally available so the sibling `Icod.Terminal` layer can consume it without teaching `Icod.TermInfo` about live input state.

## VT100 and padding

VT100 strings preserve their historical terminfo padding annotations through parameter expansion:

```csharp
TerminalDescription vt100 = TerminalProfiles.Vt100;

string move = vt100.Expand(
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
TermInfoOutputOptions options = new(
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
bool am = TermInfoCompatibility.TiGetFlag(xterm, "am");
int? colorCount = TermInfoCompatibility.TiGetNum(xterm, "colors");
string? cup = TermInfoCompatibility.TiGetStr(xterm, "cup");
```

There is no process-global `cur_term`, no sentinel-pointer result, and no hidden persistent expansion state. Persistent uppercase `%P/%g` variables require an explicit caller-owned `TermInfoExpansionContext`.

## Terminal size

Live dimensions are distinct from configured and profile-default dimensions:

```csharp
TerminalSize size;

if (TerminalEnvironment.TryGetLiveSize(out size)) {
    Console.WriteLine($"Live: {size.Columns}x{size.Rows}");
} else if (TerminalEnvironment.TryGetEnvironmentSize(out size)) {
    Console.WriteLine($"Configured: {size.Columns}x{size.Rows}");
} else if (TerminalEnvironment.TryGetProfileSize(terminal, out size)) {
    Console.WriteLine($"Profile default: {size.Columns}x{size.Rows}");
}
```

A failed live query never substitutes `COLUMNS`/`LINES` or a profile default. Fallback order belongs to the caller.

## Windows virtual-terminal output

Windows VT output mode is always opt-in:

```csharp
using IDisposable? mode = WindowsVirtualTerminal.TryEnableOutput();
```

The helper returns `null` on non-Windows systems, redirected output, non-console handles, or when Windows refuses the mode change. When it changes console mode, disposing the returned lease restores the exact previous mode. Loading a terminal profile never changes console state.

Windows profile selection is separate and side-effect free:

```csharp
TerminalDescription console = TerminalProfiles.WinConsole;
TerminalDescription wt = TerminalProfiles.MsTerminal;
TerminalDescription wtDirect = TerminalProfiles.MsTerminalDirect;
```

`winconsole` describes the authoritative modern Windows Console terminfo identity. `ms-terminal` is the indexed-color Windows Terminal identity, while `ms-terminal-direct` advertises direct RGB through the same generic color engine used by other profiles. `WT_SESSION`, `WT_PROFILE_ID`, and `COLORTERM` do not silently select or mutate any profile.

## Custom terminal providers

Applications can add descriptions without changing the built-in database or generic engines:

```csharp
TerminalDescription example = new TerminalDescriptionBuilder("example-terminal")
    .SetBoolean(BooleanCapability.AutoRightMargin)
    .SetNumber(NumericCapability.Columns, 80)
    .SetNumber(NumericCapability.Lines, 24)
    .SetExtendedBoolean("exampleFlag")
    .SetExtendedString("exampleString", "value")
    .Build();

ITerminalDescriptionProvider provider = new InMemoryTerminalDescriptionProvider(
    new[] { example }
);

TerminalDatabase database = new(new[] { provider });
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
TerminalDescription parsed = CompiledTermInfoParser.Parse(entry);
```

`CompiledTermInfoParserOptions` bounds accepted entry size. Malformed or
unsupported compiled data throws `CompiledTermInfoFormatException`; it is not
reported as a clean provider miss.

### Load an explicit directory

When an application owns a conventional terminfo directory tree, use
`DirectoryTerminalDescriptionProvider`:

```csharp
ITerminalDescriptionProvider applicationTermInfo = new DirectoryTerminalDescriptionProvider(
    "/opt/myapp/share/terminfo"
);

TerminalDescription terminal = new TerminalDatabase(
    new[] { applicationTermInfo }
)
    .Load("my-terminal");
```

The provider performs exact-name lookup only and propagates malformed-entry,
permission, and I/O failures.

### Construct a restricted system provider

Each system discovery category can be disabled independently:

```csharp
SystemTerminalDescriptionProvider restricted = new(
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
SystemTerminalDescriptionProvider system = new();
```

On non-Windows systems this can search encoded/directory `TERMINFO`, the
user-local `.terminfo` database, `TERMINFO_DIRS`, and frozen platform defaults.
Windows does not invent Unix-style implicit roots; explicit `TERMINFO`,
`TERMINFO_DIRS`, and `DirectoryTerminalDescriptionProvider` remain available.

### Compose system and built-in fallback

`TerminalDatabase` is itself an `ITerminalDescriptionProvider`, so built-in
fallback remains explicit:

```csharp
TerminalDatabase database = new(
    new ITerminalDescriptionProvider[] {
        new SystemTerminalDescriptionProvider(),
        TerminalDatabase.BuiltIn,
    }
);
```

The first provider which resolves the requested name wins.
`TerminalDatabase.BuiltIn` is never mutated by system discovery.

## Sample applications

The repository contains three executable API samples plus one command-suite
walkthrough with deliberately different purposes.

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

All three executable API sample projects target `net8.0`, `net9.0`, and
`net10.0`; `dotnet run` therefore needs an explicit framework. Run the ordinary
demonstration with:

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

### Reusable library-toolchain sample

`samples/Icod.TermInfo.Toolchain.Sample` demonstrates the reusable post-1.0
library stack without invoking the command layer. It parses and resolves a
controlled `.ti` source document, synthesizes the child relative to its base,
reparses and resolves the synthesized source, compiles and publishes it into a
temporary conventional database, reloads the child through the Runtime provider,
and verifies semantic equality through Inspection.

Run it with:

```text
dotnet run --project samples/Icod.TermInfo.Toolchain.Sample/Icod.TermInfo.Toolchain.Sample.csproj -f net10.0
```

The sample is deterministic and does not inspect the host terminfo database. It
is executed by release validation on Windows, Linux, and macOS. Use `-f net8.0`
or `-f net9.0` when exercising those reusable-library target frameworks.

See `samples/Icod.TermInfo.Toolchain.Sample/README.md` for the complete flow.

### Managed tool-suite walkthrough

`samples/ToolSuite` is a data-and-command walkthrough for `tic`, `infocmp`, `toe`,
`captoinfo`, and `infotocap`. It uses controlled terminfo and termcap source files
and an explicit local database root so validation, publication, rendering,
comparison, relative synthesis through `infocmp -u`, enumeration,
explicit-candidate planning through `infocmp --plan-use`, forward/reverse `use=`
dependency reporting, and bidirectional conversion do not depend on
host-installed terminfo or termcap databases. The planning walkthrough includes
an inferior decoy, direct and routed forms, and `tic -c` validation of the
selected source.

See `samples/README.md`, `samples/ToolSuite/README.md`,
`samples/Icod.TermInfo.Acquisition.Sample/README.md`,
`samples/Icod.TermInfo.Toolchain.Sample/README.md`, and
`docs/0.9.0-ACQUISITION-GUIDE.md` for the complete examples.

## Project-family boundary

`Icod.TermInfo` owns immutable terminal-description data, acquisition of that data, and pure transformations required to interpret, expand, and output terminal capabilities. `Icod.TermInfo.Source` owns optional source-language parsing and inheritance resolution, `Icod.TermInfo.Compiler` owns compiled output, `Icod.TermInfo.Inspection` owns canonical rendering and semantic comparison, and `Icod.TermInfo.Termcap` owns optional termcap interoperability. None of those packages owns a live terminal session, a child pseudo-terminal, or a virtual screen.

The intended family boundary is now explicit:

- **`Icod.TermInfo`** — descriptions, compiled-database acquisition, capability semantics, parameter expansion, and output transformation;
- **`Icod.TermInfo.Source`** — `.ti` lexical analysis, source diagnostics, unresolved entries, cancellation, `use=` inheritance, and materialization into `TerminalDescription`;
- **`Icod.TermInfo.Compiler`** — deterministic compiled-entry writing, source compilation, and explicit conventional database-layout publication;
- **`Icod.TermInfo.Inspection`** — canonical effective/source rendering, relative-source synthesis and parent planning, structured semantic comparison, and provider-aware inspection;
- **`Icod.TermInfo.Termcap`** — bounded termcap parsing, classification, `tc=` resolution, Runtime conversion, reverse rendering, and explicit termcap acquisition;
- **`tic`, `infocmp`, `toe`, `captoinfo`, and `infotocap`** — managed command applications which compose the reusable libraries and own command-line policy;
- **`Icod.TermInfo.Tools` / `icod-terminfo`** — distribution-only .NET tool router which dispatches to the five command applications;
- **`Icod.Terminal`** — sibling live-terminal/session layer for modes, input decoding, keyboard/mouse/paste/focus events, active probing/negotiation, and reversible presentation lifecycle;
- **future `Icod.Pty`** — Unix PTY and Windows ConPTY creation, resize propagation, and child-process plumbing;
- **`Icod.DCurses`** — sibling curses-like virtual-screen/window layer above `Icod.Terminal` and `Icod.TermInfo`.

Current post-1.0 package-family ownership and future release planning are governed
by `Icod.TermInfo-Post-1.0-Development-Roadmap.md`.

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
API/package freeze.

## Build, test, and pack

```text
dotnet restore Icod.TermInfo.sln

dotnet build Icod.TermInfo.sln -c Debug
dotnet test Icod.TermInfo.sln -c Debug

dotnet build Icod.TermInfo.sln -c Staging
dotnet test Icod.TermInfo.sln -c Staging
dotnet pack Icod.TermInfo.csproj -c Staging --output artifacts
dotnet pack Icod.TermInfo.Source/Icod.TermInfo.Source.csproj -c Staging --output artifacts
dotnet pack Icod.TermInfo.Termcap/Icod.TermInfo.Termcap.csproj -c Staging --output artifacts
dotnet pack Icod.TermInfo.Compiler/Icod.TermInfo.Compiler.csproj -c Staging --output artifacts
dotnet pack Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj -c Staging --output artifacts
dotnet pack icod-terminfo/Icod.TermInfo.Router.csproj -c Staging --output artifacts

dotnet build Icod.TermInfo.sln -c Release
dotnet test Icod.TermInfo.sln -c Release
dotnet pack Icod.TermInfo.csproj -c Release --output artifacts
dotnet pack Icod.TermInfo.Source/Icod.TermInfo.Source.csproj -c Release --output artifacts
dotnet pack Icod.TermInfo.Termcap/Icod.TermInfo.Termcap.csproj -c Release --output artifacts
dotnet pack Icod.TermInfo.Compiler/Icod.TermInfo.Compiler.csproj -c Release --output artifacts
dotnet pack Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj -c Release --output artifacts
dotnet pack icod-terminfo/Icod.TermInfo.Router.csproj -c Release --output artifacts
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

Both wrappers retain the coordinated five-library release verifier: generated
capability metadata, all five public-API baselines, net8/net9/net10 API
equivalence, package/XML/symbol/dependency validation, all five isolated
package-reference-only smoke consumers, the sample's non-interactive
`--describe-only` path, the deterministic reusable toolchain sample, and
structural validation of the sixth registry package, `Icod.TermInfo.Tools`.
The separate `smoke-tool-package.ps1` gate installs and executes that router
package on each supported host family. Windows package validation does not
require Bash or Python.

Pull requests use Staging throughout and may upload verified package artifacts,
but never publish. Pushes to `main` run the non-publishing Release validation
matrix. Only an immutable `v*` tag matching the coordinated package version may
start registry publication through `.github/workflows/release.yaml`.

See `RELEASING.md` for the current release procedure,
`Icod.TermInfo-1.4.0-Tool-Suite-Roadmap.md` for the frozen T01-T11 command
semantic contract, `docs/1.5.0-RELEASE-AUDIT.md` for the published 1.5
distribution/versioning gate,
`docs/1.6.0-TC08-DIFFERENTIAL-VALIDATION-FUZZING-AND-FREEZE.md` for frozen 1.6
pre-release closure evidence, `docs/1.6.0-RELEASE-AUDIT.md` for the published
1.6.0 contract and post-publication record, and `docs/1.6.1-RELEASE-AUDIT.md`
for the release-verifier isolation hotfix and 1.6.1 publication gate. The 1.7
release contract is defined by
`Icod.TermInfo 1.7.0 - Relative Terminfo Source Synthesis Roadmap.md`. The 1.8
planning contract is defined by
`Icod.TermInfo-1.8.0-Relative-Source-Planning-and-Parent-Selection-Roadmap.md`,
and its publication gate is recorded in `docs/1.8.0-RELEASE-AUDIT.md`.

## Scope

`Icod.TermInfo` is not curses, a terminal emulator, a PTY implementation, a termios session manager, an input-event parser, or a general terminal UI toolkit. It intentionally carries low-level descriptive data which those higher-level systems may consume. Source, Compiler, Inspection, and Termcap remain optional sibling layers and do not change those runtime boundaries.

See `Icod.TermInfo-Development-Roadmap-0.9.0.md` for the frozen acquisition
contract, `Icod.TermInfo-Development-Roadmap-1.0.0.md` for the 1.0 runtime
stability contract, `Icod.TermInfo-Post-1.0-Development-Roadmap.md` for the
post-1.0 package-family sequence, `Icod.TermInfo-1.3.0-Inspection-and-Comparison-Roadmap.md`
for the 1.3 Inspection contract,
`Icod.TermInfo-1.4.0-Tool-Suite-Roadmap.md` for the frozen 1.4 command contract,
`docs/1.6.0-RELEASE-AUDIT.md` for the frozen 1.6.0 release contract,
`docs/1.6.1-RELEASE-AUDIT.md` for the published patch-release contract,
`Icod.TermInfo 1.7.0 - Relative Terminfo Source Synthesis Roadmap.md` and
`docs/1.7.0-RELEASE-AUDIT.md` for the frozen 1.7 synthesis contract, and
`Icod.TermInfo-1.8.0-Relative-Source-Planning-and-Parent-Selection-Roadmap.md`
and `docs/1.8.0-RELEASE-AUDIT.md` for the current 1.8 planning and release
contracts. See `docs/VERSIONING.md` and `docs/COMPATIBILITY.md` for the 1.x
promises.
The 0.6.0 through 1.0.0 roadmaps remain historical frozen contracts.

## Authors

Inspired by original work from Bill Joy, author of the original `termcap`; Mary Ann (born Mark) Horton, author of `terminfo`; Pavel Curtis, author of `pcurses`; and Zeyd Ben-Halim, Eric S. Raymond, and Thomas Dickey, whose work developed and maintained `libtinfo` and ncurses.

Managed .NET implementation by Timothy J. Bruce <uniblab@hotmail.com>.

## Copyright

Copyright (c) 2026 Timothy J. Bruce

## License

Licensed under the GNU Lesser General Public License v3.0 or later. See `LICENSE`.
