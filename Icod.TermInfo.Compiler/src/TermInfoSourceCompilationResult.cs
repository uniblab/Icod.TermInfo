using Icod.TermInfo.Source;

namespace Icod.TermInfo.Compiler;

/// <summary>
/// Contains the deterministic result of compiling one terminfo source
/// document.
/// </summary>
public sealed class TermInfoSourceCompilationResult {
	internal TermInfoSourceCompilationResult(
		IEnumerable<CompiledTermInfoSourceEntry> entries,
		IEnumerable<TermInfoSourceDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( entries );
		ArgumentNullException.ThrowIfNull( diagnostics );

		CompiledTermInfoSourceEntry[] entryArray =
			entries.ToArray();
		TermInfoSourceDiagnostic[] diagnosticArray =
			diagnostics.ToArray();

		Entries = entryArray;
		Diagnostics = diagnosticArray;
		HasErrors =
			diagnosticArray.Any(
				diagnostic =>
					diagnostic.Severity
						== TermInfoSourceDiagnosticSeverity.Error
			);
	}

	/// <summary>
	/// Gets successfully compiled entries in source-document order.
	/// </summary>
	public IReadOnlyList<CompiledTermInfoSourceEntry> Entries { get; }

	/// <summary>
	/// Gets source parser and resolver diagnostics with their original source
	/// spans preserved.
	/// </summary>
	public IReadOnlyList<TermInfoSourceDiagnostic> Diagnostics { get; }

	/// <summary>
	/// Gets whether at least one source error diagnostic was produced.
	/// </summary>
	public bool HasErrors { get; }
}
