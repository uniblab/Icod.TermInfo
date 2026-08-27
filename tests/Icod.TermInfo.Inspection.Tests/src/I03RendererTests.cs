using System.Globalization;
using Icod.TermInfo.Inspection;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class I03RendererTests {
	private const string OrderedSource =
		"# I03 comments are lexical trivia and are not preserved.\n"
		+ "i03-base|I03 base terminal,\n"
		+ "    am,\n"
		+ "    cols#0120,\n"
		+ "    clear=^L,\n"
		+ "i03-left|I03 left terminal,\n"
		+ "    lines#24,\n"
		+ "    Vendor=left,\n"
		+ "    use=i03-base,\n"
		+ "i03-right|I03 right terminal,\n"
		+ "    cols#100,\n"
		+ "    Vendor=right,\n"
		+ "i03-child|i03c|I03 child terminal,\n"
		+ "    cols#132,\n"
		+ "    Vendor=local\\,value,\n"
		+ "    use=i03-left,\n"
		+ "    Vendor@,\n"
		+ "    .clear=\\E[?25h,\n"
		+ "    use=i03-right,\n"
		+ "    cols#140,\n"
		+ "    Vendor=last,\n";

	[Fact]
	public void RenderDocument_PreservesOrderedUnresolvedModelAndResolution() {
		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse(
				OrderedSource,
				"i03-ordered.ti"
			);
		Assert.False(
			parsed.HasErrors
		);

		string rendered =
			TermInfoSourceRenderer.Render(
				parsed.Document
			);
		TermInfoSourceParseResult reparsed =
			TermInfoSourceParser.Parse(
				rendered,
				"i03-normalized.ti"
			);

		Assert.False(
			reparsed.HasErrors
		);
		Assert.DoesNotContain(
			"# I03 comments",
			rendered
		);
		Assert.Contains(
			"    cols#80,\n",
			rendered
		);
		Assert.Contains(
			"    clear=\\f,\n",
			rendered
		);
		Assert.Contains(
			"    .clear,\n",
			rendered
		);
		Assert.DoesNotContain(
			".clear=",
			rendered
		);
		AssertStructuredDocumentEquivalent(
			parsed.Document,
			reparsed.Document
		);
		AssertResolvesEquivalent(
			parsed.Document,
			reparsed.Document
		);
	}

	[Fact]
	public void RenderEntry_NormalizesValuesWithoutReorderingFields() {
		const string source =
			"i03-normal|i03n|I03 normalized source,\n"
			+ "    cols#0x50,\n"
			+ "    clear=^L\\,,\n"
			+ "    .cup=\\E[%i%p1%d;%p2%dH,\n"
			+ "    Vendor#010,\n"
			+ "    VendorText=hello\\sworld,\n"
			+ "    High=\\200,\n"
			+ "    Unicode=\u0100,\n";
		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse(
				source,
				"i03-values.ti"
			);
		Assert.False(
			parsed.HasErrors
		);

		TermInfoSourceEntry entry =
			Assert.Single(
				parsed.Document.Entries
			);
		string rendered =
			TermInfoSourceRenderer.Render(
				entry
			);

		Assert.Equal(
			"i03-normal|i03n|I03 normalized source,\n"
				+ "    cols#80,\n"
				+ "    clear=\\f\\,,\n"
				+ "    .cup,\n"
				+ "    Vendor#8,\n"
				+ "    VendorText=hello\\sworld,\n"
				+ "    High=\\200,\n"
				+ "    Unicode=\u0100,\n",
			rendered
		);

		TermInfoSourceParseResult reparsed =
			TermInfoSourceParser.Parse(
				rendered,
				"i03-values-normalized.ti"
			);
		Assert.False(
			reparsed.HasErrors
		);
		AssertStructuredEntryEquivalent(
			entry,
			Assert.Single(
				reparsed.Document.Entries
			)
		);
	}

	[Fact]
	public void RenderEntry_PreservesOneComponentDualAliasDescriptionHeader() {
		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse(
				"i03-dual|i03d,am,",
				"i03-dual.ti"
			);
		Assert.False(
			parsed.HasErrors
		);

		TermInfoSourceEntry entry =
			Assert.Single(
				parsed.Document.Entries
			);
		Assert.Equal(
			"i03d",
			entry.Description
		);
		Assert.Equal(
			new[] { "i03d" },
			entry.Aliases.ToArray()
		);

		string rendered =
			TermInfoSourceRenderer.Render(
				entry
			);
		Assert.Equal(
			"i03-dual|i03d,\n"
				+ "    am,\n",
			rendered
		);

		TermInfoSourceParseResult reparsed =
			TermInfoSourceParser.Parse(
				rendered,
				"i03-dual-normalized.ti"
			);
		Assert.False(
			reparsed.HasErrors
		);
		AssertStructuredEntryEquivalent(
			entry,
			Assert.Single(
				reparsed.Document.Entries
			)
		);
	}

	[Fact]
	public void RenderDocument_UsesDeterministicLayoutAndTextWriterEntryPoint() {
		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse(
				OrderedSource,
				"i03-layout.ti"
			);
		Assert.False(
			parsed.HasErrors
		);

		string first =
			TermInfoSourceRenderer.Render(
				parsed.Document
			);
		string second =
			TermInfoSourceRenderer.Render(
				parsed.Document
			);
		Assert.Equal(
			first,
			second
		);
		Assert.DoesNotContain(
			'\r',
			first
		);
		Assert.Contains(
			"\n\ni03-left|",
			first
		);

		using StringWriter writer =
			new(
				CultureInfo.GetCultureInfo( "de-DE" )
			);
		TermInfoSourceRenderer.Write(
			writer,
			parsed.Document
		);
		Assert.Equal(
			first,
			writer.ToString()
		);

		using StringWriter entryWriter = new();
		TermInfoSourceEntry firstEntry =
			parsed.Document.Entries[ 0 ];
		TermInfoSourceRenderer.Write(
			entryWriter,
			firstEntry
		);
		Assert.Equal(
			TermInfoSourceRenderer.Render(
				firstEntry
			),
			entryWriter.ToString()
		);
	}

	[Fact]
	public void RenderEntry_IsCultureIndependentAndWrapsStringsDeterministically() {
		string longValue =
			new string(
				'x',
				130
			);
		string source =
			"i03-culture|I03 culture fixture,\n"
				+ "    cols#1234567,\n"
				+ "    Vendor="
				+ longValue
				+ ",\n";
		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse(
				source,
				"i03-culture.ti"
			);
		Assert.False(
			parsed.HasErrors
		);
		TermInfoSourceEntry entry =
			Assert.Single(
				parsed.Document.Entries
			);

		CultureInfo originalCulture =
			CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture =
			CultureInfo.CurrentUICulture;

		try {
			CultureInfo.CurrentCulture =
				CultureInfo.GetCultureInfo( "fr-FR" );
			CultureInfo.CurrentUICulture =
				CultureInfo.GetCultureInfo( "fr-FR" );
			string french =
				TermInfoSourceRenderer.Render(
					entry
				);

			CultureInfo.CurrentCulture =
				CultureInfo.GetCultureInfo( "tr-TR" );
			CultureInfo.CurrentUICulture =
				CultureInfo.GetCultureInfo( "tr-TR" );
			string turkish =
				TermInfoSourceRenderer.Render(
					entry
				);

			Assert.Equal(
				french,
				turkish
			);
			Assert.Contains(
				"cols#1234567,",
				french
			);
			Assert.Contains(
				"\n        ",
				french
			);

			TermInfoSourceParseResult reparsed =
				TermInfoSourceParser.Parse(
					french,
					"i03-culture-normalized.ti"
				);
			Assert.False(
				reparsed.HasErrors
			);
			AssertStructuredEntryEquivalent(
				entry,
				Assert.Single(
					reparsed.Document.Entries
				)
			);
		}
		finally {
			CultureInfo.CurrentCulture =
				originalCulture;
			CultureInfo.CurrentUICulture =
				originalUiCulture;
		}
	}

	[Fact]
	public void RenderDocument_EmptyDocumentProducesEmptySource() {
		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse(
				string.Empty,
				"i03-empty.ti"
			);
		Assert.False(
			parsed.HasErrors
		);
		Assert.Empty(
			parsed.Document.Entries
		);
		Assert.Equal(
			string.Empty,
			TermInfoSourceRenderer.Render(
				parsed.Document
			)
		);
	}

	[Fact]
	public void RenderEntry_RejectsFieldWithoutDecodedValue() {
		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse(
				"i03-invalid|I03 invalid numeric fixture,cols#0x,",
				"i03-invalid.ti"
			);
		Assert.True(
			parsed.HasErrors
		);
		TermInfoSourceEntry entry =
			Assert.Single(
				parsed.Document.Entries
			);

		Assert.Throws<InvalidOperationException>(
			() =>
				TermInfoSourceRenderer.Render(
					entry
				)
		);
	}

	[Fact]
	public void Render_ValidatesPublicArguments() {
		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse(
				"i03-args|I03 arguments fixture,am,",
				"i03-args.ti"
			);
		TermInfoSourceEntry entry =
			Assert.Single(
				parsed.Document.Entries
			);

		Assert.Throws<ArgumentNullException>(
			() =>
				TermInfoSourceRenderer.Render(
					(TermInfoSourceEntry)null!
				)
		);
		Assert.Throws<ArgumentNullException>(
			() =>
				TermInfoSourceRenderer.Render(
					(TermInfoSourceDocument)null!
				)
		);
		Assert.Throws<ArgumentNullException>(
			() =>
				TermInfoSourceRenderer.Write(
					null!,
					entry
				)
		);
		Assert.Throws<ArgumentNullException>(
			() =>
				TermInfoSourceRenderer.Write(
					TextWriter.Null,
					(TermInfoSourceEntry)null!
				)
		);
		Assert.Throws<ArgumentNullException>(
			() =>
				TermInfoSourceRenderer.Write(
					null!,
					parsed.Document
				)
		);
		Assert.Throws<ArgumentNullException>(
			() =>
				TermInfoSourceRenderer.Write(
					TextWriter.Null,
					(TermInfoSourceDocument)null!
				)
		);
	}

	private static void AssertStructuredDocumentEquivalent(
		TermInfoSourceDocument expected,
		TermInfoSourceDocument actual
	) {
		ArgumentNullException.ThrowIfNull( expected );
		ArgumentNullException.ThrowIfNull( actual );

		Assert.Equal(
			expected.Entries.Count,
			actual.Entries.Count
		);

		for ( int index = 0; index < expected.Entries.Count; index++ ) {
			AssertStructuredEntryEquivalent(
				expected.Entries[ index ],
				actual.Entries[ index ]
			);
		}
	}

	private static void AssertStructuredEntryEquivalent(
		TermInfoSourceEntry expected,
		TermInfoSourceEntry actual
	) {
		ArgumentNullException.ThrowIfNull( expected );
		ArgumentNullException.ThrowIfNull( actual );

		Assert.Equal(
			expected.CanonicalName,
			actual.CanonicalName
		);
		Assert.Equal(
			expected.Aliases.ToArray(),
			actual.Aliases.ToArray()
		);
		Assert.Equal(
			expected.Description,
			actual.Description
		);
		Assert.Equal(
			expected.Fields.Count,
			actual.Fields.Count
		);

		for ( int index = 0; index < expected.Fields.Count; index++ ) {
			AssertStructuredFieldEquivalent(
				expected.Fields[ index ],
				actual.Fields[ index ]
			);
		}
	}

	private static void AssertStructuredFieldEquivalent(
		TermInfoSourceField expected,
		TermInfoSourceField actual
	) {
		ArgumentNullException.ThrowIfNull( expected );
		ArgumentNullException.ThrowIfNull( actual );

		Assert.Equal( expected.Kind, actual.Kind );
		Assert.Equal( expected.CapabilityName, actual.CapabilityName );
		Assert.Equal( expected.CapabilityClassification, actual.CapabilityClassification );
		Assert.Equal( expected.CanonicalCapabilityName, actual.CanonicalCapabilityName );
		Assert.Equal( expected.StandardValueKind, actual.StandardValueKind );
		Assert.Equal( expected.StandardBooleanCapability, actual.StandardBooleanCapability );
		Assert.Equal( expected.StandardNumericCapability, actual.StandardNumericCapability );
		Assert.Equal( expected.StandardStringCapability, actual.StandardStringCapability );
		Assert.Equal( expected.ReferenceName, actual.ReferenceName );
		Assert.Equal( expected.NumericValue, actual.NumericValue );
		Assert.Equal( expected.StringValue, actual.StringValue );
	}

	private static void AssertResolvesEquivalent(
		TermInfoSourceDocument expectedDocument,
		TermInfoSourceDocument actualDocument
	) {
		ArgumentNullException.ThrowIfNull( expectedDocument );
		ArgumentNullException.ThrowIfNull( actualDocument );

		foreach ( TermInfoSourceEntry entry in expectedDocument.Entries ) {
			TermInfoSourceResolveResult expected =
				TermInfoSourceResolver.Resolve(
					expectedDocument,
					entry.CanonicalName
				);
			TermInfoSourceResolveResult actual =
				TermInfoSourceResolver.Resolve(
					actualDocument,
					entry.CanonicalName
				);

			Assert.False( expected.HasErrors );
			Assert.False( actual.HasErrors );
			Assert.NotNull( expected.Entry );
			Assert.NotNull( actual.Entry );

			string expectedEffective =
				TerminalDescriptionSourceRenderer.Render(
					expected.Entry!.ToTerminalDescription()
				);
			string actualEffective =
				TerminalDescriptionSourceRenderer.Render(
					actual.Entry!.ToTerminalDescription()
				);
			Assert.Equal(
				expectedEffective,
				actualEffective
			);
		}
	}
}
