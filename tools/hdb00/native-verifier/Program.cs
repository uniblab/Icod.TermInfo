/*
	Hdb07c.NativeVerifier
	Verifies CI-generated native Berkeley DB evidence without runtime dependencies.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

if ( args.Length != 3 ) {
	Console.Error.WriteLine(
		"Usage: Hdb07c.NativeVerifier DATABASE SOURCE_DUMP DESTINATION_DUMP"
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

static string[] ReadRecords( string path ) {
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

	string[] records = new string[dataLines.Length / 2];
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
		records[index / 2] =
			Convert.ToHexString( key )
			+ ":"
			+ Convert.ToHexString( value )
		;
	}

	Array.Sort( records, StringComparer.Ordinal );
	return records;
}
