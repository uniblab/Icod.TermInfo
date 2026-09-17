/*
	Icod.TermInfo.BerkeleyDb.Sample
	Demonstrates deterministic explicit Berkeley DB Hash-v9 acquisition.
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
	File.WriteAllBytes(
		databasePath,
		CreateCatalogStore( canonicalName, description, aliasName )
	);

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

static byte[] CreateCatalogStore(
	string canonical,
	string description,
	params string[] aliases
) {
	var names = new List<string> { canonical };
	names.AddRange( aliases );
	byte[] storageKey = Encoding.UTF8.GetBytes(
		string.Join( "|", names ) + "|" + description
	);
	var records = new List<( byte[] Key, byte[] Value )>();
	foreach ( string name in names ) {
		records.Add(
			(
				Encoding.UTF8.GetBytes( name ),
				PrependMarker( storageKey, 2 )
			)
		);
	}
	records.Add(
		(
			storageKey,
			PrependMarker(
				CreateCompiledEntry( canonical, description, aliases ),
				0
			)
		)
	);
	return CreateDatabase( records.ToArray() );
}

static byte[] CreateCompiledEntry(
	string canonical,
	string description,
	string[] aliases
) {
	string identity = canonical + "|" + string.Join( "|", aliases )
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

static byte[] PrependMarker( byte[] bytes, byte marker ) {
	byte[] value = new byte[bytes.Length + 1];
	value[0] = marker;
	bytes.CopyTo( value.AsSpan( 1 ) );
	return value;
}

static byte[] CreateDatabase(
	params ( byte[] Key, byte[] Value )[] records
) {
	const int pageSize = 512;
	byte[] database = new byte[pageSize * ( records.Length + 1 )];
	BinaryPrimitives.WriteUInt32LittleEndian(
		database.AsSpan( 12, 4 ),
		0x00061561
	);
	BinaryPrimitives.WriteUInt32LittleEndian( database.AsSpan( 16, 4 ), 9 );
	BinaryPrimitives.WriteUInt32LittleEndian(
		database.AsSpan( 20, 4 ),
		pageSize
	);
	database[25] = 8;
	BinaryPrimitives.WriteUInt32LittleEndian(
		database.AsSpan( 32, 4 ),
		checked( (uint)records.Length )
	);

	for ( int index = 0; index < records.Length; index++ ) {
		( byte[] key, byte[] value ) = records[index];
		Span<byte> page = database.AsSpan(
			pageSize * ( index + 1 ),
			pageSize
		);
		BinaryPrimitives.WriteUInt32LittleEndian(
			page.Slice( 8, 4 ),
			checked( (uint)( index + 1 ) )
		);
		page[25] = 13;
		ushort keyOffset = checked( (ushort)( 511 - key.Length ) );
		ushort valueOffset = checked(
			(ushort)( keyOffset - value.Length - 1 )
		);
		BinaryPrimitives.WriteUInt16LittleEndian( page.Slice( 20, 2 ), 2 );
		BinaryPrimitives.WriteUInt16LittleEndian(
			page.Slice( 22, 2 ),
			valueOffset
		);
		BinaryPrimitives.WriteUInt16LittleEndian(
			page.Slice( 26, 2 ),
			keyOffset
		);
		BinaryPrimitives.WriteUInt16LittleEndian(
			page.Slice( 28, 2 ),
			valueOffset
		);
		page[keyOffset] = 1;
		key.CopyTo( page[( keyOffset + 1 )..] );
		page[valueOffset] = 1;
		value.CopyTo( page[( valueOffset + 1 )..] );
	}
	return database;
}
