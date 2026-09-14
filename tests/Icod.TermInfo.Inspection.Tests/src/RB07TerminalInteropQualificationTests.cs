using System.Xml.Linq;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RB07TerminalInteropQualificationTests {
	[Fact]
	public void RasterBackendPackageOnlyQualificationPinsStableTerminalOneThirteen() {
		string root = FindRepositoryRoot();
		string projectPath = Path.Combine(
			root,
			"tools",
			"raster-backend-selection-package-smoke",
			"Icod.TermInfo.RasterBackendSelection.PackageSmoke.csproj"
		);
		Assert.True( File.Exists( projectPath ) );

		XDocument project = XDocument.Load( projectPath );
		Assert.Equal(
			"net8.0;net9.0;net10.0",
			Assert.Single(
				project.Descendants(),
				element => element.Name.LocalName == "TargetFrameworks"
			).Value
		);

		XElement[] packageReferences = project
			.Descendants()
			.Where( element => element.Name.LocalName == "PackageReference" )
			.ToArray();
		Assert.Equal( 2, packageReferences.Length );
		Assert.Equal(
			"$(IcodTermInfoInspectionPackageVersion)",
			Assert.Single(
				packageReferences,
				element => element.Attribute( "Include" )?.Value
					== "Icod.TermInfo.Inspection"
			).Attribute( "Version" )?.Value
		);
		Assert.Equal(
			"1.13.0",
			Assert.Single(
				packageReferences,
				element => element.Attribute( "Include" )?.Value == "Icod.Terminal"
			).Attribute( "Version" )?.Value
		);
		Assert.DoesNotContain(
			project.Descendants(),
			element => element.Name.LocalName == "ProjectReference"
		);
	}

	[Fact]
	public void RasterBackendPackageConsumerKeepsTerminalMappingCallerOwned() {
		string sourcePath = Path.Combine(
			FindRepositoryRoot(),
			"tools",
			"raster-backend-selection-package-smoke",
			"Program.cs"
		);
		Assert.True( File.Exists( sourcePath ) );

		string source = File.ReadAllText( sourcePath );
		foreach ( string requiredToken in new[] {
			"RasterBackendKind.Sixel",
			"RasterBackendKind.KittyGraphics",
			"RasterBackendInspector.Inspect",
			"RasterBackendEvidence",
			"RasterBackendEvidenceKind.Verified",
			"TerminalCapabilityStatus",
			"TerminalCapability.PersistentRasterGraphics",
			"PersistentRasterRuntimeLifecycleObservation",
			"PersistentRasterRuntimeObservationSet",
			"PersistentRasterRuntimeEvidenceIntegrator.Integrate",
			"RasterBackendCandidate",
			"RasterBackendPlanner.Plan",
			"RasterBackendSelectionOptions",
		} ) {
			Assert.Contains( requiredToken, source, StringComparison.Ordinal );
		}
		Assert.DoesNotContain(
			"Icod.Terminal.Internal",
			source,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void FocusedRasterBackendSelectionSampleExistsAndPinsTerminalOneThirteen() {
		string root = FindRepositoryRoot();
		string sampleDirectory = Path.Combine(
			root,
			"samples",
			"Icod.TermInfo.RasterBackendSelection.Sample"
		);
		string projectPath = Path.Combine(
			sampleDirectory,
			"Icod.TermInfo.RasterBackendSelection.Sample.csproj"
		);
		string sourcePath = Path.Combine( sampleDirectory, "Program.cs" );
		string readmePath = Path.Combine( sampleDirectory, "README.md" );
		Assert.True( File.Exists( projectPath ) );
		Assert.True( File.Exists( sourcePath ) );
		Assert.True( File.Exists( readmePath ) );

		XDocument project = XDocument.Load( projectPath );
		Assert.Equal(
			"net8.0;net9.0;net10.0",
			Assert.Single(
				project.Descendants(),
				element => element.Name.LocalName == "TargetFrameworks"
			).Value
		);
		Assert.EndsWith(
			"Icod.TermInfo.Inspection.csproj",
			Assert.Single(
				project.Descendants(),
				element => element.Name.LocalName == "ProjectReference"
			).Attribute( "Include" )?.Value,
			StringComparison.Ordinal
		);
		Assert.Equal(
			"1.13.0",
			Assert.Single(
				project.Descendants(),
				element =>
					element.Name.LocalName == "PackageReference"
					&& element.Attribute( "Include" )?.Value == "Icod.Terminal"
			).Attribute( "Version" )?.Value
		);

		string source = File.ReadAllText( sourcePath );
		foreach ( string requiredToken in new[] {
			"RasterBackendKind.Sixel",
			"RasterBackendKind.KittyGraphics",
			"TerminalCapability.PersistentRasterGraphics",
			"PersistentRasterRuntimeEvidenceIntegrator.Integrate",
			"RasterBackendPlanner.Plan",
			"--live",
		} ) {
			Assert.Contains( requiredToken, source, StringComparison.Ordinal );
		}

		string readme = File.ReadAllText( readmePath );
		Assert.Contains( "deterministic", readme, StringComparison.OrdinalIgnoreCase );
		Assert.Contains( "caller policy", readme, StringComparison.OrdinalIgnoreCase );
		Assert.Contains( "live", readme, StringComparison.OrdinalIgnoreCase );
	}

	[Fact]
	public void RasterBackendQualificationIsWiredIntoPackageVerification() {
		string root = FindRepositoryRoot();
		string verification = File.ReadAllText(
			Path.Combine( root, "packaging", "VerifyPackageArtifact.ps1" )
		);
		Assert.Contains(
			"smoke-rb07-raster-backend-selection-interop.ps1",
			verification,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"Icod.TermInfo.RasterBackendSelection.Sample",
			verification,
			StringComparison.Ordinal
		);

		Assert.True(
			File.Exists(
				Path.Combine(
					root,
					".github",
					"scripts",
					"smoke-rb07-raster-backend-selection-interop.ps1"
				)
			)
		);
		Assert.True(
			File.Exists(
				Path.Combine(
					root,
					".github",
					"scripts",
					"package-smoke-rb07.NuGet.Config"
				)
			)
		);
	}

	[Fact]
	public void ProductionInspectionProjectStillHasNoTerminalDependency() {
		XDocument inspectionProject = XDocument.Load(
			Path.Combine(
				FindRepositoryRoot(),
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

	private static string FindRepositoryRoot() {
		DirectoryInfo? current = new( AppContext.BaseDirectory );
		while ( current is not null ) {
			if ( File.Exists( Path.Combine( current.FullName, "Icod.TermInfo.sln" ) ) ) {
				return current.FullName;
			}
			current = current.Parent;
		}
		throw new DirectoryNotFoundException(
			"Could not locate the repository root."
		);
	}
}
