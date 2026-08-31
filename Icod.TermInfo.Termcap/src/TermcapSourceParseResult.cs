using System.Collections.ObjectModel;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Contains a parsed termcap source document and its diagnostics.
/// </summary>
public sealed class TermcapSourceParseResult
{
	internal TermcapSourceParseResult(
		TermcapSourceDocument document,
		IEnumerable<TermcapSourceDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( document );
		ArgumentNullException.ThrowIfNull( diagnostics );

		Document = document;
		Diagnostics =
			new ReadOnlyCollection<TermcapSourceDiagnostic>(
				diagnostics.ToArray()
			);
	}

	/// <summary>Gets the unresolved parsed document.</summary>
	public TermcapSourceDocument Document { get; }

	/// <summary>Gets diagnostics in deterministic source order.</summary>
	public IReadOnlyList<TermcapSourceDiagnostic> Diagnostics { get; }

	/// <summary>Gets whether any diagnostic has error severity.</summary>
	public bool HasErrors =>
		Diagnostics.Any(
			diagnostic =>
				diagnostic.Severity == TermcapSourceDiagnosticSeverity.Error
		);
}
