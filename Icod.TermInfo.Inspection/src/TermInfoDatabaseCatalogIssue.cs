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
/// Describes one deterministic issue encountered while inspecting a
/// conventional terminfo database root.
/// </summary>
public sealed class TermInfoDatabaseCatalogIssue {
	internal TermInfoDatabaseCatalogIssue(
		TermInfoDatabaseCatalogIssueKind kind,
		string path,
		string message
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( path );
		ArgumentException.ThrowIfNullOrWhiteSpace( message );

		if ( !System.IO.Path.IsPathFullyQualified( path ) ) {
			throw new ArgumentException(
				"A catalog issue path must be fully qualified.",
				nameof( path )
			);
		}

		Kind = kind;
		Path = path;
		Message = message;
	}

	/// <summary>
	/// Gets the issue category.
	/// </summary>
	public TermInfoDatabaseCatalogIssueKind Kind {
		get;
	}

	/// <summary>
	/// Gets the normalized absolute filesystem path associated with the issue.
	/// </summary>
	public string Path {
		get;
	}

	/// <summary>
	/// Gets the deterministic issue description.
	/// </summary>
	public string Message {
		get;
	}
}
