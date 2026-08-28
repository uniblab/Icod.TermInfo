# Icod.TermInfo 1.4.0 — Tool Suite Roadmap

**Repository:** `uniblab/Icod.TermInfo`
**Baseline release:** `1.3.0` / tag `v1.3.0`
**Baseline release commit:** `7359eba4b5dffe8e69eda2fece4bd4cd8cdf5003`
**Language:** C# 13
**Library target frameworks:** `net8.0`; `net9.0`; `net10.0`
**Command target framework:** `net10.0`
**Command infrastructure:** `Icod.CommandFramework 2.0.0`
**Runtime API contract:** frozen at 1.0
**Source API contract:** frozen at 1.1
**Compiler API contract:** frozen at 1.2
**Inspection released API contract:** frozen at 1.3
**Inspection 1.4 API baseline:** active through reviewed T03 additions
**New commands:** `tic`, `infocmp`, `toe`
**Development branch:** `1.4.0`
**Development sequence:** `1.4.0-Alpha-1` through `1.4.0-Alpha-11`
**Status:** T04 implementation candidate
**Release objective:** expose the completed Runtime, Source, Compiler, and Inspection engines as useful, deterministic, cross-platform Unix-style command-line tools without moving command policy into the lower-level libraries or weakening their existing compatibility contracts.

---

# 1. Executive decision

Version 1.4.0 should be the release where `Icod.TermInfo` becomes a complete
**managed terminfo toolchain**, not merely a family of libraries.

The release should introduce three real executable projects:

```text
tic
infocmp
toe
```

The commands should be useful in ordinary terminal-development, database-
maintenance, inspection, comparison, and diagnostics workflows on Windows,
Linux, and macOS.

The release should **not** attempt to reproduce every option ever accumulated by
ncurses. The contemporary ncurses programs contain a mixture of standardized,
historical, compatibility, debugging, termcap-conversion, C-code-generation,
and implementation-specific switches. Reproducing all of them in one release
would obscure the clean architectural boundary established in 1.0 through 1.3.

The 1.4 compatibility target should therefore be:

> Implement the mainstream semantic workflows of `tic`, `infocmp`, and `toe`,
> adopt familiar option names where the existing Icod engines can support them
> honestly, and reject unsupported compatibility switches explicitly rather
> than silently approximating them.

The release should preserve these package boundaries:

```text
                 command layer — net10.0

          tic          infocmp          toe
           |              |              |
           +--------------+--------------+
                          |
                Icod.CommandFramework
                          |
          +---------------+---------------+
          |                               |
          v                               v
 Icod.TermInfo.Compiler        Icod.TermInfo.Inspection
          |                    /                   |
          v                   v                    v
 Icod.TermInfo.Source ----------------> Icod.TermInfo


                 library layer — net8/net9/net10
```

More precisely:

```text
tic
    -> Icod.CommandFramework
    -> Icod.TermInfo.Compiler
    -> Icod.TermInfo.Inspection
    -> Source / Runtime transitively

infocmp
    -> Icod.CommandFramework
    -> Icod.TermInfo.Inspection
    -> Source / Runtime transitively

toe
    -> Icod.CommandFramework
    -> Icod.TermInfo.Inspection
    -> Source / Runtime transitively

Compiler
    -> Source -> Runtime
    -> Runtime

Inspection
    -> Source -> Runtime
    -> Runtime
```

There SHALL be no dependency from Runtime, Source, Compiler, or Inspection to
`Icod.CommandFramework` or to any command project.

There SHALL be no production dependency from Inspection to Compiler.

The four library packages continue to form the coordinated package family.
Command executables sit above that family.

---

# 2. Audit of the 1.3.0 baseline

## 2.1 Release state

`v1.3.0` is the published baseline for 1.4 development.

The tag identifies release commit:

```text
7359eba4b5dffe8e69eda2fece4bd4cd8cdf5003
```

The release publishes the coordinated packages:

```text
Icod.TermInfo
Icod.TermInfo.Source
Icod.TermInfo.Compiler
Icod.TermInfo.Inspection
```

Version 1.3 intentionally stopped short of command-line tools. Its README states
that command-line `tic`, `infocmp`, and `toe` remain later work.

The existing post-1.0 roadmap already identifies 1.4 as the Tool Suite release.

## 2.2 The lower engines are ready

The required engines already exist.

Runtime supplies:

- compiled terminfo parsing;
- conventional directory lookup;
- deterministic system discovery;
- immutable `TerminalDescription`;
- standard and extended capability metadata;
- provider composition;
- built-in fallback.

Source supplies:

- `.ti` parsing;
- source diagnostics and spans;
- unresolved source entries/documents;
- `use=` inheritance;
- cancellation;
- disabled fields;
- extended capabilities;
- materialization into `TerminalDescription`.

Compiler supplies:

- deterministic legacy and wide compiled terminfo writing;
- standard and extended sections;
- source compilation;
- explicit conventional database-layout publication;
- representation validation;
- round-trip validation.

Inspection supplies:

- canonical effective rendering;
- normalized unresolved-source rendering;
- structured effective comparison;
- structured source-aware comparison;
- explicit provider/name inspection targets;
- deterministic ordering.

That means 1.4 should primarily be a **composition and command-policy release**.

The command projects should not reimplement parsers, writers, renderers, or
semantic comparison.

## 2.3 One missing reusable mechanism: database catalog inspection

`toe`, `tic -D`, and `infocmp -D` need visibility into terminfo database
locations.

The frozen Runtime provider intentionally exposes only:

```text
TryLoad(name, out terminal)
```

and keeps its system-discovery snapshot private.

That was correct for 1.0 through 1.3. It should remain correct.

Version 1.4 therefore needs one narrowly scoped addition above Runtime:

> Inspection should gain reusable read-only database-location and directory-
> catalog inspection APIs.

The Runtime public API SHALL NOT be enlarged for this purpose.

Implementation should reuse Runtime's existing internal discovery machinery
through an explicitly reviewed friend-assembly seam rather than copying
`TERMINFO`, user-directory, `TERMINFO_DIRS`, and platform-default discovery logic
into multiple commands.

The preferred implementation is:

```text
Runtime internal discovery snapshot
              |
              | InternalsVisibleTo
              v
Inspection 1.4 database-location/catalog API
              |
        +-----+-----+
        |     |     |
       tic infocmp toe
```

This creates an internal coordinated-package seam, not a new public Runtime
contract.

## 2.4 `Icod.CommandFramework` boundary

`Icod.CommandFramework 2.0.0` currently targets `net10.0`.

It already provides the command-neutral mechanisms needed by 1.4:

- option parsing;
- command execution context;
- injected standard streams;
- cancellation;
- deterministic diagnostics;
- conventional exit codes;
- cross-platform command infrastructure.

Its design rule is also the right rule for TermInfo:

> mechanism belongs in the framework; command-specific grammar, help text,
> compatibility policy, and semantics remain command-owned.

Therefore:

- the TermInfo libraries keep `net8.0;net9.0;net10.0`;
- the new commands target `net10.0`;
- no library TFM is removed;
- no library acquires an `Icod.CommandFramework` dependency.

## 2.5 Existing Icod executable convention

The existing Icod command repositories establish a useful executable pattern:

```text
<command>/
    Icod.<Suite>.<Command>.csproj
    Program.cs
    README.md
    src/
```

with:

```text
OutputType   Exe
Target       net10.0
AssemblyName conventional lowercase command name
```

`Program.Main` should remain a thin host:

- validate `args`;
- create cancellation state;
- attach Ctrl+C cancellation;
- open standard streams;
- invoke a testable command engine;
- restore the signal handler.

The command implementation should expose a form equivalent to:

```csharp
Command.RunAsync(
    args,
    stdin,
    stdout,
    stderr,
    cancellationToken
)
```

Tests should call the command engine directly with injected streams.

## 2.6 External compatibility reference

The 1.4 design should use contemporary ncurses 6.6 command behavior as an
interoperability reference, not as source code and not as an obligation to clone
every option.

The reviewed reference commands are:

- `tic(1m)`;
- `infocmp(1m)`;
- `toe(1m)`.

Important observations:

- `tic` has a large implementation-specific option surface; X/Open documents
  only a smaller core;
- `infocmp` has distinct source-listing, comparison, reconstruction,
  conversion, C-initializer, and analysis modes;
- `toe` is an ncurses utility with no POSIX/X/Open standard;
- termcap conversion is deeply entangled with several `tic`/`infocmp` switches;
- hashed database behavior is outside the current Icod Runtime contract.

Those facts strongly favor a scoped 1.4 compatibility contract.

---

# 3. Package, project, and versioning contract

## 3.1 Library package family

The coordinated package family remains:

```text
Icod.TermInfo
Icod.TermInfo.Source
Icod.TermInfo.Compiler
Icod.TermInfo.Inspection
```

All four package versions SHALL advance together during 1.4 development:

```text
T01  1.4.0-Alpha-1
T02  1.4.0-Alpha-2
T03  1.4.0-Alpha-3
T04  1.4.0-Alpha-4
T05  1.4.0-Alpha-5
T06  1.4.0-Alpha-6
T07  1.4.0-Alpha-7
T08  1.4.0-Alpha-8
T09  1.4.0-Alpha-9
T10  1.4.0-Alpha-10
T11  1.4.0-Alpha-11
```

Final release closure changes all four to:

```text
1.4.0
```

All four library assemblies retain:

```text
AssemblyVersion 1.0.0.0
```

## 3.2 Frozen API baselines

These historical baselines SHALL remain unchanged:

```text
docs/1.0.0-PUBLIC-API-BASELINE.txt
docs/1.1.0-SOURCE-PUBLIC-API-BASELINE.txt
docs/1.2.0-COMPILER-PUBLIC-API-BASELINE.txt
docs/1.3.0-INSPECTION-PUBLIC-API-BASELINE.txt
```

Runtime, Source, and Compiler public surfaces SHOULD remain exactly unchanged in
1.4.

Inspection MAY receive compatible additive API needed for:

- database-location inspection;
- directory catalog enumeration;
- optional rendering controls needed by `infocmp`.

Do not edit the historical 1.3 baseline.

Create:

```text
docs/1.4.0-INSPECTION-PUBLIC-API-BASELINE.txt
```

At T01 it should begin as an exact copy of the 1.3 Inspection public surface.
Approved 1.4 additions are recorded there tranche by tranche.

At release closure, the 1.4 Inspection baseline becomes the frozen 1.4 public
contract.

## 3.3 Command projects

Create top-level command directories rather than placing user-facing programs
under the repository's maintenance `tools/` directory:

```text
tic/
    Icod.TermInfo.Tic.csproj
    Program.cs
    README.md
    src/

infocmp/
    Icod.TermInfo.InfoCmp.csproj
    Program.cs
    README.md
    src/

toe/
    Icod.TermInfo.Toe.csproj
    Program.cs
    README.md
    src/
```

Create tests:

```text
tests/
    Icod.TermInfo.Tic.Tests/
    Icod.TermInfo.InfoCmp.Tests/
    Icod.TermInfo.Toe.Tests/
```

Command assembly names SHALL be:

```text
tic
infocmp
toe
```

The command projects SHALL:

- target `net10.0`;
- use C# 13;
- enable nullable reference analysis;
- use `Icod.CommandFramework 2.0.0`;
- produce portable PDBs;
- treat Release warnings as errors;
- be independently executable;
- have no command-to-command project dependency.

No command should invoke another command as a subprocess.

## 3.4 Command version identity

All three commands SHALL report the same semantic suite version as the
coordinated TermInfo packages.

During development:

```text
1.4.0-Alpha-X
```

and at release:

```text
1.4.0
```

The command executable assembly identity is not part of the frozen library ABI.
It MAY advance with the suite version.

A repository contract test SHALL fail if:

- any of the four package versions differ;
- a command reports a different semantic version;
- a command still reports the preceding tranche.

Version text should be generated from assembly/package metadata rather than
maintained independently in command source.

## 3.5 Shared tool code

Do **not** create `Icod.TermInfo.Tools.Shared` at T01 merely because three
commands exist.

Shared code should first be classified:

- command-neutral CLI mechanism -> `Icod.CommandFramework`;
- reusable terminfo inspection/catalog mechanism -> `Icod.TermInfo.Inspection`;
- compilation semantics -> `Icod.TermInfo.Compiler`;
- source semantics -> `Icod.TermInfo.Source`;
- runtime acquisition/semantic model -> `Icod.TermInfo`;
- genuinely command-family-only presentation/policy -> may justify a small
  internal shared project later.

A command-family shared project MAY be introduced only if real duplication
appears and the code does not naturally belong in an existing layer.

If introduced, it SHOULD be:

```text
Icod.TermInfo.Tools.Shared
```

and SHOULD be:

- `net10.0`;
- `IsPackable=false`;
- command-policy only;
- absent from the public package contract.

---

# 4. Common command contract

## 4.1 Execution model

Each command SHALL separate process hosting from command behavior.

`Program` owns:

- process console streams;
- Ctrl+C registration;
- process lifetime.

`Command` owns:

- argument parsing;
- semantic validation;
- environment snapshot;
- engine orchestration;
- output;
- diagnostics;
- exit status.

Tests SHALL exercise `Command` directly.

Command code SHALL NOT dispose caller-owned streams.

## 4.2 Exit statuses

Use the existing `Icod.CommandFramework.Diagnostics.CommandExitCodes` contract:

```text
0    Success
1    Operational/data failure
2    Usage error
130  Cancellation
```

Command-specific result data is not itself an error.

In particular:

- `infocmp` finding differences returns success;
- `toe` finding zero entries returns success unless the requested source itself
  failed;
- `tic -c` returns failure when source contains errors;
- warnings alone do not force failure unless a future explicit
  warnings-as-errors switch is introduced.

## 4.3 Standard streams

Use:

```text
stdout  requested command result
stderr  diagnostics, warnings, progress/verbose output
stdin   source input only when explicitly selected
```

Help and version output should go to stdout on successful invocation.

Usage errors go to stderr.

## 4.4 Determinism

For identical inputs and environment snapshots, command results SHALL be
independent of:

- operating-system directory enumeration order;
- dictionary insertion order;
- current culture;
- process execution order;
- hash randomization.

Where a command exposes a deliberately unsorted mode, the ordering must still
be defined by an observable source order rather than incidental dictionary
enumeration.

## 4.5 Text encoding

For 1.4:

- command source input is UTF-8;
- UTF-8 BOM MAY be accepted;
- malformed UTF-8 is a deterministic input error;
- command textual output is UTF-8 without BOM;
- terminfo capability byte semantics remain governed by the existing
  Latin-1/reversible source rules inside the Source/Compiler layers.

Do not infer application-text encoding from terminal capabilities.

## 4.6 Diagnostics

Expected errors SHALL be rendered as controlled diagnostics.

Do not expose stack traces for ordinary:

- usage errors;
- source syntax errors;
- missing entries;
- malformed compiled entries;
- unsupported database formats;
- I/O failures;
- permission failures.

Source diagnostics should retain source name, line, column/span, severity, and
stable `TIS` diagnostic code where available.

## 4.7 Environment snapshot

Environment-dependent behavior should be captured once per command invocation.

Relevant values include:

```text
TERM
TERMINFO
TERMINFO_DIRS
HOME / platform-equivalent user directory
current directory
host platform
```

A long-running command should not change behavior because another thread mutates
process environment variables after execution begins.

## 4.8 No native command dependency

Production commands SHALL NOT shell out to:

```text
tic
infocmp
toe
ncurses utilities
```

Normal CI SHALL NOT require them.

Native ncurses tools MAY be used only by optional differential-fixture
generation or explicitly marked developer validation.

---

# 5. Inspection additions required by the tool layer

## 5.1 Database-location inspection

Inspection should gain a public read-only abstraction representing the ordered
database sources the Runtime discovery model would consider.

Exact type names should receive an API-regret review during T02, but the model
must distinguish at least:

```text
explicit directory
TERMINFO directory
TERMINFO encoded entry
user database
TERMINFO_DIRS directory
platform default directory
```

A location should retain enough information for diagnostics and display without
claiming unsupported provenance.

The API SHALL NOT claim that a location is writable merely because it exists.

Writable-destination policy belongs to `tic`.

## 5.2 Reuse Runtime discovery internals

Do not duplicate Runtime discovery order in Inspection.

Preferred implementation:

1. retain the existing Runtime internal discovery snapshot/service;
2. expose it to Inspection via a deliberate `InternalsVisibleTo` seam;
3. add internal tests proving Inspection observes the same location ordering as
   `SystemTerminalDescriptionProvider`;
4. keep `docs/1.0.0-PUBLIC-API-BASELINE.txt` byte-for-byte unchanged.

This friend seam is acceptable because Runtime and Inspection are coordinated
packages and Inspection already has a production Runtime dependency.

The seam SHALL remain narrow and documented.

## 5.3 Conventional directory catalog

Inspection should also provide a reusable directory catalog operation.

It should:

- accept one explicit conventional terminfo root;
- enumerate only the conventional one-level first-character layout;
- recognize literal first-character and two-digit hexadecimal directories;
- avoid arbitrary recursive traversal;
- parse candidate files through `CompiledTermInfoParser`;
- retain canonical name, aliases, description, and source path/location;
- detect duplicate physical/canonical entries deterministically;
- return entries in deterministic order;
- surface malformed data and I/O errors explicitly;
- avoid following directory links/reparse points into recursive loops.

The catalog operation should not mutate provider caches or the filesystem.

## 5.4 Hashed databases

A `.db`/Berkeley DB terminfo store is not a conventional directory.

1.4 should detect an explicitly encountered hashed-store shape where practical
and report it as unsupported.

It SHALL NOT:

- silently treat it as an empty directory;
- add a Berkeley DB dependency;
- guess the file format.

Hashed database support remains later optional provider work.

## 5.5 Rendering options for `infocmp`

The existing 1.3 rendering overloads have frozen deterministic behavior.

Do not change their output.

If `infocmp` needs configurable width or extended-capability filtering, add new
options/overloads in 1.4 rather than changing existing overloads.

Potential responsibilities include:

```text
line width
single-line output
one-capability-per-line output
include/exclude extended capabilities
capability ordering
```

Exact public shape should be frozen only when T06 demonstrates the real command
need.

---

# 6. Command compatibility contract

The 1.4 command surface is intentionally divided into:

```text
required
optional if straightforward within existing engines
explicitly deferred
```

Unsupported ncurses-compatible options SHALL produce a usage diagnostic.
They SHALL NOT be silently accepted and ignored.

---

# 7. `tic` 1.4 contract

## 7.1 Purpose

`tic` compiles supported terminfo `.ti` source into conventional compiled
terminfo entries using the existing Source and Compiler packages.

The command layer owns:

- option parsing;
- file/stdin acquisition;
- destination selection;
- entry selection;
- user-facing diagnostics;
- exit codes;
- progress/summary output.

It does not own source parsing or compiled representation.

## 7.2 Basic syntax

The required 1.4 form is:

```text
tic [options] file
```

`file` may be:

```text
-
```

to read source from standard input.

Exactly one source operand is required in 1.4.

Implicit `./terminfo.src` fallback is not supported.

## 7.3 Required options

### `-c`

Check-only mode.

The command SHALL:

- parse the complete source;
- resolve `use=` references available within the supplied document;
- perform representation checks which can be performed without writing;
- emit diagnostics;
- produce no compiled database output.

Errors -> exit 1.

Warnings only -> exit 0.

### `-o <directory>`

Explicit output root.

This is the preferred database-write mode.

The existing `CompiledTermInfoDatabaseWriter` owns safe path derivation,
directory creation, overwrite policy, and failure-resistant publication.

### `-e <name-list>`

Compile only selected entries.

For 1.4.0, the required grammar is a comma-separated list of canonical names or
aliases.

ncurses's additional interpretation of a slash-containing value as a filename is
not required for 1.4.0.

Selection order follows source-document order, not option-list order.

An explicitly selected name which matches no source entry is an error.

### `-x`

Permit source capabilities classified as unknown extended capabilities.

Without `-x`:

- known standard capabilities are accepted;
- known extended capabilities are accepted;
- unknown extended capability declarations produce a deterministic diagnostic.

With `-x`, syntactically valid unknown extended capabilities may flow through the
existing Source and Compiler semantic model.

This makes the switch meaningful without changing Source's generic API.

### `-s`

Write a concise compile summary to stderr:

```text
destination
entries compiled
warnings
```

No progress chatter is written unless requested.

### `-D`

Print the ordered database locations known to the Icod discovery model and exit.

No database is modified.

### `-V` and `--version`

Print the Icod.TermInfo tool-suite version and exit.

The output must identify Icod, not pretend to be ncurses.

### `--help`

Document the Icod-supported contract and explicitly separate supported switches
from ncurses switches which are not implemented.

## 7.4 Destination policy

The library writer deliberately requires an explicit root.

The `tic` command may add safe user-facing destination policy.

Recommended 1.4 default order when `-o` is absent:

```text
1. directory-valued TERMINFO
2. user-local .terminfo directory on platforms where the Runtime model defines it
3. fail and require -o
```

1.4 SHOULD NOT silently write a platform system database such as
`/usr/share/terminfo`.

That is a deliberate safety difference from some native `tic` implementations.

A later explicit system-install switch can be considered if there is real need.

An encoded `TERMINFO=hex:...` or `TERMINFO=b64:...` value is not a writable
database destination.

## 7.5 Overwrite policy

The command must expose overwrite behavior deliberately.

Recommended 1.4 behavior:

```text
default      reject existing compiled destination
--force      replace using the existing failure-resistant writer policy
```

Do not infer overwrite permission from interactive status.

Do not prompt from library code.

## 7.6 Deferred `tic` options

These are **not** required for 1.4.0:

```text
-C / -K          termcap output/conversion
-I / -L          translation modes beyond canonical Icod rendering
-N / -U          ncurses post-processing compatibility modes
-R               historical vendor subsets
-r               termcap use/tc resolution behavior
-T               legacy size-limit translation mode
-a / -t          ncurses commented-out-capability translation policy
-f / -G / -g     ncurses translation presentation
-0 / -1 / -W/-w  translation presentation modes
```

Termcap conversion belongs to 1.5.

Historical implementation subsets remain later compatibility work.

## 7.7 Candidate T10 additions

If the core command is stable early, 1.4 MAY add:

```text
-Q1   emit compiled entry as hexadecimal
-Q2   emit compiled entry as base64
-Q3   emit both
-v[n] controlled verbose progress
```

These are not allowed to delay T05 completion.

---

# 8. `infocmp` 1.4 contract

## 8.1 Purpose

`infocmp` exposes the 1.3 Inspection engine as a command-line diagnostic and
comparison utility.

The command SHALL use structured Inspection results.

It SHALL NOT parse its own rendered text to perform comparisons.

## 8.2 Default operand behavior

Adopt the familiar ncurses-style default:

```text
0 terminal operands
    -> use TERM
    -> render one terminal

1 terminal operand
    -> render that terminal

2 or more terminal operands
    -> compare the first terminal against each subsequent terminal
```

A missing/empty `TERM` when required is an operational error with a clear
diagnostic.

## 8.3 Required source-listing mode

Canonical effective source listing is the default one-terminal output.

Use `TerminalDescriptionSourceRenderer`.

The default Icod output should remain the canonical Inspection representation.

The command must not claim to reconstruct original source inheritance,
cancellations, comments, or whitespace from a compiled `TerminalDescription`.

## 8.4 Required comparison modes

### `-d`

List semantic differences.

Use `TerminalDescriptionComparer` as the authoritative difference engine.

Differences found are successful command output, not an error condition.

### `-c`

List capabilities whose effective semantic value is common to the compared
entries.

This report may use Runtime capability metadata plus the already-acquired
descriptions; it must not duplicate parsing or acquisition.

### `-n`

List standard capabilities absent from all compared entries.

Extended names have no closed universe, so absence reporting for arbitrary
extended capabilities is undefined unless an explicit comparison universe is
provided.

1.4 should therefore apply `-n` to the standard catalog only.

## 8.5 Database selection

Support:

```text
-A <directory>    first terminal database
-B <directory>    subsequent terminal database
```

These construct explicit `DirectoryTerminalDescriptionProvider` instances.

They do not mutate process environment variables.

Without `-A`/`-B`, use a normal explicit system provider.

## 8.6 Required presentation options

### `-q`

Short comparison presentation.

This is presentation policy only; it does not alter semantic comparison.

### `-0`

Single logical source line where representable.

### `-1`

One capability per line.

### `-w <width>`

Requested wrapping width for source listing.

These options must be implemented through approved 1.4 renderer options or a
shared Inspection presentation mechanism, not by fragile post-processing of an
already-rendered 80-column string.

The existing no-options 1.3 renderer output must remain unchanged.

### `-s <key>`

Support deterministic ordering keys where the Runtime metadata already provides
the vocabulary:

```text
d    compiled/database order
i    terminfo short name
l    long variable name
c    termcap code
```

Ordering applies within Boolean/numeric/string groups.

## 8.7 Extended capabilities

Recommended 1.4 behavior:

```text
default   standard capabilities
-x        include extended capabilities
```

This aligns familiar `infocmp` expectations while retaining the richer Icod
semantic model.

The underlying 1.3 Inspection APIs continue to expose extended capabilities;
the filtering choice is command presentation policy unless a reusable renderer
option is required.

## 8.8 Database reporting

Support:

```text
-D
```

using the T02 Inspection database-location API.

## 8.9 Version/help

Support:

```text
-V
--version
--help
```

Version output identifies `Icod.TermInfo 1.4.0` and the command name.

## 8.10 Deferred `infocmp` modes

These are not 1.4.0 release requirements:

```text
-C / -K       termcap conversion
-L            long-name source translation mode if it requires new source grammar
-R            historical vendor subsets
-e / -E       C TERMTYPE initializer generation
-i            initialization-string semantic analyzer
-F            whole source-file pairwise comparison mode
-u            relative use= source synthesis
-p            padding-insensitive comparison
-r            termcap/source-resolution compatibility behavior
```

`-u` is attractive but is a real synthesis algorithm, not mere presentation.
It should not be smuggled into 1.4 under the assumption that structured
differences are sufficient.

It may become a 1.4.x enhancement after the base command is released.

Termcap-related modes remain naturally associated with 1.5.

---

# 9. `toe` 1.4 contract

## 9.1 Purpose

`toe` lists and analyzes terminal entries available from conventional terminfo
directories.

Unlike `tic` and the basic `infocmp` contract, `toe` has no POSIX/X/Open
standard. Icod should therefore adopt its useful workflows without claiming
complete ncurses identity.

## 9.2 Basic syntax

Required forms:

```text
toe [options] [directory ...]
toe -u file
toe -U file
```

When explicit directories are provided, enumerate those directories in operand
order.

When no directory is supplied, use the Inspection database-location discovery
API.

## 9.3 Default directory behavior

Without `-a`:

- enumerate the first applicable conventional directory in the normal search
  sequence;
- encoded `TERMINFO` is not a directory catalog and is skipped for enumeration;
- missing directories may be clean misses;
- malformed/unsupported database containers are diagnostic conditions.

With `-a`:

- enumerate all applicable conventional directory locations in search order.

## 9.4 Required listing data

Each listed terminal should expose at least:

```text
canonical name
description
```

Aliases may be included in verbose/detail output but should not replace the
canonical listing identity.

Entries must be parsed rather than inferred from filenames alone.

A malformed file named `xterm` is not valid evidence that the database contains
a terminal named `xterm`.

## 9.5 Required options

### `-a`

Enumerate all applicable conventional search directories.

Do not merge duplicate names by default.

### `-h`

Print a heading identifying each directory before its entries.

### `-s`

Sort entries by canonical terminal name.

When combined with `-a`, the command SHOULD additionally identify duplicate
canonical names across databases.

If semantic equivalence markers are provided:

- equality/difference must be determined by
  `TerminalDescriptionComparer`;
- file byte equality is not semantic equality;
- output markers must be documented as Icod format unless exact ncurses
  formatting is deliberately adopted.

### `-u <file>`

Parse a terminfo source file and list forward `use=` dependencies.

For each source entry, report the ordered set of referenced parent names.

The 1.4 mode accepts terminfo source only.

Termcap `tc=` analysis remains 1.5 work.

### `-U <file>`

Parse a terminfo source file and list reverse `use=` dependencies.

Ordering must be deterministic and source-aware.

### `-V`, `--version`, `--help`

Provide normal suite version/help behavior.

### `-v[n]`

A basic deterministic verbose progress mode MAY be supported if useful during
catalog scans.

Verbose output belongs to stderr.

## 9.6 Error continuation

Directory enumeration often encounters mixed-quality database trees.

Recommended policy:

- continue past an individual malformed/unreadable entry when it is safe to do
  so;
- emit one deterministic diagnostic for that entry;
- remember that an operational error occurred;
- return exit 1 after producing all safely obtainable results.

Do not silently omit bad entries and still report unconditional success.

## 9.7 Filesystem safety

Catalog enumeration SHALL:

- avoid unbounded recursion;
- avoid following directory links/reparse points into loops;
- validate conventional subdirectory names;
- bound entry file size using Runtime parser limits;
- use checked allocation;
- propagate cancellation.

---

# 10. Compatibility matrix for 1.4.0

The minimum release contract is:

| Command | Required 1.4.0 surface |
| --- | --- |
| `tic` | file / `-`, `-c`, `-o`, `-e`, `-x`, `-s`, `-D`, `-V`, `--version`, `--help`, explicit overwrite policy |
| `infocmp` | TERM fallback, one-entry source listing, multi-entry comparison, `-d`, `-c`, `-n`, `-A`, `-B`, `-q`, `-0`, `-1`, `-w`, `-s`, `-x`, `-D`, `-V`, `--version`, `--help` |
| `toe` | explicit/default directories, `-a`, `-h`, `-s`, `-u`, `-U`, `-V`, `--version`, `--help` |

Candidate additions which do not block release:

| Command | Candidate |
| --- | --- |
| `tic` | `-Q1/-Q2/-Q3`, `-v[n]` |
| `infocmp` | `-Q1/-Q2/-Q3` only if a command-layer Compiler dependency is deliberately accepted |
| `toe` | `-v[n]`, richer duplicate-source markers |

Explicitly deferred families:

```text
termcap conversion
captoinfo / infotocap aliases
C initializer generation
relative use= synthesis
historical vendor subset filtering
hashed/Berkeley DB stores
ncurses trace internals
exact ncurses whitespace/comment reproduction
```

---

# 11. Development tranches

The implementation program is intentionally divided into eleven small gates.
The additional tranche boundaries do not add features to the release; they
separate distinct risk classes so each can be proven before later work depends
on it.

The governing sequence is:

```text
T01  command shells and contracts
 |
 v
T02  database-location discovery seam
 |
 v
T03  conventional database catalog
 |
 v
T04  tic validation/check-only path
 |
 v
T05  tic publication/write path
 |
 v
T06  infocmp one-entry inspection/rendering
 |
 v
T07  infocmp semantic comparison
 |
 v
T08  toe database listing
 |
 v
T09  toe source dependency analysis
 |
 v
T10  CLI compatibility and distribution hardening
 |
 v
T11  differential validation, hostile input, freeze
 |
 v
release closure
```

No tranche SHALL contain implementation that belongs to a later tranche merely
because the code is convenient to add at the same time.

---

# T01 — Tool-suite foundation and command contract

**Development version:** `1.4.0-Alpha-1`

## Objective

Create the command/test/version/CI skeleton and freeze the command-layer
architecture before implementing terminfo behavior.

T01 is deliberately structural. If it becomes feature-rich, it has grown too
large.

## Required work

- create the `1.4.0` branch from the exact published `v1.3.0` commit;
- add this roadmap;
- add `docs/1.4.0-PRE-T01-CONTRACT-AUDIT.md`;
- advance Runtime, Source, Compiler, and Inspection to `1.4.0-Alpha-1`;
- retain all four library `AssemblyVersion` values at `1.0.0.0`;
- create `tic`, `infocmp`, and `toe` executable projects;
- create three corresponding command test projects;
- target all commands at `net10.0`;
- reference `Icod.CommandFramework 2.0.0` only from command projects;
- establish lowercase assembly names `tic`, `infocmp`, and `toe`;
- establish thin `Program` entry points;
- establish injected-stream `Command.RunAsync` command engines;
- establish Ctrl+C/cancellation behavior;
- establish common exit statuses;
- establish UTF-8 command text policy;
- establish minimal `--help` and `--version` behavior;
- add command projects/tests to `Icod.TermInfo.sln`;
- create `docs/1.4.0-INSPECTION-PUBLIC-API-BASELINE.txt` as an exact copy of
  the released 1.3 Inspection surface;
- update versioning, compatibility, future-work, and releasing documentation;
- update Windows/Linux/macOS CI so all three command projects build and tests
  execute;
- do not implement real `tic`, `infocmp`, or `toe` semantics yet.

## Tests

Each command SHALL prove:

- null `args` rejection at the public/internal entry boundary as appropriate;
- successful help output;
- successful version output;
- unknown option -> usage error;
- stdout/stderr separation;
- caller-owned streams are not disposed;
- cancellation maps to the suite cancellation status;
- reported version equals `1.4.0-Alpha-1`.

Repository contract tests SHALL prove:

- all four library package versions match;
- all three commands report the coordinated semantic version;
- all four library assembly versions remain `1.0.0.0`;
- no library references `Icod.CommandFramework`;
- no command references another command;
- the 1.4 Inspection baseline initially equals the released 1.3 Inspection
  public surface.

**Gate T01:** all three minimal command shells build and test on Windows, Linux,
and macOS, and the dependency/version/hosting contract is frozen.

---

# T02 — System database-location inspection

**Development version:** `1.4.0-Alpha-2`

## Objective

Expose the ordered Runtime database-discovery model for read-only inspection,
without yet enumerating database contents.

This tranche supports the future `tic -D`, `infocmp -D`, and discovery half of
`toe`.

## Required work

- add a deliberately narrow Runtime -> Inspection friend-assembly seam;
- preserve the exact Runtime 1.0 public baseline;
- expose reviewed Inspection database-location types/API;
- reuse Runtime's actual immutable discovery snapshot logic;
- distinguish:
  - encoded `TERMINFO`;
  - directory-valued `TERMINFO`;
  - user `.terminfo`;
  - `TERMINFO_DIRS` roots;
  - platform-default roots;
- preserve exact search order;
- preserve duplicate-root elimination rules;
- preserve platform path-comparison semantics;
- snapshot environment/current-directory/home/platform once;
- update the 1.4 Inspection API baseline for reviewed additions only;
- extend package-smoke validation on net8/net9/net10.

## Prohibited shortcuts

Do not copy Runtime discovery rules into Inspection or any command.

Do not expose Runtime's internal snapshot types directly as public API.

Do not add Runtime public API merely to satisfy command-line reporting.

## Tests

Synthetic discovery cases SHALL include:

```text
TERMINFO directory
TERMINFO encoded
user .terminfo
TERMINFO_DIRS
empty TERMINFO_DIRS components
duplicate roots
relative paths
Linux defaults
macOS defaults
Windows with no implicit Unix roots
environment disabled
user database disabled
system database disabled
```

**Gate T02:** Inspection can report the exact ordered database-location model
used by Runtime while the Runtime 1.0 public baseline remains unchanged.

**Implementation record:** `docs/1.4.0-T02-SYSTEM-DATABASE-LOCATION-INSPECTION.md`

---

# T03 — Conventional database catalog enumeration

**Development version:** `1.4.0-Alpha-3`

## Objective

Provide one reusable, safe, deterministic catalog engine for conventional
terminfo directory trees.

T02 answers **where Runtime would look**. T03 answers **what valid entries exist
inside one conventional directory**. They remain separate contracts.

## Required work

Inspection SHALL provide catalog enumeration which:

- accepts one explicit conventional root;
- examines only the conventional first-character layout;
- supports literal first-character directories;
- supports two-digit hexadecimal first-byte directories;
- does not perform arbitrary recursive traversal;
- parses candidate files through `CompiledTermInfoParser`;
- derives terminal identity from parsed content rather than filenames;
- returns canonical name, aliases, description, and physical source path;
- detects duplicate physical/canonical entries deterministically;
- orders results deterministically;
- applies Runtime parser resource limits;
- supports cancellation;
- surfaces malformed entries explicitly;
- surfaces I/O and permission failures explicitly;
- avoids recursive symlink/junction/reparse traversal.

## Hashed stores

Hashed/Berkeley DB stores remain unsupported in 1.4.

Where a caller explicitly encounters an identifiable unsupported store shape,
the catalog API SHALL report unsupported storage rather than silently returning
an empty catalog.

## Tests

Cover:

- empty root;
- missing root;
- literal layout;
- hexadecimal layout;
- both layouts containing the same terminal;
- aliases;
- duplicate canonical names;
- malformed entry;
- truncated entry;
- oversized entry;
- permission failure;
- symlink/reparse edge cases;
- randomized file-creation order producing identical results;
- cancellation during enumeration.

**Gate T03:** a caller can safely enumerate conventional terminfo roots through
Inspection without command-specific filesystem logic.

**Implementation record:** `docs/1.4.0-T03-CONVENTIONAL-DATABASE-CATALOG.md`

---

# T04 — `tic` validation and check-only mode

**Development version:** `1.4.0-Alpha-4`

## Objective

Make `tic` a complete source validator before allowing it to write a database.

This tranche intentionally performs no compiled-database publication.

## Required syntax

```text
tic -c [options] file
```

`file` may be `-` for standard input.

Exactly one source operand is required in 1.4.0.

## Required options

```text
-c
-e <name-list>
-x
-D
-V
--version
--help
```

## Required behavior

`tic -c` SHALL:

- read UTF-8 source;
- parse the complete document through Source;
- preserve Source diagnostics and locations;
- resolve available `use=` relationships;
- validate selected entries;
- perform representation checks which do not require publication;
- emit errors/warnings deterministically;
- return failure when errors exist;
- return success for warnings-only input;
- create no compiled database files.

`-e` SHALL accept a comma-separated list of canonical names or aliases.

A requested name that matches no source entry is an error.

`-x` SHALL control command policy for syntactically valid capabilities which
Source classifies as unknown extensions.

`-D` SHALL print the T02 location model and exit.

## Tests

Cover:

- file input;
- stdin;
- empty input;
- malformed UTF-8;
- one entry;
- many entries;
- aliases;
- `use=`;
- missing parent;
- inheritance cycle;
- cancellation;
- standard capabilities;
- known extensions;
- unknown extensions with and without `-x`;
- selected entries;
- missing selected entry;
- warnings-only source;
- source errors;
- proof that no filesystem publication occurs.

**Gate T04:** `tic -c` is a useful standalone managed source validator with
stable diagnostics and zero database mutation.

**Implementation record:** `docs/1.4.0-T04-TIC-VALIDATION-AND-CHECK-ONLY.md`

---

# T05 — `tic` compilation and database publication

**Development version:** `1.4.0-Alpha-5`

## Objective

Add the filesystem write path only after T04 source/selection/diagnostic
behavior is stable.

## Required options added

```text
-o <directory>
-s
--force
```

All T04 options remain supported.

## Destination policy

With `-o`, use the explicit directory.

Without `-o`, recommended 1.4 policy is:

```text
1. directory-valued TERMINFO
2. user-local .terminfo where Runtime defines one
3. otherwise fail and require -o
```

Do not silently write platform system databases such as `/usr/share/terminfo`.

Encoded `TERMINFO` is not a writable destination.

## Overwrite policy

Default behavior SHALL reject an existing compiled destination.

`--force` may request replacement through the existing Compiler
failure-resistant publication mechanism.

Do not prompt interactively from library or command-engine code.

## Required composition

```text
.ti source
    -> Icod.TermInfo.Source
    -> Icod.TermInfo.Compiler
    -> CompiledTermInfoDatabaseWriter
    -> conventional database
```

The command must not duplicate writer/path/publication logic.

## Tests

Add:

- explicit output root;
- default user destination;
- no safe default destination;
- encoded `TERMINFO` rejection as a destination;
- directory creation;
- existing entry rejection;
- `--force`;
- multiple compiled entries;
- `-e` selected compilation;
- cancellation during publication;
- permission/write failures;
- path-safety cases;
- interrupted/failed publication does not leave a corrupt final entry;
- Windows/Linux/macOS path behavior.

Integration acceptance SHALL prove:

```text
source
    -> tic
    -> conventional database
    -> DirectoryTerminalDescriptionProvider
    -> TerminalDescriptionComparer
    -> semantic equality
```

**Gate T05:** managed `tic` can validate and publish mainstream terminfo source
into a conventional database which Runtime can reload without semantic loss.

---

# T06 — `infocmp` one-terminal inspection and renderer controls

**Development version:** `1.4.0-Alpha-6`

## Objective

Implement acquisition and one-terminal rendering before comparison logic.

## Operand behavior

```text
0 operands -> use TERM
1 operand  -> inspect that terminal
```

Two or more operands remain a usage error until T07.

## Required options

```text
-A <directory>
-0
-1
-w <width>
-s <key>
-x
-D
-V
--version
--help
```

## Required behavior

Default one-terminal output SHALL use the canonical effective Inspection
renderer.

The command SHALL NOT claim to reconstruct source information absent from
`TerminalDescription`, including original comments, whitespace, `use=` history,
cancellation tombstones, or provenance.

`-A` constructs an explicit `DirectoryTerminalDescriptionProvider` rather than
mutating environment variables.

`-D` uses T02.

## Renderer API rule

If `-0`, `-1`, `-w`, `-s`, or `-x` require reusable renderer controls,
Inspection MAY receive additive 1.4 API.

Existing 1.3 renderer overload behavior SHALL remain unchanged.

Do not implement these options by post-processing already-rendered 80-column
text.

## Required sort keys

```text
d    compiled/catalog order
i    terminfo short name
l    long variable name
c    termcap code
```

## Tests

Cover:

- TERM fallback;
- missing TERM;
- explicit name;
- alias;
- explicit database;
- missing entry;
- malformed compiled entry;
- `-0`;
- `-1`;
- wrapping boundaries;
- every sort key;
- extended-capability filtering;
- culture independence;
- insertion-order independence;
- redirected output;
- cancellation.

**Gate T06:** `infocmp` is a complete one-terminal inspection utility and any
new renderer API needed by it is explicitly reviewed in the 1.4 Inspection
baseline.

---

# T07 — `infocmp` semantic comparison

**Development version:** `1.4.0-Alpha-7`

## Objective

Add comparison after acquisition and rendering are already proven.

## Operand behavior

```text
2 or more operands
    -> compare first terminal with each subsequent terminal
```

## Required options added

```text
-B <directory>
-d
-c
-n
-q
```

## Required semantics

`-d` SHALL list structured semantic differences produced by
`TerminalDescriptionComparer`.

Differences are successful command output and SHALL return exit status 0.

`-c` SHALL list capabilities whose effective semantic values are common.

`-n` SHALL report standard capabilities absent from all compared entries.
Extended capabilities have no closed absent-name universe and SHALL not be
invented for this mode.

`-q` controls presentation only.

`-A` selects the first terminal database and `-B` the subsequent comparison
database.

## Tests

Cover:

- equal descriptions;
- canonical-name differences;
- alias/description metadata differences;
- Boolean differences;
- numeric differences;
- string differences;
- extended differences;
- extended kind mismatch;
- standard absent-from-all;
- first-versus-many behavior;
- same terminal name in two explicit roots;
- differences -> success;
- acquisition errors -> operational failure;
- usage errors -> usage status;
- deterministic difference ordering.

**Gate T07:** common managed `infocmp` semantic comparison workflows are stable
and deterministic.

---

# T08 — `toe` conventional database listing

**Development version:** `1.4.0-Alpha-8`

## Objective

Compose T02 and T03 into a real database-listing command before adding source
dependency analysis.

## Required syntax

```text
toe [options] [directory ...]
```

## Required options

```text
-a
-h
-s
-D
-V
--version
--help
```

## Required behavior

With explicit directory operands, enumerate them in operand order.

Without explicit directories, use T02 discovery and enumerate the first
applicable conventional directory.

With `-a`, enumerate all applicable conventional directory roots in search
order.

Encoded `TERMINFO` is not a directory catalog and is skipped for enumeration.

Each entry SHALL expose at least canonical name and description.

Entries SHALL be parsed rather than inferred from filenames.

`-h` displays the source root.

`-s` sorts canonical names deterministically.

Duplicate names across roots remain visible.

## Error continuation

When an individual entry is malformed or unreadable and enumeration can safely
continue:

- emit one deterministic diagnostic;
- continue;
- remember the operational error;
- return exit status 1 after all safe results are produced.

## Tests

Cover:

- no databases;
- empty root;
- one root;
- multiple roots;
- default discovery;
- `-a`;
- headings;
- sorting;
- duplicate names;
- aliases;
- malformed entries;
- oversized entries;
- permission failures;
- missing roots;
- symlink/reparse edge cases;
- cancellation;
- stable output under randomized filesystem order.

**Gate T08:** `toe` can reliably enumerate supported conventional databases
without native ncurses or duplicated discovery logic.

---

# T09 — `toe` source dependency analysis and duplicate semantics

**Development version:** `1.4.0-Alpha-9`

## Objective

Complete `toe` with source dependency reports and optional semantic duplicate
analysis.

## Required options added

```text
-u <file>
-U <file>
```

## `-u`

Parse terminfo source and list forward `use=` dependencies for each source
entry, preserving deterministic source-aware ordering.

## `-U`

Parse terminfo source and list reverse `use=` dependencies.

## Scope

The 1.4 command accepts terminfo source only.

Termcap `tc=` dependency analysis remains 1.5 work.

## Duplicate semantic analysis

When `toe -a -s` sees the same canonical name in several roots, the command MAY
report semantic equality/difference.

If reported, equality SHALL use `TerminalDescriptionComparer`, never file-byte
identity.

## Tests

Cover:

- no dependencies;
- one `use=`;
- multiple `use=`;
- forward reference;
- reverse graph;
- inheritance cycle;
- duplicate source identities;
- missing parents;
- malformed source;
- deterministic graph ordering;
- same compiled terminal in two roots;
- same canonical name with different effective semantics;
- cancellation.

**Gate T09:** `toe` supports both conventional database enumeration and
terminfo-source dependency analysis through existing Source/Inspection engines.

---

# T10 — CLI compatibility, presentation, and distribution hardening

**Development version:** `1.4.0-Alpha-10`

## Objective

Make `tic`, `infocmp`, and `toe` behave as one coherent product suite.

No major new semantic engine belongs in T10.

## Command-line hardening

Verify:

- clustered short options where unambiguous;
- attached option values where adopted;
- separated option values;
- `--` end-of-options;
- filenames beginning with `-` after `--`;
- repeated options;
- conflicting modes;
- exact operand counts;
- unsupported ncurses switches.

Unsupported switches SHALL produce a usage diagnostic.

No unsupported switch may be silently accepted and ignored.

## Presentation hardening

Standardize:

- diagnostic prefixing;
- source-location formatting;
- headings;
- help layout;
- version layout;
- culture-independent numbers;
- deterministic ordering;
- redirected-output behavior.

No ANSI styling is required for 1.4.0.

## Documentation

Each command README SHALL document:

- synopsis;
- supported options;
- operands;
- environment interaction;
- exit statuses;
- examples;
- compatibility differences;
- explicit non-goals.

The root README SHALL add a Tool Suite section.

## Candidate additions

Only if low-risk and already supported by existing engines:

```text
tic      -Q1/-Q2/-Q3
tic      -v[n]
toe      -v[n]
```

Candidate switches SHALL NOT delay the gate.

## Distribution model

The canonical 1.4.0 command distribution SHOULD be framework-dependent .NET 10
suite archives:

```text
Icod.TermInfo.Tools.<version>.win-x64.zip
Icod.TermInfo.Tools.<version>.win-arm64.zip
Icod.TermInfo.Tools.<version>.linux-x64.tar.gz
Icod.TermInfo.Tools.<version>.linux-arm64.tar.gz
Icod.TermInfo.Tools.<version>.osx-x64.tar.gz
Icod.TermInfo.Tools.<version>.osx-arm64.tar.gz
```

Each archive contains all three commands and required managed dependencies.

The user supplies the .NET 10 runtime.

## NuGet global tools

Global-tool packaging MAY be investigated but is not a 1.4.0 release
requirement.

If it requires awkward project-reference packaging, duplicate payloads,
publication ordering hazards, or architecture compromise, defer it to 1.4.x.

**Gate T10:** syntax, help, diagnostics, presentation, documentation, and the six
suite archive builds are coherent and reproducible.

---

# T11 — Differential validation, hostile input, and freeze

**Development version:** `1.4.0-Alpha-11`

## Objective

Prove release readiness. T11 is a validation/freeze tranche, not a feature
tranche.

## Differential validation

Developer-only fixture generation MAY use a pinned ncurses 6.6 environment.

Normal CI SHALL use checked-in fixtures and SHALL NOT require native `tic`,
`infocmp`, or `toe`.

### `tic`

For the supported overlapping source subset:

```text
same source -> ncurses tic -> Icod Runtime parse
same source -> Icod tic    -> Icod Runtime parse
                              |
                              v
                    semantic comparison
```

Semantic equivalence is required.

Byte-for-byte equality is required only where separately frozen.

### `infocmp`

Compare supported semantics for:

- acquisition;
- one-entry listing content;
- `-d`;
- `-c`;
- `-n`;
- explicit database selection;
- extended filtering;
- adopted width/sort behavior.

Do not require incidental ncurses comments/whitespace unless deliberately made
part of the Icod contract.

### `toe`

Use controlled conventional database trees and compare:

- canonical identities;
- descriptions;
- root selection;
- `-a` behavior;
- sorting;
- duplicate visibility;
- source dependency graphs where applicable.

Do not use the uncontrolled host database as release evidence.

## Hostile-input validation

Exercise:

```text
argument fuzzing
option-value boundaries
malformed UTF-8
very large source files
very long terminal names
path traversal attempts
malformed conventional directory trees
truncated compiled entries
oversized compiled entries
duplicate aliases
deep inheritance graphs
cycles
permission failures
cancellation
directory replacement races
```

All failures SHALL remain bounded and deterministic.

## API freeze

At T11:

- Runtime 1.0 baseline exact;
- Source 1.1 baseline exact;
- Compiler 1.2 baseline exact;
- historical Inspection 1.3 baseline exact;
- Inspection 1.4 baseline reviewed and frozen;
- no command-facing policy type leaks into a library public API.

## Artifact validation

Validate the pre-release artifact model:

```text
4 library .nupkg
4 library .snupkg
6 command suite archives
```

A checksum manifest SHALL cover all fourteen package/archive artifacts.

**Gate T11:** the complete suite is deterministic, cross-platform,
corpus-backed, hostile-input tested, API-frozen, and ready for release closure.

---

# 12. 1.4.0 release closure

T01-T11 constitute the planned 1.4 implementation program.

Release closure remains a separate finalization step.

Release closure SHALL:

- set Runtime, Source, Compiler, and Inspection versions to exactly `1.4.0`;
- make all three commands report exactly `1.4.0`;
- retain library assembly version `1.0.0.0`;
- preserve exact Runtime 1.0 API baseline;
- preserve exact Source 1.1 API baseline;
- preserve exact Compiler 1.2 API baseline;
- preserve the historical Inspection 1.3 baseline;
- freeze the reviewed Inspection 1.4 baseline;
- pass net8/net9/net10 API equivalence for all four libraries;
- pass Windows/Linux/macOS Release build and tests;
- pack and structurally validate the four NuGet packages and four symbol
  packages;
- run all four fresh package-reference-only consumers on all three library TFMs;
- execute command integration tests on Windows/Linux/macOS;
- build the six command suite archives;
- smoke `tic`, `infocmp`, and `toe` from unpacked release artifacts on matching
  CI operating systems;
- structurally validate non-native-architecture archives;
- verify the command bundles contain no development/project-reference paths;
- verify no command depends on an unpublished package version;
- verify the lower package dependency graph remains unchanged;
- verify `Icod.CommandFramework` appears only in the command layer;
- pass non-publishing `main` validation on the exact release commit;
- create and push immutable tag `v1.4.0`;
- require the tag workflow to repeat the complete Release gate;
- publish the four validated library packages to NuGet.org;
- publish the same four library packages to GitHub Packages;
- create the GitHub Release with:
  - four `.nupkg`;
  - four `.snupkg`;
  - six command archives;
  - `SHA256SUMS.txt`.

If that artifact model is retained, the final GitHub Release contains:

```text
15 assets
```

The NuGet trusted-publishing package scope remains the four library package IDs
unless T10 deliberately adds .NET tool packages.

Do not add new NuGet package IDs during release closure itself.

---

# 13. Explicit 1.4 non-goals

Version 1.4.0 SHALL NOT require:

- termcap source parsing;
- `captoinfo`;
- `infotocap`;
- full `tic -C` termcap conversion;
- full `infocmp -C` termcap conversion;
- BSD-strict termcap conversion;
- `TERMCAP` / `TERMPATH` acquisition;
- Berkeley DB / hashed terminfo stores;
- HP-UX/AIX/OSF historical binary formats;
- `infocmp -e` / `-E` C initializer generation;
- `infocmp -i` control-sequence explanation;
- `infocmp -u` relative-source synthesis;
- exact source comment/whitespace reproduction;
- ncurses trace-file compatibility;
- exact ncurses diagnostic wording;
- system package-manager installers;
- shell completion files;
- self-contained .NET runtime bundles;
- NativeAOT;
- code signing/notarization;
- command aliases which pretend Icod is the native ncurses implementation.

These can be added later when justified.

---

# 14. Relationship to 1.5

A successful 1.4 should make 1.5 clearly about **termcap interoperability** rather
than unfinished command basics.

Expected 1.5 work includes:

```text
termcap source parser
termcap unresolved model
tc= inheritance
TERMCAP / TERMPATH
termcap <-> terminfo conversion
captoinfo
infotocap
tic termcap translation modes
infocmp termcap translation modes
toe termcap dependency analysis
loss reporting
```

The 1.4 command projects should therefore be designed so new conversion modes
can be added without altering Runtime or rewriting command hosting.

---

# 15. Why the eleven-tranche plan is safer

The original seven-tranche plan described the right release destination, but
several tranches combined two different failure domains. The refined sequence
keeps the same scope while inserting review gates at the places where bugs would
otherwise become difficult to localize.

The critical separations are:

```text
discovery before enumeration
enumeration before toe
tic validation before filesystem writes
infocmp rendering before semantic comparison
toe database listing before source graph analysis
all command semantics before distribution/freeze
```

This means a failure in T04, for example, cannot be confused with a database
publication bug because publication does not exist until T05. Likewise T06 can
stabilize renderer controls without simultaneously debugging comparison
presentation.

The roadmap is therefore ambitious in destination but conservative in execution.

---

# 16. Recommended first implementation step

Create branch:

```text
1.4.0
```

from the exact published `v1.3.0` commit:

```text
7359eba4b5dffe8e69eda2fece4bd4cd8cdf5003
```

Begin with T01 only.

Do not begin by implementing `tic`.

The first patch should establish:

1. the 1.4 roadmap and pre-T01 contract audit;
2. coordinated `1.4.0-Alpha-1` versions;
3. the three command project shells;
4. the three command test projects;
5. net10.0 command / three-TFM library separation;
6. `Icod.CommandFramework` dependency direction;
7. minimal `Program` / testable `Command` execution shape;
8. exit status, stream, cancellation, encoding, and help/version conventions;
9. the 1.4 Inspection baseline file;
10. CI awareness of all new projects.

Only after T01 is green should T02 establish database-location discovery. T03 then adds catalog enumeration.

That ordering is important.

`toe` and the `-D` modes otherwise tempt us to duplicate Runtime discovery
policy inside command code. Freezing the reusable catalog seam first gives all
three commands the correct architectural foundation.

---

# 17. Completion definition

`Icod.TermInfo 1.4.0` is complete when a user with .NET 10 can obtain the
released command bundle and perform these workflows without native ncurses:

```text
# validate source
tic -c example.ti

# compile into an explicit database
tic -o ./terminfo example.ti

# inspect a terminal
infocmp xterm

# compare two terminals
infocmp -d xterm xterm-256color

# compare the same name from two databases
infocmp -d -A ./old -B ./new xterm

# enumerate a database
toe ./terminfo

# enumerate the normal search path
toe -a -s

# inspect use= dependencies
toe -u example.ti
toe -U example.ti
```

and when those commands are backed by the same public semantic engines used by
ordinary .NET consumers.

The desired end state is:

```text
source text
    |
    v
Icod.TermInfo.Source
    |
    +------> tic ------> Icod.TermInfo.Compiler ------> compiled database
    |
    v
TerminalDescription
    |
    +------> infocmp --> Icod.TermInfo.Inspection
    |
    +------> toe ------> database catalog / source dependency inspection
    |
    v
Icod.TermInfo Runtime acquisition
```

No native `libtinfo`, `tic`, `infocmp`, or `toe` is required.

---

# 18. Audit basis

This roadmap was prepared after publication of `v1.3.0` from the current
repository and ecosystem state, including review of:

- the published `Icod.TermInfo 1.3.0` GitHub Release;
- the `v1.3.0` tag and release commit;
- current `README.md`;
- `Icod.TermInfo-Post-1.0-Development-Roadmap.md`;
- the 1.3 Inspection roadmap and release closure;
- `docs/VERSIONING.md`;
- `docs/COMPATIBILITY.md`;
- `docs/RELEASING.md`;
- `.github/workflows/release.yaml`;
- `SystemTerminalDescriptionProvider`;
- `DirectoryTerminalDescriptionProvider`;
- the Runtime friend-assembly configuration;
- the Source capability-classification model;
- the 1.3 Inspection public surface;
- `Icod.CommandFramework 2.0.0`;
- `Icod.CommandFramework.Diagnostics.CommandExitCodes`;
- existing Icod executable-project conventions in `Icod.ProcPs`;
- contemporary ncurses 6.6 `tic(1m)`, `infocmp(1m)`, and `toe(1m)` reference
  behavior.

The ncurses material is used as an interoperability reference only. The roadmap
does not incorporate ncurses source code and does not claim complete
option-for-option compatibility.
