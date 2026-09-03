using System.Text;
using Icod.TermInfo.Compiler;
using Xunit;

namespace Icod.TermInfo.Router.Tests;

public sealed class MI05RouterJsonAutomationTests {
	[Fact]
	public async Task RoutedInfocmpAndToeJsonExactlyMatchDirectCommands() {
		string root = CreateTemporaryDirectory();
		try {
			TerminalDescription parent =
				new TerminalDescriptionBuilder( "mi05-router-parent" )
					.SetDescription( "MI05 router parent" )
					.SetNumber( NumericCapability.Columns, 80 )
					.Build();
			TerminalDescription target =
				new TerminalDescriptionBuilder( "mi05-router-target" )
					.SetDescription( "MI05 router target" )
					.SetNumber( NumericCapability.Columns, 80 )
					.SetBoolean( BooleanCapability.AutoRightMargin )
					.Build();
			CompiledTermInfoDatabaseWriter.Write( root, parent );
			CompiledTermInfoDatabaseWriter.Write( root, target );

			string[][] infocmpForms = [
				[
					"--json",
					"-A",
					root,
					target.Name,
				],
				[
					"--json",
					"-d",
					"-A",
					root,
					"-B",
					root,
					target.Name,
					parent.Name,
				],
				[
					"--json",
					"--plan-use",
					"-A",
					root,
					"-B",
					root,
					target.Name,
					parent.Name,
				],
				[
					"--json",
					"--plan-use",
					"--all-candidates",
					"-A",
					root,
					"-B",
					root,
					target.Name,
				],
			];
			foreach ( string[] infocmpArguments in infocmpForms ) {
				CommandResult directInfocmp = await RunDirectInfocmpAsync(
					infocmpArguments
				);
				CommandResult routedInfocmp = await RunRoutedAsync(
					[ "infocmp", .. infocmpArguments ]
				);

				Assert.Equal( 0, directInfocmp.Status );
				Assert.Equal( directInfocmp, routedInfocmp );
			}
			string[] toeArguments = [ "--json", root ];
			CommandResult directToe = await RunDirectToeAsync( toeArguments );
			CommandResult routedToe = await RunRoutedAsync(
				[ "toe", .. toeArguments ]
			);

			Assert.Equal( 0, directToe.Status );
			Assert.Equal( directToe, routedToe );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	private static Task<CommandResult> RunDirectInfocmpAsync(
		string[] arguments
	) => RunAsync( Icod.TermInfo.InfoCmp.Command.RunAsync, arguments );

	private static Task<CommandResult> RunDirectToeAsync(
		string[] arguments
	) => RunAsync( Icod.TermInfo.Toe.Command.RunAsync, arguments );

	private static Task<CommandResult> RunRoutedAsync(
		string[] arguments
	) => RunAsync( Command.RunAsync, arguments );

	private static async Task<CommandResult> RunAsync(
		Func<string[], Stream, Stream, Stream, CancellationToken, Task<int>> command,
		string[] arguments
	) {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();
		int status = await command(
			arguments,
			stdin,
			stdout,
			stderr,
			CancellationToken.None
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
			$"icod-terminfo-router-mi05-{Guid.NewGuid():N}"
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
