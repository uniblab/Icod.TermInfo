namespace Icod.TermInfo.Inspection;

/// <summary>
/// Composes the frozen lifecycle request with optional advanced placement
/// requirements for raster-backend selection.
/// </summary>
public sealed class RasterBackendSelectionRequest {
	/// <summary>Initializes one raster-backend selection request.</summary>
	public RasterBackendSelectionRequest(
		PersistentRasterLifecycleRequest lifecycleRequest,
		PersistentRasterPlacementRequest? placementRequest = null
	) {
		ArgumentNullException.ThrowIfNull( lifecycleRequest );
		LifecycleRequest = lifecycleRequest;
		PlacementRequest = placementRequest;
	}

	/// <summary>Gets the frozen lifecycle semantic request.</summary>
	public PersistentRasterLifecycleRequest LifecycleRequest {
		get;
	}

	/// <summary>
	/// Gets optional advanced placement requirements, or <see langword="null"/> when
	/// no advanced placement semantics are requested.
	/// </summary>
	public PersistentRasterPlacementRequest? PlacementRequest {
		get;
	}
}
