using Icod.Terminal;
using Icod.TermInfo.Inspection;

PersistentRasterLifecycleProfile lifecycleProfile =
	PersistentRasterLifecycleClassifier.Classify(
		new[] {
			new PersistentRasterLifecycleEvidence(
				PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
				true,
				PersistentRasterLifecycleEvidenceKind.Verified,
				"sample lifecycle",
				0
			),
		}
	);
PersistentRasterLifecyclePlan lifecyclePlan =
	PersistentRasterLifecyclePlanner.Plan(
		lifecycleProfile,
		new PersistentRasterLifecycleRequest( placementCount: 1 )
	);
PersistentRasterPlacementRequest placementRequest =
	new(
		requireSourceRectangle: true,
		requireSignedZOrder: true
	);

PersistentRasterPlacementProfile initialPlacementProfile =
	PersistentRasterPlacementClassifier.Classify(
		Array.Empty<PersistentRasterPlacementEvidence>()
	);
PersistentRasterPlacementPlan initialPlacementPlan =
	PersistentRasterPlacementPlanner.Plan(
		lifecyclePlan,
		initialPlacementProfile,
		placementRequest
	);

Console.WriteLine( "initial placement profile:" );
Console.WriteLine( TermInfoJsonRenderer.Render( initialPlacementProfile ) );
Console.WriteLine( "initial placement plan:" );
Console.WriteLine( TermInfoJsonRenderer.Render( initialPlacementPlan ) );
if (
	initialPlacementPlan.Status
	!= PersistentRasterPlacementPlanStatus.RequiresRuntimeVerification
) {
	return 1;
}

PersistentRasterPlacementProfile verifiedPlacementProfile =
	PersistentRasterPlacementClassifier.Classify(
		new[] {
			new PersistentRasterPlacementEvidence(
				PersistentRasterPlacementSubject.SourceRectangle,
				true,
				PersistentRasterPlacementEvidenceKind.Verified,
				"consumer runtime verification",
				0
			),
			new PersistentRasterPlacementEvidence(
				PersistentRasterPlacementSubject.SignedZOrder,
				true,
				PersistentRasterPlacementEvidenceKind.Verified,
				"consumer runtime verification",
				1
			),
		}
	);
PersistentRasterPlacementPlan verifiedPlacementPlan =
	PersistentRasterPlacementPlanner.Plan(
		lifecyclePlan,
		verifiedPlacementProfile,
		placementRequest
	);

Console.WriteLine( "verified placement profile:" );
Console.WriteLine( TermInfoJsonRenderer.Render( verifiedPlacementProfile ) );
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
