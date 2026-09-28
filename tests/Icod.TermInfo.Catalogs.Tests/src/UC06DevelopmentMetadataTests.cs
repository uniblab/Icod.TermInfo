using System.Reflection;
using System.Xml.Linq;
using Icod.TermInfo.BerkeleyDb;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Catalogs.Tests;

[Collection( "UC01 process state" )]
public sealed class UC06DevelopmentMetadataTests {
	[Fact]
	public void AlphaSixCoordinatesEightPackagesWithoutChangingCatalogSurface() {
		const string version = "1.17.0";
		Assembly[] assemblies = [typeof( TerminalDescription ).Assembly,
			typeof( TerminalCatalogReader ).Assembly, typeof( TermInfoDatabaseInspector ).Assembly,
			typeof( BerkeleyDbTerminalCatalogReader ).Assembly];
		foreach ( Assembly assembly in assemblies ) {
			string informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
				.InformationalVersion.Split( '+' )[0];
			Assert.Equal( version, informationalVersion );
			Assert.Equal( new Version( 1, 0, 0, 0 ), assembly.GetName().Version );
		}
		Assembly catalog = typeof( TerminalCatalogReader ).Assembly;
		Assert.Equal( 11, catalog.GetExportedTypes().Length );
		string?[] dependencies = catalog.GetReferencedAssemblies().Select( name => name.Name )
			.Where( name => name!.StartsWith( "Icod.TermInfo", StringComparison.Ordinal ) )
			.OrderBy( name => name, StringComparer.Ordinal ).ToArray();
		Assert.Equal( new[] { "Icod.TermInfo", "Icod.TermInfo.BerkeleyDb", "Icod.TermInfo.Inspection" }, dependencies );

		string root = UC01CatalogPackageContractTests.FindRoot();
		string currentVersion = XDocument.Load( Path.Combine( root, "Directory.Build.props" ) )
			.Descendants( "IcodTermInfoSuiteVersion" ).Single().Value;
		Assert.Equal( version, currentVersion );
		string[] projects = ["Icod.TermInfo.csproj", "Icod.TermInfo.Source/Icod.TermInfo.Source.csproj",
			"Icod.TermInfo.Termcap/Icod.TermInfo.Termcap.csproj",
			"Icod.TermInfo.BerkeleyDb/Icod.TermInfo.BerkeleyDb.csproj",
			"Icod.TermInfo.Compiler/Icod.TermInfo.Compiler.csproj",
			"Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj",
			"Icod.TermInfo.Catalogs/Icod.TermInfo.Catalogs.csproj", "icod-terminfo/Icod.TermInfo.Router.csproj"];
		Assert.Equal( 8, projects.Length );
		foreach ( string project in projects ) {
			XDocument xml = XDocument.Load( Path.Combine( root, project ) );
			Assert.Equal( "$(IcodTermInfoSuiteVersion)", xml.Descendants( "PackageVersion" ).Single().Value );
			Assert.Contains( version, xml.Descendants( "PackageReleaseNotes" ).Single().Value );
		}
	}
}
