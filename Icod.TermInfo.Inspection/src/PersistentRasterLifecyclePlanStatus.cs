namespace Icod.TermInfo.Inspection;

/// <summary>
/// Identifies the deterministic outcome of persistent-raster lifecycle planning.
/// </summary>
public enum PersistentRasterLifecyclePlanStatus {
	/// <summary>All required semantic capabilities are supported.</summary>
	Success = 0,

	/// <summary>
	/// No required capability is conclusively unsupported, but one or more required
	/// capabilities are unknown or contradicted and require runtime verification.
	/// </summary>
	Indeterminate = 1,

	/// <summary>
	/// At least one required semantic capability is conclusively unsupported.
	/// </summary>
	Impossible = 2,
}
