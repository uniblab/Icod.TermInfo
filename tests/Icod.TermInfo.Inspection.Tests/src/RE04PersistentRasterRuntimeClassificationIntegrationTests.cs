using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RE04PersistentRasterRuntimeClassificationIntegrationTests {
	[Fact]
	public void RuntimeUnsupportedOverridesCapabilityDerivedPositive() {
		PersistentRasterLifecycleProfile lifecycleProfile =
			PersistentRasterLifecycleClassifier.Classify(
				new[] {
					new PersistentRasterLifecycleEvidence(
						PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
						true,
						PersistentRasterLifecycleEvidenceKind.CapabilityDerived,
						"static-capability",
						0
					),
				}
			);
		PersistentRasterRuntimeIntegrationResult result =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				lifecycleProfile,
				CreateEmptyPlacementProfile(),
				new PersistentRasterRuntimeObservationSet(
					new[] {
						new PersistentRasterRuntimeLifecycleObservation(
							PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
							PersistentRasterRuntimeObservationOutcome.Unsupported,
							"runtime",
							0
						),
					},
					Array.Empty<PersistentRasterRuntimePlacementObservation>()
				)
			);

		Assert.True( result.Succeeded );
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unsupported,
			result.LifecycleProfile.RasterDisplay
		);
		PersistentRasterLifecycleEvidence imported =
			Assert.Single( result.ImportedLifecycleEvidence );
		Assert.Equal( PersistentRasterLifecycleEvidenceKind.Verified, imported.Kind );
		Assert.False( imported.IsPositive );
	}

	[Fact]
	public void RuntimeSupportedOverridesDeclaredNegative() {
		PersistentRasterLifecycleProfile lifecycleProfile =
			PersistentRasterLifecycleClassifier.Classify(
				new[] {
					new PersistentRasterLifecycleEvidence(
						PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
						false,
						PersistentRasterLifecycleEvidenceKind.Declared,
						"declared-negative",
						0
					),
				}
			);
		PersistentRasterRuntimeIntegrationResult result =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				lifecycleProfile,
				CreateEmptyPlacementProfile(),
				new PersistentRasterRuntimeObservationSet(
					new[] {
						new PersistentRasterRuntimeLifecycleObservation(
							PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
							PersistentRasterRuntimeObservationOutcome.Supported,
							"runtime",
							0
						),
					},
					Array.Empty<PersistentRasterRuntimePlacementObservation>()
				)
			);

		Assert.True( result.Succeeded );
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Supported,
			result.LifecycleProfile.PersistentUpload
		);
	}

	[Fact]
	public void ExistingVerifiedPositiveAndRuntimeNegativeBecomeContradicted() {
		PersistentRasterLifecycleProfile lifecycleProfile =
			PersistentRasterLifecycleClassifier.Classify(
				new[] {
					new PersistentRasterLifecycleEvidence(
						PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
						true,
						PersistentRasterLifecycleEvidenceKind.Verified,
						"existing-verified-positive",
						0
					),
				}
			);
		PersistentRasterRuntimeIntegrationResult result =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				lifecycleProfile,
				CreateEmptyPlacementProfile(),
				new PersistentRasterRuntimeObservationSet(
					new[] {
						new PersistentRasterRuntimeLifecycleObservation(
							PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
							PersistentRasterRuntimeObservationOutcome.Unsupported,
							"runtime-negative",
							0
						),
					},
					Array.Empty<PersistentRasterRuntimePlacementObservation>()
				)
			);

		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Contradicted,
			result.LifecycleProfile.PlacementCreation
		);
	}

	[Fact]
	public void ExistingVerifiedNegativeAndRuntimePositiveBecomeContradicted() {
		PersistentRasterPlacementProfile placementProfile =
			PersistentRasterPlacementClassifier.Classify(
				new[] {
					new PersistentRasterPlacementEvidence(
						PersistentRasterPlacementSubject.SourceRectangle,
						false,
						PersistentRasterPlacementEvidenceKind.Verified,
						"existing-verified-negative",
						0
					),
				}
			);
		PersistentRasterRuntimeIntegrationResult result =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				CreateEmptyLifecycleProfile(),
				placementProfile,
				new PersistentRasterRuntimeObservationSet(
					Array.Empty<PersistentRasterRuntimeLifecycleObservation>(),
					new[] {
						new PersistentRasterRuntimePlacementObservation(
							PersistentRasterPlacementSubject.SourceRectangle,
							PersistentRasterRuntimeObservationOutcome.Supported,
							"runtime-positive",
							0
						),
					}
				)
			);

		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Contradicted,
			result.PlacementProfile.SourceRectangle
		);
	}

	[Fact]
	public void RepeatedCompatibleRuntimeObservationsRemainConclusive() {
		PersistentRasterRuntimeIntegrationResult result =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				CreateEmptyLifecycleProfile(),
				CreateEmptyPlacementProfile(),
				new PersistentRasterRuntimeObservationSet(
					new[] {
						new PersistentRasterRuntimeLifecycleObservation(
							PersistentRasterLifecycleEvidenceSubject.ResourceDeletion,
							PersistentRasterRuntimeObservationOutcome.Supported,
							"runtime-a",
							0
						),
						new PersistentRasterRuntimeLifecycleObservation(
							PersistentRasterLifecycleEvidenceSubject.ResourceDeletion,
							PersistentRasterRuntimeObservationOutcome.Supported,
							"runtime-b",
							1
						),
					},
					new[] {
						new PersistentRasterRuntimePlacementObservation(
							PersistentRasterPlacementSubject.SignedZOrder,
							PersistentRasterRuntimeObservationOutcome.Unsupported,
							"runtime-a",
							2
						),
						new PersistentRasterRuntimePlacementObservation(
							PersistentRasterPlacementSubject.SignedZOrder,
							PersistentRasterRuntimeObservationOutcome.Unsupported,
							"runtime-b",
							3
						),
					}
				)
			);

		Assert.Equal( 2, result.ImportedLifecycleEvidence.Count );
		Assert.Equal( 2, result.ImportedPlacementEvidence.Count );
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Supported,
			result.LifecycleProfile.ResourceDeletion
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unsupported,
			result.PlacementProfile.SignedZOrder
		);
	}

	[Fact]
	public void InconclusiveOnlyObservationsPreserveExistingSupportStates() {
		PersistentRasterLifecycleProfile lifecycleProfile =
			PersistentRasterLifecycleClassifier.Classify(
				new[] {
					new PersistentRasterLifecycleEvidence(
						PersistentRasterLifecycleEvidenceSubject.MultiplePlacements,
						true,
						PersistentRasterLifecycleEvidenceKind.Declared,
						"declared-positive",
						0
					),
				}
			);
		PersistentRasterPlacementProfile placementProfile =
			PersistentRasterPlacementClassifier.Classify(
				new[] {
					new PersistentRasterPlacementEvidence(
						PersistentRasterPlacementSubject.SignedZOrder,
						false,
						PersistentRasterPlacementEvidenceKind.Declared,
						"declared-negative",
						0
					),
				}
			);
		PersistentRasterRuntimeLifecycleObservation lifecycleObservation = new(
			PersistentRasterLifecycleEvidenceSubject.MultiplePlacements,
			PersistentRasterRuntimeObservationOutcome.Inconclusive,
			"runtime-lifecycle-inconclusive",
			0
		);
		PersistentRasterRuntimePlacementObservation placementObservation = new(
			PersistentRasterPlacementSubject.SignedZOrder,
			PersistentRasterRuntimeObservationOutcome.Inconclusive,
			"runtime-placement-inconclusive",
			1
		);

		PersistentRasterRuntimeIntegrationResult result =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				lifecycleProfile,
				placementProfile,
				new PersistentRasterRuntimeObservationSet(
					new[] { lifecycleObservation },
					new[] { placementObservation }
				)
			);

		Assert.True( result.Succeeded );
		Assert.Empty( result.ImportedLifecycleEvidence );
		Assert.Empty( result.ImportedPlacementEvidence );
		Assert.Equal(
			new[] { lifecycleObservation },
			result.InconclusiveLifecycleObservations
		);
		Assert.Equal(
			new[] { placementObservation },
			result.InconclusivePlacementObservations
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Supported,
			result.LifecycleProfile.MultiplePlacements
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unsupported,
			result.PlacementProfile.SignedZOrder
		);
	}

	[Fact]
	public void LifecycleFailureDoesNotPreventPlacementClassification() {
		PersistentRasterLifecycleProfile lifecycleProfile =
			PersistentRasterLifecycleClassifier.Classify(
				new[] {
					new PersistentRasterLifecycleEvidence(
						PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
						true,
						PersistentRasterLifecycleEvidenceKind.Verified,
						"ordinal-exhausted",
						int.MaxValue
					),
				}
			);
		PersistentRasterRuntimeIntegrationResult result =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				lifecycleProfile,
				CreateEmptyPlacementProfile(),
				new PersistentRasterRuntimeObservationSet(
					new[] {
						new PersistentRasterRuntimeLifecycleObservation(
							PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
							PersistentRasterRuntimeObservationOutcome.Supported,
							"cannot-import",
							0
						),
					},
					new[] {
						new PersistentRasterRuntimePlacementObservation(
							PersistentRasterPlacementSubject.SourceRectangle,
							PersistentRasterRuntimeObservationOutcome.Supported,
							"placement-import",
							1
						),
					}
				)
			);

		Assert.False( result.Succeeded );
		Assert.Empty( result.ImportedLifecycleEvidence );
		Assert.Same( lifecycleProfile, result.LifecycleProfile );
		Assert.Single(
			result.Issues,
			item => item.Kind
				== PersistentRasterRuntimeIntegrationIssueKind.LifecycleOrdinalSpaceExhausted
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Supported,
			result.PlacementProfile.SourceRectangle
		);
	}

	[Fact]
	public void PlacementFailureDoesNotPreventLifecycleClassification() {
		PersistentRasterPlacementProfile placementProfile =
			PersistentRasterPlacementClassifier.Classify(
				new[] {
					new PersistentRasterPlacementEvidence(
						PersistentRasterPlacementSubject.SourceRectangle,
						false,
						PersistentRasterPlacementEvidenceKind.Verified,
						"ordinal-exhausted",
						int.MaxValue
					),
				}
			);
		PersistentRasterRuntimeIntegrationResult result =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				CreateEmptyLifecycleProfile(),
				placementProfile,
				new PersistentRasterRuntimeObservationSet(
					new[] {
						new PersistentRasterRuntimeLifecycleObservation(
							PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
							PersistentRasterRuntimeObservationOutcome.Unsupported,
							"lifecycle-import",
							0
						),
					},
					new[] {
						new PersistentRasterRuntimePlacementObservation(
							PersistentRasterPlacementSubject.SignedZOrder,
							PersistentRasterRuntimeObservationOutcome.Supported,
							"cannot-import",
							1
						),
					}
				)
			);

		Assert.False( result.Succeeded );
		Assert.Empty( result.ImportedPlacementEvidence );
		Assert.Same( placementProfile, result.PlacementProfile );
		Assert.Single(
			result.Issues,
			item => item.Kind
				== PersistentRasterRuntimeIntegrationIssueKind.PlacementOrdinalSpaceExhausted
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unsupported,
			result.LifecycleProfile.RasterDisplay
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
}
