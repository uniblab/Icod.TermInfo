namespace Icod.TermInfo.Source;

/// <summary>
/// Represents one semantic lexical unit from terminfo source.
/// </summary>
public sealed class TermInfoSourceToken
{
    internal TermInfoSourceToken(
        TermInfoSourceTokenKind kind,
        string text,
        TermInfoSourceSpan span)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(span);

        Kind = kind;
        Text = text;
        Span = span;
    }

    /// <summary>
    /// Gets the lexical classification.
    /// </summary>
    public TermInfoSourceTokenKind Kind { get; }

    /// <summary>
    /// Gets the exact source text covered by this token.
    /// </summary>
    /// <remarks>
    /// Capability token text remains encoded exactly as supplied. Escape,
    /// numeric, and string-value interpretation begins in S03.
    /// </remarks>
    public string Text { get; }

    /// <summary>
    /// Gets the token's location in the original source.
    /// </summary>
    public TermInfoSourceSpan Span { get; }
}
