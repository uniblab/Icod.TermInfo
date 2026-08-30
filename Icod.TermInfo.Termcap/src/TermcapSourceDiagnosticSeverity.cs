namespace Icod.TermInfo.Termcap;

/// <summary>
/// Identifies the severity of a termcap source diagnostic.
/// </summary>
public enum TermcapSourceDiagnosticSeverity
{
	/// <summary>
	/// The source is accepted but contains a compatibility concern.
	/// </summary>
	Warning = 0,

	/// <summary>
	/// The source contains an error which prevents a clean parse.
	/// </summary>
	Error = 1,
}
