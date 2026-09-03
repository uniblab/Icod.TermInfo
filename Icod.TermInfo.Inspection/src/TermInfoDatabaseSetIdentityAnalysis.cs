namespace Icod.TermInfo.Inspection;

/// <summary>
/// Describes deterministic semantic evidence for one repeated canonical identity.
/// </summary>
public sealed class TermInfoDatabaseSetIdentityAnalysis {
	private readonly IReadOnlyList<TermInfoDatabaseSetShadowAnalysis> _equalShadows;
	private readonly IReadOnlyList<TermInfoDatabaseSetShadowAnalysis> _conflictingShadows;
	private readonly IReadOnlyList<TermInfoDatabaseSetShadowAnalysis> _indeterminateShadows;

	internal TermInfoDatabaseSetIdentityAnalysis(
		TermInfoDatabaseSetIdentity identity,
		TermInfoDatabaseSetLookupResult lookup,
		TermInfoDatabaseSetSemanticRelationship relationship,
		IEnumerable<TermInfoDatabaseSetShadowAnalysis> shadows
	) {
		ArgumentNullException.ThrowIfNull( identity );
		ArgumentNullException.ThrowIfNull( lookup );
		ArgumentNullException.ThrowIfNull( shadows );
		if ( identity.Occurrences.Count < 2 ) {
			throw new ArgumentException(
				"Identity semantic analysis requires a repeated canonical identity.",
				nameof( identity )
			);
		}
		if ( !string.Equals(
			identity.Name,
			lookup.Name,
			StringComparison.Ordinal
		) ) {
			throw new ArgumentException(
				"Lookup evidence must identify the analyzed canonical identity.",
				nameof( lookup )
			);
		}

		TermInfoDatabaseSetShadowAnalysis[] shadowArray = shadows.ToArray();
		if ( shadowArray.Any( shadow => shadow is null ) ) {
			throw new ArgumentException(
				"Identity shadow analysis cannot contain null.",
				nameof( shadows )
			);
		}

		TermInfoDatabaseSetShadowAnalysis[] equalShadows =
			shadowArray
				.Where(
					shadow => shadow.Relationship
						== TermInfoDatabaseSetSemanticRelationship.SemanticallyEqual
				)
				.ToArray();
		TermInfoDatabaseSetShadowAnalysis[] conflictingShadows =
			shadowArray
				.Where(
					shadow => shadow.Relationship
						== TermInfoDatabaseSetSemanticRelationship.SemanticallyDifferent
				)
				.ToArray();
		TermInfoDatabaseSetShadowAnalysis[] indeterminateShadows =
			shadowArray
				.Where(
					shadow => shadow.Relationship
						== TermInfoDatabaseSetSemanticRelationship.Indeterminate
				)
				.ToArray();

		if ( relationship == TermInfoDatabaseSetSemanticRelationship.SemanticallyEqual ) {
			if ( lookup.Status != TermInfoDatabaseSetLookupStatus.WinnerKnown
				|| !lookup.IsObservationComplete
				|| conflictingShadows.Length != 0
				|| indeterminateShadows.Length != 0 ) {
				throw new ArgumentException(
					"Semantically equal repeated identities require complete known-winner evidence.",
					nameof( relationship )
				);
			}
		} else if ( relationship == TermInfoDatabaseSetSemanticRelationship.SemanticallyDifferent ) {
			if ( conflictingShadows.Length == 0 ) {
				throw new ArgumentException(
					"A semantic difference requires at least one conflicting observed shadow.",
					nameof( relationship )
				);
			}
		} else if ( relationship == TermInfoDatabaseSetSemanticRelationship.Indeterminate ) {
			if ( lookup.Status != TermInfoDatabaseSetLookupStatus.Indeterminate
				&& lookup.IsObservationComplete ) {
				throw new ArgumentException(
					"Indeterminate identity evidence requires incomplete or indeterminate lookup evidence.",
					nameof( relationship )
				);
			}
		} else {
			throw new ArgumentOutOfRangeException( nameof( relationship ) );
		}

		Identity = identity;
		Lookup = lookup;
		Relationship = relationship;
		Shadows = Array.AsReadOnly( shadowArray );
		_equalShadows = Array.AsReadOnly( equalShadows );
		_conflictingShadows = Array.AsReadOnly( conflictingShadows );
		_indeterminateShadows = Array.AsReadOnly( indeterminateShadows );
	}

	/// <summary>
	/// Gets the repeated canonical identity.
	/// </summary>
	public TermInfoDatabaseSetIdentity Identity {
		get;
	}

	/// <summary>
	/// Gets the frozen DA02 precedence evidence.
	/// </summary>
	public TermInfoDatabaseSetLookupResult Lookup {
		get;
	}

	/// <summary>
	/// Gets the aggregate semantic relationship for the repeated identity.
	/// </summary>
	public TermInfoDatabaseSetSemanticRelationship Relationship {
		get;
	}

	/// <summary>
	/// Gets observed later-occurrence analyses in deterministic occurrence order.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetShadowAnalysis> Shadows {
		get;
	}

	/// <summary>
	/// Gets observed shadows semantically equal to the precedence winner.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetShadowAnalysis> EqualShadows =>
		_equalShadows;

	/// <summary>
	/// Gets observed shadows semantically different from the precedence winner.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetShadowAnalysis> ConflictingShadows =>
		_conflictingShadows;

	/// <summary>
	/// Gets later observed occurrences which cannot be compared to a known winner.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetShadowAnalysis> IndeterminateShadows =>
		_indeterminateShadows;

	/// <summary>
	/// Gets whether the complete occurrence universe was observed.
	/// </summary>
	public bool IsComplete =>
		Lookup.IsObservationComplete;
}
