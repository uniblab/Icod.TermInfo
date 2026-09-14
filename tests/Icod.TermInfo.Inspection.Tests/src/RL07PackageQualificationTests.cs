using System.Xml.Linq;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RL07PackageQualificationTests {
	[Fact]
	public void LifecyclePackageConsumerUsesOnlyInspectionNuGetPackage() {
		string root = FindRepositoryRoot();
		string projectPath = Path.Combine(
			root,
			"tools",
			"inspection-package-smoke",
			"Icod.TermInfo.Inspection.PackageSmoke.csproj"
		);
		XDocument project = XDocument.Load( projectPath );
		XElement[] packageReferences = project
			.Descendants()
			.Where(
				element => element.Name.LocalName == "PackageReference"
			)
			.ToArray();
		XElement packageReference = Assert.Single( packageReferences );
		Assert.Equal(
			"Icod.TermInfo.Inspection",
			packageReference.Attribute( "Include" )?.Value
		);
		Assert.DoesNotContain(
			project.Descendants(),
			element => element.Name.LocalName == "ProjectReference"
		);
	}

	[Fact]
	public void LifecyclePackageQualificationHarnessIsWiredIntoArtifactVerification() {
		string root = FindRepositoryRoot();
		string sourcePath = Path.Combine(
			root,
			"tools",
			"inspection-package-smoke",
			"RL07LifecyclePackageSmoke.cs"
		);
		string scriptPath = Path.Combine(
			root,
			".github",
			"scripts",
			"smoke-rl07-package-consumer.ps1"
		);
		Assert.True(
			File.Exists( sourcePath ),
			"RL07 must provide a checked-in NuGet-only lifecycle qualification consumer."
		);
		Assert.True(
			File.Exists( scriptPath ),
			"RL07 must provide an isolated package-consumer runner."
		);

		string source = File.ReadAllText( sourcePath );
		Assert.Contains(
			"PersistentRasterLifecycleInspector.Inspect",
			source,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"PersistentRasterLifecyclePlanner.Plan",
			source,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"PersistentRasterLifecycleEvidenceKind.Verified",
			source,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"PersistentRasterLifecycleClassifier.Classify",
			source,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"TermInfoJsonRenderer.Render",
			source,
			StringComparison.Ordinal
		);
		Assert.DoesNotContain(
			"Icod.Terminal",
			source,
			StringComparison.Ordinal
		);

		string script = File.ReadAllText( scriptPath );
		Assert.Contains(
			"RL07LifecyclePackageSmoke.cs",
			script,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"net8.0",
			script,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"net9.0",
			script,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"net10.0",
			script,
			StringComparison.Ordinal
		);

		string packageVerification = File.ReadAllText(
			Path.Combine(
				root,
				"packaging",
				"VerifyPackageArtifact.ps1"
			)
		);
		Assert.Contains(
			"smoke-rl07-package-consumer.ps1",
			packageVerification,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void LifecycleSampleUsesOnlyInspectionAndIsWiredIntoArtifactVerification() {
		string root = FindRepositoryRoot();
		string sampleDirectory = Path.Combine(
			root,
			"samples",
			"Icod.TermInfo.PersistentRasterLifecycle.Sample"
		);
		string projectPath = Path.Combine(
			sampleDirectory,
			"Icod.TermInfo.PersistentRasterLifecycle.Sample.csproj"
		);
		string sourcePath = Path.Combine(
			sampleDirectory,
			"Program.cs"
		);
		Assert.True(
			File.Exists( projectPath ),
			"RL07 must provide a reusable persistent-raster lifecycle consumer sample."
		);
		Assert.True(
			File.Exists( sourcePath ),
			"RL07 lifecycle sample must include executable source."
		);

		XDocument project = XDocument.Load( projectPath );
		XElement projectReference = Assert.Single(
			project.Descendants(),
			element => element.Name.LocalName == "ProjectReference"
		);
		Assert.EndsWith(
			"Icod.TermInfo.Inspection.csproj",
			projectReference.Attribute( "Include" )?.Value,
			StringComparison.Ordinal
		);
		Assert.DoesNotContain(
			project.Descendants(),
			element => element.Name.LocalName == "PackageReference"
		);

		string source = File.ReadAllText( sourcePath );
		Assert.Contains(
			"PersistentRasterLifecycleInspector.Inspect",
			source,
			StringComparison.Ordinal
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
		Assert.DoesNotContain(
			"Icod.Terminal",
			source,
			StringComparison.Ordinal
		);

		string packageVerification = File.ReadAllText(
			Path.Combine(
				root,
				"packaging",
				"VerifyPackageArtifact.ps1"
			)
		);
		Assert.Contains(
			"Icod.TermInfo.PersistentRasterLifecycle.Sample",
			packageVerification,
			StringComparison.Ordinal
		);
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current = new( AppContext.BaseDirectory );
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
		throw new DirectoryNotFoundException(
			"Could not locate repository root."
		);
	}
}
