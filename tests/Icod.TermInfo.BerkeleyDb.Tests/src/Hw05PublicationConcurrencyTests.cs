/*
	Icod.TermInfo.BerkeleyDb.Tests
	Validates HW05 safe Hash-v9 filesystem publication.
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


using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hw05PublicationConcurrencyTests( Xunit.Abstractions.ITestOutputHelper output ) {
	[Fact]
	public async Task OverwritePublicationIsSerialized() => await RunContenders( true, false );

	[Fact]
	public async Task NonOverwriteHasExactlyOneWinner() => await RunContenders( false, false );

	[Fact]
	public async Task CaseEquivalentPathsShareLockOnInsensitiveFileSystem() {
		using var scope = new Hw05PublicationTestSupport();
		string probe = Path.Combine( scope.DirectoryPath, "CaseProbe" );
		File.WriteAllText( probe, "probe" );
		if ( !File.Exists( Path.Combine( scope.DirectoryPath, "CASEPROBE" ) ) ) {
			output.WriteLine( "Case-insensitive-path fixture unavailable: this test directory is case-sensitive." );
			return;
		}
		await RunContenders( true, true );
	}

	private static async Task RunContenders( bool overwrite, bool changeCase ) {
		using var scope = new Hw05PublicationTestSupport();
		using var staged = new ManualResetEventSlim();
		using var release = new ManualResetEventSlim();
		using var contended = new ManualResetEventSlim();
		using var cancellation = new CancellationTokenSource( TimeSpan.FromSeconds( 20 ) );
		var firstFs = new BlockingFileSystem( staged, release );
		var secondFs = new Hw05PublicationTestSupport.FileSystem { OnContention = () => contended.Set() };
		Task first = Task.Run( () => Write( scope.Destination, "first", overwrite, firstFs, cancellation.Token ) );
		Task? second = null;
		try {
			Assert.True( staged.Wait( TimeSpan.FromSeconds( 10 ) ) );
			string secondPath = ( changeCase )
				? Path.Combine( scope.DirectoryPath, "TERMINFO.DB" )
				: scope.Destination;
			second = Task.Run( () => Write( secondPath, "second", overwrite, secondFs, cancellation.Token ) );
			Assert.True( contended.Wait( TimeSpan.FromSeconds( 10 ) ) );
			Assert.False( second.IsCompleted );
		}
		finally {
			release.Set();
		}
		await first.WaitAsync( TimeSpan.FromSeconds( 10 ) );
		Assert.NotNull( second );
		if ( overwrite ) {
			await second.WaitAsync( TimeSpan.FromSeconds( 10 ) );
			Assert.Equal( Image( "second" ), File.ReadAllBytes( scope.Destination ) );
		}
		else {
			await Assert.ThrowsAsync<IOException>( async () => await second.WaitAsync( TimeSpan.FromSeconds( 10 ) ) );
			Assert.Equal( Image( "first" ), File.ReadAllBytes( scope.Destination ) );
		}
	}

	[Fact]
	public async Task DifferentDestinationsDoNotBlockEachOther() {
		using var scope = new Hw05PublicationTestSupport();
		using var staged = new ManualResetEventSlim();
		using var release = new ManualResetEventSlim();
		using var cancellation = new CancellationTokenSource( TimeSpan.FromSeconds( 20 ) );
		Task first = Task.Run( () => Write( scope.Destination, "first", true, new BlockingFileSystem( staged, release ), cancellation.Token ) );
		try {
			Assert.True( staged.Wait( TimeSpan.FromSeconds( 10 ) ) );
			string other = Path.Combine( scope.DirectoryPath, "other.db" );
			await Task.Run( () => Write( other, "other", true, new SystemBerkeleyDbDatabasePublicationFileSystem(), cancellation.Token ) )
				.WaitAsync( TimeSpan.FromSeconds( 10 ) );
			Assert.Equal( Image( "other" ), File.ReadAllBytes( other ) );
		}
		finally {
			release.Set();
			await first.WaitAsync( TimeSpan.FromSeconds( 10 ) );
		}
	}

	[Fact]
	public async Task ConcurrentReaderSeesOnlyCompleteImages() {
		using var scope = new Hw05PublicationTestSupport();
		byte[] oldImage = Image( "old" );
		byte[] newImage = Image( "new" );
		File.WriteAllBytes( scope.Destination, oldImage );
		using var started = new ManualResetEventSlim();
		using var stop = new CancellationTokenSource();
		int observations = 0;
		Task reader = Task.Run( () => {
			do {
				using var stream = new FileStream( scope.Destination, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete );
				using var copy = new MemoryStream();
				stream.CopyTo( copy );
				byte[] bytes = copy.ToArray();
				Assert.True( bytes.SequenceEqual( oldImage ) || bytes.SequenceEqual( newImage ) );
				Interlocked.Increment( ref observations );
				started.Set();
			} while ( !stop.IsCancellationRequested );
		} );
		try {
			Assert.True( started.Wait( TimeSpan.FromSeconds( 10 ) ) );
			for ( int index = 0; index < 12; index++ ) {
				Write( scope.Destination, ( index % 2 == 0 ) ? "new" : "old", true, new SystemBerkeleyDbDatabasePublicationFileSystem(), default );
			}
		}
		finally {
			stop.Cancel();
			await reader.WaitAsync( TimeSpan.FromSeconds( 10 ) );
		}
		Assert.True( observations > 0 );
	}

	[Fact]
	public void ReplacementUsesFreshMetadataAndLeavesUnrelatedArtifacts() {
		using var scope = new Hw05PublicationTestSupport();
		File.WriteAllBytes( scope.Destination, [ 1 ] );
		File.WriteAllBytes( scope.LockPath, [ 7, 7 ] );
		string unrelated = Path.Combine( scope.DirectoryPath, "unrelated.tmp" );
		File.WriteAllBytes( unrelated, [ 8, 8 ] );
		if ( !OperatingSystem.IsWindows() ) {
			File.SetUnixFileMode( scope.Destination, UnixFileMode.UserRead | UnixFileMode.UserWrite );
		}
		Write( scope.Destination, "new", true, new SystemBerkeleyDbDatabasePublicationFileSystem(), default );
		if ( !OperatingSystem.IsWindows() ) {
			Assert.Equal( File.GetUnixFileMode( unrelated ), File.GetUnixFileMode( scope.Destination ) );
		}
		Assert.Equal( new byte[] { 7, 7 }, File.ReadAllBytes( scope.LockPath ) );
		Assert.Equal( new byte[] { 8, 8 }, File.ReadAllBytes( unrelated ) );
	}

	[Fact]
	public void ReadOnlyWindowsDestinationFailsSafely() {
		if ( !OperatingSystem.IsWindows() ) {
			output.WriteLine( "Windows read-only-attribute fixture is not applicable on this host." );
			return;
		}
		using var scope = new Hw05PublicationTestSupport();
		File.WriteAllBytes( scope.Destination, [ 1, 2 ] );
		File.SetAttributes( scope.Destination, FileAttributes.ReadOnly );
		try {
			Assert.Throws<UnauthorizedAccessException>( () =>
				Write( scope.Destination, "new", true, new SystemBerkeleyDbDatabasePublicationFileSystem(), default ) );
			Assert.Equal( new byte[] { 1, 2 }, File.ReadAllBytes( scope.Destination ) );
		}
		finally {
			File.SetAttributes( scope.Destination, FileAttributes.Normal );
		}
	}

	[Fact]
	public void LongSidecarNameFailsWithoutDestinationMutation() {
		using var scope = new Hw05PublicationTestSupport();
		string path = Path.Combine( scope.DirectoryPath, new string( 'x', 240 ) );
		File.WriteAllBytes( path, [ 1, 2 ] );
		Assert.ThrowsAny<IOException>( () => Write( path, "new", true, new SystemBerkeleyDbDatabasePublicationFileSystem(), default ) );
		Assert.Equal( new byte[] { 1, 2 }, File.ReadAllBytes( path ) );
	}

	internal static void Write( string path, string name, bool overwrite, BerkeleyDbDatabasePublicationFileSystem fs, CancellationToken token ) {
		byte[] compiled = Icod.TermInfo.Tests.Shared.Hdb07HashV9FixtureBuilder.CreateCompiledEntry( name, "HW05", name + "-alias" );
		BerkeleyDbTerminalDatabaseWriter.WriteCore( path, [ new( name, [ name + "-alias" ], compiled ) ], new( overwriteExisting: overwrite ), token, fs );
	}
	private static byte[] Image( string name ) {
		byte[] compiled = Icod.TermInfo.Tests.Shared.Hdb07HashV9FixtureBuilder.CreateCompiledEntry( name, "HW05", name + "-alias" );
		var prepared = BerkeleyDbTerminalDatabaseWriter.PreparePublications( [ new( name, [ name + "-alias" ], compiled ) ], new(), default );
		return BerkeleyDbHashV9ImageBuilder.Build( BerkeleyDbNcursesRecordPlanner.CreateRecords( prepared, default ), 1_048_576, default );
	}
	private sealed class BlockingFileSystem( ManualResetEventSlim staged, ManualResetEventSlim release ) : Hw05PublicationTestSupport.FileSystem {
		internal override Stream CreateTemporary( string path ) {
			staged.Set();
			if ( !release.Wait( TimeSpan.FromSeconds( 15 ) ) ) {
				throw new TimeoutException( "The test did not release the staged writer." );
			}
			return base.CreateTemporary( path );
		}
	}
}
