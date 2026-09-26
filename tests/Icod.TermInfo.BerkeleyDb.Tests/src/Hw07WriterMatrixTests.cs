using System.Text;
using Icod.TermInfo.Tests.Shared;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hw07WriterMatrixTests {
	[Theory]
	[InlineData( 7 )]
	[InlineData( 23 )]
	[InlineData( 131 )]
	[InlineData( 733 )]
	public void MixedInlineOverflowAndExactCollisionsRoundTripEveryRecord( int seed ) {
		var records = Hw07WriterMatrix.CreateRecords( seed );
		byte[] image = BerkeleyDbHashV9ImageBuilder.Build( records, 2 * 1024 * 1024, CancellationToken.None );
		Assert.Equal( 96, records.Length );
		Assert.Equal( 96, BerkeleyDbHashReader.ReadRecords( image, 16384, 96, CancellationToken.None ).Count );
		foreach ( var record in records ) {
			Assert.True( BerkeleyDbHashReader.TryReadValue( image, record.Key.Span, out var value, maximumItemSize: 16384 ) );
			Assert.Equal( record.Value.ToArray(), value );
		}
		Assert.False( BerkeleyDbHashReader.TryReadValue( image, "missing"u8, out _, maximumItemSize: 16384 ) );
		Assert.Throws<InvalidOperationException>(
			() => BerkeleyDbHashV9ImageBuilder.Build( records, 3 * 4096, CancellationToken.None )
		);
		Assert.Equal(
			image,
			BerkeleyDbHashV9ImageBuilder.Build( Hw07WriterMatrix.Sort( records.Reverse() ), image.Length, CancellationToken.None )
		);
	}

	[Fact]
	public void Utf8PublicationsDoNotManufactureLatin1FallbackKeys() {
		using var scope = new Hw05PublicationTestSupport();
		var entries = Hw07WriterMatrix.CreateUtf8Entries();
		BerkeleyDbTerminalDatabaseWriter.Write( scope.Destination, entries );
		byte[] image = File.ReadAllBytes( scope.Destination );
		var provider = new BerkeleyDbTerminalDescriptionProvider( scope.Destination );
		foreach ( var entry in entries ) {
			foreach ( string name in entry.Aliases.Prepend( entry.CanonicalName ) ) {
				Assert.True( NcursesRecordReader.TryReadCompiledEntry( scope.Destination, Encoding.UTF8.GetBytes( name ), out var data ) );
				Assert.Equal( entry.Data, data );
				Assert.False( BerkeleyDbHashReader.TryReadValue( image, Encoding.Latin1.GetBytes( name ), out _, maximumItemSize: 16384 ) );
				Assert.True( provider.TryLoad( name, out var actual ) );
				Assert.Equal( entry.CanonicalName, actual!.Name );
			}
		}
		Assert.False( provider.TryLoad( "hw07-cafe\u0301", out _ ) );
		Assert.False( provider.TryLoad( "hw07-CAF\u00c9", out _ ) );
	}
}
