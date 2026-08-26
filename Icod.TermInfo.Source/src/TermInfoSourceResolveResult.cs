namespace Icod.TermInfo.Source;

/// <summary>
/// Contains the result of resolving one terminfo source entry and its
/// inheritance graph.
/// </summary>
public sealed class TermInfoSourceResolveResult
{
    internal TermInfoSourceResolveResult(
        TermInfoSourceResolvedEntry? entry,
        IEnumerable<TermInfoSourceDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        TermInfoSourceDiagnostic[] diagnosticArray =
            diagnostics.ToArray();

        Entry = entry;
        Diagnostics = diagnosticArray;
        HasErrors =
            diagnosticArray.Any(
                diagnostic =>
                    diagnostic.Severity
                        == TermInfoSourceDiagnosticSeverity.Error);
    }

    /// <summary>
    /// Gets the resolved entry when resolution succeeded completely.
    /// </summary>
    /// <remarks>
    /// Resolution does not expose a partial semantic result when an inheritance
    /// error occurs.
    /// </remarks>
    public TermInfoSourceResolvedEntry? Entry { get; }

    /// <summary>
    /// Gets resolver diagnostics in deterministic source order.
    /// </summary>
    public IReadOnlyList<TermInfoSourceDiagnostic> Diagnostics { get; }

    /// <summary>
    /// Gets whether at least one error diagnostic was produced.
    /// </summary>
    public bool HasErrors { get; }
}
