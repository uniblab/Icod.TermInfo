# Unified Catalog Sample

This deterministic, non-interactive sample creates disposable conventional
directory and managed Hash-v9 catalogs and reads both through
`TerminalCatalogReader`. It uses public Compiler and BerkeleyDb writers only
to build controlled inputs. Neither native Berkeley DB nor a machine's ambient
terminfo database is needed.

From the repository root:

```text
dotnet run --project samples/Icod.TermInfo.Catalogs.Sample/Icod.TermInfo.Catalogs.Sample.csproj -f net10.0
dotnet run --project samples/Icod.TermInfo.Catalogs.Sample/Icod.TermInfo.Catalogs.Sample.csproj -f net10.0 -- --verify
```

Replace `net10.0` with `net8.0` or `net9.0` for the other supported targets.
The second command checks the observations and exits nonzero on failure.

Expected human output on Linux/macOS (temporary paths are replaced with
`<temp>`; Windows uses `\\` as its path separator):

```text
ConventionalDirectory: Complete at <temp>/directory
  uc06-alias [Alias] canonical=uc06-sample source=<temp>/directory entry=<temp>/directory/75/uc06-alias
  uc06-sample [Canonical] canonical=uc06-sample source=<temp>/directory entry=<temp>/directory/75/uc06-sample
BerkeleyDbHash: Complete at <temp>/catalog.db
  uc06-alias [Alias] canonical=uc06-sample source=<temp>/catalog.db entry=(none)
  uc06-sample [Canonical] canonical=uc06-sample source=<temp>/catalog.db entry=(none)
ConventionalDirectory: Partial at <temp>/directory
  uc06-alias [Alias] canonical=uc06-sample source=<temp>/directory entry=<temp>/directory/75/uc06-alias
  uc06-sample [Canonical] canonical=uc06-sample source=<temp>/directory entry=<temp>/directory/75/uc06-sample
  issue=MalformedEntry source=<temp>/directory entry=<temp>/directory/75/uc06-broken
Cancellation, inclusive limits and fresh retry verified.
```

The directory terminal declares `uc06-unpublished`, but that name has no
physical file, so it produces no row. The Hash-v9 writer publishes all aliases
declared in its payload; this fixture declares only `uc06-alias`. The common
print routine reports observed publications and provenance, not inferred
declarations. A malformed directory sibling changes the status to `Partial`
while retaining the two valid rows. Pre-cancelled reads and one-below limits
throw instead of returning partial catalogs, and the next read is fresh.

For the complete contract and the difference between publication names and
terminal declarations, see the [unified catalog guide](../../docs/1.17.0-UNIFIED-CATALOG-GUIDE.md).
