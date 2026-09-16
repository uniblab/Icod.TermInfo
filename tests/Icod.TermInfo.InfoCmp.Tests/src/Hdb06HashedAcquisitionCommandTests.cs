/*
	Icod.TermInfo.InfoCmp.Tests
	Validates HDB06 Hash-v9 command acquisition.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

using System.Text;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo.Compiler;
using Icod.TermInfo.Tests.Shared;
using Xunit;

namespace Icod.TermInfo.InfoCmp.Tests;

[Collection( EnvironmentSensitiveCollection.Name )]
public sealed class Hdb06HashedAcquisitionCommandTests {
	[Theory]
	[InlineData( "hdb06-main" )]
	[InlineData( "hdb06-alias" )]
	public async Task ExplicitHashStoreRendersCanonicalIdentity(
		string requestedName
	) {
		await WithStoreAsync(
			BerkeleyDbHashV9TestStore.CreateCatalogStore(
				"hdb06-main",
				"HDB06 terminal",
				"hdb06-alias"
			),
			async path => {
				CommandResult result = await RunAsync(
					"-A", path, requestedName
				);

				Assert.Equal( CommandExitCodes.Success, result.Status );
				Assert.Equal(
					"hdb06-main|hdb06-alias|HDB06 terminal,\n",
					result.Stdout
				);
				Assert.Equal( string.Empty, result.Stderr );
			}
		);
	}

	[Fact]
	public async Task BothExplicitRootsSupportHashComparison() {
		await WithTwoStoresAsync(
			BerkeleyDbHashV9TestStore.CreateCatalogStore(
				"hdb06-left", "HDB06 left"
			),
			BerkeleyDbHashV9TestStore.CreateCatalogStore(
				"hdb06-right", "HDB06 right"
			),
			async ( leftPath, rightPath ) => {
				CommandResult result = await RunAsync(
					"-d", "-A", leftPath, "-B", rightPath,
					"hdb06-left", "hdb06-right"
				);

				Assert.Equal( CommandExitCodes.Success, result.Status );
				Assert.Contains(
					"name: 'hdb06-left', 'hdb06-right'.",
					result.Stdout,
					StringComparison.Ordinal
				);
				Assert.Equal( string.Empty, result.Stderr );
			}
		);
	}

	[Fact]
	public async Task ConventionalAndHashRootsCanBeCompared() {
		string conventionalRoot = CreateTemporaryDirectory();
		try {
			CompiledTermInfoDatabaseWriter.Write(
				conventionalRoot,
				new TerminalDescriptionBuilder( "hdb06-conventional" )
					.SetDescription( "HDB06 conventional" )
					.Build()
			);
			await WithStoreAsync(
				BerkeleyDbHashV9TestStore.CreateCatalogStore(
					"hdb06-hash", "HDB06 hash"
				),
				async hashPath => {
					CommandResult result = await RunAsync(
						"-d", "-A", conventionalRoot, "-B", hashPath,
						"hdb06-conventional", "hdb06-hash"
					);

					Assert.Equal( CommandExitCodes.Success, result.Status );
					Assert.Contains(
						"name: 'hdb06-conventional', 'hdb06-hash'.",
						result.Stdout,
						StringComparison.Ordinal
					);
					Assert.Equal( string.Empty, result.Stderr );
				}
			);
		} finally {
			DeleteTemporaryDirectory( conventionalRoot );
		}
	}

	[Theory]
	[InlineData( "-u" )]
	[InlineData( "--plan-use" )]
	public async Task SynthesisModesAcquireFromHashRoots( string mode ) {
		await WithTwoStoresAsync(
			BerkeleyDbHashV9TestStore.CreateCatalogStore(
				"hdb06-target", "HDB06 target"
			),
			BerkeleyDbHashV9TestStore.CreateCatalogStore(
				"hdb06-parent", "HDB06 parent"
			),
			async ( targetPath, parentPath ) => {
				CommandResult result = await RunAsync(
					mode, "-A", targetPath, "-B", parentPath,
					"hdb06-target", "hdb06-parent"
				);

				Assert.Equal( CommandExitCodes.Success, result.Status );
				Assert.StartsWith(
					"hdb06-target|HDB06 target,\n",
					result.Stdout,
					StringComparison.Ordinal
				);
				Assert.Equal( string.Empty, result.Stderr );
			}
		);
	}

	[Fact]
	public async Task JsonTerminalModeAcquiresFromHashStore() {
		await WithStoreAsync(
			BerkeleyDbHashV9TestStore.CreateCatalogStore(
				"hdb06-json", "HDB06 JSON"
			),
			async path => {
				CommandResult result = await RunAsync(
					"--json", "-A", path, "hdb06-json"
				);

				Assert.Equal( CommandExitCodes.Success, result.Status );
				Assert.Contains(
					"\"name\":\"hdb06-json\"",
					result.Stdout,
					StringComparison.Ordinal
				);
				Assert.EndsWith( "\n", result.Stdout, StringComparison.Ordinal );
				Assert.Equal( string.Empty, result.Stderr );
			}
		);
	}

	[Fact]
	public async Task MissingHashPublicationIsControlledFailure() {
		await WithStoreAsync(
			BerkeleyDbHashV9TestStore.CreateCatalogStore(
				"hdb06-present", "HDB06 present"
			),
			async path => {
				CommandResult result = await RunAsync(
					"-A", path, "hdb06-missing"
				);

				Assert.Equal( CommandExitCodes.Failure, result.Status );
				Assert.Equal( string.Empty, result.Stdout );
				Assert.Contains(
					"INFOCMP0002 error",
					result.Stderr,
					StringComparison.Ordinal
				);
			}
		);
	}

	[Fact]
	public async Task MalformedHashStoreIsControlledFailure() {
		await WithStoreAsync(
			BerkeleyDbHashV9TestStore.CreateMalformedStore(),
			async path => {
				CommandResult result = await RunAsync(
					"-A", path, "hdb06-bad"
				);

				Assert.Equal( CommandExitCodes.Failure, result.Status );
				Assert.Equal( string.Empty, result.Stdout );
				Assert.Contains(
					"INFOCMP0003 error",
					result.Stderr,
					StringComparison.Ordinal
				);
			}
		);
	}

	[Fact]
	public async Task AllCandidatesRejectsHashStoreWithoutPartialOutput() {
		await WithStoreAsync(
			BerkeleyDbHashV9TestStore.CreateCatalogStore(
				"hdb06-catalog", "HDB06 catalog"
			),
			async path => {
				CommandResult result = await RunAsync(
					"--plan-use", "--all-candidates",
					"-A", path, "-B", path, "hdb06-catalog"
				);

				Assert.Equal( CommandExitCodes.Failure, result.Status );
				Assert.Equal( string.Empty, result.Stdout );
				Assert.Contains(
					"INFOCMP0004 error",
					result.Stderr,
					StringComparison.Ordinal
				);
			}
		);
	}

	private static async Task WithStoreAsync(
		byte[] store,
		Func<string, Task> assertion
	) {
		string path = Path.GetTempFileName();
		try {
			await File.WriteAllBytesAsync( path, store );
			await assertion( path );
		} finally {
			File.Delete( path );
		}
	}

	private static async Task WithTwoStoresAsync(
		byte[] first,
		byte[] second,
		Func<string, string, Task> assertion
	) {
		string firstPath = Path.GetTempFileName();
		string secondPath = Path.GetTempFileName();
		try {
			await File.WriteAllBytesAsync( firstPath, first );
			await File.WriteAllBytesAsync( secondPath, second );
			await assertion( firstPath, secondPath );
		} finally {
			File.Delete( firstPath );
			File.Delete( secondPath );
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
			$"icod-terminfo-infocmp-hdb06-{Guid.NewGuid():N}"
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
