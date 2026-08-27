using System.Reflection;
using Icod.TermInfo;
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
Require(
	inspectionAssembly.GetExportedTypes().Length == 0,
	"I01 must not accidentally expose a public Inspection surface."
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

Console.WriteLine(
	"Icod.TermInfo.Inspection package smoke test passed."
);
