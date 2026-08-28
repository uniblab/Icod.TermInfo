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
```

Package patch/minor versions do not advance `AssemblyVersion`.

This is deliberate. Advancing `AssemblyVersion` for a compatible package-minor
release would create a new binary assembly identity and would weaken the 1.x
binding contract without providing a semantic-versioning benefit. All four coordinated
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

`docs/1.4.0-INSPECTION-PUBLIC-API-BASELINE.txt` is the active Inspection
baseline for 1.4 development. T01 initialized it as an exact copy of the frozen
1.3 baseline. T02 adds reviewed read-only system database-location inspection,
and T03 adds reviewed conventional database catalog enumeration, without changing
Runtime, Source, or Compiler public API. T04 adds the non-mutating `tic -c`
validation path. T05 adds command-layer database publication through the already
frozen Compiler writer without adding library API. T06 advances the coordinated
development version to `1.4.0-Alpha-6` and adds reviewed, compatible Inspection
renderer controls for layout, width, standard-capability ordering, and extended-
capability filtering. The frozen 1.3 renderer overload behavior remains unchanged.
Later library-surface changes require another explicit, compatible 1.4 API
review.

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

Beginning with T01, command projects may depend on `Icod.CommandFramework` and
on the appropriate TermInfo libraries. Runtime, Source, Compiler, and Inspection
SHALL NOT acquire an `Icod.CommandFramework` or command-project dependency. No
command project SHALL depend on another command project.

See `COMPATIBILITY.md` for target-framework, platform, behavioral, and feature-
boundary promises.
