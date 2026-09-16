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

internal static class TerminalNameValidator {
	internal static void Validate( string name ) {
		ArgumentNullException.ThrowIfNull( name );

		if ( string.IsNullOrWhiteSpace( name ) ) {
			throw new ArgumentException(
				"The terminal name cannot be empty or whitespace.",
				nameof( name )
			);
		}

		if (
			string.Equals(
				name,
				".",
				StringComparison.Ordinal
			)
			|| string.Equals(
				name,
				"..",
				StringComparison.Ordinal
			)
		) {
			throw new ArgumentException(
				"The terminal name must be an exact database key.",
				nameof( name )
			);
		}

		foreach ( char character in name ) {
			if (
				character == '\0'
				|| character == '/'
				|| character == '\\'
				|| char.IsControl( character )
				|| char.IsSurrogate( character )
			) {
				throw new ArgumentException(
					"The terminal name contains unsafe key syntax.",
					nameof( name )
				);
			}
		}
	}
}
