using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Icod.TermInfo.Inspection.PackageVerifier;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class UC01InspectionCompatibilityTests {
	[Fact]
	public void ExactAdditionsReconstructFrozenOneFourteen() {
		Assert.Equal( 108, typeof( TermInfoDatabaseInspector ).Assembly.GetExportedTypes().Length );
		string reconstructed = Reconstruct( Snapshot() );
		Assert.Equal( "e9f240a562aec5274d64fb2ec3647862fe4ef5684582af2b442ba3c55e189497",
			Convert.ToHexString( SHA256.HashData( Encoding.UTF8.GetBytes( reconstructed ) ) ).ToLowerInvariant() );
		Assert.Equal( 106, reconstructed.Split( '\n' ).Count( line => line.StartsWith( "TYPE ", StringComparison.Ordinal ) ) );
	}

	[Fact]
	public void UnapprovedTypeMemberOrAlteredNewTypeIsRejected() {
		string current = Snapshot();
		Assert.Throws<InvalidDataException>( () => Reconstruct( current + "\nTYPE class Unexpected [sealed]\nEND\n" ) );
		Assert.Throws<InvalidDataException>( () => Reconstruct( current.Replace( "MaximumCandidateCount", "UnapprovedBudget", StringComparison.Ordinal ) ) );
		Assert.Throws<InvalidDataException>( () => Reconstruct( current.Replace( "InspectDirectoryBounded(", "UnapprovedRead(", StringComparison.Ordinal ) ) );
		Assert.Throws<InvalidDataException>( () => Reconstruct( current.Replace(
			"TYPE class Icod.TermInfo.Inspection.TermInfoDatabaseInspector [static]\n",
			"TYPE class Icod.TermInfo.Inspection.TermInfoDatabaseInspector [static]\n  METHOD public static System.Void Extra()\n", StringComparison.Ordinal ) ) );
	}

	internal static Type[] HistoricalOneFourteenTypes() => typeof( TermInfoDatabaseInspector ).Assembly.GetExportedTypes()
		.Where( type => type != typeof( TermInfoDatabaseCatalogReadOptions ) && type != typeof( TermInfoDatabaseCatalogLimitException ) ).ToArray();

	private static string Snapshot() {
		MethodInfo method = typeof( Icod.TermInfo.PublicApiSnapshot.Program ).GetMethod( "CreateManifest", BindingFlags.NonPublic | BindingFlags.Static )!;
		return (string)method.Invoke( null, [ typeof( TermInfoDatabaseInspector ).Assembly ] )!;
	}
	private static string Reconstruct( string current ) {
		DirectoryInfo? root = new( AppContext.BaseDirectory );
		while ( root is not null && !File.Exists( Path.Combine( root.FullName, "Icod.TermInfo.sln" ) ) ) root = root.Parent;
		Assert.NotNull( root );
		return InspectionUc01Compatibility.Reconstruct( current,
			File.ReadAllText( Path.Combine( root.FullName, "docs/1.17.0-UC01-INSPECTION-PUBLIC-API-ADDITIONS.txt" ) ),
			File.ReadAllText( Path.Combine( root.FullName, "docs/1.17.0-UC01-INSPECTION-PUBLIC-API-ADDITIVE-MEMBERS.txt" ) ) );
	}
}
