namespace Icod.TermInfo.Inspection;

/// <summary>
/// Contains one frozen 1.8 relative-source plan together with deterministic
/// database-set candidate provenance.
/// </summary>
public sealed class TermInfoDatabaseSetSourcePlanningResult {
	internal TermInfoDatabaseSetSourcePlanningResult(
		TermInfoDatabaseSet databaseSet,
		TerminalDescriptionSourcePlan plan,
		IEnumerable<TermInfoDatabaseSetPlanningCandidate> candidates,
		int collapsedDuplicateOccurrenceCount,
		int candidateSemanticComparisonCount
	) {
		ArgumentNullException.ThrowIfNull( databaseSet );
		ArgumentNullException.ThrowIfNull( plan );
		ArgumentNullException.ThrowIfNull( candidates );
		if ( collapsedDuplicateOccurrenceCount < 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( collapsedDuplicateOccurrenceCount )
			);
		}
		if ( candidateSemanticComparisonCount < 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( candidateSemanticComparisonCount )
			);
		}

		TermInfoDatabaseSetPlanningCandidate[] candidateArray = candidates.ToArray();
		if ( candidateArray.Any( candidate => candidate is null ) ) {
			throw new ArgumentException(
				"Database-set planning candidates cannot contain null.",
				nameof( candidates )
			);
		}
		if ( candidateArray.Length != plan.CandidateCount ) {
			throw new ArgumentException(
				"Database-set candidate evidence must match the frozen planner candidate count.",
				nameof( candidates )
			);
		}
		for ( int index = 0; index < candidateArray.Length; index++ ) {
			if ( candidateArray[ index ].CandidateIndex != index ) {
				throw new ArgumentException(
					"Database-set candidate indices must be contiguous planner positions.",
					nameof( candidates )
				);
			}
		}

		TermInfoDatabaseSetPlanningCandidate[] selected =
			new TermInfoDatabaseSetPlanningCandidate[
				plan.Score.SelectedCandidateIndices.Count
			];
		for ( int index = 0; index < selected.Length; index++ ) {
			int candidateIndex = plan.Score.SelectedCandidateIndices[ index ];
			if ( candidateIndex < 0 || candidateIndex >= candidateArray.Length ) {
				throw new ArgumentException(
					"The frozen plan selected a candidate position outside the database-set candidate evidence.",
					nameof( plan )
				);
			}
			selected[ index ] = candidateArray[ candidateIndex ];
			if ( !ReferenceEquals(
				selected[ index ].Parent,
				plan.SelectedParents[ index ]
			) ) {
				throw new ArgumentException(
					"Selected database-set candidate evidence must preserve the exact frozen planner parent objects.",
					nameof( plan )
				);
			}
		}

		DatabaseSet = databaseSet;
		Plan = plan;
		Candidates = Array.AsReadOnly( candidateArray );
		SelectedCandidates = Array.AsReadOnly( selected );
		CollapsedDuplicateOccurrenceCount = collapsedDuplicateOccurrenceCount;
		CandidateSemanticComparisonCount = candidateSemanticComparisonCount;
	}

	/// <summary>
	/// Gets the exact immutable ordered database set used for candidate discovery.
	/// </summary>
	public TermInfoDatabaseSet DatabaseSet {
		get;
	}

	/// <summary>
	/// Gets the unchanged frozen 1.8 planner result.
	/// </summary>
	public TerminalDescriptionSourcePlan Plan {
		get;
	}

	/// <summary>
	/// Gets canonical non-self candidate positions in exact order supplied to the
	/// frozen planner.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetPlanningCandidate> Candidates {
		get;
	}

	/// <summary>
	/// Gets selected candidate evidence in exact emitted <c>use=</c> order.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetPlanningCandidate> SelectedCandidates {
		get;
	}

	/// <summary>
	/// Gets the number of later semantically equal physical publications collapsed
	/// behind the first ordered canonical representative.
	/// </summary>
	public int CollapsedDuplicateOccurrenceCount {
		get;
	}

	/// <summary>
	/// Gets the number of semantic duplicate-validation comparisons performed while
	/// constructing the candidate universe before invoking the frozen planner.
	/// </summary>
	public int CandidateSemanticComparisonCount {
		get;
	}
}
