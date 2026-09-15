/*
	Icod.TermInfo.RasterBackendSelection.Sample
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
using Icod.TermInfo;
using Icod.TermInfo.Inspection;

const string terminalSourceLabel =
	"Icod.Terminal 1.13.0 PersistentRasterGraphics caller mapping";

TerminalDescription description =
	new TerminalDescriptionBuilder( "raster-backend-selection-sample" )
		.SetDescription( "Raster backend selection sample" )
		.SetExtendedBoolean( "Sixel" )
		.Build();

RasterBackendProfile sixelBackendProfile = RasterBackendInspector.Inspect(
	description,
	RasterBackendKind.Sixel
);
PersistentRasterLifecycleProfile baseLifecycleProfile =
	PersistentRasterLifecycleClassifier.Classify(
		Array.Empty<PersistentRasterLifecycleEvidence>()
	);
PersistentRasterPlacementProfile basePlacementProfile =
	PersistentRasterPlacementClassifier.Classify(
		Array.Empty<PersistentRasterPlacementEvidence>()
	);
RasterBackendCandidate sixelCandidate = new(
	sixelBackendProfile,
	baseLifecycleProfile,
	basePlacementProfile
);

PersistentRasterRuntimeObservationOutcome terminalOutcome;
if ( args.Contains( "--live", StringComparer.Ordinal ) ) {
	await using TerminalSession session = await TerminalSession.OpenAsync(
		new TerminalSessionOptions {
			InputMode = TerminalInputMode.CBreak,
			EchoInput = false
		}
	);
	TerminalCapabilityStatus status = await session.VerifyCapabilityAsync(
		TerminalCapability.PersistentRasterGraphics
	);
	terminalOutcome = MapPersistentRasterStatus( status );
	Console.WriteLine(
		$"Live Terminal 1.13 result: {status.Support}; caller-mapped lifecycle outcome: {terminalOutcome}."
	);
} else {
	terminalOutcome = MapPersistentRasterSupport(
		TerminalCapabilitySupport.Verified
	);
	Console.WriteLine(
		"Deterministic mode: using the published Terminal 1.13 Verified support value as caller-owned input. Pass --live on an interactive terminal to verify PersistentRasterGraphics."
	);
}

PersistentRasterRuntimeObservationSet kittyObservations = new(
	CreatePersistentRasterLifecycleObservations(
		terminalOutcome,
		terminalSourceLabel
	),
	Array.Empty<PersistentRasterRuntimePlacementObservation>()
);
PersistentRasterRuntimeIntegrationResult kittyIntegration =
	PersistentRasterRuntimeEvidenceIntegrator.Integrate(
		baseLifecycleProfile,
		basePlacementProfile,
		kittyObservations
	);
RasterBackendProfile kittyBackendProfile = CreateKittyBackendProfile(
	terminalOutcome
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

Console.WriteLine( "Sixel backend profile:" );
Console.WriteLine( TermInfoJsonRenderer.Render( sixelBackendProfile ) );
Console.WriteLine( "Selection plan:" );
Console.WriteLine( TermInfoJsonRenderer.Render( plan ) );

return
	(
		plan.Status == RasterBackendSelectionStatus.Selected
		&& plan.SelectedBackend == RasterBackendKind.KittyGraphics
	)
		? 0
		: 2
;

static RasterBackendProfile CreateKittyBackendProfile(
	PersistentRasterRuntimeObservationOutcome outcome
) {
	RasterBackendEvidence[] evidence = outcome switch {
		PersistentRasterRuntimeObservationOutcome.Supported => [
			new RasterBackendEvidence(
				RasterBackendKind.KittyGraphics,
				true,
				RasterBackendEvidenceKind.Verified,
				"caller policy: Terminal 1.13 PersistentRasterGraphics -> Kitty availability",
				0
			),
		],
		PersistentRasterRuntimeObservationOutcome.Unsupported => [
			new RasterBackendEvidence(
				RasterBackendKind.KittyGraphics,
				false,
				RasterBackendEvidenceKind.Verified,
				"caller policy: Terminal 1.13 PersistentRasterGraphics -> Kitty availability",
				0
			),
		],
		PersistentRasterRuntimeObservationOutcome.Inconclusive => [],
		_ => throw new ArgumentOutOfRangeException(
			nameof( outcome ),
			outcome,
			"The runtime observation outcome must be defined."
		),
	};
	return RasterBackendClassifier.Classify(
		RasterBackendKind.KittyGraphics,
		evidence
	);
}

static PersistentRasterRuntimeObservationOutcome MapPersistentRasterStatus(
	TerminalCapabilityStatus status
) {
	if ( status.Capability != TerminalCapability.PersistentRasterGraphics ) {
		throw new ArgumentException(
			"The live status must describe persistent raster graphics.",
			nameof( status )
		);
	}
	if ( status.EvidenceKind != TerminalCapabilityEvidenceKind.LiveObservation ) {
		return PersistentRasterRuntimeObservationOutcome.Inconclusive;
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
