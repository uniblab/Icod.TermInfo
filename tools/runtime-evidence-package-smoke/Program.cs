/*
	Icod.TermInfo.RuntimeEvidence.PackageSmoke
	Exercises packaged Icod.TermInfo APIs in an isolated consumer.
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

PersistentRasterRuntimeObservationOutcome verifiedOutcome =
	MapPersistentRasterSupport( TerminalCapabilitySupport.Verified );
PersistentRasterRuntimeObservationOutcome unsupportedOutcome =
	MapPersistentRasterSupport( TerminalCapabilitySupport.Unsupported );
if (
	verifiedOutcome != PersistentRasterRuntimeObservationOutcome.Supported
	|| unsupportedOutcome != PersistentRasterRuntimeObservationOutcome.Unsupported
) {
	throw new InvalidOperationException(
		"The Terminal 1.12 support vocabulary did not map to TermInfo runtime outcomes."
	);
}

PersistentRasterLifecycleProfile lifecycleProfile =
	PersistentRasterLifecycleClassifier.Classify(
		Array.Empty<PersistentRasterLifecycleEvidence>()
	);
PersistentRasterPlacementProfile placementProfile =
	PersistentRasterPlacementClassifier.Classify(
		Array.Empty<PersistentRasterPlacementEvidence>()
	);
PersistentRasterRuntimeObservationSet observations = new(
	CreatePersistentRasterLifecycleObservations(
		verifiedOutcome,
		"Icod.Terminal 1.12.0 PersistentRasterGraphics"
	),
	Array.Empty<PersistentRasterRuntimePlacementObservation>()
);
PersistentRasterRuntimeIntegrationResult integration =
	PersistentRasterRuntimeEvidenceIntegrator.Integrate(
		lifecycleProfile,
		placementProfile,
		observations
	);
if ( !integration.Succeeded || integration.ImportedLifecycleEvidence.Count != 7 ) {
	throw new InvalidOperationException(
		"The package-only Terminal status adapter did not import the complete coarse lifecycle observation set."
	);
}

PersistentRasterLifecyclePlan plan = integration.CreateLifecyclePlan(
	new PersistentRasterLifecycleRequest(
		uploadResource: true,
		placementCount: 2,
		updatePlacement: true,
		deletePlacement: true,
		deleteResource: true,
		requireAcknowledgedUpload: true
	)
);
if ( plan.Status != PersistentRasterLifecyclePlanStatus.Success ) {
	throw new InvalidOperationException(
		"The package-only runtime observations did not strengthen the lifecycle plan to success."
	);
}

Console.WriteLine(
	"RE07 package-only Icod.Terminal 1.12 runtime-evidence interoperability passed."
);

static PersistentRasterRuntimeObservationOutcome MapPersistentRasterStatus(
	TerminalCapabilityStatus status
) {
	if ( status.Capability != TerminalCapability.PersistentRasterGraphics ) {
		throw new ArgumentException(
			"The status must describe TerminalCapability.PersistentRasterGraphics.",
			nameof( status )
		);
	}
	return MapPersistentRasterSupport( status.Support );
}

static PersistentRasterRuntimeObservationOutcome MapPersistentRasterSupport(
	TerminalCapabilitySupport support
) =>
	support switch {
		TerminalCapabilitySupport.Verified =>
			PersistentRasterRuntimeObservationOutcome.Supported,
		TerminalCapabilitySupport.Unsupported =>
			PersistentRasterRuntimeObservationOutcome.Unsupported,
		TerminalCapabilitySupport.Unknown
			or TerminalCapabilitySupport.Advertised =>
			PersistentRasterRuntimeObservationOutcome.Inconclusive,
		_ => throw new ArgumentOutOfRangeException(
			nameof( support ),
			support,
			"The Terminal capability support state must be defined."
		),
	};

static IReadOnlyList<PersistentRasterRuntimeLifecycleObservation>
	CreatePersistentRasterLifecycleObservations(
		PersistentRasterRuntimeObservationOutcome outcome,
		string sourceLabel
	) {
	ArgumentException.ThrowIfNullOrWhiteSpace( sourceLabel );
	PersistentRasterLifecycleEvidenceSubject[] subjects = [
		PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
		PersistentRasterLifecycleEvidenceSubject.AcknowledgedUpload,
		PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
		PersistentRasterLifecycleEvidenceSubject.MultiplePlacements,
		PersistentRasterLifecycleEvidenceSubject.PlacementUpdate,
		PersistentRasterLifecycleEvidenceSubject.PlacementDeletion,
		PersistentRasterLifecycleEvidenceSubject.ResourceDeletion,
	];
	PersistentRasterRuntimeLifecycleObservation[] observations =
		new PersistentRasterRuntimeLifecycleObservation[ subjects.Length ];
	for ( int index = 0; index < subjects.Length; index++ ) {
		observations[ index ] = new PersistentRasterRuntimeLifecycleObservation(
			subjects[ index ],
			outcome,
			sourceLabel,
			index
		);
	}
	return observations;
}
