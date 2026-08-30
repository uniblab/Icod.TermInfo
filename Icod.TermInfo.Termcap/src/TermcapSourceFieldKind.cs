namespace Icod.TermInfo.Termcap;

/// <summary>
/// Identifies the unresolved syntactic kind of a termcap capability field.
/// </summary>
public enum TermcapSourceFieldKind
{
	/// <summary>A present Boolean capability.</summary>
	BooleanCapability = 0,
	/// <summary>A numeric capability.</summary>
	NumericCapability = 1,
	/// <summary>A string capability.</summary>
	StringCapability = 2,
	/// <summary>A canceled capability.</summary>
	CancelledCapability = 3,
	/// <summary>A <c>tc=</c> inheritance reference.</summary>
	Reference = 4,
	/// <summary>A period-prefixed capability retained as disabled source text.</summary>
	DisabledCapability = 5,
}
