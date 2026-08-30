namespace Icod.TermInfo.Termcap;

/// <summary>
/// Identifies how one termcap source construct was represented during conversion.
/// </summary>
public enum TermcapConversionDecision
{
	/// <summary>The source construct maps directly to the canonical Runtime model.</summary>
	Exact = 0,

	/// <summary>An adopted historical alias maps exactly to its canonical Runtime identity.</summary>
	HistoricalAlias = 1,

	/// <summary>An unmapped two-character field is preserved as a Runtime extended capability.</summary>
	Extended = 2,

	/// <summary>The source construct required a deterministic but non-exact choice.</summary>
	Approximation = 3,

	/// <summary>The source construct is understood but is not supported by this conversion tranche.</summary>
	Unsupported = 4,

	/// <summary>The source value cannot be represented faithfully by the Runtime model.</summary>
	Unrepresentable = 5,
}
