using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class MI04DatabaseCatalogJsonAndSchemaTests {
	private const string DevelopmentVersion = "1.9.0-Alpha-4";
	private const string CompleteRoot = "//mi04/catalog";
	private const string IncompleteRoot = "//mi04/incomplete";
	private const string SchemaRelativePath =
		"docs/Icod.TermInfo.Inspection.schema.json";

	[Theory]
	[InlineData( true, false, "database-catalog.complete.compact.json" )]
	[InlineData( true, true, "database-catalog.complete.indented.json" )]
	[InlineData( false, false, "database-catalog.incomplete.compact.json" )]
	[InlineData( false, true, "database-catalog.incomplete.indented.json" )]
	public void DatabaseCatalogRenderingMatchesCheckedInFixtureExactly(
		bool complete,
		bool writeIndented,
		string fixtureName
	) {
		TermInfoDatabaseCatalog catalog =
			complete
				? CreateCompleteCatalog()
				: CreateIncompleteCatalog();
		string actual =
			TermInfoJsonRenderer.Render(
				catalog,
				new TermInfoJsonRendererOptions(
					TermInfoJsonRendererOptions.DefaultMaximumOutputByteCount,
					writeIndented
				)
			);

		Assert.Equal(
			ReadFixtureWithoutRepositoryLineTerminator(
				"mi04",
				fixtureName
			),
			actual
		);
		Assert.DoesNotContain( "\r", actual, StringComparison.Ordinal );
		Assert.False( actual.EndsWith( "\n", StringComparison.Ordinal ) );
	}

	[Fact]
	public void CompleteManifestPreservesCatalogOrderPathsIdentityAndDuplicates() {
		TermInfoDatabaseCatalog catalog =
			CreateCompleteCatalog();

		using JsonDocument document =
			JsonDocument.Parse(
				TermInfoJsonRenderer.Render( catalog )
			);
		JsonElement root = document.RootElement;
		Assert.Equal(
			new[] { "schema", "schemaVersion", "documentKind", "data" },
			GetPropertyNames( root )
		);
		Assert.Equal( "databaseCatalog", root.GetProperty( "documentKind" ).GetString() );
		JsonElement data = root.GetProperty( "data" );
		Assert.Equal(
			new[] {
				"root",
				"kind",
				"isComplete",
				"entries",
				"issues",
				"duplicateCanonicalNames",
			},
			GetPropertyNames( data )
		);
		Assert.Equal( catalog.Root, data.GetProperty( "root" ).GetString() );
		Assert.Equal( "conventionalDirectory", data.GetProperty( "kind" ).GetString() );
		Assert.True( data.GetProperty( "isComplete" ).GetBoolean() );
		JsonElement[] entries =
			data.GetProperty( "entries" ).EnumerateArray().ToArray();
		Assert.Equal( catalog.Entries.Count, entries.Length );
		for ( int index = 0; index < entries.Length; index++ ) {
			Assert.Equal( catalog.Entries[ index ].Path, entries[ index ].GetProperty( "path" ).GetString() );
			Assert.Equal( catalog.Entries[ index ].Name, entries[ index ].GetProperty( "name" ).GetString() );
			Assert.Equal(
				catalog.Entries[ index ].Aliases,
				entries[ index ]
					.GetProperty( "aliases" )
					.EnumerateArray()
					.Select( value => value.GetString() )
					.ToArray()
			);
			Assert.Equal(
				catalog.Entries[ index ].Description,
				entries[ index ].GetProperty( "description" ).GetString()
			);
		}
		Assert.Empty( data.GetProperty( "issues" ).EnumerateArray() );
		Assert.Equal(
			catalog.DuplicateCanonicalNames,
			data
				.GetProperty( "duplicateCanonicalNames" )
				.EnumerateArray()
				.Select( value => value.GetString() )
				.ToArray()
		);
	}

	[Fact]
	public void IncompleteManifestPreservesIssueEvidenceWithoutHidingValidEntries() {
		TermInfoDatabaseCatalog catalog =
			CreateIncompleteCatalog();

		using JsonDocument document =
			JsonDocument.Parse(
				TermInfoJsonRenderer.Render( catalog )
			);
		JsonElement data = document.RootElement.GetProperty( "data" );
		Assert.Equal( "conventionalDirectory", data.GetProperty( "kind" ).GetString() );
		Assert.False( data.GetProperty( "isComplete" ).GetBoolean() );
		Assert.Single( data.GetProperty( "entries" ).EnumerateArray() );
		JsonElement issue =
			Assert.Single(
				data.GetProperty( "issues" ).EnumerateArray().ToArray()
			);
		Assert.Equal( "malformedEntry", issue.GetProperty( "kind" ).GetString() );
		Assert.Equal( catalog.Issues[ 0 ].Path, issue.GetProperty( "path" ).GetString() );
		Assert.Equal( catalog.Issues[ 0 ].Message, issue.GetProperty( "message" ).GetString() );
	}

	[Theory]
	[InlineData( TermInfoDatabaseCatalogKind.Missing, "missing", false )]
	[InlineData( TermInfoDatabaseCatalogKind.ConventionalDirectory, "conventionalDirectory", true )]
	[InlineData( TermInfoDatabaseCatalogKind.UnsupportedStore, "unsupportedStore", false )]
	[InlineData( TermInfoDatabaseCatalogKind.Unavailable, "unavailable", false )]
	public void CatalogKindAndCompletenessMappingIsExplicit(
		TermInfoDatabaseCatalogKind kind,
		string expectedKind,
		bool expectedComplete
	) {
		TermInfoDatabaseCatalog catalog =
			new(
				"//mi04/kind",
				kind,
				Array.Empty<TermInfoDatabaseCatalogEntry>(),
				Array.Empty<TermInfoDatabaseCatalogIssue>(),
				Array.Empty<string>()
			);

		using JsonDocument document =
			JsonDocument.Parse(
				TermInfoJsonRenderer.Render( catalog )
			);
		JsonElement data = document.RootElement.GetProperty( "data" );
		Assert.Equal( expectedKind, data.GetProperty( "kind" ).GetString() );
		Assert.Equal( expectedComplete, data.GetProperty( "isComplete" ).GetBoolean() );
	}

	[Fact]
	public void CatalogIssueKindMappingIsExplicitAndOrdered() {
		TermInfoDatabaseCatalogIssueKind[] kinds =
			Enum.GetValues<TermInfoDatabaseCatalogIssueKind>();
		TermInfoDatabaseCatalog catalog =
			new(
				"//mi04/issues",
				TermInfoDatabaseCatalogKind.ConventionalDirectory,
				Array.Empty<TermInfoDatabaseCatalogEntry>(),
				kinds.Select(
					kind =>
						new TermInfoDatabaseCatalogIssue(
							kind,
							$"//mi04/issues/{(int)kind}",
							$"MI04 issue {(int)kind}."
						)
				),
				Array.Empty<string>()
			);

		using JsonDocument document =
			JsonDocument.Parse(
				TermInfoJsonRenderer.Render( catalog )
			);
		Assert.Equal(
			new[] {
				"malformedEntry",
				"invalidPlacement",
				"permissionFailure",
				"ioFailure",
				"linkSkipped",
			},
			document.RootElement
				.GetProperty( "data" )
				.GetProperty( "issues" )
				.EnumerateArray()
				.Select( issue => issue.GetProperty( "kind" ).GetString() )
				.ToArray()
		);
	}

	[Fact]
	public void EveryCheckedInDocumentFixtureValidatesAgainstPublishedSchema() {
		string root = FindRepositoryRoot();
		JsonNode schema =
			JsonNode.Parse(
				File.ReadAllText(
					Path.Combine(
						root,
						SchemaRelativePath.Replace(
							'/',
							Path.DirectorySeparatorChar
						)
					)
				)
			)!;
		string[] fixturePaths = [
			Path.Combine( "mi02", "terminal-description.compact.json" ),
			Path.Combine( "mi02", "terminal-description.indented.json" ),
			Path.Combine( "mi03", "comparison.compact.json" ),
			Path.Combine( "mi03", "comparison.indented.json" ),
			Path.Combine( "mi03", "source-plan.compact.json" ),
			Path.Combine( "mi03", "source-plan.indented.json" ),
			Path.Combine( "mi04", "database-catalog.complete.compact.json" ),
			Path.Combine( "mi04", "database-catalog.complete.indented.json" ),
			Path.Combine( "mi04", "database-catalog.incomplete.compact.json" ),
			Path.Combine( "mi04", "database-catalog.incomplete.indented.json" ),
		];

		foreach ( string fixturePath in fixturePaths ) {
			string fullPath =
				Path.Combine(
					root,
					"tests",
					"Icod.TermInfo.Inspection.Tests",
					"fixtures",
					fixturePath
				);
			JsonNode fixture =
				JsonNode.Parse(
					File.ReadAllText( fullPath )
				)!;
			Assert.True(
				JsonSchemaFixtureValidator.IsValid(
					schema,
					fixture,
					out string error
				),
				$"Fixture '{fixturePath}' does not satisfy the published schema: {error}"
			);
		}
	}

	[Fact]
	public void PublishedSchemaRejectsMissingFieldsInvalidKindsAndContradictoryCompleteness() {
		JsonNode schema = ReadSchema();
		JsonObject missingRoot =
			JsonNode.Parse(
				ReadFixtureWithoutRepositoryLineTerminator(
					"mi04",
					"database-catalog.complete.compact.json"
				)
			)!.AsObject();
		missingRoot[ "data" ]!.AsObject().Remove( "root" );
		Assert.False(
			JsonSchemaFixtureValidator.IsValid(
				schema,
				missingRoot,
				out _
			)
		);

		JsonObject invalidKind =
			JsonNode.Parse(
				ReadFixtureWithoutRepositoryLineTerminator(
					"mi04",
					"database-catalog.complete.compact.json"
				)
			)!.AsObject();
		invalidKind[ "data" ]![ "kind" ] = "hostDatabase";
		Assert.False(
			JsonSchemaFixtureValidator.IsValid(
				schema,
				invalidKind,
				out _
			)
		);

		JsonObject falseCompleteCatalog =
			JsonNode.Parse(
				ReadFixtureWithoutRepositoryLineTerminator(
					"mi04",
					"database-catalog.complete.compact.json"
				)
			)!.AsObject();
		falseCompleteCatalog[ "data" ]![ "isComplete" ] = false;
		Assert.False(
			JsonSchemaFixtureValidator.IsValid(
				schema,
				falseCompleteCatalog,
				out _
			)
		);

		JsonObject trueIncompleteCatalog =
			JsonNode.Parse(
				ReadFixtureWithoutRepositoryLineTerminator(
					"mi04",
					"database-catalog.incomplete.compact.json"
				)
			)!.AsObject();
		trueIncompleteCatalog[ "data" ]![ "isComplete" ] = true;
		Assert.False(
			JsonSchemaFixtureValidator.IsValid(
				schema,
				trueIncompleteCatalog,
				out _
			)
		);
	}

	[Fact]
	public void CatalogRenderingIsDeterministicAcrossCultureAndRuns() {
		TermInfoDatabaseCatalog catalog =
			CreateCompleteCatalog();
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

		try {
			CultureInfo.CurrentCulture = new CultureInfo( "ar-SA" );
			CultureInfo.CurrentUICulture = new CultureInfo( "tr-TR" );
			string json = TermInfoJsonRenderer.Render( catalog );

			Assert.Equal( json, TermInfoJsonRenderer.Render( catalog ) );
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}
	}

	[Fact]
	public void CatalogRenderingAppliesExactUtf8Bounds() {
		TermInfoDatabaseCatalog catalog =
			CreateCompleteCatalog();
		string expected =
			TermInfoJsonRenderer.Render( catalog );
		int byteCount = Encoding.UTF8.GetByteCount( expected );

		Assert.Equal(
			expected,
			TermInfoJsonRenderer.Render(
				catalog,
				new TermInfoJsonRendererOptions( byteCount )
			)
		);
		InvalidOperationException exception =
			Assert.Throws<InvalidOperationException>(
				() => TermInfoJsonRenderer.Render(
					catalog,
					new TermInfoJsonRendererOptions( byteCount - 1 )
				)
			);
		Assert.Contains( "UTF-8 byte limit", exception.Message, StringComparison.Ordinal );
	}

	[Fact]
	public void CatalogRenderingObservesCancellationAndInvalidText() {
		using var source = new CancellationTokenSource();
		source.Cancel();
		Assert.Throws<OperationCanceledException>(
			() => TermInfoJsonRenderer.Render(
				CreateCompleteCatalog(),
				new TermInfoJsonRendererOptions(),
				source.Token
			)
		);

		TermInfoDatabaseCatalog invalid =
			new(
				"//mi04/invalid",
				TermInfoDatabaseCatalogKind.Unavailable,
				Array.Empty<TermInfoDatabaseCatalogEntry>(),
				new[] {
					new TermInfoDatabaseCatalogIssue(
						TermInfoDatabaseCatalogIssueKind.IoFailure,
						"//mi04/invalid/entry",
						"invalid \uD800 text"
					),
				},
				Array.Empty<string>()
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
	public void Mi04KeepsPublicSurfaceStableAndDocumentsCompletedSchema() {
		string root = FindRepositoryRoot();
		string implementation =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.9.0-MI04-DATABASE-CATALOG-MANIFESTS-AND-JSON-SCHEMA.md"
				)
			);
		string roadmap =
			File.ReadAllText(
				Path.Combine(
					root,
					"Icod.TermInfo-1.9.0-Machine-Readable-Inspection-and-Planning-Automation-Roadmap.md"
				)
			);

		Assert.Equal(
			31,
			typeof( TermInfoJsonRenderer ).Assembly.GetExportedTypes().Length
		);
		foreach (
			string marker
			in new[] {
				DevelopmentVersion,
				"databaseCatalog",
				"isComplete",
				"duplicateCanonicalNames",
				"Icod.TermInfo.Inspection.schema.json",
				"MI05",
			}
		) {
			Assert.Contains( marker, implementation, StringComparison.Ordinal );
		}
		Assert.Contains( "**Status:** MI04 complete", roadmap, StringComparison.Ordinal );
	}

	private static TermInfoDatabaseCatalog CreateCompleteCatalog() {
		Assert.True( Path.IsPathFullyQualified( CompleteRoot ) );
		TerminalDescription terminal =
			new TerminalDescriptionBuilder( "mi04-main" )
				.AddAlias( "mi04-alias" )
				.SetDescription( "MI04 \"complete\" catalog" )
				.Build();

		return new TermInfoDatabaseCatalog(
			CompleteRoot,
			TermInfoDatabaseCatalogKind.ConventionalDirectory,
			new[] {
				new TermInfoDatabaseCatalogEntry(
					"//mi04/catalog/6d/mi04-main",
					terminal
				),
				new TermInfoDatabaseCatalogEntry(
					"//mi04/catalog/6d/mi04-alias",
					terminal
				),
			},
			Array.Empty<TermInfoDatabaseCatalogIssue>(),
			new[] {
				terminal.Name,
			}
		);
	}

	private static TermInfoDatabaseCatalog CreateIncompleteCatalog() {
		Assert.True( Path.IsPathFullyQualified( IncompleteRoot ) );
		TerminalDescription valid =
			new TerminalDescriptionBuilder( "valid-mi04" ).Build();

		return new TermInfoDatabaseCatalog(
			IncompleteRoot,
			TermInfoDatabaseCatalogKind.ConventionalDirectory,
			new[] {
				new TermInfoDatabaseCatalogEntry(
					"//mi04/incomplete/76/valid-mi04",
					valid
				),
			},
			new[] {
				new TermInfoDatabaseCatalogIssue(
					TermInfoDatabaseCatalogIssueKind.MalformedEntry,
					"//mi04/incomplete/62/broken",
					"The compiled entry is malformed."
				),
			},
			Array.Empty<string>()
		);
	}

	private static JsonNode ReadSchema() =>
		JsonNode.Parse(
			File.ReadAllText(
				Path.Combine(
					FindRepositoryRoot(),
					"docs",
					"Icod.TermInfo.Inspection.schema.json"
				)
			)
		)!;

	private static string[] GetPropertyNames(
		JsonElement element
	) =>
		element
			.EnumerateObject()
			.Select( property => property.Name )
			.ToArray();

	private static string ReadFixtureWithoutRepositoryLineTerminator(
		string tranche,
		string fileName
	) {
		string fixture =
			File.ReadAllText(
				Path.Combine(
					FindRepositoryRoot(),
					"tests",
					"Icod.TermInfo.Inspection.Tests",
					"fixtures",
					tranche,
					fileName
				)
			)
				.Replace( "\r\n", "\n", StringComparison.Ordinal )
				.Replace( '\r', '\n' );

		Assert.True( fixture.EndsWith( "\n", StringComparison.Ordinal ) );
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
			) ) {
				return directory.FullName;
			}

			directory = directory.Parent;
		}

		throw new InvalidOperationException(
			"Repository root not found."
		);
	}
}
