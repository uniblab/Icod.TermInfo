using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RB08ReleaseClosureTests {
	private const string OneThirteenInspectionApiSha256 =
		"fd827a25abafb8e9ff3915567f45f9f2ec51b332bfd8a82dd4e4bca20290e764";
	private const string OneFourteenInspectionApiSha256 =
		"e9f240a562aec5274d64fb2ec3647862fe4ef5684582af2b442ba3c55e189497";
	private const string JsonV1SchemaSha256 =
		"76578f421b254802d24453af6868edaf8c23c4b78a87c7e8ef86b233ff0e8500";
	private const string JsonV2SchemaSha256 =
		"ae4d53608881344e902f02303c71e2d432500969e60cfb005d70feea607499d0";
	private const string JsonV3SchemaSha256 =
		"33ca95aee120f84d0d160ac189f8ddb4db183361b7bd83885c99c1c8ed355a97";
	private const string JsonV4SchemaSha256 =
		"6383052f389d903683a9e24d55b73c97eb165db89c6ab5f26dbcb5a3c7fdda87";
	private const string JsonV5SchemaSha256 =
		"a151c7915d8b637d8bb64ef9d649168a4c120d7b9b7104299b6a78dab1f0a394";
	private const string JsonV6SchemaSha256 =
		"9d51ec6659f8978c867408881eefb106f3c4976dfcf2da12b23c374a222665c2";

	[Fact]
	public void ExactOneFourteenInspectionSurfaceHasFreezeInputs() {
		Type[] currentTypes = typeof( RasterBackendCandidate ).Assembly.GetExportedTypes();

		Assert.Equal( 106, currentTypes.Length );

		string freeze = ReadRequiredRepositoryFile(
			"docs/1.14.0-INSPECTION-PUBLIC-API-FREEZE.md"
		);
		string fingerprints = ReadRequiredRepositoryFile(
			"docs/1.14.0-RB08-FREEZE-FINGERPRINTS.txt"
		);
		Assert.Contains(
			OneFourteenInspectionApiSha256,
			freeze,
			StringComparison.Ordinal
		);
		Assert.Contains( "106 exported public types", freeze, StringComparison.Ordinal );
		Assert.Contains(
			OneFourteenInspectionApiSha256,
			fingerprints,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void OneFourteenSurfaceRemainsExactlyAdditiveAboveFrozenOneThirteen() {
		HashSet<string> approvedTypes = new( StringComparer.Ordinal );
		foreach ( string path in new[] {
			"docs/1.14.0-RB01-INSPECTION-PUBLIC-API-ADDITIONS.txt",
			"docs/1.14.0-RB02-INSPECTION-PUBLIC-API-ADDITIONS.txt",
			"docs/1.14.0-RB03-INSPECTION-PUBLIC-API-ADDITIONS.txt",
		} ) {
			foreach ( string line in ReadRequiredRepositoryFile( path ).Split( '\n' ) ) {
				string candidate = line.Trim();
				if (
					candidate.Length > 0
					&& !candidate.StartsWith( "#", StringComparison.Ordinal )
				) {
					Assert.True( approvedTypes.Add( candidate ) );
				}
			}
		}

		Type[] currentTypes = typeof( RasterBackendCandidate ).Assembly.GetExportedTypes();
		Type[] reconstructedOneThirteenTypes = currentTypes
			.Where(
				type =>
					type.FullName is null
					|| !approvedTypes.Contains( type.FullName )
			)
			.ToArray();

		Assert.Equal( 16, approvedTypes.Count );
		Assert.Equal( 106, currentTypes.Length );
		Assert.Equal( 90, reconstructedOneThirteenTypes.Length );
		Assert.Equal(
			approvedTypes.Count,
			currentTypes.Length - reconstructedOneThirteenTypes.Length
		);

		string rb06Members = ReadRequiredRepositoryFile(
			"docs/1.14.0-RB06-INSPECTION-PUBLIC-API-ADDITIVE-MEMBERS.txt"
		);
		Assert.Equal(
			6,
			rb06Members.Split( '\n' ).Count(
				line =>
					line.StartsWith( "  FIELD ", StringComparison.Ordinal )
					|| line.StartsWith( "  METHOD ", StringComparison.Ordinal )
			)
		);

		string freeze = ReadRequiredRepositoryFile(
			"docs/1.14.0-INSPECTION-PUBLIC-API-FREEZE.md"
		);
		Assert.Contains(
			OneThirteenInspectionApiSha256,
			freeze,
			StringComparison.Ordinal
		);
		Assert.Contains( "RB01", freeze, StringComparison.Ordinal );
		Assert.Contains( "RB02", freeze, StringComparison.Ordinal );
		Assert.Contains( "RB03", freeze, StringComparison.Ordinal );
		Assert.Contains( "RB06", freeze, StringComparison.Ordinal );
	}

	[Fact]
	public void AllSixJsonSchemasHaveExactFrozenFingerprints() {
		string root = FindRepositoryRoot();
		(string Path, string Sha256)[] schemas = [
			(
				Path.Combine( root, "docs", "Icod.TermInfo.Inspection.schema.json" ),
				JsonV1SchemaSha256
			),
			(
				Path.Combine( root, "docs", "Icod.TermInfo.Inspection.schema.v2.json" ),
				JsonV2SchemaSha256
			),
			(
				Path.Combine( root, "docs", "Icod.TermInfo.Inspection.schema.v3.json" ),
				JsonV3SchemaSha256
			),
			(
				Path.Combine( root, "docs", "Icod.TermInfo.Inspection.schema.v4.json" ),
				JsonV4SchemaSha256
			),
			(
				Path.Combine( root, "docs", "Icod.TermInfo.Inspection.schema.v5.json" ),
				JsonV5SchemaSha256
			),
			(
				Path.Combine( root, "docs", "Icod.TermInfo.Inspection.schema.v6.json" ),
				JsonV6SchemaSha256
			),
		];

		foreach ( (string path, string sha256) in schemas ) {
			Assert.Equal( sha256, NormalizedLfSha256( path ) );
		}

		string fingerprints = ReadRequiredRepositoryFile(
			"docs/1.14.0-RB08-FREEZE-FINGERPRINTS.txt"
		);
		foreach ( (string _, string sha256) in schemas ) {
			Assert.Contains( sha256, fingerprints, StringComparison.Ordinal );
		}
	}

	[Fact]
	public void CompatibilityVerifierFreezesWholeOneFourteenBeforeHistoricalReconstruction() {
		string verifier = ReadRequiredRepositoryFile(
			".github/scripts/verify-inspection-compatibility.ps1"
		);

		Assert.Contains(
			OneFourteenInspectionApiSha256,
			verifier,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"oneFourteenApiSha256",
			verifier,
			StringComparison.OrdinalIgnoreCase
		);
		Assert.Contains(
			"oneThirteenApiSha256",
			verifier,
			StringComparison.OrdinalIgnoreCase
		);
	}

	[Fact]
	public void ProductionDependencyAndRb07QualificationTopologyRemainFrozen() {
		string root = FindRepositoryRoot();
		XDocument inspectionProject = XDocument.Load(
			Path.Combine(
				root,
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

		XDocument packageConsumer = XDocument.Load(
			Path.Combine(
				root,
				"tools",
				"raster-backend-selection-package-smoke",
				"Icod.TermInfo.RasterBackendSelection.PackageSmoke.csproj"
			)
		);
		Assert.Equal(
			"net8.0;net9.0;net10.0",
			Assert.Single(
				packageConsumer.Descendants(),
				element => element.Name.LocalName == "TargetFrameworks"
			).Value
		);
		Assert.Equal(
			"1.13.0",
			Assert.Single(
				packageConsumer.Descendants(),
				element =>
					element.Name.LocalName == "PackageReference"
					&& element.Attribute( "Include" )?.Value == "Icod.Terminal"
			).Attribute( "Version" )?.Value
		);
	}

	[Fact]
	public void ReleaseDocumentationDescribesStableOneFourteen() {
		string buildProperties = ReadRequiredRepositoryFile( "Directory.Build.props" );
		string rootReadme = ReadRequiredRepositoryFile( "README.md" );
		string inspectionReadme = ReadRequiredRepositoryFile(
			"Icod.TermInfo.Inspection/README.md"
		);
		string inspectionProject = ReadRequiredRepositoryFile(
			"Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj"
		);
		string samplesReadme = ReadRequiredRepositoryFile( "samples/README.md" );
		string versioning = ReadRequiredRepositoryFile( "docs/VERSIONING.md" );
		string compatibility = ReadRequiredRepositoryFile( "docs/COMPATIBILITY.md" );
		string longRange = ReadRequiredRepositoryFile(
			"Icod.TermInfo-Post-1.0-Development-Roadmap.md"
		);
		string trancheRoadmap = ReadRequiredRepositoryFile(
			"Icod.TermInfo-1.14.0-Raster-Backend-Capability-Evidence-Selection-and-Planning-Roadmap.md"
		);
		string guide = ReadRequiredRepositoryFile(
			"docs/1.14.0-RASTER-BACKEND-SELECTION-GUIDE.md"
		);
		string audit = ReadRequiredRepositoryFile(
			"docs/1.14.0-RELEASE-AUDIT.md"
		);

		Assert.Contains(
			"<IcodTermInfoSuiteVersion>1.15.0-Alpha-1</IcodTermInfoSuiteVersion>",
			buildProperties,
			StringComparison.Ordinal
		);
		Assert.Contains( "## Status", rootReadme, StringComparison.Ordinal );
		Assert.Contains( "Icod.TermInfo 1.14.0", rootReadme, StringComparison.Ordinal );
		Assert.Contains( "## Architecture", rootReadme, StringComparison.Ordinal );
		Assert.Contains( "## Quick Start", rootReadme, StringComparison.Ordinal );
		Assert.Contains( "## Feature Inventory", rootReadme, StringComparison.Ordinal );
		Assert.Contains( "## Packages and Tools", rootReadme, StringComparison.Ordinal );
		Assert.Contains(
			"## Design Boundaries and Guarantees",
			rootReadme,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"## Compatibility and Versioning",
			rootReadme,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"Release-by-release chronology belongs",
			rootReadme,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"dotnet add package Icod.TermInfo.Inspection --version 1.14.0",
			rootReadme,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"## 1.14 release status",
			inspectionReadme,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"<PackageReleaseNotes>1.14.0",
			inspectionProject,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"Icod.TermInfo.RasterBackendSelection.Sample",
			samplesReadme,
			StringComparison.Ordinal
		);
		Assert.Contains( "## 1.14 release line", versioning, StringComparison.Ordinal );
		Assert.Contains(
			"## 1.14 compatibility freeze",
			compatibility,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"**Current coordinated version:** `1.14.0`",
			longRange,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"**Latest completed line:** `1.14.0`",
			longRange,
			StringComparison.Ordinal
		);
		Assert.Contains( "**Status:** COMPLETE", trancheRoadmap, StringComparison.Ordinal );
		Assert.Contains( "RasterBackend", guide, StringComparison.Ordinal );
		Assert.Contains( "1.14.0-Alpha-8", audit, StringComparison.Ordinal );
	}

	[Fact]
	public void ReleaseAuthoritiesDocumentTheFrozenSelectionContract() {
		string guide = ReadRequiredRepositoryFile(
			"docs/1.14.0-RASTER-BACKEND-SELECTION-GUIDE.md"
		);
		string audit = ReadRequiredRepositoryFile(
			"docs/1.14.0-RELEASE-AUDIT.md"
		);
		string roadmap = ReadRequiredRepositoryFile(
			"Icod.TermInfo-1.14.0-Raster-Backend-Capability-Evidence-Selection-and-Planning-Roadmap.md"
		);
		string longRange = ReadRequiredRepositoryFile(
			"Icod.TermInfo-Post-1.0-Development-Roadmap.md"
		);

		foreach ( string token in new[] {
			"Sixel",
			"Kitty Graphics",
			"RequiresPreference",
			"RequiresRuntimeVerification",
			"Icod.Terminal 1.13.0",
			"JSON v6",
		} ) {
			Assert.Contains( token, guide, StringComparison.OrdinalIgnoreCase );
		}
		Assert.Contains( "RB08", audit, StringComparison.Ordinal );
		Assert.Contains( OneFourteenInspectionApiSha256, audit, StringComparison.Ordinal );
		Assert.Contains( JsonV6SchemaSha256, audit, StringComparison.Ordinal );
		Assert.Contains( "RB08 / Alpha-8", roadmap, StringComparison.Ordinal );
		Assert.Contains( "1.14.0", longRange, StringComparison.Ordinal );
	}

	private static string NormalizedLfSha256( string path ) {
		string normalized = File.ReadAllText( path )
			.Replace( "\r\n", "\n", StringComparison.Ordinal )
			.Replace( "\r", "\n", StringComparison.Ordinal )
			.TrimEnd( '\n' ) + "\n";
		byte[] bytes = Encoding.UTF8.GetBytes( normalized );
		byte[] hash = SHA256.HashData( bytes );
		return Convert.ToHexString( hash ).ToLowerInvariant();
	}

	private static string ReadRequiredRepositoryFile( string relativePath ) {
		string path = Path.Combine(
			FindRepositoryRoot(),
			relativePath.Replace( '/', Path.DirectorySeparatorChar )
		);
		Assert.True( File.Exists( path ), $"Required repository file missing: {relativePath}" );
		return File.ReadAllText( path );
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
