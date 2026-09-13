using System.Xml.Linq;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class PG07PlacementInteroperabilityQualificationTests {
	[Fact]
	public void PackageOnlyQualificationPinsStableTerminalOneTwelve() {
		string repositoryRoot = GetRepositoryRoot();
		XDocument project = XDocument.Load(
			Path.Combine(
				repositoryRoot,
				"tools",
				"placement-interop-package-smoke",
				"Icod.TermInfo.PlacementInterop.PackageSmoke.csproj"
			)
		);
		XElement[] packageReferences = project
			.Descendants()
			.Where(
				element => element.Name.LocalName == "PackageReference"
			)
			.ToArray();
		Assert.Equal( 2, packageReferences.Length );

		XElement inspectionReference = Assert.Single(
			packageReferences,
			element => element.Attribute( "Include" )?.Value
				== "Icod.TermInfo.Inspection"
		);
		Assert.Equal(
			"$(IcodTermInfoInspectionPackageVersion)",
			inspectionReference.Attribute( "Version" )?.Value
		);
		XElement terminalReference = Assert.Single(
			packageReferences,
			element => element.Attribute( "Include" )?.Value
				== "Icod.Terminal"
		);
		Assert.Equal(
			"1.12.0",
			terminalReference.Attribute( "Version" )?.Value
		);
		Assert.DoesNotContain(
			project.Descendants(),
			element => element.Name.LocalName == "ProjectReference"
		);
	}

	[Fact]
	public void QualificationConsumerMapsBothPlacementSemantics() {
		string repositoryRoot = GetRepositoryRoot();
		string sourcePath = Path.Combine(
			repositoryRoot,
			"tools",
			"placement-interop-package-smoke",
			"Program.cs"
		);

		Assert.True( File.Exists( sourcePath ) );
		string source = File.ReadAllText( sourcePath );
		Assert.Contains(
			"TerminalRasterSourceRectangle",
			source,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"TerminalRasterPlacementOptions",
			source,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"ZIndex",
			source,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"PersistentRasterPlacementSubject.SourceRectangle",
			source,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"PersistentRasterPlacementSubject.SignedZOrder",
			source,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void ProductionInspectionProjectHasNoTerminalDependency() {
		string repositoryRoot = GetRepositoryRoot();
		XDocument inspectionProject = XDocument.Load(
			Path.Combine(
				repositoryRoot,
				"Icod.TermInfo.Inspection",
				"Icod.TermInfo.Inspection.csproj"
			)
		);

		Assert.DoesNotContain(
			inspectionProject.Descendants().Where(
				element =>
					element.Name.LocalName == "PackageReference"
					|| element.Name.LocalName == "ProjectReference"
			),
			element =>
				(element.Attribute( "Include" )?.Value ?? string.Empty).Contains(
					"Icod.Terminal",
					StringComparison.Ordinal
				)
		);
	}

	[Fact]
	public void PersistentRasterPlacementSampleOwnsTerminalExecutionValues() {
		string repositoryRoot = GetRepositoryRoot();
		string sampleDirectory = Path.Combine(
			repositoryRoot,
			"samples",
			"Icod.TermInfo.PersistentRasterPlacement.Sample"
		);
		string sampleProjectPath = Path.Combine(
			sampleDirectory,
			"Icod.TermInfo.PersistentRasterPlacement.Sample.csproj"
		);
		string sampleSourcePath = Path.Combine(
			sampleDirectory,
			"Program.cs"
		);
		Assert.True( File.Exists( sampleProjectPath ) );
		Assert.True( File.Exists( sampleSourcePath ) );

		XDocument sampleProject = XDocument.Load( sampleProjectPath );
		Assert.Equal(
			"net8.0;net9.0;net10.0",
			Assert.Single(
				sampleProject.Descendants(),
				element => element.Name.LocalName == "TargetFrameworks"
			).Value
		);
		XElement inspectionReference = Assert.Single(
			sampleProject.Descendants(),
			element => element.Name.LocalName == "ProjectReference"
		);
		Assert.EndsWith(
			"Icod.TermInfo.Inspection.csproj",
			inspectionReference.Attribute( "Include" )?.Value,
			StringComparison.Ordinal
		);
		XElement terminalReference = Assert.Single(
			sampleProject.Descendants(),
			element =>
				element.Name.LocalName == "PackageReference"
				&& element.Attribute( "Include" )?.Value == "Icod.Terminal"
		);
		Assert.Equal(
			"1.12.0",
			terminalReference.Attribute( "Version" )?.Value
		);

		string source = File.ReadAllText( sampleSourcePath );
		Assert.Contains(
			"PersistentRasterPlacementPlanner.Plan",
			source,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"TerminalRasterSourceRectangle",
			source,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"ZIndex",
			source,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void PlacementInteropQualificationIsWiredIntoPackageVerification() {
		string repositoryRoot = GetRepositoryRoot();
		string verification = File.ReadAllText(
			Path.Combine(
				repositoryRoot,
				"packaging",
				"VerifyPackageArtifact.ps1"
			)
		);
		Assert.Contains(
			"smoke-pg07-placement-interop.ps1",
			verification,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"Icod.TermInfo.PersistentRasterPlacement.Sample",
			verification,
			StringComparison.Ordinal
		);

		XDocument nugetConfig = XDocument.Load(
			Path.Combine(
				repositoryRoot,
				".github",
				"scripts",
				"package-smoke-pg07.NuGet.Config"
			)
		);
		string[] nugetPatterns = nugetConfig
			.Descendants()
			.Where( element => element.Name.LocalName == "package" )
			.Select( element => element.Attribute( "pattern" )?.Value ?? string.Empty )
			.ToArray();
		Assert.Contains( "Icod.TermInfo*", nugetPatterns );
		Assert.Contains( "Icod.Terminal", nugetPatterns );
		Assert.Contains( "Icod.Timing", nugetPatterns );
	}

	private static string GetRepositoryRoot() {
		DirectoryInfo? directory = new( AppContext.BaseDirectory );
		while ( directory is not null ) {
			if ( File.Exists( Path.Combine( directory.FullName, "Directory.Build.props" ) ) ) {
				return directory.FullName;
			}
			directory = directory.Parent;
		}

		throw new DirectoryNotFoundException( "Could not locate the repository root." );
	}
}
