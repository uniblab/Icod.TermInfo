using System.Text;
using Icod.TermInfo.BerkeleyDb;
using Icod.TermInfo.Tests.Shared;

namespace Icod.TermInfo.Catalogs.Tests;

internal sealed class HashedCatalogFixture : IDisposable {
	internal string Root { get; } = Path.Combine( Path.GetTempPath(), "uc03-" + Guid.NewGuid().ToString( "N" ) );
	internal string PathName => Path.Combine( Root, "catalog.db" );
	internal TerminalCatalogSource Source => new( PathName, TerminalCatalogSourceKind.BerkeleyDbHash );
	internal HashedCatalogFixture() => Directory.CreateDirectory( Root );
	internal void Write( bool aliases = true, string? fault = null ) {
		byte[] storage = "storage"u8.ToArray();
		List<Hdb07RecordSpec> records = [
			Record( storage, Hdb07HashV9FixtureBuilder.NcursesData( Payload() ) ),
			Record( "sample"u8.ToArray(), Hdb07HashV9FixtureBuilder.NcursesIndex( fault == "missing" ? "missing"u8.ToArray() : fault == "cycle" ? "sample"u8.ToArray() : storage ) ),
		];
		if ( aliases ) {
			records.Add( Record( "a"u8.ToArray(), Hdb07HashV9FixtureBuilder.NcursesIndex( "sample"u8.ToArray() ) ) );
			records.Add( Record( "b"u8.ToArray(), Hdb07HashV9FixtureBuilder.NcursesIndex( storage ) ) );
		}
		if ( fault == "orphan" ) { records.Add( Record( "zz-orphan"u8.ToArray(), [0, 1, 2] ) ); }
		if ( fault == "identity" ) { records.Add( Record( "wrong"u8.ToArray(), Hdb07HashV9FixtureBuilder.NcursesIndex( storage ) ) ); }
		File.WriteAllBytes( PathName, Hdb07HashV9FixtureBuilder.CreateDatabase( Hdb07ByteOrder.BigEndian, 4096, records.ToArray() ) );
	}
	internal static byte[] Payload() => Hdb07HashV9FixtureBuilder.CreateCompiledEntry( "sample", "description", "a", "b" );
	internal IReadOnlyList<BerkeleyDbTerminalCatalogEntry> Rows() => new BerkeleyDbTerminalCatalogReader( PathName ).ReadBounded();
	internal TerminalCatalog Read( TerminalCatalogReadOptions? options = null, CancellationToken token = default ) => HashedTerminalCatalogAdapter.Read( Source, options ?? new(), token );
	private static Hdb07RecordSpec Record( byte[] key, byte[] value ) => new( Hdb07ItemSpec.Inline( key ), Hdb07ItemSpec.Inline( value ) );
	public void Dispose() => Directory.Delete( Root, true );
}
