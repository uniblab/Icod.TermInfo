using System.Globalization;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RL04PersistentRasterLifecyclePlannerTests {
	[Fact]
	public void PlanStatusMembershipAndNumericsAreFrozen() {
		Assert.Equal(
			new[] {
				"Success",
				"Indeterminate",
				"Impossible",
			},
			Enum.GetNames<PersistentRasterLifecyclePlanStatus>()
		);
		Assert.Equal(
			0,
			(int)PersistentRasterLifecyclePlanStatus.Success
		);
		Assert.Equal(
			1,
			(int)PersistentRasterLifecyclePlanStatus.Indeterminate
		);
		Assert.Equal(
			2,
			(int)PersistentRasterLifecyclePlanStatus.Impossible
		);
	}

	[Fact]
	public void OneShotDisplayProducesOneEphemeralStep() {
		PersistentRasterLifecycleProfile profile = CreateProfile(
			(PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
				PersistentRasterLifecycleSupportStatus.Supported)
		);
		PersistentRasterLifecycleRequest request = new(
			displayEphemeral: true
		);

		PersistentRasterLifecyclePlan plan =
			PersistentRasterLifecyclePlanner.Plan(
				profile,
				request
			);

		Assert.Equal(
			PersistentRasterLifecyclePlanStatus.Success,
			plan.Status
		);
		Assert.False( plan.RequiresRuntimeVerification );
		Assert.Empty( plan.Issues );
		Assert.Collection(
			plan.Steps,
			step => AssertStep(
				step,
				sequenceIndex: 0,
				PersistentRasterLifecycleOperation.DisplayEphemeral,
				requiresRuntimeVerification: false
			)
		);
	}

	[Fact]
	public void EphemeralSupportDoesNotSatisfyPersistentUpload() {
		PersistentRasterLifecycleProfile profile = CreateProfile(
			(PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
				PersistentRasterLifecycleSupportStatus.Supported),
			(PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
				PersistentRasterLifecycleSupportStatus.Unsupported)
		);
		PersistentRasterLifecycleRequest request = new(
			uploadResource: true
		);

		PersistentRasterLifecyclePlan plan =
			PersistentRasterLifecyclePlanner.Plan(
				profile,
				request
			);

		Assert.Equal(
			PersistentRasterLifecyclePlanStatus.Impossible,
			plan.Status
		);
		Assert.False( plan.RequiresRuntimeVerification );
		Assert.Empty( plan.Steps );
		Assert.Collection(
			plan.Issues,
			issue => AssertIssue(
				issue,
				PersistentRasterLifecycleOperation.UploadResource,
				PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
				PersistentRasterLifecycleSupportStatus.Unsupported,
				requiresRuntimeVerification: false
			)
		);
	}

	[Fact]
	public void UnknownUploadProducesIndeterminatePlanThatRequiresVerification() {
		PersistentRasterLifecycleProfile profile = CreateProfile();
		PersistentRasterLifecycleRequest request = new(
			uploadResource: true
		);

		PersistentRasterLifecyclePlan plan =
			PersistentRasterLifecyclePlanner.Plan(
				profile,
				request
			);

		Assert.Equal(
			PersistentRasterLifecyclePlanStatus.Indeterminate,
			plan.Status
		);
		Assert.True( plan.RequiresRuntimeVerification );
		Assert.Collection(
			plan.Steps,
			step => AssertStep(
				step,
				sequenceIndex: 0,
				PersistentRasterLifecycleOperation.UploadResource,
				requiresRuntimeVerification: true
			)
		);
		Assert.Collection(
			plan.Issues,
			issue => AssertIssue(
				issue,
				PersistentRasterLifecycleOperation.UploadResource,
				PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
				PersistentRasterLifecycleSupportStatus.Unknown,
				requiresRuntimeVerification: true
			)
		);
	}

	[Fact]
	public void ContradictedUploadProducesIndeterminatePlanThatRequiresVerification() {
		PersistentRasterLifecycleProfile profile = CreateProfile(
			(PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
				PersistentRasterLifecycleSupportStatus.Contradicted)
		);
		PersistentRasterLifecycleRequest request = new(
			uploadResource: true
		);

		PersistentRasterLifecyclePlan plan =
			PersistentRasterLifecyclePlanner.Plan(
				profile,
				request
			);

		Assert.Equal(
			PersistentRasterLifecyclePlanStatus.Indeterminate,
			plan.Status
		);
		Assert.True( plan.RequiresRuntimeVerification );
		Assert.Collection(
			plan.Issues,
			issue => AssertIssue(
				issue,
				PersistentRasterLifecycleOperation.UploadResource,
				PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
				PersistentRasterLifecycleSupportStatus.Contradicted,
				requiresRuntimeVerification: true
			)
		);
	}

	[Fact]
	public void AcknowledgedUploadRequirementIsStructuredWithoutAddingAnOperation() {
		PersistentRasterLifecycleProfile profile = CreateProfile(
			(PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
				PersistentRasterLifecycleSupportStatus.Supported)
		);
		PersistentRasterLifecycleRequest request = new(
			uploadResource: true,
			requireAcknowledgedUpload: true
		);

		PersistentRasterLifecyclePlan plan =
			PersistentRasterLifecyclePlanner.Plan(
				profile,
				request
			);

		Assert.Equal(
			PersistentRasterLifecyclePlanStatus.Indeterminate,
			plan.Status
		);
		Assert.True( plan.RequiresRuntimeVerification );
		Assert.Collection(
			plan.Steps,
			step => AssertStep(
				step,
				sequenceIndex: 0,
				PersistentRasterLifecycleOperation.UploadResource,
				requiresRuntimeVerification: true
			)
		);
		Assert.Collection(
			plan.Issues,
			issue => AssertIssue(
				issue,
				PersistentRasterLifecycleOperation.UploadResource,
				PersistentRasterLifecycleEvidenceSubject.AcknowledgedUpload,
				PersistentRasterLifecycleSupportStatus.Unknown,
				requiresRuntimeVerification: true
			)
		);
	}

	[Fact]
	public void SinglePlacementProducesUploadThenCreate() {
		PersistentRasterLifecycleProfile profile = CreateProfile(
			(PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
				PersistentRasterLifecycleSupportStatus.Supported),
			(PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
				PersistentRasterLifecycleSupportStatus.Supported)
		);
		PersistentRasterLifecycleRequest request = new(
			uploadResource: true,
			placementCount: 1
		);

		PersistentRasterLifecyclePlan plan =
			PersistentRasterLifecyclePlanner.Plan(
				profile,
				request
			);

		Assert.Equal(
			PersistentRasterLifecyclePlanStatus.Success,
			plan.Status
		);
		Assert.Collection(
			plan.Steps,
			step => AssertStep(
				step,
				sequenceIndex: 0,
				PersistentRasterLifecycleOperation.UploadResource,
				requiresRuntimeVerification: false
			),
			step => AssertStep(
				step,
				sequenceIndex: 1,
				PersistentRasterLifecycleOperation.CreatePlacement,
				requiresRuntimeVerification: false
			)
		);
	}

	[Fact]
	public void MultiplePlacementsProduceRepeatedCreateSteps() {
		PersistentRasterLifecycleProfile profile = CreateProfile(
			(PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
				PersistentRasterLifecycleSupportStatus.Supported),
			(PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
				PersistentRasterLifecycleSupportStatus.Supported),
			(PersistentRasterLifecycleEvidenceSubject.MultiplePlacements,
				PersistentRasterLifecycleSupportStatus.Supported)
		);
		PersistentRasterLifecycleRequest request = new(
			uploadResource: true,
			placementCount: 3
		);

		PersistentRasterLifecyclePlan plan =
			PersistentRasterLifecyclePlanner.Plan(
				profile,
				request
			);

		Assert.Equal(
			PersistentRasterLifecyclePlanStatus.Success,
			plan.Status
		);
		Assert.Equal(
			new[] {
				PersistentRasterLifecycleOperation.UploadResource,
				PersistentRasterLifecycleOperation.CreatePlacement,
				PersistentRasterLifecycleOperation.CreatePlacement,
				PersistentRasterLifecycleOperation.CreatePlacement,
			},
			plan.Steps.Select( step => step.Operation )
		);
		Assert.Equal(
			new[] { 0, 1, 2, 3 },
			plan.Steps.Select( step => step.SequenceIndex )
		);
	}

	[Fact]
	public void UnknownMultiplePlacementSupportMarksOnlyAdditionalPlacementsForVerification() {
		PersistentRasterLifecycleProfile profile = CreateProfile(
			(PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
				PersistentRasterLifecycleSupportStatus.Supported),
			(PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
				PersistentRasterLifecycleSupportStatus.Supported)
		);
		PersistentRasterLifecycleRequest request = new(
			uploadResource: true,
			placementCount: 3
		);

		PersistentRasterLifecyclePlan plan =
			PersistentRasterLifecyclePlanner.Plan(
				profile,
				request
			);

		Assert.Equal(
			PersistentRasterLifecyclePlanStatus.Indeterminate,
			plan.Status
		);
		Assert.False( plan.Steps[ 0 ].RequiresRuntimeVerification );
		Assert.False( plan.Steps[ 1 ].RequiresRuntimeVerification );
		Assert.True( plan.Steps[ 2 ].RequiresRuntimeVerification );
		Assert.True( plan.Steps[ 3 ].RequiresRuntimeVerification );
		Assert.Collection(
			plan.Issues,
			issue => AssertIssue(
				issue,
				PersistentRasterLifecycleOperation.CreatePlacement,
				PersistentRasterLifecycleEvidenceSubject.MultiplePlacements,
				PersistentRasterLifecycleSupportStatus.Unknown,
				requiresRuntimeVerification: true
			)
		);
	}

	[Fact]
	public void UnsupportedMultiplePlacementsMakesPersistentReuseImpossible() {
		PersistentRasterLifecycleProfile profile = CreateProfile(
			(PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
				PersistentRasterLifecycleSupportStatus.Supported),
			(PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
				PersistentRasterLifecycleSupportStatus.Supported),
			(PersistentRasterLifecycleEvidenceSubject.MultiplePlacements,
				PersistentRasterLifecycleSupportStatus.Unsupported)
		);
		PersistentRasterLifecycleRequest request = new(
			uploadResource: true,
			placementCount: 2
		);

		PersistentRasterLifecyclePlan plan =
			PersistentRasterLifecyclePlanner.Plan(
				profile,
				request
			);

		Assert.Equal(
			PersistentRasterLifecyclePlanStatus.Impossible,
			plan.Status
		);
		Assert.Empty( plan.Steps );
		Assert.Collection(
			plan.Issues,
			issue => AssertIssue(
				issue,
				PersistentRasterLifecycleOperation.CreatePlacement,
				PersistentRasterLifecycleEvidenceSubject.MultiplePlacements,
				PersistentRasterLifecycleSupportStatus.Unsupported,
				requiresRuntimeVerification: false
			)
		);
	}

	[Fact]
	public void CreateThenUpdatePlanUsesDeterministicOperationOrder() {
		PersistentRasterLifecycleProfile profile = CreateProfile(
			(PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
				PersistentRasterLifecycleSupportStatus.Supported),
			(PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
				PersistentRasterLifecycleSupportStatus.Supported),
			(PersistentRasterLifecycleEvidenceSubject.PlacementUpdate,
				PersistentRasterLifecycleSupportStatus.Supported)
		);
		PersistentRasterLifecycleRequest request = new(
			uploadResource: true,
			placementCount: 1,
			updatePlacement: true
		);

		PersistentRasterLifecyclePlan plan =
			PersistentRasterLifecyclePlanner.Plan(
				profile,
				request
			);

		Assert.Equal(
			new[] {
				PersistentRasterLifecycleOperation.UploadResource,
				PersistentRasterLifecycleOperation.CreatePlacement,
				PersistentRasterLifecycleOperation.UpdatePlacement,
			},
			plan.Steps.Select( step => step.Operation )
		);
	}

	[Fact]
	public void CleanupPlanOrdersPlacementDeletionBeforeResourceDeletion() {
		PersistentRasterLifecycleProfile profile = CreateProfile(
			(PersistentRasterLifecycleEvidenceSubject.PlacementDeletion,
				PersistentRasterLifecycleSupportStatus.Supported),
			(PersistentRasterLifecycleEvidenceSubject.ResourceDeletion,
				PersistentRasterLifecycleSupportStatus.Supported)
		);
		PersistentRasterLifecycleRequest request = new(
			deletePlacement: true,
			deleteResource: true
		);

		PersistentRasterLifecyclePlan plan =
			PersistentRasterLifecyclePlanner.Plan(
				profile,
				request
			);

		Assert.Equal(
			PersistentRasterLifecyclePlanStatus.Success,
			plan.Status
		);
		Assert.Equal(
			new[] {
				PersistentRasterLifecycleOperation.DeletePlacement,
				PersistentRasterLifecycleOperation.DeleteResource,
			},
			plan.Steps.Select( step => step.Operation )
		);
	}

	[Fact]
	public void RequestValidationFreezesBoundsAndSemanticExclusivity() {
		Assert.Equal(
			256,
			PersistentRasterLifecycleRequest.MaximumSupportedPlacementCount
		);
		Assert.Throws<ArgumentException>(
			() => new PersistentRasterLifecycleRequest()
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new PersistentRasterLifecycleRequest(
				placementCount: -1
			)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new PersistentRasterLifecycleRequest(
				placementCount:
					PersistentRasterLifecycleRequest.MaximumSupportedPlacementCount + 1
			)
		);
		Assert.Throws<ArgumentException>(
			() => new PersistentRasterLifecycleRequest(
				displayEphemeral: true,
				uploadResource: true
			)
		);
		Assert.Throws<ArgumentException>(
			() => new PersistentRasterLifecycleRequest(
				requireAcknowledgedUpload: true,
				deleteResource: true
			)
		);
	}

	[Fact]
	public void PlanCollectionsAreImmutable() {
		PersistentRasterLifecycleProfile profile = CreateProfile(
			(PersistentRasterLifecycleEvidenceSubject.PlacementDeletion,
				PersistentRasterLifecycleSupportStatus.Supported)
		);
		PersistentRasterLifecyclePlan plan =
			PersistentRasterLifecyclePlanner.Plan(
				profile,
				new PersistentRasterLifecycleRequest(
					deletePlacement: true
				)
			);

		Assert.False( plan.Steps is PersistentRasterLifecyclePlanStep[] );
		Assert.False( plan.Issues is PersistentRasterLifecyclePlanIssue[] );
		if ( plan.Steps is IList<PersistentRasterLifecyclePlanStep> steps ) {
			Assert.True( steps.IsReadOnly );
		}
		if ( plan.Issues is IList<PersistentRasterLifecyclePlanIssue> issues ) {
			Assert.True( issues.IsReadOnly );
		}
		Assert.All(
			typeof( PersistentRasterLifecyclePlan ).GetProperties(),
			property => Assert.Null( property.SetMethod )
		);
	}

	[Fact]
	public void PlanningIsCultureAndRepetitionIndependent() {
		PersistentRasterLifecycleProfile profile = CreateProfile(
			(PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
				PersistentRasterLifecycleSupportStatus.Supported),
			(PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
				PersistentRasterLifecycleSupportStatus.Supported),
			(PersistentRasterLifecycleEvidenceSubject.MultiplePlacements,
				PersistentRasterLifecycleSupportStatus.Contradicted),
			(PersistentRasterLifecycleEvidenceSubject.PlacementDeletion,
				PersistentRasterLifecycleSupportStatus.Supported),
			(PersistentRasterLifecycleEvidenceSubject.ResourceDeletion,
				PersistentRasterLifecycleSupportStatus.Supported)
		);
		PersistentRasterLifecycleRequest request = new(
			uploadResource: true,
			placementCount: 2,
			deletePlacement: true,
			deleteResource: true
		);
		PersistentRasterLifecyclePlan baseline =
			PersistentRasterLifecyclePlanner.Plan(
				profile,
				request
			);

		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
		try {
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "tr-TR" );
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "tr-TR" );
			PersistentRasterLifecyclePlan repeated =
				PersistentRasterLifecyclePlanner.Plan(
					profile,
					request
				);

			Assert.Equal( baseline.Status, repeated.Status );
			Assert.Equal(
				baseline.Steps.Select(
					step => ( step.Operation, step.RequiresRuntimeVerification )
				),
				repeated.Steps.Select(
					step => ( step.Operation, step.RequiresRuntimeVerification )
				)
			);
			Assert.Equal(
				baseline.Issues.Select(
					issue => ( issue.Operation, issue.Subject, issue.SupportStatus )
				),
				repeated.Issues.Select(
					issue => ( issue.Operation, issue.Subject, issue.SupportStatus )
				)
			);
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}
	}

	private static PersistentRasterLifecycleProfile CreateProfile(
		params (PersistentRasterLifecycleEvidenceSubject Subject,
			PersistentRasterLifecycleSupportStatus Status)[] states
	) {
		List<PersistentRasterLifecycleEvidence> evidence = [];
		int ordinal = 0;
		foreach ( var state in states ) {
			switch ( state.Status ) {
				case PersistentRasterLifecycleSupportStatus.Unknown:
					break;
				case PersistentRasterLifecycleSupportStatus.Supported:
					evidence.Add(
						CreateEvidence(
							state.Subject,
							isPositive: true,
							ordinal++
						)
					);
					break;
				case PersistentRasterLifecycleSupportStatus.Unsupported:
					evidence.Add(
						CreateEvidence(
							state.Subject,
							isPositive: false,
							ordinal++
						)
					);
					break;
				case PersistentRasterLifecycleSupportStatus.Contradicted:
					evidence.Add(
						CreateEvidence(
							state.Subject,
							isPositive: true,
							ordinal++
						)
					);
					evidence.Add(
						CreateEvidence(
							state.Subject,
							isPositive: false,
							ordinal++
						)
					);
					break;
				default:
					throw new ArgumentOutOfRangeException(
						nameof( states )
					);
			}
		}

		return PersistentRasterLifecycleClassifier.Classify( evidence );
	}

	private static PersistentRasterLifecycleEvidence CreateEvidence(
		PersistentRasterLifecycleEvidenceSubject subject,
		bool isPositive,
		int sourceOrdinal
	) => new(
		subject,
		isPositive,
		PersistentRasterLifecycleEvidenceKind.Verified,
		$"state-{sourceOrdinal}",
		sourceOrdinal
	);

	private static void AssertStep(
		PersistentRasterLifecyclePlanStep step,
		int sequenceIndex,
		PersistentRasterLifecycleOperation operation,
		bool requiresRuntimeVerification
	) {
		Assert.Equal( sequenceIndex, step.SequenceIndex );
		Assert.Equal( operation, step.Operation );
		Assert.Equal(
			requiresRuntimeVerification,
			step.RequiresRuntimeVerification
		);
	}

	private static void AssertIssue(
		PersistentRasterLifecyclePlanIssue issue,
		PersistentRasterLifecycleOperation operation,
		PersistentRasterLifecycleEvidenceSubject subject,
		PersistentRasterLifecycleSupportStatus supportStatus,
		bool requiresRuntimeVerification
	) {
		Assert.Equal( operation, issue.Operation );
		Assert.Equal( subject, issue.Subject );
		Assert.Equal( supportStatus, issue.SupportStatus );
		Assert.Equal(
			requiresRuntimeVerification,
			issue.RequiresRuntimeVerification
		);
	}
}
