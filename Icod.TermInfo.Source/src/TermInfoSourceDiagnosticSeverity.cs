namespace Icod.TermInfo.Source;

/// <summary>
/// Identifies the severity of a terminfo source diagnostic.
/// </summary>
public enum TermInfoSourceDiagnosticSeverity
{
    /// <summary>
    /// The condition does not prevent a source consumer from continuing.
    /// </summary>
    Warning = 0,

    /// <summary>
    /// The source is malformed or cannot be processed as requested.
    /// </summary>
    Error = 1,
}
