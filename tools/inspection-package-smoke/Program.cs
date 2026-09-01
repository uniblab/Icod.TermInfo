using System.Reflection;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Icod.TermInfo.Source;

static void Require(
	bool condition,
	string message
) {
	ArgumentNullException.ThrowIfNull( message );

	if ( !condition ) {
		throw new InvalidOperationException( message );
	}
}

Assembly inspectionAssembly =
	Assembly.Load(
		"Icod.TermInfo.Inspection"
	);
AssemblyName inspectionName =
	inspectionAssembly.GetName();
Require(
	inspectionName.Name == "Icod.TermInfo.Inspection",
	"The Inspection package assembly could not be loaded."
);
Require(
	inspectionName.Version == new Version( 1, 0, 0, 0 ),
	"The Inspection package must retain the stable 1.x assembly identity."
);
Type[] exportedTypes =
	inspectionAssembly.GetExportedTypes();
Require(
	exportedTypes.Length == 25
		&& exportedTypes.Contains( typeof( TermInfoComparisonResult ) )
		&& exportedTypes.Contains( typeof( TermInfoDatabaseCatalog ) )
		&& exportedTypes.Contains( typeof( TermInfoDatabaseCatalogEntry ) )
		&& exportedTypes.Contains( typeof( TermInfoDatabaseCatalogIssue ) )
		&& exportedTypes.Contains( typeof( TermInfoDatabaseCatalogIssueKind ) )
		&& exportedTypes.Contains( typeof( TermInfoDatabaseCatalogKind ) )
		&& exportedTypes.Contains( typeof( TermInfoDatabaseInspector ) )
		&& exportedTypes.Contains( typeof( TermInfoDatabaseLocation ) )
		&& exportedTypes.Contains( typeof( TermInfoDatabaseLocationKind ) )
		&& exportedTypes.Contains( typeof( TermInfoDifference ) )
		&& exportedTypes.Contains( typeof( TermInfoDifferenceKind ) )
		&& exportedTypes.Contains( typeof( TermInfoInspectionComparison ) )
		&& exportedTypes.Contains( typeof( TermInfoInspectionEngine ) )
		&& exportedTypes.Contains( typeof( TermInfoInspectionResult ) )
		&& exportedTypes.Contains( typeof( TermInfoInspectionTarget ) )
		&& exportedTypes.Contains( typeof( TermInfoSourceComparer ) )
		&& exportedTypes.Contains( typeof( TermInfoSourceRenderer ) )
		&& exportedTypes.Contains( typeof( TerminalDescriptionComparer ) )
		&& exportedTypes.Contains( typeof( TerminalDescriptionSourceCapabilityOrder ) )
		&& exportedTypes.Contains( typeof( TerminalDescriptionSourceLayout ) )
		&& exportedTypes.Contains( typeof( TerminalDescriptionSourceRenderer ) )
		&& exportedTypes.Contains( typeof( TerminalDescriptionSourceRendererOptions ) )
		&& exportedTypes.Contains( typeof( TerminalDescriptionSourceSynthesisOptions ) )
		&& exportedTypes.Contains( typeof( TerminalDescriptionSourceSynthesisParent ) )
		&& exportedTypes.Contains( typeof( TerminalDescriptionSourceSynthesizer ) ),
	"The Inspection package did not expose exactly the frozen 1.7 public surface."
);

Require(
	typeof( TerminalDescription ).Assembly.GetName().Name
		== "Icod.TermInfo",
	"The Inspection package did not restore its Runtime dependency."
);
Require(
	typeof( TerminalDescription ).Assembly.GetName().Version
		== new Version( 1, 0, 0, 0 ),
	"The transitive Runtime package must retain the stable 1.x assembly identity."
);
Require(
	typeof( TermInfoSourceParser ).Assembly.GetName().Name
		== "Icod.TermInfo.Source",
	"The Inspection package did not restore its Source dependency."
);
Require(
	typeof( TermInfoSourceParser ).Assembly.GetName().Version
		== new Version( 1, 0, 0, 0 ),
	"The transitive Source package must retain the stable 1.x assembly identity."
);

IReadOnlyList<TermInfoDatabaseLocation> disabledLocations =
	TermInfoDatabaseInspector.GetSystemLocations(
		new SystemTerminalDescriptionProviderOptions(
			useEnvironment: false,
			useUserDatabase: false,
			useSystemDatabases: false
		)
	);
Require(
	disabledLocations.Count == 0,
	"The T02 database inspector did not honor a fully restricted system discovery policy."
);

string missingCatalogRoot =
	System.IO.Path.Combine(
		System.IO.Path.GetTempPath(),
		$"icod-terminfo-package-smoke-missing-{Guid.NewGuid():N}"
	);
TermInfoDatabaseCatalog missingCatalog =
	TermInfoDatabaseInspector.InspectDirectory(
		missingCatalogRoot
	);
Require(
	missingCatalog.Kind == TermInfoDatabaseCatalogKind.Missing
		&& missingCatalog.Root == System.IO.Path.GetFullPath( missingCatalogRoot )
		&& missingCatalog.Entries.Count == 0
		&& missingCatalog.Issues.Count == 0
		&& missingCatalog.DuplicateCanonicalNames.Count == 0
		&& !missingCatalog.HasIssues,
	"The T03 database catalog did not report a deterministic missing-root snapshot."
);

const string source =
	"inspection-smoke|Inspection package smoke,am,cols#80,";
TermInfoSourceParseResult parsed =
	TermInfoSourceParser.Parse(
		source,
		"inspection-package-smoke.ti"
	);
Require(
	!parsed.HasErrors
		&& parsed.Document.Entries.Count == 1
		&& parsed.Document.Entries[ 0 ].CanonicalName == "inspection-smoke",
	"The Source dependency could not parse a deterministic smoke entry."
);

TermInfoSourceResolveResult resolved =
	TermInfoSourceResolver.Resolve(
		parsed.Document,
		"inspection-smoke"
	);
Require(
	!resolved.HasErrors
		&& resolved.Entry is not null,
	"The Source dependency could not resolve the smoke entry."
);
TerminalDescription terminal =
	resolved.Entry!.ToTerminalDescription();
string rendered =
	TerminalDescriptionSourceRenderer.Render(
		terminal
	);
Require(
	rendered
		== "inspection-smoke|Inspection package smoke,\n"
			+ "    am,\n"
			+ "    cols#80,\n",
	"The I02 renderer did not produce the canonical smoke representation."
);
Require(
	TerminalDescriptionSourceRenderer.Render(
		terminal,
		new TerminalDescriptionSourceRendererOptions()
	) == rendered,
	"The T06 default renderer options did not preserve the frozen I02 representation."
);
string singleLineStandard =
	TerminalDescriptionSourceRenderer.Render(
		terminal,
		new TerminalDescriptionSourceRendererOptions(
			80,
			TerminalDescriptionSourceLayout.SingleLine,
			TerminalDescriptionSourceCapabilityOrder.TermInfoName,
			includeExtendedCapabilities: false
		)
	);
Require(
	singleLineStandard
		== "inspection-smoke|Inspection package smoke, am, cols#80,\n",
	"The T06 configurable renderer did not honor single-line standard-only presentation."
);
TerminalDescriptionSourceSynthesisOptions synthesisOptions =
	new(
		80,
		TerminalDescriptionSourceLayout.Canonical,
		TerminalDescriptionSourceCapabilityOrder.Database,
		TerminalDescriptionSourceSynthesisOptions.DefaultMaximumParentCount,
		includeExtendedCapabilities: true
	);
Require(
	synthesisOptions.LineWidth == 80
		&& synthesisOptions.Layout == TerminalDescriptionSourceLayout.Canonical
		&& synthesisOptions.CapabilityOrder == TerminalDescriptionSourceCapabilityOrder.Database
		&& synthesisOptions.MaximumParentCount == 64
		&& TerminalDescriptionSourceSynthesisOptions.DefaultMaximumParentCount == 64
		&& TerminalDescriptionSourceSynthesisOptions.MaximumSupportedParentCount == 256
		&& synthesisOptions.IncludeExtendedCapabilities,
	"The frozen 1.7 synthesis options surface did not retain its reviewed contract."
);
string synthesizedWithoutParents =
	TerminalDescriptionSourceSynthesizer.Synthesize(
		terminal,
		Array.Empty<TerminalDescriptionSourceSynthesisParent>(),
		synthesisOptions
	);
Require(
	synthesizedWithoutParents == rendered,
	"The RS01 zero-parent synthesizer did not preserve effective renderer semantics."
);
TerminalDescription synthesisParentDescription =
	new TerminalDescriptionBuilder( "inspection-smoke-parent" )
		.AddAlias( "inspection-smoke-parent-alias" )
		.SetDescription( "Inspection package smoke parent" )
		.SetBoolean( BooleanCapability.AutoRightMargin )
		.SetNumber( NumericCapability.Columns, 80 )
		.Build();
TerminalDescriptionSourceSynthesisParent synthesisParent =
	new(
		synthesisParentDescription.Name,
		synthesisParentDescription
	);
Require(
	synthesisParent.UseName == "inspection-smoke-parent"
		&& ReferenceEquals(
			synthesisParent.Description,
			synthesisParentDescription
		),
	"The RS01 package parent descriptor did not preserve source and effective identity."
);
string synthesizedWithParent =
	TerminalDescriptionSourceSynthesizer.Synthesize(
		terminal,
		new[] {
			synthesisParent,
		},
		synthesisOptions
	);
Require(
	synthesizedWithParent
		== "inspection-smoke|Inspection package smoke,\n"
			+ "    use=inspection-smoke-parent,\n",
	"The RS02 package synthesizer did not omit inherited standard capabilities."
);
TerminalDescriptionSourceSynthesisParent synthesisAliasParent =
	new(
		"inspection-smoke-parent-alias",
		synthesisParentDescription
	);
string synthesizedWithRepeatedParents =
	TerminalDescriptionSourceSynthesizer.Synthesize(
		terminal,
		new[] {
			synthesisParent,
			synthesisAliasParent,
		},
		synthesisOptions
	);
Require(
	synthesizedWithRepeatedParents
		== "inspection-smoke|Inspection package smoke,\n"
			+ "    use=inspection-smoke-parent,\n"
			+ "    use=inspection-smoke-parent-alias,\n",
	"The RS04 package synthesizer did not preserve repeated canonical/alias "
		+ "parent references in caller order."
);
TerminalDescription extendedSmokeParent =
	new TerminalDescriptionBuilder( "inspection-smoke-extended-parent" )
		.SetDescription( "Inspection extended smoke parent" )
		.SetExtendedNumber( "XSmoke", 1 )
		.Build();
TerminalDescription extendedSmokeTarget =
	new TerminalDescriptionBuilder( "inspection-smoke-extended-child" )
		.SetDescription( "Inspection extended smoke child" )
		.SetExtendedString( "XSmoke", "two" )
		.Build();
string synthesizedExtended =
	TerminalDescriptionSourceSynthesizer.Synthesize(
		extendedSmokeTarget,
		new[] {
			new TerminalDescriptionSourceSynthesisParent(
				extendedSmokeParent.Name,
				extendedSmokeParent
			),
		}
	);
Require(
	synthesizedExtended.Contains( "    XSmoke=two,\n", StringComparison.Ordinal )
		&& synthesizedExtended.EndsWith(
			"    use=inspection-smoke-extended-parent,\n",
			StringComparison.Ordinal
		),
	"The RS03 package synthesizer did not emit an extended value-kind override."
);
TermInfoSourceParseResult reparsed =
	TermInfoSourceParser.Parse(
		rendered,
		"inspection-package-smoke-rendered.ti"
	);
Require(
	!reparsed.HasErrors
		&& reparsed.Document.Entries.Count == 1,
	"The canonical I02 smoke representation did not reparse."
);

string normalizedUnresolved =
	TermInfoSourceRenderer.Render(
		parsed.Document
	);
Require(
	normalizedUnresolved == rendered,
	"The I03 unresolved renderer did not produce the normalized smoke representation."
);
TermInfoSourceParseResult normalizedParsed =
	TermInfoSourceParser.Parse(
		normalizedUnresolved,
		"inspection-package-smoke-normalized.ti"
	);
Require(
	!normalizedParsed.HasErrors
		&& normalizedParsed.Document.Entries.Count == 1
		&& normalizedParsed.Document.Entries[ 0 ].Fields.Count == 2,
	"The normalized I03 smoke representation did not preserve the unresolved source model."
);

TermInfoComparisonResult comparison =
	TerminalDescriptionComparer.Compare(
		terminal,
		terminal
	);
Require(
	comparison.AreEqual
		&& comparison.Differences.Count == 0,
	"The I04 effective comparer did not report self-comparison as equal."
);

const string alteredSource =
	"inspection-smoke|Inspection package smoke,am,cols#132,";
TermInfoSourceParseResult alteredParsed =
	TermInfoSourceParser.Parse(
		alteredSource,
		"inspection-package-smoke-altered.ti"
	);
Require(
	!alteredParsed.HasErrors
		&& alteredParsed.Document.Entries.Count == 1,
	"The Source dependency could not parse the altered I05 smoke entry."
);
TermInfoComparisonResult sourceComparison =
	TermInfoSourceComparer.Compare(
		parsed.Document.Entries[ 0 ],
		alteredParsed.Document.Entries[ 0 ]
	);
Require(
	!sourceComparison.AreEqual
		&& sourceComparison.Differences.Count == 1
		&& sourceComparison.Differences[ 0 ].Kind
			== TermInfoDifferenceKind.SourceFieldValue
		&& sourceComparison.Differences[ 0 ].LeftSourceField is not null
		&& sourceComparison.Differences[ 0 ].RightSourceField is not null,
	"The I05 source-aware comparer did not report the local numeric difference."
);

InMemoryTerminalDescriptionProvider provider =
	new(
		new[] {
			terminal,
		}
	);
TermInfoInspectionTarget inspectionTarget =
	new(
		provider,
		"inspection-smoke",
		"package smoke provider"
	);
TermInfoInspectionResult inspected =
	TermInfoInspectionEngine.Inspect(
		inspectionTarget
	);
Require(
	ReferenceEquals( inspected.Target, inspectionTarget )
		&& ReferenceEquals( inspected.Terminal, terminal )
		&& inspected.Target.DisplayName == "package smoke provider",
	"The I06 inspection engine did not retain target and terminal identity."
);
Require(
	TermInfoInspectionEngine.Render( inspected ) == rendered,
	"The I06 inspection engine did not delegate to canonical effective rendering."
);
TermInfoInspectionComparison inspectionComparison =
	TermInfoInspectionEngine.Compare(
		inspected,
		inspected
	);
Require(
	inspectionComparison.AreEqual
		&& inspectionComparison.Comparison.Differences.Count == 0
		&& ReferenceEquals( inspectionComparison.Left, inspected )
		&& ReferenceEquals( inspectionComparison.Right, inspected ),
	"The I06 inspection engine did not preserve acquired results during comparison."
);

Console.WriteLine(
	"Icod.TermInfo.Inspection package smoke test passed."
);
