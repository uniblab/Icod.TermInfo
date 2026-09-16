/*
	Icod.TermInfo.BerkeleyDb
	Managed read-only support for ncurses-compatible Berkeley DB Hash-v9 terminfo stores.
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

namespace Icod.TermInfo.BerkeleyDb;

internal static class BerkeleyDbHashReader {
	private const uint HashMagic = 0x00061561;
	private const uint SupportedHashVersion = 9;
	private const byte HashMetadataPage = 8;
	private const byte UnusedPage = 0;
	private const byte OverflowPage = 7;
	private const byte HashPage = 13;
	private const byte HashKeyData = 1;
	private const byte HashOffPage = 3;
	private const int PageHeaderSize = 26;

	internal static bool TryReadValue(
		string databasePath,
		ReadOnlySpan<byte> requestedKey,
		out byte[] value,
		int maximumDatabaseSize = 64 * 1024 * 1024,
		int maximumItemSize = 1024 * 1024
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( databasePath );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumDatabaseSize );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumItemSize );

		byte[] database = ReadDatabase( databasePath, maximumDatabaseSize );
		return TryReadValue( database, requestedKey, out value, maximumItemSize );
	}

	// The caller owns this acquired image and keeps it unchanged during lookup.
	internal static bool TryReadValue(
		byte[] database,
		ReadOnlySpan<byte> requestedKey,
		out byte[] value,
		int maximumItemSize
	) {
		ArgumentNullException.ThrowIfNull( database );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumItemSize );

		DatabaseMetadata metadata = ReadMetadata( database );

		for (
			uint pageNumber = 1;
			pageNumber <= metadata.LastPageNumber;
			pageNumber++
		) {
			ReadOnlySpan<byte> page = GetPage(
				database,
				metadata,
				pageNumber
			);
			if ( page[25] == UnusedPage || page[25] == OverflowPage ) {
				continue;
			}
			if ( page[25] != HashPage ) {
				throw new InvalidDataException(
					$"Berkeley DB page type {page[25]} at page {pageNumber} is not supported."
				);
			}

			ValidatePageIdentity( page, pageNumber, metadata.IsBigEndian );

			ushort entryCount = ReadUInt16(
				page,
				20,
				metadata.IsBigEndian
			);
			if ( ( entryCount & 1 ) != 0 ) {
				throw new InvalidDataException(
					$"Hash page {pageNumber} has an odd item count {entryCount}."
				);
			}

			ValidateIndexTable(
				page,
				entryCount,
				metadata.PageSize,
				metadata.IsBigEndian
			);

			for ( int index = 0; index < entryCount; index += 2 ) {
				byte[] key = ReadHashItem(
					database,
					metadata,
					page,
					index,
					maximumItemSize
				);
				if ( !requestedKey.SequenceEqual( key ) ) {
					continue;
				}

				value = ReadHashItem(
					database,
					metadata,
					page,
					index + 1,
					maximumItemSize
				);
				return true;
			}
		}

		value = [];
		return false;
	}

	internal static IReadOnlyList<BerkeleyDbHashRecord> ReadRecords(
		byte[] database,
		int maximumItemSize,
		int maximumRecordCount,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( database );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumItemSize );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumRecordCount );
		cancellationToken.ThrowIfCancellationRequested();

		DatabaseMetadata metadata = ReadMetadata( database );
		var records = new List<BerkeleyDbHashRecord>();
		var keys = new HashSet<byte[]>( ByteArrayComparer.Instance );

		for (
			uint pageNumber = 1;
			pageNumber <= metadata.LastPageNumber;
			pageNumber++
		) {
			cancellationToken.ThrowIfCancellationRequested();

			ReadOnlySpan<byte> page = GetPage(
				database,
				metadata,
				pageNumber
			);
			if ( page[25] == UnusedPage || page[25] == OverflowPage ) {
				continue;
			}
			if ( page[25] != HashPage ) {
				throw new InvalidDataException(
					$"Berkeley DB page type {page[25]} at page {pageNumber} is not supported."
				);
			}

			ValidatePageIdentity( page, pageNumber, metadata.IsBigEndian );

			ushort entryCount = ReadUInt16(
				page,
				20,
				metadata.IsBigEndian
			);
			if ( ( entryCount & 1 ) != 0 ) {
				throw new InvalidDataException(
					$"Hash page {pageNumber} has an odd item count {entryCount}."
				);
			}

			ValidateIndexTable(
				page,
				entryCount,
				metadata.PageSize,
				metadata.IsBigEndian
			);

			for ( int index = 0; index < entryCount; index += 2 ) {
				cancellationToken.ThrowIfCancellationRequested();
				if ( records.Count >= maximumRecordCount ) {
					throw new InvalidDataException(
						$"The Berkeley DB contains more than {maximumRecordCount} records."
					);
				}

				byte[] key = ReadHashItem(
					database,
					metadata,
					page,
					index,
					maximumItemSize
				);
				if ( !keys.Add( key ) ) {
					throw new InvalidDataException(
						"The Berkeley DB contains a duplicate exact byte key."
					);
				}

				byte[] value = ReadHashItem(
					database,
					metadata,
					page,
					index + 1,
					maximumItemSize
				);
				records.Add( new BerkeleyDbHashRecord( key, value ) );
			}
		}

		records.Sort(
			static ( left, right ) =>
				ByteArrayComparer.Instance.Compare(
					left.Key.Span,
					right.Key.Span
				)
		);
		return records.AsReadOnly();
	}

	internal static byte[] ReadDatabase(
		string databasePath,
		int maximumDatabaseSize
	) {
		using FileStream stream = new FileStream(
			databasePath,
			FileMode.Open,
			FileAccess.Read,
			FileShare.Read,
			4096,
			FileOptions.SequentialScan
		);
		return ReadStableDatabase( stream, maximumDatabaseSize );
	}

	internal static byte[] ReadStableDatabase(
		Stream stream,
		int maximumDatabaseSize
	) {
		return ReadDatabase( stream, maximumDatabaseSize );
	}

	// Borrows a readable, length-reporting stream positioned at byte zero.
	// The path wrapper supplies a fresh FileStream and retains sole ownership.
	internal static byte[] ReadDatabase(
		Stream stream,
		int maximumDatabaseSize
	) {
		ArgumentNullException.ThrowIfNull( stream );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumDatabaseSize );

		long length = stream.Length;
		if ( length > maximumDatabaseSize || length > Array.MaxLength ) {
			throw new InvalidDataException(
				$"Berkeley DB file length {length} exceeds the supported database size."
			);
		}
		if ( length < 512 ) {
			throw new InvalidDataException(
				"The Berkeley DB file is too small to contain a metadata page."
			);
		}

		byte[] database = new byte[(int)length];
		stream.ReadExactly( database );
		if ( stream.ReadByte() != -1 ) {
			throw new IOException(
				"The Berkeley DB file grew while it was being read."
			);
		}

		return database;
	}

	private static DatabaseMetadata ReadMetadata( byte[] database ) {
		ArgumentNullException.ThrowIfNull( database );

		if ( database.Length < 512 ) {
			throw new InvalidDataException(
				"The Berkeley DB file is too small to contain a metadata page."
			);
		}

		ReadOnlySpan<byte> bytes = database;
		bool isBigEndian;
		uint magic = BinaryPrimitives.ReadUInt32LittleEndian( bytes[12..16] );
		if ( magic == HashMagic ) {
			isBigEndian = false;
		} else if (
			BinaryPrimitives.ReadUInt32BigEndian( bytes[12..16] ) == HashMagic
		) {
			isBigEndian = true;
		} else {
			throw new InvalidDataException(
				"The file is not a supported Berkeley DB Hash database."
			);
		}

		ValidatePageIdentity( bytes, 0, isBigEndian );

		uint version = ReadUInt32( bytes, 16, isBigEndian );
		if ( version != SupportedHashVersion ) {
			throw new InvalidDataException(
				$"Berkeley DB Hash version {version} is not supported."
			);
		}

		uint pageSizeValue = ReadUInt32( bytes, 20, isBigEndian );
		if (
			pageSizeValue < 512
			|| pageSizeValue > 64 * 1024
			|| ( pageSizeValue & ( pageSizeValue - 1 ) ) != 0
			|| pageSizeValue > int.MaxValue
		) {
			throw new InvalidDataException(
				$"Invalid Berkeley DB page size {pageSizeValue}."
			);
		}

		int pageSize = (int)pageSizeValue;
		if ( database.Length % pageSize != 0 ) {
			throw new InvalidDataException(
				"The Berkeley DB file length is not a whole number of pages."
			);
		}
		if ( bytes[24] != 0 ) {
			throw new InvalidDataException(
				"Encrypted Berkeley DB files are not supported."
			);
		}
		if ( bytes[25] != HashMetadataPage ) {
			throw new InvalidDataException(
				$"Unexpected Berkeley DB metadata page type {bytes[25]}."
			);
		}
		if ( ( bytes[26] & 0x01 ) != 0 ) {
			throw new InvalidDataException(
				"Checksummed Berkeley DB pages are not supported."
			);
		}

		if ( bytes[26] != 0 || ReadUInt32( bytes, 36, isBigEndian ) != 0 ) {
			throw new InvalidDataException(
				"Berkeley DB metadata feature flags and partitioned files are not supported."
			);
		}
		uint hashFlags = ReadUInt32( bytes, 48, isBigEndian );
		if ( hashFlags != 0 ) {
			throw new InvalidDataException(
				$"Berkeley DB Hash feature flags 0x{hashFlags:X8} are not supported."
			);
		}

		uint lastPageNumber = ReadUInt32( bytes, 32, isBigEndian );
		if ( lastPageNumber >= database.Length / pageSize ) {
			throw new InvalidDataException(
				"The Berkeley DB metadata references a page beyond the file."
			);
		}

		return new DatabaseMetadata(
			pageSize,
			lastPageNumber,
			isBigEndian
		);
	}

	private static void ValidateIndexTable(
		ReadOnlySpan<byte> page,
		ushort entryCount,
		int pageSize,
		bool isBigEndian
	) {
		int tableEnd = PageHeaderSize + ( entryCount * sizeof( ushort ) );
		if ( tableEnd > pageSize ) {
			throw new InvalidDataException(
				"The Berkeley DB hash-page index table exceeds its page."
			);
		}

		int freeOffset = ReadUInt16( page, 22, isBigEndian );
		if ( pageSize == 64 * 1024 && entryCount == 0 && freeOffset == 0 ) {
			// P_INIT stores the empty page size in a 16-bit db_indx_t.
			freeOffset = pageSize;
		}
		if ( freeOffset < tableEnd || freeOffset > pageSize ) {
			throw new InvalidDataException(
				$"Invalid Berkeley DB hash-page free offset {freeOffset}."
			);
		}

		int previousOffset = pageSize;
		for ( int index = 0; index < entryCount; index++ ) {
			ushort offset = ReadUInt16(
				page,
				PageHeaderSize + ( index * sizeof( ushort ) ),
				isBigEndian
			);
			if (
				offset < freeOffset
				|| offset >= previousOffset
				|| offset >= pageSize
			) {
				throw new InvalidDataException(
					$"Invalid Berkeley DB hash item offset {offset} at index {index}."
				);
			}
			previousOffset = offset;
		}
	}

	private static byte[] ReadHashItem(
		byte[] database,
		DatabaseMetadata metadata,
		ReadOnlySpan<byte> page,
		int index,
		int maximumItemSize
	) {
		ushort offset = ReadUInt16(
			page,
			PageHeaderSize + ( index * sizeof( ushort ) ),
			metadata.IsBigEndian
		);
		int upperOffset = metadata.PageSize;
		if ( index > 0 ) {
			upperOffset = ReadUInt16(
				page,
				PageHeaderSize + ( ( index - 1 ) * sizeof( ushort ) ),
				metadata.IsBigEndian
			);
		}

		int itemLength = upperOffset - offset;
		if ( itemLength <= 0 ) {
			throw new InvalidDataException(
				$"Invalid Berkeley DB hash item length {itemLength}."
			);
		}

		switch ( page[offset] ) {
			case HashKeyData:
				if ( itemLength - 1 > maximumItemSize ) {
					throw new InvalidDataException(
						"The Berkeley DB inline item exceeds the configured item size."
					);
				}
				return page.Slice(
					offset + 1,
					itemLength - 1
				).ToArray();

			case HashOffPage:
				if ( itemLength != 12 ) {
					throw new InvalidDataException(
						"A Berkeley DB off-page item must contain exactly its 12-byte header."
					);
				}

				uint overflowPage = ReadUInt32(
					page,
					offset + 4,
					metadata.IsBigEndian
				);
				uint totalLength = ReadUInt32(
					page,
					offset + 8,
					metadata.IsBigEndian
				);
				return ReadOverflow(
					database,
					metadata,
					overflowPage,
					totalLength,
					maximumItemSize
				);

			default:
				throw new InvalidDataException(
					$"Berkeley DB hash item type {page[offset]} is not supported yet."
				);
		}
	}

	private static byte[] ReadOverflow(
		byte[] database,
		DatabaseMetadata metadata,
		uint firstPage,
		uint totalLength,
		int maximumItemSize
	) {
		if (
			totalLength > maximumItemSize
			|| totalLength > Array.MaxLength
			|| totalLength > database.Length
		) {
			throw new InvalidDataException(
				$"Invalid Berkeley DB overflow length {totalLength}."
			);
		}

		byte[] result = new byte[(int)totalLength];
		int written = 0;
		uint pageNumber = firstPage;
		HashSet<uint> visited = [];
		if ( result.Length == 0 && pageNumber != 0 ) {
			throw new InvalidDataException(
				"The Berkeley DB overflow chain continues after its declared length."
			);
		}

		while ( pageNumber != 0 ) {
			if ( !visited.Add( pageNumber ) ) {
				throw new InvalidDataException(
					"The Berkeley DB overflow chain contains a cycle."
				);
			}

			ReadOnlySpan<byte> page = GetPage(
				database,
				metadata,
				pageNumber
			);
			if ( page[25] != OverflowPage ) {
				throw new InvalidDataException(
					$"Expected overflow page {pageNumber}, found page type {page[25]}."
				);
			}

			ValidatePageIdentity( page, pageNumber, metadata.IsBigEndian );

			ushort chunkLength = ReadUInt16(
				page,
				22,
				metadata.IsBigEndian
			);
			if (
				chunkLength > metadata.PageSize - PageHeaderSize
				|| chunkLength > result.Length - written
			) {
				throw new InvalidDataException(
					"The Berkeley DB overflow chain exceeds its declared length."
				);
			}

			page.Slice(
				PageHeaderSize,
				chunkLength
			).CopyTo(
				result.AsSpan( written )
			);
			written += chunkLength;
			uint nextPageNumber = ReadUInt32(
				page,
				16,
				metadata.IsBigEndian
			);
			if ( written == result.Length && nextPageNumber != 0 ) {
				throw new InvalidDataException(
					"The Berkeley DB overflow chain continues after its declared length."
				);
			}
			pageNumber = nextPageNumber;
		}

		if ( written != result.Length ) {
			throw new InvalidDataException(
				$"The Berkeley DB overflow chain supplied {written} bytes but declared {result.Length}."
			);
		}

		return result;
	}

	private static void ValidatePageIdentity(
		ReadOnlySpan<byte> page,
		uint expectedPageNumber,
		bool isBigEndian
	) {
		uint storedPageNumber = ReadUInt32( page, 8, isBigEndian );
		if ( storedPageNumber != expectedPageNumber ) {
			throw new InvalidDataException(
				$"Berkeley DB page {expectedPageNumber} identifies itself as page {storedPageNumber}."
			);
		}
	}

	private static ReadOnlySpan<byte> GetPage(
		byte[] database,
		DatabaseMetadata metadata,
		uint pageNumber
	) {
		if ( pageNumber > metadata.LastPageNumber ) {
			throw new InvalidDataException(
				$"Berkeley DB page {pageNumber} is outside the database."
			);
		}

		long offset = (long)pageNumber * metadata.PageSize;
		if ( offset > database.Length - metadata.PageSize ) {
			throw new InvalidDataException(
				$"Berkeley DB page {pageNumber} is truncated."
			);
		}

		return database.AsSpan(
			(int)offset,
			metadata.PageSize
		);
	}

	private static ushort ReadUInt16(
		ReadOnlySpan<byte> bytes,
		int offset,
		bool isBigEndian
	) {
		ReadOnlySpan<byte> value = bytes.Slice(
			offset,
			sizeof( ushort )
		);
		return ( isBigEndian )
			? BinaryPrimitives.ReadUInt16BigEndian( value )
			: BinaryPrimitives.ReadUInt16LittleEndian( value )
		;
	}

	private static uint ReadUInt32(
		ReadOnlySpan<byte> bytes,
		int offset,
		bool isBigEndian
	) {
		ReadOnlySpan<byte> value = bytes.Slice(
			offset,
			sizeof( uint )
		);
		return ( isBigEndian )
			? BinaryPrimitives.ReadUInt32BigEndian( value )
			: BinaryPrimitives.ReadUInt32LittleEndian( value )
		;
	}

	private readonly record struct DatabaseMetadata(
		int PageSize,
		uint LastPageNumber,
		bool IsBigEndian
	);
}
