namespace Icod.TermInfo.Inspection;

/// <summary>
/// Identifies the provenance class of an advanced persistent-raster placement
/// evidence assertion.
/// </summary>
public enum PersistentRasterPlacementEvidenceKind {
	/// <summary>
	/// Evidence recognized deterministically from static terminal capability
	/// metadata.
	/// </summary>
	CapabilityDerived = 0,

	/// <summary>
	/// Evidence explicitly declared by the caller without a live verification
	/// result.
	/// </summary>
	Declared = 1,

	/// <summary>
	/// Evidence explicitly supplied by the caller as the result of verification
	/// performed outside Icod.TermInfo.
	/// </summary>
	Verified = 2,
}
