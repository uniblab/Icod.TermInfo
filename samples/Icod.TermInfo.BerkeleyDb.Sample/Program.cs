/*
	Icod.TermInfo.BerkeleyDb.Sample
	Demonstrates deterministic public Hash-v9 publication and acquisition.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

using System.Buffers.Binary;
using System.Text;
using Icod.TermInfo;
using Icod.TermInfo.BerkeleyDb;

const string canonicalName = "hdb09-sample";
const string aliasName = "hdb09-sample-alias";
const string description = "Icod HDB09 controlled sample";

string workDirectory = Path.Combine(
	Path.GetTempPath(),
	"Icod.TermInfo.BerkeleyDb.Sample." + Guid.NewGuid().ToString( "N" )
);
string databasePath = Path.Combine( workDirectory, "controlled-hash-v9.db" );

try {
	Directory.CreateDirectory( workDirectory );
	var entry = new BerkeleyDbTerminalDatabaseEntry(
		canonicalName, [ aliasName ], CreateCompiledEntry( canonicalName, description, [ aliasName ] )
	);
	var second = new BerkeleyDbTerminalDatabaseEntry(
		"hw08-sample-second", [], CreateCompiledEntry( "hw08-sample-second", "Second publication", [] )
	);
	BerkeleyDbTerminalDatabaseWriter.Write( databasePath, [ entry, second ] );
	string reorderedPath = Path.Combine( workDirectory, "reordered.db" );
	BerkeleyDbTerminalDatabaseWriter.Write( reorderedPath, [ second, entry ] );
	if ( !File.ReadAllBytes( databasePath ).SequenceEqual( File.ReadAllBytes( reorderedPath ) ) ) {
		throw new InvalidOperationException( "Publication order changed the database image." );
	}

	var provider = new BerkeleyDbTerminalDescriptionProvider( databasePath );
	if (
		!provider.TryLoad( aliasName, out TerminalDescription? terminal )
		|| terminal is null
	) {
		throw new InvalidOperationException(
			"The controlled Hash-v9 alias was not resolved."
		);
	}
	if (
		!string.Equals( terminal.Name, canonicalName, StringComparison.Ordinal )
		|| !terminal.Aliases.Contains( aliasName, StringComparer.Ordinal )
	) {
		throw new InvalidOperationException(
			"The resolved compiled identity did not match the controlled fixture."
		);
	}

	Console.WriteLine( $"Database: {Path.GetFileName( databasePath )}" );
	Console.WriteLine( "Publications: 2; deterministic across input order" );
	Console.WriteLine( $"Requested: {aliasName}" );
	Console.WriteLine( $"Canonical: {terminal.Name}" );
	Console.WriteLine( $"Description: {terminal.Description}" );
	Console.WriteLine(
		$"Colors: {terminal.GetNumber( NumericCapability.Colors )?.ToString() ?? "(absent)"}"
	);
} finally {
	if ( Directory.Exists( workDirectory ) ) {
		Directory.Delete( workDirectory, recursive: true );
	}
}

return 0;

static byte[] CreateCompiledEntry(
	string canonical,
	string description,
	string[] aliases
) {
	string identity = string.Join( "|", aliases.Prepend( canonical ) )
		+ "|" + description + "\0";
	byte[] names = Encoding.Latin1.GetBytes( identity );
	int length = 12 + names.Length;
	if ( ( length & 1 ) != 0 ) {
		length++;
	}

	byte[] entry = new byte[length];
	BinaryPrimitives.WriteUInt16LittleEndian( entry.AsSpan( 0, 2 ), 0x011A );
	BinaryPrimitives.WriteUInt16LittleEndian(
		entry.AsSpan( 2, 2 ),
		checked( (ushort)names.Length )
	);
	names.CopyTo( entry.AsSpan( 12 ) );
	return entry;
}
