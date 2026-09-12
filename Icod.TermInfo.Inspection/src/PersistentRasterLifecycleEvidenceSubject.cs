namespace Icod.TermInfo.Inspection;

/// <summary>
/// Identifies one protocol-neutral persistent-raster lifecycle dimension about
/// which static or caller-supplied evidence may make a positive or negative
/// assertion.
/// </summary>
public enum PersistentRasterLifecycleEvidenceSubject {
	/// <summary>
	/// Ordinary one-shot raster display capability.
	/// </summary>
	RasterDisplay = 0,

	/// <summary>
	/// Upload of raster content into a reusable terminal-resident resource.
	/// </summary>
	PersistentUpload = 1,

	/// <summary>
	/// Positive acknowledgement or equivalent confirmation of an upload.
	/// </summary>
	AcknowledgedUpload = 2,

	/// <summary>
	/// Creation of a visible placement referring to a persistent resource.
	/// </summary>
	PlacementCreation = 3,

	/// <summary>
	/// Simultaneous multiple placements referring to one persistent resource.
	/// </summary>
	MultiplePlacements = 4,

	/// <summary>
	/// Update or replacement of an existing placement while retaining its
	/// semantic identity.
	/// </summary>
	PlacementUpdate = 5,

	/// <summary>
	/// Targeted deletion of a placement without necessarily deleting its
	/// underlying raster resource.
	/// </summary>
	PlacementDeletion = 6,

	/// <summary>
	/// Targeted deletion of terminal-resident raster resource data.
	/// </summary>
	ResourceDeletion = 7,
}
