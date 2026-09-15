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
/// Represents one constituent catalog in an immutable ordered terminfo database
/// set.
/// </summary>
public sealed class TermInfoDatabaseSetEntry {
	internal TermInfoDatabaseSetEntry(
		int index,
		TermInfoDatabaseCatalog catalog
	) {
		if ( index < 0 ) {
			throw new ArgumentOutOfRangeException( nameof( index ) );
		}
		ArgumentNullException.ThrowIfNull( catalog );

		Index = index;
		Catalog = catalog;
	}

	/// <summary>
	/// Gets the zero-based caller-order index of this database.
	/// </summary>
	public int Index {
		get;
	}

	/// <summary>
	/// Gets the frozen 1.9 catalog snapshot without reinterpretation.
	/// </summary>
	public TermInfoDatabaseCatalog Catalog {
		get;
	}

	/// <summary>
	/// Gets whether this constituent is a conventional directory with no catalog
	/// inspection issues.
	/// </summary>
	public bool IsComplete =>
		Catalog.Kind == TermInfoDatabaseCatalogKind.ConventionalDirectory
		&& !Catalog.HasIssues;
}
