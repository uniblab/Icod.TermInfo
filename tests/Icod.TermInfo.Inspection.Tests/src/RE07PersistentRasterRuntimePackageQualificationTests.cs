using System.Xml.Linq;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RE07PersistentRasterRuntimePackageQualificationTests {
	[Fact]
	public void RuntimeEvidencePackageOnlyQualificationPinsStableTerminalOneTwelve() {
		string repositoryRoot = GetRepositoryRoot();
		string projectPath = Path.Combine(
			repositoryRoot,
			"tools",
			"runtime-evidence-package-smoke",
			"Icod.TermInfo.RuntimeEvidence.PackageSmoke.csproj"
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
			element => element.Attribute( "Include" )?.Value == "Icod.Terminal"
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
	public void RuntimeEvidenceConsumerMapsTerminalStatusThroughObservationIntegration() {
		string repositoryRoot = GetRepositoryRoot();
		string sourcePath = Path.Combine(
			repositoryRoot,
			"tools",
			"runtime-evidence-package-smoke",
			"Program.cs"
		);
		Assert.True( File.Exists( sourcePath ) );

		string source = File.ReadAllText( sourcePath );
		foreach ( string requiredToken in new[] {
			"TerminalCapabilityStatus",
			"TerminalCapability.PersistentRasterGraphics",
			"TerminalCapabilitySupport.Verified",
			"TerminalCapabilitySupport.Unsupported",
			"PersistentRasterRuntimeLifecycleObservation",
			"PersistentRasterRuntimeObservationOutcome",
			"PersistentRasterRuntimeObservationSet",
			"PersistentRasterRuntimeEvidenceIntegrator.Integrate",
			"CreateLifecyclePlan",
			"PersistentRasterLifecycleEvidenceSubject.PersistentUpload",
			"PersistentRasterLifecycleEvidenceSubject.AcknowledgedUpload",
			"PersistentRasterLifecycleEvidenceSubject.PlacementCreation",
			"PersistentRasterLifecycleEvidenceSubject.MultiplePlacements",
			"PersistentRasterLifecycleEvidenceSubject.PlacementUpdate",
			"PersistentRasterLifecycleEvidenceSubject.PlacementDeletion",
			"PersistentRasterLifecycleEvidenceSubject.ResourceDeletion",
		} ) {
			Assert.Contains( requiredToken, source, StringComparison.Ordinal );
		}
		Assert.DoesNotContain(
			"new PersistentRasterLifecycleEvidence(",
			source,
			StringComparison.Ordinal
		);
		Assert.DoesNotContain(
			"GetNextSourceOrdinal",
			source,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void ProductionInspectionProjectStillHasNoTerminalDependency() {
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
	public void RuntimeIntegrationSampleShowsStaticVerifyIntegrateReplanBoundary() {
		string repositoryRoot = GetRepositoryRoot();
		string sampleDirectory = Path.Combine(
			repositoryRoot,
			"samples",
			"Icod.TermInfo.PersistentRasterRuntimeIntegration.Sample"
		);
		string projectPath = Path.Combine(
			sampleDirectory,
			"Icod.TermInfo.PersistentRasterRuntimeIntegration.Sample.csproj"
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
		XElement inspectionReference = Assert.Single(
			project.Descendants(),
			element => element.Name.LocalName == "ProjectReference"
		);
		Assert.EndsWith(
			"Icod.TermInfo.Inspection.csproj",
			inspectionReference.Attribute( "Include" )?.Value,
			StringComparison.Ordinal
		);
		XElement terminalReference = Assert.Single(
			project.Descendants(),
			element =>
				element.Name.LocalName == "PackageReference"
				&& element.Attribute( "Include" )?.Value == "Icod.Terminal"
		);
		Assert.Equal(
			"1.12.0",
			terminalReference.Attribute( "Version" )?.Value
		);

		string source = File.ReadAllText( sourcePath );
		foreach ( string requiredToken in new[] {
			"PersistentRasterLifecycleInspector.Inspect",
			"PersistentRasterLifecyclePlanner.Plan",
			"VerifyCapabilityAsync",
			"TerminalCapability.PersistentRasterGraphics",
			"PersistentRasterRuntimeLifecycleObservation",
			"PersistentRasterRuntimeObservationSet",
			"PersistentRasterRuntimeEvidenceIntegrator.Integrate",
			"CreateLifecyclePlan",
		} ) {
			Assert.Contains( requiredToken, source, StringComparison.Ordinal );
		}
		Assert.DoesNotContain(
			"new PersistentRasterLifecycleEvidence(",
			source,
			StringComparison.Ordinal
		);
		Assert.DoesNotContain(
			"GetNextSourceOrdinal",
			source,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void ExistingLifecycleSampleUsesRuntimeObservationIntegrationInsteadOfManualVerifiedEvidence() {
		string repositoryRoot = GetRepositoryRoot();
		string source = File.ReadAllText(
			Path.Combine(
				repositoryRoot,
				"samples",
				"Icod.TermInfo.PersistentRasterLifecycle.Sample",
				"Program.cs"
			)
		);

		Assert.Contains(
			"PersistentRasterRuntimeLifecycleObservation",
			source,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"PersistentRasterRuntimeEvidenceIntegrator.Integrate",
			source,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"CreateLifecyclePlan",
			source,
			StringComparison.Ordinal
		);
		Assert.DoesNotContain(
			"PersistentRasterLifecycleEvidenceKind.Verified",
			source,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void ExistingPlacementSampleUsesRuntimeObservationIntegrationAndRetainsTerminalExecutionValues() {
		string repositoryRoot = GetRepositoryRoot();
		string source = File.ReadAllText(
			Path.Combine(
				repositoryRoot,
				"samples",
				"Icod.TermInfo.PersistentRasterPlacement.Sample",
				"Program.cs"
			)
		);

		Assert.Contains(
			"PersistentRasterRuntimeLifecycleObservation",
			source,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"PersistentRasterRuntimePlacementObservation",
			source,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"PersistentRasterRuntimeEvidenceIntegrator.Integrate",
			source,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"CreatePlacementPlan",
			source,
			StringComparison.Ordinal
		);
		Assert.DoesNotContain(
			"PersistentRasterLifecycleEvidenceKind.Verified",
			source,
			StringComparison.Ordinal
		);
		Assert.DoesNotContain(
			"PersistentRasterPlacementEvidenceKind.Verified",
			source,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"TerminalRasterSourceRectangle",
			source,
			StringComparison.Ordinal
		);
		Assert.Contains( "ZIndex", source, StringComparison.Ordinal );
	}

	[Fact]
	public void RuntimeEvidenceQualificationIsWiredIntoPackageVerification() {
		string repositoryRoot = GetRepositoryRoot();
		string verification = File.ReadAllText(
			Path.Combine(
				repositoryRoot,
				"packaging",
				"VerifyPackageArtifact.ps1"
			)
		);
		Assert.Contains(
			"smoke-re07-runtime-evidence-interop.ps1",
			verification,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"Icod.TermInfo.PersistentRasterRuntimeIntegration.Sample",
			verification,
			StringComparison.Ordinal
		);

		string smokeScriptPath = Path.Combine(
			repositoryRoot,
			".github",
			"scripts",
			"smoke-re07-runtime-evidence-interop.ps1"
		);
		string nugetConfigPath = Path.Combine(
			repositoryRoot,
			".github",
			"scripts",
			"package-smoke-re07.NuGet.Config"
		);
		Assert.True( File.Exists( smokeScriptPath ) );
		Assert.True( File.Exists( nugetConfigPath ) );

		XDocument nugetConfig = XDocument.Load( nugetConfigPath );
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
