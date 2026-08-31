namespace Icod.TermInfo.Termcap;

/// <summary>
/// Identifies the severity of a termcap semantic-conversion diagnostic.
/// </summary>
public enum TermcapConversionDiagnosticSeverity
{
	/// <summary>The conversion is lossless, but a compatibility decision is observable.</summary>
	Information = 0,

	/// <summary>The conversion produced a usable description with an explicit approximation.</summary>
	Warning = 1,

	/// <summary>The conversion cannot produce a complete semantic description.</summary>
	Error = 2,
}
