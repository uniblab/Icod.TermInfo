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

namespace Icod.TermInfo.BerkeleyDb;

internal static class NcursesRecordReader {
	internal static bool TryReadCompiledEntry(
		string databasePath,
		ReadOnlySpan<byte> requestedKey,
		out byte[] compiledEntry,
		int maximumDatabaseSize = 64 * 1024 * 1024,
		int maximumItemSize = 1024 * 1024,
		int maximumIndexHops = 16
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( databasePath );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumDatabaseSize );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maximumItemSize );
		ArgumentOutOfRangeException.ThrowIfNegative( maximumIndexHops );

		byte[] database = BerkeleyDbHashReader.ReadDatabase(
			databasePath,
			maximumDatabaseSize
		);
		ReadOnlySpan<byte> key = requestedKey;
		HashSet<string> visited = new HashSet<string>( StringComparer.Ordinal );
		int followedLinks = 0;

		while ( true ) {
			if ( !BerkeleyDbHashReader.TryReadValue(
				database,
				key,
				out byte[] value,
				maximumItemSize
			)
			) {
				if ( followedLinks == 0 ) {
					compiledEntry = [];
					return false;
				}
				throw new InvalidDataException(
					"The ncurses index record references a missing key."
				);
			}

			// A found key has passed the stored-item size bound. Encode only then,
			// so an arbitrarily long absent requested key causes no tracking copy.
			if ( !visited.Add( Convert.ToHexString( key ) ) ) {
				throw new InvalidDataException(
					"The ncurses index chain contains a cycle."
				);
			}
			if ( value.Length == 0 ) {
				throw new InvalidDataException(
					"The ncurses hashed-term record is empty."
				);
			}

			if ( value[0] == 0 ) {
				compiledEntry = value[1..];
				return true;
			}
			if ( value[0] != 2 ) {
				throw new InvalidDataException(
					$"Ncurses hashed-term marker {value[0]} is not supported."
				);
			}
			if ( value.Length == 1 ) {
				throw new InvalidDataException(
					"The ncurses index record has an empty target."
				);
			}
			if ( followedLinks >= maximumIndexHops ) {
				throw new InvalidDataException(
					"The ncurses index chain exceeds the configured hop limit."
				);
			}

			followedLinks++;
			key = value.AsSpan( 1 );
		}
	}
}
