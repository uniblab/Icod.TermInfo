namespace Icod.TermInfo.Source;

/// <summary>
/// Identifies a contiguous span of caller-supplied terminfo source text.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Offset"/> and <see cref="Length"/> are measured in UTF-16 code
/// units so they can be applied directly to the original .NET <see cref="string"/>.
/// <see cref="Line"/> and <see cref="Column"/> are one-based.
/// </para>
/// <para>
/// <see cref="SourceName"/> is caller-supplied identity only. The source layer
/// never invents a file path for text which was not associated with one.
/// </para>
/// </remarks>
public sealed class TermInfoSourceSpan
{
    /// <summary>
    /// Initializes a source span.
    /// </summary>
    /// <param name="sourceName">Optional caller-supplied source identity.</param>
    /// <param name="offset">Zero-based UTF-16 offset.</param>
    /// <param name="line">One-based line number.</param>
    /// <param name="column">One-based column number.</param>
    /// <param name="length">Span length in UTF-16 code units.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="offset"/> or <paramref name="length"/> is negative, or
    /// <paramref name="line"/> or <paramref name="column"/> is less than one.
    /// </exception>
    public TermInfoSourceSpan(
        string? sourceName,
        int offset,
        int line,
        int column,
        int length)
    {
        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(offset),
                offset,
                "The source offset cannot be negative.");
        }

        if (line < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(line),
                line,
                "The source line must be at least one.");
        }

        if (column < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(column),
                column,
                "The source column must be at least one.");
        }

        if (length < 0
            || offset > int.MaxValue - length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(length),
                length,
                "The source span length must be non-negative and fit within the source offset range.");
        }

        SourceName = sourceName;
        Offset = offset;
        Line = line;
        Column = column;
        Length = length;
    }

    /// <summary>
    /// Gets the caller-supplied source identity, or <see langword="null"/> when
    /// the source has no external identity.
    /// </summary>
    public string? SourceName { get; }

    /// <summary>
    /// Gets the zero-based UTF-16 offset into the original source text.
    /// </summary>
    public int Offset { get; }

    /// <summary>
    /// Gets the one-based line number at which the span begins.
    /// </summary>
    public int Line { get; }

    /// <summary>
    /// Gets the one-based column number at which the span begins.
    /// </summary>
    public int Column { get; }

    /// <summary>
    /// Gets the span length in UTF-16 code units.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Gets the exclusive zero-based UTF-16 offset at the end of the span.
    /// </summary>
    public int EndOffset => checked(Offset + Length);
}
