using System.Reflection;
using System.Xml.Linq;
using Icod.TermInfo.BerkeleyDb;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Catalogs.Tests;

[Collection( "UC01 process state" )]
public sealed class UC05DevelopmentMetadataTests {
	[Fact]
	public void CoordinatedAlphaFiveUsesFrozenAssemblyIdentityAndDependencyEdges() {
		Assembly[] assemblies = [typeof( TerminalDescription ).Assembly,
			typeof( TerminalCatalogReader ).Assembly, typeof( TermInfoDatabaseInspector ).Assembly,
			typeof( BerkeleyDbTerminalCatalogReader ).Assembly];
		foreach ( Assembly assembly in assemblies ) {
			Assert.Equal( "1.17.0-Alpha-6", assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
				.InformationalVersion.Split( '+' )[0]
			);
			Assert.Equal( new Version( 1, 0, 0, 0 ), assembly.GetName().Version );
		}
		Assembly catalog = typeof( TerminalCatalogReader ).Assembly;
		Assert.Equal( 11, catalog.GetExportedTypes().Length );
		Assert.Equal( new[] { "Icod.TermInfo", "Icod.TermInfo.BerkeleyDb", "Icod.TermInfo.Inspection" },
			catalog.GetReferencedAssemblies().Select( name => name.Name ).Where( name => name!.StartsWith( "Icod.TermInfo", StringComparison.Ordinal ) )
				.OrderBy( name => name, StringComparer.Ordinal )
		);
		string root = UC01CatalogPackageContractTests.FindRoot();
		Assert.Equal( "1.17.0-Alpha-6", XDocument.Load( Path.Combine( root, "Directory.Build.props" ) )
			.Descendants( "IcodTermInfoSuiteVersion" ).Single().Value
		);
	}
}
