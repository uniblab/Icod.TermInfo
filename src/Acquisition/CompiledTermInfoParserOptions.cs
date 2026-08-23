namespace Icod.TermInfo;

/// <summary>
/// Configures immutable resource limits for compiled terminfo parsing.
/// </summary>
/// <remarks>
/// The maximum applies to a complete compiled entry whether the bytes are
/// supplied directly to <see cref="CompiledTermInfoParser"/> or read through a
/// filesystem-backed provider. Provider constructors snapshot this value.
/// Increasing the limit does not enable additional compiled formats.
/// </remarks>
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
    /// The largest complete compiled entry, in bytes, the parser will accept.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maximumEntrySize"/> is less than one byte or greater
    /// than <see cref="MaximumSupportedEntrySize"/>.
    /// </exception>
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
