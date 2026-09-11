namespace Icod.TermInfo.Inspection;

/// <summary>
/// Identifies a protocol-neutral semantic operation in the raster lifecycle.
/// </summary>
public enum PersistentRasterLifecycleOperation {
	/// <summary>
	/// Displays raster content ephemerally without establishing a reusable
	/// terminal-resident resource.
	/// </summary>
	DisplayEphemeral = 0,

	/// <summary>
	/// Uploads raster data as a reusable terminal-resident resource without
	/// requiring immediate display.
	/// </summary>
	UploadResource = 1,

	/// <summary>
	/// Creates a visible placement that refers to an existing persistent raster
	/// resource.
	/// </summary>
	CreatePlacement = 2,

	/// <summary>
	/// Replaces or updates an existing placement while retaining its semantic
	/// placement identity.
	/// </summary>
	UpdatePlacement = 3,

	/// <summary>
	/// Removes one existing placement without necessarily deleting its underlying
	/// raster resource.
	/// </summary>
	DeletePlacement = 4,

	/// <summary>
	/// Removes terminal-resident raster resource data.
	/// </summary>
	DeleteResource = 5,
}
