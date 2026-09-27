using System.Security.Cryptography;
using System.Text;

namespace Icod.TermInfo.Catalogs.PackageVerifier;

internal static class CatalogsUc04Compatibility {
	private const string AdditionSha256 = "6e419679f330a968bf377a4a3b5bcc36020ca84c6b201e364845f23db375fc3d";
	private const string FoundationSha256 = "9b7479953a060de94aadf994ddd5c1946660e8f1f2341a3c26291e2a3c9e0b3f";

	internal static string Reconstruct( string current, string addition ) {
		ArgumentNullException.ThrowIfNull( current );
		ArgumentNullException.ThrowIfNull( addition );
		string approved = Normalize( addition );
		if ( Sha256( approved ) != AdditionSha256 || !approved.StartsWith(
			"TYPE class Icod.TermInfo.Catalogs.TerminalCatalogReader [sealed]\n", StringComparison.Ordinal
		) ) {
			throw new InvalidDataException( "The UC04 reader addition differs from the approved signature." );
		}
		List<string> blocks = Normalize( current ).TrimEnd( '\n' ).Split( "\n\n", StringSplitOptions.None ).ToList();
		if ( blocks.Count( block => block == approved.TrimEnd( '\n' ) ) != 1 ) {
			throw new InvalidDataException( "The approved UC04 reader is missing, duplicated, or changed." );
		}
		blocks.Remove( approved.TrimEnd( '\n' ) );
		string reconstructed = string.Join( "\n\n", blocks ) + "\n";
		if ( Sha256( reconstructed ) != FoundationSha256 ) {
			throw new InvalidDataException( "The reconstructed UC01 Catalogs API differs from its frozen baseline." );
		}
		return reconstructed;
	}

	private static string Normalize( string value ) => value.Replace( "\r\n", "\n", StringComparison.Ordinal ).Replace( '\r', '\n' ).TrimEnd( '\n' ) + "\n";
	private static string Sha256( string value ) => Convert.ToHexString( SHA256.HashData( Encoding.UTF8.GetBytes( value ) ) ).ToLowerInvariant();
}
