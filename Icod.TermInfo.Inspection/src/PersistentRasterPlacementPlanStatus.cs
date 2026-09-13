namespace Icod.TermInfo.Inspection;

/// <summary>
/// Identifies the deterministic outcome of advanced persistent-raster placement
/// planning.
/// </summary>
public enum PersistentRasterPlacementPlanStatus {
	/// <summary>
	/// All requested advanced placement semantics and required lifecycle operations
	/// are statically admissible.
	/// </summary>
	Satisfied = 0,

	/// <summary>
	/// Lifecycle planning is admissible, but one or more requested advanced placement
	/// semantics require runtime verification.
	/// </summary>
	RequiresRuntimeVerification = 1,

	/// <summary>
	/// The placement semantics are not conclusively impossible, but the prerequisite
	/// lifecycle plan is indeterminate.
	/// </summary>
	Indeterminate = 2,

	/// <summary>
	/// The prerequisite lifecycle plan is impossible or at least one requested
	/// advanced placement semantic is unsupported or contradicted.
	/// </summary>
	Impossible = 3,
}
