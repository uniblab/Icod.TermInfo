/*
	UC06 isolated installed-package consumer
	Copyright (C) 2026 Timothy J. Bruce <uniblab@hotmail.com>
	SPDX-License-Identifier: LGPL-3.0-or-later
*/

using System.Buffers.Binary;
using System.Text;
using Icod.TermInfo.BerkeleyDb;
using Icod.TermInfo.Catalogs;

const string canonical = "uc06-package";
const string alias = "uc06-alias";
const string unpublished = "uc06-unpublished";
string root = Path.Combine( Path.GetTempPath(), "Icod.TermInfo.Catalogs.PackageSmoke." + Guid.NewGuid().ToString( "N" ) );
string directory = Path.Combine( root, "directory" );
string database = Path.Combine( root, "database.db" );
string invalid = Path.Combine( root, "invalid.db" );
try {
	Directory.CreateDirectory( Path.Combine( directory, "75" ) );
	byte[] directoryBytes = Compiled( canonical, alias, unpublished );
	File.WriteAllBytes( Path.Combine( directory, "75", canonical ), directoryBytes );
	File.WriteAllBytes( Path.Combine( directory, "75", alias ), directoryBytes );
	BerkeleyDbTerminalDatabaseWriter.Write(
		database,
		[new BerkeleyDbTerminalDatabaseEntry( canonical, [alias], Compiled( canonical, alias ) )]
	);

	TerminalCatalogSource directorySource = new( directory, TerminalCatalogSourceKind.ConventionalDirectory );
	TerminalCatalogSource hashedSource = new( database, TerminalCatalogSourceKind.BerkeleyDbHash );
	TerminalCatalogReader directoryReader = new( directorySource );
	TerminalCatalogReader hashedReader = new( hashedSource );
	VerifyRows( directoryReader.Read(), expectPath: true );
	VerifyRows( hashedReader.Read(), expectPath: false );
	Require( directoryReader.Read().Entries.Count == 2, "The second directory read must be fresh." );

	TerminalCatalogReader bounded = new( hashedSource, new TerminalCatalogReadOptions( maximumEntryCount: 1 ) );
	bool limit = false;
	try { bounded.Read(); }
	catch ( TerminalCatalogLimitException error ) {
		limit = error.LimitName == "MaximumEntryCount" && error.Limit == 1;
	}
	Require( limit && hashedReader.Read().Entries.Count == 2, "Limit failure must be typed and a retry must succeed." );

	File.WriteAllBytes( Path.Combine( directory, "75", "uc06-broken" ), [1, 2, 3] );
	TerminalCatalog partial = directoryReader.Read();
	Require(
		partial.Status == TerminalCatalogStatus.Partial && partial.Entries.Count == 2
			&& partial.Issues.Count == 1 && partial.Issues[0].Kind == TerminalCatalogIssueKind.MalformedEntry,
		"A bad directory sibling must produce Partial without losing valid publications."
	);

	File.WriteAllBytes( invalid, [1, 2, 3] );
	TerminalCatalog failed = new TerminalCatalogReader( new( invalid, TerminalCatalogSourceKind.BerkeleyDbHash ) ).Read();
	Require(
		failed.Status == TerminalCatalogStatus.InvalidStore && failed.Entries.Count == 0
			&& failed.Issues.Single().Kind == TerminalCatalogIssueKind.InvalidHashedStore,
		"Invalid Hash-v9 data must fail closed."
	);
	Console.WriteLine(
		"UC06 isolated Catalogs package consumer passed on "
			+ System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription
	);
	return 0;
} finally {
	if ( Directory.Exists( root ) ) {
		Directory.Delete( root, recursive: true );
	}
}

static byte[] Compiled( string canonicalName, params string[] aliases ) {
	string identity = string.Join( '|', aliases.Prepend( canonicalName ) ) + "|UC06 installed consumer\0";
	byte[] names = Encoding.Latin1.GetBytes( identity );
	byte[] bytes = new byte[12 + names.Length + ( names.Length & 1 )];
	BinaryPrimitives.WriteUInt16LittleEndian( bytes.AsSpan( 0, 2 ), 0x011A );
	BinaryPrimitives.WriteUInt16LittleEndian( bytes.AsSpan( 2, 2 ), checked( (ushort)names.Length ) );
	names.CopyTo( bytes.AsSpan( 12 ) );
	return bytes;
}

static void VerifyRows( TerminalCatalog catalog, bool expectPath ) {
	Require(
		catalog.Status == TerminalCatalogStatus.Complete && catalog.Entries.Count == 2,
		"Expected exactly two observed publications."
	);
	Require(
		catalog.Entries.Select( entry => entry.PublicationName )
			.SequenceEqual( new[] { "uc06-alias", "uc06-package" }, StringComparer.Ordinal ),
		"An unpublished declaration is not a publication."
	);
	foreach ( TerminalCatalogEntry entry in catalog.Entries ) {
		Require(
			entry.Terminal.Name == "uc06-package" && ( entry.EntryPath is not null ) == expectPath
				&& entry.SourcePath == catalog.Source.Path && entry.Kind ==
					( entry.PublicationName == "uc06-package" ? TerminalCatalogEntryKind.Canonical : TerminalCatalogEntryKind.Alias ),
			"Packaged publication kind or provenance differs."
		);
	}
	if ( expectPath ) {
		Require(
			catalog.Entries[0].Terminal.Aliases.Contains( "uc06-unpublished", StringComparer.Ordinal ),
			"The directory terminal must retain the unpublished declaration."
		);
	}
}

static void Require( bool condition, string message ) {
	if ( !condition ) {
		throw new InvalidOperationException( message );
	}
}
