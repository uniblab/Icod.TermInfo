/*
	Icod.TermInfo.Toe.Tests
	Characterizes HDB07 root isolation and ordering after database failures.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

using System.Globalization;
using System.Text;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo.Tests.Shared;
using Xunit;

namespace Icod.TermInfo.Toe.Tests;

[Collection( "ToeEnvironmentSensitive" )]
public sealed class Hdb07FailureBoundaryTests {
	[Theory]
	[InlineData( false, false, "en-US" )]
	[InlineData( true, false, "tr-TR" )]
	[InlineData( false, true, "ar-SA" )]
	public async Task CorruptRootIsIsolatedWithoutReorderingValidRoots(
		bool headings,
		bool summary,
		string cultureName
	) {
		using TemporaryRoot temporary = new();
		string firstPath = temporary.Write(
			"first.db",
			CreateCatalogDatabase(
				"hdb07-first",
				"HDB07 first",
				corrupt: false
			)
		);
		string corruptPath = temporary.Write(
			"corrupt.db",
			CreateCatalogDatabase(
				"hdb07-corrupt",
				"HDB07 corrupt",
				corrupt: true
			)
		);
		string laterPath = temporary.Write(
			"later.db",
			CreateCatalogDatabase(
				"hdb07-later",
				"HDB07 later",
				corrupt: false
			)
		);
		var args = new List<string>();
		if ( headings ) {
			args.Add( "-h" );
		}
		if ( summary ) {
			args.Add( "-s" );
		}
		args.AddRange( [ firstPath, corruptPath, laterPath ] );
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

		try {
			CultureInfo selected = CultureInfo.GetCultureInfo( cultureName );
			CultureInfo.CurrentCulture = selected;
			CultureInfo.CurrentUICulture = selected;
			CommandResult result = await RunAsync( args.ToArray() );

			Assert.Equal( CommandExitCodes.Failure, result.Status );
			Assert.Contains(
				"hdb07-first\tHDB07 first",
				result.Stdout,
				StringComparison.Ordinal
			);
			Assert.Contains(
				"hdb07-later\tHDB07 later",
				result.Stdout,
				StringComparison.Ordinal
			);
			Assert.DoesNotContain(
				"hdb07-corrupt",
				result.Stdout,
				StringComparison.Ordinal
			);
			Assert.True(
				result.Stdout.IndexOf(
					"hdb07-first",
					StringComparison.Ordinal
				)
				< result.Stdout.IndexOf(
					"hdb07-later",
					StringComparison.Ordinal
				)
			);
			Assert.Equal(
				1,
				CountOccurrences( result.Stderr, "TOE0005 error" )
			);
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}
	}

	[Fact]
	public async Task JsonFileRouteRetainsAcceptedAmbientSchema() {
		using TemporaryRoot temporary = new();
		string path = temporary.Write(
			"terminfo.db",
			CreateCatalogDatabase(
				"hdb07-json",
				"HDB07 JSON",
				corrupt: false
			)
		);

		CommandResult first = await RunAsync( "--json", path );
		CommandResult second = await RunAsync( "--json", path );

		Assert.Equal( CommandExitCodes.Success, first.Status );
		Assert.Contains(
			"\"kind\":\"unsupportedStore\"",
			first.Stdout,
			StringComparison.Ordinal
		);
		Assert.DoesNotContain(
			"hdb07-json",
			first.Stdout,
			StringComparison.Ordinal
		);
		Assert.Equal( first.Stdout, second.Stdout );
		Assert.Equal( first.Stderr, second.Stderr );
	}

	private static byte[] CreateCatalogDatabase(
		string name,
		string description,
		bool corrupt
	) {
		byte[] storageKey = Encoding.UTF8.GetBytes( name + "|storage" );
		byte[] payload = Hdb07HashV9FixtureBuilder.NcursesData(
			Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
				name,
				description
			)
		);
		Hdb07ItemSpec storageValue = ( corrupt )
			? Hdb07ItemSpec.OffPage(
				payload,
				[ payload.Length ],
				headerTrailingByteCount: 1
			)
			: Hdb07ItemSpec.Inline( payload )
		;
		return Hdb07HashV9FixtureBuilder.CreateDatabase(
			Hdb07ByteOrder.LittleEndian,
			512,
			new Hdb07RecordSpec(
				Hdb07ItemSpec.Inline( Encoding.UTF8.GetBytes( name ) ),
				Hdb07ItemSpec.Inline(
					Hdb07HashV9FixtureBuilder.NcursesIndex( storageKey )
				)
			),
			new Hdb07RecordSpec(
				Hdb07ItemSpec.Inline( storageKey ),
				storageValue
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

	private sealed class TemporaryRoot : IDisposable {
		internal TemporaryRoot() {
			Root = Path.Combine(
				Path.GetTempPath(),
				$"icod-terminfo-toe-hdb07-{Guid.NewGuid():N}"
			);
			Directory.CreateDirectory( Root );
		}

		internal string Root { get; }

		internal string Write( string name, byte[] data ) {
			string path = Path.Combine( Root, name );
			File.WriteAllBytes( path, data );
			return path;
		}

		public void Dispose() {
			try {
				Directory.Delete( Root, recursive: true );
			} catch ( IOException ) {
			} catch ( UnauthorizedAccessException ) {
			}
		}
	}

	private sealed record CommandResult(
		int Status,
		string Stdout,
		string Stderr
	);
}
