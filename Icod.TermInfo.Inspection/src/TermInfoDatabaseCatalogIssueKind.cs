namespace Icod.TermInfo.Inspection;

/// <summary>
/// Identifies one non-fatal issue encountered while inspecting a conventional
/// terminfo database root.
/// </summary>
public enum TermInfoDatabaseCatalogIssueKind {
	/// <summary>
	/// A candidate file could not be parsed as a supported compiled terminfo
	/// entry, including configured resource-limit failures.
	/// </summary>
	MalformedEntry = 0,

	/// <summary>
	/// A parsed entry is not conventionally placed for the identity represented
	/// by its containing file.
	/// </summary>
	InvalidPlacement = 1,

	/// <summary>
	/// Filesystem access was denied.
	/// </summary>
	PermissionFailure = 2,

	/// <summary>
	/// A filesystem operation failed for another I/O reason.
	/// </summary>
	IoFailure = 3,

	/// <summary>
	/// A symbolic-link, junction, or other reparse-point candidate was skipped
	/// rather than followed.
	/// </summary>
	LinkSkipped = 4,
}
