using System.Reflection;
using Icod.TermInfo.Catalogs.PackageVerifier;
using Xunit;

namespace Icod.TermInfo.Catalogs.Tests;

[Collection( "UC01 process state" )]
public sealed class UC04ApiCompatibilityTests {
	private static string Current() => Icod.TermInfo.PublicApiSnapshot.Program.CreateManifest( typeof( TerminalCatalogReader ).Assembly );
	private static string Addition() => File.ReadAllText( Path.Combine( UC01CatalogPackageContractTests.FindRoot(), "docs/1.17.0-UC04-CATALOGS-PUBLIC-API-ADDITIONS.txt" ) );
	private static string Baseline() => File.ReadAllText( Path.Combine( UC01CatalogPackageContractTests.FindRoot(), "docs/1.17.0-UC01-CATALOGS-PUBLIC-API-BASELINE.txt" ) );

	[Fact]
	public void TheOnlyCatalogsAdditionReconstructsTheTenTypeFoundation() {
		Assembly assembly = typeof( TerminalCatalogReader ).Assembly;
		Assert.Equal( 11, assembly.GetExportedTypes().Length );
		Assert.Equal( new Version( 1, 0, 0, 0 ), assembly.GetName().Version );
		Assert.Equal( Baseline(), CatalogsUc04Compatibility.Reconstruct( Current(), Addition() ) );
	}

	[Fact]
	public void RemovingOrChangingAReaderOrOldMemberCannotHideApiDrift() {
		string current = Current();
		string addition = Addition();
		Assert.Throws<InvalidDataException>( () => CatalogsUc04Compatibility.Reconstruct( current.Replace( addition.Trim(), "", StringComparison.Ordinal ), addition ) );
		Assert.Throws<InvalidDataException>( () => CatalogsUc04Compatibility.Reconstruct( current + addition, addition ) );
		Assert.Throws<InvalidDataException>( () => CatalogsUc04Compatibility.Reconstruct( current.Replace( "Read(System.Threading.CancellationToken", "ReadWrong(System.Threading.CancellationToken", StringComparison.Ordinal ), addition ) );
		Assert.Throws<InvalidDataException>( () => CatalogsUc04Compatibility.Reconstruct( current.Replace( "HasIssues { public get; }", "HasIssues { public set; }", StringComparison.Ordinal ), addition ) );
		Assert.Throws<InvalidDataException>( () => CatalogsUc04Compatibility.Reconstruct( current + "\nTYPE class Icod.TermInfo.Catalogs.Unexpected [sealed]\nEND\n", addition ) );
	}
}
