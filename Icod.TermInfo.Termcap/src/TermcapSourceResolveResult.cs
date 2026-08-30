using System.Collections.ObjectModel;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Contains the result of resolving one termcap source entry and its
/// <c>tc=</c> inheritance chain.
/// </summary>
public sealed class TermcapSourceResolveResult
{
	internal TermcapSourceResolveResult(
		TermcapSourceResolvedEntry? entry,
		IEnumerable<TermcapSourceDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( diagnostics );

		TermcapSourceDiagnostic[] diagnosticArray =
			diagnostics.ToArray();
		Entry = entry;
		Diagnostics =
			new ReadOnlyCollection<TermcapSourceDiagnostic>(
				diagnosticArray
			);
		HasErrors =
			diagnosticArray.Any(
				diagnostic =>
					diagnostic.Severity == TermcapSourceDiagnosticSeverity.Error
			);
	}

	/// <summary>
	/// Gets the resolved entry when resolution succeeded completely.
	/// </summary>
	/// <remarks>
	/// Resolution does not expose a partial effective field set when an
	/// inheritance error occurs.
	/// </remarks>
	public TermcapSourceResolvedEntry? Entry { get; }

	/// <summary>
	/// Gets resolver diagnostics in deterministic source order.
	/// </summary>
	public IReadOnlyList<TermcapSourceDiagnostic> Diagnostics { get; }

	/// <summary>
	/// Gets whether at least one error diagnostic was produced.
	/// </summary>
	public bool HasErrors { get; }
}
