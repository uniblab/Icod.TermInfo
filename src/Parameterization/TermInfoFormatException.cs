namespace Icod.TermInfo;

/// <summary>
/// The terminfo parameter program is malformed.
/// </summary>
public sealed class TermInfoFormatException : FormatException
{
    /// <summary>
    /// Initializes an exception with the default message.
    /// </summary>
    public TermInfoFormatException()
    {
        Position = -1;
    }

    /// <summary>
    /// Initializes an exception with the specified message.
    /// </summary>
    public TermInfoFormatException(string? message)
        : base(message)
    {
        Position = -1;
    }

    /// <summary>
    /// Initializes an exception with the specified message and inner exception.
    /// </summary>
    public TermInfoFormatException(
        string? message,
        Exception? innerException)
        : base(message, innerException)
    {
        Position = -1;
    }

    internal TermInfoFormatException(
        string message,
        int position)
        : base(CreateMessage(message, position))
    {
        Position = position;
    }

    /// <summary>
    /// Gets the zero-based source position associated with the error, or
    /// <c>-1</c> when no source position was supplied.
    /// </summary>
    public int Position { get; }

    private static string CreateMessage(
        string message,
        int position)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (position < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        return $"{message} (position {position}).";
    }
}
