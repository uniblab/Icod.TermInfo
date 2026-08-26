# Icod.TermInfo Versioning Policy

The `Icod.TermInfo` package family follows Semantic Versioning for its public
package contracts.

## Package versions

The NuGet packages use:

```text
MAJOR.MINOR.PATCH
```

with ordinary prerelease suffixes such as `-alpha.1`, `-beta.1`, and `-rc.1`.

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

## Assembly identity

The 1.x line freezes both managed assembly identities:

```text
AssemblyName       Icod.TermInfo
AssemblyVersion    1.0.0.0
Strong-name signed no

AssemblyName       Icod.TermInfo.Source
AssemblyVersion    1.0.0.0
Strong-name signed no
```

Package patch/minor versions do not advance `AssemblyVersion`.

This is deliberate. Advancing `AssemblyVersion` for a compatible package-minor
release would create a new binary assembly identity and would weaken the 1.x
binding contract without providing a semantic-versioning benefit. Both
assemblies remain unsigned throughout 1.x. Adding a strong name changes assembly
identity and is treated as a major-version design decision unless a future
compatibility review demonstrates a safe migration.

## Public API baselines

The approved `docs/1.0.0-PUBLIC-API-BASELINE.txt` is the exhaustive
machine-readable runtime contract established by 1.0 and retained throughout
1.x.

The approved `docs/1.1.0-SOURCE-PUBLIC-API-BASELINE.txt` is the independent
machine-readable public contract for `Icod.TermInfo.Source`.

The baselines record exported types, public/protected members, enum numeric
values, parameter names/order/defaults, ref/out/in/params shape, generic
constraints, nullability, and relevant attributes.

Routine release validation must check the applicable baseline and require
`net8.0` / `net10.0` API equivalence. Do not regenerate either baseline merely
because a check fails. A changed baseline must correspond to an intentional
compatibility decision.

## Deprecation

When practical, an API planned for removal should first be marked obsolete in a
compatible release and documented with its replacement. Removal belongs to a
major release.

Security or correctness emergencies may require a faster response, but such a
change must be documented explicitly.

## Package metadata

README, icon, license expression, repository metadata, dual-target managed/XML
payloads, portable symbols, Source Link, and the intended inter-package
dependency direction are part of the release-quality contract.

`Icod.TermInfo` remains dependency-free. `Icod.TermInfo.Source` depends on the
matching `Icod.TermInfo` package; the runtime package never depends on Source.

See `COMPATIBILITY.md` for target-framework, platform, behavioral, and feature-
boundary promises.
