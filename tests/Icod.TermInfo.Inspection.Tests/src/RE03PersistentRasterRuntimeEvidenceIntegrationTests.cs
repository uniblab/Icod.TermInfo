using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RE03PersistentRasterRuntimeEvidenceIntegrationTests {
	[Fact]
	public void IntegrationIssueKindMembershipAndNumericsAreFrozen() {
		Assert.Equal(
			new[] {
				PersistentRasterRuntimeIntegrationIssueKind.LifecycleEvidenceCapacityExhausted,
				PersistentRasterRuntimeIntegrationIssueKind.PlacementEvidenceCapacityExhausted,
				PersistentRasterRuntimeIntegrationIssueKind.LifecycleOrdinalSpaceExhausted,
				PersistentRasterRuntimeIntegrationIssueKind.PlacementOrdinalSpaceExhausted,
			},
			Enum.GetValues<PersistentRasterRuntimeIntegrationIssueKind>()
		);
		Assert.Equal(
			0,
			(int)PersistentRasterRuntimeIntegrationIssueKind.LifecycleEvidenceCapacityExhausted
		);
		Assert.Equal(
			1,
			(int)PersistentRasterRuntimeIntegrationIssueKind.PlacementEvidenceCapacityExhausted
		);
		Assert.Equal(
			2,
			(int)PersistentRasterRuntimeIntegrationIssueKind.LifecycleOrdinalSpaceExhausted
		);
		Assert.Equal(
			3,
			(int)PersistentRasterRuntimeIntegrationIssueKind.PlacementOrdinalSpaceExhausted
		);
	}

	[Fact]
	public void ConclusiveObservationsMapToVerifiedEvidenceAndInconclusiveRemainVisible() {
		PersistentRasterRuntimeLifecycleObservation lifecycleSupported = new(
			PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
			PersistentRasterRuntimeObservationOutcome.Supported,
			" lifecycle-supported ",
			7
		);
		PersistentRasterRuntimeLifecycleObservation lifecycleUnsupported = new(
			PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
			PersistentRasterRuntimeObservationOutcome.Unsupported,
			"lifecycle-unsupported",
			8
		);
		PersistentRasterRuntimeLifecycleObservation lifecycleInconclusive = new(
			PersistentRasterLifecycleEvidenceSubject.AcknowledgedUpload,
			PersistentRasterRuntimeObservationOutcome.Inconclusive,
			"lifecycle-inconclusive",
			9
		);
		PersistentRasterRuntimePlacementObservation placementSupported = new(
			PersistentRasterPlacementSubject.SourceRectangle,
			PersistentRasterRuntimeObservationOutcome.Supported,
			" placement-supported ",
			10
		);
		PersistentRasterRuntimePlacementObservation placementInconclusive = new(
			PersistentRasterPlacementSubject.SourceRectangle,
			PersistentRasterRuntimeObservationOutcome.Inconclusive,
			"placement-inconclusive",
			11
		);
		PersistentRasterRuntimePlacementObservation placementUnsupported = new(
			PersistentRasterPlacementSubject.SignedZOrder,
			PersistentRasterRuntimeObservationOutcome.Unsupported,
			"placement-unsupported",
			12
		);
		PersistentRasterRuntimeObservationSet observations = new(
			new[] {
				lifecycleInconclusive,
				lifecycleUnsupported,
				lifecycleSupported,
			},
			new[] {
				placementUnsupported,
				placementInconclusive,
				placementSupported,
			}
		);

		PersistentRasterRuntimeIntegrationResult result =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				CreateEmptyLifecycleProfile(),
				CreateEmptyPlacementProfile(),
				observations
			);

		Assert.Same( observations, result.Observations );
		Assert.True( result.Succeeded );
		Assert.Empty( result.Issues );
		Assert.Equal( 2, result.ImportedLifecycleEvidence.Count );
		Assert.Equal( 2, result.ImportedPlacementEvidence.Count );
		Assert.Equal(
			new[] { lifecycleInconclusive },
			result.InconclusiveLifecycleObservations
		);
		Assert.Equal(
			new[] { placementInconclusive },
			result.InconclusivePlacementObservations
		);

		PersistentRasterLifecycleEvidence importedLifecycleSupported =
			Assert.Single(
				result.ImportedLifecycleEvidence,
				item => item.Subject
					== PersistentRasterLifecycleEvidenceSubject.RasterDisplay
			);
		Assert.True( importedLifecycleSupported.IsPositive );
		Assert.Equal(
			PersistentRasterLifecycleEvidenceKind.Verified,
			importedLifecycleSupported.Kind
		);
		Assert.Equal(
			" lifecycle-supported ",
			importedLifecycleSupported.SourceLabel
		);

		PersistentRasterLifecycleEvidence importedLifecycleUnsupported =
			Assert.Single(
				result.ImportedLifecycleEvidence,
				item => item.Subject
					== PersistentRasterLifecycleEvidenceSubject.PersistentUpload
			);
		Assert.False( importedLifecycleUnsupported.IsPositive );
		Assert.Equal(
			PersistentRasterLifecycleEvidenceKind.Verified,
			importedLifecycleUnsupported.Kind
		);
		Assert.Equal(
			"lifecycle-unsupported",
			importedLifecycleUnsupported.SourceLabel
		);

		PersistentRasterPlacementEvidence importedPlacementSupported =
			Assert.Single(
				result.ImportedPlacementEvidence,
				item => item.Subject == PersistentRasterPlacementSubject.SourceRectangle
			);
		Assert.True( importedPlacementSupported.IsPositive );
		Assert.Equal(
			PersistentRasterPlacementEvidenceKind.Verified,
			importedPlacementSupported.Kind
		);
		Assert.Equal(
			" placement-supported ",
			importedPlacementSupported.SourceLabel
		);

		PersistentRasterPlacementEvidence importedPlacementUnsupported =
			Assert.Single(
				result.ImportedPlacementEvidence,
				item => item.Subject == PersistentRasterPlacementSubject.SignedZOrder
			);
		Assert.False( importedPlacementUnsupported.IsPositive );
		Assert.Equal(
			PersistentRasterPlacementEvidenceKind.Verified,
			importedPlacementUnsupported.Kind
		);
		Assert.Equal(
			"placement-unsupported",
			importedPlacementUnsupported.SourceLabel
		);

		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Supported,
			result.LifecycleProfile.RasterDisplay
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unsupported,
			result.LifecycleProfile.PersistentUpload
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unknown,
			result.LifecycleProfile.AcknowledgedUpload
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Supported,
			result.PlacementProfile.SourceRectangle
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unsupported,
			result.PlacementProfile.SignedZOrder
		);
	}

	[Fact]
	public void SafeFinalOrdinalsAppendAfterExistingEvidenceWithoutRenumbering() {
		PersistentRasterLifecycleEvidence existingLifecycle = new(
			PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
			true,
			PersistentRasterLifecycleEvidenceKind.Declared,
			"existing-lifecycle",
			10
		);
		PersistentRasterPlacementEvidence existingPlacement = new(
			PersistentRasterPlacementSubject.SourceRectangle,
			true,
			PersistentRasterPlacementEvidenceKind.Declared,
			"existing-placement",
			20
		);
		PersistentRasterLifecycleProfile lifecycleProfile =
			PersistentRasterLifecycleClassifier.Classify(
				new[] { existingLifecycle }
			);
		PersistentRasterPlacementProfile placementProfile =
			PersistentRasterPlacementClassifier.Classify(
				new[] { existingPlacement }
			);
		PersistentRasterRuntimeObservationSet observations = new(
			new[] {
				new PersistentRasterRuntimeLifecycleObservation(
					PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
					PersistentRasterRuntimeObservationOutcome.Supported,
					"zeta",
					9
				),
				new PersistentRasterRuntimeLifecycleObservation(
					PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
					PersistentRasterRuntimeObservationOutcome.Unsupported,
					"alpha",
					99
				),
			},
			new[] {
				new PersistentRasterRuntimePlacementObservation(
					PersistentRasterPlacementSubject.SignedZOrder,
					PersistentRasterRuntimeObservationOutcome.Unsupported,
					"zeta",
					3
				),
				new PersistentRasterRuntimePlacementObservation(
					PersistentRasterPlacementSubject.SourceRectangle,
					PersistentRasterRuntimeObservationOutcome.Supported,
					"alpha",
					77
				),
			}
		);

		PersistentRasterRuntimeIntegrationResult result =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				lifecycleProfile,
				placementProfile,
				observations
			);

		Assert.True( result.Succeeded );
		Assert.Equal(
			new[] { 11, 12 },
			result.ImportedLifecycleEvidence
				.Select( item => item.SourceOrdinal )
				.ToArray()
		);
		Assert.Equal(
			new[] { 21, 22 },
			result.ImportedPlacementEvidence
				.Select( item => item.SourceOrdinal )
				.ToArray()
		);
		Assert.Equal( 10, existingLifecycle.SourceOrdinal );
		Assert.Equal( 20, existingPlacement.SourceOrdinal );
		Assert.Contains( existingLifecycle, result.LifecycleProfile.Evidence );
		Assert.Contains( existingPlacement, result.PlacementProfile.Evidence );
		Assert.Equal( 3, result.LifecycleProfile.Evidence.Count );
		Assert.Equal( 3, result.PlacementProfile.Evidence.Count );
	}

	[Fact]
	public void EmptyExistingEvidenceStartsImportedOrdinalsAtZero() {
		PersistentRasterRuntimeObservationSet observations = new(
			new[] {
				new PersistentRasterRuntimeLifecycleObservation(
					PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
					PersistentRasterRuntimeObservationOutcome.Supported,
					"lifecycle",
					int.MaxValue
				),
			},
			new[] {
				new PersistentRasterRuntimePlacementObservation(
					PersistentRasterPlacementSubject.SourceRectangle,
					PersistentRasterRuntimeObservationOutcome.Unsupported,
					"placement",
					int.MaxValue
				),
			}
		);

		PersistentRasterRuntimeIntegrationResult result =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				CreateEmptyLifecycleProfile(),
				CreateEmptyPlacementProfile(),
				observations
			);

		Assert.Equal( 0, Assert.Single( result.ImportedLifecycleEvidence ).SourceOrdinal );
		Assert.Equal( 0, Assert.Single( result.ImportedPlacementEvidence ).SourceOrdinal );
	}

	[Fact]
	public void LifecycleCapacityFailureIsAtomicWhilePlacementStillIntegrates() {
		PersistentRasterLifecycleProfile lifecycleProfile =
			CreateLifecycleProfileAtCapacity();
		PersistentRasterPlacementProfile placementProfile =
			CreateEmptyPlacementProfile();
		PersistentRasterRuntimeObservationSet observations = new(
			new[] {
				new PersistentRasterRuntimeLifecycleObservation(
					PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
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
					0
				),
			}
		);

		PersistentRasterRuntimeIntegrationResult result =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				lifecycleProfile,
				placementProfile,
				observations
			);

		Assert.False( result.Succeeded );
		Assert.Empty( result.ImportedLifecycleEvidence );
		Assert.Equal( lifecycleProfile.Evidence, result.LifecycleProfile.Evidence );
		Assert.Equal( 4096, result.LifecycleProfile.Evidence.Count );
		PersistentRasterRuntimeIntegrationIssue issue = Assert.Single(
			result.Issues,
			item => item.Kind
				== PersistentRasterRuntimeIntegrationIssueKind.LifecycleEvidenceCapacityExhausted
		);
		Assert.Equal( 4096, issue.ExistingEvidenceCount );
		Assert.Equal( 1, issue.RequestedImportCount );
		Assert.Single( result.ImportedPlacementEvidence );
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Supported,
			result.PlacementProfile.SourceRectangle
		);
	}

	[Fact]
	public void PlacementCapacityFailureIsAtomicWhileLifecycleStillIntegrates() {
		PersistentRasterLifecycleProfile lifecycleProfile =
			CreateEmptyLifecycleProfile();
		PersistentRasterPlacementProfile placementProfile =
			CreatePlacementProfileAtCapacity();
		PersistentRasterRuntimeObservationSet observations = new(
			new[] {
				new PersistentRasterRuntimeLifecycleObservation(
					PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
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
					0
				),
			}
		);

		PersistentRasterRuntimeIntegrationResult result =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				lifecycleProfile,
				placementProfile,
				observations
			);

		Assert.False( result.Succeeded );
		Assert.Empty( result.ImportedPlacementEvidence );
		Assert.Equal( placementProfile.Evidence, result.PlacementProfile.Evidence );
		Assert.Equal( 4096, result.PlacementProfile.Evidence.Count );
		PersistentRasterRuntimeIntegrationIssue issue = Assert.Single(
			result.Issues,
			item => item.Kind
				== PersistentRasterRuntimeIntegrationIssueKind.PlacementEvidenceCapacityExhausted
		);
		Assert.Equal( 4096, issue.ExistingEvidenceCount );
		Assert.Equal( 1, issue.RequestedImportCount );
		Assert.Single( result.ImportedLifecycleEvidence );
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Supported,
			result.LifecycleProfile.RasterDisplay
		);
	}

	[Fact]
	public void LifecycleOrdinalFailureIsAtomicWhilePlacementStillIntegrates() {
		PersistentRasterLifecycleEvidence existingLifecycle = new(
			PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
			true,
			PersistentRasterLifecycleEvidenceKind.Verified,
			"existing-lifecycle",
			int.MaxValue
		);
		PersistentRasterLifecycleProfile lifecycleProfile =
			PersistentRasterLifecycleClassifier.Classify(
				new[] { existingLifecycle }
			);
		PersistentRasterRuntimeObservationSet observations = new(
			new[] {
				new PersistentRasterRuntimeLifecycleObservation(
					PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
					PersistentRasterRuntimeObservationOutcome.Supported,
					"runtime-lifecycle",
					0
				),
			},
			new[] {
				new PersistentRasterRuntimePlacementObservation(
					PersistentRasterPlacementSubject.SourceRectangle,
					PersistentRasterRuntimeObservationOutcome.Unsupported,
					"runtime-placement",
					0
				),
			}
		);

		PersistentRasterRuntimeIntegrationResult result =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				lifecycleProfile,
				CreateEmptyPlacementProfile(),
				observations
			);

		Assert.False( result.Succeeded );
		Assert.Empty( result.ImportedLifecycleEvidence );
		Assert.Equal( lifecycleProfile.Evidence, result.LifecycleProfile.Evidence );
		PersistentRasterRuntimeIntegrationIssue issue = Assert.Single(
			result.Issues,
			item => item.Kind
				== PersistentRasterRuntimeIntegrationIssueKind.LifecycleOrdinalSpaceExhausted
		);
		Assert.Equal( 1, issue.ExistingEvidenceCount );
		Assert.Equal( 1, issue.RequestedImportCount );
		Assert.Single( result.ImportedPlacementEvidence );
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unsupported,
			result.PlacementProfile.SourceRectangle
		);
	}

	[Fact]
	public void PlacementOrdinalFailureIsAtomicWhileLifecycleStillIntegrates() {
		PersistentRasterPlacementEvidence existingPlacement = new(
			PersistentRasterPlacementSubject.SourceRectangle,
			true,
			PersistentRasterPlacementEvidenceKind.Verified,
			"existing-placement",
			int.MaxValue
		);
		PersistentRasterPlacementProfile placementProfile =
			PersistentRasterPlacementClassifier.Classify(
				new[] { existingPlacement }
			);
		PersistentRasterRuntimeObservationSet observations = new(
			new[] {
				new PersistentRasterRuntimeLifecycleObservation(
					PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
					PersistentRasterRuntimeObservationOutcome.Unsupported,
					"runtime-lifecycle",
					0
				),
			},
			new[] {
				new PersistentRasterRuntimePlacementObservation(
					PersistentRasterPlacementSubject.SignedZOrder,
					PersistentRasterRuntimeObservationOutcome.Supported,
					"runtime-placement",
					0
				),
			}
		);

		PersistentRasterRuntimeIntegrationResult result =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				CreateEmptyLifecycleProfile(),
				placementProfile,
				observations
			);

		Assert.False( result.Succeeded );
		Assert.Empty( result.ImportedPlacementEvidence );
		Assert.Equal( placementProfile.Evidence, result.PlacementProfile.Evidence );
		PersistentRasterRuntimeIntegrationIssue issue = Assert.Single(
			result.Issues,
			item => item.Kind
				== PersistentRasterRuntimeIntegrationIssueKind.PlacementOrdinalSpaceExhausted
		);
		Assert.Equal( 1, issue.ExistingEvidenceCount );
		Assert.Equal( 1, issue.RequestedImportCount );
		Assert.Single( result.ImportedLifecycleEvidence );
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unsupported,
			result.LifecycleProfile.RasterDisplay
		);
	}

	[Fact]
	public void InconclusiveObservationsDoNotConsumeEvidenceCapacity() {
		PersistentRasterLifecycleProfile lifecycleProfile =
			CreateLifecycleProfileAtCapacity();
		PersistentRasterPlacementProfile placementProfile =
			CreatePlacementProfileAtCapacity();
		PersistentRasterRuntimeLifecycleObservation lifecycleInconclusive = new(
			PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
			PersistentRasterRuntimeObservationOutcome.Inconclusive,
			"lifecycle-inconclusive",
			0
		);
		PersistentRasterRuntimePlacementObservation placementInconclusive = new(
			PersistentRasterPlacementSubject.SourceRectangle,
			PersistentRasterRuntimeObservationOutcome.Inconclusive,
			"placement-inconclusive",
			0
		);
		PersistentRasterRuntimeObservationSet observations = new(
			new[] { lifecycleInconclusive },
			new[] { placementInconclusive }
		);

		PersistentRasterRuntimeIntegrationResult result =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				lifecycleProfile,
				placementProfile,
				observations
			);

		Assert.True( result.Succeeded );
		Assert.Empty( result.Issues );
		Assert.Empty( result.ImportedLifecycleEvidence );
		Assert.Empty( result.ImportedPlacementEvidence );
		Assert.Equal(
			new[] { lifecycleInconclusive },
			result.InconclusiveLifecycleObservations
		);
		Assert.Equal(
			new[] { placementInconclusive },
			result.InconclusivePlacementObservations
		);
		Assert.Equal( lifecycleProfile.Evidence, result.LifecycleProfile.Evidence );
		Assert.Equal( placementProfile.Evidence, result.PlacementProfile.Evidence );
	}

	private static PersistentRasterLifecycleProfile CreateEmptyLifecycleProfile()
		=> PersistentRasterLifecycleClassifier.Classify(
			Array.Empty<PersistentRasterLifecycleEvidence>()
		);

	private static PersistentRasterPlacementProfile CreateEmptyPlacementProfile()
		=> PersistentRasterPlacementClassifier.Classify(
			Array.Empty<PersistentRasterPlacementEvidence>()
		);

	private static PersistentRasterLifecycleProfile CreateLifecycleProfileAtCapacity() {
		PersistentRasterLifecycleEvidence[] evidence = Enumerable.Range( 0, 4096 )
			.Select(
				index => new PersistentRasterLifecycleEvidence(
					PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
					true,
					PersistentRasterLifecycleEvidenceKind.CapabilityDerived,
					$"existing-{index:D4}",
					index
				)
			)
			.ToArray();
		return PersistentRasterLifecycleClassifier.Classify(
			evidence,
			new PersistentRasterLifecycleEvidenceOptions( 4096 )
		);
	}

	private static PersistentRasterPlacementProfile CreatePlacementProfileAtCapacity() {
		PersistentRasterPlacementEvidence[] evidence = Enumerable.Range( 0, 4096 )
			.Select(
				index => new PersistentRasterPlacementEvidence(
					PersistentRasterPlacementSubject.SourceRectangle,
					true,
					PersistentRasterPlacementEvidenceKind.CapabilityDerived,
					$"existing-{index:D4}",
					index
				)
			)
			.ToArray();
		return PersistentRasterPlacementClassifier.Classify(
			evidence,
			new PersistentRasterPlacementEvidenceOptions( 4096 )
		);
	}
}
