namespace Icod.TermInfo.Inspection;

/// <summary>
/// Selects deterministic ordering for standard capabilities rendered within
/// each Boolean, numeric, or string group.
/// </summary>
public enum TerminalDescriptionSourceCapabilityOrder {
	/// <summary>
	/// Uses conventional compiled-table order.
	/// </summary>
	Database = 0,

	/// <summary>
	/// Orders by traditional terminfo short name using ordinal comparison.
	/// </summary>
	TermInfoName = 1,

	/// <summary>
	/// Orders by long/variable terminfo name using ordinal comparison.
	/// </summary>
	LongName = 2,

	/// <summary>
	/// Orders by termcap code using ordinal comparison.
	/// </summary>
	TermcapCode = 3,
}
