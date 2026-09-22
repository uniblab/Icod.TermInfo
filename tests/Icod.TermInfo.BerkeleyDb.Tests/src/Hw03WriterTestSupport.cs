/*
	Icod.TermInfo.BerkeleyDb.Tests
	Provides deterministic HW03 Hash-v9 writer test fixtures.
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

internal static class Hw03WriterTestSupport {
	internal const int PageSize = 4096;
	internal const int HeaderSize = 26;
	internal readonly record struct ItemLocation(
		int PageNumber,
		int Offset
	);

	internal static byte[] FirstExactCollisionKey() =>
		Encoding.ASCII.GetBytes(
			"hw03-0c5ny4k-da6" + new string( 'x', 1000 )
		)
	;

	internal static byte[] SecondExactCollisionKey() =>
		Encoding.ASCII.GetBytes(
			"hw03-0fpxptj-1j8p" + new string( 'x', 1000 )
		)
	;

	internal static BerkeleyDbHashRecord Record(
		string key,
		int valueLength,
		byte fill
	) => new(
		Encoding.ASCII.GetBytes( key ),
		Enumerable.Repeat( fill, valueLength ).ToArray()
	);

	internal static ReadOnlySpan<byte> Page( byte[] image, int pageNumber ) =>
		image.AsSpan( checked( pageNumber * PageSize ), PageSize )
	;

	internal static uint ReadUInt32( byte[] image, int offset ) =>
		BinaryPrimitives.ReadUInt32LittleEndian(
			image.AsSpan( offset, sizeof( uint ) )
		)
	;

	internal static uint ReadPageNumber( byte[] image, int pageNumber ) =>
		ReadUInt32( image, checked( ( pageNumber * PageSize ) + 8 ) )
	;

	internal static uint ReadPagePrevious( byte[] image, int pageNumber ) =>
		ReadUInt32( image, checked( ( pageNumber * PageSize ) + 12 ) )
	;

	internal static uint ReadPageNext( byte[] image, int pageNumber ) =>
		ReadUInt32( image, checked( ( pageNumber * PageSize ) + 16 ) )
	;

	internal static BerkeleyDbHashRecord[] Sort(
		params BerkeleyDbHashRecord[] records
	) {
		Array.Sort(
			records,
			static ( left, right ) =>
				BerkeleyDbHashV9WriterKeyComparer.Instance.Compare(
					left.Key.Span,
					right.Key.Span
				)
		);
		return records;
	}

	internal static byte[] Build(
		IReadOnlyList<BerkeleyDbHashRecord> records,
		int maximumDatabaseSize = 64 * 1024 * 1024,
		CancellationToken cancellationToken = default
	) => BerkeleyDbHashV9ImageBuilder.Build(
		records,
		maximumDatabaseSize,
		cancellationToken
	);

	internal static byte[] BuildSingleRecord( int valueLength ) =>
		Build( Sort( Record( "hw03-overflow", valueLength, 0x5A ) ) )
	;

	internal static ItemLocation LocateOnlyValue( byte[] image ) {
		for ( int pageNumber = 1; pageNumber <= 2; pageNumber++ ) {
			ReadOnlySpan<byte> page = Page( image, pageNumber );
			ushort itemCount = BinaryPrimitives.ReadUInt16LittleEndian(
				page[20..22]
			);
			if ( itemCount == 0 ) {
				continue;
			}
			Assert.Equal( (ushort)2, itemCount );
			ushort valueOffset = BinaryPrimitives.ReadUInt16LittleEndian(
				page[28..30]
			);
			return new ItemLocation( pageNumber, valueOffset );
		}
		throw new Xunit.Sdk.XunitException( "The only record was not found." );
	}

	internal static void AssertOverflowPage(
		byte[] image,
		int pageNumber,
		int previous,
		int next,
		int chunkLength
	) {
		ReadOnlySpan<byte> page = Page( image, pageNumber );
		Assert.Equal( (uint)pageNumber, ReadPageNumber( image, pageNumber ) );
		Assert.Equal( (uint)previous, ReadPagePrevious( image, pageNumber ) );
		Assert.Equal( (uint)next, ReadPageNext( image, pageNumber ) );
		Assert.Equal(
			(ushort)1,
			BinaryPrimitives.ReadUInt16LittleEndian( page[20..22] )
		);
		Assert.Equal(
			(ushort)chunkLength,
			BinaryPrimitives.ReadUInt16LittleEndian( page[22..24] )
		);
		Assert.Equal( (byte)7, page[25] );
	}

	internal static BerkeleyDbHashRecord[] GrowthRecords() => Sort(
		Record( "hw03-growth-003", 1024, 0x03 ),
		Record( "hw03-growth-007", 1024, 0x07 ),
		Record( "hw03-growth-001", 1024, 0x01 ),
		Record( "hw03-growth-005", 1024, 0x05 )
	);

	internal static byte[] BuildGrowthRecords( int maximumDatabaseSize ) =>
		Build( GrowthRecords(), maximumDatabaseSize )
	;

	internal static BerkeleyDbHashRecord[] ExactCollisionRecords() => Sort(
		new BerkeleyDbHashRecord(
			FirstExactCollisionKey(),
			Enumerable.Repeat( (byte)0x31, 1024 ).ToArray()
		),
		new BerkeleyDbHashRecord(
			SecondExactCollisionKey(),
			Enumerable.Repeat( (byte)0x32, 1024 ).ToArray()
		)
	);
}

internal sealed class CancelAfterReadsList : IReadOnlyList<BerkeleyDbHashRecord> {
	private readonly IReadOnlyList<BerkeleyDbHashRecord> _records;
	private readonly CancellationTokenSource _source;
	private readonly int _cancelOnRead;
	private int _readCount;

	internal CancelAfterReadsList(
		IReadOnlyList<BerkeleyDbHashRecord> records,
		CancellationTokenSource source,
		int cancelOnRead
	) {
		_records = records;
		_source = source;
		_cancelOnRead = cancelOnRead;
	}

	public int Count => _records.Count;
	public BerkeleyDbHashRecord this[int index] {
		get {
			if ( ++_readCount == _cancelOnRead ) {
				_source.Cancel();
			}
			return _records[index];
		}
	}

	public IEnumerator<BerkeleyDbHashRecord> GetEnumerator() {
		for ( int index = 0; index < Count; index++ ) {
			yield return this[index];
		}
	}

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
		GetEnumerator()
	;
}
