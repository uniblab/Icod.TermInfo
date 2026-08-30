namespace Icod.TermInfo.Termcap;

/// <summary>
/// Identifies how a parsed termcap capability code relates to the canonical
/// Runtime standard-capability catalog.
/// </summary>
public enum TermcapCapabilityClassification
{
	/// <summary>
	/// The code maps uniquely to a current standard Runtime capability.
	/// </summary>
	Standard = 0,

	/// <summary>
	/// The code maps uniquely to an obsolete termcap compatibility capability
	/// retained by the Runtime standard catalog.
	/// </summary>
	ObsoleteStandard = 1,

	/// <summary>
	/// The code is an explicitly recognized obsolete non-standard alias which
	/// maps to a current Runtime standard capability.
	/// </summary>
	ObsoleteAlias = 2,

	/// <summary>
	/// The code has more than one semantic mapping and therefore cannot be
	/// classified as one Runtime capability without additional policy.
	/// </summary>
	Ambiguous = 3,

	/// <summary>
	/// The syntactically valid two-character code is not currently mapped.
	/// </summary>
	Unmapped = 4,

	/// <summary>
	/// The field is the termcap <c>tc=</c> inheritance reference rather than a
	/// capability value.
	/// </summary>
	Reference = 5,
}
