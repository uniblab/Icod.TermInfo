# Icod.TermInfo Compatibility Policy

## 1.15 compatibility freeze

Icod.TermInfo 1.15 adds the optional `Icod.TermInfo.BerkeleyDb` package with an
exact nine-type public API freeze. It supports the reviewed ncurses-compatible
Berkeley DB Hash-v9 subset in pure managed, read only code. The package API is
equivalent on net8.0, net9.0, and net10.0 and assembly identity remains
`1.0.0.0`.

BerkeleyDb depends only on matching-version Runtime. Runtime stays
dependency-free and never discovers hashed stores implicitly. Source, Termcap,
Compiler, and Inspection do not depend on BerkeleyDb; Inspection remains
provider-neutral. Native Berkeley DB is a CI oracle only and is not shipped.

Compatibility includes exact lookup/canonical-alias identity, deterministic
catalog ordering, bounded error categories, and the reviewed explicit-path
`infocmp`/human-`toe` behavior. It excludes hashed writing, `tic` hashed
publication, atomic snapshots, other Berkeley DB access methods or revisions,
and general encoding detection. Inspection JSON versions 1 through 6 are
unchanged.

Stable 1.15 promotion preserves the exact nine-type public API freeze,
acquisition semantics, dependency direction, target frameworks, JSON and
command contracts, package topology, archive RIDs, pure-managed deployment, and
read-only boundary accepted from Alpha-8.

This document defines the supported 1.x compatibility boundary for
`Icod.TermInfo`, `Icod.TermInfo.Source`, `Icod.TermInfo.Compiler`,
`Icod.TermInfo.Inspection`, `Icod.TermInfo.Termcap`, and the coordinated tool
distribution. Exact release evidence remains in the versioned API freezes,
schema files, roadmaps, and release audits.

## 1.14 compatibility freeze

Version 1.14 is additive above the stable 1.13 boundary. RB08 freezes the complete
1.14 `Icod.TermInfo.Inspection` reflection manifest at **106 exported public
types** with normalized-LF SHA-256:

```text
e9f240a562aec5274d64fb2ec3647862fe4ef5684582af2b442ba3c55e189497
```

Release verification first proves that complete 1.14 surface across `net8.0`,
`net9.0`, and `net10.0`. It then removes only the reviewed 1.14 additions — the
16 `RasterBackend*` type blocks introduced through RB03 and the six additive
RB06 `TermInfoJsonRenderer` member lines — and requires the remainder to
reconstruct frozen 1.13 SHA-256 exactly:

```text
fd827a25abafb8e9ff3915567f45f9f2ec51b332bfd8a82dd4e4bca20290e764
```

The existing 1.13 -> 1.12 -> 1.11 -> 1.10 reconstruction chain remains
unchanged.

Version 1.14 adds only advisory raster-backend availability and selection
semantics to Inspection. Backend availability is independent from lifecycle and
placement capability truth. Sixel and Kitty Graphics are the initial concrete
backend identities. Static inspection may derive positive Sixel evidence only
from exact authoritative metadata; terminal names, emulator brands, profile
names, enum values, and candidate input order never imply support or preference.

The planner has no hidden backend ranking. With no explicit caller preference,
multiple viable candidates return `RequiresPreference`. With a complete caller
preference order, a preferred candidate requiring runtime verification blocks
fallback until it is verified or becomes impossible.

JSON schema versions 1 through 5 remain immutable historical contracts. Version
6 is additive and contains exactly `rasterBackendProfile` and
`rasterBackendSelectionPlan`, with normalized-LF SHA-256:

```text
9d51ec6659f8978c867408881eefb106f3c4976dfcf2da12b23c374a222665c2
```

Production Inspection remains free of an `Icod.Terminal` package/project
reference. RB07 qualifies a package-only consumer beside published
`Icod.Terminal 1.13.0`; the caller owns mapping of live Terminal results into
TermInfo backend evidence and backend-scoped runtime-integration contexts.

Stable 1.14 promotion may not change the exact frozen API, any released JSON
schema, production dependency direction, target frameworks, command semantics,
package-consumer topology, or archive RIDs. Stable promotion requires a fresh
full qualification matrix.

## 1.13 compatibility freeze

Version 1.13 is additive above the stable 1.12 boundary. RE08 freezes the complete
1.13 Inspection manifest at 90 exported public types with normalized-LF SHA-256
`fd827a25abafb8e9ff3915567f45f9f2ec51b332bfd8a82dd4e4bca20290e764`.
The verifier removes only the reviewed 1.13 runtime-observation type and renderer
member delta to reconstruct frozen 1.12 exactly.

Version 1.13 adds protocol-neutral caller-owned runtime observation and evidence
integration semantics. It does not add live probing, backend ranking, protocol
negotiation, raw protocol responses, terminal session/resource identity, or a
production `Icod.Terminal` dependency. JSON v5 contains exactly
`persistentRasterRuntimeObservationSet` and
`persistentRasterRuntimeIntegration`; v1-v4 remain frozen.

## 1.12 compatibility freeze

Version 1.12 is additive above the stable 1.11 boundary. PG08 freezes the
complete 1.12 Inspection manifest at 81 exported public types with normalized-LF
SHA-256
`f71501dcd27a530051c1a02083325144ced2b6173b6b967b9571c620815198f0`.
Removing only the reviewed placement delta reconstructs frozen 1.11 exactly.

Version 1.12 adds the protocol-neutral `SourceRectangle` and `SignedZOrder`
placement semantic families. Concrete placement coordinates, z-order values,
resource identities, live probing, wire-protocol selection, and terminal I/O
remain outside TermInfo. JSON v4 contains exactly
`persistentRasterPlacementProfile` and `persistentRasterPlacementPlan`; v1-v3
remain frozen.

## 1.11 compatibility freeze

Version 1.11 is additive above the stable 1.10 boundary. RL08 freezes the complete
1.11 Inspection manifest by normalized-LF SHA-256
`69c7350d5d44d502ecf1698c8fe1c1336f03d38eb1a36e36219f50ac33585a86`.
Removing only the reviewed lifecycle delta reconstructs frozen 1.10 exactly.

Version 1.11 adds protocol-neutral persistent-raster lifecycle evidence,
classification, planning, and JSON v3 only to Inspection. Live verification,
graphics protocol transmission, terminal resource/placement identity,
acknowledgements, generation invalidation, and cleanup remain outside TermInfo.

## 1.10 compatibility freeze

Version 1.10 is additive above the stable 1.9 boundary. DA08 freezes the complete
1.10 Inspection surface in `1.10.0-INSPECTION-PUBLIC-API-BASELINE.txt` and
requires exact equality across `net8.0`, `net9.0`, and `net10.0`. Version-1 JSON
remains immutable; version 2 adds only `databaseSet`, `databaseSetComparison`,
and `databaseSetPlan`.

## Public API compatibility

The 1.x line is compatible-additive:

- released public types and members are not removed or incompatibly changed;
- enum numeric values and public default values are part of the contract;
- parameter order, names, modifiers, generic constraints, nullability, and
  relevant attributes are checked by API snapshots;
- routine validation must not regenerate a baseline to accept an accidental
  difference; and
- incompatible contract changes require a new major release unless a documented
  emergency compatibility decision explicitly says otherwise.

Runtime, Source, Compiler, Inspection, and Termcap retain reusable assembly
version `1.0.0.0` throughout the compatible 1.x package line.

## Target-framework compatibility

Current coordinated reusable packages target:

```text
net8.0
net9.0
net10.0
```

The public reusable API must be equivalent across those target frameworks.
Supported release validation runs on Windows, Linux, and macOS. A platform may
have platform-specific acquisition or live-host behavior, but no reusable API
shape may silently differ by target framework or host OS.

## Package dependency compatibility

The production dependency direction is frozen:

```text
Icod.TermInfo                 dependency-free
Icod.TermInfo.Source          -> Runtime
Icod.TermInfo.Termcap         -> Runtime
Icod.TermInfo.BerkeleyDb      -> Runtime
Icod.TermInfo.Compiler        -> Runtime + Source
Icod.TermInfo.Inspection      -> Runtime + Source
```

Runtime never depends upward on optional layers. Inspection does not depend on
Compiler or Termcap. Reusable packages do not depend on command projects or
`Icod.CommandFramework`. Beginning with 1.11 and continuing through 1.14,
Inspection also must not acquire a production dependency on BerkeleyDb,
`Icod.Terminal` or
`Icod.DCurses`.

Tests, samples, and isolated package consumers may reference sibling packages to
prove downstream interoperability without changing the production graph.

## Behavioral compatibility

The project distinguishes descriptive/advisory semantics from live terminal
execution:

- Runtime owns terminal descriptions, compiled database acquisition, capability
  semantics, parameter expansion, and output transformation;
- Source owns `.ti` parsing and inheritance resolution;
- Compiler owns deterministic compiled-entry writing/publication;
- Termcap owns opt-in termcap parsing/conversion/acquisition;
- Inspection owns deterministic rendering, comparison, synthesis/planning,
  database automation, persistent-raster evidence/planning, runtime-evidence
  integration, raster-backend availability/selection, and versioned JSON views;
- `Icod.Terminal` owns live terminal/session verification and protocol execution;
- `Icod.DCurses` owns higher-level curses-style virtual-screen/window policy.

Loading descriptions or calling Inspection planners never implicitly probes a
terminal, changes terminal modes, allocates terminal-side identities, or sends a
graphics protocol.

## JSON compatibility

Released Inspection JSON versions are immutable for their historical inputs:

```text
v1  effective description/comparison/source plan/catalog
v2  ordered database-set automation
v3  persistent-raster lifecycle profile/plan
v4  persistent-raster placement profile/plan
v5  runtime observation set/integration
v6  raster backend profile/selection plan
```

New incompatible shapes require a new schema version. Existing schema identifiers,
field semantics, deterministic ordering, UTF-8 bounds, and historical renderer
forms must remain compatible. Inspection deliberately does not provide generic
JSON input/deserialization into operational terminal state.

## Command and distribution compatibility

The coordinated tool package continues to expose the `icod-terminfo` router and
the standalone archive distribution continues to provide the traditional
`tic`, `infocmp`, `toe`, `captoinfo`, and `infotocap` command names. Minor
Inspection API additions do not implicitly change existing command semantics.

Release validation verifies exact package contents, installed-tool behavior, and
all six supported archive RIDs. A stable promotion from an accepted prerelease
may not alter that topology without an explicitly reviewed release defect.

## Platform and scope boundaries

`Icod.TermInfo` is not a curses implementation, terminal emulator, PTY/ConPTY
layer, termios session manager, input-event decoder, or live graphics protocol
executor. Those exclusions are compatibility promises: adding descriptive
metadata or advisory plans does not transfer live-state ownership into TermInfo.

See `VERSIONING.md` for package/API version rules and the versioned release audits
for exact qualification evidence.
