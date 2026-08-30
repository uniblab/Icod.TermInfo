using Icod.TermInfo;
using Icod.TermInfo.Compiler;
using Icod.TermInfo.Inspection;
using Icod.TermInfo.Source;
using Path = global::System.IO.Path;

const string source =
	"""
	icod-toolchain-base|Toolchain sample base,
		am,
		lines#24,

	icod-toolchain-child|Toolchain sample child,
		cols#120,
		clear=\E[H\E[2J,
		use=icod-toolchain-base,
	""";

TermInfoSourceParseResult parsed =
	TermInfoSourceParser.Parse(
		source,
		"toolchain-sample.ti"
	);
if ( parsed.HasErrors ) {
	throw new InvalidOperationException(
		"The sample source document did not parse cleanly."
	);
}

TermInfoSourceResolveResult resolved =
	TermInfoSourceResolver.Resolve(
		parsed.Document,
		"icod-toolchain-child"
	);
if (
	resolved.HasErrors
	|| resolved.Entry is null
) {
	throw new InvalidOperationException(
		"The sample child entry did not resolve cleanly."
	);
}

TerminalDescription expected =
	resolved.Entry.ToTerminalDescription();
TermInfoSourceCompilationResult compilation =
	TermInfoSourceCompiler.Compile(
		source,
		"toolchain-sample.ti"
	);
if ( compilation.HasErrors ) {
	throw new InvalidOperationException(
		"The sample source document did not compile cleanly."
	);
}

string databaseRoot =
	Path.Combine(
		Path.GetTempPath(),
		"Icod.TermInfo.Toolchain.Sample."
			+ Guid.NewGuid().ToString( "N" )
	);

try {
	CompiledTermInfoDatabaseWriter.Write(
		databaseRoot,
		compilation
	);

	DirectoryTerminalDescriptionProvider provider =
		new( databaseRoot );
	if (
		!provider.TryLoad(
			"icod-toolchain-child",
			out TerminalDescription? acquired
		)
		|| acquired is null
	) {
		throw new InvalidOperationException(
			"The compiled sample entry could not be acquired from the temporary database."
		);
	}

	TermInfoComparisonResult comparison =
		TerminalDescriptionComparer.Compare(
			expected,
			acquired
		);
	if ( !comparison.AreEqual ) {
		throw new InvalidOperationException(
			"The acquired compiled entry is not semantically equal to the resolved source entry."
		);
	}

	Console.Write(
		TerminalDescriptionSourceRenderer.Render(
			acquired
		)
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
