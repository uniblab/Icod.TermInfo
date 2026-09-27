# Icod.TermInfo.Catalogs

The optional model foundation for the Icod.TermInfo 1.17 unified directory/hashed
catalog release. Targets .NET 8, 9, and 10; licensed LGPL-3.0-or-later.

UC01 supplies immutable source descriptors, read options, publication/result
models, typed issues, and limit exceptions. The common reader will arrive in
UC04; this foundation does not yet enumerate catalogs through this package.

```csharp
using Icod.TermInfo.Catalogs;

var source = new TerminalCatalogSource(
    "./terminfo", TerminalCatalogSourceKind.ConventionalDirectory);
var options = new TerminalCatalogReadOptions(
    maximumCandidateCount: 10_000, maximumParsedBytes: 16 * 1024 * 1024);
```

Source paths are resolved once at construction; selecting a source performs no
I/O. Publication names represent observed keys or files, while a terminal's alias
list contains declarations. Results preserve source provenance and report issues
and duplicate publications without selecting a winner.

Budget maxima are inclusive and independent. Byte budgets count input work, not
exact managed heap usage. Cancellation and limit failures do not return partial
success. Directory/hashed migration, precedence, comparison, and new JSON output
remain outside this release.

Production dependencies are Icod.TermInfo, Icod.TermInfo.Inspection, and
Icod.TermInfo.BerkeleyDb. No native Berkeley DB runtime is required or bundled.
