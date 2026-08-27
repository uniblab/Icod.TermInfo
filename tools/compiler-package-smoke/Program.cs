using System.Buffers.Binary;
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
		.SetBoolean( BooleanCapability.AutoRightMargin )
		.SetNumber( NumericCapability.Columns, 100000 )
		.SetString(
			StringCapability.ClearScreen,
			"\u001b[H\u001b[2J"
		)
		.SetExtendedBoolean( "XBool" )
		.SetExtendedNumber( "XNum", 200000 )
		.SetExtendedString(
			"XStr",
			"compiler-extended"
		)
		.Build();
byte[] compiled =
	CompiledTermInfoWriter.Write(
		description
	);
Require(
	BinaryPrimitives.ReadUInt16LittleEndian(
		compiled.AsSpan( 0, sizeof( ushort ) )
	) == 0x021E,
	"The Compiler package did not automatically select wide 01036 for wide numeric values."
);
TerminalDescription parsed =
	CompiledTermInfoParser.Parse(
		compiled
	);

Require(
	parsed.Name == description.Name
		&& parsed.Description == description.Description
		&& parsed.Aliases.SequenceEqual( description.Aliases )
		&& parsed.GetBoolean( BooleanCapability.AutoRightMargin )
		&& parsed.GetNumber( NumericCapability.Columns ) == 100000
		&& parsed.GetString( StringCapability.ClearScreen ) == "\u001b[H\u001b[2J"
		&& parsed.ExtendedCapabilities["XBool"].BooleanValue
		&& parsed.ExtendedCapabilities["XNum"].NumberValue == 200000
		&& parsed.ExtendedCapabilities["XStr"].StringValue == "compiler-extended",
	"The Compiler package did not round-trip the C04 wide standard and extended entry."
);
Require(
	typeof( TerminalDescription ).Assembly.GetName().Version
		== new Version( 1, 0, 0, 0 ),
	"The transitive Runtime package must retain the stable 1.x assembly identity."
);

Console.WriteLine(
	"Icod.TermInfo.Compiler package smoke test passed."
);