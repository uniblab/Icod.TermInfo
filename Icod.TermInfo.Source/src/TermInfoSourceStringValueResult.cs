namespace Icod.TermInfo.Source;

/// <summary>
/// Contains the result of interpreting one string terminfo source value.
/// </summary>
public sealed class TermInfoSourceStringValueResult
{
    internal TermInfoSourceStringValueResult(
        string? value,
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
    /// Gets the decoded string value, or <see langword="null"/> when the source
    /// value is invalid.
    /// </summary>
    /// <remarks>
    /// Byte-valued source escapes are represented by the corresponding Unicode
    /// code point in the range U+0001 through U+00FF. Terminfo's historical NUL
    /// compatibility rule maps source zero to U+0080 rather than U+0000.
    /// </remarks>
    public string? Value { get; }

    /// <summary>
    /// Gets value-semantics diagnostics in deterministic source order.
    /// </summary>
    public IReadOnlyList<TermInfoSourceDiagnostic> Diagnostics { get; }

    /// <summary>
    /// Gets whether at least one error diagnostic was produced.
    /// </summary>
    public bool HasErrors { get; }
}
