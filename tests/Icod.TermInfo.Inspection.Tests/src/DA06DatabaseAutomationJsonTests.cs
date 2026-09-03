using System.Globalization;
using System.Text;
using System.Text.Json;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class DA06DatabaseAutomationJsonTests {
	[Fact]
	public void VersionOneIdentityAndSchemaFileRemainFrozen() {
		Assert.Equal( "urn:icod:terminfo:inspection:json:1", TermInfoJsonRenderer.SchemaIdentifier );
		Assert.Equal( 1, TermInfoJsonRenderer.SchemaVersion );
		Assert.Equal(
			"urn:icod:terminfo:inspection:json:2",
			TermInfoJsonRenderer.DatabaseAutomationSchemaIdentifier
		);
		Assert.Equal( 2, TermInfoJsonRenderer.DatabaseAutomationSchemaVersion );

		string root = FindRepositoryRoot();
		using JsonDocument v1 = JsonDocument.Parse(
			File.ReadAllText(
				Path.Combine( root, "docs", "Icod.TermInfo.Inspection.schema.json" )
			)
		);
		using JsonDocument v2 = JsonDocument.Parse(
			File.ReadAllText(
				Path.Combine( root, "docs", "Icod.TermInfo.Inspection.schema.v2.json" )
			)
		);
		Assert.Equal(
			"urn:icod:terminfo:inspection:json:1",
			v1.RootElement.GetProperty( "$id" ).GetString()
		);
		Assert.Equal(
			"urn:icod:terminfo:inspection:json:2",
			v2.RootElement.GetProperty( "$id" ).GetString()
		);
	}

	[Fact]
	public void DatabaseSetJsonPreservesRootIdentityOccurrenceAndSemanticOrdering() {
		string firstRoot = AbsolutePath( "set-first" );
		string secondRoot = AbsolutePath( "set-second" );
		TermInfoDatabaseSet set = TermInfoDatabaseInspector.CreateSet(
			[
				CreateCatalog( firstRoot, CreateTerminal( "zeta", 80, "shared" ) ),
				CreateCatalog(
					secondRoot,
					CreateTerminal( "alpha", 100 ),
					CreateTerminal( "zeta", 132, "shared" )
				),
			]
		);

		string json = TermInfoJsonRenderer.Render( set );
		using JsonDocument document = JsonDocument.Parse( json );
		JsonElement root = document.RootElement;
		JsonElement data = root.GetProperty( "data" );
		Assert.Equal( "urn:icod:terminfo:inspection:json:2", root.GetProperty( "schema" ).GetString() );
		Assert.Equal( 2, root.GetProperty( "schemaVersion" ).GetInt32() );
		Assert.Equal( "databaseSet", root.GetProperty( "documentKind" ).GetString() );
		Assert.Equal(
			new[] { firstRoot, secondRoot },
			data.GetProperty( "databases" ).EnumerateArray()
				.Select( element => element.GetProperty( "root" ).GetString() )
				.ToArray()
		);
		Assert.Equal(
			new[] { "alpha", "zeta" },
			data.GetProperty( "identities" ).EnumerateArray()
				.Select( element => element.GetProperty( "name" ).GetString() )
				.ToArray()
		);
		JsonElement zeta = data.GetProperty( "identities" )[ 1 ];
		Assert.Equal( "winnerKnown", zeta.GetProperty( "lookupStatus" ).GetString() );
		Assert.Equal( 0, zeta.GetProperty( "winner" ).GetProperty( "databaseIndex" ).GetInt32() );
		Assert.Equal(
			new[] { 0, 1 },
			zeta.GetProperty( "occurrences" ).EnumerateArray()
				.Select( element => element.GetProperty( "databaseIndex" ).GetInt32() )
				.ToArray()
		);
		JsonElement repeated = Assert.Single(
			data.GetProperty( "semanticAnalysis" ).GetProperty( "repeatedIdentities" ).EnumerateArray()
		);
		Assert.Equal( "zeta", repeated.GetProperty( "name" ).GetString() );
		Assert.Equal( "semanticallyDifferent", repeated.GetProperty( "relationship" ).GetString() );
		JsonElement shadow = Assert.Single( repeated.GetProperty( "shadows" ).EnumerateArray() );
		Assert.Equal( 1, shadow.GetProperty( "occurrence" ).GetProperty( "databaseIndex" ).GetInt32() );
		Assert.False( shadow.GetProperty( "comparison" ).GetProperty( "areEqual" ).GetBoolean() );
	}

	[Fact]
	public void DatabaseSetComparisonJsonPreservesFrozenDifferenceOrder() {
		string root = AbsolutePath( "comparison" );
		TermInfoDatabaseSet left = TermInfoDatabaseInspector.CreateSet(
			[ CreateCatalog( root, CreateTerminal( "alpha", 80 ), CreateTerminal( "zeta", 80 ) ) ]
		);
		TermInfoDatabaseSet right = TermInfoDatabaseInspector.CreateSet(
			[ CreateCatalog( root, CreateTerminal( "beta", 80 ), CreateTerminal( "zeta", 132 ) ) ]
		);
		TermInfoDatabaseSetComparisonResult comparison =
			TermInfoDatabaseSetComparer.Compare( left, right );

		using JsonDocument document = JsonDocument.Parse( TermInfoJsonRenderer.Render( comparison ) );
		JsonElement rootElement = document.RootElement;
		Assert.Equal( "databaseSetComparison", rootElement.GetProperty( "documentKind" ).GetString() );
		string[] kinds = rootElement.GetProperty( "data" ).GetProperty( "differences" )
			.EnumerateArray()
			.Select( element => element.GetProperty( "kind" ).GetString()! )
			.ToArray();
		Assert.Equal(
			comparison.Differences.Select( difference => KindName( difference.Kind ) ).ToArray(),
			kinds
		);
	}

	[Fact]
	public void DatabaseSetPlanJsonMapsFrozenSelectedIndicesToProvenance() {
		string firstRoot = AbsolutePath( "plan-first" );
		string secondRoot = AbsolutePath( "plan-second" );
		TerminalDescription target = CreateTarget();
		TermInfoDatabaseSet set = TermInfoDatabaseInspector.CreateSet(
			[
				CreateCatalog( firstRoot, CreateParent( "parent-a", true, false ) ),
				CreateCatalog( secondRoot, CreateParent( "parent-b", false, true ) ),
			]
		);
		TermInfoDatabaseSetSourcePlanningResult plan =
			TerminalDescriptionSourcePlanner.PlanFromDatabaseSet( target, set );

		using JsonDocument document = JsonDocument.Parse( TermInfoJsonRenderer.Render( plan ) );
		JsonElement data = document.RootElement.GetProperty( "data" );
		Assert.Equal( "databaseSetPlan", document.RootElement.GetProperty( "documentKind" ).GetString() );
		JsonElement bounds = data.GetProperty( "planningBounds" );
		Assert.Equal(
			TerminalDescriptionSourcePlanningOptions.DefaultMaximumCandidateCount,
			bounds.GetProperty( "maximumCandidateCount" ).GetInt32()
		);
		Assert.Equal(
			TerminalDescriptionSourcePlanningOptions.DefaultMaximumEvaluatedPlanCount,
			bounds.GetProperty( "maximumEvaluatedPlanCount" ).GetInt32()
		);
		Assert.Equal(
			plan.Plan.Score.SelectedCandidateIndices.ToArray(),
			data.GetProperty( "selectedCandidateIndices" ).EnumerateArray()
				.Select( element => element.GetInt32() )
				.ToArray()
		);
		Assert.Equal(
			plan.SelectedCandidates.Select( candidate => candidate.DatabaseIndex ).ToArray(),
			data.GetProperty( "selectedCandidates" ).EnumerateArray()
				.Select( element => element.GetProperty( "databaseIndex" ).GetInt32() )
				.ToArray()
		);
	}

	[Fact]
	public void VersionTwoRenderingIsCultureDeterministicBoundedAndCancelable() {
		TermInfoDatabaseSet set = TermInfoDatabaseInspector.CreateSet(
			[ CreateCatalog( AbsolutePath( "culture" ), CreateTerminal( "I", 80, "İ" ) ) ]
		);
		string invariant = TermInfoJsonRenderer.Render( set );
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
		try {
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "tr-TR" );
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "tr-TR" );
			Assert.Equal( invariant, TermInfoJsonRenderer.Render( set ) );
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}

		int exactBytes = Encoding.UTF8.GetByteCount( invariant );
		Assert.Equal(
			invariant,
			TermInfoJsonRenderer.Render(
				set,
				new TermInfoJsonRendererOptions( exactBytes )
			)
		);
		Assert.Throws<InvalidOperationException>(
			() => TermInfoJsonRenderer.Render(
				set,
				new TermInfoJsonRendererOptions( exactBytes - 1 )
			)
		);
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>(
			() => TermInfoJsonRenderer.Render(
				set,
				new TermInfoJsonRendererOptions(),
				cancellation.Token
			)
		);
	}

	[Fact]
	public void Da06AddsRendererOverloadsButNoNewPublicTypes() {
		Type[] exportedTypes = typeof( TermInfoJsonRenderer ).Assembly.GetExportedTypes();
		Assert.InRange( exportedTypes.Length, 51, 51 );
	}

	private static string KindName( TermInfoDatabaseSetDifferenceKind kind ) =>
		kind switch {
			TermInfoDatabaseSetDifferenceKind.RootTopology => "rootTopology",
			TermInfoDatabaseSetDifferenceKind.Completeness => "completeness",
			TermInfoDatabaseSetDifferenceKind.Issue => "issue",
			TermInfoDatabaseSetDifferenceKind.OnlyInLeft => "onlyInLeft",
			TermInfoDatabaseSetDifferenceKind.OnlyInRight => "onlyInRight",
			TermInfoDatabaseSetDifferenceKind.EffectiveSemantic => "effectiveSemantic",
			TermInfoDatabaseSetDifferenceKind.EffectiveProvenance => "effectiveProvenance",
			TermInfoDatabaseSetDifferenceKind.AliasOwnership => "aliasOwnership",
			TermInfoDatabaseSetDifferenceKind.ShadowSet => "shadowSet",
			TermInfoDatabaseSetDifferenceKind.Indeterminate => "indeterminate",
			_ => throw new ArgumentOutOfRangeException( nameof( kind ) ),
		};

	private static TermInfoDatabaseCatalog CreateCatalog(
		string root,
		params TerminalDescription[] terminals
	) {
		TermInfoDatabaseCatalogEntry[] entries = terminals
			.Select(
				( terminal, index ) => new TermInfoDatabaseCatalogEntry(
					Path.Combine( root, index.ToString( CultureInfo.InvariantCulture ) ),
					terminal
				)
			)
			.OrderBy( entry => entry.Name, StringComparer.Ordinal )
			.ThenBy( entry => entry.Path, StringComparer.Ordinal )
			.ToArray();
		return new TermInfoDatabaseCatalog(
			root,
			TermInfoDatabaseCatalogKind.ConventionalDirectory,
			entries,
			Array.Empty<TermInfoDatabaseCatalogIssue>(),
			entries.GroupBy( entry => entry.Name, StringComparer.Ordinal )
				.Where( group => group.Count() > 1 )
				.Select( group => group.Key )
				.OrderBy( name => name, StringComparer.Ordinal )
				.ToArray()
		);
	}

	private static TerminalDescription CreateTerminal(
		string name,
		int columns,
		params string[] aliases
	) {
		TerminalDescriptionBuilder builder = new TerminalDescriptionBuilder( name )
			.SetDescription( $"DA06 {name}" )
			.SetNumber( NumericCapability.Columns, columns );
		foreach ( string alias in aliases ) {
			builder.AddAlias( alias );
		}
		return builder.Build();
	}

	private static TerminalDescription CreateTarget() =>
		new TerminalDescriptionBuilder( "da06-target" )
			.SetDescription( "DA06 target" )
			.SetBoolean( BooleanCapability.AutoRightMargin )
			.SetNumber( NumericCapability.Columns, 80 )
			.Build();

	private static TerminalDescription CreateParent(
		string name,
		bool margin,
		bool columns
	) {
		TerminalDescriptionBuilder builder = new TerminalDescriptionBuilder( name )
			.SetDescription( $"DA06 {name}" );
		if ( margin ) {
			builder.SetBoolean( BooleanCapability.AutoRightMargin );
		}
		if ( columns ) {
			builder.SetNumber( NumericCapability.Columns, 80 );
		}
		return builder.Build();
	}

	private static string AbsolutePath( string suffix ) =>
		Path.Combine( Path.GetTempPath(), $"icod-terminfo-da06-{suffix}" );

	private static string FindRepositoryRoot() {
		DirectoryInfo? current = new( AppContext.BaseDirectory );
		while ( current is not null ) {
			if ( File.Exists( Path.Combine( current.FullName, "Icod.TermInfo.sln" ) ) ) {
				return current.FullName;
			}
			current = current.Parent;
		}
		throw new DirectoryNotFoundException( "Could not locate repository root." );
	}
}
