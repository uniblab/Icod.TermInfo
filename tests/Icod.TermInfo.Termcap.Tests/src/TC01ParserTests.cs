using Icod.TermInfo.Termcap;
using Xunit;

namespace Icod.TermInfo.Termcap.Tests;

public sealed class TC01ParserTests
{
	[Fact]
	public void ParsesBasicTermcapEntryWithoutSemanticConversion() {
		TermcapSourceParseResult result =
			TermcapSourceParser.Parse(
				"vt|vt100|DEC VT100:am:co#80:cl=\\E[H\\E[2J:"
			);

		Assert.False( result.HasErrors );
		TermcapSourceEntry entry =
			Assert.Single( result.Document.Entries );
		Assert.Equal(
			new[] { "vt", "vt100", "DEC VT100" },
			entry.Names
		);
		Assert.Collection(
			entry.Fields,
			field =>
			{
				Assert.Equal( TermcapSourceFieldKind.BooleanCapability, field.Kind );
				Assert.Equal( "am", field.CapabilityName );
			},
			field =>
			{
				Assert.Equal( TermcapSourceFieldKind.NumericCapability, field.Kind );
				Assert.Equal( "co", field.CapabilityName );
				Assert.Equal( 80, field.NumericValue );
			},
			field =>
			{
				Assert.Equal( TermcapSourceFieldKind.StringCapability, field.Kind );
				Assert.Equal( "cl", field.CapabilityName );
				Assert.Equal( "\x1b[H\x1b[2J", field.StringValue );
			}
		);
	}

	[Fact]
	public void ContinuationsAndWhitespaceOnlyFieldsAreAccepted() {
		const string Source =
			"dw|vt52|DEC vt52:\\\n"
			+ "\t:cr=^M:do=^J:\\\n"
			+ "\t:co#80:li#24:\n";

		TermcapSourceParseResult result =
			TermcapSourceParser.Parse(
				Source,
				"fixture.termcap"
			);

		Assert.False( result.HasErrors );
		TermcapSourceEntry entry =
			Assert.Single( result.Document.Entries );
		Assert.Equal( 4, entry.Fields.Count );
		Assert.Equal( "\r", entry.Fields[0].StringValue );
		Assert.Equal( "\n", entry.Fields[1].StringValue );
		Assert.Equal( 80, entry.Fields[2].NumericValue );
		Assert.Equal( 24, entry.Fields[3].NumericValue );
		Assert.Equal( "fixture.termcap", entry.Span.SourceName );
		Assert.True( entry.Span.Length > entry.Fields.Sum( field => field.Text.Length ) );
	}

	[Theory]
	[InlineData( "#1=first" )]
	[InlineData( "%1=second" )]
	[InlineData( "@7=third" )]
	public void HistoricalPunctuationCapabilityNamesRemainSyntacticallyValid(
		string fieldText
	) {
		TermcapSourceParseResult result =
			TermcapSourceParser.Parse(
				$"demo|Demo:{fieldText}:"
			);

		Assert.False( result.HasErrors );
		TermcapSourceField field =
			Assert.Single(
				Assert.Single( result.Document.Entries ).Fields
			);
		Assert.Equal( fieldText[..2], field.CapabilityName );
		Assert.Equal( TermcapSourceFieldKind.StringCapability, field.Kind );
	}

	[Theory]
	[InlineData( "co#80", 80 )]
	[InlineData( "co#0100", 64 )]
	[InlineData( "co#0x50", 80 )]
	public void NumericCapabilitiesAcceptConventionalBases(
		string fieldText,
		int expected
	) {
		TermcapSourceParseResult result =
			TermcapSourceParser.Parse(
				$"demo|Demo:{fieldText}:"
			);

		Assert.False( result.HasErrors );
		TermcapSourceField field =
			Assert.Single(
				Assert.Single( result.Document.Entries ).Fields
			);
		Assert.Equal( TermcapSourceFieldKind.NumericCapability, field.Kind );
		Assert.Equal( expected, field.NumericValue );
	}

	[Theory]
	[InlineData( ".cr=9^M", "cr" )]
	[InlineData( ".ta=8\t", "ta" )]
	public void PeriodPrefixedCapabilitiesRemainDisabledSourceFields(
		string fieldText,
		string expectedName
	) {
		TermcapSourceParseResult result =
			TermcapSourceParser.Parse(
				$"demo|Demo:{fieldText}:"
			);

		Assert.False( result.HasErrors );
		TermcapSourceField field =
			Assert.Single(
				Assert.Single( result.Document.Entries ).Fields
			);
		Assert.Equal( TermcapSourceFieldKind.DisabledCapability, field.Kind );
		Assert.Equal( expectedName, field.CapabilityName );
		Assert.Equal( fieldText, field.Text );
	}

	[Fact]
	public void CancellationAndFinalReferenceRemainUnresolvedFields() {
		TermcapSourceParseResult result =
			TermcapSourceParser.Parse(
				"NZ|aaa-30-nam|no automatic margins:am@:li#30:tc=aaa-30:"
			);

		Assert.False( result.HasErrors );
		TermcapSourceEntry entry =
			Assert.Single( result.Document.Entries );
		Assert.Equal( TermcapSourceFieldKind.CancelledCapability, entry.Fields[0].Kind );
		Assert.Equal( "am", entry.Fields[0].CapabilityName );
		Assert.Equal( TermcapSourceFieldKind.Reference, entry.Fields[2].Kind );
		Assert.Equal( "aaa-30", entry.Fields[2].ReferenceName );
	}

	[Fact]
	public void ReferenceBeforeAnotherCapabilityIsRejected() {
		TermcapSourceParseResult result =
			TermcapSourceParser.Parse(
				"demo|Demo:tc=base:co#80:"
			);

		Assert.True( result.HasErrors );
		Assert.Contains(
			result.Diagnostics,
			diagnostic =>
				diagnostic.Code == TermcapSourceDiagnosticCodes.ReferenceMustBeLast
		);
	}

	[Fact]
	public void OctalColonEscapeDecodesWithoutEndingField() {
		TermcapSourceParseResult result =
			TermcapSourceParser.Parse(
				"demo|Demo:ce=ab\\072cd:"
			);

		Assert.False( result.HasErrors );
		TermcapSourceField field =
			Assert.Single(
				Assert.Single( result.Document.Entries ).Fields
			);
		Assert.Equal( "ab:cd", field.StringValue );
	}

	[Fact]
	public void BackslashColonDoesNotHideCapabilitySeparator() {
		TermcapSourceParseResult result =
			TermcapSourceParser.Parse(
				"demo|Demo:ce=ab\\:cd:"
			);

		Assert.True( result.HasErrors );
		Assert.Contains(
			result.Diagnostics,
			diagnostic =>
				diagnostic.Code == TermcapSourceDiagnosticCodes.IncompleteBackslashEscape
		);
	}

	[Theory]
	[InlineData( "demo|Demo:x:" )]
	[InlineData( "demo|Demo:abc:" )]
	[InlineData( "demo|Demo:a #1:" )]
	public void InvalidCapabilityNamesAreRejected(
		string source
	) {
		TermcapSourceParseResult result =
			TermcapSourceParser.Parse( source );

		Assert.True( result.HasErrors );
		Assert.Contains(
			result.Diagnostics,
			diagnostic =>
				diagnostic.Code == TermcapSourceDiagnosticCodes.InvalidCapabilityName
					|| diagnostic.Code == TermcapSourceDiagnosticCodes.MalformedCapability
		);
	}

	[Fact]
	public void NegativeNumericValueIsRejected() {
		TermcapSourceParseResult result =
			TermcapSourceParser.Parse(
				"demo|Demo:co#-1:"
			);

		Assert.True( result.HasErrors );
		Assert.Contains(
			result.Diagnostics,
			diagnostic =>
				diagnostic.Code == TermcapSourceDiagnosticCodes.InvalidNumericValue
		);
	}

	[Fact]
	public void MaximumSourceLengthFailsBeforePartialParsing() {
		TermcapSourceParserOptions options =
			new(
				maximumSourceLength: 8
			);

		TermcapSourceParseResult result =
			TermcapSourceParser.Parse(
				"demo|Demo:am:",
				options: options
			);

		Assert.True( result.HasErrors );
		Assert.Empty( result.Document.Entries );
		Assert.Contains(
			result.Diagnostics,
			diagnostic =>
				diagnostic.Code == TermcapSourceDiagnosticCodes.MaximumSourceLengthExceeded
		);
	}

	[Fact]
	public void ReaderOverloadUsesTheSameBoundedParser() {
		using StringReader reader =
			new(
				"demo|Demo:am:co#80:"
			);

		TermcapSourceParseResult result =
			TermcapSourceParser.Parse(
				reader,
				"reader.termcap"
			);

		Assert.False( result.HasErrors );
		Assert.Equal(
			"reader.termcap",
			Assert.Single( result.Document.Entries ).Span.SourceName
		);
	}
}
