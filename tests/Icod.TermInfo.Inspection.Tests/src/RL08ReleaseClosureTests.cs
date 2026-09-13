using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RL08ReleaseClosureTests {
	private const string InspectionApiSha256 =
		"69c7350d5d44d502ecf1698c8fe1c1336f03d38eb1a36e36219f50ac33585a86";
	private const string JsonV1SchemaSha256 =
		"76578f421b254802d24453af6868edaf8c23c4b78a87c7e8ef86b233ff0e8500";
	private const string JsonV2SchemaSha256 =
		"ae4d53608881344e902f02303c71e2d432500969e60cfb005d70feea607499d0";
	private const string JsonV3SchemaSha256 =
		"33ca95aee120f84d0d160ac189f8ddb4db183361b7bd83885c99c1c8ed355a97";

	[Fact]
	public void ExactOneElevenInspectionSurfaceIsFrozen() {
		string freeze = ReadRepositoryFile(
			"docs/1.11.0-INSPECTION-PUBLIC-API-FREEZE.md"
		);
		string fingerprints = ReadRepositoryFile(
			"docs/1.11.0-RL08-FREEZE-FINGERPRINTS.txt"
		);
		string additions = ReadRepositoryFile(
			"docs/1.11.0-INSPECTION-PUBLIC-API-ADDITIONS.txt"
		);
		string additiveMembers = ReadRepositoryFile(
			"docs/1.11.0-INSPECTION-PUBLIC-API-ADDITIVE-MEMBERS.txt"
		);
		string oneTwelveAdditions = ReadRepositoryFile(
			"docs/1.12.0-PG01-INSPECTION-PUBLIC-API-ADDITIONS.txt"
		);
		HashSet<string> approvedOneTwelveTypes = oneTwelveAdditions
			.Split( '\n' )
			.Select( line => line.Trim() )
			.Where(
				line =>
					line.Length > 0
					&& !line.StartsWith( "#", StringComparison.Ordinal )
			)
			.ToHashSet( StringComparer.Ordinal );
		Type[] currentTypes =
			typeof( PersistentRasterLifecycleProfile ).Assembly.GetExportedTypes();
		Type[] reconstructedOneElevenTypes = currentTypes
			.Where(
				type =>
					type.FullName is null
					|| !approvedOneTwelveTypes.Contains( type.FullName )
			)
			.ToArray();

		Assert.Equal( 67, reconstructedOneElevenTypes.Length );
		Assert.Equal(
			approvedOneTwelveTypes.Count,
			currentTypes.Count(
				type =>
					type.FullName?.StartsWith(
						"Icod.TermInfo.Inspection.PersistentRasterPlacement",
						StringComparison.Ordinal
					) == true
			)
		);
		foreach ( string approvedType in approvedOneTwelveTypes ) {
			Assert.Contains(
				currentTypes,
				type => string.Equals(
					type.FullName,
					approvedType,
					StringComparison.Ordinal
				)
			);
		}
		Assert.Contains(
			"Icod.TermInfo.Inspection.PersistentRasterPlacementSubject",
			oneTwelveAdditions,
			StringComparison.Ordinal
		);
		Assert.Contains( InspectionApiSha256, freeze, StringComparison.Ordinal );
		Assert.Contains( InspectionApiSha256, fingerprints, StringComparison.Ordinal );
		Assert.Contains(
			"Icod.TermInfo.Inspection.PersistentRasterLifecycleProfile",
			additions,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"PersistentRasterLifecycleSchemaIdentifier",
			additiveMembers,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void AllThreeJsonSchemasHaveExactFrozenFingerprints() {
		string v1 = ReadRepositoryFile(
			"docs/Icod.TermInfo.Inspection.schema.json"
		);
		string v2 = ReadRepositoryFile(
			"docs/Icod.TermInfo.Inspection.schema.v2.json"
		);
		string v3 = ReadRepositoryFile(
			"docs/Icod.TermInfo.Inspection.schema.v3.json"
		);
		string fingerprints = ReadRepositoryFile(
			"docs/1.11.0-RL08-FREEZE-FINGERPRINTS.txt"
		);

		Assert.Equal( JsonV1SchemaSha256, ComputeSha256( v1 ) );
		Assert.Equal( JsonV2SchemaSha256, ComputeSha256( v2 ) );
		Assert.Equal( JsonV3SchemaSha256, ComputeSha256( v3 ) );
		Assert.Contains( InspectionApiSha256, fingerprints, StringComparison.Ordinal );
		Assert.Contains( JsonV1SchemaSha256, fingerprints, StringComparison.Ordinal );
		Assert.Contains( JsonV2SchemaSha256, fingerprints, StringComparison.Ordinal );
		Assert.Contains( JsonV3SchemaSha256, fingerprints, StringComparison.Ordinal );
	}

	[Fact]
	public void ReleaseVerifierRequiresExactOneElevenAndRetainsOneTenCompatibility() {
		string shell = ReadRepositoryFile(
			".github/scripts/verify-release-package.sh"
		);
		string command = ReadRepositoryFile(
			".github/scripts/verify-release-package.cmd"
		);
		string compatibility = ReadRepositoryFile(
			".github/scripts/verify-inspection-compatibility.ps1"
		);

		foreach ( string verifier in new[] { shell, command } ) {
			Assert.Contains(
				"verify-inspection-compatibility.ps1",
				verifier,
				StringComparison.Ordinal
			);
		}
		Assert.Contains( InspectionApiSha256, compatibility, StringComparison.Ordinal );
		Assert.Contains(
			"1.10.0-INSPECTION-PUBLIC-API-BASELINE.txt",
			compatibility,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"1.11.0-INSPECTION-PUBLIC-API-ADDITIONS.txt",
			compatibility,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"1.11.0-INSPECTION-PUBLIC-API-ADDITIVE-MEMBERS.txt",
			compatibility,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"1.12.0-PG01-INSPECTION-PUBLIC-API-ADDITIONS.txt",
			compatibility,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void ReleaseFacingDocumentationAndMetadataDescribeOneEleven() {
		string rootReadme = ReadRepositoryFile( "README.md" );
		string inspectionReadme = ReadRepositoryFile(
			"Icod.TermInfo.Inspection/README.md"
		);
		string versioning = ReadRepositoryFile( "docs/VERSIONING.md" );
		string compatibility = ReadRepositoryFile( "docs/COMPATIBILITY.md" );
		string guide = ReadRepositoryFile(
			"docs/1.11.0-PERSISTENT-RASTER-LIFECYCLE-GUIDE.md"
		);
		string audit = ReadRepositoryFile(
			"docs/1.11.0-RELEASE-AUDIT.md"
		);
		string inspectionProject = ReadRepositoryFile(
			"Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj"
		);

		Assert.Contains( "1.11", rootReadme, StringComparison.Ordinal );
		Assert.Contains( "1.11", inspectionReadme, StringComparison.Ordinal );
		Assert.Contains( "## 1.11 release line", versioning, StringComparison.Ordinal );
		Assert.Contains( "## 1.11 compatibility freeze", compatibility, StringComparison.Ordinal );
		Assert.Contains( "PersistentRasterLifecycle", guide, StringComparison.Ordinal );
		Assert.Contains( "1.11.0-Alpha-8", audit, StringComparison.Ordinal );
		Assert.Contains(
			"<PackageReleaseNotes>1.11.0",
			inspectionProject,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void Rl07PackageAndSampleQualificationRemainPermanent() {
		string packageVerification = ReadRepositoryFile(
			"packaging/VerifyPackageArtifact.ps1"
		);
		string workflow = ReadRepositoryFile(
			".github/workflows/pull-request.yaml"
		);

		Assert.Contains(
			"smoke-rl07-package-consumer.ps1",
			packageVerification,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"Icod.TermInfo.PersistentRasterLifecycle.Sample.csproj",
			packageVerification,
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
		string relativePath
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( relativePath );
		string root = FindRepositoryRoot();
		return File.ReadAllText(
			Path.Combine(
				root,
				relativePath.Replace(
					'/',
					Path.DirectorySeparatorChar
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
