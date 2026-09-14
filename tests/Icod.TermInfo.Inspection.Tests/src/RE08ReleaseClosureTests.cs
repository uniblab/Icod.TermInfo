using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RE08ReleaseClosureTests {
	private const string OneTwelveInspectionApiSha256 =
		"f71501dcd27a530051c1a02083325144ced2b6173b6b967b9571c620815198f0";
	private const string OneThirteenInspectionApiSha256 =
		"fd827a25abafb8e9ff3915567f45f9f2ec51b332bfd8a82dd4e4bca20290e764";
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

	[Fact]
	public void ExactOneThirteenInspectionSurfaceHasFreezeInputs() {
		Type[] exportedTypes =
			typeof( PersistentRasterRuntimeObservationSet ).Assembly.GetExportedTypes();
		Assert.Equal( 90, exportedTypes.Length );
		Assert.Equal(
			9,
			exportedTypes.Count(
				type => type.FullName?.StartsWith(
					"Icod.TermInfo.Inspection.PersistentRasterRuntime",
					StringComparison.Ordinal
				) == true
			)
		);

		string freeze = ReadRequiredRepositoryFile(
			"docs/1.13.0-INSPECTION-PUBLIC-API-FREEZE.md"
		);
		string fingerprints = ReadRequiredRepositoryFile(
			"docs/1.13.0-RE08-FREEZE-FINGERPRINTS.txt"
		);
		Assert.Contains(
			OneThirteenInspectionApiSha256,
			freeze,
			StringComparison.Ordinal
		);
		Assert.Contains( "90 exported public types", freeze, StringComparison.Ordinal );
		Assert.Contains(
			OneThirteenInspectionApiSha256,
			fingerprints,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void AllFiveJsonSchemasHaveExactFrozenFingerprints() {
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
		];

		foreach ( (string path, string sha256) in schemas ) {
			Assert.Equal( sha256, NormalizedLfSha256( path ) );
		}

		string fingerprints = ReadRequiredRepositoryFile(
			"docs/1.13.0-RE08-FREEZE-FINGERPRINTS.txt"
		);
		foreach ( (string _, string sha256) in schemas ) {
			Assert.Contains( sha256, fingerprints, StringComparison.Ordinal );
		}
	}

	[Fact]
	public void RuntimeObservationBoundsOrdinalsAndSnapshotsRemainFrozen() {
		string maximumLabel = new(
			'x',
			PersistentRasterRuntimeObservationOptions.MaximumSourceLabelLength
		);
		List<PersistentRasterRuntimeLifecycleObservation> source =
			Enumerable.Range(
				0,
				PersistentRasterRuntimeObservationOptions.MaximumSupportedObservationCount
			)
			.Select(
				index => new PersistentRasterRuntimeLifecycleObservation(
					PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
					PersistentRasterRuntimeObservationOutcome.Supported,
					maximumLabel,
					(
						index
							== PersistentRasterRuntimeObservationOptions.MaximumSupportedObservationCount - 1
					)
						? int.MaxValue
						: index
				)
			)
			.ToList();
		PersistentRasterRuntimeObservationSet snapshot = new(
			source,
			Array.Empty<PersistentRasterRuntimePlacementObservation>(),
			new PersistentRasterRuntimeObservationOptions(
				PersistentRasterRuntimeObservationOptions.MaximumSupportedObservationCount
			)
		);
		source.Clear();

		Assert.Equal(
			PersistentRasterRuntimeObservationOptions.MaximumSupportedObservationCount,
			snapshot.Count
		);
		Assert.Equal(
			PersistentRasterRuntimeObservationOptions.MaximumSupportedObservationCount,
			snapshot.LifecycleObservations.Count
		);
		Assert.Equal( 0, snapshot.LifecycleObservations[ 0 ].SourceOrdinal );
		Assert.Equal( int.MaxValue, snapshot.LifecycleObservations[ ^1 ].SourceOrdinal );
		Assert.Equal( maximumLabel, snapshot.LifecycleObservations[ 0 ].SourceLabel );

		Assert.Throws<ArgumentException>(
			() => new PersistentRasterRuntimeObservationSet(
				Enumerable.Range(
					0,
					PersistentRasterRuntimeObservationOptions.MaximumSupportedObservationCount + 1
				)
				.Select(
					index => new PersistentRasterRuntimeLifecycleObservation(
						PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
						PersistentRasterRuntimeObservationOutcome.Inconclusive,
						"one-past",
						index
					)
				),
				Array.Empty<PersistentRasterRuntimePlacementObservation>(),
				new PersistentRasterRuntimeObservationOptions(
					PersistentRasterRuntimeObservationOptions.MaximumSupportedObservationCount
				)
			)
		);
		Assert.Throws<ArgumentException>(
			() => new PersistentRasterRuntimeLifecycleObservation(
				PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
				PersistentRasterRuntimeObservationOutcome.Supported,
				new string(
					'x',
					PersistentRasterRuntimeObservationOptions.MaximumSourceLabelLength + 1
				),
				0
			)
		);
	}

	[Fact]
	public void RuntimeIntegrationCapacityOrdinalAndContradictionRemainConservative() {
		PersistentRasterLifecycleEvidence[] atCapacity =
			Enumerable.Range(
				0,
				PersistentRasterLifecycleEvidenceOptions.MaximumSupportedEvidenceCount
			)
			.Select(
				index => new PersistentRasterLifecycleEvidence(
					PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
					isPositive: true,
					PersistentRasterLifecycleEvidenceKind.Declared,
					$"re08-capacity-{index:D4}",
					index
				)
			)
			.ToArray();
		PersistentRasterLifecycleProfile capacityProfile =
			PersistentRasterLifecycleClassifier.Classify(
				atCapacity,
				new PersistentRasterLifecycleEvidenceOptions(
					PersistentRasterLifecycleEvidenceOptions.MaximumSupportedEvidenceCount
				)
			);
		PersistentRasterRuntimeIntegrationResult capacityResult =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				capacityProfile,
				CreateEmptyPlacementProfile(),
				new PersistentRasterRuntimeObservationSet(
					new[] {
						new PersistentRasterRuntimeLifecycleObservation(
							PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
							PersistentRasterRuntimeObservationOutcome.Supported,
							"capacity-import",
							0
						),
					},
					Array.Empty<PersistentRasterRuntimePlacementObservation>()
				)
			);
		Assert.False( capacityResult.Succeeded );
		Assert.Empty( capacityResult.ImportedLifecycleEvidence );
		Assert.Equal( capacityProfile.Evidence, capacityResult.LifecycleProfile.Evidence );
		Assert.Contains(
			capacityResult.Issues,
			issue => issue.Kind
				== PersistentRasterRuntimeIntegrationIssueKind.LifecycleEvidenceCapacityExhausted
		);

		PersistentRasterLifecycleProfile ordinalProfile =
			PersistentRasterLifecycleClassifier.Classify(
				new[] {
					new PersistentRasterLifecycleEvidence(
						PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
						isPositive: true,
						PersistentRasterLifecycleEvidenceKind.Verified,
						"max-ordinal",
						int.MaxValue
					),
				}
			);
		PersistentRasterRuntimeIntegrationResult ordinalResult =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				ordinalProfile,
				CreateEmptyPlacementProfile(),
				new PersistentRasterRuntimeObservationSet(
					new[] {
						new PersistentRasterRuntimeLifecycleObservation(
							PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
							PersistentRasterRuntimeObservationOutcome.Supported,
							"ordinal-import",
							int.MaxValue
						),
					},
					Array.Empty<PersistentRasterRuntimePlacementObservation>()
				)
			);
		Assert.False( ordinalResult.Succeeded );
		Assert.Empty( ordinalResult.ImportedLifecycleEvidence );
		Assert.Contains(
			ordinalResult.Issues,
			issue => issue.Kind
				== PersistentRasterRuntimeIntegrationIssueKind.LifecycleOrdinalSpaceExhausted
		);

		PersistentRasterRuntimeIntegrationResult contradiction =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				CreateEmptyLifecycleProfile(),
				CreateEmptyPlacementProfile(),
				new PersistentRasterRuntimeObservationSet(
					new[] {
						new PersistentRasterRuntimeLifecycleObservation(
							PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
							PersistentRasterRuntimeObservationOutcome.Supported,
							"positive",
							0
						),
						new PersistentRasterRuntimeLifecycleObservation(
							PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
							PersistentRasterRuntimeObservationOutcome.Unsupported,
							"negative",
							1
						),
					},
					Array.Empty<PersistentRasterRuntimePlacementObservation>()
				)
			);
		Assert.True( contradiction.Succeeded );
		Assert.Equal( 2, contradiction.ImportedLifecycleEvidence.Count );
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Contradicted,
			contradiction.LifecycleProfile.PlacementCreation
		);
	}

	[Fact]
	public void RuntimeObservationRenderingIsRepeatedAndCultureIndependent() {
		PersistentRasterRuntimeObservationSet observations = new(
			new[] {
				new PersistentRasterRuntimeLifecycleObservation(
					PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
					PersistentRasterRuntimeObservationOutcome.Supported,
					"i",
					1
				),
				new PersistentRasterRuntimeLifecycleObservation(
					PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
					PersistentRasterRuntimeObservationOutcome.Supported,
					"I",
					0
				),
				new PersistentRasterRuntimeLifecycleObservation(
					PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
					PersistentRasterRuntimeObservationOutcome.Supported,
					"I",
					0
				),
				new PersistentRasterRuntimeLifecycleObservation(
					PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
					PersistentRasterRuntimeObservationOutcome.Inconclusive,
					"İ",
					2
				),
			},
			Array.Empty<PersistentRasterRuntimePlacementObservation>()
		);
		string invariant = TermInfoJsonRenderer.Render( observations );
		Assert.Equal( invariant, TermInfoJsonRenderer.Render( observations ) );

		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
		try {
			foreach ( string cultureName in new[] { "tr-TR", "fr-FR" } ) {
				CultureInfo culture = CultureInfo.GetCultureInfo( cultureName );
				CultureInfo.CurrentCulture = culture;
				CultureInfo.CurrentUICulture = culture;
				Assert.Equal( invariant, TermInfoJsonRenderer.Render( observations ) );
			}
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}
	}

	[Fact]
	public void ProductionDependencyAndRe07QualificationTopologyRemainFrozen() {
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
				&& (element.Attribute( "Include" )?.Value ?? string.Empty).Contains(
					"Icod.Terminal",
					StringComparison.Ordinal
				)
		);

		XDocument runtimeSmoke = XDocument.Load(
			Path.Combine(
				root,
				"tools",
				"runtime-evidence-package-smoke",
				"Icod.TermInfo.RuntimeEvidence.PackageSmoke.csproj"
			)
		);
		XElement[] packageReferences = runtimeSmoke
			.Descendants()
			.Where( element => element.Name.LocalName == "PackageReference" )
			.ToArray();
		Assert.Equal( 2, packageReferences.Length );
		Assert.Contains(
			packageReferences,
			element =>
				element.Attribute( "Include" )?.Value == "Icod.TermInfo.Inspection"
				&& element.Attribute( "Version" )?.Value
					== "$(IcodTermInfoInspectionPackageVersion)"
		);
		Assert.Contains(
			packageReferences,
			element =>
				element.Attribute( "Include" )?.Value == "Icod.Terminal"
				&& element.Attribute( "Version" )?.Value == "1.12.0"
		);
		Assert.DoesNotContain(
			runtimeSmoke.Descendants(),
			element => element.Name.LocalName == "ProjectReference"
		);

		string verification = ReadRequiredRepositoryFile(
			"packaging/VerifyPackageArtifact.ps1"
		);
		Assert.Contains(
			"smoke-re07-runtime-evidence-interop.ps1",
			verification,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"Icod.TermInfo.PersistentRasterRuntimeIntegration.Sample.csproj",
			verification,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void CompatibilityVerifierFreezesWholeOneThirteenBeforeReconstruction() {
		string verifier = ReadRequiredRepositoryFile(
			".github/scripts/verify-inspection-compatibility.ps1"
		);
		Assert.Contains(
			OneThirteenInspectionApiSha256,
			verifier,
			StringComparison.Ordinal
		);
		Assert.Contains(
			OneTwelveInspectionApiSha256,
			verifier,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"1.13.0-INSPECTION-PUBLIC-API-FREEZE.md",
			verifier,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"Verified exact 1.13 Inspection public API SHA-256",
			verifier,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void ReleaseDocumentationDescribesStableOneThirteen() {
		string rootReadme = ReadRequiredRepositoryFile( "README.md" );
		string inspectionReadme = ReadRequiredRepositoryFile(
			"Icod.TermInfo.Inspection/README.md"
		);
		string inspectionProject = ReadRequiredRepositoryFile(
			"Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj"
		);
		string versioning = ReadRequiredRepositoryFile( "docs/VERSIONING.md" );
		string compatibility = ReadRequiredRepositoryFile( "docs/COMPATIBILITY.md" );
		string roadmap = ReadRequiredRepositoryFile(
			"Icod.TermInfo-Post-1.0-Development-Roadmap.md"
		);
		string guide = ReadRequiredRepositoryFile(
			"docs/1.13.0-PERSISTENT-RASTER-RUNTIME-EVIDENCE-GUIDE.md"
		);
		string hardening = ReadRequiredRepositoryFile(
			"docs/1.13.0-RE08-RELEASE-HARDENING-AND-FREEZE.md"
		);
		string audit = ReadRequiredRepositoryFile(
			"docs/1.13.0-RELEASE-AUDIT.md"
		);

		Assert.Contains( "1.13", rootReadme, StringComparison.Ordinal );
		Assert.Contains( "1.13", inspectionReadme, StringComparison.Ordinal );
		Assert.Contains(
			"dotnet add package Icod.TermInfo.Inspection --version 1.13.0",
			rootReadme,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"<PackageReleaseNotes>1.13.0",
			inspectionProject,
			StringComparison.Ordinal
		);
		Assert.Contains( "## 1.13 release line", versioning, StringComparison.Ordinal );
		Assert.Contains(
			"## 1.13 compatibility freeze",
			compatibility,
			StringComparison.Ordinal
		);
		Assert.Contains( "1.13", roadmap, StringComparison.Ordinal );
		Assert.Contains( "PersistentRasterRuntime", guide, StringComparison.Ordinal );
		Assert.Contains( "1.13.0-Alpha-8", hardening, StringComparison.Ordinal );
		Assert.Contains( "1.13.0-Alpha-8", audit, StringComparison.Ordinal );
	}

	private static PersistentRasterLifecycleProfile CreateEmptyLifecycleProfile() {
		return PersistentRasterLifecycleClassifier.Classify(
			Array.Empty<PersistentRasterLifecycleEvidence>()
		);
	}

	private static PersistentRasterPlacementProfile CreateEmptyPlacementProfile() {
		return PersistentRasterPlacementClassifier.Classify(
			Array.Empty<PersistentRasterPlacementEvidence>()
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

	private static string ReadRequiredRepositoryFile(
		string relativePath
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( relativePath );
		string path = Path.Combine(
			FindRepositoryRoot(),
			relativePath.Replace(
				'/',
				Path.DirectorySeparatorChar
			)
		);
		Assert.True(
			File.Exists( path ),
			$"Required RE08 release-closure file is missing: {relativePath}"
		);
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
			"Could not locate repository root."
		);
	}
}
