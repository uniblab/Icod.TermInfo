using System.Reflection;
using Icod.TermInfo.BerkeleyDb;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Catalogs.Tests;

public sealed class UC04DevelopmentMetadataTests {
	[Fact]
	public void ReaderAndBothLowerLayersCarryCoordinatedAlphaFourMetadata() {
		foreach ( Assembly assembly in new[] { typeof( TerminalCatalogReader ).Assembly,
			typeof( BerkeleyDbTerminalCatalogReader ).Assembly, typeof( TermInfoDatabaseInspector ).Assembly } ) {
			Assert.Equal( "1.17.0", assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion.Split( '+' )[0] );
			Assert.Equal( new Version( 1, 0, 0, 0 ), assembly.GetName().Version );
		}
		Assembly catalogs = typeof( TerminalCatalogReader ).Assembly;
		Assert.Equal( 11, catalogs.GetExportedTypes().Length );
		Assert.Equal( new[] { "Icod.TermInfo", "Icod.TermInfo.BerkeleyDb", "Icod.TermInfo.Inspection" },
			catalogs.GetReferencedAssemblies().Select( name => name.Name ).Where( name => name!.StartsWith( "Icod.TermInfo", StringComparison.Ordinal ) ).OrderBy( name => name, StringComparer.Ordinal )
		);
	}
}
