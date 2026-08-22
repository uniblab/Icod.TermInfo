namespace Icod.TermInfo;

/// <summary>
/// A valid terminfo parameter program cannot be evaluated with the supplied values.
/// </summary>
public sealed class TermInfoEvaluationException : InvalidOperationException
{
    /// <summary>
    /// Initializes an exception with the default message.
    /// </summary>
    public TermInfoEvaluationException()
    {
        Position = -1;
    }

    /// <summary>
    /// Initializes an exception with the specified message.
    /// </summary>
    public TermInfoEvaluationException(string? message)
        : base(message)
    {
        Position = -1;
    }

    /// <summary>
    /// Initializes an exception with the specified message and inner exception.
    /// </summary>
    public TermInfoEvaluationException(
        string? message,
        Exception? innerException)
        : base(message, innerException)
    {
        Position = -1;
    }

    internal TermInfoEvaluationException(
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
