using System.Globalization;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RB02RasterBackendClassificationTests {
	[Fact]
	public void ClassifierRejectsUndefinedBackend() {
		Assert.Throws<ArgumentOutOfRangeException>(
			() => RasterBackendClassifier.Classify(
				(RasterBackendKind)99,
				Array.Empty<RasterBackendEvidence>()
			)
		);
	}

	[Fact]
	public void ClassifierRejectsNullEvidence() {
		Assert.Throws<ArgumentNullException>(
			() => RasterBackendClassifier.Classify(
				RasterBackendKind.Sixel,
				null!
			)
		);
	}

	[Fact]
	public void ClassifierRejectsEvidenceForAnotherBackend() {
		RasterBackendEvidence[] evidence = [
			CreateEvidence(
				RasterBackendKind.Sixel,
				true,
				RasterBackendEvidenceKind.Verified,
				"sixel",
				0
			),
			CreateEvidence(
				RasterBackendKind.KittyGraphics,
				true,
				RasterBackendEvidenceKind.Verified,
				"kitty",
				0
			),
		];

		Assert.Throws<ArgumentException>(
			() => RasterBackendClassifier.Classify(
				RasterBackendKind.Sixel,
				evidence
			)
		);
	}

	[Fact]
	public void EmptyEvidenceIsUnknown() {
		RasterBackendProfile profile =
			RasterBackendClassifier.Classify(
				RasterBackendKind.Sixel,
				Array.Empty<RasterBackendEvidence>()
			);

		Assert.Equal( RasterBackendKind.Sixel, profile.Backend );
		Assert.Equal( RasterBackendSupportStatus.Unknown, profile.Status );
		Assert.Empty( profile.Evidence );
	}

	[Theory]
	[InlineData( RasterBackendEvidenceKind.CapabilityDerived )]
	[InlineData( RasterBackendEvidenceKind.Declared )]
	[InlineData( RasterBackendEvidenceKind.Verified )]
	public void PositiveEvidenceAtHighestPresentPrecedenceIsSupported(
		RasterBackendEvidenceKind kind
	) {
		RasterBackendProfile profile =
			RasterBackendClassifier.Classify(
				RasterBackendKind.Sixel,
				[
					CreateEvidence(
						RasterBackendKind.Sixel,
						true,
						kind,
						"positive",
						0
					),
				]
			);

		Assert.Equal( RasterBackendSupportStatus.Supported, profile.Status );
	}

	[Theory]
	[InlineData( RasterBackendEvidenceKind.CapabilityDerived )]
	[InlineData( RasterBackendEvidenceKind.Declared )]
	[InlineData( RasterBackendEvidenceKind.Verified )]
	public void NegativeEvidenceAtHighestPresentPrecedenceIsUnsupported(
		RasterBackendEvidenceKind kind
	) {
		RasterBackendProfile profile =
			RasterBackendClassifier.Classify(
				RasterBackendKind.Sixel,
				[
					CreateEvidence(
						RasterBackendKind.Sixel,
						false,
						kind,
						"negative",
						0
					),
				]
			);

		Assert.Equal( RasterBackendSupportStatus.Unsupported, profile.Status );
	}

	[Theory]
	[InlineData( RasterBackendEvidenceKind.CapabilityDerived )]
	[InlineData( RasterBackendEvidenceKind.Declared )]
	[InlineData( RasterBackendEvidenceKind.Verified )]
	public void BothPolaritiesAtHighestPresentPrecedenceAreContradicted(
		RasterBackendEvidenceKind kind
	) {
		RasterBackendProfile profile =
			RasterBackendClassifier.Classify(
				RasterBackendKind.KittyGraphics,
				[
					CreateEvidence(
						RasterBackendKind.KittyGraphics,
						true,
						kind,
						"positive",
						0
					),
					CreateEvidence(
						RasterBackendKind.KittyGraphics,
						false,
						kind,
						"negative",
						1
					),
				]
			);

		Assert.Equal( RasterBackendSupportStatus.Contradicted, profile.Status );
	}

	[Fact]
	public void VerifiedPositiveOverridesLowerPrecedenceNegativeAndContradiction() {
		RasterBackendEvidence[] evidence = [
			CreateEvidence(
				RasterBackendKind.Sixel,
				false,
				RasterBackendEvidenceKind.CapabilityDerived,
				"capability-negative",
				0
			),
			CreateEvidence(
				RasterBackendKind.Sixel,
				true,
				RasterBackendEvidenceKind.CapabilityDerived,
				"capability-positive",
				1
			),
			CreateEvidence(
				RasterBackendKind.Sixel,
				false,
				RasterBackendEvidenceKind.Declared,
				"declared-negative",
				2
			),
			CreateEvidence(
				RasterBackendKind.Sixel,
				true,
				RasterBackendEvidenceKind.Verified,
				"verified-positive",
				3
			),
		];

		RasterBackendProfile profile =
			RasterBackendClassifier.Classify(
				RasterBackendKind.Sixel,
				evidence
			);

		Assert.Equal( RasterBackendSupportStatus.Supported, profile.Status );
		Assert.Equal( evidence.Length, profile.Evidence.Count );
	}

	[Fact]
	public void DeclaredNegativeOverridesCapabilityDerivedPositive() {
		RasterBackendProfile profile =
			RasterBackendClassifier.Classify(
				RasterBackendKind.Sixel,
				[
					CreateEvidence(
						RasterBackendKind.Sixel,
						true,
						RasterBackendEvidenceKind.CapabilityDerived,
						"capability",
						0
					),
					CreateEvidence(
						RasterBackendKind.Sixel,
						false,
						RasterBackendEvidenceKind.Declared,
						"declared",
						1
					),
				]
			);

		Assert.Equal( RasterBackendSupportStatus.Unsupported, profile.Status );
	}

	[Fact]
	public void ClassificationRetainsCanonicalEvidenceAndIsPermutationAndCultureIndependent() {
		RasterBackendEvidence a = CreateEvidence(
			RasterBackendKind.Sixel,
			true,
			RasterBackendEvidenceKind.Verified,
			"I",
			2
		);
		RasterBackendEvidence b = CreateEvidence(
			RasterBackendKind.Sixel,
			false,
			RasterBackendEvidenceKind.CapabilityDerived,
			"z",
			0
		);
		RasterBackendEvidence c = CreateEvidence(
			RasterBackendKind.Sixel,
			true,
			RasterBackendEvidenceKind.Declared,
			"i",
			1
		);
		RasterBackendEvidence[] expected = [ b, c, a ];

		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
		try {
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "tr-TR" );
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "tr-TR" );
			RasterBackendProfile first =
				RasterBackendClassifier.Classify(
					RasterBackendKind.Sixel,
					[ a, b, c ]
				);

			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "fr-FR" );
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "fr-FR" );
			RasterBackendProfile second =
				RasterBackendClassifier.Classify(
					RasterBackendKind.Sixel,
					[ c, a, b ]
				);

			Assert.Equal( RasterBackendSupportStatus.Supported, first.Status );
			Assert.Equal( first.Status, second.Status );
			Assert.Equal( expected, first.Evidence );
			Assert.Equal( expected, second.Evidence );
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}
	}

	[Fact]
	public void SixelCapabilityNameIsFrozen() {
		Assert.Equal( "Sixel", RasterBackendInspector.SixelCapabilityName );
	}

	[Fact]
	public void ExactSixelBooleanAdvertisementProducesSupportedCapabilityDerivedProfile() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "sixel-terminal" )
				.SetExtendedBoolean( "Sixel" )
				.Build();

		RasterBackendProfile profile =
			RasterBackendInspector.Inspect(
				description,
				RasterBackendKind.Sixel
			);

		Assert.Equal( RasterBackendKind.Sixel, profile.Backend );
		Assert.Equal( RasterBackendSupportStatus.Supported, profile.Status );
		RasterBackendEvidence item = Assert.Single( profile.Evidence );
		Assert.Equal( RasterBackendKind.Sixel, item.Backend );
		Assert.True( item.IsPositive );
		Assert.Equal( RasterBackendEvidenceKind.CapabilityDerived, item.Kind );
		Assert.Equal( "Sixel", item.SourceLabel );
		Assert.Equal( 0, item.SourceOrdinal );
	}

	[Fact]
	public void AbsentSixelAdvertisementRemainsUnknown() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "no-sixel" ).Build();

		RasterBackendProfile profile =
			RasterBackendInspector.Inspect(
				description,
				RasterBackendKind.Sixel
			);

		Assert.Equal( RasterBackendSupportStatus.Unknown, profile.Status );
		Assert.Empty( profile.Evidence );
	}

	[Fact]
	public void NonBooleanSixelAdvertisementIsRejected() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "wrong-kind" )
				.SetExtendedString( "Sixel", "yes" )
				.Build();

		Assert.Throws<InvalidOperationException>(
			() => RasterBackendInspector.Inspect(
				description,
				RasterBackendKind.Sixel
			)
		);
	}

	[Theory]
	[InlineData( "kitty" )]
	[InlineData( "xterm-kitty" )]
	[InlineData( "WindowsTerminal" )]
	[InlineData( "xterm-256color" )]
	public void KittyIsNeverInferredFromNameOrUnrelatedStaticMetadata(
		string terminalName
	) {
		TerminalDescription description =
			new TerminalDescriptionBuilder( terminalName )
				.SetExtendedBoolean( "Sixel" )
				.SetExtendedBoolean( "XT" )
				.Build();

		RasterBackendProfile profile =
			RasterBackendInspector.Inspect(
				description,
				RasterBackendKind.KittyGraphics
			);

		Assert.Equal( RasterBackendKind.KittyGraphics, profile.Backend );
		Assert.Equal( RasterBackendSupportStatus.Unknown, profile.Status );
		Assert.Empty( profile.Evidence );
	}

	[Fact]
	public void InspectorRejectsUndefinedBackend() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "undefined-backend" ).Build();

		Assert.Throws<ArgumentOutOfRangeException>(
			() => RasterBackendInspector.Inspect(
				description,
				(RasterBackendKind)99
			)
		);
	}

	[Fact]
	public void InspectorRejectsNullDescription() {
		Assert.Throws<ArgumentNullException>(
			() => RasterBackendInspector.Inspect(
				null!,
				RasterBackendKind.Sixel
			)
		);
	}

	[Fact]
	public void InspectionProductionAssemblyRemainsTerminalFree() {
		Assert.DoesNotContain(
			typeof( RasterBackendEvidence )
				.Assembly
				.GetReferencedAssemblies(),
			assemblyName => string.Equals(
				assemblyName.Name,
				"Icod.Terminal",
				StringComparison.Ordinal
			)
		);
	}

	private static RasterBackendEvidence CreateEvidence(
		RasterBackendKind backend,
		bool isPositive,
		RasterBackendEvidenceKind kind,
		string sourceLabel,
		int sourceOrdinal
	) {
		return new RasterBackendEvidence(
			backend,
			isPositive,
			kind,
			sourceLabel,
			sourceOrdinal
		);
	}
}
