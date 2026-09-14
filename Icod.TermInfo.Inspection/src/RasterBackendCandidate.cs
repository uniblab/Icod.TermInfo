namespace Icod.TermInfo.Inspection;

/// <summary>
/// Composes one concrete backend availability profile with the lifecycle and
/// placement semantic profiles that apply to that backend context.
/// </summary>
public sealed class RasterBackendCandidate {
	/// <summary>Initializes one immutable raster-backend candidate.</summary>
	public RasterBackendCandidate(
		RasterBackendProfile backendProfile,
		PersistentRasterLifecycleProfile lifecycleProfile,
		PersistentRasterPlacementProfile placementProfile
	) {
		ArgumentNullException.ThrowIfNull( backendProfile );
		ArgumentNullException.ThrowIfNull( lifecycleProfile );
		ArgumentNullException.ThrowIfNull( placementProfile );

		BackendProfile = backendProfile;
		LifecycleProfile = lifecycleProfile;
		PlacementProfile = placementProfile;
	}

	/// <summary>Gets the concrete backend availability profile.</summary>
	public RasterBackendProfile BackendProfile {
		get;
	}

	/// <summary>Gets the lifecycle semantic profile for this backend context.</summary>
	public PersistentRasterLifecycleProfile LifecycleProfile {
		get;
	}

	/// <summary>Gets the placement semantic profile for this backend context.</summary>
	public PersistentRasterPlacementProfile PlacementProfile {
		get;
	}
}
