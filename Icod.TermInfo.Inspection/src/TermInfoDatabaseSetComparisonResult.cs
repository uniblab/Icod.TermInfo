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
/// Contains the stable deterministic comparison of two ordered terminfo database
/// sets.
/// </summary>
public sealed class TermInfoDatabaseSetComparisonResult {
	internal TermInfoDatabaseSetComparisonResult(
		IEnumerable<TermInfoDatabaseSetDifference> differences,
		int semanticComparisonCount,
		int aliasOccurrenceCount
	) {
		ArgumentNullException.ThrowIfNull( differences );
		if ( semanticComparisonCount < 0 ) {
			throw new ArgumentOutOfRangeException( nameof( semanticComparisonCount ) );
		}
		if ( aliasOccurrenceCount < 0 ) {
			throw new ArgumentOutOfRangeException( nameof( aliasOccurrenceCount ) );
		}

		TermInfoDatabaseSetDifference[] differenceArray = differences.ToArray();
		if ( differenceArray.Any( difference => difference is null ) ) {
			throw new ArgumentException(
				"Database-set differences cannot contain null.",
				nameof( differences )
			);
		}

		Differences = Array.AsReadOnly( differenceArray );
		SemanticComparisonCount = semanticComparisonCount;
		AliasOccurrenceCount = aliasOccurrenceCount;
	}

	/// <summary>
	/// Gets differences in the frozen DA04 kind/name/provenance order.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetDifference> Differences {
		get;
	}

	/// <summary>
	/// Gets the number of cross-set calls made to
	/// <see cref="TerminalDescriptionComparer"/>.
	/// </summary>
	public int SemanticComparisonCount {
		get;
	}

	/// <summary>
	/// Gets the total number of alias declarations scanned across both sets.
	/// </summary>
	public int AliasOccurrenceCount {
		get;
	}

	/// <summary>
	/// Gets whether incomplete evidence leaves any whole-set conclusion
	/// indeterminate.
	/// </summary>
	public bool IsConclusive =>
		!Differences.Any(
			difference => difference.Kind == TermInfoDatabaseSetDifferenceKind.Indeterminate
		);

	/// <summary>
	/// Gets whether a conclusive comparison found no effective identity, winner, or
	/// alias-ownership difference.
	/// </summary>
	public bool AreEffectivelyEquivalent =>
		IsConclusive
		&& !Differences.Any(
			difference =>
				difference.Kind == TermInfoDatabaseSetDifferenceKind.OnlyInLeft
				|| difference.Kind == TermInfoDatabaseSetDifferenceKind.OnlyInRight
				|| difference.Kind == TermInfoDatabaseSetDifferenceKind.EffectiveSemantic
				|| difference.Kind == TermInfoDatabaseSetDifferenceKind.AliasOwnership
		);

	/// <summary>
	/// Gets whether a conclusive comparison found no topology, provenance, issue,
	/// alias, shadow, or membership difference.
	/// </summary>
	public bool AreStructurallyEquivalent =>
		IsConclusive
		&& !Differences.Any(
			difference =>
				difference.Kind != TermInfoDatabaseSetDifferenceKind.EffectiveSemantic
				&& difference.Kind != TermInfoDatabaseSetDifferenceKind.Indeterminate
		);

	/// <summary>
	/// Gets whether the database sets are conclusively equivalent in both effective
	/// semantics and structure/provenance.
	/// </summary>
	public bool AreEquivalent =>
		IsConclusive && Differences.Count == 0;
}
