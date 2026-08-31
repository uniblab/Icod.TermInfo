namespace Icod.TermInfo.Termcap;

/// <summary>
/// Identifies which explicitly configured termcap source supplied an acquired
/// root entry.
/// </summary>
public enum TermcapAcquisitionSourceKind
{
	/// <summary>An inline termcap description supplied directly to acquisition.</summary>
	InlineTermcap = 0,

	/// <summary>A database path supplied as the explicit TERMCAP path.</summary>
	TermcapDatabasePath = 1,

	/// <summary>A database path supplied through the ordered TERMPATH list.</summary>
	TermPathDatabase = 2,

	/// <summary>A database selected by an explicit conventional-default policy.</summary>
	ConventionalDefaultDatabase = 3,
}
