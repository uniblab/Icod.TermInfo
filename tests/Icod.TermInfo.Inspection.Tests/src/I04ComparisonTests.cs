using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class I04ComparisonTests {
	[Fact]
	public void Compare_Self_HasNoDifferences() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "i04-self" )
				.SetDescription( "I04 self comparison" )
				.AddAlias( "i04-self-alias" )
				.SetBoolean(
					BooleanCapability.AutoRightMargin
				)
				.SetNumber(
					NumericCapability.Columns,
					132
				)
				.SetString(
					StringCapability.Bell,
					"\a"
				)
				.SetExtendedBoolean( "XBool" )
				.SetExtendedNumber( "XNum", 17 )
				.SetExtendedString( "XStr", "value" )
				.Build();

		TermInfoComparisonResult result =
			TerminalDescriptionComparer.Compare(
				description,
				description
			);

		Assert.True( result.AreEqual );
		Assert.Empty( result.Differences );
	}

	[Fact]
	public void Compare_IdentityMetadata_IsStructuredAndOrdered() {
		TerminalDescription left =
			new TerminalDescriptionBuilder( "i04-left" )
				.SetDescription( "left description" )
				.AddAlias( "common" )
				.AddAlias( "left-alias" )
				.Build();
		TerminalDescription right =
			new TerminalDescriptionBuilder( "i04-right" )
				.SetDescription( "right description" )
				.AddAlias( "common" )
				.AddAlias( "right-alias" )
				.Build();

		TermInfoComparisonResult result =
			TerminalDescriptionComparer.Compare(
				left,
				right
			);

		Assert.False( result.AreEqual );
		Assert.Equal( 3, result.Differences.Count );

		TermInfoDifference name =
			result.Differences[ 0 ];
		Assert.Equal(
			TermInfoDifferenceKind.IdentityName,
			name.Kind
		);
		Assert.False( name.IsCapabilityDifference );
		Assert.Null( name.CapabilityName );
		Assert.Null( name.IsExtendedCapability );
		Assert.Equal( "i04-left", name.LeftText );
		Assert.Equal( "i04-right", name.RightText );
		Assert.Null( name.LeftAliases );
		Assert.Null( name.RightAliases );
		Assert.Null( name.LeftCapabilityValue );
		Assert.Null( name.RightCapabilityValue );

		TermInfoDifference aliases =
			result.Differences[ 1 ];
		Assert.Equal(
			TermInfoDifferenceKind.IdentityAliases,
			aliases.Kind
		);
		Assert.False( aliases.IsCapabilityDifference );
		Assert.NotNull( aliases.LeftAliases );
		Assert.Equal(
			new[] {
				"common",
				"left-alias",
			},
			aliases.LeftAliases!.ToArray()
		);
		Assert.NotNull( aliases.RightAliases );
		Assert.Equal(
			new[] {
				"common",
				"right-alias",
			},
			aliases.RightAliases!.ToArray()
		);
		Assert.Null( aliases.LeftText );
		Assert.Null( aliases.RightText );

		TermInfoDifference description =
			result.Differences[ 2 ];
		Assert.Equal(
			TermInfoDifferenceKind.IdentityDescription,
			description.Kind
		);
		Assert.False( description.IsCapabilityDifference );
		Assert.Equal(
			"left description",
			description.LeftText
		);
		Assert.Equal(
			"right description",
			description.RightText
		);
	}

	[Fact]
	public void Compare_StandardCapabilities_UsesCanonicalOrderAndTypedValues() {
		StandardCapabilityMetadata<BooleanCapability> booleanMetadata =
			StandardCapabilityCatalog.BooleanCapabilities[ 0 ];
		StandardCapabilityMetadata<NumericCapability> numericMetadata =
			StandardCapabilityCatalog.NumericCapabilities[ 0 ];
		StandardCapabilityMetadata<StringCapability> stringMetadata =
			StandardCapabilityCatalog.StringCapabilities[ 0 ];

		TerminalDescription left =
			new TerminalDescriptionBuilder( "i04-standard" )
				.SetDescription( "I04 standard comparison" )
				.SetNumber(
					numericMetadata.Capability,
					17
				)
				.SetBoolean(
					booleanMetadata.Capability
				)
				.Build();
		TerminalDescription right =
			new TerminalDescriptionBuilder( "i04-standard" )
				.SetDescription( "I04 standard comparison" )
				.SetString(
					stringMetadata.Capability,
					"right"
				)
				.SetNumber(
					numericMetadata.Capability,
					23
				)
				.Build();

		TermInfoComparisonResult result =
			TerminalDescriptionComparer.Compare(
				left,
				right
			);

		Assert.Equal( 3, result.Differences.Count );

		TermInfoDifference booleanDifference =
			result.Differences[ 0 ];
		Assert.Equal(
			TermInfoDifferenceKind.OnlyInLeft,
			booleanDifference.Kind
		);
		Assert.Equal(
			booleanMetadata.ShortName,
			booleanDifference.CapabilityName
		);
		Assert.True( booleanDifference.IsCapabilityDifference );
		Assert.Equal(
			false,
			booleanDifference.IsExtendedCapability
		);
		Assert.True(
			booleanDifference.LeftCapabilityValue.HasValue
		);
		Assert.True(
			booleanDifference.LeftCapabilityValue.Value.BooleanValue
		);
		Assert.Null(
			booleanDifference.RightCapabilityValue
		);

		TermInfoDifference numericDifference =
			result.Differences[ 1 ];
		Assert.Equal(
			TermInfoDifferenceKind.DifferentValue,
			numericDifference.Kind
		);
		Assert.Equal(
			numericMetadata.ShortName,
			numericDifference.CapabilityName
		);
		Assert.Equal(
			17,
			numericDifference.LeftCapabilityValue!.Value.NumberValue
		);
		Assert.Equal(
			23,
			numericDifference.RightCapabilityValue!.Value.NumberValue
		);

		TermInfoDifference stringDifference =
			result.Differences[ 2 ];
		Assert.Equal(
			TermInfoDifferenceKind.OnlyInRight,
			stringDifference.Kind
		);
		Assert.Equal(
			stringMetadata.ShortName,
			stringDifference.CapabilityName
		);
		Assert.Null(
			stringDifference.LeftCapabilityValue
		);
		Assert.Equal(
			"right",
			stringDifference.RightCapabilityValue!.Value.StringValue
		);
	}

	[Fact]
	public void Compare_ExtendedCapabilities_IsCaseSensitiveAndDeterministic() {
		TerminalDescription left =
			new TerminalDescriptionBuilder( "i04-extended" )
				.SetDescription( "I04 extended comparison" )
				.SetExtendedString( "value", "left" )
				.SetExtendedNumber( "leftOnly", 11 )
				.SetExtendedNumber( "kind", 7 )
				.SetExtendedBoolean( "Feature" )
				.Build();
		TerminalDescription right =
			new TerminalDescriptionBuilder( "i04-extended" )
				.SetDescription( "I04 extended comparison" )
				.SetExtendedString( "kind", "7" )
				.SetExtendedString( "value", "right" )
				.SetExtendedBoolean( "feature" )
				.SetExtendedNumber( "rightOnly", 13 )
				.Build();

		TermInfoComparisonResult result =
			TerminalDescriptionComparer.Compare(
				left,
				right
			);

		Assert.Equal(
			new[] {
				"Feature",
				"feature",
				"kind",
				"leftOnly",
				"rightOnly",
				"value",
			},
			result.Differences
				.Select(
					difference => difference.CapabilityName!
				)
				.ToArray()
		);
		Assert.All(
			result.Differences,
			difference =>
				Assert.Equal(
					true,
					difference.IsExtendedCapability
				)
		);

		Assert.Equal(
			TermInfoDifferenceKind.OnlyInLeft,
			result.Differences[ 0 ].Kind
		);
		Assert.Equal(
			TermInfoDifferenceKind.OnlyInRight,
			result.Differences[ 1 ].Kind
		);
		Assert.Equal(
			TermInfoDifferenceKind.DifferentValueKind,
			result.Differences[ 2 ].Kind
		);
		Assert.Equal(
			TermInfoCapabilityValueKind.Number,
			result.Differences[ 2 ]
				.LeftCapabilityValue!.Value.Kind
		);
		Assert.Equal(
			TermInfoCapabilityValueKind.String,
			result.Differences[ 2 ]
				.RightCapabilityValue!.Value.Kind
		);
		Assert.Equal(
			TermInfoDifferenceKind.OnlyInLeft,
			result.Differences[ 3 ].Kind
		);
		Assert.Equal(
			TermInfoDifferenceKind.OnlyInRight,
			result.Differences[ 4 ].Kind
		);
		Assert.Equal(
			TermInfoDifferenceKind.DifferentValue,
			result.Differences[ 5 ].Kind
		);
	}

	[Fact]
	public void Compare_Reversal_SwapsSidesAndOnlyInKinds() {
		TerminalDescription left =
			new TerminalDescriptionBuilder( "i04-left" )
				.SetDescription( "left description" )
				.AddAlias( "left-alias" )
				.SetBoolean(
					BooleanCapability.AutoRightMargin
				)
				.SetNumber(
					NumericCapability.Columns,
					80
				)
				.SetExtendedNumber( "kind", 1 )
				.SetExtendedString( "value", "left" )
				.SetExtendedBoolean( "leftOnly" )
				.Build();
		TerminalDescription right =
			new TerminalDescriptionBuilder( "i04-right" )
				.SetDescription( "right description" )
				.AddAlias( "right-alias" )
				.SetNumber(
					NumericCapability.Columns,
					132
				)
				.SetExtendedString( "kind", "1" )
				.SetExtendedString( "value", "right" )
				.SetExtendedBoolean( "rightOnly" )
				.Build();

		TermInfoComparisonResult forward =
			TerminalDescriptionComparer.Compare(
				left,
				right
			);
		TermInfoComparisonResult reverse =
			TerminalDescriptionComparer.Compare(
				right,
				left
			);

		Assert.Equal(
			forward.Differences.Count,
			reverse.Differences.Count
		);

		for ( int index = 0; index < forward.Differences.Count; index++ ) {
			TermInfoDifference first =
				forward.Differences[ index ];
			TermInfoDifference second =
				reverse.Differences[ index ];

			Assert.Equal(
				ReverseKind( first.Kind ),
				second.Kind
			);
			Assert.Equal(
				first.CapabilityName,
				second.CapabilityName
			);
			Assert.Equal(
				first.IsExtendedCapability,
				second.IsExtendedCapability
			);
			Assert.Equal(
				first.LeftText,
				second.RightText
			);
			Assert.Equal(
				first.RightText,
				second.LeftText
			);
			AssertAliasSequence(
				first.LeftAliases,
				second.RightAliases
			);
			AssertAliasSequence(
				first.RightAliases,
				second.LeftAliases
			);
			Assert.Equal(
				first.LeftCapabilityValue,
				second.RightCapabilityValue
			);
			Assert.Equal(
				first.RightCapabilityValue,
				second.LeftCapabilityValue
			);
		}
	}

	[Fact]
	public void Compare_DoesNotMutateEitherDescription() {
		TerminalDescription left =
			new TerminalDescriptionBuilder( "i04-stable" )
				.SetDescription( "I04 stable comparison" )
				.SetExtendedString( "z", "left" )
				.SetNumber(
					NumericCapability.Columns,
					80
				)
				.Build();
		TerminalDescription right =
			new TerminalDescriptionBuilder( "i04-stable" )
				.SetDescription( "I04 stable comparison" )
				.SetNumber(
					NumericCapability.Columns,
					132
				)
				.SetExtendedString( "a", "right" )
				.Build();
		string leftBefore =
			TerminalDescriptionSourceRenderer.Render(
				left
			);
		string rightBefore =
			TerminalDescriptionSourceRenderer.Render(
				right
			);

		_ = TerminalDescriptionComparer.Compare(
			left,
			right
		);

		Assert.Equal(
			leftBefore,
			TerminalDescriptionSourceRenderer.Render(
				left
			)
		);
		Assert.Equal(
			rightBefore,
			TerminalDescriptionSourceRenderer.Render(
				right
			)
		);
	}

	[Fact]
	public void Compare_NullArguments_AreRejected() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "i04-null" )
				.SetDescription( "I04 null validation" )
				.Build();

		Assert.Throws<ArgumentNullException>(
			() =>
				TerminalDescriptionComparer.Compare(
					null!,
					description
				)
		);
		Assert.Throws<ArgumentNullException>(
			() =>
				TerminalDescriptionComparer.Compare(
					description,
					null!
				)
		);
	}

	private static void AssertAliasSequence(
		IReadOnlyList<string>? expected,
		IReadOnlyList<string>? actual
	) {
		if ( expected is null ) {
			Assert.Null( actual );
			return;
		}

		Assert.NotNull( actual );
		Assert.Equal(
			expected.ToArray(),
			actual!.ToArray()
		);
	}

	private static TermInfoDifferenceKind ReverseKind(
		TermInfoDifferenceKind kind
	) {
		return kind switch {
			TermInfoDifferenceKind.OnlyInLeft =>
				TermInfoDifferenceKind.OnlyInRight,
			TermInfoDifferenceKind.OnlyInRight =>
				TermInfoDifferenceKind.OnlyInLeft,
			_ => kind,
		};
	}
}
