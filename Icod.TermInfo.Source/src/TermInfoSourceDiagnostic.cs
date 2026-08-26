namespace Icod.TermInfo.Source;

/// <summary>
/// Describes a deterministic diagnostic produced while reading terminfo source.
/// </summary>
public sealed class TermInfoSourceDiagnostic
{
    internal TermInfoSourceDiagnostic(
        string code,
        TermInfoSourceDiagnosticSeverity severity,
        string message,
        TermInfoSourceSpan? span)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentNullException.ThrowIfNull(message);

        Code = code;
        Severity = severity;
        Message = message;
        Span = span;
    }

    /// <summary>
    /// Gets the stable machine-readable <c>TISdddd</c> diagnostic code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the diagnostic severity.
    /// </summary>
    public TermInfoSourceDiagnosticSeverity Severity { get; }

    /// <summary>
    /// Gets the human-readable diagnostic message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the relevant source span, or <see langword="null"/> when the
    /// diagnostic applies to the source as a whole.
    /// </summary>
    public TermInfoSourceSpan? Span { get; }
}
