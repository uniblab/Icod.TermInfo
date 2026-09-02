using System.Globalization;
using System.Text;
using System.Text.Json;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class MI02TerminalDescriptionJsonTests {
	private const string DevelopmentVersion = "1.9.0-Alpha-2";

	[Fact]
	public void CompactRenderingMatchesCheckedInFixtureExactly() {
		string actual =
			TermInfoJsonRenderer.Render(
				CreateFixtureDescription()
			);

		Assert.Equal(
			ReadFixtureWithoutRepositoryLineTerminator(
				"terminal-description.compact.json"
			),
			actual
		);
		Assert.DoesNotContain( "\r", actual, StringComparison.Ordinal );
		Assert.False( actual.EndsWith( "\n", StringComparison.Ordinal ) );
	}

	[Fact]
	public void IndentedRenderingMatchesCheckedInLfFixtureExactly() {
		string actual =
			TermInfoJsonRenderer.Render(
				CreateFixtureDescription(),
				new TermInfoJsonRendererOptions(
					TermInfoJsonRendererOptions.DefaultMaximumOutputByteCount,
					writeIndented: true
				)
			);

		Assert.Equal(
			ReadFixtureWithoutRepositoryLineTerminator(
				"terminal-description.indented.json"
			),
			actual
		);
		Assert.DoesNotContain( "\r", actual, StringComparison.Ordinal );
		Assert.False( actual.EndsWith( "\n", StringComparison.Ordinal ) );
		Assert.All(
			actual.Split( '\n' ).Skip( 1 ),
			line => {
				int spaces = line.TakeWhile( character => character == ' ' ).Count();
				Assert.Equal( 0, spaces % 2 );
			}
		);
	}

	[Fact]
	public void PayloadPreservesIdentityTypedValuesAndFrozenOrder() {
		using JsonDocument document =
			JsonDocument.Parse(
				TermInfoJsonRenderer.Render(
					CreateFixtureDescription()
				)
			);
		JsonElement root = document.RootElement;

		Assert.Equal(
			new[] { "schema", "schemaVersion", "documentKind", "data" },
			GetPropertyNames( root )
		);
		Assert.Equal(
			TermInfoJsonRenderer.SchemaIdentifier,
			root.GetProperty( "schema" ).GetString()
		);
		Assert.Equal( 1, root.GetProperty( "schemaVersion" ).GetInt32() );
		Assert.Equal(
			"terminalDescription",
			root.GetProperty( "documentKind" ).GetString()
		);

		JsonElement data = root.GetProperty( "data" );
		Assert.Equal(
			new[] { "identity", "capabilities" },
			GetPropertyNames( data )
		);
		JsonElement identity = data.GetProperty( "identity" );
		Assert.Equal(
			new[] { "name", "aliases", "description" },
			GetPropertyNames( identity )
		);
		Assert.Equal( "mi02-terminal", identity.GetProperty( "name" ).GetString() );
		Assert.Equal(
			new[] { "mi02-z-alias", "mi02-a-alias" },
			identity
				.GetProperty( "aliases" )
				.EnumerateArray()
				.Select( value => value.GetString() )
				.ToArray()
		);
		Assert.Equal(
			"MI02 \"safe\" café",
			identity.GetProperty( "description" ).GetString()
		);

		JsonElement capabilities = data.GetProperty( "capabilities" );
		Assert.Equal(
			new[] { "booleans", "numbers", "strings", "extended" },
			GetPropertyNames( capabilities )
		);
		Assert.Equal(
			new[] { "bw", "am" },
			GetCapabilityNames( capabilities.GetProperty( "booleans" ) )
		);
		Assert.All(
			capabilities.GetProperty( "booleans" ).EnumerateArray(),
			value => Assert.True( value.GetProperty( "value" ).GetBoolean() )
		);
		Assert.Equal(
			new[] { "cols", "lines", "colors" },
			GetCapabilityNames( capabilities.GetProperty( "numbers" ) )
		);
		Assert.Equal(
			new[] { 80, 24, 256 },
			capabilities
				.GetProperty( "numbers" )
				.EnumerateArray()
				.Select( value => value.GetProperty( "value" ).GetInt32() )
				.ToArray()
		);
		Assert.Equal(
			new[] { "bel", "clear", "cup" },
			GetCapabilityNames( capabilities.GetProperty( "strings" ) )
		);
		Assert.Equal(
			new[] { "\a", "\u001b[H\u001b[2J", "\u001b[%i%p1%d;%p2%dH" },
			capabilities
				.GetProperty( "strings" )
				.EnumerateArray()
				.Select( value => value.GetProperty( "value" ).GetString() )
				.ToArray()
		);

		JsonElement extended = capabilities.GetProperty( "extended" );
		Assert.Equal(
			new[] { "booleans", "numbers", "strings" },
			GetPropertyNames( extended )
		);
		Assert.Equal(
			new[] { "aB", "zB" },
			GetCapabilityNames( extended.GetProperty( "booleans" ) )
		);
		Assert.All(
			extended.GetProperty( "booleans" ).EnumerateArray(),
			value => Assert.True( value.GetProperty( "value" ).GetBoolean() )
		);
		Assert.Equal(
			new[] { "aN", "zN" },
			GetCapabilityNames( extended.GetProperty( "numbers" ) )
		);
		Assert.Equal(
			new[] { 42, -7 },
			extended
				.GetProperty( "numbers" )
				.EnumerateArray()
				.Select( value => value.GetProperty( "value" ).GetInt32() )
				.ToArray()
		);
		Assert.Equal(
			new[] { "aS", "zS" },
			GetCapabilityNames( extended.GetProperty( "strings" ) )
		);
		Assert.Equal(
			new[] { "quote \" slash \\", "zeta" },
			extended
				.GetProperty( "strings" )
				.EnumerateArray()
				.Select( value => value.GetProperty( "value" ).GetString() )
				.ToArray()
		);
	}

	[Fact]
	public void MissingDescriptionAndCapabilitiesRemainExplicitWithoutDefaults() {
		TerminalDescription empty =
			new TerminalDescriptionBuilder( "mi02-empty" ).Build();
		using JsonDocument document =
			JsonDocument.Parse(
				TermInfoJsonRenderer.Render( empty )
			);
		JsonElement data = document.RootElement.GetProperty( "data" );
		JsonElement identity = data.GetProperty( "identity" );
		JsonElement capabilities = data.GetProperty( "capabilities" );

		Assert.Equal(
			JsonValueKind.Null,
			identity.GetProperty( "description" ).ValueKind
		);
		Assert.Empty( identity.GetProperty( "aliases" ).EnumerateArray() );
		Assert.Empty( capabilities.GetProperty( "booleans" ).EnumerateArray() );
		Assert.Empty( capabilities.GetProperty( "numbers" ).EnumerateArray() );
		Assert.Empty( capabilities.GetProperty( "strings" ).EnumerateArray() );
		JsonElement extended = capabilities.GetProperty( "extended" );
		Assert.Empty( extended.GetProperty( "booleans" ).EnumerateArray() );
		Assert.Empty( extended.GetProperty( "numbers" ).EnumerateArray() );
		Assert.Empty( extended.GetProperty( "strings" ).EnumerateArray() );
	}

	[Fact]
	public void RenderingIsIndependentOfInsertionOrderCultureAndRepeatedRuns() {
		TerminalDescription first =
			CreateFixtureDescription();
		TerminalDescription second =
			new TerminalDescriptionBuilder( "mi02-terminal" )
				.AddAlias( "mi02-z-alias" )
				.AddAlias( "mi02-a-alias" )
				.SetDescription( "MI02 \"safe\" café" )
				.SetBoolean( BooleanCapability.AutoLeftMargin )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 80 )
				.SetNumber( NumericCapability.Lines, 24 )
				.SetNumber( NumericCapability.Colors, 256 )
				.SetString( StringCapability.Bell, "\a" )
				.SetString( StringCapability.ClearScreen, "\u001b[H\u001b[2J" )
				.SetString( StringCapability.CursorAddress, "\u001b[%i%p1%d;%p2%dH" )
				.SetExtendedBoolean( "aB" )
				.SetExtendedBoolean( "zB" )
				.SetExtendedNumber( "aN", 42 )
				.SetExtendedNumber( "zN", -7 )
				.SetExtendedString( "aS", "quote \" slash \\" )
				.SetExtendedString( "zS", "zeta" )
				.Build();

		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
		try {
			CultureInfo.CurrentCulture = new CultureInfo( "ar-SA" );
			CultureInfo.CurrentUICulture = new CultureInfo( "tr-TR" );
			string expected = TermInfoJsonRenderer.Render( first );

			Assert.Equal( expected, TermInfoJsonRenderer.Render( first ) );
			Assert.Equal( expected, TermInfoJsonRenderer.Render( second ) );
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}
	}

	[Fact]
	public void ExactUtf8BoundSucceedsAndOneByteLessFails() {
		TerminalDescription description =
			CreateFixtureDescription();
		string expected =
			TermInfoJsonRenderer.Render( description );
		int byteCount = Encoding.UTF8.GetByteCount( expected );

		Assert.Equal(
			expected,
			TermInfoJsonRenderer.Render(
				description,
				new TermInfoJsonRendererOptions( byteCount )
			)
		);
		InvalidOperationException exception =
			Assert.Throws<InvalidOperationException>(
				() => TermInfoJsonRenderer.Render(
					description,
					new TermInfoJsonRendererOptions( byteCount - 1 )
				)
			);
		Assert.Contains(
			"UTF-8 byte limit",
			exception.Message,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void InvalidUtf16AndPreCancellationFailDeterministically() {
		TerminalDescription invalid =
			new TerminalDescriptionBuilder( "mi02-invalid" )
				.SetDescription( "invalid \uD800 text" )
				.Build();
		InvalidOperationException invalidException =
			Assert.Throws<InvalidOperationException>(
				() => TermInfoJsonRenderer.Render( invalid )
			);
		Assert.Contains(
			"invalid UTF-16",
			invalidException.Message,
			StringComparison.Ordinal
		);

		using var source = new CancellationTokenSource();
		source.Cancel();
		Assert.Throws<OperationCanceledException>(
			() => TermInfoJsonRenderer.Render(
				CreateFixtureDescription(),
				new TermInfoJsonRendererOptions(),
				source.Token
			)
		);
	}

	[Fact]
	public void Mi02KeepsLaterPayloadsDeferredAndPublicSurfaceStable() {
		TerminalDescription description =
			CreateFixtureDescription();
		TermInfoComparisonResult comparison =
			TerminalDescriptionComparer.Compare(
				description,
				description
			);
		TerminalDescriptionSourcePlan plan =
			TerminalDescriptionSourcePlanner.Plan(
				description,
				Array.Empty<TerminalDescriptionSourceSynthesisParent>()
			);
		TermInfoDatabaseCatalog catalog =
			TermInfoDatabaseInspector.InspectDirectory(
				Path.Combine(
					System.IO.Path.GetTempPath(),
					$"icod-terminfo-mi02-missing-{Guid.NewGuid():N}"
				)
			);

		Assert.Contains(
			"MI03",
			Assert.Throws<NotSupportedException>(
				() => TermInfoJsonRenderer.Render( comparison )
			).Message,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"MI03",
			Assert.Throws<NotSupportedException>(
				() => TermInfoJsonRenderer.Render( plan )
			).Message,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"MI04",
			Assert.Throws<NotSupportedException>(
				() => TermInfoJsonRenderer.Render( catalog )
			).Message,
			StringComparison.Ordinal
		);
		Assert.Equal(
			31,
			typeof( TermInfoJsonRenderer ).Assembly.GetExportedTypes().Length
		);
	}

	[Fact]
	public void Mi02DocumentationAndVersionRecordTheOperationalContract() {
		string root = FindRepositoryRoot();
		string implementation =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.9.0-MI02-EFFECTIVE-DESCRIPTION-JSON.md"
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
				"terminalDescription",
				"compiled database position",
				"ordinal name",
				"exact UTF-8 byte count",
				"LF",
				"two-space",
				"invalid UTF-16",
				"MI03",
				"MI04",
			}
		) {
			Assert.Contains( marker, implementation, StringComparison.Ordinal );
		}
		Assert.Contains( "**Status:** MI02 complete", roadmap, StringComparison.Ordinal );
	}

	private static TerminalDescription CreateFixtureDescription() =>
		new TerminalDescriptionBuilder( "mi02-terminal" )
			.AddAlias( "mi02-z-alias" )
			.AddAlias( "mi02-a-alias" )
			.SetDescription( "MI02 \"safe\" café" )
			.SetBoolean( BooleanCapability.AutoRightMargin )
			.SetBoolean( BooleanCapability.AutoLeftMargin )
			.SetNumber( NumericCapability.Colors, 256 )
			.SetNumber( NumericCapability.Lines, 24 )
			.SetNumber( NumericCapability.Columns, 80 )
			.SetString( StringCapability.CursorAddress, "\u001b[%i%p1%d;%p2%dH" )
			.SetString( StringCapability.ClearScreen, "\u001b[H\u001b[2J" )
			.SetString( StringCapability.Bell, "\a" )
			.SetExtendedString( "zS", "zeta" )
			.SetExtendedNumber( "zN", -7 )
			.SetExtendedBoolean( "zB" )
			.SetExtendedString( "aS", "quote \" slash \\" )
			.SetExtendedNumber( "aN", 42 )
			.SetExtendedBoolean( "aB" )
			.Build();

	private static string[] GetPropertyNames(
		JsonElement element
	) =>
		element
			.EnumerateObject()
			.Select( property => property.Name )
			.ToArray();

	private static string[] GetCapabilityNames(
		JsonElement array
	) =>
		array
			.EnumerateArray()
			.Select( value => value.GetProperty( "name" ).GetString()! )
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
					"mi02",
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
