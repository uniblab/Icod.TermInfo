/*
	Icod.TermInfo.BerkeleyDb.Interop.Tests
	Compares the production reader with native Berkeley DB dump records.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.Text;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Interop.Tests;

public sealed class NativeOracleTests {
	[Theory]
	[InlineData( "hashed-db", 3 )]
	[InlineData( "overflow-hashed-db", 2 )]
	public void ProductionReaderMatchesEveryNativeRecord(
		string fixtureName,
		int expectedRecordCount
	) {
		List<( byte[] Key, byte[] Value )> records = ReadNativeRecords( fixtureName );
		Assert.Equal( expectedRecordCount, records.Count );
		string databasePath = FixturePath( fixtureName + ".db" );

		foreach ( ( byte[] key, byte[] expected ) in records ) {
			Assert.True(
				BerkeleyDbHashReader.TryReadValue( databasePath, key, out byte[] actual ),
				$"Native key {Convert.ToHexString( key )} was not found."
			);
			Assert.Equal( expected, actual );
		}
	}

	[Theory]
	[InlineData( "hashed-db" )]
	[InlineData( "overflow-hashed-db" )]
	public void ProductionReaderReturnsCleanMissForAbsentNativeKey( string fixtureName ) {
		byte[] key = Encoding.UTF8.GetBytes( "hdb00-missing" );
		List<( byte[] Key, byte[] Value )> records = ReadNativeRecords( fixtureName );
		Assert.NotEmpty( records );
		Assert.DoesNotContain(
			records,
			record => record.Key.AsSpan().SequenceEqual( key )
		);

		Assert.False(
			BerkeleyDbHashReader.TryReadValue(
				FixturePath( fixtureName + ".db" ),
				key,
				out byte[] actual
			)
		);
		Assert.Empty( actual );
	}

	[Theory]
	[InlineData( "not-hash.db" )]
	[InlineData( "random.db" )]
	public void ProductionReaderRejectsUnsupportedNativeInput( string fixtureName ) {
		Assert.Throws<InvalidDataException>(
			() => BerkeleyDbHashReader.TryReadValue(
				FixturePath( fixtureName ),
				new byte[] { 0x6B, 0x65, 0x79 },
				out _
			)
		);
	}

	[Theory]
	[InlineData( "hashed-db", "hdb00-primary", "hdb00-primary.bin" )]
	[InlineData( "hashed-db", "hdb00-alias", "hdb00-primary.bin" )]
	[InlineData( "overflow-hashed-db", "hdb00-overflow", "hdb00-overflow.bin" )]
	public void ProductionRecordResolverMatchesNativeCompiledEntry(
		string fixtureName,
		string terminalName,
		string expectedFile
	) {
		byte[] expected = File.ReadAllBytes( FixturePath( expectedFile ) );
		Assert.NotEmpty( expected );

		Assert.True( NcursesRecordReader.TryReadCompiledEntry(
			FixturePath( fixtureName + ".db" ),
			Encoding.UTF8.GetBytes( terminalName ),
			out byte[] actual
		) );

		Assert.Equal( expected, actual );
	}

	private static List<( byte[] Key, byte[] Value )> ReadNativeRecords( string fixtureName ) {
		string[] lines = File.ReadAllLines( FixturePath( fixtureName + ".dump" ) );
		int headerEnd = Array.IndexOf( lines, "HEADER=END" );
		Assert.True( headerEnd >= 3, "The native dump header is missing." );
		string[] header = lines[..headerEnd];
		Assert.Contains( "VERSION=3", header );
		Assert.Contains( "format=bytevalue", header );
		Assert.Contains( "type=hash", header );
		Assert.Equal( "DATA=END", lines[^1] );
		int dataLineCount = lines.Length - headerEnd - 2;
		Assert.True( dataLineCount > 0 && ( dataLineCount % 2 ) == 0 );

		List<( byte[] Key, byte[] Value )> records = [];
		for ( int index = headerEnd + 1; index < lines.Length - 1; index += 2 ) {
			Assert.StartsWith( " ", lines[index] );
			Assert.StartsWith( " ", lines[index + 1] );
			records.Add(
				(
					Convert.FromHexString( lines[index][1..] ),
					Convert.FromHexString( lines[index + 1][1..] )
				)
			);
		}
		return records;
	}

	private static string FixturePath( string name ) {
		string? root = Environment.GetEnvironmentVariable( "ICOD_HDB02_FIXTURE_ROOT" );
		if ( string.IsNullOrWhiteSpace( root ) ) {
			throw new InvalidOperationException(
				"ICOD_HDB02_FIXTURE_ROOT must point to native-generated HDB00 fixtures."
			);
		}
		return Path.Combine( root, name );
	}
}
