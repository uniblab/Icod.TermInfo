# Icod.TermInfo.Catalogs

The optional composition package for the Icod.TermInfo 1.17.0 unified
directory/hashed catalog release.
Targets .NET 8, 9, and 10; licensed LGPL-3.0-or-later.

UC01 supplies immutable source descriptors, read options, publication/result
models, typed issues, and limit exceptions. UC02 adds the internal adapter for
conventional directories. UC03 adds the internal hashed adapter, composing
BerkeleyDb's public bounded read API. UC04 adds the public reader to select one
explicit source and acquire a fresh catalog on each call.
UC05 exercises aggregate limits, cancellation, malformed and replaced sources,
links, and frozen old-reader behavior without changing the public contract.
UC06 adds the controlled two-format sample, installed-package consumer, and
usage guide without changing the reader contract.

The hashed adapter maps actual canonical/alias keys, retaining their Runtime
terminal objects and database path; `EntryPath` is null. Declared aliases do not
create rows. Invalid images, index chains, identities, or orphan storage records
reject the whole store with one `InvalidHashedStore` issue and zero entries.
Missing, directory-as-file, permission, and I/O outcomes remain distinct. Typed
limits and cancellation throw without a catalog; only `MaximumPublicationCount`
is renamed to the common `MaximumEntryCount`, with the original exception retained.

```csharp
using Icod.TermInfo.Catalogs;

var source = new TerminalCatalogSource(
    "./terminfo", TerminalCatalogSourceKind.ConventionalDirectory);
var options = new TerminalCatalogReadOptions(
    maximumCandidateCount: 10_000, maximumParsedBytes: 16 * 1024 * 1024);
var reader = new TerminalCatalogReader(source, options);
TerminalCatalog catalog = reader.Read();
foreach (TerminalCatalogEntry entry in catalog.Entries)
    Console.WriteLine($"{entry.PublicationName}: {entry.Terminal.Name}");

// Select a hashed file explicitly; a CancellationToken can stop a bounded read.
var hashed = new TerminalCatalogReader(new TerminalCatalogSource(
    "./terminfo.db", TerminalCatalogSourceKind.BerkeleyDbHash));
using var cancellation = new CancellationTokenSource();
TerminalCatalog hashedCatalog = hashed.Read(cancellation.Token);
```

Source paths are resolved once at construction; selecting a source performs no
I/O. Publication names represent observed keys or files, while a terminal's alias
list contains declarations. Results preserve source provenance and report issues
and duplicate publications without selecting a winner.
The caller checks `Status`: Complete for a valid readable source, Partial for
directory candidate issues, Missing, UnsupportedSource, Unavailable, or
InvalidStore for malformed hashed data. Limits raise `TerminalCatalogLimitException`
with no partial result; cancellation raises `OperationCanceledException`. A read
holds no source handle afterward and does not promise an atomic snapshot during
concurrent source replacement. Directory entries retain absolute `EntryPath`;
hashed keys retain the database `SourcePath` and a null `EntryPath`.
Cancellation is checked around synchronous filesystem operations, but cannot
interrupt a blocked OS call or guarantee an atomic replacement snapshot.

The directory adapter uses bounded Inspection observations. A canonical file
declaring two aliases contributes one row; separately published alias files add
their own rows. Literal and hexadecimal copies of the same publication are all
retained, with one duplicate issue per repeated name and Partial status. Different
aliases of the same canonical terminal are not duplicate publications. Misplaced
files are excluded from unified rows while their original diagnostics remain.

Directory candidate errors or skipped child links produce Partial results with
the valid observed rows, which may be empty. Missing, unsupported, and unavailable
roots remain distinct outcomes. Physical acquisition issues and duplicate issues
share one issue budget. Filtered parses, malformed input and separate alias copies
still consume their applicable acquisition budgets; normalization refunds none.

Budget maxima are inclusive and independent. Byte budgets count input work, not
exact managed heap usage. Cancellation and limit failures do not return partial
success. Directory/hashed migration, precedence, comparison, and new JSON output
remain outside this release.

Production dependencies are Icod.TermInfo, Icod.TermInfo.Inspection, and
Icod.TermInfo.BerkeleyDb. No native Berkeley DB runtime is required or bundled.

The [unified catalog guide](../docs/1.17.0-UNIFIED-CATALOG-GUIDE.md) explains
publication versus declaration, status/issue handling, limits and compatibility.
Run the [controlled directory/Hash-v9 sample](../samples/Icod.TermInfo.Catalogs.Sample/README.md)
on any supported TFM to inspect both formats through the same reader.
