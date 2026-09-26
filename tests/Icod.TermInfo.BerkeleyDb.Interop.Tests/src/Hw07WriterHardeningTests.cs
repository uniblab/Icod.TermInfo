using System.Globalization;
using System.Text;
using Icod.TermInfo.Tests.Shared;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Interop.Tests;

public sealed class Hw07WriterHardeningTests {
	[Theory]
	[InlineData( 7 )]
	[InlineData( 23 )]
	[InlineData( 131 )]
	[InlineData( 733 )]
	public void EveryGeneratedRecordSurvivesNativeReloadAndCrossHostReconstruction( int seed ) {
		var expected = Hw07WriterMatrix.CreateRecords( seed );
		string stem = "hw07-matrix-" + seed.ToString( CultureInfo.InvariantCulture );
		byte[] original = File.ReadAllBytes( Fixture( stem + ".db" ) );
		Assert.Equal( original, BerkeleyDbHashV9ImageBuilder.Build( expected, 2 * 1024 * 1024, CancellationToken.None ) );
		foreach ( string suffix in new[] { ".db", "-repacked.db" } ) {
			byte[] database = File.ReadAllBytes( Fixture( stem + suffix ) );
			Assert.Equal( 96, BerkeleyDbHashReader.ReadRecords( database, 16384, 96, CancellationToken.None ).Count );
			foreach ( var record in expected ) {
				Assert.True( BerkeleyDbHashReader.TryReadValue( database, record.Key.Span, out var actual, 16384 ) );
				Assert.Equal( record.Value.ToArray(), actual );
			}
		}
	}

	[Theory]
	[InlineData( "hw07-native", 8 )]
	[InlineData( "hw07-utf8", 2 )]
	public void PublicWriterReconstructsNativeQualifiedCatalogOnEveryHost( string stem, int canonicalCount ) {
		var entries = new Dictionary<string, BerkeleyDbTerminalDatabaseEntry>( StringComparer.Ordinal );
		string database = Fixture( stem + ".db" );
		foreach ( string line in File.ReadAllLines( Fixture( stem + ".names" ) ) ) {
			string[] fields = line.Split( '\t' );
			byte[] expected = File.ReadAllBytes( Fixture( fields[1] ) );
			Assert.True( NcursesRecordReader.TryReadCompiledEntry( database, Encoding.UTF8.GetBytes( fields[0] ), out var actual ) );
			Assert.Equal( expected, actual );
			TerminalDescription parsed = CompiledTermInfoParser.Parse( expected );
			entries.TryAdd( parsed.Name, new( parsed.Name, parsed.Aliases, expected ) );
		}
		Assert.Equal( canonicalCount, entries.Count );
		string temporary = Path.Combine( Path.GetTempPath(), "icod-hw07-" + Guid.NewGuid().ToString( "N" ) );
		Directory.CreateDirectory( temporary );
		try {
			string destination = Path.Combine( temporary, "rebuilt.db" );
			BerkeleyDbTerminalDatabaseWriter.Write( destination, entries.Values.Reverse() );
			Assert.Equal( File.ReadAllBytes( database ), File.ReadAllBytes( destination ) );
		} finally {
			Directory.Delete( temporary, true );
		}
	}

	private static string Fixture( string file ) {
		string root = Environment.GetEnvironmentVariable( "ICOD_HDB02_FIXTURE_ROOT" )
			?? throw new InvalidOperationException( "ICOD_HDB02_FIXTURE_ROOT is required." );
		return Path.Combine( root, file );
	}
}
