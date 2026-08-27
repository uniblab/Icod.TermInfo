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
	exportedTypes.Length == 6
		&& exportedTypes.Contains( typeof( TermInfoComparisonResult ) )
		&& exportedTypes.Contains( typeof( TermInfoDifference ) )
		&& exportedTypes.Contains( typeof( TermInfoDifferenceKind ) )
		&& exportedTypes.Contains( typeof( TermInfoSourceRenderer ) )
		&& exportedTypes.Contains( typeof( TerminalDescriptionComparer ) )
		&& exportedTypes.Contains( typeof( TerminalDescriptionSourceRenderer ) ),
	"The Inspection package did not expose exactly the reviewed I02-I04 surface."
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

Console.WriteLine(
	"Icod.TermInfo.Inspection package smoke test passed."
);
