namespace Icod.TermInfo;

/// <summary>
/// Configures resource limits for compiled terminfo parsing.
/// </summary>
public sealed class CompiledTermInfoParserOptions
{
    /// <summary>
    /// The default maximum compiled-entry size: 1 MiB.
    /// </summary>
    public const int DefaultMaximumEntrySize = 1_048_576;

    /// <summary>
    /// The largest configurable compiled-entry size: 16 MiB.
    /// </summary>
    public const int MaximumSupportedEntrySize = 16_777_216;

    /// <summary>
    /// Initializes immutable parser options.
    /// </summary>
    /// <param name="maximumEntrySize">
    /// The largest compiled entry the parser will accept.
    /// </param>
    public CompiledTermInfoParserOptions(
        int maximumEntrySize = DefaultMaximumEntrySize)
    {
        if (maximumEntrySize <= 0
            || maximumEntrySize > MaximumSupportedEntrySize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumEntrySize),
                maximumEntrySize,
                $"The maximum entry size must be between 1 and {MaximumSupportedEntrySize} bytes.");
        }

        MaximumEntrySize = maximumEntrySize;
    }

    /// <summary>
    /// Gets the largest compiled entry the parser will accept.
    /// </summary>
    public int MaximumEntrySize { get; }
}
