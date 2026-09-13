using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class PG07PlacementInteroperabilityQualificationTests {
	[Fact]
	public void PackageOnlyQualificationPinsStableTerminalOneTwelve() {
		string repositoryRoot = GetRepositoryRoot();
		string project = File.ReadAllText(
			Path.Combine(
				repositoryRoot,
				"tools",
				"inspection-package-smoke",
				"Icod.TermInfo.Inspection.PackageSmoke.csproj"
			)
		);

		Assert.Contains(
			"<PackageReference Include=\"Icod.Terminal\" Version=\"1.12.0\" />",
			project,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void QualificationConsumerMapsBothPlacementSemantics() {
		string repositoryRoot = GetRepositoryRoot();
		string sourcePath = Path.Combine(
			repositoryRoot,
			"tools",
			"inspection-package-smoke",
			"PG07PlacementInteropSmoke.cs"
		);

		Assert.True( File.Exists( sourcePath ) );
		string source = File.ReadAllText( sourcePath );
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
	public void PersistentRasterPlacementSampleExistsWithoutProductionTerminalDependency() {
		string repositoryRoot = GetRepositoryRoot();
		string sampleProject = Path.Combine(
			repositoryRoot,
			"samples",
			"Icod.TermInfo.PersistentRasterPlacement.Sample",
			"Icod.TermInfo.PersistentRasterPlacement.Sample.csproj"
		);
		Assert.True( File.Exists( sampleProject ) );

		string inspectionProject = File.ReadAllText(
			Path.Combine(
				repositoryRoot,
				"Icod.TermInfo.Inspection",
				"Icod.TermInfo.Inspection.csproj"
			)
		);
		Assert.DoesNotContain(
			"Icod.Terminal",
			inspectionProject,
			StringComparison.Ordinal
		);
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
