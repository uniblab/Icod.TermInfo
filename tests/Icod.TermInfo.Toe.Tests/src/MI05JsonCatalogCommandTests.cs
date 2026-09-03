using System.Text;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo.Compiler;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Toe.Tests;

public sealed class MI05JsonCatalogCommandTests {
	[Fact]
	public async Task JsonExactlyRendersTheExplicitCatalogWithOneLf() {
		string root = CreateTemporaryDirectory();
		try {
			CompiledTermInfoDatabaseWriter.Write(
				root,
				new TerminalDescriptionBuilder( "mi05-toe" )
					.SetDescription( "MI05 toe entry" )
					.SetNumber( NumericCapability.Columns, 80 )
					.Build()
			);
			TermInfoDatabaseCatalog catalog =
				TermInfoDatabaseInspector.InspectDirectory( root );

			CommandResult result = await RunAsync( "--json", root );

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Equal(
				TermInfoJsonRenderer.Render( catalog ) + "\n",
				result.Stdout
			);
			Assert.Equal( string.Empty, result.Stderr );
			Assert.EndsWith(
				"\n",
				result.Stdout,
				StringComparison.Ordinal
			);
			Assert.False(
				result.Stdout.EndsWith( "\n\n", StringComparison.Ordinal )
			);
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task JsonPreservesMissingCatalogEvidenceAsData() {
		string parent = CreateTemporaryDirectory();
		try {
			string missing = System.IO.Path.Combine( parent, "missing" );
			TermInfoDatabaseCatalog catalog =
				TermInfoDatabaseInspector.InspectDirectory( missing );

			CommandResult result = await RunAsync( "--json", missing );

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Equal(
				TermInfoJsonRenderer.Render( catalog ) + "\n",
				result.Stdout
			);
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			DeleteTemporaryDirectory( parent );
		}
	}

	[Fact]
	public async Task JsonRequiresOneUnmodifiedExplicitDirectory() {
		foreach ( string[] args in new[] {
			new[] { "--json" },
			[ "--json", "first", "second" ],
			[ "--json", "-a", "catalog" ],
			[ "--json", "-h", "catalog" ],
			[ "--json", "-s", "catalog" ],
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
			[ "--json", "catalog" ],
			stdin,
			stdout,
			stderr,
			cancellation.Token
		);

		Assert.Equal( CommandExitCodes.Canceled, status );
		Assert.Empty( stdout.ToArray() );
		Assert.Empty( stderr.ToArray() );
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
			$"icod-terminfo-toe-mi05-{Guid.NewGuid():N}"
		);
		Directory.CreateDirectory( path );
		return path;
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
