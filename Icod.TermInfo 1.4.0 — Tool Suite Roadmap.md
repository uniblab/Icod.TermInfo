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
**Inspection API contract:** frozen at 1.3  
**New commands:** `tic`, `infocmp`, `toe`  
**Development branch:** `1.4.0`  
**Development sequence:** `1.4.0-Alpha-1` through `1.4.0-Alpha-7`  
**Status:** planned  
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

Exact public shape should be frozen only when T04 demonstrates the real command
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

## 7.7 Candidate T06 additions

If the core command is stable early, 1.4 MAY add:

```text
-Q1   emit compiled entry as hexadecimal
-Q2   emit compiled entry as base64
-Q3   emit both
-v[n] controlled verbose progress
```

These are not allowed to delay T03 completion.

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

# T01 — Tool-suite foundation and command contract

**Development version:** `1.4.0-Alpha-1`

Create the command/test/release foundation before implementing command
semantics.

Required work:

- cut `1.4.0` from the published `v1.3.0` release commit;
- add this 1.4 roadmap;
- add `docs/1.4.0-PRE-T01-CONTRACT-AUDIT.md`;
- advance all four library package versions to `1.4.0-Alpha-1`;
- retain library `AssemblyVersion` `1.0.0.0`;
- create `tic`, `infocmp`, and `toe` executable projects;
- create all three command test projects;
- target commands at `net10.0`;
- reference `Icod.CommandFramework 2.0.0` only from command projects;
- establish lowercase command assembly names;
- establish minimal `Program` + injected-stream `Command.RunAsync` pattern;
- establish exit-code contract;
- establish help/version conventions;
- establish UTF-8 command I/O policy;
- add command projects/tests to the solution;
- add `docs/1.4.0-INSPECTION-PUBLIC-API-BASELINE.txt`, initially equal to the
  released 1.3 Inspection surface;
- update versioning/compatibility/future-work documentation for the 1.4 line;
- update CI so all command projects build and tests run on Windows, Linux, and
  macOS;
- do not yet publish command artifacts.

**Gate T01:** all three minimal commands build on the three CI operating systems,
respond correctly to `--help`/`--version`, honor injected streams and
cancellation, and no lower-level package acquires a command-framework or command
dependency.

---

# T02 — Database discovery and catalog inspection

**Development version:** `1.4.0-Alpha-2`

Provide the reusable database-inspection mechanism needed by all three commands.

Required work:

- add a narrow Runtime -> Inspection friend-assembly seam;
- preserve the exact Runtime 1.0 public baseline;
- expose approved Inspection database-location types/API;
- expose approved conventional-directory catalog API;
- ensure database-location discovery shares Runtime's actual snapshot logic;
- represent encoded `TERMINFO` distinctly from directory sources;
- preserve platform-specific path comparison semantics;
- enumerate literal and hexadecimal conventional layouts;
- parse entries through Runtime rather than trusting filenames;
- retain canonical names, aliases, descriptions, and source location/path;
- deterministic ordering;
- cancellation;
- resource limits;
- link/reparse safety;
- explicit unsupported hashed-database diagnostics;
- update the 1.4 Inspection API baseline only for reviewed additions;
- add package-smoke coverage for the new Inspection API on net8/net9/net10.

Testing SHALL include synthetic discovery snapshots for:

```text
TERMINFO directory
TERMINFO encoded
user .terminfo
TERMINFO_DIRS
empty TERMINFO_DIRS components
duplicate roots
Linux defaults
macOS defaults
Windows with no implicit Unix roots
relative environment paths
missing roots
permission failure
malformed entries
literal layout
hex layout
duplicate aliases
```

**Gate T02:** a caller can inspect the exact ordered Icod database-location model
and enumerate conventional roots without adding Runtime public API or duplicating
system-discovery rules.

---

# T03 — `tic`

**Development version:** `1.4.0-Alpha-3`

Implement the first complete command.

Required behavior:

- UTF-8 file input;
- `-` standard input;
- `-c`;
- `-o`;
- safe default destination policy;
- `-e`;
- `-x`;
- `-s`;
- `-D`;
- `-V` / `--version`;
- `--help`;
- explicit overwrite/`--force` policy;
- Source diagnostics with source location;
- cancellation;
- deterministic exit statuses;
- no native `tic` invocation.

Tests SHALL cover:

- one valid entry;
- multiple entries;
- forward/backward `use=`;
- cancellations;
- standard capabilities;
- known extended capabilities;
- unknown extended with/without `-x`;
- malformed source;
- missing parent;
- cycle;
- check-only mode;
- entry selection;
- no selected match;
- explicit output root;
- existing destination rejection;
- explicit overwrite;
- standard input;
- malformed UTF-8;
- encoded `TERMINFO` not used as writable destination;
- Windows destination behavior;
- Linux/macOS user destination behavior;
- cancellation during work.

Integration acceptance:

```text
.ti source
    -> tic
    -> conventional database tree
    -> DirectoryTerminalDescriptionProvider
    -> semantic equality
```

**Gate T03:** the managed `tic` command can validate and compile mainstream
terminfo source into a database which the released Runtime acquisition path can
load without semantic loss.

---

# T04 — `infocmp`

**Development version:** `1.4.0-Alpha-4`

Expose the 1.3 Inspection engine as a real diagnostic command.

Required behavior:

- TERM fallback;
- one-terminal canonical source listing;
- first-versus-rest default comparison;
- `-d`;
- `-c`;
- `-n`;
- `-A`;
- `-B`;
- `-q`;
- `-0`;
- `-1`;
- `-w`;
- `-s`;
- `-x`;
- `-D`;
- `-V` / `--version`;
- `--help`;
- deterministic output;
- success exit when semantic differences are found.

If configurable rendering requires new Inspection API:

- add options/overloads;
- preserve all 1.3 overload behavior;
- review/freeze additions in the 1.4 Inspection baseline.

Tests SHALL include:

- no operand with TERM;
- missing TERM;
- built-in and system providers;
- explicit `-A`/`-B` roots;
- same name in two different roots;
- alias acquisition;
- metadata-only differences;
- Boolean/numeric/string differences;
- extended capability differences;
- extended kind mismatch;
- standard absent-from-all;
- width boundaries;
- one-line and one-capability-per-line modes;
- every sort mode;
- culture variation;
- insertion-order variation;
- redirected stdout/stderr;
- cancellation.

**Gate T04:** common `infocmp` listing and comparison workflows are implemented
entirely through managed Runtime/Inspection APIs with deterministic output.

---

# T05 — `toe`

**Development version:** `1.4.0-Alpha-5`

Implement conventional terminfo catalog enumeration and source dependency
reports.

Required behavior:

- explicit directory operands;
- default discovered directory;
- `-a`;
- `-h`;
- `-s`;
- `-u`;
- `-U`;
- `-V` / `--version`;
- `--help`;
- canonical names/descriptions;
- deterministic duplicates;
- semantic duplicate comparison where reported;
- per-entry error continuation with final failure status;
- no arbitrary recursive traversal.

Tests SHALL include:

- empty directory;
- one root;
- several roots;
- literal/hex layouts;
- duplicate names;
- aliases;
- same semantic entry in multiple roots;
- different semantic entry with same name;
- malformed compiled entry;
- oversized entry;
- permission error;
- symlink/reparse edge cases;
- normal system-discovery ordering from a synthetic snapshot;
- forward source dependencies;
- reverse source dependencies;
- cycles in source dependency graph;
- duplicate source identities;
- termcap input rejected as unsupported in 1.4.

**Gate T05:** `toe` can reliably enumerate supported conventional databases and
analyze `.ti` `use=` dependencies without native ncurses or private filesystem
guesswork.

---

# T06 — CLI compatibility, presentation, and distribution

**Development version:** `1.4.0-Alpha-6`

Harden the three commands as products rather than merely passing engine tests.

Required work:

## Command-line compatibility

- clustered short-option parsing where unambiguous;
- attached option values where the adopted syntax permits them;
- `--` end-of-options handling;
- exact operand-count validation;
- stable help output;
- stable version output;
- explicit diagnostics for unsupported ncurses switches;
- no silently ignored options;
- no accidental option interpretation of source path operands after `--`.

## Presentation

- deterministic headings;
- stable diagnostic prefixes;
- stable source-location formatting;
- no culture-sensitive numeric output;
- no environment-dependent column order;
- redirected output tests;
- narrow/wide output tests;
- no ANSI styling in redirected output unless an explicit future option enables
  it.

## Candidate options

Add only if the core commands remain stable:

```text
tic      -Q1/-Q2/-Q3, -v[n]
toe      -v[n]
infocmp  -Q1/-Q2/-Q3 only if a command-layer Compiler dependency is reviewed
```

## Distribution decision

For 1.4.0, the canonical library distribution remains NuGet.

The canonical command distribution SHOULD be framework-dependent .NET 10
release bundles rather than self-contained runtime copies.

Produce six suite archives:

```text
Icod.TermInfo.Tools.<version>.win-x64.zip
Icod.TermInfo.Tools.<version>.win-arm64.zip
Icod.TermInfo.Tools.<version>.linux-x64.tar.gz
Icod.TermInfo.Tools.<version>.linux-arm64.tar.gz
Icod.TermInfo.Tools.<version>.osx-x64.tar.gz
Icod.TermInfo.Tools.<version>.osx-arm64.tar.gz
```

Each archive contains all three commands and their managed dependencies.

The commands require an installed .NET 10 runtime.

The archive build SHALL verify that the three command apphosts report the same
suite version.

### NuGet tool packaging

Publishing `tic`, `infocmp`, and `toe` as .NET global-tool packages is useful but
SHOULD NOT be a 1.4.0 release blocker.

The SDK's ordinary tool packaging is centered on a tool command name per package,
and project-reference packaging introduces an additional dependency/distribution
problem for commands developed in the same repository as their libraries.

If a clean three-tool NuGet packaging design is proven during T06 without:

- duplicating library payloads improperly;
- weakening local development;
- publishing mismatched prerelease dependencies;
- complicating trusted-publishing safety;

it MAY be added.

Otherwise defer global-tool packages to 1.4.x.

**Gate T06:** all supported syntax and output contracts are documented, the
three commands behave consistently on all three operating systems, and the six
release bundles can be produced reproducibly.

---

# T07 — Differential validation, hostile inputs, and freeze

**Development version:** `1.4.0-Alpha-7`

Close the implementation program with independent evidence.

## 11.1 Pinned differential corpus

Extend the existing ncurses-derived fixture philosophy.

Developer-only fixture generation MAY use a pinned ncurses 6.6 environment.

Check in enough provenance to reproduce the reference data.

Normal CI SHALL consume checked-in fixtures and SHALL NOT require host `tic`,
`infocmp`, or `toe`.

## 11.2 `tic` differential evidence

For the supported source subset:

- compile the same sources with pinned ncurses and Icod where representations
  overlap;
- parse both compiled results through Icod Runtime;
- compare semantic `TerminalDescription` values;
- do not require byte-for-byte equality unless a representation has explicitly
  been frozen to that exact encoding;
- verify Icod database paths are consumable through its normal provider.

## 11.3 `infocmp` differential evidence

For supported modes:

- compare canonical capability identity/value results;
- compare selected `-d`, `-c`, and `-n` semantics;
- compare explicit database selection behavior;
- compare TERM fallback;
- compare extended-capability filtering;
- compare width/sort behavior only where Icod deliberately adopts the same
  contract.

Do not claim exact ncurses comments, whitespace, or incidental punctuation
unless specifically frozen by tests.

## 11.4 `toe` differential evidence

Use controlled temporary conventional directory trees.

Compare:

- canonical names;
- descriptions;
- directory selection;
- `-a`;
- sort semantics;
- duplicate visibility;
- source dependency graph results where applicable.

Do not use the host's uncontrolled system terminfo tree as release evidence.

## 11.5 Hostile input

Fuzz or systematically mutate:

```text
command arguments
option-value boundaries
UTF-8 source bytes
very large source files
very long names
path traversal attempts
malformed conventional directory layouts
compiled entry truncation
oversized entry files
duplicate aliases
source inheritance graphs
cancelled operations
permission failures
directory replacement races
```

All failures must remain bounded and deterministic.

## 11.6 Public API freeze

At T07:

- Runtime 1.0 baseline exact;
- Source 1.1 baseline exact;
- Compiler 1.2 baseline exact;
- Inspection 1.3 baseline retained as historical;
- Inspection 1.4 baseline reviewed and frozen;
- no command-facing type leaks accidentally into a library public API.

## 11.7 Release artifact validation

Validate:

```text
4 library .nupkg
4 library .snupkg
6 command suite archives
```

The checksum manifest should cover all fourteen binary/package artifacts.

**Gate T07:** the complete tool suite is deterministic, cross-platform,
corpus-backed, bounded under hostile input, package-valid, and ready for
`1.4.0` release closure.

---

# 12. 1.4.0 release closure

T01-T07 constitute the planned 1.4 implementation program.

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
unless T06 deliberately adds .NET tool packages.

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

# 15. Recommended first implementation step

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

Only after T01 is green should T02 establish database catalog/discovery.

That ordering is important.

`toe` and the `-D` modes otherwise tempt us to duplicate Runtime discovery
policy inside command code. Freezing the reusable catalog seam first gives all
three commands the correct architectural foundation.

---

# 16. Completion definition

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

# 17. Audit basis

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