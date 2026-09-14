namespace Icod.TermInfo.Inspection;

/// <summary>
/// Identifies the provenance class of a raster-backend availability assertion.
/// </summary>
public enum RasterBackendEvidenceKind {
	/// <summary>Evidence recognized from static terminal capability metadata.</summary>
	CapabilityDerived = 0,

	/// <summary>Evidence explicitly declared by the caller without live verification.</summary>
	Declared = 1,

	/// <summary>Evidence supplied as the result of verification outside Icod.TermInfo.</summary>
	Verified = 2,
}
