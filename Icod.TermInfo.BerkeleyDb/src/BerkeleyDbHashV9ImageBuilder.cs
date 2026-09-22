/*
	Icod.TermInfo.BerkeleyDb
	Builds deterministic ncurses-compatible Berkeley DB Hash-v9 images.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This library is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This library is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

	You should have received a copy of the GNU Lesser General Public License
	along with this library.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Icod.TermInfo.BerkeleyDb;

internal static class BerkeleyDbHashV9ImageBuilder {
	private const uint HashMagic = 0x00061561;
	private const uint HashVersion = 9;
	private const byte MetadataPage = 8;
	private const byte OverflowPage = 7;
	private const byte HashPage = 13;
	private const byte InlineItem = 1;
	private const byte OffPageItem = 3;
	private const int PageHeaderSize = 26;
	private static readonly byte[] CharacterKey =
		Encoding.ASCII.GetBytes( "%$sniglet^&\0" )
	;

	internal const int PageSize = 4096;

	internal static byte[] Build(
		IReadOnlyList<BerkeleyDbHashRecord> records,
		int maximumDatabaseSize,
		CancellationToken cancellationToken
	) {
		BerkeleyDbHashV9LayoutPlan plan =
			BerkeleyDbHashV9LayoutPlanner.Create(
				records,
				maximumDatabaseSize,
				cancellationToken
			);
		byte[] image = new byte[plan.ImageSize];
		WriteMetadata( image, records, plan );
		foreach ( BerkeleyDbHashV9HashPagePlan page in plan.HashPages ) {
			cancellationToken.ThrowIfCancellationRequested();
			WriteHashPage( image, page );
		}
		foreach (
			BerkeleyDbHashV9OverflowPagePlan page in plan.OverflowPages
		) {
			cancellationToken.ThrowIfCancellationRequested();
			WriteOverflowPage( image, page );
		}
		return image;
	}

	internal static uint Hash( ReadOnlySpan<byte> key ) {
		uint result = 0;
		foreach ( byte value in key ) {
			result = unchecked( result * 16777619 );
			result ^= value;
		}
		return result;
	}

	private static void WriteMetadata(
		Span<byte> image,
		IReadOnlyList<BerkeleyDbHashRecord> records,
		BerkeleyDbHashV9LayoutPlan plan
	) {
		WriteNotLoggedLsn( image );
		WriteUInt32( image, 12, HashMagic );
		WriteUInt32( image, 16, HashVersion );
		WriteUInt32( image, 20, checked( (uint)PageSize ) );
		image[25] = MetadataPage;
		WriteUInt32(
			image,
			32,
			checked( (uint)( plan.PageCount - 1 ) )
		);

		CreateFileId( records ).CopyTo( image[52..72] );
		WriteUInt32( image, 72, 1 );
		WriteUInt32( image, 76, 1 );
		WriteUInt32( image, 80, 0 );
		WriteUInt32( image, 84, 0 );
		WriteUInt32( image, 88, checked( (uint)records.Count ) );
		WriteUInt32( image, 92, Hash( CharacterKey ) );
		WriteUInt32( image, 96, 1 );
		WriteUInt32( image, 100, 1 );
	}

	private static byte[] CreateFileId(
		IReadOnlyList<BerkeleyDbHashRecord> records
	) {
		using IncrementalHash hash = IncrementalHash.CreateHash(
			HashAlgorithmName.SHA256
		);
		Span<byte> length = stackalloc byte[sizeof( int )];
		foreach ( BerkeleyDbHashRecord record in records ) {
			BinaryPrimitives.WriteInt32LittleEndian(
				length,
				record.Key.Length
			);
			hash.AppendData( length );
			hash.AppendData( record.Key.Span );
			BinaryPrimitives.WriteInt32LittleEndian(
				length,
				record.Value.Length
			);
			hash.AppendData( length );
			hash.AppendData( record.Value.Span );
		}
		return hash.GetHashAndReset()[..20];
	}

	private static void WriteHashPage(
		byte[] image,
		BerkeleyDbHashV9HashPagePlan source
	) {
		Span<byte> page = image.AsSpan(
			checked( source.PageNumber * PageSize ),
			PageSize
		);
		WriteNotLoggedLsn( page );
		WriteUInt32( page, 8, checked( (uint)source.PageNumber ) );
		WriteUInt32( page, 12, checked( (uint)source.PreviousPageNumber ) );
		WriteUInt32( page, 16, checked( (uint)source.NextPageNumber ) );
		page[25] = HashPage;

		int itemCount = checked( source.Records.Count * 2 );
		WriteUInt16( page, 20, checked( (ushort)itemCount ) );
		int tableEnd = checked(
			PageHeaderSize + checked( itemCount * sizeof( ushort ) )
		);
		int offset = PageSize;
		int itemIndex = 0;
		foreach ( BerkeleyDbHashV9RecordPlan record in source.Records ) {
			WriteItem(
				page,
				record.Key,
				tableEnd,
				ref offset,
				itemIndex++
			);
			WriteItem(
				page,
				record.Value,
				tableEnd,
				ref offset,
				itemIndex++
			);
		}
		WriteUInt16( page, 22, checked( (ushort)offset ) );
	}

	private static void WriteItem(
		Span<byte> page,
		BerkeleyDbHashV9ItemPlan item,
		int tableEnd,
		ref int offset,
		int itemIndex
	) {
		int itemLength = item.EncodedLength;
		offset = checked( offset - itemLength );
		if ( offset < tableEnd ) {
			throw new InvalidOperationException(
				"A preflighted Berkeley DB hash item does not fit its planned page."
			);
		}

		if ( !item.IsOffPage ) {
			page[offset] = InlineItem;
			item.Payload.Span.CopyTo( page[checked( offset + 1 )..] );
		} else {
			page[offset] = OffPageItem;
			WriteUInt32(
				page,
				checked( offset + 4 ),
				checked( (uint)item.FirstOverflowPageNumber )
			);
			WriteUInt32(
				page,
				checked( offset + 8 ),
				checked( (uint)item.Payload.Length )
			);
		}
		WriteUInt16(
			page,
			checked( PageHeaderSize + checked( itemIndex * sizeof( ushort ) ) ),
			checked( (ushort)offset )
		);
	}

	private static void WriteOverflowPage(
		byte[] image,
		BerkeleyDbHashV9OverflowPagePlan source
	) {
		Span<byte> page = image.AsSpan(
			checked( source.PageNumber * PageSize ),
			PageSize
		);
		WriteNotLoggedLsn( page );
		WriteUInt32( page, 8, checked( (uint)source.PageNumber ) );
		WriteUInt32( page, 12, checked( (uint)source.PreviousPageNumber ) );
		WriteUInt32( page, 16, checked( (uint)source.NextPageNumber ) );
		WriteUInt16( page, 20, 1 );
		WriteUInt16(
			page,
			22,
			checked( (ushort)source.Payload.Length )
		);
		page[25] = OverflowPage;
		source.Payload.Span.CopyTo( page[PageHeaderSize..] );
	}

	private static void WriteNotLoggedLsn( Span<byte> bytes ) =>
		WriteUInt32( bytes, 4, 1 )
	;

	private static void WriteUInt16(
		Span<byte> bytes,
		int offset,
		ushort value
	) => BinaryPrimitives.WriteUInt16LittleEndian(
		bytes.Slice( offset, sizeof( ushort ) ),
		value
	);

	private static void WriteUInt32(
		Span<byte> bytes,
		int offset,
		uint value
	) => BinaryPrimitives.WriteUInt32LittleEndian(
		bytes.Slice( offset, sizeof( uint ) ),
		value
	);
}
