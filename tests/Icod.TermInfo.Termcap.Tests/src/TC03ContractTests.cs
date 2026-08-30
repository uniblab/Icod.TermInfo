using System.Reflection;
using System.Xml.Linq;
using Icod.TermInfo;
using Icod.TermInfo.Termcap;
using Xunit;

namespace Icod.TermInfo.Termcap.Tests;

public sealed class TC03ContractTests
{
	private const string Tc03DevelopmentVersion = "1.6.0-Alpha-3";

	[Fact]
	public void CoordinatedDevelopmentVersionAdvancesToTc03() {
		string root = FindRepositoryRoot();
		XDocument buildProperties =
			XDocument.Load(
				Path.Combine(
					root,
					"Directory.Build.props"
				),
				LoadOptions.None
			);

		Assert.Equal(
			Tc03DevelopmentVersion,
			ReadRequiredProperty(
				buildProperties,
				"IcodTermInfoSuiteVersion"
			)
		);
	}

	[Fact]
	public void ResolverDepthBoundaryIsExplicitAndBounded() {
		Assert.Equal(
			64,
			TermcapSourceResolverOptions.DefaultMaximumInheritanceDepth
		);
		Assert.Equal(
			256,
			TermcapSourceResolverOptions.MaximumSupportedInheritanceDepth
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new TermcapSourceResolverOptions( -1 )
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() =>
				new TermcapSourceResolverOptions(
					TermcapSourceResolverOptions.MaximumSupportedInheritanceDepth + 1
				)
		);
	}

	[Fact]
	public void ResolverPublicSurfaceRemainsTermcapSpecific() {
		Assembly termcapAssembly =
			typeof( TermcapSourceResolver ).Assembly;
		Assert.DoesNotContain(
			termcapAssembly.GetReferencedAssemblies(),
			assembly => assembly.Name == "Icod.TermInfo.Source"
		);

		MethodInfo[] publicMethods =
			typeof( TermcapSourceResolvedEntry )
				.GetMethods(
					BindingFlags.Public
					| BindingFlags.Instance
					| BindingFlags.DeclaredOnly
				);
		Assert.DoesNotContain(
			publicMethods,
			method => method.ReturnType == typeof( TerminalDescription )
		);
	}

	[Fact]
	public void ResolverDiagnosticCodesAreStableAndSeparateFromParserFailures() {
		Assert.Equal(
			"TCAP0017",
			TermcapSourceDiagnosticCodes.MissingSourceEntry
		);
		Assert.Equal(
			"TCAP0018",
			TermcapSourceDiagnosticCodes.InheritanceCycle
		);
		Assert.Equal(
			"TCAP0019",
			TermcapSourceDiagnosticCodes.MaximumInheritanceDepthExceeded
		);
	}

	[Fact]
	public void TC03DocumentationFreezesResolverBoundary() {
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
				"1.6.0-TC03-TERMCAP-INHERITANCE-AND-CANCELLATION.md"
			);

		Assert.True( File.Exists( implementationPath ) );
		string implementation = File.ReadAllText( implementationPath );
		Assert.Contains( Tc03DevelopmentVersion, implementation );
		Assert.Contains( "TermcapSourceResolver", roadmap );
		Assert.Contains( "ITermcapSourceEntryProvider", roadmap );
		Assert.Contains( "local fields first", roadmap );
		Assert.Contains( "TermInfoSourceResolver", implementation );
		Assert.Contains( "does not delegate", implementation );
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
