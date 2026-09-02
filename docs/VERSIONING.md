# Icod.TermInfo Versioning Policy

The `Icod.TermInfo` package family follows Semantic Versioning for its public
package contracts.

## Package versions

The NuGet packages use:

```text
MAJOR.MINOR.PATCH
```

Development tranches use the repository's established prerelease form, such as
`1.4.0-Alpha-1`, `1.4.0-Alpha-2`, and later `-Beta-X` / `-RC-X` forms when
needed.

For the 1.x line:

- patch releases correct defects without intentionally changing the supported
  public contract;
- minor releases may add compatible public API, capability/profile data, or
  optional sibling packages;
- removal, incompatible signature changes, incompatible enum-value changes, or
  deliberate semantic-contract breaks require a new major version.

Beginning with 1.1.0, `Icod.TermInfo` and `Icod.TermInfo.Source` advance
together. In each project, `<Version>` and `<PackageVersion>` must be identical,
and the package versions of the two projects must match.

Beginning with C01 in the 1.2.0 development line,
`Icod.TermInfo.Compiler` joins the coordinated package family. From that point
forward, Runtime, Source, and Compiler carry the same package version. The
C01-C07 development sequence is `1.2.0-Alpha-1` through `1.2.0-Alpha-7`.

Beginning with I01 in the 1.3.0 development line,
`Icod.TermInfo.Inspection` joins the coordinated package family. Runtime, Source,
Compiler, and Inspection SHALL all carry the same package version for every I01-I07
development tranche and final release. The I01-I07 development sequence is
`1.3.0-Alpha-1` through `1.3.0-Alpha-7`.

Beginning with T01 in the 1.4.0 development line, the four library packages
continue to advance together. The `tic`, `infocmp`, and `toe` command projects
carry the matching 1.4 development version for command identity, but T01 keeps
them non-packable executables rather than adding three new coordinated NuGet
package IDs. The command layer targets `net10.0` because it uses
`Icod.CommandFramework 2.0.0`; this does not reduce the library package family
from its `net8.0` / `net9.0` / `net10.0` targets.

Patch release 1.4.1 advances all four package versions and all three command
versions together. It corrects release-facing documentation and metadata only;
it does not create a new public API baseline or change the frozen 1.4.0 command
semantics.

Beginning with 1.5.0, `Directory.Build.props` contains the single
`IcodTermInfoSuiteVersion` release-version authority. Runtime, Source, Compiler,
Inspection, `tic`, `infocmp`, `toe`, and the `Icod.TermInfo.Tools` router consume
that property rather than carrying independent current-version literals. The
router is a packable .NET tool with command name `icod-terminfo`; the three
semantic command projects remain non-packable. The router joins the coordinated
registry package set without changing any reusable-library assembly identity or
frozen public API baseline.

Beginning with TC01 in the 1.6.0 development line, `Icod.TermInfo.Termcap` joins
the coordinated reusable package family. It targets `net8.0`, `net9.0`, and
`net10.0`, consumes the centralized suite version, retains assembly version
`1.0.0.0`, and depends only on Runtime. Existing reusable packages do not acquire
a Termcap dependency. Stable publication of the new package ID and its final API
baseline are release-closure work for the 1.6 line.

TC02 advances the coordinated development version to `1.6.0-Alpha-2` and adds
public termcap capability-mapping and classification APIs only to
`Icod.TermInfo.Termcap`. Runtime, Source, Compiler, and Inspection retain their
frozen public API baselines. The Termcap public surface remains a development
contract until the 1.6 release-closure freeze.

TC03 advances the coordinated development version to `1.6.0-Alpha-3` and adds
termcap-specific bounded `tc=` resolution, cancellation, provider lookup, and
source-provenance APIs only to `Icod.TermInfo.Termcap`. The resolver does not add
a Source dependency and does not alter any frozen Runtime, Source, Compiler, or
Inspection public API baseline.

TC04 advances the coordinated development version to `1.6.0-Alpha-4` and adds
resolved-termcap semantic conversion APIs only to `Icod.TermInfo.Termcap`. The
converter materializes the existing Runtime `TerminalDescription` model directly,
preserves representable unmapped fields through Runtime extended capabilities,
and does not add a Source dependency or alter any frozen Runtime, Source,
Compiler, or Inspection public API baseline.

TC05 advances the coordinated development version to `1.6.0-Alpha-5` and adds
Runtime-to-termcap representability and deterministic reverse-rendering APIs only
to `Icod.TermInfo.Termcap`. The renderer consumes the existing Runtime model and
TC02 mapping metadata directly, does not add a Source dependency, performs no
environment or filesystem acquisition, and does not alter any frozen Runtime,
Source, Compiler, or Inspection public API baseline.

TC06 advances the coordinated development version to `1.6.0-Alpha-6` and adds
explicit opt-in termcap acquisition APIs only to `Icod.TermInfo.Termcap`.
Environment and filesystem access are isolated behind caller-selected provider
seams, and acquisition composes the existing Termcap parser, resolver, and
converter without joining Runtime terminal discovery. The Termcap package still
depends only on Runtime, and no frozen Runtime, Source, Compiler, or Inspection
public API baseline changes.

TC07 advances the coordinated development version to `1.6.0-Alpha-7` and adds
the non-packable `captoinfo` and `infotocap` command projects. They consume the
central suite version and are distributed both as standalone archive launchers
and as routes of the single `Icod.TermInfo.Tools` command. TC07 adds no reusable
Termcap public API: `captoinfo` composes Termcap with Inspection, `infotocap`
composes Source with Termcap, and `Icod.TermInfo.Termcap` itself continues to
depend only on Runtime. The existing `tic`, `infocmp`, and `toe` command
semantics remain frozen.

TC08 advances the coordinated development version to `1.6.0-Alpha-8` without
adding reusable API or command semantics. It freezes the active Termcap public
surface, adds checked-in differential and bounded hostile-input/mutation
validation, requires structural verification of the packed Termcap artifact,
and executes an isolated package-reference-only Termcap consumer on `net8.0`,
`net9.0`, and `net10.0`. The Runtime-only Termcap dependency and the TC07
command/router/archive topology are frozen for 1.6 release closure.

Stable 1.6.0 promotes that frozen Alpha-8 surface without further public API or
command-semantic changes. Runtime, Source, Termcap, Compiler, Inspection, all
five standalone commands, and `Icod.TermInfo.Tools` consume the centralized
`1.6.0` suite version while the five reusable assemblies retain `1.0.0.0`.

Patch release 1.6.1 advances the coordinated package and command version to
`1.6.1` while preserving every frozen reusable API baseline, command contract,
dependency direction, target framework, and assembly identity. Its production
change is limited to release-verifier environment isolation: temporary
package-smoke `NUGET_PACKAGES` values must not leak into repository sample or
toolchain builds.

RS01 advances the coordinated development version to `1.7.0-Alpha-1` and adds
relative-source synthesis contract types only to `Icod.TermInfo.Inspection`.
Runtime, Source, Compiler, and Termcap retain their frozen public API baselines;
Inspection continues to depend only on Runtime and Source in production, and all
five reusable assemblies retain `1.0.0.0`. During 1.7 development the release
verifier enforces cross-framework Inspection API equality and package smoke
coverage for the additive surface. The exact stable 1.7 Inspection API baseline
is intentionally frozen by RS08 rather than by RS01.

RS02 advances the coordinated development version to `1.7.0-Alpha-2` without
adding public API. Inspection now executes deterministic standard Boolean,
numeric, and string relative-source deltas and cancellations against the ordered
parent contract frozen by RS01. Runtime, Source, Compiler, and Termcap retain
their frozen public API baselines; Inspection retains Runtime-and-Source-only
production dependencies and assembly version `1.0.0.0`. Extended capability
relative synthesis remains assigned to RS03.

RS03 advances the coordinated development version to `1.7.0-Alpha-3` and adds
only the additive `TerminalDescriptionSourceSynthesisOptions` extended-output
property/constructor overload to Inspection. Relative synthesis now covers
ordinal case-sensitive extended values, inherited cancellation, value-kind
changes, deterministic ordering, and semantically safe filtering. Runtime,
Source, Compiler, and Termcap remain frozen; Inspection retains its
Runtime-and-Source-only dependency graph and assembly version `1.0.0.0`.

RS04 advances the coordinated development version to `1.7.0-Alpha-4` without
adding public API. Inspection now freezes exact ordered multi-parent composition
and source-reference fidelity: `UseName` is emitted independently of effective
parent canonical identity, repeated/equivalent parents remain legal under
distinct references, and Source-backed cross-checks verify the existing
leftmost-parent precedence across standard and extended capabilities. Runtime,
Source, Compiler, and Termcap remain frozen; Inspection retains its
Runtime-and-Source-only dependency graph and assembly version `1.0.0.0`.

RS05 advances the coordinated development version to `1.7.0-Alpha-5` without
adding public API. It freezes deterministic relative-source layout, wrapping,
capability ordering, LF output, target identity, cancellations, and ordered
`use=` rendering. Source parser/resolver and Compiler round trips become
permanent semantic gates while Compiler remains a test/sample dependency only.

RS06 advances the coordinated development version to `1.7.0-Alpha-6` without
adding reusable API. It exposes relative synthesis through `infocmp -u`, keeps
the command as a thin adapter over Inspection, preserves `-A` target and `-B`
parent acquisition policy, freezes presentation-option interactions, and routes
the same behavior through `icod-terminfo` without duplicating command semantics.

RS07 advances the coordinated development version to `1.7.0-Alpha-7` without
adding API or command semantics. It adds reproducible generated-state round
trips, maximum-parent and pathological-input coverage, and a checked-in semantic
differential corpus pinned to ncurses `6.5.20250216`. Normal validation remains
independent of host ncurses installation and host terminfo state.

RS08 advances the coordinated development version to `1.7.0-Alpha-8` and
freezes the complete additive Inspection 1.7 public surface. It validates the
five reusable packages, router package, six standalone archives, Toolchain
sample, version reporting, dependency direction, and cross-platform release
pipeline. Stable 1.7.0 promotes this validated surface without semantic or API
changes.

RP01 advances the coordinated development version to `1.8.0-Alpha-1` and adds
relative-source planning contract types only to `Icod.TermInfo.Inspection`.
Runtime, Source, Compiler, and Termcap retain their frozen public API baselines;
the frozen 1.7 Inspection baseline remains immutable historical evidence;
Inspection continues to depend only on Runtime and Source in production; and all
five reusable assemblies retain `1.0.0.0`. During 1.8 development the release
verifier enforces cross-framework Inspection API equality and package-smoke
coverage for the additive surface. RP08 freezes the exact stable 1.8 Inspection
API baseline.

RP02 advances the coordinated development version to `1.8.0-Alpha-2` and makes
the planner operational for the zero-parent baseline plus every legal single
candidate position. The public API surface is unchanged from RP01. Inspection
adds only internal synthesis evidence so the frozen score can be computed during
rendering without reparsing generated source; the frozen 1.7 public synthesizer
contract and Runtime-and-Source-only production dependency boundary remain
unchanged. The other coordinated packages and commands advance their package and
reported versions without public API or command-semantic changes.

RP03 advances the coordinated development version to `1.8.0-Alpha-3` and makes
ordered multi-parent planning operational through the configured selected-parent
bound. Candidate positions cannot repeat within one plan, distinct equal
positions remain eligible, fixed-depth enumeration is lexicographic, and exact
selected order is passed unchanged to the frozen leftmost-precedence synthesizer.
The public API and production dependency graph remain unchanged.

## Assembly identity

The 1.x line freezes the managed assembly identities:

```text
AssemblyName       Icod.TermInfo
AssemblyVersion    1.0.0.0
Strong-name signed no

AssemblyName       Icod.TermInfo.Source
AssemblyVersion    1.0.0.0
Strong-name signed no

AssemblyName       Icod.TermInfo.Compiler
AssemblyVersion    1.0.0.0
Strong-name signed no

AssemblyName       Icod.TermInfo.Inspection
AssemblyVersion    1.0.0.0
Strong-name signed no

AssemblyName       Icod.TermInfo.Termcap
AssemblyVersion    1.0.0.0
Strong-name signed no
```

Package patch/minor versions do not advance `AssemblyVersion`.

This is deliberate. Advancing `AssemblyVersion` for a compatible package-minor
release would create a new binary assembly identity and would weaken the 1.x
binding contract without providing a semantic-versioning benefit. All five reusable
assemblies remain unsigned throughout 1.x. Adding a strong name changes assembly
identity and is treated as a major-version design decision unless a future
compatibility review demonstrates a safe migration.

## Public API baselines

The approved `docs/1.0.0-PUBLIC-API-BASELINE.txt` is the exhaustive
machine-readable runtime contract established by 1.0 and retained throughout
1.x.

The approved `docs/1.1.0-SOURCE-PUBLIC-API-BASELINE.txt` is the independent
machine-readable public contract for `Icod.TermInfo.Source`.

Beginning with C01, `docs/1.2.0-COMPILER-PUBLIC-API-BASELINE.txt` records the
developing public contract for `Icod.TermInfo.Compiler` and becomes the frozen
Compiler contract at 1.2 release closure.

The approved `docs/1.3.0-INSPECTION-PUBLIC-API-BASELINE.txt` is the independent
machine-readable public contract for `Icod.TermInfo.Inspection`, frozen at the
1.3 release closure after the I02-I06 API additions and I07 validation gate.

`docs/1.4.0-INSPECTION-PUBLIC-API-BASELINE.txt` is the frozen Inspection
baseline for the 1.4 line. T01 initialized it as an exact copy of the frozen 1.3
baseline. T02 added reviewed read-only system database-location inspection, T03
added reviewed conventional database catalog enumeration, and T06 added reviewed
renderer controls for layout, width, standard-capability ordering, and extended-
capability filtering. T04, T05, and T07 changed only command-layer composition.
The reviewed baseline was frozen at the 1.4.0 release and remains byte-for-byte
unchanged through 1.5.0. The 1.5 distribution/versioning changes add no reusable
library API. Any later public Inspection surface change requires a new compatible
minor-release API review rather than changing this historical baseline.

`docs/1.6.0-TERMCAP-PUBLIC-API-BASELINE.txt` is the frozen Termcap public
surface established by TC08. The validated Alpha-7 `PublicApiSnapshot/v1` rich
reflection manifest has SHA-256 `1e24b8a555b506594c58cf58d03bf87b2b60192f6316537cb4200498c6a92ab0`; release verification requires that
exact fingerprint and net8/net9/net10 equivalence. The same file also records the
241 sorted packaged XML documentation member IDs as a human-reviewable inventory.
The baseline must not be regenerated merely to accept an unintended public
Termcap change.

`docs/1.7.0-INSPECTION-PUBLIC-API-BASELINE.txt` is the frozen complete
Inspection surface for the 1.7 line. It retains every previously released
Inspection member and adds only `TerminalDescriptionSourceSynthesisParent`,
`TerminalDescriptionSourceSynthesisOptions`, and
`TerminalDescriptionSourceSynthesizer`. The historical 1.3 and 1.4 Inspection
baselines remain immutable. Release verification requires exact net8/net9/net10
API equivalence and an exact match with the 1.7 baseline.

The baselines record exported types, public/protected members, enum numeric
values, parameter names/order/defaults, ref/out/in/params shape, generic
constraints, nullability, and relevant attributes.

Routine release validation must check the applicable baseline and require
`net8.0` / `net9.0` / `net10.0` API equivalence. Do not regenerate any
baseline merely because a check fails. A changed baseline must correspond to an
intentional compatibility decision.

## Deprecation

When practical, an API planned for removal should first be marked obsolete in a
compatible release and documented with its replacement. Removal belongs to a
major release.

Security or correctness emergencies may require a faster response, but such a
change must be documented explicitly.

## Package metadata

README, icon, license expression, repository metadata, multi-target managed/XML
payloads, portable symbols, Source Link, and the intended inter-package
dependency direction are part of the release-quality contract.

`Icod.TermInfo` remains dependency-free. `Icod.TermInfo.Source` depends on the
matching `Icod.TermInfo` package; the runtime package never depends on Source.

`Icod.TermInfo.Compiler` depends directly on the matching `Icod.TermInfo` and
`Icod.TermInfo.Source` packages. Neither Runtime nor Source may acquire a
dependency on Compiler.

Beginning with I01, `Icod.TermInfo.Inspection` depends directly on the matching
`Icod.TermInfo` and `Icod.TermInfo.Source` packages. Inspection SHALL NOT depend
on Compiler, and Runtime, Source, and Compiler SHALL NOT acquire a dependency on
Inspection. Inspection tests may reference Compiler for differential evidence
without changing the production package graph.

Beginning with TC01, `Icod.TermInfo.Termcap` depends directly and exclusively on
the matching `Icod.TermInfo` package. Runtime, Source, Compiler, and Inspection
SHALL NOT acquire a Termcap dependency. TC08 freezes that Runtime-only package
graph for the 1.6 release.

Beginning with T01, command projects may depend on `Icod.CommandFramework` and
on the appropriate TermInfo libraries. Runtime, Source, Compiler, Inspection, and
Termcap SHALL NOT acquire an `Icod.CommandFramework` or command-project
dependency. No command project SHALL depend on another command project.

See `COMPATIBILITY.md` for target-framework, platform, behavioral, and feature-
boundary promises.
