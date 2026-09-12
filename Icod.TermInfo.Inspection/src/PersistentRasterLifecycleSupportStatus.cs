namespace Icod.TermInfo.Inspection;

/// <summary>
/// Describes the deterministic semantic support state for one persistent-raster
/// lifecycle capability.
/// </summary>
public enum PersistentRasterLifecycleSupportStatus {
	/// <summary>
	/// The available evidence does not establish either support or lack of support.
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// The available evidence establishes support for the semantic capability.
	/// </summary>
	Supported = 1,

	/// <summary>
	/// The available evidence establishes that the semantic capability is not
	/// supported.
	/// </summary>
	Unsupported = 2,

	/// <summary>
	/// Conclusive positive and negative evidence coexist and the contradiction is
	/// retained rather than silently resolved.
	/// </summary>
	Contradicted = 3,
}
