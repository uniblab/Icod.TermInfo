namespace Icod.TermInfo.Inspection;

/// <summary>
/// Identifies the semantic category of one terminfo comparison difference.
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
	/// An effective capability is present only in the left description.
	/// </summary>
	OnlyInLeft = 3,

	/// <summary>
	/// An effective capability is present only in the right description.
	/// </summary>
	OnlyInRight = 4,

	/// <summary>
	/// An effective capability is present on both sides with the same value kind
	/// but a different value.
	/// </summary>
	DifferentValue = 5,

	/// <summary>
	/// An extended effective capability is present on both sides with different
	/// value kinds.
	/// </summary>
	DifferentValueKind = 6,

	/// <summary>
	/// An unresolved source document contains an entry only on the left at the
	/// compared source position.
	/// </summary>
	SourceEntryOnlyInLeft = 7,

	/// <summary>
	/// An unresolved source document contains an entry only on the right at the
	/// compared source position.
	/// </summary>
	SourceEntryOnlyInRight = 8,

	/// <summary>
	/// An unresolved source entry contains a field only on the left at the
	/// compared source position.
	/// </summary>
	SourceFieldOnlyInLeft = 9,

	/// <summary>
	/// An unresolved source entry contains a field only on the right at the
	/// compared source position.
	/// </summary>
	SourceFieldOnlyInRight = 10,

	/// <summary>
	/// Two unresolved source fields at the same position have different field
	/// kinds, including present/cancelled/disabled state differences.
	/// </summary>
	SourceFieldKind = 11,

	/// <summary>
	/// Two capability-bearing unresolved source fields at the same position refer
	/// to different semantic capability identities.
	/// </summary>
	SourceFieldCapability = 12,

	/// <summary>
	/// Two unresolved source fields at the same position refer to the same
	/// capability but carry different local values.
	/// </summary>
	SourceFieldValue = 13,

	/// <summary>
	/// Two unresolved <c>use=</c> fields at the same position reference different
	/// parent entries.
	/// </summary>
	SourceUseReference = 14,
}
