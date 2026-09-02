using System.Text;
using Icod.TermInfo;
using Icod.TermInfo.Compiler;
using Xunit;

namespace Icod.TermInfo.Router.Tests;

public sealed class RP06RouterPlanningTests {
	[Fact]
	public async Task RoutedPlanningExactlyMatchesDirectInfocmp() {
		string root = CreateTemporaryDirectory();
		try {
			TerminalDescription decoy =
				new TerminalDescriptionBuilder( "rp06-router-decoy" )
					.SetDescription( "RP06 router decoy" )
					.SetNumber( NumericCapability.Lines, 12 )
					.Build();
			TerminalDescription useful =
				new TerminalDescriptionBuilder( "rp06-router-useful" )
					.AddAlias( "rp06-router-parent" )
					.SetDescription( "RP06 router useful candidate" )
					.SetBoolean( BooleanCapability.AutoRightMargin )
					.SetNumber( NumericCapability.Columns, 80 )
					.Build();
			TerminalDescription target =
				new TerminalDescriptionBuilder( "rp06-router-target" )
					.SetDescription( "RP06 router target" )
					.SetBoolean( BooleanCapability.AutoRightMargin )
					.SetNumber( NumericCapability.Columns, 80 )
					.SetString( StringCapability.ClearScreen, "\u001b[H\u001b[2J" )
					.Build();
			Publish( root, decoy );
			Publish( root, useful );
			Publish( root, target );

			string[] arguments = [
				"-A",
				root,
				"-B",
				root,
				"--max-parents",
				"1",
				"--require-exhaustive",
				"--plan-use",
				target.Name,
				decoy.Name,
				"rp06-router-parent",
			];
			CommandResult direct = await RunDirectAsync( arguments );
			CommandResult routed = await RunRoutedAsync(
				[ "infocmp", .. arguments ]
			);

			Assert.Equal( 0, direct.Status );
			Assert.Equal( direct, routed );
			Assert.Contains(
				"use=rp06-router-parent",
				routed.Stdout,
				StringComparison.Ordinal
			);
			Assert.DoesNotContain(
				"use=rp06-router-decoy",
				routed.Stdout,
				StringComparison.Ordinal
			);
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	private static async Task<CommandResult> RunDirectAsync(
		string[] arguments
	) {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();
		int status = await Icod.TermInfo.InfoCmp.Command.RunAsync(
			arguments,
			stdin,
			stdout,
			stderr
		);
		return CreateResult( status, stdout, stderr );
	}

	private static async Task<CommandResult> RunRoutedAsync(
		string[] arguments
	) {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();
		int status = await Command.RunAsync(
			arguments,
			stdin,
			stdout,
			stderr
		);
		return CreateResult( status, stdout, stderr );
	}

	private static CommandResult CreateResult(
		int status,
		MemoryStream stdout,
		MemoryStream stderr
	) {
		return new CommandResult(
			status,
			Encoding.UTF8.GetString( stdout.ToArray() ),
			Encoding.UTF8.GetString( stderr.ToArray() )
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

	private static string CreateTemporaryDirectory() {
		string path = System.IO.Path.Combine(
			System.IO.Path.GetTempPath(),
			$"icod-terminfo-router-rp06-{Guid.NewGuid():N}"
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

	private sealed record CommandResult(
		int Status,
		string Stdout,
		string Stderr
	);
}
