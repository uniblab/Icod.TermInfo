namespace Icod.TermInfo.Inspection;

/// <summary>
/// Represents the immutable derived evaluation of one raster-backend candidate.
/// </summary>
public sealed class RasterBackendCandidateEvaluation {
	internal RasterBackendCandidateEvaluation(
		RasterBackendCandidate candidate,
		PersistentRasterLifecyclePlan lifecyclePlan,
		PersistentRasterPlacementPlan? placementPlan,
		RasterBackendCandidateStatus status
	) {
		ArgumentNullException.ThrowIfNull( candidate );
		ArgumentNullException.ThrowIfNull( lifecyclePlan );
		if ( !Enum.IsDefined( status ) ) {
			throw new ArgumentOutOfRangeException( nameof( status ) );
		}

		Candidate = candidate;
		LifecyclePlan = lifecyclePlan;
		PlacementPlan = placementPlan;
		Status = status;
	}

	/// <summary>Gets the candidate that was evaluated.</summary>
	public RasterBackendCandidate Candidate {
		get;
	}

	/// <summary>Gets the frozen lifecycle planner result.</summary>
	public PersistentRasterLifecyclePlan LifecyclePlan {
		get;
	}

	/// <summary>
	/// Gets the frozen placement planner result, or <see langword="null"/> when no
	/// advanced placement semantics were requested.
	/// </summary>
	public PersistentRasterPlacementPlan? PlacementPlan {
		get;
	}

	/// <summary>Gets the derived candidate status.</summary>
	public RasterBackendCandidateStatus Status {
		get;
	}
}
