using System.Text.Json;
using System.Text.Json.Nodes;
using Icod.TermInfo;
using Icod.TermInfo.Compiler;
using Icod.TermInfo.Inspection;
using Path = global::System.IO.Path;

bool verifyFixtures =
	args.Length == 2
	&& string.Equals(
		args[ 0 ],
		"--verify-fixtures",
		StringComparison.Ordinal
	);
bool writeFixtures =
	args.Length == 2
	&& string.Equals(
		args[ 0 ],
		"--write-fixtures",
		StringComparison.Ordinal
	);
if (
	args.Length != 0
	&& !verifyFixtures
	&& !writeFixtures
) {
	throw new ArgumentException(
		"Usage: Icod.TermInfo.DatabaseSet.Sample [--verify-fixtures directory|--write-fixtures directory]"
	);
}

string root = Path.Combine(
	Path.GetTempPath(),
	"Icod.TermInfo.DatabaseSet.Sample."
		+ Guid.NewGuid().ToString( "N" )
);
string firstRoot = Path.Combine( root, "inspection-first" );
string secondRoot = Path.Combine( root, "inspection-second" );
string planningFirstRoot = Path.Combine( root, "planning-first" );
string planningSecondRoot = Path.Combine( root, "planning-second" );

try {
	CompiledTermInfoDatabaseWriter.Write(
		firstRoot,
		[
			CreateTerminal( "sample-shared", 80 ),
			CreateTerminal( "sample-alias-owner-a", 90, "sample-collision" ),
		]
	);
	CompiledTermInfoDatabaseWriter.Write(
		secondRoot,
		[
			CreateTerminal( "sample-shared", 132 ),
			CreateTerminal( "sample-alias-owner-b", 100, "sample-collision" ),
		]
	);
	CompiledTermInfoDatabaseWriter.Write(
		planningFirstRoot,
		CreateParent( "sample-parent-a", margin: true, columns: false )
	);
	CompiledTermInfoDatabaseWriter.Write(
		planningSecondRoot,
		CreateParent( "sample-parent-b", margin: false, columns: true )
	);

	TermInfoDatabaseSet set = TermInfoDatabaseInspector.InspectSet(
		[ firstRoot, secondRoot ]
	);
	TermInfoDatabaseSetLookupResult lookup = set.LookupCanonicalName( "sample-shared" );
	if (
		lookup.Status != TermInfoDatabaseSetLookupStatus.WinnerKnown
		|| lookup.Winner is null
		|| lookup.Winner.DatabaseIndex != 0
		|| lookup.Occurrences.Count != 2
		|| lookup.ShadowedOccurrences.Count != 1
		|| lookup.ShadowedOccurrences[ 0 ].DatabaseIndex != 1
	) {
		throw new InvalidOperationException(
			"The sample database-set precedence result was not the expected first-root winner."
		);
	}

	TermInfoDatabaseSetSemanticAnalysis semantic = set.AnalyzeSemantics();
	TermInfoDatabaseSetIdentityAnalysis repeated = semantic.RepeatedIdentities.Single(
		identity => identity.Identity.Name == "sample-shared"
	);
	if (
		repeated.Relationship
			!= TermInfoDatabaseSetSemanticRelationship.SemanticallyDifferent
	) {
		throw new InvalidOperationException(
			"The sample did not classify the shadowed shared identity as semantically different."
		);
	}
	TermInfoDatabaseSetAliasAnalysis alias = semantic.Aliases.Single(
		item => item.Alias == "sample-collision"
	);
	if ( !alias.HasMultipleCanonicalOwners ) {
		throw new InvalidOperationException(
			"The sample did not retain the expected alias-collision evidence."
		);
	}

	TermInfoDatabaseSet comparisonRight = TermInfoDatabaseInspector.InspectSet(
		[ firstRoot ]
	);
	TermInfoDatabaseSetComparisonResult comparison =
		TermInfoDatabaseSetComparer.Compare( set, comparisonRight );
	if (
		comparison.AreEquivalent
		|| comparison.Differences.Count == 0
	) {
		throw new InvalidOperationException(
			"The sample database-set comparison unexpectedly reported equality."
		);
	}

	TerminalDescription target = new TerminalDescriptionBuilder( "sample-target" )
		.SetDescription( "Icod.TermInfo 1.10 database-set sample target" )
		.SetBoolean( BooleanCapability.AutoRightMargin )
		.SetNumber( NumericCapability.Columns, 80 )
		.Build();
	TermInfoDatabaseSet planningSet = TermInfoDatabaseInspector.InspectSet(
		[ planningFirstRoot, planningSecondRoot ]
	);
	TermInfoDatabaseSetSourcePlanningResult plan =
		TerminalDescriptionSourcePlanner.PlanFromDatabaseSet(
			target,
			planningSet
		);
	if (
		plan.Candidates.Count != 2
		|| plan.SelectedCandidates.Count != 2
		|| plan.SelectedCandidates[ 0 ].CanonicalName != "sample-parent-a"
		|| plan.SelectedCandidates[ 1 ].CanonicalName != "sample-parent-b"
	) {
		throw new InvalidOperationException(
			"The sample planner did not select the two complementary database-set parents."
		);
	}

	string setJson = NormalizeDocument(
		TermInfoJsonRenderer.Render( set ),
		root
	);
	string comparisonJson = NormalizeDocument(
		TermInfoJsonRenderer.Render( comparison ),
		root
	);
	string planJson = NormalizeDocument(
		TermInfoJsonRenderer.Render( plan ),
		root
	);

	if ( verifyFixtures ) {
		VerifyFixture(
			Path.Combine( args[ 1 ], "database-set.json" ),
			setJson
		);
		VerifyFixture(
			Path.Combine( args[ 1 ], "database-set-comparison.json" ),
			comparisonJson
		);
		VerifyFixture(
			Path.Combine( args[ 1 ], "database-set-plan.json" ),
			planJson
		);
	} else if ( writeFixtures ) {
		Directory.CreateDirectory( args[ 1 ] );
		WriteFixture(
			Path.Combine( args[ 1 ], "database-set.json" ),
			setJson
		);
		WriteFixture(
			Path.Combine( args[ 1 ], "database-set-comparison.json" ),
			comparisonJson
		);
		WriteFixture(
			Path.Combine( args[ 1 ], "database-set-plan.json" ),
			planJson
		);
	}

	Console.WriteLine( "databaseSet" );
	Console.WriteLine( setJson );
	Console.WriteLine( "databaseSetComparison" );
	Console.WriteLine( comparisonJson );
	Console.WriteLine( "databaseSetPlan" );
	Console.WriteLine( planJson );
}
finally {
	if ( Directory.Exists( root ) ) {
		Directory.Delete(
			root,
			recursive: true
		);
	}
}

static TerminalDescription CreateTerminal(
	string name,
	int columns,
	params string[] aliases
) {
	TerminalDescriptionBuilder builder = new TerminalDescriptionBuilder( name )
		.SetDescription( $"Icod.TermInfo 1.10 sample {name}" )
		.SetNumber( NumericCapability.Columns, columns );
	foreach ( string alias in aliases ) {
		builder.AddAlias( alias );
	}
	return builder.Build();
}

static TerminalDescription CreateParent(
	string name,
	bool margin,
	bool columns
) {
	TerminalDescriptionBuilder builder = new TerminalDescriptionBuilder( name )
		.SetDescription( $"Icod.TermInfo 1.10 sample {name}" );
	if ( margin ) {
		builder.SetBoolean( BooleanCapability.AutoRightMargin );
	}
	if ( columns ) {
		builder.SetNumber( NumericCapability.Columns, 80 );
	}
	return builder.Build();
}

static string NormalizeDocument(
	string json,
	string root
) {
	ArgumentNullException.ThrowIfNull( json );
	ArgumentException.ThrowIfNullOrWhiteSpace( root );

	JsonNode node = JsonNode.Parse( json )
		?? throw new InvalidOperationException(
			"The rendered JSON document could not be parsed."
		);
	NormalizeNode( node, root );
	return node.ToJsonString(
		new JsonSerializerOptions {
			WriteIndented = false,
		}
	);
}

static void NormalizeNode(
	JsonNode node,
	string root
) {
	ArgumentNullException.ThrowIfNull( node );
	ArgumentException.ThrowIfNullOrWhiteSpace( root );

	if ( node is JsonObject objectNode ) {
		foreach ( string key in objectNode.Select( pair => pair.Key ).ToArray() ) {
			JsonNode? child = objectNode[ key ];
			if ( child is JsonValue value
				&& value.TryGetValue<string>( out string? text )
				&& text is not null
				&& text.StartsWith( root, StringComparison.Ordinal ) ) {
				objectNode[ key ] =
					"$ROOT"
						+ text[ root.Length.. ].Replace( '\\', '/' );
			} else if ( child is not null ) {
				NormalizeNode( child, root );
			}
		}
		return;
	}

	if ( node is JsonArray arrayNode ) {
		for ( int index = 0; index < arrayNode.Count; ++index ) {
			JsonNode? child = arrayNode[ index ];
			if ( child is JsonValue value
				&& value.TryGetValue<string>( out string? text )
				&& text is not null
				&& text.StartsWith( root, StringComparison.Ordinal ) ) {
				arrayNode[ index ] =
					"$ROOT"
						+ text[ root.Length.. ].Replace( '\\', '/' );
			} else if ( child is not null ) {
				NormalizeNode( child, root );
			}
		}
	}
}

static void VerifyFixture(
	string path,
	string actual
) {
	ArgumentException.ThrowIfNullOrWhiteSpace( path );
	ArgumentNullException.ThrowIfNull( actual );

	string expected = File.ReadAllText( path )
		.Replace( "\r\n", "\n", StringComparison.Ordinal )
		.Replace( '\r', '\n' )
		.TrimEnd( '\n' );
	if ( !string.Equals( expected, actual, StringComparison.Ordinal ) ) {
		throw new InvalidOperationException(
			$"The rendered database-set document did not match '{path}'."
		);
	}
}

static void WriteFixture(
	string path,
	string value
) {
	ArgumentException.ThrowIfNullOrWhiteSpace( path );
	ArgumentNullException.ThrowIfNull( value );

	File.WriteAllText(
		path,
		value + "\n",
		new System.Text.UTF8Encoding( encoderShouldEmitUTF8Identifier: false )
	);
}
