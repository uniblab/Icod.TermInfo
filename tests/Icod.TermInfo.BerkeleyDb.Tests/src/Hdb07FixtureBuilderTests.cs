/*
	Icod.TermInfo.BerkeleyDb.Tests
	Validates the independent deterministic HDB07 Hash-v9 fixture builder.
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
using Icod.TermInfo.Tests.Shared;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hdb07FixtureBuilderTests {
	[Theory]
	[InlineData( Hdb07ByteOrder.LittleEndian )]
	[InlineData( Hdb07ByteOrder.BigEndian )]
	public void CreateDatabaseWritesLiteralMetadataAndInlineItems(
		Hdb07ByteOrder byteOrder
	) {
		byte[] database = Hdb07HashV9FixtureBuilder.CreateDatabase(
			byteOrder,
			512,
			new Hdb07RecordSpec(
				Hdb07ItemSpec.Inline( [ 0x6B, 0x31 ] ),
				Hdb07ItemSpec.Inline( [ 0x41, 0x42, 0x43 ] )
			)
		);

		Assert.Equal( 1024, database.Length );
		Assert.Equal(
			(byteOrder == Hdb07ByteOrder.LittleEndian)
				? new byte[] { 0x61, 0x15, 0x06, 0x00 }
				: new byte[] { 0x00, 0x06, 0x15, 0x61 },
			database[12..16]
		);
		Assert.Equal(
			(byteOrder == Hdb07ByteOrder.LittleEndian)
				? new byte[] { 0x09, 0x00, 0x00, 0x00 }
				: new byte[] { 0x00, 0x00, 0x00, 0x09 },
			database[16..20]
		);
		Assert.Equal(
			(byteOrder == Hdb07ByteOrder.LittleEndian)
				? new byte[] { 0x00, 0x02, 0x00, 0x00 }
				: new byte[] { 0x00, 0x00, 0x02, 0x00 },
			database[20..24]
		);
		Assert.Equal( (byte)8, database[25] );
		Assert.Equal( 0U, ReadUInt32( database, 8, byteOrder ) );
		Assert.Equal( 1U, ReadUInt32( database, 32, byteOrder ) );

		ReadOnlySpan<byte> page = database.AsSpan( 512, 512 );
		Assert.Equal( 1U, ReadUInt32( page, 8, byteOrder ) );
		Assert.Equal( (byte)13, page[25] );
		Assert.Equal( (ushort)2, ReadUInt16( page, 20, byteOrder ) );
		Assert.Equal( (ushort)509, ReadUInt16( page, 26, byteOrder ) );
		Assert.Equal( (ushort)505, ReadUInt16( page, 28, byteOrder ) );
		Assert.Equal( new byte[] { 1, 0x6B, 0x31 }, page[509..].ToArray() );
		Assert.Equal( new byte[] { 1, 0x41, 0x42, 0x43 }, page[505..509].ToArray() );
	}

	[Theory]
	[InlineData( Hdb07ByteOrder.LittleEndian )]
	[InlineData( Hdb07ByteOrder.BigEndian )]
	public void CreateDatabaseWritesRequestedOffPageChain(
		Hdb07ByteOrder byteOrder
	) {
		byte[] database = Hdb07HashV9FixtureBuilder.CreateDatabase(
			byteOrder,
			512,
			new Hdb07RecordSpec(
				Hdb07ItemSpec.OffPage(
					[ 0x10, 0x20, 0x30, 0x40, 0x50 ],
					[ 2, 3 ]
				),
				Hdb07ItemSpec.Inline( [ 0x44 ] )
			)
		);

		Assert.Equal( 2048, database.Length );
		Assert.Equal( 3U, ReadUInt32( database, 32, byteOrder ) );
		for ( uint pageNumber = 0; pageNumber <= 3; pageNumber++ ) {
			Assert.Equal(
				pageNumber,
				ReadUInt32( database, checked( (int)( pageNumber * 512 ) ) + 8, byteOrder )
			);
		}

		ReadOnlySpan<byte> hashPage = database.AsSpan( 512, 512 );
		int keyOffset = ReadUInt16( hashPage, 26, byteOrder );
		Assert.Equal( 500, keyOffset );
		Assert.Equal( (byte)3, hashPage[keyOffset] );
		Assert.Equal( 2U, ReadUInt32( hashPage, keyOffset + 4, byteOrder ) );
		Assert.Equal( 5U, ReadUInt32( hashPage, keyOffset + 8, byteOrder ) );

		AssertOverflowPage(
			database.AsSpan( 1024, 512 ),
			byteOrder,
			pageNumber: 2,
			previousPage: 0,
			nextPage: 3,
			[ 0x10, 0x20 ]
		);
		AssertOverflowPage(
			database.AsSpan( 1536, 512 ),
			byteOrder,
			pageNumber: 3,
			previousPage: 2,
			nextPage: 0,
			[ 0x30, 0x40, 0x50 ]
		);
	}

	[Fact]
	public void CompiledAndNcursesHelpersWriteExactMinimalEnvelopes() {
		byte[] compiled = Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
			"alpha",
			"samplex",
			"a"
		);
		byte[] names = Encoding.Latin1.GetBytes( "alpha|a|samplex\0" );

		Assert.Equal( 12 + names.Length, compiled.Length );
		Assert.Equal( new byte[] { 0x1A, 0x01 }, compiled[0..2] );
		Assert.Equal(
			checked( (ushort)names.Length ),
			BinaryPrimitives.ReadUInt16LittleEndian( compiled.AsSpan( 2, 2 ) )
		);
		Assert.Equal( new byte[8], compiled[4..12] );
		Assert.Equal( names, compiled[12..] );
		Assert.Equal( new byte[] { 0, 0x1A, 0x01 }, Hdb07HashV9FixtureBuilder.NcursesData( compiled )[0..3] );
		Assert.Equal(
			new byte[] { 2, 0x6B, 0x65, 0x79 },
			Hdb07HashV9FixtureBuilder.NcursesIndex( Encoding.UTF8.GetBytes( "key" ) )
		);
	}

	private static void AssertOverflowPage(
		ReadOnlySpan<byte> page,
		Hdb07ByteOrder byteOrder,
		uint pageNumber,
		uint previousPage,
		uint nextPage,
		byte[] expectedPayload
	) {
		Assert.Equal( pageNumber, ReadUInt32( page, 8, byteOrder ) );
		Assert.Equal( previousPage, ReadUInt32( page, 12, byteOrder ) );
		Assert.Equal( nextPage, ReadUInt32( page, 16, byteOrder ) );
		Assert.Equal( (ushort)1, ReadUInt16( page, 20, byteOrder ) );
		Assert.Equal(
			checked( (ushort)expectedPayload.Length ),
			ReadUInt16( page, 22, byteOrder )
		);
		Assert.Equal( (byte)7, page[25] );
		Assert.Equal( expectedPayload, page[26..( 26 + expectedPayload.Length )].ToArray() );
	}

	private static ushort ReadUInt16(
		ReadOnlySpan<byte> bytes,
		int offset,
		Hdb07ByteOrder byteOrder
	) =>
		(byteOrder == Hdb07ByteOrder.BigEndian)
			? BinaryPrimitives.ReadUInt16BigEndian( bytes.Slice( offset, 2 ) )
			: BinaryPrimitives.ReadUInt16LittleEndian( bytes.Slice( offset, 2 ) )
	;

	private static uint ReadUInt32(
		ReadOnlySpan<byte> bytes,
		int offset,
		Hdb07ByteOrder byteOrder
	) =>
		(byteOrder == Hdb07ByteOrder.BigEndian)
			? BinaryPrimitives.ReadUInt32BigEndian( bytes.Slice( offset, 4 ) )
			: BinaryPrimitives.ReadUInt32LittleEndian( bytes.Slice( offset, 4 ) )
	;
}
