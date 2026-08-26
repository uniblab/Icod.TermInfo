namespace Icod.TermInfo.Source;

/// <summary>
/// Represents one parsed terminfo source document before inheritance resolution.
/// </summary>
public sealed class TermInfoSourceDocument
{
    internal TermInfoSourceDocument(
        IEnumerable<TermInfoSourceEntry> entries,
        IEnumerable<TermInfoSourceToken> tokens)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(tokens);

        Entries = entries.ToArray();
        Tokens = tokens.ToArray();
    }

    /// <summary>
    /// Gets parsed entries in document order.
    /// </summary>
    public IReadOnlyList<TermInfoSourceEntry> Entries { get; }

    /// <summary>
    /// Gets the complete lexical token stream retained from S02.
    /// </summary>
    /// <remarks>
    /// Retaining the token stream preserves comments, invalid lexical units,
    /// exact field text, and source spans for later diagnostics and inspection
    /// without making them part of resolved terminal semantics.
    /// </remarks>
    public IReadOnlyList<TermInfoSourceToken> Tokens { get; }
}
