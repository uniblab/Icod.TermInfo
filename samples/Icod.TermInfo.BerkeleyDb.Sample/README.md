# Icod.TermInfo BerkeleyDb Sample

This deterministic, non-interactive sample demonstrates public Hash-v9
publication followed by explicit terminal acquisition from a controlled file.

The sample publishes two compiled entries with
`BerkeleyDbTerminalDatabaseWriter.Write`, repeats publication in reversed input
order, and checks byte-for-byte equality. It then constructs a
`BerkeleyDbTerminalDescriptionProvider`, requests a controlled alias, and prints
the canonical identity, description, and selected capability state returned by
Runtime's compiled-entry parser. It removes the temporary fixture on exit.

The sample prepares minimal compiled-entry bytes locally so it needs only the
BerkeleyDb package and Runtime. Applications can supply compiled bytes produced
by `Icod.TermInfo.Compiler`. Hash pages are produced by the public writer.
Publication creates a persistent sibling lock; the sample removes its entire
private temporary directory after all operations finish. The sample uses no
native Berkeley DB library, no native subprocess, no ambient terminfo database,
and no network access.

Run any reusable target framework:

```text
dotnet run --project samples/Icod.TermInfo.BerkeleyDb.Sample/Icod.TermInfo.BerkeleyDb.Sample.csproj -f net10.0
```

Substitute `-f net8.0` or `-f net9.0` to exercise the other package targets.
The stable output identifies `hdb09-sample-alias` as the requested publication
and `hdb09-sample` as the canonical terminal.

Expected output:

```text
Database: controlled-hash-v9.db
Publications: 2; deterministic across input order
Requested: hdb09-sample-alias
Canonical: hdb09-sample
Description: Icod HDB09 controlled sample
Colors: (absent)
```

For the full acquisition and compatibility boundary, see:

- `../../docs/1.16.0-BERKELEY-DB-WRITING-GUIDE.md`;
- `../../docs/1.15.0-BERKELEY-DB-HASHED-ACQUISITION-GUIDE.md`; and
- `../../docs/1.15.0-BERKELEY-DB-HASH-V9-COMPATIBILITY.md`.
