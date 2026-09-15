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

public sealed class Hdb02HashLayoutTests {
	[Theory]
	[InlineData( 512, false )]
	[InlineData( 512, true )]
	[InlineData( 1024, false )]
	[InlineData( 1024, true )]
	[InlineData( 2048, false )]
	[InlineData( 2048, true )]
	[InlineData( 4096, false )]
	[InlineData( 4096, true )]
	[InlineData( 8192, false )]
	[InlineData( 8192, true )]
	[InlineData( 16384, false )]
	[InlineData( 16384, true )]
	[InlineData( 32768, false )]
	[InlineData( 32768, true )]
	[InlineData( 65536, false )]
	[InlineData( 65536, true )]
	public void TryReadValueSupportsPageSizes(
		int pageSize,
		bool isBigEndian
	) {
		byte[] key = [ 0x00, 0xFF, 0x6B ];
		byte[] expected = [ 0x00, 0x80, 0xFF, 0x42 ];
		byte[] database = CreateDatabase( pageSize, 1, isBigEndian );
		WriteHashPage(
			database.AsSpan( pageSize, pageSize ),
			1,
			isBigEndian,
			InlineItem( key ),
			InlineItem( expected )
		);

		WithDatabase(
			database,
			path => AssertLookup( path, key, expected )
		);
	}

	[Theory]
	[InlineData( 512, false, false )]
	[InlineData( 512, false, true )]
	[InlineData( 512, true, false )]
	[InlineData( 512, true, true )]
	[InlineData( 65536, false, false )]
	[InlineData( 65536, false, true )]
	[InlineData( 65536, true, false )]
	[InlineData( 65536, true, true )]
	public void TryReadValueHandlesEmptyHashPage(
		int pageSize,
		bool isBigEndian,
		bool hasLaterRecord
	) {
		byte[] key = [ 0x6B ];
		byte[] database = CreateDatabase( pageSize, 2, isBigEndian );
		WriteHashPage(
			database.AsSpan( pageSize, pageSize ),
			1,
			isBigEndian
		);
		Span<byte> laterPage = database.AsSpan( pageSize * 2, pageSize );
		if ( hasLaterRecord ) {
			WriteHashPage(
				laterPage,
				2,
				isBigEndian,
				InlineItem( key ),
				InlineItem( [ 0x42 ] )
			);
		} else {
			WriteHashPage( laterPage, 2, isBigEndian );
		}

		WithDatabase(
			database,
			path => {
				if ( hasLaterRecord ) {
					AssertLookup( path, key, [ 0x42 ] );
				} else {
					Assert.False(
						BerkeleyDbHashReader.TryReadValue( path, key, out byte[] actual )
					);
					Assert.Empty( actual );
				}
			}
		);
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void TryReadValueFindsRecordsAcrossHashAndSparsePages( bool isBigEndian ) {
		const int pageSize = 512;
		byte[] database = CreateDatabase( pageSize, 3, isBigEndian );
		WriteHashPage(
			database.AsSpan( pageSize, pageSize ),
			1,
			isBigEndian,
			InlineItem( [ 0x61 ] ),
			InlineItem( [ 0x11 ] ),
			InlineItem( [ 0x61, 0x62 ] ),
			InlineItem( [ 0x22, 0x23 ] )
		);
		// Page 2 is unused and deliberately has no initialized page identity.
		WriteHashPage(
			database.AsSpan( pageSize * 3, pageSize ),
			3,
			isBigEndian,
			InlineItem( [ 0x7A ] ),
			InlineItem( [ 0x33, 0x34, 0x35 ] )
		);

		WithDatabase(
			database,
			path => {
				AssertLookup( path, [ 0x61 ], [ 0x11 ] );
				AssertLookup( path, [ 0x61, 0x62 ], [ 0x22, 0x23 ] );
				AssertLookup( path, [ 0x7A ], [ 0x33, 0x34, 0x35 ] );
				Assert.False(
					BerkeleyDbHashReader.TryReadValue(
						path,
						new byte[] { 0x61, 0x62, 0x63 },
						out byte[] actual
					)
				);
				Assert.Empty( actual );
			}
		);
	}

	[Theory]
	[InlineData( false, true, false )]
	[InlineData( false, false, true )]
	[InlineData( false, true, true )]
	[InlineData( true, true, false )]
	[InlineData( true, false, true )]
	[InlineData( true, true, true )]
	public void TryReadValueReconstructsNoncontiguousOverflowChains(
		bool isBigEndian,
		bool offPageKey,
		bool offPageValue
	) {
		byte[] key = ( offPageKey )
			? Enumerable.Range( 0, 700 ).Select( index => (byte)( index % 251 ) ).ToArray()
			: [ 0x6B ]
		;
		byte[] expected = ( offPageValue )
			? Enumerable.Range( 0, 1000 ).Select( index => (byte)( 255 - ( index % 251 ) ) ).ToArray()
			: [ 0xA1, 0xB2 ]
		;
		byte[] database = CreateOverflowDatabase(
			key,
			expected,
			isBigEndian,
			offPageKey,
			offPageValue
		);

		WithDatabase(
			database,
			path => {
				AssertLookup( path, key, expected );
				byte[] differentKey = (byte[])key.Clone();
				differentKey[^1] ^= 0x80;
				Assert.False(
					BerkeleyDbHashReader.TryReadValue( path, differentKey, out byte[] actual )
				);
				Assert.Empty( actual );
			}
		);
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void TryReadValueBoundsOffPageKeys( bool isBigEndian ) {
		byte[] key = Enumerable.Range( 0, 700 )
			.Select( index => (byte)( index % 251 ) )
			.ToArray();
		byte[] database = CreateOverflowDatabase(
			key,
			[ 0x42 ],
			isBigEndian,
			offPageKey: true,
			offPageValue: false
		);

		WithDatabase(
			database,
			path => Assert.Throws<InvalidDataException>(
				() => BerkeleyDbHashReader.TryReadValue(
					path,
					new byte[] { 0x00 },
					out _,
					maximumItemSize: 699
				)
			)
		);
	}

	[Theory]
	[InlineData( 512, false )]
	[InlineData( 512, true )]
	[InlineData( 65536, false )]
	[InlineData( 65536, true )]
	public void TryReadValueRejectsZeroOffsetOutsideEmpty64KiBPage(
		int pageSize,
		bool isBigEndian
	) {
		byte[] database = CreateDatabase( pageSize, 1, isBigEndian );
		Span<byte> page = database.AsSpan( pageSize, pageSize );
		if ( pageSize == 65536 ) {
			WriteHashPage(
				page,
				1,
				isBigEndian,
				InlineItem( [ 0x6B ] ),
				InlineItem( [ 0x42 ] )
			);
		} else {
			WriteHashPage( page, 1, isBigEndian );
		}
		WriteUInt16( page, 22, 0, isBigEndian );

		WithDatabase(
			database,
			path => Assert.Throws<InvalidDataException>(
				() => BerkeleyDbHashReader.TryReadValue(
					path,
					new byte[] { 0x6B },
					out _
				)
			)
		);
	}

	private static byte[] CreateOverflowDatabase(
		byte[] key,
		byte[] value,
		bool isBigEndian,
		bool offPageKey,
		bool offPageValue
	) {
		const int pageSize = 512;
		byte[] database = CreateDatabase( pageSize, 6, isBigEndian );
		byte[] keyItem = ( offPageKey )
			? OffPageItem( 4, key.Length, isBigEndian )
			: InlineItem( key )
		;
		byte[] valueItem = ( offPageValue )
			? OffPageItem( 6, value.Length, isBigEndian )
			: InlineItem( value )
		;
		WriteHashPage(
			database.AsSpan( pageSize, pageSize ),
			1,
			isBigEndian,
			keyItem,
			valueItem
		);
		if ( offPageKey ) {
			WriteOverflowChain( database, key, [ 4, 2 ], isBigEndian );
		}
		if ( offPageValue ) {
			WriteOverflowChain( database, value, [ 6, 3, 5 ], isBigEndian );
		}
		return database;
	}

	private static void WriteOverflowChain(
		byte[] database,
		byte[] payload,
		int[] pageNumbers,
		bool isBigEndian
	) {
		const int pageSize = 512;
		const int payloadCapacity = pageSize - 26;
		int copied = 0;
		for ( int index = 0; index < pageNumbers.Length; index++ ) {
			Span<byte> page = database.AsSpan( pageNumbers[index] * pageSize, pageSize );
			page[25] = 7;
			WriteUInt32( page, 8, (uint)pageNumbers[index], isBigEndian );
			uint previous = ( index == 0 )
				? 0
				: (uint)pageNumbers[index - 1]
			;
			uint next = ( index + 1 == pageNumbers.Length )
				? 0
				: (uint)pageNumbers[index + 1]
			;
			WriteUInt32( page, 12, previous, isBigEndian );
			WriteUInt32( page, 16, next, isBigEndian );
			WriteUInt16( page, 20, 1, isBigEndian );
			int chunkLength = Math.Min( payloadCapacity, payload.Length - copied );
			WriteUInt16( page, 22, checked( (ushort)chunkLength ), isBigEndian );
			payload.AsSpan( copied, chunkLength ).CopyTo( page[26..] );
			copied += chunkLength;
		}
		Assert.Equal( payload.Length, copied );
	}

	private static byte[] CreateDatabase(
		int pageSize,
		int lastPageNumber,
		bool isBigEndian
	) {
		byte[] database = new byte[pageSize * ( lastPageNumber + 1 )];
		WriteUInt32( database, 12, 0x00061561, isBigEndian );
		WriteUInt32( database, 16, 9, isBigEndian );
		WriteUInt32( database, 20, (uint)pageSize, isBigEndian );
		database[25] = 8;
		WriteUInt32( database, 32, (uint)lastPageNumber, isBigEndian );
		return database;
	}

	private static void WriteHashPage(
		Span<byte> page,
		uint pageNumber,
		bool isBigEndian,
		params byte[][] items
	) {
		page[25] = 13;
		WriteUInt32( page, 8, pageNumber, isBigEndian );
		WriteUInt16( page, 20, checked( (ushort)items.Length ), isBigEndian );
		int offset = page.Length;
		for ( int index = 0; index < items.Length; index++ ) {
			offset -= items[index].Length;
			items[index].CopyTo( page[offset..] );
			WriteUInt16( page, 26 + ( index * 2 ), checked( (ushort)offset ), isBigEndian );
		}
		// Berkeley DB P_INIT casts the empty page's size to db_indx_t.
		// An empty 65536-byte page therefore stores hf_offset as zero.
		WriteUInt16( page, 22, unchecked( (ushort)offset ), isBigEndian );
	}

	private static byte[] InlineItem( byte[] payload ) {
		byte[] item = new byte[payload.Length + 1];
		item[0] = 1;
		payload.CopyTo( item, 1 );
		return item;
	}

	private static byte[] OffPageItem(
		uint firstPage,
		int length,
		bool isBigEndian
	) {
		byte[] item = new byte[12];
		item[0] = 3;
		WriteUInt32( item, 4, firstPage, isBigEndian );
		WriteUInt32( item, 8, (uint)length, isBigEndian );
		return item;
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

	private static void AssertLookup(
		string path,
		byte[] key,
		byte[] expected
	) {
		Assert.True(
			BerkeleyDbHashReader.TryReadValue( path, key, out byte[] actual )
		);
		Assert.Equal( expected, actual );
	}

	private static void WithDatabase(
		byte[] database,
		Action<string> assertion
	) {
		string path = Path.GetTempFileName();
		try {
			File.WriteAllBytes( path, database );
			assertion( path );
		} finally {
			File.Delete( path );
		}
	}
}
