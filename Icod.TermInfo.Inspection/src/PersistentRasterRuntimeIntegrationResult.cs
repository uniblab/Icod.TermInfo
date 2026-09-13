namespace Icod.TermInfo.Inspection;

/// <summary>
/// Represents the immutable deterministic audit result of integrating one runtime-
/// observation set into persistent-raster lifecycle and placement evidence.
/// </summary>
public sealed class PersistentRasterRuntimeIntegrationResult {
	internal PersistentRasterRuntimeIntegrationResult(
		PersistentRasterRuntimeObservationSet observations,
		IEnumerable<PersistentRasterLifecycleEvidence> importedLifecycleEvidence,
		IEnumerable<PersistentRasterPlacementEvidence> importedPlacementEvidence,
		IEnumerable<PersistentRasterRuntimeLifecycleObservation>
			inconclusiveLifecycleObservations,
		IEnumerable<PersistentRasterRuntimePlacementObservation>
			inconclusivePlacementObservations,
		PersistentRasterLifecycleProfile lifecycleProfile,
		PersistentRasterPlacementProfile placementProfile,
		IEnumerable<PersistentRasterRuntimeIntegrationIssue> issues
	) {
		ArgumentNullException.ThrowIfNull( observations );
		ArgumentNullException.ThrowIfNull( importedLifecycleEvidence );
		ArgumentNullException.ThrowIfNull( importedPlacementEvidence );
		ArgumentNullException.ThrowIfNull( inconclusiveLifecycleObservations );
		ArgumentNullException.ThrowIfNull( inconclusivePlacementObservations );
		ArgumentNullException.ThrowIfNull( lifecycleProfile );
		ArgumentNullException.ThrowIfNull( placementProfile );
		ArgumentNullException.ThrowIfNull( issues );

		Observations = observations;
		ImportedLifecycleEvidence = Array.AsReadOnly(
			importedLifecycleEvidence.ToArray()
		);
		ImportedPlacementEvidence = Array.AsReadOnly(
			importedPlacementEvidence.ToArray()
		);
		InconclusiveLifecycleObservations = Array.AsReadOnly(
			inconclusiveLifecycleObservations.ToArray()
		);
		InconclusivePlacementObservations = Array.AsReadOnly(
			inconclusivePlacementObservations.ToArray()
		);
		LifecycleProfile = lifecycleProfile;
		PlacementProfile = placementProfile;
		Issues = Array.AsReadOnly( issues.ToArray() );
		Succeeded = Issues.Count == 0;
	}

	/// <summary>
	/// Gets the exact immutable runtime-observation set supplied to the integrator.
	/// </summary>
	public PersistentRasterRuntimeObservationSet Observations {
		get;
	}

	/// <summary>
	/// Gets the lifecycle evidence imported from conclusive runtime observations.
	/// </summary>
	public IReadOnlyList<PersistentRasterLifecycleEvidence>
		ImportedLifecycleEvidence {
		get;
	}

	/// <summary>
	/// Gets the placement evidence imported from conclusive runtime observations.
	/// </summary>
	public IReadOnlyList<PersistentRasterPlacementEvidence>
		ImportedPlacementEvidence {
		get;
	}

	/// <summary>
	/// Gets lifecycle runtime observations retained as inconclusive audit evidence.
	/// </summary>
	public IReadOnlyList<PersistentRasterRuntimeLifecycleObservation>
		InconclusiveLifecycleObservations {
		get;
	}

	/// <summary>
	/// Gets placement runtime observations retained as inconclusive audit evidence.
	/// </summary>
	public IReadOnlyList<PersistentRasterRuntimePlacementObservation>
		InconclusivePlacementObservations {
		get;
	}

	/// <summary>
	/// Gets the resulting lifecycle profile after successful lifecycle-family
	/// integration, or the original lifecycle profile when that family could not be
	/// imported atomically.
	/// </summary>
	public PersistentRasterLifecycleProfile LifecycleProfile {
		get;
	}

	/// <summary>
	/// Gets the resulting placement profile after successful placement-family
	/// integration, or the original placement profile when that family could not be
	/// imported atomically.
	/// </summary>
	public PersistentRasterPlacementProfile PlacementProfile {
		get;
	}

	/// <summary>Gets represented integration limitations in deterministic order.</summary>
	public IReadOnlyList<PersistentRasterRuntimeIntegrationIssue> Issues {
		get;
	}

	/// <summary>
	/// Gets whether all conclusive lifecycle and placement observations were safely
	/// imported. This does not imply that downstream plans are satisfiable.
	/// </summary>
	public bool Succeeded {
		get;
	}
}
