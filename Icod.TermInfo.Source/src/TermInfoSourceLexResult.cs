namespace Icod.TermInfo.Source;

/// <summary>
/// Contains tokens and diagnostics produced from one terminfo source document.
/// </summary>
public sealed class TermInfoSourceLexResult
{
    internal TermInfoSourceLexResult(
        IEnumerable<TermInfoSourceToken> tokens,
        IEnumerable<TermInfoSourceDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(tokens);
        ArgumentNullException.ThrowIfNull(diagnostics);

        TermInfoSourceToken[] tokenArray =
            tokens.ToArray();
        TermInfoSourceDiagnostic[] diagnosticArray =
            diagnostics.ToArray();

        Tokens = tokenArray;
        Diagnostics = diagnosticArray;
        HasErrors =
            diagnosticArray.Any(
                diagnostic =>
                    diagnostic.Severity
                        == TermInfoSourceDiagnosticSeverity.Error);
    }

    /// <summary>
    /// Gets the semantic lexical units in source order.
    /// </summary>
    public IReadOnlyList<TermInfoSourceToken> Tokens { get; }

    /// <summary>
    /// Gets diagnostics in deterministic source order.
    /// </summary>
    public IReadOnlyList<TermInfoSourceDiagnostic> Diagnostics { get; }

    /// <summary>
    /// Gets whether at least one error diagnostic was produced.
    /// </summary>
    public bool HasErrors { get; }
}
