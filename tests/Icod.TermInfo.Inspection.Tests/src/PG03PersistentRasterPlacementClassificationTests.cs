using System.Globalization;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class PG03PersistentRasterPlacementClassificationTests {
	[Fact]
	public void EmptyEvidenceClassifiesBothSubjectsAsUnknown() {
		PersistentRasterPlacementProfile profile =
			PersistentRasterPlacementClassifier.Classify(
				Array.Empty<PersistentRasterPlacementEvidence>()
			);

		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unknown,
			profile.SourceRectangle
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unknown,
			profile.SignedZOrder
		);
		Assert.Empty( profile.Evidence );
	}

	[Fact]
	public void HighestPrecedenceEvidenceWinsPerSubject() {
		PersistentRasterPlacementProfile profile =
			PersistentRasterPlacementClassifier.Classify(
				new[] {
					new PersistentRasterPlacementEvidence(
						PersistentRasterPlacementSubject.SourceRectangle,
						false,
						PersistentRasterPlacementEvidenceKind.CapabilityDerived,
						"static-negative",
						0
					),
					new PersistentRasterPlacementEvidence(
						PersistentRasterPlacementSubject.SourceRectangle,
						true,
						PersistentRasterPlacementEvidenceKind.Declared,
						"declared-positive",
						1
					),
					new PersistentRasterPlacementEvidence(
						PersistentRasterPlacementSubject.SourceRectangle,
						false,
						PersistentRasterPlacementEvidenceKind.Verified,
						"verified-negative",
						2
					),
				}
			);

		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unsupported,
			profile.SourceRectangle
		);
	}

	[Fact]
	public void EqualHighestPrecedencePositiveAndNegativeEvidenceIsContradicted() {
		PersistentRasterPlacementProfile profile =
			PersistentRasterPlacementClassifier.Classify(
				new[] {
					new PersistentRasterPlacementEvidence(
						PersistentRasterPlacementSubject.SignedZOrder,
						true,
						PersistentRasterPlacementEvidenceKind.Verified,
						"verified-positive",
						0
					),
					new PersistentRasterPlacementEvidence(
						PersistentRasterPlacementSubject.SignedZOrder,
						false,
						PersistentRasterPlacementEvidenceKind.Verified,
						"verified-negative",
						1
					),
				}
			);

		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Contradicted,
			profile.SignedZOrder
		);
	}

	[Fact]
	public void LowerPrecedenceContradictionDoesNotOverrideHigherEvidence() {
		PersistentRasterPlacementProfile profile =
			PersistentRasterPlacementClassifier.Classify(
				new[] {
					new PersistentRasterPlacementEvidence(
						PersistentRasterPlacementSubject.SourceRectangle,
						true,
						PersistentRasterPlacementEvidenceKind.Declared,
						"declared-positive",
						0
					),
					new PersistentRasterPlacementEvidence(
						PersistentRasterPlacementSubject.SourceRectangle,
						false,
						PersistentRasterPlacementEvidenceKind.Declared,
						"declared-negative",
						1
					),
					new PersistentRasterPlacementEvidence(
						PersistentRasterPlacementSubject.SourceRectangle,
						true,
						PersistentRasterPlacementEvidenceKind.Verified,
						"verified-positive",
						2
					),
				}
			);

		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Supported,
			profile.SourceRectangle
		);
		Assert.Equal( 3, profile.Evidence.Count );
	}

	[Fact]
	public void GetStatusRejectsUndefinedSubject() {
		PersistentRasterPlacementProfile profile =
			PersistentRasterPlacementClassifier.Classify(
				Array.Empty<PersistentRasterPlacementEvidence>()
			);

		Assert.Throws<ArgumentOutOfRangeException>(
			() => profile.GetStatus( (PersistentRasterPlacementSubject)99 )
		);
	}

	[Fact]
	public void ClassificationIsCultureIndependentAndRepeatable() {
		PersistentRasterPlacementEvidence[] evidence = [
			new(
				PersistentRasterPlacementSubject.SignedZOrder,
				true,
				PersistentRasterPlacementEvidenceKind.Declared,
				"I",
				1
			),
			new(
				PersistentRasterPlacementSubject.SourceRectangle,
				true,
				PersistentRasterPlacementEvidenceKind.CapabilityDerived,
				"ı",
				0
			),
		];
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
		try {
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "tr-TR" );
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "tr-TR" );
			PersistentRasterPlacementProfile first =
				PersistentRasterPlacementClassifier.Classify( evidence );
			PersistentRasterPlacementProfile second =
				PersistentRasterPlacementClassifier.Classify( evidence.Reverse() );

			Assert.Equal( first.SourceRectangle, second.SourceRectangle );
			Assert.Equal( first.SignedZOrder, second.SignedZOrder );
			Assert.Equal(
				first.Evidence.Select( item => item.SourceLabel ),
				second.Evidence.Select( item => item.SourceLabel )
			);
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}
	}
}
