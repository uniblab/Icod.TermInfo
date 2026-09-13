using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RE05PersistentRasterRuntimeReplanningTests {
	[Fact]
	public void CreateLifecyclePlanDelegatesUsingStrengthenedLifecycleProfile() {
		PersistentRasterRuntimeIntegrationResult integration =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				CreateEmptyLifecycleProfile(),
				CreateEmptyPlacementProfile(),
				new PersistentRasterRuntimeObservationSet(
					new[] {
						new PersistentRasterRuntimeLifecycleObservation(
							PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
							PersistentRasterRuntimeObservationOutcome.Supported,
							"runtime-upload",
							0
						),
						new PersistentRasterRuntimeLifecycleObservation(
							PersistentRasterLifecycleEvidenceSubject.AcknowledgedUpload,
							PersistentRasterRuntimeObservationOutcome.Supported,
							"runtime-acknowledged-upload",
							1
						),
						new PersistentRasterRuntimeLifecycleObservation(
							PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
							PersistentRasterRuntimeObservationOutcome.Supported,
							"runtime-placement-creation",
							2
						),
						new PersistentRasterRuntimeLifecycleObservation(
							PersistentRasterLifecycleEvidenceSubject.MultiplePlacements,
							PersistentRasterRuntimeObservationOutcome.Supported,
							"runtime-multiple-placements",
							3
						),
					},
					Array.Empty<PersistentRasterRuntimePlacementObservation>()
				)
			);
		PersistentRasterLifecycleRequest request = new(
			uploadResource: true,
			placementCount: 2,
			requireAcknowledgedUpload: true
		);
		PersistentRasterLifecyclePlan expected =
			PersistentRasterLifecyclePlanner.Plan(
				integration.LifecycleProfile,
				request
			);

		PersistentRasterLifecyclePlan actual =
			integration.CreateLifecyclePlan( request );

		Assert.Equal( PersistentRasterLifecyclePlanStatus.Success, actual.Status );
		AssertLifecyclePlansEquivalent( expected, actual );
	}

	[Fact]
	public void CreateLifecyclePlanRejectsNullRequest() {
		PersistentRasterRuntimeIntegrationResult integration =
			CreateEmptyIntegrationResult();

		Assert.Throws<ArgumentNullException>(
			() => integration.CreateLifecyclePlan( null! )
		);
	}

	[Fact]
	public void CreatePlacementPlanProducesSatisfiedResultByDirectComposition() {
		PersistentRasterRuntimeIntegrationResult integration =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				CreateEmptyLifecycleProfile(),
				CreateEmptyPlacementProfile(),
				new PersistentRasterRuntimeObservationSet(
					new[] {
						new PersistentRasterRuntimeLifecycleObservation(
							PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
							PersistentRasterRuntimeObservationOutcome.Supported,
							"runtime-lifecycle",
							0
						),
					},
					new[] {
						new PersistentRasterRuntimePlacementObservation(
							PersistentRasterPlacementSubject.SourceRectangle,
							PersistentRasterRuntimeObservationOutcome.Supported,
							"runtime-placement",
							1
						),
					}
				)
			);
		PersistentRasterLifecycleRequest lifecycleRequest = new(
			placementCount: 1
		);
		PersistentRasterPlacementRequest placementRequest = new(
			requireSourceRectangle: true
		);

		PersistentRasterPlacementPlan expected = DirectPlacementPlan(
			integration,
			lifecycleRequest,
			placementRequest
		);
		PersistentRasterPlacementPlan actual = integration.CreatePlacementPlan(
			lifecycleRequest,
			placementRequest
		);

		Assert.Equal( PersistentRasterPlacementPlanStatus.Satisfied, actual.Status );
		AssertPlacementPlansEquivalent( expected, actual );
	}

	[Fact]
	public void CreatePlacementPlanProducesRequiresRuntimeVerificationByDirectComposition() {
		PersistentRasterLifecycleProfile lifecycleProfile =
			PersistentRasterLifecycleClassifier.Classify(
				new[] {
					new PersistentRasterLifecycleEvidence(
						PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
						true,
						PersistentRasterLifecycleEvidenceKind.Verified,
						"verified-display",
						0
					),
				}
			);
		PersistentRasterRuntimeIntegrationResult integration =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				lifecycleProfile,
				CreateEmptyPlacementProfile(),
				new PersistentRasterRuntimeObservationSet(
					Array.Empty<PersistentRasterRuntimeLifecycleObservation>(),
					Array.Empty<PersistentRasterRuntimePlacementObservation>()
				)
			);
		PersistentRasterLifecycleRequest lifecycleRequest = new(
			displayEphemeral: true
		);
		PersistentRasterPlacementRequest placementRequest = new(
			requireSourceRectangle: true
		);

		PersistentRasterPlacementPlan expected = DirectPlacementPlan(
			integration,
			lifecycleRequest,
			placementRequest
		);
		PersistentRasterPlacementPlan actual = integration.CreatePlacementPlan(
			lifecycleRequest,
			placementRequest
		);

		Assert.Equal(
			PersistentRasterPlacementPlanStatus.RequiresRuntimeVerification,
			actual.Status
		);
		AssertPlacementPlansEquivalent( expected, actual );
	}

	[Fact]
	public void CreatePlacementPlanProducesIndeterminateByDirectComposition() {
		PersistentRasterPlacementProfile placementProfile =
			PersistentRasterPlacementClassifier.Classify(
				new[] {
					new PersistentRasterPlacementEvidence(
						PersistentRasterPlacementSubject.SignedZOrder,
						true,
						PersistentRasterPlacementEvidenceKind.Verified,
						"verified-z-order",
						0
					),
				}
			);
		PersistentRasterRuntimeIntegrationResult integration =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				CreateEmptyLifecycleProfile(),
				placementProfile,
				new PersistentRasterRuntimeObservationSet(
					Array.Empty<PersistentRasterRuntimeLifecycleObservation>(),
					Array.Empty<PersistentRasterRuntimePlacementObservation>()
				)
			);
		PersistentRasterLifecycleRequest lifecycleRequest = new(
			displayEphemeral: true
		);
		PersistentRasterPlacementRequest placementRequest = new(
			requireSignedZOrder: true
		);

		PersistentRasterPlacementPlan expected = DirectPlacementPlan(
			integration,
			lifecycleRequest,
			placementRequest
		);
		PersistentRasterPlacementPlan actual = integration.CreatePlacementPlan(
			lifecycleRequest,
			placementRequest
		);

		Assert.Equal(
			PersistentRasterPlacementPlanStatus.Indeterminate,
			actual.Status
		);
		AssertPlacementPlansEquivalent( expected, actual );
	}

	[Fact]
	public void CreatePlacementPlanProducesImpossibleByDirectComposition() {
		PersistentRasterLifecycleProfile lifecycleProfile =
			PersistentRasterLifecycleClassifier.Classify(
				new[] {
					new PersistentRasterLifecycleEvidence(
						PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
						false,
						PersistentRasterLifecycleEvidenceKind.Verified,
						"verified-display-negative",
						0
					),
				}
			);
		PersistentRasterPlacementProfile placementProfile =
			PersistentRasterPlacementClassifier.Classify(
				new[] {
					new PersistentRasterPlacementEvidence(
						PersistentRasterPlacementSubject.SourceRectangle,
						true,
						PersistentRasterPlacementEvidenceKind.Verified,
						"verified-source-rectangle",
						0
					),
				}
			);
		PersistentRasterRuntimeIntegrationResult integration =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				lifecycleProfile,
				placementProfile,
				new PersistentRasterRuntimeObservationSet(
					Array.Empty<PersistentRasterRuntimeLifecycleObservation>(),
					Array.Empty<PersistentRasterRuntimePlacementObservation>()
				)
			);
		PersistentRasterLifecycleRequest lifecycleRequest = new(
			displayEphemeral: true
		);
		PersistentRasterPlacementRequest placementRequest = new(
			requireSourceRectangle: true
		);

		PersistentRasterPlacementPlan expected = DirectPlacementPlan(
			integration,
			lifecycleRequest,
			placementRequest
		);
		PersistentRasterPlacementPlan actual = integration.CreatePlacementPlan(
			lifecycleRequest,
			placementRequest
		);

		Assert.Equal( PersistentRasterPlacementPlanStatus.Impossible, actual.Status );
		AssertPlacementPlansEquivalent( expected, actual );
	}

	[Fact]
	public void CreatePlacementPlanRejectsNullRequests() {
		PersistentRasterRuntimeIntegrationResult integration =
			CreateEmptyIntegrationResult();
		PersistentRasterLifecycleRequest lifecycleRequest = new(
			displayEphemeral: true
		);
		PersistentRasterPlacementRequest placementRequest = new(
			requireSourceRectangle: true
		);

		Assert.Throws<ArgumentNullException>(
			() => integration.CreatePlacementPlan( null!, placementRequest )
		);
		Assert.Throws<ArgumentNullException>(
			() => integration.CreatePlacementPlan( lifecycleRequest, null! )
		);
	}

	private static PersistentRasterRuntimeIntegrationResult CreateEmptyIntegrationResult() {
		return PersistentRasterRuntimeEvidenceIntegrator.Integrate(
			CreateEmptyLifecycleProfile(),
			CreateEmptyPlacementProfile(),
			new PersistentRasterRuntimeObservationSet(
				Array.Empty<PersistentRasterRuntimeLifecycleObservation>(),
				Array.Empty<PersistentRasterRuntimePlacementObservation>()
			)
		);
	}

	private static PersistentRasterLifecycleProfile CreateEmptyLifecycleProfile() {
		return PersistentRasterLifecycleClassifier.Classify(
			Array.Empty<PersistentRasterLifecycleEvidence>()
		);
	}

	private static PersistentRasterPlacementProfile CreateEmptyPlacementProfile() {
		return PersistentRasterPlacementClassifier.Classify(
			Array.Empty<PersistentRasterPlacementEvidence>()
		);
	}

	private static PersistentRasterPlacementPlan DirectPlacementPlan(
		PersistentRasterRuntimeIntegrationResult integration,
		PersistentRasterLifecycleRequest lifecycleRequest,
		PersistentRasterPlacementRequest placementRequest
	) {
		PersistentRasterLifecyclePlan lifecyclePlan =
			PersistentRasterLifecyclePlanner.Plan(
				integration.LifecycleProfile,
				lifecycleRequest
			);
		return PersistentRasterPlacementPlanner.Plan(
			lifecyclePlan,
			integration.PlacementProfile,
			placementRequest
		);
	}

	private static void AssertLifecyclePlansEquivalent(
		PersistentRasterLifecyclePlan expected,
		PersistentRasterLifecyclePlan actual
	) {
		Assert.Equal( expected.Status, actual.Status );
		Assert.Equal(
			expected.RequiresRuntimeVerification,
			actual.RequiresRuntimeVerification
		);
		Assert.Equal(
			expected.Steps
				.Select(
					item => (
						item.SequenceIndex,
						item.Operation,
						item.RequiresRuntimeVerification
					)
				),
			actual.Steps
				.Select(
					item => (
						item.SequenceIndex,
						item.Operation,
						item.RequiresRuntimeVerification
					)
				)
		);
		Assert.Equal(
			expected.Issues
				.Select(
					item => (
						item.Operation,
						item.Subject,
						item.SupportStatus,
						item.RequiresRuntimeVerification
					)
				),
			actual.Issues
				.Select(
					item => (
						item.Operation,
						item.Subject,
						item.SupportStatus,
						item.RequiresRuntimeVerification
					)
				)
		);
	}

	private static void AssertPlacementPlansEquivalent(
		PersistentRasterPlacementPlan expected,
		PersistentRasterPlacementPlan actual
	) {
		Assert.Equal( expected.Status, actual.Status );
		Assert.Equal( expected.LifecycleStatus, actual.LifecycleStatus );
		Assert.Equal(
			expected.Requirements
				.Select(
					item => (
						item.Subject,
						item.SupportStatus,
						item.RequiresRuntimeVerification
					)
				),
			actual.Requirements
				.Select(
					item => (
						item.Subject,
						item.SupportStatus,
						item.RequiresRuntimeVerification
					)
				)
		);
		Assert.Equal(
			expected.Issues
				.Select(
					item => (
						item.Subject,
						item.SupportStatus,
						item.RequiresRuntimeVerification
					)
				),
			actual.Issues
				.Select(
					item => (
						item.Subject,
						item.SupportStatus,
						item.RequiresRuntimeVerification
					)
				)
		);
	}
}
