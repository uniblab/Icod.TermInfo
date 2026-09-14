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
