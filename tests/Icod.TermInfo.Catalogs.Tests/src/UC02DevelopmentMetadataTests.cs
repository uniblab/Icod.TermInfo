using System.Reflection;
using Xunit;

namespace Icod.TermInfo.Catalogs.Tests;

public sealed class UC02DevelopmentMetadataTests {
	[Fact]
	public void AdapterAssemblyCarriesTheCoordinatedDevelopmentIdentity() {
		Assembly assembly = typeof( TerminalCatalog ).Assembly;
		string identity = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
		Assert.Equal( "1.17.0-Alpha-5", identity.Split( '+' )[ 0 ] );
		Assert.Equal( new Version( 1, 0, 0, 0 ), assembly.GetName().Version );
		Assert.DoesNotContain( assembly.GetReferencedAssemblies(), name => name.Name == "Icod.TermInfo.Compiler" );
	}
}
