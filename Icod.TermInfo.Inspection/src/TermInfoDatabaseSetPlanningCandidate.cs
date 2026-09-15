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
/// Maps one frozen planner candidate position back to its original ordered
/// database-set publication.
/// </summary>
public sealed class TermInfoDatabaseSetPlanningCandidate {
	internal TermInfoDatabaseSetPlanningCandidate(
		int candidateIndex,
		TermInfoDatabaseSetEntry database,
		TermInfoDatabaseSetOccurrence occurrence,
		TerminalDescriptionSourceSynthesisParent parent
	) {
		if ( candidateIndex < 0 ) {
			throw new ArgumentOutOfRangeException( nameof( candidateIndex ) );
		}
		ArgumentNullException.ThrowIfNull( database );
		ArgumentNullException.ThrowIfNull( occurrence );
		ArgumentNullException.ThrowIfNull( parent );
		if ( database.Index != occurrence.DatabaseIndex ) {
			throw new ArgumentException(
				"The candidate database must contain the mapped occurrence.",
				nameof( database )
			);
		}
		if ( !string.Equals(
			occurrence.Name,
			parent.UseName,
			StringComparison.Ordinal
		) ) {
			throw new ArgumentException(
				"Database-set planning candidates must emit the canonical occurrence name.",
				nameof( parent )
			);
		}
		if ( !ReferenceEquals( occurrence.Entry.Terminal, parent.Description ) ) {
			throw new ArgumentException(
				"The planning parent must preserve the exact occurrence description.",
				nameof( parent )
			);
		}

		CandidateIndex = candidateIndex;
		Database = database;
		Occurrence = occurrence;
		Parent = parent;
	}

	/// <summary>
	/// Gets the zero-based position supplied to the frozen 1.8 planner.
	/// </summary>
	public int CandidateIndex {
		get;
	}

	/// <summary>
	/// Gets the original constituent database evidence.
	/// </summary>
	public TermInfoDatabaseSetEntry Database {
		get;
	}

	/// <summary>
	/// Gets the exact original physical occurrence used as the candidate
	/// representative.
	/// </summary>
	public TermInfoDatabaseSetOccurrence Occurrence {
		get;
	}

	/// <summary>
	/// Gets the exact frozen synthesis-parent object supplied to the planner.
	/// </summary>
	public TerminalDescriptionSourceSynthesisParent Parent {
		get;
	}

	/// <summary>
	/// Gets the caller-order database index.
	/// </summary>
	public int DatabaseIndex =>
		Occurrence.DatabaseIndex;

	/// <summary>
	/// Gets the constituent catalog-entry index.
	/// </summary>
	public int CatalogEntryIndex =>
		Occurrence.CatalogEntryIndex;

	/// <summary>
	/// Gets the canonical terminal name of the represented publication.
	/// </summary>
	public string CanonicalName =>
		Occurrence.Name;

	/// <summary>
	/// Gets the exact <c>use=</c> spelling supplied to the frozen planner.
	/// </summary>
	public string UseName =>
		Parent.UseName;

	/// <summary>
	/// Gets the exact effective terminal semantics supplied to the frozen planner.
	/// </summary>
	public TerminalDescription Description =>
		Parent.Description;
}
