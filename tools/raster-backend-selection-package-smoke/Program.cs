/*
	Icod.TermInfo.RasterBackendSelection.PackageSmoke
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
using Icod.TermInfo;
using Icod.TermInfo.Inspection;

_ = typeof( TerminalCapabilityStatus );
_ = TerminalCapability.PersistentRasterGraphics;

TerminalDescription description =
	new TerminalDescriptionBuilder( "rb07-package-smoke" )
		.SetDescription( "RB07 package-only raster backend selection smoke" )
		.SetExtendedBoolean( "Sixel" )
		.Build();

RasterBackendProfile sixelBackendProfile = RasterBackendInspector.Inspect(
	description,
	RasterBackendKind.Sixel
);
PersistentRasterLifecycleProfile emptyLifecycleProfile =
	PersistentRasterLifecycleClassifier.Classify(
		Array.Empty<PersistentRasterLifecycleEvidence>()
	);
PersistentRasterPlacementProfile emptyPlacementProfile =
	PersistentRasterPlacementClassifier.Classify(
		Array.Empty<PersistentRasterPlacementEvidence>()
	);
RasterBackendCandidate sixelCandidate = new(
	sixelBackendProfile,
	emptyLifecycleProfile,
	emptyPlacementProfile
);

PersistentRasterRuntimeObservationOutcome terminalOutcome =
	MapPersistentRasterSupport( TerminalCapabilitySupport.Verified );
PersistentRasterRuntimeObservationSet observations = new(
	CreatePersistentRasterLifecycleObservations(
		terminalOutcome,
		"Icod.Terminal 1.13.0 PersistentRasterGraphics caller mapping"
	),
	Array.Empty<PersistentRasterRuntimePlacementObservation>()
);
PersistentRasterRuntimeIntegrationResult kittyIntegration =
	PersistentRasterRuntimeEvidenceIntegrator.Integrate(
		emptyLifecycleProfile,
		emptyPlacementProfile,
		observations
	);
RasterBackendProfile kittyBackendProfile = RasterBackendClassifier.Classify(
	RasterBackendKind.KittyGraphics,
	new[] {
		new RasterBackendEvidence(
			RasterBackendKind.KittyGraphics,
			true,
			RasterBackendEvidenceKind.Verified,
			"caller policy: Terminal 1.13 PersistentRasterGraphics -> Kitty availability",
			0
		),
	}
);
RasterBackendCandidate kittyCandidate = new(
	kittyBackendProfile,
	kittyIntegration
);

RasterBackendSelectionRequest request = new(
	new PersistentRasterLifecycleRequest(
		uploadResource: true,
		placementCount: 2,
		updatePlacement: true,
		deletePlacement: true,
		deleteResource: true,
		requireAcknowledgedUpload: true
	)
);
RasterBackendSelectionOptions options = new(
	new[] {
		RasterBackendKind.KittyGraphics,
		RasterBackendKind.Sixel,
	}
);
RasterBackendSelectionPlan plan = RasterBackendPlanner.Plan(
	new[] {
		sixelCandidate,
		kittyCandidate,
	},
	request,
	options
);

if (
	plan.Status != RasterBackendSelectionStatus.Selected
	|| plan.SelectedBackend != RasterBackendKind.KittyGraphics
) {
	throw new InvalidOperationException(
		"The package-only Terminal 1.13 adapter did not select the caller-preferred verified Kitty candidate."
	);
}

Console.WriteLine(
	"RB07 package-only Icod.Terminal 1.13 raster-backend selection interoperability passed."
);

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
	PersistentRasterRuntimeLifecycleObservation[] result =
		new PersistentRasterRuntimeLifecycleObservation[ subjects.Length ];
	for ( int index = 0; index < subjects.Length; index++ ) {
		result[ index ] = new PersistentRasterRuntimeLifecycleObservation(
			subjects[ index ],
			outcome,
			sourceLabel,
			index
		);
	}
	return result;
}
