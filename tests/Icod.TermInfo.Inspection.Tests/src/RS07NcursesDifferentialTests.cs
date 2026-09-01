using System.Text;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RS07NcursesDifferentialTests {
	private const string PinnedNcursesVersion = "ncurses 6.5.20250216";

	[Fact]
	public void PinnedNcursesRelativeCorpusIsSemanticallyEquivalent() {
		string fixtureRoot = GetFixtureRoot();
		string casesPath = Path.Combine( fixtureRoot, "cases.tsv" );
		string casesText = File.ReadAllText( casesPath );
		Assert.Contains( PinnedNcursesVersion, casesText, StringComparison.Ordinal );

		string effectivePath = Path.Combine( fixtureRoot, "effective.ti" );
		TermInfoSourceParseResult effective = TermInfoSourceParser.Parse(
			File.ReadAllText( effectivePath ),
			effectivePath
		);
		Assert.False(
			effective.HasErrors,
			FormatDiagnostics( effective.Diagnostics )
		);

		foreach ( NcursesDifferentialCase testCase in ReadCases( casesPath ) ) {
			TerminalDescription target = ResolveDescription(
				effective.Document,
				testCase.Target,
				$"{testCase.Id}: target"
			);
			TerminalDescriptionSourceSynthesisParent[] parents = testCase.Parents
				.Select(
					parentName =>
						new TerminalDescriptionSourceSynthesisParent(
							parentName,
							ResolveDescription(
								effective.Document,
								parentName,
								$"{testCase.Id}: parent {parentName}"
							)
						)
				)
				.ToArray();
			string ncursesRelativePath = Path.Combine(
				fixtureRoot,
				testCase.RelativeFile
			);
			string ncursesRelative = File.ReadAllText( ncursesRelativePath );
			string icodRelative = TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				parents
			);

			Assert.Contains( "use=", ncursesRelative, StringComparison.Ordinal );
			AssertRelativeResolvesToTarget(
				target,
				parents,
				ncursesRelative,
				$"{testCase.Id}: pinned ncurses"
			);
			AssertRelativeResolvesToTarget(
				target,
				parents,
				icodRelative,
				$"{testCase.Id}: Icod"
			);
		}
	}

	[Fact]
	public void CorpusProvenanceAndRepresentativeFamiliesRemainPinned() {
		string fixtureRoot = GetFixtureRoot();
		string readme = File.ReadAllText(
			Path.Combine( fixtureRoot, "README.md" )
		);
		IReadOnlyList<NcursesDifferentialCase> cases = ReadCases(
			Path.Combine( fixtureRoot, "cases.tsv" )
		);

		Assert.Contains( PinnedNcursesVersion, readme, StringComparison.Ordinal );
		Assert.Contains( "Debian GNU/Linux 13", readme, StringComparison.Ordinal );
		Assert.Contains( cases, item => item.Target.StartsWith( "xterm", StringComparison.Ordinal ) );
		Assert.Contains( cases, item => item.Target.StartsWith( "screen", StringComparison.Ordinal ) );
		Assert.Contains( cases, item => item.Target.StartsWith( "tmux", StringComparison.Ordinal ) );
		Assert.Contains( cases, item => string.Equals( item.Target, "linux", StringComparison.Ordinal ) );
		Assert.Contains( cases, item => item.Target.StartsWith( "vt", StringComparison.Ordinal ) );
		Assert.Contains( cases, item => item.Parents.Count > 1 );
	}

	private static IReadOnlyList<NcursesDifferentialCase> ReadCases(
		string path
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( path );

		List<NcursesDifferentialCase> cases = [];
		foreach ( string line in File.ReadLines( path ) ) {
			if ( string.IsNullOrWhiteSpace( line )
				|| line.StartsWith( "#", StringComparison.Ordinal )
				|| line.StartsWith( "id\t", StringComparison.Ordinal ) ) {
				continue;
			}

			string[] fields = line.Split( '\t' );
			if ( fields.Length != 4 ) {
				throw new InvalidDataException(
					$"Malformed RS07 ncurses corpus row: {line}"
				);
			}
			string[] parents = fields[ 2 ].Split(
				',',
				StringSplitOptions.RemoveEmptyEntries
					| StringSplitOptions.TrimEntries
			);
			if ( parents.Length == 0 ) {
				throw new InvalidDataException(
					$"RS07 ncurses corpus case '{fields[ 0 ]}' has no parent."
				);
			}
			cases.Add(
				new NcursesDifferentialCase(
					fields[ 0 ],
					fields[ 1 ],
					parents,
					fields[ 3 ]
				)
			);
		}
		return cases;
	}

	private static TerminalDescription ResolveDescription(
		TermInfoSourceDocument document,
		string name,
		string context
	) {
		ArgumentNullException.ThrowIfNull( document );
		ArgumentException.ThrowIfNullOrWhiteSpace( name );
		ArgumentException.ThrowIfNullOrWhiteSpace( context );

		TermInfoSourceResolveResult resolved = TermInfoSourceResolver.Resolve(
			document,
			name
		);
		Assert.False(
			resolved.HasErrors,
			$"{context}{Environment.NewLine}{FormatDiagnostics( resolved.Diagnostics )}"
		);
		Assert.NotNull( resolved.Entry );
		return resolved.Entry!.ToTerminalDescription();
	}

	private static void AssertRelativeResolvesToTarget(
		TerminalDescription target,
		IReadOnlyList<TerminalDescriptionSourceSynthesisParent> parents,
		string relativeSource,
		string context
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( parents );
		ArgumentNullException.ThrowIfNull( relativeSource );
		ArgumentException.ThrowIfNullOrWhiteSpace( context );

		StringBuilder source = new();
		source.Append( relativeSource );
		HashSet<string> rendered = new( StringComparer.Ordinal );
		foreach ( TerminalDescriptionSourceSynthesisParent parent in parents ) {
			if ( !rendered.Add( parent.Description.Name ) ) {
				continue;
			}
			source.Append(
				TerminalDescriptionSourceRenderer.Render(
					parent.Description
				)
			);
		}

		TermInfoSourceParseResult parsed = TermInfoSourceParser.Parse(
			source.ToString(),
			$"rs07-{context}.ti"
		);
		Assert.False(
			parsed.HasErrors,
			$"{context}{Environment.NewLine}{FormatDiagnostics( parsed.Diagnostics )}"
		);
		TermInfoSourceResolveResult resolved = TermInfoSourceResolver.Resolve(
			parsed.Document,
			target.Name
		);
		Assert.False(
			resolved.HasErrors,
			$"{context}{Environment.NewLine}{FormatDiagnostics( resolved.Diagnostics )}"
		);
		Assert.NotNull( resolved.Entry );
		TermInfoComparisonResult comparison = TerminalDescriptionComparer.Compare(
			target,
			resolved.Entry!.ToTerminalDescription()
		);
		Assert.True(
			comparison.AreEqual,
			$"{context}{Environment.NewLine}"
				+ string.Join(
					Environment.NewLine,
					comparison.Differences.Select(
						difference => difference.ToString()
					)
				)
		);
	}

	private static string FormatDiagnostics(
		IEnumerable<TermInfoSourceDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( diagnostics );

		return string.Join(
			Environment.NewLine,
			diagnostics.Select(
				diagnostic => diagnostic.Message
			)
		);
	}

	private static string GetFixtureRoot() {
		return Path.Combine(
			FindRepositoryRoot(),
			"tests",
			"Icod.TermInfo.Inspection.Tests",
			"fixtures",
			"rs07-ncurses-6.5.20250216"
		);
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current = new( AppContext.BaseDirectory );
		while ( current is not null ) {
			if ( File.Exists( Path.Combine( current.FullName, "Icod.TermInfo.sln" ) ) ) {
				return current.FullName;
			}
			current = current.Parent;
		}
		throw new DirectoryNotFoundException(
			"Could not locate the repository root."
		);
	}

	private sealed record NcursesDifferentialCase(
		string Id,
		string Target,
		IReadOnlyList<string> Parents,
		string RelativeFile
	);
}
