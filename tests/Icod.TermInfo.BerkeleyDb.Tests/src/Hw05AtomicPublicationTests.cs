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

public sealed class Hw05AtomicPublicationTests {
	public static TheoryData<string, bool> Failures {
		get {
			var data = new TheoryData<string, bool>();
			foreach ( string stage in new[] { "create", "write", "flush", "close", "reopen", "read", "bytes", "records", "catalog", "validate", "move" } ) {
				data.Add( stage, true );
				data.Add( stage, false );
			}
			return data;
		}
	}

	[Theory]
	[MemberData( nameof( Failures ) )]
	public void PreCommitFailurePreservesDestination( string stage, bool exists ) {
		using var scope = new Hw05PublicationTestSupport();
		byte[] prior = [ 8, 6, 7, 5, 3, 0, 9 ];
		if ( exists ) {
			File.WriteAllBytes( scope.Destination, prior );
		}
		var fs = new FaultFileSystem( stage );
		var data = Hw05PublicationVerificationTests.Prepare();
		var records = ( stage == "records" ) ? data.Records.Skip( 1 ).ToArray() : data.Records;
		var publications = ( stage == "catalog" )
			? new[] { data.Publications[0] with { Aliases = [ new( "wrong", [ 1 ] ) ] } }
			: data.Publications;
		Assert.ThrowsAny<IOException>( () => BerkeleyDbDatabasePublisher.Publish(
			scope.Destination, data.Image, records, publications, new( overwriteExisting: true ), fs, default
		) );
		if ( exists ) {
			Assert.Equal( prior, File.ReadAllBytes( scope.Destination ) );
		}
		else {
			Assert.False( File.Exists( scope.Destination ) );
		}
		Assert.Empty( Directory.GetFiles( scope.DirectoryPath, "*.tmp" ) );
		Assert.True( File.Exists( scope.LockPath ) );
		using var reacquired = scope.Acquire();
	}

	[Fact]
	public void CreateNewCollisionNeverDeletesForeignArtifact() {
		using var scope = new Hw05PublicationTestSupport();
		var fs = new FaultFileSystem( "collision" );
		Assert.Throws<IOException>( () => Publish( scope, fs ) );
		Assert.NotNull( fs.TemporaryPath );
		Assert.Equal( new byte[] { 4, 2 }, File.ReadAllBytes( fs.TemporaryPath ) );
		Assert.Equal( 0, fs.DeleteCalls );
	}

	[Fact]
	public void WriteFailureSurvivesCloseAndCleanupFailures() {
		using var scope = new Hw05PublicationTestSupport();
		var fs = new FaultFileSystem( "compound" );
		var error = Assert.Throws<IOException>( () => Publish( scope, fs ) );
		Assert.Same( fs.Failure, error );
		Assert.Equal( 1, fs.DeleteCalls );
		using var reacquired = scope.Acquire();
	}

	[Theory]
	[InlineData( "write" )]
	[InlineData( "flush" )]
	[InlineData( "reopen" )]
	[InlineData( "read" )]
	[InlineData( "validate" )]
	public void CancellationBeforeCommitPreservesDestination( string stage ) {
		using var scope = new Hw05PublicationTestSupport();
		File.WriteAllBytes( scope.Destination, [ 9, 8 ] );
		using var cancellation = new CancellationTokenSource();
		var fs = new FaultFileSystem( stage ) { Cancellation = cancellation };
		Assert.Throws<OperationCanceledException>( () => Publish( scope, fs, cancellation.Token ) );
		Assert.Equal( new byte[] { 9, 8 }, File.ReadAllBytes( scope.Destination ) );
		Assert.Empty( Directory.GetFiles( scope.DirectoryPath, "*.tmp" ) );
	}

	[Fact]
	public void CancellationInsideCommitDoesNotTurnSuccessIntoFailure() {
		using var scope = new Hw05PublicationTestSupport();
		using var cancellation = new CancellationTokenSource();
		var fs = new FaultFileSystem( "move" ) { Cancellation = cancellation, RecreateAfterMove = true };
		Publish( scope, fs, cancellation.Token );
		Assert.True( cancellation.IsCancellationRequested );
		Assert.Equal( Hw05PublicationVerificationTests.Prepare().Image, File.ReadAllBytes( scope.Destination ) );
		Assert.Equal( new byte[] { 4, 2 }, File.ReadAllBytes( fs.TemporaryPath! ) );
		Assert.Equal( 0, fs.DeleteCalls );
	}

	[Fact]
	public void PublicWriterUsesVerifiedPublication() {
		using var scope = new Hw05PublicationTestSupport();
		var data = Hw05PublicationVerificationTests.Prepare();
		File.WriteAllBytes( scope.Destination, [ 9, 8 ] );
		Assert.Throws<InvalidDataException>( () => BerkeleyDbTerminalDatabaseWriter.WriteCore(
			scope.Destination,
			[ new( "hw05-primary", [ "hw05-alias" ], data.Publications[0].Data ) ],
			new( overwriteExisting: true ), default, new FaultFileSystem( "bytes" )
		) );
		Assert.Equal( new byte[] { 9, 8 }, File.ReadAllBytes( scope.Destination ) );
	}

	internal static void Publish( Hw05PublicationTestSupport scope, BerkeleyDbDatabasePublicationFileSystem fs, CancellationToken token = default ) {
		var data = Hw05PublicationVerificationTests.Prepare();
		BerkeleyDbDatabasePublisher.Publish(
			scope.Destination, data.Image, data.Records, data.Publications,
			new( overwriteExisting: true ), fs, token
		);
	}

	private sealed class FaultFileSystem( string stage ) : Hw05PublicationTestSupport.FileSystem {
		private string Stage => stage;
		internal IOException Failure { get; } = new( "Injected " + stage );
		internal CancellationTokenSource? Cancellation { get; init; }
		internal bool RecreateAfterMove { get; init; }
		internal string? TemporaryPath { get; private set; }
		internal int DeleteCalls { get; private set; }
		private int _validations;
		private void Visit( string current ) {
			if ( current != stage ) {
				return;
			}
			if ( Cancellation is not null ) {
				Cancellation.Cancel();
			}
			else {
				throw Failure;
			}
		}
		internal override void ValidatePaths( string destination, string lockPath, bool overwrite ) {
			base.ValidatePaths( destination, lockPath, overwrite );
			if ( ++_validations == 3 ) {
				Visit( "validate" );
			}
		}
		internal override Stream CreateTemporary( string path ) {
			TemporaryPath = path;
			Visit( "create" );
			if ( stage == "collision" ) {
				File.WriteAllBytes( path, [ 4, 2 ] );
			}
			return new FaultStream( base.CreateTemporary( path ), this, false );
		}
		internal override void FlushToDisk( Stream stream ) {
			Visit( "flush" );
			base.FlushToDisk( ((FaultStream)stream).Inner );
		}
		internal override Stream OpenRead( string path ) {
			Visit( "reopen" );
			if ( stage == "bytes" ) {
				using var corrupt = new FileStream( path, FileMode.Open, FileAccess.Write );
				corrupt.Position = corrupt.Length - 1;
				corrupt.WriteByte( 1 );
			}
			return new FaultStream( base.OpenRead( path ), this, true );
		}
		internal override void Move( string source, string destination, bool overwrite ) {
			Visit( "move" );
			base.Move( source, destination, overwrite );
			if ( RecreateAfterMove ) {
				File.WriteAllBytes( source, [ 4, 2 ] );
			}
		}
		internal override void DeleteTemporary( string path ) {
			DeleteCalls++;
			if ( stage == "compound" ) {
				throw new UnauthorizedAccessException( "Injected cleanup failure" );
			}
			base.DeleteTemporary( path );
		}

		private sealed class FaultStream( Stream inner, FaultFileSystem owner, bool reading ) : Stream {
			internal Stream Inner => inner;
			public override bool CanRead => inner.CanRead;
			public override bool CanSeek => inner.CanSeek;
			public override bool CanWrite => inner.CanWrite;
			public override long Length => inner.Length;
			public override long Position { get => inner.Position; set => inner.Position = value; }
			public override int Read( byte[] buffer, int offset, int count ) {
				owner.Visit( "read" );
				return inner.Read( buffer, offset, count );
			}
			public override void Write( byte[] buffer, int offset, int count ) {
				inner.Write( buffer, offset, Math.Min( count, 7 ) );
				if ( owner.Stage == "compound" ) {
					throw owner.Failure;
				}
				owner.Visit( "write" );
				inner.Write( buffer, offset + Math.Min( count, 7 ), count - Math.Min( count, 7 ) );
			}
			public override long Seek( long offset, SeekOrigin origin ) => inner.Seek( offset, origin );
			public override void Flush() => inner.Flush();
			public override void SetLength( long value ) => inner.SetLength( value );
			protected override void Dispose( bool disposing ) {
				if ( disposing ) {
					inner.Dispose();
					if ( !reading ) {
						if ( owner.Stage == "compound" ) {
							throw new IOException( "Injected close failure" );
						}
						owner.Visit( "close" );
					}
				}
				base.Dispose( disposing );
			}
		}
	}
}
