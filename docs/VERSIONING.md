# Icod.TermInfo Versioning Policy

The `Icod.TermInfo` package family follows Semantic Versioning for its public
package contracts. Version-specific roadmaps, API freezes, schema fingerprints,
and release audits remain the authoritative historical evidence for completed
releases; this document defines the current cross-release policy.

## 1.15 release line

The HDB01-HDB09 development sequence used `1.15.0-Alpha-1` through the
accepted `1.15.0-Alpha-8` feature/API source.

Stable `1.15.0` is the current coordinated release. Version 1.15 adds one
optional package, `Icod.TermInfo.BerkeleyDb`, for pure-managed, read-only
acquisition from the reviewed ncurses-compatible Berkeley DB Hash-v9 subset.

HDB09 freezes the complete BerkeleyDb reflection manifest at **9 exported public
types** with normalized-LF SHA-256:

```text
f519600aa4085d07c2d20bd8dc7a32c4dc06a43f4e361554b205ce2f97a8bf36
```

The package depends only on matching-version Runtime and has equivalent API on
net8.0, net9.0, and net10.0. Runtime's 1.0 API, the other reusable package APIs,
Inspection JSON versions 1 through 6, and existing commands remain frozen except
for the accepted explicit hashed-file behavior in `infocmp` and human `toe`.

The promotion changed release identity and release-facing text only. It did not
change API, acquisition semantics, dependencies, target frameworks, JSON,
command contracts, package topology, or archive RIDs.

## 1.14 release line

The RB01-RB08 development sequence is `1.14.0-Alpha-1` through
`1.14.0-Alpha-8`. Version 1.14 adds compatible public API only to
`Icod.TermInfo.Inspection` for raster-backend availability evidence,
classification, lifecycle/placement-aware candidate evaluation, explicit
preference-aware backend selection, composition with the frozen 1.13 runtime
integration result, and additive version-6 JSON. Runtime, Source, Compiler, and
Termcap public APIs remain frozen.

RB08 freezes the complete 1.14 Inspection reflection manifest at **106 exported
public types** with normalized-LF SHA-256:

```text
e9f240a562aec5274d64fb2ec3647862fe4ef5684582af2b442ba3c55e189497
```

The reviewed 1.14 type delta is the 16 `RasterBackend*` type blocks introduced
through RB03. RB06 contributes exactly six additive `TermInfoJsonRenderer`
member lines. Release verification first requires the complete 1.14 surface,
then removes only those reviewed 1.14 additions to reconstruct the frozen 1.13
surface with SHA-256:

```text
fd827a25abafb8e9ff3915567f45f9f2ec51b332bfd8a82dd4e4bca20290e764
```

The established historical reconstruction chain then continues through frozen
1.12, 1.11, and 1.10.

JSON versions 1 through 5 remain immutable historical contracts. Version 6 is
additive and contains exactly `rasterBackendProfile` and
`rasterBackendSelectionPlan`; its normalized-LF schema SHA-256 is:

```text
9d51ec6659f8978c867408881eefb106f3c4976dfcf2da12b23c374a222665c2
```

Production `Icod.TermInfo.Inspection` does not acquire an `Icod.Terminal`
dependency. RB07 qualifies a package-only consumer against published
`Icod.Terminal 1.13.0` on `net8.0`, `net9.0`, and `net10.0`.

After the exact Alpha-8 head passes the complete Staging package, historical
compatibility, RB07 package-consumer/sample, installed-tool, and six-RID archive
gates, stable `1.14.0` is a promotion-only transition. Promotion may change the
coordinated release identity and stable-facing documentation only; it may not
introduce feature semantics, public API, schema fields, production dependencies,
target frameworks, command behavior, package-consumer topology, or archive RIDs,
and it requires its own fresh full validation.

## 1.13 release line

The RE01-RE08 sequence is `1.13.0-Alpha-1` through `1.13.0-Alpha-8`.
Version 1.13 added compatible public API only to Inspection for caller-owned
persistent-raster runtime observations, deterministic integration into the frozen
1.11 lifecycle and 1.12 placement evidence models, planner-delegating replanning,
and additive version-5 JSON.

RE08 freezes the complete 1.13 Inspection surface at 90 exported public types
with normalized-LF SHA-256
`fd827a25abafb8e9ff3915567f45f9f2ec51b332bfd8a82dd4e4bca20290e764`.
JSON versions 1 through 4 remain immutable; version 5 contains exactly
`persistentRasterRuntimeObservationSet` and
`persistentRasterRuntimeIntegration`. Stable 1.13 promotion is semantic/API/schema
neutral relative to the validated Alpha-8 contract.

## 1.12 release line

The PG01-PG08 sequence is `1.12.0-Alpha-1` through `1.12.0-Alpha-8`.
Version 1.12 added compatible Inspection API for advanced persistent-raster
placement evidence and planning. PG08 freezes the complete 1.12 Inspection
surface at 81 exported public types with normalized-LF SHA-256
`f71501dcd27a530051c1a02083325144ced2b6173b6b967b9571c620815198f0`.
JSON versions 1 through 3 remain immutable; version 4 contains exactly
`persistentRasterPlacementProfile` and `persistentRasterPlacementPlan`.

## 1.11 release line

The RL01-RL08 sequence is `1.11.0-Alpha-1` through `1.11.0-Alpha-8`.
Version 1.11 established protocol-neutral persistent-raster lifecycle evidence,
classification, and deterministic semantic planning in Inspection. The complete
1.11 manifest is frozen by normalized-LF SHA-256
`69c7350d5d44d502ecf1698c8fe1c1336f03d38eb1a36e36219f50ac33585a86`.
JSON versions 1 and 2 remain immutable; version 3 contains only
`persistentRasterLifecycleProfile` and `persistentRasterLifecyclePlan`.

## 1.10 release line

The DA01-DA08 sequence is `1.10.0-Alpha-1` through `1.10.0-Alpha-8`.
Version 1.10 added deterministic ordered multi-database inspection, precedence,
conflict analysis, comparison, planning, and additive version-2 JSON. The frozen
complete Inspection surface is recorded in
`1.10.0-INSPECTION-PUBLIC-API-BASELINE.txt`; version-1 JSON remains immutable.

## 1.9 release line

The MI01-MI07 sequence established deterministic versioned JSON rendering and
command automation in Inspection without changing earlier synthesis/planning
semantics. Stable 1.9 freezes the 31-type Inspection surface in
`1.9.0-INSPECTION-PUBLIC-API-BASELINE.txt` and the version-1 JSON contract in
`Icod.TermInfo.Inspection.schema.json`.

## Package versions

Coordinated NuGet packages use Semantic Versioning:

```text
MAJOR.MINOR.PATCH
```

Development tranches use prerelease identities such as `1.14.0-Alpha-1`.
Stable release tags are exactly `v<PackageVersion>`.

For the 1.x line:

- patch releases correct defects without intentionally changing the supported
  public contract;
- minor releases may add compatible public API, capability/profile data, or
  optional sibling packages;
- removal, incompatible signature changes, incompatible enum-value changes, or
  deliberate semantic-contract breaks require a new major version.

Beginning with 1.5.0, `Directory.Build.props` contains the single
`IcodTermInfoSuiteVersion` authority. Runtime, Source, Compiler, Inspection,
Termcap, all five standalone command projects, and the `Icod.TermInfo.Tools`
router consume that coordinated release identity rather than carrying independent
current-version literals.

## Coordinated package family

The coordinated registry family is:

```text
Icod.TermInfo
Icod.TermInfo.Source
Icod.TermInfo.Compiler
Icod.TermInfo.Inspection
Icod.TermInfo.Termcap
Icod.TermInfo.Tools
```

The five reusable libraries target `net8.0`, `net9.0`, and `net10.0`. The command
applications and router follow the repository's separately documented command
framework/TFM policy. A coordinated minor or patch release advances package and
reported command identities together even when only one optional layer receives
new compatible API.

## Assembly identity

The compatible 1.x line freezes reusable managed assembly identities:

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

All five reusable assemblies remain **unsigned** throughout the compatible 1.x
line. Package minor/patch versions do not advance `AssemblyVersion`. Adding a
strong name or otherwise changing assembly identity is a major-version design
decision unless a future compatibility review proves a safe migration.

## Public API freezes

The repository keeps independent immutable public API authorities for each
reusable package and release line. Important current authorities include:

- `1.0.0-PUBLIC-API-BASELINE.txt` — Runtime;
- `1.1.0-SOURCE-PUBLIC-API-BASELINE.txt` — Source;
- `1.2.0-COMPILER-PUBLIC-API-BASELINE.txt` — Compiler;
- `1.6.0-TERMCAP-PUBLIC-API-BASELINE.txt` — Termcap;
- `1.10.0-INSPECTION-PUBLIC-API-BASELINE.txt` — frozen pre-raster Inspection
  authority;
- `1.11.0-INSPECTION-PUBLIC-API-FREEZE.md` — composite 1.11 freeze;
- `1.12.0-INSPECTION-PUBLIC-API-FREEZE.md` — composite 1.12 freeze;
- `1.13.0-INSPECTION-PUBLIC-API-FREEZE.md` — composite 1.13 freeze; and
- `1.14.0-INSPECTION-PUBLIC-API-FREEZE.md` — current composite 1.14 freeze.

Routine validation must require equivalent public API across `net8.0`, `net9.0`,
and `net10.0` for reusable libraries. A baseline/fingerprint must never be
regenerated merely to accept an unintended API change.

## JSON schema versioning

Inspection JSON schemas are additive and immutable once released:

```text
v1  effective description/comparison/source plan/catalog
v2  ordered database-set automation
v3  persistent-raster lifecycle profile/plan
v4  persistent-raster placement profile/plan
v5  runtime observation set/integration
v6  raster backend profile/selection plan
```

Adding a new document kind or incompatible shape requires a new schema version;
older schema files and renderer behavior remain frozen for their historical
inputs. Inspection does not provide a generic operational JSON deserializer.

## Package dependency direction

The supported production dependency graph is intentionally one-way:

- `Icod.TermInfo` is dependency-free;
- Source depends on matching Runtime;
- Termcap depends only on matching Runtime;
- Compiler depends on matching Runtime and Source;
- Inspection depends on matching Runtime and Source;
- reusable libraries do not depend on command projects or `Icod.CommandFramework`;
- Inspection does not depend on Compiler, Termcap, `Icod.Terminal`, or
  `Icod.DCurses` in production.

Tests, samples, and package-only qualification consumers may reference sibling
packages to prove interoperability without changing that production graph.

## Deprecation

When practical, API planned for removal should first be marked obsolete in a
compatible release and documented with its replacement. Removal belongs to a
major release. Security or correctness emergencies may require a faster response,
but the compatibility impact must be documented explicitly.

## Package metadata

README, icon, license expression, repository metadata, multi-target managed/XML
payloads, portable symbols, Source Link, package dependency direction, and the
published Inspection schemas are part of the release-quality contract.

See `COMPATIBILITY.md` for target-framework, platform, behavioral, and feature-
boundary promises, and the versioned release audits for exact qualification
evidence.
