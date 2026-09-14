using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class PG08ReleaseClosureTests {
	private const string InspectionApiSha256 =
		"f71501dcd27a530051c1a02083325144ced2b6173b6b967b9571c620815198f0";
	private const string JsonV1SchemaSha256 =
		"76578f421b254802d24453af6868edaf8c23c4b78a87c7e8ef86b233ff0e8500";
	private const string JsonV2SchemaSha256 =
		"ae4d53608881344e902f02303c71e2d432500969e60cfb005d70feea607499d0";
	private const string JsonV3SchemaSha256 =
		"33ca95aee120f84d0d160ac189f8ddb4db183361b7bd83885c99c1c8ed355a97";
	private const string JsonV4SchemaSha256 =
		"6383052f389d903683a9e24d55b73c97eb165db89c6ab5f26dbcb5a3c7fdda87";

	[Fact]
	public void ExactOneTwelveInspectionSurfaceIsFrozen() {
		string freeze = ReadRepositoryFile(
			"docs/1.12.0-INSPECTION-PUBLIC-API-FREEZE.md"
		);
		string fingerprints = ReadRepositoryFile(
			"docs/1.12.0-PG08-FREEZE-FINGERPRINTS.txt"
		);
		string additions = ReadRepositoryFile(
			"docs/1.12.0-PG01-INSPECTION-PUBLIC-API-ADDITIONS.txt"
		);
		string additiveMembers = ReadRepositoryFile(
			"docs/1.12.0-PG06-INSPECTION-PUBLIC-API-ADDITIVE-MEMBERS.txt"
		);
		string oneThirteenAdditions = ReadRepositoryFile(
			"docs/1.13.0-RE01-INSPECTION-PUBLIC-API-ADDITIONS.txt"
		);
		string oneFourteenAdditions = ReadRepositoryFile(
			"docs/1.14.0-RB01-INSPECTION-PUBLIC-API-ADDITIONS.txt"
		);
		HashSet<string> approvedOneFourteenTypes = oneFourteenAdditions
			.Split( '\n' )
			.Select( line => line.Trim() )
			.Where(
				line =>
					line.Length > 0
					&& !line.StartsWith( "#", StringComparison.Ordinal )
			)
			.ToHashSet( StringComparer.Ordinal );
		HashSet<string> approvedOneThirteenTypes = oneThirteenAdditions
			.Split( '\n' )
			.Select( line => line.Trim() )
			.Where(
				line =>
					line.Length > 0
					&& !line.StartsWith( "#", StringComparison.Ordinal )
			)
			.ToHashSet( StringComparer.Ordinal );
		Type[] currentTypes =
			typeof( PersistentRasterPlacementProfile ).Assembly.GetExportedTypes();
		Type[] reconstructedOneThirteenTypes = currentTypes
			.Where(
				type =>
					type.FullName is null
					|| !approvedOneFourteenTypes.Contains( type.FullName )
			)
			.ToArray();
		Type[] reconstructedOneTwelveTypes = reconstructedOneThirteenTypes
			.Where(
				type =>
					type.FullName is null
					|| !approvedOneThirteenTypes.Contains( type.FullName )
			)
			.ToArray();
		string compatibility = ReadRepositoryFile(
			".github/scripts/verify-inspection-compatibility.ps1"
		);

		Assert.Equal( 13, approvedOneFourteenTypes.Count );
		Assert.Equal( 81, reconstructedOneTwelveTypes.Length );
		Assert.Equal(
			approvedOneThirteenTypes.Count,
			reconstructedOneThirteenTypes.Count(
				type =>
					type.FullName?.StartsWith(
						"Icod.TermInfo.Inspection.PersistentRasterRuntime",
						StringComparison.Ordinal
					) == true
			)
		);
		foreach ( string approvedType in approvedOneThirteenTypes ) {
			Assert.Contains(
				reconstructedOneThirteenTypes,
				type => string.Equals(
					type.FullName,
					approvedType,
					StringComparison.Ordinal
				)
			);
		}
		Assert.Contains( InspectionApiSha256, freeze, StringComparison.Ordinal );
		Assert.Contains( InspectionApiSha256, fingerprints, StringComparison.Ordinal );
		Assert.Contains( InspectionApiSha256, compatibility, StringComparison.Ordinal );
		Assert.Contains(
			"Icod.TermInfo.Inspection.PersistentRasterPlacementProfile",
			additions,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"PersistentRasterPlacementSchemaIdentifier",
			additiveMembers,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void AllFourJsonSchemasHaveExactFrozenFingerprints() {
		string root = FindRepositoryRoot();
		string fingerprints = ReadRepositoryFile(
			"docs/1.12.0-PG08-FREEZE-FINGERPRINTS.txt"
		);
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
		];

		foreach ( (string path, string sha256) in schemas ) {
			Assert.Equal( sha256, NormalizedLfSha256( path ) );
			Assert.Contains( sha256, fingerprints, StringComparison.Ordinal );
		}
		Assert.Contains( InspectionApiSha256, fingerprints, StringComparison.Ordinal );
	}

	[Fact]
	public void ReleaseFacingMetadataDescribesStableOneTwelve() {
		string rootReadme = ReadRepositoryFile( "README.md" );
		string inspectionReadme = ReadRepositoryFile(
			"Icod.TermInfo.Inspection/README.md"
		);
		string versioning = ReadRepositoryFile( "docs/VERSIONING.md" );
		string compatibility = ReadRepositoryFile( "docs/COMPATIBILITY.md" );
		string guide = ReadRepositoryFile(
			"docs/1.12.0-ADVANCED-PERSISTENT-RASTER-PLACEMENT-GUIDE.md"
		);
		string hardening = ReadRepositoryFile(
			"docs/1.12.0-PG08-RELEASE-HARDENING-AND-FREEZE.md"
		);
		string audit = ReadRepositoryFile(
			"docs/1.12.0-RELEASE-AUDIT.md"
		);

		Assert.Contains( "1.12", rootReadme, StringComparison.Ordinal );
		Assert.Contains( "1.12", inspectionReadme, StringComparison.Ordinal );
		Assert.Contains( "## 1.12 release line", versioning, StringComparison.Ordinal );
		Assert.Contains( "## 1.12 compatibility freeze", compatibility, StringComparison.Ordinal );
		Assert.Contains( "PersistentRasterPlacement", guide, StringComparison.Ordinal );
		Assert.Contains( "1.12.0-Alpha-8", hardening, StringComparison.Ordinal );
		Assert.Contains( "1.12.0-Alpha-8", audit, StringComparison.Ordinal );
		Assert.Contains(
			"The coordinated version is `1.12.0`",
			audit,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void PackageGraphAndPg07QualificationRemainFrozen() {
		string root = FindRepositoryRoot();
		XDocument inspectionProject = XDocument.Load(
			Path.Combine(
				root,
				"Icod.TermInfo.Inspection",
				"Icod.TermInfo.Inspection.csproj"
			)
		);
		Assert.DoesNotContain(
			inspectionProject.Descendants(),
			element =>
				(element.Name.LocalName == "PackageReference"
					|| element.Name.LocalName == "ProjectReference")
				&& string.Equals(
					element.Attribute( "Include" )?.Value,
					"Icod.Terminal",
					StringComparison.Ordinal
				)
		);

		XDocument lifecycleSmoke = XDocument.Load(
			Path.Combine(
				root,
				"tools",
				"inspection-package-smoke",
				"Icod.TermInfo.Inspection.PackageSmoke.csproj"
			)
		);
		XElement lifecyclePackageReference = Assert.Single(
			lifecycleSmoke.Descendants(),
			element => element.Name.LocalName == "PackageReference"
		);
		Assert.Equal(
			"Icod.TermInfo.Inspection",
			lifecyclePackageReference.Attribute( "Include" )?.Value
		);

		XDocument placementSmoke = XDocument.Load(
			Path.Combine(
				root,
				"tools",
				"placement-interop-package-smoke",
				"Icod.TermInfo.PlacementInterop.PackageSmoke.csproj"
			)
		);
		string[] placementPackages = placementSmoke
			.Descendants()
			.Where( element => element.Name.LocalName == "PackageReference" )
			.Select( element => element.Attribute( "Include" )?.Value )
			.Cast<string>()
			.OrderBy( value => value, StringComparer.Ordinal )
			.ToArray();
		Assert.Equal(
			new[] {
				"Icod.TermInfo.Inspection",
				"Icod.Terminal",
			},
			placementPackages
		);

		string verification = ReadRepositoryFile(
			"packaging/VerifyPackageArtifact.ps1"
		);
		Assert.Contains(
			"smoke-pg07-placement-interop.ps1",
			verification,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"Icod.TermInfo.PersistentRasterPlacement.Sample.csproj",
			verification,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void EvidenceMaximumBoundaryAndExtremeOrdinalRemainDeterministic() {
		PersistentRasterPlacementEvidence[] maximum =
			Enumerable.Range(
				0,
				PersistentRasterPlacementEvidenceOptions.MaximumSupportedEvidenceCount
			)
			.Select(
				index => new PersistentRasterPlacementEvidence(
					PersistentRasterPlacementSubject.SourceRectangle,
					isPositive: true,
					PersistentRasterPlacementEvidenceKind.Declared,
					$"pg08-{index:D4}",
					index == PersistentRasterPlacementEvidenceOptions.MaximumSupportedEvidenceCount - 1 ? int.MaxValue : index
				)
			)
			.ToArray();

		IReadOnlyList<PersistentRasterPlacementEvidence> snapshot =
			PersistentRasterPlacementEvidence.Snapshot(
				maximum,
				new PersistentRasterPlacementEvidenceOptions(
					PersistentRasterPlacementEvidenceOptions.MaximumSupportedEvidenceCount
				)
			);
		Assert.Equal(
			PersistentRasterPlacementEvidenceOptions.MaximumSupportedEvidenceCount,
			snapshot.Count
		);
		Assert.Equal( 0, snapshot[ 0 ].SourceOrdinal );
		Assert.Equal( int.MaxValue, snapshot[ ^1 ].SourceOrdinal );

		Assert.Throws<ArgumentException>(
			() => PersistentRasterPlacementEvidence.Snapshot(
				maximum.Append(
					new PersistentRasterPlacementEvidence(
						PersistentRasterPlacementSubject.SignedZOrder,
						isPositive: true,
						PersistentRasterPlacementEvidenceKind.Verified,
						"overflow",
						int.MaxValue
					)
				),
				new PersistentRasterPlacementEvidenceOptions(
					PersistentRasterPlacementEvidenceOptions.MaximumSupportedEvidenceCount
				)
			)
		);
	}

	[Fact]
	public void UnknownContradictionAndDefaultRequestRemainConservative() {
		PersistentRasterPlacementProfile unknown =
			PersistentRasterPlacementClassifier.Classify(
				Array.Empty<PersistentRasterPlacementEvidence>()
			);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unknown,
			unknown.SourceRectangle
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unknown,
			unknown.SignedZOrder
		);
		Assert.Throws<ArgumentException>(
			() => new PersistentRasterPlacementRequest()
		);

		PersistentRasterPlacementProfile contradicted =
			PersistentRasterPlacementClassifier.Classify(
				new[] {
					new PersistentRasterPlacementEvidence(
						PersistentRasterPlacementSubject.SignedZOrder,
						isPositive: false,
						PersistentRasterPlacementEvidenceKind.Verified,
						"negative",
						0
					),
					new PersistentRasterPlacementEvidence(
						PersistentRasterPlacementSubject.SignedZOrder,
						isPositive: true,
						PersistentRasterPlacementEvidenceKind.Verified,
						"positive",
						1
					),
				}
			);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Contradicted,
			contradicted.SignedZOrder
		);
	}

	private static string NormalizedLfSha256(
		string path
	) {
		string text =
			File.ReadAllText( path )
				.Replace( "\r\n", "\n", StringComparison.Ordinal )
				.Replace( '\r', '\n' );
		return Convert.ToHexString(
			SHA256.HashData(
				Encoding.UTF8.GetBytes( text )
			)
		).ToLowerInvariant();
	}

	private static string ReadRepositoryFile(
		string relativePath
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( relativePath );
		return File.ReadAllText(
			Path.Combine(
				FindRepositoryRoot(),
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
