# Icod.TermInfo.Inspection

`Icod.TermInfo.Inspection` is the optional managed inspection, semantic-comparison,
planning, and machine-readable automation layer for the `Icod.TermInfo` package
family. It depends on matching Runtime and Source packages and deliberately has
no production dependency on Compiler, Termcap, `Icod.Terminal`, or
`Icod.DCurses`.

## 1.14 release status

Version `1.14.0` promotes the validated `1.14.0-Alpha-8` raster-backend contract
without changing its feature semantics, public API, schemas, dependency graph,
target frameworks, command behavior, package-consumer topology, or archive RIDs.

Inspection now provides bounded `RasterBackend*` availability evidence and
profiles for the initial **Sixel** and **Kitty Graphics** backend identities.
Availability is intentionally separate from persistent-raster lifecycle and
placement truth. `RasterBackendClassifier` uses
`Verified > Declared > CapabilityDerived` evidence precedence, while
`RasterBackendInspector` recognizes only exact Boolean Sixel metadata and never
infers Kitty Graphics from names, brands, xterm identity, Windows Terminal
identity, or unrelated capabilities.

`RasterBackendPlanner.Evaluate(...)` delegates lifecycle and placement semantics
to the frozen 1.11 and 1.12 planners. `RasterBackendPlanner.Plan(...)` adds no
hidden backend ranking: without explicit preference, multiple viable candidates
return `RequiresPreference`; with a complete caller preference order, an
unverified preferred candidate blocks fallback until it is verified or becomes
impossible. Backend-scoped frozen 1.13 runtime integration results can be composed
directly into `RasterBackendCandidate` values without changing the backend-neutral
observation model.

JSON version 6 adds exactly:

```text
rasterBackendProfile
rasterBackendSelectionPlan
```

JSON versions 1 through 5 remain frozen. The complete 1.14 Inspection reflection
manifest contains **106 exported public types** and has normalized-LF SHA-256:

```text
e9f240a562aec5274d64fb2ec3647862fe4ef5684582af2b442ba3c55e189497
```

The v6 schema has normalized-LF SHA-256:

```text
9d51ec6659f8978c867408881eefb106f3c4976dfcf2da12b23c374a222665c2
```

RB07 qualifies the consumer boundary with published `Icod.Terminal 1.13.0` on
`net8.0`, `net9.0`, and `net10.0`; mapping live Terminal results into backend
evidence remains explicit caller policy. The Alpha-8 contract passed workflow
#924 / run `34903967732` on exact head
`911e8d44428a26b588193a27e07696c2489bcb93` with all 12 jobs green. Stable
promotion is independently requalified before release.

See:

- `../docs/1.14.0-RASTER-BACKEND-SELECTION-GUIDE.md`;
- `../docs/1.14.0-INSPECTION-PUBLIC-API-FREEZE.md`;
- `../docs/1.14.0-RB08-FREEZE-FINGERPRINTS.txt`; and
- `../docs/1.14.0-RELEASE-AUDIT.md`.

## Install

```text
dotnet add package Icod.TermInfo.Inspection --version 1.14.0
```

Inspection targets `net8.0`, `net9.0`, and `net10.0` and retains assembly
identity `Icod.TermInfo.Inspection, Version=1.0.0.0` throughout the compatible
1.x package line.

## 1.13 release status

Version `1.13.0` added protocol-neutral caller-owned persistent-raster runtime
observations, deterministic integration into the existing lifecycle and placement
evidence models, planner-delegating replanning, and JSON version 5 documents
`persistentRasterRuntimeObservationSet` and `persistentRasterRuntimeIntegration`.
The complete frozen 1.13 Inspection surface contains 90 exported public types with
normalized-LF SHA-256
`fd827a25abafb8e9ff3915567f45f9f2ec51b332bfd8a82dd4e4bca20290e764`.
Production Inspection remained free of an `Icod.Terminal` dependency; downstream
qualification used published `Icod.Terminal 1.12.0`.

See `../docs/1.13.0-PERSISTENT-RASTER-RUNTIME-EVIDENCE-GUIDE.md` and
`../docs/1.13.0-RELEASE-AUDIT.md`.

## 1.12 release status

Version `1.12.0` added the protocol-neutral `SourceRectangle` and `SignedZOrder`
advanced-placement semantic families above the frozen 1.11 lifecycle model. The
complete frozen 1.12 Inspection surface contains 81 exported public types with
normalized-LF SHA-256
`f71501dcd27a530051c1a02083325144ced2b6173b6b967b9571c620815198f0`.
JSON version 4 contains exactly `persistentRasterPlacementProfile` and
`persistentRasterPlacementPlan`.

See `../docs/1.12.0-ADVANCED-PERSISTENT-RASTER-PLACEMENT-GUIDE.md` and
`../docs/1.12.0-RELEASE-AUDIT.md`.

## 1.11 release status

Version `1.11.0` established protocol-neutral persistent-raster lifecycle
evidence, classification, contradiction handling, uncertainty, and deterministic
semantic planning. The complete frozen 1.11 Inspection surface has normalized-LF
SHA-256
`69c7350d5d44d502ecf1698c8fe1c1336f03d38eb1a36e36219f50ac33585a86`.
JSON version 3 contains exactly `persistentRasterLifecycleProfile` and
`persistentRasterLifecyclePlan`.

See `../docs/1.11.0-PERSISTENT-RASTER-LIFECYCLE-GUIDE.md` and
`../docs/1.11.0-RELEASE-AUDIT.md`.

## Earlier Inspection layers

The earlier compatible 1.x releases remain frozen historical contracts:

- **1.3** established canonical rendering and semantic comparison;
- **1.4** added reusable database inspection and renderer controls used by the
  managed command suite;
- **1.7** added deterministic relative terminfo source synthesis;
- **1.8** added bounded deterministic parent planning;
- **1.9** added versioned deterministic machine-readable JSON output;
- **1.10** added ordered multi-database inspection, precedence, comparison, and
  planning automation.

Their exact API baselines, schemas, and release evidence remain in the matching
versioned files under `../docs/`.

## Ownership boundary

Inspection is advisory and deterministic. It may classify evidence, render data,
compare semantic state, inspect explicit databases, synthesize source, plan
parents, integrate caller-owned runtime observations, and plan raster-backend
selection. It does **not**:

- probe a live terminal;
- transmit Sixel or Kitty Graphics protocol traffic;
- allocate terminal-side image, resource, or placement identities;
- decode input events;
- own terminal mode or presentation lifecycle;
- rank terminal brands or emulator names; or
- deserialize untrusted JSON into operational terminal state.

Those live-session and execution responsibilities belong to callers or sibling
layers such as `Icod.Terminal`; curses-style virtual-screen/window policy belongs
to `Icod.DCurses`.

## Raster-backend selection flow

The intended 1.14 flow is:

```text
static TermInfo metadata
    -> backend availability evidence/profile
    -> lifecycle + placement profiles
    -> RasterBackendCandidate
    -> RasterBackendPlanner.Plan(...)

RequiresRuntimeVerification
    -> caller-owned live verification
    -> backend-scoped 1.13 observations/integration
    -> strengthened RasterBackendCandidate
    -> plan again

Selected
    -> execute the chosen graphics protocol outside TermInfo
```

Preference is caller policy. Canonical ordering exists only to make output and
audit data deterministic; it never chooses a backend.

## Machine-readable output

`TermInfoJsonRenderer` provides bounded deterministic JSON. Version 1 through 6
are additive historical contracts. The current v6 backend documents preserve the
backend profile/evidence or complete selection plan, including caller preference,
canonical candidate evaluations, selected backend only when selected, and the
remaining blocker for non-selected outcomes.

The published schemas are packaged with `Icod.TermInfo.Inspection` under
`docs/`. No generic JSON import/deserialization API is provided.

## Samples

The focused public-API samples are:

- `../samples/Icod.TermInfo.PersistentRasterLifecycle.Sample`;
- `../samples/Icod.TermInfo.PersistentRasterPlacement.Sample`;
- `../samples/Icod.TermInfo.PersistentRasterRuntimeIntegration.Sample`; and
- `../samples/Icod.TermInfo.RasterBackendSelection.Sample`.

The 1.14 raster-backend sample has deterministic CI-safe behavior by default and
an explicit `--live` mode at the downstream `Icod.Terminal` boundary.

## Compatibility

Runtime, Source, Compiler, Inspection, and Termcap continue to advance as one
coordinated package family while retaining their frozen dependency directions and
1.0.0.0 reusable assembly identities. See `../docs/VERSIONING.md` and
`../docs/COMPATIBILITY.md` for the 1.x policy and
`../Icod.TermInfo-Post-1.0-Development-Roadmap.md` for release sequencing.
