using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class PG06PersistentRasterPlacementJsonTests {
	[Fact]
	public void PlacementProfileRendersExactVersionFourDocument() {
		PersistentRasterPlacementProfile profile =
			PersistentRasterPlacementClassifier.Classify(
				new[] {
					new PersistentRasterPlacementEvidence(
						PersistentRasterPlacementSubject.SourceRectangle,
						true,
						PersistentRasterPlacementEvidenceKind.Declared,
						"fixture",
						0
					),
					new PersistentRasterPlacementEvidence(
						PersistentRasterPlacementSubject.SignedZOrder,
						false,
						PersistentRasterPlacementEvidenceKind.Verified,
						"runtime",
						1
					),
				}
			);

		string json = TermInfoJsonRenderer.Render( profile );

		Assert.Equal(
			"{\"schema\":\"urn:icod:terminfo:inspection:json:4\",\"schemaVersion\":4,\"documentKind\":\"persistentRasterPlacementProfile\",\"data\":{\"states\":{\"sourceRectangle\":\"supported\",\"signedZOrder\":\"unsupported\"},\"evidenceCount\":2,\"evidence\":[{\"subject\":\"sourceRectangle\",\"isPositive\":true,\"kind\":\"declared\",\"sourceLabel\":\"fixture\",\"sourceOrdinal\":0},{\"subject\":\"signedZOrder\",\"isPositive\":false,\"kind\":\"verified\",\"sourceLabel\":\"runtime\",\"sourceOrdinal\":1}]}}",
			json
		);
	}

	[Fact]
	public void PlacementPlanRendersExactVersionFourDocument() {
		PersistentRasterLifecycleProfile lifecycleProfile =
			PersistentRasterLifecycleClassifier.Classify(
				Array.Empty<PersistentRasterLifecycleEvidence>()
			);
		PersistentRasterLifecyclePlan lifecyclePlan =
			PersistentRasterLifecyclePlanner.Plan(
				lifecycleProfile,
				new PersistentRasterLifecycleRequest( placementCount: 1 )
			);
		PersistentRasterPlacementProfile placementProfile =
			PersistentRasterPlacementClassifier.Classify(
				new[] {
					new PersistentRasterPlacementEvidence(
						PersistentRasterPlacementSubject.SourceRectangle,
						true,
						PersistentRasterPlacementEvidenceKind.Verified,
						"runtime",
						0
					),
				}
			);
		PersistentRasterPlacementPlan plan =
			PersistentRasterPlacementPlanner.Plan(
				lifecyclePlan,
				placementProfile,
				new PersistentRasterPlacementRequest(
					requireSourceRectangle: true
				)
			);

		string json = TermInfoJsonRenderer.Render( plan );

		Assert.Equal(
			"{\"schema\":\"urn:icod:terminfo:inspection:json:4\",\"schemaVersion\":4,\"documentKind\":\"persistentRasterPlacementPlan\",\"data\":{\"status\":\"indeterminate\",\"lifecycleStatus\":\"indeterminate\",\"requiresRuntimeVerification\":false,\"requirementCount\":1,\"issueCount\":0,\"requirements\":[{\"subject\":\"sourceRectangle\",\"supportStatus\":\"supported\",\"requiresRuntimeVerification\":false}],\"issues\":[]}}",
			json
		);
	}

	[Fact]
	public void PlacementJsonVersionIdentityIsFrozen() {
		Assert.Equal(
			"urn:icod:terminfo:inspection:json:4",
			TermInfoJsonRenderer.PersistentRasterPlacementSchemaIdentifier
		);
		Assert.Equal(
			4,
			TermInfoJsonRenderer.PersistentRasterPlacementSchemaVersion
		);
	}

	[Fact]
	public void PlacementRenderingIsDeterministicBoundedCancelableAndCultureIndependent() {
		PersistentRasterPlacementProfile profile =
			PersistentRasterPlacementClassifier.Classify(
				new[] {
					new PersistentRasterPlacementEvidence(
						PersistentRasterPlacementSubject.SourceRectangle,
						true,
						PersistentRasterPlacementEvidenceKind.Declared,
						"İ-runtime",
						0
					),
				}
			);
		string invariant = TermInfoJsonRenderer.Render( profile );
		Assert.Equal( invariant, TermInfoJsonRenderer.Render( profile ) );

		CultureInfo previousCulture = CultureInfo.CurrentCulture;
		CultureInfo previousUiCulture = CultureInfo.CurrentUICulture;
		try {
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "tr-TR" );
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "tr-TR" );
			Assert.Equal( invariant, TermInfoJsonRenderer.Render( profile ) );
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "fr-FR" );
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "fr-FR" );
			Assert.Equal( invariant, TermInfoJsonRenderer.Render( profile ) );
		} finally {
			CultureInfo.CurrentCulture = previousCulture;
			CultureInfo.CurrentUICulture = previousUiCulture;
		}

		int exactBytes = Encoding.UTF8.GetByteCount( invariant );
		Assert.Equal(
			invariant,
			TermInfoJsonRenderer.Render(
				profile,
				new TermInfoJsonRendererOptions( exactBytes )
			)
		);
		Assert.Throws<InvalidOperationException>(
			() => TermInfoJsonRenderer.Render(
				profile,
				new TermInfoJsonRendererOptions( exactBytes - 1 )
			)
		);

		using CancellationTokenSource cancellation = new();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>(
			() => TermInfoJsonRenderer.Render(
				profile,
				new TermInfoJsonRendererOptions(),
				cancellation.Token
			)
		);
	}

	[Fact]
	public void VersionFourSchemaAndPackageWiringAreFrozen() {
		string root = FindRepositoryRoot();
		string schemaPath = Path.Combine(
			root,
			"docs",
			"Icod.TermInfo.Inspection.schema.v4.json"
		);
		Assert.Equal(
			"6383052f389d903683a9e24d55b73c97eb165db89c6ab5f26dbcb5a3c7fdda87",
			NormalizedLfSha256( schemaPath )
		);
		using JsonDocument schema = JsonDocument.Parse( File.ReadAllText( schemaPath ) );
		Assert.Equal(
			"urn:icod:terminfo:inspection:json:4",
			schema.RootElement.GetProperty( "$id" ).GetString()
		);
		string[] documentReferences =
			schema.RootElement
				.GetProperty( "oneOf" )
				.EnumerateArray()
				.Select( branch => branch.GetProperty( "$ref" ).GetString() )
				.Cast<string>()
				.ToArray();
		Assert.Equal(
			new[] {
				"#/$defs/persistentRasterPlacementProfileDocument",
				"#/$defs/persistentRasterPlacementPlanDocument",
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
			"Icod.TermInfo.Inspection.schema.v4.json",
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
