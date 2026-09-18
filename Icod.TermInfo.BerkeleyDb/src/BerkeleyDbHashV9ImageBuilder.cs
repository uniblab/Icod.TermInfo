/*
	Icod.TermInfo.BerkeleyDb
	Builds deterministic ncurses-compatible Berkeley DB Hash-v9 inline images.
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
	private const byte HashPage = 13;
	private const byte InlineItem = 1;
	private const int PageHeaderSize = 26;
	private const int BucketCount = 2;
	private const int PageCount = BucketCount + 1;
	private const int BigItemThreshold = PageSize / 4;
	private static readonly byte[] CharacterKey =
		Encoding.ASCII.GetBytes( "%$sniglet^&\0" )
	;

	internal const int PageSize = 4096;

	internal static byte[] Build(
		IReadOnlyList<BerkeleyDbHashRecord> records,
		int maximumDatabaseSize,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( records );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
			maximumDatabaseSize
		);
		cancellationToken.ThrowIfCancellationRequested();
		if ( records.Count == 0 ) {
			throw new ArgumentException(
				"At least one Berkeley DB record is required.",
				nameof( records )
			);
		}

		int imageSize = checked( PageCount * PageSize );
		if ( maximumDatabaseSize < imageSize ) {
			throw new InvalidOperationException(
				$"The {maximumDatabaseSize}-byte database limit is smaller than the {imageSize}-byte Hash-v9 inline image."
			);
		}

		var buckets = new[] {
			new List<BerkeleyDbHashRecord>(),
			new List<BerkeleyDbHashRecord>(),
		};
		foreach ( BerkeleyDbHashRecord record in records ) {
			cancellationToken.ThrowIfCancellationRequested();
			ValidateInlinePayload( record.Key, "key" );
			ValidateInlinePayload( record.Value, "value" );
			int bucket = checked( (int)( Hash( record.Key.Span ) & 1 ) );
			buckets[bucket].Add( record );
		}

		byte[] image = new byte[imageSize];
		WriteMetadata( image, records );
		for ( int bucket = 0; bucket < BucketCount; bucket++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			WriteHashPage(
				image,
				checked( bucket + 1 ),
				buckets[bucket]
			);
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

	private static void ValidateInlinePayload(
		ReadOnlyMemory<byte> payload,
		string role
	) {
		if ( payload.Length > BigItemThreshold ) {
			throw new InvalidOperationException(
				$"A Berkeley DB record {role} is {payload.Length} bytes; HW02 supports at most {BigItemThreshold}-byte inline payloads."
			);
		}
	}

	private static void WriteMetadata(
		Span<byte> image,
		IReadOnlyList<BerkeleyDbHashRecord> records
	) {
		WriteNotLoggedLsn( image );
		WriteUInt32( image, 12, HashMagic );
		WriteUInt32( image, 16, HashVersion );
		WriteUInt32( image, 20, checked( (uint)PageSize ) );
		image[25] = MetadataPage;
		WriteUInt32(
			image,
			32,
			checked( (uint)( PageCount - 1 ) )
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
		int pageNumber,
		IReadOnlyList<BerkeleyDbHashRecord> records
	) {
		Span<byte> page = image.AsSpan(
			checked( pageNumber * PageSize ),
			PageSize
		);
		WriteNotLoggedLsn( page );
		WriteUInt32( page, 8, checked( (uint)pageNumber ) );
		page[25] = HashPage;

		int itemCount = checked( records.Count * 2 );
		WriteUInt16( page, 20, checked( (ushort)itemCount ) );
		int tableEnd = checked(
			PageHeaderSize + checked( itemCount * sizeof( ushort ) )
		);
		int offset = PageSize;
		int itemIndex = 0;
		foreach ( BerkeleyDbHashRecord record in records ) {
			WriteInlineItem(
				page,
				record.Key.Span,
				tableEnd,
				ref offset,
				itemIndex++
			);
			WriteInlineItem(
				page,
				record.Value.Span,
				tableEnd,
				ref offset,
				itemIndex++
			);
		}
		WriteUInt16( page, 22, checked( (ushort)offset ) );
	}

	private static void WriteInlineItem(
		Span<byte> page,
		ReadOnlySpan<byte> payload,
		int tableEnd,
		ref int offset,
		int itemIndex
	) {
		int itemLength = checked( payload.Length + 1 );
		offset = checked( offset - itemLength );
		if ( offset < tableEnd ) {
			throw new InvalidOperationException(
				"A Berkeley DB hash bucket does not fit in its canonical HW02 inline page."
			);
		}

		page[offset] = InlineItem;
		payload.CopyTo( page[checked( offset + 1 )..] );
		WriteUInt16(
			page,
			checked( PageHeaderSize + checked( itemIndex * sizeof( ushort ) ) ),
			checked( (ushort)offset )
		);
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
