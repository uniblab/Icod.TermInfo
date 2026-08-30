using System.Collections.ObjectModel;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Contains the preflight decision for rendering a Runtime terminal description
/// as conventional termcap source.
/// </summary>
public sealed class TermcapRepresentabilityResult
{
	internal TermcapRepresentabilityResult(
		IEnumerable<TermcapRenderDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( diagnostics );

		TermcapRenderDiagnostic[] diagnosticArray = diagnostics.ToArray();
		Diagnostics =
			new ReadOnlyCollection<TermcapRenderDiagnostic>(
				diagnosticArray
			);
		HasErrors =
			diagnosticArray.Any(
				diagnostic =>
					diagnostic.Severity == TermcapRenderDiagnosticSeverity.Error
			);
	}

	/// <summary>Gets preflight diagnostics in deterministic decision order.</summary>
	public IReadOnlyList<TermcapRenderDiagnostic> Diagnostics { get; }

	/// <summary>Gets whether at least one representability error was found.</summary>
	public bool HasErrors { get; }

	/// <summary>Gets whether the description can be rendered without semantic loss.</summary>
	public bool IsRepresentable => !HasErrors;
}
