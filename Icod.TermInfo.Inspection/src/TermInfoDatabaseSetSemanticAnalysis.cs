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
/// Contains deterministic semantic analysis of repeated canonical identities and
/// alias collisions in one ordered database set.
/// </summary>
public sealed class TermInfoDatabaseSetSemanticAnalysis {
	internal TermInfoDatabaseSetSemanticAnalysis(
		IEnumerable<TermInfoDatabaseSetIdentityAnalysis> repeatedIdentities,
		IEnumerable<TermInfoDatabaseSetAliasAnalysis> aliases,
		int semanticComparisonCount,
		int aliasOccurrenceCount,
		bool isComplete
	) {
		ArgumentNullException.ThrowIfNull( repeatedIdentities );
		ArgumentNullException.ThrowIfNull( aliases );
		if ( semanticComparisonCount < 0 ) {
			throw new ArgumentOutOfRangeException( nameof( semanticComparisonCount ) );
		}
		if ( aliasOccurrenceCount < 0 ) {
			throw new ArgumentOutOfRangeException( nameof( aliasOccurrenceCount ) );
		}

		TermInfoDatabaseSetIdentityAnalysis[] identityArray =
			repeatedIdentities.ToArray();
		TermInfoDatabaseSetAliasAnalysis[] aliasArray = aliases.ToArray();
		if ( identityArray.Any( identity => identity is null ) ) {
			throw new ArgumentException(
				"Repeated identity analyses cannot contain null.",
				nameof( repeatedIdentities )
			);
		}
		if ( aliasArray.Any( alias => alias is null ) ) {
			throw new ArgumentException(
				"Alias analyses cannot contain null.",
				nameof( aliases )
			);
		}

		RepeatedIdentities = Array.AsReadOnly( identityArray );
		Aliases = Array.AsReadOnly( aliasArray );
		SemanticComparisonCount = semanticComparisonCount;
		AliasOccurrenceCount = aliasOccurrenceCount;
		IsComplete = isComplete;
	}

	/// <summary>
	/// Gets repeated canonical identities in ordinal canonical-name order.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetIdentityAnalysis> RepeatedIdentities {
		get;
	}

	/// <summary>
	/// Gets repeated or canonical-name-colliding aliases in ordinal alias order.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetAliasAnalysis> Aliases {
		get;
	}

	/// <summary>
	/// Gets the number of winner-versus-shadow calls made to the frozen semantic
	/// comparer.
	/// </summary>
	public int SemanticComparisonCount {
		get;
	}

	/// <summary>
	/// Gets the number of alias declarations scanned while constructing the alias
	/// collision index.
	/// </summary>
	public int AliasOccurrenceCount {
		get;
	}

	/// <summary>
	/// Gets whether every constituent database was inspected completely.
	/// </summary>
	public bool IsComplete {
		get;
	}

	/// <summary>
	/// Gets whether any repeated identity or alias collision is definitively
	/// semantically different.
	/// </summary>
	public bool HasSemanticDifferences =>
		RepeatedIdentities.Any(
			identity => identity.Relationship
				== TermInfoDatabaseSetSemanticRelationship.SemanticallyDifferent
		)
		|| Aliases.Any(
			alias => alias.Relationship
				== TermInfoDatabaseSetSemanticRelationship.SemanticallyDifferent
		);

	/// <summary>
	/// Gets whether incomplete input leaves any analysis conclusion indeterminate.
	/// </summary>
	public bool HasIndeterminateEvidence =>
		!IsComplete
		|| RepeatedIdentities.Any(
			identity => identity.Relationship
				== TermInfoDatabaseSetSemanticRelationship.Indeterminate
		)
		|| Aliases.Any(
			alias => alias.Relationship
				== TermInfoDatabaseSetSemanticRelationship.Indeterminate
		);
}
