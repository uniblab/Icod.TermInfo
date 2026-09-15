/*
	Icod.TermInfo.BerkeleyDb.Tests
	Validates the HDB02 managed Berkeley DB Hash-v9 reader.
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

using System.Buffers.Binary;
using System.Text;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hdb02HashReaderTests {
	[Fact]
	public void TryReadValueFindsInlineValueByExactKey() {
		byte[] key = Encoding.UTF8.GetBytes( "xterm-hdb02" );
		byte[] expected = [ 0x10, 0x20, 0x30, 0x40 ];
		byte[] database = CreateInlineHashV9Database( key, expected );
		string path = Path.GetTempFileName();

		try {
			File.WriteAllBytes( path, database );

			bool found = BerkeleyDbHashReader.TryReadValue(
				path,
				key,
				out byte[] actual
			);

			Assert.True( found );
			Assert.Equal( expected, actual );
		} finally {
			File.Delete( path );
		}
	}

	[Fact]
	public void TryReadValueReconstructsOverflowValue() {
		byte[] key = Encoding.UTF8.GetBytes( "xterm-hdb02-overflow" );
		byte[] expected = Enumerable.Range( 0, 96 )
			.Select( index => (byte)index )
			.ToArray();
		byte[] database = CreateOverflowHashV9Database( key, expected );
		string path = Path.GetTempFileName();

		try {
			File.WriteAllBytes( path, database );

			bool found = BerkeleyDbHashReader.TryReadValue(
				path,
				key,
				out byte[] actual
			);

			Assert.True( found );
			Assert.Equal( expected, actual );
		} finally {
			File.Delete( path );
		}
	}

	[Fact]
	public void TryReadValueFindsBigEndianInlineValue() {
		byte[] key = Encoding.UTF8.GetBytes( "xterm-hdb02-big-endian" );
		byte[] expected = [ 0x55, 0x66, 0x77 ];
		byte[] database = CreateBigEndianInlineHashV9Database( key, expected );
		string path = Path.GetTempFileName();

		try {
			File.WriteAllBytes( path, database );

			bool found = BerkeleyDbHashReader.TryReadValue(
				path,
				key,
				out byte[] actual
			);

			Assert.True( found );
			Assert.Equal( expected, actual );
		} finally {
			File.Delete( path );
		}
	}

	[Theory]
	[InlineData( false, 0 )]
	[InlineData( false, 1 )]
	[InlineData( true, 0 )]
	[InlineData( true, 1 )]
	public void TryReadValueRejectsMismatchedPageIdentity(
		bool isBigEndian,
		int pageNumber
	) {
		byte[] key = Encoding.UTF8.GetBytes( "hdb02-page-identity" );
		byte[] database = ( isBigEndian )
			? CreateBigEndianInlineHashV9Database( key, [ 0x42 ] )
			: CreateInlineHashV9Database( key, [ 0x42 ] )
		;
		Span<byte> identity = database.AsSpan( ( pageNumber * 512 ) + 8, 4 );
		if ( isBigEndian ) {
			BinaryPrimitives.WriteUInt32BigEndian( identity, 99 );
		} else {
			BinaryPrimitives.WriteUInt32LittleEndian( identity, 99 );
		}

		AssertInvalidDatabase( database, key );
	}

	[Fact]
	public void TryReadValueRejectsMismatchedOverflowPageIdentity() {
		byte[] key = Encoding.UTF8.GetBytes( "hdb02-overflow-identity" );
		byte[] database = CreateOverflowHashV9Database( key, [ 0x42 ] );
		BinaryPrimitives.WriteUInt32LittleEndian(
			database.AsSpan( ( 2 * 512 ) + 8, 4 ),
			99
		);

		AssertInvalidDatabase( database, key );
	}

	[Theory]
	[InlineData( 12, 4, 0 )]
	[InlineData( 16, 4, 8 )]
	[InlineData( 20, 4, 0 )]
	[InlineData( 20, 4, 511 )]
	[InlineData( 20, 4, 513 )]
	[InlineData( 20, 4, 131072 )]
	[InlineData( 24, 1, 1 )]
	[InlineData( 25, 1, 9 )]
	[InlineData( 26, 1, 1 )]
	[InlineData( 32, 4, 2 )]
	[InlineData( 532, 2, 1 )]
	[InlineData( 532, 2, 244 )]
	[InlineData( 534, 2, 29 )]
	[InlineData( 534, 2, 513 )]
	[InlineData( 538, 2, 512 )]
	[InlineData( 540, 2, 512 )]
	public void TryReadValueRejectsMalformedGeometry(
		int offset,
		int width,
		uint invalidValue
	) {
		byte[] key = Encoding.UTF8.GetBytes( "hdb02-corruption" );
		byte[] database = CreateInlineHashV9Database( key, [ 0x42 ] );
		Span<byte> field = database.AsSpan( offset, width );
		if ( width == 4 ) {
			BinaryPrimitives.WriteUInt32LittleEndian( field, invalidValue );
		} else if ( width == 2 ) {
			BinaryPrimitives.WriteUInt16LittleEndian( field, (ushort)invalidValue );
		} else {
			field[0] = (byte)invalidValue;
		}

		AssertInvalidDatabase( database, key );
	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( 511 )]
	[InlineData( 1023 )]
	public void TryReadValueRejectsTruncatedDatabase( int length ) {
		byte[] key = Encoding.UTF8.GetBytes( "hdb02-truncated" );
		byte[] database = CreateInlineHashV9Database( key, [ 0x42 ] );
		Array.Resize( ref database, length );

		AssertInvalidDatabase( database, key );
	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( 1 )]
	[InlineData( 2 )]
	[InlineData( 3 )]
	[InlineData( 4 )]
	[InlineData( 5 )]
	public void TryReadValueRejectsMalformedOverflow( int corruption ) {
		byte[] key = Encoding.UTF8.GetBytes( "hdb02-overflow-corruption" );
		byte[] database = CreateOverflowHashV9Database( key, [ 0x11, 0x22 ] );
		int valueOffset = ( 2 * 512 ) - 1 - key.Length - 12;
		switch ( corruption ) {
			case 0:
				// A cycle must fail even after all declared bytes were copied.
				BinaryPrimitives.WriteUInt32LittleEndian(
					database.AsSpan( ( 2 * 512 ) + 16, 4 ),
					2
				);
				break;
			case 1:
				BinaryPrimitives.WriteUInt32LittleEndian(
					database.AsSpan( valueOffset + 4, 4 ),
					3
				);
				break;
			case 2:
				database[( 2 * 512 ) + 25] = 13;
				break;
			case 3:
				BinaryPrimitives.WriteUInt32LittleEndian(
					database.AsSpan( valueOffset + 8, 4 ),
					3
				);
				break;
			case 4:
				BinaryPrimitives.WriteUInt32LittleEndian(
					database.AsSpan( valueOffset + 8, 4 ),
					1
				);
				break;
			case 5:
				BinaryPrimitives.WriteUInt32LittleEndian(
					database.AsSpan( valueOffset + 8, 4 ),
					uint.MaxValue
				);
				break;
		}

		AssertInvalidDatabase( database, key );
	}

	[Theory]
	[InlineData( "HDB02-exact" )]
	[InlineData( "hdb02" )]
	[InlineData( "hdb02-exact-extra" )]
	public void TryReadValueReturnsCleanMissForDifferentKey( string requestedName ) {
		byte[] key = Encoding.UTF8.GetBytes( "hdb02-exact" );
		byte[] database = CreateInlineHashV9Database( key, [ 0x42 ] );
		byte[] requestedKey = Encoding.UTF8.GetBytes( requestedName );
		string path = Path.GetTempFileName();
		try {
			File.WriteAllBytes( path, database );
			bool found = BerkeleyDbHashReader.TryReadValue(
				path,
				requestedKey,
				out byte[] actual
			);

			Assert.False( found );
			Assert.Empty( actual );
		} finally {
			File.Delete( path );
		}
	}

	[Theory]
	[InlineData( 1023, false )]
	[InlineData( 1024, true )]
	[InlineData( 1025, true )]
	public void TryReadValueEnforcesInclusiveDatabaseLimit(
		int maximumDatabaseSize,
		bool shouldSucceed
	) {
		byte[] key = [ 0x6B ];
		byte[] expected = [ 0x42 ];
		byte[] database = CreateInlineHashV9Database( key, expected );
		string path = Path.GetTempFileName();
		try {
			File.WriteAllBytes( path, database );
			if ( shouldSucceed ) {
				Assert.True(
					BerkeleyDbHashReader.TryReadValue(
						path,
						key,
						out byte[] actual,
						maximumDatabaseSize: maximumDatabaseSize
					)
				);
				Assert.Equal( expected, actual );
			} else {
				Assert.Throws<InvalidDataException>(
					() => BerkeleyDbHashReader.TryReadValue(
						path,
						key,
						out _,
						maximumDatabaseSize: maximumDatabaseSize
					)
				);
			}

			AssertFileCanBeOpenedExclusively( path );
		} finally {
			File.Delete( path );
		}
	}

	[Theory]
	[InlineData( false, 1, false )]
	[InlineData( false, 2, true )]
	[InlineData( false, 3, true )]
	[InlineData( true, 95, false )]
	[InlineData( true, 96, true )]
	[InlineData( true, 97, true )]
	public void TryReadValueEnforcesInclusiveValueLimit(
		bool overflow,
		int maximumItemSize,
		bool shouldSucceed
	) {
		byte[] key = [ 0x6B ];
		byte[] expected = ( overflow )
			? Enumerable.Range( 0, 96 ).Select( index => (byte)index ).ToArray()
			: [ 0x11, 0x22 ]
		;
		byte[] database = ( overflow )
			? CreateOverflowHashV9Database( key, expected )
			: CreateInlineHashV9Database( key, expected )
		;
		string path = Path.GetTempFileName();
		try {
			File.WriteAllBytes( path, database );
			if ( shouldSucceed ) {
				Assert.True(
					BerkeleyDbHashReader.TryReadValue(
						path,
						key,
						out byte[] actual,
						maximumItemSize: maximumItemSize
					)
				);
				Assert.Equal( expected, actual );
			} else {
				Assert.Throws<InvalidDataException>(
					() => BerkeleyDbHashReader.TryReadValue(
						path,
						key,
						out _,
						maximumItemSize: maximumItemSize
					)
				);
			}

			AssertFileCanBeOpenedExclusively( path );
		} finally {
			File.Delete( path );
		}
	}

	[Theory]
	[InlineData( 1, false )]
	[InlineData( 2, true )]
	[InlineData( 3, true )]
	public void TryReadValueBoundsStoredKeys(
		int maximumItemSize,
		bool shouldSucceed
	) {
		byte[] key = [ 0x61, 0x62 ];
		byte[] database = CreateInlineHashV9Database( key, [ 0x42 ] );
		string path = Path.GetTempFileName();
		try {
			File.WriteAllBytes( path, database );
			if ( shouldSucceed ) {
				Assert.True(
					BerkeleyDbHashReader.TryReadValue(
						path,
						key,
						out byte[] actual,
						maximumItemSize: maximumItemSize
					)
				);
				Assert.Equal( new byte[] { 0x42 }, actual );
			} else {
				// Search for a shorter key so the stored-key allocation is tested.
				Assert.Throws<InvalidDataException>(
					() => BerkeleyDbHashReader.TryReadValue(
						path,
						new byte[] { 0x61 },
						out _,
						maximumItemSize: maximumItemSize
					)
				);
			}
		} finally {
			File.Delete( path );
		}
	}

	[Theory]
	[InlineData( 0, 1, "maximumDatabaseSize" )]
	[InlineData( -1, 1, "maximumDatabaseSize" )]
	[InlineData( 1024, 0, "maximumItemSize" )]
	[InlineData( 1024, -1, "maximumItemSize" )]
	public void TryReadValueRejectsInvalidLimitsBeforeOpeningFile(
		int maximumDatabaseSize,
		int maximumItemSize,
		string parameterName
	) {
		string path = Path.Combine(
			Path.GetTempPath(),
			Guid.NewGuid().ToString( "N" ) + ".db"
		);
		ArgumentOutOfRangeException error = Assert.Throws<ArgumentOutOfRangeException>(
			() => BerkeleyDbHashReader.TryReadValue(
				path,
				new byte[] { 0x6B },
				out _,
				maximumDatabaseSize: maximumDatabaseSize,
				maximumItemSize: maximumItemSize
			)
		);

		Assert.Equal( parameterName, error.ParamName );
	}

	[Fact]
	public void TryReadValueAllowsEmptyValueUnderPositiveLimit() {
		byte[] key = [ 0x6B ];
		byte[] database = CreateInlineHashV9Database( key, [] );
		string path = Path.GetTempFileName();
		try {
			File.WriteAllBytes( path, database );
			Assert.True(
				BerkeleyDbHashReader.TryReadValue(
					path,
					key,
					out byte[] actual,
					maximumItemSize: 1
				)
			);
			Assert.Empty( actual );
		} finally {
			File.Delete( path );
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void TryReadValueReleasesFileAfterMissOrMalformedData( bool malformed ) {
		byte[] key = [ 0x6B ];
		byte[] database = CreateInlineHashV9Database( key, [ 0x42 ] );
		if ( malformed ) {
			database[12] = 0;
		}

		string path = Path.GetTempFileName();
		try {
			File.WriteAllBytes( path, database );
			if ( malformed ) {
				Assert.Throws<InvalidDataException>(
					() => BerkeleyDbHashReader.TryReadValue( path, key, out _ )
				);
			} else {
				Assert.False(
					BerkeleyDbHashReader.TryReadValue(
						path,
						new byte[] { 0x78 },
						out byte[] actual
					)
				);
				Assert.Empty( actual );
			}

			AssertFileCanBeOpenedExclusively( path );
		} finally {
			File.Delete( path );
		}
	}

	[Fact]
	public void TryReadValueKeepsLimitsIndependentAcrossConcurrentReads() {
		byte[] key = [ 0x6B ];
		byte[] expected = [ 0x11, 0x22 ];
		byte[] database = CreateInlineHashV9Database( key, expected );
		string path = Path.GetTempFileName();
		try {
			File.WriteAllBytes( path, database );
			Parallel.For(
				0,
				16,
				index => {
					if ( ( index & 1 ) == 0 ) {
						Assert.Throws<InvalidDataException>(
							() => BerkeleyDbHashReader.TryReadValue(
								path,
								key,
								out _,
								maximumItemSize: 1
							)
						);
					} else {
						Assert.True(
							BerkeleyDbHashReader.TryReadValue(
								path,
								key,
								out byte[] actual,
								maximumItemSize: 2
							)
						);
						Assert.Equal( expected, actual );
					}
				}
			);

			AssertFileCanBeOpenedExclusively( path );
		} finally {
			File.Delete( path );
		}
	}

	private static void AssertFileCanBeOpenedExclusively( string path ) {
		using FileStream reopened = new FileStream(
			path,
			FileMode.Open,
			FileAccess.ReadWrite,
			FileShare.None
		);
		Assert.True( reopened.CanWrite );
	}

	private static void AssertInvalidDatabase(
		byte[] database,
		byte[] key
	) {
		string path = Path.GetTempFileName();
		try {
			File.WriteAllBytes( path, database );
			Assert.Throws<InvalidDataException>(
				() => BerkeleyDbHashReader.TryReadValue( path, key, out _ )
			);
		} finally {
			File.Delete( path );
		}
	}

	private static byte[] CreateInlineHashV9Database(
		byte[] key,
		byte[] value
	) {
		const int pageSize = 512;
		byte[] database = CreateDatabase( pageSize, lastPageNumber: 1 );
		Span<byte> page = database.AsSpan( pageSize, pageSize );
		WriteHashPageHeader( page );

		int keyOffset = pageSize - 1 - key.Length;
		int valueOffset = keyOffset - 1 - value.Length;
		WriteHashOffsets( page, keyOffset, valueOffset );

		page[keyOffset] = 1;
		key.CopyTo( page[( keyOffset + 1 )..] );
		page[valueOffset] = 1;
		value.CopyTo( page[( valueOffset + 1 )..keyOffset] );

		return database;
	}

	private static byte[] CreateBigEndianInlineHashV9Database(
		byte[] key,
		byte[] value
	) {
		const int pageSize = 512;
		const int pageHeaderSize = 26;
		const uint hashMagic = 0x00061561;
		const uint hashVersion = 9;

		byte[] database = new byte[pageSize * 2];
		Span<byte> metadata = database.AsSpan( 0, pageSize );
		BinaryPrimitives.WriteUInt32BigEndian( metadata[12..16], hashMagic );
		BinaryPrimitives.WriteUInt32BigEndian( metadata[16..20], hashVersion );
		BinaryPrimitives.WriteUInt32BigEndian( metadata[20..24], pageSize );
		metadata[24] = 0;
		metadata[25] = 8;
		metadata[26] = 0;
		BinaryPrimitives.WriteUInt32BigEndian( metadata[32..36], 1 );

		Span<byte> page = database.AsSpan( pageSize, pageSize );
		page[25] = 13;
		BinaryPrimitives.WriteUInt32BigEndian( page[8..12], 1 );
		BinaryPrimitives.WriteUInt16BigEndian( page[20..22], 2 );
		int keyOffset = pageSize - 1 - key.Length;
		int valueOffset = keyOffset - 1 - value.Length;
		BinaryPrimitives.WriteUInt16BigEndian(
			page[pageHeaderSize..( pageHeaderSize + 2 )],
			(ushort)keyOffset
		);
		BinaryPrimitives.WriteUInt16BigEndian(
			page[( pageHeaderSize + 2 )..( pageHeaderSize + 4 )],
			(ushort)valueOffset
		);
		BinaryPrimitives.WriteUInt16BigEndian(
			page[22..24],
			(ushort)valueOffset
		);
		page[keyOffset] = 1;
		key.CopyTo( page[( keyOffset + 1 )..] );
		page[valueOffset] = 1;
		value.CopyTo( page[( valueOffset + 1 )..keyOffset] );

		return database;
	}

	private static byte[] CreateOverflowHashV9Database(
		byte[] key,
		byte[] value
	) {
		const int pageSize = 512;
		const int offPageItemLength = 12;
		byte[] database = CreateDatabase( pageSize, lastPageNumber: 2 );

		Span<byte> hashPage = database.AsSpan( pageSize, pageSize );
		WriteHashPageHeader( hashPage );
		int keyOffset = pageSize - 1 - key.Length;
		int valueOffset = keyOffset - offPageItemLength;
		WriteHashOffsets( hashPage, keyOffset, valueOffset );

		hashPage[keyOffset] = 1;
		key.CopyTo( hashPage[( keyOffset + 1 )..] );
		hashPage[valueOffset] = 3;
		BinaryPrimitives.WriteUInt32LittleEndian(
			hashPage[( valueOffset + 4 )..( valueOffset + 8 )],
			2
		);
		BinaryPrimitives.WriteUInt32LittleEndian(
			hashPage[( valueOffset + 8 )..( valueOffset + 12 )],
			(uint)value.Length
		);

		Span<byte> overflowPage = database.AsSpan( pageSize * 2, pageSize );
		overflowPage[25] = 7;
		BinaryPrimitives.WriteUInt32LittleEndian( overflowPage[8..12], 2 );
		BinaryPrimitives.WriteUInt32LittleEndian( overflowPage[16..20], 0 );
		BinaryPrimitives.WriteUInt16LittleEndian(
			overflowPage[22..24],
			(ushort)value.Length
		);
		value.CopyTo( overflowPage[26..] );

		return database;
	}

	private static byte[] CreateDatabase(
		int pageSize,
		uint lastPageNumber
	) {
		const uint hashMagic = 0x00061561;
		const uint hashVersion = 9;
		const byte hashMetadataPage = 8;

		byte[] database = new byte[pageSize * ( lastPageNumber + 1 )];
		Span<byte> metadata = database.AsSpan( 0, pageSize );
		BinaryPrimitives.WriteUInt32LittleEndian( metadata[12..16], hashMagic );
		BinaryPrimitives.WriteUInt32LittleEndian( metadata[16..20], hashVersion );
		BinaryPrimitives.WriteUInt32LittleEndian( metadata[20..24], (uint)pageSize );
		metadata[24] = 0;
		metadata[25] = hashMetadataPage;
		metadata[26] = 0;
		BinaryPrimitives.WriteUInt32LittleEndian(
			metadata[32..36],
			lastPageNumber
		);
		return database;
	}

	private static void WriteHashPageHeader( Span<byte> page ) {
		page[25] = 13;
		BinaryPrimitives.WriteUInt32LittleEndian( page[8..12], 1 );
		BinaryPrimitives.WriteUInt16LittleEndian( page[20..22], 2 );
	}

	private static void WriteHashOffsets(
		Span<byte> page,
		int keyOffset,
		int valueOffset
	) {
		const int pageHeaderSize = 26;
		BinaryPrimitives.WriteUInt16LittleEndian(
			page[pageHeaderSize..( pageHeaderSize + 2 )],
			(ushort)keyOffset
		);
		BinaryPrimitives.WriteUInt16LittleEndian(
			page[( pageHeaderSize + 2 )..( pageHeaderSize + 4 )],
			(ushort)valueOffset
		);
		BinaryPrimitives.WriteUInt16LittleEndian(
			page[22..24],
			(ushort)valueOffset
		);
	}
}
