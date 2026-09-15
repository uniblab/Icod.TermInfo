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
/// Represents one canonical terminal identity observed across an ordered
/// database set.
/// </summary>
public sealed class TermInfoDatabaseSetIdentity {
	internal TermInfoDatabaseSetIdentity(
		string name,
		IEnumerable<TermInfoDatabaseSetOccurrence> occurrences
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( name );
		ArgumentNullException.ThrowIfNull( occurrences );

		TermInfoDatabaseSetOccurrence[] occurrenceArray =
			occurrences.ToArray();
		if ( occurrenceArray.Length == 0 ) {
			throw new ArgumentException(
				"A database-set identity must contain at least one occurrence.",
				nameof( occurrences )
			);
		}
		if ( occurrenceArray.Any( occurrence => occurrence is null ) ) {
			throw new ArgumentException(
				"A database-set identity occurrence collection cannot contain null.",
				nameof( occurrences )
			);
		}
		if (
			occurrenceArray.Any(
				occurrence => !string.Equals(
					occurrence.Name,
					name,
					StringComparison.Ordinal
				)
			)
		) {
			throw new ArgumentException(
				"Every occurrence must declare the database-set canonical identity.",
				nameof( occurrences )
			);
		}

		Name = name;
		Occurrences = Array.AsReadOnly( occurrenceArray );
	}

	/// <summary>
	/// Gets the canonical terminal name, compared and ordered ordinally.
	/// </summary>
	public string Name {
		get;
	}

	/// <summary>
	/// Gets physical occurrences in database order and then constituent catalog
	/// entry order.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetOccurrence> Occurrences {
		get;
	}
}
