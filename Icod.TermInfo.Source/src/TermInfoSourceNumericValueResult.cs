namespace Icod.TermInfo.Source;

/// <summary>
/// Contains the result of interpreting one numeric terminfo source value.
/// </summary>
public sealed class TermInfoSourceNumericValueResult
{
    internal TermInfoSourceNumericValueResult(
        int? value,
        IEnumerable<TermInfoSourceDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        TermInfoSourceDiagnostic[] diagnosticArray =
            diagnostics.ToArray();

        Value = value;
        Diagnostics = diagnosticArray;
        HasErrors =
            diagnosticArray.Any(
                diagnostic =>
                    diagnostic.Severity
                        == TermInfoSourceDiagnosticSeverity.Error);
    }

    /// <summary>
    /// Gets the decoded numeric value, or <see langword="null"/> when the source
    /// value is invalid.
    /// </summary>
    public int? Value { get; }

    /// <summary>
    /// Gets value-semantics diagnostics in deterministic source order.
    /// </summary>
    public IReadOnlyList<TermInfoSourceDiagnostic> Diagnostics { get; }

    /// <summary>
    /// Gets whether at least one error diagnostic was produced.
    /// </summary>
    public bool HasErrors { get; }
}
