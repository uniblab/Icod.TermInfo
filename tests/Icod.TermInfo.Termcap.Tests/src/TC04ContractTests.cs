using System.Reflection;
using System.Xml.Linq;
using Icod.TermInfo;
using Icod.TermInfo.Termcap;
using Xunit;

namespace Icod.TermInfo.Termcap.Tests;

public sealed class TC04ContractTests
{
	private const string Tc04DevelopmentVersion = "1.6.0-Alpha-4";

	[Fact]
	public void CoordinatedDevelopmentVersionAdvancesToTc04() {
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
			Tc04DevelopmentVersion,
			ReadRequiredProperty(
				buildProperties,
				"IcodTermInfoSuiteVersion"
			)
		);
	}

	[Fact]
	public void ConverterRemainsInTermcapPackageAndUsesRuntimeModelDirectly() {
		Assembly termcapAssembly =
			typeof( TermcapConverter ).Assembly;
		Assert.DoesNotContain(
			termcapAssembly.GetReferencedAssemblies(),
			assembly => assembly.Name == "Icod.TermInfo.Source"
		);
		Assert.Equal(
			typeof( TerminalDescription ),
			typeof( TermcapConversionResult )
				.GetProperty(
					nameof( TermcapConversionResult.Description )
				)!
				.PropertyType
		);
	}

	[Fact]
	public void ConversionDecisionSurfaceKeepsLossExplicit() {
		Assert.Contains(
			TermcapConversionDecision.Exact,
			Enum.GetValues<TermcapConversionDecision>()
		);
		Assert.Contains(
			TermcapConversionDecision.HistoricalAlias,
			Enum.GetValues<TermcapConversionDecision>()
		);
		Assert.Contains(
			TermcapConversionDecision.Extended,
			Enum.GetValues<TermcapConversionDecision>()
		);
		Assert.Contains(
			TermcapConversionDecision.Approximation,
			Enum.GetValues<TermcapConversionDecision>()
		);
		Assert.Contains(
			TermcapConversionDecision.Unsupported,
			Enum.GetValues<TermcapConversionDecision>()
		);
		Assert.Contains(
			TermcapConversionDecision.Unrepresentable,
			Enum.GetValues<TermcapConversionDecision>()
		);
	}

	[Fact]
	public void TC04DocumentationFreezesConversionBoundary() {
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
				"1.6.0-TC04-TERMCAP-SEMANTIC-CONVERSION.md"
			);

		Assert.True( File.Exists( implementationPath ) );
		string implementation = File.ReadAllText( implementationPath );
		Assert.Contains( Tc04DevelopmentVersion, implementation );
		Assert.Contains( "TermcapConverter", roadmap );
		Assert.Contains( "TermcapConversionResult", roadmap );
		Assert.Contains( "Loss SHALL", roadmap );
		Assert.Contains( "TERMCAP", implementation );
		Assert.Contains( "TC05", implementation );
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
