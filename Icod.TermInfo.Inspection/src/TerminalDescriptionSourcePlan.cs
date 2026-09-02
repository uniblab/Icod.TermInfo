namespace Icod.TermInfo.Inspection;

/// <summary>
/// Contains the immutable result and evidence of one relative-source planning
/// operation.
/// </summary>
public sealed class TerminalDescriptionSourcePlan {
	internal TerminalDescriptionSourcePlan(
		IEnumerable<TerminalDescriptionSourceSynthesisParent> selectedParents,
		string source,
		TerminalDescriptionSourcePlanningScore score,
		int evaluatedPlanCount,
		bool isExhaustive,
		int candidateCount
	) {
		ArgumentNullException.ThrowIfNull( selectedParents );
		ArgumentNullException.ThrowIfNull( source );
		ArgumentNullException.ThrowIfNull( score );
		if ( evaluatedPlanCount < 1 ) {
			throw new ArgumentOutOfRangeException(
				nameof( evaluatedPlanCount ),
				evaluatedPlanCount,
				"The evaluated plan count must be positive."
			);
		}
		if ( candidateCount < 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( candidateCount ),
				candidateCount,
				"The considered candidate count cannot be negative."
			);
		}

		TerminalDescriptionSourceSynthesisParent[] parentArray =
			selectedParents.ToArray();
		if ( parentArray.Any( parent => parent is null ) ) {
			throw new ArgumentException(
				"The selected parent sequence cannot contain null.",
				nameof( selectedParents )
			);
		}
		if ( parentArray.Length != score.ParentCount ) {
			throw new ArgumentException(
				"The selected parent count must equal the planning score parent count.",
				nameof( selectedParents )
			);
		}
		if ( parentArray.Length > candidateCount ) {
			throw new ArgumentException(
				"The selected parent count cannot exceed the considered candidate count.",
				nameof( selectedParents )
			);
		}
		if (
			score.SelectedCandidateIndices.Any(
				candidateIndex => candidateIndex >= candidateCount
			)
		) {
			throw new ArgumentException(
				"Selected candidate indices must identify considered candidate positions.",
				nameof( score )
			);
		}

		SelectedParents = Array.AsReadOnly( parentArray );
		Source = source;
		Score = score;
		EvaluatedPlanCount = evaluatedPlanCount;
		IsExhaustive = isExhaustive;
		CandidateCount = candidateCount;
	}

	/// <summary>
	/// Gets the selected parents in exact emitted <c>use=</c> order.
	/// </summary>
	public IReadOnlyList<TerminalDescriptionSourceSynthesisParent> SelectedParents {
		get;
	}

	/// <summary>
	/// Gets the deterministic generated LF terminfo source.
	/// </summary>
	public string Source {
		get;
	}

	/// <summary>
	/// Gets the frozen lexicographic score of the selected plan.
	/// </summary>
	public TerminalDescriptionSourcePlanningScore Score {
		get;
	}

	/// <summary>
	/// Gets the number of valid or rejected candidate plans evaluated.
	/// </summary>
	public int EvaluatedPlanCount {
		get;
	}

	/// <summary>
	/// Gets whether every legal plan under the active limits was evaluated.
	/// </summary>
	public bool IsExhaustive {
		get;
	}

	/// <summary>
	/// Gets the number of non-self candidate positions considered.
	/// </summary>
	public int CandidateCount {
		get;
	}
}
