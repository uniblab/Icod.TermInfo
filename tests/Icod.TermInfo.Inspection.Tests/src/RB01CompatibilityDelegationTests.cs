using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RB01CompatibilityDelegationTests {
	[Fact]
	public void CurrentCompatibilityGateDelegatesReconstructedOneThirteenHistory() {
		string root = FindRepositoryRoot();
		string currentVerifier = File.ReadAllText(
			Path.Combine(
				root,
				".github",
				"scripts",
				"verify-inspection-compatibility.ps1"
			)
		);
		string historyVerifier = File.ReadAllText(
			Path.Combine(
				root,
				".github",
				"scripts",
				"verify-inspection-compatibility-history.ps1"
			)
		);

		Assert.Contains(
			"& $historyVerifierPath",
			currentVerifier,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"-ManifestPath $reconstructedOneThirteenManifestPath",
			currentVerifier,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"[string]$ManifestPath",
			historyVerifier,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"[string]$AssemblyPath",
			historyVerifier,
			StringComparison.Ordinal
		);
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? directory = new( AppContext.BaseDirectory );
		while ( directory is not null ) {
			if (
				File.Exists(
					Path.Combine(
						directory.FullName,
						"Icod.TermInfo.sln"
					)
				)
			) {
				return directory.FullName;
			}

			directory = directory.Parent;
		}

		throw new InvalidOperationException(
			"Repository root not found."
		);
	}
}
