# Public API Snapshot

`Icod.TermInfo.PublicApiSnapshot` emits a deterministic reflection manifest of
the complete exported `Icod.TermInfo` API or of an explicitly supplied built
assembly.

The manifest records:

- exported type kind, identity, base type, interfaces, and generic constraints;
- enum names and numeric values;
- public/protected fields and constants;
- public/protected constructors;
- public/protected properties and indexers;
- public/protected events;
- public/protected methods and operators;
- parameter names, order, ref/out/in/params shape, optional/default values;
- generic method constraints;
- nullability state;
- relevant `System.Diagnostics.CodeAnalysis`, `Flags`, and `Obsolete`
  attributes.

The tool intentionally targets `net10.0`; it is repository maintenance tooling
rather than a shipped runtime asset.

Print the current runtime manifest:

```text
dotnet run --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj -c Release
```

Write the candidate 1.0 runtime baseline:

```text
dotnet run --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj -c Release -- --write
```

The default runtime path is:

```text
docs/1.0.0-PUBLIC-API-BASELINE.txt
```

The first runtime baseline was reviewed during T42. Do not use `--write` as a
way to silence an unexplained API change.

Verify the approved runtime baseline with:

```text
dotnet run --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj -c Release -- --check
```

S02 adds explicit-assembly baseline support. The reviewed Source baseline can be
checked with:

```text
dotnet run --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj -c Release --no-build -- --check docs/1.1.0-SOURCE-PUBLIC-API-BASELINE.txt Icod.TermInfo.Source/bin/Release/net10.0/Icod.TermInfo.Source.dll
```

Use the same three-argument form with `--write` only when deliberately creating
or reviewing a new baseline.

Compare the public contract of two built assemblies:

```text
dotnet run --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj -c Release -- --compare bin/Release/net8.0/Icod.TermInfo.dll bin/Release/net10.0/Icod.TermInfo.dll
```

Release validation requires exact public API equivalence between the `net8.0`
and `net10.0` package targets. It also checks the frozen runtime baseline and,
from S02 onward, the reviewed Source baseline. The release/package gate fails
rather than silently blessing an unexplained public API change.
