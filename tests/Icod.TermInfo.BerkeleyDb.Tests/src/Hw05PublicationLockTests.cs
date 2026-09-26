/*
	Icod.TermInfo.BerkeleyDb.Tests
	Validates the HW04 public Hash-v9 publication engine.
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

public sealed class Hw05PublicationLockTests {
	[Fact]
	public async Task SecondHolderWaitsUntilRelease() {
		using var scope = new Hw05PublicationTestSupport();
		IDisposable first = scope.Acquire();
		using var attempted = new ManualResetEventSlim();
		using var cancellation = new CancellationTokenSource( TimeSpan.FromSeconds( 10 ) );
		var fs = new Hw05PublicationTestSupport.FileSystem {
			OnContention = () => attempted.Set()
		};
		Task<IDisposable> second = Task.Run( () => scope.Acquire( fs, cancellation.Token ) );
		try {
			Assert.True( attempted.Wait( TimeSpan.FromSeconds( 5 ) ) );
			Assert.False( second.IsCompleted );
		}
		finally {
			first.Dispose();
		}
		using IDisposable acquired = await second.WaitAsync( TimeSpan.FromSeconds( 5 ) );
	}

	[Fact]
	public async Task CancelledWaitReleasesResources() {
		using var scope = new Hw05PublicationTestSupport();
		using IDisposable first = scope.Acquire();
		using var cancellation = new CancellationTokenSource( TimeSpan.FromSeconds( 10 ) );
		var fs = new Hw05PublicationTestSupport.FileSystem { OnContention = cancellation.Cancel };
		await Assert.ThrowsAnyAsync<OperationCanceledException>(
			() => Task.Run( () => scope.Acquire( fs, cancellation.Token ) )
		);
		Assert.False( File.Exists( scope.Destination ) );
	}

	[Fact]
	public void NonContentionFailureIsNotRetried() {
		using var scope = new Hw05PublicationTestSupport();
		var error = new IOException( "disk fault" );
		var fs = new Hw05PublicationTestSupport.FileSystem { LockError = error };
		Assert.Same( error, Assert.Throws<IOException>( () => scope.Acquire( fs ) ) );
		Assert.Equal( 1, fs.LockAttempts );
	}

	[Fact]
	public void UnenforcedLockFailsBeforeStaging() {
		using var scope = new Hw05PublicationTestSupport();
		var fs = new Hw05PublicationTestSupport.FileSystem { IgnoreLocking = true };
		Assert.Throws<NotSupportedException>( () => scope.Acquire( fs ) );
		Assert.False( File.Exists( scope.Destination ) );
		Assert.All( fs.UnlockedStreams, stream => Assert.False( stream.CanRead ) );
	}

	[Fact]
	public void ExistingLockIsNeverTruncatedOrDeleted() {
		using var scope = new Hw05PublicationTestSupport();
		File.WriteAllBytes( scope.LockPath, [ 3, 1, 4 ] );
		using ( scope.Acquire() ) {
			Assert.True( File.Exists( scope.LockPath ) );
		}
		Assert.Equal( new byte[] { 3, 1, 4 }, File.ReadAllBytes( scope.LockPath ) );
		using IDisposable again = scope.Acquire();
	}

	[Theory]
	[InlineData( "missing-parent" )]
	[InlineData( "destination-directory" )]
	[InlineData( "lock-directory" )]
	public void UnsafePathFailsWithoutPublication( string shape ) {
		using var scope = new Hw05PublicationTestSupport();
		if ( shape == "missing-parent" ) {
			Directory.Delete( scope.DirectoryPath );
		}
		else if ( shape == "destination-directory" ) {
			Directory.CreateDirectory( scope.Destination );
		}
		else {
			Directory.CreateDirectory( scope.LockPath );
		}
		Assert.ThrowsAny<IOException>( () => scope.Acquire() );
		Assert.False( File.Exists( scope.Destination ) );
	}

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void LiveAndDanglingLinksAreRejected( bool lockLink, bool dangling ) {
		using var scope = new Hw05PublicationTestSupport();
		string target = Path.Combine( scope.DirectoryPath, "target" );
		if ( !dangling ) {
			File.WriteAllBytes( target, [ 4, 2 ] );
		}
		string link = ( lockLink ) ? scope.LockPath : scope.Destination;
		File.CreateSymbolicLink( link, target );
		Assert.Throws<IOException>( () => scope.Acquire() );
		Assert.Equal( !dangling, File.Exists( target ) );
		if ( !dangling ) {
			Assert.Equal( new byte[] { 4, 2 }, File.ReadAllBytes( target ) );
		}
	}

	[Fact]
	public void LinkedImmediateParentIsRejected() {
		using var scope = new Hw05PublicationTestSupport();
		string real = Path.Combine( scope.DirectoryPath, "real" );
		string link = Path.Combine( scope.DirectoryPath, "link" );
		Directory.CreateDirectory( real );
		Directory.CreateSymbolicLink( link, real );
		Assert.Throws<IOException>( () => BerkeleyDbDatabasePublicationLock.Acquire(
			Path.Combine( link, "db" ), Path.Combine( link, ".db.icod-terminfo.lock" ),
			true, new SystemBerkeleyDbDatabasePublicationFileSystem(), CancellationToken.None
		) );
		Assert.Empty( Directory.GetFileSystemEntries( real ) );
	}

	[Fact]
	public void PermissionDenialIsNotRetried() {
		using var scope = new Hw05PublicationTestSupport();
		var error = new UnauthorizedAccessException( "denied" );
		var fs = new Hw05PublicationTestSupport.FileSystem { LockError = error };
		Assert.Same( error, Assert.Throws<UnauthorizedAccessException>( () => scope.Acquire( fs ) ) );
		Assert.Equal( 1, fs.LockAttempts );
	}
}
