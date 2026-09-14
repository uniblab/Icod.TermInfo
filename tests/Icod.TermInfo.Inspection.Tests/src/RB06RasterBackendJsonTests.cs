using System.Globalization;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RB06RasterBackendJsonTests {
	[Fact]
	public void RasterBackendJsonVersionIdentityIsSix() {
		Assert.Equal(
			"urn:icod:terminfo:inspection:json:6",
			TermInfoJsonRenderer.RasterBackendSchemaIdentifier
		);
		Assert.Equal(
			6,
			TermInfoJsonRenderer.RasterBackendSchemaVersion
		);
	}

	[Fact]
	public void BackendProfileRendersExactVersionSixDocument() {
		RasterBackendProfile profile = CreateBackendProfileFixture();

		string json = TermInfoJsonRenderer.Render( profile );

		Assert.Equal(
			"{\"schema\":\"urn:icod:terminfo:inspection:json:6\",\"schemaVersion\":6,\"documentKind\":\"rasterBackendProfile\",\"data\":{\"backend\":\"sixel\",\"status\":\"supported\",\"evidenceCount\":3,\"evidence\":[{\"backend\":\"sixel\",\"isPositive\":true,\"kind\":\"capabilityDerived\",\"sourceLabel\":\"alpha\",\"sourceOrdinal\":1},{\"backend\":\"sixel\",\"isPositive\":false,\"kind\":\"declared\",\"sourceLabel\":\"beta\",\"sourceOrdinal\":2},{\"backend\":\"sixel\",\"isPositive\":true,\"kind\":\"verified\",\"sourceLabel\":\"zeta\",\"sourceOrdinal\":3}]}}",
			json
		);
	}

	[Fact]
	public void SelectionPlanRendersVersionSixAuditDocument() {
		RasterBackendSelectionPlan plan = CreateSelectionPlanFixture();

		using JsonDocument document = JsonDocument.Parse(
			TermInfoJsonRenderer.Render( plan )
		);
		JsonElement root = document.RootElement;
		Assert.Equal(
			"urn:icod:terminfo:inspection:json:6",
			root.GetProperty( "schema" ).GetString()
		);
		Assert.Equal( 6, root.GetProperty( "schemaVersion" ).GetInt32() );
		Assert.Equal(
			"rasterBackendSelectionPlan",
			root.GetProperty( "documentKind" ).GetString()
		);

		JsonElement data = root.GetProperty( "data" );
		JsonElement lifecycleRequest = data
			.GetProperty( "request" )
			.GetProperty( "lifecycle" );
		Assert.False( lifecycleRequest.GetProperty( "displayEphemeral" ).GetBoolean() );
		Assert.True( lifecycleRequest.GetProperty( "uploadResource" ).GetBoolean() );
		Assert.Equal( 1, lifecycleRequest.GetProperty( "placementCount" ).GetInt32() );
		Assert.False( lifecycleRequest.GetProperty( "updatePlacement" ).GetBoolean() );
		Assert.False( lifecycleRequest.GetProperty( "deletePlacement" ).GetBoolean() );
		Assert.False( lifecycleRequest.GetProperty( "deleteResource" ).GetBoolean() );
		Assert.True(
			lifecycleRequest.GetProperty( "requireAcknowledgedUpload" ).GetBoolean()
		);

		JsonElement placementRequest = data
			.GetProperty( "request" )
			.GetProperty( "placement" );
		Assert.True(
			placementRequest.GetProperty( "requireSourceRectangle" ).GetBoolean()
		);
		Assert.True(
			placementRequest.GetProperty( "requireSignedZOrder" ).GetBoolean()
		);

		Assert.Equal(
			new[] { "kittyGraphics", "sixel" },
			data.GetProperty( "preferenceOrder" )
				.EnumerateArray()
				.Select( item => item.GetString() )
				.Cast<string>()
				.ToArray()
		);
		Assert.Equal( "selected", data.GetProperty( "status" ).GetString() );
		Assert.Equal(
			"kittyGraphics",
			data.GetProperty( "selectedBackend" ).GetString()
		);
		Assert.Equal(
			JsonValueKind.Null,
			data.GetProperty( "remainingBlocker" ).ValueKind
		);

		JsonElement[] evaluations = data
			.GetProperty( "candidateEvaluations" )
			.EnumerateArray()
			.ToArray();
		Assert.Equal( 2, evaluations.Length );
		Assert.Equal( "sixel", evaluations[ 0 ].GetProperty( "backend" ).GetString() );
		Assert.Equal(
			"supported",
			evaluations[ 0 ].GetProperty( "backendStatus" ).GetString()
		);
		Assert.Equal(
			"requiresRuntimeVerification",
			evaluations[ 0 ].GetProperty( "status" ).GetString()
		);
		Assert.Equal(
			"success",
			evaluations[ 0 ]
				.GetProperty( "lifecyclePlan" )
				.GetProperty( "status" )
				.GetString()
		);
		Assert.Equal(
			"requiresRuntimeVerification",
			evaluations[ 0 ]
				.GetProperty( "placementPlan" )
				.GetProperty( "status" )
				.GetString()
		);
		Assert.Equal(
			"kittyGraphics",
			evaluations[ 1 ].GetProperty( "backend" ).GetString()
		);
		Assert.Equal(
			"satisfied",
			evaluations[ 1 ].GetProperty( "status" ).GetString()
		);
		Assert.Equal(
			"satisfied",
			evaluations[ 1 ]
				.GetProperty( "placementPlan" )
				.GetProperty( "status" )
				.GetString()
		);
	}

	[Fact]
	public void SelectionPlanRendersExplicitBlockerForNonSelectedOutcomes() {
		RasterBackendSelectionRequest request = new(
			new PersistentRasterLifecycleRequest( displayEphemeral: true )
		);
		RasterBackendCandidate sixel = new(
			RasterBackendClassifier.Classify(
				RasterBackendKind.Sixel,
				Array.Empty<RasterBackendEvidence>()
			),
			CreateLifecycleProfile(
				"blocker-sixel",
				PersistentRasterLifecycleEvidenceSubject.RasterDisplay
			),
			CreateEmptyPlacementProfile()
		);
		RasterBackendCandidate kitty = new(
			RasterBackendClassifier.Classify(
				RasterBackendKind.KittyGraphics,
				Array.Empty<RasterBackendEvidence>()
			),
			CreateLifecycleProfile(
				"blocker-kitty",
				PersistentRasterLifecycleEvidenceSubject.RasterDisplay
			),
			CreateEmptyPlacementProfile()
		);
		RasterBackendSelectionPlan plan = RasterBackendPlanner.Plan(
			[ sixel, kitty ],
			request
		);

		using JsonDocument document = JsonDocument.Parse(
			TermInfoJsonRenderer.Render( plan )
		);
		JsonElement data = document.RootElement.GetProperty( "data" );
		Assert.Equal(
			"requiresPreference",
			data.GetProperty( "status" ).GetString()
		);
		Assert.Equal(
			"preference",
			data.GetProperty( "remainingBlocker" ).GetString()
		);
		Assert.Equal(
			JsonValueKind.Null,
			data.GetProperty( "selectedBackend" ).ValueKind
		);
	}

	[Fact]
	public void BackendProfileRenderingIsDeterministicBoundedCancelableAndCultureIndependent() {
		RasterBackendProfile profile = CreateBackendProfileFixture();
		string invariant = TermInfoJsonRenderer.Render( profile );
		Assert.Equal( invariant, TermInfoJsonRenderer.Render( profile ) );
		AssertCultureIndependent(
			invariant,
			() => TermInfoJsonRenderer.Render( profile )
		);

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
	public void SelectionPlanRenderingIsDeterministicBoundedCancelableAndCultureIndependent() {
		RasterBackendSelectionPlan plan = CreateSelectionPlanFixture();
		string invariant = TermInfoJsonRenderer.Render( plan );
		Assert.Equal( invariant, TermInfoJsonRenderer.Render( plan ) );
		AssertCultureIndependent(
			invariant,
			() => TermInfoJsonRenderer.Render( plan )
		);

		int exactBytes = Encoding.UTF8.GetByteCount( invariant );
		Assert.Equal(
			invariant,
			TermInfoJsonRenderer.Render(
				plan,
				new TermInfoJsonRendererOptions( exactBytes )
			)
		);
		Assert.Throws<InvalidOperationException>(
			() => TermInfoJsonRenderer.Render(
				plan,
				new TermInfoJsonRendererOptions( exactBytes - 1 )
			)
		);

		using CancellationTokenSource cancellation = new();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>(
			() => TermInfoJsonRenderer.Render(
				plan,
				new TermInfoJsonRendererOptions(),
				cancellation.Token
			)
		);
	}

	[Fact]
	public void VersionSixSchemaAndPackageWiringAreAdditive() {
		string root = FindRepositoryRoot();
		string schemaPath = Path.Combine(
			root,
			"docs",
			"Icod.TermInfo.Inspection.schema.v6.json"
		);
		using JsonDocument schema = JsonDocument.Parse(
			File.ReadAllText( schemaPath )
		);
		Assert.Equal(
			"urn:icod:terminfo:inspection:json:6",
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
				"#/$defs/rasterBackendProfileDocument",
				"#/$defs/rasterBackendSelectionPlanDocument",
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
			"Icod.TermInfo.Inspection.schema.v6.json",
			projectText,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"Icod.TermInfo.Inspection.schema.v5.json",
			projectText,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void PreviousRendererVersionIdentitiesRemainFrozen() {
		Assert.Equal(
			"urn:icod:terminfo:inspection:json:3",
			TermInfoJsonRenderer.PersistentRasterLifecycleSchemaIdentifier
		);
		Assert.Equal( 3, TermInfoJsonRenderer.PersistentRasterLifecycleSchemaVersion );
		Assert.Equal(
			"urn:icod:terminfo:inspection:json:4",
			TermInfoJsonRenderer.PersistentRasterPlacementSchemaIdentifier
		);
		Assert.Equal( 4, TermInfoJsonRenderer.PersistentRasterPlacementSchemaVersion );
		Assert.Equal(
			"urn:icod:terminfo:inspection:json:5",
			TermInfoJsonRenderer.PersistentRasterRuntimeSchemaIdentifier
		);
		Assert.Equal( 5, TermInfoJsonRenderer.PersistentRasterRuntimeSchemaVersion );
	}

	private static RasterBackendProfile CreateBackendProfileFixture() {
		return RasterBackendClassifier.Classify(
			RasterBackendKind.Sixel,
			[
				new RasterBackendEvidence(
					RasterBackendKind.Sixel,
					true,
					RasterBackendEvidenceKind.Verified,
					"zeta",
					3
				),
				new RasterBackendEvidence(
					RasterBackendKind.Sixel,
					true,
					RasterBackendEvidenceKind.CapabilityDerived,
					"alpha",
					1
				),
				new RasterBackendEvidence(
					RasterBackendKind.Sixel,
					false,
					RasterBackendEvidenceKind.Declared,
					"beta",
					2
				),
			]
		);
	}

	private static RasterBackendSelectionPlan CreateSelectionPlanFixture() {
		PersistentRasterLifecycleRequest lifecycleRequest = new(
			uploadResource: true,
			placementCount: 1,
			requireAcknowledgedUpload: true
		);
		PersistentRasterPlacementRequest placementRequest = new(
			requireSourceRectangle: true,
			requireSignedZOrder: true
		);
		RasterBackendSelectionRequest request = new(
			lifecycleRequest,
			placementRequest
		);
		RasterBackendSelectionOptions options = new(
			[
				RasterBackendKind.KittyGraphics,
				RasterBackendKind.Sixel,
			]
		);
		RasterBackendCandidate sixel = new(
			CreateSupportedBackendProfile( RasterBackendKind.Sixel, "sixel-backend" ),
			CreateLifecycleProfile(
				"sixel-lifecycle",
				PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
				PersistentRasterLifecycleEvidenceSubject.AcknowledgedUpload,
				PersistentRasterLifecycleEvidenceSubject.PlacementCreation
			),
			CreatePlacementProfile(
				"sixel-placement",
				includeSignedZOrder: false
			)
		);
		RasterBackendCandidate kitty = new(
			CreateSupportedBackendProfile(
				RasterBackendKind.KittyGraphics,
				"kitty-backend"
			),
			CreateLifecycleProfile(
				"kitty-lifecycle",
				PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
				PersistentRasterLifecycleEvidenceSubject.AcknowledgedUpload,
				PersistentRasterLifecycleEvidenceSubject.PlacementCreation
			),
			CreatePlacementProfile(
				"kitty-placement",
				includeSignedZOrder: true
			)
		);

		return RasterBackendPlanner.Plan(
			[ kitty, sixel ],
			request,
			options
		);
	}

	private static RasterBackendProfile CreateSupportedBackendProfile(
		RasterBackendKind backend,
		string sourceLabel
	) {
		return RasterBackendClassifier.Classify(
			backend,
			[
				new RasterBackendEvidence(
					backend,
					true,
					RasterBackendEvidenceKind.Verified,
					sourceLabel,
					0
				),
			]
		);
	}

	private static PersistentRasterLifecycleProfile CreateLifecycleProfile(
		string sourceLabel,
		params PersistentRasterLifecycleEvidenceSubject[] subjects
	) {
		return PersistentRasterLifecycleClassifier.Classify(
			subjects.Select(
				(subject, index) => new PersistentRasterLifecycleEvidence(
					subject,
					true,
					PersistentRasterLifecycleEvidenceKind.Verified,
					sourceLabel,
					index
				)
			)
		);
	}

	private static PersistentRasterPlacementProfile CreatePlacementProfile(
		string sourceLabel,
		bool includeSignedZOrder
	) {
		List<PersistentRasterPlacementEvidence> evidence = [
			new PersistentRasterPlacementEvidence(
				PersistentRasterPlacementSubject.SourceRectangle,
				true,
				PersistentRasterPlacementEvidenceKind.Verified,
				sourceLabel,
				0
			),
		];
		if ( includeSignedZOrder ) {
			evidence.Add(
				new PersistentRasterPlacementEvidence(
					PersistentRasterPlacementSubject.SignedZOrder,
					true,
					PersistentRasterPlacementEvidenceKind.Verified,
					sourceLabel,
					1
				)
			);
		}
		return PersistentRasterPlacementClassifier.Classify( evidence );
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
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
		try {
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "fr-FR" );
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "tr-TR" );
			Assert.Equal( expected, render() );
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}
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
