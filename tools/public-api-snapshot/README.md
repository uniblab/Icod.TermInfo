# Public API Snapshot

`Icod.TermInfo.PublicApiSnapshot` emits a deterministic reflection manifest of
the complete exported `Icod.TermInfo` API.

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

The tool intentionally targets `net10.0`; it describes the public contract of
the project reference rather than becoming a shipped runtime asset.

Print the current manifest:

```text
dotnet run --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj -c Release
```

Write the candidate 1.0 baseline:

```text
dotnet run --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj -c Release -- --write
```

The default path is:

```text
docs/1.0.0-PUBLIC-API-BASELINE.txt
```

The first baseline must be reviewed by a human during T42 before it is accepted.
Do not use `--write` as a way to silence an unexplained API change.

After the baseline is approved and committed, verify it with:

```text
dotnet run --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj -c Release -- --check
```

Compare the public contract of two built assemblies:

```text
dotnet run --project tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj -c Release -- --compare bin/Release/net8.0/Icod.TermInfo.dll bin/Release/net10.0/Icod.TermInfo.dll
```

T43 release validation uses this mode to require exact public API equivalence
between the `net8.0` and `net10.0` package targets.

Once the T42 baseline is reviewed and committed, release validation also runs
`--check`. The release/package gate therefore fails rather than silently
blessing an unexplained public API change.
