/*
	Icod.TermInfo.Catalogs.Sample
	Demonstrates explicit unified reads from controlled directory and Hash-v9 sources.
	Copyright (C) 2026 Timothy J. Bruce <uniblab@hotmail.com>
	SPDX-License-Identifier: LGPL-3.0-or-later
*/

using Icod.TermInfo;
using Icod.TermInfo.BerkeleyDb;
using Icod.TermInfo.Catalogs;
using Icod.TermInfo.Compiler;

if ( args.Length > 1 || ( args.Length == 1 && args[0] != "--verify" ) ) {
	Console.Error.WriteLine( "Usage: Icod.TermInfo.Catalogs.Sample [--verify]" );
	return 2;
}

const string canonical = "uc06-sample";
const string alias = "uc06-alias";
const string unpublished = "uc06-unpublished";
string root = Path.Combine( Path.GetTempPath(), "Icod.TermInfo.Catalogs.Sample." + Guid.NewGuid().ToString( "N" ) );
string directoryPath = Path.Combine( root, "directory" );
string hashedPath = Path.Combine( root, "catalog.db" );
try {
	Directory.CreateDirectory( root );
	TerminalDescription directoryTerminal = new TerminalDescriptionBuilder( canonical )
		.SetDescription( "Controlled UC06 directory sample" )
		.AddAlias( alias ).AddAlias( unpublished ).Build();
	CompiledTermInfoDatabaseWriter.Write( directoryPath, directoryTerminal );
	// A declaration does not imply publication. The writer published all aliases;
	// remove one physical file to model a directory with a declared-only alias.
	File.Delete( Path.Combine( directoryPath, "75", unpublished ) );

	TerminalDescription hashedTerminal = new TerminalDescriptionBuilder( canonical )
		.SetDescription( "Controlled UC06 hashed sample" ).AddAlias( alias ).Build();
	BerkeleyDbTerminalDatabaseWriter.Write(
		hashedPath,
		[new BerkeleyDbTerminalDatabaseEntry( canonical, [alias], CompiledTermInfoWriter.Write( hashedTerminal ) )]
	);

	var directorySource = new TerminalCatalogSource( directoryPath, TerminalCatalogSourceKind.ConventionalDirectory );
	var hashedSource = new TerminalCatalogSource( hashedPath, TerminalCatalogSourceKind.BerkeleyDbHash );
	var directoryReader = new TerminalCatalogReader( directorySource );
	var hashedReader = new TerminalCatalogReader( hashedSource );
	TerminalCatalog directory = directoryReader.Read();
	TerminalCatalog hashed = hashedReader.Read();
	CheckRows( directory, directoryPath, expectEntryPath: true );
	CheckRows( hashed, hashedPath, expectEntryPath: false );
	Require(
		directory.Entries[0].Terminal.Aliases.Contains( unpublished, StringComparer.Ordinal ),
		"The directory terminal must declare its unpublished alias."
	);
	Require(
		!directory.Entries.Any( entry => entry.PublicationName == unpublished ),
		"An unpublished declaration appeared as a directory publication."
	);

	File.WriteAllBytes( Path.Combine( directoryPath, "75", "uc06-broken" ), [1, 2, 3] );
	TerminalCatalog partial = directoryReader.Read();
	Require(
		partial.Status == TerminalCatalogStatus.Partial && partial.Entries.Count == 2,
		"The malformed directory sibling must retain valid publications with Partial status."
	);
	Require(
		partial.Issues.Count == 1 && partial.Issues[0].Kind == TerminalCatalogIssueKind.MalformedEntry,
		"The malformed sibling must have a typed issue."
	);

	using var cancelled = new CancellationTokenSource();
	cancelled.Cancel();
	bool cancellationObserved = false;
	try { hashedReader.Read( cancelled.Token ); }
	catch ( OperationCanceledException ) { cancellationObserved = true; }
	Require( cancellationObserved, "An already-cancelled read must throw." );

	var exactReader = new TerminalCatalogReader( hashedSource, new TerminalCatalogReadOptions( maximumEntryCount: 2 ) );
	Require( exactReader.Read().Entries.Count == 2, "The inclusive publication limit must accept two rows." );
	var limitedReader = new TerminalCatalogReader( hashedSource, new TerminalCatalogReadOptions( maximumEntryCount: 1 ) );
	bool limitObserved = false;
	try { limitedReader.Read(); }
	catch ( TerminalCatalogLimitException exception ) {
		limitObserved = exception.LimitName == "MaximumEntryCount" && exception.Limit == 1;
	}
	Require( limitObserved, "One fewer publication slot must throw its named limit." );
	Require( hashedReader.Read().Entries.Count == 2, "A failed read must not consume the next read's budget." );

	if ( args.Length == 0 ) {
		PrintCatalog( directory, root );
		PrintCatalog( hashed, root );
		PrintCatalog( partial, root );
		Console.WriteLine( "Cancellation, inclusive limits and fresh retry verified." );
	} else {
		Console.WriteLine( "UC06 unified sample verified on " + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription );
	}
	return 0;
} finally {
	if ( Directory.Exists( root ) ) {
		Directory.Delete( root, recursive: true );
	}
}

static void Require( bool condition, string message ) {
	if ( !condition ) {
		throw new InvalidOperationException( message );
	}
}

static void CheckRows( TerminalCatalog catalog, string path, bool expectEntryPath ) {
	Require(
		catalog.Status == TerminalCatalogStatus.Complete && catalog.Entries.Count == 2,
		"Expected exactly the canonical and published alias rows."
	);
	Require(
		catalog.Entries.Select( entry => entry.PublicationName )
			.SequenceEqual( new[] { "uc06-alias", "uc06-sample" }, StringComparer.Ordinal ),
		"The observed publication names must be sorted and must exclude declarations alone."
	);
	foreach ( TerminalCatalogEntry entry in catalog.Entries ) {
		Require(
			entry.SourcePath == path && ( entry.EntryPath is not null ) == expectEntryPath,
			"The publication must report truthful source and entry provenance."
		);
		Require(
			entry.Terminal.Name == "uc06-sample" && entry.Kind ==
				( entry.PublicationName == "uc06-sample" ? TerminalCatalogEntryKind.Canonical : TerminalCatalogEntryKind.Alias ),
			"The observed publication kind and declared canonical identity differ."
		);
	}
}

static void PrintCatalog( TerminalCatalog catalog, string temporaryRoot ) {
	string Display( string? path ) => path is null ? "(none)" : path.Replace( temporaryRoot, "<temp>", StringComparison.Ordinal );
	Console.WriteLine( $"{catalog.Source.Kind}: {catalog.Status} at {Display( catalog.Source.Path )}" );
	foreach ( TerminalCatalogEntry entry in catalog.Entries ) {
		Console.WriteLine( $"  {entry.PublicationName} [{entry.Kind}] canonical={entry.Terminal.Name} source={Display( entry.SourcePath )} entry={Display( entry.EntryPath )}" );
	}
	foreach ( TerminalCatalogIssue issue in catalog.Issues ) {
		Console.WriteLine( $"  issue={issue.Kind} source={Display( issue.SourcePath )} entry={Display( issue.EntryPath )}" );
	}
}
