using System.Text;

namespace Icod.TermInfo.Inspection;

/// <summary>
/// Selects deterministic ordered parents for relative terminfo source synthesis.
/// </summary>
/// <remarks>
/// RP01 freezes planning inputs, score ordering, result evidence, and candidate
/// snapshot semantics. RP02 evaluates the zero-parent baseline and every legal
/// single-parent candidate position. RP03 evaluates every legal ordered parent
/// permutation up to the active selected-parent bound. RP04 freezes bounded-
/// search arithmetic, cancellation boundaries, and search evidence. RP05 adds
/// explicit, completeness-preserving catalog and conventional-directory
/// orchestration without host discovery.
/// </remarks>
public static partial class TerminalDescriptionSourcePlanner {
	/// <summary>
	/// Plans relative source using the canonical bounded planning policy.
	/// </summary>
	/// <param name="target">The effective target terminal description.</param>
	/// <param name="candidates">
	/// Caller-ordered candidate parent positions. The sequence is enumerated at
	/// most once.
	/// </param>
	/// <returns>The selected deterministic relative-source plan.</returns>
	/// <exception cref="ArgumentException">
	/// The candidate sequence contains null or exceeds the configured limit.
	/// </exception>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="target"/> or <paramref name="candidates"/> is
	/// <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// Operational planning cannot satisfy the active exhaustive or representation
	/// policy.
	/// </exception>
	public static TerminalDescriptionSourcePlan Plan(
		TerminalDescription target,
		IEnumerable<TerminalDescriptionSourceSynthesisParent> candidates
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( candidates );

		return Plan(
			target,
			candidates,
			new TerminalDescriptionSourcePlanningOptions(),
			CancellationToken.None
		);
	}

	/// <summary>
	/// Plans relative source using an explicit bounded planning policy.
	/// </summary>
	/// <param name="target">The effective target terminal description.</param>
	/// <param name="candidates">
	/// Caller-ordered candidate parent positions. The sequence is enumerated at
	/// most once.
	/// </param>
	/// <param name="options">The immutable planning policy.</param>
	/// <returns>The selected deterministic relative-source plan.</returns>
	/// <exception cref="ArgumentException">
	/// The candidate sequence contains null or exceeds the configured limit.
	/// </exception>
	/// <exception cref="ArgumentNullException">
	/// A required argument is <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// Operational planning cannot satisfy the active exhaustive or representation
	/// policy.
	/// </exception>
	public static TerminalDescriptionSourcePlan Plan(
		TerminalDescription target,
		IEnumerable<TerminalDescriptionSourceSynthesisParent> candidates,
		TerminalDescriptionSourcePlanningOptions options
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( candidates );
		ArgumentNullException.ThrowIfNull( options );

		return Plan(
			target,
			candidates,
			options,
			CancellationToken.None
		);
	}

	/// <summary>
	/// Plans relative source using explicit policy and caller cancellation.
	/// </summary>
	/// <param name="target">The effective target terminal description.</param>
	/// <param name="candidates">
	/// Caller-ordered candidate parent positions. The sequence is enumerated at
	/// most once.
	/// </param>
	/// <param name="options">The immutable planning policy.</param>
	/// <param name="cancellationToken">
	/// A token observed before and during candidate snapshot and deterministic plan
	/// evaluation.
	/// </param>
	/// <returns>The selected deterministic relative-source plan.</returns>
	/// <exception cref="ArgumentException">
	/// The candidate sequence contains null or exceeds the configured limit.
	/// </exception>
	/// <exception cref="ArgumentNullException">
	/// A required argument is <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// Operational planning cannot satisfy the active exhaustive or representation
	/// policy.
	/// </exception>
	/// <exception cref="OperationCanceledException">
	/// <paramref name="cancellationToken"/> requests cancellation.
	/// </exception>
	public static TerminalDescriptionSourcePlan Plan(
		TerminalDescription target,
		IEnumerable<TerminalDescriptionSourceSynthesisParent> candidates,
		TerminalDescriptionSourcePlanningOptions options,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( candidates );
		ArgumentNullException.ThrowIfNull( options );

		TerminalDescriptionSourcePlanningRequest request =
			CreateRequest(
				target,
				candidates,
				options,
				cancellationToken
			);

		return EvaluateOrderedPlans(
			request,
			cancellationToken
		);
	}

	private static TerminalDescriptionSourcePlan EvaluateOrderedPlans(
		TerminalDescriptionSourcePlanningRequest request,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( request );
		cancellationToken.ThrowIfCancellationRequested();
		bool completePlanCountKnown =
			TryGetPlanCountWithinLimit(
				request.Candidates.Count,
				request.Options.MaximumSelectedParentCount,
				request.Options.MaximumEvaluatedPlanCount,
				out int requiredPlanCount
			);
		if ( !completePlanCountKnown
			&& !request.Options.AllowNonExhaustiveResult ) {
			throw new InvalidOperationException(
				$"Exhaustive ordered planning requires more than the configured maximum of {request.Options.MaximumEvaluatedPlanCount} evaluations."
			);
		}

		int evaluationLimit =
			completePlanCountKnown
				? requiredPlanCount
				: request.Options.MaximumEvaluatedPlanCount;
		int evaluatedPlanCount = 0;
		TerminalDescriptionSourcePlanningScore? bestScore = null;
		string? bestSource = null;
		TerminalDescriptionSourceSynthesisParent[]? bestParents = null;
		int maximumDepth =
			Math.Min(
				request.Candidates.Count,
				request.Options.MaximumSelectedParentCount
			);
		int[] selectedCandidateIndices = new int[ maximumDepth ];
		bool[] selectedCandidatePositions =
			new bool[ request.Candidates.Count ];

		EvaluatePlan(
			request,
			selectedCandidateIndices,
			selectedCount: 0,
			ref evaluatedPlanCount,
			ref bestScore,
			ref bestSource,
			ref bestParents,
			cancellationToken
		);

		for ( int depth = 1;
			depth <= maximumDepth && evaluatedPlanCount < evaluationLimit;
			depth++ ) {
			EnumeratePlansAtDepth(
				request,
				depth,
				selectedCandidateIndices,
				selectedCandidatePositions,
				selectedCount: 0,
				evaluationLimit,
				ref evaluatedPlanCount,
				ref bestScore,
				ref bestSource,
				ref bestParents,
				cancellationToken
			);
		}
		cancellationToken.ThrowIfCancellationRequested();

		if ( bestScore is null || bestSource is null ) {
			throw new InvalidOperationException(
				"No evaluated ordered plan satisfied the active synthesis and generated-source limits."
			);
		}

		return new TerminalDescriptionSourcePlan(
			bestParents ?? [],
			bestSource,
			bestScore,
			evaluatedPlanCount,
			isExhaustive: completePlanCountKnown
				&& evaluatedPlanCount == requiredPlanCount,
			candidateCount: request.Candidates.Count
		);
	}

	private static bool TryGetPlanCountWithinLimit(
		int candidateCount,
		int maximumSelectedParentCount,
		int limit,
		out int planCount
	) {
		int total = 1;
		int permutations = 1;
		int maximumDepth =
			Math.Min( candidateCount, maximumSelectedParentCount );
		for ( int depth = 1; depth <= maximumDepth; depth++ ) {
			int factor = candidateCount - depth + 1;
			if ( permutations > ( limit - total ) / factor ) {
				planCount = limit;
				return false;
			}

			permutations = checked( permutations * factor );
			total = checked( total + permutations );
		}

		planCount = total;
		return true;
	}

	private static void EnumeratePlansAtDepth(
		TerminalDescriptionSourcePlanningRequest request,
		int depth,
		int[] selectedCandidateIndices,
		bool[] selectedCandidatePositions,
		int selectedCount,
		int evaluationLimit,
		ref int evaluatedPlanCount,
		ref TerminalDescriptionSourcePlanningScore? bestScore,
		ref string? bestSource,
		ref TerminalDescriptionSourceSynthesisParent[]? bestParents,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( request );
		ArgumentNullException.ThrowIfNull( selectedCandidateIndices );
		ArgumentNullException.ThrowIfNull( selectedCandidatePositions );

		cancellationToken.ThrowIfCancellationRequested();
		if ( evaluatedPlanCount >= evaluationLimit ) {
			return;
		}
		if ( selectedCount == depth ) {
			EvaluatePlan(
				request,
				selectedCandidateIndices,
				selectedCount,
				ref evaluatedPlanCount,
				ref bestScore,
				ref bestSource,
				ref bestParents,
				cancellationToken
			);
			return;
		}

		for ( int candidateIndex = 0;
			candidateIndex < request.Candidates.Count
				&& evaluatedPlanCount < evaluationLimit;
			candidateIndex++ ) {
			if ( selectedCandidatePositions[ candidateIndex ] ) {
				continue;
			}

			selectedCandidateIndices[ selectedCount ] = candidateIndex;
			selectedCandidatePositions[ candidateIndex ] = true;
			EnumeratePlansAtDepth(
				request,
				depth,
				selectedCandidateIndices,
				selectedCandidatePositions,
				selectedCount + 1,
				evaluationLimit,
				ref evaluatedPlanCount,
				ref bestScore,
				ref bestSource,
				ref bestParents,
				cancellationToken
			);
			selectedCandidatePositions[ candidateIndex ] = false;
		}
	}

	private static void EvaluatePlan(
		TerminalDescriptionSourcePlanningRequest request,
		int[] selectedCandidateIndices,
		int selectedCount,
		ref int evaluatedPlanCount,
		ref TerminalDescriptionSourcePlanningScore? bestScore,
		ref string? bestSource,
		ref TerminalDescriptionSourceSynthesisParent[]? bestParents,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( request );
		ArgumentNullException.ThrowIfNull( selectedCandidateIndices );

		cancellationToken.ThrowIfCancellationRequested();
		evaluatedPlanCount = checked( evaluatedPlanCount + 1 );

		TerminalDescriptionSourceSynthesisParent[] parents =
			new TerminalDescriptionSourceSynthesisParent[ selectedCount ];
		int[] candidateIndices = new int[ selectedCount ];
		for ( int index = 0; index < selectedCount; index++ ) {
			int candidateIndex = selectedCandidateIndices[ index ];
			TerminalDescriptionSourceSynthesisParent parent =
				request.Candidates[ candidateIndex ];
			for ( int priorIndex = 0; priorIndex < index; priorIndex++ ) {
				if ( string.Equals(
					parents[ priorIndex ].UseName,
					parent.UseName,
					StringComparison.Ordinal
				) ) {
					return;
				}
			}
			parents[ index ] = parent;
			candidateIndices[ index ] = candidateIndex;
		}
		cancellationToken.ThrowIfCancellationRequested();
		TerminalDescriptionSourceSynthesisResult result;
		try {
			result =
				TerminalDescriptionSourceSynthesizer.SynthesizeWithEvidence(
					request.Target,
					parents,
					request.Options.SynthesisOptions
				);
		} catch ( InvalidOperationException ) {
			return;
		}
		cancellationToken.ThrowIfCancellationRequested();

		if ( result.Source.Length
			> request.Options.MaximumGeneratedSourceLength ) {
			return;
		}

		TerminalDescriptionSourcePlanningScore score =
			new(
				result.LocalDirectiveCount,
				result.CancellationCount,
				selectedCount,
				Encoding.UTF8.GetByteCount( result.Source ),
				candidateIndices
			);
		if ( bestScore is not null && score.CompareTo( bestScore ) >= 0 ) {
			return;
		}

		bestScore = score;
		bestSource = result.Source;
		bestParents = parents;
	}

	internal static TerminalDescriptionSourcePlanningRequest CreateRequest(
		TerminalDescription target,
		IEnumerable<TerminalDescriptionSourceSynthesisParent> candidates,
		TerminalDescriptionSourcePlanningOptions options,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( candidates );
		ArgumentNullException.ThrowIfNull( options );

		cancellationToken.ThrowIfCancellationRequested();

		HashSet<string> targetNames =
			new(
				StringComparer.Ordinal
			) {
				target.Name,
			};
		foreach ( string alias in target.Aliases ) {
			targetNames.Add( alias );
		}

		List<TerminalDescriptionSourceSynthesisParent> snapshot = [];
		foreach ( TerminalDescriptionSourceSynthesisParent? candidate in candidates ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( candidate is null ) {
				throw new ArgumentException(
					"The planning candidate sequence cannot contain null.",
					nameof( candidates )
				);
			}
			if ( targetNames.Contains( candidate.UseName ) ) {
				continue;
			}
			if ( snapshot.Count >= options.MaximumCandidateCount ) {
				throw new ArgumentException(
					$"The planning request exceeds the configured maximum of {options.MaximumCandidateCount} non-self candidates.",
					nameof( candidates )
				);
			}

			snapshot.Add( candidate );
		}
		cancellationToken.ThrowIfCancellationRequested();

		return new TerminalDescriptionSourcePlanningRequest(
			target,
			snapshot,
			options
		);
	}
}
