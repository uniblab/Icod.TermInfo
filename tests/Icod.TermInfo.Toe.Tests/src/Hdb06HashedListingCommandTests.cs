/*
	Icod.TermInfo.Toe.Tests
	Validates HDB06 Hash-v9 human listing.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

using System.Text;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo.Compiler;
using Icod.TermInfo.Tests.Shared;
using Xunit;

namespace Icod.TermInfo.Toe.Tests;

public sealed class Hdb06HashedListingCommandTests {
	[Theory]
	[InlineData()]
	[InlineData( "-s" )]
	public async Task HashStoreListsLogicalPublicationsInOrdinalOrder(
		params string[] options
	) {
		await WithStoreAsync(
			BerkeleyDbHashV9TestStore.CreateCatalogStore(
				"z-canonical",
				"HDB06 terminal",
				"b-alias",
				"a-alias"
			),
			async path => {
				CommandResult result = await RunAsync( [ .. options, path ] );
				string expected =
					$"a-alias\tHDB06 terminal{Environment.NewLine}"
					+ $"b-alias\tHDB06 terminal{Environment.NewLine}"
					+ $"z-canonical\tHDB06 terminal{Environment.NewLine}";

				Assert.Equal( CommandExitCodes.Success, result.Status );
				Assert.Equal( expected, result.Stdout );
				Assert.DoesNotContain(
					"z-canonical|b-alias|a-alias|HDB06 terminal",
					result.Stdout,
					StringComparison.Ordinal
				);
				Assert.Equal( string.Empty, result.Stderr );
			}
		);
	}

	[Fact]
	public async Task HeadingUsesCanonicalAbsoluteHashPath() {
		await WithStoreAsync(
			BerkeleyDbHashV9TestStore.CreateCatalogStore(
				"hdb06-heading", "HDB06 heading"
			),
			async path => {
				CommandResult result = await RunAsync( "-h", path );

				Assert.Equal( CommandExitCodes.Success, result.Status );
				Assert.StartsWith(
					$"# {System.IO.Path.GetFullPath( path )}{Environment.NewLine}",
					result.Stdout,
					StringComparison.Ordinal
				);
				Assert.Equal( string.Empty, result.Stderr );
			}
		);
	}

	[Fact]
	public async Task MixedRootsPreserveCallerOrder() {
		string conventionalRoot = CreateTemporaryDirectory();
		try {
			CompiledTermInfoDatabaseWriter.Write(
				conventionalRoot,
				new TerminalDescriptionBuilder( "hdb06-directory" )
					.SetDescription( "HDB06 directory" )
					.Build()
			);
			await WithStoreAsync(
				BerkeleyDbHashV9TestStore.CreateCatalogStore(
					"hdb06-file", "HDB06 file"
				),
				async path => {
					CommandResult result = await RunAsync(
						conventionalRoot,
						path
					);

					Assert.Equal( CommandExitCodes.Success, result.Status );
					Assert.True(
						result.Stdout.IndexOf(
							"hdb06-directory\tHDB06 directory",
							StringComparison.Ordinal
						)
						< result.Stdout.IndexOf(
							"hdb06-file\tHDB06 file",
							StringComparison.Ordinal
						)
					);
					Assert.Equal( string.Empty, result.Stderr );
				}
			);
		} finally {
			DeleteTemporaryDirectory( conventionalRoot );
		}
	}

	[Fact]
	public async Task DuplicateAnalysisUsesLogicalPublicationName() {
		await WithTwoStoresAsync(
			BerkeleyDbHashV9TestStore.CreateCatalogStore(
				"hdb06-first", "HDB06 first", "hdb06-shared"
			),
			BerkeleyDbHashV9TestStore.CreateCatalogStore(
				"hdb06-second", "HDB06 second", "hdb06-shared"
			),
			async ( firstPath, secondPath ) => {
				CommandResult result = await RunAsync(
					"-a", "-s", firstPath, secondPath
				);

				Assert.Equal( CommandExitCodes.Success, result.Status );
				Assert.Contains(
					$"# Icod duplicate hdb06-shared: semantically different from {System.IO.Path.GetFullPath( firstPath )}",
					result.Stdout,
					StringComparison.Ordinal
				);
				Assert.Equal( string.Empty, result.Stderr );
			}
		);
	}

	[Fact]
	public async Task MalformedHashStoreDoesNotHideLaterRoot() {
		await WithTwoStoresAsync(
			BerkeleyDbHashV9TestStore.CreateMalformedStore(),
			BerkeleyDbHashV9TestStore.CreateCatalogStore(
				"hdb06-later", "HDB06 later"
			),
			async ( badPath, laterPath ) => {
				CommandResult result = await RunAsync( badPath, laterPath );

				Assert.Equal( CommandExitCodes.Failure, result.Status );
				Assert.Equal(
					$"hdb06-later\tHDB06 later{Environment.NewLine}",
					result.Stdout
				);
				Assert.Contains(
					"TOE0005 error",
					result.Stderr,
					StringComparison.Ordinal
				);
			}
		);
	}

	[Fact]
	public async Task JsonFileRouteRetainsUnsupportedStoreSchema() {
		await WithStoreAsync(
			BerkeleyDbHashV9TestStore.CreateCatalogStore(
				"hdb06-json", "HDB06 JSON"
			),
			async path => {
				CommandResult result = await RunAsync( "--json", path );

				Assert.Equal( CommandExitCodes.Success, result.Status );
				Assert.Contains(
					"\"kind\":\"unsupportedStore\"",
					result.Stdout,
					StringComparison.Ordinal
				);
				Assert.DoesNotContain(
					"hdb06-json",
					result.Stdout,
					StringComparison.Ordinal
				);
				Assert.Equal( string.Empty, result.Stderr );
			}
		);
	}

	private static async Task WithStoreAsync(
		byte[] store,
		Func<string, Task> assertion
	) {
		string root = CreateTemporaryDirectory();
		string path = System.IO.Path.Combine( root, "terminfo.db" );
		try {
			await File.WriteAllBytesAsync( path, store );
			await assertion( path );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	private static async Task WithTwoStoresAsync(
		byte[] first,
		byte[] second,
		Func<string, string, Task> assertion
	) {
		string root = CreateTemporaryDirectory();
		string firstPath = System.IO.Path.Combine( root, "first.db" );
		string secondPath = System.IO.Path.Combine( root, "second.db" );
		try {
			await File.WriteAllBytesAsync( firstPath, first );
			await File.WriteAllBytesAsync( secondPath, second );
			await assertion( firstPath, secondPath );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	private static async Task<CommandResult> RunAsync(
		params string[] args
	) {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();
		int status = await Command.RunAsync(
			args, stdin, stdout, stderr
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
			$"icod-terminfo-toe-hdb06-{Guid.NewGuid():N}"
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

	private sealed record CommandResult(
		int Status,
		string Stdout,
		string Stderr
	);
}
