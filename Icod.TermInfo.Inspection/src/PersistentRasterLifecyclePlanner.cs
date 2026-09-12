namespace Icod.TermInfo.Inspection;

/// <summary>
/// Produces bounded deterministic protocol-neutral plans from lifecycle profiles
/// and semantic requests.
/// </summary>
public static class PersistentRasterLifecyclePlanner {
	/// <summary>
	/// Plans one lifecycle request from the supplied classified lifecycle profile.
	/// </summary>
	/// <param name="profile">The immutable lifecycle support profile.</param>
	/// <param name="request">The bounded semantic lifecycle request.</param>
	/// <returns>
	/// An immutable successful, indeterminate, or impossible planning result.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="profile"/> or <paramref name="request"/> is
	/// <see langword="null"/>.
	/// </exception>
	public static PersistentRasterLifecyclePlan Plan(
		PersistentRasterLifecycleProfile profile,
		PersistentRasterLifecycleRequest request
	) {
		ArgumentNullException.ThrowIfNull( profile );
		ArgumentNullException.ThrowIfNull( request );

		List<Requirement> requirements = [];
		if ( request.DisplayEphemeral ) {
			requirements.Add(
				new Requirement(
					PersistentRasterLifecycleOperation.DisplayEphemeral,
					PersistentRasterLifecycleEvidenceSubject.RasterDisplay
				)
			);
		}
		if ( request.UploadResource ) {
			requirements.Add(
				new Requirement(
					PersistentRasterLifecycleOperation.UploadResource,
					PersistentRasterLifecycleEvidenceSubject.PersistentUpload
				)
			);
			if ( request.RequireAcknowledgedUpload ) {
				requirements.Add(
					new Requirement(
						PersistentRasterLifecycleOperation.UploadResource,
						PersistentRasterLifecycleEvidenceSubject.AcknowledgedUpload
					)
				);
			}
		}
		if ( request.PlacementCount > 0 ) {
			requirements.Add(
				new Requirement(
					PersistentRasterLifecycleOperation.CreatePlacement,
					PersistentRasterLifecycleEvidenceSubject.PlacementCreation
				)
			);
			if ( request.PlacementCount > 1 ) {
				requirements.Add(
					new Requirement(
						PersistentRasterLifecycleOperation.CreatePlacement,
						PersistentRasterLifecycleEvidenceSubject.MultiplePlacements
					)
				);
			}
		}
		if ( request.UpdatePlacement ) {
			requirements.Add(
				new Requirement(
					PersistentRasterLifecycleOperation.UpdatePlacement,
					PersistentRasterLifecycleEvidenceSubject.PlacementUpdate
				)
			);
		}
		if ( request.DeletePlacement ) {
			requirements.Add(
				new Requirement(
					PersistentRasterLifecycleOperation.DeletePlacement,
					PersistentRasterLifecycleEvidenceSubject.PlacementDeletion
				)
			);
		}
		if ( request.DeleteResource ) {
			requirements.Add(
				new Requirement(
					PersistentRasterLifecycleOperation.DeleteResource,
					PersistentRasterLifecycleEvidenceSubject.ResourceDeletion
				)
			);
		}

		List<PersistentRasterLifecyclePlanIssue> issues = [];
		bool hasUnsupported = false;
		bool hasUncertain = false;
		foreach ( Requirement requirement in requirements ) {
			PersistentRasterLifecycleSupportStatus supportStatus =
				profile.GetStatus(
					requirement.Subject
				);
			if ( supportStatus == PersistentRasterLifecycleSupportStatus.Supported ) {
				continue;
			}

			issues.Add(
				new PersistentRasterLifecyclePlanIssue(
					requirement.Operation,
					requirement.Subject,
					supportStatus
				)
			);
			if ( supportStatus == PersistentRasterLifecycleSupportStatus.Unsupported ) {
				hasUnsupported = true;
			} else {
				hasUncertain = true;
			}
		}

		PersistentRasterLifecyclePlanStatus planStatus =
			( hasUnsupported )
				? PersistentRasterLifecyclePlanStatus.Impossible
				: ( hasUncertain )
					? PersistentRasterLifecyclePlanStatus.Indeterminate
					: PersistentRasterLifecyclePlanStatus.Success
		;
		if ( planStatus == PersistentRasterLifecyclePlanStatus.Impossible ) {
			return new PersistentRasterLifecyclePlan(
				planStatus,
				Array.Empty<PersistentRasterLifecyclePlanStep>(),
				issues
			);
		}

		List<PersistentRasterLifecyclePlanStep> steps = [];
		if ( request.DisplayEphemeral ) {
			AddStep(
				steps,
				PersistentRasterLifecycleOperation.DisplayEphemeral,
				NeedsVerification(
					profile.RasterDisplay
				)
			);
		}
		if ( request.UploadResource ) {
			bool uploadNeedsVerification =
				NeedsVerification(
					profile.PersistentUpload
				)
				|| (
					request.RequireAcknowledgedUpload
					&& NeedsVerification(
						profile.AcknowledgedUpload
					)
				);
			AddStep(
				steps,
				PersistentRasterLifecycleOperation.UploadResource,
				uploadNeedsVerification
			);
		}
		for ( int placementIndex = 0; placementIndex < request.PlacementCount; placementIndex++ ) {
			bool placementNeedsVerification =
				NeedsVerification(
					profile.PlacementCreation
				)
				|| (
					placementIndex > 0
					&& NeedsVerification(
						profile.MultiplePlacements
					)
				);
			AddStep(
				steps,
				PersistentRasterLifecycleOperation.CreatePlacement,
				placementNeedsVerification
			);
		}
		if ( request.UpdatePlacement ) {
			AddStep(
				steps,
				PersistentRasterLifecycleOperation.UpdatePlacement,
				NeedsVerification(
					profile.PlacementUpdate
				)
			);
		}
		if ( request.DeletePlacement ) {
			AddStep(
				steps,
				PersistentRasterLifecycleOperation.DeletePlacement,
				NeedsVerification(
					profile.PlacementDeletion
				)
			);
		}
		if ( request.DeleteResource ) {
			AddStep(
				steps,
				PersistentRasterLifecycleOperation.DeleteResource,
				NeedsVerification(
					profile.ResourceDeletion
				)
			);
		}

		return new PersistentRasterLifecyclePlan(
			planStatus,
			steps,
			issues
		);
	}

	private static void AddStep(
		List<PersistentRasterLifecyclePlanStep> steps,
		PersistentRasterLifecycleOperation operation,
		bool requiresRuntimeVerification
	) {
		ArgumentNullException.ThrowIfNull( steps );

		steps.Add(
			new PersistentRasterLifecyclePlanStep(
				steps.Count,
				operation,
				requiresRuntimeVerification
			)
		);
	}

	private static bool NeedsVerification(
		PersistentRasterLifecycleSupportStatus supportStatus
	) {
		return supportStatus
			is PersistentRasterLifecycleSupportStatus.Unknown
			or PersistentRasterLifecycleSupportStatus.Contradicted;
	}

	private readonly record struct Requirement(
		PersistentRasterLifecycleOperation Operation,
		PersistentRasterLifecycleEvidenceSubject Subject
	);
}
