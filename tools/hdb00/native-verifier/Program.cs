/*
	Hdb07c.NativeVerifier
	Verifies CI-generated native Berkeley DB evidence without runtime dependencies.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

if (
	args.Length == 4
	&& string.Equals(
		args[0],
		"latin1",
		StringComparison.Ordinal
	)
) {
	VerifyLatin1Dump( args[1], args[2], args[3] );
	return 0;
}

if ( args.Length != 3 ) {
	Console.Error.WriteLine(
		"Usage: Hdb07c.NativeVerifier DATABASE SOURCE_DUMP DESTINATION_DUMP\n"
		+ "   or: Hdb07c.NativeVerifier latin1 DUMP CANONICAL_OUTPUT ALIAS_OUTPUT"
	);
	return 64;
}

byte[] database = File.ReadAllBytes( args[0] );
if ( database.Length < 512 ) {
	throw new InvalidDataException(
		"Database is smaller than one metadata page."
	);
}

ReadOnlySpan<byte> expectedMagic = [ 0x00, 0x06, 0x15, 0x61 ];
ReadOnlySpan<byte> actualMagic = database.AsSpan( 12, 4 );
if ( !actualMagic.SequenceEqual( expectedMagic ) ) {
	throw new InvalidDataException(
		$"Expected big-endian Hash magic {Convert.ToHexString( expectedMagic )}, got {Convert.ToHexString( actualMagic )}."
	);
}

string[] source = ReadRecords( args[1] );
string[] destination = ReadRecords( args[2] );
if ( !source.SequenceEqual( destination, StringComparer.Ordinal ) ) {
	throw new InvalidDataException(
		"Big-endian repack changed one or more exact records."
	);
}
if ( destination.Length != 3 ) {
	throw new InvalidDataException(
		$"Expected 3 big-endian records, got {destination.Length}."
	);
}

Console.WriteLine( "HDB07C byte order: big-endian" );
Console.WriteLine( "HDB07C native big-endian records: 3" );
return 0;

static void VerifyLatin1Dump(
	string dumpPath,
	string canonicalOutput,
	string aliasOutput
) {
	List<( byte[] Key, byte[] Value )> records =
		ReadRecordPairs( dumpPath );
	if ( records.Count != 3 ) {
		throw new InvalidDataException(
			$"Expected 3 Latin-1 records, got {records.Count}."
		);
	}

	int storageCount = records.Count(
		static record =>
			record.Value.Length > 0
			&& record.Value[0] == 0
	);
	int publicationCount = records.Count(
		static record =>
			record.Value.Length > 0
			&& record.Value[0] == 2
	);
	if (
		storageCount != 1
		|| publicationCount != 2
	) {
		throw new InvalidDataException(
			$"Expected one Latin-1 storage record and two publications, got {storageCount} and {publicationCount}."
		);
	}

	byte[] canonicalKey = Convert.FromHexString(
		"6864623037632D636166E9"
	);
	byte[] aliasKey = Convert.FromHexString(
		"6864623037632D616C69E9"
	);
	byte[] canonical = ResolveCompiledEntry( records, canonicalKey );
	byte[] alias = ResolveCompiledEntry( records, aliasKey );
	if ( !canonical.AsSpan().SequenceEqual( alias ) ) {
		throw new InvalidDataException(
			"The Latin-1 canonical and alias publications resolve to different compiled entries."
		);
	}

	File.WriteAllBytes( canonicalOutput, canonical );
	File.WriteAllBytes( aliasOutput, alias );
	Console.WriteLine( "HDB07C native Latin-1 records: 3" );
	Console.WriteLine( "HDB07C native Latin-1 publications: 2" );
}

static byte[] ResolveCompiledEntry(
	List<( byte[] Key, byte[] Value )> records,
	byte[] publicationKey
) {
	( byte[] _, byte[] publicationValue ) = FindRecord(
		records,
		publicationKey
	);
	if (
		publicationValue.Length < 2
		|| publicationValue[0] != 2
	) {
		throw new InvalidDataException(
			$"Expected marker-2 publication {Convert.ToHexString( publicationKey )}."
		);
	}

	( byte[] _, byte[] storageValue ) = FindRecord(
		records,
		publicationValue[1..]
	);
	if (
		storageValue.Length < 2
		|| storageValue[0] != 0
	) {
		throw new InvalidDataException(
			$"Publication {Convert.ToHexString( publicationKey )} does not reference one marker-0 compiled record."
		);
	}
	return storageValue[1..];
}

static ( byte[] Key, byte[] Value ) FindRecord(
	List<( byte[] Key, byte[] Value )> records,
	ReadOnlySpan<byte> key
) {
	foreach ( ( byte[] recordKey, byte[] recordValue ) in records ) {
		if ( key.SequenceEqual( recordKey ) ) {
			return ( recordKey, recordValue );
		}
	}

	throw new InvalidDataException(
		$"Native dump is missing exact key {Convert.ToHexString( key )}."
	);
}

static string[] ReadRecords( string path ) {
	List<( byte[] Key, byte[] Value )> pairs =
		ReadRecordPairs( path );
	string[] records = new string[pairs.Count];
	for ( int index = 0; index < pairs.Count; index++ ) {
		records[index] =
			Convert.ToHexString( pairs[index].Key )
			+ ":"
			+ Convert.ToHexString( pairs[index].Value )
		;
	}

	Array.Sort( records, StringComparer.Ordinal );
	return records;
}

static List<( byte[] Key, byte[] Value )> ReadRecordPairs(
	string path
) {
	string[] lines = File.ReadAllLines( path );
	int headerEnd = Array.IndexOf( lines, "HEADER=END" );
	if ( headerEnd < 0 ) {
		throw new InvalidDataException(
			$"{path}: missing HEADER=END"
		);
	}
	if ( lines.Length == 0 || lines[^1] != "DATA=END" ) {
		throw new InvalidDataException(
			$"{path}: missing DATA=END"
		);
	}

	string[] header = lines[..headerEnd];
	foreach ( string expected in new[] {
		"VERSION=3",
		"format=bytevalue",
		"type=hash",
	} ) {
		if ( !header.Contains( expected, StringComparer.Ordinal ) ) {
			throw new InvalidDataException(
				$"{path}: missing {expected}"
			);
		}
	}

	string[] dataLines = lines[( headerEnd + 1 )..^1];
	if ( dataLines.Length == 0 || ( dataLines.Length & 1 ) != 0 ) {
		throw new InvalidDataException(
			$"{path}: malformed key/value lines"
		);
	}

	var records = new List<( byte[] Key, byte[] Value )>(
		dataLines.Length / 2
	);
	for ( int index = 0; index < dataLines.Length; index += 2 ) {
		if (
			!dataLines[index].StartsWith( ' ' )
			|| !dataLines[index + 1].StartsWith( ' ' )
		) {
			throw new InvalidDataException(
				$"{path}: malformed bytevalue line"
			);
		}

		byte[] key = Convert.FromHexString( dataLines[index][1..] );
		byte[] value = Convert.FromHexString( dataLines[index + 1][1..] );
		records.Add( ( key, value ) );
	}
	return records;
}
