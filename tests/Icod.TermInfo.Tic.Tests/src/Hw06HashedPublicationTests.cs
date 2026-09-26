using System.Text;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo.BerkeleyDb;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Tic.Tests;

public sealed class Hw06HashedPublicationTests : IDisposable {
	private const string Source = "one|one-alias|First terminal,am,cols#80,\ntwo|two-alias|Second terminal,cols#132,\n";
	private readonly string _root = System.IO.Path.Combine( System.IO.Path.GetTempPath(), "icod-hw06-" + Guid.NewGuid().ToString( "N" ) );

	public Hw06HashedPublicationTests() => Directory.CreateDirectory( _root );

	[Fact]
	public async Task HashedPublicationUsesExactFileAndMatchesDirectorySemantics() {
		string database = System.IO.Path.Combine( _root, "exact-name" );
		string directory = System.IO.Path.Combine( _root, "directory.db" );
		var result = await RunAsync( [ "--database-format", "hashed", "-o", database, "-" ] );
		Assert.Equal( CommandExitCodes.Success, result.Status );
		Assert.Empty( result.Output );
		Assert.Empty( result.Error );
		Assert.True( File.Exists( database ) );
		Assert.False( File.Exists( database + ".db" ) );
		Assert.Equal( CommandExitCodes.Success, (await RunAsync( [ "--database-format", "directory", "-o", directory, "-" ] )).Status );
		var hashed = new BerkeleyDbTerminalDescriptionProvider( database );
		var conventional = new DirectoryTerminalDescriptionProvider( directory );
		foreach ( string name in new[] { "one", "one-alias", "two", "two-alias" } ) {
			Assert.True( hashed.TryLoad( name, out var actual ) );
			Assert.True( conventional.TryLoad( name, out var expected ) );
			Assert.True( TerminalDescriptionComparer.Compare( expected!, actual! ).AreEqual );
		}
	}

	[Fact]
	public async Task AliasSelectionAndSummaryDescribeOnlyPublishedKeys() {
		string database = System.IO.Path.Combine( _root, "selected" );
		var result = await RunAsync( [ "--database-format", "hashed", "-e", "two,two-alias", "-s", "-o", database, "-" ] );
		Assert.Equal( CommandExitCodes.Success, result.Status );
		Assert.Empty( result.Output );
		Assert.Contains( "tic: format: hashed", result.Error );
		Assert.Contains( "tic: output: " + database, result.Error );
		Assert.Contains( "tic: compiled entries: 1", result.Error );
		Assert.Contains( "tic: alias keys: 1", result.Error );
		Assert.Contains( "tic: warnings: 0", result.Error );
		var provider = new BerkeleyDbTerminalDescriptionProvider( database );
		Assert.False( provider.TryLoad( "one", out _ ) );
		Assert.True( provider.TryLoad( "two-alias", out var description ) );
		Assert.Equal( "two", description!.Name );
	}

	[Fact]
	public async Task ExistingFileIsPreservedUnlessForceReplacesWholeDatabase() {
		string database = System.IO.Path.Combine( _root, "replace" );
		byte[] sentinel = [ 1, 2, 3 ];
		File.WriteAllBytes( database, sentinel );
		var refused = await RunAsync( [ "--database-format", "hashed", "-o", database, "-" ] );
		Assert.Equal( CommandExitCodes.Failure, refused.Status );
		Assert.Contains( "TIC0007", refused.Error );
		Assert.Equal( sentinel, File.ReadAllBytes( database ) );
		var replaced = await RunAsync( [ "--database-format", "hashed", "--force", "-e", "two", "-o", database, "-" ] );
		Assert.Equal( CommandExitCodes.Success, replaced.Status );
		var provider = new BerkeleyDbTerminalDescriptionProvider( database );
		Assert.True( provider.TryLoad( "two", out _ ) );
		Assert.False( provider.TryLoad( "one", out _ ) );
		Assert.Empty( Directory.GetFiles( _root, "*.tmp" ) );
	}

	[Fact]
	public async Task HashedOutputIsDeterministicAcrossPaths() {
		string first = System.IO.Path.Combine( _root, "first" );
		string second = System.IO.Path.Combine( _root, "second" );
		Assert.Equal( CommandExitCodes.Success, (await RunAsync( [ "--database-format", "hashed", "-o", first, "-" ] )).Status );
		Assert.Equal( CommandExitCodes.Success, (await RunAsync( [ "--database-format", "hashed", "-o", second, "-" ] )).Status );
		Assert.Equal( File.ReadAllBytes( first ), File.ReadAllBytes( second ) );
	}

	[Theory]
	[InlineData( "unknown", "requires 'directory' or 'hashed'" )]
	[InlineData( "HASHED", "requires 'directory' or 'hashed'" )]
	[InlineData( "", "requires 'directory' or 'hashed'" )]
	[InlineData( "-s", "requires 'directory' or 'hashed'" )]
	public async Task InvalidFormatIsControlledUsageError( string format, string message ) {
		var result = await RunAsync( [ "--database-format", format, "-o", System.IO.Path.Combine( _root, "invalid" ), "-" ] );
		Assert.Equal( CommandExitCodes.UsageError, result.Status );
		Assert.Contains( message, result.Error );
		Assert.Empty( Directory.GetFileSystemEntries( _root ) );
	}

	[Fact]
	public async Task MissingFormatValueIsControlledUsageError() {
		var result = await RunAsync( [ "--database-format" ] );
		Assert.Equal( CommandExitCodes.UsageError, result.Status );
		Assert.Contains( "requires 'directory' or 'hashed'", result.Error );
	}

	[Fact]
	public async Task HashedFormatRequiresExplicitDestinationBeforeReadingSource() {
		var result = await RunAsync( [ "--database-format", "hashed", System.IO.Path.Combine( _root, "absent-source" ) ] );
		Assert.Equal( CommandExitCodes.UsageError, result.Status );
		Assert.Contains( "requires an explicit '-o'", result.Error );
		Assert.DoesNotContain( "TIC0001", result.Error );
		Assert.Empty( Directory.GetFileSystemEntries( _root ) );
	}

	[Theory]
	[InlineData( "directory" )]
	[InlineData( "hashed" )]
	public async Task CheckOnlyRejectsExplicitFormatWithoutArtifacts( string format ) {
		var result = await RunAsync( [ "-c", "--database-format", format, "-" ] );
		Assert.Equal( CommandExitCodes.UsageError, result.Status );
		Assert.Contains( "check-only", result.Error );
		Assert.Empty( Directory.GetFileSystemEntries( _root ) );
	}

	[Fact]
	public async Task DuplicateFormatIsRejected() {
		var result = await RunAsync( [ "--database-format", "directory", "--database-format", "hashed", "-" ] );
		Assert.Equal( CommandExitCodes.UsageError, result.Status );
		Assert.Contains( "only once", result.Error );
	}

	[Fact]
	public async Task MissingParentFailsWithoutCreatingDirectoriesOrLeakingTemporaryPath() {
		var result = await RunAsync( [ "--database-format", "hashed", "-o", System.IO.Path.Combine( _root, "absent", "store" ), "-" ] );
		Assert.Equal( CommandExitCodes.Failure, result.Status );
		Assert.Contains( "TIC0007", result.Error );
		Assert.DoesNotContain( "Exception", result.Error );
		Assert.DoesNotContain( _root, result.Error );
		Assert.Empty( Directory.GetFileSystemEntries( _root ) );
	}

	[Fact]
	public async Task InvalidSourceLeavesNoPublicationArtifacts() {
		var result = await RunAsync( [ "--database-format", "hashed", "-o", System.IO.Path.Combine( _root, "invalid" ), "-" ], "bad|Bad terminal,use=missing,\n" );
		Assert.Equal( CommandExitCodes.Failure, result.Status );
		Assert.Empty( Directory.GetFileSystemEntries( _root ) );
	}

	[Fact]
	public async Task PreCanceledHashedPublicationLeavesNoArtifacts() {
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		var result = await RunAsync( [ "--database-format", "hashed", "-o", System.IO.Path.Combine( _root, "canceled" ), "-" ], cancellationToken: cancellation.Token );
		Assert.Equal( CommandExitCodes.Canceled, result.Status );
		Assert.Empty( Directory.GetFileSystemEntries( _root ) );
	}

	[Fact]
	public async Task CancellationDuringSummaryDoesNotReportCommittedDatabaseAsCanceled() {
		string database = System.IO.Path.Combine( _root, "committed" );
		using var cancellation = new CancellationTokenSource();
		using var stdin = new MemoryStream( Encoding.UTF8.GetBytes( Source ) );
		using var stdout = new MemoryStream();
		using var stderr = new CancelOnWriteStream( cancellation );
		int status = await Command.RunAsync(
			[ "--database-format", "hashed", "-s", "-o", database, "-" ],
			stdin, stdout, stderr, cancellation.Token
		);
		Assert.True( cancellation.IsCancellationRequested );
		Assert.Equal( CommandExitCodes.Success, status );
		Assert.Contains( "tic: alias keys: 2", Encoding.UTF8.GetString( stderr.ToArray() ) );
		Assert.True( new BerkeleyDbTerminalDescriptionProvider( database ).TryLoad( "one-alias", out _ ) );
	}

	[Fact]
	public async Task ContendedHashedLockCanBeCanceledWithoutChangingDestination() {
		string database = System.IO.Path.Combine( _root, "locked" );
		byte[] sentinel = [ 3, 2, 1 ];
		File.WriteAllBytes( database, sentinel );
		using var heldLock = new FileStream(
			System.IO.Path.Combine( _root, ".locked.icod-terminfo.lock" ),
			FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None
		);
		using var cancellation = new CancellationTokenSource( TimeSpan.FromMilliseconds( 250 ) );
		var result = await RunAsync(
			[ "--database-format", "hashed", "--force", "-o", database, "-" ],
			cancellationToken: cancellation.Token
		);
		Assert.Equal( CommandExitCodes.Canceled, result.Status );
		Assert.Equal( sentinel, File.ReadAllBytes( database ) );
		Assert.Empty( Directory.GetFiles( _root, "*.tmp" ) );
	}

	private sealed class CancelOnWriteStream( CancellationTokenSource cancellation ) : MemoryStream {
		public override ValueTask WriteAsync( ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default ) {
			cancellation.Cancel();
			return base.WriteAsync( buffer, cancellationToken );
		}
	}

	private static async Task<(int Status, string Output, string Error)> RunAsync(
		string[] args, string source = Source, CancellationToken cancellationToken = default
	) {
		using var stdin = new MemoryStream( Encoding.UTF8.GetBytes( source ) );
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();
		int status = await Command.RunAsync( args, stdin, stdout, stderr, cancellationToken );
		return (status, Encoding.UTF8.GetString( stdout.ToArray() ), Encoding.UTF8.GetString( stderr.ToArray() ));
	}

	public void Dispose() => Directory.Delete( _root, recursive: true );
}
