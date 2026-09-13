namespace Icod.TermInfo.Inspection;

/// <summary>
/// Identifies one protocol-neutral advanced persistent-raster placement semantic
/// about which static or caller-supplied evidence may make a support assertion.
/// </summary>
/// <remarks>
/// These values describe support for placement semantics only. They do not carry
/// concrete source-rectangle coordinates, signed z-order values, terminal-side
/// identifiers, or wire-protocol fields.
/// </remarks>
public enum PersistentRasterPlacementSubject {
	/// <summary>
	/// Placement of a selected pixel-space source rectangle from an uploaded raster
	/// resource.
	/// </summary>
	SourceRectangle = 0,

	/// <summary>
	/// Placement using an explicit signed stacking or z-order value.
	/// </summary>
	SignedZOrder = 1,
}
