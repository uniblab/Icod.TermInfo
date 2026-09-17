# Icod.TermInfo BerkeleyDb Sample

This deterministic, non-interactive sample demonstrates explicit read-only
terminal acquisition from one controlled Berkeley DB Hash-v9 file.

The sample creates a small fixture in a temporary directory, then constructs a
`BerkeleyDbTerminalDescriptionProvider`, requests a controlled alias, and prints
the canonical identity, description, and selected capability state returned by
Runtime's compiled-entry parser. It removes the temporary fixture on exit.

The fixture writer is demonstration setup only. `Icod.TermInfo.BerkeleyDb`
remains read-only production code and exposes no write API. The sample uses no
native Berkeley DB library, no native subprocess, no ambient terminfo database,
and no network access.

Run any reusable target framework:

```text
dotnet run --project samples/Icod.TermInfo.BerkeleyDb.Sample/Icod.TermInfo.BerkeleyDb.Sample.csproj -f net10.0
```

Substitute `-f net8.0` or `-f net9.0` to exercise the other package targets.
The stable output identifies `hdb09-sample-alias` as the requested publication
and `hdb09-sample` as the canonical terminal.

For the full acquisition and compatibility boundary, see:

- `../../docs/1.15.0-BERKELEY-DB-HASHED-ACQUISITION-GUIDE.md`; and
- `../../docs/1.15.0-BERKELEY-DB-HASH-V9-COMPATIBILITY.md`.
