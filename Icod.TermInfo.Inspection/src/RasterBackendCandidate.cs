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

	/// <summary>
	/// Initializes one immutable raster-backend candidate from the exact lifecycle
	/// and placement profiles retained by a frozen 1.13 runtime-integration result.
	/// </summary>
	/// <param name="backendProfile">The classified backend availability profile.</param>
	/// <param name="integration">
	/// The backend-scoped runtime-integration result whose resulting lifecycle and
	/// placement profiles are composed into the candidate.
	/// </param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="backendProfile"/> or <paramref name="integration"/> is
	/// <see langword="null"/>.
	/// </exception>
	public RasterBackendCandidate(
		RasterBackendProfile backendProfile,
		PersistentRasterRuntimeIntegrationResult integration
	) {
		ArgumentNullException.ThrowIfNull( backendProfile );
		ArgumentNullException.ThrowIfNull( integration );

		BackendProfile = backendProfile;
		LifecycleProfile = integration.LifecycleProfile;
		PlacementProfile = integration.PlacementProfile;
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
