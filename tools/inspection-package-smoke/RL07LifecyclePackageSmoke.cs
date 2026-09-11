using System.Runtime.CompilerServices;
using System.Text.Json;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;

internal static class RL07LifecyclePackageSmoke {
	[ModuleInitializer]
	internal static void Run() {
		const string persistentUploadExtendedBooleanCapabilityName =
			PersistentRasterLifecycleInspector.PersistentUploadCapabilityName;
		Require(
			persistentUploadExtendedBooleanCapabilityName
				== "IcodPersistentRasterUpload",
			"The package consumer did not observe the frozen persistent-upload semantic capability name."
		);

		TerminalDescription description =
			new TerminalDescriptionBuilder( "rl07-persistent-raster" )
				.SetDescription( "RL07 package-only lifecycle qualification" )
				.SetExtendedBoolean( "Sixel" )
				.Build();
		PersistentRasterLifecycleProfile staticProfile =
			PersistentRasterLifecycleInspector.Inspect(
				description
			);
		Require(
			staticProfile.PersistentUpload
				== PersistentRasterLifecycleSupportStatus.Unknown,
			"Sixel alone must not imply persistent upload support."
		);
		Require(
			staticProfile.PlacementCreation
				== PersistentRasterLifecycleSupportStatus.Unknown,
			"Sixel alone must not imply placement support."
		);

		PersistentRasterLifecycleRequest request =
			new PersistentRasterLifecycleRequest(
				uploadResource: true,
				placementCount: 1
			);
		PersistentRasterLifecyclePlan staticPlan =
			PersistentRasterLifecyclePlanner.Plan(
				staticProfile,
				request
			);
		Require(
			staticPlan.Status
				== PersistentRasterLifecyclePlanStatus.Indeterminate,
			"Static unknown lifecycle support must produce an indeterminate plan."
		);
		Require(
			staticPlan.RequiresRuntimeVerification,
			"The static plan must require consumer-owned runtime verification."
		);
		Require(
			staticPlan.Steps.Count == 2,
			"The static plan must contain upload and placement steps."
		);
		Require(
			staticPlan.Steps.All(
				step => step.RequiresRuntimeVerification
			),
			"Unknown persistent upload and placement support must require verification for both steps."
		);

		List<PersistentRasterLifecycleEvidence> strengthenedEvidence =
			staticProfile.Evidence.ToList();
		strengthenedEvidence.Add(
			new PersistentRasterLifecycleEvidence(
				PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
				isPositive: true,
				PersistentRasterLifecycleEvidenceKind.Verified,
				"consumer-runtime-verification",
				sourceOrdinal: 0
			)
		);
		strengthenedEvidence.Add(
			new PersistentRasterLifecycleEvidence(
				PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
				isPositive: true,
				PersistentRasterLifecycleEvidenceKind.Verified,
				"consumer-runtime-verification",
				sourceOrdinal: 1
			)
		);

		PersistentRasterLifecycleProfile strengthenedProfile =
			PersistentRasterLifecycleClassifier.Classify(
				strengthenedEvidence
			);
		PersistentRasterLifecyclePlan strengthenedPlan =
			PersistentRasterLifecyclePlanner.Plan(
				strengthenedProfile,
				request
			);
		Require(
			strengthenedPlan.Status
				== PersistentRasterLifecyclePlanStatus.Success,
			"Verified consumer evidence must strengthen the plan to success."
		);
		Require(
			!strengthenedPlan.RequiresRuntimeVerification,
			"Verified lifecycle support must remove runtime-verification requirements."
		);
		Require(
			strengthenedPlan.Steps.Count == 2,
			"The strengthened plan must contain upload and placement steps."
		);
		Require(
			strengthenedPlan.Steps[ 0 ].Operation
				== PersistentRasterLifecycleOperation.UploadResource,
			"The strengthened plan must upload the persistent resource first."
		);
		Require(
			strengthenedPlan.Steps[ 1 ].Operation
				== PersistentRasterLifecycleOperation.CreatePlacement,
			"The strengthened plan must create the placement after upload."
		);
		Require(
			strengthenedPlan.Issues.Count == 0,
			"The strengthened plan must not retain uncertainty issues."
		);

		string profileJson =
			TermInfoJsonRenderer.Render(
				strengthenedProfile
			);
		string planJson =
			TermInfoJsonRenderer.Render(
				strengthenedPlan
			);
		using JsonDocument profileDocument =
			JsonDocument.Parse(
				profileJson
			);
		using JsonDocument planDocument =
			JsonDocument.Parse(
				planJson
			);
		Require(
			profileDocument.RootElement.GetProperty( "schemaVersion" ).GetInt32() == 3,
			"Lifecycle profile automation must use schema version 3."
		);
		Require(
			profileDocument.RootElement.GetProperty( "documentKind" ).GetString()
				== "persistentRasterLifecycleProfile",
			"Lifecycle profile automation must use the reviewed document kind."
		);
		Require(
			planDocument.RootElement.GetProperty( "schemaVersion" ).GetInt32() == 3,
			"Lifecycle plan automation must use schema version 3."
		);
		Require(
			planDocument.RootElement.GetProperty( "documentKind" ).GetString()
				== "persistentRasterLifecyclePlan",
			"Lifecycle plan automation must use the reviewed document kind."
		);
	}

	private static void Require(
		bool condition,
		string message
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( message );
		if ( !condition ) {
			throw new InvalidOperationException(
				message
			);
		}
	}
}
