/*
	Icod.TermInfo.BerkeleyDb.Tests
	Characterizes HDB07 retry, caching, replacement, concurrency, and ownership.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.Text;
using Icod.TermInfo;
using Icod.TermInfo.Tests.Shared;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hdb07LifecycleHardeningTests {
	private const int PageSize = 512;

	public static TheoryData<string> RetryableFailureCases => new() {
		"missing-database",
		"sharing-violation",
		"malformed-storage",
		"malformed-envelope",
		"parser-failure",
		"identity-mismatch",
	};

	[Theory]
	[MemberData( nameof( RetryableFailureCases ) )]
	public void ExplicitProviderRetriesEveryIncompleteFailure(
		string caseName
	) {
		using TemporaryRoot temporary = new();
		string path = temporary.GetPath( "retry.db" );
		string name = "hdb07-retry";
		PrepareFailure( path, name, caseName );
		BerkeleyDbTerminalDescriptionProvider provider = new( path );

		AssertInitialFailure( provider, path, name, caseName );

		WriteProviderDatabase( path, name, "recovered" );
		Assert.True(
			provider.TryLoad(
				name,
				out TerminalDescription? terminal
			)
		);
		Assert.Equal( "recovered", terminal!.Description );
		AssertFileCanBeOpenedExclusively( path );
	}

	[Fact]
	public void SuccessfulProviderCallIsCachedAndNewProviderSeesReplacement() {
		using TemporaryRoot temporary = new();
		string path = temporary.GetPath( "replacement.db" );
		string name = "hdb07-replacement";
		WriteProviderDatabase( path, name, "first" );
		BerkeleyDbTerminalDescriptionProvider original = new( path );

		TerminalDescription first = Load( original, name );
		WriteProviderDatabase( path, name, "second" );
		TerminalDescription cached = Load( original, name );
		TerminalDescription replacement = Load(
			new BerkeleyDbTerminalDescriptionProvider( path ),
			name
		);

		Assert.Same( first, cached );
		Assert.Equal( "first", cached.Description );
		Assert.NotSame( first, replacement );
		Assert.Equal( "second", replacement.Description );
		AssertFileCanBeOpenedExclusively( path );
	}

	[Fact]
	public void CatalogReturnsFreshSnapshotsAndHonorsPreCancellation() {
		using TemporaryRoot temporary = new();
		string path = temporary.GetPath( "catalog.db" );
		string name = "hdb07-catalog";
		File.WriteAllBytes(
			path,
			CreateCatalogDatabase( name, "first" )
		);
		BerkeleyDbTerminalCatalogReader reader = new( path );

		BerkeleyDbTerminalCatalogEntry first =
			Assert.Single( reader.Read() );
		BerkeleyDbTerminalCatalogEntry second =
			Assert.Single( reader.Read() );
		Assert.NotSame( first, second );
		Assert.NotSame( first.Terminal, second.Terminal );

		using CancellationTokenSource cancellation = new();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>(
			() => reader.Read( cancellation.Token )
		);
		AssertFileCanBeOpenedExclusively( path );
	}

	[Fact]
	public void ConcurrentExplicitProvidersKeepResourceLimitsIndependent() {
		using TemporaryRoot temporary = new();
		string path = temporary.GetPath( "limits.db" );
		string name = "hdb07-limits";
		WriteProviderDatabase( path, name, "accepted" );
		int length = checked( (int)new FileInfo( path ).Length );
		BerkeleyDbTerminalDescriptionProvider accepted =
			CreateProviderWithDatabaseLimit( path, length );
		BerkeleyDbTerminalDescriptionProvider rejected =
			CreateProviderWithDatabaseLimit( path, length - 1 );

		Task success = Task.Run(
			() => Assert.Equal(
				"accepted",
				Load( accepted, name ).Description
			)
		);
		Task failure = Task.Run(
			() => Assert.Throws<BerkeleyDbDatabaseFormatException>(
				() => rejected.TryLoad( name, out _ )
			)
		);
		Task.WaitAll( success, failure );

		AssertFileCanBeOpenedExclusively( path );
	}

	[Fact]
	public void SystemProviderPreservesPrecedenceAndSuccessfulCache() {
		using TemporaryRoot temporary = new();
		string name = "hdb07-system-precedence";
		string explicitPath = temporary.GetPath( "explicit.db" );
		string fallbackPath = temporary.GetPath( "fallback.db" );
		WriteProviderDatabase( explicitPath, name, "explicit" );
		WriteProviderDatabase( fallbackPath, name, "fallback" );
		BerkeleyDbSystemTerminalDescriptionProvider provider =
			CreateSystemProvider(
				temporary.Root,
				explicitPath,
				[ fallbackPath ]
			);

		TerminalDescription first = Load( provider, name );
		WriteProviderDatabase( explicitPath, name, "changed" );
		TerminalDescription cached = Load( provider, name );

		Assert.Equal( "explicit", first.Description );
		Assert.Same( first, cached );
		AssertFileCanBeOpenedExclusively( explicitPath );
		AssertFileCanBeOpenedExclusively( fallbackPath );
	}

	[Fact]
	public void SystemProviderRetriesCleanMissAndMalformedFailure() {
		using TemporaryRoot temporary = new();
		string name = "hdb07-system-retry";
		string path = temporary.GetPath( "system.db" );
		BerkeleyDbSystemTerminalDescriptionProvider missingProvider =
			CreateSystemProvider(
				temporary.Root,
				path,
				[]
			);

		Assert.False( missingProvider.TryLoad( name, out _ ) );
		WriteProviderDatabase( path, name, "appeared" );
		Assert.Equal(
			"appeared",
			Load( missingProvider, name ).Description
		);

		string malformedPath = temporary.GetPath( "malformed.db" );
		File.WriteAllBytes( malformedPath, [ 0x00 ] );
		BerkeleyDbSystemTerminalDescriptionProvider malformedProvider =
			CreateSystemProvider(
				temporary.Root,
				malformedPath,
				[]
			);
		Assert.Throws<BerkeleyDbDatabaseFormatException>(
			() => malformedProvider.TryLoad( name, out _ )
		);
		WriteProviderDatabase( malformedPath, name, "recovered" );
		Assert.Equal(
			"recovered",
			Load( malformedProvider, name ).Description
		);
		AssertFileCanBeOpenedExclusively( malformedPath );
	}

	private static void PrepareFailure(
		string path,
		string name,
		string caseName
	) {
		switch ( caseName ) {
			case "missing-database":
				break;
			case "sharing-violation":
				WriteProviderDatabase( path, name, "locked" );
				break;
			case "malformed-storage":
				File.WriteAllBytes( path, [ 0x00 ] );
				break;
			case "malformed-envelope":
				File.WriteAllBytes(
					path,
					CreateDatabase(
						CreateRecord(
							Encoding.UTF8.GetBytes( name ),
							[ 0x07 ]
						)
					)
				);
				break;
			case "parser-failure":
				File.WriteAllBytes(
					path,
					CreateDatabase(
						CreateRecord(
							Encoding.UTF8.GetBytes( name ),
							[ 0x00, 0x01, 0x02 ]
						)
					)
				);
				break;
			case "identity-mismatch":
				WriteProviderDatabase(
					path,
					name,
					"wrong identity",
					canonicalName: "other"
				);
				break;
			default:
				throw new InvalidOperationException(
					$"Unknown retry case '{caseName}'."
				);
		}
	}

	private static void AssertInitialFailure(
		BerkeleyDbTerminalDescriptionProvider provider,
		string path,
		string name,
		string caseName
	) {
		switch ( caseName ) {
			case "missing-database":
				Assert.Throws<FileNotFoundException>(
					() => provider.TryLoad( name, out _ )
				);
				break;
			case "sharing-violation":
				using ( FileStream locked = new FileStream(
					path,
					FileMode.Open,
					FileAccess.ReadWrite,
					FileShare.None
				) ) {
					Assert.Throws<IOException>(
						() => provider.TryLoad( name, out _ )
					);
				}
				break;
			case "malformed-storage":
			case "malformed-envelope":
				Assert.Throws<BerkeleyDbDatabaseFormatException>(
					() => provider.TryLoad( name, out _ )
				);
				break;
			case "parser-failure":
				Assert.Throws<CompiledTermInfoFormatException>(
					() => provider.TryLoad( name, out _ )
				);
				break;
			case "identity-mismatch":
				Assert.Throws<InvalidDataException>(
					() => provider.TryLoad( name, out _ )
				);
				break;
			default:
				throw new InvalidOperationException(
					$"Unknown retry case '{caseName}'."
				);
		}
	}

	private static BerkeleyDbTerminalDescriptionProvider
		CreateProviderWithDatabaseLimit(
			string path,
			int maximumDatabaseSize
		) {
		return new BerkeleyDbTerminalDescriptionProvider(
			path,
			new BerkeleyDbTerminalDescriptionProviderOptions(
				maximumDatabaseSize: maximumDatabaseSize
			)
		);
	}

	private static BerkeleyDbSystemTerminalDescriptionProvider
		CreateSystemProvider(
			string currentDirectory,
			string termInfo,
			IReadOnlyList<string> defaultRoots
		) {
		SystemTerminalDiscoverySnapshot snapshot =
			new(
				termInfo: termInfo,
				termInfoDirs: null,
				homeDirectory: null,
				currentDirectory: currentDirectory,
				platform: TerminalHostPlatform.Linux
			);
		return new BerkeleyDbSystemTerminalDescriptionProvider(
			new BerkeleyDbSystemTerminalDescriptionProviderOptions(
				useEnvironment: true,
				useUserDatabase: false,
				useSystemDatabases: true
			),
			snapshot,
			defaultRoots
		);
	}

	private static TerminalDescription Load(
		BerkeleyDbTerminalDescriptionProvider provider,
		string name
	) {
		Assert.True(
			provider.TryLoad(
				name,
				out TerminalDescription? terminal
			)
		);
		return terminal!;
	}

	private static TerminalDescription Load(
		BerkeleyDbSystemTerminalDescriptionProvider provider,
		string name
	) {
		Assert.True(
			provider.TryLoad(
				name,
				out TerminalDescription? terminal
			)
		);
		return terminal!;
	}

	private static byte[] CreateCatalogDatabase(
		string name,
		string description
	) {
		byte[] storageKey = Encoding.UTF8.GetBytes( name + "|storage" );
		return CreateDatabase(
			CreateRecord(
				Encoding.UTF8.GetBytes( name ),
				Hdb07HashV9FixtureBuilder.NcursesIndex( storageKey )
			),
			CreateRecord(
				storageKey,
				Hdb07HashV9FixtureBuilder.NcursesData(
					Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
						name,
						description
					)
				)
			)
		);
	}

	private static void WriteProviderDatabase(
		string path,
		string requestedName,
		string description,
		string? canonicalName = null
	) {
		byte[] entry = Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
			canonicalName ?? requestedName,
			description
		);
		File.WriteAllBytes(
			path,
			CreateDatabase(
				CreateRecord(
					Encoding.UTF8.GetBytes( requestedName ),
					Hdb07HashV9FixtureBuilder.NcursesData( entry )
				)
			)
		);
	}

	private static byte[] CreateDatabase(
		params Hdb07RecordSpec[] records
	) {
		return Hdb07HashV9FixtureBuilder.CreateDatabase(
			Hdb07ByteOrder.LittleEndian,
			PageSize,
			records
		);
	}

	private static Hdb07RecordSpec CreateRecord(
		byte[] key,
		byte[] value
	) {
		return new Hdb07RecordSpec(
			Hdb07ItemSpec.Inline( key ),
			Hdb07ItemSpec.Inline( value )
		);
	}

	private static void AssertFileCanBeOpenedExclusively(
		string path
	) {
		using FileStream stream = new FileStream(
			path,
			FileMode.Open,
			FileAccess.ReadWrite,
			FileShare.None
		);
		Assert.True( stream.CanRead );
		Assert.True( stream.CanWrite );
	}

	private sealed class TemporaryRoot : IDisposable {
		internal TemporaryRoot() {
			Root = Path.Combine(
				Path.GetTempPath(),
				Guid.NewGuid().ToString( "N" )
			);
			Directory.CreateDirectory( Root );
		}

		internal string Root { get; }

		internal string GetPath( string name ) =>
			Path.Combine( Root, name )
		;

		public void Dispose() {
			Directory.Delete( Root, recursive: true );
		}
	}
}
