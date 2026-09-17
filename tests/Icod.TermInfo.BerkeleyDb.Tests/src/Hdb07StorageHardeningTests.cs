/*
	Icod.TermInfo.BerkeleyDb.Tests
	Characterizes HDB07 storage geometry, limits, ownership, and lookup scope.
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
using Icod.TermInfo.Tests.Shared;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hdb07StorageHardeningTests {
	private const int PageSize = 512;
	private const int PageHeaderSize = 26;

	public static TheoryData<bool, int> ValidPageSizeCases => new() {
		{ false, 512 },
		{ true, 512 },
		{ false, 64 * 1024 },
		{ true, 64 * 1024 },
	};

	public static TheoryData<string, string> InvalidGeometryCases => new() {
		{
			"duplicate-offset",
			"Invalid Berkeley DB hash item offset 510 at index 1."
		},
		{
			"ascending-offsets",
			"Invalid Berkeley DB hash item offset 510 at index 1."
		},
		{
			"offset-in-index-table",
			"Invalid Berkeley DB hash item offset 29 at index 0."
		},
		{
			"odd-item-count",
			"Hash page 1 has an odd item count 3."
		},
	};

	public static TheoryData<string, string> InvalidOverflowCases => new() {
		{
			"missing-page",
			"Berkeley DB page 4 is outside the database."
		},
		{
			"wrong-page-type",
			"Expected overflow page 2, found page type 13."
		},
		{
			"wrong-page-identity",
			"Berkeley DB page 2 identifies itself as page 99."
		},
		{
			"cycle",
			"The Berkeley DB overflow chain contains a cycle."
		},
		{
			"premature-end",
			"The Berkeley DB overflow chain supplied 1 bytes but declared 2."
		},
		{
			"overlong-chunk",
			"The Berkeley DB overflow chain exceeds its declared length."
		},
	};

	public static TheoryData<string> FileReleaseCases => new() {
		"success",
		"miss",
		"corruption",
		"cancellation",
	};

	[Theory]
	[MemberData( nameof( ValidPageSizeCases ) )]
	public void ReadRecordsAcceptsBoundaryPageSizes(
		bool isBigEndian,
		int pageSize
	) {
		byte[] database = Hdb07HashV9FixtureBuilder.CreateDatabase(
			GetByteOrder( isBigEndian ),
			pageSize,
			CreateInlineRecord( [ 0x61 ], [ 0x42 ] )
		);

		BerkeleyDbHashRecord record = Assert.Single(
			ReadRecords( database )
		);
		AssertRecord( record, [ 0x61 ], [ 0x42 ] );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void ReadRecordsAcceptsLastPageAtExactFileBoundary(
		bool isBigEndian
	) {
		byte[] database = Hdb07HashV9FixtureBuilder.CreateDatabase(
			GetByteOrder( isBigEndian ),
			PageSize,
			CreateInlineRecord( [ 0x61 ], [ 0x42 ] )
		);

		Assert.Single( ReadRecords( database ) );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void ReadRecordsRejectsLastPageBeyondFile(
		bool isBigEndian
	) {
		byte[] database = Hdb07HashV9FixtureBuilder.CreateDatabase(
			GetByteOrder( isBigEndian ),
			PageSize,
			CreateInlineRecord( [ 0x61 ], [ 0x42 ] )
		);
		WriteUInt32( database, 32, 2, isBigEndian );

		InvalidDataException error = Assert.Throws<InvalidDataException>(
			() => ReadRecords( database )
		);
		Assert.Equal(
			"The Berkeley DB metadata references a page beyond the file.",
			error.Message
		);
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void ReadRecordsAcceptsIndexTableEndAsFreeOffset(
		bool isBigEndian
	) {
		byte[] database = Hdb07HashV9FixtureBuilder.CreateDatabase(
			GetByteOrder( isBigEndian ),
			PageSize,
			CreateInlineRecord( [ 0x61 ], [ 0x42 ] )
		);
		WriteUInt16(
			database,
			PageSize + 22,
			PageHeaderSize + ( 2 * sizeof( ushort ) ),
			isBigEndian
		);

		BerkeleyDbHashRecord record = Assert.Single(
			ReadRecords( database )
		);
		AssertRecord( record, [ 0x61 ], [ 0x42 ] );
	}

	[Theory]
	[MemberData( nameof( InvalidGeometryCases ) )]
	public void ReadRecordsRejectsInvalidGeometryInTraversalOrder(
		string caseName,
		string expectedMessage
	) {
		byte[] database = Hdb07HashV9FixtureBuilder.CreateDatabase(
			Hdb07ByteOrder.LittleEndian,
			PageSize,
			CreateInlineRecord( [ 0x61 ], [ 0x42 ] )
		);
		MutateGeometry( database, caseName );

		InvalidDataException error = Assert.Throws<InvalidDataException>(
			() => ReadRecords( database )
		);
		Assert.Equal( expectedMessage, error.Message );
	}

	[Theory]
	[MemberData( nameof( InvalidOverflowCases ) )]
	public void ReadRecordsRejectsMalformedOverflowInTraversalOrder(
		string caseName,
		string expectedMessage
	) {
		byte[] database = CreateMalformedOverflowFixture( caseName );

		InvalidDataException error = Assert.Throws<InvalidDataException>(
			() => ReadRecords( database )
		);
		Assert.Equal( expectedMessage, error.Message );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void ReadRecordsAcceptsValidSharedOverflowTail(
		bool isBigEndian
	) {
		byte[] database = Hdb07HashV9FixtureBuilder.CreateDatabase(
			GetByteOrder( isBigEndian ),
			PageSize,
			CreateOffPageValueRecord( [ 0x61 ], [ 0x42 ], [ 1 ] ),
			CreateOffPageValueRecord( [ 0x62 ], [ 0x42 ], [ 1 ] )
		);
		int secondValueOffset = GetItemOffset(
			database,
			pageNumber: 2,
			itemIndex: 1,
			isBigEndian
		);
		WriteUInt32(
			database,
			( 2 * PageSize ) + secondValueOffset + 4,
			3,
			isBigEndian
		);

		IReadOnlyList<BerkeleyDbHashRecord> records = ReadRecords(
			database,
			maximumRecordCount: 2
		);
		Assert.Collection(
			records,
			record => AssertRecord( record, [ 0x61 ], [ 0x42 ] ),
			record => AssertRecord( record, [ 0x62 ], [ 0x42 ] )
		);
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void ReadRecordsRejectsDuplicateExactBinaryKeysOnSeparatePages(
		bool isBigEndian
	) {
		byte[] database = Hdb07HashV9FixtureBuilder.CreateDatabase(
			GetByteOrder( isBigEndian ),
			PageSize,
			CreateInlineRecord( [ 0x00, 0xFF ], [ 0x01 ] ),
			CreateInlineRecord( [ 0x00, 0xFF ], [ 0x02 ] )
		);

		InvalidDataException error = Assert.Throws<InvalidDataException>(
			() => ReadRecords( database, maximumRecordCount: 2 )
		);
		Assert.Equal(
			"The Berkeley DB contains a duplicate exact byte key.",
			error.Message
		);
	}

	[Fact]
	public void DatabaseSizeLimitIsInclusiveAtExactBoundary() {
		byte[] key = [ 0x61 ];
		byte[] database = Hdb07HashV9FixtureBuilder.CreateDatabase(
			Hdb07ByteOrder.LittleEndian,
			PageSize,
			CreateInlineRecord( key, [ 0x42 ] )
		);
		WithTemporaryDatabase(
			database,
			path => {
				Assert.True(
					BerkeleyDbHashReader.TryReadValue(
						path,
						key,
						out byte[] value,
						maximumDatabaseSize: database.Length
					)
				);
				Assert.Equal( new byte[] { 0x42 }, value );

				InvalidDataException error =
					Assert.Throws<InvalidDataException>(
						() => BerkeleyDbHashReader.TryReadValue(
							path,
							key,
							out _,
							maximumDatabaseSize: database.Length - 1
						)
					);
				Assert.Equal(
					$"Berkeley DB file length {database.Length} exceeds the supported database size.",
					error.Message
				);
			}
		);
	}

	[Fact]
	public void ItemSizeLimitIsInclusiveAtExactBoundary() {
		byte[] database = Hdb07HashV9FixtureBuilder.CreateDatabase(
			Hdb07ByteOrder.LittleEndian,
			PageSize,
			CreateInlineRecord( [ 0x61, 0x62 ], [ 0x01, 0x02, 0x03 ] )
		);

		BerkeleyDbHashRecord record = Assert.Single(
			ReadRecords( database, maximumItemSize: 3 )
		);
		AssertRecord(
			record,
			[ 0x61, 0x62 ],
			[ 0x01, 0x02, 0x03 ]
		);

		InvalidDataException error = Assert.Throws<InvalidDataException>(
			() => ReadRecords( database, maximumItemSize: 2 )
		);
		Assert.Equal(
			"The Berkeley DB inline item exceeds the configured item size.",
			error.Message
		);
	}

	[Fact]
	public void RecordCountLimitIsInclusiveAtExactBoundary() {
		byte[] database = Hdb07HashV9FixtureBuilder.CreateDatabase(
			Hdb07ByteOrder.LittleEndian,
			PageSize,
			CreateInlineRecord( [ 0x61 ], [ 0x01 ] ),
			CreateInlineRecord( [ 0x62 ], [ 0x02 ] )
		);

		Assert.Equal(
			2,
			ReadRecords( database, maximumRecordCount: 2 ).Count
		);
		InvalidDataException error = Assert.Throws<InvalidDataException>(
			() => ReadRecords( database, maximumRecordCount: 1 )
		);
		Assert.Equal(
			"The Berkeley DB contains more than 1 records.",
			error.Message
		);
	}

	[Fact]
	public void PreCancellationWinsBeforeMetadataValidation() {
		using CancellationTokenSource cancellation = new();
		cancellation.Cancel();

		Assert.Throws<OperationCanceledException>(
			() => BerkeleyDbHashReader.ReadRecords(
				[],
				maximumItemSize: 1,
				maximumRecordCount: 1,
				cancellation.Token
			)
		);
	}

	[Theory]
	[MemberData( nameof( FileReleaseCases ) )]
	public void FileIsReleasedAfterEveryStorageOutcome(
		string outcome
	) {
		byte[] key = [ 0x61 ];
		byte[] database = Hdb07HashV9FixtureBuilder.CreateDatabase(
			Hdb07ByteOrder.LittleEndian,
			PageSize,
			CreateInlineRecord( key, [ 0x42 ] )
		);
		if ( outcome == "corruption" ) {
			database[12] = 0;
		}

		WithTemporaryDatabase(
			database,
			path => {
				switch ( outcome ) {
					case "success":
						Assert.True(
							BerkeleyDbHashReader.TryReadValue(
								path,
								key,
								out _
							)
						);
						break;
					case "miss":
						Assert.False(
							BerkeleyDbHashReader.TryReadValue(
								path,
								[ 0x62 ],
								out _
							)
						);
						break;
					case "corruption":
						Assert.Throws<InvalidDataException>(
							() => BerkeleyDbHashReader.TryReadValue(
								path,
								key,
								out _
							)
						);
						break;
					case "cancellation":
						byte[] acquired = BerkeleyDbHashReader.ReadDatabase(
							path,
							database.Length
						);
						using ( CancellationTokenSource cancellation = new() ) {
							cancellation.Cancel();
							Assert.Throws<OperationCanceledException>(
								() => BerkeleyDbHashReader.ReadRecords(
									acquired,
									maximumItemSize: 8,
									maximumRecordCount: 1,
									cancellation.Token
								)
							);
						}
						break;
					default:
						throw new InvalidOperationException(
							$"Unknown file-release case '{outcome}'."
						);
				}

				AssertFileCanBeOpenedExclusively( path );
			}
		);
	}

	[Fact]
	public void ConcurrentReadsKeepDifferentLimitsIndependent() {
		byte[] key = [ 0x61 ];
		byte[] value = [ 0x01, 0x02 ];
		byte[] database = Hdb07HashV9FixtureBuilder.CreateDatabase(
			Hdb07ByteOrder.LittleEndian,
			PageSize,
			CreateInlineRecord( key, value )
		);
		WithTemporaryDatabase(
			database,
			path => Parallel.For(
				0,
				16,
				index => {
					if ( ( index & 1 ) == 0 ) {
						InvalidDataException error =
							Assert.Throws<InvalidDataException>(
								() => BerkeleyDbHashReader.TryReadValue(
									path,
									key,
									out _,
									maximumItemSize: 1
								)
							);
						Assert.Equal(
							"The Berkeley DB inline item exceeds the configured item size.",
							error.Message
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
						Assert.Equal( value, actual );
					}
				}
			)
		);
	}

	[Fact]
	public void ExactLookupStopsBeforeAnUnrelatedMalformedValue() {
		byte[] requestedKey = [ 0x61 ];
		byte[] database = Hdb07HashV9FixtureBuilder.CreateDatabase(
			Hdb07ByteOrder.LittleEndian,
			PageSize,
			CreateInlineRecord( requestedKey, [ 0x01 ] ),
			CreateInlineRecord( [ 0x62 ], [ 0x02 ] )
		);
		int malformedValueOffset = GetItemOffset(
			database,
			pageNumber: 2,
			itemIndex: 1,
			isBigEndian: false
		);
		database[( 2 * PageSize ) + malformedValueOffset] = 99;

		Assert.True(
			BerkeleyDbHashReader.TryReadValue(
				database,
				requestedKey,
				out byte[] value,
				maximumItemSize: 8
			)
		);
		Assert.Equal( new byte[] { 0x01 }, value );
	}

	[Fact]
	public void ExactLookupRejectsAMalformedKeyBeforeTheMatch() {
		byte[] requestedKey = [ 0x62 ];
		byte[] database = Hdb07HashV9FixtureBuilder.CreateDatabase(
			Hdb07ByteOrder.LittleEndian,
			PageSize,
			CreateInlineRecord( [ 0x61 ], [ 0x01 ] ),
			CreateInlineRecord( requestedKey, [ 0x02 ] )
		);
		int malformedKeyOffset = GetItemOffset(
			database,
			pageNumber: 1,
			itemIndex: 0,
			isBigEndian: false
		);
		database[PageSize + malformedKeyOffset] = 99;

		InvalidDataException error = Assert.Throws<InvalidDataException>(
			() => BerkeleyDbHashReader.TryReadValue(
				database,
				requestedKey,
				out _,
				maximumItemSize: 8
			)
		);
		Assert.Equal(
			"Berkeley DB hash item type 99 is not supported yet.",
			error.Message
		);
	}

	private static IReadOnlyList<BerkeleyDbHashRecord> ReadRecords(
		byte[] database,
		int maximumItemSize = 4096,
		int maximumRecordCount = 8
	) {
		return BerkeleyDbHashReader.ReadRecords(
			database,
			maximumItemSize,
			maximumRecordCount,
			CancellationToken.None
		);
	}

	private static Hdb07RecordSpec CreateInlineRecord(
		byte[] key,
		byte[] value
	) {
		return new Hdb07RecordSpec(
			Hdb07ItemSpec.Inline( key ),
			Hdb07ItemSpec.Inline( value )
		);
	}

	private static Hdb07RecordSpec CreateOffPageValueRecord(
		byte[] key,
		byte[] value,
		int[] chunkLengths
	) {
		return new Hdb07RecordSpec(
			Hdb07ItemSpec.Inline( key ),
			Hdb07ItemSpec.OffPage( value, chunkLengths )
		);
	}

	private static byte[] CreateMalformedOverflowFixture(
		string caseName
	) {
		byte[] payload;
		int[] chunkLengths;
		if ( caseName == "cycle" ) {
			payload = [ 0x01, 0x02, 0x03 ];
			chunkLengths = [ 1, 1, 1 ];
		} else if ( caseName == "overlong-chunk" ) {
			payload = [ 0x01 ];
			chunkLengths = [ 1 ];
		} else {
			payload = [ 0x01, 0x02 ];
			chunkLengths = [ 1, 1 ];
		}

		byte[] database = Hdb07HashV9FixtureBuilder.CreateDatabase(
			Hdb07ByteOrder.LittleEndian,
			PageSize,
			CreateOffPageValueRecord(
				[ 0x61 ],
				payload,
				chunkLengths
			)
		);
		int valueOffset = GetItemOffset(
			database,
			pageNumber: 1,
			itemIndex: 1,
			isBigEndian: false
		);

		switch ( caseName ) {
			case "missing-page":
				WriteUInt32(
					database,
					PageSize + valueOffset + 4,
					4,
					isBigEndian: false
				);
				break;
			case "wrong-page-type":
				database[( 2 * PageSize ) + 25] = 13;
				break;
			case "wrong-page-identity":
				WriteUInt32(
					database,
					( 2 * PageSize ) + 8,
					99,
					isBigEndian: false
				);
				break;
			case "cycle":
				WriteUInt32(
					database,
					( 3 * PageSize ) + 16,
					2,
					isBigEndian: false
				);
				break;
			case "premature-end":
				WriteUInt32(
					database,
					( 2 * PageSize ) + 16,
					0,
					isBigEndian: false
				);
				break;
			case "overlong-chunk":
				WriteUInt16(
					database,
					( 2 * PageSize ) + 22,
					2,
					isBigEndian: false
				);
				break;
			default:
				throw new InvalidOperationException(
					$"Unknown overflow case '{caseName}'."
				);
		}

		return database;
	}

	private static void MutateGeometry(
		byte[] database,
		string caseName
	) {
		switch ( caseName ) {
			case "duplicate-offset":
				WriteUInt16(
					database,
					PageSize + PageHeaderSize + sizeof( ushort ),
					510,
					isBigEndian: false
				);
				break;
			case "ascending-offsets":
				WriteUInt16(
					database,
					PageSize + PageHeaderSize,
					509,
					isBigEndian: false
				);
				WriteUInt16(
					database,
					PageSize + PageHeaderSize + sizeof( ushort ),
					510,
					isBigEndian: false
				);
				break;
			case "offset-in-index-table":
				WriteUInt16(
					database,
					PageSize + 22,
					30,
					isBigEndian: false
				);
				WriteUInt16(
					database,
					PageSize + PageHeaderSize,
					29,
					isBigEndian: false
				);
				break;
			case "odd-item-count":
				WriteUInt16(
					database,
					PageSize + 20,
					3,
					isBigEndian: false
				);
				break;
			default:
				throw new InvalidOperationException(
					$"Unknown geometry case '{caseName}'."
				);
		}
	}

	private static int GetItemOffset(
		byte[] database,
		int pageNumber,
		int itemIndex,
		bool isBigEndian
	) {
		int offset = ( pageNumber * PageSize )
			+ PageHeaderSize
			+ ( itemIndex * sizeof( ushort ) )
		;
		ReadOnlySpan<byte> bytes = database.AsSpan(
			offset,
			sizeof( ushort )
		);
		return ( isBigEndian )
			? BinaryPrimitives.ReadUInt16BigEndian( bytes )
			: BinaryPrimitives.ReadUInt16LittleEndian( bytes )
		;
	}

	private static void AssertRecord(
		BerkeleyDbHashRecord record,
		byte[] expectedKey,
		byte[] expectedValue
	) {
		Assert.Equal( expectedKey, record.Key.ToArray() );
		Assert.Equal( expectedValue, record.Value.ToArray() );
	}

	private static void AssertFileCanBeOpenedExclusively(
		string path
	) {
		using FileStream stream = new FileStream(
			path,
			FileMode.Open,
			FileAccess.ReadWrite,
			FileShare.None
		);
		Assert.True( stream.CanRead );
		Assert.True( stream.CanWrite );
	}

	private static void WithTemporaryDatabase(
		byte[] database,
		Action<string> action
	) {
		string path = Path.GetTempFileName();
		try {
			File.WriteAllBytes( path, database );
			action( path );
		} finally {
			File.Delete( path );
		}
	}

	private static Hdb07ByteOrder GetByteOrder( bool isBigEndian ) =>
		( isBigEndian )
			? Hdb07ByteOrder.BigEndian
			: Hdb07ByteOrder.LittleEndian
	;

	private static void WriteUInt16(
		Span<byte> bytes,
		int offset,
		int value,
		bool isBigEndian
	) {
		if ( isBigEndian ) {
			BinaryPrimitives.WriteUInt16BigEndian(
				bytes.Slice( offset, sizeof( ushort ) ),
				checked( (ushort)value )
			);
		} else {
			BinaryPrimitives.WriteUInt16LittleEndian(
				bytes.Slice( offset, sizeof( ushort ) ),
				checked( (ushort)value )
			);
		}
	}

	private static void WriteUInt32(
		Span<byte> bytes,
		int offset,
		uint value,
		bool isBigEndian
	) {
		if ( isBigEndian ) {
			BinaryPrimitives.WriteUInt32BigEndian(
				bytes.Slice( offset, sizeof( uint ) ),
				value
			);
		} else {
			BinaryPrimitives.WriteUInt32LittleEndian(
				bytes.Slice( offset, sizeof( uint ) ),
				value
			);
		}
	}
}
