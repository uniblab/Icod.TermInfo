using System.Globalization;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RL03PersistentRasterLifecycleClassificationTests {
	[Fact]
	public void EmptyEvidenceProducesUnknownProfileAndEmptyEvidence() {
		PersistentRasterLifecycleProfile profile =
			PersistentRasterLifecycleClassifier.Classify(
				Array.Empty<PersistentRasterLifecycleEvidence>()
			);

		foreach (
			PersistentRasterLifecycleEvidenceSubject subject
			in Enum.GetValues<PersistentRasterLifecycleEvidenceSubject>()
		) {
			Assert.Equal(
				PersistentRasterLifecycleSupportStatus.Unknown,
				profile.GetStatus( subject )
			);
		}
		Assert.Empty( profile.Evidence );
	}

	[Fact]
	public void EvidenceClassifiesSubjectsIndependently() {
		PersistentRasterLifecycleProfile profile =
			PersistentRasterLifecycleClassifier.Classify(
				new[] {
					CreateEvidence(
						PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
						isPositive: true,
						PersistentRasterLifecycleEvidenceKind.CapabilityDerived,
						"raster",
						sourceOrdinal: 0
					),
					CreateEvidence(
						PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
						isPositive: false,
						PersistentRasterLifecycleEvidenceKind.Declared,
						"upload",
						sourceOrdinal: 1
					),
				}
			);

		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Supported,
			profile.RasterDisplay
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unsupported,
			profile.PersistentUpload
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unknown,
			profile.AcknowledgedUpload
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unknown,
			profile.PlacementCreation
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unknown,
			profile.MultiplePlacements
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unknown,
			profile.PlacementUpdate
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unknown,
			profile.PlacementDeletion
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unknown,
			profile.ResourceDeletion
		);
	}

	[Fact]
	public void SameHighestPrecedencePositiveAndNegativeEvidenceIsContradicted() {
		PersistentRasterLifecycleProfile profile =
			PersistentRasterLifecycleClassifier.Classify(
				new[] {
					CreateEvidence(
						PersistentRasterLifecycleEvidenceSubject.PlacementUpdate,
						isPositive: true,
						PersistentRasterLifecycleEvidenceKind.Declared,
						"declared-positive",
						sourceOrdinal: 0
					),
					CreateEvidence(
						PersistentRasterLifecycleEvidenceSubject.PlacementUpdate,
						isPositive: false,
						PersistentRasterLifecycleEvidenceKind.Declared,
						"declared-negative",
						sourceOrdinal: 1
					),
				}
			);

		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Contradicted,
			profile.PlacementUpdate
		);
	}

	[Fact]
	public void DeclaredEvidenceOutranksCapabilityDerivedEvidenceAndRetainsBoth() {
		PersistentRasterLifecycleEvidence capability = CreateEvidence(
			PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
			isPositive: true,
			PersistentRasterLifecycleEvidenceKind.CapabilityDerived,
			"terminfo",
			sourceOrdinal: 0
		);
		PersistentRasterLifecycleEvidence declared = CreateEvidence(
			PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
			isPositive: false,
			PersistentRasterLifecycleEvidenceKind.Declared,
			"caller",
			sourceOrdinal: 1
		);

		PersistentRasterLifecycleProfile profile =
			PersistentRasterLifecycleClassifier.Classify(
				new[] {
					declared,
					capability,
				}
			);

		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unsupported,
			profile.PersistentUpload
		);
		Assert.Equal(
			new[] {
				capability,
				declared,
			},
			profile.Evidence
		);
	}

	[Fact]
	public void VerifiedEvidenceOutranksDeclaredAndCapabilityDerivedEvidence() {
		PersistentRasterLifecycleEvidence capability = CreateEvidence(
			PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
			isPositive: false,
			PersistentRasterLifecycleEvidenceKind.CapabilityDerived,
			"terminfo",
			sourceOrdinal: 0
		);
		PersistentRasterLifecycleEvidence declared = CreateEvidence(
			PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
			isPositive: false,
			PersistentRasterLifecycleEvidenceKind.Declared,
			"caller",
			sourceOrdinal: 1
		);
		PersistentRasterLifecycleEvidence verified = CreateEvidence(
			PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
			isPositive: true,
			PersistentRasterLifecycleEvidenceKind.Verified,
			"probe",
			sourceOrdinal: 2
		);

		PersistentRasterLifecycleProfile profile =
			PersistentRasterLifecycleClassifier.Classify(
				new[] {
					verified,
					declared,
					capability,
				}
			);

		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Supported,
			profile.PlacementCreation
		);
		Assert.Equal( 3, profile.Evidence.Count );
	}

	[Fact]
	public void VerifiedContradictionRemainsContradictedDespiteLowerEvidence() {
		PersistentRasterLifecycleProfile profile =
			PersistentRasterLifecycleClassifier.Classify(
				new[] {
					CreateEvidence(
						PersistentRasterLifecycleEvidenceSubject.ResourceDeletion,
						isPositive: true,
						PersistentRasterLifecycleEvidenceKind.CapabilityDerived,
						"terminfo",
						sourceOrdinal: 0
					),
					CreateEvidence(
						PersistentRasterLifecycleEvidenceSubject.ResourceDeletion,
						isPositive: true,
						PersistentRasterLifecycleEvidenceKind.Verified,
						"probe-positive",
						sourceOrdinal: 1
					),
					CreateEvidence(
						PersistentRasterLifecycleEvidenceSubject.ResourceDeletion,
						isPositive: false,
						PersistentRasterLifecycleEvidenceKind.Verified,
						"probe-negative",
						sourceOrdinal: 2
					),
				}
			);

		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Contradicted,
			profile.ResourceDeletion
		);
	}

	[Fact]
	public void ProfileRetainsCanonicalImmutableEvidence() {
		PersistentRasterLifecycleEvidence later = CreateEvidence(
			PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
			isPositive: true,
			PersistentRasterLifecycleEvidenceKind.Verified,
			"later",
			sourceOrdinal: 2
		);
		PersistentRasterLifecycleEvidence earlier = CreateEvidence(
			PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
			isPositive: true,
			PersistentRasterLifecycleEvidenceKind.Declared,
			"earlier",
			sourceOrdinal: 1
		);
		List<PersistentRasterLifecycleEvidence> source = [
			later,
			earlier,
		];

		PersistentRasterLifecycleProfile profile =
			PersistentRasterLifecycleClassifier.Classify( source );
		source.Clear();

		Assert.Equal(
			new[] {
				earlier,
				later,
			},
			profile.Evidence
		);
		Assert.False( profile.Evidence is PersistentRasterLifecycleEvidence[] );
		if ( profile.Evidence is IList<PersistentRasterLifecycleEvidence> list ) {
			Assert.True( list.IsReadOnly );
		}
		Assert.All(
			typeof( PersistentRasterLifecycleProfile ).GetProperties(),
			property => Assert.Null( property.SetMethod )
		);
	}

	[Fact]
	public void ClassificationIsCultureAndInputOrderIndependent() {
		PersistentRasterLifecycleEvidence[] evidence = [
			CreateEvidence(
				PersistentRasterLifecycleEvidenceSubject.AcknowledgedUpload,
				isPositive: false,
				PersistentRasterLifecycleEvidenceKind.CapabilityDerived,
				"İ",
				sourceOrdinal: 0
			),
			CreateEvidence(
				PersistentRasterLifecycleEvidenceSubject.AcknowledgedUpload,
				isPositive: true,
				PersistentRasterLifecycleEvidenceKind.Declared,
				"I",
				sourceOrdinal: 0
			),
			CreateEvidence(
				PersistentRasterLifecycleEvidenceSubject.MultiplePlacements,
				isPositive: true,
				PersistentRasterLifecycleEvidenceKind.Verified,
				"i",
				sourceOrdinal: 0
			),
		];
		PersistentRasterLifecycleProfile baseline =
			PersistentRasterLifecycleClassifier.Classify( evidence );

		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
		try {
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "tr-TR" );
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "tr-TR" );
			PersistentRasterLifecycleProfile repeated =
				PersistentRasterLifecycleClassifier.Classify(
					evidence.Reverse()
				);

			foreach (
				PersistentRasterLifecycleEvidenceSubject subject
				in Enum.GetValues<PersistentRasterLifecycleEvidenceSubject>()
			) {
				Assert.Equal(
					baseline.GetStatus( subject ),
					repeated.GetStatus( subject )
				);
			}
			Assert.Equal(
				baseline.Evidence.Select( item => item.SourceLabel ),
				repeated.Evidence.Select( item => item.SourceLabel )
			);
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}
	}

	[Fact]
	public void ClassifierHonorsEvidenceBounds() {
		Assert.Throws<ArgumentException>(
			() => PersistentRasterLifecycleClassifier.Classify(
				new[] {
					CreateEvidence(
						PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
						isPositive: true,
						PersistentRasterLifecycleEvidenceKind.Declared,
						"one",
						sourceOrdinal: 0
					),
					CreateEvidence(
						PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
						isPositive: true,
						PersistentRasterLifecycleEvidenceKind.Declared,
						"two",
						sourceOrdinal: 1
					),
				},
				new PersistentRasterLifecycleEvidenceOptions(
					maximumEvidenceCount: 1
				)
			)
		);
	}

	[Fact]
	public void ProfileGetStatusRejectsUndefinedSubject() {
		PersistentRasterLifecycleProfile profile =
			PersistentRasterLifecycleClassifier.Classify(
				Array.Empty<PersistentRasterLifecycleEvidence>()
			);

		Assert.Throws<ArgumentOutOfRangeException>(
			() => {
				_ = profile.GetStatus(
					(PersistentRasterLifecycleEvidenceSubject)99
				);
			}
		);
	}

	private static PersistentRasterLifecycleEvidence CreateEvidence(
		PersistentRasterLifecycleEvidenceSubject subject,
		bool isPositive,
		PersistentRasterLifecycleEvidenceKind kind,
		string sourceLabel,
		int sourceOrdinal
	) => new(
		subject,
		isPositive,
		kind,
		sourceLabel,
		sourceOrdinal
	);
}
