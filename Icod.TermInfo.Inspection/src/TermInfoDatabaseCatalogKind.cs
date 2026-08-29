namespace Icod.TermInfo.Inspection;

/// <summary>
/// Identifies the observed storage state for one explicitly inspected terminfo
/// database root.
/// </summary>
public enum TermInfoDatabaseCatalogKind {
	/// <summary>
	/// The requested root does not currently exist.
	/// </summary>
	Missing = 0,

	/// <summary>
	/// The requested root is a conventional terminfo directory.
	/// </summary>
	ConventionalDirectory = 1,

	/// <summary>
	/// The requested root identifies a non-directory store which this release
	/// does not support.
	/// </summary>
	UnsupportedStore = 2,

	/// <summary>
	/// The requested root exists or may exist, but could not be inspected
	/// sufficiently to determine or enumerate its conventional contents.
	/// </summary>
	Unavailable = 3,
}
