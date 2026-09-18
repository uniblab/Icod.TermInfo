/*
	Icod.TermInfo.Hw00.ManagedWriterProbe
	Research-only managed Berkeley DB Hash-v9 writer prototype.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Icod.TermInfo.Hw00.ManagedWriterProbe;

public static class Hw00HashV9Writer {
	private const uint HashMagic = 0x00061561;
	private const uint HashVersion = 9;
	private const byte MetadataPage = 8;
	private const byte OverflowPage = 7;
	private const byte HashPage = 13;
	private const byte InlineItem = 1;
	private const byte OffPageItem = 3;
	private const int PageHeaderSize = 26;
	private const int BigItemThreshold = PageSize / 4;
	private const int OverflowPayloadSize = PageSize - PageHeaderSize;
	private const ushort ClassicTermInfoMagic = 0x011A;
	private const ushort ExtendedTermInfoMagic = 0x021E;
	private static readonly byte[] CharacterKey =
		Encoding.ASCII.GetBytes( "%$sniglet^&\0" )
	;

	public const int PageSize = 4096;

	public static void WriteNcursesCatalog(
		Stream destination,
		IEnumerable<ReadOnlyMemory<byte>> compiledEntries
	) {
		ArgumentNullException.ThrowIfNull( destination );
		ArgumentNullException.ThrowIfNull( compiledEntries );
		if ( !destination.CanWrite ) {
			throw new ArgumentException(
				"The destination stream must be writable.",
				nameof( destination )
			);
		}

		List<HashRecord> records = CreateRecords( compiledEntries );
		var buckets = new[] {
			new List<HashRecord>(),
			new List<HashRecord>(),
		};
		foreach ( HashRecord record in records ) {
			int bucket = checked( (int)( Hash( record.Key ) & 1 ) );
			buckets[bucket].Add( record );
		}

		int overflowPageCount = records.Sum(
			static record =>
				GetOverflowPageCount( record.Key )
				+ GetOverflowPageCount( record.Value )
		);
		int lastPageNumber = checked( 2 + overflowPageCount );
		byte[] database = new byte[
			checked( ( lastPageNumber + 1 ) * PageSize )
		];

		WriteMetadata( database, records, lastPageNumber );
		int nextOverflowPage = 3;
		for ( int bucket = 0; bucket < buckets.Length; bucket++ ) {
			WriteHashPage(
				database,
				bucket + 1,
				buckets[bucket],
				ref nextOverflowPage
			);
		}
		if ( nextOverflowPage != lastPageNumber + 1 ) {
			throw new InvalidOperationException(
				"The HW00 writer did not allocate its declared overflow pages."
			);
		}

		if ( destination.CanSeek ) {
			destination.Position = 0;
			destination.SetLength( 0 );
		}
		destination.Write( database );
	}

	private static List<HashRecord> CreateRecords(
		IEnumerable<ReadOnlyMemory<byte>> compiledEntries
	) {
		var records = new List<HashRecord>();
		foreach ( ReadOnlyMemory<byte> source in compiledEntries ) {
			CompiledEntry entry = ParseCompiledEntry( source );
			foreach ( byte[] name in entry.Names ) {
				records.Add(
					new HashRecord(
						name,
						PrependMarker( entry.StorageKey, 2 )
					)
				);
			}
			records.Add(
				new HashRecord(
					entry.StorageKey,
					PrependMarker( entry.CompiledBytes, 0 )
				)
			);
		}

		records.Sort(
			static ( left, right ) => ByteArrayComparer.Instance.Compare(
				left.Key,
				right.Key
			)
		);
		for ( int index = 1; index < records.Count; index++ ) {
			if ( records[index - 1].Key.AsSpan().SequenceEqual( records[index].Key ) ) {
				throw new ArgumentException(
					$"Duplicate ncurses catalog key {Convert.ToHexString( records[index].Key )}.",
					nameof( compiledEntries )
				);
			}
		}
		return records;
	}

	private static CompiledEntry ParseCompiledEntry(
		ReadOnlyMemory<byte> source
	) {
		byte[] compiledBytes = source.ToArray();
		if ( compiledBytes.Length < 12 ) {
			throw new InvalidDataException(
				"A compiled terminfo entry is shorter than its header."
			);
		}

		ushort magic = BinaryPrimitives.ReadUInt16LittleEndian( compiledBytes );
		if ( magic != ClassicTermInfoMagic && magic != ExtendedTermInfoMagic ) {
			throw new InvalidDataException(
				$"Unsupported compiled terminfo magic 0x{magic:X4}."
			);
		}

		int namesLength = BinaryPrimitives.ReadUInt16LittleEndian(
			compiledBytes.AsSpan( 2, 2 )
		);
		if (
			namesLength < 3
			|| namesLength > compiledBytes.Length - 12
			|| compiledBytes[12 + namesLength - 1] != 0
		) {
			throw new InvalidDataException(
				"The compiled terminfo names section is invalid."
			);
		}

		byte[] identity = compiledBytes.AsSpan( 12, namesLength - 1 ).ToArray();
		var names = new List<byte[]>();
		int segmentStart = 0;
		for ( int index = 0; index < identity.Length; index++ ) {
			if ( identity[index] != (byte)'|' ) {
				continue;
			}
			if ( index == segmentStart ) {
				throw new InvalidDataException(
					"A compiled terminfo entry contains an empty published name."
				);
			}
			names.Add( identity.AsSpan( segmentStart, index - segmentStart ).ToArray() );
			segmentStart = index + 1;
		}
		if ( names.Count == 0 || segmentStart >= identity.Length ) {
			throw new InvalidDataException(
				"The compiled terminfo names section must contain a name and description."
			);
		}

		return new CompiledEntry( compiledBytes, identity, names );
	}

	private static void WriteMetadata(
		Span<byte> database,
		IReadOnlyList<HashRecord> records,
		int lastPageNumber
	) {
		WriteNotLoggedLsn( database );
		WriteUInt32( database, 12, HashMagic );
		WriteUInt32( database, 16, HashVersion );
		WriteUInt32( database, 20, PageSize );
		database[25] = MetadataPage;
		WriteUInt32( database, 32, checked( (uint)lastPageNumber ) );

		byte[] fileId = CreateFileId( records );
		fileId.CopyTo( database[52..72] );
		WriteUInt32( database, 72, 1 );
		WriteUInt32( database, 76, 1 );
		WriteUInt32( database, 80, 0 );
		WriteUInt32( database, 84, 0 );
		WriteUInt32( database, 88, checked( (uint)records.Count ) );
		WriteUInt32( database, 92, Hash( CharacterKey ) );
		WriteUInt32( database, 96, 1 );
		WriteUInt32( database, 100, 1 );
	}

	private static byte[] CreateFileId( IReadOnlyList<HashRecord> records ) {
		using IncrementalHash hash = IncrementalHash.CreateHash(
			HashAlgorithmName.SHA256
		);
		Span<byte> length = stackalloc byte[sizeof( int )];
		foreach ( HashRecord record in records ) {
			BinaryPrimitives.WriteInt32LittleEndian( length, record.Key.Length );
			hash.AppendData( length );
			hash.AppendData( record.Key );
			BinaryPrimitives.WriteInt32LittleEndian( length, record.Value.Length );
			hash.AppendData( length );
			hash.AppendData( record.Value );
		}
		return hash.GetHashAndReset()[..20];
	}

	private static void WriteHashPage(
		byte[] database,
		int pageNumber,
		IReadOnlyList<HashRecord> records,
		ref int nextOverflowPage
	) {
		Span<byte> page = database.AsSpan( pageNumber * PageSize, PageSize );
		WriteNotLoggedLsn( page );
		WriteUInt32( page, 8, checked( (uint)pageNumber ) );
		page[25] = HashPage;

		var items = new List<byte[]>( checked( records.Count * 2 ) );
		foreach ( HashRecord record in records ) {
			items.Add( CreateItem( database, record.Key, ref nextOverflowPage ) );
			items.Add( CreateItem( database, record.Value, ref nextOverflowPage ) );
		}
		WriteUInt16( page, 20, checked( (ushort)items.Count ) );

		int offset = PageSize;
		int tableEnd = checked( PageHeaderSize + ( items.Count * sizeof( ushort ) ) );
		for ( int index = 0; index < items.Count; index++ ) {
			offset = checked( offset - items[index].Length );
			if ( offset < tableEnd ) {
				throw new InvalidOperationException(
					$"Bucket {pageNumber - 1} does not fit in the canonical HW00 page."
				);
			}
			items[index].CopyTo( page[offset..] );
			WriteUInt16(
				page,
				PageHeaderSize + ( index * sizeof( ushort ) ),
				checked( (ushort)offset )
			);
		}
		WriteUInt16( page, 22, checked( (ushort)offset ) );
	}

	private static byte[] CreateItem(
		byte[] database,
		byte[] payload,
		ref int nextOverflowPage
	) {
		if ( payload.Length <= BigItemThreshold ) {
			return PrependMarker( payload, InlineItem );
		}

		int pageCount = GetOverflowPageCount( payload );
		int firstPage = nextOverflowPage;
		int payloadOffset = 0;
		for ( int index = 0; index < pageCount; index++ ) {
			int pageNumber = nextOverflowPage++;
			int chunkLength = Math.Min(
				OverflowPayloadSize,
				payload.Length - payloadOffset
			);
			Span<byte> page = database.AsSpan(
				checked( pageNumber * PageSize ),
				PageSize
			);
			WriteNotLoggedLsn( page );
			WriteUInt32( page, 8, checked( (uint)pageNumber ) );
			WriteUInt32(
				page,
				12,
				( index == 0 ) ? 0U : checked( (uint)( pageNumber - 1 ) )
			);
			WriteUInt32(
				page,
				16,
				( index + 1 == pageCount )
					? 0U
					: checked( (uint)( pageNumber + 1 ) )
			);
			WriteUInt16( page, 20, 1 );
			WriteUInt16( page, 22, checked( (ushort)chunkLength ) );
			page[25] = OverflowPage;
			payload.AsSpan( payloadOffset, chunkLength ).CopyTo(
				page[PageHeaderSize..]
			);
			payloadOffset += chunkLength;
		}

		byte[] result = new byte[12];
		result[0] = OffPageItem;
		WriteUInt32( result, 4, checked( (uint)firstPage ) );
		WriteUInt32( result, 8, checked( (uint)payload.Length ) );
		return result;
	}

	private static int GetOverflowPageCount( byte[] payload ) =>
		( payload.Length <= BigItemThreshold )
			? 0
			: checked(
				( payload.Length + OverflowPayloadSize - 1 )
				/ OverflowPayloadSize
			)
	;

	private static byte[] PrependMarker( byte[] payload, byte marker ) {
		byte[] result = new byte[checked( payload.Length + 1 )];
		result[0] = marker;
		payload.CopyTo( result.AsSpan( 1 ) );
		return result;
	}

	private static uint Hash( ReadOnlySpan<byte> key ) {
		uint result = 0;
		foreach ( byte value in key ) {
			result = unchecked( result * 16777619 );
			result ^= value;
		}
		return result;
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

	private sealed record CompiledEntry(
		byte[] CompiledBytes,
		byte[] StorageKey,
		IReadOnlyList<byte[]> Names
	);

	private sealed record HashRecord( byte[] Key, byte[] Value );

	private sealed class ByteArrayComparer : IComparer<byte[]> {
		internal static readonly ByteArrayComparer Instance = new();

		public int Compare( byte[]? left, byte[]? right ) {
			if ( ReferenceEquals( left, right ) ) {
				return 0;
			}
			if ( left is null ) {
				return -1;
			}
			if ( right is null ) {
				return 1;
			}
			int commonLength = Math.Min( left.Length, right.Length );
			for ( int index = 0; index < commonLength; index++ ) {
				int comparison = left[index].CompareTo( right[index] );
				if ( comparison != 0 ) {
					return comparison;
				}
			}

			// Berkeley DB Hash-v9 orders a longer key before its exact prefix.
			return right.Length.CompareTo( left.Length );
		}
	}
}
