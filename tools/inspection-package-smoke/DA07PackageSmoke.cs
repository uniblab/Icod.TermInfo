using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;

internal static class DA07PackageSmoke {
	[ModuleInitializer]
	internal static void Run() {
		string firstRoot = AbsolutePath( "first" );
		string secondRoot = AbsolutePath( "second" );
		TermInfoDatabaseSet set = TermInfoDatabaseInspector.CreateSet(
			[
				CreateCatalog(
					firstRoot,
					CreateTerminal( "shared", 80, "collision" ),
					CreateParent( "parent-a", true, false )
				),
				CreateCatalog(
					secondRoot,
					CreateTerminal( "shared", 132 ),
					CreateTerminal( "other-owner", 100, "collision" ),
					CreateParent( "parent-b", false, true )
				),
			]
		);
		Require(
			set.Entries.Count == 2 && set.IsComplete,
			"DA07 package smoke could not construct a complete ordered database set."
		);

		TermInfoDatabaseSetSemanticAnalysis analysis = set.AnalyzeSemantics();
		Require(
			analysis.RepeatedIdentities.Any(
				identity => identity.Identity.Name == "shared"
					&& identity.Relationship
						== TermInfoDatabaseSetSemanticRelationship.SemanticallyDifferent
			),
			"DA07 package smoke did not classify a conflicting canonical shadow."
		);
		Require(
			analysis.Aliases.Any(
				alias => alias.Alias == "collision"
					&& alias.HasMultipleCanonicalOwners
			),
			"DA07 package smoke did not retain alias-collision evidence."
		);

		TermInfoDatabaseSet comparisonRight = TermInfoDatabaseInspector.CreateSet(
			[
				CreateCatalog(
					firstRoot,
					CreateTerminal( "shared", 80, "collision" ),
					CreateParent( "parent-a", true, false )
				),
			]
		);
		TermInfoDatabaseSetComparisonResult comparison =
			TermInfoDatabaseSetComparer.Compare( set, comparisonRight );
		Require(
			comparison.Differences.Count > 0,
			"DA07 package smoke did not compare two database sets."
		);

		TerminalDescription target = new TerminalDescriptionBuilder( "da07-package-target" )
			.SetDescription( "DA07 package target" )
			.SetBoolean( BooleanCapability.AutoRightMargin )
			.SetNumber( NumericCapability.Columns, 80 )
			.Build();
		TermInfoDatabaseSetSourcePlanningResult plan =
			TerminalDescriptionSourcePlanner.PlanFromDatabaseSet( target, set );
		Require(
			plan.Candidates.Count >= 2
				&& plan.Plan.CandidateCount == plan.Candidates.Count,
			"DA07 package smoke did not perform multi-database planning."
		);

		string json = TermInfoJsonRenderer.Render( set );
		using JsonDocument document = JsonDocument.Parse( json );
		Require(
			document.RootElement.GetProperty( "schemaVersion" ).GetInt32() == 2
				&& document.RootElement.GetProperty( "documentKind" ).GetString()
					== "databaseSet",
			"DA07 package smoke did not render the database-set machine contract."
		);
		int byteCount = Encoding.UTF8.GetByteCount( json );
		bool rejectedInsufficientBound = false;
		try {
			TermInfoJsonRenderer.Render(
				set,
				new TermInfoJsonRendererOptions( byteCount - 1 )
			);
		} catch ( InvalidOperationException ) {
			rejectedInsufficientBound = true;
		}
		Require(
			rejectedInsufficientBound,
			"DA07 package smoke accepted an intentionally insufficient JSON bound."
		);
	}

	private static TermInfoDatabaseCatalog CreateCatalog(
		string root,
		params TerminalDescription[] terminals
	) {
		TermInfoDatabaseCatalogEntry[] entries = terminals
			.Select(
				( terminal, index ) => new TermInfoDatabaseCatalogEntry(
					System.IO.Path.Combine( root, index.ToString( System.Globalization.CultureInfo.InvariantCulture ) ),
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
			.SetDescription( $"DA07 package {name}" )
			.SetNumber( NumericCapability.Columns, columns );
		foreach ( string alias in aliases ) {
			builder.AddAlias( alias );
		}
		return builder.Build();
	}

	private static TerminalDescription CreateParent(
		string name,
		bool margin,
		bool columns
	) {
		TerminalDescriptionBuilder builder = new TerminalDescriptionBuilder( name )
			.SetDescription( $"DA07 package {name}" );
		if ( margin ) {
			builder.SetBoolean( BooleanCapability.AutoRightMargin );
		}
		if ( columns ) {
			builder.SetNumber( NumericCapability.Columns, 80 );
		}
		return builder.Build();
	}

	private static string AbsolutePath(
		string suffix
	) => System.IO.Path.Combine(
		System.IO.Path.GetTempPath(),
		$"icod-terminfo-da07-package-{suffix}"
	);

	private static void Require(
		bool condition,
		string message
	) {
		ArgumentNullException.ThrowIfNull( message );
		if ( !condition ) {
			throw new InvalidOperationException( message );
		}
	}
}
