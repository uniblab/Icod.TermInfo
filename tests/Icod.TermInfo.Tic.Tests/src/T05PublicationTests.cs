using System.Text;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Tic.Tests;

public sealed class T05PublicationTests {
	private const string SimpleSource =
		"demo|demo-alias|Demo terminal,\n"
		+ "    am,\n"
		+ "    cols#80,\n";

	[Fact]
	public async Task ExplicitDatabasePublicationRoundTripsSemantically() {
		string root = CreateTemporaryDirectory();
		try {
			using MemoryStream stdin = CreateInput( SimpleSource );
			using var stdout = new MemoryStream();
			using var stderr = new MemoryStream();

			int status = await Command.RunAsync(
				[ "-o", root, "-" ],
				stdin,
				stdout,
				stderr
			);

			Assert.Equal( CommandExitCodes.Success, status );
			Assert.Empty( ReadText( stdout ) );
			Assert.Empty( ReadText( stderr ) );

			DirectoryTerminalDescriptionProvider provider =
				new( root );
			Assert.True(
				provider.TryLoad(
					"demo",
					out TerminalDescription? actual
				)
			);
			TerminalDescription loaded =
				Assert.IsType<TerminalDescription>( actual );
			Assert.True(
				TerminalDescriptionComparer.Compare(
					ResolveSource( SimpleSource, "demo" ),
					loaded
				).AreEqual
			);
			Assert.True(
				provider.TryLoad(
					"demo-alias",
					out TerminalDescription? alias
				)
			);
			TerminalDescription loadedAlias =
				Assert.IsType<TerminalDescription>( alias );
			Assert.True(
				TerminalDescriptionComparer.Compare(
					loaded,
					loadedAlias
				).AreEqual
			);
		}
		finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task PublicationCreatesMissingDatabaseDirectory() {
		string parent = CreateTemporaryDirectory();
		string root = System.IO.Path.Combine( parent, "new-database" );
		try {
			int status = await RunPublishAsync(
				SimpleSource,
				[ "-o", root, "-" ]
			);

			Assert.Equal( CommandExitCodes.Success, status );
			Assert.True( Directory.Exists( root ) );
			DirectoryTerminalDescriptionProvider provider =
				new( root );
			Assert.True( provider.TryLoad( "demo", out _ ) );
		}
		finally {
			DeleteTemporaryDirectory( parent );
		}
	}

	[Fact]
	public async Task PublicationWritesEveryEntryWhenNoSelectionIsProvided() {
		const string source =
			"one|One terminal,am,\n"
			+ "two|Two terminal,cols#80,\n";
		string root = CreateTemporaryDirectory();
		try {
			int status = await RunPublishAsync(
				source,
				[ "-o", root, "-" ]
			);

			Assert.Equal( CommandExitCodes.Success, status );
			DirectoryTerminalDescriptionProvider provider =
				new( root );
			Assert.True( provider.TryLoad( "one", out _ ) );
			Assert.True( provider.TryLoad( "two", out _ ) );
		}
		finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task SelectionPublishesOnlySelectedEntry() {
		const string source =
			"one|One terminal,am,\n"
			+ "two|two-alias|Two terminal,cols#80,\n";
		string root = CreateTemporaryDirectory();
		try {
			int status = await RunPublishAsync(
				source,
				[ "-e", "two-alias", "-o", root, "-" ]
			);

			Assert.Equal( CommandExitCodes.Success, status );
			DirectoryTerminalDescriptionProvider provider =
				new( root );
			Assert.False( provider.TryLoad( "one", out _ ) );
			Assert.True( provider.TryLoad( "two", out _ ) );
			Assert.True( provider.TryLoad( "two-alias", out _ ) );
		}
		finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task ExistingDestinationFailsWithoutChangingPublishedEntry() {
		const string firstSource =
			"demo|Demo terminal,cols#80,\n";
		const string secondSource =
			"demo|Demo terminal,cols#132,\n";
		string root = CreateTemporaryDirectory();
		try {
			Assert.Equal(
				CommandExitCodes.Success,
				await RunPublishAsync(
					firstSource,
					[ "-o", root, "-" ]
				)
			);
			string entryPath = GetEntryPath( root, "demo" );
			byte[] before = await File.ReadAllBytesAsync( entryPath );

			using MemoryStream stdin = CreateInput( secondSource );
			using var stdout = new MemoryStream();
			using var stderr = new MemoryStream();
			int status = await Command.RunAsync(
				[ "-o", root, "-" ],
				stdin,
				stdout,
				stderr
			);

			Assert.Equal( CommandExitCodes.Failure, status );
			Assert.Contains( "TIC0007 error", ReadText( stderr ) );
			Assert.Equal(
				before,
				await File.ReadAllBytesAsync( entryPath )
			);
		}
		finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task ForceReplacesExistingEntry() {
		const string firstSource =
			"demo|Demo terminal,cols#80,\n";
		const string secondSource =
			"demo|Demo terminal,cols#132,\n";
		string root = CreateTemporaryDirectory();
		try {
			Assert.Equal(
				CommandExitCodes.Success,
				await RunPublishAsync(
					firstSource,
					[ "-o", root, "-" ]
				)
			);

			int status = await RunPublishAsync(
				secondSource,
				[ "--force", "-o", root, "-" ]
			);

			Assert.Equal( CommandExitCodes.Success, status );
			DirectoryTerminalDescriptionProvider provider =
				new( root );
			Assert.True(
				provider.TryLoad(
					"demo",
					out TerminalDescription? actual
				)
			);
			TerminalDescription loaded =
				Assert.IsType<TerminalDescription>( actual );
			Assert.True(
				TerminalDescriptionComparer.Compare(
					ResolveSource( secondSource, "demo" ),
					loaded
				).AreEqual
			);
		}
		finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task SummaryReportsDestinationEntryAndWarningCounts() {
		const string source =
			"one|shared|One terminal,am,\n"
			+ "two|shared|Two terminal,am,\n";
		string root = CreateTemporaryDirectory();
		try {
			using MemoryStream stdin = CreateInput( source );
			using var stdout = new MemoryStream();
			using var stderr = new MemoryStream();

			int status = await Command.RunAsync(
				[ "-e", "one", "-s", "-o", root, "-" ],
				stdin,
				stdout,
				stderr
			);
			string output = ReadText( stderr );

			Assert.Equal( CommandExitCodes.Success, status );
			Assert.Contains( "TIS0026 warning", output );
			Assert.Contains( $"tic: output: {System.IO.Path.GetFullPath( root )}", output );
			Assert.Contains( "tic: compiled entries: 1", output );
			Assert.Contains( "tic: warnings: 1", output );
		}
		finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Theory]
	[InlineData( "-s" )]
	[InlineData( "--force" )]
	public async Task CheckOnlyRejectsPublicationOnlyFlag(
		string publicationOption
	) {
		using MemoryStream stdin = CreateInput( SimpleSource );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-c", publicationOption, "-" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.UsageError, status );
		Assert.Contains( "not valid with check-only mode", ReadText( stderr ) );
	}

	[Fact]
	public async Task CheckOnlyRejectsExplicitOutputDirectory() {
		string root = CreateTemporaryDirectory();
		try {
			using MemoryStream stdin = CreateInput( SimpleSource );
			using var stdout = new MemoryStream();
			using var stderr = new MemoryStream();

			int status = await Command.RunAsync(
				[ "-c", "-o", root, "-" ],
				stdin,
				stdout,
				stderr
			);

			Assert.Equal( CommandExitCodes.UsageError, status );
			Assert.Empty( Directory.GetFileSystemEntries( root ) );
		}
		finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task OutputOptionRequiresOneValue() {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-o" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.UsageError, status );
		Assert.Contains( "requires an output directory", ReadText( stderr ) );
	}

	[Fact]
	public async Task OutputOptionMayBeSpecifiedOnlyOnce() {
		using MemoryStream stdin = CreateInput( SimpleSource );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-o", "first", "-o", "second", "-" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.UsageError, status );
		Assert.Contains( "may be specified only once", ReadText( stderr ) );
	}

	[Fact]
	public void ExplicitDestinationIsNormalizedAndPreferred() {
		string relative =
			System.IO.Path.Combine(
				"relative",
				"terminfo"
			);

		TicDestinationResolution result =
			TicDestinationResolver.ResolveExplicit( relative );

		Assert.Null( result.Error );
		Assert.Equal(
			System.IO.Path.GetFullPath( relative ),
			result.Path
		);
	}

	[Fact]
	public void DefaultDestinationPrefersTermInfoDirectoryThenUserDatabase() {
		string environmentRoot =
			System.IO.Path.GetFullPath( "environment-root" );
		string userRoot =
			System.IO.Path.GetFullPath( "user-root" );
		TicDestinationCandidate[] locations = [
			new(
				TermInfoDatabaseLocationKind.EncodedTermInfo,
				null
			),
			new(
				TermInfoDatabaseLocationKind.UserDatabase,
				userRoot
			),
			new(
				TermInfoDatabaseLocationKind.TermInfoDirsDirectory,
				System.IO.Path.GetFullPath( "terminfo-dirs-root" )
			),
			new(
				TermInfoDatabaseLocationKind.TermInfoDirectory,
				environmentRoot
			),
		];

		TicDestinationResolution environmentResult =
			TicDestinationResolver.ResolveDefault( locations );
		TicDestinationResolution userResult =
			TicDestinationResolver.ResolveDefault(
				locations
					.Where(
						location =>
							location.Kind
								!= TermInfoDatabaseLocationKind.TermInfoDirectory
					)
					.ToArray()
			);

		Assert.Equal( environmentRoot, environmentResult.Path );
		Assert.Equal( userRoot, userResult.Path );
	}

	[Fact]
	public void DefaultDestinationNeverFallsThroughToTermInfoDirsOrSystemRoots() {
		TicDestinationCandidate[] locations = [
			new(
				TermInfoDatabaseLocationKind.EncodedTermInfo,
				null
			),
			new(
				TermInfoDatabaseLocationKind.TermInfoDirsDirectory,
				System.IO.Path.GetFullPath( "terminfo-dirs-root" )
			),
			new(
				TermInfoDatabaseLocationKind.PlatformDefaultDirectory,
				System.IO.Path.GetFullPath( "system-root" )
			),
		];

		TicDestinationResolution result =
			TicDestinationResolver.ResolveDefault( locations );

		Assert.Null( result.Path );
		Assert.Contains(
			"specify one with '-o'",
			Assert.IsType<string>( result.Error )
		);
	}

	[Fact]
	public async Task InvalidExplicitDestinationIsAControlledDestinationFailure() {
		using MemoryStream stdin = CreateInput( SimpleSource );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-o", "bad\0path", "-" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.Failure, status );
		Assert.Contains( "TIC0006 error", ReadText( stderr ) );
	}

	[Fact]
	public async Task PublicationRejectsUnsafeTerminalPathWithoutEscapingRoot() {
		const string source =
			"bad/name|Unsafe terminal,am,\n";
		string parent = CreateTemporaryDirectory();
		string root = System.IO.Path.Combine( parent, "terminfo" );
		try {
			using MemoryStream stdin = CreateInput( source );
			using var stdout = new MemoryStream();
			using var stderr = new MemoryStream();

			int status = await Command.RunAsync(
				[ "-o", root, "-" ],
				stdin,
				stdout,
				stderr
			);

			Assert.Equal( CommandExitCodes.Failure, status );
			Assert.Contains( "TIC0007 error", ReadText( stderr ) );
			Assert.False( Directory.Exists( root ) );
			Assert.False(
				File.Exists(
					System.IO.Path.Combine(
						parent,
						"name"
					)
				)
			);
		}
		finally {
			DeleteTemporaryDirectory( parent );
		}
	}

	[Fact]
	public async Task OutputRootWhichIsAFileIsReportedAsPublicationFailure() {
		string parent = CreateTemporaryDirectory();
		try {
			string root =
				System.IO.Path.Combine(
					parent,
					"not-a-directory"
				);
			await File.WriteAllTextAsync( root, "sentinel" );
			using MemoryStream stdin = CreateInput( SimpleSource );
			using var stdout = new MemoryStream();
			using var stderr = new MemoryStream();

			int status = await Command.RunAsync(
				[ "-o", root, "-" ],
				stdin,
				stdout,
				stderr
			);

			Assert.Equal( CommandExitCodes.Failure, status );
			Assert.Contains( "TIC0007 error", ReadText( stderr ) );
			Assert.Equal( "sentinel", await File.ReadAllTextAsync( root ) );
		}
		finally {
			DeleteTemporaryDirectory( parent );
		}
	}

	[Fact]
	public async Task CancellationBeforePublicationCreatesNoDatabase() {
		string parent = CreateTemporaryDirectory();
		string root = System.IO.Path.Combine( parent, "terminfo" );
		try {
			using MemoryStream stdin = CreateInput( SimpleSource );
			using var stdout = new MemoryStream();
			using var stderr = new MemoryStream();
			using var cancellation = new CancellationTokenSource();
			cancellation.Cancel();

			int status = await Command.RunAsync(
				[ "-o", root, "-" ],
				stdin,
				stdout,
				stderr,
				cancellation.Token
			);

			Assert.Equal( CommandExitCodes.Canceled, status );
			Assert.False( Directory.Exists( root ) );
		}
		finally {
			DeleteTemporaryDirectory( parent );
		}
	}

	private static async Task<int> RunPublishAsync(
		string source,
		string[] arguments
	) {
		ArgumentNullException.ThrowIfNull( source );
		ArgumentNullException.ThrowIfNull( arguments );

		using MemoryStream stdin = CreateInput( source );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		return await Command.RunAsync(
			arguments,
			stdin,
			stdout,
			stderr
		);
	}

	private static TerminalDescription ResolveSource(
		string source,
		string name
	) {
		ArgumentNullException.ThrowIfNull( source );
		ArgumentException.ThrowIfNullOrWhiteSpace( name );

		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse( source );
		Assert.False( parsed.HasErrors );
		TermInfoSourceResolveResult resolved =
			TermInfoSourceResolver.Resolve(
				parsed.Document,
				name
			);
		Assert.False( resolved.HasErrors );
		return Assert.IsType<TermInfoSourceResolvedEntry>(
			resolved.Entry
		).ToTerminalDescription();
	}

	private static string GetEntryPath(
		string root,
		string name
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		ArgumentException.ThrowIfNullOrWhiteSpace( name );

		return System.IO.Path.Combine(
			root,
			((byte)name[ 0 ]).ToString( "x2" ),
			name
		);
	}

	private static MemoryStream CreateInput(
		string text
	) {
		ArgumentNullException.ThrowIfNull( text );

		return new MemoryStream(
			new UTF8Encoding( false ).GetBytes( text )
		);
	}

	private static string ReadText(
		MemoryStream stream
	) {
		ArgumentNullException.ThrowIfNull( stream );

		return Encoding.UTF8.GetString(
			stream.ToArray()
		);
	}

	private static string CreateTemporaryDirectory() {
		string path =
			System.IO.Path.Combine(
				System.IO.Path.GetTempPath(),
				"icod-terminfo-tic-t05-" + Guid.NewGuid().ToString( "N" )
			);
		Directory.CreateDirectory( path );
		return path;
	}

	private static void DeleteTemporaryDirectory(
		string path
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( path );

		if ( Directory.Exists( path ) ) {
			Directory.Delete(
				path,
				recursive: true
			);
		}
	}
}
