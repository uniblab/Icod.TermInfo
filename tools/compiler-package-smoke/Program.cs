using System.Reflection;
using Icod.TermInfo;
using Icod.TermInfo.Compiler;

static void Require(
	bool condition,
	string message
) {
	ArgumentNullException.ThrowIfNull( message );

	if ( !condition ) {
		throw new InvalidOperationException( message );
	}
}

Assembly compilerAssembly =
	typeof( CompiledTermInfoWriter ).Assembly;
AssemblyName compilerName =
	compilerAssembly.GetName();
Require(
	compilerName.Name == "Icod.TermInfo.Compiler",
	"The Compiler package assembly could not be loaded."
);
Require(
	compilerName.Version == new Version( 1, 0, 0, 0 ),
	"The Compiler package must retain the stable 1.x assembly identity."
);

TerminalDescription description =
	new TerminalDescriptionBuilder( "compiler-package-smoke" )
		.AddAlias( "compiler-smoke" )
		.SetDescription( "Compiler package smoke terminal" )
		.Build();
byte[] compiled =
	CompiledTermInfoWriter.Write(
		description
	);
TerminalDescription parsed =
	CompiledTermInfoParser.Parse(
		compiled
	);

Require(
	parsed.Name == description.Name
		&& parsed.Description == description.Description
		&& parsed.Aliases.SequenceEqual( description.Aliases ),
	"The Compiler package did not round-trip the C01 identity-only entry."
);
Require(
	typeof( TerminalDescription ).Assembly.GetName().Version
		== new Version( 1, 0, 0, 0 ),
	"The transitive Runtime package must retain the stable 1.x assembly identity."
);

Console.WriteLine(
	"Icod.TermInfo.Compiler package smoke test passed."
);