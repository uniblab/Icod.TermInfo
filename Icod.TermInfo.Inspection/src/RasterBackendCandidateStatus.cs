namespace Icod.TermInfo.Inspection;

/// <summary>
/// Identifies the deterministic evaluation status of one raster-backend
/// candidate against a semantic request.
/// </summary>
public enum RasterBackendCandidateStatus {
	/// <summary>The candidate satisfies the requested semantics.</summary>
	Satisfied = 0,

	/// <summary>The candidate requires caller-owned runtime verification.</summary>
	RequiresRuntimeVerification = 1,

	/// <summary>The candidate cannot satisfy the requested semantics.</summary>
	Impossible = 2,
}
