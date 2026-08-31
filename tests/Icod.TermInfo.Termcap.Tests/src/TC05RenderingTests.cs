using Icod.TermInfo;
using Icod.TermInfo.Termcap;
using Xunit;

namespace Icod.TermInfo.Termcap.Tests;

public sealed class TC05RenderingTests
{
	[Fact]
	public void RendererUsesCanonicalCodesHistoricalSafeEscapesAndStableOrdering() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "demo" )
				.SetDescription( "Demo terminal" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 80 )
				.SetString(
					StringCapability.ClearScreen,
					"\u001b[H:\\"
				)
				.SetExtendedString( "!!", "vendor" )
				.Build();

		TermcapRenderResult result =
			TermcapRenderer.Render(
				description,
				new TermcapRenderOptions( 512 )
			);

		Assert.True( result.IsRepresentable );
		Assert.False( result.HasErrors );
		Assert.Empty( result.Diagnostics );
		Assert.Equal(
			"demo|Demo terminal:!!=vendor:am:cl=\\E[H\\072\\\\:co#80:\n",
			result.Text
		);
	}

	[Fact]
	public void WrappedOutputParsesWithoutContinuationWhitespace() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "demo" )
				.SetDescription( "Demo terminal" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 132 )
				.SetString(
					StringCapability.ClearScreen,
					"\u001b[H\u001b[2J"
				)
				.Build();

		TermcapRenderResult result =
			TermcapRenderer.Render(
				description,
				new TermcapRenderOptions( 24 )
			);

		Assert.True( result.IsRepresentable );
		string text = Assert.IsType<string>( result.Text );
		Assert.Contains( "\\\n", text );
		Assert.DoesNotContain( "\\\n\t", text );
		TerminalDescription roundTrip =
			ConvertRendered( text );
		Assert.True(
			roundTrip.GetBoolean(
				BooleanCapability.AutoRightMargin
			)
		);
		Assert.Equal(
			132,
			roundTrip.GetNumber(
				NumericCapability.Columns
			)
		);
		Assert.Equal(
			description.GetRequiredString( StringCapability.ClearScreen ),
			roundTrip.GetRequiredString( StringCapability.ClearScreen )
		);
	}

	[Theory]
	[InlineData( "demo|Demo terminal:cm=\\E[%i%d;%dH:" )]
	[InlineData( "demo|Demo terminal:cm=6\\E&a%r%2c%2Y:" )]
	[InlineData( "demo|Demo terminal:cm=\\E[%2;%3H:" )]
	[InlineData( "demo|Demo terminal:cm=%n%B%d;%D%d:" )]
	[InlineData( "demo|Demo terminal:cm=%B%r%i%d;%d:" )]
	[InlineData( "demo|Demo terminal:cm=%>AZ%r%i%d;%d:" )]
	[InlineData( "demo|Demo terminal:cm=%i%r%d:" )]
	public void Tc04ParameterizedProgramsRenderBackToEquivalentRuntimePrograms(
		string source
	) {
		TerminalDescription original = Convert( source );

		TermcapRenderResult rendered =
			TermcapRenderer.Render(
				original,
				new TermcapRenderOptions( 512 )
			);

		Assert.True( rendered.IsRepresentable );
		TerminalDescription roundTrip =
			ConvertRendered(
				Assert.IsType<string>( rendered.Text )
			);
		Assert.Equal(
			original.GetRequiredString( StringCapability.CursorAddress ),
			roundTrip.GetRequiredString( StringCapability.CursorAddress )
		);
	}

	[Fact]
	public void NativeTerminfoIncrementProgramRoundTripsSemantically() {
		const string program = "\u001b[%i%p1%d;%p2%dH";
		TerminalDescription description =
			new TerminalDescriptionBuilder( "demo" )
				.SetDescription( "Demo terminal" )
				.SetString( StringCapability.CursorAddress, program )
				.Build();

		TermcapRenderResult rendered =
			TermcapRenderer.Render(
				description,
				new TermcapRenderOptions( 512 )
			);

		Assert.True( rendered.IsRepresentable );
		TerminalDescription roundTrip =
			ConvertRendered(
				Assert.IsType<string>( rendered.Text )
			);
		string roundTripProgram =
			roundTrip.GetRequiredString(
				StringCapability.CursorAddress
			);
		Assert.Equal(
			TermInfoParameterExpander.Expand( program, 1, 2 ),
			TermInfoParameterExpander.Expand( roundTripProgram, 1, 2 )
		);
		Assert.Equal(
			TermInfoParameterExpander.Expand( program, 24, 80 ),
			TermInfoParameterExpander.Expand( roundTripProgram, 24, 80 )
		);
	}

	[Fact]
	public void LeadingTermcapPaddingRoundTripsThroughRuntimeDelaySuffix() {
		TerminalDescription original =
			Convert(
				"demo|Demo terminal:cl=5.5*\\E[H:"
			);

		TermcapRenderResult rendered =
			TermcapRenderer.Render(
				original,
				new TermcapRenderOptions( 512 )
			);

		Assert.True( rendered.IsRepresentable );
		string text = Assert.IsType<string>( rendered.Text );
		Assert.Contains( "cl=5.5*\\E[H:", text );
		TerminalDescription roundTrip =
			ConvertRendered( text );
		Assert.Equal(
			original.GetRequiredString( StringCapability.ClearScreen ),
			roundTrip.GetRequiredString( StringCapability.ClearScreen )
		);
	}

	[Fact]
	public void NegativeNumericValueFailsPreflightWithoutEmittingText() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "demo" )
				.SetDescription( "Demo terminal" )
				.SetNumber( NumericCapability.Columns, -1 )
				.Build();

		TermcapRepresentabilityResult analysis =
			TermcapRenderer.Analyze( description );
		TermcapRenderResult rendered =
			TermcapRenderer.Render( description );

		Assert.False( analysis.IsRepresentable );
		Assert.True( analysis.HasErrors );
		Assert.True( rendered.HasErrors );
		Assert.Null( rendered.Text );
		Assert.Contains(
			rendered.Diagnostics,
			diagnostic =>
				diagnostic.Code
					== TermcapRenderDiagnosticCodes.NumericValueNotRepresentable
		);
	}

	[Fact]
	public void HeaderDescriptionWithoutWhitespaceFailsPreflight() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "demo" )
				.SetDescription( "Verbose" )
				.Build();

		TermcapRenderResult result =
			TermcapRenderer.Render( description );

		Assert.True( result.HasErrors );
		Assert.Null( result.Text );
		Assert.Contains(
			result.Diagnostics,
			diagnostic =>
				diagnostic.Code
					== TermcapRenderDiagnosticCodes.HeaderNotRepresentable
		);
	}

	[Fact]
	public void ExtendedCodeWhichWouldClassifyAsStandardIsRejected() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "demo" )
				.SetDescription( "Demo terminal" )
				.SetExtendedString( "UP", "vendor" )
				.Build();

		TermcapRenderResult result =
			TermcapRenderer.Render( description );

		Assert.True( result.HasErrors );
		Assert.Null( result.Text );
		Assert.Contains(
			result.Diagnostics,
			diagnostic =>
				diagnostic.Code
					== TermcapRenderDiagnosticCodes.ExtendedCapabilityCollision
		);
	}

	private static TerminalDescription ConvertRendered(
		string source
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( source );
		return Convert( source );
	}

	private static TerminalDescription Convert(
		string source
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( source );

		TermcapSourceParseResult parsed =
			TermcapSourceParser.Parse(
				source,
				"tc05-test"
			);
		Assert.False( parsed.HasErrors );
		TermcapSourceEntry entry =
			Assert.Single( parsed.Document.Entries );
		TermcapSourceResolveResult resolved =
			TermcapSourceResolver.Resolve(
				parsed.Document,
				entry.Names[0]
			);
		Assert.False( resolved.HasErrors );
		TermcapConversionResult converted =
			TermcapConverter.Convert(
				Assert.IsType<TermcapSourceResolvedEntry>( resolved.Entry )
			);
		Assert.False( converted.HasErrors );
		return Assert.IsType<TerminalDescription>( converted.Description );
	}
}
