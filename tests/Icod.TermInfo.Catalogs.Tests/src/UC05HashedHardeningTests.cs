using Icod.TermInfo.BerkeleyDb;
using Icod.TermInfo.Tests.Shared;
using Xunit;

namespace Icod.TermInfo.Catalogs.Tests;

public sealed class UC05HashedHardeningTests {
	[Theory]
	[InlineData( "MaximumDatabaseSize" )]
	[InlineData( "MaximumRecordCount" )]
	[InlineData( "MaximumDecodedBytes" )]
	[InlineData( "MaximumParsedBytes" )]
	[InlineData( "MaximumEntryCount" )]
	[InlineData( "MaximumEntrySize" )]
	[InlineData( "MaximumIndexHops" )]
	public void ExactPhysicalAndLogicalMaximumSucceedsAndOneLessFails( string limitName ) {
		using HashedCatalogFixture fixture = new();
		fixture.Write();
		int imageLength = checked( (int)new FileInfo( fixture.PathName ).Length );
		int payloadLength = HashedCatalogFixture.Payload().Length;
		// Four physical key/value pairs. Each item is decoded once, including
		// the storage value's marker and both alias indexes.
		long decoded = payloadLength + 39L;
		long exact = limitName switch {
			"MaximumDatabaseSize" => imageLength,
			"MaximumRecordCount" => 4,
			"MaximumDecodedBytes" => decoded,
			"MaximumParsedBytes" => payloadLength,
			"MaximumEntryCount" => 3,
			"MaximumEntrySize" => payloadLength,
			_ => 2,
		};
		TerminalCatalogSource source = fixture.Source;
		TerminalCatalogReadOptions Options( long bound ) => limitName switch {
			"MaximumDatabaseSize" => new( maximumDatabaseSize: checked( (int)bound ) ),
			"MaximumRecordCount" => new( maximumRecordCount: checked( (int)bound ) ),
			"MaximumDecodedBytes" => new( maximumDecodedBytes: bound ),
			"MaximumParsedBytes" => new( maximumParsedBytes: bound ),
			"MaximumEntryCount" => new( maximumEntryCount: checked( (int)bound ) ),
			"MaximumEntrySize" => new( new( checked( (int)bound ) ) ),
			_ => new( maximumIndexHops: checked( (int)bound ) ),
		};
		TerminalCatalogReader reader = new( source, Options( exact ) );
		Assert.Equal( 3, reader.Read().Entries.Count );
		Assert.Equal( 3, reader.Read().Entries.Count );
		TerminalCatalogLimitException error = Assert.Throws<TerminalCatalogLimitException>( () => new TerminalCatalogReader( source, Options( exact - 1 ) ).Read() );
		Assert.Same( source, error.Source );
		string observedName = limitName == "MaximumEntrySize" ? "MaximumStoredItemSize" : limitName;
		Assert.Equal( observedName, error.LimitName );
		Assert.Equal( limitName == "MaximumEntrySize" ? exact : exact - 1, error.Limit );
		BerkeleyDbCatalogLimitException cause = Assert.IsType<BerkeleyDbCatalogLimitException>( error.InnerException );
		Assert.Equal( limitName == "MaximumEntryCount" ? "MaximumPublicationCount" : observedName, cause.LimitName );
	}

	[Fact]
	public void InvalidOrphanCannotReturnPublishedRowsAndPhysicalLimitsPrecedeValidation() {
		using HashedCatalogFixture fixture = new();
		fixture.Write( fault: "orphan" );
		TerminalCatalogSource source = fixture.Source;
		TerminalCatalog result = new TerminalCatalogReader( source ).Read();
		Assert.Equal( TerminalCatalogStatus.InvalidStore, result.Status );
		Assert.Empty( result.Entries );
		Assert.Equal( TerminalCatalogIssueKind.InvalidHashedStore, Assert.Single( result.Issues ).Kind );
		AssertLimit( source, new( maximumRecordCount: 4 ), "MaximumRecordCount", 4 );
		AssertLimit( source, new( maximumDecodedBytes: HashedCatalogFixture.Payload().Length + 39 ), "MaximumDecodedBytes", HashedCatalogFixture.Payload().Length + 39 );
		long parsedBeforeMalformed = HashedCatalogFixture.Payload().Length;
		AssertLimit( source, new( maximumParsedBytes: parsedBeforeMalformed + 1 ), "MaximumParsedBytes", parsedBeforeMalformed + 1 );
		Assert.Equal( TerminalCatalogStatus.InvalidStore,
			new TerminalCatalogReader( source, new( maximumParsedBytes: parsedBeforeMalformed + 2 ) ).Read().Status
		);
		Assert.ThrowsAny<Exception>( () => new BerkeleyDbTerminalCatalogReader( fixture.PathName ).Read() );
	}

	[Fact]
	public void OrphanCompiledPayloadIsChargedEvenWithoutPublication() {
		using HashedCatalogFixture fixture = new();
		byte[] payload = HashedCatalogFixture.Payload();
		byte[] orphan = Hdb07HashV9FixtureBuilder.CreateCompiledEntry( "orphan", "orphan" );
		Hdb07RecordSpec[] records = [
			new( Hdb07ItemSpec.Inline( "storage"u8.ToArray() ), Hdb07ItemSpec.Inline( Hdb07HashV9FixtureBuilder.NcursesData( payload ) ) ),
			new( Hdb07ItemSpec.Inline( "sample"u8.ToArray() ), Hdb07ItemSpec.Inline( Hdb07HashV9FixtureBuilder.NcursesIndex( "storage"u8.ToArray() ) ) ),
			new( Hdb07ItemSpec.Inline( "zz-orphan"u8.ToArray() ), Hdb07ItemSpec.Inline( Hdb07HashV9FixtureBuilder.NcursesData( orphan ) ) ),
		];
		File.WriteAllBytes( fixture.PathName, Hdb07HashV9FixtureBuilder.CreateDatabase( Hdb07ByteOrder.BigEndian, 4096, records ) );
		TerminalCatalogSource source = fixture.Source;
		long total = payload.Length + orphan.Length;
		Assert.Single( new TerminalCatalogReader( source, new( maximumParsedBytes: total ) ).Read().Entries );
		AssertLimit( source, new( maximumParsedBytes: total - 1 ), "MaximumParsedBytes", total - 1 );
	}

	[Fact]
	public void OverflowPayloadWithManyAliasesCountsEachPhysicalPublication() {
		using HashedCatalogFixture fixture = new();
		string[] aliases = Enumerable.Range( 0, 12 ).Select( i => $"alias-{i:D2}" ).ToArray();
		byte[] payload = Hdb07HashV9FixtureBuilder.CreateCompiledEntry( "sample", new string( 'd', 8000 ), aliases );
		byte[] storage = "storage"u8.ToArray();
		byte[] data = Hdb07HashV9FixtureBuilder.NcursesData( payload );
		List<Hdb07RecordSpec> records = [
			new( Hdb07ItemSpec.Inline( storage ), Hdb07ItemSpec.OffPage( data, [3000, 3000, data.Length - 6000] ) ),
			new( Hdb07ItemSpec.Inline( "sample"u8.ToArray() ), Hdb07ItemSpec.Inline( Hdb07HashV9FixtureBuilder.NcursesIndex( storage ) ) ),
		];
		foreach ( string alias in aliases ) {
			records.Add( new( Hdb07ItemSpec.Inline( System.Text.Encoding.ASCII.GetBytes( alias ) ),
				Hdb07ItemSpec.Inline( Hdb07HashV9FixtureBuilder.NcursesIndex( storage ) )
			)
			);
		}
		File.WriteAllBytes( fixture.PathName, Hdb07HashV9FixtureBuilder.CreateDatabase( Hdb07ByteOrder.LittleEndian, 4096, records.ToArray() ) );
		TerminalCatalogSource source = fixture.Source;
		Assert.Equal( 13, new TerminalCatalogReader( source, new( maximumEntryCount: 13, maximumRecordCount: 14,
			maximumParsedBytes: payload.Length
		)
		).Read().Entries.Count
		);
		AssertLimit( source, new( maximumRecordCount: 13 ), "MaximumRecordCount", 13 );
		AssertLimit( source, new( maximumEntryCount: 12 ), "MaximumEntryCount", 12 );
	}

	private static void AssertLimit( TerminalCatalogSource source, TerminalCatalogReadOptions options, string name, long value ) {
		TerminalCatalogLimitException error = Assert.Throws<TerminalCatalogLimitException>( () => new TerminalCatalogReader( source, options ).Read() );
		Assert.Equal( name, error.LimitName );
		Assert.Equal( value, error.Limit );
		Assert.IsType<BerkeleyDbCatalogLimitException>( error.InnerException );
	}
}
