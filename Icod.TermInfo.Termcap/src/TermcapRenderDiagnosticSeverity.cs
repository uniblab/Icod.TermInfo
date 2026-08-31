namespace Icod.TermInfo.Termcap;

/// <summary>
/// Identifies the severity of a termcap reverse-rendering diagnostic.
/// </summary>
public enum TermcapRenderDiagnosticSeverity
{
	/// <summary>The diagnostic records non-lossy rendering information.</summary>
	Information = 0,

	/// <summary>The diagnostic records a condition which callers should review.</summary>
	Warning = 1,

	/// <summary>The description cannot be represented faithfully as termcap.</summary>
	Error = 2,
}
