using System.Reflection;
using Icod.TermInfo.Catalogs.PackageVerifier;
using Xunit;

namespace Icod.TermInfo.Catalogs.Tests;

[Collection( "UC01 process state" )]
public sealed class UC04ApiCompatibilityTests {
	private static string Current() => Icod.TermInfo.PublicApiSnapshot.Program.CreateManifest( typeof( TerminalCatalogReader ).Assembly );
	private static string Addition() => File.ReadAllText( Path.Combine( UC01CatalogPackageContractTests.FindRoot(), "docs/1.17.0-UC04-CATALOGS-PUBLIC-API-ADDITIONS.txt" ) );
	private static string Baseline() => File.ReadAllText( Path.Combine( UC01CatalogPackageContractTests.FindRoot(), "docs/1.17.0-UC01-CATALOGS-PUBLIC-API-BASELINE.txt" ) );
	private static string Normalize( string value ) => value.Replace( "\r\n", "\n", StringComparison.Ordinal ).Replace( '\r', '\n' ).TrimEnd( '\n' ) + "\n";
	private static string Mutate( string current, string original, string replacement ) {
		Assert.Contains( original, current, StringComparison.Ordinal );
		return current.Replace( original, replacement, StringComparison.Ordinal );
	}

	[Fact]
	public void TheOnlyCatalogsAdditionReconstructsTheTenTypeFoundation() {
		Assembly assembly = typeof( TerminalCatalogReader ).Assembly;
		Assert.Equal( 11, assembly.GetExportedTypes().Length );
		Assert.Equal( new Version( 1, 0, 0, 0 ), assembly.GetName().Version );
		Assert.Equal( Normalize( Baseline() ), CatalogsUc04Compatibility.Reconstruct( Current(), Addition() ) );
		Assert.Equal( Normalize( Baseline() ), CatalogsUc04Compatibility.Reconstruct(
			Normalize( Current() ).Replace( "\n", "\r\n", StringComparison.Ordinal ),
			Normalize( Addition() ).Replace( "\n", "\r\n", StringComparison.Ordinal )
		)
		);
	}

	[Fact]
	public void RemovingOrChangingAReaderOrOldMemberCannotHideApiDrift() {
		string current = Current();
		string addition = Addition();
		Assert.Throws<InvalidDataException>( () => CatalogsUc04Compatibility.Reconstruct( Mutate( current, Normalize( addition ).TrimEnd( '\n' ), "" ), addition ) );
		Assert.Throws<InvalidDataException>( () => CatalogsUc04Compatibility.Reconstruct( current + addition, addition ) );
		Assert.Throws<InvalidDataException>( () => CatalogsUc04Compatibility.Reconstruct( Mutate( current, "Read(System.Threading.CancellationToken", "ReadWrong(System.Threading.CancellationToken" ), addition ) );
		Assert.Throws<InvalidDataException>( () => CatalogsUc04Compatibility.Reconstruct( Mutate( current, "HasIssues { public get; }", "HasIssues { public set; }" ), addition ) );
		Assert.Throws<InvalidDataException>( () => CatalogsUc04Compatibility.Reconstruct( current + "\nTYPE class Icod.TermInfo.Catalogs.Unexpected [sealed]\nEND\n", addition ) );
	}
}
