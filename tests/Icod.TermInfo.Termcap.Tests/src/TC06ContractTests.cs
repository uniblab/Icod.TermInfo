using System.Reflection;
using System.Xml.Linq;
using Icod.TermInfo;
using Icod.TermInfo.Termcap;
using Xunit;

namespace Icod.TermInfo.Termcap.Tests;

public sealed class TC06ContractTests
{
	private const string Tc06DevelopmentVersion = "1.6.0-Alpha-6";

	[Fact]
	public void Tc06VersionAndCentralVersionWiringRemainRecorded() {
		string root = FindRepositoryRoot();
		XDocument termcapProject =
			XDocument.Load(
				Path.Combine(
					root,
					"Icod.TermInfo.Termcap",
					"Icod.TermInfo.Termcap.csproj"
				),
				LoadOptions.None
			);
		string implementation =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.6.0-TC06-EXPLICIT-TERMCAP-ACQUISITION.md"
				)
			);

		Assert.Contains( Tc06DevelopmentVersion, implementation );
		Assert.Equal(
			"$(IcodTermInfoSuiteVersion)",
			ReadRequiredProperty(
				termcapProject,
				"Version"
			)
		);
	}

	[Fact]
	public void AcquisitionRemainsInTermcapPackageAndRuntimeDiscoveryIsNotComposed() {
		Assembly termcapAssembly =
			typeof( TermcapAcquirer ).Assembly;
		Assert.DoesNotContain(
			termcapAssembly.GetReferencedAssemblies(),
			assembly => assembly.Name == "Icod.TermInfo.Source"
		);
		Assert.Equal(
			typeof( TerminalDescription ),
			typeof( TermcapAcquisitionResult )
				.GetProperty(
					nameof( TermcapAcquisitionResult.Description )
				)!
				.PropertyType
		);
	}

	[Fact]
	public void EnvironmentAndFilesystemAccessHaveExplicitProviderSeams() {
		Assert.True( typeof( ITermcapEnvironmentProvider ).IsInterface );
		Assert.True( typeof( ITermcapFileProvider ).IsInterface );
		Assert.True(
			typeof( ITermcapEnvironmentProvider ).IsAssignableFrom(
				typeof( SystemTermcapEnvironmentProvider )
			)
		);
		Assert.True(
			typeof( ITermcapFileProvider ).IsAssignableFrom(
				typeof( SystemTermcapFileProvider )
			)
		);
		Assert.NotNull(
			typeof( TermcapAcquisitionOptions ).GetMethod(
				nameof( TermcapAcquisitionOptions.FromEnvironment ),
				BindingFlags.Public | BindingFlags.Static
			)
		);
	}

	[Fact]
	public void TC06DocumentationFreezesAcquisitionBoundary() {
		string root = FindRepositoryRoot();
		string roadmap =
			File.ReadAllText(
				Path.Combine(
					root,
					"Icod.TermInfo-1.6.0-Termcap-Interoperability-Roadmap.md"
				)
			);
		string implementationPath =
			Path.Combine(
				root,
				"docs",
				"1.6.0-TC06-EXPLICIT-TERMCAP-ACQUISITION.md"
			);

		Assert.True( File.Exists( implementationPath ) );
		string implementation = File.ReadAllText( implementationPath );
		Assert.Contains( Tc06DevelopmentVersion, implementation );
		Assert.Contains( "TermcapAcquirer", roadmap );
		Assert.Contains( "TERMPATH", implementation );
		Assert.Contains( "SystemTermcapFileProvider", implementation );
		Assert.Contains( "Runtime discovery", implementation );
		Assert.Contains( "TC07", implementation );
	}

	private static string ReadRequiredProperty(
		XDocument project,
		string propertyName
	) {
		ArgumentNullException.ThrowIfNull( project );
		ArgumentException.ThrowIfNullOrWhiteSpace( propertyName );

		return project
			.Descendants()
			.First(
				element => element.Name.LocalName == propertyName
			)
			.Value
			.Trim();
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current =
			new(
				AppContext.BaseDirectory
			);

		while ( current is not null ) {
			if (
				File.Exists(
					Path.Combine(
						current.FullName,
						"Icod.TermInfo.sln"
					)
				)
			) {
				return current.FullName;
			}
			current = current.Parent;
		}

		throw new InvalidOperationException(
			"Unable to locate the Icod.TermInfo repository root."
		);
	}
}
