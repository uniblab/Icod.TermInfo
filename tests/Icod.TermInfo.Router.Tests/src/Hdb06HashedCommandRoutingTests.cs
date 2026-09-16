/*
	Icod.TermInfo.Router.Tests
	Validates HDB06 direct/routed Hash-v9 command equivalence.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

using System.Text;
using Icod.TermInfo.Tests.Shared;
using Xunit;

namespace Icod.TermInfo.Router.Tests;

public sealed class Hdb06HashedCommandRoutingTests {
	[Theory]
	[InlineData( "hdb06-router-main" )]
	[InlineData( "hdb06-router-alias" )]
	public async Task RoutedInfocmpHashAcquisitionExactlyMatchesDirectCommand(
		string requestedName
	) {
		await WithStoreAsync(
			async path => {
				string[] arguments = [
					"-A", path, requestedName
				];
				CommandResult direct = await RunAsync(
					Icod.TermInfo.InfoCmp.Command.RunAsync,
					arguments
				);
				CommandResult routed = await RunAsync(
					Command.RunAsync,
					[ "infocmp", .. arguments ]
				);

				Assert.Equal( 0, direct.Status );
				Assert.Equal(
					"hdb06-router-main|hdb06-router-alias|HDB06 router terminal,\n",
					direct.Stdout
				);
				Assert.Equal( string.Empty, direct.Stderr );
				Assert.Equal( direct, routed );
			}
		);
	}

	[Fact]
	public async Task RoutedToeHashListingExactlyMatchesDirectCommand() {
		await WithStoreAsync(
			async path => {
				string[] arguments = [ "-s", path ];
				CommandResult direct = await RunAsync(
					Icod.TermInfo.Toe.Command.RunAsync,
					arguments
				);
				CommandResult routed = await RunAsync(
					Command.RunAsync,
					[ "toe", .. arguments ]
				);
				string expected =
					$"hdb06-router-alias\tHDB06 router terminal{Environment.NewLine}"
					+ $"hdb06-router-main\tHDB06 router terminal{Environment.NewLine}";

				Assert.Equal( 0, direct.Status );
				Assert.Equal( expected, direct.Stdout );
				Assert.Equal( string.Empty, direct.Stderr );
				Assert.Equal( direct, routed );
			}
		);
	}

	private static async Task WithStoreAsync(
		Func<string, Task> assertion
	) {
		string path = System.IO.Path.Combine(
			System.IO.Path.GetTempPath(),
			$"icod-terminfo-router-hdb06-{Guid.NewGuid():N}.db"
		);
		try {
			await File.WriteAllBytesAsync(
				path,
				BerkeleyDbHashV9TestStore.CreateCatalogStore(
					"hdb06-router-main",
					"HDB06 router terminal",
					"hdb06-router-alias"
				)
			);
			await assertion( path );
		} finally {
			File.Delete( path );
		}
	}

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

	private sealed record CommandResult(
		int Status,
		string Stdout,
		string Stderr
	);
}
