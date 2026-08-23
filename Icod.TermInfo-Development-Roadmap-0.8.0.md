# Icod.TermInfo Development Roadmap — Version 0.8.0 Contract

**Project:** `Icod.TermInfo`  
**Package:** `Icod.TermInfo`  
**Target framework:** `net10.0`  
**Language:** C# 13  
**Status:** Implementation in progress — T27 Windows Console (`winconsole`)<br>
**Previous contract:** `0.7.0` — complete and frozen  
**Contract target:** `0.8.0`  
**Initial development version:** `0.8.0-alpha.1`

---

# 1. Purpose

Version 0.8.0 is the **semantic-completion release** for `Icod.TermInfo`.

Version 0.7.0 established a mature immutable terminal-description architecture with:

- standard capabilities required by the selected ANSI, DEC, and xterm profiles;
- generic extended/user-defined Boolean, numeric, and string capabilities;
- DEC VT102/VT220 and modern xterm-family built-ins;
- semantic indexed and direct-RGB color handling;
- full-screen/cursor-addressing primitives;
- modern xterm mouse, focus, bracketed-paste, key, reporting, and related descriptive metadata;
- generic terminfo parameter expansion;
- generic padding-aware output;
- explicit separation between terminal-description data and live terminal-session behavior.

Version 0.8.0 SHALL preserve those decisions while completing the in-memory terminfo model and runtime semantics required before arbitrary compiled terminal descriptions are admitted.

The principal goals of 0.8.0 are:

1. complete the standard terminfo capability universe selected for the future compiled-database baseline;
2. make that universe discoverable through canonical metadata and enumeration;
3. complete terminal identity with the verbose terminal description/long name;
4. freeze signed 32-bit numeric semantics for standard and extended capabilities;
5. complete and harden the terminfo parameter-program engine for arbitrary future capability programs;
6. add safe parameter-program analysis and reusable parsed-program execution;
7. complete terminal-aware `tputs`-class padding semantics without introducing process-global terminal state;
8. freeze an explicit reversible 8-bit capability-string representation;
9. make standard and extended parameterized strings operationally symmetrical;
10. formalize profile inheritance/cancellation semantics;
11. add authoritative built-in `winconsole`, `ms-terminal`, and `ms-terminal-direct` profiles;
12. freeze the exact arbitrary-compiled-terminfo target that version 0.9.0 SHALL implement;
13. check in a deterministic compiled-entry fixture corpus so 0.9 can begin with an already-frozen semantic target.

Version 0.8.0 SHALL **NOT** implement arbitrary compiled terminfo loading, system terminfo database discovery, `TERMINFO`, `TERMINFO_DIRS`, or filesystem-loaded terminal-description caching.

Those features are **required** for version 0.9.0.

The intended release progression is:

```text
0.6.0
    foundational managed terminfo
    ANSI / VT100 / dumb
    parameter expansion
    padding/output
    provider architecture

0.7.0
    DEC + modern xterm families
    extended capabilities
    semantic indexed/direct color
    full-screen primitives
    modern descriptive metadata

0.8.0
    complete in-memory terminfo semantics
    complete standard capability metadata/enumeration
    complete/safe parameter runtime
    complete terminal-aware output runtime
    exact 8-bit capability semantics
    profile cancellation/composition fidelity
    Windows Console
    Windows Terminal
    Windows Terminal direct color
    frozen arbitrary-terminfo target contract

0.9.0
    REQUIRED arbitrary terminal support
    compiled terminfo parser
    system/directory providers
    TERMINFO / TERMINFO_DIRS / ~/.terminfo
    external-data diagnostics, caching, and refresh policy
```

The design objective is that 0.9.0 should primarily teach the library **where arbitrary terminfo comes from and how to read it**, not redesign what terminfo means once it is in memory.

---

# 2. Relationship to the 0.7.0 Contract

The 0.7.0 release is a frozen historical contract.

Version 0.8.0 SHALL preserve, unless an explicit 0.8 contract change says otherwise:

- `TerminalDescription` immutability;
- exact and conservative terminal-name resolution;
- `TerminalDatabase.BuiltIn` as deterministic, dependency-free, and I/O-free;
- all selected 0.7 built-in profile identities and aliases;
- the established meanings of `dumb`, `ansi`, `vt100`, `vt102`, `vt220`, and the selected xterm-family built-ins;
- no automatic mapping of unknown names to ANSI, VT100, xterm, Windows Console, or Windows Terminal;
- no process-global `cur_term` equivalent;
- no hidden mutable current-terminal state;
- standard typed capability APIs as the preferred APIs for standard capabilities;
- generic extended capability storage and exact case-sensitive extended names;
- the standard-name collision rule for extended capabilities;
- generic parameter expansion with explicit parameter values and optional caller-owned expansion context;
- generic padding/output transformation;
- semantic color behavior derived from capabilities rather than terminal names;
- live dimensions remaining distinct from profile dimensions;
- explicit Windows virtual-terminal mode enablement;
- provider ordering remaining deterministic and first-match-wins;
- modern protocol strings remaining descriptive metadata rather than live protocol operations;
- the boundary between `Icod.TermInfo` and future live terminal/session, curses/UI, and PTY/process layers.

Existing public enum numeric values and existing public member signatures SHALL remain source- and binary-compatible wherever practical.

When the standard capability enums are completed in 0.8:

- new enum members SHALL be appended;
- existing enum members SHALL NOT be renumbered;
- a managed enum ordinal SHALL NOT be used as a compiled terminfo table index;
- the compiled table index SHALL live in canonical capability metadata.

---

# 3. Governing Boundary

The 0.7 governing rule remains authoritative:

> `Icod.TermInfo` owns immutable terminal-description data and pure transformations required to interpret, expand, or emit terminal capabilities. It does not own a live terminal session, maintain a virtual screen, or interpret a continuous input stream into application events.

Version 0.8.0 refines that rule with a second boundary:

> `Icod.TermInfo` 0.8 SHALL complete the **meaning and execution** of an arbitrary terminfo description in memory. Version 0.9 SHALL add the **acquisition** of arbitrary descriptions from compiled databases.

The intended project-family boundary remains:

```text
Icod.TermInfo
    immutable terminal descriptions
    complete standard capability catalog
    standard capability metadata/enumeration
    extended capabilities
    terminal identity and verbose description
    parameter-program parsing/analysis/evaluation
    bounded parsed-program reuse
    padding/output transformation
    semantic color interpretation
    built-in terminal profiles
    descriptive key/protocol metadata

Icod.Terminal                 (future/separate)
    live terminal session ownership
    raw/cooked mode lifecycle
    keyboard/mouse/paste/focus decoding
    terminal probing
    host negotiation
    clipboard/hyperlink operations
    full-screen lifecycle
    progress/spinner helpers

Icod.Curses                   (future/separate)
    virtual screen
    refresh optimization
    windows/pads/panels
    menus/forms/widgets
    high-level style policy

Icod.Pty                      (future/separate)
    pseudo-terminal creation
    ConPTY/PTY lifecycle
    child-process plumbing
```

Version 0.8.0 SHALL NOT introduce a live `Terminal` object merely because Windows host support expands.

---

# 4. Semantic Completion Versus Database Acquisition

The most important 0.8/0.9 architectural distinction is:

```text
0.8 responsibility
    bytes already interpreted somehow
        ↓
    complete immutable TerminalDescription
        ↓
    query / enumerate / analyze / expand / output correctly

0.9 responsibility
    terminal name / compiled bytes / system database
        ↓
    locate and parse
        ↓
    the same already-complete TerminalDescription model
```

At the end of 0.8.0 it SHALL be possible, without changing library internals, to construct a `TerminalDescription` representing any terminal description expressible by the 0.9 supported semantic model, including:

- any known standard Boolean capability;
- any known standard numeric capability;
- any known standard string capability;
- any extended Boolean capability;
- any extended numeric capability;
- any extended string capability;
- signed 32-bit numeric values;
- high-byte capability-string data represented through the 0.8 byte contract;
- arbitrary adopted terminfo parameter programs;
- historical padding annotations and profile-dependent padding facts;
- a canonical name;
- aliases;
- a verbose terminal description.

If 0.9 implementation requires redesigning any of the following, the 0.8 readiness goal has not been met:

- `TerminalDescription` identity shape;
- the standard capability catalog;
- numeric value width;
- standard capability metadata;
- standard capability enumeration;
- extended capability representation;
- parameter-program semantics;
- parameter-program safety rules;
- padding semantics;
- raw capability-string representation;
- provider clean-miss semantics.

---

# 5. Scope of the 0.8.0 Contract

## 5.1 Included

Version 0.8.0 SHALL include:

- everything retained from the frozen 0.7.0 contract;
- a complete standard Boolean capability catalog for the selected conventional ncurses/System V-compatible baseline;
- a complete standard numeric capability catalog for that baseline;
- a complete standard string capability catalog for that baseline;
- append-only enum completion without renumbering 0.7 values;
- canonical metadata for every standard capability;
- fixed future compiled-table indices independent of managed enum values;
- read-only enumeration/introspection of the standard capability universe;
- read-only enumeration of standard capabilities present in a particular terminal description;
- terminal verbose description/long-name support;
- explicit signed 32-bit numeric capability semantics;
- complete adopted terminfo parameter-language conformance;
- parameter-program structural/type analysis;
- parameter/padding parser hardening and resource-safety rules;
- bounded lazy parsed-program reuse for immutable terminal descriptions;
- explicit expansion convenience for parameterized extended string capabilities;
- explicit reversible 8-bit capability-string semantics;
- terminal-aware padding semantics for `xon`, `pb`, `npc`, and `pad`;
- profile composition with explicit cancellation behavior;
- first-class authoritative `winconsole` built-in support;
- first-class authoritative `ms-terminal` built-in support;
- first-class authoritative `ms-terminal-direct` built-in support;
- golden upstream provenance for new Windows profiles;
- a frozen 0.9 compiled-binary semantic contract;
- a checked-in deterministic compiled-entry fixture corpus for future 0.9 parser work;
- frozen provider clean-miss/error semantics for future I/O providers;
- reserved compiled-database exception vocabulary which does not collide with the existing parameter-program `TermInfoFormatException`;
- updated public API baseline, documentation, samples, package validation, and completion gate.

## 5.2 Explicitly excluded from 0.8.0

Version 0.8.0 SHALL NOT implement:

- a production compiled terminfo entry parser;
- arbitrary terminal loading from compiled terminfo files;
- `/usr/share/terminfo` or other system directory discovery;
- an explicit directory-tree terminfo provider;
- `TERMINFO` lookup;
- `TERMINFO_DIRS` lookup;
- `$HOME/.terminfo` lookup;
- platform system terminfo path search;
- `TERMINFO=hex:...` parsing;
- `TERMINFO=b64:...` parsing;
- caching of filesystem-loaded terminal descriptions;
- filesystem refresh semantics;
- negative caching for system terminal lookup;
- filesystem watchers;
- Berkeley DB hashed terminfo storage;
- termcap source/database parsing;
- `TERMCAP`;
- `TERMPATH`;
- terminfo source parsing;
- `use=` source inheritance resolution;
- compiled-entry writing;
- `tic` runtime functionality;
- `infocmp` runtime functionality;
- full database enumeration analogous to `toe`;
- automatic Windows host/profile selection based on environment hints;
- terminal probing or device-identification handshakes;
- keyboard/mouse/paste/focus event decoding;
- Unix `termios` session ownership;
- PTY or ConPTY lifecycle;
- curses windows, pads, panels, menus, forms, or refresh optimization;
- spinner/progress animation policy;
- automatic full-screen session lifecycle.

The presence of checked-in compiled fixtures in tests SHALL NOT be interpreted as implementation of a production compiled-entry parser.

---

# 6. Complete Standard Capability Catalog

## 6.1 Why completeness belongs in 0.8

Version 0.7 intentionally expanded the standard capability vocabulary only as far as the selected DEC/xterm work required.

That selective model is no longer sufficient for the next release boundary.

A conventional compiled terminfo entry represents standard Boolean, numeric, and string capabilities by fixed positional tables rather than by storing their names with each value. Therefore arbitrary compiled terminfo support in 0.9 requires the complete supported standard table to be known in advance.

Completing the table in 0.8 also improves fidelity for `winconsole`, whose authoritative description uses standard capabilities outside the subset required by the 0.7 built-ins.

Version 0.8.0 SHALL therefore complete the standard capability catalog used by the selected conventional ncurses/System V-compatible binary baseline.

## 6.2 Canonical capability record

There SHALL be one authoritative metadata record for every standard capability containing at least:

- capability kind: Boolean, numeric, or string;
- fixed compiled binary table index within that kind;
- traditional terminfo short code (`am`, `cols`, `cup`, etc.);
- long/variable name;
- termcap code where one exists;
- corresponding managed enum member.

The implementation SHOULD use one checked-in canonical source of truth and either generate or mechanically validate any derived enum/name/index tables.

Builds SHALL NOT require network access to regenerate capability metadata.

## 6.3 Public enum stability

`BooleanCapability`, `NumericCapability`, and `StringCapability` SHALL remain the preferred typed APIs for standard capabilities.

Requirements:

- every 0.7 enum numeric value remains unchanged;
- new values are append-only;
- managed enum ordinal is never treated as a compiled binary index;
- binary indices live in metadata;
- every standard short code maps deterministically to exactly one kind/member;
- every supported binary table index maps deterministically to exactly one standard capability;
- unknown future trailing compiled table positions can later be ignored safely without shifting known indices.

## 6.4 Mechanical validation

Tests SHALL verify:

- unique binary indices within each capability kind;
- unique short names within each capability kind;
- deterministic managed-enum mapping;
- deterministic long-name mapping;
- deterministic termcap-code mapping where defined;
- complete coverage of the selected baseline;
- stability of all 0.7 enum numeric values.

---

# 7. Standard Capability Metadata and Enumeration

0.8 SHALL make the completed standard model inspectable rather than only queryable one capability at a time.

Two distinct kinds of enumeration are required.

## 7.1 Catalog enumeration

Managed code SHALL be able to enumerate the entire standard capability universe known to this version of `Icod.TermInfo`.

Each enumerated metadata record SHALL provide at least:

- capability kind;
- managed identifier;
- terminfo short name;
- terminfo long/variable name;
- termcap code when present;
- future compiled binary index.

The public API SHALL be read-only and deterministic.

The exact public type names are subject to T30 API review.

## 7.2 Per-description enumeration

Managed code SHALL also be able to enumerate standard capabilities which are effectively present in a particular `TerminalDescription`.

This enumeration SHALL expose actual effective terminal values and SHALL distinguish capability kinds without forcing consumers to probe every enum value manually.

Possible final shapes include separate read-only Boolean/numeric/string enumerations or one typed discriminated metadata/value representation.

The exact public shape is subject to T30 review, but these semantics are required:

- no mutation through enumeration;
- deterministic ordering;
- no process-global state;
- only effective present capabilities are returned;
- normal enumeration does not expose internal canceled state;
- extended capabilities remain separately enumerable through the established extended-capability model.

## 7.3 Lookup semantics remain conservative

Adding long names, termcap codes, and metadata enumeration SHALL NOT silently broaden normal string lookup semantics.

Existing normal terminfo string lookup SHALL continue to use documented terminfo short names unless a separate explicit metadata lookup API is deliberately adopted.

---

# 8. Terminal Identity and Verbose Description

A complete terminfo identity has more information than a canonical name and aliases.

The conventional names field represents:

```text
primary-name|alias-1|alias-2|verbose terminal description
```

Version 0.8.0 SHALL preserve the final verbose description separately.

`TerminalDescription` SHALL gain an immutable concept tentatively named:

```csharp
public string? Description { get; }
```

or an equivalently clear final API name.

Semantics:

- `Name` remains the canonical terminal name;
- `Aliases` contains actual resolvable synonyms;
- `Description` contains the verbose descriptive field;
- the verbose description is not treated as an alias;
- built-in profiles SHOULD populate the description where a stable authoritative description exists;
- builders/providers can provide a description without changing terminal-name resolution;
- description data is immutable and thread-safe.

The final API spelling is subject to T30 review, but the semantic field SHALL exist before 0.9.

---

# 9. Numeric Capability Width

Version 0.8.0 SHALL explicitly freeze standard and extended numeric capability values as signed 32-bit managed integers.

The existing use of `int` is therefore elevated from an implementation fact to a contract requirement.

Requirements:

- valid values above the signed 16-bit range are preserved;
- no standard or extended numeric path narrows a valid value to 16 bits;
- color semantics continue to support values such as `colors#0x1000000`;
- enumeration and metadata/value APIs preserve the full signed 32-bit value;
- future 0.9 parsing of the extended-number compiled format requires no model change.

Tests SHALL include representative values around and above legacy 16-bit boundaries.

No 0.8 binary parser is required to prove this contract.

---

# 10. Parameter-Expansion Conformance

0.7 proved the generic parameter engine against selected ANSI/DEC/xterm programs.

0.8 SHALL treat the parameter engine as a general runtime for arbitrary future terminfo capability programs.

The conformance audit SHALL cover the complete adopted documented terminfo parameter language used by current conventional entries, including at least:

- numeric parameters;
- string parameters;
- `%i`;
- `%p1` through `%p9`;
- `%P` variable assignment;
- `%g` variable retrieval;
- dynamic variable semantics;
- persistent/static variable semantics through explicit caller-owned context;
- integer constants;
- character constants;
- arithmetic operators;
- bitwise operators;
- logical operators;
- comparisons;
- conditionals `%?` / `%t` / `%e` / `%;`;
- `%l`;
- character output;
- string output;
- decimal/octal/hexadecimal numeric output;
- printf-style flags;
- field width;
- precision;
- legacy terminfo formatting constructs still intentionally accepted by the selected compatibility baseline.

Requirements:

- no terminal-name-specific branch;
- no Windows-specific branch;
- malformed programs fail deterministically;
- type mismatches fail deterministically;
- no partially expanded output is returned after evaluation failure;
- ordinary conformance tests use checked-in deterministic cases rather than requiring host ncurses at test time;
- differential testing against host ncurses MAY be used as a development aid but SHALL NOT define the package's runtime dependency or golden expectations.

Existing 0.7 expansion behavior SHALL remain compatible for valid programs.

---

# 11. Parameter-Program Analysis and Safe Evaluation

Arbitrary 0.9 terminal descriptions will contain programs which were not hand-selected by this project.

Version 0.8 SHALL therefore add a safe analysis phase around parsed terminfo parameter programs.

## 11.1 Required analysis information

A parsed `TermInfoParameterProgram` or associated immutable analysis representation SHALL be able to determine, where meaningful:

- parameter indices referenced by the program;
- the highest parameter index used;
- whether each parameter is used in numeric and/or string context;
- dynamic variable usage;
- persistent/static variable usage;
- instruction count;
- structural conditional nesting;
- stack requirements or validated stack-safety information;
- whether the program is structurally valid before evaluation.

The analysis does not need to clone ncurses C varargs APIs. `TermInfoParameter` already provides a safer managed typed value representation.

## 11.2 Evaluation safety

Evaluation SHALL validate supplied parameter values against the parsed program's requirements before or while executing them deterministically.

A malformed or incompatible program/parameter set SHALL never be treated as terminal-specific fallback behavior.

## 11.3 Public versus internal analysis surface

The analysis machinery is required.

A public analysis API is OPTIONAL and SHALL be exposed only if T30 concludes that it provides durable consumer value without unnecessarily freezing parser internals.

Internal implementation details such as individual instruction objects SHALL remain internal unless there is a concrete public contract reason to expose them.

---

# 12. Parameter and Padding Parser Hardening

Version 0.8 SHALL harden both transformation engines before 0.9 can feed them external database content.

## 12.1 General hardening rules

The implementation SHALL use checked and bounded behavior for:

- source-length processing;
- instruction-count construction;
- conditional nesting;
- evaluation stack growth;
- numeric parsing and arithmetic where overflow is possible;
- field-width and precision handling;
- output-size calculations;
- padding numeric parsing;
- affected-line multiplication;
- delay calculations.

Exact sane limits MAY be implementation-defined, but they SHALL be:

- deterministic;
- documented where they are externally observable;
- high enough for legitimate conventional terminfo entries;
- covered by boundary tests;
- resistant to unbounded allocation or pathological CPU use.

## 12.2 Error behavior

Malformed parameter programs SHALL continue to use the existing parameter-program exception vocabulary.

Malformed padding strings SHALL continue to use the padding-specific exception vocabulary.

The implementation SHALL NOT reuse these exception types later for malformed compiled terminfo database records.

## 12.3 Regression corpus

Fuzzing/property generation MAY be used during development.

Every defect found through such testing SHALL gain a deterministic checked-in regression test before the tranche is considered complete.

---

# 13. Parsed-Program Reuse and Extended String Expansion

## 13.1 Why parsed-program reuse belongs in 0.8

`TermInfoParameterProgram` is an immutable reusable representation.

Repeatedly parsing immutable capability strings such as `cup`, `setaf`, `setab`, or extended `XM` is unnecessary and becomes more expensive once arbitrary future terminal descriptions contain many parameterized capabilities.

Version 0.8 SHALL therefore provide bounded lazy parsed-program reuse associated with immutable terminal descriptions.

## 13.2 Cache scope

The cache SHALL be:

- per `TerminalDescription` or equivalently bounded by one immutable description;
- lazy;
- thread-safe;
- transparent to public semantics;
- bounded by the string capabilities actually contained in that description;
- free of process-global lifetime ambiguity.

The implementation SHALL NOT add a hidden unbounded process-global dictionary keyed by arbitrary source strings.

Constructing a `TermInfoParameterProgram` directly remains valid and reusable independently of a terminal description.

## 13.3 Standard capability expansion

Existing standard typed expansion APIs SHALL transparently benefit from parsed-program reuse without changing their valid-output semantics.

## 13.4 Extended capability expansion

Version 0.8 SHALL add an explicit convenience path for parameterized extended string capabilities.

Conceptually:

```csharp
terminal.ExpandExtended("XM", parameters);
```

and an explicit-context equivalent are acceptable shapes.

Requirements:

- extended names remain exact and case-sensitive;
- the API rejects a missing capability deterministically;
- the API rejects a non-string extended capability deterministically;
- standard and extended namespaces are not silently conflated;
- expansion uses the same parser/analyzer/evaluator as standard strings;
- parsed-program reuse applies equally to extended strings.

The final method names are subject to T30 review.

---

# 14. 8-Bit Capability-String Contract

Native terminfo capability values are byte strings without an intrinsic UTF-8 encoding.

`Icod.TermInfo` exposes capability programs as .NET `string`, so 0.8 SHALL freeze an explicit reversible bridge before binary loading exists.

The 0.8 rule is:

> Every terminfo capability byte `0x01` through `0xFF` maps one-to-one to the Unicode code point with the same value through `Encoding.Latin1`; NUL remains the compiled-string terminator and is not representable as embedded compiled string data.

Requirements:

- bytes `0x80` through `0xFF` are never silently decoded as UTF-8;
- `Encoding.Latin1.GetBytes(value)` reproduces the corresponding terminfo capability bytes for the one-to-one representation;
- parameter parsing/analysis/evaluation preserves high-byte characters as data where they are not syntax;
- padding parsing preserves high-byte non-padding content;
- exact byte-stream output examples use `Encoding.Latin1`;
- ordinary application text encoding remains the caller's concern and is distinct from raw terminfo capability-byte fidelity;
- a new public terminfo-string wrapper SHALL NOT be added unless implementation proves that the one-to-one `string` representation is insufficient.

Tests SHALL cover:

- ASCII control sequences;
- `0x7F`;
- representative `0x80`–`0xFF` bytes;
- storage in standard and extended strings;
- parameter expansion;
- padding removal;
- exact byte-stream round trip.

---

# 15. Complete Terminal-Aware Padding Semantics

0.6/0.7 deliberately supplied explicit generic padding parsing and delay handling without process-global terminal state.

A complete libtinfo-class output runtime must also account for profile capabilities traditionally used by `tputs`, including:

- `xon` — XON/XOFF flow-control behavior;
- `pb` — padding baud-rate threshold;
- `npc` — no pad character;
- `pad` — explicit pad character.

Version 0.8 SHALL complete the model and explicit policy path for those semantics.

## 15.1 Capability coverage

The completed standard capability catalog SHALL include the relevant padding capabilities including `pb` and `pad`.

## 15.2 Explicit terminal-aware output policy

A new overload or immutable policy/options object MAY accept:

- a `TerminalDescription`;
- affected-line count;
- optional caller-supplied transport facts such as baud rate;
- padding mode;
- caller-supplied delay provider where applicable.

The exact public shape is subject to T30 review.

## 15.3 Required semantics

The implementation SHALL correctly model:

- affected-line multiplication where `*` is present;
- mandatory `/` padding;
- suppression of advisory/non-mandatory padding when appropriate for `xon`;
- `pb` threshold behavior when baud rate is supplied;
- `npc` behavior;
- `pad` character behavior where character padding is selected;
- timed-delay behavior through injectable/testable `ITermInfoDelayProvider`;
- deterministic validation of delay computations.

Mandatory padding SHALL NOT be accidentally discarded by advisory flow-control policy.

## 15.4 No live transport ownership

`Icod.TermInfo` SHALL NOT:

- discover serial baud rate by taking ownership of a tty;
- introduce process-global `ospeed`;
- own an output file descriptor;
- mutate terminal modes merely to compute padding;
- introduce a process-global current terminal.

Transport facts remain caller-owned inputs.

Existing simple padding/output APIs SHALL remain compatible.

---

# 16. Profile Composition and Cancellation Semantics

Reusable fragments are already valuable for DEC/xterm profiles and become more important for the Windows descriptions.

Version 0.8 SHALL formalize internal profile composition so inherited capability cancellation is explicit and deterministic.

## 16.1 Construction states

During composition, the implementation SHALL be capable of distinguishing conceptually:

```text
absent
present
canceled
```

A cancellation means that a capability inherited from an earlier fragment/source layer must not survive into the final effective description.

## 16.2 Effective public description

Normal public `TerminalDescription` lookup MAY continue to collapse canceled and absent to the same effective result.

A public canceled-state API is not required in 0.8.

The important contract is that cancellation is not mistaken for a real false/negative/string value and cannot leak an inherited capability accidentally.

## 16.3 Extended capabilities

The existing rule remains:

- standard names remain authoritative in the standard catalog;
- an extended capability SHALL NOT silently shadow a known standard capability;
- extended names remain case-sensitive;
- cancellation/removal during profile composition must remain unambiguous.

## 16.4 Future 0.9 relevance

The internal cancellation semantics adopted in 0.8 SHALL be suitable for mapping future compiled absent/canceled encodings without redesigning the effective immutable description model.

---

# 17. Windows Console Profile (`winconsole`)

## 17.1 Profile identity

Version 0.8.0 SHALL add a first-class built-in profile for the authoritative ncurses identity:

```text
winconsole
```

It SHALL NOT alias classic Windows Console behavior to `ansi`, `xterm`, or `xterm-256color`.

No convenience aliases such as `conhost` SHALL be invented unless an authoritative terminal identity justifies them in a later contract.

## 17.2 Virtual-terminal interpretation

The `winconsole` TermInfo profile SHALL describe terminal-control sequences genuinely supported by the selected modern Windows Console baseline when virtual-terminal processing is enabled.

Classic Win32 console calls SHALL NOT be invented as fake terminfo capability strings.

## 17.3 Relationship to `WindowsVirtualTerminal`

The existing explicit/reversible Windows VT-mode helper remains the state-changing seam.

Loading or accessing `TerminalProfiles.WinConsole` (or the final property name) SHALL NOT:

- call `SetConsoleMode`;
- enable VT processing;
- mutate standard handles;
- inspect a live console merely to construct the profile.

The intended use remains conceptually:

```text
choose/resolve winconsole profile
        +
explicitly enable Windows VT output when the application owns that decision
```

## 17.4 Fidelity and cancellation

The profile SHALL be golden-tested against a recorded authoritative ncurses `terminfo.src` baseline.

Source cancellations SHALL remove inherited capabilities rather than allowing internal composition fragments to leak them into the final profile.

Standard and extended capabilities required by the authoritative profile SHALL be represented using the complete 0.8 model.

## 17.5 Color boundary

No unsupported direct-color `winconsole` identity SHALL be invented merely because some Windows host versions may accept additional SGR sequences.

The profile means the selected authoritative `winconsole` terminfo identity, not "everything a recent conhost might happen to do."

---

# 18. Windows Terminal Profiles

Version 0.8.0 SHALL add separate first-class built-ins corresponding to the authoritative current ncurses identities:

```text
ms-terminal
ms-terminal-direct
```

The profiles SHALL be modeled independently from xterm even where internal implementation correctly reuses xterm/DEC capability fragments.

## 18.1 `ms-terminal`

`ms-terminal` SHALL represent the selected authoritative indexed-color Windows Terminal description.

Its capability values, cancellations, keys, screen behavior, and modern protocol metadata SHALL follow the recorded source baseline.

## 18.2 `ms-terminal-direct`

`ms-terminal-direct` SHALL represent the selected authoritative direct-color Windows Terminal description.

Direct-color behavior SHALL continue to use the generic semantic color machinery and the profile's actual capability programs.

Generic code SHALL NOT contain a Windows-Terminal-specific RGB escape-string branch.

## 18.3 Composition rules

Internal fragment reuse is encouraged where the authoritative source genuinely inherits common xterm/DEC data.

However:

- Windows-specific overrides SHALL win;
- Windows-specific cancellations SHALL remove inherited values;
- unsupported xterm capabilities SHALL not survive merely because an xterm fragment was reused;
- the final profile SHALL be golden-tested as a complete effective description, not only as fragments.

## 18.4 Identity

Windows Terminal SHALL NOT silently resolve as `xterm`, `xterm-256color`, or another xterm identity.

`ms-terminal` and `ms-terminal-direct` are separate terminal identities with separate advertised semantics.

---

# 19. Windows Host Hints Are Not Terminal Identity

Version 0.8 SHALL NOT silently choose or upgrade a terminal description based only on environment hints such as:

- `WT_SESSION`;
- `WT_PROFILE_ID`;
- `COLORTERM`.

These values may be useful to a future policy/session layer, but they are not authoritative terminal identity.

They can be inherited through process trees and may not describe the actual endpoint that consumes the application's output.

If 0.8 exposes any Windows/environment hints, they SHALL remain descriptive-only.

Automatic host negotiation/probing belongs in a future `Icod.Terminal` layer.

---

# 20. 0.9 Arbitrary-Term Support Contract Reservation

Version 0.9.0 is **required** to add arbitrary terminal support.

For the purposes of this roadmap, arbitrary terminal support means:

> For every valid terminal description encoded in the supported conventional System V/ncurses compiled formats, `Icod.TermInfo` can load that description by arbitrary terminal name without adding terminal-specific C# code or registering a new built-in profile.

The 0.8 release SHALL deliberately prepare for, but SHALL NOT implement, the following 0.9 areas:

1. conventional compiled terminfo parsing;
2. ncurses extended compiled sections;
3. ncurses extended-number/32-bit numeric format;
4. absent/canceled binary handling;
5. explicit directory-tree database provider;
6. `TERMINFO`;
7. `TERMINFO_DIRS`;
8. `$HOME/.terminfo` on applicable platforms;
9. platform system database locations;
10. encoded `TERMINFO=hex:...`;
11. encoded `TERMINFO=b64:...`;
12. malformed/untrusted compiled-database policy;
13. provider-instance loaded-description caching;
14. refresh/new-provider semantics;
15. system-provider composition with optional explicit built-in fallback.

`linux`, `screen`, `tmux`, `rxvt`, VTE, Kitty, foot, and other Unix/emulator identities SHALL NOT be added to 0.8 merely as preparation for this work.

The point of 0.9 is to stop requiring a built-in C# profile for every conventional installed terminfo identity.

---

# 21. Frozen 0.9 Compiled Binary Semantic Baseline

0.8 SHALL freeze the binary dialects that the first 0.9 arbitrary-terminal implementation is expected to support.

This is a **contract and fixture requirement only**. No production parser belongs in 0.8.

## 21.1 Supported future baseline

The 0.9 baseline SHALL include the conventional ncurses/System V-compatible entry family used by default ncurses configurations:

- legacy magic `0432` (octal), with 16-bit numeric table;
- ncurses extended capability data appended to the conventional entry;
- extended-number magic `01036` (octal), with 32-bit numeric values.

Multi-byte numeric/header values use the conventional little-endian compiled representation documented for this baseline.

## 21.2 Conventional layout

The future parser contract SHALL account for:

1. six-short header;
2. names section;
3. Boolean table;
4. required alignment before numerics;
5. numeric table;
6. string-offset table;
7. string table.

## 21.3 Extended layout

The future parser contract SHALL account for:

- extended header/counts;
- extended Booleans;
- extended numerics;
- extended string offsets;
- extended string values;
- extended capability names;
- ordering rules among extended kinds.

## 21.4 Absent and canceled values

The 0.9 binary contract SHALL distinguish absent and canceled values during parsing.

The future parser must preserve enough information to prevent canceled data from becoming a real capability value or leaking inherited values during construction.

The effective public lookup model MAY continue to collapse canceled values to absence where appropriate.

## 21.5 Vendor portability boundary

X/Open does not standardize one universal compiled binary representation across every historical vendor.

The initial 0.9 contract SHALL target the selected conventional System V/Solaris-like ordering used by default ncurses, including ncurses extensions and 32-bit numeric format.

0.8 SHALL NOT claim that this frozen future target covers every HP-UX, AIX, OSF/1, or other divergent historical vendor binary dialect.

---

# 22. 0.9 Fixture Corpus and Provenance

Before 0.8.0 is complete, the repository SHALL contain a deterministic compiled-entry fixture corpus suitable for immediate 0.9 parser development.

## 22.1 Fixture principle

For each fixture, preserve where practical:

```text
source .ti description
compiled binary entry
expected semantic manifest
exact tic/ncurses provenance
generation instructions
```

Normal tests SHALL consume checked-in fixtures and SHALL NOT require `tic` to be installed on the test host.

`tic` MAY be used by maintainers to generate/re-generate documented fixture inputs deliberately.

## 22.2 Required legacy fixtures

Include at least:

- minimal valid legacy entry;
- primary name;
- aliases;
- verbose description;
- Boolean capability values;
- numeric capability values;
- string capability values;
- absent values;
- canceled values;
- odd/even Boolean-table alignment cases;
- representative parameterized strings;
- representative padding annotations;
- representative high-byte string data.

## 22.3 Required extended/32-bit fixtures

Include at least:

- extended Boolean;
- extended numeric;
- extended string;
- multiple extended names;
- standard/extended collision scenario for future rejection/handling tests;
- 32-bit numeric values above legacy 16-bit range;
- direct-color-scale values such as `0x1000000` where appropriate;
- malformed extended-count/offset examples for future parser hardening.

## 22.4 Malformed fixture seeds

0.8 SHOULD also check in small malformed binary seeds covering obvious future parser boundaries such as:

- truncated header;
- impossible counts;
- bad terminator;
- illegal offset;
- unsupported magic;
- malformed extended header.

These files do not imply a 0.8 parser. They simply ensure 0.9 begins with a stable adversarial corpus.

## 22.5 Architecture guard

A 0.8 test/source guard SHALL make it difficult to accidentally introduce a production compiled parser while adding fixture infrastructure.

---

# 23. Provider Semantics Reserved for 0.9

The existing `ITerminalDescriptionProvider` and ordered `TerminalDatabase` composition remain the intended high-level provider architecture.

Version 0.8 SHALL freeze one important semantic rule before I/O providers exist:

> `ITerminalDescriptionProvider.TryLoad(...) == false` means a clean provider miss.

It SHALL NOT mean an undifferentiated mixture of:

- terminal absent;
- permission denied;
- corrupt compiled entry;
- unsupported compiled format;
- I/O failure;
- internal parser failure.

Future 0.9 providers may search multiple physical candidates internally and apply documented recovery rules, but the high-level clean-miss contract remains meaningful.

0.8 SHALL NOT yet implement a broad database diagnostics framework.

## 23.1 Future exception vocabulary

The existing `TermInfoFormatException` belongs to malformed parameter-program syntax and SHALL retain that meaning.

A future malformed compiled-entry exception SHALL therefore use a distinct name.

The 0.8 contract reserves a name conceptually such as:

```text
CompiledTermInfoFormatException
```

The exact 0.9 type name may be refined when the parser API is designed, but it SHALL NOT collide semantically with the existing parameter-program exception.

---

# 24. Testing Strategy

Version 0.8 materially enlarges the semantic surface even though it still does not parse external databases at runtime.

Testing SHALL combine compatibility guards, complete metadata tests, parameter/output conformance, adversarial transformation tests, golden Windows profiles, concurrency tests, and future-parser fixtures.

## 24.1 0.7 compatibility tests

Verify:

- all 0.7 public enum numeric values are unchanged;
- all 0.7 built-in identities still resolve exactly;
- existing 0.7 aliases remain unchanged;
- existing selected capability values remain golden-compatible;
- existing semantic color behavior is unchanged;
- existing parameter expansions remain valid;
- existing simple padding APIs remain compatible;
- no previously unsupported terminal begins resolving accidentally except the explicitly added Windows identities.

## 24.2 Capability catalog tests

Verify:

- complete selected standard Boolean coverage;
- complete selected standard numeric coverage;
- complete selected standard string coverage;
- unique binary indices within kinds;
- unique short names;
- correct long names;
- correct termcap codes where defined;
- enum/catalog coverage is complete;
- enum ordinals are not assumed to equal binary indices;
- existing enum values did not renumber.

## 24.3 Enumeration tests

Verify:

- deterministic catalog enumeration;
- deterministic per-description enumeration;
- present standard Boolean/numeric/string values appear correctly;
- absent values do not appear as present;
- extended enumeration remains distinct and compatible;
- enumeration cannot mutate a description;
- concurrent enumeration is safe.

## 24.4 Identity/description tests

Verify:

- canonical names;
- aliases;
- verbose description;
- description does not resolve as an alias;
- built-in descriptions remain immutable.

## 24.5 Numeric-width tests

Cover representative signed 32-bit values including boundaries around:

- legacy signed 16-bit maximum;
- 32768;
- 65535;
- direct-color-scale values;
- high positive values which remain valid `int` values.

## 24.6 Parameter conformance tests

Cover every adopted language construct including:

- integer/string parameters;
- variables;
- constants;
- arithmetic/bitwise/logical/comparison operators;
- nested conditionals;
- `%l`;
- character/string output;
- formatting flags;
- width;
- precision;
- case-sensitive hexadecimal formatting;
- persistent-context behavior.

## 24.7 Parameter analysis tests

Verify:

- parameter references are discovered correctly;
- numeric/string usage is classified correctly;
- variables are classified correctly;
- malformed structure is rejected before unsafe evaluation;
- stack/conditional invariants hold;
- analysis remains immutable/thread-safe if exposed.

## 24.8 Hardening tests

Exercise:

- excessive nesting;
- excessive program length;
- malformed formats;
- overflow attempts;
- huge width/precision requests;
- stack underflow;
- incompatible parameter types;
- invalid padding numbers;
- overflow in affected-line multiplication;
- excessively large delay expressions.

Every case SHALL fail deterministically without uncontrolled allocation.

## 24.9 Parsed-program cache tests

Verify:

- repeated standard capability expansion reuses the parsed form as intended;
- repeated extended capability expansion reuses the parsed form as intended;
- concurrent first expansion is safe;
- no partially constructed program escapes;
- cache lifetime is bounded by the description;
- no process-global arbitrary-string cache exists;
- public output remains identical with/without first-use cache initialization.

## 24.10 Byte-fidelity tests

Verify:

- one-to-one Latin-1 representation;
- `0x7F`;
- representative `0x80`–`0xFF` values;
- standard/extended storage;
- parameter transformation;
- padding transformation;
- exact `Encoding.Latin1` byte-stream output.

## 24.11 Padding fidelity tests

Cover:

- plain padding;
- decimal padding;
- multiplication by affected lines;
- mandatory padding;
- `xon` suppression rules;
- `pb` threshold behavior;
- `npc` behavior;
- explicit `pad` character behavior;
- timed delay provider injection;
- missing transport facts;
- synchronous/asynchronous paths where applicable.

## 24.12 Profile-composition tests

Verify:

- inherited values;
- explicit overrides;
- explicit cancellations;
- cancellation of standard capabilities;
- cancellation/removal of extended capabilities where supported;
- no inherited value leaks after cancellation.

## 24.13 Windows golden tests

Verify exact selected capabilities/cancellations for:

- `winconsole`;
- `ms-terminal`;
- `ms-terminal-direct`.

Also verify:

- exact name resolution;
- no unintended aliases;
- Windows Terminal does not resolve as xterm;
- `winconsole` does not resolve as ANSI;
- indexed/direct color semantics;
- representative keys and modern metadata;
- profile access itself does not change console mode;
- explicit Windows VT enablement remains reversible and separate.

## 24.14 0.9 readiness fixture tests

Tests SHALL validate fixture provenance/manifests without implementing a production parser.

At minimum verify that:

- every required fixture exists;
- provenance is recorded;
- expected semantic manifests are checked in;
- fixture generation instructions are documented;
- fixture files are stable repository test assets.

## 24.15 Architecture guard tests

Maintain/extend guards proving:

- no process-global current terminal;
- no process-global parsed-program cache keyed by arbitrary strings;
- no terminal-name branch in parameter/output engines;
- no terminal probing;
- no keyboard/mouse/paste/focus event decoder;
- no termcap parser;
- no source terminfo parser/compiler;
- no native ncurses runtime dependency;
- no Berkeley DB dependency;
- no production compiled terminfo parser;
- no explicit directory/system terminfo provider;
- no `TERMINFO`/`TERMINFO_DIRS` runtime discovery;
- no automatic Windows-host-to-profile mutation.

---

# 25. Documentation Requirements

The 0.8 README and supporting documentation SHALL explain:

- the 0.8 semantic-completion purpose;
- why arbitrary compiled/system loading is deliberately required but deferred to 0.9;
- the complete standard capability catalog;
- canonical capability metadata;
- binary-index versus managed-enum separation;
- standard capability catalog enumeration;
- per-description standard capability enumeration;
- standard versus extended capabilities;
- terminal canonical name, aliases, and verbose description;
- signed 32-bit numeric semantics;
- parameter-program parsing versus analysis versus evaluation;
- safe parameter requirements;
- parsed-program reuse and its bounded per-description scope;
- standard versus extended parameterized string expansion;
- raw 8-bit capability-string semantics;
- why exact capability-byte output uses Latin-1;
- terminal-aware padding and `xon`/`pb`/`npc`/`pad`;
- why baud rate remains caller-supplied;
- profile composition and cancellation semantics at a conceptual level;
- Windows Console versus Windows Terminal;
- `winconsole` versus `ms-terminal` versus `ms-terminal-direct`;
- why profile loading does not enable Windows VT mode;
- why `WT_SESSION`/`COLORTERM` do not silently change terminal identity;
- the exact 0.9 arbitrary-terminal reservation;
- the boundary with future `Icod.Terminal`, `Icod.Curses`, and `Icod.Pty` work.

Examples SHOULD include:

- enumerating standard capability metadata;
- enumerating effective capabilities on a terminal description;
- reading the verbose terminal description;
- parsing/analyzing/reusing a parameter program;
- expanding a typed standard string capability;
- expanding a parameterized extended string capability;
- exact high-byte output through `Encoding.Latin1`;
- terminal-aware padding with explicit transport facts;
- selecting `winconsole` and explicitly enabling Windows VT processing;
- selecting `ms-terminal`;
- selecting `ms-terminal-direct` and using semantic direct color;
- showing the future 0.9 boundary without implying a system parser exists in 0.8.

---

# 26. Packaging and Versioning

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

The first 0.8 implementation tranche SHALL advance both project version fields together:

```xml
<Version>0.8.0-alpha.1</Version>
<PackageVersion>0.8.0-alpha.1</PackageVersion>
```

Every subsequent tranche SHALL advance both together.

A suggested progression is:

```text
T21    0.8.0-alpha.1
T22    0.8.0-alpha.2
T23    0.8.0-alpha.3
T24    0.8.0-alpha.4
T25    0.8.0-alpha.5
T26    0.8.0-alpha.6
T27    0.8.0-alpha.7
T28    0.8.0-alpha.8
T29    0.8.0-beta.1
T30    0.8.0-rc.1
T31    0.8.0
```

The exact prerelease labels MAY be adjusted during implementation, but `<Version>` and `<PackageVersion>` SHALL remain synchronized at every tranche.

The package SHOULD remain dependency-free.

Adding a native ncurses, termcap, Berkeley DB, or other runtime dependency is outside the 0.8 contract.

Package validation SHALL continue to prove:

- no unexpected native/runtime payload;
- deterministic build;
- portable symbols/Source Link;
- fresh-package consumer;
- no runtime NuGet dependency added accidentally;
- new 0.8 APIs work from the packed package;
- Windows profiles are usable in non-interactive package-consumer tests;
- fixture data intended only for repository tests does not accidentally bloat the runtime package unless deliberately included.

---

# 27. Implementation Roadmap

Implementation SHALL proceed in dependency order.

The tranche sequence is designed so Windows profiles consume the completed model rather than forcing piecemeal additions, and so the final 0.9-readiness tranche freezes parser inputs without implementing the parser itself.

---

## T21 — 0.8 Foundation and Contract Reset

### Work

- add this 0.8.0 roadmap to the repository;
- preserve the 0.6.0 and 0.7.0 roadmaps/audits as historical records;
- formally supersede the earlier proposal that placed arbitrary/system database loading in 0.8;
- reserve arbitrary/system compiled terminfo support as REQUIRED for 0.9;
- advance both `<Version>` and `<PackageVersion>` to `0.8.0-alpha.1`;
- freeze the 0.7 exported public API/enum numeric baselines;
- record the selected upstream ncurses capability/profile provenance baseline;
- establish directories/conventions for 0.8 metadata generation/validation and future 0.9 fixtures;
- freeze the 0.8 architectural guard: no production compiled parser or system provider.

### Acceptance gate

T21 is complete when:

- the 0.8 scope is explicit and committed;
- the 0.9 arbitrary-terminal requirement is explicit and committed;
- all 0.7 behavior remains unchanged;
- all 0.7 enum numeric values are guarded;
- both version fields are synchronized;
- later tranches can extend the model without ambiguity about the database boundary.

---

## T22 — Complete Standard Terminfo Model

### Work

- complete the standard Boolean capability enum/catalog for the selected ncurses/System V-compatible baseline;
- complete the standard numeric capability enum/catalog;
- complete the standard string capability enum/catalog;
- append enum members without renumbering any 0.7 value;
- establish one canonical metadata record per standard capability;
- include kind, binary index, short name, long/variable name, termcap code, and managed enum identity;
- mechanically generate or validate derived tables;
- add read-only catalog metadata enumeration;
- add read-only per-description standard capability enumeration;
- add terminal verbose `Description`/long-name support to `TerminalDescription` and its builder;
- populate existing built-in descriptions where authoritative/stable;
- freeze signed 32-bit standard/extended numeric semantics;
- add numeric-width boundary tests.

### Acceptance gate

T22 is complete when:

- every supported standard table position has one canonical managed representation;
- compiled indices are independent of enum ordinals;
- all 0.7 enum numeric values are unchanged;
- metadata names/codes/indices are mechanically consistent;
- managed code can enumerate both the standard universe and one description's effective standard capabilities;
- primary name, aliases, and verbose description are represented separately;
- values above legacy 16-bit numeric range are preserved without narrowing.

---

## T23 — Parameter Program Completion, Analysis, and Hardening

### Work

- audit the existing parameter engine against the complete adopted terminfo parameter language;
- add/fix any missing generic operators or formatting semantics;
- verify integer and string parameter behavior;
- verify dynamic and persistent/static variable behavior;
- verify nested conditional behavior;
- verify printf-style flags/width/precision;
- add immutable parameter-program analysis sufficient to identify parameter/type/variable/structural requirements;
- validate incompatible parameter values deterministically;
- harden parser/evaluator source length, nesting, stack, width/precision, arithmetic, and output-growth behavior;
- build a broad checked-in deterministic conformance corpus not limited to xterm programs;
- add regression tests for malformed/adversarial programs;
- preserve existing `TermInfoFormatException` meaning for parameter syntax.

### Acceptance gate

T23 is complete when:

- all adopted parameter constructs are covered by deterministic tests;
- no terminal-specific code is required;
- parameter/type requirements can be analyzed safely;
- malformed programs cannot trigger uncontrolled stack/allocation behavior;
- valid existing 0.7 expansions remain compatible;
- arbitrary future capability programs can be treated as untrusted input by the existing engine.

---

## T24 — Expansion Reuse and Extended Capability Symmetry

### Work

- add bounded lazy parsed-program reuse associated with immutable `TerminalDescription` instances;
- make cache initialization thread-safe;
- reuse parsed programs for standard string capability expansion;
- add explicit parameterized extended-string expansion convenience APIs;
- add context-aware extended expansion equivalent to standard expansion;
- reuse parsed programs for extended string capability expansion;
- keep standard and extended namespaces explicit;
- ensure direct `TermInfoParameterProgram.Parse` remains available independently;
- add concurrency and cache-bound tests;
- verify no process-global arbitrary-string cache exists.

### Acceptance gate

T24 is complete when:

- repeated expansion of the same immutable capability does not require reparsing;
- cache lifetime is naturally bounded by one terminal description;
- concurrent first-use expansion is safe;
- standard/extended parameterized strings use the same analyzer/evaluator semantics;
- extended expansion rejects missing/non-string values deterministically;
- public expansion results remain unchanged except for new explicit extended convenience functionality.

---

## T25 — Byte and Output Fidelity

### Work

- freeze/document the `Encoding.Latin1` one-to-one capability-byte representation;
- add standard/extended high-byte storage tests;
- add high-byte parameter/padding transformation tests;
- add exact Latin-1 stream-output examples/tests;
- complete standard `pb` and `pad` catalog support if not already covered by T22 data completion;
- design/finalize the explicit terminal-aware padding policy/overload;
- implement `xon`, `pb`, `npc`, and `pad` semantics;
- preserve affected-line multiplication and mandatory `/` behavior;
- keep delay injection through `ITermInfoDelayProvider`;
- harden padding numeric parsing and delay calculations;
- preserve existing simple output APIs;
- keep baud/transport facts explicit and caller-owned.

### Acceptance gate

T25 is complete when:

- high-byte capability data round-trips exactly through the documented byte path;
- arbitrary valid parameter/padding strings do not assume UTF-8;
- terminal-aware padding models the selected historical semantics without `cur_term` or global `ospeed`;
- mandatory padding remains mandatory;
- existing 0.7 output behavior remains compatible;
- malformed/extreme padding input fails deterministically.

---

## T26 — Profile Composition and Cancellation Fidelity

### Work

- formalize reusable profile-fragment composition rules;
- add explicit internal cancellation semantics;
- ensure cancellation removes previously inherited standard capabilities;
- ensure extended removal/cancellation remains explicit where composition needs it;
- retain effective public absent/present lookup semantics;
- add composition/cancellation golden tests;
- establish reusable provenance conventions for generated/transcribed profile data;
- audit existing DEC/xterm fragments for compatibility with the formalized rules;
- ensure the refactor does not change any 0.7 effective profile accidentally.

### Acceptance gate

T26 is complete when:

- set/inherit/override/cancel behavior is deterministic;
- canceled inherited capabilities cannot leak;
- no public canceled-state API is required for effective descriptions;
- all existing built-ins remain behaviorally compatible;
- T27/T28 can express authoritative Windows source cancellations without special cases;
- the same internal semantics are suitable for future 0.9 compiled cancellation handling.

---

## T27 — Windows Console (`winconsole`)

### Work

- implement the authoritative `winconsole` built-in;
- use the complete T22 standard catalog rather than ad-hoc extended-name substitutes;
- compose reusable fragments only where authoritative;
- apply source overrides/cancellations exactly;
- retain genuine extended capabilities;
- golden-test the full effective profile against a recorded current ncurses baseline;
- add exact-name resolution;
- document relationship to explicit Windows VT processing;
- test that profile access itself has no Windows console-mode side effect;
- verify `WindowsVirtualTerminal` remains explicit and reversible;
- do not invent an unsupported direct-color `winconsole` identity.

### Acceptance gate

T27 is complete when:

- `winconsole` resolves exactly;
- the profile matches the selected authoritative data represented by the library;
- inherited canceled capabilities do not leak;
- profile access does not call Windows Console mode APIs;
- explicit VT enablement remains separate;
- no ANSI/xterm aliasing is introduced;
- no invented direct-color Console identity is added.

---

## T28 — Windows Terminal (`ms-terminal` / `ms-terminal-direct`)

### Work

- implement authoritative `ms-terminal`;
- implement authoritative `ms-terminal-direct`;
- reuse DEC/xterm/color/modern-metadata fragments only where the authoritative source genuinely inherits them;
- apply Windows Terminal-specific overrides/cancellations;
- golden-test complete effective profiles;
- verify indexed color semantics for `ms-terminal`;
- verify direct-RGB semantics for `ms-terminal-direct` through the generic color engine;
- verify representative key, mouse, focus, paste, reporting, and other retained metadata;
- document differences from xterm identities;
- keep `WT_SESSION`, `WT_PROFILE_ID`, and `COLORTERM` non-authoritative;
- add exact-name resolution and unsupported-nearby-name tests.

### Acceptance gate

T28 is complete when:

- both names resolve exactly;
- Windows Terminal is not an xterm alias;
- indexed/direct color semantics match the selected profiles;
- unsupported inherited xterm behavior does not survive accidentally;
- profile loading is side-effect-free;
- no automatic environment-based profile mutation is introduced.

---

## T29 — 0.9 Binary and Provider Readiness Gate

### Work

- freeze/document the future 0.9 `0432` legacy binary contract;
- freeze/document the future ncurses extended-section contract;
- freeze/document the future `01036` / 32-bit numeric contract;
- freeze absent/canceled semantic mapping expectations;
- freeze the selected conventional ncurses/System V vendor boundary;
- reserve distinct future malformed compiled-entry exception vocabulary;
- freeze `ITerminalDescriptionProvider.TryLoad == false` as a clean miss;
- create checked-in source/binary/manifest/provenance fixtures;
- include legacy, extended, 32-bit, high-byte, parameter, padding, absent, canceled, and malformed seed fixtures;
- document fixture regeneration with an authoritative `tic` baseline;
- add architecture guards proving that no production compiled parser, directory provider, `TERMINFO`, or `TERMINFO_DIRS` implementation entered 0.8.

### Acceptance gate

T29 is complete when:

- 0.9's input formats and semantic targets are explicit;
- every major future parser feature has deterministic checked-in fixtures;
- fixture expected semantics map cleanly into the completed 0.8 object/runtime model;
- provider clean-miss semantics require no 0.9 interface redesign;
- malformed compiled-entry exception naming cannot collide with parameter-program format errors;
- **no production arbitrary-terminal parser/provider exists yet**.

---

## T30 — API Hardening, Documentation, Samples, and Package Freeze

### Work

- review every new 0.8 public type/member;
- freeze exported public API baseline;
- review nullability and exception contracts;
- decide whether parameter-program analysis needs public exposure beyond required internal safety;
- review standard metadata/enumeration API shape;
- review per-description enumeration shape;
- review `Description` naming;
- review extended-string expansion naming;
- review terminal-aware padding options/overloads;
- review parsed-program cache thread safety and ensure it remains an implementation detail;
- update README and supporting docs;
- add samples for metadata/enumeration, description, extended expansion, byte output, terminal-aware padding, and Windows profiles;
- document the required 0.9 arbitrary-terminal boundary prominently;
- expand package smoke tests/fresh consumer to use the new 0.8 APIs;
- verify test-only compiled fixtures do not introduce unintended runtime/package dependencies.

### Acceptance gate

T30 is complete when:

- public APIs are intentional and minimal;
- internal parser/cache machinery has not leaked unnecessarily;
- no stream/encoding/context ownership ambiguity remains;
- docs distinguish capability bytes from application text;
- docs distinguish profile selection from live Windows VT enablement;
- docs cannot be read as claiming arbitrary compiled/system loading in 0.8;
- fresh packaged consumers can use every intended new public 0.8 feature;
- package validation remains clean.

---

## T31 — 0.8.0 Completion Gate

Before tagging `0.8.0`, perform a final audit.

### Required checks

- Windows/Linux/macOS Debug CI passes;
- Windows/Linux/macOS Release CI passes;
- package validation passes;
- fresh-package consumer passes;
- all 0.7 public enum numeric values remain unchanged;
- all 0.7 exported public members remain compatible except deliberate reviewed additions;
- all 0.7 built-ins remain behaviorally compatible;
- complete standard Boolean catalog is internally consistent;
- complete standard numeric catalog is internally consistent;
- complete standard string catalog is internally consistent;
- canonical short/long/termcap names match the selected baseline;
- future binary indices are unique and independent of enum ordinals;
- catalog enumeration passes;
- per-description enumeration passes;
- terminal verbose descriptions are represented correctly;
- signed 32-bit numeric boundary tests pass;
- complete adopted parameter-program conformance passes;
- parameter-program analysis/safe evaluation gates pass;
- parameter parser hardening gates pass;
- padding parser hardening gates pass;
- parsed-program cache/concurrency gates pass;
- extended-string expansion gates pass;
- high-byte/Latin-1 round-trip gates pass;
- terminal-aware `xon`/`pb`/`npc`/`pad` semantics pass;
- profile composition/cancellation gates pass;
- `winconsole` golden tests pass;
- `ms-terminal` golden tests pass;
- `ms-terminal-direct` golden tests pass;
- Windows profile access remains side-effect-free;
- explicit Windows VT enablement remains reversible and separate;
- Windows identities are not ANSI/xterm aliases;
- 0.9 binary contract is documented;
- complete 0.9 fixture corpus/provenance is checked in;
- provider `TryLoad` clean-miss semantics are documented;
- future compiled-format exception vocabulary does not collide with parameter format exceptions;
- no native ncurses dependency exists;
- no Berkeley DB dependency exists;
- no termcap parser exists;
- no terminfo source compiler exists;
- no process-global current terminal exists;
- no unbounded process-global parsed-program cache exists;
- no live terminal probing exists;
- no keyboard/mouse/paste/focus event decoder exists;
- no production compiled terminfo parser exists;
- no explicit directory/system terminfo provider exists;
- no `TERMINFO`/`TERMINFO_DIRS` runtime discovery exists;
- README scope matches this contract.

### Semantic completion test

The final audit SHALL answer **yes** to this question:

> Could a caller/provider construct, without changing `Icod.TermInfo` internals, a complete immutable `TerminalDescription` representing any terminal semantic shape expected from the supported future 0.9 binary formats, and then query, enumerate, analyze, expand, and output its capabilities correctly?

If the answer is no because the object model or transformation runtime still needs redesign, 0.8 is not complete.

### Completion

When every T31 item passes:

- set/confirm both project version fields as `0.8.0`;
- tag the exact validated commit `v0.8.0`;
- publish the exact validated package and symbol package;
- confirm restore/use from a fresh public-package consumer;
- mark the 0.8.0 contract complete.

No source/package content should change between final successful validation and tagging. Any change requires rerunning the completion gate.

---

# 28. Required 0.9.0 Scope

Version 0.9.0 SHALL implement arbitrary terminal support against the semantic/runtime model frozen by 0.8.

At minimum, 0.9 SHALL include:

## 28.1 Compiled parsing

- conventional `0432` compiled entry parsing;
- names/aliases/verbose description parsing;
- Boolean/numeric/string standard table parsing by fixed metadata index;
- alignment handling;
- absent/canceled recognition;
- one-to-one 8-bit capability-string decoding using the 0.8 contract;
- ncurses extended Boolean/numeric/string sections;
- extended names;
- `01036` / signed 32-bit numeric format;
- malformed/truncated/overflow-safe parsing;
- safe size/allocation limits;
- distinct malformed compiled-entry diagnostics/exceptions.

## 28.2 Explicit directory provider

- exact terminal-name lookup beneath configured roots;
- conventional first-character directory organization;
- hexadecimal first-character directory organization;
- safe terminal-name/path validation;
- parsed primary/alias verification;
- no recursive scan for exact lookup.

## 28.3 System discovery

- `TERMINFO` directory roots;
- `TERMINFO=hex:...`;
- `TERMINFO=b64:...`;
- `$HOME/.terminfo` on applicable platforms;
- `TERMINFO_DIRS`;
- empty `TERMINFO_DIRS` component semantics;
- platform-aware default terminfo roots;
- deterministic deduplication and precedence;
- Windows path-list handling without breaking drive-letter paths;
- explicit options to disable environment/user/default search.

## 28.4 Loaded-description cache and refresh

- provider-instance positive caching;
- thread-safe concurrent first load;
- coherent alias caching;
- no indefinite negative caching by default;
- no hidden process-global system database cache;
- deterministic refresh through new provider construction;
- no filesystem watcher requirement.

## 28.5 System composition

- explicit composition/factory API for system lookup;
- optional explicit built-in fallback;
- built-in fallback can be disabled;
- existing first-provider-to-resolve semantics remain authoritative;
- `TerminalDatabase.BuiltIn` remains deterministic and I/O-free.

0.9 SHALL require no terminal-specific C# profile code for ordinary arbitrary installed entries that fit the supported compiled dialect.

---

# 29. Explicitly Deferred Beyond 0.9 or Outside `Icod.TermInfo`

The following are not required merely to complete 0.8 or the required 0.9 arbitrary-terminal milestone.

## 29.1 Database/storage/tooling

- Berkeley DB hashed terminfo backend;
- HP-UX/AIX/OSF-specific divergent compiled table dialects unless separately contracted;
- termcap source/database parsing;
- `TERMCAP` / `TERMPATH`;
- terminfo source parser;
- `use=` source compiler/inheritance resolver;
- compiled-entry writer;
- `tic` implementation;
- `infocmp` implementation;
- full database enumeration analogous to `toe`.

## 29.2 Live terminal/session behavior

- process-global current terminal;
- raw/cooked mode ownership;
- terminal-mode restoration beyond narrowly scoped existing Windows VT lease behavior;
- automatic baud-rate discovery;
- automatic terminal probing;
- automatic host negotiation;
- keyboard event decoding;
- mouse event decoding;
- bracketed-paste decoding;
- focus event decoding;
- OSC 52 clipboard execution;
- OSC 8 hyperlink execution;
- Sixel/Kitty image transmission;
- Kitty keyboard negotiation;
- full-screen session lifecycle;
- progress/spinner policy.

## 29.3 Curses/UI behavior

- virtual screen;
- refresh/diff optimizer;
- curses windows;
- pads;
- panels;
- menus;
- forms;
- widgets;
- high-level styling/attribute policy.

## 29.4 PTY/process behavior

- pseudo-terminal creation;
- ConPTY lifecycle;
- child-process creation/plumbing.

---

# 30. Interoperability Baseline and References

The implementation SHALL record exact upstream revision/date provenance for:

- canonical capability metadata;
- generated/validated capability tables;
- Windows built-in golden profiles;
- compiled 0.9-readiness fixtures.

Primary reference baseline for this contract:

- ncurses `term(5)` — compiled terminfo format  
  https://invisible-island.net/ncurses/man/term.5.html

- ncurses `terminfo(5)` — capability vocabulary and database semantics  
  https://invisible-island.net/ncurses/man/terminfo.5.html

- ncurses `curs_terminfo(3x)` — low-level runtime interface, parameter/output semantics, capability strings  
  https://invisible-island.net/ncurses/man/curs_terminfo.3x.html

- ncurses `term_variables(3x)` — standard capability name/code tables  
  https://invisible-island.net/ncurses/man/term_variables.3x.html

- ncurses `user_caps(5)` — extended capabilities  
  https://invisible-island.net/ncurses/man/user_caps.5.html

- current ncurses `terminfo.src`  
  https://invisible-island.net/ncurses/terminfo.src.html

- Microsoft — Classic Console APIs versus Virtual Terminal Sequences  
  https://learn.microsoft.com/en-us/windows/console/classic-vs-vt

- Microsoft — Console Virtual Terminal Sequences  
  https://learn.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences

At contract creation time (2026-08-22), the previously audited authoritative ncurses `terminfo.src` baseline reports:

```text
Revision: 1.1267
Date:     2026/08/14 08:03:59
```

That baseline contains the current selected `winconsole`, `ms-terminal`, and `ms-terminal-direct` definitions used to plan this contract.

Each implementation tranche which imports or verifies authoritative upstream data SHALL re-check the current source deliberately and record any baseline change rather than silently adopting upstream drift.

A later upstream revision does not automatically change a frozen released profile without an explicit project decision.

---

# 31. Summary of the 0.8.0 Boundary

Version 0.8.0 can be summarized as:

> **0.8.0 finishes teaching `Icod.TermInfo` what terminfo means. It completes the standard capability universe and metadata, makes standard descriptions inspectable, completes terminal identity and signed-32-bit numeric semantics, finishes and hardens parameter/padding execution for arbitrary future capability programs, freezes exact 8-bit capability-string behavior, formalizes cancellation/composition, and adds authoritative Windows Console and Windows Terminal profiles.**

It intentionally does **not** yet teach the library where arbitrary system terminfo entries live or how to parse their compiled bytes.

Version 0.9.0 is therefore reserved and required to:

> **read arbitrary conventional compiled terminfo entries safely and discover them through explicit/system database providers without redesigning the 0.8 object model or transformation runtime.**

The intended handoff is:

```text
0.8 completed model

TerminalDescription
    Name
    Aliases
    Description
    complete standard capabilities
    enumerable standard metadata/values
    extended capabilities
    signed 32-bit numerics
    exact 8-bit capability strings
        ↓
parameter parse/analyze/cache/evaluate
        ↓
terminal-aware padding/output

0.9 adds

terminal name / compiled bytes
        ↓
directory/system provider
        ↓
compiled parser
        ↓
that same 0.8 TerminalDescription model
```

The core architectural principle remains:

> **terminal facts and pure capability transformations belong in `Icod.TermInfo`; live terminal policy and state belong above it.**

And the release boundary remains:

> **0.8 understands any representable terminfo description; 0.9 acquires arbitrary descriptions from real compiled databases.**
