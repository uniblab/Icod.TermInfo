using Icod.TermInfo;
using Icod.TermInfo.Termcap;
using Xunit;

namespace Icod.TermInfo.Termcap.Tests;

public sealed class TC02ClassificationTests
{
	[Fact]
	public void StandardBooleanMapsToRuntimeIdentity() {
		TermcapCapabilityClassificationResult classification =
			TermcapCapabilityClassifier.Classify(
				ParseSingleField( "am" )
			);

		Assert.Equal(
			TermcapCapabilityClassification.Standard,
			classification.Classification
		);
		Assert.Equal(
			TermInfoCapabilityValueKind.Boolean,
			classification.SourceValueKind
		);
		Assert.Equal(
			TermInfoCapabilityValueKind.Boolean,
			classification.ExpectedValueKind
		);
		Assert.False( classification.HasValueKindMismatch );
		Assert.NotNull( classification.Mapping );
		Assert.Equal(
			BooleanCapability.AutoRightMargin,
			classification.Mapping!.BooleanCapability
		);
		Assert.Equal( "am", classification.Mapping.TermInfoShortName );
	}

	[Fact]
	public void StandardNumericMapsToRuntimeIdentity() {
		TermcapCapabilityClassificationResult classification =
			TermcapCapabilityClassifier.Classify(
				ParseSingleField( "co#132" )
			);

		Assert.Equal(
			TermcapCapabilityClassification.Standard,
			classification.Classification
		);
		Assert.Equal(
			TermInfoCapabilityValueKind.Number,
			classification.SourceValueKind
		);
		Assert.Equal(
			NumericCapability.Columns,
			classification.Mapping!.NumericCapability
		);
		Assert.False( classification.HasValueKindMismatch );
	}

	[Fact]
	public void StandardStringMapsToRuntimeIdentity() {
		TermcapCapabilityClassificationResult classification =
			TermcapCapabilityClassifier.Classify(
				ParseSingleField( "cl=clear" )
			);

		Assert.Equal(
			TermcapCapabilityClassification.Standard,
			classification.Classification
		);
		Assert.Equal(
			TermInfoCapabilityValueKind.String,
			classification.SourceValueKind
		);
		Assert.Equal(
			StringCapability.ClearScreen,
			classification.Mapping!.StringCapability
		);
		Assert.False( classification.HasValueKindMismatch );
	}

	[Fact]
	public void SourceValueKindMismatchRemainsObservable() {
		TermcapCapabilityClassificationResult classification =
			TermcapCapabilityClassifier.Classify(
				ParseSingleField( "co" )
			);

		Assert.Equal(
			TermcapCapabilityClassification.Standard,
			classification.Classification
		);
		Assert.Equal(
			TermInfoCapabilityValueKind.Boolean,
			classification.SourceValueKind
		);
		Assert.Equal(
			TermInfoCapabilityValueKind.Number,
			classification.ExpectedValueKind
		);
		Assert.True( classification.HasValueKindMismatch );
	}

	[Fact]
	public void ObsoleteRuntimeCapabilityIsExplicit() {
		TermcapCapabilityClassificationResult classification =
			TermcapCapabilityClassifier.Classify(
				ParseSingleField( "bs" )
			);

		Assert.Equal(
			TermcapCapabilityClassification.ObsoleteStandard,
			classification.Classification
		);
		Assert.NotNull( classification.Mapping );
		Assert.True( classification.Mapping!.IsObsoleteStandard );
		Assert.False( classification.Mapping.IsObsoleteAlias );
		Assert.Equal(
			BooleanCapability.BackspacesWithBs,
			classification.Mapping.BooleanCapability
		);
		Assert.Equal( "OTbs", classification.Mapping.TermInfoShortName );
	}

	[Fact]
	public void ObsoleteNonStandardAliasMapsThroughCanonicalRuntimeCode() {
		TermcapCapabilityClassificationResult classification =
			TermcapCapabilityClassifier.Classify(
				ParseSingleField( "BO=reverse" )
			);

		Assert.Equal(
			TermcapCapabilityClassification.ObsoleteAlias,
			classification.Classification
		);
		Assert.NotNull( classification.Mapping );
		Assert.True( classification.Mapping!.IsObsoleteAlias );
		Assert.Equal( "AT&T", classification.Mapping.AliasOrigin );
		Assert.Equal( "mr", classification.Mapping.CanonicalTermcapCode );
		Assert.Equal(
			StringCapability.EnterReverseMode,
			classification.Mapping.StringCapability
		);
		Assert.False( classification.HasValueKindMismatch );
	}

	[Fact]
	public void ConflictingHistoricalCodeRemainsAmbiguous() {
		TermcapCapabilityClassificationResult classification =
			TermcapCapabilityClassifier.Classify(
				ParseSingleField( "UP=value" )
			);

		Assert.Equal(
			TermcapCapabilityClassification.Ambiguous,
			classification.Classification
		);
		Assert.Null( classification.Mapping );
		Assert.True( classification.Mappings.Count >= 2 );
		Assert.Contains(
			classification.Mappings,
			mapping =>
				!mapping.IsObsoleteAlias
				&& mapping.TermInfoShortName == "cuu"
		);
		Assert.Contains(
			classification.Mappings,
			mapping =>
				mapping.IsObsoleteAlias
				&& mapping.CanonicalTermcapCode == "ku"
		);
	}

	[Fact]
	public void UnmappedVendorFieldRemainsExplicit() {
		TermcapSourceField field =
			ParseSingleField( "ZZ=vendor" );
		TermcapCapabilityClassificationResult classification =
			TermcapCapabilityClassifier.Classify( field );

		Assert.Same( field, classification.Field );
		Assert.Equal(
			TermcapCapabilityClassification.Unmapped,
			classification.Classification
		);
		Assert.Empty( classification.Mappings );
		Assert.Null( classification.Mapping );
		Assert.Null( classification.ExpectedValueKind );
		Assert.False( classification.HasValueKindMismatch );
	}

	[Fact]
	public void CancellationPreservesTargetIdentityWithoutInventingSourceValueKind() {
		TermcapCapabilityClassificationResult classification =
			TermcapCapabilityClassifier.Classify(
				ParseSingleField( "co@" )
			);

		Assert.Equal(
			TermcapCapabilityClassification.Standard,
			classification.Classification
		);
		Assert.Null( classification.SourceValueKind );
		Assert.Equal(
			TermInfoCapabilityValueKind.Number,
			classification.ExpectedValueKind
		);
		Assert.Equal(
			NumericCapability.Columns,
			classification.Mapping!.NumericCapability
		);
		Assert.False( classification.HasValueKindMismatch );
	}

	[Fact]
	public void DisabledFieldCanStillBeMappedWithoutApplyingItsValue() {
		TermcapCapabilityClassificationResult classification =
			TermcapCapabilityClassifier.Classify(
				ParseSingleField( ".cr=ignored" )
			);

		Assert.Equal(
			TermcapCapabilityClassification.Standard,
			classification.Classification
		);
		Assert.Null( classification.SourceValueKind );
		Assert.Equal(
			TermInfoCapabilityValueKind.String,
			classification.ExpectedValueKind
		);
		Assert.Equal(
			StringCapability.CarriageReturn,
			classification.Mapping!.StringCapability
		);
	}

	[Fact]
	public void InheritanceReferenceIsNotClassifiedAsCapabilityData() {
		TermcapCapabilityClassificationResult classification =
			TermcapCapabilityClassifier.Classify(
				ParseSingleField( "tc=base" )
			);

		Assert.Equal(
			TermcapCapabilityClassification.Reference,
			classification.Classification
		);
		Assert.Empty( classification.Mappings );
		Assert.Null( classification.Mapping );
		Assert.Null( classification.SourceValueKind );
		Assert.Null( classification.ExpectedValueKind );
	}

	private static TermcapSourceField ParseSingleField(
		string fieldText
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( fieldText );

		TermcapSourceParseResult result =
			TermcapSourceParser.Parse(
				$"demo|Demo terminal:{fieldText}:"
			);
		Assert.False( result.HasErrors );
		TermcapSourceEntry entry =
			Assert.Single( result.Document.Entries );
		return Assert.Single( entry.Fields );
	}
}
