namespace Icod.TermInfo.Inspection;

/// <summary>
/// Identifies the semantic category of one effective terminfo difference.
/// </summary>
public enum TermInfoDifferenceKind {
	/// <summary>
	/// The canonical terminal names differ.
	/// </summary>
	IdentityName = 0,

	/// <summary>
	/// The ordered terminal alias lists differ.
	/// </summary>
	IdentityAliases = 1,

	/// <summary>
	/// The terminal descriptions differ.
	/// </summary>
	IdentityDescription = 2,

	/// <summary>
	/// A capability is present only in the left description.
	/// </summary>
	OnlyInLeft = 3,

	/// <summary>
	/// A capability is present only in the right description.
	/// </summary>
	OnlyInRight = 4,

	/// <summary>
	/// A capability is present on both sides with the same value kind but a
	/// different value.
	/// </summary>
	DifferentValue = 5,

	/// <summary>
	/// An extended capability is present on both sides with different value kinds.
	/// </summary>
	DifferentValueKind = 6,
}
