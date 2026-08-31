namespace Icod.TermInfo.Termcap;

/// <summary>
/// Describes one deterministic termcap semantic-conversion decision or failure.
/// </summary>
public sealed class TermcapConversionDiagnostic
{
	internal TermcapConversionDiagnostic(
		string code,
		TermcapConversionDiagnosticSeverity severity,
		TermcapConversionDecision decision,
		string message,
		TermcapSourceEntry sourceEntry,
		TermcapSourceField? sourceField
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( code );
		ArgumentException.ThrowIfNullOrWhiteSpace( message );
		ArgumentNullException.ThrowIfNull( sourceEntry );

		Code = code;
		Severity = severity;
		Decision = decision;
		Message = message;
		SourceEntry = sourceEntry;
		SourceField = sourceField;
	}

	/// <summary>Gets the stable diagnostic code.</summary>
	public string Code { get; }

	/// <summary>Gets the diagnostic severity.</summary>
	public TermcapConversionDiagnosticSeverity Severity { get; }

	/// <summary>Gets the conversion decision represented by this diagnostic.</summary>
	public TermcapConversionDecision Decision { get; }

	/// <summary>Gets the deterministic diagnostic message.</summary>
	public string Message { get; }

	/// <summary>Gets the unresolved entry associated with this diagnostic.</summary>
	public TermcapSourceEntry SourceEntry { get; }

	/// <summary>Gets the effective source field associated with this diagnostic, when applicable.</summary>
	public TermcapSourceField? SourceField { get; }

	/// <summary>Gets the best available source span for this diagnostic.</summary>
	public TermcapSourceSpan Span =>
		SourceField?.Span
		?? SourceEntry.Span;
}
