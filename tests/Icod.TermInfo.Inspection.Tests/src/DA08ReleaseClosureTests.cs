using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class DA08ReleaseClosureTests {
	private const string InspectionApiSha256 =
		"72c29e2df71ec9f64db1d2e545f3fa2b67ed263c0ba48a53059dabdb0641aa5f";
	private const string JsonV1SchemaSha256 =
		"76578f421b254802d24453af6868edaf8c23c4b78a87c7e8ef86b233ff0e8500";
	private const string JsonV2SchemaSha256 =
		"ae4d53608881344e902f02303c71e2d432500969e60cfb005d70feea607499d0";

	[Fact]
	public void ExactOneTenInspectionSurfaceIsFrozen() {
		string root = FindRepositoryRoot();
		string baseline = ReadRepositoryFile(
			root,
			"docs/1.10.0-INSPECTION-PUBLIC-API-BASELINE.txt"
		);

		Assert.Equal( InspectionApiSha256, ComputeSha256( baseline ) );
		Assert.Equal(
			51,
			typeof( TermInfoDatabaseSet ).Assembly.GetExportedTypes().Length
		);
		foreach (
			string marker
			in new[] {
				"TYPE class Icod.TermInfo.Inspection.TermInfoDatabaseSet [sealed]",
				"TYPE class Icod.TermInfo.Inspection.TermInfoDatabaseSetComparer [static]",
				"TYPE class Icod.TermInfo.Inspection.TermInfoDatabaseSetPlanningCandidate [sealed]",
				"TYPE class Icod.TermInfo.Inspection.TermInfoDatabaseSetSourcePlanningResult [sealed]",
				"TYPE class Icod.TermInfo.Inspection.TermInfoJsonRenderer [static]",
			}
		) {
			Assert.Contains( marker, baseline, StringComparison.Ordinal );
		}
	}

	[Fact]
	public void BothJsonSchemasHaveExactFrozenFingerprints() {
		string root = FindRepositoryRoot();
		string v1 = ReadRepositoryFile(
			root,
			"docs/Icod.TermInfo.Inspection.schema.json"
		);
		string v2 = ReadRepositoryFile(
			root,
			"docs/Icod.TermInfo.Inspection.schema.v2.json"
		);
		string fingerprints = ReadRepositoryFile(
			root,
			"docs/1.10.0-DA08-FREEZE-FINGERPRINTS.txt"
		);

		Assert.Equal( JsonV1SchemaSha256, ComputeSha256( v1 ) );
		Assert.Equal( JsonV2SchemaSha256, ComputeSha256( v2 ) );
		Assert.Contains( InspectionApiSha256, fingerprints, StringComparison.Ordinal );
		Assert.Contains( JsonV1SchemaSha256, fingerprints, StringComparison.Ordinal );
		Assert.Contains( JsonV2SchemaSha256, fingerprints, StringComparison.Ordinal );
		Assert.Contains(
			"\"$id\": \"urn:icod:terminfo:inspection:json:1\"",
			v1,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"\"$id\": \"urn:icod:terminfo:inspection:json:2\"",
			v2,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void ReleaseVerifiersEnforceOneTenInspectionApiAndBothSchemas() {
		string root = FindRepositoryRoot();
		string shell = ReadRepositoryFile(
			root,
			".github/scripts/verify-release-package.sh"
		);
		string command = ReadRepositoryFile(
			root,
			".github/scripts/verify-release-package.cmd"
		);
		string packageVerifier = ReadRepositoryFile(
			root,
			"tools/inspection-package-verifier/Program.cs"
		);

		Assert.Contains(
			"1.10.0-INSPECTION-PUBLIC-API-BASELINE.txt",
			shell,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"1.10.0-INSPECTION-PUBLIC-API-BASELINE.txt",
			command,
			StringComparison.Ordinal
		);
		Assert.Contains( JsonV1SchemaSha256, packageVerifier, StringComparison.Ordinal );
		Assert.Contains( JsonV2SchemaSha256, packageVerifier, StringComparison.Ordinal );
		Assert.Contains(
			"Icod.TermInfo.Inspection.schema.v2.json",
			packageVerifier,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void DistributionGateRetainsDa07PackageAndArchiveAutomation() {
		string workflow = ReadRepositoryFile(
			FindRepositoryRoot(),
			".github/workflows/pull-request.yaml"
		);

		Assert.Contains(
			"Smoke DA07 installed-tool database automation",
			workflow,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"Smoke DA07 archive database automation",
			workflow,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"windows-11-arm",
			workflow,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"ubuntu-24.04-arm",
			workflow,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"macos-15-intel",
			workflow,
			StringComparison.Ordinal
		);
	}

	private static string ComputeSha256(
		string text
	) {
		ArgumentNullException.ThrowIfNull( text );

		string normalized = text
			.Replace( "\r\n", "\n", StringComparison.Ordinal )
			.Replace( '\r', '\n' );
		return Convert.ToHexString(
			SHA256.HashData(
				Encoding.UTF8.GetBytes( normalized )
			)
		).ToLowerInvariant();
	}

	private static string ReadRepositoryFile(
		string root,
		string relativePath
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		ArgumentException.ThrowIfNullOrWhiteSpace( relativePath );

		return File.ReadAllText(
			System.IO.Path.Combine(
				root,
				relativePath.Replace(
					'/',
					System.IO.Path.DirectorySeparatorChar
				)
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
