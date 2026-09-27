using System.Buffers.Binary;
using System.Globalization;
using System.Text;
using Icod.TermInfo.BerkeleyDb;

namespace Icod.TermInfo.Tests.Shared;

internal static class Hw07WriterMatrix {
	internal static readonly int[] Seeds = [ 7, 23, 131, 733 ];

	internal static BerkeleyDbHashRecord[] CreateRecords( int seed ) {
		List<BerkeleyDbHashRecord> records = [];
		int[] lengths = [ 0, 1, 1023, 1024, 1025, 4095, 4096, 9000 ];
		for ( int index = 0; index < 32; index++ ) {
			string suffix = "-" + seed.ToString( CultureInfo.InvariantCulture ) + "-" + index.ToString( "D3", CultureInfo.InvariantCulture );
			// These independently qualified prefixes have the same complete Hash-v9 hash.
			// Identical suffixes preserve the collision across every bucket mask.
			foreach ( string prefix in new[] { "hw03-0c5ny4k-da6", "hw03-0fpxptj-1j8p" } ) {
				string key = prefix + suffix + new string( 'x', 1000 );
				records.Add( new( Encoding.UTF8.GetBytes( key ), Payload( seed + index + prefix.Length, lengths[ index % lengths.Length ] ) ) );
			}
			string ordinary = "hw07-prefix" + suffix;
			int keyPadding = ( index % 4 == 0 ) ? 4096 : 0;
			records.Add( new( Encoding.UTF8.GetBytes( ordinary + new string( 'k', keyPadding ) ), Payload( seed + index, lengths[ ( index + 3 ) % lengths.Length ] ) ) );
		}
		return Sort( records );
	}

	internal static BerkeleyDbHashRecord[] Sort( IEnumerable<BerkeleyDbHashRecord> records ) {
		BerkeleyDbHashRecord[] result = records.ToArray();
		Array.Sort( result, static ( left, right ) => BerkeleyDbHashV9WriterKeyComparer.Instance.Compare( left.Key.Span, right.Key.Span ) );
		return result;
	}

	internal static BerkeleyDbTerminalDatabaseEntry[] CreateUtf8Entries() => [
		Entry( "hw07-caf\u00e9", [ "hw07-\u00e9l\u00e8ve" ] ),
		Entry( "hw07-\u00c5ngstr\u00f6m", [ "hw07-\u00e9", "hw07-\u00c9" ] ),
	];

	private static BerkeleyDbTerminalDatabaseEntry Entry( string name, string[] aliases ) {
		byte[] names = Encoding.Latin1.GetBytes( name + "|" + string.Join( "|", aliases ) + "|HW07 UTF8 keys\0" );
		byte[] compiled = new byte[ ( 12 + names.Length + 1 ) & ~1 ];
		BinaryPrimitives.WriteUInt16LittleEndian( compiled, 0x011A );
		BinaryPrimitives.WriteUInt16LittleEndian( compiled.AsSpan( 2 ), checked( (ushort)names.Length ) );
		names.CopyTo( compiled.AsSpan( 12 ) );
		return new( name, aliases, compiled );
	}

	private static byte[] Payload( int seed, int length ) {
		byte[] result = new byte[length];
		uint state = (uint)seed;
		for ( int index = 0; index < length; index++ ) {
			state = unchecked( state * 1664525U + 1013904223U );
			result[index] = (byte)( state >> 24 );
		}
		return result;
	}
}
