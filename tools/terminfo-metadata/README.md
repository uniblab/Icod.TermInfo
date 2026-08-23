# Terminfo Metadata Tooling

This directory contains the deterministic source and maintenance tooling for
the complete standard capability model introduced by T22.

Runtime code remains under `src/Capabilities/`. Nothing in this directory is a
runtime package dependency. The repository's metadata workflow uses the .NET
SDK already required by `Icod.TermInfo`; Python is not required.

## Canonical table

`standard-capabilities.tsv` is the checked-in canonical 0.8 metadata source.
Each row records:

- capability kind (`B`, `N`, or `S`);
- zero-based compiled binary table index within that kind;
- terminfo short name;
- terminfo long/variable name;
- termcap compatibility code;
- managed enum member name.

The selected binary-compatible baseline contains exactly:

- 44 standard Boolean capabilities;
- 39 standard numeric capabilities;
- 414 standard string capabilities.

The ordering, terminfo names, and termcap codes follow the ncurses standard ABI
tables generated from `include/Caps`, revision 1.62 (2025-11-12). The table
includes the compatibility slots which ncurses retains at the tail of the
compiled standard arrays. Those positions are part of the selected binary
contract even when their source names are prefixed `OT`.

The `ManagedName` column deliberately preserves every public 0.7 enum member
name and supplies append-only names for capabilities first exposed in 0.8.
Managed enum numeric values are **not** binary table indices.

## Automatic validation

The normal C# test suite copies `standard-capabilities.tsv` into its test
assets and compares every canonical row with `StandardCapabilityCatalog`.
Therefore:

```text
dotnet test Icod.TermInfo.sln
```

verifies that the checked-in runtime metadata still matches the canonical TSV.
No separate scripting runtime is required.

## Regenerating the runtime table

A dependency-free `net10.0` maintenance utility is included in the solution:

```text
tools/terminfo-metadata/Icod.TermInfo.MetadataGenerator.csproj
```

Run:

```text
dotnet run --project tools/terminfo-metadata/Icod.TermInfo.MetadataGenerator.csproj
```

to regenerate:

```text
src/Capabilities/StandardCapabilityDefinitions.Generated.cs
```

Use:

```text
dotnet run --project tools/terminfo-metadata/Icod.TermInfo.MetadataGenerator.csproj -- --check
```

for a non-mutating consistency check. The generator normalizes line endings
when checking so Windows Git checkout policy does not cause false mismatches.

Generated runtime output is checked in so package builds remain deterministic
and dependency-free.

## Rules

- No network access is required by normal build, test, validation, or regeneration.
- No Python installation is required.
- Upstream provenance is recorded with imported capability tables.
- Existing 0.7 enum numeric values are immutable.
- New capability enum members are append-only.
- Managed enum numeric values are never compiled terminfo binary indices.
- The canonical metadata record owns the future binary index.
- Generated output used by the runtime is checked in and reviewable.
- Regeneration is deterministic and emits LF line endings.
