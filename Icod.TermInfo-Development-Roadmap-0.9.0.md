# Icod.TermInfo Development Roadmap — Version 0.9.0 Contract

**Project:** `Icod.TermInfo`
**Package:** `Icod.TermInfo`
**Target framework:** `net10.0`
**Language:** C# 13
**Status:** Complete and frozen — T41 0.9.0 completion gate
**Previous contract:** `0.8.0` — semantic completion and binary/provider readiness
**Contract target:** `0.9.0`
**Initial development version:** `0.9.0-alpha.1`

---

# 1. Purpose

Version 0.9.0 is the **arbitrary compiled-terminfo acquisition release** for
`Icod.TermInfo`.

Versions 0.6.0 through 0.8.0 deliberately completed the meaning and execution
of terminal descriptions before admitting arbitrary external terminal data:

```text
0.6.0
    managed immutable capability model
    ANSI / VT100 / dumb
    provider architecture
    parameter expansion
    padding/output

0.7.0
    DEC + modern xterm families
    extended capabilities
    indexed/direct color semantics
    full-screen primitives
    modern descriptive metadata

0.8.0
    complete standard capability universe
    canonical metadata and enumeration
    signed 32-bit numeric semantics
    hardened reusable parameter programs
    exact 8-bit capability-string semantics
    terminal-aware padding
    composition/cancellation fidelity
    Windows Console / Windows Terminal profiles
    frozen compiled-format/provider target
    deterministic future-parser corpus

0.9.0
    compiled-entry parsing
    directory-tree acquisition
    environment/user/system discovery
    explicit diagnostics and safety limits
    provider-local caching and refresh semantics
```

The defining 0.9 rule is:

> **0.9 teaches `Icod.TermInfo` where arbitrary compiled descriptions come
> from and how to read them. It SHALL NOT redesign what a terminal description
> means once it is in memory.**

The public semantic target remains the immutable `TerminalDescription` model
frozen by 0.8.

---

# 2. Relationship to the 0.8.0 Contract

Version 0.8.0 is a frozen historical contract and the semantic prerequisite
for this release.

Version 0.9.0 SHALL preserve, unless a narrowly documented acquisition API
addition requires otherwise:

- all 0.8 public enum numeric values;
- all 0.8 exported public members;
- all 0.8 built-in terminal identities and aliases;
- immutable `TerminalDescription` instances;
- deterministic standard-capability metadata and enumeration;
- exact case-sensitive extended-capability names;
- signed 32-bit standard and extended numeric semantics;
- the reversible Latin-1 capability-byte bridge;
- parameter-program parsing/evaluation semantics;
- per-description bounded parsed-program reuse;
- terminal-aware padding semantics;
- `ITerminalDescriptionProvider.TryLoad(...) == false` as a **clean miss**;
- deterministic first-provider-wins `TerminalDatabase` composition;
- `TerminalDatabase.BuiltIn` as dependency-free, deterministic, and I/O-free;
- explicit Windows virtual-terminal mode enablement;
- no process-global current terminal;
- no live terminal-session ownership.

The T29 fixture corpus and
`docs/0.8.0-T29-0.9-READINESS.md` are authoritative inputs to 0.9
implementation. If parser implementation exposes a semantic-model deficiency,
the deficiency must be treated as an explicit contract issue rather than
silently changing 0.8 behavior.

---

# 3. Governing Architecture

The 0.8 distinction between **semantics** and **acquisition** remains
fundamental.

```text
                         arbitrary compiled bytes
                                  |
                                  v
                    pure compiled-entry parser
                                  |
                                  v
                     TerminalDescription
                                  |
                    +-------------+-------------+
                    |             |             |
                    v             v             v
                  query        expand         output
```

Acquisition layers feed the same parser:

```text
explicit byte input -------------------+
                                       |
explicit directory root ---------------+--> compiled parser
                                       |
TERMINFO=hex:/b64: --------------------+
                                       |
TERMINFO / TERMINFO_DIRS / user roots -+
                                       |
platform default directory roots ------+
```

The parser and discovery/provider layers SHALL remain separate components.
Filesystem/provider code SHALL NOT contain a second binary parser, and the
binary parser SHALL NOT read environment variables or files.

This separation is required so that:

- parser tests can operate entirely from checked-in byte fixtures;
- parser fuzzing needs no filesystem;
- encoded `TERMINFO` entries reuse exactly the same parser;
- explicit directory and system providers share one semantic implementation;
- a future optional hashed-database provider can extract a compiled record and
  reuse the same parser;
- I/O errors remain distinguishable from malformed compiled-data errors.

---

# 4. 0.9.0 Scope

## 4.1 Required

Version 0.9.0 SHALL implement:

1. pure parsing of the frozen conventional compiled terminfo formats;
2. legacy octal `0432` entries with signed 16-bit compiled numerics;
3. the selected ncurses extended Boolean/numeric/string section;
4. extended names and ordering rules;
5. octal `01036` entries with signed 32-bit numerics;
6. names, aliases, and verbose terminal descriptions;
7. absent and canceled value recognition;
8. exact one-to-one 8-bit capability-string decoding;
9. standard table mapping by canonical binary index;
10. strict extended-name collision handling;
11. a distinct malformed compiled-entry exception contract;
12. overflow-safe and allocation-bounded parsing;
13. an explicit directory-tree terminal-description provider;
14. conventional first-character directory lookup;
15. hexadecimal first-character directory lookup;
16. safe exact terminal-name/path handling;
17. parsed primary/alias verification against the requested name;
18. `TERMINFO` directory roots;
19. `TERMINFO=hex:...`;
20. `TERMINFO=b64:...`;
21. `$HOME/.terminfo` on applicable platforms;
22. `TERMINFO_DIRS` list parsing and empty-component semantics;
23. platform-aware default directory roots;
24. deterministic path deduplication and precedence;
25. Windows-safe path-list parsing without drive-letter corruption;
26. explicit options to enable/disable environment, user, and default search;
27. provider-local successful-description caching;
28. explicit refresh/new-provider semantics;
29. optional composition with built-in fallback without changing
    `TerminalDatabase.BuiltIn`;
30. complete cross-platform tests, documentation, samples, package validation,
    and completion-gate evidence.

## 4.2 Explicitly not required

Version 0.9.0 SHALL NOT implement:

- terminfo source (`.ti`) parsing;
- source-level `use=` inheritance resolution;
- a `tic`-class source compiler;
- an `infocmp`-class decompiler/comparator;
- `toe`, `captoinfo`, `infotocap`, or other terminfo maintenance tools;
- termcap database parsing;
- `TERMCAP` or `TERMPATH` discovery;
- Berkeley DB / ncurses hashed-database storage;
- arbitrary HP-UX, AIX, OSF/1, or other divergent historical binary dialects;
- automatic network retrieval of terminal descriptions;
- aggressive addition of Linux console, tmux, screen, rxvt, VTE, Kitty,
  foot, WezTerm, Alacritty, mintty, or other host identities as built-in C#
  profiles merely to avoid database loading;
- live raw/cooked terminal-mode ownership;
- keyboard, mouse, focus, or bracketed-paste event decoding;
- active terminal probing or request/response negotiation;
- OSC 8/OSC 52 operational APIs;
- Kitty keyboard or graphics protocol operation;
- Sixel or other image transmission;
- PTY/ConPTY creation or child-process plumbing;
- curses windows, pads, panels, refresh optimization, menus, forms, or widgets;
- terminal emulation.

Those areas are inventoried separately in `docs/FUTURE-WORK-INVENTORY.md`.

---

# 5. Supported Compiled Binary Family

The binary contract frozen by T29 remains authoritative.

## 5.1 Legacy format

The parser SHALL support the conventional little-endian legacy format with
magic `0432` (octal), including:

1. six signed/unsigned 16-bit header fields as appropriate to the frozen
   format;
2. names section;
3. Boolean table;
4. required alignment before numeric values;
5. signed 16-bit numeric table;
6. signed 16-bit string-offset table;
7. string table.

The parser SHALL map standard positions through
`StandardCapabilityCatalog` binary indices. Managed enum ordinals SHALL never
be used as compiled table positions.

## 5.2 Extended section

The parser SHALL support the selected ncurses extended section appended to a
conventional entry, including:

- extended Boolean count;
- extended numeric count;
- extended string count;
- extended string-table item count;
- extended string-table byte count;
- extended Boolean values;
- extended numeric values;
- extended string offsets;
- extended string values;
- extended capability names;
- Boolean/numeric/string extended-name ordering.

Extended capability names remain exact and case-sensitive.

An extended name that collides with a known standard terminfo short name SHALL
be handled according to the T29 collision contract and SHALL NOT silently
shadow the standard capability namespace.

## 5.3 Extended-number format

The parser SHALL support the conventional little-endian format with magic
`01036` (octal), using signed 32-bit numeric values in both the standard and
applicable extended numeric tables while retaining the frozen 16-bit offset
representation for strings.

Values above the legacy 16-bit range, including direct-color-scale values,
SHALL round-trip into the existing signed 32-bit managed numeric model without
special cases.

## 5.4 Vendor boundary

0.9 SHALL target the same conventional System V/Solaris-like ordering and
ncurses extensions frozen by 0.8.

The parser SHALL NOT guess among incompatible historical vendor layouts when a
binary shape falls outside that contract. Unsupported layouts must fail
deterministically rather than produce a plausible but incorrect terminal
description.

---

# 6. Pure Parser Contract

## 6.1 Independent byte entry point

T32 SHALL freeze a public or otherwise independently reusable parser entry
point that accepts caller-supplied compiled bytes without requiring a
filesystem provider.

The exact public type/member names are a T32 API decision, but the semantic
shape must be equivalent to:

```text
compiled bytes
    -> parse
    -> immutable TerminalDescription
```

A span/memory/stream-friendly design is acceptable, but the core parser SHALL
be callable with already-available bytes and SHALL NOT require a path.

## 6.2 Parser purity

The core parser SHALL NOT:

- read files;
- inspect `TERM`;
- inspect `TERMINFO` or `TERMINFO_DIRS`;
- inspect `$HOME`;
- discover system paths;
- mutate a provider cache;
- modify Windows Console state;
- probe a live terminal;
- use native ncurses/libtinfo.

Its result depends only on the supplied bytes and explicit parser options.

## 6.3 Names and identity

The names field SHALL populate:

- canonical `TerminalDescription.Name`;
- aliases;
- verbose `TerminalDescription.Description`.

The verbose description is not an alias.

Malformed names records, missing required terminators, invalid empty primary
names, duplicate/invalid aliases, and other impossible identity states SHALL
fail deterministically.

## 6.4 Boolean values

The parser SHALL distinguish conventional Boolean states according to the
frozen format contract.

Only a real present Boolean SHALL materialize as an effective present
capability. Absent and canceled sentinels SHALL NOT become `true` values.

## 6.5 Numeric values

The parser SHALL recognize:

- absent numeric sentinel;
- canceled numeric sentinel;
- valid signed numeric values;
- 16-bit numeric width under `0432`;
- 32-bit numeric width under `01036`.

Absent/canceled values SHALL not be materialized as numeric capabilities in the
effective public description.

## 6.6 String values

The parser SHALL recognize:

- absent string offsets;
- canceled string offsets;
- valid string-table offsets;
- terminating NUL bytes;
- high-byte values `0x80` through `0xFF` as ordinary capability data.

Capability bytes SHALL use the reversible 0.8 Latin-1 bridge. The parser SHALL
not attempt UTF-8 decoding of compiled capability strings.

## 6.7 Cancellation semantics

Compiled absent/canceled sentinels SHALL never leak as actual capability
values.

If parser implementation passes through an internal composition/builder stage,
canceled state must remain strong enough to prevent a previously inherited
value from reappearing before the final immutable description is built.

The public effective `TerminalDescription` may continue to represent canceled
values as absent, consistent with 0.8.

---

# 7. Parser Safety and Resource Policy

Compiled terminfo is external, potentially untrusted binary input. 0.9 SHALL
therefore treat parser hardening as part of the feature rather than as a later
cleanup.

## 7.1 Bounds before allocation

Every count, offset, length, multiplication, and addition derived from input
bytes SHALL be validated before allocation or slicing.

Checked arithmetic or equivalent explicit range logic SHALL prevent integer
overflow.

The parser SHALL reject impossible relationships such as:

- section counts extending past the supplied entry;
- offsets outside their owning string table;
- unterminated required names/string data;
- impossible alignment;
- extended table counts which cannot fit the remaining bytes;
- extended-name counts inconsistent with the value tables.

## 7.2 Entry-size limit

T32 SHALL freeze a finite parser entry-size limit large enough to accept every
supported conventional ncurses entry represented by the contract and fixtures.

The limit SHALL be checked before large allocations. It SHALL NOT be inferred
from an attacker-controlled count field.

The roadmap deliberately leaves the exact managed limit to T32 so it can be
selected against the frozen fixture corpus, conventional ncurses limits, and
reasonable future compatibility rather than guessed during roadmap writing.

## 7.3 Name and string limits

Names, aliases, extended names, and string-table extraction SHALL remain
bounded by the validated entry size and section sizes.

The parser SHALL not allocate one object per impossible count before proving
that the corresponding bytes exist.

## 7.4 Failure atomicity

Malformed input SHALL either produce one complete immutable
`TerminalDescription` or fail. It SHALL NOT expose a partially populated
terminal description.

---

# 8. Diagnostics and Exception Vocabulary

`TermInfoFormatException` remains reserved for malformed parameter-program
syntax.

0.9 SHALL add distinct malformed compiled-entry vocabulary. The exact T32
public type name may be refined, but a name such as
`CompiledTermInfoFormatException` expresses the frozen semantic distinction.

The compiled-format exception SHOULD expose useful deterministic context such
as a byte offset/section where practical, without exposing mutable parser
internals.

Provider behavior SHALL preserve the clean-miss rule:

```text
requested entry truly absent
    -> TryLoad == false

permission denied
I/O failure
malformed compiled bytes
unsupported compiled format
invalid encoded TERMINFO
internal parser failure
    -> error, not clean miss
```

Standard .NET I/O/security exceptions may remain appropriate for filesystem
failures. 0.9 SHALL not create a broad catch-all exception merely to hide
useful failure categories.

---

# 9. Explicit Directory Provider

## 9.1 Caller-owned root

0.9 SHALL provide an explicit provider over one caller-supplied directory root.

Constructing the provider SHALL NOT consult environment variables or system
defaults unless that behavior is explicitly part of a separate system-provider
factory/options object.

## 9.2 Lookup layout

For an exact requested terminal name, the directory provider SHALL support the
conventional two-level layouts:

```text
<root>/<first-character>/<terminal-name>
```

and the hexadecimal first-character form used by filesystems/configurations
where the literal first-character directory is unsuitable.

The exact hex casing/candidate order SHALL be frozen in T36 and covered by
fixtures.

The provider SHALL NOT recursively scan the database to satisfy an exact name.

## 9.3 Safe terminal names

A requested terminal name SHALL be validated before it participates in path
construction.

At minimum, the provider SHALL reject path traversal and path syntax such as:

- embedded directory separators;
- rooted/absolute paths;
- `.` / `..` traversal forms;
- NUL;
- platform-specific path injection forms.

Validation SHALL preserve legitimate conventional terminfo names rather than
normalizing every name to an assumed terminal family.

## 9.4 Identity verification

After a candidate file is parsed, the resulting primary name/aliases SHALL be
checked against the exact requested terminal name.

A mismatched entry at a plausible path SHALL not silently satisfy the request.

## 9.5 Filesystem aliases

The provider SHALL work whether database aliases are represented by hard links,
symlinks, copies, or distinct equivalent entries. Alias correctness is based on
the parsed entry identity rather than link type.

---

# 10. Encoded `TERMINFO`

0.9 SHALL support the ncurses-style encoded forms:

```text
TERMINFO=hex:<compiled-entry-bytes>
TERMINFO=b64:<compiled-entry-bytes>
```

The encoded payload SHALL be decoded under explicit size bounds and then passed
to the same pure compiled parser used by files.

The parsed terminal entry SHALL be accepted only when its canonical name or
alias matches the name being requested.

A syntactically invalid encoded payload, unsupported compiled format, or
malformed compiled entry is an error, not a clean provider miss.

No second "environment parser" may duplicate compiled-entry logic.

---

# 11. System Discovery and Search Precedence

## 11.1 Explicit construction

System discovery SHALL be opt-in through an explicit provider/factory/options
API.

`TerminalDatabase.BuiltIn` SHALL remain I/O-free and environment-independent.
Loading a built-in profile SHALL never begin a host database search.

## 11.2 Search sources

The default system-discovery policy SHALL model the supported ncurses-style
precedence:

1. `TERMINFO` encoded entry or explicit database location;
2. applicable user-local terminfo location such as `$HOME/.terminfo`;
3. `TERMINFO_DIRS` entries in order;
4. platform-configured/default terminfo directory roots.

The exact platform default-root set SHALL be frozen and tested in T38.

## 11.3 `TERMINFO_DIRS`

`TERMINFO_DIRS` SHALL be parsed with a platform-aware list separator.

On Unix-like systems this is conventionally `:`. On Windows the design SHALL
avoid interpreting a drive-letter colon as a list separator; the normal
platform path-list separator is preferred.

An empty list component SHALL expand to the configured default system root set
rather than the current working directory.

Duplicate physical/search roots SHALL be removed deterministically without
reordering the first occurrence.

## 11.4 User-home search

User-home discovery SHALL be optional and suppressible through explicit
options.

The implementation SHALL not assume a usable home directory exists. Missing
user-home state is a normal omission; malformed/unauthorized explicit paths are
not silently converted into unrelated fallbacks.

## 11.5 Default roots

Default roots SHALL be platform-aware and deterministic.

Unix/macOS implementations may include conventional system locations selected
and frozen by T38. Windows SHALL not invent a Unix-like database root merely to
claim host discovery; explicit roots and environment configuration remain
available on Windows.

## 11.6 Security controls

The public discovery options SHALL allow callers to disable independently:

- environment-variable discovery;
- user-home discovery;
- platform default roots.

This enables privileged/sandboxed applications to construct a provider with a
restricted trust policy without changing global process state.

---

# 12. Provider Composition

The existing ordered `TerminalDatabase` remains the composition mechanism.

0.9 SHALL not turn the database into a mutable process-global provider
registry.

Applications must be able to choose explicitly among patterns such as:

```text
system only
built-ins only
system, then built-in fallback
application provider, then system, then built-ins
```

The built-in database SHALL NOT automatically consult the system provider.

Unsupported host identities which are absent from configured system roots SHALL
remain unsupported unless the caller explicitly supplied another provider or
fallback.

---

# 13. Caching and Refresh Semantics

0.9 requires I/O caching, but the cache SHALL remain provider-local.

## 13.1 Successful-entry cache

A directory/system provider SHOULD cache successfully parsed immutable
`TerminalDescription` instances by the exact resolved terminal identity/name
needed by that provider.

The cache SHALL be thread-safe and naturally bounded by names requested through
that provider instance.

## 13.2 No process-global cache

There SHALL be no process-global cache of arbitrary system terminal entries.

Separate provider instances SHALL be able to represent separate root/search
configurations without sharing hidden state.

## 13.3 Clean misses

0.9 SHOULD NOT permanently negative-cache a clean miss by default. A terminal
entry installed after a miss should therefore be observable by a later lookup
unless an explicitly documented cache mode says otherwise.

## 13.4 Refresh model

The default refresh model SHALL be simple and deterministic:

- a successfully cached immutable entry remains stable for the lifetime of that
  provider instance;
- constructing a new provider instance creates a fresh acquisition/cache view;
- no background filesystem watcher is required;
- no TTL timer is required;
- no asynchronous refresh thread is required.

If T39 adds an explicit cache-clear/reload API, it must preserve thread safety
and deterministic provider semantics; such an API is not mandatory merely for
feature completeness.

---

# 14. Hashed Database Boundary

Contemporary ncurses can be built with a Berkeley-DB-style hashed terminfo
store in addition to conventional directory trees.

0.9 SHALL **not** take a Berkeley DB runtime dependency and SHALL not make hashed
storage a completion requirement.

The parser/provider separation is intentionally designed so a future optional
hashed-database provider can:

```text
hashed record bytes
    -> existing 0.9 compiled parser
    -> TerminalDescription
```

without changing terminal semantics.

If hashed database support is added later, preference should be given to an
optional provider/package that does not impose Berkeley DB on all
`Icod.TermInfo` consumers.

---

# 15. Terminfo Source and Tooling Boundary

Compiled database acquisition and terminfo source-language tooling are separate
features.

0.9 SHALL not parse `.ti` source or implement `tic`/`infocmp` merely because the
fixture corpus contains source descriptions.

A future source/tooling family may include:

```text
terminfo source lexer/parser
        |
        v
source AST / entry model
        |
   +----+----------------+
   |                     |
   v                     v
use= inheritance      validation
resolution
   |
   v
TerminalDescription
   |
   +----------+----------+
              |
       +------+------+
       v             v
    tic-like      infocmp-like
    compiler       serializer
```

That work may live in the core package if sufficiently small and pure, or in a
separate `Icod.TermInfo.Tools`/source package if it would otherwise enlarge the
runtime package substantially. It is not a prerequisite for 0.9.

---

# 16. Built-In Profile Policy After 0.9

Arbitrary database loading changes the economics of built-in profiles.

The existing deterministic built-ins remain valuable because they provide:

- a dependency-free fallback baseline;
- reproducible tests;
- known Windows Console/Windows Terminal identities;
- operation on hosts without a terminfo database.

However, 0.9 SHOULD NOT respond to every newly encountered `TERM` value by
adding another C# built-in.

When the host already has a valid compiled description for identities such as
`linux`, `screen`, `tmux`, `rxvt`, VTE, Kitty, foot, WezTerm, or Alacritty, the
system provider should load that data instead.

New built-ins after 0.9 should therefore require a stronger justification than
"this terminal name exists."

---

# 17. Testing Strategy

## 17.1 T29 fixture-first parser development

Normal parser tests SHALL use the checked-in T29 fixture corpus and SHALL not
require `tic`, ncurses, or network access.

Each valid fixture SHALL verify:

- primary name;
- aliases;
- verbose description;
- standard values;
- extended values;
- absent/canceled semantics;
- numeric widths;
- high-byte strings;
- parameterized strings;
- padding strings.

Malformed seeds SHALL verify deterministic failure categories and offsets where
part of the exception contract.

## 17.2 Additional generated/adversarial tests

0.9 SHALL add targeted tests for:

- every truncated boundary between parser sections;
- extreme count values;
- overflow relationships;
- odd/even alignment;
- offsets at `-1`, `-2`, zero, last valid byte, and first invalid byte;
- malformed names terminators;
- malformed extended header/count relationships;
- duplicate/colliding extended names;
- invalid encoded `TERMINFO` payloads;
- maximum accepted entry size;
- one byte beyond the maximum;
- random hostile bytes that must fail without unbounded allocation.

## 17.3 Differential/maintainer validation

Maintainer-only tests or scripts MAY compare selected parser results with an
authoritative ncurses `infocmp`/`tic` installation.

Such tooling SHALL not become a normal build/runtime dependency and SHALL not
make CI correctness depend on whatever terminfo database happens to be installed
on a runner.

## 17.4 Directory-provider tests

Use temporary fixture roots to verify:

- literal first-character layout;
- hexadecimal first-character layout;
- aliases;
- missing entries;
- identity mismatch;
- malformed files;
- permission/I/O failures where portable;
- traversal rejection;
- duplicate search-root behavior.

## 17.5 Discovery tests

Environment discovery SHALL be tested with an injected/snapshotted environment
abstraction or equivalent deterministic construction rather than mutating
unrelated process state across parallel tests.

Test:

- `TERMINFO` root precedence;
- `hex:` / `b64:` precedence;
- user-home precedence;
- ordered `TERMINFO_DIRS`;
- empty `TERMINFO_DIRS` components;
- platform path-list separators;
- disabled environment/user/default options;
- built-in fallback only when explicitly composed.

## 17.6 Cache/concurrency tests

Verify:

- repeated successful lookup reuses one immutable parsed entry per provider;
- concurrent first load is safe;
- separate providers do not share cached system entries;
- clean misses are not permanently cached by default;
- new-provider construction observes changed fixture roots;
- parser exceptions are not converted into clean misses.

---

# 18. Public API Principles

0.9 adds acquisition APIs, but T32 SHALL keep the public surface narrow.

The public design SHOULD need concepts equivalent to:

- compiled bytes -> `TerminalDescription`;
- explicit directory provider;
- system discovery options/provider;
- distinct compiled-format exception.

The design SHOULD NOT require public exposure of:

- parser instruction/state-machine internals;
- raw header structs merely because the file contains headers;
- cache dictionaries;
- filesystem candidate objects;
- global provider registries;
- background watcher objects.

T32 SHALL audit names, nullability, exception contracts, option ownership, and
thread-safety before implementation spreads those choices across later
tranches.

---

# 19. Documentation and Samples

0.9 documentation SHALL show the distinction among:

```text
TerminalDatabase.BuiltIn
    deterministic built-ins only

explicit directory provider
    one caller-selected root

system provider
    configured environment/user/default search

TerminalDatabase composition
    caller-selected provider precedence/fallback
```

Samples SHALL include:

- parsing one checked-in/embedded compiled entry from bytes;
- loading from an explicit temporary/example directory root;
- constructing a system provider with default discovery;
- constructing a restricted provider with environment and/or user search
  disabled;
- explicitly composing system lookup with built-in fallback;
- demonstrating a host terminal identity not compiled into C#.

Samples must not require changing the user's terminal mode or probing the live
terminal.

---

# 20. Dependency and Packaging Rules

0.9 SHALL preserve the core package's managed/dependency-light character.

The runtime package SHALL NOT acquire a dependency on:

- ncurses/libtinfo;
- curses;
- Berkeley DB;
- a termcap library;
- a filesystem-watching framework;
- a terminal-emulation library.

Normal package verification SHALL ensure parser fixtures, malformed corpus
files, source `.ti` files, and maintainer tools do not leak into runtime package
content unless explicitly intended as documentation/package assets.

---

# 21. Tranche Plan

Version numbers SHALL keep `<Version>` and `<PackageVersion>` synchronized at
every tranche.

## T32 — 0.9 Foundation and Acquisition API Freeze

**Development version:** `0.9.0-alpha.1`
**Status:** Complete
**Foundation record:** `docs/0.9.0-T32-FOUNDATION.md`

### Work

- create/activate the `0.9.0` roadmap and development branch;
- set both version fields to `0.9.0-alpha.1`;
- freeze the pure-parser API shape;
- freeze the distinct compiled-format exception shape;
- freeze parser resource-limit policy;
- freeze directory/system provider public shape and option ownership;
- freeze environment snapshot/injection strategy for deterministic tests;
- freeze cache/refresh semantics;
- add 0.8 public compatibility guards for the new development line;
- ensure `TerminalDatabase.BuiltIn` remains I/O-free;
- document all 0.9 non-goals.

### Acceptance gate

T32 is complete when the remaining tranches can implement parser/providers
without another architectural/public ownership redesign.

---

## T33 — Legacy `0432` Compiled Parser

**Development version:** `0.9.0-alpha.2`
**Status:** Complete
**Implementation record:** `docs/0.9.0-T33-LEGACY-PARSER.md`

### Work

- implement header parsing and magic validation;
- parse names/aliases/description;
- parse Boolean table;
- implement alignment rule;
- parse signed 16-bit numerics;
- parse string offsets/table;
- map standard positions through canonical metadata;
- implement absent/canceled handling;
- implement exact Latin-1 capability-byte decoding;
- enforce T32 bounds before allocation;
- pass all legacy T29 valid/malformed fixtures.

### Acceptance gate

A caller can parse a valid legacy compiled entry from bytes into the frozen 0.8
semantic model without filesystem involvement.

---

## T34 — ncurses Extended Sections and `01036`

**Development version:** `0.9.0-alpha.3`
**Status:** Complete
**Implementation record:** `docs/0.9.0-T34-EXTENDED-PARSER.md`

### Work

- parse ncurses extended header/counts;
- parse extended Booleans;
- parse extended numerics;
- parse extended strings and names;
- enforce exact extended name ordering;
- reject standard-name collisions according to the frozen contract;
- add `01036` format detection;
- parse signed 32-bit standard/extended numerics;
- validate direct-color-scale and near-int-boundary fixtures;
- pass T29 extended/32-bit/malformed fixtures.

### Acceptance gate

Every supported frozen T29 valid compiled fixture parses to its expected
semantic manifest.

---

## T35 — Parser Hardening, Diagnostics, and Fuzz Gate

**Development version:** `0.9.0-alpha.4`
**Status:** Complete
**Implementation record:** `docs/0.9.0-T35-PARSER-HARDENING.md`

### Work

- expand truncation tests across every section boundary;
- add overflow/count/offset adversarial tests;
- enforce final parser size/name/string bounds;
- stabilize compiled-format exception diagnostics;
- add random-byte/fuzz-style safety tests;
- add optional maintainer differential checks against ncurses;
- verify failure atomicity;
- audit parser allocations and hot paths;
- verify no filesystem/environment dependency entered the parser.

### Acceptance gate

Malformed/untrusted bytes fail deterministically without unbounded allocation,
integer overflow, partial results, or native dependencies.

---

## T36 — Explicit Directory Provider

**Development version:** `0.9.0-alpha.5`
**Status:** Complete
**Implementation record:** `docs/0.9.0-T36-DIRECTORY-PROVIDER.md`

### Work

- implement one-root directory provider;
- validate terminal names before path construction;
- implement literal first-character layout;
- implement hexadecimal first-character layout;
- parse candidate files through the common parser;
- verify parsed identity/alias against requested name;
- distinguish clean misses from I/O/parser failures;
- add temporary-root and path-traversal tests;
- keep provider construction independent of environment discovery.

### Acceptance gate

An application can explicitly point `Icod.TermInfo` at an arbitrary
conventional terminfo directory tree and load exact terminal names safely.

---

## T37 — Encoded `TERMINFO` and Discovery Inputs

**Development version:** `0.9.0-beta.1`
**Status:** Complete
**Implementation record:** `docs/0.9.0-T37-DISCOVERY-INPUTS.md`

### Work

- implement bounded `TERMINFO=hex:` decoding;
- implement bounded `TERMINFO=b64:` decoding;
- route decoded bytes through the common parser;
- verify encoded-entry identity against requested name;
- freeze environment snapshot behavior;
- implement platform-safe `TERMINFO_DIRS` splitting;
- implement empty-component expansion semantics;
- implement deterministic root deduplication;
- add options for environment/user/default-source enablement.

### Acceptance gate

Every non-filesystem and search-list input can be interpreted deterministically
without duplicating parser logic or corrupting Windows paths.

---

## T38 — System Discovery and Precedence

**Development version:** `0.9.0-beta.2`
**Status:** Complete
**Implementation record:** `docs/0.9.0-T38-SYSTEM-DISCOVERY.md`

### Work

- implement `TERMINFO` location precedence;
- implement applicable `$HOME/.terminfo` discovery;
- implement ordered `TERMINFO_DIRS` roots;
- freeze/test Linux default roots;
- freeze/test macOS default roots;
- define/test Windows default behavior without inventing Unix roots;
- compose all sources in deterministic precedence;
- ensure malformed explicit sources are not silently hidden as misses;
- verify restricted options disable environment/user/default sources exactly.

### Acceptance gate

A caller can explicitly construct a deterministic system provider which finds
arbitrary installed conventional terminfo descriptions using the documented
precedence.

---

## T39 — Provider Cache, Refresh, and Composition

**Development version:** `0.9.0-beta.3`
**Status:** Complete
**Implementation record:** `docs/0.9.0-T39-CACHE-REFRESH-COMPOSITION.md`

### Work

- implement provider-local successful-entry caching;
- make concurrent first load safe;
- keep clean misses observable rather than permanently negative-cached;
- freeze new-provider refresh semantics;
- verify no process-global external-description cache exists;
- test multiple providers with different roots/options;
- document and sample explicit built-in fallback composition;
- verify system lookup never mutates `TerminalDatabase.BuiltIn`.

### Acceptance gate

Repeated system acquisition is efficient, deterministic, thread-safe, and
isolated to caller-owned provider instances.

---

## T40 — API, Documentation, Samples, and Package Freeze

**Development version:** `0.9.0-rc.1`
**Status:** Complete
**Implementation record:** `docs/0.9.0-T40-API-PACKAGE-FREEZE.md`

### Work

- review every new 0.9 public type/member;
- freeze exported public API baseline;
- review nullability/exception/ownership contracts;
- update README and supporting docs;
- add byte-parser, explicit-root, restricted-system, normal-system, and
  built-in-fallback samples;
- expand fresh-package consumer coverage;
- verify fixtures/tools remain outside runtime package content;
- verify no forbidden native/runtime dependencies were added;
- update active release workflow/branch documentation for 0.9.

### Acceptance gate

A fresh packaged consumer can parse bytes, load an explicit directory, use
system discovery under explicit options, and compose fallback providers through
the frozen public API.

---

## T41 — 0.9.0 Completion Gate

**Final version:** `0.9.0`
**Status:** Complete
**Implementation record:** `docs/0.9.0-CONTRACT-AUDIT.md`

Before tagging `v0.9.0`, perform the full completion audit.

Required evidence includes:

- Windows/Linux/macOS Release CI passes;
- package validation and fresh-package consumer pass;
- all 0.8 public enum values and compatible members remain frozen;
- all 0.8 built-ins remain behaviorally compatible;
- all valid T29 compiled fixtures parse to expected semantic manifests;
- all malformed T29 seeds fail through the intended diagnostics;
- legacy `0432` parsing passes;
- extended ncurses parsing passes;
- `01036` 32-bit parsing passes;
- exact high-byte/Latin-1 round trips pass;
- absent/canceled semantics pass;
- parser allocation/overflow/fuzz gates pass;
- pure parser has no filesystem/environment dependency;
- directory literal/hex layout tests pass;
- terminal-name path-safety tests pass;
- parsed identity verification passes;
- `TERMINFO` path and encoded forms pass;
- user-home discovery passes where applicable;
- `TERMINFO_DIRS` order/empty-component semantics pass;
- Windows path-list behavior passes;
- platform default roots match the documented contract;
- restricted discovery options pass;
- provider clean-miss versus error semantics pass;
- provider-local cache/concurrency gates pass;
- new-provider refresh semantics pass;
- explicit built-in fallback composition passes;
- no process-global current terminal/cache was introduced;
- no native ncurses dependency exists;
- no Berkeley DB dependency exists;
- no termcap parser exists;
- no terminfo source compiler/parser exists;
- no live terminal probing/input/session ownership entered the package;
- README and roadmap match the shipped behavior.

### Completion question

The final 0.9 audit SHALL answer **yes** to both questions:

> Can a caller hand `Icod.TermInfo` any valid compiled entry in the supported
> frozen System V/ncurses family and obtain the same complete immutable semantic
> shape that 0.8 could construct manually?

and

> Can a caller explicitly ask `Icod.TermInfo` to find such an entry through a
> configured directory/system search without introducing hidden global state or
> terminal-specific C# code?

If either answer is no, 0.9 is not complete.

---

# 22. Post-0.9 and 1.0 Readiness

Completing 0.9 closes the last major architectural promise currently assigned
to the **core low-level `Icod.TermInfo` library**.

At that point, the project should perform an explicit 1.0 readiness review
rather than automatically assigning every remaining terminal feature to
`Icod.TermInfo`.

A plausible 1.0 core contract after 0.9 is:

```text
identify/load a terminal description
        +
represent conventional terminfo semantics completely
        +
query/enumerate/expand/output those semantics correctly
        +
do so portably without native ncurses
```

The following are **not automatically prerequisites for Icod.TermInfo 1.0**:

- `.ti` source parsing or `tic`/`infocmp` tooling;
- termcap compatibility;
- Berkeley DB hashed stores;
- divergent historical vendor formats;
- live terminal session ownership;
- input-event decoding;
- active probing;
- graphics/clipboard/hyperlink operations;
- PTYs;
- curses/virtual-screen functionality.

Those futures are documented in `docs/FUTURE-WORK-INVENTORY.md`.

---

# 23. Reference Baseline

Implementation shall treat the frozen 0.8 documents and checked-in T29 fixtures
as the normative project contract.

Useful external cross-checks include the contemporary ncurses documentation for:

- `term(5)` compiled storage formats and directory/hashed layouts;
- `terminfo(5)` source semantics and database search precedence;
- `user_caps(5)` extended capabilities;
- `tic(1m)` and `infocmp(1m)` for maintainer fixture/differential checks.

External documentation may clarify implementation details, but SHALL NOT
silently expand the 0.9 vendor/format scope beyond this roadmap.
