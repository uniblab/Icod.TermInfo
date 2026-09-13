using Icod.TermInfo;
using Icod.TermInfo.Inspection;

namespace Icod.TermInfo.PersistentRasterLifecycle.Sample;

internal static class Program {
	public static int Main() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "persistent-raster-lifecycle-sample" )
				.SetDescription( "Persistent raster lifecycle sample" )
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
			"Unknown persistent support must produce an indeterminate plan."
		);
		Require(
			staticPlan.RequiresRuntimeVerification,
			"Unknown persistent support must require consumer-owned runtime verification."
		);

		PersistentRasterRuntimeObservationSet observations = new(
			new[] {
				new PersistentRasterRuntimeLifecycleObservation(
					PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
					PersistentRasterRuntimeObservationOutcome.Supported,
					"consumer-runtime-verification",
					0
				),
				new PersistentRasterRuntimeLifecycleObservation(
					PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
					PersistentRasterRuntimeObservationOutcome.Supported,
					"consumer-runtime-verification",
					1
				),
			},
			Array.Empty<PersistentRasterRuntimePlacementObservation>()
		);
		PersistentRasterPlacementProfile placementProfile =
			PersistentRasterPlacementClassifier.Classify(
				Array.Empty<PersistentRasterPlacementEvidence>()
			);
		PersistentRasterRuntimeIntegrationResult integration =
			PersistentRasterRuntimeEvidenceIntegrator.Integrate(
				staticProfile,
				placementProfile,
				observations
			);
		PersistentRasterLifecyclePlan strengthenedPlan =
			integration.CreateLifecyclePlan( request );

		Require(
			integration.Succeeded,
			"Conclusive runtime observations must integrate successfully."
		);
		Require(
			strengthenedPlan.Status
				== PersistentRasterLifecyclePlanStatus.Success,
			"Verified consumer observations must strengthen the lifecycle plan to success."
		);
		Require(
			!strengthenedPlan.RequiresRuntimeVerification,
			"Verified consumer observations must remove runtime-verification requirements."
		);
		Require(
			strengthenedPlan.Steps.Count == 2,
			"The strengthened plan must contain upload and placement steps."
		);
		Require(
			strengthenedPlan.Steps[ 0 ].Operation
				== PersistentRasterLifecycleOperation.UploadResource,
			"The strengthened plan must upload the resource first."
		);
		Require(
			strengthenedPlan.Steps[ 1 ].Operation
				== PersistentRasterLifecycleOperation.CreatePlacement,
			"The strengthened plan must create the placement after upload."
		);

		Console.WriteLine(
			TermInfoJsonRenderer.Render(
				integration
			)
		);
		Console.WriteLine(
			TermInfoJsonRenderer.Render(
				strengthenedPlan
			)
		);
		return 0;
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
