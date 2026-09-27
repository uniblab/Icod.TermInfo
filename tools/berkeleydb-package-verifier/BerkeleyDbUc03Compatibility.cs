using System.Security.Cryptography;
using System.Text;

namespace Icod.TermInfo.BerkeleyDb.PackageVerifier;

internal static class BerkeleyDbUc03Compatibility {
	private const string HistoricalSha256 = "01a84c409fb222324009272de0213d1b1a8dc3df1ccac46ce66341f17d4baa81";

	internal static string Reconstruct( string current, string approvedTypeBlocks, string approvedMemberLines ) {
		string[] approved = Normalize( approvedTypeBlocks ).TrimEnd( '\n' ).Split( "\n\n", StringSplitOptions.None );
		string[] expectedHeaders = [
			"TYPE class Icod.TermInfo.BerkeleyDb.BerkeleyDbCatalogLimitException [sealed]",
			"TYPE class Icod.TermInfo.BerkeleyDb.BerkeleyDbTerminalCatalogReadLimits [sealed]",
		];
		if ( approved.Length != 2 || !approved.Select( block => block.Split( '\n' )[ 0 ] ).SequenceEqual( expectedHeaders ) ) {
			throw new InvalidDataException( "UC03 must approve exactly the two bounded BerkeleyDb types." );
		}
		string member = Normalize( approvedMemberLines ).TrimEnd( '\n' );
		if ( member.Contains( '\n' ) || !member.StartsWith( "  METHOD public System.Collections.Generic.IReadOnlyList<Icod.TermInfo.BerkeleyDb.BerkeleyDbTerminalCatalogEntry> ReadBounded(", StringComparison.Ordinal ) ) {
			throw new InvalidDataException( "UC03 must approve exactly the bounded BerkeleyDb method." );
		}
		List<string> blocks = Normalize( current ).TrimEnd( '\n' ).Split( "\n\n", StringSplitOptions.None ).ToList();
		foreach ( string block in approved ) {
			if ( blocks.Count( candidate => candidate == block ) != 1 ) {
				throw new InvalidDataException( "An approved UC03 BerkeleyDb type is missing, duplicated, or changed." );
			}
			blocks.Remove( block );
		}
		int removed = 0;
		for ( int index = 0; index < blocks.Count; index++ ) {
			if ( !blocks[ index ].StartsWith( "TYPE class Icod.TermInfo.BerkeleyDb.BerkeleyDbTerminalCatalogReader [sealed]\n", StringComparison.Ordinal ) ) {
				continue;
			}
			blocks[ index ] = string.Join( '\n', blocks[ index ].Split( '\n' ).Where( line => {
				if ( line != member ) {
					return true;
				}
				removed++;
				return false;
			}
			)
			);
		}
		if ( removed != 1 ) {
			throw new InvalidDataException( "The approved UC03 BerkeleyDb method is missing or duplicated." );
		}
		string result = string.Join( "\n\n", blocks ) + "\n";
		string actual = Convert.ToHexString( SHA256.HashData( Encoding.UTF8.GetBytes( result ) ) ).ToLowerInvariant();
		if ( actual != HistoricalSha256 ) {
			throw new InvalidDataException( $"Reconstructed 1.16 BerkeleyDb API changed: {actual}." );
		}
		return result;
	}

	private static string Normalize( string value ) => value.Replace( "\r\n", "\n", StringComparison.Ordinal ).Replace( '\r', '\n' ).TrimEnd( '\n' ) + "\n";
}
