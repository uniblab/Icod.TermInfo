using Icod.TermInfo;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Describes one deterministic termcap representability or rendering decision.
/// </summary>
public sealed class TermcapRenderDiagnostic
{
	internal TermcapRenderDiagnostic(
		string code,
		TermcapRenderDiagnosticSeverity severity,
		string message,
		string? capabilityName = null,
		TermInfoCapabilityValueKind? valueKind = null
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( code );
		ArgumentException.ThrowIfNullOrWhiteSpace( message );

		Code = code;
		Severity = severity;
		Message = message;
		CapabilityName = capabilityName;
		ValueKind = valueKind;
	}

	/// <summary>Gets the stable diagnostic code.</summary>
	public string Code { get; }

	/// <summary>Gets the diagnostic severity.</summary>
	public TermcapRenderDiagnosticSeverity Severity { get; }

	/// <summary>Gets the deterministic diagnostic message.</summary>
	public string Message { get; }

	/// <summary>
	/// Gets the Runtime short name or extended capability name associated with the
	/// diagnostic, when applicable.
	/// </summary>
	public string? CapabilityName { get; }

	/// <summary>Gets the capability value kind associated with the diagnostic, when applicable.</summary>
	public TermInfoCapabilityValueKind? ValueKind { get; }
}
