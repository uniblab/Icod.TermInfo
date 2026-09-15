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
/// Locates one frozen catalog issue within an ordered terminfo database set.
/// </summary>
public sealed class TermInfoDatabaseSetIssue {
	internal TermInfoDatabaseSetIssue(
		int databaseIndex,
		int catalogIssueIndex,
		TermInfoDatabaseCatalogIssue issue
	) {
		if ( databaseIndex < 0 ) {
			throw new ArgumentOutOfRangeException( nameof( databaseIndex ) );
		}
		if ( catalogIssueIndex < 0 ) {
			throw new ArgumentOutOfRangeException( nameof( catalogIssueIndex ) );
		}
		ArgumentNullException.ThrowIfNull( issue );

		DatabaseIndex = databaseIndex;
		CatalogIssueIndex = catalogIssueIndex;
		Issue = issue;
	}

	/// <summary>
	/// Gets the zero-based caller-order database index.
	/// </summary>
	public int DatabaseIndex {
		get;
	}

	/// <summary>
	/// Gets the zero-based issue index within the frozen constituent catalog.
	/// </summary>
	public int CatalogIssueIndex {
		get;
	}

	/// <summary>
	/// Gets the original frozen catalog issue.
	/// </summary>
	public TermInfoDatabaseCatalogIssue Issue {
		get;
	}
}
