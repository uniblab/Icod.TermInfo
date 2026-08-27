using System.Buffers.Binary;
using System.Reflection;
using Icod.TermInfo;
using Icod.TermInfo.Compiler;
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

const string source =
	"""
	compiler-smoke-child|Compiler smoke child,
		cols#132,
		use=compiler-smoke-base,

	compiler-smoke-base|Compiler smoke base,
		am,
		lines#43,
	""";
TermInfoSourceCompilationResult sourceCompilation =
	TermInfoSourceCompiler.Compile(
		source,
		"compiler-package-smoke.ti"
	);
Require(
	!sourceCompilation.HasErrors
		&& sourceCompilation.Entries.Count == 2,
	"The Compiler package could not compile a multi-entry source document."
);
TerminalDescription compiledChild =
	CompiledTermInfoParser.Parse(
		sourceCompilation.Entries[0].Data
	);
Require(
	compiledChild.Name == "compiler-smoke-child"
		&& compiledChild.GetNumber( NumericCapability.Columns ) == 132
		&& compiledChild.GetNumber( NumericCapability.Lines ) == 43
		&& compiledChild.GetBoolean( BooleanCapability.AutoRightMargin ),
	"The Compiler package did not preserve C05 source inheritance semantics."
);
Require(
	typeof( TermInfoSourceParser ).Assembly.GetName().Name
		== "Icod.TermInfo.Source",
	"The Compiler package did not expose its C05 Source dependency."
);
Require(
	typeof( TermInfoSourceParser ).Assembly.GetName().Version
		== new Version( 1, 0, 0, 0 ),
	"The transitive Source package must retain the stable 1.x assembly identity."
);

string databaseRoot =
	Path.Combine(
		Path.GetTempPath(),
		"Icod.TermInfo.Compiler-package-smoke-"
			+ Guid.NewGuid().ToString( "N" )
	);

try {
	CompiledTermInfoDatabaseWriter.Write(
		databaseRoot,
		sourceCompilation
	);
	DirectoryTerminalDescriptionProvider databaseProvider =
		new( databaseRoot );
	Require(
		databaseProvider.TryLoad(
			"compiler-smoke-child",
			out TerminalDescription? databaseChild
		)
			&& databaseChild.GetNumber(
				NumericCapability.Columns
			) == 132
			&& databaseChild.GetNumber(
				NumericCapability.Lines
			) == 43
			&& databaseChild.GetBoolean(
				BooleanCapability.AutoRightMargin
			),
		"The Compiler package did not produce a C06 directory-provider-compatible database."
	);
}
finally {
	if ( Directory.Exists( databaseRoot ) ) {
		Directory.Delete(
			databaseRoot,
			recursive: true
		);
	}
}

Console.WriteLine(
	"Icod.TermInfo.Compiler package smoke test passed."
);
