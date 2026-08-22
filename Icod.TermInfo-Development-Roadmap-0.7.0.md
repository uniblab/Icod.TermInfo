# Icod.TermInfo Development Roadmap — Version 0.7.0 Contract

**Project:** `Icod.TermInfo`  
**Package:** `Icod.TermInfo`  
**Target framework:** `net10.0`  
**Language:** C# 13  
**Status:** Implementation complete — T20 release candidate<br>
**Previous contract:** `0.6.0` — complete and frozen  
**Contract target:** `0.7.0`  
**Initial development version:** `0.7.0-alpha.1`

---

# 1. Purpose

Version 0.7.0 expands `Icod.TermInfo` from the deliberately narrow ANSI/VT100 capability set of 0.6.0 into a more complete managed terminal-description library capable of faithfully representing modern xterm-family terminals and modern color models.

The primary goals of 0.7.0 are:

- preserve the successful capability-driven, immutable architecture established by 0.6.0;
- add generic extended/user-defined terminfo capabilities;
- add a substantially fuller standard terminfo capability vocabulary where required by modern terminals and full-screen applications;
- provide a coherent semantic color model covering monochrome, 4-color, 8-color, 16-color, arbitrary indexed palettes, 256-color, and direct/true-color terminals;
- add faithful built-in xterm-family profiles rather than silently mapping xterm identities to ANSI;
- carry modern xterm-related capability metadata such as mouse, focus, bracketed-paste, and extended key information without turning `Icod.TermInfo` into an input-event decoder or live terminal-session manager;
- expose enough low-level screen-control primitives to support applications such as `top`, editors, pagers, progress displays, and future higher-level terminal/curses libraries;
- keep terminal descriptions side-effect-free and independent of a process-global current terminal.

Version 0.7.0 SHALL NOT add arbitrary/system terminal-database loading.

That work, together with explicit Windows Console and Windows Terminal profiles/support, is reserved for version 0.8.0.

---

# 2. Relationship to the 0.6.0 Contract

The 0.6.0 release is a frozen historical contract.

Version 0.7.0 SHALL preserve the following 0.6.0 behaviors unless an explicitly documented 0.7.0 contract change says otherwise:

- `dumb` remains a safe minimal profile;
- `vt100` remains monochrome;
- `vt100-am` remains an accepted alias of `vt100`;
- `ansi` retains its traditional eight-color meaning;
- unknown terminal names do not silently map to ANSI, VT100, xterm, or another built-in profile;
- terminal descriptions remain immutable;
- no process-global `cur_term` equivalent is introduced;
- parameter expansion remains terminal-agnostic;
- padding-aware output remains terminal-agnostic;
- live terminal dimensions remain distinct from profile dimensions;
- Windows VT mode changes remain explicit;
- built-in profile lookup has no terminal-session side effects.

The 0.7.0 implementation may add new public types and capability identifiers. Existing public members should remain source- and binary-compatible wherever practical.

---

# 3. Governing Boundary: What Belongs in Icod.TermInfo?

Version 0.7.0 SHALL use this rule when deciding whether a feature belongs in this package:

> `Icod.TermInfo` may contain immutable terminal-description data and pure transformations required to interpret or expand terminal capabilities. It SHALL NOT own a live terminal session, maintain a virtual screen, or interpret a continuous input stream into application events.

This gives the intended project-family boundary:

```text
Icod.TermInfo
    terminal descriptions
    standard capabilities
    extended capabilities
    parameter expansion
    padding/output transformation
    static/provider-supplied key and protocol strings
    color semantics
    built-in terminal profiles

Icod.Terminal            (future/hypothetical)
    live terminal session
    raw/cooked terminal modes
    input stream decoding
    keyboard events
    mouse events
    bracketed-paste events
    focus events
    terminal probing
    hyperlink/clipboard protocol operations
    full-screen session lifecycle
    cursor-visibility lifecycle
    progress/spinner helpers

Icod.Curses              (future/hypothetical)
    virtual screen
    refresh/diff optimization
    windows
    pads
    panels
    menus
    forms
    widgets
    hit testing
    high-level color/style policy
```

A separate `Icod.Pty` may eventually own pseudo-terminal creation and process plumbing. PTY creation is not part of the 0.7.0 TermInfo contract.

---

# 4. Application Presentation Modes Are Not Terminal Types

Version 0.7.0 SHALL explicitly distinguish terminal capabilities from application presentation behavior.

## 4.1 Streaming applications such as `cat`

A program that writes an effectively unbounded stream of text requires no special terminal mode.

`Icod.TermInfo` SHALL NOT invent a "scrolling mode" capability or API.

The terminal, terminal emulator, or redirected stream determines what happens to output and scrollback.

Existing information such as redirection state, terminal width, carriage return, newline, and ordinary output remains sufficient.

## 4.2 Progress indicators and spinners

A spinner such as:

```text
|
/
-
\
```

is application presentation policy.

The spinner implementation itself SHALL NOT be part of `Icod.TermInfo`.

The low-level capabilities needed by such applications DO belong in TermInfo where standard descriptions provide them, including operations such as:

- carriage return;
- erase line;
- cursor movement;
- save/restore cursor;
- cursor visibility.

A future `Icod.Terminal` MAY provide reusable progress/spinner helpers which consume these capabilities.

## 4.3 Full-screen applications such as `top`

A full-screen application's *capability primitives* belong in `Icod.TermInfo`.

The full-screen session lifecycle does not.

Version 0.7.0 SHALL add or complete standard capabilities needed by cursor-addressed applications, including at minimum:

- `smcup` — enter cursor-addressing mode;
- `rmcup` — leave cursor-addressing mode;
- `civis` — make the cursor invisible;
- `cnorm` — restore the normal cursor;
- `cvvis` — make the cursor very visible, where advertised.

The managed names SHOULD describe the terminfo semantics rather than assume a particular emulator implementation. For example:

```text
EnterCursorAddressingMode
ExitCursorAddressingMode
CursorInvisible
CursorNormal
CursorVeryVisible
```

is preferable to naming `smcup`/`rmcup` unconditionally as "alternate screen", because not every historical terminal implements cursor-addressing mode as a modern alternate screen buffer.

A future `Icod.Terminal` MAY provide a disposable full-screen/session lease which emits those capabilities and restores state.

A future `Icod.Curses` SHALL own virtual-screen state and refresh optimization.

---

# 5. Scope of the 0.7.0 Contract

## 5.1 Included

Version 0.7.0 SHALL include:

- all capabilities and behavior retained from 0.6.0;
- generic extended/user-defined boolean capabilities;
- generic extended/user-defined numeric capabilities;
- generic extended/user-defined string capabilities;
- collision and type rules for standard versus extended capabilities;
- a fuller standard capability catalog sufficient to represent the selected xterm profiles faithfully;
- fuller cursor-addressing/full-screen primitives;
- fuller cursor-visibility primitives;
- fuller navigation and special-key description where required by xterm;
- monochrome color classification;
- 4-color indexed-terminal support;
- 8-color indexed-terminal support;
- 16-color indexed-terminal support;
- arbitrary indexed color counts;
- explicit 88-color support as an interoperability case;
- 256-color indexed-terminal support;
- direct/true-color support;
- direct-color channel-layout interpretation;
- safe indexed-color expansion helpers;
- safe direct-RGB expansion helpers;
- complete preservation of the existing terminfo `colors`, `pairs`, `ncv`, `setaf`, `setab`, and `op` semantics;
- additional standard color capabilities such as `bce`, `ccc`, `hls`, `initc`, and `oc` where applicable;
- a faithful built-in `xterm` profile;
- a faithful built-in monochrome xterm profile if adopted by the final profile matrix;
- built-in xterm indexed-color variants;
- built-in xterm direct-color variants selected by this roadmap;
- xterm mouse capability metadata;
- xterm focus capability metadata;
- xterm bracketed-paste capability metadata;
- xterm extended-key capability carriage;
- provider-defined protocol metadata through the generic extended-capability mechanism;
- documentation of which low-level protocol metadata belongs in TermInfo and which operational behavior belongs in future higher-level libraries;
- updated API baseline, documentation, package validation, samples, and completion gate.

## 5.2 Explicitly excluded from 0.7.0

Version 0.7.0 SHALL NOT implement:

- arbitrary compiled terminfo database loading;
- `/usr/share/terminfo` discovery;
- `TERMINFO`;
- `TERMINFO_DIRS`;
- `~/.terminfo`;
- arbitrary host database precedence;
- termcap database parsing;
- `tic`;
- `infocmp`;
- Windows Console as a terminal profile/family;
- Windows Terminal as a terminal profile/family;
- automatic mapping of Windows consoles to xterm/ANSI profiles;
- curses windows;
- pads;
- panels;
- menus;
- forms;
- virtual-screen refresh optimization;
- terminal emulation;
- pseudo-terminal creation or process management;
- Unix `termios` session management;
- keyboard-event decoding;
- mouse-event decoding;
- bracketed-paste event decoding;
- focus-event decoding;
- active terminal probing;
- device-identification handshakes;
- automatic upgrading of a profile from environment hints such as `COLORTERM`;
- high-level hyperlink emission APIs;
- OSC 52 clipboard operations;
- Sixel image encoding/transmission;
- Kitty graphics transmission;
- Kitty keyboard-protocol negotiation/decoding;
- spinner/progress animation policy;
- automatic full-screen session lifecycle.

Some excluded protocols may have descriptive metadata carried through the generic extended-capability mechanism, but 0.7.0 SHALL NOT turn such metadata into stateful live-terminal operations.

---

# 6. Version 0.8.0 Reservation

Version 0.8.0 is explicitly reserved to add the next major provider/platform layer:

1. Windows Console support;
2. Windows Terminal support;
3. arbitrary/system terminfo database support.

The 0.8.0 database work is expected to include, subject to its own future contract:

- compiled terminfo parsing;
- system database discovery;
- `TERMINFO`;
- `TERMINFO_DIRS`;
- user terminfo directories;
- configurable provider precedence;
- malformed-database handling;
- extended compiled-capability records;
- cache/I/O policy.

The Windows work is expected to distinguish classic Windows Console/conhost behavior from Windows Terminal behavior rather than treating either as generic ANSI or xterm.

Version 0.7.0 SHALL NOT introduce a partial database parser merely to support xterm.

The xterm family SHALL be represented as built-in immutable profiles.

---

# 7. Extended/User-Defined Capabilities

Generic extended capabilities are the foundational architectural change in 0.7.0.

Current ncurses supports user-defined Boolean, numeric, and string capabilities without extending the fixed standard capability tables. This mechanism is used for modern features such as direct color, mouse modes, focus reporting, and extended keys.

The implementation SHALL support all three value kinds.

A possible managed shape is:

```csharp
public enum TermInfoCapabilityValueKind
{
    Boolean,
    Number,
    String,
}

public readonly struct TermInfoCapabilityValue
{
    public TermInfoCapabilityValueKind Kind { get; }

    public bool BooleanValue { get; }
    public int NumberValue { get; }
    public string StringValue { get; }
}
```

The exact final API may differ, but the semantics SHALL include:

- exact case-sensitive extended names;
- immutable storage;
- Boolean, numeric, and string values;
- no hidden type coercion;
- deterministic lookup;
- deterministic handling of an extended name with the wrong requested type;
- builder support;
- provider support;
- enumeration support so higher-level libraries can discover extension names;
- thread-safe concurrent reads.

## 7.1 Standard-name collision rules

A standard capability name SHALL remain authoritative in the standard typed catalog.

An extended capability SHALL NOT silently override a standard capability of the same name.

If the builder is given an extended name which collides with a known standard capability, it SHALL either:

- reject the collision; or
- require an explicit API which makes the override/cancellation semantics unambiguous.

The simpler reject-by-default policy is preferred for 0.7.0 unless a real xterm interoperability requirement demonstrates the need for more.

## 7.2 Extended names are data, not new enum members

Common modern names such as:

```text
RGB
CO
XM
xm
XF
BE
BD
PS
PE
```

SHOULD remain extended capabilities rather than forcing every ecosystem extension into the fixed `BooleanCapability`, `NumericCapability`, or `StringCapability` enums.

Standard terminfo capabilities SHOULD continue to use typed enums.

---

# 8. Standard Capability Vocabulary Expansion

The existing 0.6.0 catalog was intentionally small.

Version 0.7.0 SHALL expand it only where useful for:

- xterm fidelity;
- color semantics;
- cursor-addressed/full-screen applications;
- widely useful key descriptions.

At minimum the audit SHALL cover the following groups.

## 8.1 Cursor visibility and screen mode

Add typed support for:

- `smcup`;
- `rmcup`;
- `civis`;
- `cnorm`;
- `cvvis`.

## 8.2 Scrolling and line movement

Audit and add as required:

- `nel`;
- `indn`;
- `rin`;
- any standard scrolling-region operations required by the selected xterm baseline.

## 8.3 Key/navigation descriptions

The selected xterm baseline requires broader key coverage than 0.6.0.

Audit at minimum:

- End;
- Insert;
- Delete;
- Page Up;
- Page Down;
- Back Tab;
- Begin;
- additional function keys;
- shifted/modified keys where they are standard capabilities.

Extended modified-key names SHALL use the extended-capability store where they are not members of the standard capability repertoire.

## 8.4 Color capabilities

Audit and add at minimum:

- `bce` — background-color erase;
- `ccc` — colors can be changed;
- `hls` — HLS color initialization convention;
- `initc` — initialize/change one color;
- `oc` — restore original colors;
- existing `colors`;
- existing `pairs`;
- existing `ncv`;
- existing `setaf`;
- existing `setab`;
- existing `op`.

Legacy color selectors/pair-oriented mechanisms (`setf`, `setb`, `scp`, `initp`, etc.) SHOULD be reviewed. They may be included where doing so materially improves fidelity without distracting from the xterm-focused 0.7.0 objective.

---

# 9. Color Model

Color support is a central 0.7.0 feature.

## 9.1 Raw terminfo data remains authoritative

The library SHALL NOT replace raw capability data with a simplified color abstraction.

The following remain authoritative terminal-description facts:

- `colors`;
- `pairs`;
- `ncv`;
- `setaf`;
- `setab`;
- `op`;
- `bce`;
- `ccc`;
- `hls`;
- `initc`;
- `oc`;
- relevant extended capabilities such as `RGB` and `CO`.

Semantic color APIs SHALL be derived from these facts.

## 9.2 Color classification

The library SHOULD expose a convenience classification similar to:

```csharp
public enum TerminalColorModel
{
    None,
    Indexed,
    DirectRgb,
}

public enum TerminalColorTier
{
    Monochrome,
    Color4,
    Color8,
    Color16,
    Color256,
    TrueColor,
    OtherIndexed,
    OtherDirectRgb,
}
```

The exact names may change during implementation, but the contract SHALL distinguish:

- no usable color;
- indexed color;
- direct RGB color.

The convenience tier SHALL recognize:

- monochrome;
- 4;
- 8;
- 16;
- 256;
- true color.

It SHALL also support indexed palettes which are not one of those exact sizes.

An 88-color terminal is the principal required interoperability example.

## 9.3 No invented `pairs = colors × colors`

`pairs` SHALL be treated as an independently advertised capability.

The implementation SHALL NOT infer it by squaring `colors`.

This is important for direct-color profiles and unusual historical terminals.

## 9.4 Four-color support

Version 0.7.0 SHALL support terminals advertising four indexed colors.

A fake built-in `$TERM` identity SHALL NOT be added merely to demonstrate 4-color operation.

Tests SHALL create an immutable/provider-supplied four-color description and verify:

- color classification;
- index validation;
- foreground expansion;
- background expansion;
- absent/present behavior;
- pair semantics.

If a real built-in terminal identity with four colors is later selected deliberately, it may be added through the normal profile review process.

## 9.5 Eight-color support

The existing `ansi` profile remains the canonical 0.6.0 eight-color example.

Its meaning SHALL NOT be upgraded to 16/256/direct color.

## 9.6 Sixteen-color support

Version 0.7.0 SHALL support the aixterm-style bright-color convention used by modern xterm 16-color descriptions.

The xterm family SHALL provide at least one faithful 16-color profile.

## 9.7 Arbitrary indexed and 88-color support

The generic color model SHALL permit any positive indexed palette size representable by terminfo.

`xterm-88color` SHOULD be included as a built-in profile because it proves that the implementation is not secretly hard-coded to powers of two or only to 16/256 colors.

## 9.8 256-color support

The selected xterm 256-color profile SHALL faithfully reproduce its conditional `setaf`/`setab` programs.

Color selection SHALL use the shared parameter-expansion engine.

There SHALL be no terminal-name-specific branch in the generic color-expansion code.

## 9.9 Direct/true-color support

Direct color SHALL be based on terminfo capability data, not on a hard-coded assumption that every true-color terminal accepts one particular SGR spelling.

The implementation SHALL support the ncurses `RGB` extension semantics:

- Boolean `RGB`;
- numeric `RGB`;
- string `RGB` channel layouts.

The implementation SHALL be able to represent channel-bit allocation rather than assuming 8/8/8 in every future profile.

For the selected xterm direct profiles, 8-bit red/green/blue channels are expected.

## 9.10 RGB value type

A small immutable managed RGB value SHOULD be added, for example:

```csharp
public readonly record struct TerminalRgbColor(
    byte Red,
    byte Green,
    byte Blue);
```

The exact public type is subject to API review.

## 9.11 Color expansion helpers

A stateless helper layer SHOULD provide safe operations resembling:

```csharp
GetColorSupport(terminal)

ExpandForeground(terminal, colorIndex)
ExpandBackground(terminal, colorIndex)

ExpandForeground(terminal, rgb)
ExpandBackground(terminal, rgb)
```

Requirements:

- validate the terminal argument;
- validate indexed ranges;
- reject indexed requests on a terminal without indexed color selectors;
- reject RGB requests on a terminal without direct-color semantics;
- derive/pack the parameter according to the profile's advertised RGB layout;
- execute the terminal's own `setaf`/`setab` program;
- never hard-code xterm escape strings in the generic helper.

---

# 10. Built-In Profile Matrix

The built-in database SHALL remain explicit and conservative.

## 10.1 Existing profiles

The following retain their established semantics:

| Name | 0.7.0 role |
| --- | --- |
| `dumb` | safe minimal/monochrome fallback |
| `vt100` | DEC VT100, monochrome |
| `vt100-am` | alias of `vt100` |
| `ansi` | traditional eight-color ANSI/pc-term-compatible profile |

T15½ additionally adopts these authoritative DEC foundation identities:

| Name | 0.7.0 role |
| --- | --- |
| `vt102` | DEC VT102, retaining VT100 semantics plus the canonical insert/delete editing delta |
| `vt220` | canonical seven-bit DEC VT220 baseline used to model later DEC/xterm capability fragments |
| `vt200` | authoritative alias of `vt220` |

Wide-mode, 8-bit, `vt220d`, and later DEC terminal identities remain outside this tranche.

## 10.2 xterm family

Version 0.7.0 SHALL add a deliberate xterm family.

The target built-in matrix SHOULD include:

| Name | Purpose |
| --- | --- |
| `xterm` | current modern xterm baseline chosen by this contract |
| `xterm-mono` | monochrome xterm description |
| `xterm-16color` | xterm with 16 indexed colors |
| `xterm-88color` | xterm with 88 indexed colors |
| `xterm-256color` | xterm with 256 indexed colors |
| `xterm-direct` | xterm direct-color description |
| `xterm-direct16` | direct color while retaining 16 indexed colors where applicable |
| `xterm-direct256` | direct color while retaining 256 indexed colors where applicable |

The final profile list may omit a variant if current authoritative xterm/ncurses descriptions demonstrate that the name is obsolete, misleading, or unsuitable as a stable built-in. Any omission SHALL be explicit in the tranche notes rather than accidental.

## 10.3 Fidelity rule

`xterm` SHALL mean the selected authoritative terminfo description named `xterm`.

It SHALL NOT mean:

> "whatever features the newest xterm executable might support."

The built-in shall track a documented source baseline and shall be golden-tested.

## 10.4 Composition

The implementation SHOULD use reusable internal capability fragments rather than duplicate hundreds of capabilities across xterm variants.

A conceptual organization is:

```text
xterm core
    + key fragments
    + screen-mode fragments
    + mouse/focus/paste fragments
    + indexed-color fragments
    + direct-color fragments
```

The composition system may remain internal.

It SHALL NOT introduce terminal-specific branches into `TerminalDescription`, parameter expansion, or output.

---

# 11. Modern xterm Capability Metadata

Version 0.7.0 SHALL carry modern capability metadata where it belongs in a terminal description.

It SHALL NOT decode a live input stream.

## 11.1 Mouse

The contract SHALL support:

- standard `kmous` where applicable;
- extended `XM` enable/disable programs;
- extended `xm` response-format metadata where the selected source profile advertises it.

The built-in modern xterm profile SHOULD prefer the current SGR 1006 mouse description where that matches the authoritative baseline.

`XM` is a parameterized capability and SHALL use the shared parameter-expansion engine.

`xm` is experimental descriptive metadata. `Icod.TermInfo` may store and return it, but SHALL NOT claim to implement a general mouse response interpreter merely because `xm` exists.

Mouse-event decoding belongs in future `Icod.Terminal`.

## 11.2 Focus in/out

The xterm focus-related extended capabilities SHOULD be carried where present in the selected profile baseline.

The library SHALL NOT turn focus reply sequences into managed focus events.

That belongs in future `Icod.Terminal`.

## 11.3 Bracketed paste

The xterm bracketed-paste capability strings SHOULD be carried as extended capabilities where present, including the enable/disable and begin/end markers represented by the authoritative profile.

The library SHALL NOT parse an input stream into paste events.

That belongs in future `Icod.Terminal`.

## 11.4 Extended keys

Provider/profile-supplied extended key strings SHALL be retainable through the generic extended-capability mechanism.

`Icod.TermInfo` SHALL describe the sequences.

A future keyboard decoder SHALL interpret them.

## 11.5 Hyperlinks

Version 0.7.0 SHALL NOT invent a private `OSC8` terminfo capability merely to expose a hyperlink API.

If an authoritative provider supplies a named extended capability related to hyperlinks, the generic extended-capability mechanism SHALL be able to carry it.

Actual OSC 8 URI/text escaping and emission belongs in future `Icod.Terminal`.

## 11.6 Clipboard

Current xterm/ncurses descriptions may carry extended clipboard/selection metadata such as `Ms`.

Such metadata may be retained when it is genuinely part of the selected authoritative xterm profile.

`Icod.TermInfo` SHALL NOT expose a high-level clipboard operation.

OSC 52 operations belong in future `Icod.Terminal`, where security and policy can be handled.

## 11.7 Sixel, Kitty graphics, and other graphics protocols

The generic extended-capability system SHALL be capable of storing provider-defined protocol metadata.

Version 0.7.0 SHALL NOT invent image encoders, transmit images, allocate image IDs, or manage graphics state.

Those operations belong above TermInfo.

## 11.8 Kitty keyboard protocol

Provider-defined support metadata may be carried.

Protocol negotiation and input decoding SHALL NOT be implemented in `Icod.TermInfo`.

---

# 12. Full-Screen Primitive Completeness

The selected xterm baseline SHALL be used to audit the standard string-capability catalog for primitives useful to screen-oriented software.

In addition to the required `smcup`, `rmcup`, and cursor visibility capabilities, the audit SHOULD consider:

- keypad/application mode;
- automatic-margin mode;
- save/restore cursor;
- insert/delete modes where applicable;
- scroll region;
- parameterized index/reverse-index;
- erase operations;
- horizontal/vertical addressing;
- tab control;
- repeat character;
- character-set mode;
- status-line operations only if actually part of the selected xterm profile;
- reset/init strings where appropriate.

The goal is not to implement curses.

The goal is to avoid a situation where `Icod.TermInfo` claims to support `xterm` but cannot describe the primitive screen operations that the canonical xterm terminfo entry advertises.

---

# 13. Terminal Resolution

Built-in lookup remains exact and conservative.

After T15½, the DEC foundation names `vt102`, `vt220`, and `vt200` SHALL resolve exactly. Nearby wide-mode, 8-bit, and later DEC identities SHALL remain unsupported unless deliberately added by a later contract.

After the relevant xterm tranche, the following names SHALL resolve if included in the final built-in matrix:

```text
xterm
xterm-mono
xterm-16color
xterm-88color
xterm-256color
xterm-direct
xterm-direct16
xterm-direct256
```

Only names deliberately implemented SHALL resolve.

Names such as the following SHALL continue to fail unless independently implemented:

```text
screen
screen-256color
tmux
tmux-256color
rxvt
rxvt-unicode
linux
foot
kitty
wezterm
alacritty
konsole
gnome
vte
ms-terminal
```

There SHALL be no fuzzy mapping based on prefixes or color counts.

An application remains free to explicitly choose a fallback.

---

# 14. Environment Hints

Version 0.7.0 MAY expose conservative environment hints such as `COLORTERM` if doing so is useful, but such hints SHALL NOT mutate terminal identity or silently upgrade a resolved profile.

For example:

```text
TERM=xterm-256color
COLORTERM=truecolor
```

SHALL NOT cause `TerminalDatabase` to return an object pretending that the `xterm-256color` profile itself is direct-color.

A future `Icod.Terminal` negotiation/policy layer may combine:

- declared terminfo profile;
- environment hints;
- active probing;
- caller policy.

If 0.7 exposes `COLORTERM`, it SHALL be clearly labeled a hint rather than authoritative capability truth.

It is acceptable to defer `COLORTERM` entirely if no clean API is needed for the 0.7 contract.

---

# 15. Parameter Expansion Requirements

No new color or xterm feature may bypass the shared terminfo parameter engine.

Version 0.7.0 SHALL add golden expansion tests for the complex xterm programs used by:

- 16-color selection;
- 88/256-color selection;
- direct-color selection;
- `XM` mouse-mode enable/disable programs;
- any additional parameterized xterm extended capability adopted into the built-in profile.

If an authoritative xterm program uses a terminfo operator or formatting construct which the 0.6.0 T2 engine does not correctly support, the parameter engine SHALL be fixed generically.

No xterm-specific interpreter branch is permitted.

---

# 16. Output and Padding Requirements

The generic output layer remains unchanged in principle.

All built-in capability strings SHALL continue to flow through:

```text
capability lookup
    ↓
parameter expansion
    ↓
padding parsing
    ↓
output
```

Version 0.7.0 SHALL ensure that new profiles do not introduce literal padding markers into output.

No color, xterm, mouse-mode, or screen-mode helper may bypass the existing padding layer where its source capability can contain padding.

---

# 17. Public API Principles

## 17.1 Preserve existing typed APIs

The 0.6.0 enum-based APIs remain the preferred way to access standard capabilities.

## 17.2 Add generic APIs only for extended capabilities

Extended names require string-based APIs by definition.

The extended API SHOULD make type identity explicit and should avoid a proliferation of ambiguous `object` values.

## 17.3 Immutability

`TerminalDescription` remains immutable.

Color-support objects, RGB values, and extended capability values SHALL be immutable value types or immutable reference types.

## 17.4 Thread safety

Concurrent read access to:

- built-in profiles;
- extended capabilities;
- color support;
- parsed parameter programs;

SHALL be safe.

## 17.5 No live terminal object

0.7.0 SHALL NOT introduce a `Terminal` object that owns streams, modes, or event loops.

That belongs in a separate future library.

---

# 18. Testing Strategy

Version 0.7.0 SHALL retain the three-OS test matrix and expand testing substantially.

## 18.1 Extended capability tests

Cover:

- boolean extended values;
- numeric extended values;
- string extended values;
- missing names;
- wrong-type lookup;
- standard-name collision behavior;
- immutability;
- enumeration;
- provider composition;
- concurrency.

## 18.2 Color classification matrix

At minimum:

| Advertised model | Expected classification |
| --- | --- |
| no colors/selectors | monochrome |
| 4 colors | 4-color |
| 8 colors | 8-color |
| 16 colors | 16-color |
| 88 colors | other indexed / 88 indexed |
| 256 colors | 256-color |
| 16,777,216 direct | true/direct color |

Also test irregular indexed counts so the implementation cannot pass merely by recognizing a fixed switch statement.

## 18.3 Indexed expansion tests

Test boundaries:

- negative index rejected;
- zero;
- last valid color;
- first invalid color;
- 4-color;
- 8-color;
- 16-color;
- 88-color;
- 256-color.

## 18.4 Direct color tests

Cover:

- black;
- white;
- pure red;
- pure green;
- pure blue;
- arbitrary RGB;
- channel packing;
- Boolean `RGB`;
- numeric `RGB`;
- string-layout `RGB`;
- malformed RGB layout;
- incompatible requested model.

## 18.5 Golden profile tests

Every advertised capability in each built-in xterm profile SHALL have golden coverage or shall be covered by a reusable golden fragment test.

Profile inheritance/composition MUST NOT make capabilities untested merely because they come from an internal fragment.

## 18.6 Identity tests

Verify:

- every supported xterm name resolves exactly;
- every unsupported nearby name fails;
- no fallback occurs without caller request.

## 18.7 Full-screen primitive tests

Verify exact `smcup`, `rmcup`, cursor visibility, cursor addressing, and selected screen-control strings for xterm.

## 18.8 Modern metadata tests

Verify exact values and parameter expansion for:

- `kmous`;
- `XM`;
- `xm` where adopted;
- focus metadata;
- bracketed paste metadata;
- selected extended keys.

Tests SHALL NOT need an interactive terminal.

## 18.9 Architecture guard tests

Maintain or extend source/API guards proving:

- no xterm-specific branches in the parameter engine;
- no xterm-specific branches in padding/output;
- no process-global mutable terminal;
- no event decoder appears in TermInfo;
- no arbitrary database loader appears in 0.7;
- no Windows Console/Windows Terminal profile appears before 0.8.

---

# 19. Documentation Requirements

The README and 0.7 documentation SHALL explain:

- the difference between terminal identity and color tier;
- why `ansi` remains 8-color;
- why 4-color support does not imply a fake built-in terminal;
- why 88-color is supported;
- indexed versus direct color;
- how `RGB` extended capability semantics work;
- how to query generic extended capabilities;
- xterm profile selection;
- exact versus fallback resolution;
- `smcup`/`rmcup` as cursor-addressing mode rather than an unconditional synonym for alternate screen;
- how a full-screen program should consume TermInfo primitives without expecting TermInfo to manage the session;
- why `cat` needs no special mode;
- why spinners are higher-level behavior;
- mouse/focus/paste metadata versus event decoding;
- the boundary with future `Icod.Terminal`;
- the boundary with future `Icod.Curses`;
- the explicit 0.8 reservation.

Examples SHOULD include:

- 4-color custom description;
- ANSI 8-color;
- xterm 16-color;
- xterm 256-color;
- xterm direct RGB;
- querying color support;
- entering/exiting cursor-addressing mode manually;
- querying xterm mouse/paste metadata without decoding events.

---

# 20. Packaging and Versioning

The package remains:

```text
Icod.TermInfo
```

Target framework remains:

```text
net10.0
```

Language remains:

```text
C# 13
```

The first 0.7 development tranche SHALL advance both:

```xml
<Version>0.7.0-alpha.1</Version>
<PackageVersion>0.7.0-alpha.1</PackageVersion>
```

Every subsequent tranche SHALL advance both together.

Expected progression may use:

```text
0.7.0-alpha.N
0.7.0-beta.N
0.7.0-rc.N
0.7.0
```

Package validation, Source Link, symbol packages, deterministic builds, and the release-package consumer smoke test established in 0.6.0 SHALL remain required.

---

# 21. Implementation Roadmap

The implementation SHALL proceed in dependency order.

---

## T11 — 0.7 Foundation and Extended Capabilities

### Work

- add this 0.7.0 roadmap to the repository;
- leave the 0.6.0 contract documents unchanged as historical records;
- advance both version fields to `0.7.0-alpha.1`;
- add immutable extended capability value representation;
- add Boolean/numeric/string extended capability storage;
- add builder APIs;
- add lookup APIs;
- add enumeration APIs;
- define standard/extended collision policy;
- add tests for type safety, immutability, concurrency, and provider composition;
- update the public API baseline.

### Acceptance gate

T11 is complete when:

- a provider can construct a terminal with arbitrary named extended capabilities;
- all three value kinds round-trip exactly;
- a standard capability cannot be accidentally shadowed;
- descriptions remain immutable;
- multiple concurrent readers are safe;
- existing 0.6 profiles are behaviorally unchanged.

---

## T12 — Standard Capability Vocabulary Expansion

### Work

Expand the typed standard capability catalog for the primitives needed by 0.7.

At minimum:

- `smcup`;
- `rmcup`;
- `civis`;
- `cnorm`;
- `cvvis`;
- `bce`;
- `ccc`;
- `hls`;
- `initc`;
- `oc`;
- selected scrolling primitives;
- selected navigation keys;
- selected additional function keys;
- other standard capabilities required by the selected xterm baseline.

Audit full-screen, scrolling, erase, addressing, tab, keypad, reset/init, and attribute primitives.

### Acceptance gate

T12 is complete when:

- every added standard capability maps correctly between typed and short-name lookup;
- full-screen/cursor-visibility primitives can be represented without extended-name hacks;
- no application lifecycle behavior has been added;
- no xterm profile is yet required to make the capability core work.

---

## T13 — Semantic Color Core

### Work

- add color model/tier representation;
- add semantic color-support inspection;
- add RGB layout representation;
- interpret `colors`, `pairs`, `ncv`, selectors, `bce`, `ccc`, `hls`, `initc`, and `oc`;
- interpret extended `RGB`;
- interpret relevant direct-color extension metadata such as `CO`;
- add indexed foreground/background expansion helpers;
- add RGB foreground/background expansion helpers;
- add strict validation;
- add monochrome/4/8/16/88/256/direct classification tests.

### Acceptance gate

T13 is complete when:

- a synthetic/provider-created 4-color description works correctly;
- `ansi` still reports exactly eight colors;
- `vt100` remains monochrome;
- arbitrary indexed counts are supported;
- direct-color layouts are not assumed blindly;
- generic helpers contain no terminal-name branches.

---

## T14 — Indexed Color Completeness

### Work

- validate aixterm-style 16-color parameter behavior;
- validate generic indexed ranges;
- add reusable indexed-color capability fragments;
- golden-test 4, 8, 16, 88, and 256 indexed behavior;
- verify palette alteration/reset capability handling.

### Acceptance gate

T14 is complete when:

- all requested indexed color depths are representable;
- 88 colors proves arbitrary indexed support;
- `pairs` remains independent of `colors`;
- color selection is driven by capability programs and the shared parameter engine.

---

## T15 — xterm Core Profile

### Work

Implement the authoritative built-in `xterm` baseline selected for 0.7.

Include:

- identity;
- aliases only if authoritative and intentional;
- cursor addressing;
- erase/edit operations;
- scrolling;
- full-screen/cursor-addressing mode;
- cursor visibility;
- keypad/application mode;
- attributes;
- character sets;
- standard keys;
- extended keys needed by the baseline;
- current baseline mouse/focus/paste fragments where composition requires them;
- ordinary color behavior for the core profile.

Use internal composition fragments rather than copy-and-paste where practical.

### Acceptance gate

T15 is complete when:

- `TERM=xterm` resolves exactly;
- the profile is golden-tested;
- it is materially faithful to the selected current xterm/ncurses description;
- no unsupported xterm-family names resolve accidentally;
- parameter/output engines remain terminal-agnostic.

---

## T15½ — DEC VT102/VT220 Foundations and xterm Composition Audit

### Work

- add first-class built-in `vt102`;
- add first-class built-in `vt220` with authoritative `vt200` alias;
- preserve `vt100` behavior exactly while factoring its data into reusable internal DEC capability fragments;
- model the canonical VT102 insert/delete editing delta without broadening `vt100`;
- model the canonical seven-bit VT220 base, DEC editing keypad, unshifted function-key layout, and DECTCEM cursor visibility;
- add typed standard capability identifiers for `kfnd`, `khlp`, `krdo`, and `kslt`;
- factor the authoritative `vt220+pcedit` mapping for reuse by xterm's PC editing-key composition;
- regression-test the completed T15 `xterm` profile so the refactor changes composition but not advertised behavior;
- keep wide-mode, 8-bit, `vt220d`, VT320+, and unrelated historical DEC variants out of this tranche.

### Acceptance gate

T15½ is complete when:

- `TERM=vt102` resolves exactly and remains monochrome;
- `TERM=vt220` and its `vt200` alias resolve to the same immutable profile;
- `vt100` remains behaviorally identical to its pre-T15½ contract;
- VT102 exposes exactly the canonical VT100-plus-editing delta represented by the library;
- VT220 carries the selected canonical seven-bit DEC editing/function-key and cursor-visibility behavior;
- xterm reuses the VT220 PC editing-key fragment without changing its T15 golden behavior;
- nearby unimplemented DEC identities continue to fail conservatively;
- no DEC- or xterm-specific branch is added to generic parameter expansion or output.

---

## T15¾ — xterm Composition Refactor Before Indexed Family

### Work

- separate the modern xterm common control/attribute/screen capability layer from the ordinary eight-color palette and selector layer;
- move `colors#8`, `pairs#64`, `setaf`, `setab`, and the legacy `setf`/`setb` mappings out of the common xterm fragment;
- keep common capabilities such as `bce` and `op` where the authoritative indexed xterm variants inherit them unchanged;
- compose the built-in `xterm` profile from the common layer plus an explicit ordinary eight-color fragment;
- preserve every T15/T15½ advertised `xterm` capability and golden value exactly;
- add internal composition tests proving the common layer does not itself choose an indexed palette or selectors;
- do not add any new `$TERM` identity in this tranche;
- defer the final `xterm-mono` decision to T16 because the authoritative ncurses `xterm-mono` entry derives from the historical `xterm-r6` family rather than from modern `xterm-new` with color merely removed.

### Acceptance gate

T15¾ is complete when:

- `TERM=xterm` remains behaviorally identical to T15½ and all existing xterm golden tests still pass;
- the reusable xterm common fragment does not set `colors`, `pairs`, `setaf`, `setab`, `setf`, or `setb`;
- the explicit xterm ordinary-eight-color fragment reconstructs the current `xterm` color behavior exactly;
- the refactor changes no public API and introduces no new resolvable terminal name;
- T16 can select 16/88/256-color data without first installing and then overriding the eight-color selector layer.

---

## T16 — xterm Indexed-Color Family

### Work

Build on the T15¾ common/color split and implement and golden-test:

- `xterm-mono` if retained by final review;
- `xterm-16color`;
- `xterm-88color`;
- `xterm-256color`.

Final T16 review does **not** retain `xterm-mono`. The authoritative ncurses
entry intentionally derives from the historical `xterm-r6` family, whose
function-key, mouse, and control behavior differs from the modern `xterm-new`
family used by T15/T15¾. Modeling it as merely "modern xterm without color"
would therefore be inaccurate. It remains unsupported unless a later contract
deliberately adds that historical family.

Reuse the T15¾ xterm common fragment and compose each selected indexed-color layer explicitly.

### Acceptance gate

T16 is complete when:

- each supported name resolves exactly;
- advertised `colors`, `pairs`, selectors, reset, palette, and `bce` behavior match the selected authoritative source;
- 16/88/256 selectors expand correctly through T2;
- `xterm-256color` is no longer rejected as unsupported;
- unrelated terminal names remain unsupported.

---

## T17 — xterm Direct/True-Color Family

### Work

Implement direct-color xterm fragments and profiles selected for the contract, expected to include:

- `xterm-direct`;
- `xterm-direct16`;
- `xterm-direct256`.

Implement current `RGB`/`CO` semantics required by those profiles.

Final T17 review retains all three direct-color identities. Against ncurses
`terminfo.src` revision 1.1267 (2026-08-14), each advertises
`colors#0x1000000`, `pairs#0x10000`, and Boolean `RGB`; their numeric `CO`
values are respectively 8, 16, and 256. `CO` is the retained indexed-color
prefix before the packed 8/8/8 RGB parameter space. The selected direct
selectors use the current colon-separated `38:2::R:G:B` / `48:2::R:G:B`
form. These profiles do not synthesize palette-changing `ccc`/`initc`/`oc`
capabilities which the authoritative direct-color entries do not advertise.

Golden-test direct-color selector programs.

### Acceptance gate

T17 is complete when:

- true RGB foreground/background selection works through profile data;
- direct color is not implemented by hard-coded ANSI escape strings in generic code;
- direct profiles report correct raw `colors`/`pairs`;
- ANSI indices retained by direct profiles behave correctly;
- RGB channel interpretation is documented and tested.

---

## T18 — Modern xterm Metadata

### Work

Complete the descriptive metadata required by the selected modern xterm baseline:

- standard `kmous`;
- extended `XM`;
- extended `xm` where adopted;
- focus enable/disable and focus-key metadata;
- bracketed-paste enable/disable and begin/end metadata;
- extended modified key strings;
- authoritative clipboard/selection extension metadata already present in the selected profile, where appropriate.

Final T18 review confirms that T15 seeded the selected metadata early so later
xterm profiles could compose correctly, and T18 now freezes that data against
ncurses `terminfo.src` revision 1.1267 (2026-08-14). The contract retains
standard `kmous`; the SGR-1006 `XM`/`xm` programs; `XF`, `fe`, `fd`, `kxIN`,
and `kxOUT` focus metadata; `BE`, `BD`, `PS`, and `PE` bracketed-paste
metadata; the selected PC-style modified-key and X11 three-key extensions; and
the `xterm+tmux2` cursor/clipboard strings `Cr`, `Cs`, `Ms`, `Se`, and `Ss`,
together with `RV`/`rv` and `XR`/`xr` reporting metadata. Every built-in xterm
color variant inherits the same descriptive metadata, and parameterized values
continue to expand through the shared T2 engine.

These capabilities remain descriptive data only. T18 introduces no mouse,
focus, or paste event decoder; no clipboard execution; no probing; no event
loop; and no live terminal-session state.

Document but DO NOT implement:

- mouse event decoding;
- focus event decoding;
- paste event decoding;
- clipboard operations;
- hyperlinks;
- graphics transmission;
- live probing.

### Acceptance gate

T18 is complete when:

- the xterm descriptions can carry the authoritative extension data without new enum members;
- parameterized extensions expand through the shared T2 engine;
- no continuous-input parser exists in `Icod.TermInfo`;
- no live session state is introduced.

---

## T19 — API Hardening, Documentation, and Samples

### Work

- review all new 0.7 public APIs;
- update exported public API baseline;
- review nullability and exception contracts;
- review allocation-sensitive paths;
- review thread safety;
- update README;
- add color examples;
- add xterm examples;
- add extended-capability examples;
- add full-screen primitive example;
- document project-family boundaries;
- document 0.8 reservation;
- update package metadata and release notes;
- ensure sample project demonstrates new features without requiring an interactive CI terminal.

Final T19 review freezes the 0.7 public surface represented by
`PublicApiSurfaceTests`; no additional public type is required for this tranche.
The README and sample are rewritten around the completed 0.7 capability/color
model, the sample gains a `--describe-only` path for non-interactive validation,
and `docs/0.7.0-CONTRACT-AUDIT.md` records the pre-T20 API/scope evidence.

T19 also hardens repository validation: pull requests and `main` validate Debug
and Release on Windows/Linux/macOS, while `main` additionally packs the Release
artifacts, runs the fresh-package verifier, exercises the non-interactive sample,
and uploads the resulting package artifacts without publishing them. Final
versioning, tagging, and publication remain the responsibility of T20.

### Acceptance gate

T19 is complete when:

- all public 0.7 members are intentional;
- documentation distinguishes capability data from live terminal behavior;
- a new consumer can select xterm and use indexed/direct color;
- examples do not imply that TermInfo owns full-screen lifecycle or input decoding;
- package/API validation is clean.

---

## T20 — 0.7.0 Completion Gate

Before tagging `0.7.0`, perform a final audit.

Final T20 implementation review closes the repository-side 0.7.0 contract. The
release candidate carries the final `0.7.0` package/assembly version, adds
explicit final-version and reserved-0.8 profile gates, and verifies that all
selected xterm variants retain their cursor-addressing and cursor-visibility
primitives. The existing validation workflow now also runs on pushes to the
`0.7.0` release branch and through `workflow_dispatch`, so the exact candidate
can produce the six cross-platform Debug/Release results and validated package
artifacts required before tagging.

Tagging and registry publication remain deliberately separate release actions.
They must use the exact commit/artifacts which pass the T20 workflow; no source
change may occur between successful final validation and the `v0.7.0` tag.

### Required checks

- all Windows/Linux/macOS CI jobs pass;
- Debug and Release tests pass;
- package validation passes;
- public API baseline is clean;
- fresh-package consumer smoke test passes;
- 0.6.0 profiles remain compatible;
- `dumb` remains safe/minimal;
- `vt100` remains monochrome;
- `vt102` remains monochrome and resolves exactly;
- `vt220`/`vt200` remain monochrome and resolve exactly;
- `ansi` remains traditional eight-color;
- synthetic/provider 4-color support passes;
- 16-color support passes;
- 88-color support passes;
- 256-color support passes;
- direct-color support passes;
- arbitrary indexed-color classification passes;
- selected xterm built-ins exactly resolve;
- unsupported terminal names still fail conservatively;
- no arbitrary/system terminfo database loader exists;
- no Windows Console/Windows Terminal profile has slipped into 0.7;
- extended capabilities are immutable and thread-safe;
- standard names cannot be accidentally shadowed by extensions;
- xterm color programs use the generic T2 engine;
- xterm padding/output uses the generic output layer;
- `smcup`/`rmcup` and cursor visibility primitives are available where profiles advertise them;
- no full-screen session manager exists in TermInfo;
- no keyboard-event decoder exists;
- no mouse-event decoder exists;
- no bracketed-paste decoder exists;
- no active terminal probing exists;
- package symbols and Source Link function correctly;
- README scope matches this contract;
- 0.8 reservation is documented.

### Completion

The repository-side 0.7.0 implementation contract is complete in this T20
release candidate. Final release sign-off is intentionally evidence-driven:

- confirm both version elements are exactly `0.7.0`;
- require all six Windows/Linux/macOS Debug/Release jobs and package validation
  to pass for the exact release commit;
- tag that exact commit `v0.7.0`;
- publish the same validated package to NuGet and GitHub Packages, together
  with its matching symbol package;
- confirm the published package restores in a fresh consumer.

Any source/package change after validation reopens the gate and requires a new
T20 validation run before tagging.

---

# 22. Deferred to 0.8.0

The following are explicitly outside 0.7 and form the starting inventory for 0.8:

## 22.1 Windows Console

Model classic Windows console behavior honestly rather than aliasing it to ANSI/xterm.

The 0.8 design SHALL distinguish native console capabilities from virtual-terminal mode where appropriate.

## 22.2 Windows Terminal

Add explicit Windows Terminal profiles, expected to be informed by current authoritative `ms-terminal` / `ms-terminal-direct` descriptions and Microsoft's documented behavior.

Windows Terminal SHALL NOT simply alias `xterm-256color`.

## 22.3 Arbitrary/system terminfo database support

Add a provider capable of loading host terminal descriptions.

The future design SHALL address:

- compiled database format;
- standard and extended capabilities;
- cancellation semantics;
- path discovery;
- environment variables;
- user directories;
- provider precedence;
- malformed files;
- caching;
- reproducibility versus host-specific definitions.

Built-in profiles SHALL remain available even after system-database loading exists.

---

# 23. Still Outside 0.8 Unless Separately Contracted

Even after database loading is added, these remain separate concerns unless a future roadmap explicitly adopts them:

- curses windows;
- pads;
- panels;
- menus;
- forms;
- refresh optimization;
- terminal emulation;
- PTY creation/process management;
- `termios` live-session management;
- keyboard event decoding;
- mouse event decoding;
- paste event decoding;
- active terminal probing;
- high-level hyperlink operations;
- high-level clipboard operations;
- Sixel/Kitty image encoders/transmitters;
- termcap source/database parsing;
- `tic` executable;
- `infocmp` executable.

The fact that `Icod.TermInfo` may carry descriptive capability metadata for some of these protocols does not move their operational implementation into this package.

---

# 24. Interoperability Baseline and References

0.7.0 profile data SHOULD be checked against current authoritative documentation at implementation time rather than copied from stale historical assumptions.

Primary references used to define this roadmap include:

- ncurses `terminfo(5)`  
  https://invisible-island.net/ncurses/man/terminfo.5.html

- ncurses user-defined capabilities (`user_caps(5)`)  
  https://invisible-island.net/ncurses/man/user_caps.5.html

- current ncurses `terminfo.src`  
  https://invisible-island.net/ncurses/terminfo.src.html

- current xterm terminfo source/contents  
  https://invisible-island.net/xterm/terminfo-contents.html

- xterm control-sequence documentation  
  https://invisible-island.net/xterm/ctlseqs/ctlseqs.html

At the time this roadmap was prepared in August 2026, the current ncurses source contains explicit xterm definitions for 16-color, 88-color, 256-color, and direct-color variants, uses extended `RGB` data for direct color, and contains modern xterm building blocks for SGR 1006 mouse, focus reporting, and bracketed paste.

The implementation SHALL record the exact upstream revision/date used for each built-in profile's golden baseline so later upstream changes can be reviewed deliberately rather than silently changing package behavior.

---

# 25. Summary of the 0.7.0 Boundary

The release can be summarized as:

> **0.7.0 teaches `Icod.TermInfo` to describe modern xterm-class terminals faithfully. It expands the capability vocabulary, adds generic extensions, models the full color spectrum from monochrome through direct RGB, and carries modern xterm protocol metadata. It does not become a terminal session manager, curses library, event decoder, or arbitrary terminal-database loader.**

And the next release is intentionally:

> **0.8.0 adds Windows Console, Windows Terminal, and arbitrary/system terminal database support.**
