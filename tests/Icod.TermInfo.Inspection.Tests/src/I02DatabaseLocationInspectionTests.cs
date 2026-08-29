using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class I02DatabaseLocationInspectionTests {
	[Fact]
	public void EncodedTermInfoPrecedesDirectoriesWithoutExposingPayload() {
		string root = CreateRoot();
		SystemTerminalDescriptionProviderOptions options = new();
		SystemTerminalDiscoverySnapshot snapshot =
			new(
				"hex:00112233",
				"search",
				Path.Combine( root, "home" ),
				root,
				TerminalHostPlatform.Linux
			);
		string defaultRoot = Path.Combine( root, "default" );

		IReadOnlyList<TermInfoDatabaseLocation> locations =
			TermInfoDatabaseInspector.GetSystemLocations(
				options,
				snapshot,
				[ defaultRoot ]
			);

		Assert.Equal( 4, locations.Count );
		Assert.Equal( TermInfoDatabaseLocationKind.EncodedTermInfo, locations[ 0 ].Kind );
		Assert.Null( locations[ 0 ].Path );
		AssertLocation(
			locations[ 1 ],
			TermInfoDatabaseLocationKind.UserDatabase,
			Path.Combine( root, "home", ".terminfo" )
		);
		AssertLocation(
			locations[ 2 ],
			TermInfoDatabaseLocationKind.TermInfoDirsDirectory,
			Path.Combine( root, "search" )
		);
		AssertLocation(
			locations[ 3 ],
			TermInfoDatabaseLocationKind.PlatformDefaultDirectory,
			defaultRoot
		);
	}

	[Fact]
	public void DirectorySourcesPreserveRuntimePrecedenceAndNormalizePaths() {
		string root = CreateRoot();
		SystemTerminalDescriptionProviderOptions options = new();
		SystemTerminalDiscoverySnapshot snapshot =
			new(
				"explicit",
				"search",
				Path.Combine( root, "home" ),
				root,
				TerminalHostPlatform.Linux
			);
		string defaultRoot = Path.Combine( root, "default" );

		IReadOnlyList<TermInfoDatabaseLocation> locations =
			TermInfoDatabaseInspector.GetSystemLocations(
				options,
				snapshot,
				[ defaultRoot ]
			);

		Assert.Collection(
			locations,
			location => AssertLocation(
				location,
				TermInfoDatabaseLocationKind.TermInfoDirectory,
				Path.Combine( root, "explicit" )
			),
			location => AssertLocation(
				location,
				TermInfoDatabaseLocationKind.UserDatabase,
				Path.Combine( root, "home", ".terminfo" )
			),
			location => AssertLocation(
				location,
				TermInfoDatabaseLocationKind.TermInfoDirsDirectory,
				Path.Combine( root, "search" )
			),
			location => AssertLocation(
				location,
				TermInfoDatabaseLocationKind.PlatformDefaultDirectory,
				defaultRoot
			)
		);
	}

	[Fact]
	public void EmptyTermInfoDirsComponentExpandsDefaultsInPlaceAndDeduplicatesFinalDefaults() {
		string root = CreateRoot();
		SystemTerminalDescriptionProviderOptions options =
			new(
				useEnvironment: true,
				useUserDatabase: false,
				useSystemDatabases: true
			);
		SystemTerminalDiscoverySnapshot snapshot =
			new(
				null,
				"before::after",
				null,
				root,
				TerminalHostPlatform.Linux
			);
		string defaultRoot = Path.Combine( root, "default" );

		IReadOnlyList<TermInfoDatabaseLocation> locations =
			TermInfoDatabaseInspector.GetSystemLocations(
				options,
				snapshot,
				[ defaultRoot ]
			);

		Assert.Collection(
			locations,
			location => AssertLocation(
				location,
				TermInfoDatabaseLocationKind.TermInfoDirsDirectory,
				Path.Combine( root, "before" )
			),
			location => AssertLocation(
				location,
				TermInfoDatabaseLocationKind.TermInfoDirsDirectory,
				defaultRoot
			),
			location => AssertLocation(
				location,
				TermInfoDatabaseLocationKind.TermInfoDirsDirectory,
				Path.Combine( root, "after" )
			)
		);
	}

	[Fact]
	public void WindowsDiscoveryDeduplicatesDirectoryPathsCaseInsensitively() {
		string root = CreateRoot();
		SystemTerminalDescriptionProviderOptions options =
			new(
				useEnvironment: true,
				useUserDatabase: false,
				useSystemDatabases: false
			);
		SystemTerminalDiscoverySnapshot snapshot =
			new(
				"Root",
				"root;other",
				null,
				root,
				TerminalHostPlatform.Windows
			);

		IReadOnlyList<TermInfoDatabaseLocation> locations =
			TermInfoDatabaseInspector.GetSystemLocations(
				options,
				snapshot,
				Array.Empty<string>()
			);

		Assert.Collection(
			locations,
			location => AssertLocation(
				location,
				TermInfoDatabaseLocationKind.TermInfoDirectory,
				Path.Combine( root, "Root" )
			),
			location => AssertLocation(
				location,
				TermInfoDatabaseLocationKind.TermInfoDirsDirectory,
				Path.Combine( root, "other" )
			)
		);
	}

	[Fact]
	public void DisabledDiscoverySourcesProduceNoLocations() {
		SystemTerminalDescriptionProviderOptions options =
			new(
				useEnvironment: false,
				useUserDatabase: false,
				useSystemDatabases: false
			);

		IReadOnlyList<TermInfoDatabaseLocation> locations =
			TermInfoDatabaseInspector.GetSystemLocations( options );

		Assert.Empty( locations );
	}

	[Fact]
	public void PlatformDefaultsRemainDistinctWhenNotInjectedThroughTermInfoDirs() {
		string root = CreateRoot();
		SystemTerminalDescriptionProviderOptions options =
			new(
				useEnvironment: false,
				useUserDatabase: false,
				useSystemDatabases: true
			);
		SystemTerminalDiscoverySnapshot snapshot =
			new(
				null,
				null,
				null,
				root,
				TerminalHostPlatform.Linux
			);
		string defaultRoot = Path.Combine( root, "default" );

		TermInfoDatabaseLocation location =
			Assert.Single(
				TermInfoDatabaseInspector.GetSystemLocations(
					options,
					snapshot,
					[ defaultRoot ]
				)
			);

		AssertLocation(
			location,
			TermInfoDatabaseLocationKind.PlatformDefaultDirectory,
			defaultRoot
		);
	}


	[Fact]
	public void PlatformDefaultSetsMatchRuntimeForLinuxMacOsAndWindows() {
		string root = CreateRoot();
		SystemTerminalDescriptionProviderOptions options =
			new(
				useEnvironment: false,
				useUserDatabase: false,
				useSystemDatabases: true
			);

		AssertDefaultKinds(
			options,
			root,
			TerminalHostPlatform.Linux,
			3
		);
		AssertDefaultKinds(
			options,
			root,
			TerminalHostPlatform.MacOS,
			1
		);
		AssertDefaultKinds(
			options,
			root,
			TerminalHostPlatform.Windows,
			0
		);
	}

	[Fact]
	public void DisablingEnvironmentIgnoresSnapshottedEnvironmentValues() {
		string root = CreateRoot();
		SystemTerminalDescriptionProviderOptions options =
			new(
				useEnvironment: false,
				useUserDatabase: true,
				useSystemDatabases: false
			);
		SystemTerminalDiscoverySnapshot snapshot =
			new(
				"encoded-or-directory",
				"search",
				Path.Combine( root, "home" ),
				root,
				TerminalHostPlatform.Linux
			);

		TermInfoDatabaseLocation location =
			Assert.Single(
				TermInfoDatabaseInspector.GetSystemLocations(
					options,
					snapshot,
					Array.Empty<string>()
				)
			);

		AssertLocation(
			location,
			TermInfoDatabaseLocationKind.UserDatabase,
			Path.Combine( root, "home", ".terminfo" )
		);
	}

	[Fact]
	public void DisablingUserDatabaseOmitsSnapshottedHomeLocation() {
		string root = CreateRoot();
		SystemTerminalDescriptionProviderOptions options =
			new(
				useEnvironment: false,
				useUserDatabase: false,
				useSystemDatabases: true
			);
		SystemTerminalDiscoverySnapshot snapshot =
			new(
				null,
				null,
				Path.Combine(root, "home"),
				root,
				TerminalHostPlatform.Linux
			);
		string defaultRoot = Path.Combine(root, "default");

		TermInfoDatabaseLocation location =
			Assert.Single(
				TermInfoDatabaseInspector.GetSystemLocations(
					options,
					snapshot,
					[defaultRoot]
				)
			);

		AssertLocation(
			location,
			TermInfoDatabaseLocationKind.PlatformDefaultDirectory,
			defaultRoot
		);
	}

	[Fact]
	public void DisablingSystemDatabasesPreventsEmptySearchComponentsFromInjectingDefaults() {
		string root = CreateRoot();
		SystemTerminalDescriptionProviderOptions options =
			new(
				useEnvironment: true,
				useUserDatabase: false,
				useSystemDatabases: false
			);
		SystemTerminalDiscoverySnapshot snapshot =
			new(
				null,
				"::search::",
				null,
				root,
				TerminalHostPlatform.Linux
			);
		string defaultRoot = Path.Combine( root, "default" );

		TermInfoDatabaseLocation location =
			Assert.Single(
				TermInfoDatabaseInspector.GetSystemLocations(
					options,
					snapshot,
					[ defaultRoot ]
				)
			);

		AssertLocation(
			location,
			TermInfoDatabaseLocationKind.TermInfoDirsDirectory,
			Path.Combine( root, "search" )
		);
	}

	[Fact]
	public void ReturnedLocationSnapshotIsReadOnly() {
		string root = CreateRoot();
		SystemTerminalDescriptionProviderOptions options =
			new(
				useEnvironment: false,
				useUserDatabase: false,
				useSystemDatabases: true
			);
		SystemTerminalDiscoverySnapshot snapshot =
			new(
				null,
				null,
				null,
				root,
				TerminalHostPlatform.Linux
			);

		IReadOnlyList<TermInfoDatabaseLocation> locations =
			TermInfoDatabaseInspector.GetSystemLocations(
				options,
				snapshot,
				[ Path.Combine( root, "default" ) ]
			);
		IList<TermInfoDatabaseLocation> mutableView =
			Assert.IsAssignableFrom<IList<TermInfoDatabaseLocation>>( locations );

		Assert.True( mutableView.IsReadOnly );
		Assert.Throws<NotSupportedException>(
			() => mutableView.Clear()
		);
	}

	private static void AssertLocation(
		TermInfoDatabaseLocation location,
		TermInfoDatabaseLocationKind kind,
		string expectedPath
	) {
		ArgumentNullException.ThrowIfNull( location );
		ArgumentException.ThrowIfNullOrWhiteSpace( expectedPath );

		Assert.Equal( kind, location.Kind );
		Assert.Equal( Path.GetFullPath( expectedPath ), location.Path );
	}

	private static void AssertDefaultKinds(
		SystemTerminalDescriptionProviderOptions options,
		string currentDirectory,
		TerminalHostPlatform platform,
		int expectedCount
	) {
		ArgumentNullException.ThrowIfNull( options );
		ArgumentException.ThrowIfNullOrWhiteSpace( currentDirectory );

		SystemTerminalDiscoverySnapshot snapshot =
			new(
				null,
				null,
				null,
				currentDirectory,
				platform
			);
		IReadOnlyList<TermInfoDatabaseLocation> locations =
			TermInfoDatabaseInspector.GetSystemLocations(
				options,
				snapshot,
				SystemTerminalDescriptionProvider.GetDefaultRoots( platform )
			);

		Assert.Equal( expectedCount, locations.Count );
		Assert.All(
			locations,
			location => Assert.Equal(
				TermInfoDatabaseLocationKind.PlatformDefaultDirectory,
				location.Kind
			)
		);
	}

	private static string CreateRoot() {
		return Path.GetFullPath(
			Path.Combine(
				Path.GetTempPath(),
				$"Icod.TermInfo-T02-{Guid.NewGuid():N}"
			)
		);
	}
}
