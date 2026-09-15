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
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hdb02UnsupportedLayoutTests {
	[Theory]
	[InlineData( 26, 1, 2, false )]
	[InlineData( 26, 1, 2, true )]
	[InlineData( 26, 1, 4, false )]
	[InlineData( 26, 1, 4, true )]
	[InlineData( 26, 1, 128, false )]
	[InlineData( 26, 1, 128, true )]
	[InlineData( 36, 4, 1, false )]
	[InlineData( 36, 4, 1, true )]
	[InlineData( 36, 4, 2, false )]
	[InlineData( 36, 4, 2, true )]
	[InlineData( 48, 4, 1, false )]
	[InlineData( 48, 4, 1, true )]
	[InlineData( 48, 4, 2, false )]
	[InlineData( 48, 4, 2, true )]
	[InlineData( 48, 4, 4, false )]
	[InlineData( 48, 4, 4, true )]
	[InlineData( 48, 4, 0x80000000, false )]
	[InlineData( 48, 4, 0x80000000, true )]
	public void TryReadValueRejectsUnsupportedMetadataFeatures(
		int offset,
		int width,
		uint flags,
		bool isBigEndian
	) {
		byte[] database = CreateDatabase( [ 1, 0x6B ], [ 1, 0x42 ], isBigEndian );
		if ( width == 1 ) {
			database[offset] = (byte)flags;
		} else {
			WriteUInt32( database, offset, flags, isBigEndian );
		}

		AssertInvalidDatabase( database );
	}

	[Theory]
	[InlineData( 2, false )]
	[InlineData( 2, true )]
	[InlineData( 5, false )]
	[InlineData( 5, true )]
	[InlineData( 8, false )]
	[InlineData( 8, true )]
	[InlineData( 12, false )]
	[InlineData( 12, true )]
	[InlineData( 255, false )]
	[InlineData( 255, true )]
	public void TryReadValueRejectsUnsupportedScannedPageTypes(
		byte pageType,
		bool isBigEndian
	) {
		byte[] database = CreateDatabase( [ 1, 0x6B ], [ 1, 0x42 ], isBigEndian );
		database[512 + 25] = pageType;

		AssertInvalidDatabase( database );
	}

	[Theory]
	[InlineData( 0, false )]
	[InlineData( 0, true )]
	[InlineData( 7, false )]
	[InlineData( 7, true )]
	public void TryReadValueSkipsNonrecordPagesDuringScan(
		byte pageType,
		bool isBigEndian
	) {
		byte[] database = CreateDatabase( [ 1, 0x6B ], [ 1, 0x42 ], isBigEndian );
		Array.Resize( ref database, 1536 );
		database.AsSpan( 512, 512 ).CopyTo( database.AsSpan( 1024, 512 ) );
		database.AsSpan( 512, 512 ).Clear();
		database[512 + 25] = pageType;
		if ( pageType == 7 ) {
			WriteUInt32( database, 512 + 8, 1, isBigEndian );
		}
		WriteUInt32( database, 1024 + 8, 2, isBigEndian );
		WriteUInt32( database, 32, 2, isBigEndian );

		WithDatabase( database, path => AssertLookup( path, 0x42 ) );
	}

	[Theory]
	[InlineData( 0, false )]
	[InlineData( 0, true )]
	[InlineData( 2, false )]
	[InlineData( 2, true )]
	[InlineData( 4, false )]
	[InlineData( 4, true )]
	[InlineData( 255, false )]
	[InlineData( 255, true )]
	public void TryReadValueRejectsUnsupportedItemTypes(
		byte itemType,
		bool keyItem
	) {
		byte[] key = [ 1, 0x6B ];
		byte[] value = [ 1, 0x42 ];
		if ( keyItem ) {
			key[0] = itemType;
		} else {
			value[0] = itemType;
		}

		AssertInvalidDatabase( CreateDatabase( key, value, false ) );
	}

	[Theory]
	[InlineData( 1, false )]
	[InlineData( 1, true )]
	[InlineData( 11, false )]
	[InlineData( 11, true )]
	public void TryReadValueRejectsTruncatedOffPageHeaders(
		int headerLength,
		bool keyItem
	) {
		byte[] truncated = new byte[headerLength];
		truncated[0] = 3;
		byte[] key = ( keyItem )
			? truncated
			: [ 1, 0x6B ]
		;
		byte[] value = ( keyItem )
			? [ 1, 0x42 ]
			: truncated
		;

		AssertInvalidDatabase( CreateDatabase( key, value, false ) );
	}

	[Fact]
	public void TryReadValuePropagatesMissingFileAndAllowsRetry() {
		string path = Path.Combine( Path.GetTempPath(), Guid.NewGuid().ToString( "N" ) + ".db" );
		try {
			Assert.Throws<FileNotFoundException>(
				() => BerkeleyDbHashReader.TryReadValue( path, new byte[] { 0x6B }, out _ )
			);
			File.WriteAllBytes( path, CreateDatabase( [ 1, 0x6B ], [ 1, 0x42 ], false ) );
			AssertLookup( path, 0x42 );
		} finally {
			File.Delete( path );
		}
	}

	[Fact]
	public void TryReadValuePropagatesSharingFailureAndAllowsRetry() {
		WithDatabase(
			CreateDatabase( [ 1, 0x6B ], [ 1, 0x42 ], false ),
			path => {
				using ( FileStream exclusive = new FileStream(
					path,
					FileMode.Open,
					FileAccess.ReadWrite,
					FileShare.None
				) ) {
					Assert.ThrowsAny<IOException>(
						() => BerkeleyDbHashReader.TryReadValue( path, new byte[] { 0x6B }, out _ )
					);
				}
				AssertLookup( path, 0x42 );
			}
		);
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void TryReadValueObservesReplacementAfterSuccessOrCorruption( bool malformed ) {
		byte[] database = CreateDatabase( [ 1, 0x6B ], [ 1, 0x42 ], false );
		if ( malformed ) {
			database[12] = 0;
		}

		WithDatabase(
			database,
			path => {
				if ( malformed ) {
					Assert.Throws<InvalidDataException>(
						() => BerkeleyDbHashReader.TryReadValue( path, new byte[] { 0x6B }, out _ )
					);
				} else {
					AssertLookup( path, 0x42 );
				}

				string replacement = Path.GetTempFileName();
				try {
					File.WriteAllBytes(
						replacement,
						CreateDatabase( [ 1, 0x6B ], [ 1, 0x43 ], false )
					);
					File.Move( replacement, path, overwrite: true );
					AssertLookup( path, 0x43 );
				} finally {
					File.Delete( replacement );
				}
			}
		);
	}

	[Fact]
	public void TryReadValuePropagatesDirectoryOpenFailure() {
		string path = Path.Combine( Path.GetTempPath(), Guid.NewGuid().ToString( "N" ) );
		Directory.CreateDirectory( path );
		try {
			Exception? error = Record.Exception(
				() => BerkeleyDbHashReader.TryReadValue( path, new byte[] { 0x6B }, out _ )
			);
			Assert.True( error is IOException or UnauthorizedAccessException );
		} finally {
			Directory.Delete( path );
		}
	}

	private static byte[] CreateDatabase(
		byte[] keyItem,
		byte[] valueItem,
		bool isBigEndian
	) {
		byte[] database = new byte[1024];
		WriteUInt32( database, 12, 0x00061561, isBigEndian );
		WriteUInt32( database, 16, 9, isBigEndian );
		WriteUInt32( database, 20, 512, isBigEndian );
		database[25] = 8;
		WriteUInt32( database, 32, 1, isBigEndian );
		Span<byte> page = database.AsSpan( 512, 512 );
		WriteUInt32( page, 8, 1, isBigEndian );
		page[25] = 13;
		int keyOffset = 512 - keyItem.Length;
		int valueOffset = keyOffset - valueItem.Length;
		WriteUInt16( page, 20, 2, isBigEndian );
		WriteUInt16( page, 22, (ushort)valueOffset, isBigEndian );
		WriteUInt16( page, 26, (ushort)keyOffset, isBigEndian );
		WriteUInt16( page, 28, (ushort)valueOffset, isBigEndian );
		keyItem.CopyTo( page[keyOffset..] );
		valueItem.CopyTo( page[valueOffset..] );
		return database;
	}

	private static void WriteUInt16(
		Span<byte> bytes,
		int offset,
		ushort value,
		bool isBigEndian
	) {
		if ( isBigEndian ) {
			BinaryPrimitives.WriteUInt16BigEndian( bytes.Slice( offset, 2 ), value );
		} else {
			BinaryPrimitives.WriteUInt16LittleEndian( bytes.Slice( offset, 2 ), value );
		}
	}

	private static void WriteUInt32(
		Span<byte> bytes,
		int offset,
		uint value,
		bool isBigEndian
	) {
		if ( isBigEndian ) {
			BinaryPrimitives.WriteUInt32BigEndian( bytes.Slice( offset, 4 ), value );
		} else {
			BinaryPrimitives.WriteUInt32LittleEndian( bytes.Slice( offset, 4 ), value );
		}
	}

	private static void AssertInvalidDatabase( byte[] database ) {
		WithDatabase(
			database,
			path => Assert.Throws<InvalidDataException>(
				() => BerkeleyDbHashReader.TryReadValue( path, new byte[] { 0x6B }, out _ )
			)
		);
	}

	private static void AssertLookup( string path, byte expected ) {
		Assert.True(
			BerkeleyDbHashReader.TryReadValue( path, new byte[] { 0x6B }, out byte[] actual )
		);
		Assert.Equal( new byte[] { expected }, actual );
	}

	private static void WithDatabase( byte[] database, Action<string> assertion ) {
		string path = Path.GetTempFileName();
		try {
			File.WriteAllBytes( path, database );
			assertion( path );
		} finally {
			File.Delete( path );
		}
	}
}
