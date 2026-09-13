using System.Globalization;

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
	public void PlacementRenderingIsCultureIndependent() {
		CultureInfo previousCulture = CultureInfo.CurrentCulture;
		CultureInfo previousUiCulture = CultureInfo.CurrentUICulture;
		try {
			PersistentRasterPlacementProfile profile =
				PersistentRasterPlacementClassifier.Classify(
					Array.Empty<PersistentRasterPlacementEvidence>()
				);
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "tr-TR" );
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "tr-TR" );
			string first = TermInfoJsonRenderer.Render( profile );
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "fr-FR" );
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "fr-FR" );
			string second = TermInfoJsonRenderer.Render( profile );
			Assert.Equal( first, second );
		} finally {
			CultureInfo.CurrentCulture = previousCulture;
			CultureInfo.CurrentUICulture = previousUiCulture;
		}
	}
}
