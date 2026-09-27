using Icod.TermInfo.Tests.Shared;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hw07WriterResourceTests {
	[Fact]
	public void ExactPhysicalRecordBudgetStillPublishesAllAliases() {
		using var scope = new Hw05PublicationTestSupport();
		var entry = new BerkeleyDbTerminalDatabaseEntry(
			"hw07-exact", [ "hw07-first", "hw07-second" ],
			Hdb07HashV9FixtureBuilder.CreateCompiledEntry( "hw07-exact", "HW07", "hw07-first", "hw07-second" )
		);
		BerkeleyDbTerminalDatabaseWriter.Write( scope.Destination, [ entry ], new( maximumRecordCount: 4 ) );
		var provider = new BerkeleyDbTerminalDescriptionProvider( scope.Destination );
		Assert.True( provider.TryLoad( "hw07-first", out _ ) );
		Assert.True( provider.TryLoad( "hw07-second", out _ ) );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void EnumerationAbortPreservesExistingDestinationAndDisposes( bool cancel ) {
		using var scope = new Hw05PublicationTestSupport();
		byte[] sentinel = [ 4, 5, 6 ];
		File.WriteAllBytes( scope.Destination, sentinel );
		using var cancellation = new CancellationTokenSource();
		bool disposed = false;
		IEnumerable<BerkeleyDbTerminalDatabaseEntry> Entries() {
			try {
				yield return new( "hw07", [], Hdb07HashV9FixtureBuilder.CreateCompiledEntry( "hw07", "HW07" ) );
				if ( cancel ) {
					cancellation.Cancel();
					yield return new( "second", [], [ 0 ] );
				}
				throw new IOException( "Source enumeration failed." );
			} finally {
				disposed = true;
			}
		}
		Exception? error = Record.Exception(
			() => BerkeleyDbTerminalDatabaseWriter.Write( scope.Destination, Entries(), new( overwriteExisting: true ), cancellation.Token )
		);
		if ( cancel ) {
			Assert.IsType<OperationCanceledException>( error );
		} else {
			Assert.IsType<IOException>( error );
		}
		Assert.True( disposed );
		Assert.Equal( sentinel, File.ReadAllBytes( scope.Destination ) );
		Assert.Single( Directory.GetFileSystemEntries( scope.DirectoryPath ) );
	}

	[Theory]
	[InlineData( 4, 0, 3 )]
	[InlineData( 4, 2, 2 )]
	[InlineData( 1, 0, 1 )]
	public void RecordBudgetStopsEnumerationAndDisposesBeforeFilesystemWork( int limit, int aliasCount, int expectedReads ) {
		using var scope = new Hw05PublicationTestSupport();
		int reads = 0;
		bool disposed = false;
		IEnumerable<BerkeleyDbTerminalDatabaseEntry> Entries() {
			try {
				for ( int index = 0; index < 10; index++ ) {
					reads++;
					if ( reads > expectedReads ) {
						throw new InvalidOperationException( "Enumeration exceeded the physical record budget." );
					}
					string name = "hw07-" + index;
					string[] aliases = Enumerable.Range( 0, aliasCount ).Select( value => name + "-alias-" + value ).ToArray();
					yield return new BerkeleyDbTerminalDatabaseEntry(
						name, aliases, Hdb07HashV9FixtureBuilder.CreateCompiledEntry( name, "HW07", aliases )
					);
				}
			} finally {
				disposed = true;
			}
		}
		var error = Assert.Throws<InvalidOperationException>(
			() => BerkeleyDbTerminalDatabaseWriter.Write( scope.Destination, Entries(), new( maximumRecordCount: limit ) )
		);
		Assert.Contains( "configured Hash record limit", error.Message );
		Assert.Equal( expectedReads, reads );
		Assert.True( disposed );
		Assert.Empty( Directory.GetFileSystemEntries( scope.DirectoryPath ) );
	}

	[Fact]
	public void OversizedAliasBudgetIsRejectedBeforeIdentityPreparation() {
		// The budget must be checked before allocating/validating each alias.
		var entry = new BerkeleyDbTerminalDatabaseEntry( "hw07", [ "../unsafe", "second" ], [ 0 ] );
		var error = Assert.Throws<InvalidOperationException>(
			() => BerkeleyDbTerminalDatabaseWriter.PreparePublications( [ entry ], new( maximumRecordCount: 2 ), CancellationToken.None )
		);
		Assert.Contains( "configured Hash record limit", error.Message );
	}
}
