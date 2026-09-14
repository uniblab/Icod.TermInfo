using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RB05RuntimeIntegrationCompositionTests {
	[Fact]
	public void CompositionRejectsNullBackendProfile() {
		PersistentRasterRuntimeIntegrationResult integration =
			CreateSuccessfulIntegration( "null-backend" );

		Assert.Throws<ArgumentNullException>(
			() => new RasterBackendCandidate(
				null!,
				integration
			)
		);
	}

	[Fact]
	public void CompositionRejectsNullIntegrationResult() {
		RasterBackendProfile backendProfile =
			CreateSupportedBackendProfile( RasterBackendKind.Sixel );

		Assert.Throws<ArgumentNullException>(
			() => new RasterBackendCandidate(
				backendProfile,
				null!
			)
		);
	}

	[Fact]
	public void CompositionRetainsExactIntegratedProfiles() {
		RasterBackendProfile backendProfile =
			CreateSupportedBackendProfile( RasterBackendKind.KittyGraphics );
		PersistentRasterRuntimeIntegrationResult integration =
			CreateSuccessfulIntegration( "exact" );

		RasterBackendCandidate candidate = new(
			backendProfile,
			integration
		);

		Assert.Same( backendProfile, candidate.BackendProfile );
		Assert.Same( integration.LifecycleProfile, candidate.LifecycleProfile );
		Assert.Same( integration.PlacementProfile, candidate.PlacementProfile );
	}

	[Fact]
	public void FailedIntegrationCanBePackagedWithoutElevatingBackendAvailability() {
		RasterBackendProfile backendProfile = RasterBackendClassifier.Classify(
			RasterBackendKind.KittyGraphics,
			Array.Empty<RasterBackendEvidence>()
		);
		PersistentRasterRuntimeIntegrationResult integration =
			CreateLifecycleOrdinalFailureIntegration();

		Assert.False( integration.Succeeded );
		RasterBackendCandidate candidate = new(
			backendProfile,
			integration
		);

		Assert.Same( backendProfile, candidate.BackendProfile );
		Assert.Equal(
			RasterBackendSupportStatus.Unknown,
			candidate.BackendProfile.Status
		);
		Assert.Same( integration.LifecycleProfile, candidate.LifecycleProfile );
		Assert.Same( integration.PlacementProfile, candidate.PlacementProfile );

		RasterBackendCandidateEvaluation evaluation = RasterBackendPlanner.Evaluate(
			candidate,
			new RasterBackendSelectionRequest(
				new PersistentRasterLifecycleRequest( displayEphemeral: true )
			)
		);
		Assert.Equal(
			RasterBackendCandidateStatus.RequiresRuntimeVerification,
			evaluation.Status
		);
	}

	[Fact]
	public void CompositionIsEquivalentToManualCandidateConstruction() {
		RasterBackendProfile backendProfile =
			CreateSupportedBackendProfile( RasterBackendKind.Sixel );
		PersistentRasterRuntimeIntegrationResult integration =
			CreateSuccessfulIntegration( "equivalence" );
		RasterBackendCandidate manual = new(
			backendProfile,
			integration.LifecycleProfile,
			integration.PlacementProfile
		);
		RasterBackendCandidate composed = new(
			backendProfile,
			integration
		);
		RasterBackendSelectionRequest request = new(
			new PersistentRasterLifecycleRequest( displayEphemeral: true )
		);

		RasterBackendCandidateEvaluation manualEvaluation =
			RasterBackendPlanner.Evaluate( manual, request );
		RasterBackendCandidateEvaluation composedEvaluation =
			RasterBackendPlanner.Evaluate( composed, request );
		RasterBackendSelectionPlan manualPlan = RasterBackendPlanner.Plan(
			[ manual ],
			request
		);
		RasterBackendSelectionPlan composedPlan = RasterBackendPlanner.Plan(
			[ composed ],
			request
		);

		Assert.Equal( manualEvaluation.Status, composedEvaluation.Status );
		Assert.Equal( manualPlan.Status, composedPlan.Status );
		Assert.Equal( manualPlan.SelectedBackend, composedPlan.SelectedBackend );
	}

	[Fact]
	public void SeparateBackendIntegrationContextsRemainIndependent() {
		PersistentRasterRuntimeIntegrationResult sixelIntegration =
			CreateSuccessfulIntegration( "sixel" );
		PersistentRasterRuntimeIntegrationResult kittyIntegration =
			CreateSuccessfulIntegration( "kitty" );
		RasterBackendCandidate sixel = new(
			CreateSupportedBackendProfile( RasterBackendKind.Sixel ),
			sixelIntegration
		);
		RasterBackendCandidate kitty = new(
			CreateSupportedBackendProfile( RasterBackendKind.KittyGraphics ),
			kittyIntegration
		);

		Assert.NotSame( sixelIntegration, kittyIntegration );
		Assert.Same( sixelIntegration.LifecycleProfile, sixel.LifecycleProfile );
		Assert.Same( kittyIntegration.LifecycleProfile, kitty.LifecycleProfile );
		Assert.Same( sixelIntegration.PlacementProfile, sixel.PlacementProfile );
		Assert.Same( kittyIntegration.PlacementProfile, kitty.PlacementProfile );
		Assert.NotSame( sixel.LifecycleProfile, kitty.LifecycleProfile );
		Assert.NotSame( sixel.PlacementProfile, kitty.PlacementProfile );
	}

	private static RasterBackendProfile CreateSupportedBackendProfile(
		RasterBackendKind backend
	) {
		return RasterBackendClassifier.Classify(
			backend,
			[
				new RasterBackendEvidence(
					backend,
					true,
					RasterBackendEvidenceKind.Verified,
					"rb05-backend",
					0
				),
			]
		);
	}

	private static PersistentRasterRuntimeIntegrationResult CreateSuccessfulIntegration(
		string sourceLabel
	) {
		PersistentRasterLifecycleProfile lifecycleProfile =
			PersistentRasterLifecycleClassifier.Classify(
				Array.Empty<PersistentRasterLifecycleEvidence>()
			);
		PersistentRasterPlacementProfile placementProfile =
			PersistentRasterPlacementClassifier.Classify(
				Array.Empty<PersistentRasterPlacementEvidence>()
			);
		PersistentRasterRuntimeObservationSet observations = new(
			[
				new PersistentRasterRuntimeLifecycleObservation(
					PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
					PersistentRasterRuntimeObservationOutcome.Supported,
					sourceLabel,
					0
				),
			],
			[
				new PersistentRasterRuntimePlacementObservation(
					PersistentRasterPlacementSubject.SourceRectangle,
					PersistentRasterRuntimeObservationOutcome.Supported,
					sourceLabel,
					0
				),
			]
		);

		return PersistentRasterRuntimeEvidenceIntegrator.Integrate(
			lifecycleProfile,
			placementProfile,
			observations
		);
	}

	private static PersistentRasterRuntimeIntegrationResult
		CreateLifecycleOrdinalFailureIntegration() {
		PersistentRasterLifecycleProfile lifecycleProfile =
			PersistentRasterLifecycleClassifier.Classify(
				[
					new PersistentRasterLifecycleEvidence(
						PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
						true,
						PersistentRasterLifecycleEvidenceKind.Verified,
						"rb05-existing",
						int.MaxValue
					),
				]
			);
		PersistentRasterPlacementProfile placementProfile =
			PersistentRasterPlacementClassifier.Classify(
				Array.Empty<PersistentRasterPlacementEvidence>()
			);
		PersistentRasterRuntimeObservationSet observations = new(
			[
				new PersistentRasterRuntimeLifecycleObservation(
					PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
					PersistentRasterRuntimeObservationOutcome.Supported,
					"rb05-runtime",
					0
				),
			],
			Array.Empty<PersistentRasterRuntimePlacementObservation>()
		);

		return PersistentRasterRuntimeEvidenceIntegrator.Integrate(
			lifecycleProfile,
			placementProfile,
			observations
		);
	}
}
