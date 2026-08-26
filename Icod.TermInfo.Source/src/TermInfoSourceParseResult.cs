namespace Icod.TermInfo.Source;

/// <summary>
/// Contains the result of parsing a terminfo source document into unresolved
/// entries.
/// </summary>
public sealed class TermInfoSourceParseResult
{
    internal TermInfoSourceParseResult(
        TermInfoSourceDocument document,
        IEnumerable<TermInfoSourceDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(diagnostics);

        TermInfoSourceDiagnostic[] diagnosticArray =
            diagnostics.ToArray();

        Document = document;
        Diagnostics = diagnosticArray;
        HasErrors =
            diagnosticArray.Any(
                diagnostic =>
                    diagnostic.Severity
                        == TermInfoSourceDiagnosticSeverity.Error);
    }

    /// <summary>
    /// Gets the parsed unresolved document, including recoverable content when
    /// diagnostics are present.
    /// </summary>
    public TermInfoSourceDocument Document { get; }

    /// <summary>
    /// Gets lexical and value-semantics diagnostics in deterministic source
    /// order.
    /// </summary>
    public IReadOnlyList<TermInfoSourceDiagnostic> Diagnostics { get; }

    /// <summary>
    /// Gets whether at least one error diagnostic was produced.
    /// </summary>
    public bool HasErrors { get; }
}
