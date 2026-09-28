using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Icod.TermInfo.Catalogs.Tests;

public sealed class UC07ApiFreezeTests {
	public static TheoryData<string, int> Surfaces => new() {
		{ "Catalogs", 11 }, { "Inspection", 108 }, { "BerkeleyDb", 14 }
	};

	private static string Normalize( string text ) =>
		text.Replace( "\r\n", "\n", StringComparison.Ordinal ).Replace( '\r', '\n' );

	private static (string Live, string Baseline, string Fingerprint) Read( string component, int types ) {
		string name = "Icod.TermInfo." + component;
		Assembly assembly = Assembly.Load( name );
		Assert.Equal( types, assembly.GetExportedTypes().Length );
		Assert.Equal( new Version( 1, 0, 0, 0 ), assembly.GetName().Version );
		string path = Path.Combine(
			UC01CatalogPackageContractTests.FindRoot(),
			$"docs/1.17.0-{component.ToUpperInvariant()}-PUBLIC-API-BASELINE.txt"
		);
		string baseline = Normalize( File.ReadAllText( path ) );
		string live = Normalize( Icod.TermInfo.PublicApiSnapshot.Program.CreateManifest( assembly ) );
		string fingerprint = Convert.ToHexString( SHA256.HashData( Encoding.UTF8.GetBytes( baseline ) ) ).ToLowerInvariant();
		return ( live, baseline, fingerprint );
	}

	[Theory]
	[MemberData( nameof( Surfaces ) )]
	public void CompleteLiveSurfaceIsFrozen( string component, int types ) {
		var ( live, baseline, fingerprint ) = Read( component, types );
		Assert.Equal( baseline, live );
		string freezePath = Path.Combine(
			UC01CatalogPackageContractTests.FindRoot(),
			"docs/1.17.0-PUBLIC-API-FREEZE.md"
		);
		string freeze = File.ReadAllText( freezePath );
		Assert.Contains( fingerprint, freeze, StringComparison.Ordinal );
	}

	[Theory]
	[MemberData( nameof( Surfaces ) )]
	public void ExactFreezeRejectsAddedTypesAndChangedMembers( string component, int types ) {
		var ( live, baseline, _ ) = Read( component, types );
		Assert.Equal( baseline, live );
		Assert.NotEqual( baseline, live + "TYPE class Unexpected [sealed]\nEND\n" );
		Assert.NotEqual( baseline, live.Replace( "TYPE class", "TYPE struct", StringComparison.Ordinal ) );
		Assert.Equal( baseline, Normalize( baseline.Replace( "\n", "\r\n", StringComparison.Ordinal ) ) );
	}
}
