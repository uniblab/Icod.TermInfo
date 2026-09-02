namespace Icod.TermInfo.Inspection;

/// <summary>
/// Selects deterministic ordered parents for relative terminfo source synthesis.
/// </summary>
/// <remarks>
/// RP01 freezes planning inputs, score ordering, result evidence, and candidate
/// snapshot semantics. Operational zero- and single-parent search begins in RP02.
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
	/// <exception cref="NotSupportedException">
	/// RP01 contract-only planning is active; search begins in RP02.
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
	/// <exception cref="NotSupportedException">
	/// RP01 contract-only planning is active; search begins in RP02.
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
	/// A token observed before and during candidate snapshot and, from RP02,
	/// deterministic plan evaluation.
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
	/// <exception cref="NotSupportedException">
	/// RP01 contract-only planning is active; search begins in RP02.
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

		_ = CreateRequest(
			target,
			candidates,
			options,
			cancellationToken
		);

		throw new NotSupportedException(
			"RP01 establishes the relative-source planning contract. "
				+ "Operational zero- and single-parent planning begins in RP02."
		);
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
