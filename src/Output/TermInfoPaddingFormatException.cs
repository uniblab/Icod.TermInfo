namespace Icod.TermInfo;

/// <summary>
/// Represents malformed terminfo padding syntax.
/// </summary>
public sealed class TermInfoPaddingFormatException : FormatException
{
    /// <summary>
    /// Initializes an exception with the default message.
    /// </summary>
    public TermInfoPaddingFormatException()
    {
        Position = -1;
    }

    /// <summary>
    /// Initializes an exception with the specified message.
    /// </summary>
    public TermInfoPaddingFormatException(string? message)
        : base(message)
    {
        Position = -1;
    }

    /// <summary>
    /// Initializes an exception with the specified message and inner exception.
    /// </summary>
    public TermInfoPaddingFormatException(
        string? message,
        Exception? innerException)
        : base(message, innerException)
    {
        Position = -1;
    }

    internal TermInfoPaddingFormatException(
        string message,
        int position)
        : base(message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (position < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        Position = position;
    }

    /// <summary>
    /// Gets the zero-based position of the malformed padding directive, or
    /// <c>-1</c> when no source position was supplied.
    /// </summary>
    public int Position { get; }
}
