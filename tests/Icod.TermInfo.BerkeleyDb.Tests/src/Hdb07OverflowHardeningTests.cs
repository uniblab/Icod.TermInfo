/*
	Icod.TermInfo.BerkeleyDb.Tests
	Validates HDB07 rejection of trailing off-page and overflow storage data.
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

using System.Text;
using Icod.TermInfo.Tests.Shared;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hdb07OverflowHardeningTests {
	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void ReadRecordsRejectsOffPageHeaderTrailingBytes(
		bool isBigEndian
	) {
		byte[] database = Hdb07HashV9FixtureBuilder.CreateDatabase(
			GetByteOrder( isBigEndian ),
			512,
			new Hdb07RecordSpec(
				Hdb07ItemSpec.OffPage(
					Encoding.UTF8.GetBytes( "key" ),
					[ 3 ],
					headerTrailingByteCount: 1
				),
				Hdb07ItemSpec.Inline( [ 0x00 ] )
			)
		);

		InvalidDataException error = Assert.Throws<InvalidDataException>(
			() => BerkeleyDbHashReader.ReadRecords(
				database,
				maximumItemSize: 1024,
				maximumRecordCount: 8,
				CancellationToken.None
			)
		);
		Assert.Equal(
			"A Berkeley DB off-page item must contain exactly its 12-byte header.",
			error.Message
		);
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void TryReadValueRejectsOverflowPagesAfterDeclaredLength(
		bool isBigEndian
	) {
		byte[] requestedKey = Encoding.UTF8.GetBytes( "key" );
		byte[] database = Hdb07HashV9FixtureBuilder.CreateDatabase(
			GetByteOrder( isBigEndian ),
			512,
			new Hdb07RecordSpec(
				Hdb07ItemSpec.Inline( requestedKey ),
				Hdb07ItemSpec.OffPage(
					[ 0x42 ],
					[ 1 ],
					appendEmptyOverflowPage: true
				)
			)
		);

		InvalidDataException error = Assert.Throws<InvalidDataException>(
			() => BerkeleyDbHashReader.TryReadValue(
				database,
				requestedKey,
				out _,
				maximumItemSize: 1024
			)
		);
		Assert.Equal(
			"The Berkeley DB overflow chain continues after its declared length.",
			error.Message
		);
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void TryReadValueAcceptsZeroLengthOffPageItemWithPageZero(
		bool isBigEndian
	) {
		byte[] requestedKey = Encoding.UTF8.GetBytes( "key" );
		byte[] database = Hdb07HashV9FixtureBuilder.CreateDatabase(
			GetByteOrder( isBigEndian ),
			512,
			new Hdb07RecordSpec(
				Hdb07ItemSpec.Inline( requestedKey ),
				Hdb07ItemSpec.OffPage( [], [] )
			)
		);

		Assert.True(
			BerkeleyDbHashReader.TryReadValue(
				database,
				requestedKey,
				out byte[] value,
				maximumItemSize: 1024
			)
		);
		Assert.Empty( value );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void TryReadValueRejectsOverflowPageForZeroLengthItem(
		bool isBigEndian
	) {
		byte[] requestedKey = Encoding.UTF8.GetBytes( "key" );
		byte[] database = Hdb07HashV9FixtureBuilder.CreateDatabase(
			GetByteOrder( isBigEndian ),
			512,
			new Hdb07RecordSpec(
				Hdb07ItemSpec.Inline( requestedKey ),
				Hdb07ItemSpec.OffPage(
					[],
					[],
					appendEmptyOverflowPage: true
				)
			)
		);

		InvalidDataException error = Assert.Throws<InvalidDataException>(
			() => BerkeleyDbHashReader.TryReadValue(
				database,
				requestedKey,
				out _,
				maximumItemSize: 1024
			)
		);
		Assert.Equal(
			"The Berkeley DB overflow chain continues after its declared length.",
			error.Message
		);
	}

	private static Hdb07ByteOrder GetByteOrder( bool isBigEndian ) =>
		( isBigEndian )
			? Hdb07ByteOrder.BigEndian
			: Hdb07ByteOrder.LittleEndian
	;
}
