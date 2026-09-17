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

public sealed class Hdb03RecordReaderTests {
	[Fact]
	public void DirectDataPreservesOpaqueBytesWithNoIndexHops() {
		WithDatabase(
			CreateDatabase( ( new byte[] { 0x6B }, new byte[] { 0, 0x1A, 1, 0, 0xFF } ) ),
			path => {
				Assert.True( NcursesRecordReader.TryReadCompiledEntry(
					path, new byte[] { 0x6B }, out byte[] actual, maximumIndexHops: 0
				)
				);
				Assert.Equal( new byte[] { 0x1A, 1, 0, 0xFF }, actual );
			}
		);
	}

	[Fact]
	public void EmptyCompiledPayloadIsLeftForTheCompiledParser() {
		WithDatabase(
			CreateDatabase( ( new byte[] { 0x6B }, new byte[] { 0 } ) ),
			path => {
				Assert.True( NcursesRecordReader.TryReadCompiledEntry(
					path, new byte[] { 0x6B }, out byte[] actual
				)
				);
				Assert.Empty( actual );
			}
		);
	}

	[Fact]
	public void IndexTargetPreservesBinaryBytesAndEmbeddedNul() {
		WithDatabase(
			CreateDatabase(
				( new byte[] { 0x6B }, new byte[] { 2, 0, 0xFF, 0x7C } ),
				( new byte[] { 0xFF, 0x7C }, new byte[] { 0, 0x99 } ),
				( new byte[] { 0, 0xFF, 0x7C }, new byte[] { 0, 0x42 } )
			),
			path => {
				Assert.True( NcursesRecordReader.TryReadCompiledEntry(
					path, new byte[] { 0x6B }, out byte[] actual, maximumIndexHops: 1
				)
				);
				Assert.Equal( new byte[] { 0x42 }, actual );
			}
		);
	}

	[Fact]
	public void TwoIndexLinksResolveAtInclusiveHopLimit() {
		WithDatabase(
			CreateChain(),
			path => {
				Assert.True( NcursesRecordReader.TryReadCompiledEntry(
					path, new byte[] { 0x6B }, out byte[] actual, maximumIndexHops: 2
				)
				);
				Assert.Equal( new byte[] { 0x42 }, actual );
			}
		);
	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( 1 )]
	public void IndexLinksBeyondHopLimitAreMalformed( int limit ) {
		WithDatabase(
			CreateChain(),
			path => Assert.Throws<InvalidDataException>(
				() => NcursesRecordReader.TryReadCompiledEntry(
					path, new byte[] { 0x6B }, out _, maximumIndexHops: limit
				)
			)
		);
	}

	[Theory]
	[InlineData( 0x4B )]
	[InlineData( 0x78 )]
	public void AbsentInitialKeyIsACleanMiss( byte requestedKey ) {
		WithDatabase(
			CreateDatabase( ( new byte[] { 0x6B }, new byte[] { 0, 0x42 } ) ),
			path => {
				Assert.False( NcursesRecordReader.TryReadCompiledEntry(
					path, new byte[] { requestedKey }, out byte[] actual
				)
				);
				Assert.Empty( actual );
			}
		);
	}

	[Theory]
	[InlineData( -1 )]
	[InlineData( 1 )]
	[InlineData( 2 )]
	[InlineData( 3 )]
	[InlineData( 255 )]
	public void EmptyOrUnsupportedEnvelopeIsMalformed( int marker ) {
		byte[] record = ( marker < 0 )
			? []
			: [ (byte)marker ]
		;
		AssertMalformed( CreateDatabase( ( new byte[] { 0x6B }, record ) ) );
	}

	[Fact]
	public void MissingIndexTargetIsMalformedRatherThanACleanMiss() {
		AssertMalformed( CreateDatabase(
			( new byte[] { 0x6B }, new byte[] { 2, 0x61 } )
		)
		);
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void RepeatedExactKeyIsACycle( bool twoKeys ) {
		byte[] database = ( twoKeys )
			? CreateDatabase(
				( new byte[] { 0x6B }, new byte[] { 2, 0x61 } ),
				( new byte[] { 0x61 }, new byte[] { 2, 0x6B } )
			)
			: CreateDatabase( ( new byte[] { 0x6B }, new byte[] { 2, 0x6B } ) )
		;
		AssertMalformed( database );
	}

	[Fact]
	public void DatabaseAndRecordLimitsAreInclusive() {
		WithDatabase(
			CreateDatabase( ( new byte[] { 0x6B }, new byte[] { 0, 1, 2, 3 } ) ),
			path => {
				Assert.True( NcursesRecordReader.TryReadCompiledEntry(
					path, new byte[] { 0x6B }, out byte[] actual,
					maximumDatabaseSize: 1024, maximumItemSize: 4
				)
				);
				Assert.Equal( new byte[] { 1, 2, 3 }, actual );
			}
		);
	}

	[Theory]
	[InlineData( 1023, 4 )]
	[InlineData( 1024, 3 )]
	public void DatabaseAndRecordLimitsAreEnforced( int databaseLimit, int itemLimit ) {
		WithDatabase(
			CreateDatabase( ( new byte[] { 0x6B }, new byte[] { 0, 1, 2, 3 } ) ),
			path => Assert.Throws<InvalidDataException>(
				() => NcursesRecordReader.TryReadCompiledEntry(
					path, new byte[] { 0x6B }, out _,
					maximumDatabaseSize: databaseLimit, maximumItemSize: itemLimit
				)
			)
		);
	}

	[Theory]
	[InlineData( 0, 1, 1 )]
	[InlineData( -1, 1, 1 )]
	[InlineData( 1, 0, 1 )]
	[InlineData( 1, -1, 1 )]
	[InlineData( 1, 1, -1 )]
	public void InvalidLimitsFailBeforeOpening( int databaseLimit, int itemLimit, int hopLimit ) {
		string missing = Path.Combine( Path.GetTempPath(), Guid.NewGuid().ToString( "N" ) + ".db" );
		Assert.Throws<ArgumentOutOfRangeException>(
			() => NcursesRecordReader.TryReadCompiledEntry(
				missing, new byte[] { 0x6B }, out _,
				databaseLimit, itemLimit, hopLimit
			)
		);
	}

	[Theory]
	[InlineData( null )]
	[InlineData( "" )]
	[InlineData( " " )]
	public void InvalidDatabasePathIsRejected( string? path ) {
		Assert.ThrowsAny<ArgumentException>(
			() => NcursesRecordReader.TryReadCompiledEntry( path!, new byte[] { 0x6B }, out _ )
		);
	}

	[Fact]
	public void ContainerCorruptionPropagates() {
		byte[] database = CreateDatabase( ( new byte[] { 0x6B }, new byte[] { 0, 0x42 } ) );
		database[12] = 0;
		AssertMalformed( database );
	}

	[Fact]
	public void MissingFilePropagatesAndTheNextCallCanRetry() {
		string path = Path.Combine( Path.GetTempPath(), Guid.NewGuid().ToString( "N" ) + ".db" );
		try {
			Assert.Throws<FileNotFoundException>(
				() => NcursesRecordReader.TryReadCompiledEntry( path, new byte[] { 0x6B }, out _ )
			);
			File.WriteAllBytes( path, CreateDatabase( ( new byte[] { 0x6B }, new byte[] { 0, 0x42 } ) ) );
			Assert.True( NcursesRecordReader.TryReadCompiledEntry(
				path, new byte[] { 0x6B }, out byte[] actual
			)
			);
			Assert.Equal( new byte[] { 0x42 }, actual );
		} finally {
			File.Delete( path );
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void NoFileHandleSurvivesSuccessOrEnvelopeFailure( bool malformed ) {
		byte[] record = ( malformed )
			? [ 2 ]
			: [ 0, 0x42 ]
		;
		WithDatabase(
			CreateDatabase( ( new byte[] { 0x6B }, record ) ),
			path => {
				if ( malformed ) {
					Assert.Throws<InvalidDataException>(
						() => NcursesRecordReader.TryReadCompiledEntry( path, new byte[] { 0x6B }, out _ )
					);
				} else {
					Assert.True( NcursesRecordReader.TryReadCompiledEntry(
						path, new byte[] { 0x6B }, out _
					)
					);
				}
				using FileStream exclusive = new FileStream(
					path, FileMode.Open, FileAccess.ReadWrite, FileShare.None
				);
				Assert.Equal( 1024, exclusive.Length );
			}
		);
	}

	private static byte[] CreateChain() {
		return CreateDatabase(
			( new byte[] { 0x6B }, new byte[] { 2, 0x61 } ),
			( new byte[] { 0x62 }, new byte[] { 0, 0x42 } ),
			( new byte[] { 0x61 }, new byte[] { 2, 0x62 } )
		);
	}

	private static byte[] CreateDatabase( params ( byte[] Key, byte[] Value )[] records ) {
		byte[] database = new byte[512 * ( records.Length + 1 )];
		BinaryPrimitives.WriteUInt32LittleEndian( database.AsSpan( 12, 4 ), 0x00061561 );
		BinaryPrimitives.WriteUInt32LittleEndian( database.AsSpan( 16, 4 ), 9 );
		BinaryPrimitives.WriteUInt32LittleEndian( database.AsSpan( 20, 4 ), 512 );
		database[25] = 8;
		BinaryPrimitives.WriteUInt32LittleEndian( database.AsSpan( 32, 4 ), (uint)records.Length );

		for ( int index = 0; index < records.Length; index++ ) {
			( byte[] key, byte[] value ) = records[index];
			Span<byte> page = database.AsSpan( 512 * ( index + 1 ), 512 );
			BinaryPrimitives.WriteUInt32LittleEndian( page.Slice( 8, 4 ), (uint)( index + 1 ) );
			page[25] = 13;
			ushort keyOffset = checked( (ushort)( 511 - key.Length ) );
			ushort valueOffset = checked( (ushort)( keyOffset - value.Length - 1 ) );
			BinaryPrimitives.WriteUInt16LittleEndian( page.Slice( 20, 2 ), 2 );
			BinaryPrimitives.WriteUInt16LittleEndian( page.Slice( 22, 2 ), valueOffset );
			BinaryPrimitives.WriteUInt16LittleEndian( page.Slice( 26, 2 ), keyOffset );
			BinaryPrimitives.WriteUInt16LittleEndian( page.Slice( 28, 2 ), valueOffset );
			page[keyOffset] = 1;
			key.CopyTo( page[( keyOffset + 1 )..] );
			page[valueOffset] = 1;
			value.CopyTo( page[( valueOffset + 1 )..] );
		}
		return database;
	}

	private static void AssertMalformed( byte[] database ) {
		WithDatabase(
			database,
			path => Assert.Throws<InvalidDataException>(
				() => NcursesRecordReader.TryReadCompiledEntry( path, new byte[] { 0x6B }, out _ )
			)
		);
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
