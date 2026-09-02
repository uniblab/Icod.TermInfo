using System.Text;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo;
using Icod.TermInfo.Compiler;
using Icod.TermInfo.Inspection;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.InfoCmp.Tests;

[Collection( EnvironmentSensitiveCollection.Name )]
public sealed class RP06PlanningCommandTests {
	[Fact]
	public async Task ExplicitCandidatesSelectUsefulAliasAndPreserveSemantics() {
		string targetRoot = CreateTemporaryDirectory();
		string candidateRoot = CreateTemporaryDirectory();
		try {
			TerminalDescription decoy =
				new TerminalDescriptionBuilder( "rp06-decoy" )
					.SetDescription( "RP06 decoy candidate" )
					.SetNumber( NumericCapability.Lines, 12 )
					.Build();
			TerminalDescription useful =
				new TerminalDescriptionBuilder( "rp06-useful" )
					.AddAlias( "rp06-useful-alias" )
					.SetDescription( "RP06 useful candidate" )
					.SetBoolean( BooleanCapability.AutoRightMargin )
					.SetNumber( NumericCapability.Columns, 80 )
					.SetNumber( NumericCapability.Lines, 24 )
					.Build();
			TerminalDescription target =
				new TerminalDescriptionBuilder( "rp06-target" )
					.SetDescription( "RP06 planning target" )
					.SetBoolean( BooleanCapability.AutoRightMargin )
					.SetNumber( NumericCapability.Columns, 80 )
					.SetNumber( NumericCapability.Lines, 24 )
					.SetString( StringCapability.ClearScreen, "\u001b[H\u001b[2J" )
					.Build();
			Publish( targetRoot, target );
			Publish( candidateRoot, decoy );
			Publish( candidateRoot, useful );

			CommandResult result = await RunAsync(
				"-A",
				targetRoot,
				"-B",
				candidateRoot,
				"-1",
				"-s",
				"i",
				"--plan-use",
				target.Name,
				decoy.Name,
				"rp06-useful-alias"
			);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Equal( string.Empty, result.Stderr );
			Assert.Contains(
				"    use=rp06-useful-alias,\n",
				result.Stdout,
				StringComparison.Ordinal
			);
			Assert.DoesNotContain(
				"use=rp06-decoy",
				result.Stdout,
				StringComparison.Ordinal
			);
			AssertSemanticRoundTrip(
				target,
				[ useful ],
				result.Stdout
			);
		} finally {
			DeleteTemporaryDirectory( targetRoot );
			DeleteTemporaryDirectory( candidateRoot );
		}
	}

	[Fact]
	public async Task CandidateOrderBreaksOtherwiseEqualPlanningTie() {
		string root = CreateTemporaryDirectory();
		try {
			TerminalDescription first =
				new TerminalDescriptionBuilder( "rp06-alpha" )
					.SetDescription( "RP06 first equal candidate" )
					.SetNumber( NumericCapability.Columns, 80 )
					.Build();
			TerminalDescription second =
				new TerminalDescriptionBuilder( "rp06-bravo" )
					.SetDescription( "RP06 second equal candidate" )
					.SetNumber( NumericCapability.Columns, 80 )
					.Build();
			TerminalDescription target =
				new TerminalDescriptionBuilder( "rp06-tie-target" )
					.SetDescription( "RP06 tie target" )
					.SetNumber( NumericCapability.Columns, 80 )
					.Build();
			Publish( root, first );
			Publish( root, second );
			Publish( root, target );

			CommandResult result = await RunAsync(
				"-A",
				root,
				"-B",
				root,
				"--max-parents",
				"1",
				"--plan-use",
				target.Name,
				second.Name,
				first.Name
			);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Contains( "use=rp06-bravo", result.Stdout, StringComparison.Ordinal );
			Assert.DoesNotContain( "use=rp06-alpha", result.Stdout, StringComparison.Ordinal );
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task ExhaustiveDefaultRejectsSmallBudgetAndBoundedModeOptsIn() {
		string root = CreateTemporaryDirectory();
		try {
			TerminalDescription useful =
				new TerminalDescriptionBuilder( "rp06-budget-useful" )
					.SetDescription( "RP06 budget useful candidate" )
					.SetNumber( NumericCapability.Columns, 80 )
					.Build();
			TerminalDescription decoy =
				new TerminalDescriptionBuilder( "rp06-budget-decoy" )
					.SetDescription( "RP06 budget decoy candidate" )
					.SetNumber( NumericCapability.Lines, 24 )
					.Build();
			TerminalDescription target =
				new TerminalDescriptionBuilder( "rp06-budget-target" )
					.SetDescription( "RP06 budget target" )
					.SetNumber( NumericCapability.Columns, 80 )
					.Build();
			Publish( root, useful );
			Publish( root, decoy );
			Publish( root, target );

			string[] commonArguments = [
				"-A",
				root,
				"-B",
				root,
				"--max-parents",
				"2",
				"--max-plans",
				"2",
				"--plan-use",
				target.Name,
				useful.Name,
				decoy.Name,
			];
			CommandResult exhaustive = await RunAsync( commonArguments );
			CommandResult bounded = await RunAsync(
				[ "--allow-bounded", .. commonArguments ]
			);

			Assert.Equal( CommandExitCodes.Failure, exhaustive.Status );
			Assert.Equal( string.Empty, exhaustive.Stdout );
			Assert.Contains( "INFOCMP0004 error", exhaustive.Stderr, StringComparison.Ordinal );
			Assert.Contains( "Exhaustive", exhaustive.Stderr, StringComparison.Ordinal );
			Assert.Equal( CommandExitCodes.Success, bounded.Status );
			Assert.Contains( "use=rp06-budget-useful", bounded.Stdout, StringComparison.Ordinal );
			Assert.Equal( string.Empty, bounded.Stderr );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Theory]
	[InlineData( "-u" )]
	[InlineData( "-d" )]
	[InlineData( "-c" )]
	[InlineData( "-n" )]
	[InlineData( "-q" )]
	[InlineData( "-D" )]
	public async Task PlanningRejectsIncompatibleSelectors( string option ) {
		CommandResult result = await RunAsync(
			option,
			"--plan-use",
			"target",
			"candidate"
		);

		Assert.Equal( CommandExitCodes.UsageError, result.Status );
		Assert.Equal( string.Empty, result.Stdout );
		Assert.NotEqual( string.Empty, result.Stderr );
	}

	[Theory]
	[InlineData( "--max-parents", "65" )]
	[InlineData( "--max-parents", "-1" )]
	[InlineData( "--max-plans", "0" )]
	[InlineData( "--max-plans", "1000001" )]
	public async Task PlanningBoundsRejectUnsupportedValues(
		string option,
		string value
	) {
		CommandResult result = await RunAsync(
			option,
			value,
			"--plan-use",
			"target",
			"candidate"
		);

		Assert.Equal( CommandExitCodes.UsageError, result.Status );
		Assert.Equal( string.Empty, result.Stdout );
		Assert.Contains( "requires", result.Stderr, StringComparison.Ordinal );
	}

	[Fact]
	public async Task PlanningPolicyControlsRequirePlanningMode() {
		CommandResult result = await RunAsync(
			"--allow-bounded",
			"target"
		);
		CommandResult conflict = await RunAsync(
			"--require-exhaustive",
			"--allow-bounded",
			"--plan-use",
			"target",
			"candidate"
		);

		Assert.Equal( CommandExitCodes.UsageError, result.Status );
		Assert.Equal( string.Empty, result.Stdout );
		Assert.Contains( "require '--plan-use'", result.Stderr, StringComparison.Ordinal );
		Assert.Equal( CommandExitCodes.UsageError, conflict.Status );
		Assert.Equal( string.Empty, conflict.Stdout );
		Assert.Contains( "mutually exclusive", conflict.Stderr, StringComparison.Ordinal );
	}

	[Fact]
	public async Task PlanningRequiresCandidateAndRejectsDuplicateReference() {
		CommandResult missing = await RunAsync(
			"--plan-use",
			"target"
		);
		CommandResult duplicated = await RunAsync(
			"--plan-use",
			"target",
			"candidate",
			"candidate"
		);

		Assert.Equal( CommandExitCodes.UsageError, missing.Status );
		Assert.Contains( "at least one candidate", missing.Stderr, StringComparison.Ordinal );
		Assert.Equal( CommandExitCodes.UsageError, duplicated.Status );
		Assert.Contains( "duplicated", duplicated.Stderr, StringComparison.Ordinal );
	}

	[Fact]
	public async Task PlanningHonorsPreCanceledTokenWithoutOutput() {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();

		int status = await Command.RunAsync(
			[ "--plan-use", "target", "candidate" ],
			stdin,
			stdout,
			stderr,
			cancellation.Token
		);

		Assert.Equal( CommandExitCodes.Canceled, status );
		Assert.Empty( stdout.ToArray() );
		Assert.Empty( stderr.ToArray() );
	}

	[Fact]
	public async Task HelpAndImplementationRecordFreezeRp06Contract() {
		CommandResult help = await RunAsync( "--help" );
		string root = FindRepositoryRoot();
		string implementation = File.ReadAllText(
			System.IO.Path.Combine(
				root,
				"docs",
				"1.8.0-RP06-INFOCMP-PLANNING-COMMAND-AND-DISTRIBUTION.md"
			)
		);

		Assert.Equal( CommandExitCodes.Success, help.Status );
		Assert.Contains( "--plan-use", help.Stdout, StringComparison.Ordinal );
		Assert.Contains( "--max-parents", help.Stdout, StringComparison.Ordinal );
		Assert.Contains( "--max-plans", help.Stdout, StringComparison.Ordinal );
		Assert.Contains( "--require-exhaustive", help.Stdout, StringComparison.Ordinal );
		Assert.Contains( "--allow-bounded", help.Stdout, StringComparison.Ordinal );
		Assert.Contains( "1.8.0-Alpha-6", implementation, StringComparison.Ordinal );
		Assert.Contains( "stdout", implementation, StringComparison.Ordinal );
		Assert.Contains( "six standalone archives", implementation, StringComparison.Ordinal );
	}

	[Fact]
	public void PackageArchiveAndToolSuiteSurfacesExercisePlanning() {
		string root = FindRepositoryRoot();
		string packageSmoke = ReadRepositoryFile(
			root,
			".github/scripts/smoke-tool-package.ps1"
		);
		string archiveSmoke = ReadRepositoryFile(
			root,
			".github/scripts/smoke-tool-archive.ps1"
		);
		string sample = ReadRepositoryFile(
			root,
			"samples/ToolSuite/README.md"
		);

		foreach ( string smoke in new[] { packageSmoke, archiveSmoke } ) {
			Assert.Contains( "'--plan-use'", smoke, StringComparison.Ordinal );
			Assert.Contains( "release-plan-decoy", smoke, StringComparison.Ordinal );
			Assert.Contains( "use=release-plan-parent", smoke, StringComparison.Ordinal );
			Assert.Contains( "'-c'", smoke, StringComparison.Ordinal );
		}
		Assert.Contains( "infocmp -A ./terminfo", sample, StringComparison.Ordinal );
		Assert.Contains( "icod-terminfo infocmp", sample, StringComparison.Ordinal );
		Assert.Contains( "--plan-use", sample, StringComparison.Ordinal );
		Assert.Contains( "tic -c -x planned-validation.ti", sample, StringComparison.Ordinal );
		Assert.True(
			File.Exists(
				System.IO.Path.Combine(
					root,
					"samples",
					"ToolSuite",
					"planning-parent.ti"
				)
			)
		);

		foreach ( string workflowPath in new[] {
			".github/workflows/pull-request.yaml",
			".github/workflows/main.yaml",
			".github/workflows/release.yaml",
		} ) {
			string workflow = ReadRepositoryFile( root, workflowPath );
			Assert.Contains( "windows-11-arm", workflow, StringComparison.Ordinal );
			Assert.Contains( "ubuntu-24.04-arm", workflow, StringComparison.Ordinal );
			Assert.Contains( "macos-15-intel", workflow, StringComparison.Ordinal );
			Assert.Contains( "name: Archive ${{ matrix.name }}", workflow, StringComparison.Ordinal );
		}
	}

	private static void AssertSemanticRoundTrip(
		TerminalDescription target,
		IReadOnlyList<TerminalDescription> parents,
		string relativeSource
	) {
		StringBuilder source = new();
		source.Append( relativeSource );
		foreach ( TerminalDescription parent in parents ) {
			source.Append( TerminalDescriptionSourceRenderer.Render( parent ) );
		}
		TermInfoSourceParseResult parsed = TermInfoSourceParser.Parse(
			source.ToString(),
			"rp06-command-roundtrip.ti"
		);
		Assert.False( parsed.HasErrors );
		TermInfoSourceResolveResult resolved = TermInfoSourceResolver.Resolve(
			parsed.Document,
			target.Name
		);
		Assert.False( resolved.HasErrors );
		Assert.NotNull( resolved.Entry );
		TermInfoComparisonResult comparison = TerminalDescriptionComparer.Compare(
			target,
			resolved.Entry!.ToTerminalDescription()
		);
		Assert.True(
			comparison.AreEqual,
			string.Join(
				Environment.NewLine,
				comparison.Differences.Select(
					difference => difference.ToString()
				)
			)
		);
	}

	private static void Publish(
		string root,
		TerminalDescription description
	) {
		CompiledTermInfoDatabaseWriter.Write(
			root,
			description
		);
	}

	private static async Task<CommandResult> RunAsync(
		params string[] args
	) {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();
		int status = await Command.RunAsync(
			args,
			stdin,
			stdout,
			stderr
		);

		return new CommandResult(
			status,
			Encoding.UTF8.GetString( stdout.ToArray() ),
			Encoding.UTF8.GetString( stderr.ToArray() )
		);
	}

	private static string CreateTemporaryDirectory() {
		string path = System.IO.Path.Combine(
			System.IO.Path.GetTempPath(),
			$"icod-terminfo-infocmp-rp06-{Guid.NewGuid():N}"
		);
		Directory.CreateDirectory( path );
		return path;
	}

	private static void DeleteTemporaryDirectory(
		string path
	) {
		try {
			Directory.Delete(
				path,
				recursive: true
			);
		} catch ( IOException ) {
		} catch ( UnauthorizedAccessException ) {
		}
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current = new( AppContext.BaseDirectory );
		while ( current is not null ) {
			if ( File.Exists(
				System.IO.Path.Combine( current.FullName, "Icod.TermInfo.sln" )
			) ) {
				return current.FullName;
			}
			current = current.Parent;
		}

		throw new DirectoryNotFoundException(
			"Could not locate the repository root."
		);
	}

	private static string ReadRepositoryFile(
		string root,
		string relativePath
	) {
		return File.ReadAllText(
			System.IO.Path.Combine(
				root,
				relativePath.Replace(
					'/',
					System.IO.Path.DirectorySeparatorChar
				)
			)
		);
	}

	private sealed record CommandResult(
		int Status,
		string Stdout,
		string Stderr
	);
}
