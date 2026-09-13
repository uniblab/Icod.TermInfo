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
PersistentRasterPlacementProfile placementProfile =
	PersistentRasterPlacementClassifier.Classify(
		new[] {
			new PersistentRasterPlacementEvidence(
				PersistentRasterPlacementSubject.SourceRectangle,
				true,
				PersistentRasterPlacementEvidenceKind.Verified,
				"sample placement",
				0
			),
			new PersistentRasterPlacementEvidence(
				PersistentRasterPlacementSubject.SignedZOrder,
				true,
				PersistentRasterPlacementEvidenceKind.Verified,
				"sample placement",
				1
			),
		}
	);
PersistentRasterPlacementPlan placementPlan =
	PersistentRasterPlacementPlanner.Plan(
		lifecyclePlan,
		placementProfile,
		new PersistentRasterPlacementRequest(
			requireSourceRectangle: true,
			requireSignedZOrder: true
		)
	);

Console.WriteLine( TermInfoJsonRenderer.Render( placementPlan ) );
if ( placementPlan.Status != PersistentRasterPlacementPlanStatus.Satisfied ) {
	return 1;
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
