using System.Reflection;
using Icod.TermInfo.BerkeleyDb;
using Xunit;

namespace Icod.TermInfo.Catalogs.Tests;

public sealed class UC03DevelopmentMetadataTests {
	[Fact]
	public void HashedAdapterAndReaderCarryCoordinatedAlphaThreeIdentity() {
		foreach ( Assembly assembly in new[] { typeof( TerminalCatalog ).Assembly, typeof( BerkeleyDbTerminalCatalogReader ).Assembly } ) {
			Assert.Equal( "1.17.0-Alpha-5", assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion.Split( '+' )[0] );
			Assert.Equal( new Version( 1, 0, 0, 0 ), assembly.GetName().Version );
			Assert.DoesNotContain( assembly.GetReferencedAssemblies(), name => name.Name == "Icod.TermInfo.Compiler" );
		}
		Assert.DoesNotContain( typeof( BerkeleyDbTerminalCatalogReader ).Assembly.GetReferencedAssemblies(), name => name.Name is "Icod.TermInfo.Catalogs" or "Icod.TermInfo.Inspection" );
	}
}
