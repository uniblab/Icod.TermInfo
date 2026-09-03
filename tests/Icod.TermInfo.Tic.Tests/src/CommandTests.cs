using System.Text;
using Icod.CommandFramework.Diagnostics;
using Xunit;

namespace Icod.TermInfo.Tic.Tests;

public sealed class CommandTests {
	private const string SimpleSource =
		"demo|demo-alias|Demo terminal,\n"
		+ "    am,\n"
		+ "    cols#80,\n";

	[Fact]
	public async Task HelpWritesStdoutAndReturnsSuccess() {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "--help" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.Success, status );
		Assert.Contains(
			"Usage: tic",
			ReadText( stdout )
		);
		Assert.Contains( "-e name,...", ReadText( stdout ) );
		Assert.Contains( "-x", ReadText( stdout ) );
		Assert.Contains( "-o directory", ReadText( stdout ) );
		Assert.Contains( "--force", ReadText( stdout ) );
		Assert.Contains( "-D", ReadText( stdout ) );
		Assert.Empty( ReadText( stderr ) );
	}

	[Theory]
	[InlineData( "--version" )]
	[InlineData( "-V" )]
	public async Task VersionReportsCoordinatedDevelopmentVersion(
		string argument
	) {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ argument ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.Success, status );
		Assert.Contains( "1.10.0", ReadText( stdout ) );
		Assert.Empty( ReadText( stderr ) );
	}

	[Fact]
	public async Task DatabaseLocationsReturnSuccessWithoutSourceOperand() {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-D" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.Success, status );
		Assert.Empty( ReadText( stderr ) );
	}

	[Fact]
	public async Task UnknownOptionWritesStderrAndReturnsUsageError() {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "--not-a-t05-option" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.UsageError, status );
		Assert.Empty( ReadText( stdout ) );
		Assert.Contains( "unsupported option", ReadText( stderr ) );
	}

	[Fact]
	public async Task ValidationRequiresExactlyOneSourceOperand() {
		foreach (
			string[] arguments
			in new[] {
				new[] { "-c" },
				new[] { "-c", "first.ti", "second.ti" },
			}
		) {
			using var stdin = new MemoryStream();
			using var stdout = new MemoryStream();
			using var stderr = new MemoryStream();

			int status = await Command.RunAsync(
				arguments,
				stdin,
				stdout,
				stderr
			);

			Assert.Equal( CommandExitCodes.UsageError, status );
			Assert.Contains( "exactly one source operand", ReadText( stderr ) );
		}
	}

	[Fact]
	public async Task CheckOnlyValidatesStandardInput() {
		using MemoryStream stdin = CreateInput( SimpleSource );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-c", "-" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.Success, status );
		Assert.Empty( ReadText( stdout ) );
		Assert.Empty( ReadText( stderr ) );
	}

	[Fact]
	public async Task CheckOnlyValidatesUtf8File() {
		string root = CreateTemporaryDirectory();
		try {
			string sourcePath =
				System.IO.Path.Combine(
					root,
					"input.ti"
				);
			await File.WriteAllTextAsync(
				sourcePath,
				SimpleSource,
				new UTF8Encoding( false )
			);
			using var stdin = new MemoryStream();
			using var stdout = new MemoryStream();
			using var stderr = new MemoryStream();

			int status = await Command.RunAsync(
				[ "-c", sourcePath ],
				stdin,
				stdout,
				stderr
			);

			Assert.Equal( CommandExitCodes.Success, status );
			Assert.Empty( ReadText( stdout ) );
			Assert.Empty( ReadText( stderr ) );
		}
		finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task CheckOnlyValidatesEveryEntryWhenNoSelectionIsProvided() {
		const string source =
			"one|One terminal,am,\n"
			+ "two|Two terminal,cols#80,\n"
			+ "three|Three terminal,lines#24,\n";
		using MemoryStream stdin = CreateInput( source );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-c", "-" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.Success, status );
		Assert.Empty( ReadText( stdout ) );
		Assert.Empty( ReadText( stderr ) );
	}

	[Fact]
	public async Task CheckOnlyReportsMissingInputFileAsOperationalFailure() {
		string root = CreateTemporaryDirectory();
		try {
			string sourcePath =
				System.IO.Path.Combine(
					root,
					"missing.ti"
				);
			using var stdin = new MemoryStream();
			using var stdout = new MemoryStream();
			using var stderr = new MemoryStream();

			int status = await Command.RunAsync(
				[ "-c", sourcePath ],
				stdin,
				stdout,
				stderr
			);

			Assert.Equal( CommandExitCodes.Failure, status );
			Assert.Contains( "TIC0005 error", ReadText( stderr ) );
			Assert.Contains( sourcePath, ReadText( stderr ) );
		}
		finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task CheckOnlyRejectsEmptyInput() {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-c", "-" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.Failure, status );
		Assert.Contains( "TIC0001 error", ReadText( stderr ) );
	}

	[Fact]
	public async Task CheckOnlyAcceptsUtf8Bom() {
		byte[] preamble =
			new UTF8Encoding( true ).GetPreamble();
		byte[] source =
			new UTF8Encoding( false ).GetBytes( SimpleSource );
		using var stdin = new MemoryStream(
			[ .. preamble, .. source ]
		);
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-c", "-" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.Success, status );
		Assert.Empty( ReadText( stderr ) );
	}

	[Fact]
	public async Task CheckOnlyRejectsUtf16BomInput() {
		byte[] preamble = Encoding.Unicode.GetPreamble();
		byte[] source = Encoding.Unicode.GetBytes( SimpleSource );
		using var stdin = new MemoryStream(
			[ .. preamble, .. source ]
		);
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-c", "-" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.Failure, status );
		Assert.Contains( "TIC0005 error", ReadText( stderr ) );
		Assert.Contains( "valid UTF-8", ReadText( stderr ) );
	}

	[Fact]
	public async Task CheckOnlyRejectsMalformedUtf8() {
		using var stdin = new MemoryStream(
			[ 0x66, 0x6f, 0x80, 0x6f ]
		);
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-c", "-" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.Failure, status );
		Assert.Contains( "TIC0005 error", ReadText( stderr ) );
		Assert.Contains( "valid UTF-8", ReadText( stderr ) );
	}

	[Fact]
	public async Task CheckOnlyPreservesSourceErrorsAndLocations() {
		const string source =
			"broken|Broken terminal,\n"
			+ "    cols#not-a-number,\n";
		using MemoryStream stdin = CreateInput( source );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-c", "-" ],
			stdin,
			stdout,
			stderr
		);
		string diagnostic = ReadText( stderr );

		Assert.Equal( CommandExitCodes.Failure, status );
		Assert.Contains( "<stdin>:2:", diagnostic );
		Assert.Contains( "TIS0011 error", diagnostic );
	}

	[Fact]
	public async Task SelectionDoesNotHideWholeDocumentParseErrors() {
		const string source =
			"good|Good terminal,am,\n"
			+ "broken|Broken terminal,cols#not-a-number,\n";
		using MemoryStream stdin = CreateInput( source );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-c", "-e", "good", "-" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.Failure, status );
		Assert.Contains( "TIS0011 error", ReadText( stderr ) );
	}

	[Fact]
	public async Task CheckOnlyReturnsSuccessForWarningsOnlySource() {
		const string source =
			"one|shared|One terminal,am,\n"
			+ "two|shared|Two terminal,am,\n";
		using MemoryStream stdin = CreateInput( source );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-c", "-e", "one", "-" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.Success, status );
		Assert.Contains( "TIS0026 warning", ReadText( stderr ) );
	}

	[Fact]
	public async Task CheckOnlyResolvesUseReferences() {
		const string source =
			"parent|Parent terminal,am,cols#80,\n"
			+ "child|Child terminal,use=parent,\n";
		using MemoryStream stdin = CreateInput( source );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-c", "-e", "child", "-" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.Success, status );
		Assert.Empty( ReadText( stderr ) );
	}

	[Fact]
	public async Task CheckOnlyReportsMissingUseReference() {
		const string source =
			"child|Child terminal,use=missing-parent,\n";
		using MemoryStream stdin = CreateInput( source );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-c", "-" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.Failure, status );
		Assert.Contains( "TIS0022 error", ReadText( stderr ) );
	}

	[Fact]
	public async Task CheckOnlyReportsInheritanceCycle() {
		const string source =
			"cycle-a|Cycle A,use=cycle-b,\n"
			+ "cycle-b|Cycle B,use=cycle-a,\n";
		using MemoryStream stdin = CreateInput( source );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-c", "-e", "cycle-a", "-" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.Failure, status );
		Assert.Contains( "TIS0023 error", ReadText( stderr ) );
	}

	[Fact]
	public async Task EntrySelectionAcceptsAliasesAndKeepsUnselectedResolutionErrorsOut() {
		const string source =
			"good|good-alias|Good terminal,am,\n"
			+ "bad|Bad terminal,VendorFeature,use=missing-parent,\n";
		using MemoryStream stdin = CreateInput( source );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-c", "-e", "good-alias", "-" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.Success, status );
		Assert.Empty( ReadText( stderr ) );
	}

	[Fact]
	public async Task DuplicateAliasSelectionUsesFirstSourceIdentityDeterministically() {
		const string source =
			"first|shared|First terminal,am,\n"
			+ "second|shared|Second terminal,use=missing-parent,\n";
		using MemoryStream stdin = CreateInput( source );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-c", "-e", "shared", "-" ],
			stdin,
			stdout,
			stderr
		);
		string diagnostic = ReadText( stderr );

		Assert.Equal( CommandExitCodes.Success, status );
		Assert.Contains( "TIS0026 warning", diagnostic );
		Assert.DoesNotContain( "TIS0022", diagnostic );
	}

	[Fact]
	public async Task MissingSelectedEntryIsAnError() {
		using MemoryStream stdin = CreateInput( SimpleSource );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-c", "-e", "missing", "-" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.Failure, status );
		Assert.Contains( "TIC0002 error", ReadText( stderr ) );
	}

	[Fact]
	public async Task KnownExtendedCapabilityIsAcceptedWithoutX() {
		const string source =
			"known|Known extension,AX,\n";
		using MemoryStream stdin = CreateInput( source );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-c", "-" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.Success, status );
		Assert.Empty( ReadText( stderr ) );
	}

	[Fact]
	public async Task UnknownExtendedCapabilityRequiresX() {
		const string source =
			"unknown|Unknown extension,VendorFeature,\n";
		using MemoryStream stdin = CreateInput( source );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-c", "-" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.Failure, status );
		Assert.Contains( "TIC0003 error", ReadText( stderr ) );
		Assert.Contains( "requires -x", ReadText( stderr ) );
	}

	[Fact]
	public async Task XAllowsUnknownExtendedCapability() {
		const string source =
			"unknown|Unknown extension,VendorFeature,\n";
		using MemoryStream stdin = CreateInput( source );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-c", "-x", "-" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.Success, status );
		Assert.Empty( ReadText( stderr ) );
	}

	[Fact]
	public async Task UnknownExtendedCapabilityInheritedBySelectionRequiresX() {
		const string source =
			"parent|Parent extension,VendorFeature,\n"
			+ "child|Child terminal,use=parent,\n";
		using MemoryStream stdin = CreateInput( source );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-c", "-e", "child", "-" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.Failure, status );
		Assert.Contains( "VendorFeature", ReadText( stderr ) );
	}

	[Fact]
	public async Task CheckOnlyReportsCompiledRepresentationFailureWithoutPublishing() {
		const string source =
			"unicode|Unicode \u2603 description,am,\n";
		using MemoryStream stdin = CreateInput( source );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-c", "-" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.Failure, status );
		Assert.Contains( "TIC0004 error", ReadText( stderr ) );
		Assert.Contains( "cannot be represented", ReadText( stderr ) );
	}

	[Fact]
	public async Task CheckOnlyDoesNotPublishDatabaseFiles() {
		string root = CreateTemporaryDirectory();
		try {
			string sourcePath =
				System.IO.Path.Combine(
					root,
					"input.ti"
				);
			await File.WriteAllTextAsync(
				sourcePath,
				SimpleSource,
				new UTF8Encoding( false )
			);
			string[] before = Directory.GetFileSystemEntries( root );
			using var stdin = new MemoryStream();
			using var stdout = new MemoryStream();
			using var stderr = new MemoryStream();

			int status = await Command.RunAsync(
				[ "-c", sourcePath ],
				stdin,
				stdout,
				stderr
			);
			string[] after = Directory.GetFileSystemEntries( root );

			Assert.Equal( CommandExitCodes.Success, status );
			Assert.Equal( before, after );
			Assert.Equal( sourcePath, Assert.Single( after ) );
		}
		finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task CancellationReturnsCanceledWithoutOutput() {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();

		int status = await Command.RunAsync(
			[],
			stdin,
			stdout,
			stderr,
			cancellation.Token
		);

		Assert.Equal( CommandExitCodes.Canceled, status );
		Assert.Empty( ReadText( stdout ) );
		Assert.Empty( ReadText( stderr ) );
	}

	[Fact]
	public async Task CommandLeavesCallerOwnedStreamsOpen() {
		using MemoryStream stdin = CreateInput( SimpleSource );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		_ = await Command.RunAsync(
			[ "-c", "-" ],
			stdin,
			stdout,
			stderr
		);

		stdout.WriteByte( 0 );
		stderr.WriteByte( 0 );
		Assert.True( stdin.CanRead );
		Assert.True( stdout.CanWrite );
		Assert.True( stderr.CanWrite );
	}

	[Fact]
	public async Task NullArgumentsAreRejectedBeforeExecution() {
		using var stream = new MemoryStream();

		await Assert.ThrowsAsync<ArgumentNullException>(
			() => Command.RunAsync(
				null!,
				stream,
				stream,
				stream
			)
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
				"icod-terminfo-tic-t04-" + Guid.NewGuid().ToString( "N" )
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
