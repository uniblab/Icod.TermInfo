using System.Buffers.Binary;
using System.Reflection;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T39ProviderCacheCompositionTests {
	private const int CompiledHeaderSize = 12;

	[Fact]
	public void TerminalDatabaseCanParticipateAsAProvider() {
		Assert.True(
			typeof( ITerminalDescriptionProvider ).IsAssignableFrom(
				typeof( TerminalDatabase ) ) );

		ITerminalDescriptionProvider provider =
			TerminalDatabase.BuiltIn;

		Assert.True(
			provider.TryLoad(
				"xterm",
				out TerminalDescription? terminal ) );
		Assert.Same(
			TerminalProfiles.Xterm,
			terminal );
	}

	[Fact]
	public void DirectorySuccessfulEntryRemainsStableForProviderLifetime() {
		using TemporaryDirectory temporary = new();
		string name =
			"t29-legacy-minimal";
		string path =
			WriteLiteralCandidate(
				temporary.Root,
				name,
				CreateEntryWithColumns( 80 ) );
		DirectoryTerminalDescriptionProvider provider =
			new(
				temporary.Root );

		TerminalDescription first =
			Load(
				provider,
				name );

		File.WriteAllBytes(
			path,
			CreateEntryWithColumns( 99 ) );

		TerminalDescription second =
			Load(
				provider,
				name );

		Assert.Same(
			first,
			second );
		AssertColumns(
			80,
			second );
	}

	[Fact]
	public async Task DirectoryConcurrentFirstLoadPublishesOneDescription() {
		using TemporaryDirectory temporary = new();
		string name =
			"t29-legacy-minimal";

		WriteLiteralCandidate(
			temporary.Root,
			name,
			CreateEntryWithColumns( 80 ) );

		DirectoryTerminalDescriptionProvider provider =
			new(
				temporary.Root );
		Task<TerminalDescription>[] tasks =
			Enumerable
				.Range(
					0,
					32 )
				.Select(
					_ => Task.Run(
						() => Load(
							provider,
							name ) ) )
				.ToArray();

		TerminalDescription[] terminals =
			await Task.WhenAll(
				tasks );

		TerminalDescription first =
			terminals[ 0 ];
		Assert.All(
			terminals,
			terminal => Assert.Same(
				first,
				terminal ) );
	}

	[Fact]
	public void DirectoryCleanMissIsNotNegativeCached() {
		using TemporaryDirectory temporary = new();
		string name =
			"t29-legacy-minimal";
		DirectoryTerminalDescriptionProvider provider =
			new(
				temporary.Root );

		Assert.False(
			provider.TryLoad(
				name,
				out TerminalDescription? missing ) );
		Assert.Null(
			missing );

		WriteLiteralCandidate(
			temporary.Root,
			name,
			CreateEntryWithColumns( 80 ) );

		AssertColumns(
			80,
			Load(
				provider,
				name ) );
	}

	[Fact]
	public void DirectoryFailureIsRetryableAfterEntryIsCorrected() {
		using TemporaryDirectory temporary = new();
		string name =
			"t29-legacy-minimal";
		string path =
			WriteLiteralCandidate(
				temporary.Root,
				name,
				ReadFixture(
					"malformed/unsupported-magic.bin" ) );
		DirectoryTerminalDescriptionProvider provider =
			new(
				temporary.Root );

		Assert.Throws<CompiledTermInfoFormatException>(
			() => provider.TryLoad(
				name,
				out _ ) );

		File.WriteAllBytes(
			path,
			CreateEntryWithColumns( 80 ) );

		AssertColumns(
			80,
			Load(
				provider,
				name ) );
	}

	[Fact]
	public void SystemSuccessfulEntryRemainsStableForProviderLifetime() {
		using TemporaryDirectory temporary = new();
		string name =
			"t29-legacy-minimal";
		string path =
			WriteLiteralCandidate(
				temporary.Root,
				name,
				CreateEntryWithColumns( 80 ) );
		SystemTerminalDescriptionProvider provider =
			CreateSystemProvider(
				temporary.Root );

		TerminalDescription first =
			Load(
				provider,
				name );

		File.WriteAllBytes(
			path,
			CreateEntryWithColumns( 99 ) );

		TerminalDescription second =
			Load(
				provider,
				name );

		Assert.Same(
			first,
			second );
		AssertColumns(
			80,
			second );
	}

	[Fact]
	public async Task SystemConcurrentFirstLoadPublishesOneDescription() {
		using TemporaryDirectory temporary = new();
		string name =
			"t29-legacy-minimal";

		WriteLiteralCandidate(
			temporary.Root,
			name,
			CreateEntryWithColumns( 80 ) );

		SystemTerminalDescriptionProvider provider =
			CreateSystemProvider(
				temporary.Root );
		Task<TerminalDescription>[] tasks =
			Enumerable
				.Range(
					0,
					32 )
				.Select(
					_ => Task.Run(
						() => Load(
							provider,
							name ) ) )
				.ToArray();

		TerminalDescription[] terminals =
			await Task.WhenAll(
				tasks );

		TerminalDescription first =
			terminals[ 0 ];
		Assert.All(
			terminals,
			terminal => Assert.Same(
				first,
				terminal ) );
	}

	[Fact]
	public void SystemCleanMissIsNotNegativeCached() {
		using TemporaryDirectory temporary = new();
		string name =
			"t29-legacy-minimal";
		SystemTerminalDescriptionProvider provider =
			CreateSystemProvider(
				temporary.Root );

		Assert.False(
			provider.TryLoad(
				name,
				out TerminalDescription? missing ) );
		Assert.Null(
			missing );

		WriteLiteralCandidate(
			temporary.Root,
			name,
			CreateEntryWithColumns( 80 ) );

		AssertColumns(
			80,
			Load(
				provider,
				name ) );
	}

	[Fact]
	public void SystemFailureIsRetryableAfterEntryIsCorrected() {
		using TemporaryDirectory temporary = new();
		string name =
			"t29-legacy-minimal";
		string path =
			WriteLiteralCandidate(
				temporary.Root,
				name,
				ReadFixture(
					"malformed/unsupported-magic.bin" ) );
		SystemTerminalDescriptionProvider provider =
			CreateSystemProvider(
				temporary.Root );

		Assert.Throws<CompiledTermInfoFormatException>(
			() => provider.TryLoad(
				name,
				out _ ) );

		File.WriteAllBytes(
			path,
			CreateEntryWithColumns( 80 ) );

		AssertColumns(
			80,
			Load(
				provider,
				name ) );
	}

	[Fact]
	public void EncodedSystemEntryIsCachedPerProvider() {
		using TemporaryDirectory temporary = new();
		string name =
			"t29-legacy-minimal";
		byte[] encoded =
			CreateEntryWithColumns( 80 );
		SystemTerminalDiscoverySnapshot snapshot =
			new(
				termInfo:
					"hex:"
					+ Convert.ToHexString(
						encoded ),
				termInfoDirs: null,
				homeDirectory: null,
				currentDirectory: temporary.Root,
				platform: TerminalHostPlatform.Linux );
		SystemTerminalDescriptionProviderOptions options =
			new(
				useEnvironment: true,
				useUserDatabase: false,
				useSystemDatabases: false );
		SystemTerminalDescriptionProvider provider =
			new(
				options,
				snapshot,
				Array.Empty<string>() );

		TerminalDescription first =
			Load(
				provider,
				name );
		TerminalDescription second =
			Load(
				provider,
				name );

		Assert.Same(
			first,
			second );
	}

	[Fact]
	public void NewSystemProviderObservesChangedEntry() {
		using TemporaryDirectory temporary = new();
		string name =
			"t29-legacy-minimal";
		string path =
			WriteLiteralCandidate(
				temporary.Root,
				name,
				CreateEntryWithColumns( 80 ) );
		SystemTerminalDescriptionProvider firstProvider =
			CreateSystemProvider(
				temporary.Root );
		TerminalDescription first =
			Load(
				firstProvider,
				name );

		File.WriteAllBytes(
			path,
			CreateEntryWithColumns( 99 ) );

		SystemTerminalDescriptionProvider secondProvider =
			CreateSystemProvider(
				temporary.Root );
		TerminalDescription second =
			Load(
				secondProvider,
				name );

		AssertColumns(
			80,
			first );
		AssertColumns(
			99,
			second );
		Assert.Same(
			first,
			Load(
				firstProvider,
				name ) );
		Assert.NotSame(
			first,
			second );
	}

	[Fact]
	public void SeparateSystemProvidersDoNotShareCacheAcrossRoots() {
		using TemporaryDirectory firstRoot = new();
		using TemporaryDirectory secondRoot = new();
		string name =
			"t29-legacy-minimal";

		WriteLiteralCandidate(
			firstRoot.Root,
			name,
			CreateEntryWithColumns( 80 ) );
		WriteLiteralCandidate(
			secondRoot.Root,
			name,
			CreateEntryWithColumns( 99 ) );

		SystemTerminalDescriptionProvider firstProvider =
			CreateSystemProvider(
				firstRoot.Root );
		SystemTerminalDescriptionProvider secondProvider =
			CreateSystemProvider(
				secondRoot.Root );

		TerminalDescription first =
			Load(
				firstProvider,
				name );
		TerminalDescription second =
			Load(
				secondProvider,
				name );

		AssertColumns(
			80,
			first );
		AssertColumns(
			99,
			second );
		Assert.NotSame(
			first,
			second );
	}

	[Fact]
	public void SeparateSystemProvidersRespectIndependentOptions() {
		using TemporaryDirectory temporary = new();
		string name =
			"t29-legacy-minimal";
		string environmentRoot =
			temporary.CreateSubdirectory(
				"environment" );
		string home =
			temporary.CreateSubdirectory(
				"home" );

		WriteLiteralCandidate(
			environmentRoot,
			name,
			CreateEntryWithColumns( 80 ) );
		WriteLiteralCandidate(
			Path.Combine(
				home,
				".terminfo" ),
			name,
			CreateEntryWithColumns( 99 ) );

		SystemTerminalDiscoverySnapshot snapshot =
			new(
				termInfo: environmentRoot,
				termInfoDirs: null,
				homeDirectory: home,
				currentDirectory: temporary.Root,
				platform: TerminalHostPlatform.Linux );
		SystemTerminalDescriptionProvider environmentProvider =
			new(
				new SystemTerminalDescriptionProviderOptions(
					useEnvironment: true,
					useUserDatabase: false,
					useSystemDatabases: false ),
				snapshot,
				Array.Empty<string>() );
		SystemTerminalDescriptionProvider userProvider =
			new(
				new SystemTerminalDescriptionProviderOptions(
					useEnvironment: false,
					useUserDatabase: true,
					useSystemDatabases: false ),
				snapshot,
				Array.Empty<string>() );

		AssertColumns(
			80,
			Load(
				environmentProvider,
				name ) );
		AssertColumns(
			99,
			Load(
				userProvider,
				name ) );
	}

	[Fact]
	public void SystemWithBuiltInFallbackUsesBuiltInOnSystemMiss() {
		using TemporaryDirectory temporary = new();
		SystemTerminalDescriptionProvider systemProvider =
			CreateSystemProvider(
				temporary.Root );
		TerminalDatabase database =
			new(
				new ITerminalDescriptionProvider[]
				{
					systemProvider,
					TerminalDatabase.BuiltIn,
				} );

		TerminalDescription terminal =
			database.Load(
				"xterm" );

		Assert.Same(
			TerminalProfiles.Xterm,
			terminal );
	}

	[Fact]
	public void SystemLookupDoesNotMutateBuiltInDatabase() {
		using TemporaryDirectory temporary = new();
		string name =
			"t29-legacy-minimal";
		TerminalDescription builtInBefore =
			TerminalDatabase.BuiltIn.Load(
				"xterm" );

		WriteLiteralCandidate(
			temporary.Root,
			name,
			CreateEntryWithColumns( 80 ) );
		SystemTerminalDescriptionProvider systemProvider =
			CreateSystemProvider(
				temporary.Root );

		AssertColumns(
			80,
			Load(
				systemProvider,
				name ) );

		TerminalDescription builtInAfter =
			TerminalDatabase.BuiltIn.Load(
				"xterm" );

		Assert.Same(
			builtInBefore,
			builtInAfter );
		Assert.Same(
			TerminalProfiles.Xterm,
			builtInAfter );
		Assert.False(
			TerminalDatabase.BuiltIn.TryLoad(
				name,
				out TerminalDescription? leaked ) );
		Assert.Null(
			leaked );
	}

	[Fact]
	public void SystemCacheIsInstanceOwnedRatherThanStatic() {
		FieldInfo cache =
			typeof( SystemTerminalDescriptionProvider )
				.GetField(
					"_cache",
					BindingFlags.NonPublic
					| BindingFlags.Instance )!;

		Assert.NotNull(
			cache );
		Assert.False(
			cache.IsStatic );
	}

	private static SystemTerminalDescriptionProvider CreateSystemProvider(
		string root ) {
		ArgumentNullException.ThrowIfNull( root );

		SystemTerminalDescriptionProviderOptions options =
			new(
				useEnvironment: true,
				useUserDatabase: false,
				useSystemDatabases: false );
		SystemTerminalDiscoverySnapshot snapshot =
			new(
				termInfo: root,
				termInfoDirs: null,
				homeDirectory: null,
				currentDirectory: Path.GetFullPath( root ),
				platform: TerminalHostPlatform.Linux );

		return new SystemTerminalDescriptionProvider(
			options,
			snapshot,
			Array.Empty<string>() );
	}

	private static TerminalDescription Load(
		ITerminalDescriptionProvider provider,
		string name ) {
		ArgumentNullException.ThrowIfNull( provider );
		ArgumentNullException.ThrowIfNull( name );

		Assert.True(
			provider.TryLoad(
				name,
				out TerminalDescription? terminal ) );
		return Assert.IsType<TerminalDescription>(
			terminal );
	}

	private static void AssertColumns(
		int expected,
		TerminalDescription terminal ) {
		ArgumentNullException.ThrowIfNull( terminal );

		Assert.Equal<int?>(
			expected,
			terminal.GetNumber(
				NumericCapability.Columns ) );
	}

	private static byte[] CreateEntryWithColumns(
		int columns ) {
		byte[] entry =
			ReadFixture(
				"compiled/t29-legacy-minimal.bin" );
		SetLegacyColumns(
			entry,
			columns );
		return entry;
	}

	private static void SetLegacyColumns(
		byte[] entry,
		int columns ) {
		ArgumentNullException.ThrowIfNull( entry );

		int names =
			BinaryPrimitives.ReadUInt16LittleEndian(
				entry.AsSpan(
					2,
					sizeof( ushort ) ) );
		int booleans =
			BinaryPrimitives.ReadUInt16LittleEndian(
				entry.AsSpan(
					4,
					sizeof( ushort ) ) );
		int numericOffset =
			CompiledHeaderSize
			+ names
			+ booleans;

		if ( ( numericOffset & 1 ) != 0 ) {
			numericOffset++;
		}

		BinaryPrimitives.WriteInt16LittleEndian(
			entry.AsSpan(
				numericOffset,
				sizeof( short ) ),
			checked((short)columns) );
	}

	private static string WriteLiteralCandidate(
		string root,
		string name,
		byte[] entry ) {
		ArgumentNullException.ThrowIfNull( root );
		ArgumentNullException.ThrowIfNull( name );
		ArgumentNullException.ThrowIfNull( entry );

		string directory =
			Path.Combine(
				root,
				name[ 0 ].ToString() );
		Directory.CreateDirectory(
			directory );

		string path =
			Path.Combine(
				directory,
				name );
		File.WriteAllBytes(
			path,
			entry );
		return path;
	}

	private static byte[] ReadFixture(
		string relativePath ) {
		ArgumentNullException.ThrowIfNull( relativePath );

		return File.ReadAllBytes(
			Path.Combine(
				AppContext.BaseDirectory,
				"fixtures",
				"compiled-terminfo",
				relativePath.Replace(
					'/',
					Path.DirectorySeparatorChar ) ) );
	}

	private sealed class TemporaryDirectory : IDisposable {
		internal TemporaryDirectory() {
			Root =
				Path.Combine(
					Path.GetTempPath(),
					"icod-terminfo-t39-"
					+ Guid.NewGuid().ToString( "N" ) );
			Directory.CreateDirectory(
				Root );
		}

		internal string Root {
			get;
		}

		internal string CreateSubdirectory(
			string name ) {
			ArgumentNullException.ThrowIfNull( name );

			string path =
				Path.Combine(
					Root,
					name );
			Directory.CreateDirectory(
				path );
			return path;
		}

		public void Dispose() {
			if ( Directory.Exists(
					Root ) ) {
				Directory.Delete(
					Root,
					recursive: true );
			}
		}
	}
}
