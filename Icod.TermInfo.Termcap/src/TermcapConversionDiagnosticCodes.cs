namespace Icod.TermInfo.Termcap;

/// <summary>
/// Defines stable diagnostic codes emitted by termcap semantic conversion.
/// </summary>
public static class TermcapConversionDiagnosticCodes
{
	/// <summary>An adopted historical termcap alias was mapped to its canonical Runtime capability.</summary>
	public const string HistoricalAlias = "TCON0001";
	/// <summary>An unmapped two-character field was preserved as a Runtime extended capability.</summary>
	public const string UnmappedExtendedCapability = "TCON0002";
	/// <summary>A historical termcap code has more than one possible semantic target.</summary>
	public const string AmbiguousCapability = "TCON0003";
	/// <summary>The source field kind does not match the mapped Runtime capability kind.</summary>
	public const string ValueKindMismatch = "TCON0004";
	/// <summary>Two source fields map to the same Runtime capability identity.</summary>
	public const string DuplicateSemanticCapability = "TCON0005";
	/// <summary>An unmapped field name collides with an existing standard terminfo short name.</summary>
	public const string ExtendedNameCollision = "TCON0006";
	/// <summary>A termcap parameter-string operator cannot be translated exactly.</summary>
	public const string UnsupportedParameterOperator = "TCON0007";
	/// <summary>A resolved field has a kind which cannot occur in an effective TC03 field set.</summary>
	public const string InvalidResolvedFieldKind = "TCON0008";
	/// <summary>A duplicate terminal header identity was ignored while constructing Runtime metadata.</summary>
	public const string DuplicateTerminalName = "TCON0009";
	/// <summary>A parameterized termcap string uses a capability profile outside the TC04 two-parameter conversion boundary.</summary>
	public const string UnsupportedParameterizedCapability = "TCON0010";
}
