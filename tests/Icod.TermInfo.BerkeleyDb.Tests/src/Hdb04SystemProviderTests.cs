/*
	Icod.TermInfo.BerkeleyDb.Tests
	Validates HDB04 hashed-aware system discovery.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

using System.Buffers.Binary;
using System.Text;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hdb04SystemProviderTests {
	[Fact]
	public void PublicSurfaceImplementsRuntimeProviderContract() {
		Assert.True(
			typeof( ITerminalDescriptionProvider ).IsAssignableFrom(
				typeof( BerkeleyDbSystemTerminalDescriptionProvider )
			)
		);

		BerkeleyDbSystemTerminalDescriptionProvider provider = new();
		Assert.NotNull( provider );
	}

	[Fact]
	public void OptionsSnapshotPolicyParserAndReaderLimits() {
		CompiledTermInfoParserOptions parserOptions =
			new( maximumEntrySize: 2048 );
		BerkeleyDbSystemTerminalDescriptionProviderOptions options =
			new(
				useEnvironment: false,
				useUserDatabase: true,
				useSystemDatabases: false,
				parserOptions,
				maximumDatabaseSize: 4096,
				maximumIndexHops: 7
			);

		Assert.False( options.UseEnvironment );
		Assert.True( options.UseUserDatabase );
		Assert.False( options.UseSystemDatabases );
		Assert.NotSame( parserOptions, options.ParserOptions );
		Assert.Equal( 2048, options.ParserOptions.MaximumEntrySize );
		Assert.Equal( 4096, options.MaximumDatabaseSize );
		Assert.Equal( 7, options.MaximumIndexHops );
	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( -1 )]
	public void OptionsRejectNonpositiveDatabaseLimit( int value ) {
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new BerkeleyDbSystemTerminalDescriptionProviderOptions(
				maximumDatabaseSize: value
			)
		);
	}

	[Theory]
	[InlineData( -1 )]
	[InlineData( 1025 )]
	public void OptionsRejectUnsupportedHopLimit( int value ) {
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new BerkeleyDbSystemTerminalDescriptionProviderOptions(
				maximumIndexHops: value
			)
		);
	}

	[Fact]
	public void EncodedTermInfoPrecedesOtherSources() {
		using TemporaryDirectory temporary = new();
		string name = "hdb04-encoded";
		string search = temporary.CreateSubdirectory( "search" );
		WriteDirectoryStore( search, name, "directory" );

		SystemTerminalDiscoverySnapshot snapshot =
			CreateSnapshot(
				temporary.Root,
				termInfo:
					"hex:"
					+ Convert.ToHexString(
						CreateCompiledEntry(
							name,
							"encoded"
						)
					),
				termInfoDirs: search
			);
		BerkeleyDbSystemTerminalDescriptionProvider provider =
			CreateProvider(
				snapshot,
				Array.Empty<string>()
			);

		AssertDescription(
			"encoded",
			Load(
				provider,
				name
			)
		);
	}

	[Fact]
	public void ExplicitTermInfoHashedFileIsRecognized() {
		using TemporaryDirectory temporary = new();
		string name = "hdb04-explicit";
		string databasePath =
			Path.Combine(
				temporary.Root,
				"explicit.db"
			);
		WriteHashedStore(
			databasePath,
			name,
			"explicit hashed"
		);

		BerkeleyDbSystemTerminalDescriptionProvider provider =
			CreateProvider(
				CreateSnapshot(
					temporary.Root,
					termInfo: databasePath
				),
				Array.Empty<string>()
			);

		AssertDescription(
			"explicit hashed",
			Load(
				provider,
				name
			)
		);
	}

	[Fact]
	public void ExactDirectoryPrecedesNcursesDbCompanion() {
		using TemporaryDirectory temporary = new();
		string name = "hdb04-shape";
		string logicalPath =
			temporary.CreateSubdirectory(
				"shape"
			);
		WriteDirectoryStore(
			logicalPath,
			name,
			"directory"
		);
		WriteHashedStore(
			logicalPath + ".db",
			name,
			"hashed companion"
		);

		BerkeleyDbSystemTerminalDescriptionProvider provider =
			CreateProvider(
				CreateSnapshot(
					temporary.Root,
					termInfo: logicalPath
				),
				Array.Empty<string>()
			);

		AssertDescription(
			"directory",
			Load(
				provider,
				name
			)
		);
	}

	[Fact]
	public void MissingExplicitSourceFallsThroughToUserHashedCompanion() {
		using TemporaryDirectory temporary = new();
		string name = "hdb04-user";
		string home =
			temporary.CreateSubdirectory(
				"home"
			);
		WriteHashedStore(
			Path.Combine(
				home,
				".terminfo.db"
			),
			name,
			"user hashed"
		);

		BerkeleyDbSystemTerminalDescriptionProvider provider =
			CreateProvider(
				CreateSnapshot(
					temporary.Root,
					termInfo: "missing",
					homeDirectory: home
				),
				Array.Empty<string>()
			);

		AssertDescription(
			"user hashed",
			Load(
				provider,
				name
			)
		);
	}

	[Fact]
	public void TermInfoDirsPreservesOrderAcrossStorageShapes() {
		using TemporaryDirectory temporary = new();
		string name = "hdb04-list";
		string first =
			Path.Combine(
				temporary.Root,
				"first"
			);
		string second =
			temporary.CreateSubdirectory(
				"second"
			);
		WriteHashedStore(
			first + ".db",
			name,
			"first hashed"
		);
		WriteDirectoryStore(
			second,
			name,
			"second directory"
		);

		BerkeleyDbSystemTerminalDescriptionProvider provider =
			CreateProvider(
				CreateSnapshot(
					temporary.Root,
					termInfoDirs: "first:second"
				),
				Array.Empty<string>()
			);

		AssertDescription(
			"first hashed",
			Load(
				provider,
				name
			)
		);
	}

	[Fact]
	public void EmptyTermInfoDirsComponentExpandsHashedDefaultInPlace() {
		using TemporaryDirectory temporary = new();
		string name = "hdb04-default";
		string defaultPath =
			Path.Combine(
				temporary.Root,
				"default"
			);
		string after =
			temporary.CreateSubdirectory(
				"after"
			);
		WriteHashedStore(
			defaultPath + ".db",
			name,
			"default hashed"
		);
		WriteDirectoryStore(
			after,
			name,
			"after directory"
		);

		BerkeleyDbSystemTerminalDescriptionProvider provider =
			CreateProvider(
				CreateSnapshot(
					temporary.Root,
					termInfoDirs: "missing::after"
				),
				new[] {
					defaultPath,
				}
			);

		AssertDescription(
			"default hashed",
			Load(
				provider,
				name
			)
		);
	}

	[Fact]
	public void MalformedReachedHashedSourceIsNotHiddenByFallback() {
		using TemporaryDirectory temporary = new();
		string name = "hdb04-malformed";
		string databasePath =
			Path.Combine(
				temporary.Root,
				"malformed.db"
			);
		string home =
			temporary.CreateSubdirectory(
				"home"
			);
		File.WriteAllText(
			databasePath,
			"not a Berkeley DB"
		);
		WriteDirectoryStore(
			Path.Combine(
				home,
				".terminfo"
			),
			name,
			"fallback"
		);

		BerkeleyDbSystemTerminalDescriptionProvider provider =
			CreateProvider(
				CreateSnapshot(
					temporary.Root,
					termInfo: databasePath,
					homeDirectory: home
				),
				Array.Empty<string>()
			);

		Assert.Throws<BerkeleyDbDatabaseFormatException>(
			() => provider.TryLoad(
				name,
				out _
			)
		);
	}

	[Fact]
	public void DisablingEnvironmentLeavesUserDiscoveryAvailable() {
		using TemporaryDirectory temporary = new();
		string name = "hdb04-policy";
		string environmentPath =
			Path.Combine(
				temporary.Root,
				"environment.db"
			);
		string home =
			temporary.CreateSubdirectory(
				"home"
			);
		WriteHashedStore(
			environmentPath,
			name,
			"environment"
		);
		WriteDirectoryStore(
			Path.Combine(
				home,
				".terminfo"
			),
			name,
			"user"
		);

		BerkeleyDbSystemTerminalDescriptionProviderOptions options =
			new(
				useEnvironment: false,
				useUserDatabase: true,
				useSystemDatabases: false
			);
		BerkeleyDbSystemTerminalDescriptionProvider provider =
			CreateProvider(
				CreateSnapshot(
					temporary.Root,
					termInfo: environmentPath,
					homeDirectory: home
				),
				Array.Empty<string>(),
				options
			);

		AssertDescription(
			"user",
			Load(
				provider,
				name
			)
		);
	}

	[Fact]
	public void CleanMissRemainsRetryableWhenCompanionAppears() {
		using TemporaryDirectory temporary = new();
		string name = "hdb04-retry";
		string logicalPath =
			Path.Combine(
				temporary.Root,
				"later"
			);
		BerkeleyDbSystemTerminalDescriptionProvider provider =
			CreateProvider(
				CreateSnapshot(
					temporary.Root,
					termInfo: logicalPath
				),
				Array.Empty<string>()
			);

		Assert.False(
			provider.TryLoad(
				name,
				out TerminalDescription? missing
			)
		);
		Assert.Null( missing );

		WriteHashedStore(
			logicalPath + ".db",
			name,
			"appeared"
		);
		AssertDescription(
			"appeared",
			Load(
				provider,
				name
			)
		);
	}

	[Fact]
	public void SuccessfulResultIsCachedBySystemProvider() {
		using TemporaryDirectory temporary = new();
		string name = "hdb04-cache";
		string databasePath =
			Path.Combine(
				temporary.Root,
				"cache.db"
			);
		WriteHashedStore(
			databasePath,
			name,
			"first"
		);
		BerkeleyDbSystemTerminalDescriptionProvider provider =
			CreateProvider(
				CreateSnapshot(
					temporary.Root,
					termInfo: databasePath
				),
				Array.Empty<string>()
			);

		TerminalDescription first =
			Load(
				provider,
				name
			);
		WriteHashedStore(
			databasePath,
			name,
			"changed"
		);
		TerminalDescription second =
			Load(
				provider,
				name
			);

		Assert.Same(
			first,
			second
		);
		AssertDescription(
			"first",
			second
		);
	}

	[Fact]
	public async Task ConcurrentSuccessfulLoadsPublishOneDescription() {
		using TemporaryDirectory temporary = new();
		string name = "hdb04-concurrent";
		string databasePath =
			Path.Combine(
				temporary.Root,
				"concurrent.db"
			);
		WriteHashedStore(
			databasePath,
			name,
			"concurrent"
		);
		BerkeleyDbSystemTerminalDescriptionProvider provider =
			CreateProvider(
				CreateSnapshot(
					temporary.Root,
					termInfo: databasePath
				),
				Array.Empty<string>()
			);

		Task<TerminalDescription>[] loads =
			Enumerable.Range(
				0,
				16
			)
			.Select(
				_ => Task.Run(
					() => Load(
						provider,
						name
					)
				)
			)
			.ToArray();
		TerminalDescription[] terminals =
			await Task.WhenAll( loads );

		Assert.All(
			terminals,
			terminal => Assert.Same(
				terminals[0],
				terminal
			)
		);
	}

	[Fact]
	public void MalformedFailureRemainsRetryableAfterReplacement() {
		using TemporaryDirectory temporary = new();
		string name = "hdb04-failure-retry";
		string databasePath =
			Path.Combine(
				temporary.Root,
				"failure.db"
			);
		File.WriteAllText(
			databasePath,
			"not a Berkeley DB"
		);
		BerkeleyDbSystemTerminalDescriptionProvider provider =
			CreateProvider(
				CreateSnapshot(
					temporary.Root,
					termInfo: databasePath
				),
				Array.Empty<string>()
			);

		Assert.Throws<BerkeleyDbDatabaseFormatException>(
			() => provider.TryLoad(
				name,
				out _
			)
		);

		WriteHashedStore(
			databasePath,
			name,
			"recovered"
		);
		AssertDescription(
			"recovered",
			Load(
				provider,
				name
			)
		);
	}

	[Fact]
	public void RuntimePolicyDeduplicatesEquivalentLogicalLocations() {
		using TemporaryDirectory temporary = new();
		SystemTerminalDescriptionProviderOptions options =
			new(
				useEnvironment: true,
				useUserDatabase: false,
				useSystemDatabases: true
			);
		SystemTerminalDiscoverySnapshot snapshot =
			CreateSnapshot(
				temporary.Root,
				termInfo: "same",
				termInfoDirs: "same:./same:"
			);
		string defaultRoot =
			Path.Combine(
				temporary.Root,
				"same"
			);

		IReadOnlyList<SystemTerminalDatabaseLocation> locations =
			SystemTerminalDescriptionProvider.GetDatabaseLocations(
				options,
				snapshot,
				new[] {
					defaultRoot,
				}
			);
		SystemTerminalDatabaseLocation location =
			Assert.Single( locations );

		Assert.Equal(
			SystemTerminalDatabaseLocationKind.TermInfoDirectory,
			location.Kind
		);
		Assert.Equal(
			defaultRoot,
			location.Path
		);
	}

	private static BerkeleyDbSystemTerminalDescriptionProvider CreateProvider(
		SystemTerminalDiscoverySnapshot snapshot,
		IReadOnlyList<string> defaultRoots,
		BerkeleyDbSystemTerminalDescriptionProviderOptions? options = null
	) {
		return new BerkeleyDbSystemTerminalDescriptionProvider(
			options ?? new BerkeleyDbSystemTerminalDescriptionProviderOptions(),
			snapshot,
			defaultRoots
		);
	}

	private static SystemTerminalDiscoverySnapshot CreateSnapshot(
		string currentDirectory,
		string? termInfo = null,
		string? termInfoDirs = null,
		string? homeDirectory = null
	) {
		return new SystemTerminalDiscoverySnapshot(
			termInfo,
			termInfoDirs,
			homeDirectory,
			currentDirectory,
			TerminalHostPlatform.Linux
		);
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
		return terminal;
	}

	private static void AssertDescription(
		string expected,
		TerminalDescription terminal
	) {
		Assert.Equal(
			expected,
			terminal.Description
		);
	}

	private static void WriteDirectoryStore(
		string root,
		string name,
		string description
	) {
		string directory =
			Path.Combine(
				root,
				name[0].ToString()
			);
		Directory.CreateDirectory( directory );
		File.WriteAllBytes(
			Path.Combine(
				directory,
				name
			),
			CreateCompiledEntry(
				name,
				description
			)
		);
	}

	private static void WriteHashedStore(
		string path,
		string name,
		string description
	) {
		File.WriteAllBytes(
			path,
			CreateDatabase(
				(
					Encoding.UTF8.GetBytes( name ),
					PrependMarker(
						CreateCompiledEntry(
							name,
							description
						)
					)
				)
			)
		);
	}

	private static byte[] CreateCompiledEntry(
		string name,
		string description
	) {
		byte[] names =
			Encoding.Latin1.GetBytes(
				name + "|" + description + "\0"
			);
		int length =
			12 + names.Length;
		if ( ( length & 1 ) != 0 ) {
			length++;
		}
		byte[] entry =
			new byte[length];
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan(
				0,
				2
			),
			0x011A
		);
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan(
				2,
				2
			),
			checked(
				(ushort)names.Length
			)
		);
		names.CopyTo(
			entry.AsSpan( 12 )
		);
		return entry;
	}

	private static byte[] PrependMarker( byte[] entry ) {
		byte[] value =
			new byte[entry.Length + 1];
		entry.CopyTo(
			value.AsSpan( 1 )
		);
		return value;
	}

	private static byte[] CreateDatabase(
		params ( byte[] Key, byte[] Value )[] records
	) {
		byte[] database =
			new byte[
				512
				* ( records.Length + 1 )
			];
		BinaryPrimitives.WriteUInt32LittleEndian(
			database.AsSpan(
				12,
				4
			),
			0x00061561
		);
		BinaryPrimitives.WriteUInt32LittleEndian(
			database.AsSpan(
				16,
				4
			),
			9
		);
		BinaryPrimitives.WriteUInt32LittleEndian(
			database.AsSpan(
				20,
				4
			),
			512
		);
		database[25] = 8;
		BinaryPrimitives.WriteUInt32LittleEndian(
			database.AsSpan(
				32,
				4
			),
			(uint)records.Length
		);

		for ( int index = 0; index < records.Length; index++ ) {
			( byte[] key, byte[] value ) =
				records[index];
			Span<byte> page =
				database.AsSpan(
					512
						* ( index + 1 ),
					512
				);
			BinaryPrimitives.WriteUInt32LittleEndian(
				page.Slice(
					8,
					4
				),
				(uint)( index + 1 )
			);
			page[25] = 13;
			ushort keyOffset =
				checked(
					(ushort)(
						511 - key.Length
					)
				);
			ushort valueOffset =
				checked(
					(ushort)(
						keyOffset
						- value.Length
						- 1
					)
				);
			BinaryPrimitives.WriteUInt16LittleEndian(
				page.Slice(
					20,
					2
				),
				2
			);
			BinaryPrimitives.WriteUInt16LittleEndian(
				page.Slice(
					22,
					2
				),
				valueOffset
			);
			BinaryPrimitives.WriteUInt16LittleEndian(
				page.Slice(
					26,
					2
				),
				keyOffset
			);
			BinaryPrimitives.WriteUInt16LittleEndian(
				page.Slice(
					28,
					2
				),
				valueOffset
			);
			page[keyOffset] = 1;
			key.CopyTo(
				page[( keyOffset + 1 )..]
			);
			page[valueOffset] = 1;
			value.CopyTo(
				page[( valueOffset + 1 )..]
			);
		}
		return database;
	}

	private sealed class TemporaryDirectory : IDisposable {
		internal TemporaryDirectory() {
			Root =
				Path.Combine(
					Path.GetTempPath(),
					"Icod.TermInfo.HDB04."
						+ Guid.NewGuid().ToString( "N" )
				);
			Directory.CreateDirectory( Root );
		}

		internal string Root { get; }

		internal string CreateSubdirectory( string name ) {
			string path =
				Path.Combine(
					Root,
					name
				);
			Directory.CreateDirectory( path );
			return path;
		}

		public void Dispose() {
			Directory.Delete(
				Root,
				recursive: true
			);
		}
	}
}
