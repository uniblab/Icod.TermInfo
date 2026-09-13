namespace Icod.TermInfo.Inspection;

/// <summary>
/// Describes one represented limitation encountered while importing persistent-
/// raster runtime observations into frozen lifecycle or placement evidence.
/// </summary>
public enum PersistentRasterRuntimeIntegrationIssueKind {
	/// <summary>
	/// Conclusive lifecycle observations cannot fit within the frozen lifecycle
	/// evidence-count bound.
	/// </summary>
	LifecycleEvidenceCapacityExhausted = 0,

	/// <summary>
	/// Conclusive placement observations cannot fit within the frozen placement
	/// evidence-count bound.
	/// </summary>
	PlacementEvidenceCapacityExhausted = 1,

	/// <summary>
	/// Consecutive final lifecycle evidence ordinals cannot be assigned without
	/// overflowing <see cref="int"/>.
	/// </summary>
	LifecycleOrdinalSpaceExhausted = 2,

	/// <summary>
	/// Consecutive final placement evidence ordinals cannot be assigned without
	/// overflowing <see cref="int"/>.
	/// </summary>
	PlacementOrdinalSpaceExhausted = 3,
}
