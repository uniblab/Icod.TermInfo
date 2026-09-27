using System.Security.Cryptography;
using System.Text;

namespace Icod.TermInfo.Inspection.PackageVerifier;

internal static class InspectionUc01Compatibility {
	private const string HistoricalSha256 = "e9f240a562aec5274d64fb2ec3647862fe4ef5684582af2b442ba3c55e189497";

	internal static string Reconstruct( string current, string approvedTypeBlocks, string approvedMemberLines ) {
		string[] approved = Normalize( approvedTypeBlocks ).TrimEnd( '\n' ).Split( "\n\n", StringSplitOptions.None );
		string[] expectedHeaders = [
			"TYPE class Icod.TermInfo.Inspection.TermInfoDatabaseCatalogLimitException [sealed]",
			"TYPE class Icod.TermInfo.Inspection.TermInfoDatabaseCatalogReadOptions [sealed]",
		];
		if ( approved.Length != 2 || !approved.Select( block => block.Split( '\n' )[ 0 ] ).SequenceEqual( expectedHeaders ) )
			throw new InvalidDataException( "UC01 must approve exactly the two bounded Inspection types." );
		string member = Normalize( approvedMemberLines ).TrimEnd( '\n' );
		if ( member.Contains( '\n' ) || !member.StartsWith( "  METHOD public static Icod.TermInfo.Inspection.TermInfoDatabaseCatalog InspectDirectoryBounded(", StringComparison.Ordinal ) )
			throw new InvalidDataException( "UC01 must approve exactly the bounded Inspection method." );
		List<string> blocks = Normalize( current ).TrimEnd( '\n' ).Split( "\n\n", StringSplitOptions.None ).ToList();
		foreach ( string block in approved ) {
			if ( blocks.Count( candidate => candidate == block ) != 1 )
				throw new InvalidDataException( "An approved UC01 Inspection type is missing, duplicated, or changed." );
			blocks.Remove( block );
		}
		int removed = 0;
		for ( int index = 0; index < blocks.Count; index++ ) {
			if ( !blocks[ index ].StartsWith( "TYPE class Icod.TermInfo.Inspection.TermInfoDatabaseInspector [static]\n", StringComparison.Ordinal ) ) continue;
			blocks[ index ] = string.Join( '\n', blocks[ index ].Split( '\n' ).Where( line => {
				if ( line != member ) return true;
				removed++;
				return false;
			} ) );
		}
		if ( removed != 1 ) throw new InvalidDataException( "The approved UC01 Inspection method is missing or duplicated." );
		string result = string.Join( "\n\n", blocks ) + "\n";
		string actual = Convert.ToHexString( SHA256.HashData( Encoding.UTF8.GetBytes( result ) ) ).ToLowerInvariant();
		if ( actual != HistoricalSha256 ) throw new InvalidDataException( $"Reconstructed 1.14 Inspection API changed: {actual}." );
		return result;
	}

	private static string Normalize( string value ) => value.Replace( "\r\n", "\n", StringComparison.Ordinal ).Replace( '\r', '\n' ).TrimEnd( '\n' ) + "\n";
}
