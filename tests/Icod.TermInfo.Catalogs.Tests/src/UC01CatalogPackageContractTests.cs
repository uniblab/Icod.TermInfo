using System.Xml.Linq;
using Xunit;

namespace Icod.TermInfo.Catalogs.Tests;

[Collection( "UC01 process state" )]
public sealed class UC01CatalogPackageContractTests {
	[Fact]
	public void FoundationSurfaceIsRetainedWithOnlyTheApprovedReaderAddition() {
		var assembly = typeof( TerminalCatalog ).Assembly;
		Assert.Equal( new Version( 1, 0, 0, 0 ), assembly.GetName().Version );
		Assert.Equal( 11, assembly.GetExportedTypes().Length );
		Assert.Equal( 4, assembly.GetExportedTypes().Count( type => type.IsEnum ) );
		Assert.All( assembly.GetExportedTypes().Where( type => !type.IsEnum ), type => Assert.True( type.IsSealed ) );
		Assert.Contains( assembly.GetExportedTypes(), type => type == typeof( TerminalCatalogReader ) );
		string root = FindRoot();
		Assert.Equal( File.ReadAllText( Path.Combine( root, "docs/1.17.0-UC01-CATALOGS-PUBLIC-API-BASELINE.txt" ) ).Replace( "\r\n", "\n", StringComparison.Ordinal ).TrimEnd( '\n' ) + "\n",
			PackageVerifier.CatalogsUc04Compatibility.Reconstruct( Icod.TermInfo.PublicApiSnapshot.Program.CreateManifest( assembly ),
				File.ReadAllText( Path.Combine( root, "docs/1.17.0-UC04-CATALOGS-PUBLIC-API-ADDITIONS.txt" ) )
			)
		);
	}

	[Fact]
	public void ProjectDeclaresOnlyApprovedDependenciesAndAllSolutionConfigurations() {
		string root = FindRoot();
		XDocument project = XDocument.Load( Path.Combine( root, "Icod.TermInfo.Catalogs", "Icod.TermInfo.Catalogs.csproj" ) );
		Assert.Equal( new[] { "..\\Icod.TermInfo.BerkeleyDb\\Icod.TermInfo.BerkeleyDb.csproj", "..\\Icod.TermInfo.Inspection\\Icod.TermInfo.Inspection.csproj", "..\\Icod.TermInfo.csproj" }, project.Descendants( "ProjectReference" ).Select( x => x.Attribute( "Include" )!.Value ).OrderBy( x => x, StringComparer.Ordinal ) );
		Assert.Empty( project.Descendants( "PackageReference" ) );
		Assert.Equal( "net8.0;net9.0;net10.0", Assert.Single( project.Descendants( "TargetFrameworks" ) ).Value );
		Assert.Equal( "LGPL-3.0-or-later", Assert.Single( project.Descendants( "PackageLicenseExpression" ) ).Value );
		string solution = File.ReadAllText( Path.Combine( root, "Icod.TermInfo.sln" ) );
		Assert.Contains( "Icod.TermInfo.Catalogs\\Icod.TermInfo.Catalogs.csproj", solution, StringComparison.Ordinal );
		Assert.Contains( "tests\\Icod.TermInfo.Catalogs.Tests\\Icod.TermInfo.Catalogs.Tests.csproj", solution, StringComparison.Ordinal );
	}

	[Fact]
	public void AlphaOneIsRegisteredAcrossCoordinatedDistribution() {
		string root = FindRoot();
		string Read( string path ) => File.ReadAllText( Path.Combine( root, path ) );
		Assert.Equal( "1.17.0-Alpha-5", XDocument.Parse( Read( "Directory.Build.props" ) ).Descendants( "IcodTermInfoSuiteVersion" ).Single().Value );
		Assert.Contains( "Icod.TermInfo.Catalogs/Icod.TermInfo.Catalogs.csproj", Read( "packaging/PackPackages.ps1" ), StringComparison.Ordinal );
		Assert.Contains( "tools\\catalogs-package-verifier\\Icod.TermInfo.Catalogs.PackageVerifier.csproj", Read( "Icod.TermInfo.sln" ), StringComparison.Ordinal );
		Assert.Contains( "catalogs-package-verifier", Read( ".github/scripts/verify-release-package.sh" ), StringComparison.Ordinal );
		Assert.Contains( "catalogs-package-verifier", Read( ".github/scripts/verify-release-package.cmd" ), StringComparison.Ordinal );
		Assert.Contains( "Expected eight coordinated .nupkg files", Read( ".github/workflows/release.yaml" ), StringComparison.Ordinal );
		Assert.Contains( "Expected seven reusable-library symbol packages", Read( ".github/workflows/release.yaml" ), StringComparison.Ordinal );
	}

	internal static string FindRoot() {
		DirectoryInfo? directory = new( AppContext.BaseDirectory );
		while ( directory is not null ) {
			if ( File.Exists( Path.Combine( directory.FullName, "Icod.TermInfo.sln" ) ) ) {
				return directory.FullName;
			}
			directory = directory.Parent;
		}
		throw new InvalidOperationException( "Repository root missing." );
	}
}
