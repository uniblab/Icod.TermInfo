using System.Reflection;
using System.Xml.Linq;
using Icod.TermInfo.BerkeleyDb;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Catalogs.Tests;

[Collection( "UC01 process state" )]
public sealed class UC07StableMetadataTests {
	[Fact]
	public void EightPackagesHaveOneStableCandidateIdentityAndFrozenAssemblies() {
		const string version = "1.17.0";
		string root = UC01CatalogPackageContractTests.FindRoot();
		string centralVersion = XDocument.Load( Path.Combine( root, "Directory.Build.props" ) )
			.Descendants( "IcodTermInfoSuiteVersion" ).Single().Value;
		Assert.Equal( version, centralVersion );
		string[] projects = ["Icod.TermInfo.csproj", "Icod.TermInfo.Source/Icod.TermInfo.Source.csproj",
			"Icod.TermInfo.Termcap/Icod.TermInfo.Termcap.csproj", "Icod.TermInfo.BerkeleyDb/Icod.TermInfo.BerkeleyDb.csproj",
			"Icod.TermInfo.Compiler/Icod.TermInfo.Compiler.csproj", "Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj",
			"Icod.TermInfo.Catalogs/Icod.TermInfo.Catalogs.csproj", "icod-terminfo/Icod.TermInfo.Router.csproj"];
		Assert.Equal( 8, projects.Length );
		foreach ( string project in projects ) {
			XDocument xml = XDocument.Load( Path.Combine( root, project ) );
			Assert.Equal( "$(IcodTermInfoSuiteVersion)", xml.Descendants( "PackageVersion" ).Single().Value );
			string notes = xml.Descendants( "PackageReleaseNotes" ).Single().Value;
			Assert.StartsWith( version + " ", notes, StringComparison.Ordinal );
			Assert.DoesNotContain( "Alpha", notes, StringComparison.Ordinal );
		}
		foreach ( Assembly assembly in new[] { typeof( TerminalDescription ).Assembly,
			typeof( TerminalCatalogReader ).Assembly, typeof( TermInfoDatabaseInspector ).Assembly,
			typeof( BerkeleyDbTerminalCatalogReader ).Assembly } ) {
				string informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
					.InformationalVersion.Split( '+' )[0];
				Assert.Equal( version, informationalVersion );
			Assert.Equal( new Version( 1, 0, 0, 0 ), assembly.GetName().Version );
		}
		Assembly catalogs = typeof( TerminalCatalogReader ).Assembly;
		Assert.Equal( 11, catalogs.GetExportedTypes().Length );
		string[] dependencies = catalogs.GetReferencedAssemblies().Select( name => name.Name! )
			.Where( name => name.StartsWith( "Icod.TermInfo", StringComparison.Ordinal ) )
			.OrderBy( name => name, StringComparer.Ordinal ).ToArray();
		Assert.Equal( ["Icod.TermInfo", "Icod.TermInfo.BerkeleyDb", "Icod.TermInfo.Inspection"], dependencies );
	}

	[Fact]
	public void PublishedInstallExamplesStillTargetAvailableVersion() {
		string root = UC01CatalogPackageContractTests.FindRoot();
		string readme = File.ReadAllText( Path.Combine( root, "README.md" ) );
		Assert.Contains( "dotnet add package Icod.TermInfo --version 1.16.0", readme, StringComparison.Ordinal );
		Assert.Contains( "dotnet tool install --global Icod.TermInfo.Tools --version 1.16.0", readme, StringComparison.Ordinal );
		Assert.DoesNotContain( "--version 1.17.0", readme, StringComparison.Ordinal );
	}
}
