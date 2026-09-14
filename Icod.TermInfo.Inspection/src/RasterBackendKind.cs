namespace Icod.TermInfo.Inspection;

/// <summary>
/// Identifies a concrete raster graphics protocol backend whose availability can
/// be classified independently from persistent-raster lifecycle semantics.
/// </summary>
public enum RasterBackendKind {
	/// <summary>The Sixel raster graphics backend.</summary>
	Sixel = 0,

	/// <summary>The Kitty Graphics raster graphics backend.</summary>
	KittyGraphics = 1,
}
