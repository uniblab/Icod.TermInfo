using System.Security.Cryptography;
using System.Text;

namespace Icod.TermInfo.BerkeleyDb.PackageVerifier;

internal static class BerkeleyDbApiFreeze {
	internal static void VerifyReaderReconstruction( string current, string frozenReader ) {
		string reader = Normalize( frozenReader );
		string fingerprint = Convert.ToHexString( SHA256.HashData( Encoding.UTF8.GetBytes( reader ) ) );
		if ( fingerprint != "F519600AA4085D07C2D20BD8DC7A32C4DC06A43F4E361554B205CE2F97A8BF36" ) {
			throw new InvalidDataException( "The immutable 1.15 reader baseline fingerprint changed." );
		}
		var writerHeaders = new HashSet<string>( StringComparer.Ordinal ) {
			"TYPE class Icod.TermInfo.BerkeleyDb.BerkeleyDbTerminalDatabaseEntry [sealed]",
			"TYPE class Icod.TermInfo.BerkeleyDb.BerkeleyDbTerminalDatabaseWriter [static]",
			"TYPE class Icod.TermInfo.BerkeleyDb.BerkeleyDbTerminalDatabaseWriterOptions [sealed]",
		};
		List<string> retained = [];
		foreach ( string block in Normalize( current ).TrimEnd( '\n' ).Split( "\n\n", StringSplitOptions.None ) ) {
			string header = block.Split( '\n' )[0];
			if ( writerHeaders.Remove( header ) ) {
				if ( !block.EndsWith( "\nEND", StringComparison.Ordinal ) ) {
					throw new InvalidDataException( "The writer API block is incomplete." );
				}
			} else {
				retained.Add( block );
			}
		}
		string reconstructed = string.Join( "\n\n", retained ) + "\n";
		if ( writerHeaders.Count != 0 || reconstructed != reader ) {
			throw new InvalidDataException( "Removing the three writer types does not reconstruct the frozen 1.15 reader API." );
		}
	}

	private static string Normalize( string text ) => text.Replace( "\r\n", "\n", StringComparison.Ordinal )
		.Replace( "\r", "\n", StringComparison.Ordinal ).TrimEnd( '\n' ) + "\n";
}
