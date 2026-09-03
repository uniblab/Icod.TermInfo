using System.Text;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo.Compiler;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.InfoCmp.Tests;

[Collection( EnvironmentSensitiveCollection.Name )]
public sealed class MI05JsonAutomationCommandTests {
	[Fact]
	public async Task TerminalAndComparisonJsonExactlyReuseInspectionRenderer() {
		string root = CreateTemporaryDirectory();
		try {
			TerminalDescription left =
				new TerminalDescriptionBuilder( "mi05-left" )
					.SetDescription( "MI05 left" )
					.SetBoolean( BooleanCapability.AutoRightMargin )
					.SetNumber( NumericCapability.Columns, 80 )
					.Build();
			TerminalDescription right =
				new TerminalDescriptionBuilder( "mi05-right" )
					.SetDescription( "MI05 right" )
					.SetNumber( NumericCapability.Columns, 132 )
					.Build();
			Publish( root, left );
			Publish( root, right );

			TerminalDescription inspectedLeft = Inspect( root, left.Name );
			TerminalDescription inspectedRight = Inspect( root, right.Name );
			CommandResult terminal = await RunAsync(
				"--json",
				"-A",
				root,
				left.Name
			);
			CommandResult comparison = await RunAsync(
				"--json",
				"-d",
				"-A",
				root,
				"-B",
				root,
				left.Name,
				right.Name
			);

			Assert.Equal( CommandExitCodes.Success, terminal.Status );
			Assert.Equal(
				TermInfoJsonRenderer.Render( inspectedLeft ) + "\n",
				terminal.Stdout
			);
			Assert.Equal( string.Empty, terminal.Stderr );
			Assert.Equal( CommandExitCodes.Success, comparison.Status );
			Assert.Equal(
				TermInfoJsonRenderer.Render(
					TerminalDescriptionComparer.Compare(
						inspectedLeft,
						inspectedRight
					)
				) + "\n",
				comparison.Stdout
			);
			Assert.Equal( string.Empty, comparison.Stderr );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task ExplicitCandidatePlanningJsonExactlyReusesPlanRenderer() {
		string root = CreateTemporaryDirectory();
		try {
			TerminalDescription candidate =
				new TerminalDescriptionBuilder( "mi05-explicit-parent" )
					.SetDescription( "MI05 explicit parent" )
					.SetNumber( NumericCapability.Columns, 80 )
					.Build();
			TerminalDescription target =
				new TerminalDescriptionBuilder( "mi05-explicit-target" )
					.SetDescription( "MI05 explicit target" )
					.SetNumber( NumericCapability.Columns, 80 )
					.SetBoolean( BooleanCapability.AutoRightMargin )
					.Build();
			Publish( root, candidate );
			Publish( root, target );

			TerminalDescriptionSourcePlan expected =
				TerminalDescriptionSourcePlanner.Plan(
					Inspect( root, target.Name ),
					[
						new TerminalDescriptionSourceSynthesisParent(
							candidate.Name,
							Inspect( root, candidate.Name )
						),
					]
				);
			CommandResult result = await RunAsync(
				"--json",
				"--plan-use",
				"-A",
				root,
				"-B",
				root,
				target.Name,
				candidate.Name
			);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Equal(
				TermInfoJsonRenderer.Render( expected ) + "\n",
				result.Stdout
			);
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task AllCandidatesJsonUsesExactCatalogPlanAndExcludesTarget() {
		string root = CreateTemporaryDirectory();
		try {
			TerminalDescription useful =
				new TerminalDescriptionBuilder( "mi05-useful" )
					.SetDescription( "MI05 useful parent" )
					.SetNumber( NumericCapability.Columns, 80 )
					.Build();
			TerminalDescription decoy =
				new TerminalDescriptionBuilder( "mi05-decoy" )
					.SetDescription( "MI05 decoy" )
					.SetNumber( NumericCapability.Lines, 12 )
					.Build();
			TerminalDescription target =
				new TerminalDescriptionBuilder( "mi05-target" )
					.SetDescription( "MI05 target" )
					.SetNumber( NumericCapability.Columns, 80 )
					.SetString( StringCapability.ClearScreen, "\u001b[H\u001b[2J" )
					.Build();
			Publish( root, useful );
			Publish( root, decoy );
			Publish( root, target );

			TerminalDescription inspectedTarget = Inspect( root, target.Name );
			TerminalDescriptionSourcePlan expected =
				TerminalDescriptionSourcePlanner.PlanFromDirectory(
					inspectedTarget,
					root,
					new TerminalDescriptionSourcePlanningOptions()
				);
			CommandResult json = await RunAsync(
				"--json",
				"--plan-use",
				"--all-candidates",
				"-A",
				root,
				"-B",
				root,
				target.Name
			);
			CommandResult source = await RunAsync(
				"--plan-use",
				"--all-candidates",
				"-A",
				root,
				"-B",
				root,
				target.Name
			);

			Assert.Equal( CommandExitCodes.Success, json.Status );
			Assert.Equal(
				TermInfoJsonRenderer.Render( expected ) + "\n",
				json.Stdout
			);
			Assert.Equal( string.Empty, json.Stderr );
			Assert.DoesNotContain(
				"use=mi05-target",
				json.Stdout,
				StringComparison.Ordinal
			);
			Assert.Equal( CommandExitCodes.Success, source.Status );
			Assert.Equal( expected.Source, source.Stdout );
			Assert.Equal( string.Empty, source.Stderr );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task AllCandidatesRejectsIncompleteCatalogWithoutPartialStdout() {
		string root = CreateTemporaryDirectory();
		try {
			TerminalDescription target =
				new TerminalDescriptionBuilder( "mi05-broken-target" )
					.SetDescription( "MI05 broken target" )
					.Build();
			Publish( root, target );
			string malformedDirectory = System.IO.Path.Combine( root, "78" );
			Directory.CreateDirectory( malformedDirectory );
			File.WriteAllBytes(
				System.IO.Path.Combine( malformedDirectory, "malformed" ),
				[ 0x01, 0x02, 0x03 ]
			);

			CommandResult result = await RunAsync(
				"--json",
				"--plan-use",
				"--all-candidates",
				"-A",
				root,
				"-B",
				root,
				target.Name
			);

			Assert.Equal( CommandExitCodes.Failure, result.Status );
			Assert.Equal( string.Empty, result.Stdout );
			Assert.Contains( "INFOCMP0004 error", result.Stderr, StringComparison.Ordinal );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task InvalidJsonAndAllCandidateFormsAreUsageErrors() {
		foreach ( string[] args in new[] {
			new[] { "--all-candidates", "target" },
			[ "--plan-use", "--all-candidates", "target" ],
			[ "--plan-use", "--all-candidates", "-B", "catalog", "target", "candidate" ],
			[ "--json", "-u", "target", "parent" ],
			[ "--json", "-c", "left", "right" ],
			[ "--json", "-0", "target" ],
		} ) {
			CommandResult result = await RunAsync( args );

			Assert.Equal( CommandExitCodes.UsageError, result.Status );
			Assert.Equal( string.Empty, result.Stdout );
			Assert.NotEqual( string.Empty, result.Stderr );
		}
	}

	[Fact]
	public async Task JsonHonorsPreCanceledTokenWithoutOutput() {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();

		int status = await Command.RunAsync(
			[ "--json", "target" ],
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
	public async Task HelpDocumentationAndDistributionFreezeMi05Contract() {
		CommandResult help = await RunAsync( "--help" );
		string root = FindRepositoryRoot();
		string record = File.ReadAllText(
			System.IO.Path.Combine(
				root,
				"docs",
				"1.9.0-MI05-INFOCMP-TOE-JSON-AUTOMATION.md"
			)
		);

		Assert.Equal( CommandExitCodes.Success, help.Status );
		Assert.Contains( "--json", help.Stdout, StringComparison.Ordinal );
		Assert.Contains( "--all-candidates", help.Stdout, StringComparison.Ordinal );
		Assert.Contains( "1.9.0-Alpha-5", record, StringComparison.Ordinal );
		Assert.Contains( "PlanFromDirectory", record, StringComparison.Ordinal );
		Assert.Contains( "exactly one LF", record, StringComparison.Ordinal );
		Assert.Contains( "MI06", record, StringComparison.Ordinal );
		foreach ( string script in new[] {
			".github/scripts/smoke-tool-package.ps1",
			".github/scripts/smoke-tool-archive.ps1",
		} ) {
			string smoke = File.ReadAllText(
				System.IO.Path.Combine(
					root,
					script.Replace(
						'/',
						System.IO.Path.DirectorySeparatorChar
					)
				)
			);
			Assert.Contains( "'--json'", smoke, StringComparison.Ordinal );
			Assert.Contains( "'--all-candidates'", smoke, StringComparison.Ordinal );
			Assert.Contains( "ConvertFrom-Json", smoke, StringComparison.Ordinal );
			Assert.Contains( "databaseCatalog", smoke, StringComparison.Ordinal );
		}
	}

	private static TerminalDescription Inspect(
		string root,
		string name
	) {
		TermInfoDatabaseCatalog catalog =
			TermInfoDatabaseInspector.InspectDirectory( root );
		return catalog.Entries.Single(
			entry => string.Equals(
				entry.Name,
				name,
				StringComparison.Ordinal
			)
		).Terminal;
	}

	private static void Publish(
		string root,
		TerminalDescription description
	) => CompiledTermInfoDatabaseWriter.Write( root, description );

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
			$"icod-terminfo-infocmp-mi05-{Guid.NewGuid():N}"
		);
		Directory.CreateDirectory( path );
		return path;
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

	private static void DeleteTemporaryDirectory(
		string path
	) {
		try {
			Directory.Delete( path, recursive: true );
		} catch ( IOException ) {
		} catch ( UnauthorizedAccessException ) {
		}
	}

	private sealed record CommandResult(
		int Status,
		string Stdout,
		string Stderr
	);
}
