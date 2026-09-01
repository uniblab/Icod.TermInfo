using System.Text;
using Icod.TermInfo;
using Icod.TermInfo.Compiler;
using Xunit;

namespace Icod.TermInfo.Router.Tests;

public sealed class RS06RouterSynthesisTests {
	[Fact]
	public async Task RouterPreservesInfocmpRelativeSynthesisBehavior() {
		string targetRoot = CreateTemporaryDirectory();
		string parentRoot = CreateTemporaryDirectory();
		try {
			TerminalDescription parent =
				new TerminalDescriptionBuilder( "rs06-router-parent" )
					.SetDescription( "RS06 router parent" )
					.SetNumber( NumericCapability.Columns, 80 )
					.Build();
			TerminalDescription target =
				new TerminalDescriptionBuilder( "rs06-router-child" )
					.SetDescription( "RS06 router child" )
					.SetNumber( NumericCapability.Columns, 132 )
					.Build();
			CompiledTermInfoDatabaseWriter.Write( parentRoot, parent );
			CompiledTermInfoDatabaseWriter.Write( targetRoot, target );
			string[] arguments = [
				"-A",
				targetRoot,
				"-B",
				parentRoot,
				"-u",
				target.Name,
				parent.Name,
			];

			CommandResult direct = await RunDirectAsync( arguments );
			CommandResult routed = await RunRoutedAsync(
				[ "infocmp", .. arguments ]
			);

			Assert.Equal( 0, direct.Status );
			Assert.Equal( direct.Status, routed.Status );
			Assert.Equal( direct.Stdout, routed.Stdout );
			Assert.Equal( direct.Stderr, routed.Stderr );
			Assert.Contains(
				"use=rs06-router-parent",
				routed.Stdout,
				StringComparison.Ordinal
			);
		} finally {
			DeleteTemporaryDirectory( targetRoot );
			DeleteTemporaryDirectory( parentRoot );
		}
	}

	private static async Task<CommandResult> RunDirectAsync(
		string[] arguments
	) {
		ArgumentNullException.ThrowIfNull( arguments );

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
		ArgumentNullException.ThrowIfNull( arguments );

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
		ArgumentNullException.ThrowIfNull( stdout );
		ArgumentNullException.ThrowIfNull( stderr );

		return new CommandResult(
			status,
			Encoding.UTF8.GetString( stdout.ToArray() ),
			Encoding.UTF8.GetString( stderr.ToArray() )
		);
	}

	private static string CreateTemporaryDirectory() {
		string path = System.IO.Path.Combine(
			System.IO.Path.GetTempPath(),
			$"icod-terminfo-router-rs06-{Guid.NewGuid():N}"
		);
		Directory.CreateDirectory( path );
		return path;
	}

	private static void DeleteTemporaryDirectory(
		string path
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( path );

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
