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
/// Represents one successfully parsed physical file in a conventional terminfo
/// database root.
/// </summary>
public sealed class TermInfoDatabaseCatalogEntry {
	internal TermInfoDatabaseCatalogEntry(
		string path,
		TerminalDescription terminal
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( path );
		ArgumentNullException.ThrowIfNull( terminal );

		if ( !System.IO.Path.IsPathFullyQualified( path ) ) {
			throw new ArgumentException(
				"A catalog entry path must be fully qualified.",
				nameof( path )
			);
		}

		Path = path;
		Terminal = terminal;
	}

	/// <summary>
	/// Gets the normalized absolute path of the compiled entry file.
	/// </summary>
	public string Path {
		get;
	}

	/// <summary>
	/// Gets the parsed immutable terminal description.
	/// </summary>
	public TerminalDescription Terminal {
		get;
	}

	/// <summary>
	/// Gets the canonical terminal name represented by the compiled entry.
	/// </summary>
	public string Name =>
		Terminal.Name;

	/// <summary>
	/// Gets the aliases represented by the compiled entry.
	/// </summary>
	public IReadOnlyList<string> Aliases =>
		Terminal.Aliases;

	/// <summary>
	/// Gets the terminal description, when present.
	/// </summary>
	public string? Description =>
		Terminal.Description;
}
