using Icod.TermInfo;
using Icod.TermInfo.Termcap;
using Xunit;

namespace Icod.TermInfo.Termcap.Tests;

public sealed class TC04ConversionTests
{
	[Fact]
	public void StandardAndExtendedFieldsConvertIntoRuntimeModel() {
		TermcapConversionResult result =
			Convert(
				"vt|vt100|DEC VT100:am:co#80:cl=\\E[H\\E[2J:!!=vendor:"
			);

		Assert.False( result.HasErrors );
		Assert.False( result.HasLoss );
		Assert.True( result.IsLossless );
		TerminalDescription description =
			Assert.IsType<TerminalDescription>( result.Description );
		Assert.Equal( "vt", description.Name );
		Assert.Equal( "DEC VT100", description.Description );
		Assert.Equal(
			new[] { "vt100" },
			description.Aliases
		);
		Assert.True(
			description.GetBoolean(
				BooleanCapability.AutoRightMargin
			)
		);
		Assert.Equal(
			80,
			description.GetNumber(
				NumericCapability.Columns
			)
		);
		Assert.Equal(
			"\u001b[H\u001b[2J",
			description.GetString(
				StringCapability.ClearScreen
			)
		);
		Assert.True(
			description.ExtendedCapabilities.TryGetValue(
				"!!",
				out TermInfoCapabilityValue extended
			)
		);
		Assert.True( extended.IsString );
		Assert.Equal( "vendor", extended.StringValue );
		TermcapConversionDiagnostic diagnostic =
			Assert.Single( result.Diagnostics );
		Assert.Equal(
			TermcapConversionDiagnosticCodes.UnmappedExtendedCapability,
			diagnostic.Code
		);
		Assert.Equal(
			TermcapConversionDecision.Extended,
			diagnostic.Decision
		);
	}

	[Fact]
	public void HistoricalAliasMapsLosslesslyAndRemainsObservable() {
		TermcapConversionResult result =
			Convert(
				"demo|Demo terminal:BO=reverse:"
			);

		Assert.False( result.HasErrors );
		Assert.False( result.HasLoss );
		Assert.True( result.IsLossless );
		TerminalDescription description =
			Assert.IsType<TerminalDescription>( result.Description );
		Assert.Equal(
			"reverse",
			description.GetString(
				StringCapability.EnterReverseMode
			)
		);
		TermcapConversionDiagnostic diagnostic =
			Assert.Single( result.Diagnostics );
		Assert.Equal(
			TermcapConversionDiagnosticCodes.HistoricalAlias,
			diagnostic.Code
		);
		Assert.Equal(
			TermcapConversionDecision.HistoricalAlias,
			diagnostic.Decision
		);
		Assert.Equal(
			TermcapConversionDiagnosticSeverity.Information,
			diagnostic.Severity
		);
	}

	[Fact]
	public void SourceValueKindMismatchFailsWithoutPartialDescription() {
		TermcapConversionResult result =
			Convert(
				"demo|Demo terminal:co:"
			);

		Assert.True( result.HasErrors );
		Assert.True( result.HasLoss );
		Assert.Null( result.Description );
		TermcapConversionDiagnostic diagnostic =
			Assert.Single( result.Diagnostics );
		Assert.Equal(
			TermcapConversionDiagnosticCodes.ValueKindMismatch,
			diagnostic.Code
		);
		Assert.Equal(
			TermcapConversionDecision.Unrepresentable,
			diagnostic.Decision
		);
	}

	[Fact]
	public void AmbiguousHistoricalCodeFailsWithoutGuessing() {
		TermcapConversionResult result =
			Convert(
				"demo|Demo terminal:UP=value:"
			);

		Assert.True( result.HasErrors );
		Assert.Null( result.Description );
		TermcapConversionDiagnostic diagnostic =
			Assert.Single( result.Diagnostics );
		Assert.Equal(
			TermcapConversionDiagnosticCodes.AmbiguousCapability,
			diagnostic.Code
		);
		Assert.Equal(
			TermcapConversionDecision.Unsupported,
			diagnostic.Decision
		);
	}

	[Fact]
	public void HigherPrioritySemanticMappingWinsWhenAliasesCollide() {
		TermcapConversionResult result =
			Convert(
				"demo|Demo terminal:mr=canonical:BO=alias:"
			);

		Assert.False( result.HasErrors );
		Assert.True( result.HasLoss );
		TerminalDescription description =
			Assert.IsType<TerminalDescription>( result.Description );
		Assert.Equal(
			"canonical",
			description.GetString(
				StringCapability.EnterReverseMode
			)
		);
		Assert.Contains(
			result.Diagnostics,
			diagnostic =>
				diagnostic.Code
					== TermcapConversionDiagnosticCodes.DuplicateSemanticCapability
				&& diagnostic.Decision
					== TermcapConversionDecision.Approximation
		);
	}

	[Fact]
	public void ClassicCursorAddressProgramTranslatesToRuntimeParameterSyntax() {
		TermcapConversionResult result =
			Convert(
				"demo|Demo terminal:cm=\\E[%i%d;%dH:"
			);

		Assert.False( result.HasErrors );
		TerminalDescription description =
			Assert.IsType<TerminalDescription>( result.Description );
		string program =
			description.GetRequiredString(
				StringCapability.CursorAddress
			);
		Assert.Equal(
			"\u001b[%p1%{1}%+%d;%p2%{1}%+%dH",
			program
		);
		Assert.Equal(
			"\u001b[2;3H",
			TermInfoParameterExpander.Expand(
				program,
				1,
				2
			)
		);
	}

	[Fact]
	public void ClassicParameterReverseOperatorTranslatesExactly() {
		TermcapConversionResult result =
			Convert(
				"demo|Demo terminal:cm=\\E[%r%d;%dH:"
			);

		Assert.False( result.HasErrors );
		TerminalDescription description =
			Assert.IsType<TerminalDescription>( result.Description );
		string program =
			description.GetRequiredString(
				StringCapability.CursorAddress
			);
		Assert.Equal(
			"\u001b[%p2%d;%p1%dH",
			program
		);
		Assert.Equal(
			"\u001b[2;1H",
			TermInfoParameterExpander.Expand(
				program,
				1,
				2
			)
		);
	}

	[Fact]
	public void ClassicFixedWidthOperatorsPreserveZeroPaddedTermcapSemantics() {
		TermcapConversionResult result =
			Convert(
				"demo|Demo terminal:cm=6\\E&a%r%2c%2Y:"
			);

		Assert.False( result.HasErrors );
		TerminalDescription description =
			Assert.IsType<TerminalDescription>( result.Description );
		string program =
			description.GetRequiredString(
				StringCapability.CursorAddress
			);
		Assert.Equal(
			"\u001b&a%p2%{100}%m%02dc%p1%{100}%m%02dY$<6/>",
			program
		);
		Assert.Equal(
			"\u001b&a12c03Y$<6/>",
			TermInfoParameterExpander.Expand(
				program,
				3,
				12
			)
		);
	}

	[Fact]
	public void ClassicFixedWidthOperatorsTruncateToHistoricalFieldWidth() {
		TermcapConversionResult result =
			Convert(
				"demo|Demo terminal:cm=\\E[%2;%3H:"
			);

		Assert.False( result.HasErrors );
		TerminalDescription description =
			Assert.IsType<TerminalDescription>( result.Description );
		string program =
			description.GetRequiredString(
				StringCapability.CursorAddress
			);
		Assert.Equal(
			"\u001b[%p1%{100}%m%02d;%p2%{1000}%m%03dH",
			program
		);
		Assert.Equal(
			"\u001b[23;234H",
			TermInfoParameterExpander.Expand(
				program,
				123,
				1234
			)
		);
	}

	[Fact]
	public void KnownParameterSyntaxOutsideAdoptedProfileFailsExplicitly() {
		TermcapConversionResult result =
			Convert(
				"demo|Demo terminal:sa=%d:"
			);

		Assert.True( result.HasErrors );
		Assert.True( result.HasLoss );
		Assert.Null( result.Description );
		Assert.Contains(
			result.Diagnostics,
			diagnostic =>
				diagnostic.Code
					== TermcapConversionDiagnosticCodes.UnsupportedParameterizedCapability
				&& diagnostic.Decision
					== TermcapConversionDecision.Unsupported
		);
	}

	[Fact]
	public void PercentInNonParameterizedStringRemainsLiteral() {
		TermcapConversionResult result =
			Convert(
				"demo|Demo terminal:cl=done 100% complete:"
			);

		Assert.False( result.HasErrors );
		TerminalDescription description =
			Assert.IsType<TerminalDescription>( result.Description );
		Assert.Equal(
			"done 100% complete",
			description.GetRequiredString(
				StringCapability.ClearScreen
			)
		);
	}

	[Fact]
	public void LeadingTermcapPaddingBecomesTerminfoDelaySuffix() {
		TermcapConversionResult result =
			Convert(
				"demo|Demo terminal:cl=5.5*\\E[H:"
			);

		Assert.False( result.HasErrors );
		TerminalDescription description =
			Assert.IsType<TerminalDescription>( result.Description );
		Assert.Equal(
			"\u001b[H$<5.5*/>",
			description.GetRequiredString(
				StringCapability.ClearScreen
			)
		);
	}

	[Fact]
	public void UnsupportedParameterOperatorIsReportedAsUnrepresentable() {
		TermcapConversionResult result =
			Convert(
				"demo|Demo terminal:cm=\\E[%f%d:"
			);

		Assert.True( result.HasErrors );
		Assert.True( result.HasLoss );
		Assert.Null( result.Description );
		Assert.Contains(
			result.Diagnostics,
			diagnostic =>
				diagnostic.Code
					== TermcapConversionDiagnosticCodes.UnsupportedParameterOperator
				&& diagnostic.Decision
					== TermcapConversionDecision.Unrepresentable
		);
	}

	[Fact]
	public void ResolvedCancellationMaterializesAsRuntimeAbsence() {
		TermcapSourceDocument document =
			ParseDocument(
				"base|Base terminal:co#80:\n"
				+ "child|Child terminal:co@:tc=base:\n"
			);
		TermcapSourceResolveResult resolved =
			TermcapSourceResolver.Resolve(
				document,
				"child"
			);
		Assert.False( resolved.HasErrors );

		TermcapConversionResult converted =
			TermcapConverter.Convert(
				Assert.IsType<TermcapSourceResolvedEntry>( resolved.Entry )
			);

		Assert.False( converted.HasErrors );
		TerminalDescription description =
			Assert.IsType<TerminalDescription>( converted.Description );
		Assert.Null(
			description.GetNumber(
				NumericCapability.Columns
			)
		);
	}

	[Fact]
	public void HeaderWithoutVerboseNameKeepsAllRemainingComponentsAsAliases() {
		TermcapConversionResult result =
			Convert(
				"vt|vt100|vt100-am:am:"
			);

		Assert.False( result.HasErrors );
		TerminalDescription description =
			Assert.IsType<TerminalDescription>( result.Description );
		Assert.Equal( "vt", description.Name );
		Assert.Null( description.Description );
		Assert.Equal(
			new[] { "vt100", "vt100-am" },
			description.Aliases
		);
	}

	private static TermcapConversionResult Convert(
		string source
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( source );

		TermcapSourceDocument document =
			ParseDocument( source );
		TermcapSourceResolveResult resolved =
			TermcapSourceResolver.Resolve(
				document,
				document.Entries[0].Names[0]
			);
		Assert.False( resolved.HasErrors );
		return TermcapConverter.Convert(
			Assert.IsType<TermcapSourceResolvedEntry>( resolved.Entry )
		);
	}

	private static TermcapSourceDocument ParseDocument(
		string source
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( source );

		TermcapSourceParseResult parsed =
			TermcapSourceParser.Parse(
				source,
				"tc04-test"
			);
		Assert.False( parsed.HasErrors );
		return parsed.Document;
	}
}
