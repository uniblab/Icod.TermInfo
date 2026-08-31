namespace Icod.TermInfo.Termcap;

/// <summary>
/// Describes a deterministic termcap source parsing diagnostic.
/// </summary>
public sealed class TermcapSourceDiagnostic
{
	internal TermcapSourceDiagnostic(
		string code,
		TermcapSourceDiagnosticSeverity severity,
		string message,
		TermcapSourceSpan? span
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( code );
		ArgumentNullException.ThrowIfNull( message );

		Code = code;
		Severity = severity;
		Message = message;
		Span = span;
	}

	/// <summary>Gets the stable diagnostic code.</summary>
	public string Code { get; }

	/// <summary>Gets the diagnostic severity.</summary>
	public TermcapSourceDiagnosticSeverity Severity { get; }

	/// <summary>Gets the diagnostic message.</summary>
	public string Message { get; }

	/// <summary>Gets the related source span, when one is available.</summary>
	public TermcapSourceSpan? Span { get; }
}
