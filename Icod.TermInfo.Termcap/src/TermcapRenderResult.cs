using System.Collections.ObjectModel;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Contains deterministic termcap source when reverse rendering succeeds, or the
/// complete preflight diagnostics when it cannot be performed losslessly.
/// </summary>
public sealed class TermcapRenderResult
{
	internal TermcapRenderResult(
		string? text,
		IEnumerable<TermcapRenderDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( diagnostics );

		TermcapRenderDiagnostic[] diagnosticArray = diagnostics.ToArray();
		Text = text;
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

	/// <summary>Gets rendered conventional termcap source, or null when preflight failed.</summary>
	public string? Text { get; }

	/// <summary>Gets rendering diagnostics in deterministic decision order.</summary>
	public IReadOnlyList<TermcapRenderDiagnostic> Diagnostics { get; }

	/// <summary>Gets whether at least one representability error was found.</summary>
	public bool HasErrors { get; }

	/// <summary>Gets whether rendering completed without semantic loss.</summary>
	public bool IsRepresentable => !HasErrors && Text is not null;
}
