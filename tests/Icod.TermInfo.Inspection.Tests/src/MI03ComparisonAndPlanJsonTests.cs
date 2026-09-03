using System.Globalization;
using System.Text;
using System.Text.Json;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class MI03ComparisonAndPlanJsonTests {
	private const string DevelopmentVersion = "1.9.0-Alpha-3";

	[Theory]
	[InlineData( false, "comparison.compact.json" )]
	[InlineData( true, "comparison.indented.json" )]
	public void ComparisonRenderingMatchesCheckedInFixtureExactly(
		bool writeIndented,
		string fixtureName
	) {
		string actual =
			TermInfoJsonRenderer.Render(
				CreateFixtureComparison(),
				new TermInfoJsonRendererOptions(
					TermInfoJsonRendererOptions.DefaultMaximumOutputByteCount,
					writeIndented
				)
			);

		Assert.Equal(
			ReadFixtureWithoutRepositoryLineTerminator( fixtureName ),
			actual
		);
		Assert.DoesNotContain( "\r", actual, StringComparison.Ordinal );
		Assert.False( actual.EndsWith( "\n", StringComparison.Ordinal ) );
	}

	[Theory]
	[InlineData( false, "source-plan.compact.json" )]
	[InlineData( true, "source-plan.indented.json" )]
	public void SourcePlanRenderingMatchesCheckedInFixtureExactly(
		bool writeIndented,
		string fixtureName
	) {
		string actual =
			TermInfoJsonRenderer.Render(
				CreateFixturePlan(),
				new TermInfoJsonRendererOptions(
					TermInfoJsonRendererOptions.DefaultMaximumOutputByteCount,
					writeIndented
				)
			);

		Assert.Equal(
			ReadFixtureWithoutRepositoryLineTerminator( fixtureName ),
			actual
		);
		Assert.DoesNotContain( "\r", actual, StringComparison.Ordinal );
		Assert.False( actual.EndsWith( "\n", StringComparison.Ordinal ) );
	}

	[Fact]
	public void EffectiveComparisonPreservesOrderKindsAndTypedSideValues() {
		TerminalDescription left =
			new TerminalDescriptionBuilder( "mi03-left" )
				.AddAlias( "left-alias" )
				.SetDescription( "Left description" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 80 )
				.SetString( StringCapability.Bell, "\a" )
				.SetExtendedBoolean( "ABool" )
				.SetExtendedNumber( "AKind", 7 )
				.Build();
		TerminalDescription right =
			new TerminalDescriptionBuilder( "mi03-right" )
				.AddAlias( "right-alias" )
				.SetBoolean( BooleanCapability.AutoLeftMargin )
				.SetNumber( NumericCapability.Columns, 132 )
				.SetString( StringCapability.Bell, "\b" )
				.SetExtendedString( "AKind", "seven" )
				.SetExtendedString( "ZString", "right" )
				.Build();
		TermInfoComparisonResult comparison =
			TerminalDescriptionComparer.Compare(
				left,
				right
			);

		using JsonDocument document =
			JsonDocument.Parse(
				TermInfoJsonRenderer.Render( comparison )
			);
		JsonElement root = document.RootElement;
		Assert.Equal(
			new[] { "schema", "schemaVersion", "documentKind", "data" },
			GetPropertyNames( root )
		);
		Assert.Equal( "comparison", root.GetProperty( "documentKind" ).GetString() );
		JsonElement data = root.GetProperty( "data" );
		Assert.False( data.GetProperty( "areEqual" ).GetBoolean() );
		JsonElement[] differences =
			data.GetProperty( "differences" ).EnumerateArray().ToArray();
		Assert.Equal(
			new[] {
				"identityName",
				"identityAliases",
				"identityDescription",
				"onlyInRight",
				"onlyInLeft",
				"differentValue",
				"differentValue",
				"onlyInLeft",
				"differentValueKind",
				"onlyInRight",
			},
			differences
				.Select( difference => difference.GetProperty( "kind" ).GetString() )
				.ToArray()
		);
		Assert.Equal(
			new[] { "bw", "am", "cols", "bel", "ABool", "AKind", "ZString" },
			differences
				.Skip( 3 )
				.Select( difference => difference.GetProperty( "capabilityName" ).GetString() )
				.ToArray()
		);
		JsonElement kindMismatch = differences[ 8 ];
		Assert.True( kindMismatch.GetProperty( "isExtendedCapability" ).GetBoolean() );
		Assert.Equal(
			"number",
			kindMismatch
				.GetProperty( "left" )
				.GetProperty( "capabilityValue" )
				.GetProperty( "kind" )
				.GetString()
		);
		Assert.Equal(
			7,
			kindMismatch
				.GetProperty( "left" )
				.GetProperty( "capabilityValue" )
				.GetProperty( "value" )
				.GetInt32()
		);
		Assert.Equal(
			"string",
			kindMismatch
				.GetProperty( "right" )
				.GetProperty( "capabilityValue" )
				.GetProperty( "kind" )
				.GetString()
		);
		Assert.Equal(
			"seven",
			kindMismatch
				.GetProperty( "right" )
				.GetProperty( "capabilityValue" )
				.GetProperty( "value" )
				.GetString()
		);
	}

	[Fact]
	public void SourceComparisonPreservesEntryFieldIndexAndSpanEvidence() {
		TermInfoSourceDocument leftDocument =
			ParseDocument(
				"entry|MI03 source,use=left-base,cols#80,",
				"mi03-left.ti"
			);
		TermInfoSourceDocument rightDocument =
			ParseDocument(
				"entry|MI03 source,use=right-base,cols#132,",
				"mi03-right.ti"
			);
		TermInfoSourceEntry left = Assert.Single( leftDocument.Entries );
		TermInfoSourceEntry right = Assert.Single( rightDocument.Entries );
		TermInfoComparisonResult comparison =
			TermInfoSourceComparer.Compare(
				leftDocument,
				rightDocument
			);

		using JsonDocument document =
			JsonDocument.Parse(
				TermInfoJsonRenderer.Render( comparison )
			);
		JsonElement[] differences =
			document.RootElement
				.GetProperty( "data" )
				.GetProperty( "differences" )
				.EnumerateArray()
				.ToArray();
		Assert.Equal( 2, differences.Length );
		Assert.Equal( "sourceUseReference", differences[ 0 ].GetProperty( "kind" ).GetString() );
		Assert.Equal( "sourceFieldValue", differences[ 1 ].GetProperty( "kind" ).GetString() );

		JsonElement leftSide = differences[ 0 ].GetProperty( "left" );
		JsonElement leftEntry = leftSide.GetProperty( "sourceEntry" );
		JsonElement leftField = leftSide.GetProperty( "sourceField" );
		Assert.Equal( "entry", leftEntry.GetProperty( "canonicalName" ).GetString() );
		Assert.Equal( 2, leftEntry.GetProperty( "fieldCount" ).GetInt32() );
		Assert.Equal( 0, leftSide.GetProperty( "sourceEntryIndex" ).GetInt32() );
		Assert.Equal( 0, leftSide.GetProperty( "sourceFieldIndex" ).GetInt32() );
		Assert.Equal( "useReference", leftField.GetProperty( "kind" ).GetString() );
		Assert.Equal( "left-base", leftField.GetProperty( "referenceName" ).GetString() );
		Assert.Equal( "use=left-base", leftField.GetProperty( "text" ).GetString() );
		Assert.Equal(
			"mi03-left.ti",
			leftSide
				.GetProperty( "sourceSpan" )
				.GetProperty( "sourceName" )
				.GetString()
		);
		Assert.Equal(
			left.Fields[ 0 ].Span.Offset,
			leftSide
				.GetProperty( "sourceSpan" )
				.GetProperty( "offset" )
				.GetInt32()
		);
		Assert.Equal(
			left.Fields[ 0 ].Span.EndOffset,
			leftSide
				.GetProperty( "sourceSpan" )
				.GetProperty( "endOffset" )
				.GetInt32()
		);

		JsonElement numericField = differences[ 1 ].GetProperty( "right" ).GetProperty( "sourceField" );
		Assert.Equal( "numericCapability", numericField.GetProperty( "kind" ).GetString() );
		Assert.Equal( "standard", numericField.GetProperty( "capabilityClassification" ).GetString() );
		Assert.Equal( "cols", numericField.GetProperty( "canonicalCapabilityName" ).GetString() );
		Assert.Equal( "number", numericField.GetProperty( "standardValueKind" ).GetString() );
		Assert.Equal( 132, numericField.GetProperty( "numericValue" ).GetInt32() );
	}

	[Fact]
	public void SourcePlanCorrespondsExactlyToManagedResultAndScore() {
		TerminalDescription target =
			new TerminalDescriptionBuilder( "mi03-child" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
		TerminalDescription parent =
			new TerminalDescriptionBuilder( "mi03-parent" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
		TerminalDescriptionSourcePlan plan =
			TerminalDescriptionSourcePlanner.Plan(
				target,
				new[] {
					new TerminalDescriptionSourceSynthesisParent(
						"mi03-base",
						parent
					),
				}
			);

		using JsonDocument document =
			JsonDocument.Parse(
				TermInfoJsonRenderer.Render( plan )
			);
		JsonElement root = document.RootElement;
		Assert.Equal( "sourcePlan", root.GetProperty( "documentKind" ).GetString() );
		JsonElement data = root.GetProperty( "data" );
		Assert.Equal(
			new[] {
				"selectedParentCount",
				"selectedParentUseNames",
				"source",
				"score",
				"evaluatedPlanCount",
				"isExhaustive",
				"candidateCount",
			},
			GetPropertyNames( data )
		);
		Assert.Equal( plan.SelectedParents.Count, data.GetProperty( "selectedParentCount" ).GetInt32() );
		Assert.Equal(
			plan.SelectedParents.Select( selected => selected.UseName ).ToArray(),
			data
				.GetProperty( "selectedParentUseNames" )
				.EnumerateArray()
				.Select( value => value.GetString() )
				.ToArray()
		);
		Assert.Equal( plan.Source, data.GetProperty( "source" ).GetString() );
		JsonElement score = data.GetProperty( "score" );
		Assert.Equal( plan.Score.LocalDirectiveCount, score.GetProperty( "localDirectiveCount" ).GetInt32() );
		Assert.Equal( plan.Score.CancellationCount, score.GetProperty( "cancellationCount" ).GetInt32() );
		Assert.Equal( plan.Score.ParentCount, score.GetProperty( "parentCount" ).GetInt32() );
		Assert.Equal( plan.Score.RenderedUtf8ByteCount, score.GetProperty( "renderedUtf8ByteCount" ).GetInt32() );
		Assert.Equal(
			plan.Score.SelectedCandidateIndices,
			score
				.GetProperty( "selectedCandidateIndices" )
				.EnumerateArray()
				.Select( value => value.GetInt32() )
				.ToArray()
		);
		Assert.Equal( plan.EvaluatedPlanCount, data.GetProperty( "evaluatedPlanCount" ).GetInt32() );
		Assert.Equal( plan.IsExhaustive, data.GetProperty( "isExhaustive" ).GetBoolean() );
		Assert.Equal( plan.CandidateCount, data.GetProperty( "candidateCount" ).GetInt32() );
	}

	[Fact]
	public void ComparisonAndPlanRenderingAreDeterministicAcrossCultureAndRuns() {
		TermInfoComparisonResult comparison = CreateFixtureComparison();
		TerminalDescriptionSourcePlan plan = CreateFixturePlan();
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

		try {
			CultureInfo.CurrentCulture = new CultureInfo( "ar-SA" );
			CultureInfo.CurrentUICulture = new CultureInfo( "tr-TR" );
			string comparisonJson = TermInfoJsonRenderer.Render( comparison );
			string planJson = TermInfoJsonRenderer.Render( plan );

			Assert.Equal( comparisonJson, TermInfoJsonRenderer.Render( comparison ) );
			Assert.Equal( planJson, TermInfoJsonRenderer.Render( plan ) );
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}
	}

	[Theory]
	[InlineData( true )]
	[InlineData( false )]
	public void ComparisonAndPlanApplyExactUtf8Bounds(
		bool renderComparison
	) {
		string expected =
			renderComparison
				? TermInfoJsonRenderer.Render( CreateFixtureComparison() )
				: TermInfoJsonRenderer.Render( CreateFixturePlan() );
		int byteCount = Encoding.UTF8.GetByteCount( expected );

		string exact =
			renderComparison
				? TermInfoJsonRenderer.Render(
					CreateFixtureComparison(),
					new TermInfoJsonRendererOptions( byteCount )
				)
				: TermInfoJsonRenderer.Render(
					CreateFixturePlan(),
					new TermInfoJsonRendererOptions( byteCount )
				);
		Assert.Equal( expected, exact );

		InvalidOperationException exception =
			Assert.Throws<InvalidOperationException>(
				() => {
					if ( renderComparison ) {
						TermInfoJsonRenderer.Render(
							CreateFixtureComparison(),
							new TermInfoJsonRendererOptions( byteCount - 1 )
						);
					} else {
						TermInfoJsonRenderer.Render(
							CreateFixturePlan(),
							new TermInfoJsonRendererOptions( byteCount - 1 )
						);
					}
				}
			);
		Assert.Contains( "UTF-8 byte limit", exception.Message, StringComparison.Ordinal );
	}

	[Fact]
	public void ComparisonAndPlanObserveCancellationAndInvalidText() {
		using var source = new CancellationTokenSource();
		source.Cancel();
		Assert.Throws<OperationCanceledException>(
			() => TermInfoJsonRenderer.Render(
				CreateFixtureComparison(),
				new TermInfoJsonRendererOptions(),
				source.Token
			)
		);
		Assert.Throws<OperationCanceledException>(
			() => TermInfoJsonRenderer.Render(
				CreateFixturePlan(),
				new TermInfoJsonRendererOptions(),
				source.Token
			)
		);

		TermInfoComparisonResult invalid =
			TerminalDescriptionComparer.Compare(
				new TerminalDescriptionBuilder( "mi03-invalid" )
					.SetDescription( "invalid \uD800 text" )
					.Build(),
				new TerminalDescriptionBuilder( "mi03-invalid" ).Build()
			);
		Assert.Contains(
			"invalid UTF-16",
			Assert.Throws<InvalidOperationException>(
				() => TermInfoJsonRenderer.Render( invalid )
			).Message,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void Mi04ActivatesCatalogWithoutChangingTheMi03PublicSurface() {
		TermInfoDatabaseCatalog catalog =
			TermInfoDatabaseInspector.InspectDirectory(
				Path.Combine(
					System.IO.Path.GetTempPath(),
					$"icod-terminfo-mi03-missing-{Guid.NewGuid():N}"
				)
			);

		Assert.Contains(
			"databaseCatalog",
			TermInfoJsonRenderer.Render( catalog ),
			StringComparison.Ordinal
		);
		Assert.Equal(
			31,
			typeof( TermInfoJsonRenderer ).Assembly.GetExportedTypes().Length
		);
	}

	[Fact]
	public void Mi03DocumentationAndVersionFreezeDirectProjection() {
		string root = FindRepositoryRoot();
		string implementation =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.9.0-MI03-COMPARISON-AND-PLANNING-EVIDENCE-JSON.md"
				)
			);
		string roadmap =
			File.ReadAllText(
				Path.Combine(
					root,
					"Icod.TermInfo-1.9.0-Machine-Readable-Inspection-and-Planning-Automation-Roadmap.md"
				)
			);

		foreach (
			string marker
			in new[] {
				DevelopmentVersion,
				"comparison",
				"sourcePlan",
				"sourceEntryIndex",
				"sourceFieldIndex",
				"selectedCandidateIndices",
				"isExhaustive",
				"does not recompute",
				"MI04",
			}
		) {
			Assert.Contains( marker, implementation, StringComparison.Ordinal );
		}
		Assert.Contains( "## MI03 - Comparison and Planning Evidence JSON", roadmap, StringComparison.Ordinal );
		Assert.Contains( "**Development version:** `1.9.0-Alpha-3`", roadmap, StringComparison.Ordinal );
	}

	private static TermInfoComparisonResult CreateFixtureComparison() {
		TerminalDescription left =
			new TerminalDescriptionBuilder( "mi03-comparison" )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
		TerminalDescription right =
			new TerminalDescriptionBuilder( "mi03-comparison" )
				.SetNumber( NumericCapability.Columns, 132 )
				.Build();

		return TerminalDescriptionComparer.Compare(
			left,
			right
		);
	}

	private static TerminalDescriptionSourcePlan CreateFixturePlan() =>
		TerminalDescriptionSourcePlanner.Plan(
			new TerminalDescriptionBuilder( "mi03-plan" ).Build(),
			Array.Empty<TerminalDescriptionSourceSynthesisParent>()
		);

	private static TermInfoSourceDocument ParseDocument(
		string source,
		string sourceName
	) {
		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse(
				source,
				sourceName
			);
		Assert.False( parsed.HasErrors );
		return parsed.Document;
	}

	private static string[] GetPropertyNames(
		JsonElement element
	) =>
		element
			.EnumerateObject()
			.Select( property => property.Name )
			.ToArray();

	private static string ReadFixtureWithoutRepositoryLineTerminator(
		string fileName
	) {
		string fixture =
			File.ReadAllText(
				Path.Combine(
					FindRepositoryRoot(),
					"tests",
					"Icod.TermInfo.Inspection.Tests",
					"fixtures",
					"mi03",
					fileName
				)
			)
				.Replace( "\r\n", "\n", StringComparison.Ordinal )
				.Replace( '\r', '\n' );

		Assert.EndsWith( "\n", fixture, StringComparison.Ordinal );
		Assert.False( fixture.EndsWith( "\n\n", StringComparison.Ordinal ) );
		return fixture[ ..^1 ];
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? directory =
			new( AppContext.BaseDirectory );

		while ( directory is not null ) {
			if (
				File.Exists(
					Path.Combine(
						directory.FullName,
						"Icod.TermInfo.sln"
					)
				)
			) {
				return directory.FullName;
			}

			directory = directory.Parent;
		}

		throw new InvalidOperationException(
			"Repository root not found."
		);
	}
}
