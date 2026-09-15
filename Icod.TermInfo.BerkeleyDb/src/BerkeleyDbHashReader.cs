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
	private const byte HashPage = 13;
	private const byte HashKeyData = 1;
	private const int PageHeaderSize = 26;

	internal static bool TryReadValue(
		string databasePath,
		ReadOnlySpan<byte> requestedKey,
		out byte[] value
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( databasePath );

		byte[] database = File.ReadAllBytes( databasePath );
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
			if ( page[25] != HashPage ) {
				continue;
			}

			ushort entryCount = ReadUInt16( page, 20 );
			if ( ( entryCount & 1 ) != 0 ) {
				throw new InvalidDataException(
					$"Hash page {pageNumber} has an odd item count {entryCount}."
				);
			}

			ValidateIndexTable( page, entryCount, metadata.PageSize );

			for ( int index = 0; index < entryCount; index += 2 ) {
				byte[] key = ReadInlineItem(
					page,
					index,
					metadata.PageSize
				);
				if ( !requestedKey.SequenceEqual( key ) ) {
					continue;
				}

				value = ReadInlineItem(
					page,
					index + 1,
					metadata.PageSize
				);
				return true;
			}
		}

		value = [];
		return false;
	}

	private static DatabaseMetadata ReadMetadata( byte[] database ) {
		ArgumentNullException.ThrowIfNull( database );

		if ( database.Length < 512 ) {
			throw new InvalidDataException(
				"The Berkeley DB file is too small to contain a metadata page."
			);
		}

		ReadOnlySpan<byte> bytes = database;
		uint magic = BinaryPrimitives.ReadUInt32LittleEndian( bytes[12..16] );
		if ( magic != HashMagic ) {
			throw new InvalidDataException(
				"The file is not a supported little-endian Berkeley DB Hash database."
			);
		}

		uint version = BinaryPrimitives.ReadUInt32LittleEndian( bytes[16..20] );
		if ( version != SupportedHashVersion ) {
			throw new InvalidDataException(
				$"Berkeley DB Hash version {version} is not supported."
			);
		}

		uint pageSizeValue = BinaryPrimitives.ReadUInt32LittleEndian( bytes[20..24] );
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

		uint lastPageNumber = BinaryPrimitives.ReadUInt32LittleEndian( bytes[32..36] );
		if ( lastPageNumber >= database.Length / pageSize ) {
			throw new InvalidDataException(
				"The Berkeley DB metadata references a page beyond the file."
			);
		}

		return new DatabaseMetadata(
			pageSize,
			lastPageNumber
		);
	}

	private static void ValidateIndexTable(
		ReadOnlySpan<byte> page,
		ushort entryCount,
		int pageSize
	) {
		int tableEnd = PageHeaderSize + ( entryCount * sizeof( ushort ) );
		if ( tableEnd > pageSize ) {
			throw new InvalidDataException(
				"The Berkeley DB hash-page index table exceeds its page."
			);
		}

		ushort freeOffset = ReadUInt16( page, 22 );
		if ( freeOffset < tableEnd || freeOffset > pageSize ) {
			throw new InvalidDataException(
				$"Invalid Berkeley DB hash-page free offset {freeOffset}."
			);
		}

		int previousOffset = pageSize;
		for ( int index = 0; index < entryCount; index++ ) {
			ushort offset = ReadUInt16(
				page,
				PageHeaderSize + ( index * sizeof( ushort ) )
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

	private static byte[] ReadInlineItem(
		ReadOnlySpan<byte> page,
		int index,
		int pageSize
	) {
		ushort offset = ReadUInt16(
			page,
			PageHeaderSize + ( index * sizeof( ushort ) )
		);
		int upperOffset = pageSize;
		if ( index > 0 ) {
			upperOffset = ReadUInt16(
				page,
				PageHeaderSize + ( ( index - 1 ) * sizeof( ushort ) )
			);
		}

		int itemLength = upperOffset - offset;
		if ( itemLength <= 0 ) {
			throw new InvalidDataException(
				$"Invalid Berkeley DB hash item length {itemLength}."
			);
		}
		if ( page[offset] != HashKeyData ) {
			throw new InvalidDataException(
				$"Berkeley DB hash item type {page[offset]} is not supported yet."
			);
		}

		return page.Slice(
			offset + 1,
			itemLength - 1
		).ToArray();
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
		int offset
	) {
		return BinaryPrimitives.ReadUInt16LittleEndian(
			bytes.Slice(
				offset,
				sizeof( ushort )
			)
		);
	}

	private readonly record struct DatabaseMetadata(
		int PageSize,
		uint LastPageNumber
	);
}
