/*
	Hdb00.ManagedProbe
	Research-only managed reader for ncurses Berkeley DB Hash v9 fixtures.
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

namespace Hdb00.ManagedProbe;

internal static class Program {
	private const uint HashMagic = 0x00061561;
	private const uint SupportedHashVersion = 9;
	private const byte HashMetadataPage = 8;
	private const byte OverflowPage = 7;
	private const byte HashPage = 13;
	private const byte HashKeyData = 1;
	private const byte HashOffPage = 3;
	private const byte NcursesDataRecord = 0;
	private const byte NcursesIndexRecord = 2;
	private const int PageHeaderSize = 26;
	private const int MaximumNcursesHops = 3;

	private static int Main( string[] args ) {
		if ( args.Length != 3 ) {
			Console.Error.WriteLine(
				"Usage: Hdb00.ManagedProbe DATABASE TERM OUTPUT"
			);
			return 64;
		}

		try {
			byte[] database =
				File.ReadAllBytes( args[0] );
			DatabaseMetadata metadata =
				ReadMetadata( database );
			byte[] key =
				Encoding.UTF8.GetBytes( args[1] );

			for ( int hop = 1; hop <= MaximumNcursesHops; hop++ ) {
				if (
					!TryFindValue(
						database,
						metadata,
						key,
						out byte[] value
					)
				) {
					Console.Error.WriteLine(
						$"Clean miss for '{args[1]}'."
					);
					return 3;
				}

				if ( value.Length == 0 ) {
					throw new InvalidDataException(
						"The ncurses hashed-term record is empty."
					);
				}

				switch ( value[0] ) {
					case NcursesDataRecord:
						byte[] compiled = value[1..];
						File.WriteAllBytes( args[2], compiled );

						( int dataRecords, int indexRecords ) =
							CountNcursesRecords(
								database,
								metadata
							);

						Console.WriteLine(
							$"Berkeley DB Hash version: {metadata.Version}"
						);
						Console.WriteLine(
							$"Byte order: {( metadata.BigEndian ? "big" : "little" )}-endian"
						);
						Console.WriteLine(
							$"Page size: {metadata.PageSize}"
						);
						Console.WriteLine(
							$"Lookup: {args[1]}"
						);
						Console.WriteLine(
							$"Hops: {hop}"
						);
						Console.WriteLine(
							$"Compiled bytes: {compiled.Length}"
						);
						Console.WriteLine(
							$"Data records: {dataRecords}"
						);
						Console.WriteLine(
							$"Index records: {indexRecords}"
						);
						return 0;

					case NcursesIndexRecord:
						key = value[1..];
						break;

					default:
						throw new InvalidDataException(
							$"Unexpected ncurses hashed-term marker {value[0]}."
						);
				}
			}

			throw new InvalidDataException(
				"Lookup exceeded the bounded ncurses index chain."
			);
		} catch ( Exception exception )
			when (
				exception is IOException
				|| exception is UnauthorizedAccessException
				|| exception is InvalidDataException
				|| exception is ArgumentException
			) {
			Console.Error.WriteLine( exception.Message );
			return 1;
		}
	}

	private static DatabaseMetadata ReadMetadata( byte[] database ) {
		ArgumentNullException.ThrowIfNull( database );

		if ( database.Length < 512 ) {
			throw new InvalidDataException(
				"The Berkeley DB file is too small to contain a metadata page."
			);
		}

		ReadOnlySpan<byte> bytes = database;
		uint littleMagic =
			BinaryPrimitives.ReadUInt32LittleEndian( bytes[12..16] );
		uint bigMagic =
			BinaryPrimitives.ReadUInt32BigEndian( bytes[12..16] );

		bool bigEndian;
		if ( littleMagic == HashMagic ) {
			bigEndian = false;
		} else if ( bigMagic == HashMagic ) {
			bigEndian = true;
		} else {
			throw new InvalidDataException(
				"The file is not a supported Berkeley DB Hash database."
			);
		}

		uint version =
			ReadUInt32( bytes, 16, bigEndian );
		if ( version != SupportedHashVersion ) {
			throw new InvalidDataException(
				$"Berkeley DB Hash version {version} is not supported by the HDB00 managed probe."
			);
		}

		uint pageSizeValue =
			ReadUInt32( bytes, 20, bigEndian );
		if (
			pageSizeValue < 512
			|| pageSizeValue > 64 * 1024
			|| ( pageSizeValue & ( pageSizeValue - 1 ) ) != 0
		) {
			throw new InvalidDataException(
				$"Invalid Berkeley DB page size {pageSizeValue}."
			);
		}

		if ( pageSizeValue > int.MaxValue ) {
			throw new InvalidDataException(
				"The Berkeley DB page size exceeds managed limits."
			);
		}

		int pageSize =
			(int)pageSizeValue;
		if ( database.Length % pageSize != 0 ) {
			throw new InvalidDataException(
				"The Berkeley DB file length is not a whole number of pages."
			);
		}

		if ( bytes[24] != 0 ) {
			throw new InvalidDataException(
				"Encrypted Berkeley DB files are outside the HDB00 managed probe."
			);
		}

		if ( bytes[25] != HashMetadataPage ) {
			throw new InvalidDataException(
				$"Unexpected Berkeley DB metadata page type {bytes[25]}."
			);
		}

		if ( ( bytes[26] & 0x01 ) != 0 ) {
			throw new InvalidDataException(
				"Checksummed Berkeley DB pages are outside the HDB00 managed probe."
			);
		}

		uint lastPageNumber =
			ReadUInt32( bytes, 32, bigEndian );
		if ( lastPageNumber >= database.Length / pageSize ) {
			throw new InvalidDataException(
				"The Berkeley DB metadata references a page beyond the file."
			);
		}

		return new DatabaseMetadata(
			bigEndian,
			version,
			pageSize,
			lastPageNumber
		);
	}

	private static bool TryFindValue(
		byte[] database,
		DatabaseMetadata metadata,
		ReadOnlySpan<byte> requestedKey,
		out byte[] value
	) {
		for (
			uint pageNumber = 1;
			pageNumber <= metadata.LastPageNumber;
			pageNumber++
		) {
			ReadOnlySpan<byte> page =
				GetPage( database, metadata, pageNumber );

			if ( page[25] != HashPage ) {
				continue;
			}

			ushort entryCount =
				GetHashEntryCount(
					page,
					metadata,
					pageNumber
				);

			for ( int index = 0; index < entryCount; index += 2 ) {
				byte[] key =
					ReadHashItem(
						database,
						metadata,
						page,
						index
					);

				if ( !requestedKey.SequenceEqual( key ) ) {
					continue;
				}

				value =
					ReadHashItem(
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

	private static ( int DataRecords, int IndexRecords ) CountNcursesRecords(
		byte[] database,
		DatabaseMetadata metadata
	) {
		int dataRecords = 0;
		int indexRecords = 0;

		for (
			uint pageNumber = 1;
			pageNumber <= metadata.LastPageNumber;
			pageNumber++
		) {
			ReadOnlySpan<byte> page =
				GetPage( database, metadata, pageNumber );

			if ( page[25] != HashPage ) {
				continue;
			}

			ushort entryCount =
				GetHashEntryCount(
					page,
					metadata,
					pageNumber
				);

			for ( int index = 1; index < entryCount; index += 2 ) {
				byte[] record =
					ReadHashItem(
						database,
						metadata,
						page,
						index
					);

				if ( record.Length == 0 ) {
					throw new InvalidDataException(
						$"Hash page {pageNumber} contains an empty value."
					);
				}

				switch ( record[0] ) {
					case NcursesDataRecord:
						dataRecords++;
						break;

					case NcursesIndexRecord:
						indexRecords++;
						break;

					default:
						throw new InvalidDataException(
							$"Hash page {pageNumber} contains unknown ncurses record marker {record[0]}."
						);
				}
			}
		}

		return ( dataRecords, indexRecords );
	}

	private static ushort GetHashEntryCount(
		ReadOnlySpan<byte> page,
		DatabaseMetadata metadata,
		uint pageNumber
	) {
		ushort entryCount =
			ReadUInt16(
				page,
				20,
				metadata.BigEndian
			);

		if ( ( entryCount & 1 ) != 0 ) {
			throw new InvalidDataException(
				$"Hash page {pageNumber} has an odd item count {entryCount}."
			);
		}

		ValidateIndexTable(
			page,
			entryCount,
			metadata
		);
		return entryCount;
	}

	private static void ValidateIndexTable(
		ReadOnlySpan<byte> page,
		ushort entryCount,
		DatabaseMetadata metadata
	) {
		int tableEnd =
			PageHeaderSize + ( entryCount * sizeof( ushort ) );
		if ( tableEnd > metadata.PageSize ) {
			throw new InvalidDataException(
				"The Berkeley DB hash-page index table exceeds its page."
			);
		}

		ushort freeOffset =
			ReadUInt16(
				page,
				22,
				metadata.BigEndian
			);
		if (
			freeOffset < tableEnd
			|| freeOffset > metadata.PageSize
		) {
			throw new InvalidDataException(
				$"Invalid Berkeley DB hash-page free offset {freeOffset}."
			);
		}

		int previousOffset = metadata.PageSize;
		for ( int index = 0; index < entryCount; index++ ) {
			ushort offset =
				ReadUInt16(
					page,
					PageHeaderSize + ( index * sizeof( ushort ) ),
					metadata.BigEndian
				);

			if (
				offset < freeOffset
				|| offset >= previousOffset
				|| offset >= metadata.PageSize
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
		ushort offset =
			ReadUInt16(
				page,
				PageHeaderSize + ( index * sizeof( ushort ) ),
				metadata.BigEndian
			);
		int upperOffset =
			( index == 0 )
				? metadata.PageSize
				: ReadUInt16(
					page,
					PageHeaderSize + ( ( index - 1 ) * sizeof( ushort ) ),
					metadata.BigEndian
				)
		;

		int itemLength = upperOffset - offset;
		if ( itemLength <= 0 ) {
			throw new InvalidDataException(
				$"Invalid Berkeley DB hash item length {itemLength}."
			);
		}

		byte itemType = page[offset];
		switch ( itemType ) {
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

				uint overflowPage =
					ReadUInt32(
						page,
						offset + 4,
						metadata.BigEndian
					);
				uint totalLength =
					ReadUInt32(
						page,
						offset + 8,
						metadata.BigEndian
					);
				return ReadOverflow(
					database,
					metadata,
					overflowPage,
					totalLength
				);

			default:
				throw new InvalidDataException(
					$"Berkeley DB hash item type {itemType} is outside the HDB00 managed subset."
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

		byte[] result =
			new byte[(int)totalLength];
		int written = 0;
		uint pageNumber = firstPage;
		HashSet<uint> visited = [];

		while ( pageNumber != 0 ) {
			if ( !visited.Add( pageNumber ) ) {
				throw new InvalidDataException(
					"The Berkeley DB overflow chain contains a cycle."
				);
			}

			ReadOnlySpan<byte> page =
				GetPage(
					database,
					metadata,
					pageNumber
				);
			if ( page[25] != OverflowPage ) {
				throw new InvalidDataException(
					$"Expected overflow page {pageNumber}, found page type {page[25]}."
				);
			}

			ushort chunkLength =
				ReadUInt16(
					page,
					22,
					metadata.BigEndian
				);
			if (
				chunkLength > metadata.PageSize - PageHeaderSize
				|| written > result.Length - chunkLength
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
			pageNumber =
				ReadUInt32(
					page,
					16,
					metadata.BigEndian
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

		long offset =
			(long)pageNumber * metadata.PageSize;
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
		bool bigEndian
	) {
		ReadOnlySpan<byte> value =
			bytes.Slice(
				offset,
				sizeof( ushort )
			);
		return ( bigEndian )
			? BinaryPrimitives.ReadUInt16BigEndian( value )
			: BinaryPrimitives.ReadUInt16LittleEndian( value )
		;
	}

	private static uint ReadUInt32(
		ReadOnlySpan<byte> bytes,
		int offset,
		bool bigEndian
	) {
		ReadOnlySpan<byte> value =
			bytes.Slice(
				offset,
				sizeof( uint )
			);
		return ( bigEndian )
			? BinaryPrimitives.ReadUInt32BigEndian( value )
			: BinaryPrimitives.ReadUInt32LittleEndian( value )
		;
	}

	private readonly record struct DatabaseMetadata(
		bool BigEndian,
		uint Version,
		int PageSize,
		uint LastPageNumber
	);
}
