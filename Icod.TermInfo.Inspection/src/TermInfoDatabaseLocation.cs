/*
	Icod.TermInfo.Inspection
	Provides terminfo inspection, comparison, planning, and machine-readable automation.
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

namespace Icod.TermInfo.Inspection;

/// <summary>
/// Describes one immutable location in the ordered system terminfo discovery
/// snapshot.
/// </summary>
public sealed class TermInfoDatabaseLocation {
	internal TermInfoDatabaseLocation(
		TermInfoDatabaseLocationKind kind,
		string? path
	) {
		if ( kind == TermInfoDatabaseLocationKind.EncodedTermInfo ) {
			if ( path is not null ) {
				throw new ArgumentException(
					"An encoded TERMINFO location cannot expose a filesystem path.",
					nameof( path )
				);
			}
		} else {
			ArgumentException.ThrowIfNullOrWhiteSpace( path );

			if ( !System.IO.Path.IsPathFullyQualified( path ) ) {
				throw new ArgumentException(
					"A terminfo database directory path must be fully qualified.",
					nameof( path )
				);
			}
		}

		Kind = kind;
		Path = path;
	}

	/// <summary>
	/// Gets the discovery-source kind.
	/// </summary>
	public TermInfoDatabaseLocationKind Kind {
		get;
	}

	/// <summary>
	/// Gets the normalized absolute directory path, or <see langword="null"/>
	/// when <see cref="Kind"/> is <see cref="TermInfoDatabaseLocationKind.EncodedTermInfo"/>.
	/// </summary>
	/// <remarks>
	/// Encoded <c>TERMINFO</c> bytes are intentionally not exposed by inspection.
	/// </remarks>
	public string? Path {
		get;
	}
}
