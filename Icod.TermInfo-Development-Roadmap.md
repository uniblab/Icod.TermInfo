# Icod.TermInfo Development Roadmap

This unversioned document is the **master roadmap index**. The original 0.6.0
planning body is preserved below for historical context, while active and later
version contracts live in version-specific roadmap files.

## Version contracts

- `Icod.TermInfo Development Roadmap — Version 0.6.0 Contract.md` — foundational
  managed terminfo contract; complete and frozen.
- `Icod.TermInfo-Development-Roadmap-0.7.0.md` — modern xterm, extended
  capabilities, color, and descriptive protocol expansion; complete and frozen.
- `Icod.TermInfo-Development-Roadmap-0.8.0.md` — semantic completion, Windows
  profiles, binary/provider readiness, and final 0.8 contract; complete and
  frozen.
- `Icod.TermInfo-Development-Roadmap-0.9.0.md` — arbitrary compiled terminfo
  acquisition: pure binary parser, explicit directory provider,
  environment/system discovery, and provider-local cache/refresh semantics;
  complete and frozen.
- `Icod.TermInfo-Development-Roadmap-1.0.0.md` — active 1.0 readiness contract:
  API-regret audit, dual-target robustness/compatibility, documentation/package
  freeze, and final completion gate.

## Roadmap sequence

```text
0.6.0  foundation
  -> 0.7.0  modern descriptive capability model
  -> 0.8.0  semantic completion
  -> 0.9.0  arbitrary compiled acquisition
  -> 1.0.0  stable public contract
```

The 1.0 readiness work deliberately does **not** absorb every remaining terminal
feature. Terminfo source-language tooling, termcap compatibility, hashed
Berkeley-DB stores, live terminal sessions/input/probing, PTYs, curses/virtual
screens, graphics protocols, and terminal emulation remain separate future-work
families. Their dependencies and likely package homes are recorded in
`docs/FUTURE-WORK-INVENTORY.md`.

The remainder of this file preserves the original 0.6.0 planning document.

---

## Version 0.6.0 Contract

**Project:** `Icod.TermInfo`  
**Package:** `Icod.TermInfo`  
**Target framework:** `net10.0`  
**Language:** C# 13  
**Status:** Planned / initial implementation  
**Contract target:** `0.6.0`

---

## 1. Purpose

`Icod.TermInfo` is a small, dependency-free managed .NET library implementing the low-level terminal capability model traditionally provided by `libtinfo`.

The initial project intentionally supports only:

- ANSI / ECMA-48-style terminals as represented by the traditional color-capable `ansi` terminfo profile;
- DEC VT100 terminals;
- a minimal `dumb` terminal profile for safe fallback behavior.

The library is not intended to reproduce the whole of ncurses, curses, or the historical terminfo database. Its purpose is to give .NET applications a compact, portable abstraction for terminal capabilities, parameterized terminal control strings, padding-aware output, terminal sizing, and Windows virtual-terminal support.

The package is intended for reuse by command-line utilities, process-monitoring tools, line editors, pagers, interactive applications, and other software which needs terminal control without taking a dependency on ncurses or implementing ANSI escape sequences independently.

Other terminal families may be added by contributors after the 0.6.0 contract, provided that they fit the capability architecture defined here.

---

## 2. Design Principles

The 0.6.0 implementation SHALL follow these principles.

### 2.1 Managed and dependency-free

The library SHALL:

- be implemented in managed C# except for narrowly scoped platform interop required for terminal sizing or Windows console mode management;
- have no runtime dependency on ncurses, curses, `libtinfo`, `libtermcap`, or another native terminal library;
- have no third-party runtime dependencies;
- operate on Windows, Linux, and macOS.

### 2.2 Terminfo semantics, not an escape-code helper

The library SHALL model terminal capabilities rather than expose only a collection of ANSI escape-code constants.

Applications should be able to ask a terminal description for a capability such as cursor addressing, clear-screen, bold text, or a cursor-key sequence without knowing the underlying byte sequence.

ANSI and VT100 SHALL therefore be expressed as terminal descriptions using the same capability model that future terminal providers can use.

### 2.3 Explicit terminal identity

`xterm`, `xterm-256color`, `screen`, `tmux`, `linux`, `rxvt`, `cygwin`, and other terminal names SHALL NOT silently resolve to `ansi` or `vt100`.

An application may explicitly request an ANSI fallback, but `Icod.TermInfo` SHALL NOT claim that an unsupported terminal is equivalent to a supported terminal.

### 2.4 No process-global `cur_term`

The primary API SHALL be instance-based.

The implementation SHALL NOT depend on a process-global current terminal analogous to the C `cur_term` variable.

This permits:

- multiple terminal descriptions in one process;
- deterministic tests;
- use by terminal servers, SSH software, pseudo-terminal software, and serial-console applications;
- straightforward thread safety.

A compatibility-shaped API may be provided, but it SHALL remain bound to an explicit terminal instance.

### 2.5 Immutable terminal descriptions

Resolved built-in terminal descriptions SHALL be immutable and safe for concurrent readers.

Terminal capability definitions SHALL NOT be mutated as a side effect of output, parameter expansion, environment detection, or Windows console configuration.

### 2.6 Capability defaults are not live terminal state

A profile's `cols` and `lines` values describe the terminal definition.

They SHALL NOT be treated as the current terminal window dimensions when live dimensions can be determined from the operating system.

---

## 3. Scope of the 0.6.0 Contract

### 3.1 Included

Version 0.6.0 SHALL provide:

- boolean terminfo capabilities;
- numeric terminfo capabilities;
- string terminfo capabilities;
- lookup by traditional short terminfo capability name;
- typed capability identifiers for normal managed use;
- parameterized string expansion compatible with the terminfo stack-expression model;
- `tputs`-style padding parsing and output;
- ANSI terminal description;
- VT100 terminal description;
- `dumb` terminal description;
- terminal-name resolution;
- explicit environment-based terminal selection;
- current terminal-window size querying;
- Windows virtual-terminal output mode helpers;
- input key-sequence capabilities where they belong to the terminal profile;
- synchronous output helpers;
- asynchronous output helpers where delay handling would otherwise block;
- NuGet packaging;
- GitHub Packages publishing;
- symbol packages;
- Source Link;
- cross-platform CI.

### 3.2 Explicitly excluded

Version 0.6.0 SHALL NOT implement:

- curses windows;
- pads;
- panels;
- menus;
- forms;
- screen refresh optimization;
- terminal emulation;
- pseudo-terminal management;
- `termios`;
- a keyboard-event decoder;
- mouse-event decoding;
- compiled system terminfo database loading;
- `/usr/share/terminfo` database discovery;
- `TERMINFO`;
- `TERMINFO_DIRS`;
- `~/.terminfo`;
- termcap database parsing;
- `tic`;
- `infocmp`;
- xterm-specific capabilities;
- xterm 256-color behavior;
- true-color behavior;
- OSC 8 hyperlinks;
- OSC 52 clipboard operations;
- Sixel;
- Kitty graphics or keyboard protocols;
- bracketed-paste processing;
- terminal probing or active device-identification handshakes;
- automatic mapping of unsupported terminal names to ANSI or VT100.

These features may be considered after 0.6.0, but none is required to complete this contract.

---

## 4. Built-In Terminal Profiles

### 4.1 `ansi`

`ansi` is the built-in color-capable ANSI profile.

The intent is to follow the traditional terminfo meaning of `ansi`: an ANSI/pc-term-compatible terminal with standard cursor movement, screen erasure, rendition controls, and the classic eight-color palette.

The profile SHALL include, where applicable:

- 80-column and 24-line capability defaults;
- automatic margins;
- cursor addressing;
- relative cursor movement;
- cursor save and restore;
- erase display;
- erase line;
- insert/delete character operations;
- insert/delete line operations;
- tab operations;
- standard rendition controls;
- normal cursor-key strings;
- eight foreground colors;
- eight background colors;
- default color restoration.

The color model SHALL be limited to the traditional eight ANSI colors.

The 0.6.0 contract SHALL NOT advertise 16-color, 256-color, or 24-bit true-color capability.

### 4.2 `vt100`

`vt100` is the DEC VT100 profile in normal 80-column operation with advanced-video capabilities.

The profile SHALL model the VT100 as monochrome.

The profile SHALL include, where applicable:

- `cols = 80`;
- `lines = 24`;
- automatic margins;
- VT100 cursor movement;
- VT100 cursor addressing;
- clear-screen and erase-line behavior;
- scrolling-region control;
- bold;
- blink;
- reverse video;
- underline;
- alternate character-set support;
- keypad/application-mode controls;
- VT100 cursor-key sequences;
- PF-key capabilities;
- traditional VT100 padding annotations.

The profile SHALL NOT advertise ANSI color capabilities.

`vt100-am` MAY be accepted as an alias of the canonical `vt100` built-in profile.

Other VT100 variants, including no-automargin, wide-column, status-line, and no-advanced-video variants, are outside the 0.6.0 contract unless added before release without expanding the architectural surface.

### 4.3 `dumb`

`dumb` is a deliberately minimal profile used when the caller wants a safe terminal with effectively no screen-control abilities.

It SHALL provide only capabilities which are harmless and meaningful for a simple output stream.

It SHALL NOT advertise cursor addressing, color, screen editing, or advanced video attributes.

### 4.4 Profile extensibility

Built-in profiles SHALL be implemented through the same description/builder/provider mechanisms available to future contributors.

The core library SHALL NOT contain ANSI- or VT100-specific branches in generic capability lookup, parameter expansion, or output processing.

---

## 5. Capability Model

Capabilities SHALL be represented in the three traditional terminfo categories:

1. boolean;
2. numeric;
3. string.

The implementation SHALL preserve traditional short capability names for compatibility while also providing typed managed identifiers.

### 5.1 Boolean capability examples

The initial capability catalog should include at least the capabilities needed by the built-in profiles, including candidates such as:

- `am` — automatic right margin;
- `msgr` — safe cursor movement in standout mode;
- `xenl` — newline behavior at the right margin;
- `xon` — terminal uses XON/XOFF flow control.

### 5.2 Numeric capability examples

The initial capability catalog should include at least:

- `cols`;
- `lines`;
- `colors`;
- `pairs`;
- `it`;
- `vt`.

A capability which is not present SHALL remain absent; the library SHALL NOT synthesize a false value which could be confused with a real terminal capability.

### 5.3 String capability examples

The initial catalog should cover the strings required by ANSI and VT100, including as applicable:

- `bel`;
- `blink`;
- `bold`;
- `cbt`;
- `clear`;
- `cr`;
- `csr`;
- `cub`;
- `cub1`;
- `cud`;
- `cud1`;
- `cuf`;
- `cuf1`;
- `cup`;
- `cuu`;
- `cuu1`;
- `dch`;
- `dch1`;
- `dl`;
- `dl1`;
- `ed`;
- `el`;
- `el1`;
- `home`;
- `hpa`;
- `ht`;
- `hts`;
- `ich`;
- `ich1`;
- `il`;
- `il1`;
- `ind`;
- `invis`;
- `op`;
- `rc`;
- `rev`;
- `ri`;
- `rmacs`;
- `rmam`;
- `rmkx`;
- `rmso`;
- `rmul`;
- `sc`;
- `setab`;
- `setaf`;
- `sgr`;
- `sgr0`;
- `smacs`;
- `smam`;
- `smkx`;
- `smso`;
- `smul`;
- `vpa`.

Input-oriented string capabilities SHALL include the key sequences actually defined by the supported profiles, including cursor keys, backspace/home where applicable, and VT100 PF keys.

### 5.4 Typed capability identifiers

The managed API should expose typed identifiers along the lines of:

```csharp
BooleanCapability.AutoRightMargin
NumericCapability.Columns
NumericCapability.Lines
StringCapability.ClearScreen
StringCapability.CursorAddress
StringCapability.EnterBoldMode
StringCapability.ExitAttributeMode
StringCapability.SetForegroundColor
StringCapability.SetBackgroundColor
```

The mapping between typed identifiers and traditional terminfo names SHALL be deterministic and tested.

### 5.5 Unknown capabilities

String-based lookup SHALL distinguish between:

- a valid capability name which is absent from a terminal profile; and
- an unknown capability name which the library does not recognize.

The managed API should favor `TryGet...` methods and nullable results where appropriate.

A compatibility layer MAY reproduce familiar `tiget*`-style sentinel semantics, but sentinel values SHALL NOT be required by the primary managed API.

---

## 6. Proposed Managed API

The exact public names may be refined before the T8 public-API gate, but the architecture SHALL remain equivalent to the following.

```csharp
namespace Icod.TermInfo;

public sealed class TerminalDescription
{
    public string Name { get; }

    public IReadOnlyList<string> Aliases { get; }

    public bool TryGetBoolean(string name, out bool value);

    public bool TryGetNumber(string name, out int value);

    public bool TryGetString(
        string name,
        [NotNullWhen(true)] out string? value);

    public bool GetBoolean(BooleanCapability capability);

    public int? GetNumber(NumericCapability capability);

    public string? GetString(StringCapability capability);

    public string GetRequiredString(StringCapability capability);

    public string Expand(
        StringCapability capability,
        params TermInfoParameter[] parameters);
}
```

Terminal resolution should resemble:

```csharp
TerminalDescription terminal =
    TerminalDatabase.Load("vt100");

if (TerminalDatabase.TryLoad(name, out TerminalDescription? found))
{
    // Use found.
}
```

An explicit fallback should resemble:

```csharp
TerminalDescription terminal =
    TerminalDatabase.Resolve(
        requestedName,
        fallback: TerminalProfiles.Ansi);
```

No fallback SHALL occur merely because the requested name contains words such as `xterm`, `screen`, or `linux`.

---

## 7. Terminal Providers

The architecture SHALL permit terminal descriptions to be supplied by providers.

A provider abstraction should resemble:

```csharp
public interface ITerminalDescriptionProvider
{
    bool TryLoad(
        string name,
        [NotNullWhen(true)] out TerminalDescription? terminal);
}
```

The built-in provider SHALL resolve the profiles supplied with the package.

A future package or application should be able to add support for another terminal family without modifying the parameter engine or output layer.

Provider lookup SHALL be deterministic.

Provider registration SHALL NOT require mutable process-global state. If a registry abstraction is provided, callers should own or explicitly configure it.

---

## 8. Parameterized String Expansion

Parameterized terminfo strings are a core part of the 0.6.0 contract.

The implementation SHALL provide a real stack-based terminfo parameter evaluator rather than special-case ANSI cursor-position strings.

The evaluator SHALL be reusable independently of a terminal profile.

### 8.1 Required expression features

The evaluator SHALL support the terminfo operations required by the built-in definitions and SHOULD support the complete commonly documented expression set:

- `%%`;
- formatted numeric output;
- `%c`;
- `%s`;
- `%p1` through `%p9`;
- `%P` variable assignment;
- `%g` variable retrieval;
- character constants;
- integer constants;
- `%l`;
- `%+`;
- `%-`;
- `%*`;
- `%/`;
- `%m`;
- `%&`;
- `%|`;
- `%^`;
- `%=`;
- `%>`;
- `%<`;
- `%A`;
- `%O`;
- `%!`;
- `%~`;
- `%i`;
- `%?`;
- `%t`;
- `%e`;
- `%;`.

The formatter SHALL support the width/precision forms needed by standard terminfo parameterized strings.

### 8.2 Parameter representation

Parameters SHALL have an explicit managed representation capable of holding at least:

- integer values;
- string values.

A type such as `TermInfoParameter` is preferred over an untyped `object[]` public API.

Convenience overloads MAY be provided for the common all-integer case.

### 8.3 Expansion context

The evaluator SHALL NOT use hidden global variables.

If persistent "static" terminfo variables are supported across expansions, persistence SHALL belong to an explicit caller-owned expansion context.

Dynamic variables SHALL be scoped to an expansion invocation.

### 8.4 Error handling

Malformed parameter programs SHALL produce deterministic managed errors.

Error cases SHALL include:

- stack underflow;
- invalid parameter index;
- unterminated conditional;
- invalid format expression;
- invalid constant;
- invalid variable name;
- type mismatch;
- impossible conversion.

The implementation SHALL NOT silently emit a partially expanded control string after an evaluation error.

### 8.5 Parser design

The evaluator SHOULD be split into distinct responsibilities:

```text
parameter string
    |
    v
tokenizer/parser
    |
    v
parameter program
    |
    v
stack evaluator
    |
    v
expanded string
```

A direct interpreter is acceptable if it remains testable and preserves equivalent separation of concerns.

---

## 9. Padding and Output

Terminfo strings may contain delay annotations such as `$<2>` or `$<50>`.

Version 0.6.0 SHALL parse these annotations.

### 9.1 Padding modes

The library SHALL provide at least:

```csharp
PaddingMode.Ignore
PaddingMode.Delay
```

`Ignore` SHALL be the normal modern default.

Ignoring padding means removing the padding annotation while emitting the terminal-control sequence itself. The `$<...>` text SHALL never be written literally to the terminal.

### 9.2 Affected-line multiplier

`tputs`-style padding which depends on the number of affected lines SHALL accept an affected-line count.

The parser SHALL support the standard multiplicative marker used in terminfo padding expressions.

### 9.3 Mandatory delays

The parser SHOULD retain enough information to distinguish ordinary and mandatory padding markers even if the initial policies treat them similarly.

### 9.4 Output destinations

Output helpers SHALL support at least:

- `TextWriter`;
- `Stream` or another byte-oriented output path where needed;
- a delegate/callback form suitable for compatibility-style `tputs`.

The library SHOULD provide asynchronous output when real delays are enabled so applications are not forced to block a thread.

### 9.5 Encoding

Terminal capability strings SHALL be treated as control sequences, not localized text.

The output layer SHALL avoid transforming escape/control bytes through culture-sensitive operations.

Application text encoding is the application's responsibility and is outside the terminfo capability model.

---

## 10. Environment and Terminal Resolution

### 10.1 `$TERM`

The environment helper MAY inspect `TERM`, but terminal resolution SHALL remain explicit and conservative.

The following SHALL resolve when built into the library:

- `ansi`;
- `vt100`;
- supported aliases of `vt100`;
- `dumb`.

An unrecognized terminal name SHALL NOT automatically resolve as `ansi`.

### 10.2 Missing terminal identity

When terminal identity is unavailable, the caller SHALL be able to choose among:

- receiving no resolved profile;
- receiving an exception from a required-load API;
- supplying an explicit fallback;
- deliberately selecting `dumb`.

### 10.3 Redirected output

The library SHALL expose whether the relevant standard stream is redirected when that information is available.

Environment detection SHALL NOT emit control sequences as a side effect.

### 10.4 No active probing

Version 0.6.0 SHALL NOT send device-attribute requests or other identification sequences to the terminal merely to determine what terminal is attached.

---

## 11. Terminal Window Size

Runtime window size SHALL be modeled separately from profile dimensions.

A value type should resemble:

```csharp
public readonly record struct TerminalSize(
    int Columns,
    int Rows);
```

The environment API should provide a `TryGet...` form.

### 11.1 Windows

Windows sizing MAY use the Console API or narrowly scoped Win32 interop.

### 11.2 Unix-like systems

Linux and macOS sizing MAY use a narrowly scoped `ioctl(TIOCGWINSZ)` interop implementation.

### 11.3 Fallbacks

If live size cannot be determined, the caller MAY explicitly request fallback to:

1. configured environment dimensions, if supported;
2. terminal profile defaults.

Profile values SHALL NOT silently masquerade as a successfully queried live size.

---

## 12. Windows Virtual-Terminal Support

The package SHALL include a small Windows-specific helper for ANSI/VT output processing.

The helper SHALL:

- detect whether the target is a Windows console handle;
- detect redirection;
- enable virtual-terminal output processing when possible;
- preserve unrelated console-mode flags;
- report failure without crashing when VT processing cannot be enabled;
- permit restoration of the previous mode.

An API should resemble:

```csharp
using IDisposable? mode =
    WindowsVirtualTerminal.TryEnableOutput();
```

or an equivalent explicit scope object.

Enabling VT mode SHALL NOT happen merely because a terminal description was loaded.

The application controls when process console state is modified.

---

## 13. Input Capabilities

Input strings such as cursor keys are valid terminfo capabilities and SHALL be represented when defined by a supported profile.

For example, a caller may retrieve the terminal sequence corresponding to:

- cursor up;
- cursor down;
- cursor left;
- cursor right;
- backspace;
- home;
- VT100 PF keys.

Version 0.6.0 SHALL NOT provide an input-event decoder.

Turning an incoming stream such as `ESC O A` into a managed key event belongs to a future layer.

This boundary keeps `Icod.TermInfo` focused on terminal information rather than terminal input policy.

---

## 14. Compatibility-Shaped API

A lower-level compatibility surface MAY be provided for code being ported from C.

It may expose concepts corresponding to:

- `tigetflag`;
- `tigetnum`;
- `tigetstr`;
- `tparm`;
- `tiparm`;
- `tputs`;
- `putp`.

The managed spelling does not have to reproduce every C ABI convention.

The compatibility API SHALL:

- remain managed;
- take or belong to an explicit terminal instance;
- avoid `(char *)-1`-style sentinel tricks;
- avoid process-global `cur_term`;
- never require native ncurses.

The primary public API remains the idiomatic managed API.

---

## 15. Repository Layout

The initial repository should use:

```text
Icod.TermInfo/
|
+-- .github/
|   +-- workflows/
|       +-- build.yml
|       +-- publish.yml
|
+-- src/
|   +-- Icod.TermInfo/
|       +-- Icod.TermInfo.csproj
|       +-- Capabilities/
|       +-- Environment/
|       +-- Output/
|       +-- Parameterization/
|       +-- Platform/
|       +-- Profiles/
|       +-- Providers/
|
+-- tests/
|   +-- Icod.TermInfo.Tests/
|       +-- Icod.TermInfo.Tests.csproj
|       +-- Capabilities/
|       +-- Environment/
|       +-- Output/
|       +-- Parameterization/
|       +-- Platform/
|       +-- Profiles/
|
+-- samples/
|   +-- Icod.TermInfo.Sample/
|
+-- Icod.TermInfo.sln
+-- Directory.Build.props
+-- Directory.Packages.props
+-- README.md
+-- CONTRIBUTING.md
+-- LICENSE
+-- Icod.TermInfo-Development-Roadmap.md
```

Additional folders should be introduced only when they improve separation of concerns.

The initial package SHALL remain a single package rather than being split into `Core`, `Abstractions`, `Ansi`, or similar micro-packages.

---

## 16. Build and Coding Policy

The repository SHALL use:

- `net10.0`;
- C# 13;
- nullable reference types;
- deterministic builds;
- repository UTF-8 text;
- LF line endings;
- Release builds treating warnings as errors, except intentionally suppressed documentation warnings where the repository policy permits them;
- braces for all `if`, `else`, loop, and similar control-flow bodies;
- parameter validation at public/protected/internal API boundaries;
- culture-invariant handling of terminal control syntax.

Tests SHALL NOT write to standard output or standard error except when output is the specific behavior under test or is required to communicate with a child process.

Platform-specific code SHALL be isolated behind small internal abstractions.

---

## 17. Test Strategy

The test suite is part of the contract, not an afterthought.

### 17.1 Capability-table tests

Every built-in profile SHALL have golden tests covering every capability it advertises.

Tests SHALL verify:

- canonical name;
- aliases;
- all booleans;
- all numerics;
- all strings;
- absence of capabilities which the profile must not advertise.

Examples:

- `ansi` advertises eight colors;
- `vt100` does not advertise color;
- `vt100` reports its profile defaults as 80×24;
- `dumb` does not advertise cursor addressing.

### 17.2 Parameter-engine tests

The parameter evaluator SHALL receive direct tests for every supported operator.

Tests SHALL include:

- single and multiple parameters;
- `%i`;
- arithmetic;
- nested conditionals;
- string parameters;
- character output;
- formatted numbers;
- variables;
- malformed expressions;
- stack underflow;
- type mismatch;
- empty strings;
- boundary numeric values.

### 17.3 Golden escape-sequence tests

Tests SHALL verify exact expanded strings for important capabilities such as:

- `cup`;
- `clear`;
- `home`;
- `el`;
- `ed`;
- bold;
- underline;
- reverse video;
- reset;
- ANSI foreground color;
- ANSI background color;
- VT100 keypad mode;
- VT100 cursor keys.

Tests SHALL compare exact characters, including ESC and control characters.

### 17.4 Padding tests

Tests SHALL cover:

- no padding;
- integer delays;
- fractional delays if supported;
- affected-line multiplication;
- mandatory markers;
- ignored padding;
- actual delay policy through an injectable/testable delay abstraction;
- malformed padding expressions.

Tests SHALL NOT depend on real wall-clock sleeps when a fake delay provider can be used.

### 17.5 Environment tests

Environment tests SHALL cover:

- recognized `TERM`;
- missing `TERM`;
- unknown `TERM`;
- explicit fallback;
- `dumb`;
- redirected streams where practical.

No test SHALL depend on the CI runner's actual terminal type.

### 17.6 Platform tests

Windows tests SHALL cover:

- non-console handles;
- redirected output;
- successful VT-mode enablement where supported;
- preservation/restoration of prior console flags.

Linux/macOS tests SHALL cover live-size query behavior where a TTY is available and graceful failure where one is not.

CI tests SHALL remain deterministic when no interactive TTY exists.

---

## 18. Continuous Integration

The normal build workflow SHALL run on:

- `windows-latest`;
- `ubuntu-latest`;
- `macos-latest`.

CI SHALL perform at least:

1. restore;
2. build Debug;
3. test Debug;
4. build Release;
5. test Release;
6. package validation or package construction where appropriate.

Release builds SHALL fail on unexpected compiler/analyzer warnings according to repository policy.

The package produced by CI SHALL be reproducible from the corresponding release commit as far as the .NET SDK permits.

---

## 19. Packaging

The initial public package SHALL be:

```text
Icod.TermInfo
```

The package SHALL target `net10.0`.

The package SHALL include:

- assembly metadata;
- repository metadata;
- package README;
- XML documentation if enabled by repository policy;
- Source Link information;
- deterministic source information;
- `.nupkg`;
- `.snupkg`.

The project SHALL enable .NET package validation before the 0.6.0 release.

The package SHALL contain no native library payload.

The package SHALL be suitable for publication to both:

- NuGet;
- GitHub Packages.

The same compiled release SHOULD be used for both feeds.

---

## 20. Versioning Policy

The first contract release described by this roadmap is `0.6.0`.

Development builds before the final contract gate SHOULD use prerelease versions such as:

```text
0.6.0-alpha.N
0.6.0-beta.N
0.6.0-rc.N
```

The public API may evolve during alpha development.

Beginning with the first release candidate, breaking public-API changes SHOULD require deliberate contract review.

The `0.6.0` tag freezes the public behavior described in this roadmap, subject to normal pre-1.0 semantic-versioning expectations.

---

# Implementation Roadmap

## T0 — Repository Foundation

### Work

- create `Icod.TermInfo.sln`;
- create `src/Icod.TermInfo/Icod.TermInfo.csproj`;
- create `tests/Icod.TermInfo.Tests/Icod.TermInfo.Tests.csproj`;
- create sample project;
- establish shared build properties;
- establish nullable and analyzer policy;
- establish LF/editor configuration;
- add README;
- add CONTRIBUTING;
- add license file;
- add three-OS CI;
- configure package metadata for prerelease `0.6.0`.

### Acceptance gate

T0 is complete when:

- the solution restores on all three supported operating systems;
- Debug and Release builds succeed;
- the empty test suite executes successfully;
- a prerelease `.nupkg` can be produced.

---

## T1 — Capability Core

### Work

Implement:

- capability type definitions;
- traditional short-name catalog;
- typed capability identifiers;
- immutable `TerminalDescription`;
- terminal-description builder used internally or publicly as appropriate;
- `ITerminalDescriptionProvider`;
- built-in provider skeleton;
- `TerminalDatabase`;
- absent-versus-unknown capability behavior.

### Acceptance gate

T1 is complete when:

- an in-memory terminal description can be created;
- boolean, numeric, and string capabilities can be queried;
- traditional-name and typed lookup agree;
- descriptions are immutable to consumers;
- provider lookup is deterministic;
- concurrency tests show safe parallel reads.

---

## T2 — Parameter Expansion Engine

### Work

Implement:

- `TermInfoParameter`;
- parser/tokenizer;
- stack evaluator;
- numeric formatting;
- string formatting;
- constants;
- arithmetic;
- bitwise operators;
- logical operators;
- `%i`;
- variables;
- conditionals;
- nested conditionals;
- explicit expansion context if needed;
- deterministic parse/evaluation errors.

### Acceptance gate

T2 is complete when:

- the full required operator test matrix passes;
- malformed programs fail deterministically;
- ANSI-style cursor addressing expands correctly;
- nested `sgr`-style conditionals can be evaluated;
- the engine contains no terminal-specific special cases.

---

## T3 — ANSI Profile

### Work

Implement the built-in color ANSI terminal description.

Include:

- profile identity;
- dimensions;
- movement;
- cursor addressing;
- erasure;
- line/character insertion and deletion;
- tabs;
- rendition;
- cursor keys;
- eight colors;
- color reset;
- appropriate query/identification strings as passive capabilities where desired.

Do not implement active terminal probing.

### Acceptance gate

T3 is complete when:

- every advertised ANSI capability has a golden test;
- `cup` and other parameterized strings use the T2 engine;
- `colors == 8`;
- `pairs == 64` if that value is adopted by the final profile table;
- 256-color and true-color capabilities are absent;
- unsupported terminal names still do not resolve to ANSI automatically.

---

## T4 — VT100 Profile

### Work

Implement the DEC VT100 description.

Include:

- 80×24 profile dimensions;
- margins;
- movement;
- cursor addressing;
- erasure;
- scrolling region;
- video attributes;
- alternate character set;
- application keypad controls;
- cursor-key strings;
- PF-key capabilities;
- VT100 delay annotations.

### Acceptance gate

T4 is complete when:

- every advertised VT100 capability has a golden test;
- no color capability is advertised;
- parameterized capabilities use the shared T2 engine;
- padding annotations are preserved for T5 processing;
- `vt100` and accepted aliases resolve deterministically.

---

## T5 — Padding and Output

### Work

Implement:

- padding parser;
- `PaddingMode`;
- affected-line multiplication;
- synchronous output;
- asynchronous delayed output;
- `TextWriter` output;
- byte/stream output where appropriate;
- compatibility callback output;
- injectable delay mechanism for tests;
- `putp`/`tputs`-shaped convenience behavior.

### Acceptance gate

T5 is complete when:

- padding markers are never emitted literally;
- ignore mode produces correct terminal sequences;
- delay mode invokes the expected delays;
- VT100 padded capabilities can be emitted correctly;
- tests require no real sleeping.

---

## T6 — Terminal Environment

### Work

Implement:

- `TERM` inspection;
- explicit environment resolution;
- unknown-terminal behavior;
- redirected-stream inspection;
- `TerminalSize`;
- live window-size query;
- Windows size provider;
- Linux/macOS `TIOCGWINSZ` provider;
- optional explicit fallback to configured/profile dimensions.

### Acceptance gate

T6 is complete when:

- environment selection never silently aliases an unknown terminal to ANSI;
- live size and profile size are distinct APIs;
- redirected/non-TTY scenarios fail gracefully;
- tests do not depend on the host CI terminal.

---

## T7 — Windows Virtual-Terminal Integration

### Work

Implement:

- Windows console-handle detection;
- VT output flag enablement;
- preservation of unrelated mode flags;
- restoration scope;
- redirected-output behavior;
- graceful unsupported/failure behavior.

### Acceptance gate

T7 is complete when:

- enabling VT mode is always explicit;
- terminal profile loading has no console-mode side effects;
- original console state can be restored;
- the implementation does nothing harmful to redirected output;
- non-Windows builds contain no accidental Windows-only API usage.

---

## T8 — Compatibility Surface and Public API Hardening

### Work

Review the full public surface.

Add or finalize compatibility-shaped equivalents for:

- `tigetflag`;
- `tigetnum`;
- `tigetstr`;
- `tparm` / `tiparm`;
- `tputs`;
- `putp`.

Review:

- naming;
- nullability;
- exception contracts;
- parameter validation;
- thread safety;
- allocations;
- XML documentation;
- API visibility;
- extension points;
- package namespace consistency.

Establish a public API baseline for package validation.

### Acceptance gate

T8 is complete when:

- every public member is intentional;
- no process-global current terminal is required;
- the compatibility surface uses managed semantics;
- public API validation is enabled;
- all public/protected/internal entry points validate parameters consistently;
- documentation clearly distinguishes profile dimensions from live dimensions.

---

## T9 — Documentation, Samples, and Packaging

### Work

Complete:

- README;
- getting-started example;
- ANSI example;
- VT100 example;
- explicit fallback example;
- cursor-position example;
- color example;
- Windows VT-mode example;
- terminal-size example;
- provider-extension example;
- CONTRIBUTING guidance for adding terminal profiles;
- package README;
- Source Link;
- symbols;
- NuGet metadata;
- GitHub Packages workflow;
- release workflow.

### Acceptance gate

T9 is complete when a new consumer can:

1. install the prerelease package;
2. load `ansi` or `vt100`;
3. move the cursor;
4. clear the screen;
5. apply attributes;
6. query live terminal size;
7. explicitly enable Windows VT output where necessary;
8. publish a small application without native ncurses present.

---

## T10 — 0.6.0 Completion Gate

Before tagging `0.6.0`, perform a final contract audit.

### Required checks

- all three operating-system CI jobs pass;
- Debug and Release tests pass;
- package validation passes;
- public API baseline is clean;
- package installs into a fresh sample project;
- no runtime package dependency has been introduced;
- no native ncurses dependency exists;
- `ansi` behavior is golden-tested;
- `vt100` behavior is golden-tested;
- `dumb` behavior is golden-tested;
- `ansi` advertises only the intended eight-color model;
- `vt100` advertises no color;
- unknown terminal names do not silently alias to supported profiles;
- live terminal size remains separate from profile dimensions;
- VT mode changes on Windows remain explicit;
- parameter expansion has no ANSI/VT100 special cases;
- output strips or processes all padding annotations;
- package symbols and Source Link function correctly;
- NuGet artifact is ready;
- GitHub Packages artifact is ready;
- README scope and non-goals match this contract.

### Completion

When every T10 item passes:

- tag `v0.6.0`;
- publish `Icod.TermInfo` 0.6.0 to NuGet;
- publish the same release to GitHub Packages;
- mark this roadmap's 0.6.0 contract complete.

---

# Post-0.6.0 Possibilities

The following are deliberately deferred and are not commitments.

## Additional terminal profiles

Contributors may add providers or profiles for:

- xterm;
- xterm-256color;
- Linux console;
- screen;
- tmux;
- rxvt;
- other DEC terminals;
- other historical hardware terminals.

Each profile should be capability-driven and should not require changes to the generic engine.

## System terminfo databases

A future version may add:

- compiled terminfo file parsing;
- `/usr/share/terminfo`;
- user terminfo directories;
- `TERMINFO`;
- `TERMINFO_DIRS`;
- source-format terminfo loading.

This should be introduced through a provider rather than coupled into `TerminalDescription`.

## Extended color

A future version may add:

- 16-color distinctions;
- 256-color indexed output;
- direct/true-color capabilities.

These SHALL NOT retroactively change the 0.6.0 meaning of the built-in `vt100` profile.

## Input decoding

A separate future layer may consume terminfo key strings and decode byte streams into managed key events.

This should remain separable from the terminal-description library.

## Modern terminal protocols

Modern extensions such as mouse tracking, bracketed paste, hyperlinks, clipboard protocols, Sixel, and Kitty protocols may be considered separately.

They should not be forced into traditional terminfo abstractions where a different abstraction is more appropriate.

---

# 0.6.0 Definition of Done

`Icod.TermInfo` 0.6.0 is complete when it is possible for a managed .NET application on Windows, Linux, or macOS to:

```csharp
TerminalDescription terminal =
    TerminalDatabase.Load("vt100");

string clear =
    terminal.GetRequiredString(StringCapability.ClearScreen);

string move =
    terminal.Expand(
        StringCapability.CursorAddress,
        10,
        20);
```

and emit those capabilities through the library's padding-aware output layer without depending on ncurses or another native terminal library.

It must also be possible to select the built-in ANSI profile and use its traditional eight-color capabilities, while a VT100 remains correctly monochrome.

The library must reject or explicitly report unsupported terminal identities rather than pretending that every modern terminal is ANSI or VT100.

That behavior is the central compatibility and scope boundary of the 0.6.0 contract.
