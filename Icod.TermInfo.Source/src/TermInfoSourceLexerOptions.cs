namespace Icod.TermInfo.Source;

/// <summary>
/// Configures immutable resource limits for terminfo source tokenization.
/// </summary>
public sealed class TermInfoSourceLexerOptions
{
    /// <summary>
    /// The default maximum source length: 4 Mi UTF-16 code units.
    /// </summary>
    public const int DefaultMaximumSourceLength = 4_194_304;

    /// <summary>
    /// The largest configurable source length: 64 Mi UTF-16 code units.
    /// </summary>
    public const int MaximumSupportedSourceLength = 67_108_864;

    /// <summary>
    /// Initializes immutable lexer options.
    /// </summary>
    /// <param name="maximumSourceLength">
    /// The largest complete source document, in UTF-16 code units, the lexer
    /// will accept.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maximumSourceLength"/> is less than one or greater than
    /// <see cref="MaximumSupportedSourceLength"/>.
    /// </exception>
    public TermInfoSourceLexerOptions(
        int maximumSourceLength = DefaultMaximumSourceLength)
    {
        if (maximumSourceLength <= 0
            || maximumSourceLength > MaximumSupportedSourceLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumSourceLength),
                maximumSourceLength,
                $"The maximum source length must be between 1 and {MaximumSupportedSourceLength} UTF-16 code units.");
        }

        MaximumSourceLength = maximumSourceLength;
    }

    /// <summary>
    /// Gets the largest source document accepted by the lexer, in UTF-16 code
    /// units.
    /// </summary>
    public int MaximumSourceLength { get; }
}
