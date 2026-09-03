using System.Text;
using System.Text.Json;
using Icod.CommandFramework.Diagnostics;
using Xunit;

namespace Icod.TermInfo.Toe.Tests;

public sealed class DA06DatabaseAutomationCommandTests {
	[Fact]
	public async Task OneRootJsonRemainsVersionOneAndMultipleRootsUseVersionTwo() {
		string first = CreateTemporaryDirectory();
		string second = CreateTemporaryDirectory();
		try {
			CommandResult single = await RunAsync( "--json", first );
			CommandResult multiple = await RunAsync( "--json", first, second );

			Assert.Equal( CommandExitCodes.Success, single.Status );
			Assert.Equal( CommandExitCodes.Success, multiple.Status );
			using JsonDocument v1 = JsonDocument.Parse( single.Stdout );
			using JsonDocument v2 = JsonDocument.Parse( multiple.Stdout );
			Assert.Equal( 1, v1.RootElement.GetProperty( "schemaVersion" ).GetInt32() );
			Assert.Equal( "databaseCatalog", v1.RootElement.GetProperty( "documentKind" ).GetString() );
			Assert.Equal( 2, v2.RootElement.GetProperty( "schemaVersion" ).GetInt32() );
			Assert.Equal( "databaseSet", v2.RootElement.GetProperty( "documentKind" ).GetString() );
			Assert.Equal( new[] { System.IO.Path.GetFullPath( first ), System.IO.Path.GetFullPath( second ) },
				v2.RootElement.GetProperty( "data" ).GetProperty( "databases" ).EnumerateArray()
					.Select( element => element.GetProperty( "root" ).GetString() )
					.ToArray() );
			Assert.EndsWith( "\n", single.Stdout, StringComparison.Ordinal );
			Assert.False( single.Stdout.EndsWith( "\n\n", StringComparison.Ordinal ) );
			Assert.EndsWith( "\n", multiple.Stdout, StringComparison.Ordinal );
			Assert.False( multiple.Stdout.EndsWith( "\n\n", StringComparison.Ordinal ) );
			Assert.Equal( string.Empty, single.Stderr );
			Assert.Equal( string.Empty, multiple.Stderr );
		} finally {
			DeleteTemporaryDirectory( first );
			DeleteTemporaryDirectory( second );
		}
	}

	[Fact]
	public async Task ExplicitSetComparisonEmitsVersionTwoComparison() {
		string left = CreateTemporaryDirectory();
		string right = CreateTemporaryDirectory();
		try {
			CommandResult result = await RunAsync(
				"--json",
				"--compare-set",
				"--left-root",
				left,
				"--right-root",
				right
			);
			Assert.Equal( CommandExitCodes.Success, result.Status );
			using JsonDocument document = JsonDocument.Parse( result.Stdout );
			Assert.Equal( 2, document.RootElement.GetProperty( "schemaVersion" ).GetInt32() );
			Assert.Equal( "databaseSetComparison", document.RootElement.GetProperty( "documentKind" ).GetString() );
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			DeleteTemporaryDirectory( left );
			DeleteTemporaryDirectory( right );
		}
	}

	[Theory]
	[InlineData( "--compare-set", "--left-root", "left", "--right-root", "right" )]
	[InlineData( "--json", "--compare-set", "--left-root", "left" )]
	public async Task InvalidSetComparisonFormsAreUsageErrors( params string[] args ) {
		CommandResult result = await RunAsync( args );
		Assert.Equal( CommandExitCodes.UsageError, result.Status );
		Assert.Equal( string.Empty, result.Stdout );
		Assert.NotEqual( string.Empty, result.Stderr );
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
			$"icod-terminfo-toe-da06-{Guid.NewGuid():N}"
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
