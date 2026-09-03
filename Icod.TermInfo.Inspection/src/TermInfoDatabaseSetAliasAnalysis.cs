namespace Icod.TermInfo.Inspection;

/// <summary>
/// Describes one repeated or canonical-name-colliding alias across an ordered
/// database set.
/// </summary>
public sealed class TermInfoDatabaseSetAliasAnalysis {
	internal TermInfoDatabaseSetAliasAnalysis(
		string alias,
		IEnumerable<TermInfoDatabaseSetOccurrence> occurrences,
		IEnumerable<string> canonicalNames,
		TermInfoDatabaseSetOccurrence? precedenceOwner,
		TermInfoDatabaseSetIdentity? matchingCanonicalIdentity,
		TermInfoDatabaseSetSemanticRelationship relationship,
		bool isComplete,
		IEnumerable<int> blockingDatabaseIndices
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( alias );
		ArgumentNullException.ThrowIfNull( occurrences );
		ArgumentNullException.ThrowIfNull( canonicalNames );
		ArgumentNullException.ThrowIfNull( blockingDatabaseIndices );

		TermInfoDatabaseSetOccurrence[] occurrenceArray = occurrences.ToArray();
		string[] canonicalNameArray = canonicalNames.ToArray();
		int[] blockingArray = blockingDatabaseIndices.ToArray();
		if ( occurrenceArray.Length == 0
			|| occurrenceArray.Any( occurrence => occurrence is null ) ) {
			throw new ArgumentException(
				"Alias analysis requires at least one non-null physical occurrence.",
				nameof( occurrences )
			);
		}
		if ( canonicalNameArray.Length == 0
			|| canonicalNameArray.Any( string.IsNullOrWhiteSpace )
			|| !canonicalNameArray.SequenceEqual(
				canonicalNameArray.OrderBy( name => name, StringComparer.Ordinal ),
				StringComparer.Ordinal
			) ) {
			throw new ArgumentException(
				"Canonical alias owners must be non-empty and ordinally ordered.",
				nameof( canonicalNames )
			);
		}
		if ( precedenceOwner is not null
			&& !ReferenceEquals( precedenceOwner, occurrenceArray[ 0 ] ) ) {
			throw new ArgumentException(
				"Alias precedence ownership must select the first observed alias occurrence.",
				nameof( precedenceOwner )
			);
		}
		if ( matchingCanonicalIdentity is not null
			&& !string.Equals(
				matchingCanonicalIdentity.Name,
				alias,
				StringComparison.Ordinal
			) ) {
			throw new ArgumentException(
				"The matching canonical identity must exactly equal the alias.",
				nameof( matchingCanonicalIdentity )
			);
		}
		if ( blockingArray.Any( index => index < 0 )
			|| blockingArray.Distinct().Count() != blockingArray.Length ) {
			throw new ArgumentException(
				"Alias blocking database indices must be unique and non-negative.",
				nameof( blockingDatabaseIndices )
			);
		}
		if ( relationship == TermInfoDatabaseSetSemanticRelationship.SemanticallyEqual
			&& !isComplete ) {
			throw new ArgumentException(
				"Semantically equal alias evidence requires complete database-set evidence.",
				nameof( relationship )
			);
		}
		if ( relationship == TermInfoDatabaseSetSemanticRelationship.Indeterminate
			&& isComplete ) {
			throw new ArgumentException(
				"Indeterminate alias evidence requires incomplete database-set evidence.",
				nameof( relationship )
			);
		}

		Alias = alias;
		Occurrences = Array.AsReadOnly( occurrenceArray );
		CanonicalNames = Array.AsReadOnly( canonicalNameArray );
		PrecedenceOwner = precedenceOwner;
		MatchingCanonicalIdentity = matchingCanonicalIdentity;
		Relationship = relationship;
		IsComplete = isComplete;
		BlockingDatabaseIndices = Array.AsReadOnly( blockingArray );
	}

	/// <summary>
	/// Gets the exact ordinal alias string.
	/// </summary>
	public string Alias {
		get;
	}

	/// <summary>
	/// Gets physical declarations in database and catalog-entry order.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetOccurrence> Occurrences {
		get;
	}

	/// <summary>
	/// Gets distinct canonical owner names in ordinal order.
	/// </summary>
	public IReadOnlyList<string> CanonicalNames {
		get;
	}

	/// <summary>
	/// Gets the first declared alias owner when earlier incomplete databases do not
	/// block precedence evidence.
	/// </summary>
	public TermInfoDatabaseSetOccurrence? PrecedenceOwner {
		get;
	}

	/// <summary>
	/// Gets the canonical database-set identity whose name exactly matches this
	/// alias, when such an identity is observed.
	/// </summary>
	public TermInfoDatabaseSetIdentity? MatchingCanonicalIdentity {
		get;
	}

	/// <summary>
	/// Gets the aggregate semantic or ownership relationship.
	/// </summary>
	public TermInfoDatabaseSetSemanticRelationship Relationship {
		get;
	}

	/// <summary>
	/// Gets whether all constituent databases were inspected completely.
	/// </summary>
	public bool IsComplete {
		get;
	}

	/// <summary>
	/// Gets incomplete database indices which prevent a conclusive precedence owner.
	/// </summary>
	public IReadOnlyList<int> BlockingDatabaseIndices {
		get;
	}

	/// <summary>
	/// Gets whether more than one canonical name declares the alias.
	/// </summary>
	public bool HasMultipleCanonicalOwners =>
		CanonicalNames.Count > 1;

	/// <summary>
	/// Gets whether the alias also exactly names an observed canonical identity.
	/// </summary>
	public bool MatchesCanonicalName =>
		MatchingCanonicalIdentity is not null;
}
