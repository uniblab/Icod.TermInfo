using System.Collections;
using System.Globalization;
using System.Text;
using Icod.TermInfo.Tests.Shared;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class UC03BoundedCatalogTests {
	[Theory]
	[InlineData( "en-US" )]
	[InlineData( "tr-TR" )]
	public void AliasesChargeOneParseAndEachPublication( string culture ) {
		var previous = CultureInfo.CurrentCulture;
		try {
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( culture );
			byte[] payload = Payload();
			WithRecords( Records(), reader => {
				var limits = new BerkeleyDbTerminalCatalogReadLimits( 3, maximumParsedBytes: payload.Length );
				for ( int read = 0; read < 2; read++ ) {
					var rows = reader.ReadBounded( limits );
					Assert.Equal( new[] { "I-alias", "sample", "z-alias" }, rows.Select( row => row.Name ) );
					Assert.Equal( BerkeleyDbTerminalCatalogEntryKind.Canonical, rows[1].Kind );
					Assert.Equal( BerkeleyDbTerminalCatalogEntryKind.Alias, rows[0].Kind );
					Assert.All( rows, row => Assert.Same( rows[0].Terminal, row.Terminal ) );
				}
				Limit( "MaximumParsedBytes", payload.Length - 1, () => reader.ReadBounded( new( maximumParsedBytes: payload.Length - 1 ) ) );
				Limit( "MaximumPublicationCount", 2, () => reader.ReadBounded( new( maximumPublicationCount: 2 ) ) );
			}
			);
		} finally { CultureInfo.CurrentCulture = previous; }
	}

	[Fact]
	public void OrphanStorageConsumesParsedBudgetOnce() {
		byte[] orphan = Hdb07HashV9FixtureBuilder.CreateCompiledEntry( "orphan", "description" );
		var records = Records().Append( new BerkeleyDbHashRecord( "zz-orphan"u8.ToArray(), Hdb07HashV9FixtureBuilder.NcursesData( orphan ) ) ).ToArray();
		long total = Payload().Length + orphan.Length;
		WithRecords( records, reader => {
			Assert.Equal( 3, reader.ReadBounded( new( maximumParsedBytes: total ) ).Count );
			Limit( "MaximumParsedBytes", total - 1, () => reader.ReadBounded( new( maximumParsedBytes: total - 1 ) ) );
		}
		);
	}

	[Fact]
	public void CanonicalOnlyDoesNotManufactureDeclaredAliasesAndEmptyIsValid() {
		WithRecords( Records().Where( r => !r.Key.Span.SequenceEqual( "I-alias"u8 ) && !r.Key.Span.SequenceEqual( "z-alias"u8 ) ).ToArray(),
			reader => Assert.Equal( "sample", Assert.Single( reader.ReadBounded() ).Name )
		);
		WithRecords( [], reader => Assert.Empty( reader.ReadBounded() ) );
	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( 1 )]
	[InlineData( 2 )]
	public void IndexHopLimitCountsEachFollowedLink( int hops ) {
		var records = Records();
		records[1] = new( "sample"u8.ToArray(), Hdb07HashV9FixtureBuilder.NcursesIndex( "I-alias"u8.ToArray() ) );
		WithRecords( records, reader => {
			if ( hops == 2 ) { Assert.Equal( 3, reader.ReadBounded().Count ); }
			else {
				Limit( "MaximumIndexHops", hops, () => reader.ReadBounded() );
				Assert.Throws<BerkeleyDbDatabaseFormatException>( () => reader.Read() );
			}
		}, new( maximumIndexHops: hops )
		);
	}

	[Fact]
	public void EntryLimitIsTypedAtLogicalSeam() {
		var options = new BerkeleyDbTerminalCatalogReaderOptions( new( Payload().Length - 1 ) );
		var budget = new BerkeleyDbCatalogReadBudget( Path.GetFullPath( "logical.db" ), options, new(), default );
		Limit( "MaximumEntrySize", Payload().Length - 1, () => NcursesCatalogReader.Read( Records(), options.ParserOptions, 16, default, budget ) );
	}

	[Theory]
	[InlineData( "orphan" )]
	[InlineData( "missing" )]
	[InlineData( "cycle" )]
	[InlineData( "marker" )]
	[InlineData( "identity" )]
	[InlineData( "raw-duplicate" )]
	[InlineData( "decoded-duplicate" )]
	public void InvalidRecordsRejectWholeCatalogAndPreserveLegacyExceptionFamily( string fault ) {
		var records = Records().ToList();
		switch ( fault ) {
			case "orphan": records.Add( new( "zz-orphan"u8.ToArray(), new byte[] { 0, 1, 2 } ) ); break;
			case "missing": records[1] = new( "sample"u8.ToArray(), new byte[] { 2, 99 } ); break;
			case "cycle": records[1] = new( "sample"u8.ToArray(), Hdb07HashV9FixtureBuilder.NcursesIndex( "sample"u8.ToArray() ) ); break;
			case "marker": records.Add( new( "zz-unknown"u8.ToArray(), new byte[] { 3 } ) ); break;
			case "identity": records.Add( new( "wrong"u8.ToArray(), Hdb07HashV9FixtureBuilder.NcursesIndex( "storage"u8.ToArray() ) ) ); break;
			case "raw-duplicate": records.Add( records[1] ); break;
			case "decoded-duplicate":
				records.Add( new( Encoding.UTF8.GetBytes( "caf\u00e9" ), new byte[] { 2, 99 } ) );
				records.Add( new( Encoding.Latin1.GetBytes( "caf\u00e9" ), new byte[] { 2, 99 } ) ); break;
		}
		WithRecords( records.ToArray(), reader => {
			Exception legacy = Assert.ThrowsAny<Exception>( () => reader.Read() );
			Exception bounded = Assert.ThrowsAny<Exception>( () => reader.ReadBounded() );
			Assert.Equal( legacy.GetType(), bounded.GetType() );
			Assert.True( bounded is BerkeleyDbDatabaseFormatException or CompiledTermInfoFormatException or InvalidDataException );
		}
		);
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void NonAsciiPublicationKeysRetainExactIdentity( bool utf8 ) {
		byte[] payload = Hdb07HashV9FixtureBuilder.CreateCompiledEntry( "caf\u00e9", "description", "\u00e9l\u00e8ve" );
		Encoding encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		WithRecords( [new( "storage"u8.ToArray(), Hdb07HashV9FixtureBuilder.NcursesData( payload ) ),
			new( encoding.GetBytes( "caf\u00e9" ), Hdb07HashV9FixtureBuilder.NcursesIndex( "storage"u8.ToArray() ) ),
			new( encoding.GetBytes( "\u00e9l\u00e8ve" ), Hdb07HashV9FixtureBuilder.NcursesIndex( "storage"u8.ToArray() ) )],
			reader => Assert.Equal( new[] { "caf\u00e9", "\u00e9l\u00e8ve" }, reader.ReadBounded().Select( row => row.Name ) )
		);
	}

	[Fact]
	public void WriterChainedBucketsAndOverflowAreReadable() {
		using var scope = new Hw05PublicationTestSupport();
		var entries = Enumerable.Range( 0, 150 ).Select( i => {
			string name = "uc03-" + i.ToString( "D3", CultureInfo.InvariantCulture );
			return new BerkeleyDbTerminalDatabaseEntry( name, [], Hdb07HashV9FixtureBuilder.CreateCompiledEntry( name, new string( 'd', 10000 ) ) );
		}
		).ToArray();
		BerkeleyDbTerminalDatabaseWriter.Write( scope.Destination, entries );
		var rows = new BerkeleyDbTerminalCatalogReader( scope.Destination ).ReadBounded();
		Assert.Equal( entries.Select( e => e.CanonicalName ), rows.Select( r => r.Name ) );
	}

	[Theory]
	[InlineData( 1 )]
	[InlineData( 2 )]
	public void CancellationDuringEitherDiscoveryPassStopsBeforeProcessingRecord( int pass ) {
		using var cancellation = new CancellationTokenSource();
		var records = new CancelingRecords( Records(), pass, cancellation );
		var budget = new BerkeleyDbCatalogReadBudget( Path.GetFullPath( "cancel.db" ), new(), new(), cancellation.Token );
		Assert.Throws<OperationCanceledException>( () => NcursesCatalogReader.Read( records, new(), 16, cancellation.Token, budget ) );
		Assert.Equal( pass, records.Enumerations );
		Assert.Equal( 1, records.YieldedOnCanceledPass );
	}

	[Fact]
	public void PreCanceledReadDoesNotOpenMissingFile() {
		using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => new BerkeleyDbTerminalCatalogReader( Path.Combine( Path.GetTempPath(), Guid.NewGuid() + ".db" ) ).ReadBounded( cancellationToken: cancellation.Token ) );
	}

	private static byte[] Payload() => Hdb07HashV9FixtureBuilder.CreateCompiledEntry( "sample", "description", "I-alias", "z-alias" );
	private static BerkeleyDbHashRecord[] Records() => [
		new( "storage"u8.ToArray(), Hdb07HashV9FixtureBuilder.NcursesData( Payload() ) ),
		new( "sample"u8.ToArray(), Hdb07HashV9FixtureBuilder.NcursesIndex( "storage"u8.ToArray() ) ),
		new( "I-alias"u8.ToArray(), Hdb07HashV9FixtureBuilder.NcursesIndex( "storage"u8.ToArray() ) ),
		new( "z-alias"u8.ToArray(), Hdb07HashV9FixtureBuilder.NcursesIndex( "storage"u8.ToArray() ) ),
	];
	private static void WithRecords( BerkeleyDbHashRecord[] records, Action<BerkeleyDbTerminalCatalogReader> action, BerkeleyDbTerminalCatalogReaderOptions? options = null ) {
		byte[] image = Hdb07HashV9FixtureBuilder.CreateDatabase( Hdb07ByteOrder.LittleEndian, 4096,
			records.Select( r => new Hdb07RecordSpec( Hdb07ItemSpec.Inline( r.Key.ToArray() ), Hdb07ItemSpec.Inline( r.Value.ToArray() ) ) ).ToArray()
		);
		using var scope = new Hw05PublicationTestSupport();
		File.WriteAllBytes( scope.Destination, image );
		action( new( scope.Destination, options ) );
	}
	private static void Limit( string name, long value, Action action ) {
		var error = Assert.Throws<BerkeleyDbCatalogLimitException>( action );
		Assert.Equal( name, error.LimitName ); Assert.Equal( value, error.Limit ); Assert.True( Path.IsPathFullyQualified( error.SourcePath ) );
	}
	private sealed class CancelingRecords( IReadOnlyList<BerkeleyDbHashRecord> records, int pass, CancellationTokenSource cancellation ) : IReadOnlyList<BerkeleyDbHashRecord> {
		public int Enumerations { get; private set; }
		public int YieldedOnCanceledPass { get; private set; }
		public int Count => records.Count;
		public BerkeleyDbHashRecord this[int index] => records[index];
		public IEnumerator<BerkeleyDbHashRecord> GetEnumerator() {
			Enumerations++;
			foreach ( var record in records ) {
				if ( Enumerations == pass ) { cancellation.Cancel(); YieldedOnCanceledPass++; }
				yield return record;
			}
		}
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
