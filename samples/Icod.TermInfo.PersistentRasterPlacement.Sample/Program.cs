/*
	Icod.TermInfo.PersistentRasterPlacement.Sample
	Demonstrates Icod.TermInfo APIs and integration patterns.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

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
