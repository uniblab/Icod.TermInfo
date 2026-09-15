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
