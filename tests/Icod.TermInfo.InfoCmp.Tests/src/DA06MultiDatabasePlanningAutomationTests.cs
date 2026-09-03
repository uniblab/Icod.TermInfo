using System.Text;
using System.Text.Json;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo.Compiler;
using Xunit;

namespace Icod.TermInfo.InfoCmp.Tests;

public sealed class DA06MultiDatabasePlanningAutomationTests {
	[Fact]
	public async Task CandidateRootsEmitVersionTwoPlanWhileLegacyBRemainsVersionOne() {
		string targetRoot = CreateTemporaryDirectory();
		string first = CreateTemporaryDirectory();
		string second = CreateTemporaryDirectory();
		try {
			TerminalDescription target = new TerminalDescriptionBuilder( "da06-target" )
				.SetDescription( "DA06 target" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
			TerminalDescription firstParent = new TerminalDescriptionBuilder( "da06-parent-a" )
				.SetDescription( "DA06 parent a" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.Build();
			TerminalDescription secondParent = new TerminalDescriptionBuilder( "da06-parent-b" )
				.SetDescription( "DA06 parent b" )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
			CompiledTermInfoDatabaseWriter.Write( targetRoot, target );
			CompiledTermInfoDatabaseWriter.Write( first, firstParent );
			CompiledTermInfoDatabaseWriter.Write( second, secondParent );

			CommandResult v2 = await RunAsync(
				"--json", "--plan-use", "--all-candidates",
				"-A", targetRoot,
				"--candidate-root", first,
				"--candidate-root", second,
				target.Name
			);
			CommandResult legacy = await RunAsync(
				"--json", "--plan-use", "--all-candidates",
				"-A", targetRoot,
				"-B", first,
				target.Name
			);

			Assert.Equal( CommandExitCodes.Success, v2.Status );
			Assert.Equal( CommandExitCodes.Success, legacy.Status );
			using JsonDocument v2Document = JsonDocument.Parse( v2.Stdout );
			using JsonDocument legacyDocument = JsonDocument.Parse( legacy.Stdout );
			Assert.Equal( 2, v2Document.RootElement.GetProperty( "schemaVersion" ).GetInt32() );
			Assert.Equal( "databaseSetPlan", v2Document.RootElement.GetProperty( "documentKind" ).GetString() );
			Assert.Equal(
				new[] { System.IO.Path.GetFullPath( first ), System.IO.Path.GetFullPath( second ) },
				v2Document.RootElement.GetProperty( "data" ).GetProperty( "databases" )
					.EnumerateArray()
					.Select( element => element.GetProperty( "root" ).GetString() )
					.ToArray()
			);
			Assert.Equal( 1, legacyDocument.RootElement.GetProperty( "schemaVersion" ).GetInt32() );
			Assert.Equal( "sourcePlan", legacyDocument.RootElement.GetProperty( "documentKind" ).GetString() );
			Assert.Equal( string.Empty, v2.Stderr );
			Assert.Equal( string.Empty, legacy.Stderr );
		} finally {
			DeleteTemporaryDirectory( targetRoot );
			DeleteTemporaryDirectory( first );
			DeleteTemporaryDirectory( second );
		}
	}

	[Fact]
	public async Task CandidateRootHumanModeWritesOnlyFrozenSelectedSource() {
		string targetRoot = CreateTemporaryDirectory();
		string candidateRoot = CreateTemporaryDirectory();
		try {
			TerminalDescription target = new TerminalDescriptionBuilder( "da06-human-target" )
				.SetDescription( "DA06 human target" )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
			TerminalDescription parent = new TerminalDescriptionBuilder( "da06-human-parent" )
				.SetDescription( "DA06 human parent" )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
			CompiledTermInfoDatabaseWriter.Write( targetRoot, target );
			CompiledTermInfoDatabaseWriter.Write( candidateRoot, parent );

			CommandResult result = await RunAsync(
				"--plan-use", "--all-candidates",
				"-A", targetRoot,
				"--candidate-root", candidateRoot,
				target.Name
			);
			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Contains( "da06-human-target", result.Stdout, StringComparison.Ordinal );
			Assert.DoesNotContain( "schemaVersion", result.Stdout, StringComparison.Ordinal );
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			DeleteTemporaryDirectory( targetRoot );
			DeleteTemporaryDirectory( candidateRoot );
		}
	}

	[Fact]
	public async Task CandidateRootRejectsAmbiguousOrUnboundForms() {
		foreach ( string[] args in new[] {
			new[] { "--candidate-root", "one", "target" },
			[ "--plan-use", "--candidate-root", "one", "target", "parent" ],
			[ "--plan-use", "--all-candidates", "-B", "legacy", "--candidate-root", "one", "target" ],
		} ) {
			CommandResult result = await RunAsync( args );
			Assert.Equal( CommandExitCodes.UsageError, result.Status );
			Assert.Equal( string.Empty, result.Stdout );
			Assert.NotEqual( string.Empty, result.Stderr );
		}
	}

	private static async Task<CommandResult> RunAsync( params string[] args ) {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();
		int status = await Command.RunAsync( args, stdin, stdout, stderr );
		return new CommandResult(
			status,
			Encoding.UTF8.GetString( stdout.ToArray() ),
			Encoding.UTF8.GetString( stderr.ToArray() )
		);
	}

	private static string CreateTemporaryDirectory() {
		string path = System.IO.Path.Combine(
			System.IO.Path.GetTempPath(),
			$"icod-terminfo-infocmp-da06-{Guid.NewGuid():N}"
		);
		Directory.CreateDirectory( path );
		return path;
	}

	private static void DeleteTemporaryDirectory( string path ) {
		try {
			Directory.Delete( path, recursive: true );
		} catch ( IOException ) {
		} catch ( UnauthorizedAccessException ) {
		}
	}

	private sealed record CommandResult( int Status, string Stdout, string Stderr );
}
