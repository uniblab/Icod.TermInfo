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
	private const byte OverflowPage = 7;
	private const byte HashPage = 13;
	private const byte HashKeyData = 1;
	private const byte HashOffPage = 3;
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
					index
				);
				if ( !requestedKey.SequenceEqual( key ) ) {
					continue;
				}

				value = ReadHashItem(
					database,
					metadata,
					page,
					index + 1
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

		ushort freeOffset = ReadUInt16( page, 22, isBigEndian );
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
		int index
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
				return page.Slice(
					offset + 1,
					itemLength - 1
				).ToArray();

			case HashOffPage:
				if ( itemLength < 12 ) {
					throw new InvalidDataException(
						"A Berkeley DB off-page item is shorter than its header."
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
					totalLength
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
		uint totalLength
	) {
		if (
			totalLength > int.MaxValue
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
			pageNumber = ReadUInt32(
				page,
				16,
				metadata.IsBigEndian
			);
		}

		if ( written != result.Length ) {
			throw new InvalidDataException(
				$"The Berkeley DB overflow chain supplied {written} bytes but declared {result.Length}."
			);
		}

		return result;
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
		return isBigEndian
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
		return isBigEndian
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
