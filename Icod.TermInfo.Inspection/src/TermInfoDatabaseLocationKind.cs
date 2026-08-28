namespace Icod.TermInfo.Inspection;

/// <summary>
/// Identifies the discovery source represented by a terminfo database location.
/// </summary>
public enum TermInfoDatabaseLocationKind {
	/// <summary>
	/// An encoded <c>TERMINFO</c> entry which precedes directory discovery.
	/// </summary>
	EncodedTermInfo = 0,

	/// <summary>
	/// A directory selected by the <c>TERMINFO</c> environment variable.
	/// </summary>
	TermInfoDirectory = 1,

	/// <summary>
	/// The platform user-local terminfo database.
	/// </summary>
	UserDatabase = 2,

	/// <summary>
	/// A directory contributed by <c>TERMINFO_DIRS</c>, including a platform
	/// default inserted by an empty component.
	/// </summary>
	TermInfoDirsDirectory = 3,

	/// <summary>
	/// A platform default directory reached after environment and user sources.
	/// </summary>
	PlatformDefaultDirectory = 4,
}
