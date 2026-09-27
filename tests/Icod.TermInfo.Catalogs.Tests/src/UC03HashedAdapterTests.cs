using System.Globalization;
using System.Buffers.Binary;
using Icod.TermInfo.BerkeleyDb;
using Icod.TermInfo.Tests.Shared;
using Xunit;

namespace Icod.TermInfo.Catalogs.Tests;

public sealed class UC03HashedAdapterTests {
	[Fact]
	public void ExactHashCollisionsUseChainedBucketsThroughBothReaders() {
		using HashedCatalogFixture fixture = new();
		// Qualified HW03 collision prefixes retain equal hashes after the same suffix.
		string[] names = [ "hw03-0c5ny4k-da6" + new string( 'x', 989 ), "hw03-0fpxptj-1j8p" + new string( 'x', 989 ) ];
		BerkeleyDbTerminalDatabaseWriter.Write( fixture.PathName, names.Select( name =>
			new BerkeleyDbTerminalDatabaseEntry( name, [], Hdb07HashV9FixtureBuilder.CreateCompiledEntry( name, "d" ) )
		)
		);
		byte[] image = File.ReadAllBytes( fixture.PathName );
		int pageSize = checked( (int)BinaryPrimitives.ReadUInt32LittleEndian( image.AsSpan( 20 ) ) );
		Assert.Contains( Enumerable.Range( 1, image.Length / pageSize - 1 ), page =>
			image[page * pageSize + 25] == 13 && BinaryPrimitives.ReadUInt32LittleEndian( image.AsSpan( page * pageSize + 16 ) ) != 0
		);
		Assert.Equal( names, fixture.Rows().Select( row => row.Name ) );
		Assert.Equal( names, fixture.Read().Entries.Select( row => row.PublicationName ) );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void MapsActualPublicationsAndFileProvenance( bool aliases ) {
		using HashedCatalogFixture fixture = new(); fixture.Write( aliases );
		var result = fixture.Read();
		Assert.Equal( TerminalCatalogStatus.Complete, result.Status );
		Assert.Empty( result.Issues ); Assert.Empty( result.DuplicatePublicationNames );
		Assert.Equal( aliases ? new[] { "a", "b", "sample" } : new[] { "sample" }, result.Entries.Select( e => e.PublicationName ) );
		Assert.All( result.Entries, e => {
			Assert.Equal( fixture.PathName, e.SourcePath ); Assert.Null( e.EntryPath );
			Assert.Same( result.Entries[0].Terminal, e.Terminal );
			Assert.Equal( e.PublicationName == "sample" ? TerminalCatalogEntryKind.Canonical : TerminalCatalogEntryKind.Alias, e.Kind );
		}
		);
	}

	[Theory]
	[InlineData( "en-US" )]
	[InlineData( "tr-TR" )]
	public void ReversedLowerRowsRetainObjectsAndOrdinalOrder( string culture ) {
		using HashedCatalogFixture fixture = new(); fixture.Write();
		var rows = fixture.Rows(); var previous = CultureInfo.CurrentCulture;
		try {
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( culture );
			var result = HashedTerminalCatalogAdapter.ReadCore( fixture.Source, new(), default, File.GetAttributes, ( p, o, l, c ) => rows.Reverse().ToArray() );
			Assert.Equal( new[] { "a", "b", "sample" }, result.Entries.Select( e => e.PublicationName ) );
			Assert.All( result.Entries, e => Assert.Same( rows[0].Terminal, e.Terminal ) );
		} finally { CultureInfo.CurrentCulture = previous; }
	}

	[Fact]
	public void EmptyHashedStoreIsComplete() {
		using HashedCatalogFixture fixture = new();
		File.WriteAllBytes( fixture.PathName, Hdb07HashV9FixtureBuilder.CreateDatabase( Hdb07ByteOrder.LittleEndian, 512 ) );
		var result = fixture.Read(); Assert.Equal( TerminalCatalogStatus.Complete, result.Status ); Assert.Empty( result.Entries ); Assert.Empty( result.Issues );
	}

	[Fact]
	public void NonAsciiAndLargeWriterPublicationsArePreserved() {
		using HashedCatalogFixture fixture = new();
		BerkeleyDbTerminalDatabaseWriter.Write( fixture.PathName, [new( "caf\u00e9", [ "\u00e9l\u00e8ve" ],
			Hdb07HashV9FixtureBuilder.CreateCompiledEntry( "caf\u00e9", new string( 'd', 10000 ), "\u00e9l\u00e8ve" )
		)]
		);
		Assert.Equal( new[] { "caf\u00e9", "\u00e9l\u00e8ve" }, fixture.Read().Entries.Select( e => e.PublicationName ) );
	}

	[Theory]
	[InlineData( "orphan" )]
	[InlineData( "missing" )]
	[InlineData( "cycle" )]
	[InlineData( "identity" )]
	public void InvalidStoreNeverReturnsPartialRows( string fault ) {
		using HashedCatalogFixture fixture = new(); fixture.Write( fault: fault );
		var result = fixture.Read(); Assert.Equal( TerminalCatalogStatus.InvalidStore, result.Status ); Assert.Empty( result.Entries );
		var issue = Assert.Single( result.Issues ); Assert.Equal( TerminalCatalogIssueKind.InvalidHashedStore, issue.Kind );
		Assert.Null( issue.EntryPath ); Assert.Null( issue.PublicationName ); Assert.Equal( fixture.PathName, issue.SourcePath );
	}

	[Fact]
	public void RealLimitsAcceptBoundaryAndTranslateNextPublicationAndByte() {
		using HashedCatalogFixture fixture = new(); fixture.Write();
		Assert.Equal( 3, fixture.Read( new( maximumEntryCount: 3, maximumParsedBytes: HashedCatalogFixture.Payload().Length ) ).Entries.Count );
		var publications = Assert.Throws<TerminalCatalogLimitException>( () => fixture.Read( new( maximumEntryCount: 2 ) ) );
		Assert.Equal( "MaximumEntryCount", publications.LimitName ); Assert.Equal( 2, publications.Limit );
		var parsed = Assert.Throws<TerminalCatalogLimitException>( () => fixture.Read( new( maximumParsedBytes: HashedCatalogFixture.Payload().Length - 1 ) ) );
		Assert.Equal( "MaximumParsedBytes", parsed.LimitName ); Assert.IsType<BerkeleyDbCatalogLimitException>( parsed.InnerException );
	}
}
