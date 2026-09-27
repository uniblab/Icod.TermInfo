# Icod.TermInfo.BerkeleyDb

Pure-managed acquisition and deterministic publication for ncurses-compatible
Berkeley DB Hash-v9 terminfo stores. Version 1.16 adds whole-file writing to the
unchanged 1.15 reader API. The package targets .NET 8, 9, and 10 and depends only
on matching-version `Icod.TermInfo` Runtime. No native library is required.

## Read an explicit database

```csharp
using Icod.TermInfo.BerkeleyDb;

var provider = new BerkeleyDbTerminalDescriptionProvider("./terminfo.db");
if (provider.TryLoad("xterm-256color", out var terminal))
    Console.WriteLine(terminal.Name);
```

Lookup tries the exact ordinal UTF-8 key first, then one distinct exact Latin-1
key after a clean miss when every character is representable. Malformed or
identity-invalid found records never become misses or fall through to another
encoding. Successful lookups are cached; misses and failures remain retryable.
After replacing a database, construct a new provider to observe changed content
that was previously loaded successfully; publication does not invalidate caches.

`BerkeleyDbTerminalCatalogReader` enumerates canonical and alias publications
from an explicit file with deterministic ordering. `BerkeleyDbSystemTerminalDescriptionProvider`
is the opt-in discovery provider: it preserves Runtime's environment/user/system
precedence while supporting exact hashed files and absent-location `.db`
companions. Runtime's conventional discovery remains unchanged.

Readers bound file size, record count where applicable, parser work, and index
hops. They support the qualified Hash-v9 inline/overflow subset in both byte
orders. Acquisition is not an atomic snapshot or general encoding detection.

## Publish compiled entries

```csharp
using Icod.TermInfo;
using Icod.TermInfo.BerkeleyDb;

byte[] data = File.ReadAllBytes("compiled-entry");
TerminalDescription parsed = CompiledTermInfoParser.Parse(data);
var entry = new BerkeleyDbTerminalDatabaseEntry(parsed.Name, parsed.Aliases, data);
BerkeleyDbTerminalDatabaseWriter.Write("./terminfo.db", new[] { entry });
```

The parent directory must exist. The default refuses an existing destination;
`new BerkeleyDbTerminalDatabaseWriterOptions(overwriteExisting: true)` permits
whole-file replacement. Supply exactly the entries to publish: writing does not
merge with the old database or update pages in place.

Entries copy their aliases and compiled bytes. Names must be portable, globally
unique identities and agree with the parsed payload, including alias order.
Logical keys are exact UTF-8; the writer does not manufacture Latin-1 aliases,
case-fold, or normalize names. Compiled bytes remain opaque validated payloads.
Output is deterministic across entry enumeration order and uses little-endian
Hash-v9, 4096-byte pages, mask-addressed primary buckets, collision chains, and
overflow pages.

`MaximumRecordCount` defaults to 65,536 and bounds source enumeration. Each
entry costs two records plus one per alias. `MaximumDatabaseSize` defaults to
64 MiB and bounds the image, not total heap usage. Parser limits are separate.
Caller-owned allocations and blocked caller enumeration are outside these bounds.

## Safe publication

The writer builds the complete image, acquires a cooperative exclusive lock,
writes and flushes a unique sibling file, closes and reopens it through the
production reader, verifies bytes/records/catalog, and commits by a
same-directory move. Pre-commit failures preserve the old destination or its
absence; owned temporary-file cleanup is best effort.

For `terminfo.db`, the persistent lock is `.terminfo.db.icod-terminfo.lock`.
Its presence does not mean a writer is active. Do not delete it while cooperating
writers might use the destination. Contention waits until acquisition or
cancellation. Cancellation is observed through the final pre-move check; a
successful commit remains successful if cancellation arrives during the move.

Replacement uses fresh metadata and inherited access controls. The immediate
parent, destination, and lock must not be symbolic links or reparse points.
Atomic visibility depends on supported filesystem move semantics. No universal
power-loss durability, hostile ancestor-substitution protection, or native-writer
coordination is promised. Windows readers may cause safe replacement refusal.
There is no copy/delete fallback.

## Commands and examples

```sh
icod-terminfo tic --database-format hashed -o ./terminfo.db source.ti
icod-terminfo tic --database-format hashed --force -s -o ./terminfo.db source.ti
icod-terminfo infocmp -A ./terminfo.db xterm-256color
```

Directory output remains the default. Explicit `infocmp -A/-B` files and human
`toe` file operands use hashed acquisition. Ambient discovery and frozen JSON
schemas retain their established behavior. Migration and catalog automation
remain deferred to 1.17.

- [Public writer sample](../samples/Icod.TermInfo.BerkeleyDb.Sample/README.md):
  controlled publication, alias lookup, and input-order determinism.
- [Writing guide](../docs/1.16.0-BERKELEY-DB-WRITING-GUIDE.md): API, commands,
  exceptions, resource limits, and filesystem scope.
- [Acquisition guide](../docs/1.15.0-BERKELEY-DB-HASHED-ACQUISITION-GUIDE.md):
  exact reader/discovery/catalog behavior.
- [API freeze](../docs/1.16.0-BERKELEYDB-PUBLIC-API-FREEZE.md):
  12 exported public types; subtracting the three writer types reconstructs the
  frozen 1.15 reader API.
- [Release audit](../docs/1.16.0-RELEASE-AUDIT.md): exact candidate evidence.

The package remains LGPL-3.0-or-later. Native Berkeley DB and pinned ncurses are
CI oracles only; no native code or runtime assets are shipped.
