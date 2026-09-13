using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class PG04PersistentRasterPlacementPlannerTests {
	[Fact]
	public void EmptyPlacementRequestIsRejected() {
		Assert.Throws<ArgumentException>(
			() => new PersistentRasterPlacementRequest()
		);
	}

	[Fact]
	public void SupportedSourceRectangleRequirementIsSatisfied() {
		PersistentRasterPlacementPlan plan =
			PersistentRasterPlacementPlanner.Plan(
				CreateLifecyclePlan(
					PersistentRasterLifecycleSupportStatus.Supported
				),
				CreatePlacementProfile(
					PersistentRasterLifecycleSupportStatus.Supported,
					PersistentRasterLifecycleSupportStatus.Unknown
				),
				new PersistentRasterPlacementRequest(
					requireSourceRectangle: true
				)
			);

		Assert.Equal(
			PersistentRasterPlacementPlanStatus.Satisfied,
			plan.Status
		);
		PersistentRasterPlacementPlanRequirement requirement =
			Assert.Single( plan.Requirements );
		Assert.Equal(
			PersistentRasterPlacementSubject.SourceRectangle,
			requirement.Subject
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Supported,
			requirement.SupportStatus
		);
		Assert.False( requirement.RequiresRuntimeVerification );
		Assert.Empty( plan.Issues );
	}

	[Fact]
	public void SupportedSignedZOrderRequirementIsSatisfied() {
		PersistentRasterPlacementPlan plan =
			PersistentRasterPlacementPlanner.Plan(
				CreateLifecyclePlan(
					PersistentRasterLifecycleSupportStatus.Supported
				),
				CreatePlacementProfile(
					PersistentRasterLifecycleSupportStatus.Unknown,
					PersistentRasterLifecycleSupportStatus.Supported
				),
				new PersistentRasterPlacementRequest(
					requireSignedZOrder: true
				)
			);

		Assert.Equal(
			PersistentRasterPlacementPlanStatus.Satisfied,
			plan.Status
		);
		PersistentRasterPlacementPlanRequirement requirement =
			Assert.Single( plan.Requirements );
		Assert.Equal(
			PersistentRasterPlacementSubject.SignedZOrder,
			requirement.Subject
		);
		Assert.False( requirement.RequiresRuntimeVerification );
	}

	[Fact]
	public void CombinedRequirementsAreCanonicalAndSatisfied() {
		PersistentRasterPlacementPlan plan =
			PersistentRasterPlacementPlanner.Plan(
				CreateLifecyclePlan(
					PersistentRasterLifecycleSupportStatus.Supported
				),
				CreatePlacementProfile(
					PersistentRasterLifecycleSupportStatus.Supported,
					PersistentRasterLifecycleSupportStatus.Supported
				),
				new PersistentRasterPlacementRequest(
					requireSourceRectangle: true,
					requireSignedZOrder: true
				)
			);

		Assert.Equal(
			new[] {
				PersistentRasterPlacementSubject.SourceRectangle,
				PersistentRasterPlacementSubject.SignedZOrder,
			},
			plan.Requirements.Select( item => item.Subject )
		);
		Assert.Equal(
			PersistentRasterPlacementPlanStatus.Satisfied,
			plan.Status
		);
	}

	[Fact]
	public void UnknownPlacementSupportRequiresRuntimeVerification() {
		PersistentRasterPlacementPlan plan =
			PersistentRasterPlacementPlanner.Plan(
				CreateLifecyclePlan(
					PersistentRasterLifecycleSupportStatus.Supported
				),
				CreatePlacementProfile(
					PersistentRasterLifecycleSupportStatus.Unknown,
					PersistentRasterLifecycleSupportStatus.Supported
				),
				new PersistentRasterPlacementRequest(
					requireSourceRectangle: true
				)
			);

		Assert.Equal(
			PersistentRasterPlacementPlanStatus.RequiresRuntimeVerification,
			plan.Status
		);
		PersistentRasterPlacementPlanRequirement requirement =
			Assert.Single( plan.Requirements );
		Assert.True( requirement.RequiresRuntimeVerification );
		PersistentRasterPlacementPlanIssue issue = Assert.Single( plan.Issues );
		Assert.Equal(
			PersistentRasterPlacementSubject.SourceRectangle,
			issue.Subject
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unknown,
			issue.SupportStatus
		);
		Assert.True( issue.RequiresRuntimeVerification );
	}

	[Theory]
	[InlineData( PersistentRasterLifecycleSupportStatus.Unsupported )]
	[InlineData( PersistentRasterLifecycleSupportStatus.Contradicted )]
	public void ConclusivePlacementFailureIsImpossible(
		PersistentRasterLifecycleSupportStatus supportStatus
	) {
		PersistentRasterPlacementPlan plan =
			PersistentRasterPlacementPlanner.Plan(
				CreateLifecyclePlan(
					PersistentRasterLifecycleSupportStatus.Supported
				),
				CreatePlacementProfile(
					supportStatus,
					PersistentRasterLifecycleSupportStatus.Supported
				),
				new PersistentRasterPlacementRequest(
					requireSourceRectangle: true
				)
			);

		Assert.Equal(
			PersistentRasterPlacementPlanStatus.Impossible,
			plan.Status
		);
		PersistentRasterPlacementPlanRequirement requirement =
			Assert.Single( plan.Requirements );
		Assert.Equal( supportStatus, requirement.SupportStatus );
		Assert.False( requirement.RequiresRuntimeVerification );
		PersistentRasterPlacementPlanIssue issue = Assert.Single( plan.Issues );
		Assert.Equal( supportStatus, issue.SupportStatus );
		Assert.False( issue.RequiresRuntimeVerification );
	}

	[Fact]
	public void IndeterminateLifecyclePlanMakesPlacementPlanIndeterminate() {
		PersistentRasterPlacementPlan plan =
			PersistentRasterPlacementPlanner.Plan(
				CreateLifecyclePlan(
					PersistentRasterLifecycleSupportStatus.Unknown
				),
				CreatePlacementProfile(
					PersistentRasterLifecycleSupportStatus.Supported,
					PersistentRasterLifecycleSupportStatus.Unknown
				),
				new PersistentRasterPlacementRequest(
					requireSourceRectangle: true
				)
			);

		Assert.Equal(
			PersistentRasterPlacementPlanStatus.Indeterminate,
			plan.Status
		);
		Assert.Equal(
			PersistentRasterLifecyclePlanStatus.Indeterminate,
			plan.LifecycleStatus
		);
	}

	[Fact]
	public void ImpossibleLifecyclePlanMakesPlacementPlanImpossible() {
		PersistentRasterPlacementPlan plan =
			PersistentRasterPlacementPlanner.Plan(
				CreateLifecyclePlan(
					PersistentRasterLifecycleSupportStatus.Unsupported
				),
				CreatePlacementProfile(
					PersistentRasterLifecycleSupportStatus.Supported,
					PersistentRasterLifecycleSupportStatus.Unknown
				),
				new PersistentRasterPlacementRequest(
					requireSourceRectangle: true
				)
			);

		Assert.Equal(
			PersistentRasterPlacementPlanStatus.Impossible,
			plan.Status
		);
		Assert.Equal(
			PersistentRasterLifecyclePlanStatus.Impossible,
			plan.LifecycleStatus
		);
	}

	[Fact]
	public void PlacementImpossibilityDominatesIndeterminateLifecycle() {
		PersistentRasterPlacementPlan plan =
			PersistentRasterPlacementPlanner.Plan(
				CreateLifecyclePlan(
					PersistentRasterLifecycleSupportStatus.Unknown
				),
				CreatePlacementProfile(
					PersistentRasterLifecycleSupportStatus.Unsupported,
					PersistentRasterLifecycleSupportStatus.Unknown
				),
				new PersistentRasterPlacementRequest(
					requireSourceRectangle: true
				)
			);

		Assert.Equal(
			PersistentRasterPlacementPlanStatus.Impossible,
			plan.Status
		);
	}

	[Fact]
	public void PlannerRejectsNullArguments() {
		PersistentRasterLifecyclePlan lifecyclePlan =
			CreateLifecyclePlan(
				PersistentRasterLifecycleSupportStatus.Supported
			);
		PersistentRasterPlacementProfile placementProfile =
			CreatePlacementProfile(
				PersistentRasterLifecycleSupportStatus.Supported,
				PersistentRasterLifecycleSupportStatus.Supported
			);
		PersistentRasterPlacementRequest request =
			new(
				requireSourceRectangle: true
			);

		Assert.Throws<ArgumentNullException>(
			() => PersistentRasterPlacementPlanner.Plan(
				null!,
				placementProfile,
				request
			)
		);
		Assert.Throws<ArgumentNullException>(
			() => PersistentRasterPlacementPlanner.Plan(
				lifecyclePlan,
				null!,
				request
			)
		);
		Assert.Throws<ArgumentNullException>(
			() => PersistentRasterPlacementPlanner.Plan(
				lifecyclePlan,
				placementProfile,
				null!
			)
		);
	}

	private static PersistentRasterLifecyclePlan CreateLifecyclePlan(
		PersistentRasterLifecycleSupportStatus placementCreationStatus
	) {
		List<PersistentRasterLifecycleEvidence> evidence = [];
		if ( placementCreationStatus != PersistentRasterLifecycleSupportStatus.Unknown ) {
			bool isPositive =
				placementCreationStatus
					== PersistentRasterLifecycleSupportStatus.Supported;
			evidence.Add(
				new PersistentRasterLifecycleEvidence(
					PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
					isPositive,
					PersistentRasterLifecycleEvidenceKind.Verified,
					"pg04-lifecycle",
					0
				)
			);
			if ( placementCreationStatus == PersistentRasterLifecycleSupportStatus.Contradicted ) {
				evidence.Add(
					new PersistentRasterLifecycleEvidence(
						PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
						true,
						PersistentRasterLifecycleEvidenceKind.Verified,
						"pg04-lifecycle-positive",
						1
					)
				);
			}
		}

		PersistentRasterLifecycleProfile profile =
			PersistentRasterLifecycleClassifier.Classify( evidence );
		return PersistentRasterLifecyclePlanner.Plan(
			profile,
			new PersistentRasterLifecycleRequest(
				placementCount: 1
			)
		);
	}

	private static PersistentRasterPlacementProfile CreatePlacementProfile(
		PersistentRasterLifecycleSupportStatus sourceRectangleStatus,
		PersistentRasterLifecycleSupportStatus signedZOrderStatus
	) {
		List<PersistentRasterPlacementEvidence> evidence = [];
		AddPlacementEvidence(
			evidence,
			PersistentRasterPlacementSubject.SourceRectangle,
			sourceRectangleStatus
		);
		AddPlacementEvidence(
			evidence,
			PersistentRasterPlacementSubject.SignedZOrder,
			signedZOrderStatus
		);
		return PersistentRasterPlacementClassifier.Classify( evidence );
	}

	private static void AddPlacementEvidence(
		List<PersistentRasterPlacementEvidence> evidence,
		PersistentRasterPlacementSubject subject,
		PersistentRasterLifecycleSupportStatus supportStatus
	) {
		ArgumentNullException.ThrowIfNull( evidence );
		if ( supportStatus == PersistentRasterLifecycleSupportStatus.Unknown ) {
			return;
		}

		int ordinal = evidence.Count;
		bool isPositive =
			supportStatus == PersistentRasterLifecycleSupportStatus.Supported;
		evidence.Add(
			new PersistentRasterPlacementEvidence(
				subject,
				isPositive,
				PersistentRasterPlacementEvidenceKind.Verified,
				$"pg04-{subject}-primary",
				ordinal
			)
		);
		if ( supportStatus == PersistentRasterLifecycleSupportStatus.Contradicted ) {
			evidence.Add(
				new PersistentRasterPlacementEvidence(
					subject,
					true,
					PersistentRasterPlacementEvidenceKind.Verified,
					$"pg04-{subject}-positive",
					ordinal + 1
				)
			);
		}
	}
}
