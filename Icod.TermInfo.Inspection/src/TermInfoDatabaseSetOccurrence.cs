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
/// Identifies one physical catalog occurrence of a canonical terminal identity
/// in an ordered database set.
/// </summary>
public sealed class TermInfoDatabaseSetOccurrence {
	internal TermInfoDatabaseSetOccurrence(
		int databaseIndex,
		int catalogEntryIndex,
		TermInfoDatabaseCatalogEntry entry
	) {
		if ( databaseIndex < 0 ) {
			throw new ArgumentOutOfRangeException( nameof( databaseIndex ) );
		}
		if ( catalogEntryIndex < 0 ) {
			throw new ArgumentOutOfRangeException( nameof( catalogEntryIndex ) );
		}
		ArgumentNullException.ThrowIfNull( entry );

		DatabaseIndex = databaseIndex;
		CatalogEntryIndex = catalogEntryIndex;
		Entry = entry;
	}

	/// <summary>
	/// Gets the zero-based caller-order database index.
	/// </summary>
	public int DatabaseIndex {
		get;
	}

	/// <summary>
	/// Gets the zero-based entry index within the frozen constituent catalog.
	/// </summary>
	public int CatalogEntryIndex {
		get;
	}

	/// <summary>
	/// Gets the original immutable catalog entry.
	/// </summary>
	public TermInfoDatabaseCatalogEntry Entry {
		get;
	}

	/// <summary>
	/// Gets the canonical terminal identity declared by the occurrence.
	/// </summary>
	public string Name =>
		Entry.Name;

	/// <summary>
	/// Gets the aliases declared by this physical occurrence. Aliases remain
	/// occurrence evidence and are not promoted to canonical database-set
	/// identities.
	/// </summary>
	public IReadOnlyList<string> Aliases =>
		Entry.Aliases;
}
