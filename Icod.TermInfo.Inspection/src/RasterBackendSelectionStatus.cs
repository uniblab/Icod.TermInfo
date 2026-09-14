namespace Icod.TermInfo.Inspection;

/// <summary>
/// Identifies the deterministic outcome of raster-backend selection planning.
/// </summary>
public enum RasterBackendSelectionStatus {
	/// <summary>Exactly one backend has been selected under caller policy.</summary>
	Selected = 0,

	/// <summary>Caller-owned runtime verification is required before selection.</summary>
	RequiresRuntimeVerification = 1,

	/// <summary>Multiple viable backends remain and explicit preference is required.</summary>
	RequiresPreference = 2,

	/// <summary>No candidate can satisfy the request.</summary>
	Impossible = 3,
}
