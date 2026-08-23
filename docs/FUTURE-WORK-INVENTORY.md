# Icod.TermInfo and Terminal-System Future Work Inventory

This document records the broader terminal-related work that remains after the
0.6.0, 0.7.0, and 0.8.0 `Icod.TermInfo` contracts, and identifies which work
belongs in 0.9 versus sibling or later layers.

Its purpose is to prevent the existence of a missing terminal feature from
being mistaken for evidence that the feature belongs in the `Icod.TermInfo`
runtime package.

The governing distinction is:

> **`Icod.TermInfo` owns immutable terminal-description data, acquisition of
> that data, and pure transformations required to interpret/expand/output it.
> Live terminal conversations, process plumbing, and virtual-screen/UI policy
> belong elsewhere.**

---

## 1. Current foundation

By the end of 0.8.0, `Icod.TermInfo` already provides:

- immutable terminal descriptions;
- complete standard capability metadata;
- standard and extended Boolean/numeric/string capabilities;
- signed 32-bit numeric semantics;
- exact reversible 8-bit capability strings;
- robust terminfo parameter parsing/evaluation;
- bounded per-description parsed-program reuse;
- terminal-aware padding/output semantics;
- ANSI, DEC, xterm, Windows Console, and Windows Terminal built-ins;
- semantic indexed/direct color support;
- descriptive modern protocol/key metadata;
- deterministic provider composition;
- no process-global current terminal;
- a frozen compiled-format target and parser fixture corpus.

0.9 is therefore an acquisition release rather than a semantic rewrite.

---

## 2. Inventory by architectural family

| Family | Missing work | Natural home | Common dependency |
| --- | --- | --- | --- |
| Compiled acquisition | `0432`, extended sections, `01036`, diagnostics | `Icod.TermInfo` 0.9 | 0.8 semantic model |
| Filesystem/system discovery | directory provider, `TERMINFO`, `TERMINFO_DIRS`, user/default roots | `Icod.TermInfo` 0.9 | compiled parser |
| External-data lifecycle | cache, refresh, concurrency, hostile-file bounds | `Icod.TermInfo` 0.9 | parser + providers |
| Hashed databases | Berkeley DB/ncurses hashed stores | optional later provider/package | compiled parser |
| Historical vendor formats | HP-UX/AIX/OSF/1 divergent binary layouts | optional later | parser abstraction + fixtures |
| Terminfo source language | `.ti`, escapes, cancellation, `use=` inheritance | later core/source package | 0.8 semantic model + source AST |
| Terminfo tooling | `tic`, `infocmp`, `toe`, conversion tooling | likely `Icod.TermInfo.Tools` | source parser + binary reader/writer |
| Termcap interoperability | termcap syntax, `TERMCAP`, `TERMPATH`, conversion | optional compatibility/tooling | source/conversion model |
| Live session | raw/cooked/cbreak, restore, tty ownership, full-screen/cursor lifecycle | `Icod.Terminal` | `Icod.TermInfo` + OS interop |
| Input events | keyboard, modifiers, mouse, focus, paste, resize | `Icod.Terminal` | raw session + incremental decoder |
| Probing/negotiation | DA/DSR/DECRQSS/XTGETTCAP and response routing | `Icod.Terminal` | input framing + request router |
| Operational protocols | OSC 8/52, synchronized output, title/notifications | `Icod.Terminal` | live session + response policy |
| Modern keyboard | CSI-u/Kitty keyboard negotiation and events | `Icod.Terminal` | input decoder + negotiation |
| Graphics | Sixel, Kitty graphics, iTerm-style images | terminal protocol extensions | output + query/response routing |
| PTY/ConPTY | pseudo-terminal creation, resize, child plumbing | `Icod.Pty` | OS/process layer |
| Virtual screen/curses | cell grid, diff/refresh, windows, pads, panels, widgets | `Icod.Curses` | `Icod.Terminal` + `Icod.TermInfo` |
| Unicode display model | graphemes, width, combining marks, emoji, continuation cells | `Icod.Curses`/presentation layer | virtual cell model |
| Terminal emulation | consume control stream and emulate terminal state | separate emulator library | escape parser + Unicode cell model |
| Cross-cutting quality | fuzzing, differential tests, corpus growth, resource/security audits | all relevant projects | per-layer inputs |

---

## 3. Core `Icod.TermInfo` work after 0.9

### 3.1 Terminfo source syntax

The largest coherent terminfo feature not included in 0.9 is source-language
support.

A future design would need:

```text
.ti text
   |
   v
lexer/parser
   |
   v
source entry / AST
   |
   +----------------+
   |                |
   v                v
validation       use= inheritance
                    |
                    v
             resolved source entry
                    |
                    v
           TerminalDescription
```

This includes source escapes, Boolean/numeric/string fields, cancellation,
extended capabilities, aliases/descriptions, and `use=` inheritance.

It is intentionally separate from 0.9 because compiled entries already contain
the resolved semantic result; a source parser introduces a second language and
an inheritance/resolution model.

### 3.2 `tic`-class binary writing

Once a source model exists, binary emission can compile resolved descriptions
into the supported compiled formats.

A writer should reuse the same canonical metadata and byte semantics as the
0.9 reader rather than maintaining a separate capability table.

### 3.3 `infocmp`-class inspection

A tooling layer could provide:

- compiled-entry decompilation to source;
- terminal-description comparison;
- canonical/source ordering;
- optional resolved/unresolved inheritance views;
- extended-capability display;
- machine-readable output useful for tests and diagnostics.

This is operational tooling, not required for runtime terminal capability use.

### 3.4 Termcap conversion

Termcap is less expressive and introduces its own source/search semantics.
Support should be treated as an interoperability/tooling feature rather than as
part of the 0.9 compiled-term acquisition contract.

### 3.5 Hashed database provider

Contemporary ncurses may store compiled entries in a Berkeley DB-backed hashed
database. 0.9 deliberately avoids that runtime dependency.

If real user demand appears, the preferred architecture is an optional provider
which extracts the same compiled entry bytes and passes them to the existing
0.9 parser.

### 3.6 Divergent historical binary dialects

Commercial Unix formats that diverged from the selected System V/ncurses
baseline should be added only from authoritative documentation/fixtures.

They must not be recognized by guesswork because capability-table collisions
can produce semantically plausible but wrong descriptions.

---

## 4. `Icod.Terminal` family

The most important post-0.9 terminal work for interactive applications is
probably **not more TermInfo**. It is a live terminal-session layer.

A likely dependency structure is:

```text
Icod.TermInfo
     |
     v
Icod.Terminal session
     |
     +-----------------------+
     |                       |
     v                       v
output/session lifecycle   raw input
     |                       |
     |                       v
     |                incremental framing
     |                       |
 +---+----+             +----+----------+
 |        |             |               |
 v        v             v               v
screen   protocol     application     probe/query
lease    output       events           responses
```

### 4.1 Live session ownership

A terminal-session layer should own, where applicable:

- terminal/console handles or streams;
- redirection facts;
- raw/cooked/cbreak mode transitions;
- exact restoration of prior modes;
- live size and resize notifications;
- selected `TerminalDescription`;
- output encoding policy;
- full-screen/cursor-visibility leases;
- deterministic exception/disposal recovery.

This stateful lifecycle does not belong in immutable `TerminalDescription`.

### 4.2 Input decoding

A robust decoder must distinguish incomplete and overlapping sequences across:

- ordinary text/UTF-8;
- traditional terminfo key strings;
- CSI/SS3 keys;
- modifiers;
- mouse protocols;
- focus events;
- bracketed paste;
- resize/event sources;
- modern CSI-u/Kitty keyboard sequences;
- responses generated by active terminal queries.

The decoder must be incremental; a one-shot dictionary lookup is insufficient
for ESC ambiguity and prefix sharing.

### 4.3 Probe/request-response router

Active capabilities such as device attributes, DSR, DECRQSS, XTGETTCAP, and
modern protocol negotiation all need a common response-routing mechanism.

That router should prevent every future protocol feature from inventing its own
read loop, timeout, and response matcher.

### 4.4 Operational protocols

Once a live session and response router exist, higher-level terminal operations
can be layered cleanly:

- OSC 8 hyperlinks;
- OSC 52 clipboard/selection;
- title and notification controls;
- synchronized output;
- cursor style/color operations;
- modern underline/style extensions;
- terminal-specific negotiated features.

Terminfo may carry descriptive strings for some of these, but the conversation
and state belong to the live terminal layer.

### 4.5 Graphics and advanced keyboard protocols

Kitty graphics, Sixel, iTerm-style images, and advanced keyboard protocols are
large enough to deserve protocol-focused components built on the common live
session/framing/router foundation.

---

## 5. `Icod.Pty`

Pseudo-terminal support is an orthogonal branch:

- Unix PTY allocation;
- Windows ConPTY;
- child process creation/plumbing;
- terminal-size propagation;
- stream lifetime;
- process exit/cancellation;
- signal/control-event interaction where applicable.

PTY support can use `Icod.TermInfo` and `Icod.Terminal`, but neither should
require PTY support to function.

A PTY layer also creates valuable integration-test infrastructure for future
interactive terminal components.

---

## 6. `Icod.Curses` / virtual-screen family

A curses-style library should begin with a Unicode-aware cell model rather than
with widgets.

The likely dependency order is:

```text
Icod.TermInfo
      |
Icod.Terminal
      |
      v
Unicode cell/grid model
      |
      v
damage tracking + refresh/diff engine
      |
      v
windows / pads / panels
      |
      v
menus / forms / widgets
```

### 6.1 Unicode cell semantics

The grid must eventually define behavior for:

- grapheme clusters;
- combining marks;
- zero/one/two-cell display width;
- emoji presentation/width;
- wide-character continuation cells;
- clipping and overwriting wide cells;
- style/color attachment;
- cursor positioning relative to logical/display cells.

### 6.2 Refresh engine

Only after the cell model is stable should the library implement:

- dirty regions;
- old/new screen comparison;
- cost-aware cursor movement;
- attribute/color transition minimization;
- insert/delete line/character optimization;
- resize reconciliation.

### 6.3 Higher-level UI

Windows, pads, panels, menus, forms, and widgets build on the grid/refresh
engine. They are not `Icod.TermInfo` responsibilities.

---

## 7. Terminal emulator family

A terminal emulator solves the inverse problem from `Icod.TermInfo`:

```text
application output bytes
        |
        v
escape/control parser
        |
        v
terminal state machine
        |
        v
screen/cursor/modes
```

It may share protocol and Unicode primitives with terminal-session/curses work,
but should remain a separate package because it consumes terminal output rather
than describing how an application should produce it.

---

## 8. Cross-cutting robustness

### 8.1 Fuzzing

Highest-value fuzz targets include:

- 0.9 compiled parser;
- future terminfo source parser;
- live input escape decoder;
- active-query response parser;
- graphics protocol decoders;
- terminal-emulator control parser.

### 8.2 Differential testing

Where authoritative implementations exist, optional differential tests can
compare:

- compiled parsing with ncurses `infocmp`;
- future source compilation with `tic`;
- parameter expansion with ncurses behavior for adopted syntax;
- terminal-emulator sequences with protocol conformance fixtures.

Runtime/normal CI must remain deterministic and not depend on host databases.

### 8.3 Security/resource limits

Every layer accepting external or terminal-supplied bytes needs explicit:

- input-size limits;
- checked arithmetic;
- bounded buffering;
- timeout/cancellation policy where I/O exists;
- path/input validation;
- failure atomicity.

---

## 9. Dependency graph and recommended sequence

The recommended near-term sequence is:

```text
0.8 semantic completion
        |
        v
0.9 compiled acquisition
        |
        +----------------------------+
        |                            |
        v                            v
Icod.TermInfo 1.0 review       source/tooling family
                                     (optional/later)
        |
        v
Icod.Terminal
        |
   +----+----------------+
   |                     |
   v                     v
Icod.Pty              input/probing/protocols
   |                     |
   +----------+----------+
              |
              v
          Icod.Curses
```

`Icod.Pty` may also proceed independently/parallel because its core OS/process
plumbing is largely orthogonal to terminfo database acquisition.

---

## 10. 1.0 interpretation

After 0.9, `Icod.TermInfo` should be evaluated for 1.0 based on whether its core
job is complete and stable, not on whether every terminal-related project has
been implemented.

A strong 1.0 definition would be:

> `Icod.TermInfo` can deterministically identify/load supported conventional
> terminal descriptions, represent their standard and extended terminfo
> semantics completely, and query/expand/output them correctly without native
> ncurses or hidden process-global terminal state.

Under that definition, source tooling, termcap, live input, probing, graphics,
PTYs, curses, and terminal emulation are valuable future systems but are not
prerequisites for a stable `Icod.TermInfo` 1.0.
