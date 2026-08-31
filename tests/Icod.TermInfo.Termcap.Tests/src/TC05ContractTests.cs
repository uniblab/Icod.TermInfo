using System.Reflection;
using System.Xml.Linq;
using Icod.TermInfo;
using Icod.TermInfo.Termcap;
using Xunit;

namespace Icod.TermInfo.Termcap.Tests;

public sealed class TC05ContractTests
{
	private const string Tc05DevelopmentVersion = "1.6.0-Alpha-5";

	[Fact]
	public void Tc05VersionAndCentralVersionWiringRemainRecorded() {
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
					"1.6.0-TC05-TERMCAP-REVERSE-CONVERSION-AND-RENDERING.md"
				)
			);

		Assert.Contains( Tc05DevelopmentVersion, implementation );
		Assert.Equal(
			"$(IcodTermInfoSuiteVersion)",
			ReadRequiredProperty(
				termcapProject,
				"Version"
			)
		);
	}

	[Fact]
	public void RendererRemainsInTermcapPackageAndUsesRuntimeModelDirectly() {
		Assembly termcapAssembly =
			typeof( TermcapRenderer ).Assembly;
		Assert.DoesNotContain(
			termcapAssembly.GetReferencedAssemblies(),
			assembly => assembly.Name == "Icod.TermInfo.Source"
		);
		Assert.NotNull(
			typeof( TermcapRenderer ).GetMethod(
				nameof( TermcapRenderer.Analyze ),
				new[] { typeof( TerminalDescription ) }
			)
		);
		Assert.Equal(
			typeof( string ),
			typeof( TermcapRenderResult )
				.GetProperty(
					nameof( TermcapRenderResult.Text )
				)!
				.PropertyType
		);
	}

	[Fact]
	public void TC05DocumentationFreezesReverseRenderingBoundary() {
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
				"1.6.0-TC05-TERMCAP-REVERSE-CONVERSION-AND-RENDERING.md"
			);

		Assert.True( File.Exists( implementationPath ) );
		string implementation = File.ReadAllText( implementationPath );
		Assert.Contains( Tc05DevelopmentVersion, implementation );
		Assert.Contains( "determine representability before emitting text", roadmap );
		Assert.Contains( "\\072", roadmap );
		Assert.Contains( "TermcapRenderer", implementation );
		Assert.Contains( "TC06", implementation );
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
