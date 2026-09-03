namespace Icod.TermInfo.Inspection;

/// <summary>
/// Describes the deterministic canonical-name lookup state for an ordered
/// terminfo database set.
/// </summary>
public enum TermInfoDatabaseSetLookupStatus {
	/// <summary>
	/// The canonical identity was not observed and every constituent database was
	/// inspected completely, so absence is conclusive.
	/// </summary>
	NotObserved = 0,

	/// <summary>
	/// At least one occurrence was observed and the first applicable occurrence is
	/// known under caller-selected database precedence.
	/// </summary>
	WinnerKnown = 1,

	/// <summary>
	/// Incomplete evidence prevents a conclusive absence or winner determination.
	/// </summary>
	Indeterminate = 2,
}
