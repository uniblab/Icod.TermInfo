using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RB03RasterBackendCandidateEvaluationTests {
	[Fact]
	public void EvaluateRejectsNullCandidate() {
		RasterBackendSelectionRequest request = new(
			new PersistentRasterLifecycleRequest( displayEphemeral: true )
		);

		Assert.Throws<ArgumentNullException>(
			() => RasterBackendPlanner.Evaluate( null!, request )
		);
	}

	[Fact]
	public void EvaluateRejectsNullRequest() {
		RasterBackendCandidate candidate = CreateLifecycleCandidate(
			RasterBackendKind.Sixel,
			RasterBackendSupportStatus.Supported,
			PersistentRasterLifecycleSupportStatus.Supported
		);

		Assert.Throws<ArgumentNullException>(
			() => RasterBackendPlanner.Evaluate( candidate, null! )
		);
	}

	[Fact]
	public void SupportedBackendAndSuccessfulLifecycleAreSatisfiedWithoutPlacementPlan() {
		RasterBackendCandidate candidate = CreateLifecycleCandidate(
			RasterBackendKind.Sixel,
			RasterBackendSupportStatus.Supported,
			PersistentRasterLifecycleSupportStatus.Supported
		);
		RasterBackendSelectionRequest request = new(
			new PersistentRasterLifecycleRequest( displayEphemeral: true )
		);

		RasterBackendCandidateEvaluation evaluation =
			RasterBackendPlanner.Evaluate( candidate, request );

		Assert.Same( candidate, evaluation.Candidate );
		Assert.Equal( RasterBackendCandidateStatus.Satisfied, evaluation.Status );
		Assert.Equal(
			PersistentRasterLifecyclePlanStatus.Success,
			evaluation.LifecyclePlan.Status
		);
		Assert.Null( evaluation.PlacementPlan );
	}

	[Fact]
	public void SupportedBackendAndImpossibleLifecycleAreImpossible() {
		RasterBackendCandidate candidate = CreateLifecycleCandidate(
			RasterBackendKind.Sixel,
			RasterBackendSupportStatus.Supported,
			PersistentRasterLifecycleSupportStatus.Unsupported
		);
		RasterBackendSelectionRequest request = new(
			new PersistentRasterLifecycleRequest( displayEphemeral: true )
		);

		RasterBackendCandidateEvaluation evaluation =
			RasterBackendPlanner.Evaluate( candidate, request );

		Assert.Equal( RasterBackendCandidateStatus.Impossible, evaluation.Status );
		Assert.Equal(
			PersistentRasterLifecyclePlanStatus.Impossible,
			evaluation.LifecyclePlan.Status
		);
	}

	[Fact]
	public void SupportedBackendAndIndeterminateLifecycleRequireRuntimeVerification() {
		RasterBackendCandidate candidate = CreateLifecycleCandidate(
			RasterBackendKind.Sixel,
			RasterBackendSupportStatus.Supported,
			PersistentRasterLifecycleSupportStatus.Unknown
		);
		RasterBackendSelectionRequest request = new(
			new PersistentRasterLifecycleRequest( displayEphemeral: true )
		);

		RasterBackendCandidateEvaluation evaluation =
			RasterBackendPlanner.Evaluate( candidate, request );

		Assert.Equal(
			RasterBackendCandidateStatus.RequiresRuntimeVerification,
			evaluation.Status
		);
		Assert.Equal(
			PersistentRasterLifecyclePlanStatus.Indeterminate,
			evaluation.LifecyclePlan.Status
		);
	}

	[Fact]
	public void SupportedBackendAndSatisfiedPlacementAreSatisfied() {
		RasterBackendCandidate candidate = CreatePlacementCandidate(
			RasterBackendKind.KittyGraphics,
			RasterBackendSupportStatus.Supported,
			PersistentRasterLifecycleSupportStatus.Supported,
			PersistentRasterLifecycleSupportStatus.Supported
		);
		RasterBackendSelectionRequest request = CreatePlacementRequest();

		RasterBackendCandidateEvaluation evaluation =
			RasterBackendPlanner.Evaluate( candidate, request );

		Assert.Equal( RasterBackendCandidateStatus.Satisfied, evaluation.Status );
		Assert.Equal(
			PersistentRasterLifecyclePlanStatus.Success,
			evaluation.LifecyclePlan.Status
		);
		PersistentRasterPlacementPlan placementPlan =
			Assert.IsType<PersistentRasterPlacementPlan>( evaluation.PlacementPlan );
		Assert.Equal(
			PersistentRasterPlacementPlanStatus.Satisfied,
			placementPlan.Status
		);
	}

	[Fact]
	public void SupportedBackendAndImpossiblePlacementAreImpossible() {
		RasterBackendCandidate candidate = CreatePlacementCandidate(
			RasterBackendKind.KittyGraphics,
			RasterBackendSupportStatus.Supported,
			PersistentRasterLifecycleSupportStatus.Supported,
			PersistentRasterLifecycleSupportStatus.Unsupported
		);
		RasterBackendSelectionRequest request = CreatePlacementRequest();

		RasterBackendCandidateEvaluation evaluation =
			RasterBackendPlanner.Evaluate( candidate, request );

		Assert.Equal( RasterBackendCandidateStatus.Impossible, evaluation.Status );
		PersistentRasterPlacementPlan placementPlan =
			Assert.IsType<PersistentRasterPlacementPlan>( evaluation.PlacementPlan );
		Assert.Equal(
			PersistentRasterPlacementPlanStatus.Impossible,
			placementPlan.Status
		);
	}

	[Fact]
	public void SupportedBackendAndUnknownPlacementRequireRuntimeVerification() {
		RasterBackendCandidate candidate = CreatePlacementCandidate(
			RasterBackendKind.KittyGraphics,
			RasterBackendSupportStatus.Supported,
			PersistentRasterLifecycleSupportStatus.Supported,
			PersistentRasterLifecycleSupportStatus.Unknown
		);
		RasterBackendSelectionRequest request = CreatePlacementRequest();

		RasterBackendCandidateEvaluation evaluation =
			RasterBackendPlanner.Evaluate( candidate, request );

		Assert.Equal(
			RasterBackendCandidateStatus.RequiresRuntimeVerification,
			evaluation.Status
		);
		PersistentRasterPlacementPlan placementPlan =
			Assert.IsType<PersistentRasterPlacementPlan>( evaluation.PlacementPlan );
		Assert.Equal(
			PersistentRasterPlacementPlanStatus.RequiresRuntimeVerification,
			placementPlan.Status
		);
	}

	[Fact]
	public void SupportedBackendAndIndeterminateLifecycleRetainIndeterminatePlacementPlan() {
		RasterBackendCandidate candidate = CreatePlacementCandidate(
			RasterBackendKind.Sixel,
			RasterBackendSupportStatus.Supported,
			PersistentRasterLifecycleSupportStatus.Unknown,
			PersistentRasterLifecycleSupportStatus.Supported
		);
		RasterBackendSelectionRequest request = CreatePlacementRequest();

		RasterBackendCandidateEvaluation evaluation =
			RasterBackendPlanner.Evaluate( candidate, request );

		Assert.Equal(
			RasterBackendCandidateStatus.RequiresRuntimeVerification,
			evaluation.Status
		);
		Assert.Equal(
			PersistentRasterLifecyclePlanStatus.Indeterminate,
			evaluation.LifecyclePlan.Status
		);
		PersistentRasterPlacementPlan placementPlan =
			Assert.IsType<PersistentRasterPlacementPlan>( evaluation.PlacementPlan );
		Assert.Equal(
			PersistentRasterPlacementPlanStatus.Indeterminate,
			placementPlan.Status
		);
	}

	[Fact]
	public void UnsupportedBackendIsImpossibleEvenWhenSemanticPlansSucceed() {
		RasterBackendCandidate candidate = CreatePlacementCandidate(
			RasterBackendKind.Sixel,
			RasterBackendSupportStatus.Unsupported,
			PersistentRasterLifecycleSupportStatus.Supported,
			PersistentRasterLifecycleSupportStatus.Supported
		);

		RasterBackendCandidateEvaluation evaluation =
			RasterBackendPlanner.Evaluate(
				candidate,
				CreatePlacementRequest()
			);

		Assert.Equal( RasterBackendCandidateStatus.Impossible, evaluation.Status );
		Assert.Equal(
			PersistentRasterLifecyclePlanStatus.Success,
			evaluation.LifecyclePlan.Status
		);
		Assert.Equal(
			PersistentRasterPlacementPlanStatus.Satisfied,
			Assert.IsType<PersistentRasterPlacementPlan>( evaluation.PlacementPlan ).Status
		);
	}

	[Theory]
	[InlineData( RasterBackendSupportStatus.Unknown )]
	[InlineData( RasterBackendSupportStatus.Contradicted )]
	public void UncertainBackendRequiresVerificationEvenWhenRetainedSemanticPlansAreImpossible(
		RasterBackendSupportStatus backendStatus
	) {
		RasterBackendCandidate candidate = CreatePlacementCandidate(
			RasterBackendKind.KittyGraphics,
			backendStatus,
			PersistentRasterLifecycleSupportStatus.Unsupported,
			PersistentRasterLifecycleSupportStatus.Unsupported
		);

		RasterBackendCandidateEvaluation evaluation =
			RasterBackendPlanner.Evaluate(
				candidate,
				CreatePlacementRequest()
			);

		Assert.Equal(
			RasterBackendCandidateStatus.RequiresRuntimeVerification,
			evaluation.Status
		);
		Assert.Equal(
			PersistentRasterLifecyclePlanStatus.Impossible,
			evaluation.LifecyclePlan.Status
		);
		Assert.Equal(
			PersistentRasterPlacementPlanStatus.Impossible,
			Assert.IsType<PersistentRasterPlacementPlan>( evaluation.PlacementPlan ).Status
		);
	}

	[Fact]
	public void RequestedPlacementPlanIsRetainedWhenBackendAvailabilityAlreadyBlocksUse() {
		RasterBackendCandidate candidate = CreatePlacementCandidate(
			RasterBackendKind.Sixel,
			RasterBackendSupportStatus.Unsupported,
			PersistentRasterLifecycleSupportStatus.Supported,
			PersistentRasterLifecycleSupportStatus.Unknown
		);

		RasterBackendCandidateEvaluation evaluation =
			RasterBackendPlanner.Evaluate(
				candidate,
				CreatePlacementRequest()
			);

		Assert.Equal( RasterBackendCandidateStatus.Impossible, evaluation.Status );
		Assert.NotNull( evaluation.PlacementPlan );
		Assert.Equal(
			PersistentRasterPlacementPlanStatus.RequiresRuntimeVerification,
			evaluation.PlacementPlan!.Status
		);
	}

	[Fact]
	public void BackendIdentityDoesNotAlterSemanticEvaluation() {
		RasterBackendSelectionRequest request = CreatePlacementRequest();
		RasterBackendCandidate sixel = CreatePlacementCandidate(
			RasterBackendKind.Sixel,
			RasterBackendSupportStatus.Supported,
			PersistentRasterLifecycleSupportStatus.Supported,
			PersistentRasterLifecycleSupportStatus.Unknown
		);
		RasterBackendCandidate kitty = CreatePlacementCandidate(
			RasterBackendKind.KittyGraphics,
			RasterBackendSupportStatus.Supported,
			PersistentRasterLifecycleSupportStatus.Supported,
			PersistentRasterLifecycleSupportStatus.Unknown
		);

		RasterBackendCandidateEvaluation sixelEvaluation =
			RasterBackendPlanner.Evaluate( sixel, request );
		RasterBackendCandidateEvaluation kittyEvaluation =
			RasterBackendPlanner.Evaluate( kitty, request );

		Assert.Equal( sixelEvaluation.Status, kittyEvaluation.Status );
		Assert.Equal(
			sixelEvaluation.LifecyclePlan.Status,
			kittyEvaluation.LifecyclePlan.Status
		);
		Assert.Equal(
			sixelEvaluation.PlacementPlan!.Status,
			kittyEvaluation.PlacementPlan!.Status
		);
	}

	[Fact]
	public void EvaluationMatchesFrozenPlannerResults() {
		RasterBackendCandidate candidate = CreatePlacementCandidate(
			RasterBackendKind.KittyGraphics,
			RasterBackendSupportStatus.Supported,
			PersistentRasterLifecycleSupportStatus.Supported,
			PersistentRasterLifecycleSupportStatus.Unknown
		);
		RasterBackendSelectionRequest request = CreatePlacementRequest();
		PersistentRasterLifecyclePlan expectedLifecycle =
			PersistentRasterLifecyclePlanner.Plan(
				candidate.LifecycleProfile,
				request.LifecycleRequest
			);
		PersistentRasterPlacementPlan expectedPlacement =
			PersistentRasterPlacementPlanner.Plan(
				expectedLifecycle,
				candidate.PlacementProfile,
				request.PlacementRequest!
			);

		RasterBackendCandidateEvaluation evaluation =
			RasterBackendPlanner.Evaluate( candidate, request );

		Assert.Equal( expectedLifecycle.Status, evaluation.LifecyclePlan.Status );
		Assert.Equal( expectedLifecycle.Steps.Count, evaluation.LifecyclePlan.Steps.Count );
		Assert.Equal( expectedLifecycle.Issues.Count, evaluation.LifecyclePlan.Issues.Count );
		Assert.Equal( expectedPlacement.Status, evaluation.PlacementPlan!.Status );
		Assert.Equal(
			expectedPlacement.Requirements.Count,
			evaluation.PlacementPlan.Requirements.Count
		);
		Assert.Equal(
			expectedPlacement.Issues.Count,
			evaluation.PlacementPlan.Issues.Count
		);
	}

	private static RasterBackendSelectionRequest CreatePlacementRequest() {
		return new RasterBackendSelectionRequest(
			new PersistentRasterLifecycleRequest( placementCount: 1 ),
			new PersistentRasterPlacementRequest(
				requireSourceRectangle: true
			)
		);
	}

	private static RasterBackendCandidate CreateLifecycleCandidate(
		RasterBackendKind backend,
		RasterBackendSupportStatus backendStatus,
		PersistentRasterLifecycleSupportStatus lifecycleStatus
	) {
		return new RasterBackendCandidate(
			CreateBackendProfile( backend, backendStatus ),
			CreateLifecycleProfile(
				PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
				lifecycleStatus
			),
			PersistentRasterPlacementClassifier.Classify(
				Array.Empty<PersistentRasterPlacementEvidence>()
			)
		);
	}

	private static RasterBackendCandidate CreatePlacementCandidate(
		RasterBackendKind backend,
		RasterBackendSupportStatus backendStatus,
		PersistentRasterLifecycleSupportStatus lifecycleStatus,
		PersistentRasterLifecycleSupportStatus placementStatus
	) {
		return new RasterBackendCandidate(
			CreateBackendProfile( backend, backendStatus ),
			CreateLifecycleProfile(
				PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
				lifecycleStatus
			),
			CreatePlacementProfile( placementStatus )
		);
	}

	private static RasterBackendProfile CreateBackendProfile(
		RasterBackendKind backend,
		RasterBackendSupportStatus status
	) {
		IEnumerable<RasterBackendEvidence> evidence = status switch {
			RasterBackendSupportStatus.Unknown => Array.Empty<RasterBackendEvidence>(),
			RasterBackendSupportStatus.Supported => [
				new RasterBackendEvidence(
					backend,
					true,
					RasterBackendEvidenceKind.Verified,
					"rb03-backend-positive",
					0
				),
			],
			RasterBackendSupportStatus.Unsupported => [
				new RasterBackendEvidence(
					backend,
					false,
					RasterBackendEvidenceKind.Verified,
					"rb03-backend-negative",
					0
				),
			],
			RasterBackendSupportStatus.Contradicted => [
				new RasterBackendEvidence(
					backend,
					true,
					RasterBackendEvidenceKind.Verified,
					"rb03-backend-positive",
					0
				),
				new RasterBackendEvidence(
					backend,
					false,
					RasterBackendEvidenceKind.Verified,
					"rb03-backend-negative",
					1
				),
			],
			_ => throw new ArgumentOutOfRangeException( nameof( status ) ),
		};

		return RasterBackendClassifier.Classify( backend, evidence );
	}

	private static PersistentRasterLifecycleProfile CreateLifecycleProfile(
		PersistentRasterLifecycleEvidenceSubject subject,
		PersistentRasterLifecycleSupportStatus status
	) {
		IEnumerable<PersistentRasterLifecycleEvidence> evidence = status switch {
			PersistentRasterLifecycleSupportStatus.Unknown =>
				Array.Empty<PersistentRasterLifecycleEvidence>(),
			PersistentRasterLifecycleSupportStatus.Supported => [
				new PersistentRasterLifecycleEvidence(
					subject,
					true,
					PersistentRasterLifecycleEvidenceKind.Verified,
					"rb03-lifecycle-positive",
					0
				),
			],
			PersistentRasterLifecycleSupportStatus.Unsupported => [
				new PersistentRasterLifecycleEvidence(
					subject,
					false,
					PersistentRasterLifecycleEvidenceKind.Verified,
					"rb03-lifecycle-negative",
					0
				),
			],
			PersistentRasterLifecycleSupportStatus.Contradicted => [
				new PersistentRasterLifecycleEvidence(
					subject,
					true,
					PersistentRasterLifecycleEvidenceKind.Verified,
					"rb03-lifecycle-positive",
					0
				),
				new PersistentRasterLifecycleEvidence(
					subject,
					false,
					PersistentRasterLifecycleEvidenceKind.Verified,
					"rb03-lifecycle-negative",
					1
				),
			],
			_ => throw new ArgumentOutOfRangeException( nameof( status ) ),
		};

		return PersistentRasterLifecycleClassifier.Classify( evidence );
	}

	private static PersistentRasterPlacementProfile CreatePlacementProfile(
		PersistentRasterLifecycleSupportStatus status
	) {
		IEnumerable<PersistentRasterPlacementEvidence> evidence = status switch {
			PersistentRasterLifecycleSupportStatus.Unknown =>
				Array.Empty<PersistentRasterPlacementEvidence>(),
			PersistentRasterLifecycleSupportStatus.Supported => [
				new PersistentRasterPlacementEvidence(
					PersistentRasterPlacementSubject.SourceRectangle,
					true,
					PersistentRasterPlacementEvidenceKind.Verified,
					"rb03-placement-positive",
					0
				),
			],
			PersistentRasterLifecycleSupportStatus.Unsupported => [
				new PersistentRasterPlacementEvidence(
					PersistentRasterPlacementSubject.SourceRectangle,
					false,
					PersistentRasterPlacementEvidenceKind.Verified,
					"rb03-placement-negative",
					0
				),
			],
			PersistentRasterLifecycleSupportStatus.Contradicted => [
				new PersistentRasterPlacementEvidence(
					PersistentRasterPlacementSubject.SourceRectangle,
					true,
					PersistentRasterPlacementEvidenceKind.Verified,
					"rb03-placement-positive",
					0
				),
				new PersistentRasterPlacementEvidence(
					PersistentRasterPlacementSubject.SourceRectangle,
					false,
					PersistentRasterPlacementEvidenceKind.Verified,
					"rb03-placement-negative",
					1
				),
			],
			_ => throw new ArgumentOutOfRangeException( nameof( status ) ),
		};

		return PersistentRasterPlacementClassifier.Classify( evidence );
	}
}
