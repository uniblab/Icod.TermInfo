using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RB04RasterBackendSelectionTests {
	[Fact]
	public void PlanRejectsNullCandidateSequence() {
		RasterBackendSelectionRequest request = CreateDisplayRequest();

		Assert.Throws<ArgumentNullException>(
			() => RasterBackendPlanner.Plan(
				null!,
				request
			)
		);
	}

	[Fact]
	public void PlanRejectsNullRequest() {
		RasterBackendCandidate candidate = CreateDisplayCandidate(
			RasterBackendKind.Sixel,
			RasterBackendSupportStatus.Supported
		);

		Assert.Throws<ArgumentNullException>(
			() => RasterBackendPlanner.Plan(
				[ candidate ],
				null!
			)
		);
	}

	[Fact]
	public void PlanRejectsEmptyCandidateSequence() {
		Assert.Throws<ArgumentException>(
			() => RasterBackendPlanner.Plan(
				Array.Empty<RasterBackendCandidate>(),
				CreateDisplayRequest()
			)
		);
	}

	[Fact]
	public void PlanRejectsNullCandidateElement() {
		Assert.Throws<ArgumentException>(
			() => RasterBackendPlanner.Plan(
				new RasterBackendCandidate[] { null! },
				CreateDisplayRequest()
			)
		);
	}

	[Fact]
	public void PlanRejectsDuplicateBackendCandidates() {
		RasterBackendCandidate first = CreateDisplayCandidate(
			RasterBackendKind.Sixel,
			RasterBackendSupportStatus.Supported
		);
		RasterBackendCandidate second = CreateDisplayCandidate(
			RasterBackendKind.Sixel,
			RasterBackendSupportStatus.Unsupported
		);

		Assert.Throws<ArgumentException>(
			() => RasterBackendPlanner.Plan(
				[ first, second ],
				CreateDisplayRequest()
			)
		);
	}

	[Fact]
	public void PlanRejectsMoreThanClosedBackendBound() {
		RasterBackendCandidate sixel = CreateDisplayCandidate(
			RasterBackendKind.Sixel,
			RasterBackendSupportStatus.Supported
		);
		RasterBackendCandidate kitty = CreateDisplayCandidate(
			RasterBackendKind.KittyGraphics,
			RasterBackendSupportStatus.Supported
		);

		Assert.Throws<ArgumentException>(
			() => RasterBackendPlanner.Plan(
				[ sixel, kitty, sixel ],
				CreateDisplayRequest()
			)
		);
	}

	[Fact]
	public void NullOptionsMeanNoPreference() {
		RasterBackendCandidate sixel = CreateDisplayCandidate(
			RasterBackendKind.Sixel,
			RasterBackendSupportStatus.Supported
		);
		RasterBackendSelectionRequest request = CreateDisplayRequest();

		RasterBackendSelectionPlan implicitDefaults =
			RasterBackendPlanner.Plan(
				[ sixel ],
				request,
				null
			);
		RasterBackendSelectionPlan explicitDefaults =
			RasterBackendPlanner.Plan(
				[ sixel ],
				request,
				new RasterBackendSelectionOptions()
			);

		Assert.Equal( explicitDefaults.Status, implicitDefaults.Status );
		Assert.Equal( explicitDefaults.SelectedBackend, implicitDefaults.SelectedBackend );
		Assert.Empty( implicitDefaults.Options.PreferenceOrder );
		Assert.Empty( explicitDefaults.Options.PreferenceOrder );
	}

	[Fact]
	public void OneSatisfiedCandidateWithoutPreferenceIsSelected() {
		RasterBackendCandidate candidate = CreateDisplayCandidate(
			RasterBackendKind.Sixel,
			RasterBackendSupportStatus.Supported
		);

		RasterBackendSelectionPlan plan = RasterBackendPlanner.Plan(
			[ candidate ],
			CreateDisplayRequest()
		);

		Assert.Equal( RasterBackendSelectionStatus.Selected, plan.Status );
		Assert.Equal( RasterBackendKind.Sixel, plan.SelectedBackend );
		RasterBackendCandidateEvaluation evaluation =
			Assert.Single( plan.CandidateEvaluations );
		Assert.Equal( RasterBackendCandidateStatus.Satisfied, evaluation.Status );
	}

	[Fact]
	public void OneUnverifiedCandidateWithoutPreferenceRequiresRuntimeVerification() {
		RasterBackendCandidate candidate = CreateDisplayCandidate(
			RasterBackendKind.KittyGraphics,
			RasterBackendSupportStatus.Unknown
		);

		RasterBackendSelectionPlan plan = RasterBackendPlanner.Plan(
			[ candidate ],
			CreateDisplayRequest()
		);

		Assert.Equal(
			RasterBackendSelectionStatus.RequiresRuntimeVerification,
			plan.Status
		);
		Assert.Null( plan.SelectedBackend );
	}

	[Fact]
	public void OneImpossibleCandidateWithoutPreferenceIsImpossible() {
		RasterBackendCandidate candidate = CreateDisplayCandidate(
			RasterBackendKind.Sixel,
			RasterBackendSupportStatus.Unsupported
		);

		RasterBackendSelectionPlan plan = RasterBackendPlanner.Plan(
			[ candidate ],
			CreateDisplayRequest()
		);

		Assert.Equal( RasterBackendSelectionStatus.Impossible, plan.Status );
		Assert.Null( plan.SelectedBackend );
	}

	[Fact]
	public void TwoImpossibleCandidatesWithoutPreferenceAreImpossible() {
		RasterBackendSelectionPlan plan = RasterBackendPlanner.Plan(
			[
				CreateDisplayCandidate(
					RasterBackendKind.KittyGraphics,
					RasterBackendSupportStatus.Unsupported
				),
				CreateDisplayCandidate(
					RasterBackendKind.Sixel,
					RasterBackendSupportStatus.Unsupported
				),
			],
			CreateDisplayRequest()
		);

		Assert.Equal( RasterBackendSelectionStatus.Impossible, plan.Status );
		Assert.Null( plan.SelectedBackend );
	}

	[Fact]
	public void TwoSatisfiedCandidatesWithoutPreferenceRequirePreference() {
		RasterBackendSelectionPlan plan = RasterBackendPlanner.Plan(
			[
				CreateDisplayCandidate(
					RasterBackendKind.KittyGraphics,
					RasterBackendSupportStatus.Supported
				),
				CreateDisplayCandidate(
					RasterBackendKind.Sixel,
					RasterBackendSupportStatus.Supported
				),
			],
			CreateDisplayRequest()
		);

		Assert.Equal(
			RasterBackendSelectionStatus.RequiresPreference,
			plan.Status
		);
		Assert.Null( plan.SelectedBackend );
	}

	[Fact]
	public void SatisfiedAndUnverifiedCandidatesWithoutPreferenceRequirePreference() {
		RasterBackendSelectionPlan plan = RasterBackendPlanner.Plan(
			[
				CreateDisplayCandidate(
					RasterBackendKind.Sixel,
					RasterBackendSupportStatus.Supported
				),
				CreateDisplayCandidate(
					RasterBackendKind.KittyGraphics,
					RasterBackendSupportStatus.Unknown
				),
			],
			CreateDisplayRequest()
		);

		Assert.Equal(
			RasterBackendSelectionStatus.RequiresPreference,
			plan.Status
		);
		Assert.Null( plan.SelectedBackend );
	}

	[Fact]
	public void InputPermutationDoesNotChangeCanonicalEvaluationOrderOrResult() {
		RasterBackendCandidate sixel = CreateDisplayCandidate(
			RasterBackendKind.Sixel,
			RasterBackendSupportStatus.Supported
		);
		RasterBackendCandidate kitty = CreateDisplayCandidate(
			RasterBackendKind.KittyGraphics,
			RasterBackendSupportStatus.Unknown
		);
		RasterBackendSelectionRequest request = CreateDisplayRequest();

		RasterBackendSelectionPlan forward = RasterBackendPlanner.Plan(
			[ sixel, kitty ],
			request
		);
		RasterBackendSelectionPlan reverse = RasterBackendPlanner.Plan(
			[ kitty, sixel ],
			request
		);

		Assert.Equal( forward.Status, reverse.Status );
		Assert.Equal( forward.SelectedBackend, reverse.SelectedBackend );
		Assert.Equal(
			new[] {
				RasterBackendKind.Sixel,
				RasterBackendKind.KittyGraphics,
			},
			forward.CandidateEvaluations.Select(
				item => item.Candidate.BackendProfile.Backend
			)
		);
		Assert.Equal(
			forward.CandidateEvaluations.Select(
				item => item.Candidate.BackendProfile.Backend
			),
			reverse.CandidateEvaluations.Select(
				item => item.Candidate.BackendProfile.Backend
			)
		);
		Assert.Equal(
			forward.CandidateEvaluations.Select( item => item.Status ),
			reverse.CandidateEvaluations.Select( item => item.Status )
		);
	}

	[Fact]
	public void ExplicitPreferenceSkipsImpossibleAndSelectsNextSatisfied() {
		RasterBackendCandidate kitty = CreateDisplayCandidate(
			RasterBackendKind.KittyGraphics,
			RasterBackendSupportStatus.Unsupported
		);
		RasterBackendCandidate sixel = CreateDisplayCandidate(
			RasterBackendKind.Sixel,
			RasterBackendSupportStatus.Supported
		);
		RasterBackendSelectionOptions options = new(
			[
				RasterBackendKind.KittyGraphics,
				RasterBackendKind.Sixel,
			]
		);

		RasterBackendSelectionPlan plan = RasterBackendPlanner.Plan(
			[ sixel, kitty ],
			CreateDisplayRequest(),
			options
		);

		Assert.Equal( RasterBackendSelectionStatus.Selected, plan.Status );
		Assert.Equal( RasterBackendKind.Sixel, plan.SelectedBackend );
		Assert.Same( options, plan.Options );
	}

	[Fact]
	public void ExplicitPreferenceStopsAtUnverifiedPreferredCandidate() {
		RasterBackendCandidate kitty = CreateDisplayCandidate(
			RasterBackendKind.KittyGraphics,
			RasterBackendSupportStatus.Unknown
		);
		RasterBackendCandidate sixel = CreateDisplayCandidate(
			RasterBackendKind.Sixel,
			RasterBackendSupportStatus.Supported
		);

		RasterBackendSelectionPlan plan = RasterBackendPlanner.Plan(
			[ sixel, kitty ],
			CreateDisplayRequest(),
			new RasterBackendSelectionOptions(
				[
					RasterBackendKind.KittyGraphics,
					RasterBackendKind.Sixel,
				]
			)
		);

		Assert.Equal(
			RasterBackendSelectionStatus.RequiresRuntimeVerification,
			plan.Status
		);
		Assert.Null( plan.SelectedBackend );
	}

	[Fact]
	public void ExplicitPreferenceWithAllImpossibleCandidatesIsImpossible() {
		RasterBackendSelectionPlan plan = RasterBackendPlanner.Plan(
			[
				CreateDisplayCandidate(
					RasterBackendKind.Sixel,
					RasterBackendSupportStatus.Unsupported
				),
				CreateDisplayCandidate(
					RasterBackendKind.KittyGraphics,
					RasterBackendSupportStatus.Unsupported
				),
			],
			CreateDisplayRequest(),
			new RasterBackendSelectionOptions(
				[
					RasterBackendKind.KittyGraphics,
					RasterBackendKind.Sixel,
				]
			)
		);

		Assert.Equal( RasterBackendSelectionStatus.Impossible, plan.Status );
		Assert.Null( plan.SelectedBackend );
	}

	[Fact]
	public void ExplicitPreferenceForSingleCandidateIsComplete() {
		RasterBackendSelectionPlan plan = RasterBackendPlanner.Plan(
			[
				CreateDisplayCandidate(
					RasterBackendKind.KittyGraphics,
					RasterBackendSupportStatus.Supported
				),
			],
			CreateDisplayRequest(),
			new RasterBackendSelectionOptions(
				[ RasterBackendKind.KittyGraphics ]
			)
		);

		Assert.Equal( RasterBackendSelectionStatus.Selected, plan.Status );
		Assert.Equal( RasterBackendKind.KittyGraphics, plan.SelectedBackend );
	}

	[Fact]
	public void PartialPreferenceForTwoCandidatesIsRejected() {
		RasterBackendCandidate sixel = CreateDisplayCandidate(
			RasterBackendKind.Sixel,
			RasterBackendSupportStatus.Supported
		);
		RasterBackendCandidate kitty = CreateDisplayCandidate(
			RasterBackendKind.KittyGraphics,
			RasterBackendSupportStatus.Supported
		);

		Assert.Throws<ArgumentException>(
			() => RasterBackendPlanner.Plan(
				[ sixel, kitty ],
				CreateDisplayRequest(),
				new RasterBackendSelectionOptions(
					[ RasterBackendKind.Sixel ]
				)
			)
		);
	}

	[Fact]
	public void PreferenceForAbsentCandidateIsRejected() {
		RasterBackendCandidate sixel = CreateDisplayCandidate(
			RasterBackendKind.Sixel,
			RasterBackendSupportStatus.Supported
		);

		Assert.Throws<ArgumentException>(
			() => RasterBackendPlanner.Plan(
				[ sixel ],
				CreateDisplayRequest(),
				new RasterBackendSelectionOptions(
					[ RasterBackendKind.KittyGraphics ]
				)
			)
		);
	}

	[Fact]
	public void SelectionPlanRetainsRequestDrivenPlacementEvaluation() {
		RasterBackendCandidate kitty = CreatePlacementCandidate(
			RasterBackendKind.KittyGraphics
		);
		RasterBackendSelectionRequest request = new(
			new PersistentRasterLifecycleRequest( placementCount: 1 ),
			new PersistentRasterPlacementRequest(
				requireSourceRectangle: true
			)
		);

		RasterBackendSelectionPlan plan = RasterBackendPlanner.Plan(
			[ kitty ],
			request
		);

		Assert.Same( request, plan.Request );
		RasterBackendCandidateEvaluation evaluation =
			Assert.Single( plan.CandidateEvaluations );
		Assert.Equal(
			PersistentRasterLifecyclePlanStatus.Success,
			evaluation.LifecyclePlan.Status
		);
		Assert.Equal(
			PersistentRasterPlacementPlanStatus.Satisfied,
			Assert.IsType<PersistentRasterPlacementPlan>(
				evaluation.PlacementPlan
			).Status
		);
	}

	[Fact]
	public void SelectedBackendExistsOnlyForSelectedStatus() {
		RasterBackendSelectionRequest request = CreateDisplayRequest();
		RasterBackendSelectionPlan selected = RasterBackendPlanner.Plan(
			[
				CreateDisplayCandidate(
					RasterBackendKind.Sixel,
					RasterBackendSupportStatus.Supported
				),
			],
			request
		);
		RasterBackendSelectionPlan verification = RasterBackendPlanner.Plan(
			[
				CreateDisplayCandidate(
					RasterBackendKind.Sixel,
					RasterBackendSupportStatus.Unknown
				),
			],
			request
		);
		RasterBackendSelectionPlan preference = RasterBackendPlanner.Plan(
			[
				CreateDisplayCandidate(
					RasterBackendKind.Sixel,
					RasterBackendSupportStatus.Supported
				),
				CreateDisplayCandidate(
					RasterBackendKind.KittyGraphics,
					RasterBackendSupportStatus.Supported
				),
			],
			request
		);
		RasterBackendSelectionPlan impossible = RasterBackendPlanner.Plan(
			[
				CreateDisplayCandidate(
					RasterBackendKind.Sixel,
					RasterBackendSupportStatus.Unsupported
				),
			],
			request
		);

		Assert.NotNull( selected.SelectedBackend );
		Assert.Null( verification.SelectedBackend );
		Assert.Null( preference.SelectedBackend );
		Assert.Null( impossible.SelectedBackend );
	}

	private static RasterBackendSelectionRequest CreateDisplayRequest() {
		return new RasterBackendSelectionRequest(
			new PersistentRasterLifecycleRequest( displayEphemeral: true )
		);
	}

	private static RasterBackendCandidate CreateDisplayCandidate(
		RasterBackendKind backend,
		RasterBackendSupportStatus backendStatus
	) {
		return new RasterBackendCandidate(
			CreateBackendProfile( backend, backendStatus ),
			PersistentRasterLifecycleClassifier.Classify(
				[
					new PersistentRasterLifecycleEvidence(
						PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
						true,
						PersistentRasterLifecycleEvidenceKind.Verified,
						"rb04-display",
						0
					),
				]
			),
			PersistentRasterPlacementClassifier.Classify(
				Array.Empty<PersistentRasterPlacementEvidence>()
			)
		);
	}

	private static RasterBackendCandidate CreatePlacementCandidate(
		RasterBackendKind backend
	) {
		return new RasterBackendCandidate(
			CreateBackendProfile(
				backend,
				RasterBackendSupportStatus.Supported
			),
			PersistentRasterLifecycleClassifier.Classify(
				[
					new PersistentRasterLifecycleEvidence(
						PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
						true,
						PersistentRasterLifecycleEvidenceKind.Verified,
						"rb04-placement-lifecycle",
						0
					),
				]
			),
			PersistentRasterPlacementClassifier.Classify(
				[
					new PersistentRasterPlacementEvidence(
						PersistentRasterPlacementSubject.SourceRectangle,
						true,
						PersistentRasterPlacementEvidenceKind.Verified,
						"rb04-placement",
						0
					),
				]
			)
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
					"rb04-backend-positive",
					0
				),
			],
			RasterBackendSupportStatus.Unsupported => [
				new RasterBackendEvidence(
					backend,
					false,
					RasterBackendEvidenceKind.Verified,
					"rb04-backend-negative",
					0
				),
			],
			RasterBackendSupportStatus.Contradicted => [
				new RasterBackendEvidence(
					backend,
					true,
					RasterBackendEvidenceKind.Verified,
					"rb04-backend-positive",
					0
				),
				new RasterBackendEvidence(
					backend,
					false,
					RasterBackendEvidenceKind.Verified,
					"rb04-backend-negative",
					1
				),
			],
			_ => throw new ArgumentOutOfRangeException( nameof( status ) ),
		};

		return RasterBackendClassifier.Classify( backend, evidence );
	}
}
