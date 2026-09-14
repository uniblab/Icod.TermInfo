using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RE06PersistentRasterRuntimeJsonTests {
	[Fact]
	public void RuntimeObservationSetRendersExactVersionFiveDocument() {
		PersistentRasterRuntimeObservationSet observations =
			CreateObservationSetFixture();

		string json = TermInfoJsonRenderer.Render( observations );

		Assert.Equal(
			"{\"schema\":\"urn:icod:terminfo:inspection:json:5\",\"schemaVersion\":5,\"documentKind\":\"persistentRasterRuntimeObservationSet\",\"data\":{\"observationCount\":3,\"lifecycleObservationCount\":2,\"placementObservationCount\":1,\"lifecycleObservations\":[{\"subject\":\"persistentUpload\",\"outcome\":\"supported\",\"sourceLabel\":\"runtime-a\",\"sourceOrdinal\":4},{\"subject\":\"resourceDeletion\",\"outcome\":\"inconclusive\",\"sourceLabel\":\"runtime-b\",\"sourceOrdinal\":8}],\"placementObservations\":[{\"subject\":\"signedZOrder\",\"outcome\":\"unsupported\",\"sourceLabel\":\"runtime-z\",\"sourceOrdinal\":2}]}}",
			json
		);
	}

	[Fact]
	public void RuntimeIntegrationRendersExactVersionFiveAuditDocument() {
		PersistentRasterRuntimeIntegrationResult integration =
			CreateIntegrationFixture();

		string json = TermInfoJsonRenderer.Render( integration );

		Assert.Equal(
			"{\"schema\":\"urn:icod:terminfo:inspection:json:5\",\"schemaVersion\":5,\"documentKind\":\"persistentRasterRuntimeIntegration\",\"data\":{\"succeeded\":true,\"observationCount\":4,\"observations\":{\"lifecycle\":[{\"subject\":\"persistentUpload\",\"outcome\":\"supported\",\"sourceLabel\":\"lifecycle-ok\",\"sourceOrdinal\":1},{\"subject\":\"resourceDeletion\",\"outcome\":\"inconclusive\",\"sourceLabel\":\"lifecycle-question\",\"sourceOrdinal\":2}],\"placement\":[{\"subject\":\"sourceRectangle\",\"outcome\":\"unsupported\",\"sourceLabel\":\"placement-no\",\"sourceOrdinal\":0},{\"subject\":\"signedZOrder\",\"outcome\":\"inconclusive\",\"sourceLabel\":\"placement-question\",\"sourceOrdinal\":3}]},\"importedLifecycleEvidence\":[{\"subject\":\"persistentUpload\",\"isPositive\":true,\"kind\":\"verified\",\"sourceLabel\":\"lifecycle-ok\",\"sourceOrdinal\":0}],\"importedPlacementEvidence\":[{\"subject\":\"sourceRectangle\",\"isPositive\":false,\"kind\":\"verified\",\"sourceLabel\":\"placement-no\",\"sourceOrdinal\":0}],\"inconclusiveLifecycleObservations\":[{\"subject\":\"resourceDeletion\",\"outcome\":\"inconclusive\",\"sourceLabel\":\"lifecycle-question\",\"sourceOrdinal\":2}],\"inconclusivePlacementObservations\":[{\"subject\":\"signedZOrder\",\"outcome\":\"inconclusive\",\"sourceLabel\":\"placement-question\",\"sourceOrdinal\":3}],\"issues\":[],\"lifecycleStates\":{\"rasterDisplay\":\"unknown\",\"persistentUpload\":\"supported\",\"acknowledgedUpload\":\"unknown\",\"placementCreation\":\"unknown\",\"multiplePlacements\":\"unknown\",\"placementUpdate\":\"unknown\",\"placementDeletion\":\"unknown\",\"resourceDeletion\":\"unknown\"},\"placementStates\":{\"sourceRectangle\":\"unsupported\",\"signedZOrder\":\"unknown\"}}}",
			json
		);
	}

	[Fact]
	public void RuntimeIntegrationIssuesRenderStructuredDeterministicRecords() {
		PersistentRasterLifecycleProfile lifecycleProfile =
			PersistentRasterLifecycleClassifier.Classify(
				new[] {
					new PersistentRasterLifecycleEvidence(
						PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
						true,
						PersistentRasterLifecycleEvidenceKind.Verified,
						"max-ordinal",
						int.MaxValue
					),
				}
			);
		PersistentRasterRuntimeIntegrationResult integration =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				lifecycleProfile,
				CreateEmptyPlacementProfile(),
				new PersistentRasterRuntimeObservationSet(
					new[] {
						new PersistentRasterRuntimeLifecycleObservation(
							PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
							PersistentRasterRuntimeObservationOutcome.Supported,
							"cannot-import",
							0
						),
					},
					Array.Empty<PersistentRasterRuntimePlacementObservation>()
				)
			);

		using JsonDocument document = JsonDocument.Parse(
			TermInfoJsonRenderer.Render( integration )
		);
		JsonElement issue = document.RootElement
			.GetProperty( "data" )
			.GetProperty( "issues" )[ 0 ];
		Assert.Equal(
			"lifecycleOrdinalSpaceExhausted",
			issue.GetProperty( "kind" ).GetString()
		);
		Assert.Equal( 1, issue.GetProperty( "existingEvidenceCount" ).GetInt32() );
		Assert.Equal( 1, issue.GetProperty( "requestedImportCount" ).GetInt32() );
	}

	[Fact]
	public void RuntimeJsonVersionIdentityIsFrozen() {
		Assert.Equal(
			"urn:icod:terminfo:inspection:json:5",
			TermInfoJsonRenderer.PersistentRasterRuntimeSchemaIdentifier
		);
		Assert.Equal(
			5,
			TermInfoJsonRenderer.PersistentRasterRuntimeSchemaVersion
		);
	}

	[Fact]
	public void RuntimeObservationRenderingIsDeterministicBoundedCancelableAndCultureIndependent() {
		PersistentRasterRuntimeObservationSet observations =
			CreateObservationSetFixture();
		string invariant = TermInfoJsonRenderer.Render( observations );
		Assert.Equal( invariant, TermInfoJsonRenderer.Render( observations ) );

		AssertCultureIndependent(
			invariant,
			() => TermInfoJsonRenderer.Render( observations )
		);

		int exactBytes = Encoding.UTF8.GetByteCount( invariant );
		Assert.Equal(
			invariant,
			TermInfoJsonRenderer.Render(
				observations,
				new TermInfoJsonRendererOptions( exactBytes )
			)
		);
		Assert.Throws<InvalidOperationException>(
			() => TermInfoJsonRenderer.Render(
				observations,
				new TermInfoJsonRendererOptions( exactBytes - 1 )
			)
		);

		using CancellationTokenSource cancellation = new();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>(
			() => TermInfoJsonRenderer.Render(
				observations,
				new TermInfoJsonRendererOptions(),
				cancellation.Token
			)
		);
	}

	[Fact]
	public void RuntimeIntegrationRenderingIsDeterministicBoundedCancelableAndCultureIndependent() {
		PersistentRasterRuntimeIntegrationResult integration =
			CreateIntegrationFixture();
		string invariant = TermInfoJsonRenderer.Render( integration );
		Assert.Equal( invariant, TermInfoJsonRenderer.Render( integration ) );

		AssertCultureIndependent(
			invariant,
			() => TermInfoJsonRenderer.Render( integration )
		);

		int exactBytes = Encoding.UTF8.GetByteCount( invariant );
		Assert.Equal(
			invariant,
			TermInfoJsonRenderer.Render(
				integration,
				new TermInfoJsonRendererOptions( exactBytes )
			)
		);
		Assert.Throws<InvalidOperationException>(
			() => TermInfoJsonRenderer.Render(
				integration,
				new TermInfoJsonRendererOptions( exactBytes - 1 )
			)
		);

		using CancellationTokenSource cancellation = new();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>(
			() => TermInfoJsonRenderer.Render(
				integration,
				new TermInfoJsonRendererOptions(),
				cancellation.Token
			)
		);
	}

	[Fact]
	public void VersionFiveSchemaAndPackageWiringAreFrozen() {
		string root = FindRepositoryRoot();
		string schemaPath = Path.Combine(
			root,
			"docs",
			"Icod.TermInfo.Inspection.schema.v5.json"
		);
		using JsonDocument schema = JsonDocument.Parse(
			File.ReadAllText( schemaPath )
		);
		Assert.Equal(
			"urn:icod:terminfo:inspection:json:5",
			schema.RootElement.GetProperty( "$id" ).GetString()
		);
		string[] documentReferences = schema.RootElement
			.GetProperty( "oneOf" )
			.EnumerateArray()
			.Select( branch => branch.GetProperty( "$ref" ).GetString() )
			.Cast<string>()
			.ToArray();
		Assert.Equal(
			new[] {
				"#/$defs/persistentRasterRuntimeObservationSetDocument",
				"#/$defs/persistentRasterRuntimeIntegrationDocument",
			},
			documentReferences
		);

		string projectText = File.ReadAllText(
			Path.Combine(
				root,
				"Icod.TermInfo.Inspection",
				"Icod.TermInfo.Inspection.csproj"
			)
		);
		Assert.Contains(
			"Icod.TermInfo.Inspection.schema.v5.json",
			projectText,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void PreviousSchemaFingerprintsRemainFrozen() {
		string root = FindRepositoryRoot();
		Assert.Equal(
			"76578f421b254802d24453af6868edaf8c23c4b78a87c7e8ef86b233ff0e8500",
			NormalizedLfSha256(
				Path.Combine( root, "docs", "Icod.TermInfo.Inspection.schema.json" )
			)
		);
		Assert.Equal(
			"ae4d53608881344e902f02303c71e2d432500969e60cfb005d70feea607499d0",
			NormalizedLfSha256(
				Path.Combine( root, "docs", "Icod.TermInfo.Inspection.schema.v2.json" )
			)
		);
		Assert.Equal(
			"33ca95aee120f84d0d160ac189f8ddb4db183361b7bd83885c99c1c8ed355a97",
			NormalizedLfSha256(
				Path.Combine( root, "docs", "Icod.TermInfo.Inspection.schema.v3.json" )
			)
		);
		Assert.Equal(
			"6383052f389d903683a9e24d55b73c97eb165db89c6ab5f26dbcb5a3c7fdda87",
			NormalizedLfSha256(
				Path.Combine( root, "docs", "Icod.TermInfo.Inspection.schema.v4.json" )
			)
		);
	}

	private static PersistentRasterRuntimeObservationSet CreateObservationSetFixture() {
		return new PersistentRasterRuntimeObservationSet(
			new[] {
				new PersistentRasterRuntimeLifecycleObservation(
					PersistentRasterLifecycleEvidenceSubject.ResourceDeletion,
					PersistentRasterRuntimeObservationOutcome.Inconclusive,
					"runtime-b",
					8
				),
				new PersistentRasterRuntimeLifecycleObservation(
					PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
					PersistentRasterRuntimeObservationOutcome.Supported,
					"runtime-a",
					4
				),
			},
			new[] {
				new PersistentRasterRuntimePlacementObservation(
					PersistentRasterPlacementSubject.SignedZOrder,
					PersistentRasterRuntimeObservationOutcome.Unsupported,
					"runtime-z",
					2
				),
			}
		);
	}

	private static PersistentRasterRuntimeIntegrationResult CreateIntegrationFixture() {
		PersistentRasterRuntimeObservationSet observations =
			new PersistentRasterRuntimeObservationSet(
				new[] {
					new PersistentRasterRuntimeLifecycleObservation(
						PersistentRasterLifecycleEvidenceSubject.ResourceDeletion,
						PersistentRasterRuntimeObservationOutcome.Inconclusive,
						"lifecycle-question",
						2
					),
					new PersistentRasterRuntimeLifecycleObservation(
						PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
						PersistentRasterRuntimeObservationOutcome.Supported,
						"lifecycle-ok",
						1
					),
				},
				new[] {
					new PersistentRasterRuntimePlacementObservation(
						PersistentRasterPlacementSubject.SignedZOrder,
						PersistentRasterRuntimeObservationOutcome.Inconclusive,
						"placement-question",
						3
					),
					new PersistentRasterRuntimePlacementObservation(
						PersistentRasterPlacementSubject.SourceRectangle,
						PersistentRasterRuntimeObservationOutcome.Unsupported,
						"placement-no",
						0
					),
				}
			);
		return PersistentRasterRuntimeEvidenceIntegrator.Integrate(
			PersistentRasterLifecycleClassifier.Classify(
				Array.Empty<PersistentRasterLifecycleEvidence>()
			),
			CreateEmptyPlacementProfile(),
			observations
		);
	}

	private static PersistentRasterPlacementProfile CreateEmptyPlacementProfile() {
		return PersistentRasterPlacementClassifier.Classify(
			Array.Empty<PersistentRasterPlacementEvidence>()
		);
	}

	private static void AssertCultureIndependent(
		string expected,
		Func<string> render
	) {
		CultureInfo previousCulture = CultureInfo.CurrentCulture;
		CultureInfo previousUiCulture = CultureInfo.CurrentUICulture;
		try {
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "tr-TR" );
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "tr-TR" );
			Assert.Equal( expected, render() );
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "fr-FR" );
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "fr-FR" );
			Assert.Equal( expected, render() );
		} finally {
			CultureInfo.CurrentCulture = previousCulture;
			CultureInfo.CurrentUICulture = previousUiCulture;
		}
	}

	private static string NormalizedLfSha256(
		string path
	) {
		string text = File.ReadAllText( path )
			.Replace( "\r\n", "\n", StringComparison.Ordinal )
			.Replace( '\r', '\n' );
		return Convert.ToHexString(
			SHA256.HashData(
				Encoding.UTF8.GetBytes( text )
			)
		).ToLowerInvariant();
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current = new( AppContext.BaseDirectory );
		while ( current is not null ) {
			if ( File.Exists( Path.Combine( current.FullName, "Icod.TermInfo.csproj" ) ) ) {
				return current.FullName;
			}
			current = current.Parent;
		}
		throw new InvalidOperationException( "Repository root not found." );
	}
}
