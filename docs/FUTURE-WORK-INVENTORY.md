# Icod.TermInfo and Terminal-System Future Work Inventory

This document records terminal-related work which remains outside the current
`Icod.TermInfo` 1.x package-family boundary and identifies its natural
package/layer.

Its purpose is to prevent the existence of a missing terminal feature from
being mistaken for evidence that the feature belongs in the low-level
`Icod.TermInfo` runtime package or another already-defined package-family layer.

The governing distinction is:

> **`Icod.TermInfo` owns immutable terminal-description data, acquisition of
> that data, and pure transformations required to interpret/expand/output it.
> `Icod.TermInfo.Source` owns `.ti` source parsing and inheritance resolution.
> `Icod.TermInfo.Compiler` owns deterministic compiled-entry writing and the
> reusable source-to-compiled engine introduced in the 1.2 line.
> `Icod.TermInfo.Inspection` owns canonical human-readable inspection and
> semantic comparison beginning with the 1.3 line.
> The 1.4 `tic`, `infocmp`, and `toe` projects own command-line policy and depend
> downward on the reusable libraries without changing their ownership.
> Live terminal conversations, process plumbing, and virtual-screen/UI policy
> belong elsewhere.**

---

## 1. Current foundation

The published 1.3 line preserves the Runtime, Source, and Compiler foundations
and adds the optional Inspection layer without moving those responsibilities
into the older packages:

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
- pure conventional compiled-entry parsing;
- explicit directory acquisition;
- deterministic `TERMINFO`/`TERMINFO_DIRS`/user/system discovery;
- provider-local successful-entry caching and new-provider refresh;
- deterministic provider composition;
- `.ti` lexical analysis and source diagnostics;
- unresolved source documents and entries;
- standard/extended capability classification;
- cancellation and `use=` inheritance resolution;
- materialization into the existing immutable `TerminalDescription` model;
- deterministic legacy/wide and extended compiled-entry writing;
- source-to-compiled composition through the existing Source parser/resolver;
- controlled conventional database-layout output;
- deterministic compiler round-trip/differential validation;
- canonical effective and unresolved-source rendering;
- structured effective and source-aware semantic comparison;
- provider-aware inspection orchestration;
- deterministic corpus/fuzz/resource-bound validation;
- no process-global current terminal;
- three-target `net8.0`/`net9.0`/`net10.0` package and compatibility gates.

The runtime public contract remains the frozen 1.0 contract. Source-language
functionality is isolated in `Icod.TermInfo.Source`; compiled writing is isolated
in `Icod.TermInfo.Compiler`; canonical rendering and semantic comparison are
isolated in `Icod.TermInfo.Inspection`, whose 1.3 public contract is now frozen.

Beginning with 1.2, active package-family validation uses the three-target
`net8.0`/`net9.0`/`net10.0` matrix. This additive support change does not rewrite
the frozen 1.0/1.1 target-framework contracts.

Active 1.4 development now adds a `net10.0` command layer. T01 establishes only
the `tic`, `infocmp`, and `toe` shells, their tests, dependency boundaries, and
command contract; operational command semantics follow in later 1.4 tranches.

---

## 2. Inventory by architectural family

| Family | Status / future work | Natural home | Common dependency |
| --- | --- | --- | --- |
| Compiled acquisition | completed for frozen conventional `0432`, ncurses extended sections, and `01036` | `Icod.TermInfo` 0.9/1.x | semantic model |
| Filesystem/system discovery | completed for directory, `TERMINFO`, `TERMINFO_DIRS`, user/default roots | `Icod.TermInfo` 0.9/1.x | compiled parser |
| External-data lifecycle | completed provider-local cache/refresh/concurrency/bounds contract | `Icod.TermInfo` 0.9/1.x | parser + providers |
| Hashed databases | Berkeley DB/ncurses hashed stores | optional later provider/package | compiled parser |
| Historical vendor formats | HP-UX/AIX/OSF/1 divergent binary layouts | optional later | parser abstraction + fixtures |
| Terminfo source language | completed in 1.1: `.ti`, diagnostics, cancellation, `use=` inheritance, materialization | `Icod.TermInfo.Source` | runtime semantic model |
| Terminfo compiler | completed in 1.2: deterministic compiled-entry writer, source compiler engine, and safe database-layout output | `Icod.TermInfo.Compiler` | Runtime + Source |
| Terminfo inspection/comparison | completed in 1.3: canonical effective/source-aware rendering, structured semantic comparison, provider-aware inspection | `Icod.TermInfo.Inspection` | Runtime + Source |
| Terminfo command-line tooling | active in 1.4: T01 establishes `tic`, `infocmp`, and `toe` command shells; semantics follow tranche-by-tranche | 1.4 command projects | CommandFramework + Source/Compiler/Inspection as appropriate |
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

## 3. Core package-family work after 1.1

### 3.1 Terminfo source syntax — completed in 1.1

`Icod.TermInfo.Source` now implements the source-language path:

```text
.ti text
   |
   v
lexer/parser
   |
   v
unresolved source entries
   |
   v
validation + cancellation + use= inheritance
   |
   v
resolved source entry
   |
   v
TerminalDescription
```

The 1.1 contract includes source escapes, Boolean/numeric/string fields,
cancellation, extended capabilities, aliases/descriptions, `use=` inheritance,
source spans and diagnostics, hostile-input bounds, and deterministic duplicate
source-identity handling.

The package remains separate because compiled entries already contain the
resolved semantic result while source processing introduces language and
inheritance concerns that runtime-only consumers do not need.

### 3.2 `tic`-class binary writing

The completed 1.2 line introduced `Icod.TermInfo.Compiler` as an optional sibling package.
Its low-level writer accepts an already-resolved `TerminalDescription` and emits
the conventional compiled formats accepted by the 0.9 runtime parser.

The writer reuses the same canonical metadata and byte semantics as the 0.9
reader rather than maintaining a separate capability table. It is deterministic,
strictly byte-oriented, and fails rather than silently replacing or truncating
unrepresentable state.

The 1.2 dependency boundary is:

```text
Compiler -> Runtime
Compiler -> Source -> Runtime   (from C05)
```

The Runtime package remains dependency-free. Source never depends on Compiler.

The source compiler composes the already-shipped parser and resolver rather than
creating a second terminfo source implementation. Database-layout output is a
separate later layer so the core binary writer remains pure.

The complete pre-C01 representation and package contract is recorded in
`docs/1.2.0-PRE-C01-CONTRACT-AUDIT.md`.

### 3.3 `infocmp`-class inspection — completed in 1.3

The 1.3 line establishes `Icod.TermInfo.Inspection` as the reusable engine for:

- canonical effective `TerminalDescription` rendering;
- normalized unresolved Source rendering without flattening `use=` inheritance;
- effective and source-aware semantic comparison;
- deterministic structured differences;
- provider-aware inspection orchestration;
- future `infocmp`-style applications without embedding console policy.

I01 established the package, dependency, API-baseline, smoke, and release
infrastructure; I02-I06 added the reviewed rendering, comparison, and inspection
surface; and I07 froze the API/package boundary with differential validation.
Inspection depends directly on Runtime and Source and has no production
dependency on Compiler.

### 3.4 Command-line tool suite — active in 1.4

The 1.4 line introduces `tic`, `infocmp`, and `toe` as executable command
projects above the reusable package family. T01 establishes only the shared
command contract: `net10.0`, `Icod.CommandFramework 2.0.0`, thin process entry
points, injected streams, deterministic help/version output, conventional exit
codes, cancellation, and strict dependency direction.

Real database discovery, compilation, inspection/comparison, and enumeration
are intentionally assigned to later 1.4 tranches. The command layer must reuse
existing engines rather than duplicate Source, Compiler, Inspection, or Runtime
semantics.

### 3.5 Termcap conversion

Termcap is less expressive and introduces its own source/search semantics.
Support should be treated as an interoperability/tooling feature rather than as
part of the 0.9 compiled-term acquisition contract.

### 3.6 Hashed database provider

Contemporary ncurses may store compiled entries in a Berkeley DB-backed hashed
database. 0.9 deliberately avoids that runtime dependency.

If real user demand appears, the preferred architecture is an optional provider
which extracts the same compiled entry bytes and passes them to the existing
0.9 parser.

### 3.7 Divergent historical binary dialects

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

- compiled terminfo parsing;
- the 1.1 terminfo source parser/resolver;
- compiled-entry writing;
- Inspection rendering and comparison inputs;
- live input escape decoding;
- active-query response parsing;
- graphics protocol decoding;
- terminal-emulator control parsing.

The source parser/resolver already has a deterministic bounded mutation corpus;
future work should widen coverage deliberately without making ordinary CI depend
on wall-clock randomness or host databases.

### 8.2 Differential testing

Where authoritative implementations exist, optional differential tests can
compare:

- compiled parsing with ncurses `infocmp`;
- Source/Compiler output with `tic`;
- Inspection semantic/rendering results with pinned `infocmp` evidence where useful;
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
        v
1.0 stable runtime contract
        |
        +------------------------------+
        |                              |
        v                              v
1.1 Icod.TermInfo.Source          Icod.Terminal
        |                              |
        v                         +----+----------------+
1.2 compiler/writer              |                     |
        |                        v                     v
        v                     Icod.Pty        input/probing/protocols
1.3 inspection/comparison          |                     |
        |                           +----------+----------+
        v                                      |
1.4 tool commands                             v
                                         Icod.Curses
```

`Icod.Pty` may also proceed independently/parallel because its core OS/process
plumbing is largely orthogonal to terminfo source/compiler work.

---

## 10. Current package-family boundaries

The runtime definition remains explicit:

> `Icod.TermInfo` can deterministically identify/load supported conventional
> terminal descriptions, represent their standard and extended terminfo
> semantics completely, and query/expand/output them correctly without native
> ncurses or hidden process-global terminal state.

The Source boundary is equally explicit:

> `Icod.TermInfo.Source` can parse and resolve supported `.ti` source, preserve
> deterministic source diagnostics and hostile-input bounds, and materialize the
> resolved result into the same immutable runtime semantic model.

The completed 1.2 Compiler boundary is explicit:

> `Icod.TermInfo.Compiler` can deterministically write the supported conventional
> compiled formats, compile through the Source parser/resolver, and publish
> explicit conventional database layouts without moving compiler policy into
> Runtime or Source.

The released 1.3 Inspection boundary is explicit:

> `Icod.TermInfo.Inspection` owns normalized human-readable representation,
> semantic comparison, structured differences, and reusable inspection
> orchestration while Runtime, Source, and Compiler retain their frozen APIs.

The active 1.4 command-layer boundary is equally explicit:

> `tic`, `infocmp`, and `toe` own process/CLI policy and compose the frozen
> managed engines; the commands do not move command-framework dependencies or
> command semantics into Runtime, Source, Compiler, or Inspection.

Termcap, live input, probing, graphics, PTYs, curses, and terminal emulation
remain valuable future or sibling systems. New work should preserve the four
package-family ownership boundaries and the one-way command dependency layer
unless a future deliberate compatibility review revisits them.
