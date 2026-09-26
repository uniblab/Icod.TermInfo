/*
	Icod.TermInfo.BerkeleyDb.Tests
	Defines the HW03 Hash-v9 off-page and overflow image contract.
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
using System.Security.Cryptography;
using Xunit;
using static Icod.TermInfo.BerkeleyDb.Tests.Hw03WriterTestSupport;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hw03WriterOverflowTests {
	[Theory]
	[InlineData( 1024, 1, 0 )]
	[InlineData( 1025, 3, 1 )]
	[InlineData( 4070, 3, 1 )]
	[InlineData( 4071, 3, 2 )]
	[InlineData( 8140, 3, 2 )]
	public void SelectsCanonicalItemShape(
		int payloadLength,
		byte expectedItemType,
		int expectedOverflowPages
	) {
		byte[] image = BuildSingleRecord( payloadLength );
		ItemLocation value = LocateOnlyValue( image );
		Assert.Equal(
			expectedItemType,
			Page( image, value.PageNumber )[value.Offset]
		);
		Assert.Equal(
			expectedOverflowPages,
			checked( image.Length / PageSize ) - 3
		);
	}

	[Fact]
	public void ExactOverflowMultipleDoesNotAllocateAnEmptyTailPage() {
		byte[] image = BuildSingleRecord( 8140 );
		Assert.Equal( 5 * PageSize, image.Length );
		AssertOverflowPage( image, 3, previous: 0, next: 4, chunkLength: 4070 );
		AssertOverflowPage( image, 4, previous: 3, next: 0, chunkLength: 4070 );
	}

	[Fact]
	public void WritesExactTwelveByteOffPageDescriptor() {
		byte[] image = BuildSingleRecord( 1025 );
		ItemLocation value = LocateOnlyValue( image );
		ReadOnlySpan<byte> page = Page( image, value.PageNumber );
		ushort precedingOffset = BinaryPrimitives.ReadUInt16LittleEndian(
			page[26..28]
		);
		byte[] expected = new byte[12];
		expected[0] = 3;
		BinaryPrimitives.WriteUInt32LittleEndian( expected.AsSpan( 4, 4 ), 3 );
		BinaryPrimitives.WriteUInt32LittleEndian( expected.AsSpan( 8, 4 ), 1025 );

		Assert.Equal( expected.Length, precedingOffset - value.Offset );
		Assert.Equal(
			expected,
			page.Slice( value.Offset, expected.Length ).ToArray()
		);
	}

	[Fact]
	public void WritesLargeKeyThenLargeValueAsIndependentChains() {
		byte[] key = Enumerable.Repeat( (byte)0x4B, 1025 ).ToArray();
		byte[] value = Enumerable.Repeat( (byte)0x56, 4071 ).ToArray();
		byte[] image = Build( [ new BerkeleyDbHashRecord( key, value ) ] );
		int bucketPage = checked(
			(int)( BerkeleyDbHashV9ImageBuilder.Hash( key ) & 1 ) + 1
		);
		ReadOnlySpan<byte> page = Page( image, bucketPage );
		ushort keyOffset = BinaryPrimitives.ReadUInt16LittleEndian( page[26..28] );
		ushort valueOffset = BinaryPrimitives.ReadUInt16LittleEndian( page[28..30] );

		Assert.Equal( (byte)3, page[keyOffset] );
		Assert.Equal(
			3U,
			BinaryPrimitives.ReadUInt32LittleEndian(
				page.Slice( keyOffset + 4, 4 )
			)
		);
		Assert.Equal(
			1025U,
			BinaryPrimitives.ReadUInt32LittleEndian(
				page.Slice( keyOffset + 8, 4 )
			)
		);
		Assert.Equal( (byte)3, page[valueOffset] );
		Assert.Equal(
			4U,
			BinaryPrimitives.ReadUInt32LittleEndian(
				page.Slice( valueOffset + 4, 4 )
			)
		);
		Assert.Equal(
			4071U,
			BinaryPrimitives.ReadUInt32LittleEndian(
				page.Slice( valueOffset + 8, 4 )
			)
		);

		AssertOverflowPage( image, 3, previous: 0, next: 0, chunkLength: 1025 );
		AssertOverflowPage( image, 4, previous: 0, next: 5, chunkLength: 4070 );
		AssertOverflowPage( image, 5, previous: 4, next: 0, chunkLength: 1 );
		Assert.True(
			Page( image, 3 )
				[HeaderSize..( HeaderSize + key.Length )]
				.SequenceEqual( key )
		);
		Assert.True(
			Page( image, 4 )[HeaderSize..]
				.SequenceEqual( value.AsSpan( 0, 4070 ) )
		);
		Assert.Equal( value[^1], Page( image, 5 )[HeaderSize] );
	}

	[Fact]
	public void ManagedReaderReconstructsMultiPagePayloadExactly() {
		byte[] key = Enumerable.Range( 0, 4071 )
			.Select( static value => checked( (byte)( value % 251 ) ) )
			.ToArray()
		;
		byte[] value = Enumerable.Range( 0, 8141 )
			.Select( static value => checked( (byte)( 250 - ( value % 251 ) ) ) )
			.ToArray()
		;
		byte[] image = Build( [ new BerkeleyDbHashRecord( key, value ) ] );

		BerkeleyDbHashRecord actual = Assert.Single(
			BerkeleyDbHashReader.ReadRecords(
				image,
				maximumItemSize: value.Length,
				maximumRecordCount: 1,
				CancellationToken.None
			)
		);
		Assert.True( actual.Key.Span.SequenceEqual( key ) );
		Assert.True( actual.Value.Span.SequenceEqual( value ) );
	}

	[Fact]
	public void TwoBucketInlineImageRemainsByteIdenticalToHw02ExpectedBytes() {
		byte[] image = Build(
			Sort(
				Record( "hw03-inline-0", 4, 0x11 ),
				Record( "hw03-inline-1", 5, 0x22 )
			),
			maximumDatabaseSize: 3 * PageSize
		);
		byte[] expectedDigest = Convert.FromHexString(
			"CF1FF3DA9BA1B66EB395C02E16EC5B0F589521E2693EC74A745C86FF799B21B6"
		);

		Assert.Equal( 3 * PageSize, image.Length );
		Assert.Equal( expectedDigest, SHA256.HashData( image ) );
	}

	[Fact]
	public void UnusedFinalOverflowBytesRemainZero() {
		byte[] image = BuildSingleRecord( 1025 );
		ReadOnlySpan<byte> unused = Page( image, 3 )[( HeaderSize + 1025 )..];

		Assert.All(
			unused.ToArray(),
			static value => Assert.Equal( (byte)0, value )
		);
	}
}
