using Icod.Terminal;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;

const string runtimeSourceLabel =
	"Icod.Terminal 1.12.0 PersistentRasterGraphics live verification";

TerminalDescription description =
	new TerminalDescriptionBuilder( "persistent-raster-runtime-integration-sample" )
		.SetDescription( "Persistent raster runtime integration sample" )
		.SetExtendedBoolean( "Sixel" )
		.Build();
PersistentRasterLifecycleProfile staticProfile =
	PersistentRasterLifecycleInspector.Inspect( description );
PersistentRasterLifecycleRequest request = new(
	uploadResource: true,
	placementCount: 1,
	updatePlacement: true,
	deletePlacement: true,
	deleteResource: true,
	requireAcknowledgedUpload: true
);
PersistentRasterLifecyclePlan staticPlan =
	PersistentRasterLifecyclePlanner.Plan(
		staticProfile,
		request
	);

Console.WriteLine( $"Static plan: {staticPlan.Status}" );

PersistentRasterRuntimeObservationOutcome outcome;
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
	outcome = MapPersistentRasterStatus( status );
	Console.WriteLine(
		$"Live Terminal capability result: {status.Support}; mapped TermInfo outcome: {outcome}."
	);
} else {
	outcome = MapPersistentRasterSupport(
		TerminalCapabilitySupport.Verified
	);
	Console.WriteLine(
		"Dry-run mode: using the published Icod.Terminal 1.12 Verified support value. Pass --live on an interactive terminal to call VerifyCapabilityAsync."
	);
}

PersistentRasterRuntimeObservationSet observations = new(
	CreatePersistentRasterLifecycleObservations(
		outcome,
		runtimeSourceLabel
	),
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

Console.WriteLine( "Runtime integration audit:" );
Console.WriteLine( TermInfoJsonRenderer.Render( integration ) );
Console.WriteLine( "Plan after runtime evidence:" );
Console.WriteLine( TermInfoJsonRenderer.Render( strengthenedPlan ) );

return
	( strengthenedPlan.Status == PersistentRasterLifecyclePlanStatus.Success )
		? 0
		: 2
;

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
