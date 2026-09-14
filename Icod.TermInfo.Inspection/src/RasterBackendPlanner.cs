namespace Icod.TermInfo.Inspection;

/// <summary>
/// Evaluates raster-backend candidates by composing backend availability with the
/// frozen persistent-raster lifecycle and placement planners.
/// </summary>
public static class RasterBackendPlanner {
	/// <summary>
	/// Evaluates one raster-backend candidate against the supplied semantic request.
	/// </summary>
	/// <param name="candidate">The immutable backend candidate to evaluate.</param>
	/// <param name="request">The lifecycle and optional placement requirements.</param>
	/// <returns>
	/// An immutable candidate evaluation retaining the exact lifecycle plan and,
	/// when requested, placement plan used to derive its status.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="candidate"/> or <paramref name="request"/> is
	/// <see langword="null"/>.
	/// </exception>
	public static RasterBackendCandidateEvaluation Evaluate(
		RasterBackendCandidate candidate,
		RasterBackendSelectionRequest request
	) {
		ArgumentNullException.ThrowIfNull( candidate );
		ArgumentNullException.ThrowIfNull( request );

		PersistentRasterLifecyclePlan lifecyclePlan =
			PersistentRasterLifecyclePlanner.Plan(
				candidate.LifecycleProfile,
				request.LifecycleRequest
			);

		PersistentRasterPlacementPlan? placementPlan = null;
		if ( request.PlacementRequest is not null ) {
			placementPlan = PersistentRasterPlacementPlanner.Plan(
				lifecyclePlan,
				candidate.PlacementProfile,
				request.PlacementRequest
			);
		}

		RasterBackendCandidateStatus status = candidate.BackendProfile.Status switch {
			RasterBackendSupportStatus.Unsupported =>
				RasterBackendCandidateStatus.Impossible,
			RasterBackendSupportStatus.Unknown
				or RasterBackendSupportStatus.Contradicted =>
				RasterBackendCandidateStatus.RequiresRuntimeVerification,
			RasterBackendSupportStatus.Supported =>
				DeriveSupportedStatus(
					lifecyclePlan,
					placementPlan
				),
			_ => throw new InvalidOperationException(
				"The raster-backend profile contains an undefined support status."
			),
		};

		return new RasterBackendCandidateEvaluation(
			candidate,
			lifecyclePlan,
			placementPlan,
			status
		);
	}

	/// <summary>
	/// Builds a deterministic raster-backend selection plan from one or two
	/// candidates and optional caller-owned preference policy.
	/// </summary>
	/// <param name="candidates">The candidates available to the caller.</param>
	/// <param name="request">The lifecycle and optional placement requirements.</param>
	/// <param name="options">
	/// Optional explicit backend preference. <see langword="null"/> means no
	/// caller ranking.
	/// </param>
	/// <returns>An immutable deterministic raster-backend selection plan.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="candidates"/> or <paramref name="request"/> is
	/// <see langword="null"/>.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// Candidate input is empty, exceeds the closed backend bound, contains null
	/// or duplicate candidates, or a non-empty preference does not exactly order
	/// the supplied candidate backends.
	/// </exception>
	public static RasterBackendSelectionPlan Plan(
		IEnumerable<RasterBackendCandidate> candidates,
		RasterBackendSelectionRequest request,
		RasterBackendSelectionOptions? options = null
	) {
		ArgumentNullException.ThrowIfNull( candidates );
		ArgumentNullException.ThrowIfNull( request );

		RasterBackendSelectionOptions effectiveOptions =
			options ?? new RasterBackendSelectionOptions();
		RasterBackendCandidate[] snapshot = candidates.ToArray();
		ValidateSelectionInput( snapshot, effectiveOptions );

		RasterBackendCandidateEvaluation[] evaluations = snapshot
			.Select( candidate => Evaluate( candidate, request ) )
			.ToArray();
		RasterBackendCandidateEvaluation[] canonicalEvaluations = evaluations
			.OrderBy(
				evaluation => (int)evaluation.Candidate.BackendProfile.Backend
			)
			.ToArray();

		if ( effectiveOptions.PreferenceOrder.Count != 0 ) {
			return PlanWithPreference(
				request,
				effectiveOptions,
				canonicalEvaluations
			);
		}

		return PlanWithoutPreference(
			request,
			effectiveOptions,
			canonicalEvaluations
		);
	}

	private static void ValidateSelectionInput(
		IReadOnlyList<RasterBackendCandidate> candidates,
		RasterBackendSelectionOptions options
	) {
		if ( candidates.Count == 0 ) {
			throw new ArgumentException(
				"Raster-backend selection requires at least one candidate.",
				nameof( candidates )
			);
		}
		if ( candidates.Count > RasterBackendSelectionOptions.MaximumSupportedPreferenceCount ) {
			throw new ArgumentException(
				$"Raster-backend selection cannot evaluate more than {RasterBackendSelectionOptions.MaximumSupportedPreferenceCount} candidates.",
				nameof( candidates )
			);
		}

		HashSet<RasterBackendKind> candidateBackends = [];
		foreach ( RasterBackendCandidate? candidate in candidates ) {
			if ( candidate is null ) {
				throw new ArgumentException(
					"Raster-backend candidates cannot contain null elements.",
					nameof( candidates )
				);
			}
			if ( !candidateBackends.Add( candidate.BackendProfile.Backend ) ) {
				throw new ArgumentException(
					"Raster-backend selection cannot contain duplicate backend candidates.",
					nameof( candidates )
				);
			}
		}

		if ( options.PreferenceOrder.Count == 0 ) {
			return;
		}
		if ( options.PreferenceOrder.Count != candidates.Count ) {
			throw new ArgumentException(
				"A raster-backend preference order must contain every supplied candidate exactly once.",
				nameof( options )
			);
		}
		foreach ( RasterBackendKind preferredBackend in options.PreferenceOrder ) {
			if ( !candidateBackends.Contains( preferredBackend ) ) {
				throw new ArgumentException(
					"A raster-backend preference order cannot contain an absent candidate.",
					nameof( options )
				);
			}
		}
	}

	private static RasterBackendSelectionPlan PlanWithPreference(
		RasterBackendSelectionRequest request,
		RasterBackendSelectionOptions options,
		IReadOnlyList<RasterBackendCandidateEvaluation> evaluations
	) {
		foreach ( RasterBackendKind preferredBackend in options.PreferenceOrder ) {
			RasterBackendCandidateEvaluation evaluation = evaluations.Single(
				item => item.Candidate.BackendProfile.Backend == preferredBackend
			);

			switch ( evaluation.Status ) {
				case RasterBackendCandidateStatus.Impossible:
					continue;
				case RasterBackendCandidateStatus.RequiresRuntimeVerification:
					return new RasterBackendSelectionPlan(
						request,
						options,
						evaluations,
						RasterBackendSelectionStatus.RequiresRuntimeVerification,
						null
					);
				case RasterBackendCandidateStatus.Satisfied:
					return new RasterBackendSelectionPlan(
						request,
						options,
						evaluations,
						RasterBackendSelectionStatus.Selected,
						preferredBackend
					);
				default:
					throw new InvalidOperationException(
						"The raster-backend candidate evaluation contains an undefined status."
					);
			}
		}

		return new RasterBackendSelectionPlan(
			request,
			options,
			evaluations,
			RasterBackendSelectionStatus.Impossible,
			null
		);
	}

	private static RasterBackendSelectionPlan PlanWithoutPreference(
		RasterBackendSelectionRequest request,
		RasterBackendSelectionOptions options,
		IReadOnlyList<RasterBackendCandidateEvaluation> evaluations
	) {
		RasterBackendCandidateEvaluation[] viable = evaluations
			.Where( item => item.Status != RasterBackendCandidateStatus.Impossible )
			.ToArray();

		if ( viable.Length == 0 ) {
			return new RasterBackendSelectionPlan(
				request,
				options,
				evaluations,
				RasterBackendSelectionStatus.Impossible,
				null
			);
		}
		if ( viable.Length > 1 ) {
			return new RasterBackendSelectionPlan(
				request,
				options,
				evaluations,
				RasterBackendSelectionStatus.RequiresPreference,
				null
			);
		}

		RasterBackendCandidateEvaluation soleViable = viable[ 0 ];
		return soleViable.Status switch {
			RasterBackendCandidateStatus.Satisfied =>
				new RasterBackendSelectionPlan(
					request,
					options,
					evaluations,
					RasterBackendSelectionStatus.Selected,
					soleViable.Candidate.BackendProfile.Backend
				),
			RasterBackendCandidateStatus.RequiresRuntimeVerification =>
				new RasterBackendSelectionPlan(
					request,
					options,
					evaluations,
					RasterBackendSelectionStatus.RequiresRuntimeVerification,
					null
				),
			_ => throw new InvalidOperationException(
				"The sole viable raster-backend candidate has an invalid status."
			),
		};
	}

	private static RasterBackendCandidateStatus DeriveSupportedStatus(
		PersistentRasterLifecyclePlan lifecyclePlan,
		PersistentRasterPlacementPlan? placementPlan
	) {
		if ( lifecyclePlan.Status == PersistentRasterLifecyclePlanStatus.Impossible ) {
			return RasterBackendCandidateStatus.Impossible;
		}
		if ( lifecyclePlan.Status == PersistentRasterLifecyclePlanStatus.Indeterminate ) {
			return RasterBackendCandidateStatus.RequiresRuntimeVerification;
		}
		if ( placementPlan is null ) {
			return RasterBackendCandidateStatus.Satisfied;
		}

		return placementPlan.Status switch {
			PersistentRasterPlacementPlanStatus.Impossible =>
				RasterBackendCandidateStatus.Impossible,
			PersistentRasterPlacementPlanStatus.RequiresRuntimeVerification
				or PersistentRasterPlacementPlanStatus.Indeterminate =>
				RasterBackendCandidateStatus.RequiresRuntimeVerification,
			PersistentRasterPlacementPlanStatus.Satisfied =>
				RasterBackendCandidateStatus.Satisfied,
			_ => throw new InvalidOperationException(
				"The persistent-raster placement plan contains an undefined status."
			),
		};
	}
}
