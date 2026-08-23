# Icod.TermInfo Versioning Policy

`Icod.TermInfo` follows Semantic Versioning for its public package contract.

## Package versions

The NuGet package uses:

```text
MAJOR.MINOR.PATCH
```

with ordinary prerelease suffixes such as `-alpha.1`, `-beta.1`, and `-rc.1`.

For the 1.x line:

- patch releases correct defects without intentionally changing the supported
  public contract;
- minor releases may add compatible public API or capability/profile data;
- removal, incompatible signature changes, incompatible enum-value changes, or
  deliberate semantic-contract breaks require a new major version.

`<Version>` and `<PackageVersion>` must remain identical.

## Assembly identity

The 1.x line freezes:

```text
AssemblyName       Icod.TermInfo
AssemblyVersion    1.0.0.0
Strong-name signed no
```

Package patch/minor versions do not advance `AssemblyVersion`.

The package remains unsigned throughout 1.x. Adding a strong name changes
assembly identity and is treated as a major-version design decision unless a
future compatibility review demonstrates a safe migration.

## Public API baseline

The approved `docs/1.0.0-PUBLIC-API-BASELINE.txt` is the exhaustive machine-
readable public contract for 1.0.

It records exported types, public/protected members, enum numeric values,
parameter names/order/defaults, ref/out/in/params shape, generic constraints,
nullability, and relevant attributes.

Routine changes must run:

```text
public-api-snapshot --check
```

Do not regenerate the baseline merely because a check fails. A changed baseline
must correspond to an intentional compatibility decision.

## Deprecation

When practical, an API planned for removal should first be marked obsolete in a
compatible release and documented with its replacement. Removal belongs to a
major release.

Security or correctness emergencies may require a faster response, but such a
change must be documented explicitly.

## Package metadata

README, icon, license expression, repository metadata, dual-target managed/XML
payloads, portable symbols, and Source Link are part of the release-quality
contract enforced by the package verifier.

See `COMPATIBILITY.md` for target-framework, platform, behavioral, and feature-
boundary promises.
