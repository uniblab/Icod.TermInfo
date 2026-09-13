namespace Icod.TermInfo.Inspection;

/// <summary>
/// Describes the result of one caller-owned protocol-neutral persistent-raster
/// runtime observation.
/// </summary>
public enum PersistentRasterRuntimeObservationOutcome {
	/// <summary>
	/// Runtime observation establishes support for the semantic subject.
	/// </summary>
	Supported = 0,

	/// <summary>
	/// Runtime observation establishes non-support for the semantic subject.
	/// </summary>
	Unsupported = 1,

	/// <summary>
	/// Runtime observation does not establish either support or non-support.
	/// </summary>
	Inconclusive = 2,
}
