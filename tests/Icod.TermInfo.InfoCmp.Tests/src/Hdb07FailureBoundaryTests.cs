/*
	Icod.TermInfo.InfoCmp.Tests
	Characterizes HDB07 command failure and output boundaries.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

using System.Text;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo.Tests.Shared;
using Xunit;

namespace Icod.TermInfo.InfoCmp.Tests;

[Collection( EnvironmentSensitiveCollection.Name )]
public sealed class Hdb07FailureBoundaryTests {
	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public async Task TrailingStorageDataIsSanitizedCommandFailure(
		bool appendOverflowPage
	) {
		string name = "hdb07-corrupt";
		byte[] database = CreateCorruptDatabase(
			name,
			appendOverflowPage
		);

		await WithStoreAsync(
			database,
			async path => {
				CommandResult result = await RunAsync(
					CancellationToken.None,
					"-A", path, name
				);

				Assert.Equal( 1, result.Status );
				Assert.Equal( string.Empty, result.Stdout );
				Assert.Equal(
					1,
					CountOccurrences( result.Stderr, "INFOCMP0003 error" )
				);
				Assert.DoesNotContain(
					"page ",
					result.Stderr,
					StringComparison.OrdinalIgnoreCase
				);
				Assert.DoesNotContain(
					"BerkeleyDbHashReader",
					result.Stderr,
					StringComparison.Ordinal
				);
				Assert.DoesNotContain(
					"InvalidDataException",
					result.Stderr,
					StringComparison.Ordinal
				);
				Assert.DoesNotContain(
					"   at ",
					result.Stderr,
					StringComparison.Ordinal
				);
			}
		);
	}

	[Fact]
	public async Task CleanExactMissRetainsInfoCmp0002() {
		await WithStoreAsync(
			CreateValidDatabase( "hdb07-present" ),
			async path => {
				CommandResult result = await RunAsync(
					CancellationToken.None,
					"-A", path, "hdb07-missing"
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
	public async Task PreCancellationRetainsStatus130WithoutOutput() {
		using CancellationTokenSource cancellation = new();
		cancellation.Cancel();

		CommandResult result = await RunAsync(
			cancellation.Token,
			"-A", "unused.db", "hdb07-canceled"
		);

		Assert.Equal( 130, result.Status );
		Assert.Equal( string.Empty, result.Stdout );
		Assert.Equal( string.Empty, result.Stderr );
	}

	[Fact]
	public async Task EveryRejectedCandidateHasStableFinalDiagnostic() {
		await WithTwoStoresAsync(
			CreateCorruptDatabase(
				"hdb07-left",
				appendOverflowPage: false
			),
			CreateCorruptDatabase(
				"hdb07-right",
				appendOverflowPage: true
			),
			async ( leftPath, rightPath ) => {
				string[] args = [
					"-d",
					"-A", leftPath,
					"-B", rightPath,
					"hdb07-left",
					"hdb07-right",
				];
				CommandResult first = await RunAsync(
					CancellationToken.None,
					args
				);
				CommandResult second = await RunAsync(
					CancellationToken.None,
					args
				);

				Assert.Equal( CommandExitCodes.Failure, first.Status );
				Assert.Equal( string.Empty, first.Stdout );
				Assert.Contains(
					"INFOCMP0003 error",
					first.Stderr,
					StringComparison.Ordinal
				);
				Assert.Equal( first.Stderr, second.Stderr );
			}
		);
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public async Task AcceptedTextAndJsonOutputsMatchAcceptedFixture(
		bool json
	) {
		string name = "hdb07-success";
		await WithTwoStoresAsync(
			CreateValidDatabase( name ),
			BerkeleyDbHashV9TestStore.CreateCatalogStore(
				name,
				"HDB07 terminal"
			),
			async ( hdb07Path, acceptedPath ) => {
				string[] hdb07Args = ( json )
					? [ "--json", "-A", hdb07Path, name ]
					: [ "-A", hdb07Path, name ]
				;
				string[] acceptedArgs = ( json )
					? [ "--json", "-A", acceptedPath, name ]
					: [ "-A", acceptedPath, name ]
				;
				CommandResult hdb07 = await RunAsync(
					CancellationToken.None,
					hdb07Args
				);
				CommandResult accepted = await RunAsync(
					CancellationToken.None,
					acceptedArgs
				);

				Assert.Equal( CommandExitCodes.Success, hdb07.Status );
				Assert.Equal( accepted.Status, hdb07.Status );
				Assert.Equal( accepted.Stdout, hdb07.Stdout );
				Assert.Equal( accepted.Stderr, hdb07.Stderr );
			}
		);
	}

	private static byte[] CreateValidDatabase( string name ) {
		byte[] compiled = Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
			name,
			"HDB07 terminal"
		);
		return Hdb07HashV9FixtureBuilder.CreateDatabase(
			Hdb07ByteOrder.LittleEndian,
			512,
			new Hdb07RecordSpec(
				Hdb07ItemSpec.Inline( Encoding.UTF8.GetBytes( name ) ),
				Hdb07ItemSpec.Inline(
					Hdb07HashV9FixtureBuilder.NcursesData( compiled )
				)
			)
		);
	}

	private static byte[] CreateCorruptDatabase(
		string name,
		bool appendOverflowPage
	) {
		byte[] payload = Hdb07HashV9FixtureBuilder.NcursesData(
			Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
				name,
				"HDB07 corrupt terminal"
			)
		);
		return Hdb07HashV9FixtureBuilder.CreateDatabase(
			Hdb07ByteOrder.LittleEndian,
			512,
			new Hdb07RecordSpec(
				Hdb07ItemSpec.Inline( Encoding.UTF8.GetBytes( name ) ),
				Hdb07ItemSpec.OffPage(
					payload,
					[ payload.Length ],
					headerTrailingByteCount: appendOverflowPage ? 0 : 1,
					appendEmptyOverflowPage: appendOverflowPage
				)
			)
		);
	}

	private static int CountOccurrences(
		string value,
		string needle
	) {
		int count = 0;
		int offset = 0;
		while (
			( offset = value.IndexOf(
				needle,
				offset,
				StringComparison.Ordinal
			) ) >= 0
		) {
			count++;
			offset += needle.Length;
		}
		return count;
	}

	private static async Task WithStoreAsync(
		byte[] store,
		Func<string, Task> assertion
	) {
		string path = System.IO.Path.GetTempFileName();
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
		string firstPath = System.IO.Path.GetTempFileName();
		string secondPath = System.IO.Path.GetTempFileName();
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
		CancellationToken cancellationToken,
		params string[] args
	) {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();
		int status = await Command.RunAsync(
			args,
			stdin,
			stdout,
			stderr,
			cancellationToken
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
