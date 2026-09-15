/*
	Icod.TermInfo
	Provides managed terminfo runtime parsing, discovery, capabilities, and terminal profiles.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

	You should have received a copy of the GNU Lesser General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

namespace Icod.TermInfo;

/// <summary>
/// Represents terminal dimensions in character cells.
/// </summary>
public readonly record struct TerminalSize {
	/// <summary>
	/// Initializes terminal dimensions.
	/// </summary>
	public TerminalSize(
		int columns,
		int rows
	) {
		if ( columns <= 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( columns ),
				columns,
				"The terminal column count must be positive."
			);
		}

		if ( rows <= 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( rows ),
				rows,
				"The terminal row count must be positive."
			);
		}

		Columns = columns;
		Rows = rows;
	}

	/// <summary>
	/// Gets the number of character columns.
	/// </summary>
	public int Columns { get; }

	/// <summary>
	/// Gets the number of character rows.
	/// </summary>
	public int Rows { get; }
}
