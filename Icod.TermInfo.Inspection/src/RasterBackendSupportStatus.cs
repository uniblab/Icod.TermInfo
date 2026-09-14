namespace Icod.TermInfo.Inspection;

/// <summary>
/// Identifies the deterministic availability classification of one raster
/// backend.
/// </summary>
public enum RasterBackendSupportStatus {
	/// <summary>No conclusive backend availability evidence is available.</summary>
	Unknown = 0,

	/// <summary>The backend is conclusively supported.</summary>
	Supported = 1,

	/// <summary>The backend is conclusively unsupported.</summary>
	Unsupported = 2,

	/// <summary>Equally authoritative evidence contradicts backend availability.</summary>
	Contradicted = 3,
}
