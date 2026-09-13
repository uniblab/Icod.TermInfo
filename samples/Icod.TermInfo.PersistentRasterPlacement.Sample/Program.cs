using Icod.Terminal;
using Icod.TermInfo.Inspection;

PersistentRasterLifecycleProfile initialLifecycleProfile =
	PersistentRasterLifecycleClassifier.Classify(
		Array.Empty<PersistentRasterLifecycleEvidence>()
	);
PersistentRasterPlacementProfile initialPlacementProfile =
	PersistentRasterPlacementClassifier.Classify(
		Array.Empty<PersistentRasterPlacementEvidence>()
	);
PersistentRasterLifecycleRequest lifecycleRequest =
	new( placementCount: 1 );
PersistentRasterPlacementRequest placementRequest = new(
	requireSourceRectangle: true,
	requireSignedZOrder: true
);

PersistentRasterRuntimeObservationSet lifecycleObservations = new(
	new[] {
		new PersistentRasterRuntimeLifecycleObservation(
			PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
			PersistentRasterRuntimeObservationOutcome.Supported,
			"sample lifecycle runtime verification",
			0
		),
	},
	Array.Empty<PersistentRasterRuntimePlacementObservation>()
);
PersistentRasterRuntimeIntegrationResult lifecycleIntegration =
	PersistentRasterRuntimeEvidenceIntegrator.Integrate(
		initialLifecycleProfile,
		initialPlacementProfile,
		lifecycleObservations
	);
PersistentRasterLifecyclePlan lifecyclePlan =
	lifecycleIntegration.CreateLifecyclePlan( lifecycleRequest );
PersistentRasterPlacementPlan initialPlacementPlan =
	PersistentRasterPlacementPlanner.Plan(
		lifecyclePlan,
		lifecycleIntegration.PlacementProfile,
		placementRequest
	);

Console.WriteLine( "initial runtime integration:" );
Console.WriteLine( TermInfoJsonRenderer.Render( lifecycleIntegration ) );
Console.WriteLine( "initial placement plan:" );
Console.WriteLine( TermInfoJsonRenderer.Render( initialPlacementPlan ) );
if (
	initialPlacementPlan.Status
	!= PersistentRasterPlacementPlanStatus.RequiresRuntimeVerification
) {
	return 1;
}

PersistentRasterRuntimeObservationSet placementObservations = new(
	Array.Empty<PersistentRasterRuntimeLifecycleObservation>(),
	new[] {
		new PersistentRasterRuntimePlacementObservation(
			PersistentRasterPlacementSubject.SourceRectangle,
			PersistentRasterRuntimeObservationOutcome.Supported,
			"consumer runtime verification",
			0
		),
		new PersistentRasterRuntimePlacementObservation(
			PersistentRasterPlacementSubject.SignedZOrder,
			PersistentRasterRuntimeObservationOutcome.Supported,
			"consumer runtime verification",
			1
		),
	}
);
PersistentRasterRuntimeIntegrationResult verifiedIntegration =
	PersistentRasterRuntimeEvidenceIntegrator.Integrate(
		lifecycleIntegration.LifecycleProfile,
		lifecycleIntegration.PlacementProfile,
		placementObservations
	);
PersistentRasterPlacementPlan verifiedPlacementPlan =
	verifiedIntegration.CreatePlacementPlan(
		lifecycleRequest,
		placementRequest
	);

Console.WriteLine( "verified runtime integration:" );
Console.WriteLine( TermInfoJsonRenderer.Render( verifiedIntegration ) );
Console.WriteLine( "verified placement plan:" );
Console.WriteLine( TermInfoJsonRenderer.Render( verifiedPlacementPlan ) );
if ( verifiedPlacementPlan.Status != PersistentRasterPlacementPlanStatus.Satisfied ) {
	return 2;
}

// TermInfo has now finished its job: the required semantics are admissible.
// Concrete execution values remain application/Terminal-owned.
TerminalRasterSourceRectangle sourceRectangle = new(
	8,
	4,
	320,
	180
);
TerminalRasterPlacementOptions executionOptions = new() {
	SourceRectangle = sourceRectangle,
	ZIndex = -2,
};

Console.WriteLine(
	$"consumer execution values: crop={executionOptions.SourceRectangle?.X},{executionOptions.SourceRectangle?.Y} "
		+ $"{executionOptions.SourceRectangle?.Width}x{executionOptions.SourceRectangle?.Height}; "
		+ $"z={executionOptions.ZIndex}"
);
return 0;
