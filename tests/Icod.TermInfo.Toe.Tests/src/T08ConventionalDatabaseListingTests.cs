using System.Globalization;
using System.Text;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo.Compiler;
using Xunit;

namespace Icod.TermInfo.Toe.Tests;

[CollectionDefinition( "ToeEnvironmentSensitive", DisableParallelization = true )]
public sealed class ToeEnvironmentSensitiveCollection {
}

[Collection( "ToeEnvironmentSensitive" )]
public sealed class T08ConventionalDatabaseListingTests {
	[Fact]
	public async Task EmptyExplicitRootReturnsSuccessWithoutEntries() {
		string root = CreateTemporaryDirectory();

		try {
			CommandResult result = await RunAsync( root );

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Equal( string.Empty, result.Stdout );
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			DeleteDirectory( root );
		}
	}

	[Fact]
	public async Task ExplicitRootListsParsedCanonicalNameAndDescription() {
		string root = CreateTemporaryDirectory();

		try {
			Publish(
				root,
				CreateTerminal(
					"demo",
					"Demonstration terminal"
				)
			);

			CommandResult result = await RunAsync( root );

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Equal(
				$"demo\tDemonstration terminal{Environment.NewLine}",
				result.Stdout
			);
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			DeleteDirectory( root );
		}
	}

	[Fact]
	public async Task ExplicitRootsRemainInOperandOrder() {
		string firstRoot = CreateTemporaryDirectory();
		string secondRoot = CreateTemporaryDirectory();

		try {
			Publish(
				firstRoot,
				CreateTerminal(
					"first",
					"First terminal"
				)
			);
			Publish(
				secondRoot,
				CreateTerminal(
					"second",
					"Second terminal"
				)
			);

			CommandResult result = await RunAsync(
				firstRoot,
				secondRoot
			);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.True(
				result.Stdout.IndexOf(
					"first\tFirst terminal",
					StringComparison.Ordinal
				)
				< result.Stdout.IndexOf(
					"second\tSecond terminal",
					StringComparison.Ordinal
				)
			);
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			DeleteDirectory( firstRoot );
			DeleteDirectory( secondRoot );
		}
	}

	[Fact]
	public async Task AllDatabasesDoesNotChangeExplicitOperandProcessing() {
		string firstRoot = CreateTemporaryDirectory();
		string secondRoot = CreateTemporaryDirectory();

		try {
			Publish(
				firstRoot,
				CreateTerminal(
					"first",
					"First terminal"
				)
			);
			Publish(
				secondRoot,
				CreateTerminal(
					"second",
					"Second terminal"
				)
			);

			CommandResult result = await RunAsync(
				"-a",
				firstRoot,
				secondRoot
			);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Contains(
				"first\tFirst terminal",
				result.Stdout,
				StringComparison.Ordinal
			);
			Assert.Contains(
				"second\tSecond terminal",
				result.Stdout,
				StringComparison.Ordinal
			);
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			DeleteDirectory( firstRoot );
			DeleteDirectory( secondRoot );
		}
	}

	[Fact]
	public async Task DefaultDiscoveryUsesDirectoryValuedTermInfoFirst() {
		string root = CreateTemporaryDirectory();

		try {
			Publish(
				root,
				CreateTerminal(
					"discovered",
					"Discovered terminal"
				)
			);

			using var termInfo = new EnvironmentVariableLease(
				"TERMINFO",
				root
			);

			CommandResult result = await RunAsync();

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Equal(
				$"discovered\tDiscovered terminal{Environment.NewLine}",
				result.Stdout
			);
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			DeleteDirectory( root );
		}
	}

	[Fact]
	public async Task HeadingsIdentifyEachConventionalDatabase() {
		string firstRoot = CreateTemporaryDirectory();
		string secondRoot = CreateTemporaryDirectory();

		try {
			Publish(
				firstRoot,
				CreateTerminal(
					"first",
					"First terminal"
				)
			);
			Publish(
				secondRoot,
				CreateTerminal(
					"second",
					"Second terminal"
				)
			);

			CommandResult result = await RunAsync(
				"-h",
				firstRoot,
				secondRoot
			);

			string firstHeading = $"# {System.IO.Path.GetFullPath( firstRoot )}{Environment.NewLine}";
			string secondHeading = $"# {System.IO.Path.GetFullPath( secondRoot )}{Environment.NewLine}";

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Contains(
				firstHeading,
				result.Stdout,
				StringComparison.Ordinal
			);
			Assert.Contains(
				secondHeading,
				result.Stdout,
				StringComparison.Ordinal
			);
			Assert.True(
				result.Stdout.IndexOf(
					firstHeading,
					StringComparison.Ordinal
				)
				< result.Stdout.IndexOf(
					secondHeading,
					StringComparison.Ordinal
				)
			);
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			DeleteDirectory( firstRoot );
			DeleteDirectory( secondRoot );
		}
	}

	[Fact]
	public async Task SortUsesCanonicalOrdinalNames() {
		string root = CreateTemporaryDirectory();
		CultureInfo originalCulture = CultureInfo.CurrentCulture;

		try {
			Publish(
				root,
				CreateTerminal(
					"zeta",
					"Zeta terminal"
				)
			);
			Publish(
				root,
				CreateTerminal(
					"alpha",
					"Alpha terminal"
				)
			);

			CultureInfo.CurrentCulture = new CultureInfo( "tr-TR" );
			CommandResult result = await RunAsync(
				"-s",
				root
			);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.True(
				result.Stdout.IndexOf(
					"alpha\tAlpha terminal",
					StringComparison.Ordinal
				)
				< result.Stdout.IndexOf(
					"zeta\tZeta terminal",
					StringComparison.Ordinal
				)
			);
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			DeleteDirectory( root );
		}
	}

	[Fact]
	public async Task DuplicateCanonicalNamesAcrossRootsRemainVisible() {
		string firstRoot = CreateTemporaryDirectory();
		string secondRoot = CreateTemporaryDirectory();

		try {
			Publish(
				firstRoot,
				CreateTerminal(
					"shared",
					"First copy"
				)
			);
			Publish(
				secondRoot,
				CreateTerminal(
					"shared",
					"Second copy"
				)
			);

			CommandResult result = await RunAsync(
				firstRoot,
				secondRoot
			);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Contains(
				$"shared\tFirst copy{Environment.NewLine}",
				result.Stdout,
				StringComparison.Ordinal
			);
			Assert.Contains(
				$"shared\tSecond copy{Environment.NewLine}",
				result.Stdout,
				StringComparison.Ordinal
			);
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			DeleteDirectory( firstRoot );
			DeleteDirectory( secondRoot );
		}
	}

	[Fact]
	public async Task AliasPublicationUsesParsedCanonicalIdentity() {
		string root = CreateTemporaryDirectory();

		try {
			TerminalDescription terminal = new TerminalDescriptionBuilder( "canonical" )
				.SetDescription( "Alias test" )
				.AddAlias( "alias-name" )
				.Build();
			Publish(
				root,
				terminal
			);

			CommandResult result = await RunAsync( root );

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Contains(
				"canonical\tAlias test",
				result.Stdout,
				StringComparison.Ordinal
			);
			Assert.DoesNotContain(
				"alias-name\t",
				result.Stdout,
				StringComparison.Ordinal
			);
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			DeleteDirectory( root );
		}
	}

	[Fact]
	public async Task MalformedEntryDoesNotHideSafeEntriesButReturnsFailure() {
		string root = CreateTemporaryDirectory();

		try {
			Publish(
				root,
				CreateTerminal(
					"good",
					"Good terminal"
				)
			);
			WriteCandidate(
				root,
				"b",
				"bad",
				[ 0x00, 0x01, 0x02 ]
			);

			CommandResult result = await RunAsync( root );

			Assert.Equal( CommandExitCodes.Failure, result.Status );
			Assert.Contains(
				"good\tGood terminal",
				result.Stdout,
				StringComparison.Ordinal
			);
			Assert.Contains( "TOE0004", result.Stderr, StringComparison.Ordinal );
			Assert.Contains(
				"MalformedEntry",
				result.Stderr,
				StringComparison.Ordinal
			);
		} finally {
			DeleteDirectory( root );
		}
	}

	[Fact]
	public async Task OversizedEntryIsDiagnosticAndReturnsFailure() {
		string root = CreateTemporaryDirectory();

		try {
			byte[] oversized = new byte[ CompiledTermInfoParserOptions.DefaultMaximumEntrySize + 1 ];
			WriteCandidate(
				root,
				"o",
				"oversized",
				oversized
			);

			CommandResult result = await RunAsync( root );

			Assert.Equal( CommandExitCodes.Failure, result.Status );
			Assert.Equal( string.Empty, result.Stdout );
			Assert.Contains( "TOE0004", result.Stderr, StringComparison.Ordinal );
			Assert.Contains(
				"MalformedEntry",
				result.Stderr,
				StringComparison.Ordinal
			);
		} finally {
			DeleteDirectory( root );
		}
	}

	[Fact]
	public async Task MissingExplicitRootReturnsOperationalFailure() {
		string parent = CreateTemporaryDirectory();
		string missing = System.IO.Path.Combine(
			parent,
			"missing"
		);

		try {
			CommandResult result = await RunAsync( missing );

			Assert.Equal( CommandExitCodes.Failure, result.Status );
			Assert.Equal( string.Empty, result.Stdout );
			Assert.Contains( "TOE0002", result.Stderr, StringComparison.Ordinal );
		} finally {
			DeleteDirectory( parent );
		}
	}

	[Fact]
	public async Task NonDirectoryStoreReturnsOperationalFailure() {
		string parent = CreateTemporaryDirectory();
		string file = System.IO.Path.Combine(
			parent,
			"terminfo.db"
		);
		System.IO.File.WriteAllText(
			file,
			"unsupported"
		);

		try {
			CommandResult result = await RunAsync( file );

			Assert.Equal( CommandExitCodes.Failure, result.Status );
			Assert.Equal( string.Empty, result.Stdout );
			Assert.Contains( "TOE0003", result.Stderr, StringComparison.Ordinal );
		} finally {
			DeleteDirectory( parent );
		}
	}

	[Fact]
	public async Task LinkedConventionalChildIsDiagnosticWhenLinksAreSupported() {
		string root = CreateTemporaryDirectory();
		string target = CreateTemporaryDirectory();
		string link = Path.Combine(
			root,
			"l"
		);

		try {
			try {
				Directory.CreateSymbolicLink(
					link,
					target
				);
			} catch ( PlatformNotSupportedException ) {
				return;
			} catch ( UnauthorizedAccessException ) {
				return;
			} catch ( IOException ) {
				return;
			}

			CommandResult result = await RunAsync( root );

			Assert.Equal( CommandExitCodes.Failure, result.Status );
			Assert.Contains( "TOE0004", result.Stderr, StringComparison.Ordinal );
			Assert.Contains( "LinkSkipped", result.Stderr, StringComparison.Ordinal );
		} finally {
			DeleteDirectory( root );
			DeleteDirectory( target );
		}
	}

	[Fact]
	public async Task UnavailableRootIsOperationalFailureWhenPermissionsCanBeRestricted() {
		if ( OperatingSystem.IsWindows() ) {
			return;
		}

		string root = CreateTemporaryDirectory();
		UnixFileMode originalMode = File.GetUnixFileMode( root );

		try {
			File.SetUnixFileMode(
				root,
				(UnixFileMode)0
			);

			CommandResult result = await RunAsync( root );
			if ( result.Status == CommandExitCodes.Success ) {
				return;
			}

			Assert.Equal( CommandExitCodes.Failure, result.Status );
			Assert.Contains( "TOE0004", result.Stderr, StringComparison.Ordinal );
		} finally {
			File.SetUnixFileMode(
				root,
				originalMode
			);
			DeleteDirectory( root );
		}
	}

	[Fact]
	public async Task CreationOrderDoesNotAffectListingOutput() {
		string firstRoot = CreateTemporaryDirectory();
		string secondRoot = CreateTemporaryDirectory();

		try {
			Publish(
				firstRoot,
				CreateTerminal(
					"zeta",
					"Zeta terminal"
				)
			);
			Publish(
				firstRoot,
				CreateTerminal(
					"alpha",
					"Alpha terminal"
				)
			);

			Publish(
				secondRoot,
				CreateTerminal(
					"alpha",
					"Alpha terminal"
				)
			);
			Publish(
				secondRoot,
				CreateTerminal(
					"zeta",
					"Zeta terminal"
				)
			);

			CommandResult first = await RunAsync( firstRoot );
			CommandResult second = await RunAsync( secondRoot );

			Assert.Equal( CommandExitCodes.Success, first.Status );
			Assert.Equal( CommandExitCodes.Success, second.Status );
			Assert.Equal( first.Stdout, second.Stdout );
			Assert.Equal( string.Empty, first.Stderr );
			Assert.Equal( string.Empty, second.Stderr );
		} finally {
			DeleteDirectory( firstRoot );
			DeleteDirectory( secondRoot );
		}
	}

	[Fact]
	public async Task DatabaseLocationModeUsesInspectionDiscovery() {
		CommandResult result = await RunAsync( "-D" );

		Assert.Equal( CommandExitCodes.Success, result.Status );
		Assert.Equal( string.Empty, result.Stderr );
	}

	private static TerminalDescription CreateTerminal(
		string name,
		string description
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( name );
		ArgumentException.ThrowIfNullOrWhiteSpace( description );

		return new TerminalDescriptionBuilder( name )
			.SetDescription( description )
			.Build();
	}

	private static void Publish(
		string root,
		TerminalDescription terminal
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		ArgumentNullException.ThrowIfNull( terminal );

		CompiledTermInfoDatabaseWriter.Write(
			root,
			terminal
		);
	}

	private static void WriteCandidate(
		string root,
		string directoryName,
		string fileName,
		byte[] bytes
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		ArgumentException.ThrowIfNullOrWhiteSpace( directoryName );
		ArgumentException.ThrowIfNullOrWhiteSpace( fileName );
		ArgumentNullException.ThrowIfNull( bytes );

		string directory = System.IO.Path.Combine(
			root,
			directoryName
		);
		System.IO.Directory.CreateDirectory( directory );
		System.IO.File.WriteAllBytes(
			System.IO.Path.Combine(
				directory,
				fileName
			),
			bytes
		);
	}

	private static async Task<CommandResult> RunAsync( params string[] args ) {
		ArgumentNullException.ThrowIfNull( args );

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
			ReadText( stdout ),
			ReadText( stderr )
		);
	}

	private static string CreateTemporaryDirectory() {
		string path = Path.Combine(
			System.IO.Path.GetTempPath(),
			"Icod.TermInfo.Toe.Tests",
			Guid.NewGuid().ToString( "N" )
		);
		Directory.CreateDirectory( path );
		return path;
	}

	private static void DeleteDirectory( string path ) {
		ArgumentException.ThrowIfNullOrWhiteSpace( path );

		try {
			if ( Directory.Exists( path ) ) {
				Directory.Delete(
					path,
					recursive: true
				);
			}
		} catch ( IOException ) {
		} catch ( UnauthorizedAccessException ) {
		}
	}

	private static string ReadText( MemoryStream stream ) {
		ArgumentNullException.ThrowIfNull( stream );

		return Encoding.UTF8.GetString(
			stream.ToArray()
		);
	}

	private sealed class EnvironmentVariableLease : IDisposable {
		private readonly string name;
		private readonly string? originalValue;
		private bool disposed;

		public EnvironmentVariableLease(
			string name,
			string? value
		) {
			ArgumentException.ThrowIfNullOrWhiteSpace( name );

			this.name = name;
			originalValue = Environment.GetEnvironmentVariable( name );
			Environment.SetEnvironmentVariable(
				name,
				value
			);
		}

		public void Dispose() {
			if ( disposed ) {
				return;
			}

			Environment.SetEnvironmentVariable(
				name,
				originalValue
			);
			disposed = true;
		}
	}

	private sealed class CommandResult {
		public CommandResult(
			int status,
			string stdout,
			string stderr
		) {
			ArgumentNullException.ThrowIfNull( stdout );
			ArgumentNullException.ThrowIfNull( stderr );

			Status = status;
			Stdout = stdout;
			Stderr = stderr;
		}

		public int Status { get; }

		public string Stdout { get; }

		public string Stderr { get; }
	}
}
