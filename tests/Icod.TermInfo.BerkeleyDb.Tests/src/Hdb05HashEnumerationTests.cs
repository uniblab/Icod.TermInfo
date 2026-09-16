/*
	Icod.TermInfo.BerkeleyDb.Tests
	Validates HDB05 complete Hash-v9 record enumeration.
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

public sealed class Hdb05HashEnumerationTests {
	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void ReadRecordsReturnsEveryPairFromOnePageInByteKeyOrder(
		bool isBigEndian
	) {
		byte[] database = CreateInlineDatabase(
			isBigEndian,
			separatePages: false,
			( new byte[] { 0x7A }, new byte[] { 0x02 } ),
			( new byte[] { 0x61 }, new byte[] { 0x01 } )
		);

		IReadOnlyList<BerkeleyDbHashRecord> records =
			BerkeleyDbHashReader.ReadRecords(
				database,
				maximumItemSize: 16,
				maximumRecordCount: 2,
				CancellationToken.None
			);

		Assert.Collection(
			records,
			record => AssertRecord( record, [ 0x61 ], [ 0x01 ] ),
			record => AssertRecord( record, [ 0x7A ], [ 0x02 ] )
		);
	}

	[Fact]
	public void ReadRecordsOrderingDoesNotDependOnHashPageOrder() {
		byte[] database = CreateInlineDatabase(
			isBigEndian: false,
			separatePages: true,
			( new byte[] { 0xFF }, new byte[] { 0x03 } ),
			( new byte[] { 0x10, 0x01 }, new byte[] { 0x02 } ),
			( new byte[] { 0x10 }, new byte[] { 0x01 } )
		);

		IReadOnlyList<BerkeleyDbHashRecord> records =
			ReadRecords( database, maximumRecordCount: 3 );

		Assert.Collection(
			records,
			record => AssertRecord( record, [ 0x10 ], [ 0x01 ] ),
			record => AssertRecord( record, [ 0x10, 0x01 ], [ 0x02 ] ),
			record => AssertRecord( record, [ 0xFF ], [ 0x03 ] )
		);
	}

	[Fact]
	public void ReadRecordsReconstructsOffPageKeyAndValue() {
		byte[] key = [ 0x61, 0x62, 0x63 ];
		byte[] value = [ 0x10, 0x20, 0x30, 0x40 ];
		byte[] database = CreateOffPageDatabase( key, value );

		IReadOnlyList<BerkeleyDbHashRecord> records =
			ReadRecords( database, maximumItemSize: 4 );

		BerkeleyDbHashRecord record = Assert.Single( records );
		AssertRecord( record, key, value );
	}

	[Fact]
	public void ReadRecordsAcceptsAnEmptyHashDatabase() {
		byte[] database = CreateInlineDatabase(
			isBigEndian: false,
			separatePages: false
		);

		Assert.Empty( ReadRecords( database ) );
	}

	[Fact]
	public void ReadRecordsEnforcesAnInclusiveRecordLimit() {
		byte[] database = CreateInlineDatabase(
			isBigEndian: false,
			separatePages: true,
			( new byte[] { 0x61 }, new byte[] { 0x01 } ),
			( new byte[] { 0x62 }, new byte[] { 0x02 } )
		);

		Assert.Equal(
			2,
			ReadRecords(
				database,
				maximumRecordCount: 2
			).Count
		);
		Assert.Throws<InvalidDataException>(
			() => ReadRecords(
				database,
				maximumRecordCount: 1
			)
		);
	}

	[Fact]
	public void ReadRecordsRejectsDuplicateExactByteKeys() {
		byte[] database = CreateInlineDatabase(
			isBigEndian: false,
			separatePages: true,
			( new byte[] { 0x61 }, new byte[] { 0x01 } ),
			( new byte[] { 0x61 }, new byte[] { 0x02 } )
		);

		Assert.Throws<InvalidDataException>(
			() => ReadRecords( database )
		);
	}

	[Fact]
	public void ReadRecordsHonorsPreCanceledToken() {
		using CancellationTokenSource cancellation = new();
		cancellation.Cancel();

		Assert.Throws<OperationCanceledException>(
			() => BerkeleyDbHashReader.ReadRecords(
				CreateInlineDatabase(
					isBigEndian: false,
					separatePages: true,
					( new byte[] { 0x61 }, new byte[] { 0x01 } )
				),
				maximumItemSize: 16,
				maximumRecordCount: 1,
				cancellation.Token
			)
		);
	}

	[Fact]
	public void CompleteEnumerationRejectsAReachedUnsupportedPage() {
		byte[] database = CreateInlineDatabase(
			isBigEndian: false,
			separatePages: true,
			( new byte[] { 0x61 }, new byte[] { 0x01 } ),
			( new byte[] { 0x62 }, new byte[] { 0x02 } )
		);
		database[( 2 * PageSize ) + 25] = 5;

		Assert.Throws<InvalidDataException>(
			() => ReadRecords( database )
		);
	}

	[Fact]
	public void ExistingExactLookupStillStopsBeforeAnUnrelatedMalformedPage() {
		byte[] database = CreateInlineDatabase(
			isBigEndian: false,
			separatePages: true,
			( new byte[] { 0x61 }, new byte[] { 0x01 } ),
			( new byte[] { 0x62 }, new byte[] { 0x02 } )
		);
		database[( 2 * PageSize ) + 25] = 5;

		Assert.True(
			BerkeleyDbHashReader.TryReadValue(
				database,
				new byte[] { 0x61 },
				out byte[] value,
				maximumItemSize: 16
			)
		);
		Assert.Equal( new byte[] { 0x01 }, value );
	}

	[Fact]
	public void ReturnedRecordsDoNotBorrowTheDatabaseImage() {
		byte[] database = CreateInlineDatabase(
			isBigEndian: false,
			separatePages: true,
			( new byte[] { 0x61 }, new byte[] { 0x01 } )
		);
		IReadOnlyList<BerkeleyDbHashRecord> records =
			ReadRecords( database );

		Array.Fill<byte>( database, 0 );

		AssertRecord(
			Assert.Single( records ),
			[ 0x61 ],
			[ 0x01 ]
		);
	}

	[Theory]
	[InlineData( 0, 1, "maximumItemSize" )]
	[InlineData( -1, 1, "maximumItemSize" )]
	[InlineData( 1, 0, "maximumRecordCount" )]
	[InlineData( 1, -1, "maximumRecordCount" )]
	public void InvalidLimitsFailBeforeReadingMetadata(
		int itemLimit,
		int recordLimit,
		string parameterName
	) {
		ArgumentOutOfRangeException error =
			Assert.Throws<ArgumentOutOfRangeException>(
				() => BerkeleyDbHashReader.ReadRecords(
					Array.Empty<byte>(),
					itemLimit,
					recordLimit,
					CancellationToken.None
				)
			);

		Assert.Equal( parameterName, error.ParamName );
	}

	[Fact]
	public void NullDatabaseFailsBeforeOtherWork() {
		Assert.Throws<ArgumentNullException>(
			() => BerkeleyDbHashReader.ReadRecords(
				null!,
				maximumItemSize: 1,
				maximumRecordCount: 1,
				CancellationToken.None
			)
		);
	}

	private const int PageSize = 512;
	private const int PageHeaderSize = 26;

	private static IReadOnlyList<BerkeleyDbHashRecord> ReadRecords(
		byte[] database,
		int maximumItemSize = 16,
		int maximumRecordCount = 16
	) {
		return BerkeleyDbHashReader.ReadRecords(
			database,
			maximumItemSize,
			maximumRecordCount,
			CancellationToken.None
		);
	}

	private static void AssertRecord(
		BerkeleyDbHashRecord record,
		byte[] expectedKey,
		byte[] expectedValue
	) {
		Assert.Equal( expectedKey, record.Key.ToArray() );
		Assert.Equal( expectedValue, record.Value.ToArray() );
	}

	private static byte[] CreateInlineDatabase(
		bool isBigEndian,
		bool separatePages,
		params ( byte[] Key, byte[] Value )[] records
	) {
		int hashPageCount = ( records.Length == 0 )
			? 0
			: ( separatePages )
				? records.Length
				: 1
		;
		byte[] database = CreateDatabase(
			hashPageCount,
			isBigEndian
		);

		if ( separatePages ) {
			for ( int index = 0; index < records.Length; index++ ) {
				WriteHashPage(
					database.AsSpan(
						( index + 1 ) * PageSize,
						PageSize
					),
					(uint)( index + 1 ),
					isBigEndian,
					records[index]
				);
			}
		} else if ( records.Length != 0 ) {
			WriteHashPage(
				database.AsSpan( PageSize, PageSize ),
				pageNumber: 1,
				isBigEndian,
				records
			);
		}

		return database;
	}

	private static byte[] CreateOffPageDatabase(
		byte[] key,
		byte[] value
	) {
		byte[] database = CreateDatabase(
			lastPageNumber: 3,
			isBigEndian: false
		);
		Span<byte> hashPage =
			database.AsSpan( PageSize, PageSize );
		WriteUInt32(
			hashPage.Slice( 8, 4 ),
			1,
			isBigEndian: false
		);
		hashPage[25] = 13;
		WriteUInt16(
			hashPage.Slice( 20, 2 ),
			2,
			isBigEndian: false
		);

		const int keyOffset = PageSize - 12;
		const int valueOffset = keyOffset - 12;
		WriteUInt16(
			hashPage.Slice( PageHeaderSize, 2 ),
			keyOffset,
			isBigEndian: false
		);
		WriteUInt16(
			hashPage.Slice( PageHeaderSize + 2, 2 ),
			valueOffset,
			isBigEndian: false
		);
		WriteUInt16(
			hashPage.Slice( 22, 2 ),
			valueOffset,
			isBigEndian: false
		);
		WriteOffPageItem(
			hashPage.Slice( keyOffset, 12 ),
			overflowPage: 2,
			key.Length
		);
		WriteOffPageItem(
			hashPage.Slice( valueOffset, 12 ),
			overflowPage: 3,
			value.Length
		);
		WriteOverflowPage(
			database.AsSpan( 2 * PageSize, PageSize ),
			pageNumber: 2,
			key
		);
		WriteOverflowPage(
			database.AsSpan( 3 * PageSize, PageSize ),
			pageNumber: 3,
			value
		);
		return database;
	}

	private static byte[] CreateDatabase(
		int lastPageNumber,
		bool isBigEndian
	) {
		byte[] database =
			new byte[PageSize * ( lastPageNumber + 1 )];
		Span<byte> metadata =
			database.AsSpan( 0, PageSize );
		WriteUInt32(
			metadata.Slice( 12, 4 ),
			0x00061561,
			isBigEndian
		);
		WriteUInt32(
			metadata.Slice( 16, 4 ),
			9,
			isBigEndian
		);
		WriteUInt32(
			metadata.Slice( 20, 4 ),
			PageSize,
			isBigEndian
		);
		metadata[25] = 8;
		WriteUInt32(
			metadata.Slice( 32, 4 ),
			lastPageNumber,
			isBigEndian
		);
		return database;
	}

	private static void WriteHashPage(
		Span<byte> page,
		uint pageNumber,
		bool isBigEndian,
		params ( byte[] Key, byte[] Value )[] records
	) {
		WriteUInt32(
			page.Slice( 8, 4 ),
			pageNumber,
			isBigEndian
		);
		page[25] = 13;
		WriteUInt16(
			page.Slice( 20, 2 ),
			records.Length * 2,
			isBigEndian
		);

		int offset = PageSize;
		int itemIndex = 0;
		foreach ( ( byte[] key, byte[] value ) in records ) {
			offset = WriteInlineItem(
				page,
				offset,
				itemIndex++,
				key,
				isBigEndian
			);
			offset = WriteInlineItem(
				page,
				offset,
				itemIndex++,
				value,
				isBigEndian
			);
		}

		WriteUInt16(
			page.Slice( 22, 2 ),
			offset,
			isBigEndian
		);
	}

	private static int WriteInlineItem(
		Span<byte> page,
		int upperOffset,
		int itemIndex,
		byte[] payload,
		bool isBigEndian
	) {
		int offset = checked(
			upperOffset - payload.Length - 1
		);
		WriteUInt16(
			page.Slice(
				PageHeaderSize + ( itemIndex * sizeof( ushort ) ),
				sizeof( ushort )
			),
			offset,
			isBigEndian
		);
		page[offset] = 1;
		payload.CopyTo( page[( offset + 1 )..upperOffset] );
		return offset;
	}

	private static void WriteOffPageItem(
		Span<byte> item,
		uint overflowPage,
		int length
	) {
		item[0] = 3;
		WriteUInt32(
			item.Slice( 4, 4 ),
			overflowPage,
			isBigEndian: false
		);
		WriteUInt32(
			item.Slice( 8, 4 ),
			length,
			isBigEndian: false
		);
	}

	private static void WriteOverflowPage(
		Span<byte> page,
		uint pageNumber,
		byte[] payload
	) {
		WriteUInt32(
			page.Slice( 8, 4 ),
			pageNumber,
			isBigEndian: false
		);
		WriteUInt32(
			page.Slice( 16, 4 ),
			0,
			isBigEndian: false
		);
		WriteUInt16(
			page.Slice( 22, 2 ),
			payload.Length,
			isBigEndian: false
		);
		page[25] = 7;
		payload.CopyTo( page[PageHeaderSize..] );
	}

	private static void WriteUInt16(
		Span<byte> bytes,
		int value,
		bool isBigEndian
	) {
		if ( isBigEndian ) {
			BinaryPrimitives.WriteUInt16BigEndian(
				bytes,
				checked( (ushort)value )
			);
		} else {
			BinaryPrimitives.WriteUInt16LittleEndian(
				bytes,
				checked( (ushort)value )
			);
		}
	}

	private static void WriteUInt32(
		Span<byte> bytes,
		uint value,
		bool isBigEndian
	) {
		if ( isBigEndian ) {
			BinaryPrimitives.WriteUInt32BigEndian(
				bytes,
				value
			);
		} else {
			BinaryPrimitives.WriteUInt32LittleEndian(
				bytes,
				value
			);
		}
	}
}
