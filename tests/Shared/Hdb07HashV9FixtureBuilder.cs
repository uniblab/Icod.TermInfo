/*
	Icod.TermInfo.Tests.Shared
	Creates deterministic valid and malformed Berkeley DB Hash-v9 fixtures for HDB07.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

using System.Buffers.Binary;
using System.Text;

namespace Icod.TermInfo.Tests.Shared;

internal enum Hdb07ByteOrder {
	LittleEndian,
	BigEndian,
}

internal readonly record struct Hdb07ItemSpec {
	private Hdb07ItemSpec(
		byte[] payload,
		bool isOffPage,
		int[] chunkLengths,
		int headerTrailingByteCount,
		bool appendEmptyOverflowPage
	) {
		Payload = payload;
		IsOffPage = isOffPage;
		ChunkLengths = chunkLengths;
		HeaderTrailingByteCount = headerTrailingByteCount;
		AppendEmptyOverflowPage = appendEmptyOverflowPage;
	}

	internal byte[] Payload { get; }

	internal bool IsOffPage { get; }

	internal int[] ChunkLengths { get; }

	internal int HeaderTrailingByteCount { get; }

	internal bool AppendEmptyOverflowPage { get; }

	internal int OverflowPageCount =>
		( IsOffPage )
			? checked(
				ChunkLengths.Length
				+ ( AppendEmptyOverflowPage ? 1 : 0 )
			)
			: 0
	;

	internal static Hdb07ItemSpec Inline( byte[] payload ) {
		ArgumentNullException.ThrowIfNull( payload );
		return new Hdb07ItemSpec(
			(byte[])payload.Clone(),
			isOffPage: false,
			[],
			headerTrailingByteCount: 0,
			appendEmptyOverflowPage: false
		);
	}

	internal static Hdb07ItemSpec OffPage(
		byte[] payload,
		int[] chunkLengths,
		int headerTrailingByteCount = 0,
		bool appendEmptyOverflowPage = false
	) {
		ArgumentNullException.ThrowIfNull( payload );
		ArgumentNullException.ThrowIfNull( chunkLengths );
		ArgumentOutOfRangeException.ThrowIfNegative( headerTrailingByteCount );

		int declaredLength = 0;
		foreach ( int chunkLength in chunkLengths ) {
			ArgumentOutOfRangeException.ThrowIfNegative( chunkLength );
			declaredLength = checked( declaredLength + chunkLength );
		}
		if ( declaredLength != payload.Length ) {
			throw new ArgumentException(
				"Overflow chunk lengths must exactly consume the payload.",
				nameof( chunkLengths )
			);
		}

		return new Hdb07ItemSpec(
			(byte[])payload.Clone(),
			isOffPage: true,
			(int[])chunkLengths.Clone(),
			headerTrailingByteCount,
			appendEmptyOverflowPage
		);
	}
}

internal readonly record struct Hdb07RecordSpec(
	Hdb07ItemSpec Key,
	Hdb07ItemSpec Value
);

internal static class Hdb07HashV9FixtureBuilder {
	private const uint HashMagic = 0x00061561;
	private const uint HashVersion = 9;
	private const byte MetadataPage = 8;
	private const byte OverflowPage = 7;
	private const byte HashPage = 13;
	private const byte InlineItem = 1;
	private const byte OffPageItem = 3;
	private const int PageHeaderSize = 26;

	internal static byte[] CreateDatabase(
		Hdb07ByteOrder byteOrder,
		int pageSize,
		params Hdb07RecordSpec[] records
	) {
		ArgumentNullException.ThrowIfNull( records );
		if (
			pageSize < 512
			|| pageSize > 64 * 1024
			|| ( pageSize & ( pageSize - 1 ) ) != 0
		) {
			throw new ArgumentOutOfRangeException(
				nameof( pageSize ),
				pageSize,
				"The page size must be a power of two from 512 through 65536."
			);
		}

		int overflowPageCount = 0;
		foreach ( Hdb07RecordSpec record in records ) {
			ValidateItem( record.Key );
			ValidateItem( record.Value );
			overflowPageCount = checked(
				overflowPageCount
				+ record.Key.OverflowPageCount
				+ record.Value.OverflowPageCount
			);
		}

		int lastPageNumber = checked( records.Length + overflowPageCount );
		byte[] database = new byte[
			checked( pageSize * ( lastPageNumber + 1 ) )
		];
		WriteUInt32( database, 8, 0, byteOrder );
		WriteUInt32( database, 12, HashMagic, byteOrder );
		WriteUInt32( database, 16, HashVersion, byteOrder );
		WriteUInt32( database, 20, checked( (uint)pageSize ), byteOrder );
		database[25] = MetadataPage;
		WriteUInt32(
			database,
			32,
			checked( (uint)lastPageNumber ),
			byteOrder
		);

		int nextOverflowPage = records.Length + 1;
		for ( int index = 0; index < records.Length; index++ ) {
			Hdb07RecordSpec record = records[index];
			byte[] key = CreateItem(
				record.Key,
				database,
				pageSize,
				byteOrder,
				ref nextOverflowPage
			);
			byte[] value = CreateItem(
				record.Value,
				database,
				pageSize,
				byteOrder,
				ref nextOverflowPage
			);
			WriteHashPage(
				database.AsSpan( ( index + 1 ) * pageSize, pageSize ),
				checked( (uint)( index + 1 ) ),
				byteOrder,
				key,
				value
			);
		}

		if ( nextOverflowPage != lastPageNumber + 1 ) {
			throw new InvalidOperationException(
				"The HDB07 fixture builder did not allocate its declared pages."
			);
		}
		return database;
	}

	internal static byte[] CreateCompiledEntry(
		string canonical,
		string description,
		params string[] aliases
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( canonical );
		ArgumentException.ThrowIfNullOrWhiteSpace( description );
		ArgumentNullException.ThrowIfNull( aliases );
		foreach ( string alias in aliases ) {
			ArgumentException.ThrowIfNullOrWhiteSpace( alias );
		}

		string identity = ( aliases.Length == 0 )
			? canonical + "|" + description + "\0"
			: canonical + "|" + string.Join( "|", aliases )
				+ "|" + description + "\0"
		;
		byte[] names = Encoding.Latin1.GetBytes( identity );
		int entryLength = checked( 12 + names.Length );
		if ( ( entryLength & 1 ) != 0 ) {
			entryLength++;
		}

		byte[] entry = new byte[entryLength];
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan( 0, 2 ),
			0x011A
		);
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan( 2, 2 ),
			checked( (ushort)names.Length )
		);
		names.CopyTo( entry.AsSpan( 12 ) );
		return entry;
	}

	internal static byte[] NcursesData( byte[] compiledEntry ) =>
		PrependMarker( compiledEntry, 0 )
	;

	internal static byte[] NcursesIndex( byte[] targetKey ) =>
		PrependMarker( targetKey, 2 )
	;

	private static void ValidateItem( Hdb07ItemSpec item ) {
		if ( item.Payload is null || item.ChunkLengths is null ) {
			throw new ArgumentException(
				"HDB07 record items must be created by Inline or OffPage."
			);
		}
	}

	private static byte[] CreateItem(
		Hdb07ItemSpec item,
		byte[] database,
		int pageSize,
		Hdb07ByteOrder byteOrder,
		ref int nextOverflowPage
	) {
		if ( !item.IsOffPage ) {
			byte[] inlineItem = new byte[checked( item.Payload.Length + 1 )];
			inlineItem[0] = InlineItem;
			item.Payload.CopyTo( inlineItem.AsSpan( 1 ) );
			return inlineItem;
		}

		int pageCount = item.OverflowPageCount;
		uint firstPage = ( pageCount == 0 )
			? 0
			: checked( (uint)nextOverflowPage )
		;
		byte[] offPageItem = new byte[
			checked( 12 + item.HeaderTrailingByteCount )
		];
		offPageItem[0] = OffPageItem;
		WriteUInt32( offPageItem, 4, firstPage, byteOrder );
		WriteUInt32(
			offPageItem,
			8,
			checked( (uint)item.Payload.Length ),
			byteOrder
		);

		int payloadOffset = 0;
		for ( int index = 0; index < pageCount; index++ ) {
			int chunkLength = ( index < item.ChunkLengths.Length )
				? item.ChunkLengths[index]
				: 0
			;
			if ( chunkLength > pageSize - PageHeaderSize ) {
				throw new ArgumentOutOfRangeException(
					nameof( item ),
					chunkLength,
					"An overflow chunk exceeds the selected page's payload capacity."
				);
			}

			int pageNumber = nextOverflowPage++;
			uint previousPage = ( index == 0 )
				? 0
				: checked( (uint)( pageNumber - 1 ) )
			;
			uint nextPage = ( index + 1 == pageCount )
				? 0
				: checked( (uint)( pageNumber + 1 ) )
			;
			Span<byte> page = database.AsSpan(
				checked( pageNumber * pageSize ),
				pageSize
			);
			WriteUInt32( page, 8, checked( (uint)pageNumber ), byteOrder );
			WriteUInt32( page, 12, previousPage, byteOrder );
			WriteUInt32( page, 16, nextPage, byteOrder );
			WriteUInt16( page, 20, 1, byteOrder );
			WriteUInt16(
				page,
				22,
				checked( (ushort)chunkLength ),
				byteOrder
			);
			page[25] = OverflowPage;
			item.Payload.AsSpan( payloadOffset, chunkLength ).CopyTo(
				page[PageHeaderSize..]
			);
			payloadOffset += chunkLength;
		}

		if ( payloadOffset != item.Payload.Length ) {
			throw new InvalidOperationException(
				"The HDB07 fixture builder did not consume the off-page payload."
			);
		}
		return offPageItem;
	}

	private static void WriteHashPage(
		Span<byte> page,
		uint pageNumber,
		Hdb07ByteOrder byteOrder,
		params byte[][] items
	) {
		WriteUInt32( page, 8, pageNumber, byteOrder );
		WriteUInt16(
			page,
			20,
			checked( (ushort)items.Length ),
			byteOrder
		);
		page[25] = HashPage;

		int offset = page.Length;
		for ( int index = 0; index < items.Length; index++ ) {
			offset = checked( offset - items[index].Length );
			int tableEnd = PageHeaderSize + ( items.Length * sizeof( ushort ) );
			if ( offset < tableEnd ) {
				throw new ArgumentException(
					"The record items do not fit on the selected Hash page."
				);
			}
			items[index].CopyTo( page[offset..] );
			WriteUInt16(
				page,
				PageHeaderSize + ( index * sizeof( ushort ) ),
				checked( (ushort)offset ),
				byteOrder
			);
		}
		WriteUInt16(
			page,
			22,
			unchecked( (ushort)offset ),
			byteOrder
		);
	}

	private static byte[] PrependMarker(
		byte[] payload,
		byte marker
	) {
		ArgumentNullException.ThrowIfNull( payload );
		byte[] result = new byte[checked( payload.Length + 1 )];
		result[0] = marker;
		payload.CopyTo( result.AsSpan( 1 ) );
		return result;
	}

	private static void WriteUInt16(
		Span<byte> bytes,
		int offset,
		ushort value,
		Hdb07ByteOrder byteOrder
	) {
		if ( byteOrder == Hdb07ByteOrder.BigEndian ) {
			BinaryPrimitives.WriteUInt16BigEndian(
				bytes.Slice( offset, sizeof( ushort ) ),
				value
			);
		} else {
			BinaryPrimitives.WriteUInt16LittleEndian(
				bytes.Slice( offset, sizeof( ushort ) ),
				value
			);
		}
	}

	private static void WriteUInt32(
		Span<byte> bytes,
		int offset,
		uint value,
		Hdb07ByteOrder byteOrder
	) {
		if ( byteOrder == Hdb07ByteOrder.BigEndian ) {
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
