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

TermInfoSourceResolveResult resolvedParent =
	TermInfoSourceResolver.Resolve(
		parsed.Document,
		"icod-toolchain-base"
	);
if (
	resolvedParent.HasErrors
	|| resolvedParent.Entry is null
) {
	throw new InvalidOperationException(
		"The sample base entry did not resolve cleanly."
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

TerminalDescription parent =
	resolvedParent.Entry.ToTerminalDescription();
TerminalDescription expected =
	resolved.Entry.ToTerminalDescription();
TerminalDescriptionSourceSynthesisParent[] synthesisParents = [
	new( parent.Name, parent ),
];
string relativeSource =
	TerminalDescriptionSourceSynthesizer.Synthesize(
		expected,
		synthesisParents
	);
if ( relativeSource.Contains( '\r' )
	|| !relativeSource.EndsWith( '\n' ) ) {
	throw new InvalidOperationException(
		"The synthesized sample source did not preserve the RS05 LF-only rendering contract."
	);
}

string combinedSource =
	relativeSource
		+ TerminalDescriptionSourceRenderer.Render( parent );
TermInfoSourceParseResult synthesizedParsed =
	TermInfoSourceParser.Parse(
		combinedSource,
		"toolchain-synthesized.ti"
	);
if ( synthesizedParsed.HasErrors ) {
	throw new InvalidOperationException(
		"The synthesized sample source document did not parse cleanly."
	);
}
TermInfoSourceResolveResult synthesizedResolved =
	TermInfoSourceResolver.Resolve(
		synthesizedParsed.Document,
		expected.Name
	);
if (
	synthesizedResolved.HasErrors
	|| synthesizedResolved.Entry is null
) {
	throw new InvalidOperationException(
		"The synthesized sample child did not resolve cleanly."
	);
}
TermInfoComparisonResult synthesizedComparison =
	TerminalDescriptionComparer.Compare(
		expected,
		synthesizedResolved.Entry.ToTerminalDescription()
	);
if ( !synthesizedComparison.AreEqual ) {
	throw new InvalidOperationException(
		"The synthesized sample child is not semantically equal to the original resolved child."
	);
}

TermInfoSourceCompilationResult compilation =
	TermInfoSourceCompiler.Compile(
		combinedSource,
		"toolchain-synthesized.ti"
	);
if ( compilation.HasErrors ) {
	throw new InvalidOperationException(
		"The synthesized sample source document did not compile cleanly."
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

	Console.Write( relativeSource );
}
finally {
	if ( Directory.Exists( databaseRoot ) ) {
		Directory.Delete(
			databaseRoot,
			recursive: true
		);
	}
}
