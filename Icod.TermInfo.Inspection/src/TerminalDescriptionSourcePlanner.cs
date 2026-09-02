using System.Text;

namespace Icod.TermInfo.Inspection;

/// <summary>
/// Selects deterministic ordered parents for relative terminfo source synthesis.
/// </summary>
/// <remarks>
/// RP01 freezes planning inputs, score ordering, result evidence, and candidate
/// snapshot semantics. RP02 evaluates the zero-parent baseline and every legal
/// single-parent candidate position.
/// </remarks>
public static class TerminalDescriptionSourcePlanner {
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

		return EvaluateZeroAndSingleParentPlans(
			request,
			cancellationToken
		);
	}

	private static TerminalDescriptionSourcePlan EvaluateZeroAndSingleParentPlans(
		TerminalDescriptionSourcePlanningRequest request,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( request );
		cancellationToken.ThrowIfCancellationRequested();
		if ( request.Candidates.Count > 1
			&& request.Options.MaximumSelectedParentCount > 1 ) {
			throw new InvalidOperationException(
				"The active limits admit ordered multi-parent plans, which begin in RP03. Configure MaximumSelectedParentCount as one for exhaustive RP02 planning."
			);
		}

		int singleParentPlanCount =
			request.Options.MaximumSelectedParentCount == 0
				? 0
				: request.Candidates.Count;
		int requiredPlanCount =
			checked( singleParentPlanCount + 1 );
		if ( requiredPlanCount > request.Options.MaximumEvaluatedPlanCount
			&& !request.Options.AllowNonExhaustiveResult ) {
			throw new InvalidOperationException(
				$"Exhaustive zero- and single-parent planning requires {requiredPlanCount} evaluations, but the configured maximum is {request.Options.MaximumEvaluatedPlanCount}."
			);
		}

		int evaluationLimit =
			Math.Min(
				requiredPlanCount,
				request.Options.MaximumEvaluatedPlanCount
			);
		int evaluatedPlanCount = 0;
		TerminalDescriptionSourcePlanningScore? bestScore = null;
		string? bestSource = null;
		TerminalDescriptionSourceSynthesisParent? bestParent = null;

		EvaluatePlan(
			request,
			candidateIndex: null,
			ref evaluatedPlanCount,
			ref bestScore,
			ref bestSource,
			ref bestParent,
			cancellationToken
		);

		for ( int candidateIndex = 0;
			candidateIndex < singleParentPlanCount
				&& evaluatedPlanCount < evaluationLimit;
			candidateIndex++ ) {
			EvaluatePlan(
				request,
				candidateIndex,
				ref evaluatedPlanCount,
				ref bestScore,
				ref bestSource,
				ref bestParent,
				cancellationToken
			);
		}
		cancellationToken.ThrowIfCancellationRequested();

		if ( bestScore is null || bestSource is null ) {
			throw new InvalidOperationException(
				"No evaluated zero- or single-parent plan satisfied the active synthesis and generated-source limits."
			);
		}

		IEnumerable<TerminalDescriptionSourceSynthesisParent> selectedParents =
			bestParent is null
				? []
				: [ bestParent ];
		return new TerminalDescriptionSourcePlan(
			selectedParents,
			bestSource,
			bestScore,
			evaluatedPlanCount,
			isExhaustive: evaluatedPlanCount == requiredPlanCount,
			candidateCount: request.Candidates.Count
		);
	}

	private static void EvaluatePlan(
		TerminalDescriptionSourcePlanningRequest request,
		int? candidateIndex,
		ref int evaluatedPlanCount,
		ref TerminalDescriptionSourcePlanningScore? bestScore,
		ref string? bestSource,
		ref TerminalDescriptionSourceSynthesisParent? bestParent,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( request );

		cancellationToken.ThrowIfCancellationRequested();
		evaluatedPlanCount = checked( evaluatedPlanCount + 1 );

		TerminalDescriptionSourceSynthesisParent? candidate =
			candidateIndex.HasValue
				? request.Candidates[ candidateIndex.Value ]
				: null;
		IEnumerable<TerminalDescriptionSourceSynthesisParent> parents =
			candidate is null
				? []
				: [ candidate ];
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

		if ( result.Source.Length
			> request.Options.MaximumGeneratedSourceLength ) {
			return;
		}

		int[] selectedCandidateIndices =
			candidateIndex.HasValue
				? [ candidateIndex.Value ]
				: [];
		TerminalDescriptionSourcePlanningScore score =
			new(
				result.LocalDirectiveCount,
				result.CancellationCount,
				selectedCandidateIndices.Length,
				Encoding.UTF8.GetByteCount( result.Source ),
				selectedCandidateIndices
			);
		if ( bestScore is not null && score.CompareTo( bestScore ) >= 0 ) {
			return;
		}

		bestScore = score;
		bestSource = result.Source;
		bestParent = candidate;
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
