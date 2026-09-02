# Icod.TermInfo Compatibility Policy

This document defines the supported 1.x compatibility boundary for
`Icod.TermInfo`, the optional `Icod.TermInfo.Source` and
`Icod.TermInfo.Compiler` packages, beginning with 1.3 the optional
`Icod.TermInfo.Inspection` package, and beginning with 1.6 the optional
`Icod.TermInfo.Termcap` package.

## Supported target frameworks

The frozen 1.0 and 1.1 package lines support:

```text
net8.0
net10.0
```

Beginning with 1.2.0, every package in the coordinated family supports:

```text
net8.0
net9.0
net10.0
```

For 1.2 and later, all three target frameworks are first-class package targets.
Release validation requires equivalent public API manifests between target
frameworks and fresh-package execution for each target for every package present
in that release.

Beginning with T01 in 1.4, the `tic`, `infocmp`, and `toe` command layer targets
`net10.0`. Beginning with 1.5, the `icod-terminfo` router also targets `net10.0`.
TC07 in 1.6 adds `captoinfo` and `infotocap` as additional `net10.0` command
projects. These command-host choices do not remove `net8.0` or `net9.0` from any
reusable TermInfo library package.

Dropping a supported target framework is considered a breaking support-contract
change and normally requires a new major version.

## Supported host families

The repository validates on:

```text
Windows
Linux
macOS
```

The package family is predominantly managed and platform-neutral. Narrow
platform-specific runtime functionality, such as Windows virtual-terminal mode
enablement, remains explicitly isolated and must fail gracefully when it is not
applicable.

Support means a package is expected to operate on platform/runtime combinations
supported by the corresponding .NET target. It does not promise every historical
OS release.

## Public API compatibility

The runtime 1.0 public API is frozen by
`docs/1.0.0-PUBLIC-API-BASELINE.txt` and its semantic surface tests.

The Source 1.1 public API is independently frozen by
`docs/1.1.0-SOURCE-PUBLIC-API-BASELINE.txt` and its source-contract tests.

The Compiler 1.2 public API is frozen through
`docs/1.2.0-COMPILER-PUBLIC-API-BASELINE.txt` and its compiler-contract tests.

The Inspection 1.3 public API is independently frozen by
`docs/1.3.0-INSPECTION-PUBLIC-API-BASELINE.txt` and Inspection contract tests.
I01 started with an empty public surface, I02-I06 established the reviewed API,
and I07 froze that contract for release.

The frozen 1.4 Inspection baseline is
`docs/1.4.0-INSPECTION-PUBLIC-API-BASELINE.txt`. T01 began from the frozen 1.3
surface; the reviewed T02/T03 database-inspection additions and T06 renderer
controls were added compatibly and frozen at 1.4.0. Patch release 1.4.1 reuses
that baseline unchanged rather than creating a new API contract.

RS08 freezes the additive 1.7 Inspection public API in
`docs/1.7.0-INSPECTION-PUBLIC-API-BASELINE.txt`. Relative-source synthesis adds
only `TerminalDescriptionSourceSynthesisParent`,
`TerminalDescriptionSourceSynthesisOptions`, and
`TerminalDescriptionSourceSynthesizer` to the already-frozen 1.4 Inspection
surface. The 1.3 and 1.4 baselines remain immutable historical records.

RP08 freezes the additive 1.8 Inspection public API in
`docs/1.8.0-INSPECTION-PUBLIC-API-BASELINE.txt`. Planning adds only
`TerminalDescriptionSourcePlan`, `TerminalDescriptionSourcePlanner`,
`TerminalDescriptionSourcePlanningOptions`, and
`TerminalDescriptionSourcePlanningScore` to the frozen 1.7 Inspection surface.
The 1.7 synthesis types and every earlier Inspection baseline remain unchanged.

TC08 freezes the 1.6 Termcap public API in
`docs/1.6.0-TERMCAP-PUBLIC-API-BASELINE.txt`. Release verification requires the
full `PublicApiSnapshot/v1` reflection-manifest SHA-256
`1e24b8a555b506594c58cf58d03bf87b2b60192f6316537cb4200498c6a92ab0`, exact compiled-assembly API equivalence across net8/net9/net10, and the
packaged XML documentation member-ID inventory recorded by the same baseline.

Within 1.x:

- existing public signatures remain source/binary compatible;
- enum names and numeric values remain stable;
- nullability and optional/default parameter contracts are treated as public
  contract;
- additions must be compatible and documented;
- behavior changes must preserve documented semantic contracts unless they
  correct an acknowledged defect.

Runtime, Source, Compiler, Inspection, and Termcap assemblies retain version `1.0.0.0`
and remain unsigned throughout 1.x.

## 1.7 relative-source synthesis compatibility

Inspection 1.7 may synthesize deterministic terminfo source for an effective
`TerminalDescription` relative to an explicit ordered parent list. The caller's
parent order and exact `UseName` spelling are semantic inputs. Parent aggregation
follows the existing Source resolver precedence, required inherited removals are
rendered as cancellations, and extended capability names remain ordinal and
case-sensitive.

The stable command adapter is `infocmp -u target parent [parent ...]`; `-A`
selects the target database and `-B` the parent database. `-c -u` is the frozen
ncurses-compatible synonym, while `-d -u`, `-n -u`, and `-q -u` remain usage
errors. The command does not duplicate synthesis semantics.

The production Inspection package continues to depend only on Runtime and Source.
Compiler and ncurses are verification references only.

## Runtime terminfo semantic compatibility

The stable runtime responsibility includes:

- immutable terminal descriptions;
- complete standard and extended capability representation;
- signed 32-bit numeric semantics;
- reversible 8-bit compiled capability strings;
- parameter expansion;
- padding-aware output;
- built-in terminal profiles;
- conventional compiled terminfo parsing;
- explicit directory acquisition;
- deterministic environment/user/system discovery;
- provider-local caching and explicit new-provider refresh;
- explicit provider composition.

The supported compiled family is the frozen conventional System V/ncurses
contract documented by the 0.9 acquisition records: legacy `0432`, ncurses
extended sections, and `01036` 32-bit numerics.

## Source-language compatibility

`Icod.TermInfo.Source` 1.1 adds the optional source-language path:

- `.ti` lexical analysis and source locations;
- deterministic diagnostics;
- Boolean, numeric, string, cancellation, and `use=` source forms;
- standard and extended capability classification;
- unresolved source documents and entries;
- bounded inheritance resolution with cycle and depth diagnostics;
- deterministic duplicate source-name/alias warnings;
- materialization into the existing immutable `TerminalDescription` model.

The Source package does not redefine runtime capability semantics. A resolved
source entry is required to enter the same runtime model used by compiled
acquisition.

## Compiler compatibility

`Icod.TermInfo.Compiler` 1.2 adds the optional compiled-output path without
moving compiler responsibilities into the runtime package.

The compiler contract includes:

- deterministic conventional compiled-entry writing;
- legacy `0432` output;
- `01036` wide-numeric output;
- ncurses extended sections;
- standard ordering through the runtime capability catalog;
- strict reversible Latin-1 byte semantics;
- checked count, offset, and total-size arithmetic;
- explicit representation failure rather than silent truncation;
- source compilation through the existing Source parser/resolver;
- controlled conventional database-layout output;
- semantic round-trip validation through the existing runtime parser.

The low-level binary writer is pure. It does not read environment variables,
discover system databases, invoke native ncurses tools, or write filesystem
layouts. Filesystem output belongs to the later database-layout layer.

`TerminalDescription` represents effective runtime state and does not retain
source cancellation tombstones. A writer receiving only a
`TerminalDescription` therefore emits absence for absent capabilities and does
not invent cancellation.

Compiled output is byte-oriented. Identity strings, capability names, and
capability values which cannot be represented under the selected conventional
format fail deterministically. The compiler does not silently replace Unicode,
truncate numeric values, wrap offsets, or synthesize missing identity metadata.

For deterministic output, standard capabilities use canonical binary metadata
and extended capability names are ordered ordinally within their value kinds.

## Inspection compatibility

`Icod.TermInfo.Inspection` 1.3 is the optional human-readable inspection and
semantic-comparison layer. It is deliberately separate from Runtime, Source, and
Compiler so those frozen public contracts do not acquire tooling-oriented API.

The 1.3 architectural contract distinguishes two domains:

- effective inspection/comparison over `TerminalDescription`;
- source-aware inspection/comparison over unresolved Source models.

Effective inspection SHALL NOT invent `use=` relationships, cancellation
tombstones, duplicate-source history, comments, or provenance that
`TerminalDescription` does not retain. Source-aware operations SHALL preserve
field order where order is semantically significant and likewise shall not
invent source information the parsed model does not retain.

The released 1.3 contract includes canonical effective rendering, normalized
unresolved-source rendering, structured effective and source-aware comparison,
and provider-aware inspection orchestration. Those behaviors are frozen through
the independent Inspection baseline and its semantic tests.

Beginning with T02 in the 1.4 line, Inspection additionally exposes a read-only
snapshot of the ordered system database locations considered by Runtime
discovery. The API distinguishes encoded `TERMINFO`, directory `TERMINFO`, the
user database, `TERMINFO_DIRS`, and final platform defaults. Encoded payload bytes
are not exposed. Directory paths are normalized, Runtime precedence and
platform-specific duplicate handling are preserved, and no database contents are
enumerated until the separate T03 catalog tranche. Runtime public API remains
unchanged.

Beginning with T03, Inspection can also enumerate one explicit conventional
terminfo directory without changing Runtime provider semantics. Enumeration is
limited to immediate literal first-character and two-digit hexadecimal
subdirectories, parses candidate bytes through `CompiledTermInfoParser`, applies
the configured Runtime parser size limit, preserves physical paths and parsed
terminal identity, reports duplicate canonical identities deterministically, and
retains malformed/I/O/link/placement issues instead of silently discarding them.
Arbitrary recursion and hashed/Berkeley DB parsing remain outside the contract.

## 1.6 termcap interoperability compatibility

Beginning with TC01, `Icod.TermInfo.Termcap` is a fifth coordinated reusable
package targeting `net8.0`, `net9.0`, and `net10.0`. It depends only on Runtime;
no existing reusable package acquires a Termcap dependency.

TC01-TC06 establish bounded conventional termcap parsing, Runtime-derived
two-character capability classification, bounded `tc=` inheritance resolution,
explicit semantic conversion to `TerminalDescription`, deterministic reverse
representability/rendering, and opt-in `TERMCAP` / `TERMPATH` acquisition.
Termcap acquisition remains separate from Runtime `TERMINFO` discovery.

TC07 composes those engines into `net10.0` `captoinfo` and `infotocap` commands.
Both commands emit effective resolved state rather than reconstructing source
history. `captoinfo` composes Termcap with Inspection's effective terminfo source
renderer; `infotocap` composes Source with the Termcap reverse renderer.
Representational loss and incompatibility remain explicit diagnostics.

TC08 freezes the active 1.6 Termcap public API and package graph without adding
new semantics. Checked-in BSD/GNU-style corpus tests, hostile-input and bounded
seeded mutation tests, package-structure verification, and isolated package-only
consumers become normal release evidence. Runtime, Source, Compiler, Inspection,
`tic`, `infocmp`, `toe`, `captoinfo`, `infotocap`, and router semantics are not
reopened by the freeze.

## T04 `tic` validation compatibility

Beginning with T04 in the 1.4 line, the `net10.0` `tic` command exposes a
non-mutating validation path over the already-frozen Source and Compiler engines.
`tic -c` reads one strict UTF-8 source document from a file or standard input,
parses the complete document, preserves Source diagnostic codes and locations,
optionally selects canonical names or aliases through `-e`, resolves each selected
entry and its `use=` graph, and performs compiled representability checks through
`CompiledTermInfoWriter` entirely in memory.

Without `-x`, selected entries and their reachable parents may use standard and
known extended capabilities, but a syntactically valid capability classified by
Source as `UnknownExtended` is a command error. `-x` permits those unknown
extensions to flow through the existing Source/Compiler semantic model. Source
parser errors anywhere in the supplied document remain errors even when `-e`
selects only a subset, because T04 parses the complete source before selection.
Resolver/representation validation is limited to selected entries and the parents
needed by their inheritance graphs.

T04 adds no public Runtime, Source, Compiler, or Inspection API. It does not call
`CompiledTermInfoDatabaseWriter`, create terminfo database directories, or publish
compiled entries.

## T05 `tic` publication compatibility

Beginning with T05, omitting `-c` after successful source validation publishes the
selected effective terminal descriptions through the existing frozen
`CompiledTermInfoDatabaseWriter`. `-o` chooses an explicit conventional database
root. Without `-o`, command policy considers only directory-valued `TERMINFO`, then
the Runtime-defined user database. Encoded `TERMINFO`, `TERMINFO_DIRS`, and
platform-default/system roots are never selected implicitly for writes.

Existing destinations are rejected by default. `--force` maps to the Compiler
writer's existing explicit overwrite option, while `-s` reports the normalized
output root, selected entry count, and warning count on standard error. The command
does not duplicate Compiler path derivation, alias publication, preflight, staging,
reparse/link rejection, or final move/replace behavior.

The frozen Compiler writer is synchronous, so T05 checks cancellation before the
publication transaction begins and then treats the writer call as an indivisible
commit boundary. T05 does not change Runtime, Source, Compiler, or Inspection
public API.

## T06 `infocmp` rendering compatibility

T06 makes `infocmp` operational for zero/one-terminal inspection. Normal
acquisition uses `SystemTerminalDescriptionProvider`; `-A` uses an explicit
`DirectoryTerminalDescriptionProvider` without mutating process discovery
environment. A clean provider miss remains distinguishable from malformed data or
other provider failures.

The additive `TerminalDescriptionSourceRendererOptions`,
`TerminalDescriptionSourceLayout`, and
`TerminalDescriptionSourceCapabilityOrder` contracts provide reusable layout,
wrapping, ordering, and extended-capability filtering. Existing 1.3
`TerminalDescriptionSourceRenderer.Render(TerminalDescription)` and
`Write(TextWriter, TerminalDescription)` output is unchanged. A parameterless
options instance selects that same frozen policy.

Standard-capability ordering is ordinal and deterministic within Boolean, numeric,
and string groups. `infocmp` defaults to standard capabilities and requires `-x`
to include effective extended capabilities. This filtering changes presentation
only; it never mutates the acquired `TerminalDescription`. T06 adds no Runtime,
Source, or Compiler public API.

## T07 `infocmp` comparison compatibility

T07 extends `infocmp` to two or more terminal operands. The first terminal is
compared with each subsequent terminal. With no explicit `-d`, `-c`, or `-n`
selector, comparison defaults to semantic differences. `-A` selects the first
terminal database and `-B` selects the database used for subsequent terminals;
neither option mutates process environment variables.

Difference mode delegates to the frozen `TerminalDescriptionComparer`; the
command does not parse rendered source to determine equality. Differences are
successful command output and return status 0. Common-capability reporting uses
the already-acquired immutable descriptions and Runtime capability metadata.
Absent-capability reporting is defined only over the closed standard capability
catalog and therefore does not invent absent extended names. `-q` changes
presentation only. T07 adds no Runtime, Source, Compiler, or Inspection public
API.

## Discovery and failure compatibility

Runtime discovery precedence, clean-miss behavior, parser failures,
I/O/permission propagation, terminal-name validation, and provider-local
cache/refresh rules are part of the compatibility contract.

For source resolution, a clean `ITermInfoSourceEntryProvider` miss becomes a
source diagnostic. Provider failures propagate. Resolver diagnostics and
duplicate-identity lookup remain deterministic and ordinal/case-sensitive.

`TerminalDatabase.BuiltIn` remains environment-independent and I/O-free.

## Package compatibility

Beginning with 1.2, `Icod.TermInfo` contains managed/XML assets and portable
symbols for all three supported target frameworks. It has no runtime NuGet
dependency and no native ncurses/terminfo payload.

`Icod.TermInfo.Source` likewise contains corresponding three-target managed/XML
and symbol assets and depends on the matching `Icod.TermInfo` package. The
dependency direction is one-way: `Icod.TermInfo` never depends on Source.

Beginning with 1.2, `Icod.TermInfo.Compiler` contains corresponding three-target
managed/XML and symbol assets. It depends directly on the matching runtime
package and may depend on the matching Source package for source compilation.
Runtime and Source never depend on Compiler.

Beginning with 1.3, `Icod.TermInfo.Inspection` contains corresponding three-target
managed/XML and symbol assets and depends directly on the matching Runtime and
Source packages. Inspection does not depend on Compiler. Runtime, Source, and
Compiler do not depend on Inspection.

Beginning with 1.6, `Icod.TermInfo.Termcap` contains corresponding three-target
managed/XML and symbol assets and depends only on the matching Runtime package.
No existing reusable package depends on Termcap.

Beginning with 1.4, the command executables sit above this package family. They
may use `Icod.CommandFramework` and the appropriate TermInfo libraries, but no
dependency flows back from Runtime, Source, Compiler, Inspection, or Termcap
into the command layer. The command projects remain non-packable and are
distributed together as six framework-dependent .NET 10 suite archives.

Beginning with 1.5, `Icod.TermInfo.Tools` is a distribution-only .NET tool
package. Its `icod-terminfo` router may reference `tic`, `infocmp`, and `toe` to
dispatch to their existing `Command.RunAsync` entry points. The three semantic
commands still do not reference one another, and the router introduces no
terminfo semantics of its own. Archive distribution remains independent and
continues to expose the traditional command names directly.

Beginning with 1.6, the router and archives additionally expose `captoinfo` and
`infotocap`. All five command projects remain mutually independent; the router
is the only project which references command implementations for dispatch.

The same validated registry package artifacts for a release are used for
NuGet.org and GitHub Packages.

## Explicit non-goals

The reusable `Icod.TermInfo` package family does not promise:

- Berkeley DB/hashed terminfo stores;
- divergent undocumented vendor binary dialects;
- live raw/cooked terminal session ownership;
- input-event decoding or active probing;
- PTY/ConPTY lifecycle;
- curses/virtual-screen behavior;
- terminal emulation or graphics protocols.

The 1.4 line provides the managed `tic`, `infocmp`, and `toe` command
applications. Version 1.5 adds the `Icod.TermInfo.Tools` installation router.
Version 1.6 adds the optional Termcap package plus `captoinfo` and `infotocap`
without reopening the frozen 1.4 mainstream terminfo command semantics.
Exhaustive ncurses option compatibility is not claimed.

The remaining non-goals stay outside the reusable TermInfo package family and
belong to later or sibling-system work. Current post-1.0 planning is governed by
`../Icod.TermInfo-Post-1.0-Development-Roadmap.md`; the old
`FUTURE-WORK-INVENTORY.md` is retained only as a retired historical document.
