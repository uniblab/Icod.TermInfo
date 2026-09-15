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

	private static byte[] CreateInlineHashV9Database(
		byte[] key,
		byte[] value
	) {
		const int pageSize = 512;
		const int pageHeaderSize = 26;
		const uint hashMagic = 0x00061561;
		const uint hashVersion = 9;
		const byte hashMetadataPage = 8;
		const byte hashPage = 13;
		const byte hashKeyData = 1;

		byte[] database = new byte[pageSize * 2];
		Span<byte> metadata = database.AsSpan( 0, pageSize );
		BinaryPrimitives.WriteUInt32LittleEndian( metadata[12..16], hashMagic );
		BinaryPrimitives.WriteUInt32LittleEndian( metadata[16..20], hashVersion );
		BinaryPrimitives.WriteUInt32LittleEndian( metadata[20..24], pageSize );
		metadata[24] = 0;
		metadata[25] = hashMetadataPage;
		metadata[26] = 0;
		BinaryPrimitives.WriteUInt32LittleEndian( metadata[32..36], 1 );

		Span<byte> page = database.AsSpan( pageSize, pageSize );
		page[25] = hashPage;
		BinaryPrimitives.WriteUInt16LittleEndian( page[20..22], 2 );

		int keyOffset = pageSize - 1 - key.Length;
		int valueOffset = keyOffset - 1 - value.Length;
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

		page[keyOffset] = hashKeyData;
		key.CopyTo( page[( keyOffset + 1 )..] );
		page[valueOffset] = hashKeyData;
		value.CopyTo( page[( valueOffset + 1 )..keyOffset] );

		return database;
	}
}
