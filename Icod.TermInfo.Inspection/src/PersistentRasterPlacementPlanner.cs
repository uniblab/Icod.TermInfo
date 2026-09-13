namespace Icod.TermInfo.Inspection;

/// <summary>
/// Produces deterministic protocol-neutral advanced persistent-raster placement
/// plans from a frozen lifecycle plan, placement profile, and semantic request.
/// </summary>
public static class PersistentRasterPlacementPlanner {
	/// <summary>
	/// Plans one advanced placement request using the supplied lifecycle readiness
	/// and classified placement profile.
	/// </summary>
	/// <param name="lifecyclePlan">
	/// The prerequisite frozen 1.11 lifecycle plan.
	/// </param>
	/// <param name="placementProfile">
	/// The classified advanced-placement support profile.
	/// </param>
	/// <param name="request">The non-empty advanced-placement requirement request.</param>
	/// <returns>An immutable deterministic placement plan.</returns>
	/// <exception cref="ArgumentNullException">
	/// Any argument is <see langword="null"/>.
	/// </exception>
	public static PersistentRasterPlacementPlan Plan(
		PersistentRasterLifecyclePlan lifecyclePlan,
		PersistentRasterPlacementProfile placementProfile,
		PersistentRasterPlacementRequest request
	) {
		ArgumentNullException.ThrowIfNull( lifecyclePlan );
		ArgumentNullException.ThrowIfNull( placementProfile );
		ArgumentNullException.ThrowIfNull( request );

		List<PersistentRasterPlacementPlanRequirement> requirements = [];
		List<PersistentRasterPlacementPlanIssue> issues = [];
		bool hasPlacementImpossible = false;
		bool hasPlacementUnknown = false;

		if ( request.RequireSourceRectangle ) {
			AddRequirement(
				requirements,
				issues,
				PersistentRasterPlacementSubject.SourceRectangle,
				placementProfile.SourceRectangle,
				ref hasPlacementImpossible,
				ref hasPlacementUnknown
			);
		}
		if ( request.RequireSignedZOrder ) {
			AddRequirement(
				requirements,
				issues,
				PersistentRasterPlacementSubject.SignedZOrder,
				placementProfile.SignedZOrder,
				ref hasPlacementImpossible,
				ref hasPlacementUnknown
			);
		}

		PersistentRasterPlacementPlanStatus status =
			( lifecyclePlan.Status == PersistentRasterLifecyclePlanStatus.Impossible
				|| hasPlacementImpossible )
				? PersistentRasterPlacementPlanStatus.Impossible
				: ( lifecyclePlan.Status == PersistentRasterLifecyclePlanStatus.Indeterminate )
					? PersistentRasterPlacementPlanStatus.Indeterminate
					: ( hasPlacementUnknown )
						? PersistentRasterPlacementPlanStatus.RequiresRuntimeVerification
						: PersistentRasterPlacementPlanStatus.Satisfied
		;

		return new PersistentRasterPlacementPlan(
			status,
			lifecyclePlan.Status,
			requirements,
			issues
		);
	}

	private static void AddRequirement(
		List<PersistentRasterPlacementPlanRequirement> requirements,
		List<PersistentRasterPlacementPlanIssue> issues,
		PersistentRasterPlacementSubject subject,
		PersistentRasterLifecycleSupportStatus supportStatus,
		ref bool hasPlacementImpossible,
		ref bool hasPlacementUnknown
	) {
		ArgumentNullException.ThrowIfNull( requirements );
		ArgumentNullException.ThrowIfNull( issues );

		requirements.Add(
			new PersistentRasterPlacementPlanRequirement(
				subject,
				supportStatus
			)
		);
		if ( supportStatus == PersistentRasterLifecycleSupportStatus.Supported ) {
			return;
		}

		issues.Add(
			new PersistentRasterPlacementPlanIssue(
				subject,
				supportStatus
			)
		);
		if ( supportStatus == PersistentRasterLifecycleSupportStatus.Unknown ) {
			hasPlacementUnknown = true;
			return;
		}

		hasPlacementImpossible = true;
	}
}
